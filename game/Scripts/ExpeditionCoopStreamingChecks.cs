using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
public partial class MigrationScene {
 async Task CheckIndependentExploration(CoopActor a){
  a.Body.Position=Ground(game.World.RouteX(-640),-640)+Vector3.Up*.2f;a.Target=a.Body.Position;
  SendCoopPersonal(a.Id,CoopProfileJson(a.Game),"Independent exploration check",a.Body.Position,true);
  await CoopWait(()=>chunks.Count>90,"Remote physics chunks were not streamed",20);WithCoopActor(a,SpawnObjects);
  bool clear=false;foreach(var node in props.Where(p=>p.Kind==1&&Distance(p.Position,a.Body.Position)<60).ToArray()){
   a.Body.Position=node.Position+Vector3.Up*.2f;a.Yaw=a.Pitch=0;WithCoopActor(a,()=>clear=OutdoorFree(player.Position.X,player.Position.Z)&&ToolPoint(out _));if(clear)break;
  }
  if(!clear)throw new Exception("No clear exploration fixture location");
  var far=a.Body.Position;
  if(Distance(far,HostPosition)<500)throw new Exception("Fixture was not far enough from host");
  AddEnemy("coop-check/nearby-only",12,-65,1);
  AddEnemy("coop-check/far-infected",far.X,far.Z-6,2);var enemy=infected.Find(e=>e.Id=="coop-check/far-infected");if(enemy==null)throw new Exception("Remote infected did not spawn");enemy.Hp=1;enemy.Body.Rotation=new Vector3(0,Mathf.Pi,0);await CoopDelay(.5);if(enemy.Memory<=0||Distance(enemy.Target,a.Body.Position)>2)throw new Exception("Distant infected did not sense and pursue the guest");
  if(FrameFor(far).Enemies.Any(e=>e.Id=="coop-check/nearby-only")||!FrameFor(far).Enemies.Any(e=>e.Id==enemy.Id))throw new Exception("Enemy interest filtering failed");
  if(FrameFor(HostPosition).Enemies.Any(e=>e.Id==enemy.Id))throw new Exception("Distant infected sent to host interest area");
  var rendered=chunks.Values.Count(n=>!n.GetMeta("simulation_only",false).AsBool());
  if(rendered>49||chunks.Count>98)throw new Exception("Two-player streaming budget exceeded");
  if(!chunks[(ExpeditionWorld.Cell(far.X),ExpeditionWorld.Cell(far.Z))].GetMeta("simulation_only",false).AsBool())throw new Exception("Remote area is rendering full scenery on host");
  if(CoopPartyReady())throw new Exception("Party dungeon entry allowed with a distant guest");
  SendCoopPersonal(a.Id,CoopProfileJson(a.Game),"",far,true);
  RpcId(a.Id,nameof(CoopCheckStage),"explore",far);
  await CoopWait(()=>game.Skills.Defeated.Contains("coop-check/far-infected"),"Remote gunfire failed",20);
  await CoopWait(()=>game.Building.Pieces.Count==2,"Remote building failed",20);
  await CoopWait(()=>game.World.Changes.Paths.Count>0,"Remote earthwork failed",20);
  await CoopDelay(2);
  if(Distance(a.Body.Position,HostPosition)<500)throw new Exception("Guest was pulled back to host");
  if(!game.Craft.Gathered.Any(id=>id.Contains("/node/")&&id.EndsWith(":"+(int)Math.Round(far.Z/32))))throw new Exception("Remote resource claim not retained");
  foreach(var e in infected.Where(e=>Distance(e.Body.Position,far)<80).ToArray())RemoveEnemy(e);ExpeditionBag bag=null;foreach(int dx in new[]{0,6,-6,12,-12}){var candidate=new ExpeditionBag{Id="far-bag",Seed=game.Save.Seed,X=far.X+dx,Z=far.Z};if(CampSafe(candidate)){bag=candidate;break;}}if(bag==null)throw new Exception("No safe distant camp fixture");WithCoopActor(a,()=>{game.Life.Camps.Add(bag);game.Life.Respawn=bag.Id;CoopDie();});
  if(Distance(a.Body.Position,Ground(bag.X,bag.Z))>2)throw new Exception("Remote sleeping bag respawn was replaced with host position");
  a.Game.Life.Respawn=null;
  GD.Print("PASS / independent exploration: 600m separation, bounded remote physics, per-player enemies and interest filtering, remote combat/build/gather/paths, no forced regroup, distant bag respawn");
 }
 async Task ClientIndependentExploration(Vector3 target){
  await CoopWait(()=>player.Position.DistanceTo(target)<3,"Remote teleport fixture not applied");
  await CoopWait(()=>infected.Any(e=>e.Id=="coop-check/far-infected"),"Remote enemy not replicated");
  if(infected.Any(e=>e.Id=="coop-check/nearby-only"))throw new Exception("Out-of-range enemy replica present");
  if(chunks.Count>49)throw new Exception("Client rendered multiple players' terrain");
  player.Rotation=Vector3.Zero;pitch=0;camera.Rotation=Vector3.Zero;await CoopDelay(.3);CoopRequest("shoot");
  await CoopWait(()=>game.Skills.Defeated.Contains("coop-check/far-infected"),"Remote kill reward missing");
  CoopRequest("interact");await CoopDelay(.3);CoopRequest("path");
  BuildPiece piece=null;foreach(int dx in new[]{7,-7,10,-10})foreach(int dz in new[]{0,3,-3}){var candidate=new BuildPiece{Kind=0,X=target.X+dx,Z=target.Z+dz,Y=game.World.Height(target.X+dx,target.Z+dz),Yaw=45};if(game.Building.Placement(candidate).Valid&&!StaticPlacementBlocked(candidate)){piece=candidate;break;}}
  if(piece==null)throw new Exception("No clear remote building position");CoopRequest("place",Json.Write(piece));
  await CoopWait(()=>game.Building.Pieces.Count==2,"Remote structure snapshot missing");
  // Send legitimate movement updates while distant; old tether code would reject these.
  for(int i=0;i<40;i++){player.Position+=Vector3.Right*.04f;await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);}
 }
}
