using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
public partial class MigrationScene {
 async Task WorldStoryChecks(){
  var a=ExpeditionStorySite.Nearby(game.World,0,0);var b=ExpeditionStorySite.Nearby(game.World,0,0);MigrationChecks.Assert(a.Count==b.Count&&a.Zip(b).All(p=>p.First.Id==p.Second.Id&&p.First.X==p.Second.X&&p.First.Kind==p.Second.Kind),"Story seeded layout");
  SpawnStorySites(true);var note=props.FirstOrDefault(p=>p.Kind==8&&p.Id.Contains("/scene/"));MigrationChecks.Assert(note!=null,"Restored world site missing");int count=props.Count(p=>p.Kind==8);SpawnStorySites(true);MigrationChecks.Assert(props.Count(p=>p.Kind==8)==count,"Duplicate story sites");
  var old=player.Position;player.Position=note.Position;int seen=game.Profile.Journal.Seen.Count;Interact();MigrationChecks.Assert(game.Profile.Journal.Seen.Contains(note.Id+"/"+note.Material),"Surface letter discovery");Interact();MigrationChecks.Assert(game.Profile.Journal.Seen.Count==seen+1,"Duplicate journal fragment");CloseMenu();var saved=new ExpeditionGame(game.PathName);MigrationChecks.Assert(saved.Profile.Journal.Seen.Contains(note.Id+"/"+note.Material),"Story journal persistence");
  var s=a.First(x=>x.Kind==4);game.Building.Pieces.Add(new BuildPiece{Kind=0,X=s.X,Z=s.Z,Y=game.World.Height(s.X,s.Z)});MigrationChecks.Assert(!StoryAllowed(s),"Story overlaps player structure");game.Building.Pieces.RemoveAt(game.Building.Pieces.Count-1);
  EnterDungeon(ExpeditionDungeon.Nearby(game.World,0,0)[0]);player.Position=new(2,0,0);ReadDungeonRecord();MigrationChecks.Assert(game.Profile.Journal.Seen.Any(id=>id.Contains("/vault/")&&id.EndsWith("/note/2")),"Dungeon register discovery");CloseMenu();await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);var q=PhysicsRayQueryParameters3D.Create(new Vector3(2,.9f,-1.5f),new Vector3(2,.9f,.4f));q.Exclude=new Godot.Collections.Array<Rid>{player.GetRid()};MigrationChecks.Assert(GetWorld3D().DirectSpaceState.IntersectRay(q).Count>0,"Dungeon desk collision");ExitDungeon();player.Position=old;ClearEnemies();GD.Print("PASS / restored world stories: deterministic layout, duplicate prevention, surface/dungeon records, journal save, settlement exclusion, furniture collision");
 }
 async void CaptureWorldStories(){try{
  testing=true;CloseMenu();paused=true;ClearEnemies();weaponRoot.Visible=false;game.World.Changes.Time=900;SpawnStorySites(true);camera.Fov=65;
  var site=props.FirstOrDefault(p=>p.Kind==8&&p.Material==3&&p.Id.Contains("/scene/"))??props.First(p=>p.Kind==8&&p.Id.Contains("/scene/"));camera.GlobalPosition=site.Node.GlobalPosition+new Vector3(7,2.8f,-7);camera.LookAt(site.Node.GlobalPosition+new Vector3(0,.65f,0));await ArtShot("world-systems-01-abandoned-site");
  var wreck=props.FirstOrDefault(p=>p.Kind==2&&p.Id.Contains("/scene/"));if(wreck!=null){camera.GlobalPosition=wreck.Position+new Vector3(6,2.4f,-6);camera.LookAt(wreck.Position+Vector3.Up*.6f);await ArtShot("world-systems-02-evacuation-wreck");}
  player.Position=site.Position;ReadStory(site.Material,site.Id+"/"+site.Material);await ArtShot("world-systems-03-letter");CloseMenu();EnterDungeon(ExpeditionDungeon.Nearby(game.World,0,0)[0]);ClearEnemies();player.Position=new(19,0,1);camera.GlobalPosition=new(19,1.65f,1);camera.LookAt(new Vector3(24,.8f,-4));await ArtShot("world-systems-04-triage-room");GD.Print("PASS / restored world systems GPU captures");GetTree().Quit();
 }catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}

