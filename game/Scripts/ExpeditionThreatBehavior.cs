using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
public partial class MigrationScene {
 readonly Dictionary<string,ExpeditionGame> cinderOwners=new();
 void ClaimThreatKill(Infected e,bool train){var credited=game;if(InfectedKind(e)=="cinder"&&cinderOwners.Remove(e.Id,out var owner))credited=owner;credited.Kill(e.Id,e.Tier,e.Night,train);}
 double RiotResistance(Infected e){var delta=player.Position-e.Body.Position;delta.Y=0;return delta.LengthSquared()>.01&&(-e.Body.Basis.Z).Dot(delta.Normalized())>.35?Math.Min(.7,.25+game.UpgradePower(0)/100.0):1;}
 bool PrimeCinder(Infected e){if(InfectedKind(e)!="cinder"||e.BossState=="cinder-spent")return false;e.Hp=1;if(e.BossState!="cinder-death"){cinderOwners[e.Id]=game;e.BossState="cinder-death";e.BossTime=1.8;e.AttackState="";e.Body.Velocity=Vector3.Zero;game.Notice="CINDER IGNITING / get away from the body";}return true;}
 void InterruptThreat(Infected e){if(ExpeditionThreat.Find(InfectedKind(e))==null||e.BossState=="cinder-death")return;if(e.BossState.EndsWith("-windup")){e.BossState="threat-recover";e.BossTime=1.4;}}
 bool TickNewThreat(Infected e,double dt){
  string kind=InfectedKind(e);var spec=ExpeditionThreat.Find(kind);if(spec==null)return false;
  if(e.BossState=="cinder-death"){e.BossTime-=dt;SpecialMove(e,Vector3.Zero,dt);if(e.BossTime<=0){e.BossState="cinder-spent";e.BossHits.Clear();SpecialHit(e,3.2f,15,false);CreateMillPool(e.Body.Position,5,1);HearNoise(e.Body.Position,45);KillEnemy(e);}return true;}
  var targets=MillPlayers().Where(p=>p.Game.Life.Health>0).OrderBy(p=>Distance(p.Body.Position,e.Body.Position)).ToArray();if(targets.Length==0)return true;var target=targets[0];float distance=Distance(e.Body.Position,target.Body.Position);
  var sightDelta=target.Body.Position-e.Body.Position;bool visible=distance<(e.Memory>0?30:target.Game.VisualRange)&&(distance<4||e.Memory>0||(-e.Body.Basis.Z).Dot(sightDelta.Normalized())>.1f)&&SpecialLine(e.Body.Position,target.Body);e.BossTime=Math.Max(0,e.BossTime-dt);
  if(e.BossState=="threat-hidden"){
   if(distance<(kind=="gravecrawler"?4:7)||e.Memory>0||e.Hp<e.MaxHp){e.BossState="threat-hunt";e.Memory=10;EnemySound(e.Body.Position,0);}else{SpecialMove(e,Vector3.Zero,dt);PoseNewThreat(e);return true;}
  }
  if(e.BossState=="threat-rush"){
   var delta=e.BossAim-e.Body.Position;delta.Y=0;SpecialMove(e,delta.Length()>.4f?delta.Normalized()*(kind=="palehunter"?11:9):Vector3.Zero,dt);SpecialHit(e,kind=="palehunter"?1.8f:2.1f,kind=="palehunter"?27:23,true);
   bool wall=Enumerable.Range(0,e.Body.GetSlideCollisionCount()).Any(i=>Math.Abs(e.Body.GetSlideCollision(i).GetNormal().Y)<.4);
   if(e.BossTime<=0||delta.Length()<.8||wall){e.BossState="threat-recover";e.BossTime=kind=="roadbreaker"?2.8:1.8;}PoseNewThreat(e);return true;
  }
  if(e.BossState=="threat-recover"){
   SpecialMove(e,Vector3.Zero,dt);if(e.BossTime<=0){e.BossState="threat-hunt";e.BossTime=kind=="howler"?12:kind=="bilecaster"?5:kind=="wirebound"?6:4;}PoseNewThreat(e);return true;
  }
  if(e.BossState.EndsWith("-windup")){
   SpecialMove(e,Vector3.Zero,dt);var direction=e.BossAim-e.Body.Position;if(direction.LengthSquared()>.01)e.Body.Rotation=new(0,Mathf.Atan2(-direction.X,-direction.Z),0);
   if(e.BossTime<=0){
    if(kind=="roadbreaker"||kind=="palehunter"){e.BossState="threat-rush";e.BossTime=kind=="palehunter"?.75:1.2;e.BossHits.Clear();}
    else{if(kind=="howler"){HearNoise(e.Body.Position,80);EnemySound(e.Body.Position,0);}
     if(kind=="bilecaster"){CreateMillPool(e.BossAim,6);ImpactParticles(e.Body.Position+Vector3.Up*1.45f,new(.38f,.65f,.13f),10,.05f);}
     if(kind=="wirebound")foreach(var p in targets)if(Distance(p.Body.Position,e.BossAim)<2.2f&&SpecialLine(e.Body.Position,p.Body)){p.Game.Stamina=Math.Max(0,p.Game.Stamina-35);MillDamage(p.Game,p.Body,8,false);}
     e.BossState="threat-recover";e.BossTime=kind=="howler"?2:1.7;
    }
   }PoseNewThreat(e);return true;
  }
  if(visible&&e.BossTime<=0&&e.Stagger<=0&&e.AttackState==""&&((kind=="howler"&&distance<20)||(kind=="bilecaster"&&distance>6&&distance<23)||(kind=="wirebound"&&distance<6)||(kind=="roadbreaker"&&distance>4&&distance<16)||(kind=="palehunter"&&distance>3&&distance<13))){
   e.BossState=kind+"-windup";e.BossTime=kind=="howler"?1.8:kind=="bilecaster"?1.4:1.2;e.BossAim=target.Body.Position;e.Memory=10;PoseNewThreat(e);return true;
  }
  if(kind=="palehunter"&&visible&&distance>6&&distance<20&&e.BossTime>0){var flank=target.Body.Position+target.Body.Basis.X*4;SpecialMove(e,PursuitDirection(e,flank,dt).Normalized()*4,dt);PoseNewThreat(e);return true;}
  var health=kind=="blooddrinker"?targets.Select(p=>p.Game.Life.Health).ToArray():null;
  TickZombie(e,dt);if(health!=null&&targets.Where((p,i)=>p.Game.Life.Health<health[i]).Any())e.Hp=Math.Min(e.MaxHp,e.Hp+30);
  PoseNewThreat(e);return true;
 }
}
