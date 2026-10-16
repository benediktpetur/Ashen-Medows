using System;
using System.Linq;
sealed partial class ExpeditionGame {
 public double MedicalRecovery=>Building==null?1:Building.Pieces.Where(p=>p.Def.Function=="medical"&&Distance(p.X,p.Z,Save.X,Save.Z)<3&&Building.Pieces.Any(g=>g.Def.Function=="power"&&g.Charge>0&&Distance(g.X,g.Z,p.X,p.Z)<24)).Select(p=>p.Def.Tier==5?3.0:2.0).DefaultIfEmpty(1).Max();
 public string ResearchTab(ExpeditionRecipe r)=>r.Kind=="build"?BuildDefinition.All[r.Result].Tab:r.Kind is "weapon" or "armor" or "night" or "attachment"?"Weapons and armor":r.Kind=="bench"?"Buildings":"Survival";
 public bool FreeBlueprint(ExpeditionRecipe r)=>r.Kind=="bench"||r.Kind=="build"&&r.Result<8||r.Kind is "bandage" or "food" or "ammo" or "arrow";
 public int ResearchCost(ExpeditionRecipe r)=>FreeBlueprint(r)?0:r.Tier;
 public bool Knows(ExpeditionRecipe r)=>FreeBlueprint(r)||Craft.Researched.Contains(ResearchId(r));
 public void ResearchNormalize(){if(Craft.Researched==null)Craft.Researched=new();if(Craft.ResearchAwards==null)Craft.ResearchAwards=new();Craft.ResearchPoints=Math.Max(0,Craft.ResearchPoints);if(Craft.ResearchVersion==0){foreach(var r in ExpeditionRecipe.All)if(r.Tier<=Craft.BenchTier||ResearchComplete(r))Craft.Researched.Add(ResearchId(r));Craft.ResearchVersion=1;}foreach(int tier in Enumerable.Range(1,Math.Max(1,Craft.UnlockedTier)))AwardResearch("zone/"+tier,10+tier*5);}
 public void AwardResearch(string id,int points){if(Craft.ResearchAwards.Contains(id))return;Craft.ResearchAwards.Add(id);Craft.ResearchPoints+=points;Career.Gain(points*4);}
 public string ResearchReason(ExpeditionRecipe r){if(Knows(r))return "Researched";string access=ResearchAccess(r);if(access!="Unlocked")return access;return Craft.ResearchPoints<ResearchCost(r)?"Needs "+ResearchCost(r)+" research points":"Ready";}
 public bool Research(int index){if(index<0||index>=ExpeditionRecipe.All.Length)return false;var r=ExpeditionRecipe.All[index];if(ResearchReason(r)!="Ready"){Notice=ResearchReason(r);return false;}bool ok=Change(()=>{Craft.ResearchPoints-=ResearchCost(r);Craft.Researched.Add(ResearchId(r));});if(ok)Notice="Researched "+r.Name;return ok;}
 public string BuildReason(BuildPiece p){var r=ExpeditionRecipe.All.FirstOrDefault(r=>r.Kind=="build"&&r.Result==p.Kind);if(r==null)return "Unknown blueprint";if(!Knows(r))return "Research this blueprint first";return RecipeReason(r);}
 public bool PlaceBlueprint(BuildPiece p){var support=Building.Placement(p);if(!support.Valid){Notice=support.Reason;return false;}string reason=BuildReason(p);if(reason!="Ready"){Notice=reason;return false;}return Change(()=>{for(int i=0;i<7;i++)Craft.Materials[i]-=p.Def.Cost[i];p.Id=Guid.NewGuid().ToString("N");p.Health=p.Def.Durability;p.Open=false;p.Charge=0;p.Progress=0;p.Stock=new int[11];Building.Pieces.Add(p);Skills.Crafting.Gain(p.Def.Tier*3);Career.Gain(p.Def.Tier*3);AwardResearch("milestone/first-shelter",8);});}
}
