using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class MigrationScene {
 readonly Dictionary<string,HashSet<ExpeditionGame>> campaignParticipants=new();
 void StartCampaignFight(int index){
  if(index<0||index>2||CampaignGate(index)!="Ready"||game.World.Changes.CampaignBosses.Contains(index)){game.Notice=index>=0&&index<3?CampaignGate(index):"Unknown encounter";return;}
  if(index==2&&!InHeartRoom)return;
  if(infected.Any(e=>IsCampaignBoss(e.Id))){game.Notice="The encounter is already underway.";return;}
  var point=ExpeditionCampaign.Site(game.World,index);var position=index==2?new Vector3(0,0,-9):Ground(point.X,point.Z);
  AddEnemy(CampaignBossId(index),position.X,position.Z,index+3);
  var boss=infected.FirstOrDefault(e=>e.Id==CampaignBossId(index));if(boss==null)return;
  campaignParticipants[boss.Id]=MillPlayers().Where(p=>Distance(p.Body.Position,boss.Home)<65).Select(p=>p.Game).ToHashSet();
  game.Notice=ExpeditionCampaign.Names[index].ToUpperInvariant()+" / watch the warning, dodge, punish recovery";
 }
 void AddCampaignBoss(string id,double x,double z){
  int index=ExpeditionCampaign.BossIndex(id);if(index<0||infected.Any(e=>e.Id==id)||game.World.Changes.CampaignBosses.Contains(index))return;
  var body=new CharacterBody3D{Name=ExpeditionCampaign.Names[index].Replace(" ",""),Position=dungeon==null?Ground(x,z):new((float)x,0,(float)z),FloorSnapLength=.5f};
  float height=index==2?3.4f:3.0f;
  body.AddChild(new CollisionShape3D{Shape=new CapsuleShape3D{Radius=index==2?1.15f:.85f,Height=height},Position=new(0,height/2,0)});
  var visual=new Node3D();body.AddChild(visual);CampaignBossArt(visual,index);AddChild(body);
  int players=CoopHost?MillPlayers().Count(p=>Distance(p.Body.Position,body.Position)<65):1;
  double hp=(index==0?1100:index==1?1600:2400)*(1+.4*Math.Max(0,players-1));
  infected.Add(new(){Id=id,Body=body,Visual=visual,Tier=index+3,Hp=hp,MaxHp=hp,Home=body.Position,Target=body.Position,BossState="hunt",BossTime=3});
 }
 void CampaignBossArt(Node3D root,int index){
  HumanCharacter(root,index+3,0,false,index==2?6:index==1?4:2);root.Scale=Vector3.One*(index==2?1.85f:1.65f);
  var torso=root.GetNode<Node3D>("Torso");var head=root.GetNode<Node3D>("Torso/Head");
  if(index==0){torso.Scale=new(1.5f,1,1.25f);foreach(string side in new[]{"Left","Right"}){var arm=root.GetNode<Node3D>(side+"Arm");arm.Scale=new(1.4f,1.2f,1.4f);Bevel(arm.GetNode<Node3D>("Elbow/Hand"),new(0,-.12f,0),new(.23f,.25f,.2f),Rust);}for(int side=-1;side<=1;side+=2)Tube(torso,new(side*.2f,.4f,0),new(side*.14f,-.15f,-.20f),.045f,Steel);}
  if(index==1){Bevel(torso,new(0,.22f,.20f),new(.38f,.42f,.2f),Steel);for(int side=-1;side<=1;side+=2){var bell=Tube(torso,new(side*.24f,.38f,.15f),new(side*.24f,.59f,.15f),.12f,new(.52f,.44f,.24f),.055f);bell.RotationDegrees=new(0,0,side*20);Box(head,new(side*.075f,.11f,-.075f),new(.06f,.04f,.04f),new(.65f,.23f,.08f));}}
  if(index==2){torso.Scale=new(2,1.1f,1.8f);for(int i=0;i<5;i++)Ellipsoid(torso,new((i%2==0?-1:1)*.13f,.02f+i*.07f,-.22f),new(.09f,.08f,.08f),new(.42f,.57f,.17f));head.Scale=new(1.1f,1.2f,1.1f);}
 }
 void TickCampaignBoss(Infected boss,double dt){
  int index=ExpeditionCampaign.BossIndex(boss.Id);
  var players=MillPlayers().Where(p=>p.Game.Life.Health>0&&Distance(p.Body.Position,boss.Home)<65).ToArray();
  if(players.Length==0){boss.Memory+=dt;if(boss.Memory>8){campaignParticipants.Remove(boss.Id);RemoveEnemy(boss);ClearBrood();ClearMillPools();}return;}
  boss.Memory=0;boss.BossTime-=dt;bool angry=boss.Hp<boss.MaxHp*.5;
  var target=players.OrderBy(p=>Distance(p.Body.Position,boss.Body.Position)).First();boss.Target=target.Body.Position;
  if(!campaignParticipants.TryGetValue(boss.Id,out var participants))campaignParticipants[boss.Id]=participants=new();
  foreach(var p in players)participants.Add(p.Game);
  if(boss.BossState=="hunt"){
   var direction=PursuitDirection(boss,target.Body.Position,dt);if(Distance(boss.Body.Position,target.Body.Position)>4)SpecialMove(boss,direction.Normalized()*(index==0?1.7f:index==1?1.5f:1.1f),dt);
   if(boss.BossTime<=0){boss.BossStep++;boss.BossAim=target.Body.Position;boss.BossHits.Clear();boss.BossState=index==0?boss.BossStep%2==1?"ram-windup":"sweep-windup":index==1?boss.BossStep%3==1?"alarm-windup":boss.BossStep%3==2?"knell-windup":"sweep-windup":boss.BossStep%3==1?"brood-windup":boss.BossStep%3==2?"bile-windup":"rupture-windup";boss.BossTime=(index==2?2.5:2)*(angry?.8:1);}
   return;
  }
  if(boss.BossState=="ram"){
   var delta=boss.BossAim-boss.Body.Position;delta.Y=0;SpecialMove(boss,delta.Length()>.4?delta.Normalized()*12:Vector3.Zero,dt);SpecialHit(boss,2.3f,28,true);
   bool wall=Enumerable.Range(0,boss.Body.GetSlideCollisionCount()).Any(i=>Math.Abs(boss.Body.GetSlideCollision(i).GetNormal().Y)<.4);
   if(wall||boss.BossTime<=0||Distance(boss.Body.Position,boss.Home)>23){boss.BossState="recover";boss.BossTime=wall?5:3.5;}return;
  }
  if(boss.BossTime>0)return;
  if(boss.BossState=="recover"){boss.BossState="hunt";boss.BossTime=angry?1.8:3;return;}
  string attack=boss.BossState;
  if(attack=="ram-windup"){var dir=boss.BossAim-boss.Body.Position;dir.Y=0;boss.BossAim=boss.Body.Position+dir.Normalized()*19;boss.BossState="ram";boss.BossTime=1.7;return;}
  if(attack is "sweep-windup" or "rupture-windup"){SpecialHit(boss,attack=="rupture-windup"?6.5f:5,attack=="rupture-windup"?30:25,false);ImpactParticles(boss.Body.Position,attack=="rupture-windup"?new(.4f,.62f,.14f):new Color(.61f,.41f,.16f),22,.12f);}
  if(attack=="knell-windup")foreach(var p in players)if(Distance(p.Body.Position,boss.BossAim)<3.6&&Math.Abs(p.Body.Position.Y-boss.BossAim.Y)<2&&SpecialLine(boss.BossAim,p.Body))MillDamage(p.Game,p.Body,24,false);
  if(attack=="alarm-windup"){HearNoise(boss.Body.Position,85);SummonCampaignAdds(boss,angry?3:2);}
  if(attack=="brood-windup"&&infected.Count(e=>InfectedKind(e)=="worm")<10){var shell=new Infected{Id=boss.Id+"/brood/"+boss.BossStep,Body=boss.Body,Tier=5};QueueBrood(shell,angry?5:4);}
  if(attack=="bile-windup"){CreateMillPool(boss.BossAim,7);}
  boss.BossState="recover";boss.BossTime=index==2?4:3.5;
 }
 void SummonCampaignAdds(Infected boss,int count){
  if(infected.Count(e=>e.Id.StartsWith(boss.Id+"/add/"))>=6)return;
  for(int i=0;i<count;i++){float a=(i+boss.BossStep)*2.4f;var p=boss.Home+new Vector3(Mathf.Sin(a)*17,0,Mathf.Cos(a)*17);if(dungeon==null)p=Ground(p.X,p.Z);if(MillPlayers().Any(t=>Distance(t.Body.Position,p)<10))continue;string id=boss.Id+"/add/"+boss.BossStep+"/"+i;AddEnemy(id,p.X,p.Z,boss.Tier);var add=infected.FirstOrDefault(e=>e.Id==id);if(add!=null){add.Memory=10;add.Target=boss.Target;}}
 }
 void DefeatCampaignBoss(Infected boss){
  int index=ExpeditionCampaign.BossIndex(boss.Id);var authority=CoopHost?coopHostGame:game;
  if(authority.World.Changes.CampaignBosses.Contains(index)){RemoveEnemy(boss);return;}
  if(!authority.Change(()=>authority.World.Changes.CampaignBosses.Add(index))){boss.Hp=1;boss.BossState="recover";boss.BossTime=4;return;}
  if(!campaignParticipants.TryGetValue(boss.Id,out var participants))participants=new();foreach(var p in MillPlayers())if(Distance(p.Body.Position,boss.Home)<70)participants.Add(p.Game);
  foreach(var character in participants)ClaimCampaignReward(character,index);
  campaignParticipants.Remove(boss.Id);ClearBrood();if(index==2)QueueBrood(boss,6);BloodImpact(boss.Body.Position+Vector3.Up,Vector3.Up,30);DeathRemains(boss);RemoveEnemy(boss);
  foreach(var add in infected.Where(e=>e.Id.StartsWith(boss.Id+"/add/")).ToArray())RemoveEnemy(add);
  ClearMillPools();authority.Notice=index==2?"SOUTH BANK SECURED / legendary heart recovered. Upgrade a maximum-tier weapon at workshop 5.":ExpeditionCampaign.Names[index].ToUpperInvariant()+" DEFEATED / new equipment, supplies and research";
  authority.Persist();if(CoopHost&&coopActors.Count>0)Rpc(nameof(CoopWorldUpdate),Json.Write(WorldSnapshot()));
 }
 void ClaimCampaignReward(ExpeditionGame character,int index){
  var world=CoopHost?coopHostGame.World:game.World;if(!world.Changes.CampaignBosses.Contains(index))return;
  string reward=$"campaign/{game.Save.Seed}/{index}/reward";if(character.Craft.ResearchAwards.Contains(reward))return;
  character.Change(()=>{
   character.AwardResearch(reward,15+index*8);character.Career.Gain(350+index*250);character.Craft.Reserve+=72+index*24;character.Craft.Arrows+=24;character.Life.Bandages+=4;character.Life.Food+=3;
   int tier=index+3;character.Craft.UnlockedTier=Math.Max(tier,character.Craft.UnlockedTier);character.Craft.Materials[tier+1]+=8;character.AwardResearch("zone/"+tier,10+tier*5);
   character.Profile.Items.Add(new(){Id=reward+"/gear",Name=index==0?"Wrecker's road helmet":index==1?"Bellkeeper's armored gloves":"Brood-sealed cuirass",Origin=ExpeditionCampaign.Names[index],Slot=index==0?"head":index==1?"hands":"chest",Level=tier,Armor=index==2?65:14+index*6,Vitality=8+index*7});
   if(index==2)character.Life.LegendaryCores++;
  });
 }
}
