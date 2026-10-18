using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
public partial class MigrationScene {
 int journeyCoopAck;
 [Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]void CoopJourneyAck(int stage){if(coopCheck&&CoopHost&&coopActors.ContainsKey(Multiplayer.GetRemoteSenderId()))journeyCoopAck=Math.Max(journeyCoopAck,stage);}
 async Task JourneyCoopChecks(CoopActor actor){bool old=journeyFixture,oldContent=contentFixture;try{
  journeyFixture=contentFixture=true;journeyCoopAck=0;ClearEnemies();Teleport(ExpeditionWorld.SpawnX,ExpeditionWorld.SpawnZ);actor.Body.Position=player.Position+Vector3.Right*2;actor.Game.Save.X=actor.Body.Position.X;actor.Game.Save.Z=actor.Body.Position.Z;actor.Game.Life.Journey=new();actor.Game.Craft.Materials[1]=5;actor.Game.Stamina=100;actor.Shot=actor.Reload=0;int hostStone=game.Craft.Materials[1];
  SendCoopPersonal(actor.Id,CoopProfileJson(actor.Game),"",actor.Body.Position,true);RpcId(actor.Id,nameof(CoopCheckStage),"journey-depart",actor.Body.Position);await CoopWait(()=>journeyCoopAck>=1,"Guest journey/distraction actions failed",20);MigrationChecks.Assert(actor.Game.Life.Journey.Active&&actor.Game.Life.Journey.Site==14&&game.Life.Journey?.Active!=true&&actor.Game.Craft.Materials[1]==4&&game.Craft.Materials[1]==hostStone,"Journey or distraction affected host character");
  var at=ExpeditionContentSite.Place(game.World,14);Teleport(at.X,at.Z+6);SpawnContentAtlas();var site=props.First(p=>p.Kind==36&&p.Material==14);actor.Body.Position=site.Position;actor.Game.Save.X=actor.Body.Position.X;actor.Game.Save.Z=actor.Body.Position.Z;Field(14).DeadGuards=(1<<FieldGuardCount(14))-1;ClearEnemies();int hostPart=game.Craft.SpareParts.GetValueOrDefault(6),guestPart=actor.Game.Craft.SpareParts.GetValueOrDefault(6);actor.Game.Shield=60;
  SendCoopPersonal(actor.Id,CoopProfileJson(actor.Game),"",actor.Body.Position,true);FeverSync();RpcId(actor.Id,nameof(CoopCheckStage),"journey-locker",actor.Body.Position);await CoopWait(()=>journeyCoopAck>=2,"Guest timed locker did not replicate",20);MigrationChecks.Assert(Field(14).LockerOpened&&actor.Game.Craft.SpareParts.GetValueOrDefault(6)==guestPart+1&&game.Craft.SpareParts.GetValueOrDefault(6)==hostPart,"Shared locker paid wrong character or duplicated");
  GD.Print("PASS / two-process expedition loop: private plans, resource-cost distractions, timed shared lockers, private specialist loot, duplicate rejection and plan cancellation");
 }finally{ClearEnemies();journeyFixture=old;contentFixture=oldContent;}}
 async Task ClientJourneyChecks(string stage,Vector3 target){journeyFixture=true;if(stage=="journey-depart"){
  await CoopWait(()=>Distance(player.Position,target)<1&&game.Craft.Materials[1]==5,"Departure profile missing");CoopRequest("journey","14");await CoopWait(()=>game.Life.Journey?.Active==true&&game.Life.Journey.Site==14,"Guest plan missing");CoopRequest("distract");await CoopWait(()=>game.Craft.Materials[1]==4,"Guest distraction did not spend one stone");RpcId(1,nameof(CoopJourneyAck),1);
 }else{
  contentFixture=true;await CoopWait(()=>Distance(player.Position,target)<1&&Field(14).DeadGuards==7,"Specialist locker site missing");SpawnContentAtlas();int before=game.Craft.SpareParts.GetValueOrDefault(6);CoopRequest("content","locker-force:14");await CoopWait(()=>Field(14).LockerOpened&&game.Craft.SpareParts.GetValueOrDefault(6)==before+1,"Timed specialist loot not synchronized",15);CoopRequest("content","locker-force:14");await CoopWait(()=>game.Notice.Contains("empty"),"Duplicate locker request was not rejected");CoopRequest("journey","cancel");await CoopWait(()=>game.Life.Journey.Active==false,"Guest journey cancellation missing");journeyFixture=contentFixture=false;RpcId(1,nameof(CoopJourneyAck),2);
 }}
}
