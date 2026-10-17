using System;
using System.Collections.Generic;
sealed partial class ExpeditionGame {
 public static bool IsGun(int id)=>id>=0&&id<=5||id==7;
 public static bool IsBow(int id)=>id==6||id==12||id==18||id==19;
 public static bool IsMelee(int id)=>id>=8&&!IsBow(id);
 public bool GunsUnlocked=>Career.Level>=2&&Craft.UnlockedTier>=2;
 public ExpeditionSkill ProficiencyFor(int id)=>IsMelee(id)?Skills.Melee:IsBow(id)?Skills.Archery:Skills.Marksmanship;
 public ExpeditionSkill WeaponProficiency=>ProficiencyFor(Craft.Weapon);
 public string WeaponClass=>IsMelee(Craft.Weapon)?"MELEE":IsBow(Craft.Weapon)?"BOW":"GUN";
 public string WeaponGate(int id)=>IsGun(id)&&!GunsUnlocked?"Guns require character level 2 and tier 2 exploration":"Ready";
 public static double WeaponReach(int id)=>id is 22 or 23?1.65:id is >=24 and <=27?3.1:id is 11 or 16?3.6:id is 8 or 10 or 13 or 15 or 21?2.6:2;
 public static double WeaponDelay(int id)=>IsPoweredSaw(id)?.22:id is 22 or 23?.4:id is >=24 and <=26?.85:id is 11 or 16?.85:id==21?1.25:id==9?.65:.7;
 public double MeleeCost=>Math.Max(3,(IsPoweredSaw(Craft.Weapon)?6:Craft.Weapon is 22 or 23?5:Craft.Weapon is >=24 and <=26?12:Craft.Weapon==21?18:9)*(1-Math.Min(25,Skills.Melee.Level-1)*.01));
 public void TrainWeapon(double xp)=>WeaponProficiency.Gain(xp);
 public static IEnumerable<ExpeditionRecipe> PrimitiveRecipes(){
  yield return new("Stone chopping axe","tool",1,0,0,2,3,2,0,0,0,0);
  for(int id=8;id<GunNames.Length;id++){int tier=GunTier(id);bool bow=IsBow(id);int wood=id==9?0:bow?5:3,stone=id is 9 or 11?3:0,fiber=bow?5:id==11?2:id==13?2:0;if(id==13)wood=7;if(id==10)fiber=1;yield return new(GunNames[id],"weapon",tier,tier==1?0:tier,id,wood,stone,fiber,tier==2?4:0,tier>=3?5:0,tier>=4?3:0,tier==5?8:0);}
  yield return new("Chainsaw fuel / 40 cuts","fuel",3,2,40,0,0,2,4,2,0,0);
 }
}
