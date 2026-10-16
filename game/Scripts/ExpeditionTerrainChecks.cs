using Godot;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
public partial class MigrationScene {
 async Task TerrainReliefChecks(){
  var w=new ExpeditionWorld(431);MigrationChecks.Assert(Math.Abs(w.BaseHeight(0,12)-13)<.001,"Spawn pad height");double lo=double.MaxValue,hi=double.MinValue;for(int x=-200;x<=200;x+=20)for(int z=-240;z<=100;z+=20){double h=w.BaseHeight(x,z);lo=Math.Min(lo,h);hi=Math.Max(hi,h);}MigrationChecks.Assert(hi-lo>15,"Nearby meadow relief missing");
  double grade=0;for(int z=-2300;z<100;z+=4)grade=Math.Max(grade,Math.Abs(w.BaseHeight(w.LegacyRouteX(z+4),z+4)-w.BaseHeight(w.LegacyRouteX(z),z))/4);MigrationChecks.Assert(grade<.18,"Road grade too steep");
  string path=ProjectSettings.GlobalizePath("user://tests/terrain-legacy.json");if(File.Exists(path))File.Delete(path);var g=new ExpeditionGame(path);g.World.Changes.TerrainRevision=0;double x0=130,z0=-110,old=g.World.LegacyBaseHeight(x0,z0);g.Building.Pieces.Add(new BuildPiece{Kind=0,X=x0,Z=z0,Y=old-.2});g.World.Changes.Paths.Add(new ExpeditionEarthPatch{X=160,Z=-110,Height=g.World.LegacyBaseHeight(160,-110)});g.Normalize();MigrationChecks.Assert(g.Healthy&&g.Building.Evaluate(g.Building.Pieces).Valid,"Legacy structure lost terrain support");MigrationChecks.Assert(Math.Abs(g.World.Height(x0,z0)-old)<.001,"Legacy ground changed beneath build");MigrationChecks.Assert(g.World.Changes.LegacyTerrainWeight(1000,0)==0,"Legacy preservation flattens whole map");MigrationChecks.Assert(g.Persist(),"Terrain migration save");var loaded=new ExpeditionGame(path);MigrationChecks.Assert(loaded.Healthy&&loaded.World.Changes.TerrainRevision==2&&Math.Abs(loaded.World.Height(x0,z0)-old)<.001,"Terrain migration reload");
  var start=player.Position;Teleport(90,-100);await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);var point=Ground(90,-100);var q=PhysicsRayQueryParameters3D.Create(point+Vector3.Up*4,point-Vector3.Up*4);q.Exclude=new Godot.Collections.Array<Rid>{player.GetRid()};var hit=GetWorld3D().DirectSpaceState.IntersectRay(q);MigrationChecks.Assert(hit.Count>0&&Math.Abs(((Vector3)hit["position"]).Y-point.Y)<.5,"Hill collision differs from terrain height");Teleport(start.X,start.Z);GD.Print($"PASS / terrain relief: nearby height range {hi-lo:F1}m, road grade {grade:P0}, saved settlement preservation/reload and hill collision");
 }
 async void CaptureTerrainRelief(){try{
  testing=true;CloseMenu();ClearEnemies();weaponRoot.Visible=false;game.World.Changes.Time=900;camera.Fov=70;
  Teleport(0,8);camera.GlobalPosition=Ground(0,8)+Vector3.Up*1.65f;camera.LookAt(Ground(135,-115)+Vector3.Up*3);await ArtShot("terrain-01-valley-hills");
  Teleport(135,-115);camera.GlobalPosition=Ground(135,-115)+Vector3.Up*2;camera.LookAt(Ground(-100,-80)+Vector3.Up*2);await ArtShot("terrain-02-meadow-overlook");
  Teleport(game.World.RouteX(-600),-600);camera.GlobalPosition=Ground(player.Position.X,player.Position.Z)+Vector3.Up*1.65f;camera.LookAt(Ground(260,-650)+Vector3.Up*3);await ArtShot("terrain-03-woodland-ridge");
  Teleport(160,-480);camera.GlobalPosition=Ground(160,-480)+new Vector3(0,52,-45);camera.LookAt(Ground(200,-610));await ArtShot("terrain-04-hills-overview");GD.Print("PASS / rolling terrain GPU captures");GetTree().Quit();
 }catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}

