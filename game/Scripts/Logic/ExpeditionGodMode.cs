using System;
using System.Collections.Generic;

sealed partial class ExpeditionGame {
 public bool GodMode;
 public bool DamageProtected=>GodMode||Shield>0;

 public void GrantGodModeItems(){
  const int supplies=100;
  static void Stock(int[] counts,int minimum){
   for(int i=0;i<counts.Length;i++)counts[i]=Math.Max(counts[i],minimum);
  }

  Craft.UnlockedTier=Craft.BenchTier=5;
  if(Career.Level<2)Career.Gain(Career.Need);
  for(int id=0;id<GunNames.Length;id++)
   if(!Craft.Weapons.Contains(id))Craft.Weapons.Add(id);
  Stock(Craft.Materials,supplies);
  Stock(Craft.HuntStock,supplies);
  Stock(Craft.ThrowStock,supplies);
  for(int type=0;type<ArrowNames.Length;type++)
   Craft.AddArrows(type,Math.Max(0,supplies-Craft.ArrowsOf(type)));
  for(int type=0;type<AmmoNames.Length;type++)
   Craft.GiveAmmo(type,Math.Max(0,supplies-Craft.Stock(type)));

  Craft.HasAxe=Craft.SkinningKnife=Craft.HarvestCleaver=true;
  for(int i=0;i<Craft.ToolLevels.Length;i++)Craft.ToolLevels[i]=i<2?2:3;
  Craft.ScrapMetal=Math.Max(Craft.ScrapMetal,supplies);
  Craft.Glass=Math.Max(Craft.Glass,supplies);
  Craft.Fuel=Math.Max(Craft.Fuel,supplies);
  Craft.Water=Math.Max(Craft.Water,supplies);
  Life.RawWater=Math.Max(Life.RawWater,supplies);
  Life.Food=Math.Max(Life.Food,supplies);
  Life.Bandages=Math.Max(Life.Bandages,supplies);
  Life.Bags=Math.Max(Life.Bags,supplies);
  Life.Medicine=Math.Max(Life.Medicine,supplies);
  Life.NightEssence=Math.Max(Life.NightEssence,supplies);
  Life.MillCores=Math.Max(Life.MillCores,supplies);
  Life.LegendaryCores=Math.Max(Life.LegendaryCores,supplies);
  Life.ArmorMods=Math.Max(Life.ArmorMods,supplies);
  Life.ArmorReinforcements=Math.Max(Life.ArmorReinforcements,supplies);
  Life.CommunityParts=Math.Max(Life.CommunityParts,supplies);

  foreach(var recipe in ExpeditionRecipe.All){
   string id=ResearchId(recipe);
   if(!Craft.Researched.Contains(id))Craft.Researched.Add(id);
   if(recipe.Kind=="weapon-mod")
    Craft.SpareParts[recipe.Result]=Math.Max(Craft.SpareParts.GetValueOrDefault(recipe.Result),20);
   if(recipe.Kind=="survival-tech"&&!Life.SurvivalTech.Contains(recipe.Result))
    Life.SurvivalTech.Add(recipe.Result);
   if(recipe.Kind=="armor"&&!Profile.Items.Exists(item=>item.Name==recipe.Name))
    Profile.Items.Add(WorkshopArmor(recipe,"godmode/"+id));
  }
  foreach(var recipe in HuntingRecipe.All){
   string slot=recipe.Product switch {"hood"=>"head","boots"=>"feet","vest"=>"chest",_=>null};
   if(slot==null||Profile.Items.Exists(item=>item.Name==recipe.Name))continue;
   Profile.Items.Add(new ExpeditionRelic{
    Id="godmode/hunting/"+recipe.Product,Name=recipe.Name,Origin="Handcrafted from hunted wildlife",
    Slot=slot,Level=recipe.Tier,Armor=slot=="chest"?14:slot=="feet"?6:3,
    Vitality=slot=="chest"?3:1,Warmth=slot=="chest"?.20:slot=="head"?.10:.05
   });
  }
  GrantGodModeRelics();
  RestoreGodModeVitals();
 }

 void GrantGodModeRelics(){
  string owner=Life.SoulOwner;
  for(int weapon=0;weapon<GunNames.Length;weapon++){
   string id="godmode/soul/"+owner+"/"+weapon;
   if(Life.Souls.Exists(item=>item.Id==id))continue;
   var item=SoulboundWeapon.Roll(owner,weapon,"God mode",weapon);
   item.Id=id;
   Life.Souls.Add(item);
  }
  foreach(string slot in new[]{"head","chest","hands","feet"}){
   string id="godmode/soul-armor/"+owner+"/"+slot;
   if(Profile.Items.Exists(item=>item.Id==id))continue;
   Profile.Items.Add(new ExpeditionRelic{Id=id,SoulOwner=owner,Name="Soulbound "+slot+" armor",
    Origin="God mode",Slot=slot,Level=5,Armor=slot=="chest"?50:25,Vitality=10});
  }
  for(int kind=0;kind<SoulUpgrade.Names.Length;kind++){
   string id="godmode/upgrade/"+owner+"/"+kind;
   if(!Life.SoulUpgrades.Exists(item=>item.Id==id))
    Life.SoulUpgrades.Add(new SoulUpgrade{Id=id,Owner=owner,Kind=kind,Power=25});
  }
  for(int kind=0;kind<SoulModule.Names.Length;kind++){
   string id="godmode/module/"+owner+"/"+kind;
   if(!Life.SoulModules.Exists(item=>item.Id==id))
    Life.SoulModules.Add(new SoulModule{Id=id,Owner=owner,Kind=kind,Power=30,Upkeep=1});
  }
  string harnessId="godmode/harness/"+owner;
  if(!Life.SoulCompanions.Exists(item=>item.Id==harnessId))
   Life.SoulCompanions.Add(new SoulCompanionGear{Id=harnessId,Owner=owner,Damage=25,Speed=20});
 }

 void RestoreGodModeVitals(){
  Life.Health=MaxHealth;
  Life.Hunger=Life.Hydration=Life.Energy=100;
  Life.Wounds.Clear();
  Life.Sickness=Life.Trauma=0;
  Stamina=MaxStamina;
 }
}
