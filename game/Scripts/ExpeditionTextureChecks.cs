using Godot;
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

public partial class MigrationScene {
 async Task TextureShot(string name){
  for(int i=0;i<3;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
  hudRoot.Visible=false;await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
  string path=ProjectSettings.GlobalizePath("user://tests/art/"+name+".png");Directory.CreateDirectory(Path.GetDirectoryName(path));GetViewport().GetTexture().GetImage().SavePng(path);GD.Print("Captured "+name);
 }
 async void RunTextureChecks(){try{
  game.NoDiskSave=true;paused=true;CloseMenu();ClearEnemies();
  foreach(string name in new[]{"grass","soil","asphalt","concrete","gravel","wet-shore-mud","shore-pebbles","shallow-silt","siding","brick","plaster","lake-water"}){
   var texture=GeneratedTexture(name);MigrationChecks.Assert(texture!=null&&texture.GetWidth()==768&&texture.GetHeight()==768,"Generated albedo imports / "+name);
   MigrationChecks.Assert(GeneratedTexture(name)==texture,"Generated material textures are shared / "+name);
  }
  foreach(var pair in new[]{("lake-ripples","normal"),("water-detail","normal"),("shore-foam","mask")}){
   var texture=GeneratedTexture(pair.Item1,pair.Item2);MigrationChecks.Assert(texture!=null&&texture.GetWidth()==768,"Water detail imports / "+pair.Item1);
  }
  var terrain=TerrainInk();MigrationChecks.Assert(terrain.GetShaderParameter("grass_map").AsGodotObject()==GeneratedTexture("grass"),"Terrain uses generated grass");
  var water=GeneratedLakeMaterial();MigrationChecks.Assert(water.GetShaderParameter("foam_map").AsGodotObject()==GeneratedTexture("shore-foam","mask"),"Water uses generated shoreline foam");
  foreach(int kind in new[]{0,2,4,5}){var material=(ShaderMaterial)ExteriorFinish(kind,Wood);MigrationChecks.Assert(material.GetShaderParameter("use_generated").AsBool(),"Exterior uses generated texture / "+kind);}
  GD.Print("PASS / generated textures: 15 materials, imported albedos/normals/mask, shared resources, terrain/exterior/water bindings");
  if(Array.IndexOf(OS.GetCmdlineUserArgs(),"--texture-preview")>=0){
   SetProcess(false);SetPhysicsProcess(false);weaponRoot.Visible=false;hudRoot.Visible=false;game.World.Changes.Time=900;camera.Fov=65;
   Teleport(ExpeditionWorld.HomeX,ExpeditionWorld.HomeZ);ClearEnemies();SyncStructures();camera.GlobalPosition=Ground(ExpeditionWorld.HomeX-13,ExpeditionWorld.HomeZ+10)+Vector3.Up*3.5f;camera.LookAt(new Vector3((float)ExpeditionWorld.HomeX,(float)game.World.HomeFloor+1.5f,(float)ExpeditionWorld.HomeZ));await TextureShot("textures-home-siding");
   Teleport(ExpeditionWorld.SpawnX,ExpeditionWorld.SpawnZ+20);ClearEnemies();camera.GlobalPosition=Ground(ExpeditionWorld.SpawnX,ExpeditionWorld.SpawnZ+20)+Vector3.Up*2.2f;camera.LookAt(Ground(ExpeditionWorld.SpawnX,ExpeditionWorld.SpawnZ-12)+Vector3.Up*1.2f);await TextureShot("textures-neighborhood-road");
   var shore=(from x in Enumerable.Range(0,120).Select(i=>5000+i*8d) from z in Enumerable.Range(0,200).Select(i=>-1200+i*8d) where game.World.WaterDistance(x,z)>8&&game.World.WaterDistance(x,z)<18&&game.World.Playable(x,z)&&game.World.StreetDistance(x,z)>20 orderby Math.Pow(x-5450,2)+Math.Pow(z+400,2) select new Vector2((float)x,(float)z)).First();
   var lake=(from x in Enumerable.Range(-12,25).Select(i=>shore.X+i*8d) from z in Enumerable.Range(-12,25).Select(i=>shore.Y+i*8d) where game.World.WaterDistance(x,z)<-15 orderby Math.Pow(x-shore.X,2)+Math.Pow(z-shore.Y,2) select new Vector3((float)x,(float)ExpeditionWorld.WaterLevel,(float)z)).First();
   Teleport(shore.X,shore.Y);ClearEnemies();camera.GlobalPosition=lake+(Ground(shore.X,shore.Y)-lake).Normalized()*18+Vector3.Up*15;camera.LookAt(lake);await TextureShot("textures-lake-shore");
   camera.GlobalPosition=lake+Vector3.Up*6+(Ground(shore.X,shore.Y)-lake).Normalized()*12;camera.LookAt(lake);await TextureShot("textures-water-detail");
  }
  GetTree().Quit();
 }catch(Exception error){GD.PushError(error.ToString());GetTree().Quit(1);}}
}
