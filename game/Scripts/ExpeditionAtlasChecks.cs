using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 async void RunAtlasChecks(){try{
  game.NoDiskSave=true;paused=true;SetProcess(false);SetPhysicsProcess(false);CloseMenu();ClearEnemies();
  MigrationChecks.Assert(game.Healthy,"Atlas profile initialized: "+game.Notice);
  var w=game.World;var starter=game.Building.Pieces.Where(p=>p.Id.StartsWith("starter-house/")).ToArray();MigrationChecks.Assert(starter.Count(p=>p.Def.BaseKind==0)==9&&starter.Count(p=>p.Def.BaseKind==7)==9,"Editable starter house has complete floors and roofs");MigrationChecks.Assert(game.Building.Evaluate(game.Building.Pieces).Valid,"Starter house is structurally supported");int houses=0,city=0,industry=0,wet=0,bridges=0;
  foreach(double x in new[]{ExpeditionWorld.MinX+7,ExpeditionWorld.MaxX-7})foreach(double z in new[]{ExpeditionWorld.MinZ+7,ExpeditionWorld.MaxZ-7})MigrationChecks.Assert(w.ChunkIntersects(ExpeditionWorld.Cell(x),ExpeditionWorld.Cell(z)),"Partial boundary chunks retain terrain collision");
  for(int ix=(int)Math.Floor(ExpeditionWorld.MinX/64);ix<ExpeditionWorld.MaxX/64;ix++)for(int iz=(int)Math.Floor(ExpeditionWorld.MinZ/64);iz<ExpeditionWorld.MaxZ/64;iz++){
   var lot=w.Residential(ix,iz);if(lot==null)continue;houses++;
   MigrationChecks.Assert(w.WaterDistance(lot.X,lot.Z)>18&&w.StreetDistance(lot.X,lot.Z)>17,"House clear of roads and water");
   var street=w.NearestStreet(lot.X,lot.Z);double yaw=lot.Yaw*Math.PI/180;double dot=Math.Sin(yaw)*(street.X-lot.X)+Math.Cos(yaw)*(street.Z-lot.Z);
   MigrationChecks.Assert(dot>0,"Front door faces its actual street");
  }
  for(int x=(int)ExpeditionWorld.MinX+32;x<ExpeditionWorld.MaxX;x+=64)for(int z=(int)ExpeditionWorld.MinZ+32;z<ExpeditionWorld.MaxZ;z+=64){if(w.District(x,z)==3)industry++;if(w.District(x,z)==4)city++;if(w.IsRiver(x,z))wet++;if(ExpeditionWorldAtlas.Bridge(x,z))bridges++;MigrationChecks.Assert(double.IsFinite(w.Height(x,z)),"Finite world terrain");}
  MigrationChecks.Assert(houses>200&&city>100&&industry>100&&wet>100,"Whole atlas populated");
  var temp=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"ashen-atlas-"+Guid.NewGuid()+".json");
  var legacy=new ExpeditionGame(temp);legacy.Building.Pieces.Clear();legacy.Building.Pieces.Add(new(){Id="legacy-map-foundation",Kind=0,X=600,Z=400,Y=40,Health=100});legacy.Building.Pieces.Add(new(){Id="legacy-map-floor",Kind=1,X=600,Z=400,Y=40.5,Health=100});legacy.World.Changes.AtlasRevision=0;legacy.World.Changes.StarterHouseRevision=0;legacy.World.Changes.AtlasHomeFloor=0;legacy.World.Changes.AtlasPads.Clear();MigrationChecks.Assert(legacy.Persist(),"Legacy migration fixture saved");
  var migrated=new ExpeditionGame(temp);MigrationChecks.Assert(migrated.Healthy&&migrated.Building.Pieces.Any(p=>p.Id=="legacy-map-foundation"&&p.Y==40)&&migrated.Building.Evaluate(migrated.Building.Pieces).Valid&&System.IO.File.Exists(temp+".before-world-atlas-v1"),"Legacy structures and source save preserved");
  migrated.Building.Pieces.RemoveAll(p=>p.Id.StartsWith("starter-house/")&&p.Def.BaseKind==7);MigrationChecks.Assert(migrated.Persist(),"Edited starter house saved");var edited=new ExpeditionGame(temp);MigrationChecks.Assert(edited.Healthy&&!edited.Building.Pieces.Any(p=>p.Id.StartsWith("starter-house/")&&p.Def.BaseKind==7),"Removed house pieces do not respawn on reload");
  for(int i=0;i<ExpeditionContentSite.All.Length;i++){var p=ExpeditionContentSite.Place(w,i);MigrationChecks.Assert(w.Playable(p.X,p.Z)&&w.Tier(p.X,p.Z)==ExpeditionContentSite.All[i].Tier,"Field site fits its new district: "+ExpeditionContentSite.All[i].Name);}
  Teleport(ExpeditionWorld.HomeX,ExpeditionWorld.HomeZ);MigrationChecks.Assert(player.Position.Y>=w.HomeFloor,"Home spawn is above its floor");
  for(int f=0;f<3;f++)await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
  var edge=starter.First(p=>p.Def.BaseKind==0&&p.X==ExpeditionWorld.HomeX+3&&p.Z==ExpeditionWorld.HomeZ);var expansion=ExpeditionBuildSnap.Connect(edge,0,edge.X+2,edge.Z,1,0,0,0,game.Building.Pieces);MigrationChecks.Assert(game.Building.Placement(expansion).Valid,"Starting house accepts snapped expansion");
  GD.Print($"PASS / world atlas / 9x9 km / {houses} street-facing houses / {city} city cells / {industry} industrial cells / {wet} water cells / {bridges} bridge samples / all field sites placed");
  if(Array.IndexOf(OS.GetCmdlineUserArgs(),"--atlas-preview")>=0){
   weaponRoot.Visible=false;ShowMap();await ArtShot("atlas-world-map");CloseMenu();
   foreach(var shot in new[]{(Name:"atlas-home",X:ExpeditionWorld.HomeX,Z:ExpeditionWorld.HomeZ),(Name:"atlas-city",X:300d,Z:-3400d),(Name:"atlas-industry",X:2200d,Z:-3000d),(Name:"atlas-lake",X:5100d,Z:100d)}){
    Teleport(shot.X,shot.Z);ClearEnemies();SyncStructures();camera.GlobalPosition=Ground(shot.X-(shot.Name=="atlas-city"?70:30),shot.Z+(shot.Name=="atlas-city"?90:38))+Vector3.Up*(shot.Name=="atlas-city"?70:24);camera.LookAt(Ground(shot.X,shot.Z)+Vector3.Up*2);await ArtShot(shot.Name);
   }
  }
  GetTree().Quit();
 }catch(Exception error){GD.PushError(error.ToString());GetTree().Quit(1);}}
}
