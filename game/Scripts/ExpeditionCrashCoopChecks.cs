using Godot;
using System;
using System.Threading.Tasks;
public partial class MigrationScene {
 int crashAck;
 [Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]void CoopCrashAck(int step){if(coopCheck&&CoopHost&&coopActors.ContainsKey(Multiplayer.GetRemoteSenderId()))crashAck=Math.Max(crashAck,step);}
 async Task CrashCoopChecks(CoopActor actor){crashFixture=true;game.World.Changes.Crash=new();var home=HostPosition;Teleport(ExpeditionCrash.X(game.World),ExpeditionCrash.Z);SpawnCrash();actor.Body.Position=crashRoot.ToGlobal(new Vector3(0,0,3.4f));SendCoopPersonal(actor.Id,CoopProfileJson(actor.Game),"",actor.Body.Position,true);CrashSync();RpcId(actor.Id,nameof(CoopCheckStage),"crash-trunk",actor.Body.Position);await CoopWait(()=>crashAck>=1,"Guest trunk/rabbit missing",15);if(paused||!Crash.Toy||Crash.Choice!=0)throw new Exception("Crash trunk affected host UI or branch");CheckStaleCoopPose(actor);RpcId(actor.Id,nameof(CoopCheckStage),"crash-choice",crashRoot.ToGlobal(new Vector3(2.4f,0,.3f)));await CoopWait(()=>crashAck>=2,"Guest crash choice missing",15);if(paused||Crash.Choice!=1||!actor.Game.Profile.Journal.Seen.Contains("crash/truth"))throw new Exception("Crash choice/journal isolation failed");crashFixture=false;Teleport(home.X,home.Z);actor.Body.Position=HostPosition+Vector3.Right*3;SendCoopPersonal(actor.Id,CoopProfileJson(actor.Game),"",actor.Body.Position,true);GD.Print("PASS / two-process crash: guest trunk, rabbit, handoff, shared first-choice-wins and personal dialogue/journal");}
 void CheckStaleCoopPose(CoopActor actor){
  var corrected=actor.Body.Position;int oldRevision=actor.PositionRevision;
  SendCoopPersonal(actor.Id,CoopProfileJson(actor.Game),"",corrected,true);
  ApplyCoopPose(actor,corrected+Vector3.Right*.5f,0,0,false,oldRevision);
  if(actor.Body.Position!=corrected)throw new Exception("Pre-correction pose overwrote host position");
  ApplyCoopPose(actor,corrected+Vector3.Right*.1f,0,0,false,actor.PositionRevision);
  if(actor.Body.Position==corrected)throw new Exception("Current-revision valid pose was rejected");
  actor.Body.Position=corrected;
  SendCoopPersonal(actor.Id,CoopProfileJson(actor.Game),"",corrected,true);
  GD.Print("PASS / movement correction revision: delayed old pose rejected; current valid movement accepted");
 }
 async Task ClientCrashChecks(string stage,Vector3 target){crashFixture=true;if(stage=="crash-trunk")await CoopWait(()=>Distance(player.Position,target)<1,"Crash teleport missing",15);SpawnCrash();if(stage=="crash-trunk"){CoopRequest("crash","trunk");await CoopWait(()=>paused&&Crash.Opened,"Guest trunk dialogue missing");CrashChoice("take");await CoopWait(()=>Crash.Toy&&!paused,"Guest rabbit missing");RpcId(1,nameof(CoopCrashAck),1);}else{
 // Walk around the chassis instead of issuing a second server teleport.
 // Reliable corrections and unreliable poses can cross in flight; normal movement
 // lets the authority observe the route before the distance-gated interaction.
 double walkDeadline=Time.GetTicksMsec()/1000.0+10;
 foreach(var waypoint in new[]{crashRoot.ToGlobal(new Vector3(2.4f,0,3.4f)),target}){
  while(Distance(player.Position,waypoint)>.15f){
   if(Time.GetTicksMsec()/1000.0>walkDeadline)throw new Exception("Guest could not walk around crash chassis");
   Vector3 delta=waypoint-player.Position;delta.Y=0;
   player.Position+=delta.Normalized()*Math.Min(.06f,delta.Length());
   await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
  }
 }
 await CoopDelay(.3);
 CoopRequest("crash","girl");await CoopWait(()=>paused,"Guest girl dialogue missing after walking around car");
 CrashChoice("give");await CoopWait(()=>paused&&Crash.Given,"Guest handoff missing");CrashChoice("truth");await CoopWait(()=>paused&&Crash.Choice==1&&game.Profile.Journal.Seen.Contains("crash/truth"),"Guest branch/journal missing");CloseMenu();CoopRequest("crash","secret");await CoopDelay(.2);if(Crash.Choice!=1)throw new Exception("Second crash choice overwrote first");RpcId(1,nameof(CoopCrashAck),2);crashFixture=false;}}
}
