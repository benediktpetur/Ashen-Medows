using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
public partial class MigrationScene {
 async Task ClientWeaponPartChecks(){
  foreach(var recipe in new[]{ExpeditionRecipe.All.First(r=>r.Kind=="weapon"&&r.Result==10),ExpeditionRecipe.All.First(r=>r.Kind=="weapon-mod"&&r.Result==0)}){
   int index=Array.IndexOf(ExpeditionRecipe.All,recipe);CoopRequest("research",index.ToString());await CoopWait(()=>game.Knows(recipe),"Weapon workshop research did not replicate");CoopRequest("craft",index.ToString());await CoopWait(()=>recipe.Kind=="weapon"?game.Craft.Weapons.Contains(10):game.Craft.SpareParts.GetValueOrDefault(0)==1,"Weapon workshop crafting did not replicate");
  }
  CoopRequest("equip","10");await CoopWait(()=>game.Craft.Weapon==10,"Workshop equip missing");double stamina=game.MeleeCost;
  CoopRequest("weaponpart","0|fit");await CoopWait(()=>game.Craft.FittedParts.TryGetValue(10,out var list)&&list.Contains(0),"Guest weapon part was not fitted by host");
  if(game.Craft.SpareParts[0]!=0||game.MeleeCost>=stamina)throw new Exception("Fitted part cost or effect missing");
  CoopRequest("weaponpart","0|fit");await CoopWait(()=>game.Notice=="Already fitted","Duplicate part action was not rejected");
  CoopRequest("weaponpart","0|remove");await CoopWait(()=>game.Craft.SpareParts[0]==1&&game.Craft.FittedParts[10].Count==0,"Removed part not returned");
  CoopRequest("weaponpart","0|fit");await CoopWait(()=>game.Craft.FittedParts[10].Contains(0),"Part could not be refitted");
  CoopRequest("equip","1");await CoopWait(()=>game.Craft.Weapon==1,"Combat weapon restore missing");
  GD.Print("PASS / two-process weapon workshop: real research/crafting, private fitted parts, stamina effect, duplicate rejection and remove/refit conservation");
 }
}
