using System;
using System.Linq;
sealed partial class ExpeditionGame {
 public void RememberEncounter(int index){
  string key="encounter/"+index;if(World.Changes.Consequences.Contains(key))return;var e=ExpeditionEncounter.All[index];
  WorldDecision(key,e.Title+" / "+e.After);
  Life.Morale=Math.Clamp(Life.Morale+(e.Reward is 0 or 2?4:e.Reward==3?-2:1),0,100);
  if(e.Reward is 0 or 2){var town=World.Changes.Settlements.Where(t=>t.Visits>0&&!t.Fallen&&Distance(t.X,t.Z,Save.X,Save.Z)<350).OrderBy(t=>Distance(t.X,t.Z,Save.X,Save.Z)).FirstOrDefault();if(town!=null){town.Trust+=2;town.LastEvent="Word of your help reached "+SettlementNames[town.Index]+". They remembered the supplies you left behind.";}}
 }
}
