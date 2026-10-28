using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class MigrationScene {
 const string BatchManifest="res://Assets/Hunyuan/Static/manifest.json";
 Dictionary<string,string> batchPaths;
 readonly Dictionary<string,PackedScene> batchScenes=new();
 void LoadBatchPaths(){
  if(batchPaths!=null)return;batchPaths=new();
  if(!Godot.FileAccess.FileExists(BatchManifest))return;
  var data=Godot.Json.ParseString(Godot.FileAccess.GetFileAsString(BatchManifest)).AsGodotDictionary();
  foreach(var key in data.Keys)batchPaths[key.AsString()]=data[key].AsGodotDictionary()["resource"].AsString();
 }
 static bool BatchBounds(Node node,Transform3D transform,ref Aabb bounds,ref bool found){
  if(node is Node3D n)transform*=n.Transform;
  if(node is MeshInstance3D m&&m.Mesh!=null){var b=transform*m.Mesh.GetAabb();bounds=found?bounds.Merge(b):b;found=true;}
  foreach(var child in node.GetChildren())BatchBounds(child,transform,ref bounds,ref found);
  return found;
 }
 static bool LocalMeshBounds(Node3D root,ref Aabb bounds,ref bool found){
  // Targets consumed by child model placement must be in the parent's LOCAL
  // space. Including root.Transform here applies its position a second time.
  foreach(var child in root.GetChildren())BatchBounds(child,Transform3D.Identity,ref bounds,ref found);
  return found;
 }
 bool AttachBatchModel(Node3D root,string key,Aabb target){
  LoadBatchPaths();if(!batchPaths.TryGetValue(key,out var path))return false;
  if(!batchScenes.TryGetValue(key,out var packed)){packed=GD.Load<PackedScene>(path);if(packed==null)return false;batchScenes[key]=packed;}
  var model=packed.Instantiate<Node3D>();Aabb bounds=default;bool found=false;
  BatchBounds(model,Transform3D.Identity,ref bounds,ref found);
  float longest=Math.Max(bounds.Size.X,Math.Max(bounds.Size.Y,bounds.Size.Z));
  if(!found||!float.IsFinite(longest)||longest<.001f){model.Free();return false;}
  float size=Math.Max(target.Size.X,Math.Max(target.Size.Y,target.Size.Z));
  float scale=size/longest;
  var holder=new Node3D{Name="BatchModel"};root.AddChild(holder);holder.AddChild(model);
  model.Scale*=scale;
  model.Position=target.GetCenter()-bounds.GetCenter()*scale;
  // Keep proportional scaling. Gameplay dimensions and collision remain authored separately.
  foreach(var mesh in model.FindChildren("*","MeshInstance3D",true,false).OfType<MeshInstance3D>()){
   mesh.VisibilityRangeEnd=75;
   for(int s=0;s<mesh.Mesh.GetSurfaceCount();s++)if(mesh.GetActiveMaterial(s) is StandardMaterial3D material){var copy=(StandardMaterial3D)material.Duplicate();copy.Metallic=0;copy.Roughness=.92f;mesh.SetSurfaceOverrideMaterial(s,copy);}
  }
  return true;
 }
 void ItemModel(Node3D root,string kind,int index=0,int tier=1,string appearance=""){
  var procedural=new Node3D{Name="ProceduralItem"};root.AddChild(procedural);
  ProceduralItemModel(procedural,kind,index,tier,appearance);
  // Authored relic appearances and moving first-person weapons keep their dedicated art.
  if(appearance!="")return;
  var item=ExpeditionItemDefinition.All().FirstOrDefault(i=>i.Kind==kind&&i.Index==index&&i.Tier==tier);
  if(item==null)return;
  Aabb target=default;bool found=false;BatchBounds(procedural,Transform3D.Identity,ref target,ref found);
  if(found&&AttachBatchModel(root,"catalog-"+item.Key,target))procedural.Visible=false;
 }
 void BatchResource(Node3D root,int material){
  ItemMaterial(root,material);Aabb target=default;bool found=false;
  LocalMeshBounds(root,ref target,ref found);
  var old=root.GetChildren().OfType<GeometryInstance3D>().ToArray();
  if(found&&AttachBatchModel(root,"catalog-material/"+material,target))foreach(var mesh in old)mesh.Visible=false;
 }
 void ArtWreck(Node3D root,bool burned){
  var procedural=new Node3D{Name="ProceduralWreck"};root.AddChild(procedural);ProceduralWreck(procedural,burned);
  if(TryWorldProp(root,burned?"burned-car":"wreck-car",new Aabb(new Vector3(-1.05f,0,-2.1f),new Vector3(2.1f,1.5f,4.2f))))procedural.Visible=false;
 }
 void ArtWorkbench(Node3D root){
  var procedural=new Node3D{Name="ProceduralWorkbench"};root.AddChild(procedural);ProceduralWorkbench(procedural);
  Aabb bounds=default;bool found=false;BatchBounds(procedural,Transform3D.Identity,ref bounds,ref found);
  if(found&&AttachBatchModel(root,"catalog-bench/1",bounds))procedural.Visible=false;
 }
 void RunBatchModelChecks(){try{
  game=new ExpeditionGame(ProjectSettings.GlobalizePath("user://tests/batch-model-check.json"));game.NoDiskSave=true;
  LoadBatchPaths();int count=0;
  foreach(var key in batchPaths.Keys){var root=new Node3D();AddChild(root);MigrationChecks.Assert(AttachBatchModel(root,key,new Aabb(new Vector3(-.5f,0,-.5f),Vector3.One)),"Batch model load / "+key);Aabb bounds=default;bool found=false;BatchBounds(root,Transform3D.Identity,ref bounds,ref found);MigrationChecks.Assert(found&&Math.Abs(Math.Max(bounds.Size.X,Math.Max(bounds.Size.Y,bounds.Size.Z))-1)<.01,"Batch model scale / "+key);foreach(var mesh in root.FindChildren("*","MeshInstance3D",true,false).OfType<MeshInstance3D>())for(int s=0;s<mesh.Mesh.GetSurfaceCount();s++)MigrationChecks.Assert(mesh.GetActiveMaterial(s)!=null,"Batch material / "+key);root.Free();count++;}
  ItemCatalogChecks();GD.Print($"PASS / batch models / {count} models / loading, proportional scale, materials and item catalogue");GetTree().Quit();
 }catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}}
 async void CaptureBatchModels(){try{
  GetWindow().ContentScaleSize=new Vector2I(1280,900);GetWindow().Size=new Vector2I(1280,900);
  bool worldOnly=OS.GetCmdlineUserArgs().Contains("--world-asset-preview");
  LoadBatchPaths();bool trees=OS.GetCmdlineUserArgs().Contains("--tree-candidates");if(trees)PrepareTreeReview();var keys=batchPaths.Keys.Where(k=>trees?k.StartsWith("tree-review/"):(!worldOnly||k.StartsWith("world-"))).OrderBy(k=>k).ToArray();
  string folder=ProjectSettings.GlobalizePath("res://../Ingame Screenshots/"+(worldOnly?"World Asset Candidates":"Batch Models"));System.IO.Directory.CreateDirectory(folder);
  for(int page=0;page<keys.Length;page+=12){
   var grid=new GridContainer{Columns=4};AddChild(grid);
   for(int i=page;i<Math.Min(page+12,keys.Length);i++){
    var cell=new VBoxContainer();grid.AddChild(cell);
    var container=new SubViewportContainer{CustomMinimumSize=new(310,260),Stretch=true};cell.AddChild(container);
    var viewport=new SubViewport{Size=new(310,260),OwnWorld3D=true,RenderTargetUpdateMode=SubViewport.UpdateMode.Always};container.AddChild(viewport);
    viewport.AddChild(new WorldEnvironment{Environment=new Godot.Environment{BackgroundMode=Godot.Environment.BGMode.Color,BackgroundColor=Colors.White,AmbientLightSource=Godot.Environment.AmbientSource.Color,AmbientLightColor=Colors.White,AmbientLightEnergy=.65f}});
    viewport.AddChild(new DirectionalLight3D{RotationDegrees=new(-35,-35,0),LightEnergy=1.2f});
    var root=new Node3D();viewport.AddChild(root);AttachBatchModel(root,keys[i],new Aabb(new Vector3(-.5f,0,-.5f),Vector3.One));
    var cam=new Camera3D{Position=new(1.2f,1.05f,-1.65f),Projection=Camera3D.ProjectionType.Orthogonal,Size=1.5f,Current=true};viewport.AddChild(cam);cam.LookAt(new Vector3(0,.5f,0));
    cell.AddChild(new Label{Text=keys[i],Modulate=Colors.Black});
   }
   for(int i=0;i<5;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
   await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
   GetViewport().GetTexture().GetImage().SavePng(folder+$"/batch-{page/12+1:00}.png");grid.Free();
  }
  GD.Print("PASS / batch model contact sheets captured");GetTree().Quit();
 }catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}}
}
