using System;
using System.Linq;
using System.Collections.Generic;
static class ExpeditionBlueprints {
 public static void Extend(){var definitions=BuildDefinition.All.ToList();
  for(int tier=2;tier<=5;tier++)for(int i=0;i<8;i++){var old=BuildDefinition.All[i];definitions.Add(new BuildDefinition(new[]{"","","Stone-braced ","Scrap steel ","Reinforced concrete ","Military "}[tier]+old.Name.ToLowerInvariant(),old.W,old.H,old.D,old.Weight*(1+tier*.12),old.Capacity*(1+tier*.55),old.Ground){Tier=tier,Tab="Buildings",Function="structure",BaseKind=i});}
  void Add(string name,int tier,string tab,string function,double w,double h,double d){definitions.Add(new BuildDefinition(name,w,h,d,12+tier*3,100+tier*80,true){Tier=tier,Tab=tab,Function=function,BaseKind=-1});}
  Add("Timber door",1,"Buildings","door",1.2,2.5,.2);Add("Storage chest",1,"Buildings","storage",1.2,.8,.7);Add("Window wall",2,"Buildings","window",3,2.5,.2);Add("Timber gate",2,"Buildings","door",3,2.5,.25);Add("Large storage locker",2,"Buildings","storage",1.5,1.8,.7);Add("Vehicle repair station",3,"Buildings","repair",2,1.3,1.2);Add("Garage gate",3,"Buildings","door",4,3,.3);Add("Mobile camp trailer",3,"Buildings","mobile",2,2,3);Add("Generator",4,"Buildings","power",1.5,1.2,1);Add("Powered door",4,"Buildings","door",1.2,2.5,.25);Add("Electric lamp",4,"Buildings","light",.5,2.4,.5);Add("Fortified powered gate",5,"Buildings","door",3,2.5,.4);Add("Heavy structural support",5,"Buildings","support",.5,4,.5);Add("Expedition rover",5,"Buildings","vehicle",2.1,1.7,3.5);
  Add("Wood barricade",1,"Defenses","barrier",3,1.4,.5);Add("Wood spike trap",1,"Defenses","spikes",2,.6,2);Add("Fence section",1,"Defenses","fence",3,1.5,.2);Add("Watch platform",2,"Defenses","platform",3,2.5,3);Add("Snare trap",2,"Defenses","trap",1,.25,1);Add("Reinforced gate",2,"Defenses","door",3,2.5,.3);Add("Scrap barrier",3,"Defenses","barrier",3,1.5,.5);Add("Noise lure",3,"Defenses","lure",.8,1.2,.8);Add("Mounted machine gun",3,"Defenses","turret",1,1.5,1);Add("Electric fence",4,"Defenses","electric",3,1.5,.2);Add("Perimeter alarm",4,"Defenses","alarm",.6,1.8,.6);Add("Searchlight",4,"Defenses","searchlight",.8,2,.8);Add("Heavy defensive turret",5,"Defenses","turret",1.4,1.8,1.4);Add("Powered shock trap",5,"Defenses","electrictrap",2,.3,2);
  Add("Campfire",1,"Survival","cook",1,.4,1);Add("Boiling kettle",1,"Survival","water",.8,.8,.8);Add("Rain collector",2,"Survival","water",1.5,1.2,1.5);Add("Drying rack",2,"Survival","food",1.5,1.7,.7);Add("Crop bed",2,"Survival","farm",2,.3,2);Add("Water well",3,"Survival","water",2,1,2);Add("Refrigerator",3,"Survival","fridge",1,1.8,1);Add("Field kitchen",3,"Survival","cook",1.8,1.1,.8);Add("Medical station",4,"Survival","medical",2,.9,1);Add("Water filter",4,"Survival","water",1,1.5,1);Add("Greenhouse",4,"Survival","farm",3,2.5,3);Add("Advanced medical station",5,"Survival","medical",2,.9,1);Add("Efficient generator",5,"Survival","power",1.5,1.2,1);Add("Bulk water tank",5,"Survival","water",2,2,2);Add("Hydroponic farm",5,"Survival","farm",2,1,2);
  Add("Survivor cot",1,"Survival","bed",.9,.55,2);Add("Salvaged table",1,"Buildings","decor-table",1.4,.8,.8);
  Add("Camp stool",1,"Buildings","decor-stool",.45,.5,.45);
  Add("Patchwork rug",1,"Buildings","decor-rug",1.8,.035,1.2);
  Add("Keepsake shelf",1,"Buildings","decor-shelf",1.2,1.5,.35);
  Add("Resin lantern",2,"Buildings","decor-lantern",.3,.5,.3);
  Add("Scrap workshop table",3,"Buildings","decor-table",1.6,.85,.8);
  Add("Military footlocker",5,"Buildings","storage",1.2,.6,.65);
  Add("Cabin doorway",1,"Buildings","door",3,2.5,.2);
  definitions[91].Opening="doorway";definitions[91].Name="Rough timber door wall";definitions[42].Opening="window";definitions[42].Name="Stone-braced window wall";
  string[] styles={"","Rough timber","Stone-braced","Scrap steel","Reinforced concrete","Military"};
  for(int tier=1;tier<=5;tier++){
   if(tier>1){Add(styles[tier]+" door wall",tier,"Buildings","door",3,2.5,.2);definitions[^1].Opening="doorway";}
   if(tier!=2){Add(styles[tier]+" window wall",tier,"Buildings","window",3,2.5,.2);definitions[^1].Opening="window";}
   Add(new[]{"","Timber palisade gate","Stone-post timber gate","Scrap steel perimeter gate","Concrete bastion gate","Military checkpoint gate"}[tier],tier,"Defenses","door",6,2.5,.3);definitions[^1].Opening="gate";definitions[^1].Weight=30+tier*8;definitions[^1].Capacity=180+tier*80;
  }
  BuildDefinition.All=definitions.ToArray();foreach(var d in definitions){
   d.Cost=new int[7];int t=d.Tier;
   if(d.Function=="structure"){
    int[] wood={4,3,3,2,2,2,5,3};d.Cost[0]=t<=2?wood[d.BaseKind]:t==3?2:1;
    d.Cost[1]=t==1?(d.BaseKind==0?2:0):t==2?(d.BaseKind==0?6:3):t>=4?6:0;
    d.Cost[2]=t==1?1:0;if(t==2)d.Cost[3]=1;if(t>=3)d.Cost[4]=t==3?4:t==4?3:6;if(t==4)d.Cost[5]=1;if(t==5){d.Cost[5]=1;d.Cost[6]=2;}
   }else{
    d.Cost[0]=t==1?6:t==2?6:3;d.Cost[1]=d.Function is "cook" or "water"?4:0;d.Cost[2]=t==1?3:2;
    if(t>1)d.Cost[t+1]=t+2;if(t>=4)d.Cost[4]=5;
    if(d.Function.StartsWith("decor-")){d.Cost[0]=d.Function=="decor-stool"?2:4;d.Cost[2]=d.Function=="decor-rug"?6:2;}
    if(d.Function=="door"&&t==1){d.Cost[0]=3;d.Cost[2]=1;}
   }
   if(d.Opening!=""){
    bool gate=d.Opening=="gate",door=d.Opening=="doorway";d.Cost=new int[7];
    if(t==1){d.Cost[0]=gate?12:door?5:4;d.Cost[2]=gate?3:1;}
    if(t==2){d.Cost[0]=gate?12:4;d.Cost[1]=gate?10:4;d.Cost[2]=gate?2:1;d.Cost[3]=gate?3:door?2:1;}
    if(t==3){d.Cost[0]=gate?3:2;d.Cost[2]=1;d.Cost[4]=gate?12:door?5:4;}
    if(t==4){d.Cost[1]=gate?12:8;d.Cost[4]=gate?12:door?5:4;d.Cost[5]=gate?4:door?2:1;}
    if(t==5){d.Cost[4]=gate?18:door?10:8;d.Cost[5]=gate?4:1;d.Cost[6]=gate?6:door?3:2;}
   }
   d.Durability=new[]{0,100,180,300,460,700}[t];
  }
 }
 public static IEnumerable<ExpeditionRecipe> Recipes(){for(int i=0;i<BuildDefinition.All.Length;i++){var d=BuildDefinition.All[i];yield return new ExpeditionRecipe(d.Name,"build",d.Tier,d.Tier==1?0:d.Tier,i,d.Cost[0],d.Cost[1],d.Cost[2],d.Cost[3],d.Cost[4],d.Cost[5],d.Cost[6]);}}
}
