using System;
using System.Collections.Generic;

sealed class RoadsideStop {
 public int Index,Tier,Side;public double X,Z;
 public string Name=>Tier==1?"Neighbourhood exchange":Tier==2?"Trail shelter":Tier==3?"Freight rest stop":"City transit stop";
}
partial class ExpeditionWorld {
 List<RoadsideStop> roadsideStops;
 public IReadOnlyList<RoadsideStop> RoadsideStops(){
  if(roadsideStops!=null)return roadsideStops;
  roadsideStops=new();
  for(int cell=-37;cell<=3;cell++){
   bool placed=false;
   foreach(int offset in new[]{40,24,56,16})foreach(int direction in new[]{1,-1}){
   if(placed)continue;
   double z=cell*64+offset;int side=((cell&1)==0?-1:1)*direction;double x=RouteX(z)+side*(RouteHalfWidth(z)+8);
   int tier=Tier(x,z);
   if(!Playable(x,z)||BriarReserve(x,z)||LivingLot(x,z)||ResidentialInterior(x,z)||ExpeditionContentSite.InLot(this,x,z,18)||ExpeditionEncounter.InLot(this,x,z,24)||ExpeditionCampaign.InLot(this,x,z,28)||ExpeditionCrash.InLot(this,x,z,24)||ExpeditionCheckpoint.InLot(this,x,z,24)||ExpeditionBirthday.InLot(this,x,z,24)||ExpeditionLastLetter.InLots(this,x,z,24))continue;
   bool blocked=false;
   foreach(var d in ExpeditionDungeon.Nearby(this,x,z))if(ExpeditionGame.Distance(x,z,d.X,d.Z)<24)blocked=true;
   // Shelters stay beside the pavement, not in intersections or on steep banks.
   double low=double.MaxValue,high=double.MinValue;
   foreach(int a in new[]{-4,0,4})foreach(int b in new[]{-3,0,3}){double h=Height(x+a,z+b);low=Math.Min(low,h);high=Math.Max(high,h);if(MainRoad(x+a,z+b)||UrbanStreet(x+a,z+b))blocked=true;}
   if(blocked||high-low>1.6)continue;
   roadsideStops.Add(new(){Index=cell,Tier=tier,Side=side,X=x,Z=z});placed=true;
   }
  }
  return roadsideStops;
 }
 public bool NearRoadsideStop(double x,double z,double margin=0){foreach(var p in RoadsideStops())if(Math.Abs(x-p.X)<5+margin&&Math.Abs(z-p.Z)<4+margin)return true;return false;}
}
