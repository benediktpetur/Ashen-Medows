using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
public partial class MigrationScene {
 readonly Dictionary<string,double[]> megaGrowthDamage=new();
 sealed class MegaProjectile {public string Boss,Room;public Vector3 Position,Velocity;public double Life=4;public Node3D Mesh;}
 readonly List<MegaProjectile> megaProjectiles=new();
 static bool IsMegaNest(string id)=>id?.StartsWith("mega-nest/")==true;
 double MegaResistance(Infected e)=>e.BossState=="recover"?1.5:MegaVariant(e.Id)==0?(e.MegaShell==7?.9:.45)+game.UpgradePower(0)/100.0:MegaVariant(e.Id)==1&&e.BossState=="wave"?.2:1;
 void MegaWeakpoint(Infected e,Vector3 point,double damage){
  if(!IsMega(e.Id)||MegaVariant(e.Id)!=0)return;if(!megaGrowthDamage.TryGetValue(e.Id,out var hits))megaGrowthDamage[e.Id]=hits=new double[3];
  for(int i=0;i<3;i++){var node=e.Visual.GetNode<Node3D>("Torso/Growth"+i);if((e.MegaShell&(1<<i))!=0||point.DistanceTo(node.GlobalPosition)>.65f)continue;
   hits[i]+=damage;if(hits[i]>=180){e.MegaShell|=1<<i;e.BossState="recover";e.BossTime=5;e.Hp=Math.Max(1,e.Hp-250);node.Visible=false;ImpactParticles(node.GlobalPosition,new(.93f,.54f,.16f),16,.08f);game.Notice="SHELL BROKEN / exposed for five seconds";}return;
  }
 }
 void ClearMegaChildren(Infected boss){
  foreach(var e in infected.Where(e=>e.Id.StartsWith("mega-nest/"+boss.Id+"/")||e.Id.StartsWith(boss.Id+"/wave/")).ToArray())RemoveEnemy(e);
  foreach(var p in megaProjectiles.Where(p=>p.Boss==boss.Id).ToArray()){p.Mesh?.QueueFree();megaProjectiles.Remove(p);}megaGrowthDamage.Remove(boss.Id);
 }
 void MegaNests(Infected boss){
  for(int i=0;i<3;i++){var p=new Vector3(i==0?-10:i==1?10:0,0,i==2?-25:-3);string id="mega-nest/"+boss.Id+"/"+i;AddEnemy(id,p.X,p.Z,2);var n=infected.FirstOrDefault(e=>e.Id==id);if(n!=null){n.Hp=n.MaxHp=210;var shape=n.Body.GetChild<CollisionShape3D>(0);shape.Shape=new CapsuleShape3D{Radius=.6f,Height=1.5f};shape.Position=new(0,.75f,0);}}
 }
 void MegaWave(Infected boss){
  int nests=infected.Count(e=>e.Id.StartsWith("mega-nest/"+boss.Id+"/")),count=3+nests*2;
  for(int i=0;i<count;i++){var p=new Vector3(i%2==0?-11:11,0,-23+(i%4)*6);if(MillPlayers().Any(t=>Distance(t.Body.Position,p)<4))p=new Vector3(-p.X,0,-16);
   string id=boss.Id+"/wave/"+boss.BossStep+"/"+i;AddEnemy(id,p.X,p.Z,i%3==0?3:2);var add=infected.FirstOrDefault(e=>e.Id==id);if(add!=null){add.Memory=30;add.Target=boss.Target;}}
 }
 void TickMega(Infected boss,double dt){
  var players=MillPlayers().Where(p=>p.Game.Life.Health>0&&Distance(p.Body.Position,boss.Home)<65).ToArray();
  if(players.Length==0){boss.Memory+=dt;if(boss.Memory>8){ClearMegaChildren(boss);campaignParticipants.Remove(boss.Id);RemoveEnemy(boss);ClearBrood();ClearMillPools();}return;}
  boss.Memory=0;int v=MegaVariant(boss.Id);boss.BossTime-=dt;boss.Cooldown-=dt;bool angry=boss.Hp<boss.MaxHp*.5;
  var target=players.OrderBy(p=>Distance(p.Body.Position,boss.Body.Position)).First();boss.Target=target.Body.Position;
  if(!campaignParticipants.TryGetValue(boss.Id,out var participants))campaignParticipants[boss.Id]=participants=new();foreach(var p in players)participants.Add(p.Game);
  if(v==1&&boss.BossStep==0&&!infected.Any(e=>e.Id.StartsWith("mega-nest/"+boss.Id+"/")))MegaNests(boss);
  if(boss.BossState=="wave"){
   if(!infected.Any(e=>e.Id.StartsWith(boss.Id+"/wave/"))){boss.BossState="recover";boss.BossTime=12;}
   SpecialMove(boss,Vector3.Zero,dt);return;
  }
  if(boss.BossState=="volley"){
   if(boss.Cooldown<=0){boss.Cooldown=.24;EnemySound(boss.Body.Position,3);for(int s=-1;s<=1;s++){Vector3 origin=boss.Body.Position+new Vector3(s*.65f,2.3f,-.1f);var aim=boss.BossAim+new Vector3(s*.9f,1,0);SpawnMegaProjectile(boss.Id,origin,(aim-origin).Normalized()*18);}}
   if(boss.BossTime<=0){boss.BossState="recover";boss.BossTime=5;}return;
  }
  if(boss.BossState=="ram"){
   var dir=boss.BossAim-boss.Body.Position;dir.Y=0;SpecialMove(boss,dir.Length()>.5f?dir.Normalized()*9:Vector3.Zero,dt);SpecialHit(boss,2.5f,36,true);
   if(boss.BossTime<=0||dir.Length()<1||Enumerable.Range(0,boss.Body.GetSlideCollisionCount()).Any(i=>Math.Abs(boss.Body.GetSlideCollision(i).GetNormal().Y)<.4)){boss.BossState="recover";boss.BossTime=4;}return;
  }
  if(boss.BossState=="hunt"){
   var dir=PursuitDirection(boss,target.Body.Position,dt);float speed=v==0?.65f:v==1?.5f:1.8f;
   SpecialMove(boss,Distance(boss.Body.Position,target.Body.Position)>(v==2?8:3)?dir.Normalized()*speed:Vector3.Zero,dt);
   if(boss.BossTime<=0){boss.BossStep++;boss.BossAim=target.Body.Position;boss.BossHits.Clear();boss.BossState=v==0?(boss.BossStep%2==0?"ram-windup":"rupture-windup"):v==1?"hatch-windup":boss.BossStep%2==0?"grenade-windup":"volley-windup";boss.BossTime=(v==0?2.8:2.3)*(angry?.85:1);}return;
  }
  SpecialMove(boss,Vector3.Zero,dt);if(boss.BossTime>0)return;
  if(boss.BossState=="recover"){boss.BossState="hunt";boss.BossTime=angry?1.5:3;return;}
  if(boss.BossState=="hatch-windup"){MegaWave(boss);boss.BossState="wave";boss.BossTime=0;return;}
  if(boss.BossState=="volley-windup"){boss.BossState="volley";boss.BossTime=1.4;boss.Cooldown=0;return;}
  if(boss.BossState=="ram-windup"){boss.BossState="ram";boss.BossTime=1.8;return;}
  if(boss.BossState=="grenade-windup"){
   foreach(var p in players)if(Distance(p.Body.Position,boss.BossAim)<3.6&&SpecialLine(boss.BossAim,p.Body))MillDamage(p.Game,p.Body,32,false);
   ImpactParticles(boss.BossAim+Vector3.Up*.2f,new(1,.45f,.10f),28,.13f);EnemySound(boss.BossAim,3);
  }else {SpecialHit(boss,6.5f,38,false);ImpactParticles(boss.Body.Position,new(.58f,.52f,.24f),30,.10f);}
  boss.BossState="recover";boss.BossTime=4;
 }
 void SpawnMegaProjectile(string id,Vector3 origin,Vector3 velocity){AddMegaProjectile(id,origin,velocity);if(CoopHost&&coopActors.Count>0)Rpc(nameof(CoopMegaProjectile),id,origin,velocity);}
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]void CoopMegaProjectile(string id,Vector3 origin,Vector3 velocity){if(CoopClient)AddMegaProjectile(id,origin,velocity);}
 void AddMegaProjectile(string id,Vector3 origin,Vector3 velocity){Node3D mesh=null;if(DisplayServer.GetName()!="headless"){mesh=new Node3D{Position=origin};AddChild(mesh);Ellipsoid(mesh,Vector3.Zero,new(.065f,.065f,.17f),new(1,.52f,.15f));mesh.LookAt(origin+velocity);}megaProjectiles.Add(new(){Boss=id,Room=dungeon?.Id??"",Position=origin,Velocity=velocity,Mesh=mesh});}
 void TickMegaProjectiles(double dt){
  foreach(var p in megaProjectiles.ToArray()){
   p.Life-=dt;Vector3 next=p.Position+p.Velocity*(float)dt;var q=PhysicsRayQueryParameters3D.Create(p.Position,next);q.Exclude=new Godot.Collections.Array<Rid>(infected.Where(e=>e.Id==p.Boss).Select(e=>e.Body.GetRid()));var hit=GetWorld3D().DirectSpaceState.IntersectRay(q);
   if(hit.Count>0&&!CoopClient){var target=hit["collider"].AsGodotObject();foreach(var player in MillPlayers())if(player.Body==target)MillDamage(player.Game,player.Body,9,false);}
   if(hit.Count>0||p.Life<=0||p.Room!=(dungeon?.Id??"")){p.Mesh?.QueueFree();megaProjectiles.Remove(p);}else{p.Position=next;if(p.Mesh!=null)p.Mesh.Position=next;}
  }
 }
}
