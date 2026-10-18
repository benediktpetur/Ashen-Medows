using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

[DataContract] sealed class BroodHatchState {
 [DataMember] public string Id;
 [DataMember] public float X,Y,Z;
 [DataMember] public double Time;
}

public partial class MigrationScene {
 float EnemyHeadHeight(Infected e)=>IsMillBoss(e.Id)?3.7f:IsCampaignBoss(e.Id)?e.Tier==5?2.8f:2.45f:IsSammy(e.Id)?.86f:InfectedKind(e)=="gravecrawler"?.85f:InfectedKind(e)=="worm"?100:DormantWeeper(e)?.9f:InfectedKind(e) is "bloater" or "bruiser"?1.7f:1.35f;
 string InfectedKind(Infected e)=>ExpeditionCampaign.Kind(e.Id,e.Tier,e.Night);
 bool IsCampaignBoss(string id)=>ExpeditionCampaign.BossIndex(id)>=0;
 bool DormantWeeper(Infected e)=>InfectedKind(e)=="weeper"&&e.BossState=="weeping";
 sealed class BroodHatch {public string Id;public Vector3 Point;public int Tier,Count;public double Time=1.25;public Node3D Warning;}
 readonly List<BroodHatch> broodHatches=new();
 void ConfigureSpecial(Infected e){
  if(IsMegaNest(e.Id))return;string kind=InfectedKind(e);if(kind=="walker")return;
  e.Hp=e.MaxHp=ExpeditionCampaign.Hp(kind,e.Tier);
  if(kind=="weeper")e.BossState="weeping";
  if(ExpeditionThreat.Find(kind) is {} spec){e.BossState=kind is "mossback" or "gravecrawler"?"threat-hidden":"threat-hunt";e.BossTime=3;var capsule=e.Body.GetChild<CollisionShape3D>(0);if(kind=="gravecrawler"){capsule.Shape=new CapsuleShape3D{Radius=.38f,Height=1.1f};capsule.Position=new(0,.55f,0);}return;}
  var shape=e.Body.GetChild<CollisionShape3D>(0);
  if(kind=="weeper"){shape.Shape=new CapsuleShape3D{Radius=.4f,Height=1.2f};shape.Position=new(0,.6f,0);}
  if(kind=="worm"){shape.Shape=new CapsuleShape3D{Radius=.15f,Height=.3f};shape.Position=new(0,.15f,0);e.BossTime=90;}
  else if(kind=="bloater"||kind=="bruiser"){shape.Shape=new CapsuleShape3D{Radius=.55f,Height=2.1f};shape.Position=new(0,1.05f,0);}
 }
 double InfectedResistance(Infected e,bool head)=>IsCampaignBoss(e.Id)?IsMega(e.Id)?MegaResistance(e):e.BossState=="recover"?1.5:1:InfectedKind(e)=="riot"&&!head?RiotResistance(e):InfectedKind(e)=="armored"&&!head?.65+game.UpgradePower(0)/100.0:1;
 float InfectedSpeed(Infected e,float normal)=>ExpeditionThreat.Find(InfectedKind(e)) is {} spec?normal*(float)spec.Speed:InfectedKind(e) switch{"runner"=>normal*1.5f,"stalker"=>normal*1.35f,"bloater"=>normal*.65f,"bruiser"=>normal*.8f,_=>normal};
 void QueueBrood(Infected e,int count=0){
  if(CoopClient||broodHatches.Any(h=>h.Id==e.Id))return;
  var warning=new Node3D{Position=e.Body.Position};AddChild(warning);MillRing(warning,2.2f,new(.62f,.8f,.13f,.85f));
  broodHatches.Add(new(){Id=e.Id,Point=e.Body.Position,Tier=e.Tier,Count=count>0?count:Math.Clamp(e.Tier+1,3,5),Warning=warning});
 }
 void TickBrood(double dt){
  if(CoopClient)return;
  foreach(var hatch in broodHatches.ToArray()){
   hatch.Time-=dt;if(hatch.Time>0)continue;
   hatch.Warning.QueueFree();broodHatches.Remove(hatch);
   for(int i=0;i<hatch.Count&&infected.Count<(CoopHost?CoopMaxEnemies:85);i++){
    float a=i*Mathf.Tau/hatch.Count;var p=hatch.Point+new Vector3(Mathf.Sin(a)*.6f,0,Mathf.Cos(a)*.6f);
    string id=$"brood/worm/{hatch.Id}/{i}";AddEnemy(id,p.X,p.Z,hatch.Tier);
    var worm=infected.FirstOrDefault(e=>e.Id==id);if(worm!=null){worm.Body.Position=p;worm.Home=p;worm.Memory=12;worm.Target=player.Position;}
   }
  }
 }
 void ClearBrood(){foreach(var h in broodHatches)h.Warning.QueueFree();broodHatches.Clear();}
 void ApplyBroodWarnings(List<BroodHatchState> states){
  if(!CoopClient)return;states??=new();
  foreach(var old in broodHatches.Where(h=>!states.Any(s=>s.Id==h.Id)).ToArray()){old.Warning.QueueFree();broodHatches.Remove(old);}
  foreach(var state in states){
   var hatch=broodHatches.FirstOrDefault(h=>h.Id==state.Id);
   if(hatch==null){var warning=new Node3D{Position=new(state.X,state.Y,state.Z)};AddChild(warning);MillRing(warning,2.2f,new(.62f,.8f,.13f,.85f));hatch=new(){Id=state.Id,Point=warning.Position,Warning=warning};broodHatches.Add(hatch);}
   hatch.Time=state.Time;
  }
 }
 bool TickSpecialInfected(Infected e,double dt){if(IsMegaNest(e.Id))return true;if(TickNewThreat(e,dt))return true;
  string kind=InfectedKind(e);if(kind!="weeper"&&kind!="worm")return false;
  if(kind=="worm"){TickWorm(e,dt);return true;}
  if(DormantWeeper(e)&&e.Hp<e.MaxHp)ReactEnemyHit(e,false);
  if(DormantWeeper(e)){
   e.Cooldown-=dt;e.Memory=0;e.AttackState="";e.Body.Velocity=Vector3.Zero;
   if(e.Cooldown<=0){e.Cooldown=6;HearNoise(e.Body.Position,32);}
   PoseWeeper(e);return true;
  }
  e.Stagger=Math.Max(0,e.Stagger-dt);e.BossTime-=dt;
  var target=MillPlayers().Where(p=>p.Game.Life.Health>0).OrderBy(p=>Distance(p.Body.Position,e.Body.Position)).FirstOrDefault();
  if(target.Body==null)return true;
  float distance=Distance(e.Body.Position,target.Body.Position);
  if(distance>75){e.Memory-=dt;if(e.Memory<=0){e.BossState="weeping";e.Hp=e.MaxHp;e.Body.Position=e.Home;}return true;}e.Memory=12;
  if(e.BossState=="weeper-windup"){
   e.AttackState="windup";e.Target=e.BossAim;var facing=e.BossAim-e.Body.Position;if(facing.LengthSquared()>.01)e.Body.Rotation=new(0,Mathf.Atan2(-facing.X,-facing.Z),0);
   if(e.BossTime<=0){e.BossState="weeper-charge";e.BossTime=.95;e.BossHits.Clear();}PoseWeeper(e);return true;
  }
  if(e.BossState=="weeper-charge"){
   var dir=e.BossAim-e.Body.Position;dir.Y=0;
   SpecialMove(e,dir.Length()>.3f?dir.Normalized()*10:Vector3.Zero,dt);
   SpecialHit(e,2.1f,32,true);
   bool wall=Enumerable.Range(0,e.Body.GetSlideCollisionCount()).Any(i=>Math.Abs(e.Body.GetSlideCollision(i).GetNormal().Y)<.4);
   if(e.BossTime<=0||wall){e.BossState="weeper-recover";e.BossTime=2.4;e.AttackState="recover";}PoseWeeper(e);return true;
  }
  if(e.BossState=="weeper-recover"){if(e.BossTime<=0)e.BossState="weeper-hunt";PoseWeeper(e);return true;}
  if(distance<2.5&&e.BossTime<=0){e.BossState="weeper-windup";e.BossTime=.85;e.BossAim=target.Body.Position;e.BossHits.Clear();}
  else if(distance>4&&e.BossTime<=0){e.BossState="weeper-windup";e.BossTime=1.2;e.BossAim=e.Body.Position+(target.Body.Position-e.Body.Position).Normalized()*14;e.BossHits.Clear();}
  else{e.AttackState="";SpecialMove(e,PursuitDirection(e,target.Body.Position,dt).Normalized()*3.4f,dt);}
  PoseWeeper(e);return true;
 }
 bool SpecialLine(Vector3 from,CharacterBody3D target){var q=PhysicsRayQueryParameters3D.Create(from+Vector3.Up*.6f,target.Position+Vector3.Up*.6f);q.Exclude=PursuitExclusions();return GetWorld3D().DirectSpaceState.IntersectRay(q).Count==0;}
 void SpecialHit(Infected e,float radius,double damage,bool wound){
  foreach(var p in MillPlayers())if(p.Game.Life.Health>0&&Distance(p.Body.Position,e.Body.Position)<radius&&Math.Abs(p.Body.Position.Y-e.Body.Position.Y)<2.5&&!e.BossHits.Contains(p.Body.GetInstanceId())&&SpecialLine(e.Body.Position,p.Body)){
   e.BossHits.Add(p.Body.GetInstanceId());MillDamage(p.Game,p.Body,damage,wound);
  }
  if(Dog.Rescued&&!Dog.Dead&&DogPresent&&Distance(DogPoint,e.Body.Position)<radius&&e.BossHits.Add(rescueDogActor.Body.GetInstanceId())&&SpecialLine(e.Body.Position,rescueDogActor.Body))HurtRescueDog(damage);
  if(ContentEnabled&&FieldEvent.Stage==1&&FieldEvent.Kind>0&&Distance(Ground(FieldEvent.X,FieldEvent.Z),e.Body.Position)<radius&&e.BossHits.Add(0xF13DUL))HurtFieldCivilian(damage);
  foreach(var npc in Community.Survivors.Where(n=>n.Member&&n.Health>0))if(Distance(NpcPoint(npc),e.Body.Position)<radius&&!e.BossHits.Contains((ulong)npc.Id.GetHashCodeStable())){e.BossHits.Add((ulong)npc.Id.GetHashCodeStable());HurtCommunity(npc,damage);}
 }
 void SpecialMove(Infected e,Vector3 direction,double dt){
  direction.Y=e.Body.IsOnFloor()?-.1f:e.Body.Velocity.Y-18*(float)dt;e.Body.Velocity=direction;e.Body.MoveAndSlide();
  if(direction.X*direction.X+direction.Z*direction.Z>.01)e.Body.Rotation=new(0,Mathf.Atan2(-direction.X,-direction.Z),0);
  if(dungeon==null&&e.Body.Position.Y<game.World.Height(e.Body.Position.X,e.Body.Position.Z)-8)e.Body.Position=Ground(e.Body.Position.X,e.Body.Position.Z);
 }
 void TickWorm(Infected e,double dt){
  e.BossTime-=dt;if(e.BossTime<=0){RemoveEnemy(e);return;}e.Cooldown-=dt;e.AttackTime-=dt;
  var target=MillPlayers().OrderBy(p=>Distance(p.Body.Position,e.Body.Position)).First();
  float distance=Distance(target.Body.Position,e.Body.Position);e.Target=target.Body.Position;e.Memory=12;
  if(e.AttackState=="windup"&&e.AttackTime<=0){e.BossHits.Clear();SpecialHit(e,1.2f,5,false);e.AttackState="";e.Cooldown=1.8;}
  else if(distance<1&&e.Cooldown<=0&&e.AttackState==""){e.AttackState="windup";e.AttackTime=.55;}
  if(e.AttackState=="")SpecialMove(e,PursuitDirection(e,target.Body.Position,dt).Normalized()*2.8f,dt);
  if(e.Visual.GetChildCount()>0)e.Visual.Scale=new(1,.9f+Mathf.Sin((float)game.Clock*12)*.1f,1);
 }
}
