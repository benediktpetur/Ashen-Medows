using System;
using System.Linq;
sealed class ExpeditionThreat {
 public string Kind,Name,Role,Counter;public int Min,Max;public double Health,Speed;public bool NightOnly;
 public ExpeditionThreat(string kind,string name,int min,int max,double hp,double speed,string role,string counter,bool night=false){Kind=kind;Name=name;Min=min;Max=max;Health=hp;Speed=speed;Role=role;Counter=counter;NightOnly=night;}
 public static readonly ExpeditionThreat[] All={
  new("mossback","Mossback",2,3,105,.9,"Woodland ambusher","Watch the moss-covered silhouette; it wakes at close range or from noise."),
  new("howler","Bell-throat",2,5,90,1,"Alarm infected","Interrupt its raised-head scream before it alerts nearby infected."),
  new("bilecaster","Bilecaster",2,5,100,.8,"Ranged area denial","Leave the green target circle; close the gap during recovery."),
  new("gravecrawler","Gravecrawler",2,5,85,.8,"Low ambusher","Look down near graves and ruins; its low body can approach under your aim."),
  new("roadbreaker","Roadbreaker",3,5,230,1.05,"Charging brute","Sidestep the committed rush, then punish its long recovery."),
  new("cinder","Cinder Host",3,5,140,.8,"Volatile rare","Back away when it ignites on death; the remains leave burning ground."),
  new("riot","Riot Husk",4,5,310,.65,"Shielded elite","Flank the shield or aim above it. Frontal body hits are heavily resisted."),
  new("wirebound","Wirebound",3,5,160,.95,"Stamina disruptor","Dodge the purple strike marker. Its wire lash drains stamina without rooting you."),
  new("blooddrinker","Blooddrinker",4,5,245,1.2,"Sustaining elite","Avoid its melee strikes; successful hits restore its health."),
  new("palehunter","Pale Hunter",4,5,360,1.45,"Night/depths elite","Track its flanking approach and sidestep the telegraphed leap.",true)
 };
 public static ExpeditionThreat Find(string kind)=>All.FirstOrDefault(t=>t.Kind==kind);
 public static string SpawnKey(string id){int index=id.LastIndexOf("/ecology/",StringComparison.Ordinal);if(index<0)return id;string tail=id.Substring(index+9);return tail=="native"||Find(tail)!=null?id.Substring(0,index):id;}
 public static string Population(string id,int tier,int night){int index=id.LastIndexOf("/ecology/",StringComparison.Ordinal);if(index<0)return null;string kind=id.Substring(index+9);var t=Find(kind);return t!=null&&tier>=t.Min&&tier<=t.Max?kind:null;}

}
sealed partial class ExpeditionGame {
 public void ThreatReward(string id,int tier,int night){var spec=ExpeditionThreat.Find(ExpeditionCampaign.Kind(id,tier,night));if(spec==null)return;Craft.Materials[Math.Clamp(tier+1,3,6)]+=spec.Health>=230?2:1;Career.Gain(spec.Health>=230?35:10);if(spec.NightOnly)Life.NightEssence+=2;}
}
