using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
public partial class MigrationScene {
 void SceneryPolishChecks(){
  double z=-820,x=game.World.RouteX(z)+170;int cx=ExpeditionWorld.Cell(x),cz=ExpeditionWorld.Cell(z);
  var root=new Node3D();AddChild(root);LivingGrass(root,cx,cz);ForestFloor(root,cx,cz);SceneryUndergrowth(root,cx,cz);
  var grass=root.GetNode<MultiMeshInstance3D>("MeadowGroundCover");var ferns=root.GetNode<MultiMeshInstance3D>("ForestFerns");
  MigrationChecks.Assert(grass.Multimesh.InstanceCount>700&&ferns.Multimesh.InstanceCount>100,"Dense grass and fern layer in representative forest chunk");
  foreach(var group in root.GetChildren().OfType<MultiMeshInstance3D>())for(int i=0;i<group.Multimesh.InstanceCount;i+=17){var p=group.Multimesh.GetInstanceTransform(i).Origin;
   MigrationChecks.Assert(!game.World.MainRoad(p.X,p.Z,.7)&&!game.World.Changes.OnPath(p.X,p.Z)&&!InSuburbHome(p.X,p.Z),"Decorative foliage preserves roads, earthworks and house interiors");
  }
  GD.Print($"PASS / scenery polish / grass {grass.Multimesh.InstanceCount}, ferns {ferns.Multimesh.InstanceCount} / seeded ground-cover clearance / no foliage collision");root.Free();
  LoadBatchPaths();var items=ExpeditionItemDefinition.All();var inventory=items.Select(i=>{var actual=items.First(j=>j.Kind==i.Kind&&j.Index==i.Index&&j.Tier==i.Tier);return new{key="catalog-"+i.Key,lookup_key="catalog-"+actual.Key,name=i.Name,kind=i.Kind,tier=i.Tier,registered_model=batchPaths.ContainsKey("catalog-"+actual.Key)};}).ToArray();
  string path=ProjectSettings.GlobalizePath("user://tests/runtime-model-coverage.json");System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));System.IO.File.WriteAllText(path,System.Text.Json.JsonSerializer.Serialize(inventory,new System.Text.Json.JsonSerializerOptions{WriteIndented=true}));
 }
 async Task SceneryStreetReview(){
  camera.Fov=70;
  foreach(var entry in new[]{("suburb",ExpeditionWorld.SpawnX+8,ExpeditionWorld.SpawnZ-35),
   ("woodland",game.World.RouteX(-720)+8,-720.0),("industrial",game.World.RouteX(-1450)+9,-1450.0),("city",game.World.RouteX(-2100)+7,-2100.0)}){
   var(name,x,z)=entry;Teleport(x,z);ClearEnemies();camera.GlobalPosition=SurfaceGround(x,z)+Vector3.Up*1.75f;
   double lookX=name=="suburb"?ExpeditionWorld.HomeX:game.World.RouteX(z-30);camera.LookAt(SurfaceGround(lookX,z-30)+Vector3.Up*1.7f);
   for(int i=0;i<15;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
   ulong start=Time.GetTicksMsec();for(int i=0;i<45;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
   double fps=45000.0/Math.Max(1,Time.GetTicksMsec()-start);
   GD.Print($"STATIC VIEW / {name} / {GetViewport().GetVisibleRect().Size} / 45-frame mean {fps:F1} fps / not a movement benchmark");await ArtShot("scenery-street-"+name);
  }
 }
}
