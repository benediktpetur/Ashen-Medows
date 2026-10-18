using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
sealed class ExpeditionTalent {
 public string Id,Name,Branch,Description;public int Row,MaxRank,Level;
 public ExpeditionTalent(string id,string name,string branch,int row,string description){Id=id;Name=name;Branch=branch;Row=row;Description=description;MaxRank=row==4?1:2;Level=new[]{2,4,8,12,20}[row];}
 public static readonly ExpeditionTalent[] All={
  new("damage","Deadeye","Combat",0,"+4% weapon damage per rank."),new("headshot","Precision","Combat",1,"+8% headshot damage per rank."),new("reload","Quick hands","Combat",2,"Reload 10% faster per rank."),new("melee","Heavy hitter","Combat",3,"+15% axe damage per rank."),new("execution","Executioner","Combat",4,"+20% weapon damage against enemies below 35% health."),
  new("health","Hardy","Toughness",0,"+10 maximum health per rank; no instant healing."),new("armor","Iron skin","Toughness",1,"+8 effective armor per rank."),new("recovery","Second wind","Toughness",2,"+15% slow health recovery per rank. Does not stop open bleeding."),new("guard","Braced","Toughness",3,"Take 5% less impact damage per rank; bites still bleed."),new("resolve","Last stand","Toughness",4,"Take 20% less impact damage below 30% health; bleeding continues until patched."),
  new("stamina","Deep reserves","Mobility",0,"+10 maximum stamina per rank."),new("sprint","Efficient stride","Mobility",1,"Sprint costs 8% less stamina per rank."),new("speed","Fleet footed","Mobility",2,"+3% movement speed per rank."),new("breath","Catch your breath","Mobility",3,"Stamina regenerates 10% faster per rank."),new("quiet","Ghost trail","Mobility",4,"Sprinting makes 40% less noise."),
  new("hunger","Trail rations","Survival",0,"Hunger drains 8% slower per rank."),new("water","Water discipline","Survival",1,"Hydration drains 8% slower per rank."),new("fatigue","Enduring","Survival",2,"Energy drains 8% slower per rank."),new("harvest","Resourceful","Survival",3,"Gathering and felling yield +1 material per rank."),new("travel","Wayfarer","Survival",4,"Safe bag travel costs 15 energy and has a 7-minute cooldown; other restrictions remain.")};
}
[DataContract] sealed class ExpeditionCareer {
 [DataMember] public int Level=1;[DataMember] public double Xp;[DataMember] public Dictionary<string,int> Talents=new();
 public double Need=>300+Level*100;public int Spent=>Talents.Values.Sum();public int Points=>Math.Max(0,Level-1-Spent);public int Rank(string id)=>Talents.TryGetValue(id,out int rank)?rank:0;
 public void Gain(double xp){if(!double.IsFinite(xp)||xp<=0||Level>=30)return;Xp+=xp;while(Level<30&&Xp>=Need){Xp-=Need;Level++;}if(Level>=30)Xp=0;}
 public void Normalize(){Level=Math.Clamp(Level,1,30);Xp=double.IsFinite(Xp)?Math.Clamp(Xp,0,Need-.001):0;if(Level==30)Xp=0;if(Talents==null)Talents=new();var accepted=new Dictionary<string,int>();foreach(var node in ExpeditionTalent.All.OrderBy(n=>n.Row)){int rank=Math.Clamp(Rank(node.Id),0,node.MaxRank);var parent=ExpeditionTalent.All.FirstOrDefault(n=>n.Branch==node.Branch&&n.Row==node.Row-1);int branch=ExpeditionTalent.All.Where(n=>n.Branch==node.Branch).Sum(n=>accepted.TryGetValue(n.Id,out int value)?value:0);if(Level<node.Level||parent!=null&&!accepted.ContainsKey(parent.Id)||node.Row==4&&branch<6)continue;rank=Math.Min(rank,Level-1-accepted.Values.Sum());if(rank>0)accepted[node.Id]=rank;}Talents=accepted;}
 public string Reason(ExpeditionTalent node){if(Rank(node.Id)>=node.MaxRank)return "Maximum rank";if(Level<node.Level)return "Requires character level "+node.Level;var parent=ExpeditionTalent.All.FirstOrDefault(n=>n.Branch==node.Branch&&n.Row==node.Row-1);if(parent!=null&&Rank(parent.Id)<1)return "Requires "+parent.Name;if(node.Row==4&&ExpeditionTalent.All.Where(n=>n.Branch==node.Branch).Sum(n=>Rank(n.Id))<6)return "Spend six points in this branch first";return Points<1?"Earn a character level for a talent point":"Ready";}
 public bool Learn(string id){var node=ExpeditionTalent.All.FirstOrDefault(n=>n.Id==id);if(node==null||Reason(node)!="Ready")return false;Talents[id]=Rank(id)+1;return true;}
}
sealed partial class ExpeditionGame {
 public ExpeditionCareer Career=>Skills.Career;
 public int Talent(string id)=>Career.Rank(id);
 public bool LearnTalent(string id){return Change(()=>{Notice=Career.Learn(id)?"Talent learned": "Talent prerequisites not met";});}
 public void ResetTalents(){Change(()=>{Career.Talents.Clear();Life.Health=Math.Min(Life.Health,MaxHealth);Stamina=Math.Min(Stamina,MaxStamina);Notice="Talents reset / all earned points refunded";});}
}
