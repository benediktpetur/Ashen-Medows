using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 async void CaptureLivingWorld(){try{
  livingFixture=feverFixture=true;game.NoDiskSave=true;game.World.Changes.Time=900;CloseMenu();ClearEnemies();weaponRoot.Visible=false;game.Craft.BenchTier=game.Craft.UnlockedTier=5;game.Skills.Crafting.Level=5;game.Career.Level=5;
  var town=Town(0);Teleport(town.X,town.Z);TickLivingCommunities(.1);camera.GlobalPosition=Ground(town.X,town.Z)+new Vector3(15,9,18);camera.LookAt(Ground(town.X,town.Z)+Vector3.Up);await ArtShot("living-01-emberstead");ShowLivingPanel(0);await ArtShot("living-02-community-ledger");CloseMenu();
  Teleport(FeverX,FeverZ+12);Fever.Stage=3;SetFeverBoy(FeverPoint(.8f,.06f,2.4f));DrawFeverStory(.1);camera.GlobalPosition=FeverPoint(-1,1.35f,3.4f);camera.LookAt(FeverPoint(-2.6f,.9f,1.4f));await ArtShot("living-03-lily-and-teddy");ShowFeverChoices(false);await ArtShot("living-04-door-choices");CloseMenu();
  Fever.Choice="truth";Fever.GivenName="Ash";Fever.Disclosure="name";FinishFeverArc();game.World.Changes.Time=Fever.FledAt+301;game.TickLivingWorld();Teleport(Fever.GraveX,Fever.GraveZ);SpawnLivingWorld();camera.GlobalPosition=Ground(Fever.GraveX,Fever.GraveZ)+new Vector3(2.6f,2.4f,3);camera.LookAt(Ground(Fever.GraveX,Fever.GraveZ)+Vector3.Up*.3f);await ArtShot("living-05-lilys-grave");
  var mercy=Town(2);Teleport(mercy.X,mercy.Z);TickLivingCommunities(.1);ShowLivingPanel(2);await ArtShot("living-06-mercy-row");CloseMenu();
  game.Craft.ResearchPoints=20;game.Craft.ResearchFocus["Survival"]="Waterworks";researchTab="Survival";researchChoice=Array.FindIndex(ExpeditionRecipe.All,r=>r.Kind=="survival-tech"&&r.Result==1);game.Craft.Researched.Add(ExpeditionGame.ResearchId(ExpeditionRecipe.All.First(r=>r.Kind=="survival-tech"&&r.Result==5)));ShowResearchTree();await ArtShot("living-07-research-specialization");CloseMenu();
  game.Life.RawWater=4;game.Craft.Water=3;game.Life.Sickness=480;game.Life.Energy=15;game.Life.Morale=15;game.Life.Health=62;game.Life.Wounds.Add(new());ShowWounds();await ArtShot("living-08-health-and-water");CloseMenu();GD.Print("PASS / living world GPU captures");GetTree().Quit();
 }catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}
