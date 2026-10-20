using Godot;
public partial class MigrationScene {
 Texture2D inkAtlas;ShaderMaterial terrainInk;
 ShaderMaterial TerrainInk(){if(terrainInk!=null)return terrainInk;terrainInk=new ShaderMaterial{Shader=GD.Load<Shader>("res://Shaders/terrain.gdshader")};terrainInk.SetShaderParameter("ink_atlas",GD.Load<Texture2D>("res://Assets/graphic-novel-atlas-v1.png"));return terrainInk;}
 readonly System.Collections.Generic.Dictionary<(Color,int,float),ShaderMaterial> residentialMaterialCache=new();
 ShaderMaterial Textured(Color color,int cell,float scale=1){if(residentialMaterialCache.TryGetValue((color,cell,scale),out var cached))return cached;if(inkAtlas==null)inkAtlas=GD.Load<Texture2D>("res://Assets/graphic-novel-atlas-v1.png");var m=new ShaderMaterial{Shader=GD.Load<Shader>("res://Shaders/comic.gdshader")};m.SetShaderParameter("base_color",color);m.SetShaderParameter("texture_cell",cell);m.SetShaderParameter("texture_scale",scale);m.SetShaderParameter("ink_atlas",inkAtlas);m.NextPass=new ShaderMaterial{Shader=GD.Load<Shader>("res://Shaders/ink.gdshader")};residentialMaterialCache[(color,cell,scale)]=m;return m;}
}
