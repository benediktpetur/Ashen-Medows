using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
sealed partial class ExpeditionCraftState {
 [DataMember] public int ResearchEarned,ChoiceVersion=1;
 [DataMember] public Dictionary<string,string> ResearchFocus=new();
}
sealed partial class ExpeditionGame {
 public const int ResearchBudget=48;
 public string ResearchFamily(ExpeditionRecipe r){
  if(r.Kind=="legendary"||r.Kind=="night")return IsBow(Craft.Weapon)?"Ranger":IsMelee(Craft.Weapon)?"Brawler":"Ballistics";string tab=ResearchTab(r);
  if(tab=="Weapons and armor")return r.Kind=="weapon"?IsBow(r.Result)?"Ranger":IsMelee(r.Result)?"Brawler":"Ballistics":r.Kind=="armor"?"Brawler":"Ballistics";
  if(tab=="Defenses")return r.Kind=="build"&&BuildDefinition.All[r.Result].Function is "turret" or "electric" or "electrictrap"?"Sentinel":r.Kind=="build"&&BuildDefinition.All[r.Result].Function is "trap" or "lure" or "alarm"?"Trapper":"Fortifier";
  if(tab=="Buildings")return r.Kind=="build"&&BuildDefinition.All[r.Result].Function is "medical" or "bed"?"Sanctuary":r.Kind=="build"&&BuildDefinition.All[r.Result].Function is "power" or "mobile" or "vehicle" or "repair"?"Engineer":"Homesteader";
  if(r.Kind=="build"){string function=BuildDefinition.All[r.Result].Function;if(function=="water")return "Waterworks";if(function=="medical")return "Herbalist";}return r.Kind=="survival-tech"?r.Result is 0 or 1 or 5?"Waterworks":r.Result==3?"Herbalist":"Pathfinder":r.Kind=="medicine"?"Herbalist":"Pathfinder";
 }
 public string FocusReason(ExpeditionRecipe r){if(r.Tier<3||FreeBlueprint(r))return "Ready";string tab=ResearchTab(r),family=ResearchFamily(r);if(Craft.ResearchFocus.TryGetValue(tab,out var chosen)&&chosen!=family)return "SPECIALIZATION LOCK / "+chosen+" chosen; "+family+" unavailable";if(r.Tier>=4&&!ExpeditionRecipe.All.Any(p=>p.Tier>=3&&p.Tier<r.Tier&&ResearchTab(p)==tab&&ResearchFamily(p)==family&&Craft.Researched.Contains(ResearchId(p))))return "Requires an earlier advanced "+family+" blueprint";return "Ready";}
 public string FocusWarning(ExpeditionRecipe r)=>r.Tier>=3&&!FreeBlueprint(r)?"T3+ specialization: "+ResearchFamily(r)+" / first purchase permanently locks the other advanced paths in "+ResearchTab(r)+". Existing unlocks are retained.":"Foundational research / does not lock a specialization";
 public void NormalizeResearchChoices(){Craft.ResearchFocus??=new();if(Craft.ChoiceVersion==0){Craft.ResearchEarned=Math.Min(ResearchBudget,Craft.ResearchPoints+ExpeditionRecipe.All.Where(r=>Craft.Researched.Contains(ResearchId(r))).Sum(ResearchCost));Craft.ChoiceVersion=1;}Craft.ResearchEarned=Math.Clamp(Craft.ResearchEarned,0,ResearchBudget);}
 public static IEnumerable<ExpeditionRecipe> ConditionRecipes(){
  yield return new("Charcoal water filter","survival-tech",2,2,0,3,2,6,3,0,0,0);
  yield return new("Membrane purifier kit","survival-tech",4,4,1,0,2,3,2,6,4,0);
  yield return new("Camp comfort kit","survival-tech",3,3,2,6,0,6,2,2,0,0);
  yield return new("Antibiotic field procedure","survival-tech",3,3,3,0,0,8,5,0,2,0);
  yield return new("Trail recovery tea","survival-tech",3,3,4,0,0,6,3,0,0,0);
  yield return new("Sealed water containers","survival-tech",3,3,5,0,2,4,2,4,0,0);
 }
}
