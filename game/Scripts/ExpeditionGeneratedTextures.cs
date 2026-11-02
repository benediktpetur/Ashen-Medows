using Godot;
using System.Collections.Generic;

public partial class MigrationScene {
 readonly Dictionary<string,Texture2D> generatedTextures=new();
 Texture2D GeneratedTexture(string name,string map="albedo"){
  string key=name+"/"+map;
  if(!generatedTextures.TryGetValue(key,out var texture)){
   texture=GD.Load<Texture2D>("res://Assets/GeneratedTextures/"+key+".png");
   generatedTextures[key]=texture;
  }
  return texture;
 }
 void GeneratedTerrainTextures(ShaderMaterial material){
  foreach(string name in new[]{"grass","soil","asphalt","concrete","gravel","wet-shore-mud","shore-pebbles","shallow-silt"})
   material.SetShaderParameter(name.Replace('-','_')+"_map",GeneratedTexture(name));
 }
 ShaderMaterial GeneratedLakeMaterial(){
  var material=new ShaderMaterial{Shader=GD.Load<Shader>("res://Shaders/lake-water.gdshader")};
  material.SetShaderParameter("water_map",GeneratedTexture("lake-water"));
  material.SetShaderParameter("ripple_map",GeneratedTexture("lake-ripples","normal"));
  material.SetShaderParameter("detail_map",GeneratedTexture("water-detail","normal"));
  material.SetShaderParameter("foam_map",GeneratedTexture("shore-foam","mask"));
  material.SetShaderParameter("silt_map",GeneratedTexture("shallow-silt"));
  return material;
 }
}
