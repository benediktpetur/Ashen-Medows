using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class MigrationScene {
 const string SkyscraperFolder="res://Assets/GeneratedSkyscrapers/";
 string[] skyscraperKeys;
 sealed record SkyscraperAsset(PackedScene Scene,Aabb Bounds);
 readonly Dictionary<string,SkyscraperAsset> skyscraperAssets=new();
 string[] SkyscraperKeys(){
  if(skyscraperKeys!=null)return skyscraperKeys;
  var rows=Godot.Json.ParseString(Godot.FileAccess.GetFileAsString(SkyscraperFolder+"manifest.json")).AsGodotArray();
  return skyscraperKeys=rows.Select(r=>r.AsGodotDictionary()["key"].AsString()).ToArray();
 }
 string SkyscraperKey(int x,int z){var keys=SkyscraperKeys();return keys[(int)(game.World.Hash(x,z)%(uint)keys.Length)];}
 SkyscraperAsset SkyscraperSource(string key){
  if(skyscraperAssets.TryGetValue(key,out var asset))return asset;
  var source=GD.Load<PackedScene>(SkyscraperFolder+key+".glb");if(source==null)throw new Exception("Missing skyscraper / "+key);
  var prototype=source.Instantiate<Node3D>();Aabb bounds=default;bool found=false;BatchBounds(prototype,Transform3D.Identity,ref bounds,ref found);
  if(!found||!bounds.Position.IsFinite()||!bounds.Size.IsFinite()||bounds.Size.X<.001f||bounds.Size.Y<.001f||bounds.Size.Z<.001f)throw new Exception("Invalid skyscraper bounds / "+key);
  // Override materials once per source; instances share imported meshes, LODs,
  // textures and these material resources rather than allocating per city lot.
  foreach(var mesh in prototype.FindChildren("*","MeshInstance3D",true,false).Prepend(prototype).OfType<MeshInstance3D>()){
   mesh.VisibilityRangeEnd=350;
   for(int s=0;s<mesh.Mesh.GetSurfaceCount();s++)if(mesh.GetActiveMaterial(s) is StandardMaterial3D original){var material=(StandardMaterial3D)original.Duplicate();material.Metallic=0;material.Roughness=.95f;material.CullMode=BaseMaterial3D.CullModeEnum.Disabled;mesh.SetSurfaceOverrideMaterial(s,material);}
  }
  var packed=new PackedScene();var error=packed.Pack(prototype);prototype.Free();if(error!=Error.Ok)throw new Exception("Cannot cache skyscraper / "+key);
  return skyscraperAssets[key]=new(packed,bounds);
 }
 Node3D SkyscraperVisual(Node3D parent,string key,float size,float height){
  var asset=SkyscraperSource(key);var bounds=asset.Bounds;
  var scale=new Vector3(size/bounds.Size.X,height/bounds.Size.Y,size/bounds.Size.Z);
  var origin=new Vector3(bounds.GetCenter().X,bounds.Position.Y,bounds.GetCenter().Z);
  var holder=new Node3D{Name="GeneratedSkyscraperExterior",Scale=scale,Position=-origin*scale};parent.AddChild(holder);holder.AddChild(asset.Scene.Instantiate<Node3D>());
  parent.SetMeta("skyscraper_model",key);return holder;
 }
 void GeneratedCityBuilding(Node3D root,int x,int z,float size,float height){
  string id=$"building/{game.Save.Seed}/{x}/{z}";
  var shell=new Node3D{Name="AmbientBlock_"+x+"_"+z,Position=Ground(x,z)};root.AddChild(shell);shell.SetMeta("world_structure_id",id);RegisterDamageNode(shell,id,600);if(shell.IsQueuedForDeletion())return;
  // Ordinary city blocks already use solid exteriors. Keep the exact same
  // gameplay footprint, height and damage ID as the host's simulation chunks.
  PhysicsObstacle(shell,new BoxShape3D{Size=new(size,height,size)},new(0,height/2,0));
  SkyscraperVisual(shell,SkyscraperKey(x,z),size,height);
  var pad=Box(shell,new(0,.1f,0),new(size+.2f,.2f,size+.2f),new(.27f,.28f,.27f));pad.Name="CityFoundation";pad.MaterialOverride=ExteriorFinish(4,new(.27f,.28f,.27f));
 }
 async void RunSkyscraperChecks(){try{
  game.NoDiskSave=true;paused=true;SetProcess(false);SetPhysicsProcess(false);ClearEnemies();hudRoot.Visible=false;weaponRoot.Visible=false;
  foreach(string key in SkyscraperKeys()){
   var root=new Node3D();AddChild(root);var first=SkyscraperVisual(root,key,20,32);Aabb bounds=default;bool found=false;BatchBounds(first,Transform3D.Identity,ref bounds,ref found);
   MigrationChecks.Assert(found&&bounds.Size.DistanceTo(new Vector3(20,32,20))<.02f&&Math.Abs(bounds.Position.Y)<.01f&&Math.Abs(bounds.GetCenter().X)<.01f&&Math.Abs(bounds.GetCenter().Z)<.01f,"Fitted, centered and grounded skyscraper / "+key);
   var second=SkyscraperVisual(root,key,20,32);
   var a=first.FindChildren("*","MeshInstance3D",true,false).OfType<MeshInstance3D>().ToArray();var b=second.FindChildren("*","MeshInstance3D",true,false).OfType<MeshInstance3D>().ToArray();
   MigrationChecks.Assert(a.Length>0&&a.Length==b.Length,"Skyscraper contains render meshes / "+key);
   for(int i=0;i<a.Length;i++){MigrationChecks.Assert(a[i].Mesh==b[i].Mesh,"Shared city mesh memory / "+key);for(int s=0;s<a[i].Mesh.GetSurfaceCount();s++)MigrationChecks.Assert(a[i].GetActiveMaterial(s)!=null&&a[i].GetActiveMaterial(s)==b[i].GetActiveMaterial(s),"Shared textured city material / "+key);}
   root.Free();
  }
  var seen=new HashSet<string>();int lots=0;
  for(int z=(int)Math.Floor(ExpeditionWorld.MinZ/32)*32+16;z<ExpeditionWorld.MaxZ;z+=32)for(int x=(int)Math.Floor(ExpeditionWorld.MinX/32)*32+16;x<ExpeditionWorld.MaxX;x+=32){if(game.World.Tier(x,z)!=4||!game.World.BuildingCell(x,z))continue;lots++;string key=SkyscraperKey(x,z);seen.Add(key);MigrationChecks.Assert(key==SkyscraperKey(x,z),"Deterministic skyscraper lot");}
  MigrationChecks.Assert(lots>100&&seen.Count==SkyscraperKeys().Length,"All generated skyscraper variants populate the city");
  var check=new Node3D();AddChild(check);int tx=16,tz=-2096;string damageId=$"building/{game.Save.Seed}/{tx}/{tz}";ObjectHealth.Remove(damageId);GeneratedCityBuilding(check,tx,tz,20,32);
  var shell=check.GetChildren().OfType<Node3D>().Single();var collider=shell.GetChildren().OfType<StaticBody3D>().Single();
  var shape=collider.GetChild<CollisionShape3D>(0);MigrationChecks.Assert(shape.Shape is BoxShape3D box&&box.Size.IsEqualApprox(new Vector3(20,32,20)),"City collision agrees with simulation chunks");
  WeaponStructureHit(collider,100);MigrationChecks.Assert(ObjectHealth[damageId]==500,"Existing city health ID and damage preserved");WeaponStructureHit(collider,600);await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
  MigrationChecks.Assert(check.GetChildCount()==0,"Destroyed tower loses visual and collision");GeneratedCityBuilding(check,tx,tz,20,32);await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);MigrationChecks.Assert(check.GetChildCount()==0,"Destroyed city lot stays absent after streaming");
  ObjectHealth.Remove(damageId);check.Free();GD.Print($"PASS / generated skyscrapers: {SkyscraperKeys().Length} textured models across {lots} city lots; fit, ground contact, shared resources, collision parity and persistent destruction");
  if(OS.GetCmdlineUserArgs().Contains("--skyscraper-preview"))await CaptureSkyscrapers();GetTree().Quit();
 }catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}}
 async System.Threading.Tasks.Task CaptureSkyscrapers(){
  GetWindow().Size=new(1400,900);GetWindow().ContentScaleSize=new(1400,900);
  string folder=ProjectSettings.GlobalizePath("res://../Ingame Screenshots/Generated Skyscrapers");System.IO.Directory.CreateDirectory(folder);
  var keys=SkyscraperKeys();for(int page=0;page<(keys.Length+7)/8;page++){
   var grid=new GridContainer{Columns=4};AddChild(grid);
   foreach(string key in keys.Skip(page*8).Take(8)){
    var cell=new VBoxContainer();grid.AddChild(cell);var container=new SubViewportContainer{CustomMinimumSize=new(345,390),Stretch=true};cell.AddChild(container);
    var viewport=new SubViewport{Size=new(345,390),OwnWorld3D=true,RenderTargetUpdateMode=SubViewport.UpdateMode.Always};container.AddChild(viewport);
    viewport.AddChild(new WorldEnvironment{Environment=new Godot.Environment{BackgroundMode=Godot.Environment.BGMode.Color,BackgroundColor=new(.63f,.68f,.72f),AmbientLightSource=Godot.Environment.AmbientSource.Color,AmbientLightColor=Colors.White,AmbientLightEnergy=.75f}});
    viewport.AddChild(new DirectionalLight3D{RotationDegrees=new(-35,-35,0),LightEnergy=1.1f});var root=new Node3D();viewport.AddChild(root);SkyscraperVisual(root,key,20,36);
    var cam=new Camera3D{Position=new(38,24,46),Projection=Camera3D.ProjectionType.Orthogonal,Size=48,Current=true};viewport.AddChild(cam);cam.LookAt(new Vector3(0,17,0));cell.AddChild(new Label{Text=key});
   }
   for(int i=0;i<5;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);GetViewport().GetTexture().GetImage().SavePng(folder+$"/towers-{page+1}.png");grid.Free();
  }
  game.World.Changes.Time=900;Teleport(game.World.RouteX(-2100),-2100);ClearEnemies();var towers=terrainRoot.FindChildren("AmbientBlock_*","Node3D",true,false).OfType<Node3D>().Where(n=>n.HasMeta("skyscraper_model")).OrderBy(n=>n.GlobalPosition.DistanceSquaredTo(player.GlobalPosition)).ToArray();MigrationChecks.Assert(towers.Length>0,"Generated city skyline streamed");
  var target=towers[0].GlobalPosition;camera.GlobalPosition=target+new Vector3(55,28,65);camera.LookAt(target+Vector3.Up*12);await ArtShot("generated-skyscrapers-street");camera.GlobalPosition=target+new Vector3(90,90,120);camera.LookAt(target+Vector3.Up*8);await ArtShot("generated-skyscrapers-skyline");
 }
}
