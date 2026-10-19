using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
public partial class MigrationScene {
 async Task WorldWorksChecks(){
  contentFixture=true;ClearEnemies();
  foreach(var w in WorldWork.All)foreach(bool strip in new[]{false,true}){
   game=new ExpeditionGame(ProjectSettings.GlobalizePath("user://tests/works-"+Guid.NewGuid()+".json")){NoDiskSave=true};game.Craft.Materials=Enumerable.Repeat(200,7).ToArray();game.Craft.BenchTier=game.Craft.UnlockedTier=5;game.Shield=100;
   var at=ExpeditionContentSite.Place(game.World,w.Site);Teleport(at.X,at.Z+5.5);ClearEnemies();
   var field=Field(w.Site);if(w.Tier>=2){WorkAction("restore:"+w.Site);MigrationChecks.Assert(game.World.Changes.Work(w.Site).Choice==0,"Project rejects uncleared guards / "+w.Name);}
   field.DeadGuards=(1<<FieldGuardCount(w.Site))-1;field.Repaired=true;
   game.Save.X=0;game.Save.Z=0;MigrationChecks.Assert(!game.BeginWork(w.Site,strip),"Remote commitment rejected");game.Save.X=player.Position.X;game.Save.Z=player.Position.Z;if(w.Tier>=2){game.Craft.UnlockedTier=1;WorkAction("strip:"+w.Site);MigrationChecks.Assert(game.World.Changes.Work(w.Site).Choice==0,"High-tier project cannot bypass progression");game.Craft.UnlockedTier=5;}int timberBefore=game.Craft.Materials[0];game.Craft.Materials[0]=0;WorkAction("restore:"+w.Site);MigrationChecks.Assert(game.World.Changes.Work(w.Site).Choice==0,"Missing construction materials rejected");game.Craft.Materials[0]=timberBefore;
   var s=game.World.Changes.Work(w.Site);var before=game.Craft.Materials.ToArray();WorkAction((strip?"strip:":"restore:")+w.Site);
   MigrationChecks.Assert(s.Choice==(strip?2:1)&&before.Zip(game.Craft.Materials,(a,b)=>a-b).SequenceEqual(w.Cost(strip)),"Project cost and branch / "+w.Name);
   var paid=game.Craft.Materials.ToArray();WorkAction((strip?"restore:":"strip:")+w.Site);MigrationChecks.Assert(game.Craft.Materials.SequenceEqual(paid),"Committed choice cannot be repurchased or reversed");
   player.Position=Ground(at.X+60,at.Z+60);TickWorldWorks(60);MigrationChecks.Assert(!s.Finished,"Remote work cannot finish itself");player.Position=Ground(at.X,at.Z+5.5);game.Save.X=player.Position.X;game.Save.Z=player.Position.Z;
   TickWorldWorks(15);MigrationChecks.Assert(!s.Finished,"Work is not instantaneous");TickWorldWorks(40);MigrationChecks.Assert(s.Finished,"Nearby active work completes / "+w.Name);
   double factor=game.World.WorkPressure(at.X,at.Z);MigrationChecks.Assert(strip?factor>1:w.Kind is "warning" or "refuge" or "trail"?factor<1:factor==1,"Chosen project changes actual ecology factor");
   MigrationChecks.Assert(game.World.SpawnHeat(ExpeditionWorld.SpawnX,ExpeditionWorld.SpawnZ)==0,"Starting sanctuary stays protected");
   game.FinishWork(s);int stock=s.Stock.Sum()+s.Medicine;
   if(strip){MigrationChecks.Assert(stock>0,"Every stripping branch has real recoverable supplies");game.SupplyWorks(600);MigrationChecks.Assert(s.Stock.Sum()+s.Medicine==stock,"Dismantled sites do not regenerate rewards");}
   else{for(int n=0;n<10;n++)game.SupplyWorks(600);MigrationChecks.Assert(s.Stock[7]<=6&&s.Stock[8]<=6&&s.Stock[9]<=3,"Restored production obeys shared caps");if(w.Kind=="trail")MigrationChecks.Assert(game.World.Changes.Paths.Count>0,"Relief route leaves permanent terrain paths");if(w.Kind=="refuge")MigrationChecks.Assert(game.JourneyRefuge(at.X,at.Z+5.5),"Restored refuge is a real expedition departure point");if(w.Kind is "clinic" or "refuge"){game.Life.Sickness=500;game.Life.Energy=30;game.Life.Health=40;MigrationChecks.Assert(game.UseWork(w.Site),"Service works when ready");MigrationChecks.Assert(!game.UseWork(w.Site)&&game.Life.Health==40,"Shared service cooldown and slow health recovery");}}
   int food=game.Life.Food,wood=game.Craft.Materials[0],med=game.Life.Medicine;int available=s.Stock[7],timber=s.Stock[0],medicine=s.Medicine;game.CollectWork(w.Site);MigrationChecks.Assert(game.Life.Food==food+available&&game.Craft.Materials[0]==wood+timber&&game.Life.Medicine==med+medicine,"Shared stock transfers to acting character");food=game.Life.Food;game.CollectWork(w.Site);MigrationChecks.Assert(game.Life.Food==food,"Shared rewards cannot duplicate");
   game.NoDiskSave=false;MigrationChecks.Assert(game.Persist(),"World project save");var loaded=new ExpeditionGame(game.PathName);var saved=loaded.World.Changes.Work(w.Site);MigrationChecks.Assert(loaded.Healthy&&saved.Finished&&saved.Choice==s.Choice&&saved.Stock.SequenceEqual(s.Stock)&&loaded.World.WorkPressure(at.X,at.Z)==factor,"Costs, choice, supplies and ecology survive reload");await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
  }
  var position=ExpeditionContentSite.Place(game.World,24);Teleport(position.X,position.Z+5.5);SpawnContentAtlas();DrawWorldWorks();MigrationChecks.Assert(contentRoots[24].HasNode("WorldWork"),"Choice has actual streamed world artwork");ShowWorldWorksLedger();CloseMenu();await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
  GD.Print("PASS / world works: all 24 branches, guard/range/tier/cost gates, paused work, irrevocable choices, capped services, conserved rewards, clinic cooldown/slow healing, real paths/refuges, ecology and reload/art");
 }
 async void RunWorldWorksChecks(){try{await WorldWorksChecks();SetProcess(false);SetPhysicsProcess(false);ClearChildren(worldRoot);ClearChildren(terrainRoot);ClearChildren(buildRoot);props.Clear();chunks.Clear();contentRoots.Clear();await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);GetTree().Quit();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
 async void CaptureWorldWorks(){try{contentFixture=true;game.NoDiskSave=true;game.World.Changes.Time=900;ClearEnemies();weaponRoot.Visible=false;hudRoot.Visible=false;var overview=new Camera3D{Current=true,Fov=58};AddChild(overview);
  async Task View(int site,string name,int choice){var at=ExpeditionContentSite.Place(game.World,site);Teleport(at.X,at.Z+6);SpawnContentAtlas();ClearEnemies();var s=game.World.Changes.Work(site);s.Choice=choice;s.Finished=choice!=0;s.Outcome=choice==1?WorldWork.At(site).Restore:choice==2?WorldWork.At(site).Strip:"";Field(site).Seen=true;DrawWorldWorks();var root=contentRoots[site];overview.Position=root.Position+new Vector3(1,6,22);overview.LookAt(root.Position+new Vector3(-3,1,5));hudRoot.Visible=false;await ArtShot(name);}
  await View(3,"works-01-garden-before",0);await View(3,"works-02-garden-restored",1);await View(3,"works-03-garden-stripped",2);await View(7,"works-04-clean-water",1);await View(10,"works-05-clinic",1);await View(8,"works-06-warning-relay",1);ShowWorldWork(8);await ArtShot("works-07-project-record");ShowWorldWorksLedger();await ArtShot("works-08-world-ledger");CloseMenu();GetTree().Quit();
 }catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}
