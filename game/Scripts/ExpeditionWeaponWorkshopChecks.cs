using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class MigrationScene {
 void WeaponWorkshopChecks(){var original=game;try{
  game=new ExpeditionGame(ProjectSettings.GlobalizePath("user://tests/workshop-"+Guid.NewGuid()+".json")){NoDiskSave=true};
  game.Craft.BenchTier=game.Craft.UnlockedTier=5;game.Career.Level=2;game.Skills.Crafting.Level=5;game.Craft.Materials=Enumerable.Repeat(200,7).ToArray();
  foreach(var recipe in ExpeditionRecipe.All){var visited=new HashSet<string>();var node=recipe;while(node!=null){MigrationChecks.Assert(visited.Add(ExpeditionGame.ResearchId(node)),"Acyclic research graph / "+recipe.Name);node=game.ResearchParent(node);}}
  var smg=ExpeditionRecipe.All.First(r=>r.Kind=="weapon"&&r.Result==29);MigrationChecks.Assert(game.ResearchReason(smg).Contains("Trail pistol"),"Real predecessor blocks research");
  var pistol=ExpeditionRecipe.All.First(r=>r.Kind=="weapon"&&r.Result==0);game.Craft.Researched.Add(ExpeditionGame.ResearchId(pistol));MigrationChecks.Assert(game.ResearchReason(smg)=="Ready","Weapon path available after foundation");
  game.Research(Array.IndexOf(ExpeditionRecipe.All,smg));MigrationChecks.Assert(game.Craft.ResearchFocus["Weapons and armor"]=="Ballistics","Advanced weapon research commits specialization");
  var limbs=ExpeditionRecipe.All.First(r=>r.Kind=="weapon-mod"&&r.Result==9);game.Craft.Researched.Add(ExpeditionGame.ResearchId(ExpeditionRecipe.All.First(r=>r.Kind=="weapon"&&r.Result==6)));MigrationChecks.Assert(game.ResearchReason(limbs).Contains("SPECIALIZATION"),"Bow path remains a meaningful exclusive choice");
  game.Craft.Researched.AddRange(ExpeditionRecipe.All.Select(ExpeditionGame.ResearchId));
  for(int id=28;id<34;id++){var r=ExpeditionRecipe.All.First(r=>r.Kind=="weapon"&&r.Result==id);int material=r.Tier+1;int before=game.Craft.Materials[material];game.Craft.Materials[material]=0;MigrationChecks.Assert(game.RecipeReason(r)!="Ready","Zone materials gate / "+r.Name);game.Craft.Materials[material]=before;game.CraftRecipe(r);MigrationChecks.Assert(game.Craft.Weapons.Contains(id)&&game.Craft.Weapon==id,"New weapon craft / "+r.Name);var model=new Node3D();ItemWeapon(model,id);MigrationChecks.Assert(model.FindChildren("*","MeshInstance3D",true,false).Count>3,"New weapon model / "+r.Name);model.Free();}
  game.Craft.ResearchFocus["Weapons and armor"]="Ballistics";var armorPath=ExpeditionRecipe.All.First(r=>r.Kind=="armor"&&r.Tier==3&&r.Result==3);MigrationChecks.Assert(game.FocusReason(armorPath)=="Ready","Armor progression does not exclude gun or bow users");
  game.Craft.Weapons.Add(0);game.Equip(0);game.Craft.Ammo=12;game.Craft.Reserve=20;game.Craft.CommitAmmo();
  void Make(int id){game.CraftRecipe(ExpeditionRecipe.All.First(r=>r.Kind=="weapon-mod"&&r.Result==id));}
  Make(6);MigrationChecks.Assert(game.FitPart(6)&&game.Magazine==18,"Extended magazine changes real capacity");MigrationChecks.Assert(game.Craft.Ammo+game.Craft.Reserve==32,"Fitting does not create ammunition");game.Reload();MigrationChecks.Assert(game.Craft.Ammo==18,"Extended magazine loads existing rounds");game.FitPart(6,true);MigrationChecks.Assert(game.Craft.Ammo==12&&game.Craft.Ammo+game.Craft.Reserve==32,"Removal returns excess loaded rounds without duplication");
  Make(3);Make(4);game.FitPart(3);MigrationChecks.Assert(game.WeaponNoise==.5,"Suppressor affects actual noise radius");game.FitPart(4);MigrationChecks.Assert(game.WeaponNoise==1.25&&game.Craft.SpareParts[3]==1&&game.Craft.FittedParts[0].Count==1,"Mutually exclusive muzzle slot returns replaced part");MigrationChecks.Assert(!game.FitPart(4)&&game.Craft.SpareParts[4]==0,"Cannot duplicate installed parts");
  game.Equip(28);MigrationChecks.Assert(!game.FitPart(3)&&!WeaponPart.All[6].Fits(28)&&ExpeditionGame.AmmoClassFor(28)==0,"Revolver compatibility and ammunition");
  game.Equip(32);Make(8);double speed=game.SoulArrowSpeed,draw=game.SoulBowDelay;game.FitPart(8);MigrationChecks.Assert(game.SoulArrowSpeed>speed&&game.SoulBowDelay>draw,"Bowstring tradeoff affects real projectile speed and draw cycle");
  game.Equip(33);Make(0);Make(1);double cost=game.MeleeCost,damage=game.Damage(false);game.FitPart(0);MigrationChecks.Assert(game.MeleeCost<cost,"Grip reduces actual stamina cost");game.FitPart(1);MigrationChecks.Assert(game.Damage(false)>damage&&game.SoulMeleeDelay>ExpeditionGame.WeaponDelay(33),"Weighted head affects actual damage and timing");
  MigrationChecks.Assert(PackItems().Any(i=>i.Kind=="weapon-mod"&&i.Index==3&&i.Count==1),"Spare parts appear as real inventory entries");
  var state=Json.Read<ExpeditionGameSave>(Json.Write(game.Save));MigrationChecks.Assert(state.Profile.Crafting.FittedParts[33].Count==2&&state.Profile.Crafting.SpareParts[3]==1,"Portable saved parts and inventory");
  game.Life.WeaponQuality=new int[28];game.Craft.Magazines=new int[28];game.Normalize();MigrationChecks.Assert(game.Life.WeaponQuality.Length==34&&game.Craft.Magazines.Length==34,"Old saves expand without losing weapons");
  MigrationChecks.Assert(ExpeditionRecipe.All.Where(r=>r.Kind=="weapon-mod").Sum(game.ResearchCost)>0&&ExpeditionRecipe.All.Sum(game.ResearchCost)>ExpeditionGame.ResearchBudget,"Finite research cannot buy every branch");
  GD.Print("PASS / weapon workshop: acyclic dependencies, specialization, six craftable weapon models, twelve compatible parts, real stats, slots, ammo conservation, duplicate rejection and portable old-save migration");
 }finally{game=original;}}
 async void RunWeaponWorkshopChecks(){try{WeaponWorkshopChecks();await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);GetTree().Quit();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
 async void CaptureWeaponWorkshop(){try{
  game.Craft.BenchTier=game.Craft.UnlockedTier=4;game.Career.Level=5;game.Skills.Crafting.Level=4;game.Craft.ResearchPoints=20;game.Craft.Materials=Enumerable.Repeat(30,7).ToArray();
  foreach(int id in new[]{0,2,3,29})game.Craft.Researched.Add(ExpeditionGame.ResearchId(ExpeditionRecipe.All.First(r=>r.Kind=="weapon"&&r.Result==id)));
  game.Craft.ResearchFocus["Weapons and armor"]="Ballistics";researchTab="Weapons and armor";researchFamilyFilter="Ballistics";researchChoice=Array.FindIndex(ExpeditionRecipe.All,r=>r.Kind=="weapon-mod"&&r.Result==3);ShowResearchTree();await ArtShot("workshop-01-research");
  game.Craft.Weapons.Add(29);game.Equip(29);game.Craft.SpareParts[3]=game.Craft.SpareParts[4]=game.Craft.SpareParts[6]=1;game.FitPart(3);ShowWeaponParts();await ArtShot("workshop-02-parts");CloseMenu();GetTree().Quit();
 }catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}
