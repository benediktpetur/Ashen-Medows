using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
[DataContract] sealed class PlayerTradeLine {[DataMember] public string Key,Name;[DataMember] public int Amount,Owned;}
static class ExpeditionTrading {
 public static string StackName(int i)=>i<7?ExpeditionGame.ResourceNames[i]:i<10?new[]{"Food","Clean water","Bandages"}[i-7]:i<15?ExpeditionGame.AmmoNames[i-10]:new[]{"Scrap metal","Fuel","Basic arrows","Untreated water","Medicine"}[i-15];
 public static int Stack(ExpeditionGame g,int i)=>i<7?g.Craft.Materials[i]:i==7?g.Life.Food:i==8?g.Craft.Water:i==9?g.Life.Bandages:i<15?g.Craft.Stock(i-10):i==15?g.Craft.ScrapMetal:i==16?g.Craft.Fuel:i==17?g.Craft.ArrowsOf(0):i==18?g.Life.RawWater:g.Life.Medicine;
 static void Add(ExpeditionGame g,int i,int n){if(i<7)g.Craft.Materials[i]+=n;else if(i==7)g.Life.Food+=n;else if(i==8)g.Craft.Water+=n;else if(i==9)g.Life.Bandages+=n;else if(i<15)g.Craft.GiveAmmo(i-10,n);else if(i==15)g.Craft.ScrapMetal+=n;else if(i==16)g.Craft.Fuel+=n;else if(i==17)g.Craft.AddArrows(0,n);else if(i==18)g.Life.RawWater+=n;else g.Life.Medicine+=n;}
 public static bool WeaponAllowed(ExpeditionGame g,int id)=>id>=0&&id<ExpeditionGame.GunNames.Length&&id!=8&&id!=9&&g.Craft.Weapons.Contains(id)&&g.Craft.Weapon!=id&&!g.Life.Souls.Any(s=>s.Weapon==id)&&(!g.Craft.FittedParts.TryGetValue(id,out var parts)||parts==null||parts.Count==0);
 public static bool GearAllowed(ExpeditionGame g,ExpeditionRelic item)=>item!=null&&!g.IsGearEquipped(item.Id)&&item.Bond==null&&string.IsNullOrEmpty(item.SoulOwner);
 public static List<PlayerTradeLine> Inventory(ExpeditionGame g){var rows=new List<PlayerTradeLine>();for(int i=0;i<20;i++)if(Stack(g,i)>0)rows.Add(new(){Key="s:"+i,Name=StackName(i),Owned=Stack(g,i)});foreach(var item in g.Profile.Items.Where(i=>GearAllowed(g,i)))rows.Add(new(){Key="g:"+item.Id,Name=item.Name+$" / armor {item.Armor} / vitality {item.Vitality}",Owned=1});foreach(int id in g.Craft.Weapons.Distinct().Where(i=>WeaponAllowed(g,i)))rows.Add(new(){Key="w:"+id,Name=ExpeditionGame.GunNames[id]+$" / quality +{g.Life.WeaponQuality[id]}% / {g.Craft.Magazines[id]} loaded",Owned=1});return rows;}
 public static string Validate(ExpeditionGame from,ExpeditionGame to,List<PlayerTradeLine> offer){
  if(from==to||!from.Healthy||!to.Healthy||from.Life.Health<=0||to.Life.Health<=0)return "Both players must be alive with working saves";
  if(offer==null||offer.Count>64||offer.Any(l=>l==null||string.IsNullOrEmpty(l.Key))||offer.Select(l=>l.Key).Distinct().Count()!=offer.Count)return "Invalid offer";
  foreach(var l in offer){if(l.Name!=null&&l.Key!=null&&(l.Key.StartsWith("w:")||l.Key.StartsWith("g:"))&&Inventory(from).FirstOrDefault(i=>i.Key==l.Key)?.Name!=l.Name)return "Offered equipment changed; set its offer again";if(l.Amount<1||l.Amount>100000)return "Invalid quantity";string key=l.Key;
   if(key.StartsWith("s:")&&int.TryParse(key.Substring(2),out int i)&&i>=0&&i<20){if(Stack(from,i)<l.Amount)return "Offered supplies are no longer available";if((long)Stack(to,i)+l.Amount>int.MaxValue)return "Recipient inventory is full";}
   else if(key.StartsWith("g:")){var item=from.Profile.Items.Find(p=>p.Id==key.Substring(2));if(l.Amount!=1||!GearAllowed(from,item)||to.Profile.Items.Any(p=>p.Id==item.Id))return "Armor is equipped, bound or unavailable";}
   else if(key.StartsWith("w:")&&int.TryParse(key.Substring(2),out int id)){if(l.Amount!=1||!WeaponAllowed(from,id)||to.Craft.Weapons.Contains(id))return "Weapon equipped, fitted, bound, unavailable or already owned";}
   else return "Invalid trade item";
  }return "Ready";
 }
 static void Move(ExpeditionGame from,ExpeditionGame to,List<PlayerTradeLine> offer){foreach(var line in offer){var key=line.Key;if(key.StartsWith("s:")){int i=int.Parse(key.Substring(2));Add(from,i,-line.Amount);Add(to,i,line.Amount);}else if(key.StartsWith("g:")){var item=from.Profile.Items.Single(p=>p.Id==key.Substring(2));from.Profile.Items.Remove(item);to.Profile.Items.Add(item);}else{int id=int.Parse(key.Substring(2));from.Craft.Weapons.Remove(id);to.Craft.Weapons.Add(id);to.Craft.Magazines[id]=from.Craft.Magazines[id];from.Craft.Magazines[id]=0;to.Life.WeaponQuality[id]=from.Life.WeaponQuality[id];from.Life.WeaponQuality[id]=0;}}}
 public static bool Exchange(ExpeditionGame a,ExpeditionGame b,List<PlayerTradeLine> giveA,List<PlayerTradeLine> giveB,out string reason){
  reason=Validate(a,b,giveA);if(reason!="Ready")return false;reason=Validate(b,a,giveB);if(reason!="Ready")return false;if(giveA.Count+giveB.Count==0){reason="Add something to trade first";return false;}
  a.Craft.CommitAmmo();b.Craft.CommitAmmo();string beforeA=Json.Write(a.Save),beforeB=Json.Write(b.Save);
  try{Move(a,b,giveA);Move(b,a,giveB);if(!a.Persist()||!b.Persist())throw new Exception("Save failed");reason="Trade completed";return true;}
  catch{a.Save=Json.Read<ExpeditionGameSave>(beforeA);a.Normalize();b.Save=Json.Read<ExpeditionGameSave>(beforeB);b.Normalize();a.Persist();b.Persist();reason="Trade failed / inventories restored";return false;}
 }
}
