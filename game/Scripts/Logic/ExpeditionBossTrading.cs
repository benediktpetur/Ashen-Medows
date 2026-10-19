using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
[DataContract] sealed class BossBond {
 [DataMember] public string Kill;
 [DataMember] public List<string> Eligible=new();
 public bool Allows(string owner)=>!string.IsNullOrEmpty(Kill)&&Eligible!=null&&Eligible.Contains(owner);
}
sealed partial class SoulboundWeapon {[DataMember] public BossBond Bond;}
sealed partial class SoulUpgrade {[DataMember] public BossBond Bond;}
sealed partial class SoulModule {[DataMember] public BossBond Bond;}
sealed partial class SoulCompanionGear {[DataMember] public BossBond Bond;}
sealed partial class ExpeditionRelic {[DataMember] public BossBond Bond;}
sealed class BossTradeItem {public string Id,Name;public BossBond Bond;public Action<ExpeditionGame> Move;public bool Equipped;}
sealed partial class ExpeditionGame {
 public List<BossTradeItem> BossTradeItems(){var list=new List<BossTradeItem>();
  void Add<T>(List<T> from,Func<ExpeditionGame,List<T>> target,Func<T,string> id,Func<T,string> name,Func<T,BossBond> bond,Action<T,string> owner,Func<T,bool> equipped){foreach(var item in from){var copy=item;list.Add(new(){Id=id(copy),Name=name(copy),Bond=bond(copy),Equipped=equipped(copy),Move=to=>{from.Remove(copy);owner(copy,to.Life.SoulOwner);target(to).Add(copy);}});}}
  Add(Life.Souls,g=>g.Life.Souls,i=>i.Id,i=>i.Name+" / "+i.Stats,i=>i.Bond,(i,o)=>i.Owner=o,i=>Life.ActiveSoul==i.Id);
  Add(Profile.Items,g=>g.Profile.Items,i=>i.Id,i=>i.Name+" / armor "+i.Armor,i=>i.Bond,(i,o)=>i.SoulOwner=o,i=>IsGearEquipped(i.Id));
  Add(Life.SoulUpgrades,g=>g.Life.SoulUpgrades,i=>i.Id,i=>i.Description,i=>i.Bond,(i,o)=>i.Owner=o,i=>Life.ActiveUpgrade==i.Id);
  Add(Life.SoulModules,g=>g.Life.SoulModules,i=>i.Id,i=>i.Description,i=>i.Bond,(i,o)=>i.Owner=o,i=>World.Changes.CampModules.Any(m=>m.Id==i.Id));
  Add(Life.SoulCompanions,g=>g.Life.SoulCompanions,i=>i.Id,i=>i.Description,i=>i.Bond,(i,o)=>i.Owner=o,i=>Life.ActiveCompanionGear==i.Id);return list;
 }
 public void BindBossRewards(HashSet<string> before,string kill,IEnumerable<string> owners){var eligible=owners.Distinct().ToList();foreach(var item in BossTradeItems().Where(i=>!before.Contains(i.Id))){var bond=new BossBond{Kill=kill,Eligible=eligible.ToList()};var weapon=Life.Souls.Find(i=>i.Id==item.Id);if(weapon!=null){weapon.Bond=bond;continue;}var armor=Profile.Items.Find(i=>i.Id==item.Id);if(armor!=null){armor.Bond=bond;armor.SoulOwner=Life.SoulOwner;continue;}var upgrade=Life.SoulUpgrades.Find(i=>i.Id==item.Id);if(upgrade!=null){upgrade.Bond=bond;continue;}var module=Life.SoulModules.Find(i=>i.Id==item.Id);if(module!=null){module.Bond=bond;continue;}var harness=Life.SoulCompanions.Find(i=>i.Id==item.Id);if(harness!=null)harness.Bond=bond;}Persist();}
 public string BossTradeReason(string id,ExpeditionGame target){var item=BossTradeItems().Find(i=>i.Id==id);if(item==null)return "Item no longer owned";if(target==null||target==this||Life.SoulOwner==target.Life.SoulOwner)return "Choose another character";if(!Healthy||!target.Healthy)return "Save protected";if(item.Bond==null||!item.Bond.Allows(Life.SoulOwner)||!item.Bond.Allows(target.Life.SoulOwner))return "Only original kill-group members can receive this item";if(item.Equipped)return "Unequip or uninstall this item first";if(target.BossTradeItems().Any(i=>i.Id==id))return "Duplicate item rejected";return "Ready";}
 public bool TransferBossItem(string id,ExpeditionGame target){Notice=BossTradeReason(id,target);if(Notice!="Ready")return false;Craft.CommitAmmo();target.Craft.CommitAmmo();string own=Json.Write(Save),other=Json.Write(target.Save);try{var item=BossTradeItems().Single(i=>i.Id==id);item.Move(target);var weapon=target.Life.Souls.Find(i=>i.Id==id);if(weapon!=null&&!target.Craft.Weapons.Contains(weapon.Weapon))target.Craft.Weapons.Add(weapon.Weapon);if(!target.Persist()||!Persist())throw new Exception("Could not save both characters");Notice=target.Notice="Boss item transferred / original kill-group binding retained";return true;}catch{Save=Json.Read<ExpeditionGameSave>(own);Normalize();target.Save=Json.Read<ExpeditionGameSave>(other);target.Normalize();Persist();target.Persist();Notice="Trade failed / inventories restored";return false;}}
}
