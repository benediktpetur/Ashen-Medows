using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

[DataContract] sealed class SoulboundWeapon {
 [DataMember] public string Id,Owner,Name,Source;
 [DataMember] public int Weapon,Damage,Capacity,Accuracy,Reload;
 public string Stats=>ExpeditionGame.IsGun(Weapon)?$"Damage {Damage:+0;-0;0}% / magazine {Capacity:+0;-0;0}% / spread {Accuracy:+0;-0;0}% / reload time {Reload:+0;-0;0}%":$"Damage {Damage:+0;-0;0}% / {(ExpeditionGame.IsBow(Weapon)?"arrow speed":"stamina efficiency")} {Capacity/2.0:+0.0;-0.0;0}% / {(ExpeditionGame.IsBow(Weapon)?"draw time":"attack recovery")} {Accuracy/2.0:+0.0;-0.0;0}% / noise {Reload:+0;-0;0}%";
 public static SoulboundWeapon Roll(string owner,int weapon,string source,int seed){
  var rng=new Random(seed);int archetype=rng.Next(3);
  return new(){Id=Guid.NewGuid().ToString("N"),Owner=owner,Weapon=weapon,Source=source,
   Name=(archetype==0?"Ravager's ":archetype==1?"Watcher's ":"Outrider's ")+ExpeditionGame.GunNames[weapon],
   Damage=archetype==0?rng.Next(65,101):rng.Next(15,46),Capacity=archetype==2?rng.Next(45,101):rng.Next(-35,21),
   Accuracy=archetype==1?rng.Next(-60,-34):rng.Next(-10,46),Reload=archetype==2?rng.Next(-45,-19):rng.Next(5,46)};
 }
}
sealed partial class ExpeditionLife {
 [DataMember] public string SoulOwner,ActiveSoul;
 [DataMember] public List<SoulboundWeapon> Souls=new();
 public void NormalizeSouls(){if(string.IsNullOrEmpty(SoulOwner))SoulOwner=Guid.NewGuid().ToString("N");Souls??=new();Souls.RemoveAll(s=>s==null||s.Owner!=SoulOwner||s.Weapon<0||s.Weapon>=ExpeditionGame.GunNames.Length);NormalizeMegaLoot();}
}
sealed partial class ExpeditionWorldChanges {
 [DataMember] public int MegaRuns,MegaVictories,MegaVariant;
 [DataMember] public double MegaReady;
}
sealed partial class ExpeditionGame {
 public SoulboundWeapon ActiveSoul=>Life.Souls?.Find(s=>s.Id==Life.ActiveSoul&&s.Owner==Life.SoulOwner&&s.Weapon==Craft.Weapon);
 public double SoulDamage(int weapon)=>ActiveSoul is {} s&&s.Weapon==weapon?1+Math.Clamp(s.Damage,0,100)/100.0:1;
 public double SoulSpread=>ActiveSoul is {} s?1+Math.Clamp(s.Accuracy,-60,45)/100.0:1;
 public double SoulReload=>ActiveSoul is {} s?1+Math.Clamp(s.Reload,-45,45)/100.0:1;
 public int SoulMagazine(int capacity)=>ActiveSoul is {} s?Math.Max(1,(int)Math.Round(capacity*(1+Math.Clamp(s.Capacity,-35,100)/100.0))):capacity;
 public bool EquipSoul(string id){
  var item=Life.Souls.Find(s=>s.Id==id&&s.Owner==Life.SoulOwner);if(item==null||WeaponGate(item.Weapon)!="Ready")return false;
  Craft.CommitAmmo();Equip(item.Weapon);Life.ActiveSoul=id;Handling.Weapon=-1;TrimSoulAmmo();return true;
 }
 public void TrimSoulAmmo(){if(IsGun(Craft.Weapon)&&Craft.Ammo>Magazine){Craft.Reserve+=Craft.Ammo-Magazine;Craft.Ammo=Magazine;Craft.CommitAmmo();}}
 public void ClaimSoul(string reward,string source,int seed){
  if(Craft.ResearchAwards.Contains(reward))return;
  Change(()=>{Craft.ResearchAwards.Add(reward);Life.NormalizeSouls();int[] guns={1,2,3,4,5,7};var roll=SoulboundWeapon.Roll(Life.SoulOwner,guns[(int)((uint)seed%guns.Length)],source,seed);Life.Souls.Add(roll);if(!Craft.Weapons.Contains(roll.Weapon))Craft.Weapons.Add(roll.Weapon);Notice="SOULBOUND / "+roll.Name+" / "+roll.Stats;});
 }
}
