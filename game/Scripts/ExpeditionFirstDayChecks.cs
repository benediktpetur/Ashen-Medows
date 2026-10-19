using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
public partial class MigrationScene {
 void FirstDayCoopChecks(CoopActor actor){
  var oldPosition=actor.Body.Position;double oldX=actor.Game.Save.X,oldZ=actor.Game.Save.Z;string profile=Json.Write(actor.Game.Profile);
  int hostWater=game.Craft.Water,hostMarks=game.Life.FirstDayMarks;try{
   actor.Game.Life.FirstDayCarriers=new();actor.Game.Life.FirstDayMarks=0;int initial=actor.Game.Craft.Water;
   actor.Game.Save.X=0;actor.Game.Save.Z=0;WithCoopActor(actor,()=>ExecuteCoopAction("firstday","water"));MigrationChecks.Assert(actor.Game.Craft.Water==initial,"Remote guest cannot claim trailhead ration");
   actor.Body.Position=Ground(ExpeditionWorld.SpawnX+13,ExpeditionWorld.SpawnZ-7);actor.Game.Save.X=actor.Body.Position.X;actor.Game.Save.Z=actor.Body.Position.Z;
   WithCoopActor(actor,()=>{ExecuteCoopAction("firstday","water");ExecuteCoopAction("firstday","water");ExecuteCoopAction("drink","");});
   MigrationChecks.Assert(actor.Game.Craft.Water==initial+2&&(actor.Game.Life.FirstDayMarks&1)!=0,"Guest claims once and consumes own clean water");
   var snapshot=Json.Read<CoopPersonalState>(CoopProfileJson(actor.Game)).Profile;MigrationChecks.Assert(snapshot.Life.FirstDayCarriers.Contains(game.Save.Seed)&&(snapshot.Life.FirstDayMarks&1)!=0,"Private profile carries first-day progress and finite ration record");
   MigrationChecks.Assert(game.Craft.Water==hostWater&&game.Life.FirstDayMarks==hostMarks,"Guest opening cannot alter host character");
   GD.Print("PASS / first-day co-op authority: range, duplicate protection, private ration/drink/milestones and profile transport");
  }finally{actor.Game.Save.Profile=Json.Read<ExpeditionLootSave>(profile);actor.Body.Position=oldPosition;actor.Game.Save.X=oldX;actor.Game.Save.Z=oldZ;BindWorld(actor.Game);}
 }
 async Task FirstDayChecks(){
  ClearEnemies();Teleport(ExpeditionWorld.SpawnX,ExpeditionWorld.SpawnZ);SpawnFirstDay();
  MigrationChecks.Assert(game.FirstDayStep==0,"Fresh character starts with water guidance");
  int water=game.Craft.Water;game.FirstDayAction("water");MigrationChecks.Assert(game.Craft.Water==water,"Carrier rejects remote collection");
  Teleport(ExpeditionWorld.SpawnX+13,ExpeditionWorld.SpawnZ-7);game.FirstDayAction("water");game.FirstDayAction("water");MigrationChecks.Assert(game.Craft.Water==water+3,"Carrier supplies exactly once");
  game.Change(()=>game.Drink());MigrationChecks.Assert(game.FirstDayStep==1,"Only an actual clean drink advances water step");
  foreach(var p in props.Where(p=>p.Id.StartsWith($"first-day/{game.Save.Seed}/forage/")).ToArray())game.Gather(p.Id,p.Material,p.Tier,p.Amount);
  void Craft(string kind,int result){var r=ExpeditionRecipe.All.First(r=>r.Kind==kind&&r.Result==result);if(!game.Knows(r))MigrationChecks.Assert(game.Research(Array.IndexOf(ExpeditionRecipe.All,r)),"First-day research / "+r.Name+" / "+game.Notice);MigrationChecks.Assert(game.CraftRecipe(r)=="Crafted "+r.Name,"First-day crafting / "+r.Name+" / "+game.Notice);game.TickFirstDay();}
  Craft("weapon",11);Craft("tool",0);MigrationChecks.Assert(game.FirstDayStep==3,"Crafted spear and axe count without free equipment");
  // Exercise aimed trunk collision and actual felling, not an interaction shortcut.
  SpawnObjects();var tree=props.Where(p=>p.Kind==0&&p.Tier==1).OrderBy(p=>Distance(p.Position,player.Position)).First();player.Position=tree.Position+new Vector3(1.5f,.1f,0);camera.LookAt(tree.Position+Vector3.Up*1.2f);
  await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
  for(int i=0;i<4;i++){game.Clock+=1;ChopTree();}
  MigrationChecks.Assert(game.World.Changes.Cut(tree.Id)&&(game.Life.FirstDayMarks&8)!=0,"Aimed swings fell a tree and persist the milestone");
  Craft("bench",1);MigrationChecks.Assert(game.FirstDayStep==5,"Workbench advances camp stage");
  // Place the real blueprint set using real costs and the structural solver.
  double x=ExpeditionWorld.SpawnX+45,z=ExpeditionWorld.SpawnZ-25,y=game.World.Height(x,z);
  BuildPiece Place(int kind,double xx,double zz,double yy,double yaw=0){var r=ExpeditionRecipe.All.First(r=>r.Kind=="build"&&r.Result==kind);if(!game.Knows(r))MigrationChecks.Assert(game.Research(Array.IndexOf(ExpeditionRecipe.All,r)),"Camp research / "+r.Name+" / "+game.Notice);var p=new BuildPiece{Kind=kind,X=xx,Z=zz,Y=yy,Yaw=yaw};MigrationChecks.Assert(game.PlaceBlueprint(p),"Camp placement / "+p.Def.Name+" / "+game.Notice);return p;}
  Place(0,x,z,y);Place(2,x,z-1.5,y+.5);Place(2,x,z+1.5,y+.5);Place(7,x,z,y+3);
  int cot=Array.FindIndex(BuildDefinition.All,d=>d.Function=="bed"),fire=Array.FindIndex(BuildDefinition.All,d=>d.Function=="cook"),chest=Array.FindIndex(BuildDefinition.All,d=>d.Function=="storage");
  Place(cot,x,z,y+.5);Place(fire,x+5,z,game.World.Height(x+5,z));var storage=Place(chest,x+5,z+3,game.World.Height(x+5,z+3));game.TickFirstDay();MigrationChecks.Assert(game.FirstDayStep==6,"Real shelter plus nearby utilities required");
  Teleport(ExpeditionWorld.SpawnX-19,ExpeditionWorld.SpawnZ-13);Interact();Interact();MigrationChecks.Assert(game.Life.RawWater==2,"Physical trailhead pump supplies untreated water");game.TreatWater(false);
  Teleport(storage.X,storage.Z);int food=game.Life.Food;UseBlueprint(storage.Id+"|deposit|7");UseBlueprint(storage.Id+"|deposit|8");game.TickFirstDay();MigrationChecks.Assert(game.FirstDayStep==7&&game.Life.Food<food,"Real conserved chest deposits finish first day");
  game.FirstDayAction("hide");game.Persist();var loaded=new ExpeditionGame(game.PathName);MigrationChecks.Assert(loaded.FirstDayStep==7&&loaded.Life.FirstDayHidden,"Opening milestones and opt-out survive reload");
  game.FirstDayAction("water");MigrationChecks.Assert(game.Craft.Water<water+6,"Completion cannot farm the carrier");
  GD.Print("PASS / first day: fresh character, local finite carrier, real drink/research/spear/axe, aimed felling, affordable structural camp, well boiling, conserved stores and reload");
 }
 async void RunFirstDayChecks(){try{await FirstDayChecks();GetTree().Quit();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
 async void CaptureFirstDay(){try{Teleport(ExpeditionWorld.SpawnX+7,ExpeditionWorld.SpawnZ-1);SpawnFirstDay();camera.GlobalPosition=Ground(ExpeditionWorld.SpawnX+7,ExpeditionWorld.SpawnZ-1)+Vector3.Up*2.6f;camera.LookAt(Ground(ExpeditionWorld.SpawnX+13,ExpeditionWorld.SpawnZ-7)+Vector3.Up);await ArtShot("first-day-01-trailhead");ShowFirstDay(true);await ArtShot("first-day-02-field-notes");CloseMenu();game.Life.FirstDayMarks=31;ShowFirstDay();await ArtShot("first-day-03-camp-guide");CloseMenu();GetTree().Quit();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}
