using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
public partial class MigrationScene {
 bool workshopBudgetReady;
 [Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
 void CoopCheckWorkshopBudget(){
  if(!coopCheck||!CoopHost||!coopActors.TryGetValue(Multiplayer.GetRemoteSenderId(),out var actor))return;
  // Start with no burst allowance so the test must respect normal replenishment.
  actor.ActionTokens=0;actor.LastAction=Time.GetTicksMsec()/1000.0;
  RpcId(actor.Id,nameof(CoopCheckWorkshopBudgetReady));
 }
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
 void CoopCheckWorkshopBudgetReady(){if(coopCheck&&CoopClient)workshopBudgetReady=true;}
 async Task ClientWeaponPartChecks(){
  // The production host permits 20 actions/second with an eight-action burst.
  // Fast CI round trips can exhaust that burst before the final equip request.
  // Pace against the same monotonic clock as the host limiter. A scene timer\n  // can count the frame in which it is created, shortening a 75ms delay.\n  // Do not retry mutations or bypass the real rate limiter.
  async Task Send(string operation,string data){ulong ready=Time.GetTicksMsec()+75;while(Time.GetTicksMsec()<ready)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);CoopRequest(operation,data);}
  workshopBudgetReady=false;RpcId(1,nameof(CoopCheckWorkshopBudget));
  await CoopWait(()=>workshopBudgetReady,"Workshop rate-budget fixture acknowledgement missing");

  foreach(var recipe in new[]{ExpeditionRecipe.All.First(r=>r.Kind=="weapon"&&r.Result==10),ExpeditionRecipe.All.First(r=>r.Kind=="weapon-mod"&&r.Result==0)}){
   int index=Array.IndexOf(ExpeditionRecipe.All,recipe);await Send("research",index.ToString());try{await CoopWait(()=>game.Knows(recipe),"Weapon workshop research did not replicate");}catch(Exception ex){throw new Exception($"Weapon workshop research failed / {recipe.Name} / {game.ResearchReason(recipe)} / {game.Craft.ResearchPoints} RP / host notice: {game.Notice}",ex);}await Send("craft",index.ToString());await CoopWait(()=>recipe.Kind=="weapon"?game.Craft.Weapons.Contains(10):game.Craft.SpareParts.GetValueOrDefault(0)==1,"Weapon workshop crafting did not replicate");
  }
  await Send("equip","10");await CoopWait(()=>game.Craft.Weapon==10,"Workshop equip missing");double stamina=game.MeleeCost;
  await Send("weaponpart","0|fit");await CoopWait(()=>game.Craft.FittedParts.TryGetValue(10,out var list)&&list.Contains(0),"Guest weapon part was not fitted by host");
  if(game.Craft.SpareParts[0]!=0||game.MeleeCost>=stamina)throw new Exception("Fitted part cost or effect missing");
  await Send("weaponpart","0|fit");await CoopWait(()=>game.Notice=="Already fitted","Duplicate part action was not rejected");
  await Send("weaponpart","0|remove");await CoopWait(()=>game.Craft.SpareParts[0]==1&&game.Craft.FittedParts[10].Count==0,"Removed part not returned");
  await Send("weaponpart","0|fit");await CoopWait(()=>game.Craft.FittedParts[10].Contains(0),"Part could not be refitted");
  await Send("equip","1");
  try{await CoopWait(()=>game.Craft.Weapon==1,"Combat weapon restore missing");}
  catch(Exception ex){throw new Exception($"Combat weapon restore missing / equipped {game.Craft.Weapon}, owns rifle {game.Craft.Weapons.Contains(1)}, guns unlocked {game.GunsUnlocked}, host notice: {game.Notice}",ex);}
  GD.Print("PASS / two-process weapon workshop: real research/crafting, private fitted parts, stamina effect, duplicate rejection and remove/refit conservation");
 }
}
