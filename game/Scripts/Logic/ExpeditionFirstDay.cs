using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Collections.Generic;

sealed partial class ExpeditionLife {
 [DataMember] public int FirstDayMarks;
 [DataMember] public bool FirstDayHidden;
 [DataMember] public List<int> FirstDayCarriers=new();
}
sealed partial class ExpeditionGame {
 public static readonly string[] FirstDayTitles={"A drink you can trust","Something with reach","A tool worth keeping","Timber, earned","A place to work","Make the house a home","Put something aside"};
 public static readonly string[] FirstDayHints={
  "Find the blue emergency water carrier beside Briar Lane. F searches it; C drinks clean water. Untreated water can make you sick.",
  "Gather fallen branches, loose stones and flax around Briar Lane. K opens crafting: research and craft a crude spear. It keeps teeth at a distance.",
  "Craft the stone chopping axe in K: 2 timber, 3 stone, 2 fiber. Press T to hold it.",
  "Aim at a tree trunk within reach and click to swing. Each hit costs stamina. Let it recover between swings; the falling tree leaves a permanent stump.",
  "Craft a starter workbench in K: 6 timber and 4 stone. This unlocks boiling and more useful equipment.",
  "Your house on Briar Lane is the main settlement. B opens building. Put a survivor cot inside and a campfire and storage chest within 12m. Its existing roof and walls count as shelter. Other camps are outposts. Q/E rotate; G toggles snapping; Page Up/Down adjusts height. A cot is shelter, not a respawn point; sleeping bags arrive at Tier 2.",
  "F uses your campfire to cook and boil well water. Put 2 food and 2 clean water into your camp chest. Save something for tomorrow. This camp is yours; the woodland can wait."
 };
 public int FirstDayStep { get { for(int i=0;i<7;i++)if((Life.FirstDayMarks&(1<<i))==0)return i;return 7; } }
 public void MarkFirstDay(int step){Life.FirstDayMarks|=1<<step;}
 public bool FirstDayVisible=>!Life.FirstDayHidden&&FirstDayStep<7;
 public BuildPiece FirstDayBed=>Building.Pieces.FirstOrDefault(b=>b.Def.Function=="bed"&&b.Health>0&&ExpeditionCommunity.Sheltered(b,Building.Pieces)&&Building.Pieces.Any(p=>p.Health>0&&p.Def.Function=="cook"&&Distance(p.X,p.Z,b.X,b.Z)<12)&&Building.Pieces.Any(p=>p.Health>0&&p.Def.Function=="storage"&&Distance(p.X,p.Z,b.X,b.Z)<12));
 public void TickFirstDay(){
  if(Craft.Weapons.Contains(11))MarkFirstDay(1);
  if(Craft.HasAxe)MarkFirstDay(2);
  if(Craft.BenchTier>0)MarkFirstDay(4);
  var bed=FirstDayBed;if(bed==null)return;MarkFirstDay(5);
  if(Building.Pieces.Any(p=>p.Health>0&&p.Def.Function=="storage"&&Distance(p.X,p.Z,bed.X,bed.Z)<12&&p.Stock?.Length>8&&p.Stock[7]>=2&&p.Stock[8]>=2))MarkFirstDay(6);
 }
 public string FirstDayAction(string action){
  if(action=="hide"||action=="show"){Change(()=>Life.FirstDayHidden=action=="hide");return Notice=action=="hide"?"First-day guidance tucked away / F1 brings it back":"First-day guidance restored";}
  if(action!="water")return Notice="Unknown trailhead action";
  if(Distance(Save.X,Save.Z,ExpeditionWorld.SpawnX+13,ExpeditionWorld.SpawnZ-7)>4)return Notice="Stand beside the blue Briar Lane water carrier";
  Life.FirstDayCarriers??=new();
  if(Life.FirstDayCarriers.Contains(Save.Seed))return Notice="Your emergency ration is collected / draw well water and boil it for your next refill";
  return Notice=Change(()=>{Life.FirstDayCarriers.Add(Save.Seed);Craft.Water+=3;Life.Food+=2;Life.Bandages+=2;})?"Sealed water x3 / trail food x2 / dressings x2. C drinks; V eats; H patches bleeding.":Notice;
 }
}
