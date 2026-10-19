using System;
using System.Collections.Generic;
partial class ExpeditionWorld {
 public const double SpawnX=1100,SpawnZ=160;
 public double Woodland(double x,double z){int t=Tier(x,z);if(t!=2)return t==1?.12:0;double road=Math.Abs(x-RouteX(z));return Math.Clamp(.55+road/220+(Hash((int)Math.Floor(x/64)+511,(int)Math.Floor(z/64))%100)/300.0,0,1);}
 readonly Dictionary<(int,int),double> habitatBuildingHeat=new();List<ExpeditionDungeon> habitatDungeons;
 public double SpawnHeat(double x,double z,bool night=false){if(!Playable(x,z)||ExpeditionGame.Distance(x,z,SpawnX,SpawnZ)<90)return 0;int tier=Tier(x,z);double road=Math.Exp(-Math.Pow((x-RouteX(z))/58,2));if(tier==1)return (.025+.04*road)*WorkPressure(x,z);int cellX=(int)Math.Floor(x/32),cellZ=(int)Math.Floor(z/32);if(!habitatBuildingHeat.TryGetValue((cellX,cellZ),out double buildings)){buildings=0;if(tier>=3)for(int a=-1;a<=1;a++)for(int b=-1;b<=1;b++)if(BuildingCell(cellX*32+16+a*32,cellZ*32+16+b*32))buildings+=.035;habitatBuildingHeat[(cellX,cellZ)]=buildings;}double landmark=0;foreach(var d in habitatDungeons??=ExpeditionDungeon.Nearby(this,0,0))landmark=Math.Max(landmark,.22*Math.Exp(-((x-d.X)*(x-d.X)+(z-d.Z)*(z-d.Z))/6400));double heat=(tier==2?.12+Woodland(x,z)*.13:tier==3?.28:.43)+road*(tier==2?.16:.22)+buildings+landmark;return Math.Clamp(heat*(night?1.3:1)*WorkPressure(x,z),0,1);}
 public int EncounterSize(double x,double z,uint roll,bool night=false){double heat=SpawnHeat(x,z,night);if(heat==0)return 0;if(Tier(x,z)==1)return roll%(uint)Math.Max(2,Math.Round(12/WorkPressure(x,z)))==0?1:0;return 1+(int)Math.Round(heat*7);}
}


