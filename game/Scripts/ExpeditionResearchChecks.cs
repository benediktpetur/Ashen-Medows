using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 void ResearchTreeChecks(){var original=game;int previous=researchChoice;try{
  game=new ExpeditionGame(ProjectSettings.GlobalizePath("user://tests/research-"+Guid.NewGuid()+".json")){NoDiskSave=true};game.Craft.Materials=Enumerable.Repeat(100,7).ToArray();
  MigrationChecks.Assert(ExpeditionRecipe.All.Select(ExpeditionGame.ResearchId).Distinct().Count()==ExpeditionRecipe.All.Length,"Stable unique research IDs");
  researchChoice=0;ShowResearchTree();void Press(string title){var button=menu.FindChildren("*","Button",true,false).OfType<Godot.Button>().FirstOrDefault(n=>n.Text==title);MigrationChecks.Assert(button!=null&&!button.Disabled,"Research UI button / "+title);button.EmitSignal(Godot.Button.SignalName.Pressed);}
  Press("TRACK THIS GOAL");MigrationChecks.Assert(game.TrackedResearch==ExpeditionRecipe.All[0],"Research tracking UI");Press("BUILD / RESEARCH WORKSHOP");MigrationChecks.Assert(game.Craft.BenchTier==1&&game.Craft.Materials[0]==94&&game.Craft.Materials[1]==96,"Research action spends real costs / unlocks bench");MigrationChecks.Assert(game.ResearchComplete(ExpeditionRecipe.All[0]),"Researched workshop state");
  int wood=game.Craft.Materials[0];game.CraftRecipe(ExpeditionRecipe.All[0]);MigrationChecks.Assert(game.Craft.Materials[0]==wood,"Duplicate research spend");game.CraftRecipe(ExpeditionRecipe.All[1]);MigrationChecks.Assert(game.Craft.BenchTier==1,"Research cannot bypass zone materials");
  for(int tier=2;tier<=5;tier++){game.Craft.UnlockedTier=tier;game.CraftRecipe(ExpeditionRecipe.All[tier-1]);MigrationChecks.Assert(game.Craft.BenchTier==tier,"Research main path / tier "+tier);}
  game.Career.Level=2;var gun=ExpeditionRecipe.All.First(r=>r.Kind=="weapon"&&r.Tier==5);MigrationChecks.Assert(game.ResearchAccess(gun).Contains("Crafting"),"Research skill gate");game.Skills.Crafting.Level=4;MigrationChecks.Assert(game.ResearchAccess(gun)=="Unlocked","Maximum tier research accessibility");foreach(int weapon in new[]{3,4})game.Research(Array.FindIndex(ExpeditionRecipe.All,r=>r.Kind=="weapon"&&r.Result==weapon));game.Research(Array.IndexOf(ExpeditionRecipe.All,gun));game.CraftRecipe(gun);MigrationChecks.Assert(game.Craft.Weapons.Contains(5),"Max-tier roadmap reward");game.TrackResearch(4);game.Skills.Die();var copy=Json.Read<ExpeditionGameSave>(Json.Write(game.Save));MigrationChecks.Assert(copy.Profile.Crafting.BenchTier==5&&copy.Profile.Crafting.ResearchTarget==ExpeditionGame.ResearchId(ExpeditionRecipe.All[4]),"Portable research target / death preserves progress");
  GD.Print("PASS / research tree UI, costs, duplicate prevention, tier prerequisites, skill gates, max-tier reward, tracking persistence and death retention");
 }finally{CloseMenu();game=original;researchChoice=previous;researchScroll=0;}}
}
