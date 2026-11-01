using System;

partial class ExpeditionWorld {
 // Shared by rendered surfaces, vegetation clearance and the paper map.
 // Existing lot IDs, route coordinates and terrain heights remain unchanged.
 public double RouteHalfWidth(double z)=>Tier(RouteX(z),z)==3?7:5;
 public bool BriarReserve(double x,double z)=>Math.Abs(x-SpawnX)<88&&z>SpawnZ-202&&z<SpawnZ+45;
 public bool MainRoad(double x,double z,double margin=0)=>StreetDistance(x,z)<margin;
 public bool UrbanStreet(double x,double z,double margin=0){
  return District(x,z) is 3 or 4 or 7 && StreetDistance(x,z)<margin;
 }
 public bool BriarRoad(double x,double z,double margin=0){
  double cross=SpawnZ-173,route=SpawnX+225;
  return Math.Abs(x-SpawnX)<6+margin&&z>SpawnZ-185&&z<SpawnZ+45
   ||Math.Abs(z-cross)<5+margin&&x>Math.Min(route,SpawnX)-6&&x<Math.Max(route,SpawnX)+6;
 }
 public bool ResidentialApproach(double x,double z,double margin=0){
  var lot=Residential((int)Math.Floor(x/64),(int)Math.Floor(z/64));if(lot==null||Changes?.PreservedResidentialLots?.Contains(lot.Id)==true)return false;
  double yaw=lot.Yaw*Math.PI/180,dx=x-lot.X,dz=z-lot.Z;double localX=dx*Math.Cos(yaw)-dz*Math.Sin(yaw),localZ=dx*Math.Sin(yaw)+dz*Math.Cos(yaw);
  var street=NearestStreet(lot.X,lot.Z);double length=ExpeditionGame.Distance(lot.X,lot.Z,street.X,street.Z);return Math.Abs(localX)<1.6+margin&&localZ>6&&localZ<length&&!ResidentialInterior(x,z);
 }
 public bool BriarApproach(double x,double z,double margin=0){
  if(Math.Abs(x-SpawnX)>27||z<SpawnZ-170||z>SpawnZ-15)return false;
  for(int i=0;i<8;i++){double hx=SpawnX+(i%2==0?-26:26),hz=SpawnZ-30-(i/2)*42;
   if(Math.Abs(z-hz)<1.6+margin&&Math.Abs(x-SpawnX)<20)return true;
  }return false;
 }
 public bool SurfaceClearance(double x,double z,double margin=0)=>IsRiver(x,z)||ResidentialInterior(x,z)||MainRoad(x,z,margin)||BriarRoad(x,z,margin)||ResidentialPaved(x,z)||ResidentialApproach(x,z,margin)||BriarApproach(x,z,margin)||UrbanStreet(x,z,margin)||Changes?.OnPath(x,z)==true;
}
