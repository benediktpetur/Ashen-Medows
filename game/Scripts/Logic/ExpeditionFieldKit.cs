using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
sealed partial class ExpeditionCraftState {
 [DataMember] public int[] ArrowStock=new int[6],ThrowStock=new int[10],ToolLevels=new int[4];
 [DataMember] public int ArrowType,HarvestTool,ScrapMetal,Glass;[DataMember] public int Throwable=-1,FieldKitVersion=1;
 public void NormalizeFieldKit(){void Size(ref int[] a,int n){if(a==null)a=new int[n];else if(a.Length!=n)Array.Resize(ref a,n);for(int i=0;i<n;i++)a[i]=Math.Clamp(a[i],0,1000000);}Size(ref ArrowStock,6);Size(ref ThrowStock,10);Size(ref ToolLevels,4);if(HasAxe)ToolLevels[0]=Math.Max(1,ToolLevels[0]);if(SkinningKnife)ToolLevels[3]=Math.Max(1,ToolLevels[3]);for(int i=0;i<4;i++)ToolLevels[i]=Math.Min(i>=2?3:2,ToolLevels[i]);ArrowType=Math.Clamp(ArrowType,0,5);if(FieldKitVersion==0){Throwable=-1;FieldKitVersion=1;}Throwable=Math.Clamp(Throwable,-1,9);HarvestTool=Math.Clamp(HarvestTool,0,2);ScrapMetal=Math.Max(0,ScrapMetal);Glass=Math.Max(0,Glass);}
 public int ArrowsOf(int type)=>type==0?Arrows:ArrowStock[type];
 public void AddArrows(int type,int amount){if(type==0)Arrows+=amount;else ArrowStock[type]+=amount;}
}
sealed class FieldRecipe {
 public string Name,Kind;public int Result,Amount,Tier,Station,Scrap,Glass,Fuel,Bone;public int[] Cost;
 public FieldRecipe(string name,string kind,int result,int amount,int tier,int wood=0,int stone=0,int fiber=0,int resin=0,int alloy=0,int electronics=0,int scrap=0,int glass=0,int fuel=0,int bone=0){Name=name;Kind=kind;Result=result;Amount=amount;Tier=tier;Station=tier==1?0:tier;Cost=new[]{wood,stone,fiber,resin,alloy,electronics,0};Scrap=scrap;Glass=glass;Fuel=fuel;Bone=bone;}
 public static readonly FieldRecipe[] All={
  new("Wood arrows / 12","arrow",0,12,1,wood:2,fiber:2),new("Flint arrows / 12","arrow",1,12,1,wood:2,stone:2,fiber:2),new("Bone arrows / 12","arrow",2,12,1,wood:2,fiber:2,bone:2),new("Iron arrows / 12","arrow",3,12,2,wood:2,fiber:2,scrap:3),new("Steel broadheads / 12","arrow",4,12,3,wood:2,fiber:2,alloy:2,scrap:2),new("Flaming arrows / 6","arrow",5,6,2,wood:2,fiber:3,fuel:2),
  new("Crude Molotov","throw",0,1,1,fiber:2,glass:1,fuel:2),new("Thickened Molotov","throw",1,1,3,fiber:2,resin:3,scrap:1,glass:1,fuel:5),new("Makeshift nail jar","throw",2,1,2,resin:2,fiber:2,scrap:4,glass:1),new("Military grenade","throw",3,1,4,resin:3,alloy:2,electronics:1,scrap:6),
  new("White smoke","throw",4,1,2,resin:1,fiber:2,scrap:1),new("Red smoke","throw",5,1,2,resin:1,fiber:2,scrap:1),new("Green smoke","throw",6,1,2,resin:1,fiber:2,scrap:1),new("Blue smoke","throw",7,1,2,resin:1,fiber:2,scrap:1),new("Flash grenade","throw",8,1,3,resin:2,alloy:2,scrap:2),new("Clockwork noise decoy","throw",9,1,2,wood:1,fiber:2,scrap:3),
  new("Iron chopping axe","tool",2,1,2,wood:2,fiber:2,scrap:5),new("Stone pickaxe","tool",11,1,1,wood:2,stone:4,fiber:2),new("Iron pickaxe","tool",12,1,2,wood:2,fiber:2,scrap:6),new("Salvage screwdriver","tool",21,1,1,wood:1,fiber:1,scrap:1),new("Salvage wrench","tool",22,1,2,scrap:4),new("Salvage blowtorch","tool",23,1,3,resin:3,alloy:2,scrap:6,fuel:3),new("Bone skinning knife","tool",32,1,1,fiber:1,bone:4),new("Iron skinning knife","tool",33,1,2,wood:1,fiber:1,scrap:5),
  new("Plant-oil fuel / 8","fuel",0,8,1,wood:1,fiber:4)
 };
}
sealed partial class ExpeditionGame {
 public static readonly string[] ArrowNames={"Wood arrow","Flint arrow","Bone arrow","Iron arrow","Steel broadhead","Flaming arrow"};
 public static readonly int[] ArrowBonus={0,4,7,12,18,6};
 public static readonly string[] ThrowableNames={"Crude Molotov","Thickened Molotov","Makeshift nail jar","Military grenade","White smoke","Red smoke","Green smoke","Blue smoke","Flash grenade","Clockwork noise decoy"};
 public static string HarvestToolName(int family,int level)=>family==0?(level>=2?"Iron chopping axe":"Stone chopping axe"):family==1?(level>=2?"Iron pickaxe":"Stone pickaxe"):family==2?(level>=3?"Salvage blowtorch":level==2?"Salvage wrench":"Salvage screwdriver"):level>=3?"Iron skinning knife":level==2?"Bone skinning knife":"Flint skinning knife";
 public double HarvestPower(int family)=>family==0?(Craft.ToolLevels[0]>=2?60:35):family==1?(Craft.ToolLevels[1]>=2?50:25):Craft.ToolLevels[2]>=3?65:Craft.ToolLevels[2]==2?40:20;
 public string FieldReason(FieldRecipe r){if(Craft.ScrapMetal<r.Scrap)return "Needs "+r.Scrap+" raw scrap metal";if(Craft.Glass<r.Glass)return "Needs salvaged glass / dismantle a car";if(Craft.Fuel<r.Fuel)return "Needs "+r.Fuel+" fuel";if(Craft.HuntStock[3]<r.Bone)return "Needs "+r.Bone+" bone";if(r.Kind=="tool"){int f=r.Result/10,l=r.Result%10;if(Craft.ToolLevels[f]>=l)return "Already owned";if(l>1&&Craft.ToolLevels[f]<l-1)return "Craft the previous tool first";}return "Ready";}
 public void CraftField(FieldRecipe r){Craft.Glass-=r.Glass;Craft.Fuel-=r.Fuel;Craft.HuntStock[3]-=r.Bone;if(r.Kind=="arrow")Craft.AddArrows(r.Result,r.Amount);else if(r.Kind=="throw")Craft.ThrowStock[r.Result]+=r.Amount;else if(r.Kind=="fuel")Craft.Fuel+=r.Amount;else{int f=r.Result/10;Craft.ToolLevels[f]=r.Result%10;if(f==0)Craft.HasAxe=true;if(f==3)Craft.SkinningKnife=true;}}
 public static IEnumerable<ExpeditionRecipe> FieldKitRecipes(){for(int i=0;i<FieldRecipe.All.Length;i++){var f=FieldRecipe.All[i];yield return new(f.Name,"fieldkit",f.Tier,f.Station,i,f.Cost[0],f.Cost[1],f.Cost[2],f.Cost[3],f.Cost[4],f.Cost[5],0){Scrap=f.Scrap};}yield return new("Salvaged flamethrower","weapon",3,3,34,2,0,2,3,4,0,0){Scrap=8};}
}
