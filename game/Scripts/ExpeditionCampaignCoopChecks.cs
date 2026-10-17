using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class MigrationScene {
 int campaignAck;
 [Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]void CoopCampaignAck(int step){if(coopCheck&&CoopHost&&coopActors.ContainsKey(Multiplayer.GetRemoteSenderId()))campaignAck=Math.Max(campaignAck,step);}
 async Task CampaignCoopChecks(CoopActor actor){
  ClearEnemies();Teleport(0,12);player.Position=Ground(6,14);actor.Body.Position=Ground(0,12)+Vector3.Up*.1f;
  actor.Game.Craft.Weapons.Add(0);actor.Game.Craft.Weapon=0;actor.Game.Craft.Ammo=6;actor.Game.Craft.UnlockedTier=2;actor.Game.Career.Level=Math.Max(2,actor.Game.Career.Level);actor.Game.Shield=30;
  AddEnemy("special/weeper/coop",0,8,2);SendCoopPersonal(actor.Id,CoopProfileJson(actor.Game),"",actor.Body.Position,true);RpcId(actor.Id,nameof(CoopCheckStage),"campaign-weeper",actor.Body.Position);
  await CoopWait(()=>campaignAck>=1,"Guest weeper aggression/replica missing",15);MigrationChecks.Assert(infected.Any(e=>e.Id=="special/weeper/coop"&&!DormantWeeper(e)),"Guest shot wakes shared weeper");
  ClearEnemies();actor.Body.Position=Ground(0,12)+Vector3.Up*.1f;actor.Game.Craft.Ammo=6;actor.Shot=0;
  AddEnemy("special/bloater/coop",0,9,2);infected.Last().Hp=1;SendCoopPersonal(actor.Id,CoopProfileJson(actor.Game),"",actor.Body.Position,true);RpcId(actor.Id,nameof(CoopCheckStage),"campaign-bloater",actor.Body.Position);
  await CoopWait(()=>campaignAck>=2,"Guest hatch warning/worm replica missing",15);ClearEnemies();
  game.World.Changes.MillDefeated=true;game.World.Changes.CampaignBosses.Clear();game.World.Changes.CampaignBosses.AddRange(new[]{0,1});
  var site=ExpeditionCampaign.Site(game.World,2);Teleport(site.X,site.Z+15);actor.Body.Position=player.Position+Vector3.Right*2;
  Rpc(nameof(CoopWorldUpdate),Json.Write(WorldSnapshot()));EnterDungeon(HeartDungeon());
  actor.Body.Position=new(0,.1f,-18.5f);actor.Game.Craft.Weapons.Add(19);actor.Game.Craft.Weapon=19;actor.Game.Craft.Arrows=2;actor.Game.Shield=40;actor.Shot=0;
  int guestHearts=actor.Game.Life.LegendaryCores,hostHearts=game.Life.LegendaryCores;
  SendCoopPersonal(actor.Id,CoopProfileJson(actor.Game),"",actor.Body.Position,true);RpcId(actor.Id,nameof(CoopCheckStage),"campaign-heart",actor.Body.Position);
  await CoopWait(()=>infected.Any(e=>e.Id==CampaignBossId(2)),"Guest could not activate final encounter",15);var finalBoss=infected.Single(e=>e.Id==CampaignBossId(2));finalBoss.Hp=1;finalBoss.BossState="recover";finalBoss.BossTime=30;
  await CoopWait(()=>campaignAck>=3,"Guest final boss reward/journal missing",20);
  MigrationChecks.Assert(!paused&&game.World.Changes.CampaignBosses.Contains(2)&&actor.Game.Life.LegendaryCores==guestHearts+1&&game.Life.LegendaryCores==hostHearts+1,"Final reward isolation and shared completion");
  ExitDungeon();Teleport(0,12);actor.Body.Position=HostPosition+Vector3.Right*3;SendCoopPersonal(actor.Id,CoopProfileJson(actor.Game),"",actor.Body.Position,true);
  GD.Print("PASS / two-process campaign: passive weeper/guest aggression, bloater hatch warning and worms, custom dungeon travel, guest boss activation/projectile kill, shared victory and duplicate-safe personal legendary rewards");
 }
 async Task ClientCampaignChecks(string stage,Vector3 target){
  await CoopWait(()=>Distance(player.Position,target)<1,"Campaign test position missing",15);
  if(stage=="campaign-heart"){
   await CoopWait(()=>InHeartRoom,"Custom heart dungeon not replicated",15);int before=game.Life.LegendaryCores;
   CoopRequest("campaign","heart");await CoopWait(()=>infected.Any(e=>IsCampaignBoss(e.Id)),"Final boss replica missing",15);
   var boss=infected.Single(e=>IsCampaignBoss(e.Id));await CoopWait(()=>boss.Hp==1&&boss.BossState=="recover","Final projectile target not ready");await AimCoopFixture(boss,1.6f);CoopRequest("shoot");
   await CoopWait(()=>game.World.Changes.CampaignBosses.Contains(2)&&game.Life.LegendaryCores==before+1,"Legendary heart reward missing",15);
   CoopRequest("campaign","claim");await CoopWait(()=>paused,"Guest journey panel missing");CloseMenu();CoopRequest("campaign","claim");await CoopDelay(.3);CloseMenu();
   if(game.Life.LegendaryCores!=before+1)throw new Exception("Legendary reward duplicated");RpcId(1,nameof(CoopCampaignAck),3);return;
  }
  string id=stage=="campaign-weeper"?"special/weeper/coop":"special/bloater/coop";
  await CoopWait(()=>infected.Any(e=>e.Id==id),"Special infected replica missing");var enemy=infected.Single(e=>e.Id==id);
  if(stage=="campaign-weeper"&&!DormantWeeper(enemy))throw new Exception("Remote weeper was not passive");
  await AimCoopFixture(enemy,.8f);CoopRequest("shoot");
  if(stage=="campaign-weeper"){await CoopWait(()=>!DormantWeeper(enemy),"Remote weeper did not wake");RpcId(1,nameof(CoopCampaignAck),1);}
  else{await CoopWait(()=>broodHatches.Count>0,"Guest green hatch warning absent");await CoopWait(()=>infected.Any(e=>InfectedKind(e)=="worm"),"Worm replicas absent");RpcId(1,nameof(CoopCampaignAck),2);}
 }
}
