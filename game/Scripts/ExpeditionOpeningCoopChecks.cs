using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
public partial class MigrationScene {
 bool openingCoopAck;
 [Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]void CoopOpeningAck(){if(coopCheck&&CoopHost&&coopActors.ContainsKey(Multiplayer.GetRemoteSenderId()))openingCoopAck=true;}
 async Task OpeningCoopChecks(CoopActor actor){var oldPieces=game.Building.Pieces.ToList();var position=actor.Body.Position;try{ClearEnemies();double y=game.World.Height(9,4);for(int side=-1;side<=1;side+=2)game.Building.Pieces.Add(new(){Id="coop-opening-foundation/"+side,Kind=0,X=9+side*1.5,Y=y,Z=4,Health=100});var gate=new BuildPiece{Id="coop-opening-gate",Kind=OpeningKind("gate",5),X=9,Z=4,Y=y+.5,Health=700};game.Building.Pieces.Add(gate);MigrationChecks.Assert(game.Building.Evaluate(game.Building.Pieces).Valid,"Co-op gate fixture is structurally supported");SyncStructures();actor.Body.Position=Ground(9,7)+Vector3.Up*.3f;openingCoopAck=false;
  RpcId(actor.Id,nameof(CoopWorldUpdate),Json.Write(WorldSnapshot()));SendCoopPersonal(actor.Id,CoopProfileJson(actor.Game),"",actor.Body.Position,true);RpcId(actor.Id,nameof(CoopCheckStage),"opening-gate",new Vector3(9,(float)gate.Y,4));await CoopWait(()=>openingCoopAck,"Guest gate interaction/collision replication failed",20);MigrationChecks.Assert(!gate.Open&&gate.DoorSwing==1,"Host gate final state/swing differs from guest");GD.Print("PASS / two-process tiered gate: actual guest open/close requests, manual operation, saved swing direction and physical replica collision");
 }finally{game.Building.Pieces.Clear();game.Building.Pieces.AddRange(oldPieces);if(GodotObject.IsInstanceValid(actor.Body))actor.Body.Position=position;SyncStructures();RpcId(actor.Id,nameof(CoopWorldUpdate),Json.Write(WorldSnapshot()));}}
 async Task ClientOpeningChecks(Vector3 at){await CoopWait(()=>game.Building.Pieces.Any(p=>p.Id=="coop-opening-gate"),"Gate snapshot missing");CoopRequest("baseuse","coop-opening-gate|door");await CoopWait(()=>game.Building.Pieces.First(p=>p.Id=="coop-opening-gate").Open,"Guest cannot open military gate");await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
  bool Blocked(){var query=PhysicsRayQueryParameters3D.Create(at+new Vector3(0,1.3f,1),at+new Vector3(0,1.3f,-1));query.Exclude=new(){player.GetRid()};return GetWorld3D().DirectSpaceState.IntersectRay(query).Count>0;}
  MigrationChecks.Assert(!Blocked(),"Replicated open gate still blocks passage");CoopRequest("baseuse","coop-opening-gate|door");await CoopWait(()=>!game.Building.Pieces.First(p=>p.Id=="coop-opening-gate").Open,"Guest cannot close gate");await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);MigrationChecks.Assert(Blocked(),"Replicated closed gate has no collision");RpcId(1,nameof(CoopOpeningAck));
 }
}
