using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

sealed class WeaponPart {
 public string Name,Slot,Family,Description;public int Tier;public Func<int,bool> Fits;
 public double Damage=1,Stamina=1,Delay=1,Noise=1,Spread=1,Recoil=1,Reload=1,Capacity=1,Velocity=1;
 public WeaponPart(string name,string slot,string family,int tier,Func<int,bool> fits,string description){Name=name;Slot=slot;Family=family;Tier=tier;Fits=fits;Description=description;}
 public static readonly WeaponPart[] All={
  new("Cord-wrapped grip","Grip","Brawler",1,w=>ExpeditionGame.IsMelee(w)&&!ExpeditionGame.IsPoweredSaw(w),"15% less stamina per swing."){Stamina=.85},
  new("Weighted striking head","Head","Brawler",2,w=>w is 10 or 13 or 14 or 15 or 21 or 33,"20% damage; 20% slower swings and 20% more stamina."){Damage=1.2,Delay=1.2,Stamina=1.2},
  new("Honed steel edge","Head","Brawler",2,w=>w is 16 or 20 or 23 or 24 or 25 or 26,"15% damage. Fits steel blades and spearheads."){Damage=1.15},
  new("Field suppressor","Muzzle","Ballistics",3,w=>ExpeditionGame.IsGun(w)&&w is not (7 or 28 or 30 or 5),"50% less gunshot noise; 10% more spread. Shots still attract nearby infected."){Noise=.5,Spread=1.1},
  new("Ported muzzle brake","Muzzle","Ballistics",3,w=>ExpeditionGame.IsGun(w)&&w is not (7 or 28 or 30 or 5),"30% less recoil; 25% louder shots. Replaces a suppressor."){Recoil=.7,Noise=1.25},
  new("Magnified field optic","Optic","Ballistics",3,w=>ExpeditionGame.IsGun(w)&&w is not (0 or 7 or 28 or 29 or 30),"Magnified aim and 20% less spread; 10% slower reloads."){Spread=.8,Reload=1.1},
  new("Extended magazine","Magazine","Ballistics",3,w=>w is 0 or 3 or 5 or 29 or 31,"50% magazine capacity; 20% slower reloads. Never creates ammunition."){Capacity=1.5,Reload=1.2},
  new("Fitted shoulder stock","Stock","Ballistics",2,w=>ExpeditionGame.IsGun(w)&&w is not (0 or 28),"15% less spread; 8% slower reloads."){Spread=.85,Reload=1.08},
  new("High-tension bowstring","String","Ranger",2,w=>ExpeditionGame.IsBow(w),"20% faster arrows; 15% longer draw cycle."){Velocity=1.2,Delay=1.15},
  new("Balanced bow limbs","Limbs","Ranger",3,w=>ExpeditionGame.IsBow(w),"15% shorter draw cycle; 8% less damage."){Delay=.85,Damage=.92},
  new("Hardened cutting chain","Chain","Brawler",4,w=>ExpeditionGame.IsPoweredSaw(w),"18% damage; 20% more stamina per cutting burst."){Damage=1.18,Stamina=1.2},
  new("Stormfire cooling jacket","Drive","Brawler",5,w=>w==27,"15% faster cutting bursts; 15% more stamina. Requires the chain sword blueprint."){Delay=.85,Stamina=1.15}
 };
}
sealed partial class ExpeditionCraftState {
 [DataMember] public Dictionary<int,int> SpareParts=new();
 [DataMember] public Dictionary<int,List<int>> FittedParts=new();
 public void NormalizeParts(){SpareParts??=new();FittedParts??=new();foreach(int id in SpareParts.Keys.ToArray())if(id<0||id>=WeaponPart.All.Length||SpareParts[id]<0)SpareParts.Remove(id);foreach(int weapon in FittedParts.Keys.ToArray()){if(!Weapons.Contains(weapon)||FittedParts[weapon]==null){FittedParts.Remove(weapon);continue;}var slots=new HashSet<string>();FittedParts[weapon].RemoveAll(id=>id<0||id>=WeaponPart.All.Length||!WeaponPart.All[id].Fits(weapon)||!slots.Add(WeaponPart.All[id].Slot));}}
}
sealed partial class ExpeditionGame {
 public static int WeaponParent(int id)=>id switch{10=>8,11=>9,6=>12,14=>10,15=>10,16=>11,17=>14,18=>6,19=>18,20=>23,21=>14,23=>22,24=>23,25=>24,26=>25,27=>17,3=>2,4=>3,5=>4,7=>0,28=>0,29=>0,30=>7,31=>2,32=>18,33=>16,_=>-1};
 public ExpeditionRecipe ResearchParent(ExpeditionRecipe r){
  if(r.Kind=="build"&&BuildDefinition.All[r.Result].Opening is "doorway" or "window")return ExpeditionRecipe.All.FirstOrDefault(p=>p.Kind=="build"&&p.Tier==r.Tier&&BuildDefinition.All[p.Result].Function=="structure"&&BuildDefinition.All[p.Result].BaseKind==2);
  if(r.Kind=="build"&&BuildDefinition.All[r.Result].Opening=="gate")return ExpeditionRecipe.All.FirstOrDefault(p=>p.Kind=="build"&&p.Tier==r.Tier-1&&BuildDefinition.All[p.Result].Opening=="gate");
  if(r.Kind=="armor")return ExpeditionRecipe.All.Where(p=>p.Kind=="armor"&&p.Result==r.Result&&p.Tier<r.Tier).OrderByDescending(p=>p.Tier).FirstOrDefault();
  if(r.Kind=="survival-tech"&&r.Result is 1 or 5)return ExpeditionRecipe.All.FirstOrDefault(p=>p.Kind=="survival-tech"&&p.Result==0);
  if(r.Kind=="build"&&r.Tier>1){var d=BuildDefinition.All[r.Result];return ExpeditionRecipe.All.Where(p=>p.Kind=="build"&&p.Tier<r.Tier&&ResearchFamily(p)==ResearchFamily(r)&&BuildDefinition.All[p.Result].Function==d.Function&&(d.Function!="structure"||BuildDefinition.All[p.Result].BaseKind==d.BaseKind)).OrderByDescending(p=>p.Tier).FirstOrDefault();}
  int weapon=r.Kind=="weapon"?WeaponParent(r.Result):r.Kind=="weapon-mod"?r.Result switch{0=>10,1=>10,2=>16,3 or 4 or 6=>0,5=>2,7=>2,8=>12,9=>6,10=>17,11=>27,_=>-1}:-1;return weapon<0?null:ExpeditionRecipe.All.FirstOrDefault(p=>p.Kind=="weapon"&&p.Result==weapon);
 }
 public string BlueprintDependency(ExpeditionRecipe r){var parent=ResearchParent(r);return parent!=null&&!Knows(parent)?"Research "+parent.Name+" first":"Ready";}
 public IEnumerable<WeaponPart> PartsFor(int weapon)=>Craft.FittedParts.TryGetValue(weapon,out var parts)?parts.Where(id=>id>=0&&id<WeaponPart.All.Length).Select(id=>WeaponPart.All[id]):Enumerable.Empty<WeaponPart>();
 public double PartFactor(int weapon,Func<WeaponPart,double> stat)=>PartsFor(weapon).Aggregate(1d,(value,part)=>value*stat(part));
 public string PartReason(int part,bool remove=false){if(part<0||part>=WeaponPart.All.Length)return "Unknown part";var p=WeaponPart.All[part];if(!Craft.Weapons.Contains(Craft.Weapon)||!p.Fits(Craft.Weapon))return "Incompatible with equipped weapon";if(Craft.BenchTier<p.Tier)return "Requires workshop "+p.Tier;bool fitted=Craft.FittedParts.TryGetValue(Craft.Weapon,out var list)&&list.Contains(part);if(remove)return fitted?"Ready":"Part is not fitted";if(fitted)return "Already fitted";if(!Craft.SpareParts.TryGetValue(part,out int count)||count<1)return "Craft this part first";return "Ready";}
 public bool FitPart(int part,bool remove=false){string why=PartReason(part,remove);if(why!="Ready"){Notice=why;return false;}return Change(()=>{if(!Craft.FittedParts.TryGetValue(Craft.Weapon,out var list))Craft.FittedParts[Craft.Weapon]=list=new();if(remove){list.Remove(part);Craft.SpareParts[part]=Craft.SpareParts.GetValueOrDefault(part)+1;}else{foreach(int old in list.Where(id=>WeaponPart.All[id].Slot==WeaponPart.All[part].Slot).ToArray()){list.Remove(old);Craft.SpareParts[old]=Craft.SpareParts.GetValueOrDefault(old)+1;}Craft.SpareParts[part]--;list.Add(part);}TrimSoulAmmo();Handling.Weapon=-1;Notice=remove?"Part removed and returned to pack":"Fitted "+WeaponPart.All[part].Name;});}
 public string WeaponPartSummary(int weapon)=>string.Join("\n",PartsFor(weapon).Select(p=>p.Slot+": "+p.Name+" / "+p.Description));
 public static bool Shotgun(int weapon)=>weapon is 7 or 30;
 public static bool Automatic(int weapon)=>weapon is 3 or 29;
 public double FireDelay=>Craft.Weapon switch{28=>.5,29=>.10,30=>.85,31=>.45,0=>.32,3=>.14,7=>.9,_=>.7};
 public double WeaponNoise=>PartFactor(Craft.Weapon,p=>p.Noise);
 public static IEnumerable<ExpeditionRecipe> WorkshopRecipes(){
  foreach(int id in Enumerable.Range(28,6)){int tier=GunTier(id);yield return new(GunNames[id],"weapon",tier,tier,id,id is 32 or 33?6:2,0,id==32?6:2,tier==2?7:3,tier>=3?8:0,tier>=4?5:0,0);}
  for(int id=0;id<WeaponPart.All.Length;id++){var p=WeaponPart.All[id];int[] cost=new int[7];cost[2]=p.Tier==1?4:2;cost[p.Tier==1?0:p.Tier+1]=p.Tier+2;yield return new(p.Name,"weapon-mod",p.Tier,p.Tier,id,cost[0],cost[1],cost[2],cost[3],cost[4],cost[5],cost[6]);}
 }
}
