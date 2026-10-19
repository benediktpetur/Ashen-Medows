using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

[DataContract] sealed partial class SoulUpgrade {
 [DataMember] public string Id,Owner;
 [DataMember] public int Kind,Power;
 public static readonly string[] Names={"Penetrator core","Gyro stabilizer","Quickdraw limbs","Efficient drive","Balanced grip"};
 public string Description=>Names[Kind]+" / "+Power+"% "+(Kind==0?"bonus against armor":Kind==1?"less recoil":Kind==2?"faster bow draw":Kind==3?"chance to save saw fuel":"less melee stamina cost");
}
[DataContract] sealed partial class SoulModule {
 [DataMember] public string Id,Owner;
 [DataMember] public int Kind,Power,Upkeep;
 [DataMember] public double Charge;
 public static readonly string[] Names={"Waterworks core","Workshop heart","Watchtower relay","Greenhouse regulator","Expedition board"};
 public string Description=>Names[Kind]+" / +"+Power+"% "+new[]{"water production","station production speed","guard damage and detection range","crop production speed","resident work speed"}[Kind]+" / "+Upkeep+(Kind==3?" camp water":" camp food")+" per 5 active minutes";
}
[DataContract] sealed partial class SoulCompanionGear {
 [DataMember] public string Id,Owner;
 [DataMember] public int Damage,Speed;
 public string Description=>$"Pathfinder harness / companion damage +{Damage}% / attack speed +{Speed}%";
}
sealed partial class ExpeditionRelic {
 [DataMember] public string SoulOwner;
 [DataMember] public int Mobility,Endurance,Quiet,Recovery,Reinforced,Ward;
 public string SoulStats=>$"Armor {Armor} / vitality {Vitality} / movement {Mobility:+0;-0;0}% / stamina {Endurance:+0;-0;0} / footstep noise {Quiet:+0;-0;0}% / recovery {Recovery:+0;-0;0}%";
}
sealed partial class ExpeditionLife {
 [DataMember] public List<SoulUpgrade> SoulUpgrades=new();
 [DataMember] public List<SoulModule> SoulModules=new();
 [DataMember] public List<SoulCompanionGear> SoulCompanions=new();
 [DataMember] public string ActiveUpgrade,ActiveCompanionGear;
 [DataMember] public int ArmorReinforcements,CommunityParts,ArmorMods;
 public void NormalizeMegaLoot(){SoulUpgrades??=new();SoulModules??=new();SoulCompanions??=new();}
}
sealed partial class ExpeditionWorldChanges {
 [DataMember] public List<SoulModule> CampModules=new();
}
sealed partial class ExpeditionGame {
 public IEnumerable<ExpeditionRelic> SoulArmor=>Profile.Items.Where(i=>IsGearEquipped(i.Id)&&i.SoulOwner==Life.SoulOwner);
 public double ArmorMobility=>1+Math.Clamp(SoulArmor.Sum(i=>i.Mobility),-35,20)/100.0;
 public int ArmorEndurance=>SoulArmor.Sum(i=>i.Endurance);
 public double ArmorQuiet=>1+Math.Clamp(SoulArmor.Sum(i=>i.Quiet),-50,30)/100.0;
 public double ArmorRecovery=>1+Math.Clamp(SoulArmor.Sum(i=>i.Recovery),0,60)/100.0;
 public SoulUpgrade ActiveUpgrade=>Life.SoulUpgrades.Find(i=>i.Id==Life.ActiveUpgrade&&i.Owner==Life.SoulOwner);
 public int UpgradePower(int kind)=>ActiveUpgrade?.Kind==kind?Math.Clamp(ActiveUpgrade.Power,0,35):0;
 public double SoulBowDelay=>(Craft.Weapon==32?1.0:.8)*PartFactor(Craft.Weapon,p=>p.Delay)*(1+(ActiveSoul is {} s?s.Accuracy/200.0:0))*(1-UpgradePower(2)/100.0);
 public double SoulArrowSpeed=>PartFactor(Craft.Weapon,p=>p.Velocity)*(1+(ActiveSoul is {} s?s.Capacity/200.0:0));
 public double SoulMeleeDelay=>WeaponDelay(Craft.Weapon)*PartFactor(Craft.Weapon,p=>p.Delay)*(1+(ActiveSoul is {} s?s.Accuracy/200.0:0));
 public double SoulStaminaCost=>1-(ActiveSoul is {} s?s.Capacity/200.0:0);
 public double SoulWeaponNoise=>1+(ActiveSoul is {} s&&!IsGun(s.Weapon)?s.Reload/100.0:0);
 public double CompanionDamage=>1+(Life.SoulCompanions.Find(i=>i.Id==Life.ActiveCompanionGear&&i.Owner==Life.SoulOwner)?.Damage??0)/100.0;
 public double CompanionAttack=>1-(Life.SoulCompanions.Find(i=>i.Id==Life.ActiveCompanionGear&&i.Owner==Life.SoulOwner)?.Speed??0)/100.0;
 public int ModulePower(int kind){World.Changes.CampModules??=new();return World.Changes.CampModules.Where(m=>m.Kind==kind&&m.Charge>0).Select(m=>m.Power).DefaultIfEmpty(0).Max();}
 public string ModuleGate(int kind)=>kind switch{0=>Life.SurvivalTech.Contains(0)||Life.SurvivalTech.Contains(1)?"Ready":"Craft a researched water filter first",1=>Craft.BenchTier>=5?"Ready":"Requires workshop 5",_=>"Ready"};
 public bool InstallModule(string id){
  var m=Life.SoulModules.Find(x=>x.Id==id&&x.Owner==Life.SoulOwner);World.Changes.CampModules??=new();var list=World.Changes.CampModules;
  if(m==null||list.Count>=3||list.Any(x=>x.Kind==m.Kind)||ModuleGate(m.Kind)!="Ready")return false;
  list.Add(Json.Read<SoulModule>(Json.Write(m)));return true;
 }
 public int MegaLootCount=>Life.Souls.Count+Profile.Items.Count(i=>i.SoulOwner==Life.SoulOwner)+Life.SoulModules.Count;
 public void ClaimMegaLoot(string reward,int variant,string source,int seed){
  if(Craft.ResearchAwards.Contains(reward))return;
  Change(()=>{Life.NormalizeSouls();Life.NormalizeMegaLoot();Craft.ResearchAwards.Add(reward);var r=new Random(seed);string Id()=>Guid.NewGuid().ToString("N");
   if(variant==2){int[] pool={1,2,3,4,5,7,19,16,21,26,27,23};int weapon=pool[r.Next(pool.Length)];var item=SoulboundWeapon.Roll(Life.SoulOwner,weapon,source,r.Next());Life.Souls.Add(item);if(!Craft.Weapons.Contains(weapon))Craft.Weapons.Add(weapon);
    int amount=r.Next(100)<20?2:1;for(int j=0;j<amount;j++)Life.SoulUpgrades.Add(new(){Id=Id(),Owner=Life.SoulOwner,Kind=r.Next(5),Power=r.Next(15,31)});Notice="SOULBOUND WEAPON / "+item.Name+" / weapon upgrade recovered";
   }else if(variant==0){string[] slots={"head","chest","hands","feet"};string slot=slots[r.Next(4)];int role=r.Next(3);var item=new ExpeditionRelic{Id=Id(),SoulOwner=Life.SoulOwner,Name=(role==0?"Colossus bulwark ":role==1?"Mirestalker ":"Lifekeeper ")+slot,Origin=source,Slot=slot,Level=5,Armor=(slot=="chest"?40:18)+r.Next(5,role==0?46:21),Vitality=r.Next(8,21),Mobility=role==0?-r.Next(4,11):role==1?r.Next(2,7):0,Endurance=role==0?-r.Next(4,11):r.Next(8,21),Quiet=role==1?-r.Next(8,21):role==0?8:0,Recovery=role==2?r.Next(10,21):0};Profile.Items.Add(item);Life.ArmorReinforcements+=3;if(r.Next(100)<25)Life.ArmorMods++;Notice="SOULBOUND ARMOR / "+item.Name;
   }else{var module=new SoulModule{Id=Id(),Owner=Life.SoulOwner,Kind=r.Next(5),Power=r.Next(20,46),Upkeep=r.Next(1,4)};Life.SoulModules.Add(module);Life.CommunityParts+=3;
    if(r.Next(100)<25)Life.SoulCompanions.Add(new(){Id=Id(),Owner=Life.SoulOwner,Damage=r.Next(15,31),Speed=r.Next(10,21)});Notice="SOULBOUND CAMP MODULE / "+SoulModule.Names[module.Kind];}
  });
 }
}
