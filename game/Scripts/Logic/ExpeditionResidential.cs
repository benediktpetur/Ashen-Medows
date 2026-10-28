using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
sealed class ResidentialLot {
 public int CellX,CellZ,Style;public double X,Z,Yaw,ScaleX,ScaleZ;public string Id;
 public string Address=>(CellX+25)*2+10+" "+new[]{"Alder Drive","Willow Row","Cedar Street","Orchard Lane","Hawthorn Close","Birch Road","Foxglove Avenue","Wren Street","Elm Crescent","Laurel Way","Hazel Road","Maple Row","Brook Street","Ash Lane","Fern Avenue","Oak Drive"}[((CellZ%16)+16)%16];
}
partial class ExpeditionWorld {
 public const double HomeX=SpawnX+26,HomeZ=SpawnZ-30;
 readonly Dictionary<(int,int),ResidentialLot> residentialLots=new();
 public static bool HomeInterior(double x,double z)=>Math.Abs(x-HomeX)<5.1&&Math.Abs(z-HomeZ)<5.6;
 public double HomeFloor{get{double y=Height(HomeX,HomeZ);foreach(int x in new[]{-7,7})foreach(int z in new[]{-7,7})y=Math.Max(y,Height(HomeX+x,HomeZ+z));return y+.21;}}
 public double StructuralGround(double x,double z)=>HomeInterior(x,z)?HomeFloor:Height(x,z);
 public ResidentialLot Residential(int ix,int iz){if(residentialLots.TryGetValue((ix,iz),out var known))return known;uint h=Hash(ix+1917,iz-337);double x=ix*64+32+(h%9)-4,z=iz*64+32+((h>>8)%9)-4;ResidentialLot p=null;
  if(Tier(x,z)==1&&Playable(x,z)&&!(Math.Abs(x-SpawnX)<105&&z>SpawnZ-220&&z<SpawnZ+65)&&!LivingLot(x,z)&&!ExpeditionContentSite.InLot(this,x,z,25)&&!ExpeditionEncounter.InLot(this,x,z,25)&&!ExpeditionCrash.InLot(this,x,z,25)&&!ExpeditionCheckpoint.InLot(this,x,z,25)&&!ExpeditionBirthday.InLot(this,x,z,25)&&!ExpeditionLastLetter.InLots(this,x,z,25))p=new(){Id=$"residential/{Seed}/{ix}/{iz}",X=x,Z=z,CellX=ix,CellZ=iz,Style=(int)(h%24),Yaw=(ix&1)==0?-90:90,ScaleX=.9+((h>>12)%5)*.08,ScaleZ=.9+((h>>16)%4)*.08};
  residentialLots[(ix,iz)]=p;return p;
 }
 public bool ResidentialInterior(double x,double z){var p=Residential((int)Math.Floor(x/64),(int)Math.Floor(z/64));return p!=null&&Math.Abs(x-p.X)<6.2*p.ScaleZ&&Math.Abs(z-p.Z)<6.5*p.ScaleX;}
 public static double GridStreetDistance(double v){double t=(v%128+128)%128;return Math.Min(t,128-t);}
 public bool ResidentialPaved(double x,double z)=>Tier(x,z)==1&&Playable(x,z)&&!BriarReserve(x,z)&&(GridStreetDistance(x)<8||GridStreetDistance(z)<8)&&!ResidentialInterior(x,z);
}
sealed partial class ExpeditionWorldChanges {
 [DataMember]public List<string> StrippedFurniture=new();
 [DataMember]public int ResidentialRevision;
 [DataMember]public List<string> PreservedResidentialLots=new();
}
