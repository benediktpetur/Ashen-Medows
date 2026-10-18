using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

sealed partial class ExpeditionLife {
 [DataMember] public int ConditionVersion=1,RawWater,Drinks;
 [DataMember] public double Morale=60,Sickness,Trauma,Comfort;
 [DataMember] public List<int> SurvivalTech=new();
 public void NormalizeConditions(){SurvivalTech??=new();if(ConditionVersion==0){Morale=60;ConditionVersion=1;}Morale=Math.Clamp(Morale,0,100);RawWater=Math.Max(0,RawWater);Sickness=Math.Max(0,Sickness);Trauma=Math.Max(0,Trauma);}
 public double BleedRate=>Math.Min(.36,Wounds.Count(w=>!w.Patched)*.075);
 public void TickSoftWounds(double dt){foreach(var w in Wounds)if(w.Patched)w.Recovery-=dt;Wounds.RemoveAll(w=>w.Patched&&w.Recovery<=0);Health-=BleedRate*dt;}
}
sealed partial class ExpeditionGame {
 public double RecoveryCeiling=>MaxHealth*Math.Clamp(1-(Life.Trauma>0?.15:0)-(Life.Sickness>0?.10:0)-Math.Min(.15,Life.Wounds.Count*.025),.55,1);
 public double ConditionSpeed=>(Life.Sickness>0?.82:1)*(Life.Energy<20?.85:1)*(Life.Morale<20?.92:1);
 public string Conditions=>string.Join(" / ",new[]{Life.Sickness>0?"SICK / stamina recovery -25%, recovery slower":null,Life.Energy<20?"EXHAUSTED / speed -15%":null,Life.Hunger<25?"HUNGRY / recovery blocked":null,Life.Hydration<25?"THIRSTY / recovery blocked":null,Life.Morale<20?"DESPONDENT / speed -8%":null,Life.Morale>75?"HOPEFUL / stamina recovery +15%":null,Life.Comfort>0?"SHELTERED / recovery +40%":null,Life.Trauma>0?"DEATH TRAUMA / recovery ceiling -15%":null,Life.Hunger>75&&Life.Hydration>75?"WELL FED / stamina recovery +10%":null}.Where(s=>s!=null));
 public bool TickConditions(double dt){
  if(Life.SurvivalTech.Contains(2)&&Life.Camps.Any(c=>c.Seed==Save.Seed&&Distance(c.X,c.Z,Save.X,Save.Z)<8))Life.Comfort=Math.Max(Life.Comfort,300);Life.Sickness=Math.Max(0,Life.Sickness-dt);Life.Trauma=Math.Max(0,Life.Trauma-dt);Life.Comfort=Math.Max(0,Life.Comfort-dt);
  Life.Morale=Math.Clamp(Life.Morale+dt*(Life.Comfort>0?.012:Night?-.002:Life.Hunger>60?.001:0),0,100);
  Life.TickSoftWounds(dt);if(Life.Hunger<=0)Life.Health-=dt*.025;if(Life.Hydration<=0)Life.Health-=dt*.04;
  if(Clock-LastDamage>30&&Life.Hunger>25&&Life.Hydration>25&&Life.BleedRate==0&&Life.Health<RecoveryCeiling)Life.Health=Math.Min(RecoveryCeiling,Life.Health+dt*.08*(1+Math.Min(20,Skills.Survival.Level-1)*.02)*(1+Talent("recovery")*.15)*(Life.Sickness>0?.4:1)*(Life.Comfort>0?1.4:1)*(Life.RecoveryBoost>0?2:1)*MedicalRecovery);
  return Life.Health<=0;
 }
 public bool Drink(bool untreated=false){
  if(untreated){if(Life.RawWater<1){Notice="No untreated water";return false;}Life.RawWater--;Life.Drinks++;bool ill=World.Hash(Life.Drinks,Skills.Deaths+989)%100<35;if(ill){Life.Sickness=Math.Max(Life.Sickness,600);Life.Morale=Math.Max(0,Life.Morale-5);}Notice=ill?"Untreated water / stomach illness for 10 active minutes":"Drank untreated water / you avoided illness this time";}
  else{if(Craft.Water<1){Notice="No clean water / boil or filter untreated water in Y";return false;}Craft.Water--;Notice="Drank clean water";}
  Life.Hydration=Math.Min(100,Life.Hydration+45);return true;
 }
 public bool TreatWater(bool filter){bool membrane=Life.SurvivalTech.Contains(1);int count=Math.Min(Life.SurvivalTech.Contains(5)?4:2,Life.RawWater);if(count==0){Notice="Collect untreated water at a well first";return false;}if(filter){if(membrane?Craft.Fuel<1:!Life.SurvivalTech.Contains(0)||Craft.Materials[3]<1){Notice=membrane?"Membrane purification needs 1 fuel":"Needs a crafted charcoal filter and 1 forest resin";return false;}}else if(Craft.BenchTier<1||Craft.Materials[0]<2){Notice="Boiling needs a workbench and 2 timber";return false;}
  return Change(()=>{if(filter){if(membrane)Craft.Fuel--;else Craft.Materials[3]--;}else Craft.Materials[0]-=2;Life.RawWater-=count;Craft.Water+=count;Notice="Purified "+count+" water / no illness risk";});
 }
 public void Rested(){Life.Energy=100;Life.Comfort=300;Life.Morale=Math.Min(100,Life.Morale+8);Notice="Rested / sheltered recovery for 5 minutes";}
}
