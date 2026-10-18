using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

static class ExpeditionCampaign {
 public static readonly string[] Names={"The Wrecker","The Bellkeeper","The Broodheart"};
 public static readonly double[] Z={-1450,-1940,-2250};
 static readonly ConditionalWeakTable<ExpeditionWorld,(double X,double Z)[]> Sites=new();
 static (double X,double Z)[] CreateSites(ExpeditionWorld world){
  var result=new (double X,double Z)[3];var portals=ExpeditionDungeon.Nearby(world,0,0);
  for(int i=0;i<3;i++){
   double side=i==2?1:-1,x=world.RouteX(Z[i])+(i==0?-280:i==1?-330:300);
   for(int attempt=0;attempt<10&&(ExpeditionEncounter.InLot(world,x,Z[i],40)||portals.Exists(p=>ExpeditionGame.Distance(p.X,p.Z,x,Z[i])<70));attempt++)x+=side*80;
   result[i]=(x,Z[i]);
  }
  return result;
 }
 public static (double X,double Z) Site(ExpeditionWorld world,int index)=>Sites.GetValue(world,CreateSites)[index];

 public static bool InLot(ExpeditionWorld world,double x,double z,double margin=0){for(int i=0;i<3;i++){var p=Site(world,i);if(Math.Abs(x-p.X)<27+margin&&Math.Abs(z-p.Z)<27+margin)return true;}return false;}
 public static int BossIndex(string id){if(id==null||!id.StartsWith("boss/campaign/"))return -1;var parts=id.Split('/');return (parts.Length==4||parts.Length==7&&parts[4]=="mega")&&int.TryParse(parts[3],out int n)&&n>=0&&n<3?n:-1;}
 public static string Kind(string id,int tier,int night){
  if(id.StartsWith("brood/worm/"))return "worm";
  if(id.StartsWith("special/"))return id.Split('/')[1];
  if(!(id.StartsWith("field-site/")||id.StartsWith("field-event/")||id.StartsWith("habitat/")||id.StartsWith("night/")||id.Contains("/vault/")))return "walker";
  string extra=ExpeditionThreat.Population(id,tier,night);if(extra!=null)return extra;
  id=ExpeditionThreat.SpawnKey(id);uint roll=(uint)id.GetHashCodeStable();
  if(night==2)return "bloater";if(night==1)return "stalker";
  if(tier==1)return "walker";
  if(id.StartsWith("field-event/"))return tier>=3&&roll%4==0?"bruiser":"walker";if(roll%19==0)return "weeper";
  if(roll%9==0||tier>=5&&roll%3==0)return "bloater";
  if(tier>=4&&roll%3==0)return "armored";
  if(tier>=3&&roll%5==0)return "bruiser";
  return roll%4==0?"runner":"walker";
 }
 public static double Hp(string kind,int tier)=>ExpeditionThreat.Find(kind) is {} t?t.Health+Math.Max(0,tier-t.Min)*18:kind switch{"worm"=>18,"weeper"=>280+tier*40,"bloater"=>120+tier*32,"armored"=>100+tier*24,"bruiser"=>150+tier*25,"runner"=>40+tier*17,_=>45+tier*20};
}
sealed partial class ExpeditionWorldChanges {
 [DataMember] public List<int> CampaignBosses=new();
}
sealed partial class ExpeditionGame {
 public string LegendaryReason=>Life.LegendaryCores<1?"Defeat the Broodheart for a legendary heart":GunTier(Craft.Weapon)<(IsBow(Craft.Weapon)||Craft.Weapon==21?4:5)?"Equip a maximum-tier weapon for its class":Life.WeaponQuality[Craft.Weapon]>=50?"Already legendary":"Ready";
}

