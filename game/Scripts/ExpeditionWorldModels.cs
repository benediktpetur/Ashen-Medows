using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class MigrationScene {
 readonly Dictionary<ulong,Material> worldModelMaterials=new();
 Node3D NewWorldModel(string key,out Aabb bounds){
  bounds=default;LoadBatchPaths();if(!batchPaths.TryGetValue(key,out var path))return null;
  if(!batchScenes.TryGetValue(key,out var packed)){packed=GD.Load<PackedScene>(path);if(packed==null)return null;batchScenes[key]=packed;}
  var model=packed.Instantiate<Node3D>();bool found=false;BatchBounds(model,Transform3D.Identity,ref bounds,ref found);
  if(!found||bounds.Size.X<.001f||bounds.Size.Y<.001f||bounds.Size.Z<.001f){model.Free();return null;}
  foreach(var mesh in model.FindChildren("*","MeshInstance3D",true,false).OfType<MeshInstance3D>()){
   mesh.LodBias=.35f;mesh.VisibilityRangeEnd=190;
   for(int s=0;s<mesh.Mesh.GetSurfaceCount();s++)if(mesh.GetActiveMaterial(s) is StandardMaterial3D material){
    ulong id=material.GetInstanceId();if(!worldModelMaterials.TryGetValue(id,out var copy)){var matte=(StandardMaterial3D)material.Duplicate();matte.Metallic=0;matte.Roughness=.95f;if(key=="world-pine")matte.AlbedoColor*=new Color(.62f,.74f,.46f);worldModelMaterials[id]=copy=matte;}mesh.SetSurfaceOverrideMaterial(s,copy);
   }
  }
  return model;
 }
 bool TryWorldTree(Node3D root,float height,int seed,bool pine=false){
  string key=pine?"world-pine":((uint)seed%3==0?"world-oak-d":"world-oak-c");
  var model=NewWorldModel(key,out var bounds);if(model==null)return false;
  var holder=new Node3D{Name="GeneratedTree",Rotation=new(0,((uint)seed%360)*Mathf.Pi/180,0)};root.AddChild(holder);holder.AddChild(model);
  float scale=height/bounds.Size.Y;model.Scale*=scale;
  model.Position=-new Vector3(bounds.GetCenter().X,bounds.Position.Y,bounds.GetCenter().Z)*scale;
  PhysicsObstacle(root,new CylinderShape3D{Radius=.45f,Height=height*.65f},new(0,height*.325f,0));return true;
 }
 bool TryWorldProp(Node3D root,string key,Aabb target){
  var model=NewWorldModel(key,out var bounds);if(model==null)return false;
  float scale=Math.Min(target.Size.X/bounds.Size.X,Math.Min(target.Size.Y/bounds.Size.Y,target.Size.Z/bounds.Size.Z));
  root.AddChild(model);model.Name="GeneratedWorldProp";model.Scale*=scale;
  model.Position=new Vector3(target.GetCenter().X-bounds.GetCenter().X*scale,target.Position.Y-bounds.Position.Y*scale,target.GetCenter().Z-bounds.GetCenter().Z*scale);
  return true;
 }
 void ApplyWorldFurniture(Node3D root,int kind){
  Aabb target=default;bool found=false;LocalMeshBounds(root,ref target,ref found);if(!found)return;
  var old=root.GetChildren().OfType<GeometryInstance3D>().ToArray();
  if(TryWorldProp(root,"world-furniture-"+kind,target))foreach(var mesh in old)mesh.Visible=false;
 }
 void WorldModelChecks(){
  foreach(bool pine in new[]{false,true}){
   var a=new Node3D();var b=new Node3D();AddChild(a);AddChild(b);
   MigrationChecks.Assert(TryWorldTree(a,14,71,pine)&&TryWorldTree(b,14,74,pine),"Generated tree available");
   Aabb bounds=default;bool found=false;BatchBounds(a,Transform3D.Identity,ref bounds,ref found);
   MigrationChecks.Assert(found&&Math.Abs(bounds.Size.Y-14)<.01f&&Math.Abs(bounds.Position.Y)<.01f,"Tree height and ground contact");
   var ma=a.FindChildren("*","MeshInstance3D",true,false).OfType<MeshInstance3D>().First();var mb=b.FindChildren("*","MeshInstance3D",true,false).OfType<MeshInstance3D>().First();
   MigrationChecks.Assert(ma.Mesh.GetInstanceId()==mb.Mesh.GetInstanceId(),"Repeated trees share mesh memory");
   MigrationChecks.Assert(ma.GetActiveMaterial(0).GetInstanceId()==mb.GetActiveMaterial(0).GetInstanceId(),"Repeated trees share matte materials");
   MigrationChecks.Assert(a.FindChildren("*","CollisionShape3D",true,false).OfType<CollisionShape3D>().Count()==1,"One authored tree collider");a.Free();b.Free();
  }
  for(int i=0;i<4;i++){
   var root=new Node3D();AddChild(root);Box(root,Vector3.Up*.5f,Vector3.One,Wood);PhysicsObstacle(root,new BoxShape3D{Size=Vector3.One},Vector3.Up*.5f);ApplyWorldFurniture(root,i);
   MigrationChecks.Assert(root.HasNode("GeneratedWorldProp"),"Generated house furniture / "+i);
   MigrationChecks.Assert(root.GetChildren().OfType<MeshInstance3D>().All(m=>!m.Visible),"Procedural furniture hidden / "+i);
   MigrationChecks.Assert(root.FindChildren("*","CollisionShape3D",true,false).Count==1,"Furniture collision preserved / "+i);root.Free();
  }
  GD.Print("PASS / world models / tree height and ground contact / shared mesh and material memory / furniture fallback and collision");
 }
}
