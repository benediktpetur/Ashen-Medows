using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MigrationScene {
 // Rendering stays local. Additional host chunks contain terrain/obstacle physics only.
 const float CoopEnemyRadius=112,CoopCullRadius=150;
 const int CoopEnemiesPerArea=48,CoopMaxEnemies=384;
 readonly Dictionary<long,double> coopNightTimes=new();
 Vector3 HostPosition=>GetNode<CharacterBody3D>("Survivor").Position;
 List<Vector3> ActiveSurfaceCenters(){var centers=new List<Vector3>{HostPosition};if(CoopHost&&dungeon==null)centers.AddRange(coopActors.Values.Select(a=>a.Body.Position));return centers;}
 bool NearActiveSurface(Vector3 position,float range)=>ActiveSurfaceCenters().Any(p=>Distance(p,position)<=range);
 bool LocalChunk(int x,int z){var p=HostPosition;return Math.Abs(x-ExpeditionWorld.Cell(p.X))<=3&&Math.Abs(z-ExpeditionWorld.Cell(p.Z))<=3;}
 bool ChunkLoaded(Vector3 p)=>chunks.ContainsKey((ExpeditionWorld.Cell(p.X),ExpeditionWorld.Cell(p.Z)));
 void RemoveChunk((int,int) key){var root=chunks[key];terrainRoot.RemoveChild(root);root.QueueFree();chunks.Remove(key);props.RemoveAll(p=>(p.Kind==0||p.Kind==7)&&ExpeditionWorld.Cell(p.Position.X)==key.Item1&&ExpeditionWorld.Cell(p.Position.Z)==key.Item2);}
 void StreamCoopTerrain(bool immediate){var local=HostPosition;int cx=ExpeditionWorld.Cell(local.X),cz=ExpeditionWorld.Cell(local.Z);ArtHorizon(cx,cz);
  var wanted=new Dictionary<(int,int),bool>();var order=new List<(int,int)>();var centers=ActiveSurfaceCenters();
  // Interleave players by ring so a new remote area cannot starve local streaming.
  for(int r=0;r<=3;r++)foreach(var center in centers){int x=ExpeditionWorld.Cell(center.X),z=ExpeditionWorld.Cell(center.Z);for(int dx=-r;dx<=r;dx++)for(int dz=-r;dz<=r;dz++){
   if(Math.Max(Math.Abs(dx),Math.Abs(dz))!=r||!game.World.InBlock((x+dx)*64+32,(z+dz)*64+32))continue;var key=(x+dx,z+dz);bool full=LocalChunk(key.Item1,key.Item2);if(!wanted.ContainsKey(key)){wanted[key]=full;order.Add(key);}else wanted[key]|=full;
  }}
  foreach(var key in chunks.Keys.ToArray())if(!wanted.TryGetValue(key,out bool full)||chunks[key].GetMeta("simulation_only",false).AsBool()==full)RemoveChunk(key);
  int budget=immediate?49:4;foreach(var key in order){if(budget<=0)break;if(chunks.ContainsKey(key))continue;chunks[key]=CreateChunk(key.Item1,key.Item2,!wanted[key]);budget--;}
  foreach(var actor in coopActors.Values)actor.Body.Visible=Distance(actor.Body.Position,local)<220;foreach(var prop in props)if(prop.Kind!=0&&prop.Kind!=7)prop.Node.Visible=Distance(prop.Position,local)<240;
 }
 void PhysicsObstacle(Node3D root,Shape3D shape,Vector3 position){var body=new StaticBody3D();body.AddChild(new CollisionShape3D{Shape=shape,Position=position});root.AddChild(body);}
 void SimulationStreetCollisions(Node3D root,int cx,int cz){int z=cz*64+8;double x=game.World.RouteX(z);if(x<cx*64||x>=(cx+1)*64||game.World.Tier(x,z)<3||!game.World.Playable(x,z))return;foreach(int side in new[]{-1,1}){var shop=new Node3D{Position=Ground(x+side*14,z),RotationDegrees=new Vector3(0,side<0?-90:90,0)};root.AddChild(shop);PhysicsObstacle(shop,new BoxShape3D{Size=new Vector3(7,3.5f,5)},new Vector3(0,1.75f,0));}}
 void SpawnCoopInfected(){if(dungeon!=null)return;var centers=ActiveSurfaceCenters();
  foreach(var enemy in infected.ToArray())if(!centers.Any(p=>Distance(enemy.Body.Position,p)<CoopCullRadius)||enemy.Night>0&&!game.Night)RemoveEnemy(enemy);
  var areas=new List<(long,Vector3)>{(1,HostPosition)};areas.AddRange(coopActors.Values.OrderBy(a=>a.Id).Select(a=>(a.Id,a.Body.Position)));
  foreach(var area in areas){var position=area.Item2;int count=infected.Count(e=>Distance(e.Body.Position,position)<CoopEnemyRadius);int cx=(int)Math.Floor(position.X/128),cz=(int)Math.Floor(position.Z/128);
   SpawnSurfacePopulation(position,CoopEnemiesPerArea,CoopEnemyRadius);count=infected.Count(e=>Distance(e.Body.Position,position)<CoopEnemyRadius);
   if(!game.Night||game.World.Tier(position.X,position.Z)<2||coopNightTimes.TryGetValue(area.Item1,out double due)&&game.Clock<due)continue;
   coopNightTimes[area.Item1]=game.Clock+45;for(int i=0;i<4&&count<CoopEnemiesPerArea&&infected.Count<CoopMaxEnemies;i++){double angle=game.Clock+i*1.7;double x=position.X+Math.Sin(angle)*32,z=position.Z+Math.Cos(angle)*32;if(game.World.Tier(x,z)<2||game.World.SpawnHeat(x,z)==0||!ChunkLoaded(Ground(x,z)))continue;AddEnemy("night/"+(int)game.World.Changes.Time+"/peer/"+area.Item1+"/"+i,x,z,game.World.Tier(x,z),i%2+1);count++;}
  }
 }
 bool CoopPartyReady(){if(!CoopHost||coopApplying||dungeon!=null)return true;if(coopActors.Values.Any(a=>Distance(a.Body.Position,HostPosition)>24)){game.Notice="Regroup within 24m of the host before entering a party dungeon.";return false;}return true;}
 void RefreshCoopEnemyVisual(Infected enemy){bool visible=Distance(enemy.Body.Position,HostPosition)<160;if(visible&&enemy.Visual.GetChildCount()==0)ActorArt(enemy.Visual,enemy.Id,enemy.Tier,enemy.Night);else if(!visible&&enemy.Visual.GetChildCount()>0)ClearChildren(enemy.Visual);enemy.Visual.Visible=visible;}
 CoopFrame FrameFor(Vector3 viewer){var frame=new CoopFrame{Room=dungeon?.Id??"",Fever=Fever,LastLetter=LastLetter,Checkpoint=Checkpoint,Community=CommunityFrame(viewer)};frame.Players.Add(ActorState(1,coopName,GetNode<CharacterBody3D>("Survivor"),pitch));foreach(var a in coopActors.Values)frame.Players.Add(ActorState(a.Id,a.Name,a.Body,a.Pitch));foreach(var e in infected.Where(e=>dungeon!=null||Distance(e.Body.Position,viewer)<CoopCullRadius).OrderBy(e=>Distance(e.Body.Position,viewer)).Take(96))frame.Enemies.Add(new CoopEnemyState{Id=e.Id,X=e.Body.Position.X,Y=e.Body.Position.Y,Z=e.Body.Position.Z,Yaw=e.Body.Rotation.Y,Tier=e.Tier,Night=e.Night,Hp=e.Hp,MaxHp=e.MaxHp,BossState=e.BossState,BossTime=e.BossTime,BossStep=e.BossStep,MegaShell=e.MegaShell,AttackState=e.AttackState,AttackTime=e.AttackTime,Stagger=e.Stagger,Memory=e.Memory,AimX=e.BossAim.X,AimY=e.BossAim.Y,AimZ=e.BossAim.Z});frame.Brood=broodHatches.Where(h=>Distance(h.Point,viewer)<150).Select(h=>new BroodHatchState{Id=h.Id,X=h.Point.X,Y=h.Point.Y,Z=h.Point.Z,Time=h.Time}).ToList();frame.MillPools=millPools.Where(p=>Distance(p.Position,viewer)<150).Select(p=>new MillPoolState{Kind=p.Kind,X=p.Position.X,Y=p.Position.Y,Z=p.Position.Z,Remaining=p.Remaining}).ToList();return frame;}
}
