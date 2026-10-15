using Godot;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
public partial class MigrationScene {
 bool coopCheck;string coopCheckRole;
 async Task CoopWait(Func<bool> condition,string failure,double timeout=15){double end=Time.GetTicksMsec()/1000.0+timeout;while(!condition()){if(Time.GetTicksMsec()/1000.0>end)throw new Exception(failure);await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);}}
 async Task CoopDelay(double seconds){await ToSignal(GetTree().CreateTimer(seconds),SceneTreeTimer.SignalName.Timeout);}
 async void RunCoopChecks(){try{
  coopCheck=true;coopName="Check "+coopCheckRole;
  if(coopCheckRole=="host"){
   game.Change(()=>{game.Save.Seed=431;game.Save.X=0;game.Save.Z=12;game.Normalize();});Teleport(0,12);
   StartCoop(true,"",29748);GD.Print("COOP CHECK / host ready");await CoopWait(()=>coopActors.Count==1,"Client did not join",40);
   await CoopWait(()=>game.Building.Pieces.Count>0,"Guest structure did not reach host",40);
   await CoopWait(()=>game.Craft.Gathered.Any(id=>id.Contains("/starter/")),"Guest resource was not shared",20);
   var a=coopActors.Values.First();if(a.Game.Craft.BenchTier!=1)throw new Exception("Guest crafting did not run on host");
   if(game.Craft.BenchTier!=0)throw new Exception("Guest crafting altered host character");
   var original=player.Position;var target=a.Body.Position;AddEnemy("coop-check/infected",target.X,target.Z-5,1);var enemy=infected.Find(e=>e.Id=="coop-check/infected");if(enemy==null)throw new Exception("Fixture infected failed to spawn");enemy.Hp=1;RpcId(a.Id,nameof(CoopCheckStage),"shoot",enemy.Body.Position);
   await CoopWait(()=>game.Skills.Defeated.Contains("coop-check/infected"),"Guest shot was not resolved by host",15);
   a.Game.Shield=0;a.Game.Life.Health=100;WithCoopActor(a,()=>game.Hurt(1));RpcId(a.Id,nameof(CoopPersonal),CoopProfileJson(a.Game),"",a.Body.Position,false);
   await CoopDelay(1);RpcId(a.Id,nameof(CoopCheckStage),"bandage",Vector3.Zero);
   await CoopWait(()=>a.Game.Life.Wounds.Count==1&&a.Game.Life.Wounds[0].Patched,"Guest could not patch authoritative wound",15);
   a.Game.Skills.Stamina.Xp=a.Game.Skills.Stamina.Need*.8;int deaths=a.Game.Skills.Deaths;a.Game.Life.Health=0;await CoopWait(()=>a.Game.Skills.Deaths==deaths+1,"Guest death did not resolve");if(a.Game.Skills.Stamina.Xp!=0||a.Game.Skills.Stamina.Level!=1)throw new Exception("Death XP rule failed online");var portal=ExpeditionDungeon.Nearby(game.World,0,0)[0];EnterDungeon(portal);
   await CoopDelay(2);ExitDungeon();await CoopDelay(2);
   RpcId(a.Id,nameof(CoopCheckStage),"done",Vector3.Zero);await CoopWait(()=>coopCheckRejoined,"Guest did not rejoin",20);await CoopDelay(1);
   StopCoop("Check complete");GD.Print("PASS / Godot ENet host: handshake, guest crafting isolation, shared building/resource, authoritative combat/wound/death, party dungeon travel, reconnect and host shutdown");GetTree().Quit();
  }else{
   game.Change(()=>{game.Save.Seed=777;game.Save.X=0;game.Save.Z=12;game.Normalize();game.Craft.Materials=new[]{40,40,40,40,40,40,40};game.Craft.Weapon=1;game.Craft.Ammo=6;game.Life.Bandages=3;});
   await CoopDelay(1);StartCoop(false,"127.0.0.1",29748);await CoopWait(()=>coopReady,"No host welcome",40);
   await CoopWait(()=>coopActors.ContainsKey(1),"Host avatar missing");if(game.Save.Seed!=431)throw new Exception("Host seed was not applied");
   CoopRequest("craft","0");await CoopWait(()=>game.Craft.BenchTier==1,"Craft result missing");
   var piece=new BuildPiece{Kind=0,X=9,Y=game.World.Height(9,12),Z=12,Yaw=30};CoopRequest("place",Json.Write(piece));await CoopWait(()=>game.Building.Pieces.Count==1,"Shared rotated foundation missing");
   Vector3 destination=Ground(0,4)+Vector3.Up*.2f;
   while(Distance(player.Position,destination)>.5f){Vector3 delta=destination-player.Position;delta.Y=0;player.Position+=delta.Normalized()*.07f;await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);}
   await CoopDelay(.3);CoopRequest("interact");await CoopWait(()=>game.Craft.Gathered.Any(id=>id.Contains("/starter/")),"Gather result missing");
   await CoopWait(()=>coopCheckFinished,"Host checks did not finish",50);
   if(!coopCheckSawDungeon)throw new Exception("Party dungeon was not synchronized");
   int bench=game.Craft.BenchTier;StopCoop("Check complete");if(game.Save.Seed!=777||game.Building.Pieces.Count!=0||game.Craft.BenchTier!=bench)throw new Exception("Leaving co-op did not preserve character and restore solo world");
   await CoopDelay(.5);StartCoop(false,"127.0.0.1",29748);await CoopWait(()=>coopReady,"Reconnect failed");if(game.Craft.BenchTier!=1||game.Building.Pieces.Count!=1||!game.Skills.Defeated.Contains("coop-check/infected"))throw new Exception("Rejoin did not restore shared state");RpcId(1,nameof(CoopCheckRejoin));await CoopWait(()=>!CoopActive,"Host disconnect was not handled");if(game.Save.Seed!=777||game.Building.Pieces.Count!=0)throw new Exception("Disconnect altered solo world");var reload=new ExpeditionGame(game.PathName);if(reload.Save.Seed!=777||reload.Craft.BenchTier!=1)throw new Exception("Portable character save was not persisted");
   GD.Print("PASS / Godot ENet client: host avatar, seed, craft/build/gather, enemy replication, shot/bandage, dungeon sync, solo-world isolation and character persistence");GetTree().Quit();
  }
 }catch(Exception ex){GD.PushError("COOP CHECK FAILED / "+ex);GetTree().Quit(1);}}
 bool coopCheckFinished,coopCheckSawDungeon,coopCheckRejoined;[Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)] void CoopCheckRejoin(){if(coopCheck&&CoopHost&&coopActors.ContainsKey(Multiplayer.GetRemoteSenderId()))coopCheckRejoined=true;}
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
 async void CoopCheckStage(string stage,Vector3 target){if(!coopCheck||coopCheckRole!="client")return;try{if(stage=="shoot"){await CoopWait(()=>infected.Any(e=>e.Id=="coop-check/infected"),"Enemy replica absent");player.Rotation=Vector3.Zero;pitch=0;camera.Rotation=Vector3.Zero;await CoopDelay(.3);CoopRequest("shoot");}else if(stage=="bandage"){if(game.Life.Wounds.Count!=1)throw new Exception("Host damage was not replicated");CoopRequest("bandage");}else if(stage=="done")coopCheckFinished=true;}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}
