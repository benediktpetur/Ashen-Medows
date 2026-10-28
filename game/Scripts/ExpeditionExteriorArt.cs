using Godot;
using System;
using System.Collections.Generic;

public partial class MigrationScene {
 readonly Dictionary<(int,Color,int),ShaderMaterial> exteriorMaterials=new();
 Material ExteriorFinish(int kind,Color color,int variant=0){
  var key=(kind,color,variant);if(exteriorMaterials.TryGetValue(key,out var cached))return cached;
  var material=new ShaderMaterial{Shader=GD.Load<Shader>("res://Shaders/exterior-surfaces.gdshader")};
  material.SetShaderParameter("paint",color);material.SetShaderParameter("surface_kind",kind);material.SetShaderParameter("variation",(float)variant);
  exteriorMaterials[key]=material;return material;
 }
 Color HouseRoofPaint(int style)=>(style%4) switch{0=>new Color(.22f,.24f,.27f),1=>new Color(.30f,.34f,.38f),2=>new Color(.40f,.24f,.18f),_=>new Color(.32f,.28f,.24f)};
 bool SteelCladding(Node3D shell,Vector3 center,Vector3 size,float yaw){
  var model=NewWorldModel("catalog-build/18",out var bounds);if(model==null)return false;
  // These are thin panel skins. Their three dimensions intentionally fit a
  // rectangular bay; the existing building supplies structure and collision.
  var holder=new Node3D{Name="GeneratedSteelCladding",Position=center,RotationDegrees=new(0,yaw,0)};shell.AddChild(holder,true);holder.AddChild(model);
  Vector3 scale=size/bounds.Size;model.Scale*=scale;model.Position=-bounds.GetCenter()*scale;return true;
 }
 void WarehouseCladding(Node3D shell,float size,float height){
  for(int bay=0;bay<2;bay++){
   float offset=(bay==0?-.25f:.25f)*size;
   SteelCladding(shell,new(offset,height*.5f,size*.5f+.07f),new(size*.5f-.04f,height-.12f,.10f),180);
   foreach(int side in new[]{-1,1})SteelCladding(shell,new(side*(size*.5f+.07f),height*.5f,offset),new(size*.5f-.04f,height-.12f,.10f),side*90);
  }
 }
}
