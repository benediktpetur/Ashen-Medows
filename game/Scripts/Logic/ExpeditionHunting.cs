using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
sealed partial class ExpeditionCraftState {
 [DataMember] public int[] HuntStock=new int[6];
 [DataMember] public bool SkinningKnife,HarvestCleaver;
 [DataMember] public int HuntingTool;
 [DataMember] public int[] HuntHalves=new int[6];
 public void NormalizeHunting(){if(HuntHalves==null||HuntHalves.Length!=6)HuntHalves=new int[6];for(int i=0;i<6;i++)HuntHalves[i]=Math.Clamp(HuntHalves[i],0,1);if(HuntingTool<0||HuntingTool>2||HuntingTool==1&&!SkinningKnife||HuntingTool==2&&!HarvestCleaver)HuntingTool=0;if(HuntStock==null)HuntStock=new int[6];else if(HuntStock.Length!=6)Array.Resize(ref HuntStock,6);for(int i=0;i<6;i++)HuntStock[i]=Math.Max(0,HuntStock[i]);}
}
sealed partial class ExpeditionRelic { [DataMember] public double Warmth; }
[DataContract] sealed class AnimalCarcass {
 [DataMember] public string Id,Species;
 [DataMember] public double X,Y,Z,Yaw,At;
 [DataMember] public bool Harvested;
 [DataMember] public int HarvestTool;
 [DataMember] public double Progress;[DataMember] public string Worker="";
 public int[] Yield=>Species switch {"rabbit"=>new[]{1,0,1,1,1,0},"deer"=>new[]{4,2,0,3,2,0},"boar"=>new[]{3,2,0,2,2,0},"wolf"=>new[]{2,1,3,2,2,0},_=>new int[6]};
}
sealed partial class ExpeditionWorldChanges { [DataMember] public List<AnimalCarcass> Carcasses=new(); }
sealed class HuntingRecipe {
 public string Name,Product;public int Tier,Amount;public bool Fire;public int[] Stock;public int Wood,Fiber,Resin;
 public HuntingRecipe(string name,int tier,string product,int amount,int meat,int hide,int fur,int bone,int sinew,int leather,int wood=0,int fiber=0,int resin=0,bool fire=false){Name=name;Tier=tier;Product=product;Amount=amount;Stock=new[]{meat,hide,fur,bone,sinew,leather};Wood=wood;Fiber=fiber;Resin=resin;Fire=fire;}
 public static readonly HuntingRecipe[] All={
  new("Cook game meat",1,"food",1,1,0,0,0,0,0,wood:1,fire:true),
  new("Sinew cord / 4 fiber",1,"fiber",4,0,0,0,0,2,0),
  new("Bone-tipped arrows / 8",1,"arrow",8,0,0,0,2,1,0,wood:1),
  new("Stitch rabbit-fur hood",1,"hood",1,0,0,3,0,1,0,fiber:2),
  new("Tan two hides / 2 leather",2,"leather",2,0,2,0,0,0,0,resin:1),
  new("Leather hunting boots",2,"boots",1,0,0,0,0,1,3,fiber:2),
  new("Fur-lined woodland vest",2,"vest",1,0,0,4,0,2,3,fiber:3),
  new("Bone binding paste / 4 fiber",2,"fiber",4,0,0,0,3,0,0,wood:1,fire:true),
  new("Craft flint skinning knife",1,"skinning-knife",1,0,0,0,0,0,0,wood:1,fiber:2),
  new("Craft stone butcher cleaver",1,"harvest-cleaver",1,0,0,0,0,0,0,wood:2,fiber:2)
 };
}
sealed partial class ExpeditionGame {
 public static readonly string[] HuntNames={"Raw game meat","Untanned hide","Fur","Bone","Sinew","Leather"};
 public string HarvestReason(AnimalCarcass carcass,double x,double z){if(carcass==null)return "No carcass";if(carcass.Harvested)return "Already harvested";if(Distance(x,z,carcass.X,carcass.Z)>3)return "Stand beside the carcass";if(World.Changes.Time-carcass.At>1800)return "Spoiled / no safe materials remain";if(carcass.Species=="dog")return "Infected remains / unsafe";return "Ready";}
 public bool HasHuntingTool(int tool)=>tool==0||tool==1&&Craft.SkinningKnife||tool==2&&Craft.HarvestCleaver;
 public static string HuntingToolName(int tool)=>tool==1?"Skinning knife":tool==2?"Butcher cleaver":"Balanced / hands";
 public bool SelectHuntingTool(int tool){if(!HasHuntingTool(tool)){Notice="Craft that harvesting tool first";return false;}return Change(()=>{Craft.HuntingTool=tool;Notice="Harvesting / "+HuntingToolName(tool);});}
 public static int HarvestHalfUnits(AnimalCarcass c,int tool,int i)=>c.Yield[i]*(tool==0||i>=4?2:tool==1?(i==1||i==2?4:1):(i==0||i==3?4:1));
 public static string HarvestPreview(AnimalCarcass c,int tool)=>string.Join(", ",Enumerable.Range(0,6).Where(i=>c.Yield[i]>0).Select(i=>$"{HarvestHalfUnits(c,tool,i)/2.0:0.#} {HuntNames[i]}"));
 public bool HarvestAnimal(string id,double x,double z){var c=World.Changes.Carcasses.FirstOrDefault(c=>c.Id==id);string reason=HarvestReason(c,x,z);if(reason!="Ready"){Notice=reason;return false;}if(!HasHuntingTool(c.HarvestTool)){Notice="Harvesting tool unavailable";return false;}return Change(()=>{c.Harvested=true;for(int i=0;i<6;i++){int halves=Craft.HuntHalves[i]+HarvestHalfUnits(c,c.HarvestTool,i);Craft.HuntStock[i]+=halves/2;Craft.HuntHalves[i]=halves%2;}Skills.Survival.Gain(15);Career.Gain(12);Notice="Harvested "+c.Species+" / "+HarvestPreview(c,c.HarvestTool);});}
 public string HuntingReason(int index,bool nearFire){if(index<0||index>=HuntingRecipe.All.Length)return "Unknown recipe";var r=HuntingRecipe.All[index];if(r.Product=="skinning-knife"&&Craft.SkinningKnife||r.Product=="harvest-cleaver"&&Craft.HarvestCleaver)return "Already owned";if((r.Product=="skinning-knife"||r.Product=="harvest-cleaver")&&Craft.Materials[1]<3)return "Needs 3 stone";if(Craft.UnlockedTier<r.Tier)return "Explore tier "+r.Tier;if(r.Tier>1&&Craft.BenchTier<2)return "Needs workshop 2";if(r.Fire&&!nearFire)return "Stand within 4m of your campfire or field kitchen";for(int i=0;i<6;i++)if(Craft.HuntStock[i]<r.Stock[i])return "Needs "+r.Stock[i]+" "+HuntNames[i];if(Craft.Materials[0]<r.Wood)return "Needs "+r.Wood+" timber";if(Craft.Materials[2]<r.Fiber)return "Needs "+r.Fiber+" fiber";if(Craft.Materials[3]<r.Resin)return "Needs "+r.Resin+" forest resin";return "Ready";}
 public bool CraftHunting(int index,bool nearFire){string why=HuntingReason(index,nearFire);if(why!="Ready"){Notice=why;return false;}var r=HuntingRecipe.All[index];return Change(()=>{for(int i=0;i<6;i++)Craft.HuntStock[i]-=r.Stock[i];Craft.Materials[0]-=r.Wood;Craft.Materials[2]-=r.Fiber;Craft.Materials[3]-=r.Resin;
   if(r.Product=="skinning-knife"||r.Product=="harvest-cleaver"){Craft.Materials[1]-=3;if(r.Product=="skinning-knife"){Craft.SkinningKnife=true;Craft.ToolLevels[3]=Math.Max(1,Craft.ToolLevels[3]);}else Craft.HarvestCleaver=true;}else if(r.Product=="food")Life.Food+=r.Amount;else if(r.Product=="fiber")Craft.Materials[2]+=r.Amount;else if(r.Product=="arrow")Craft.AddArrows(2,r.Amount);else if(r.Product=="leather")Craft.HuntStock[5]+=r.Amount;
   else{var gear=new ExpeditionRelic{Id="hunted/"+Guid.NewGuid(),Name=r.Name,Origin="Handcrafted from hunted wildlife",Slot=r.Product=="hood"?"head":r.Product=="boots"?"feet":"chest",Level=r.Tier,Armor=r.Product=="vest"?14:r.Product=="boots"?6:3,Vitality=r.Product=="vest"?3:1,Warmth=r.Product=="vest"?.20:r.Product=="hood"?.10:.05};Profile.Items.Add(gear);EquipGear(gear.Id);}Skills.Crafting.Gain(8*r.Tier);Notice="Crafted "+r.Name;});}
}
