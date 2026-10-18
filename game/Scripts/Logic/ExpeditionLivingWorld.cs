using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
[DataContract] sealed partial class SurvivorSettlement {
 [DataMember] public int Index,Trust,Food=8,Water=8,Visits,Thefts,Crisis;
 [DataMember] public bool WaterFixed,ClinicFixed,Fallen;
 [DataMember] public double X,Z,NextNeed,ServiceAt;
 [DataMember] public string LastEvent="A community trying to survive, not a quest dispenser.";
}
partial class ExpeditionWorld {
 public bool LivingLot(double x,double z){if(Changes?.Settlements!=null&&Changes.Settlements.Any(s=>s.QuestZ!=0&&Math.Abs(x-s.QuestX)<16&&Math.Abs(z-s.QuestZ)<16))return true;for(int i=0;i<4;i++){double tz=new[]{-790d,-1460d,-2050d,-2290d}[i],tx=RouteX(tz)+(i==1?-105:130);var town=Changes?.Settlements?.FirstOrDefault(s=>s.Index==i);if(town!=null){tx=town.X;tz=town.Z;}if(Math.Abs(x-tx)<30&&Math.Abs(z-tz)<30)return true;}return false;}
}
sealed partial class ExpeditionWorldChanges {
 [DataMember] public List<SurvivorSettlement> Settlements=new();
 [DataMember] public List<string> Explored=new(),Consequences=new();
}
sealed partial class ExpeditionGame {
 public static readonly string[] SettlementNames={"Emberstead","Switchyard","Mercy Row","Black Ledger"};
 public void LivingNormalize(){World.Changes.FieldSites??=new();World.Changes.FieldEvent??=new();World.Changes.Ledger??=new();World.Changes.Dog??=new();World.Changes.Relationships??=new();if(World.Changes.FeverStory.ArcVersion==0&&World.Changes.FeverStory.Stage<4)World.Changes.FeverStory.ArcVersion=1;World.Changes.Settlements??=new();World.Changes.Explored??=new();World.Changes.Consequences??=new();for(int i=0;i<4;i++)if(!World.Changes.Settlements.Any(s=>s.Index==i)){double z=new[]{-790d,-1460d,-2050d,-2290d}[i];World.Changes.Settlements.Add(new(){Index=i,X=World.RouteX(z)+(i==1?-105:130),Z=z,NextNeed=World.Changes.Time+900});}}
 public void WorldDecision(string key,string text){if(World.Changes.Consequences.Contains(key))return;World.Changes.Consequences.Add(key);Profile.Journal.Seen.Add("world/"+key+" / "+text);}
 public void TickLivingWorld(){
  var f=World.Changes.FeverStory;
  foreach(var town in World.Changes.Settlements){if(town.Fallen)continue;if(town.Visits==0){town.NextNeed=World.Changes.Time+900;continue;}if(World.Changes.Time>=town.NextNeed){town.NextNeed=World.Changes.Time+900;town.Food=Math.Max(0,town.Food-1);town.Water=Math.Max(0,town.Water-(town.WaterFixed?0:2));town.Crisis=town.Water==0||town.Food==0?town.Crisis+1:0;if(town.Crisis>=3){town.Fallen=true;town.LastEvent="The stores ran dry. The residents evacuated. Their ledger records who brought supplies and who took them.";continue;}town.LastEvent=town.Water<3?"Rationing water. The medic needs a working filter.":town.Food<3?"The cook has started thinning the soup.":"Supplies stretch another day. You can hear people working.";}}
  if(f.ArcVersion>0&&f.Choice is "open" or "truth" or "reinforce"){
   if(f.FledAt>0&&World.Changes.Time>=f.FledAt+300)f.GraveReady=true;
   if(f.FledAt>0&&!f.CitySeen&&!f.Redemption&&World.Changes.Time>=f.FledAt+21600){f.Offscreen=true;var town=World.Changes.Settlements.First(s=>s.Index==2);town.Fallen=true;town.LastEvent="The clinic ran out of medical supplies. An evacuation trail ends in the woods. Noah's last note is under the cot.";}
  }
 }
 public string TownAction(int index,string op){
  var t=World.Changes.Settlements.FirstOrDefault(s=>s.Index==index);if(t==null||Distance(Save.X,Save.Z,t.X,t.Z)>12)return Notice="Too far from the community";
  if(t.Fallen)return Notice="An abandoned camp / read the final ledger";
  if(op=="food"){if(Life.Food<3)return Notice="Needs 3 provisions";return Change(()=>{Life.Food-=3;t.Food+=3;t.Trust+=4;Life.Morale=Math.Min(100,Life.Morale+3);t.LastEvent="Your food is cooking on the fire. Someone has stopped crying.";WorldDecision("town/food/"+index,"Shared food with "+SettlementNames[index]);})?"Food shared":"Save failed";}
  if(op=="water"){if(Craft.Water<4)return Notice="Needs 4 clean water";return Change(()=>{Craft.Water-=4;t.Water+=4;t.Trust+=5;t.LastEvent="Clean water is being rationed to the sick.";WorldDecision("town/water/"+index,"Delivered clean water to "+SettlementNames[index]);})?"Water shared":"Save failed";}
  if(op=="repair"){if(index==2&&World.Changes.FeverStory.CitySeen&&!World.Changes.FeverStory.Redemption)return Notice="The clinic needs full restoration / a filter alone cannot treat the patients";if(t.WaterFixed)return Notice="Their filter is already repaired";if(Craft.Materials[4]<8||Craft.Materials[5]<2)return Notice="Needs 8 alloy and 2 electronics";return Change(()=>{Craft.Materials[4]-=8;Craft.Materials[5]-=2;t.WaterFixed=true;t.Water+=8;t.Trust+=12;t.LastEvent="The communal filter runs. Water no longer drains from their stores.";WorldDecision("town/filter/"+index,"Repaired the communal filter at "+SettlementNames[index]);})?"A lasting water supply / community stabilized":"Save failed";}
  if(op=="steal"){if(t.Food<2)return Notice="There is nothing left to steal";return Change(()=>{t.Food-=2;Life.Food+=2;t.Trust-=18;t.Thefts++;Life.Morale=Math.Max(0,Life.Morale-8);t.LastEvent="Missing rations. The children eat last tonight. The guards remember you.";if(index==2&&World.Changes.FeverStory.CitySeen){World.Changes.FeverStory.Violence=true;World.Changes.FeverStory.RedemptionClosed=true;}WorldDecision("town/theft/"+index,"Stole rations from "+SettlementNames[index]);})?"Stolen food / services closed if trust falls below zero":"Save failed";}
  if(op=="service"){if(t.Trust<0)return Notice="They refuse service / rebuild trust with supplies";if(index==2&&World.Changes.FeverStory.CitySeen&&!World.Changes.FeverStory.Redemption)return Notice="Mercy Row is split / only the clinic restoration can reopen services";if(World.Changes.Time<t.ServiceAt)return Notice="Supplies are being replenished / try tomorrow";if(t.Food<2||t.Water<2)return Notice="They need food and water before they can help you";
   return Change(()=>{t.ServiceAt=World.Changes.Time+900;t.Food--;t.Water--;if(index==0){Life.Food+=2;Craft.Water+=1;}if(index==1){Craft.Materials[4]+=3;Life.Bandages++;}if(index==2){Life.Sickness=0;Life.RecoveryBoost=300;}Life.Morale=Math.Min(100,Life.Morale+5);})?"Community service received / their supplies were consumed":"Save failed";
  }
  return Notice="Unknown community action";
 }
}
