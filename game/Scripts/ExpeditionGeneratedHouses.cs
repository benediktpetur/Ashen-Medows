using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class MigrationScene {
 const string HouseFolder="res://Assets/GeneratedHouses/";
 string[] generatedHouseKeys;
 readonly Dictionary<string,ArrayMesh> houseMeshes=new();
 readonly Dictionary<string,float> houseFronts=new();
 sealed record HouseSection(string Name,ArrayMesh Mesh,Shape3D Shape);
 readonly Dictionary<string,List<HouseSection>> houseSections=new();
 string[] HouseKeys(){
  if(generatedHouseKeys!=null)return generatedHouseKeys;
  var rows=Godot.Json.ParseString(Godot.FileAccess.GetFileAsString(HouseFolder+"manifest.json")).AsGodotArray();
  return generatedHouseKeys=rows.Select(r=>r.AsGodotDictionary()["key"].AsString()).ToArray();
 }
 readonly record struct HouseVertex(Vector3 P,Vector3 N,Vector2 UV){
  public HouseVertex Between(HouseVertex other,float t)=>new(P.Lerp(other.P,t),N.Lerp(other.N,t).Normalized(),UV.Lerp(other.UV,t));
 }
 // Split the triangle against a box, retaining the outside polygons. This opens
 // fused reconstructions without losing their original texture coordinates.
 static List<HouseVertex> HouseClip(List<HouseVertex> poly,int axis,float edge,bool positive){
  var result=new List<HouseVertex>();if(poly.Count==0)return result;
  float D(HouseVertex v)=>(positive?1:-1)*(v.P[axis]-edge);
  var a=poly[^1];float da=D(a);
  foreach(var b in poly){float db=D(b);if((da>=0)!=(db>=0))result.Add(a.Between(b,da/(da-db)));if(db>=0)result.Add(b);a=b;da=db;}
  return result;
 }
 static IEnumerable<List<HouseVertex>> HouseSubtract(List<HouseVertex> triangle,Aabb cut){
  var inside=triangle;
  for(int axis=0;axis<3;axis++)foreach(bool high in new[]{false,true}){
   float edge=high?cut.End[axis]:cut.Position[axis];
   var outside=HouseClip(inside,axis,edge,high);if(outside.Count>=3)yield return outside;
   inside=HouseClip(inside,axis,edge,!high);if(inside.Count<3)yield break;
  }
 }
 ArrayMesh PrepareHouse(string key){
  var packed=GD.Load<PackedScene>(HouseFolder+"Source/"+key+".glb");
  var root=packed.Instantiate<Node3D>();Aabb bounds=default;bool found=false;BatchBounds(root,Transform3D.Identity,ref bounds,ref found);
  if(!found||bounds.Size.X<.01f||bounds.Size.Z<.01f)throw new Exception("Invalid house bounds: "+key);
  float height=key.Contains("townhouse")||key.Contains("row-house")?9.5f:key.Contains("bungalow")||key.Contains("cottage")||key.Contains("cabin")||key.Contains("courtyard")?5.4f:7.2f;
  var scale=new Vector3(12.8f/bounds.Size.X,height/bounds.Size.Y,11.8f/bounds.Size.Z);
  var center=new Vector3(bounds.GetCenter().X,bounds.Position.Y,bounds.GetCenter().Z);
  var result=new ArrayMesh();
  void Visit(Node node,Transform3D transform){
   if(node is Node3D n)transform*=n.Transform;
   if(node is MeshInstance3D mesh&&mesh.Mesh!=null)for(int s=0;s<mesh.Mesh.GetSurfaceCount();s++){
    var arrays=mesh.Mesh.SurfaceGetArrays(s);var positions=arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();
    var normals=arrays[(int)Mesh.ArrayType.Normal].AsVector3Array();var uv=arrays[(int)Mesh.ArrayType.TexUV].AsVector2Array();
    var indices=arrays[(int)Mesh.ArrayType.Index].AsInt32Array();if(indices.Length==0)indices=Enumerable.Range(0,positions.Length).ToArray();
    var st=new SurfaceTool();st.Begin(Mesh.PrimitiveType.Triangles);int count=0;
    HouseVertex Vertex(int i){var p=(transform*positions[i]-center)*scale;var n=transform.Basis*normals[i]/scale;return new(p,n.Normalized(),uv.Length>i?uv[i]:Vector2.Zero);}
    for(int i=0;i<indices.Length;i+=3){
     var tri=new List<HouseVertex>{Vertex(indices[i]),Vertex(indices[i+1]),Vertex(indices[i+2])};
     // A triangle shell already has an empty interior: retain its irregular
     // walls and roof rather than cutting a rectangular room through them.
     // Remove the uneven reconstructed base in favour of an authored floor.
     var cavity=new Aabb(new(-7,-.1f,-7),new(14,.22f,14));
     var entry=new Aabb(new(-1.35f,-.1f,0),new(2.7f,2.9f,7));
     foreach(var outer in HouseSubtract(tri,cavity))for(int k=1;k<outer.Count-1;k++)
      foreach(var polygon in HouseSubtract(new(){outer[0],outer[k],outer[k+1]},entry))for(int j=1;j<polygon.Count-1;j++){
       if((polygon[j].P-polygon[0].P).Cross(polygon[j+1].P-polygon[0].P).LengthSquared()<1e-12f)continue;
       foreach(var v in new[]{polygon[0],polygon[j],polygon[j+1]}){st.SetNormal(v.N);st.SetUV(v.UV);st.AddVertex(v.P);count++;}
      }
    }
    if(count>0){st.Index();var material=mesh.GetActiveMaterial(s);if(material is StandardMaterial3D original){var copy=(StandardMaterial3D)original.Duplicate();copy.CullMode=BaseMaterial3D.CullModeEnum.Disabled;copy.Metallic=0;copy.Roughness=.92f;material=copy;}st.SetMaterial(material);st.Commit(result);}
   }
   foreach(var child in node.GetChildren())Visit(child,transform);
  }
  Visit(root,Transform3D.Identity);root.Free();
  if(result.GetSurfaceCount()==0)throw new Exception("Empty prepared house: "+key);
  return result;
 }
 ArrayMesh HouseMesh(string key){
  if(!houseMeshes.TryGetValue(key,out var mesh)){
   if(OS.GetCmdlineUserArgs().Contains("--house-prepare")){
    mesh=PrepareHouse(key);MigrationChecks.Assert(ResourceSaver.Save(mesh,HouseFolder+key+".res")==Error.Ok,"Save house / "+key);
   }else mesh=GD.Load<ArrayMesh>(HouseFolder+key+".res");
   if(mesh==null)throw new Exception("Prepare generated house first: "+key);houseMeshes[key]=mesh;
  }
  return mesh;
 }
 List<HouseSection> HouseSections(string key){
  if(houseSections.TryGetValue(key,out var cached))return cached;
  var source=HouseMesh(key);var meshes=new Dictionary<string,ArrayMesh>();
  for(int s=0;s<source.GetSurfaceCount();s++){
   var arrays=source.SurfaceGetArrays(s);var positions=arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();var normals=arrays[(int)Mesh.ArrayType.Normal].AsVector3Array();var uv=arrays[(int)Mesh.ArrayType.TexUV].AsVector2Array();var indices=arrays[(int)Mesh.ArrayType.Index].AsInt32Array();
   var tools=new Dictionary<string,SurfaceTool>();
   for(int i=0;i<indices.Length;i+=3){
    var p=(positions[indices[i]]+positions[indices[i+1]]+positions[indices[i+2]])/3;
    bool wall=p.Y<3.3f;bool side=Math.Abs(p.X)/6.4f>Math.Abs(p.Z)/5.9f;
    string section=wall?(side?(p.X<0?"west":"east"):(p.Z<0?"rear":"front"))+"-"+Math.Clamp((int)Math.Floor(((side?p.Z:p.X)+6.5f)/3.25f),0,3):"upper";
    if(!tools.TryGetValue(section,out var st)){st=new SurfaceTool();st.Begin(Mesh.PrimitiveType.Triangles);tools[section]=st;}
    for(int j=0;j<3;j++){int ix=indices[i+j];st.SetNormal(normals[ix]);st.SetUV(uv[ix]);st.AddVertex(positions[ix]);}
   }
   foreach(var pair in tools){if(!meshes.TryGetValue(pair.Key,out var mesh))meshes[pair.Key]=mesh=new ArrayMesh();pair.Value.Index();pair.Value.SetMaterial(source.SurfaceGetMaterial(s));pair.Value.Commit(mesh);}
  }
  return houseSections[key]=meshes.OrderBy(p=>p.Key).Select(p=>new HouseSection(p.Key,p.Value,p.Value.CreateTrimeshShape())).ToList();
 }
 bool GeneratedHouse(Node3D n,int style,bool simulation){
  var keys=HouseKeys();if(keys.Length==0)return false;string key=keys[Math.Abs(style)%keys.Length];var mesh=HouseMesh(key);
  n.SetMeta("generated_house",key);
  string home=n.HasMeta("house_id")?n.GetMeta("house_id").AsString():"briar/"+n.Name;
  foreach(var section in HouseSections(key)){
   string id=section.Name=="upper"?null:$"wall/{home}/generated/{key}/{section.Name}";
   var body=new StaticBody3D{Name="HouseCollision_"+section.Name};body.AddChild(new CollisionShape3D{Shape=section.Shape});n.AddChild(body);RegisterDamageNode(body,id,180);
   if(!simulation){var visual=new MeshInstance3D{Name="GeneratedHouseExterior_"+section.Name,Mesh=section.Mesh,VisibilityRangeEnd=210};n.AddChild(visual);RegisterDamageNode(visual,id,180);}
  }
  HouseFoundation(n,new(12.8f,11.8f),simulation);
  void Part(Vector3 at,Vector3 size,Color color){if(simulation)PhysicsObstacle(n,new BoxShape3D{Size=size},at);else Box(n,at,size,color,true);}
  Part(new(0,.08f,0),new(11.7f,.16f,10.7f),Wood);
  // The generated shell supplies matching inside/outside collision. Upper
  // storeys remain exterior scenery for this first batch.
  var end=n.Transform*new Vector3(0,0,13);float ground=(float)game.World.Height(end.X,end.Z)-n.Position.Y+.03f;
  var a=new Vector3(-1.1f,.16f,5.3f);var b=new Vector3(1.1f,.16f,5.3f);var c=new Vector3(1.1f,ground,13);var d=new Vector3(-1.1f,ground,13);
  PhysicsObstacle(n,new ConvexPolygonShape3D{Points=new[]{a,b,c,d,a-Vector3.Up*.12f,b-Vector3.Up*.12f,c-Vector3.Up*.12f,d-Vector3.Up*.12f}},Vector3.Zero);
  // Keep furniture clear of the smaller rooms under broad cottage eaves.
  var furniture=new Node3D{Name=n.Name,Scale=Vector3.One*.7f};
  if(n.HasMeta("house_id"))furniture.SetMeta("house_id",n.GetMeta("house_id"));
  n.AddChild(furniture);HouseFurniture(furniture,style,simulation);GardenRailings(n,simulation);
  if(!simulation){
   var ramp=new SurfaceTool();ramp.Begin(Mesh.PrimitiveType.Triangles);foreach(var v in new[]{a,b,c,a,c,d})ramp.AddVertex(v);ramp.GenerateNormals();n.AddChild(new MeshInstance3D{Mesh=ramp.Commit(),MaterialOverride=Textured(Wood,0,.6f)});
   HouseApproach(n);StoryPaper(n,new(.42f,.581f,-.84f));
   if(!houseFronts.TryGetValue(key,out float front)){
    var points=mesh.GetFaces().Where(p=>Math.Abs(Math.Abs(p.X)-1.35f)<.002f&&p.Y>.4f&&p.Y<2.7f&&p.Z>0).Select(p=>p.Z).OrderBy(z=>z).ToArray();
    houseFronts[key]=front=points.Length>0?points[points.Length/2]:5.3f;
   }
   foreach(int side in new[]{-1,1})Box(n,new(side*1.4f,1.5f,front),new(.14f,2.7f,.22f),Wood);
   Box(n,new(0,2.85f,front),new(2.94f,.14f,.22f),Wood);
   n.AddChild(new Label3D{Text=n.HasMeta("house_address")?n.GetMeta("house_address").AsString():(12+style*2).ToString(),Position=new(1.7f,2.2f,front+.18f),PixelSize=.004f,FontSize=26});
  }
  return true;
 }
 async Task CaptureGeneratedHouseCloseups(){
  Teleport(ExpeditionWorld.SpawnX,ExpeditionWorld.SpawnZ);paused=true;ClearEnemies();hudRoot.Visible=false;weaponRoot.Visible=false;
  var reference=SuburbHouseRoot(0);var transform=reference.Transform;reference.Free();
  camera.GlobalPosition=transform*new Vector3(-9,4,16);camera.LookAt(transform*new Vector3(0,2,0));await ArtShot("generated-house-exterior");
  camera.GlobalPosition=transform*new Vector3(0,1.8f,3);camera.LookAt(transform*new Vector3(-2,1,-3));await ArtShot("generated-house-interior");
 }
 async void RunHouseChecks(){try{
  game.NoDiskSave=true;
  foreach(string key in HouseKeys()){
   var mesh=HouseMesh(key);var bounds=mesh.GetAabb();MigrationChecks.Assert(bounds.Size.X>12&&bounds.Size.X<13&&bounds.Size.Z>11&&bounds.Size.Z<12,"House lot fit / "+key);
   var shape=mesh.CreateTrimeshShape();MigrationChecks.Assert(shape!=null,"House collision / "+key);
   var sections=HouseSections(key);MigrationChecks.Assert(sections.Count>4&&sections.Any(s=>s.Name=="upper"),"Independent wall sections and retained upper shell / "+key);
   // Prepared geometry must stay out of the entrance corridor.
   foreach(var point in mesh.GetFaces())MigrationChecks.Assert(!(Math.Abs(point.X)<1.349f&&point.Y>0&&point.Y<2.799f&&point.Z>0&&point.Z<6.99f),"Clear visual entrance / "+key);
  }
  var damageHouse=new Node3D{Name="GeneratedHouseDamageCheck",Position=new(5000,0,5000)};AddChild(damageHouse);GeneratedHouse(damageHouse,0,false);
  var wall=damageHouse.GetChildren().OfType<StaticBody3D>().First(n=>n.HasMeta("world_damage_id"));string id=wall.GetMeta("world_damage_id").AsString();
  WeaponStructureHit(wall,30);MigrationChecks.Assert(ObjectHealth[id]==150,"Generated wall takes independent damage");
  WeaponStructureHit(wall,200);await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
  MigrationChecks.Assert(!damageHouse.GetChildren().Any(n=>n.HasMeta("world_damage_id")&&n.GetMeta("world_damage_id").AsString()==id),"Destroyed generated wall loses mesh and collision");
  RemoveChunkFurniture(damageHouse);damageHouse.Free();
  damageHouse=new Node3D{Name="GeneratedHouseDamageCheck",Position=new(5000,0,5000)};AddChild(damageHouse);GeneratedHouse(damageHouse,0,false);await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
  MigrationChecks.Assert(!damageHouse.GetChildren().Any(n=>n.HasMeta("world_damage_id")&&n.GetMeta("world_damage_id").AsString()==id),"Destroyed generated section stays absent after streaming");
  RemoveChunkFurniture(damageHouse);damageHouse.Free();ObjectHealth.Remove(id);
  GD.Print($"PASS / generated houses: {HouseKeys().Length} textured shells, lot fit, open entrances and mesh collision");
  if(OS.GetCmdlineUserArgs().Contains("--house-preview")){
   paused=true;hudRoot.Visible=false;weaponRoot.Visible=false;
   GetWindow().Size=new(1400,900);GetWindow().ContentScaleSize=new(1400,900);
   for(int page=0;page<2;page++){
    var grid=new GridContainer{Columns=4};AddChild(grid);
    foreach(string key in HouseKeys().Skip(page*8).Take(8)){
     var cell=new VBoxContainer();grid.AddChild(cell);var container=new SubViewportContainer{CustomMinimumSize=new(345,390),Stretch=true};cell.AddChild(container);
     var viewport=new SubViewport{Size=new(345,390),OwnWorld3D=true,RenderTargetUpdateMode=SubViewport.UpdateMode.Always};container.AddChild(viewport);
     viewport.AddChild(new WorldEnvironment{Environment=new Godot.Environment{BackgroundMode=Godot.Environment.BGMode.Color,BackgroundColor=new(.72f,.76f,.78f),AmbientLightSource=Godot.Environment.AmbientSource.Color,AmbientLightColor=Colors.White,AmbientLightEnergy=.7f}});
     viewport.AddChild(new DirectionalLight3D{RotationDegrees=new(-35,-35,0),LightEnergy=1.1f});viewport.AddChild(new MeshInstance3D{Mesh=HouseMesh(key)});
     var cam=new Camera3D{Position=new(17,12,22),Projection=Camera3D.ProjectionType.Orthogonal,Size=20,Current=true};viewport.AddChild(cam);cam.LookAt(new Vector3(0,3.5f,0));cell.AddChild(new Label{Text=key});
    }
    for(int i=0;i<5;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
    string folder=ProjectSettings.GlobalizePath("res://../Ingame Screenshots/Generated Houses");System.IO.Directory.CreateDirectory(folder);GetViewport().GetTexture().GetImage().SavePng(folder+$"/houses-{page+1}.png");grid.Free();
   }
  }
  GetTree().Quit();
 }catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}}
}
