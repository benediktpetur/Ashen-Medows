using System;
using System.Linq;
using System.Collections.Generic;
sealed partial class ExpeditionGame {
 public static string ResearchId(ExpeditionRecipe r)=>r.Kind+"/"+r.Tier+"/"+r.Result;
 public bool ResearchComplete(ExpeditionRecipe r)=>r.Kind switch{
  "survival-tech"=>Life.SurvivalTech.Contains(r.Result),"bench"=>Craft.BenchTier>=r.Result,
  "weapon"=>Craft.Weapons.Contains(r.Result),
  "weapon-mod"=>Craft.SpareParts.GetValueOrDefault(r.Result)>0||Craft.FittedParts.Values.Any(parts=>parts.Contains(r.Result)),
  "armor"=>Profile.Items.Any(i=>i.Level>=r.Tier&&i.GearSlot==(r.Result==11?"head":r.Result==12?"hands":r.Result==13?"feet":"chest")),
  "mill"=>Life.WeaponQuality[Craft.Weapon]>=20,
  "attachment"=>Life.WeaponQuality[Craft.Weapon]>=r.Result,
  "build"=>Building!=null&&Building.Pieces.Any(p=>p.Kind==r.Result),
  "night"=>Life.WeaponQuality[Craft.Weapon]>=25,
  "legendary"=>Life.WeaponQuality[Craft.Weapon]>=50,
  _=>false
 };
 public string ResearchAccess(ExpeditionRecipe r){if(r.Kind=="weapon"&&WeaponGate(r.Result)!="Ready")return WeaponGate(r.Result);if(Craft.UnlockedTier<r.Tier)return "Explore tier "+r.Tier+" for materials";if(Craft.BenchTier<r.Station)return "Requires workshop "+r.Station;if(r.Kind=="weapon"&&r.Tier>=3&&Skills.Crafting.Level<r.Tier-1)return "Requires Crafting "+(r.Tier-1);if(r.Kind=="night"&&Skills.Crafting.Level<2)return "Requires Crafting 2";return "Unlocked";}
 public void TrackResearch(int index){if(index<0||index>=ExpeditionRecipe.All.Length)return;Change(()=>Craft.ResearchTarget=ResearchId(ExpeditionRecipe.All[index]));Notice="Tracking: "+ExpeditionRecipe.All[index].Name;}
 public ExpeditionRecipe TrackedResearch=>ExpeditionRecipe.All.FirstOrDefault(r=>ResearchId(r)==Craft.ResearchTarget);
}
