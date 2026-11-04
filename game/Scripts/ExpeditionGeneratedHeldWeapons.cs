using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
public partial class MigrationScene {
 const string HeldWeaponManifest="res://Assets/Hunyuan/Weapons/manifest.json";
 Godot.Collections.Dictionary generatedWeaponDefinitions,generatedWeaponAssets;
 readonly Dictionary<string,PackedScene> generatedWeaponScenes=new();
 bool generatedHeldModel;
 Vector3 generatedBowUpper,generatedBowLower;
 float generatedSightHeight=.07f;
 Vector3 generatedMuzzle;
 void LoadGeneratedWeapons(){
  if(generatedWeaponDefinitions!=null)return;
  var data=Godot.Json.ParseString(Godot.FileAccess.GetFileAsString(HeldWeaponManifest)).AsGodotDictionary();
  generatedWeaponDefinitions=data["weapons"].AsGodotDictionary();generatedWeaponAssets=data["assets"].AsGodotDictionary();
 }
 static Vector3 DefinitionVector(Godot.Collections.Dictionary data,string key){var v=data[key].AsGodotArray();return new((float)v[0].AsDouble(),(float)v[1].AsDouble(),(float)v[2].AsDouble());}
 Node3D AttachGeneratedWeaponPart(Node3D root,string asset,float length,Vector3 rotation,Vector3 anchor,Vector3 target,int axis=2,bool metal=false){
  LoadGeneratedWeapons();if(!generatedWeaponAssets.ContainsKey(asset))return null;
  var definition=generatedWeaponAssets[asset].AsGodotDictionary();string path=definition["resource"].AsString();
  if(!generatedWeaponScenes.TryGetValue(asset,out var scene)){scene=GD.Load<PackedScene>(path);if(scene==null)return null;generatedWeaponScenes[asset]=scene;}
  var model=scene.Instantiate<Node3D>();Aabb bounds=default;bool found=false;BatchBounds(model,Transform3D.Identity,ref bounds,ref found);
  if(!found){model.Free();return null;}
  var basis=Basis.FromEuler(rotation*Mathf.Pi/180);var centeredBounds=new Aabb(-bounds.Size/2,bounds.Size);var rotated=new Transform3D(basis,Vector3.Zero)*centeredBounds;
  float extent=axis<0?Math.Max(rotated.Size.X,Math.Max(rotated.Size.Y,rotated.Size.Z)):rotated.Size[axis];float longest=Math.Max(bounds.Size.X,Math.Max(bounds.Size.Y,bounds.Size.Z));
  if(!float.IsFinite(extent)||extent<.001f){model.Free();return null;}
  float scale=length/extent;
  var holder=new Node3D{Name="GeneratedHeldWeapon",Basis=basis.Scaled(Vector3.One*scale),Position=target-basis*(anchor*longest)*scale};root.AddChild(holder);
  var centered=new Node3D{Position=-bounds.GetCenter()};holder.AddChild(centered);centered.AddChild(model);
  foreach(var mesh in holder.FindChildren("*","MeshInstance3D",true,false).OfType<MeshInstance3D>()){
   mesh.CastShadow=root==weaponRoot?GeometryInstance3D.ShadowCastingSetting.Off:GeometryInstance3D.ShadowCastingSetting.On;mesh.VisibilityRangeEnd=0;
   for(int surface=0;surface<mesh.Mesh.GetSurfaceCount();surface++)if(mesh.GetActiveMaterial(surface) is StandardMaterial3D material){
    var copy=(StandardMaterial3D)material.Duplicate();copy.Roughness=.8f;copy.Metallic=.05f;copy.TextureFilter=BaseMaterial3D.TextureFilterEnum.LinearWithMipmapsAnisotropic;if(asset is "source-7" or "source-33" or "source-8" or "source-9")copy.CullMode=BaseMaterial3D.CullModeEnum.Disabled;
    if(metal){copy.AlbedoTexture=null;copy.AlbedoColor=new(.32f,.36f,.38f);copy.Metallic=.45f;copy.Roughness=.55f;}
    mesh.SetSurfaceOverrideMaterial(surface,copy);
   }
  }
  return holder;
 }
 bool TryGeneratedHeldWeapon(){generatedSightHeight=.07f;generatedHeldModel=false;generatedMuzzle=Vector3.Zero;generatedHeldModel=BuildGeneratedWeapon(weaponRoot,game.Craft.Weapon,true);if(!generatedHeldModel)ClearChildren(weaponRoot);return generatedHeldModel;}
 bool GeneratedWeaponDisplay(Node3D root,int id,Aabb target){
  var holder=new Node3D{Name="GeneratedWeaponDisplay"};root.AddChild(holder);var model=new Node3D{RotationDegrees=new(0,65,0)};holder.AddChild(model);
  if(!BuildGeneratedWeapon(model,id,false)){holder.Free();return false;}
  Aabb bounds=default;bool found=false;LocalMeshBounds(holder,ref bounds,ref found);if(!found){holder.Free();return false;}
  float scale=Math.Max(target.Size.X,Math.Max(target.Size.Y,target.Size.Z))/Math.Max(bounds.Size.X,Math.Max(bounds.Size.Y,bounds.Size.Z));holder.Scale=Vector3.One*scale;holder.Position=target.GetCenter()-bounds.GetCenter()*scale;return true;
 }
 bool BuildGeneratedWeapon(Node3D root,int id,bool animate){
  LoadGeneratedWeapons();var profile=ProfileFor(id);
  if(!generatedWeaponDefinitions.ContainsKey(id.ToString()))return false;
  var definition=generatedWeaponDefinitions[id.ToString()].AsGodotDictionary();string asset=definition["asset"].AsString();
  if(definition.ContainsKey("assembly")){
   string assembly=definition["assembly"].AsString();
   if(assembly=="crossbow"){
    FirstPersonCrossbow(false,root);if(AttachGeneratedWeaponPart(root,asset,.72f,new(-90,0,0),new(0,-.27f,0),new(0,.02f,-.39f),0)==null)return false;
   }else{
    bool chain=assembly=="chain-sword";Color housing=chain?new(.20f,.29f,.30f):new(.53f,.30f,.11f);
    Bevel(root,new(0,0,-.18f),new(.19f,.20f,.31f),housing);Tube(root,new(0,-.035f,.15f),new(0,-.015f,-.12f),.024f,Cloth);
    if(chain){Tube(root,new(0,.06f,-.02f),new(0,.06f,.23f),.05f,Steel);for(int i=0;i<3;i++)Box(root,new(.10f,.02f,-.10f-i*.07f),new(.012f,.045f,.035f),new(.18f,.60f,.72f));}
    else{Tube(root,new(-.14f,0,-.14f),new(-.14f,.2f,-.14f),.022f,Ink);Tube(root,new(-.14f,.2f,-.14f),new(.14f,.2f,-.14f),.022f,Ink);Bevel(root,new(.065f,-.055f,.015f),new(.045f,.075f,.12f),Ink);}
    if(AttachGeneratedWeaponPart(root,asset,chain?1.0f:.83f,new(0,90,0),Vector3.Zero,new(0,.02f,chain?-.77f:-.68f))==null)return false;
   }
  }else{
   Vector3 anchor=DefinitionVector(definition,"anchor");var target=new Vector3(0,profile.RightGrip.Y,profile.RightGrip.Z);
   bool bow=profile.Motion==HeldMotion.Bow;if(bow)target=new(0,-.04f,-.35f);
   var model=AttachGeneratedWeaponPart(root,asset,(float)definition["length"].AsDouble(),DefinitionVector(definition,"rotation"),anchor,target,definition["axis"].AsInt32(),id==15);
   if(model==null)return false;
   if(bow){GeneratedBowCore(root,model,id);if(animate){FindGeneratedBowTips(model);FirstPersonBow(false);}else{var tips=GeneratedBowTips(model);var nock=new Vector3(0,0,Math.Max(tips.Upper.Z,tips.Lower.Z)+.12f);Tube(root,tips.Upper,nock,.0025f,Cloth);Tube(root,tips.Lower,nock,.0025f,Cloth);Tube(root,nock,nock+new Vector3(0,0,-.8f),.005f,Wood);}}
   if(animate&&ExpeditionGame.IsGun(id)){var points=GeneratedVertices(model);float front=points.Min(p=>p.Z);var muzzle=points.Where(p=>p.Z<front+.012f).ToArray();generatedMuzzle=Vector3.Zero;foreach(var p in muzzle)generatedMuzzle+=p;generatedMuzzle/=muzzle.Length;generatedSightHeight=generatedMuzzle.Y+(id is 0 or 28?.035f:.09f);}

   if(id is 21 or 33){float z=id==21?-.90f:-1.13f;Bevel(root,new(0,.03f,z),new(id==21?.29f:.32f,.14f,.15f),Steel);if(id==33)Bevel(root,new(.19f,.03f,z),new(.14f,.045f,.06f),Steel);}
   if(id==34){Tube(root,new(-.12f,-.12f,.045f),new(-.12f,.13f,.045f),.06f,new(.38f,.32f,.18f));Tube(root,new(-.12f,-.12f,.045f),new(.08f,-.10f,-.20f),.013f,Ink);Tube(root,new(.08f,-.10f,-.20f),new(.06f,-.08f,-.65f),.013f,Ink);}
   if(id is 25 or 26){Bevel(root,new(0,.005f,-.08f),new(id==26?.24f:.20f,.035f,.045f),id==26?new Color(.48f,.43f,.29f):Steel);}
  }
  return true;
 }
 void GeneratedBowCore(Node3D root,Node3D model,int id){
  // Fill small reconstruction pinholes beneath the original textured limb surface.
  var points=GeneratedVertices(model);float min=points.Min(p=>p.Y),max=points.Max(p=>p.Y);var centers=new List<Vector3>();
  for(int i=0;i<=24;i++){float y=Mathf.Lerp(min,max,(i+.15f)/24.3f);var band=points.Where(p=>Math.Abs(p.Y-y)<(max-min)/42).ToArray();if(band.Length==0)continue;var xs=band.Select(p=>p.X).OrderBy(v=>v).ToArray();var zs=band.Select(p=>p.Z).OrderBy(v=>v).ToArray();centers.Add(new(xs[xs.Length/2],y,zs[zs.Length/2]));}
  FigureMesh(root,"generated-bow-core/"+id,b=>{for(int i=1;i<centers.Count;i++)b.Limb(new Color(.24f,.16f,.085f),centers[i-1],centers[i],.011f,.014f,.011f,.014f,10);});
 }
 static List<Vector3> GeneratedVertices(Node3D model){
  var points=new List<Vector3>();
  void Visit(Node node,Transform3D transform){if(node is Node3D n)transform*=n.Transform;if(node is MeshInstance3D mesh&&mesh.Mesh!=null)for(int s=0;s<mesh.Mesh.GetSurfaceCount();s++){var arrays=mesh.Mesh.SurfaceGetArrays(s);foreach(var p in arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array())points.Add(transform*p);}foreach(var child in node.GetChildren())Visit(child,transform);}
  Visit(model,Transform3D.Identity);return points;
 }
 void FindGeneratedBowTips(Node3D model){var tips=GeneratedBowTips(model);generatedBowUpper=tips.Upper;generatedBowLower=tips.Lower;}
 static (Vector3 Upper,Vector3 Lower) GeneratedBowTips(Node3D model){
  var points=GeneratedVertices(model);float max=points.Max(p=>p.Y),min=points.Min(p=>p.Y);var high=points.Where(p=>p.Y>max-.015f).ToArray();var low=points.Where(p=>p.Y<min+.015f).ToArray();
  static Vector3 Average(Vector3[] list){var sum=Vector3.Zero;foreach(var p in list)sum+=p;return sum/list.Length;}
  return (Average(high),Average(low));
 }
}
