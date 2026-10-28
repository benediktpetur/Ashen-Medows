using System;

partial class ExpeditionWorld {
 // Shared by rendered surfaces, vegetation clearance and the paper map.
 // Existing lot IDs, route coordinates and terrain heights remain unchanged.
 public double RouteHalfWidth(double z)=>Tier(RouteX(z),z)==3?7:5;
 public bool BriarReserve(double x,double z)=>Math.Abs(x-SpawnX)<88&&z>SpawnZ-202&&z<SpawnZ+45;
 public bool MainRoad(double x,double z,double margin=0)=>Math.Abs(x-RouteX(z))<RouteHalfWidth(z)+margin;
 public bool UrbanStreet(double x,double z,double margin=0){
  int tier=Tier(x,z);if(tier<3||!Playable(x,z))return false;
  double spacing=tier==3?64:32;
  double dx=Math.Abs(x-Math.Round(x/spacing)*spacing),dz=Math.Abs(z-Math.Round(z/spacing)*spacing);
  return dx<4+margin||dz<4+margin||Math.Abs(z-Math.Round(z/128)*128)<8+margin;
 }
 public bool BriarRoad(double x,double z,double margin=0){
  double cross=SpawnZ-173,route=RouteX(cross);
  return Math.Abs(x-SpawnX)<6+margin&&z>SpawnZ-185&&z<SpawnZ+45
   ||Math.Abs(z-cross)<5+margin&&x>Math.Min(route,SpawnX)-6&&x<Math.Max(route,SpawnX)+6;
 }
 public bool ResidentialApproach(double x,double z,double margin=0){
  var lot=Residential((int)Math.Floor(x/64),(int)Math.Floor(z/64));if(lot==null||Changes?.PreservedResidentialLots?.Contains(lot.Id)==true)return false;
  double street=(lot.CellX&1)==0?lot.CellX*64:(lot.CellX+1)*64;
  return Math.Abs(z-lot.Z)<1.6+margin&&x>Math.Min(lot.X,street)&&x<Math.Max(lot.X,street)&&!ResidentialInterior(x,z);
 }
 public bool BriarApproach(double x,double z,double margin=0){
  if(Math.Abs(x-SpawnX)>27||z<SpawnZ-170||z>SpawnZ-15)return false;
  for(int i=0;i<8;i++){double hx=SpawnX+(i%2==0?-26:26),hz=SpawnZ-30-(i/2)*42;
   if(Math.Abs(z-hz)<1.6+margin&&Math.Abs(x-SpawnX)<20)return true;
  }return false;
 }
 public bool SurfaceClearance(double x,double z,double margin=0)=>MainRoad(x,z,margin)||BriarRoad(x,z,margin)||ResidentialPaved(x,z)||ResidentialApproach(x,z,margin)||BriarApproach(x,z,margin)||UrbanStreet(x,z,margin)||Changes?.OnPath(x,z)==true;
}
