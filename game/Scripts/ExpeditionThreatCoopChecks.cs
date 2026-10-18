using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
public partial class MigrationScene {
 async Task NewThreatCoopChecks(CoopActor actor){
  ClearEnemies();EnterDungeon(HeartDungeon());player.Position=new(5,.1f,2);actor.Body.Position=new(0,.1f,0);actor.Game.Craft.Weapons.Add(0);actor.Game.Craft.Weapon=0;actor.Game.Craft.Ammo=6;actor.Game.Shield=40;actor.Shot=actor.Reload=0;
  for(int i=0;i<10;i++){var t=ExpeditionThreat.All[i];AddEnemy(t.Kind=="riot"?"habitat/coop-ecology/riot-slot/ecology/riot":"special/"+t.Kind+"/coop-new",(i%5-2)*2.4,-7-(i/5)*5,t.Min);var e=infected.Last();e.BossState="threat-recover";e.BossTime=30;}
  var howler=infected.Single(e=>InfectedKind(e)=="howler");howler.BossState="howler-windup";howler.BossTime=30;howler.Hp=howler.MaxHp=250;
  SendCoopPersonal(actor.Id,CoopProfileJson(actor.Game),"",actor.Body.Position,true);RpcId(actor.Id,nameof(CoopCheckStage),"campaign-threats",actor.Body.Position);
  await CoopWait(()=>campaignAck>=6,"Ten enemy variants did not replicate",20);await CoopWait(()=>campaignAck>=7,"Guest could not interrupt howl",20);
  MigrationChecks.Assert(howler.Hp<250&&howler.BossState!="howler-windup","Guest shot interrupts shared howl");
  foreach(var e in infected)e.Memory=0;howler.BossState="howler-windup";howler.BossTime=0;TickCoopEnemies(.01);MigrationChecks.Assert(infected.Count(e=>e.Memory>0)>2,"Host howl alerts shared neighbors outside per-actor tick view");
  int hostAlloy=game.Craft.Materials[4],guestAlloy=actor.Game.Craft.Materials[4];var cinder=infected.Single(e=>InfectedKind(e)=="cinder");cinder.Hp=0;KillEnemy(cinder);await CoopWait(()=>!infected.Contains(cinder),"Cinder self-death retained a stale server enemy",10);await CoopWait(()=>campaignAck>=8,"Guest fire/death state missing",15);MigrationChecks.Assert(game.Craft.Materials[4]==hostAlloy+1&&actor.Game.Craft.Materials[4]==guestAlloy,"Delayed Cinder loot belongs to killer rather than nearest player");
  ClearEnemies();ClearMillPools();ExitDungeon();Teleport(0,12);actor.Body.Position=HostPosition+Vector3.Right*3;SendCoopPersonal(actor.Id,CoopProfileJson(actor.Game),"",actor.Body.Position,true);
  GD.Print("PASS / two-process ten threats: all models/crawler collision, guest-interrupted howl, shared hearing, cinder warning/self-removal and replicated burning ground");
 }
 async Task ClientNewThreatChecks(){
  await CoopWait(()=>InHeartRoom&&ExpeditionThreat.All.All(t=>infected.Any(e=>InfectedKind(e)==t.Kind)),"New enemy replicas missing",15);
  foreach(var e in infected.Where(e=>ExpeditionThreat.Find(InfectedKind(e))!=null))if(!e.Visual.HasNode("Torso"))throw new Exception("Missing remote variant model");
  if(!infected.Any(e=>e.Id=="habitat/coop-ecology/riot-slot/ecology/riot"&&InfectedKind(e)=="riot"))throw new Exception("Server habitat kind was not preserved on guest");
  var crawler=infected.Single(e=>InfectedKind(e)=="gravecrawler");if(((CapsuleShape3D)crawler.Body.GetChild<CollisionShape3D>(0).Shape).Height>1.2f)throw new Exception("Remote crawler collision mismatch");RpcId(1,nameof(CoopCampaignAck),6);
  var howler=infected.Single(e=>InfectedKind(e)=="howler");await AimCoopFixture(howler,1.1f);CoopRequest("shoot");await CoopWait(()=>howler.Hp<250&&howler.BossState!="howler-windup","Howl interruption not replicated",15);RpcId(1,nameof(CoopCampaignAck),7);
  await CoopWait(()=>infected.Any(e=>InfectedKind(e)=="cinder"&&e.BossState=="cinder-death"),"Death ignition warning missing",10);
  await CoopWait(()=>!infected.Any(e=>InfectedKind(e)=="cinder")&&millPools.Any(p=>p.Kind==1),"Burning ground or self-removal missing",15);RpcId(1,nameof(CoopCampaignAck),8);
 }
}
