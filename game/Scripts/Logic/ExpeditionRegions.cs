using System;
using System.Collections.Generic;


partial class ExpeditionWorld {
 public const double MinX=-1600,MaxX=1600,MinZ=-2850,MaxZ=350,WaterLevel=3;
 void InitializeRegions(){}
 public double LegacyRouteX(double z){double phase=(Seed%997)*.006;return 105*Math.Sin(z*.003)+55*(Math.Sin(z*.007+phase)-Math.Sin(phase));}
 public double RouteX(double z){if(z<=-550)return LegacyRouteX(z);double t=TerrainSmooth((SpawnZ-z)/(SpawnZ+550));return SpawnX*(1-t)+LegacyRouteX(-550)*t;}
 public double RiverZ(double x){return -2510+55*Math.Sin(x*.003+Seed*.003)+25*Math.Sin(x*.008);}
 public bool IsRiver(double x,double z){return Math.Abs(z-RiverZ(x))<66;}
 public bool InBlock(double x,double z){return x>MinX+6&&x<MaxX-6&&z>MinZ+6&&z<MaxZ-6;}
 public bool Playable(double x,double z){return InBlock(x,z)&&z>RiverZ(x)+72;}
 int OrganicTier(double x,double z){double edge=35*Math.Sin(x*.006+Seed*.01)+20*Math.Sin(x*.014);if(z> -320+edge)return 1;if(z> -1210+edge)return 2;if(z> -1740+edge)return 3;return 4;}
 public double LegacyBaseHeight(double x,double z){double phase=(Seed%997)*.006;double baseH=13+4*Math.Sin(z*.003)+3*Math.Sin(x*.006+phase)*Math.Sin(z*.004);double hills=36*Math.Exp(-((x-520)*(x-520)/110000+(z+650)*(z+650)/210000))+27*Math.Exp(-((x+620)*(x+620)/130000+(z+1050)*(z+1050)/260000));double valley=-7*Math.Exp(-((x+260)*(x+260)/90000+(z+650)*(z+650)/260000));double h=baseH+hills+valley;double route=LegacyRouteX(z),grade=Math.Exp(-(x-route)*(x-route)/900);h=h*(1-grade)+(13+4*Math.Sin(z*.003))*grade;double spawn=Math.Exp(-(x*x+z*z)/1500);h=h*(1-spawn)+13*spawn;double river=Math.Abs(z-RiverZ(x));double cut=Math.Exp(-river*river/4200);return h*(1-cut)-1.5*cut;}
 static double TerrainSmooth(double t){t=Math.Clamp(t,0,1);return t*t*t*(t*(t*6-15)+10);}
 double TerrainNoise(double x,double z,double scale,int salt){double px=x/scale,pz=z/scale;int ix=(int)Math.Floor(px),iz=(int)Math.Floor(pz);double tx=TerrainSmooth(px-ix),tz=TerrainSmooth(pz-iz);double a=(Hash(ix+salt,iz-salt)%65536)/32767.5-1,b=(Hash(ix+1+salt,iz-salt)%65536)/32767.5-1,c=(Hash(ix+salt,iz+1-salt)%65536)/32767.5-1,d=(Hash(ix+1+salt,iz+1-salt)%65536)/32767.5-1;return (a+(b-a)*tx)*(1-tz)+(c+(d-c)*tx)*tz;}
 double NaturalHeight(double x,double z){
  double forest=TerrainSmooth((-z-220)/300),urban=TerrainSmooth((-z-1150)/400);double amp=(6+forest*12)*(1-urban*.65);
  double rolling=amp*TerrainNoise(x+37,z-51,170,91)+amp*.40*TerrainNoise(x-19,z+28,85,217);
  double east=24*Math.Exp(-((x-135)*(x-135)/10000+(z+115)*(z+115)/18000));
  double west=19*Math.Exp(-((x+155)*(x+155)/14000+(z+195)*(z+195)/22000));
  double ridge=48*Math.Exp(-((x-270)*(x-270)/42000+(z+640)*(z+640)/120000))+37*Math.Exp(-((x+310)*(x+310)/60000+(z+950)*(z+950)/160000));
  double h=20+rolling+east+west+ridge;double route=LegacyRouteX(z),road=13+4*Math.Sin(z*.003)+2*Math.Sin(z*.009);double grade=1-TerrainSmooth((Math.Abs(x-route)-7)/48);h=h*(1-grade)+road*grade;
  // A small, walkable spawn pad; the surrounding meadow is no longer flattened.
  double spawn=1-TerrainSmooth((Math.Sqrt(x*x+z*z)-16)/28);h=h*(1-spawn)+13*spawn;
  double river=Math.Abs(z-RiverZ(x)),cut=Math.Exp(-river*river/4200);return h*(1-cut)-1.5*cut;
 }
 readonly Dictionary<(int,int),double> terrainLotPads=new();
 public double BaseHeight(double x,double z){double h=NaturalHeight(x,z);if(Tier(x,z)>=3){int bx=(int)Math.Floor(x/32)*32+16,bz=(int)Math.Floor(z/32)*32+16;if(!terrainLotPads.TryGetValue((bx,bz),out double pad)){pad=BuildingCell(bx,bz)?NaturalHeight(bx,bz):double.NaN;terrainLotPads[(bx,bz)]=pad;}if(!double.IsNaN(pad)){double edge=Math.Max(Math.Abs(x-bx),Math.Abs(z-bz))-BuildingHalf(bx,bz);double blend=1-TerrainSmooth(edge/5);h=h*(1-blend)+pad*blend;}}return h;} public double BuildingHalf(double x,double z){return Tier(x,z)==3?6:10;}
 public double BuildingHeight(double x,double z){return Tier(x,z)==3?5+Hash((int)x,(int)z)%5:14+Hash((int)x,(int)z)%24;}
 public bool BuildingCell(double x,double z){foreach(var d in ExpeditionDungeon.Nearby(this,x,z))if((x-d.X)*(x-d.X)+(z-d.Z)*(z-d.Z)<20*20)return false;int tier=Tier(x,z);if(tier<3||!Playable(x,z)||Math.Abs(x-RouteX(z))<23)return false;if(Math.Abs(z-Math.Round(z/128)*128)<12)return false;if(tier==3&&Math.Abs(x-RouteX(z))>390)return false;return tier==4||Hash((int)(x/32),(int)(z/32))%3==0;}
 public static string TierName(int tier){return new[]{"","STARTING VALLEY","WOODLAND","OUTSKIRTS TOWN","SOUTH CITY","CITY DEPTHS"}[Math.Max(1,Math.Min(5,tier))];}
 static void CheckOrganic(ExpeditionWorld a,ExpeditionWorld b,ExpeditionWorld c){double[] zs={0,-700,-1450,-2100};for(int i=0;i<4;i++)if(a.Tier(a.RouteX(zs[i]),zs[i])!=i+1)throw new Exception("Block progression order failed");if(a.Playable(0,a.RiverZ(0))||a.Playable(0,a.RiverZ(0)-100)||!a.Playable(0,12))throw new Exception("River boundary failed");double low=double.MaxValue,high=double.MinValue;int changed=0;for(int x=-1200;x<=1200;x+=80)for(int z=-2200;z<=100;z+=90){double h=a.BaseHeight(x,z);if(h!=b.BaseHeight(x,z))throw new Exception("Terrain seed mismatch");if(Math.Abs(h-c.BaseHeight(x,z))>.01)changed++;low=Math.Min(low,h);high=Math.Max(high,h);}if(high-low<25||changed<50)throw new Exception("Hills/valleys or seed variation missing");}
}
