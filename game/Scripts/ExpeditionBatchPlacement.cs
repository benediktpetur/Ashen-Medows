using Godot;
using System;

public partial class MigrationScene {
 void BatchLandmarkDetails(Node3D root,string name){
  void Detail(string key,Vector3 at,Vector3 size,float yaw=0)=>PlaceBatchPart(root,key,at,size,yaw);
  switch(name){
   case "The Last Petrol Stop":
    Detail("feature-fuel-canopy",new(0,3.55f,10.1f),new(6,.45f,3.2f));
    foreach(int side in new[]{-1,1}){
     Detail("pillar-steel",new(side*2.8f,0,10.4f),new(.18f,3.55f,.18f));
     Detail("feature-fuel-pump",new(side*2.2f,0,9.9f),new(.68f,1.35f,.58f));
    }
    break;
   case "Ashfall Freight Terminal":
    Detail("feature-loading-canopy",new(2.5f,2.6f,2.6f),new(7,.65f,2.3f),180);
    Detail("beam-steel",new(2.5f,2.7f,3.55f),new(7,.27f,.32f));
    ModularHouseWall(root,"content/ashfall-freight","yard-divider",new(7,0,4.8f),0,2.4f,"Industrial",HouseWallOpening.Solid,false,.96f);
    break;
   case "The Broken Pump House":
    Detail("feature-pipe-straight",new(-4,1.12f,1.4f),new(.48f,.5f,3.2f));
    Detail("feature-pipe-elbow",new(-4,1.1f,3.1f),new(.63f,.72f,.65f));
    break;
   case "Pinewatch Radio Tower":
    Detail("feature-radio-mast",new(0,12.6f,-3),new(.26f,3.1f,.26f));
    break;
   case "East Spur Recovery Garage":
    Detail("debris-metal",new(-7.1f,0,-4.5f),new(.85f,.16f,.7f));
    Detail("debris-brick",new(-7.4f,0,-3.5f),new(.75f,.36f,.65f));
    Detail("debris-concrete",new(7.4f,0,-4.8f),new(.85f,.55f,.72f));
    break;
   case "The Orchard Tool Shed":
    Detail("pillar-timber",new(-7.8f,0,6.4f),new(.22f,3.35f,.22f));
    BatchPartBox(root,"foundation-timber",new(6,.15f,-1.5f),new(3.2f,.30f,3.2f),Wood,solid:true);
    Detail("feature-greenhouse-frame",new(6,.30f,-1.5f),new(3.15f,2.65f,3.15f));
    Detail("debris-roof",new(-7.9f,0,-3.8f),new(1.2f,.42f,1.2f));
    Detail("debris-timber",new(-7.6f,0,5.3f),new(.95f,.22f,.68f));
    break;
   case "South Meadow Post Office":
   case "Turnpike Roadhouse":
    Detail("door-double-leaf",new(0,0,7.14f),new(2.65f,2.4f,.47f),180);
    Detail("shopfront-frame",new(-2.18f,0,7.12f),new(1.58f,2.49f,.19f),180);
    Detail("shopfront-frame",new(2.18f,0,7.12f),new(1.58f,2.49f,.19f),180);
    Detail("shop-awning",new(-5.8f,2.35f,7.7f),new(4.2f,1.1f,1.55f),180);
    Detail("shop-sign",new(2,2.46f,7.17f),new(1.6f,.85f,.1f),180);
    Detail("door-shop",new(4,0,7.18f),new(1.15f,2.2f,.12f),180);
    Detail("facade-trim",new(0,3.32f,7.15f),new(17.7f,.18f,.20f));
    Detail("facade-cornice",new(0,3.49f,7.17f),new(17.9f,.16f,.23f));
    break;
   case "Cinder Signal Box":
    Detail("foundation-skirt",new(5,0,-1.49f),new(3,.44f,.18f));
    Detail("beam-concrete",new(5,1.36f,-3),new(1.9f,.25f,.4f));
    break;
   case "The Rooftop Canteen":
    Detail("balcony-railing",new(-6,0,6.9f),new(5,1.1f,.14f),180);
    break;
   case "Harbour Quarantine Works":
    if(!PlaceBatchPart(root,"feature-quarantine-fence",new(-6,0,10.2f),new(4,1.7f,.18f),180))
     Detail("street-fence",new(-6,0,10.2f),new(4,1.7f,.18f),180);
    Detail("street-fence-gate",new(-3,0,10.2f),new(1.15f,1.7f,.16f),180);
    Detail("debris-rebar",new(7.5f,0,-1),new(.88f,.32f,.69f));
    break;
  }
 }

 void BatchRoadsideDetails(Node3D root,RoadsideStop stop){
  Vector3 Grounded(float x,float z)=>new(x,(float)game.World.Height(stop.X+x,stop.Z+z)-root.Position.Y,z);
  void Detail(string key,float x,float z,Vector3 size)=>PlaceBatchPart(root,key,Grounded(x,z),size);
  Detail("street-sign-post",-2.9f,-2.25f,new(.75f,2.1f,.12f));
  if(stop.Tier<3)return;
  if(stop.Tier==4&&stop.Index%3==2)
   Detail("street-traffic-light",5.6f,-1.6f,new(.55f,3.7f,.55f));
  Detail("street-hydrant",-5.5f,1.9f,new(.42f,.78f,.42f));
  Detail("street-drain",3.1f,-1.55f,new(.47f,.025f,.38f));
  Detail("street-manhole",-3.8f,-1.1f,new(.72f,.023f,.72f));
  if(stop.Index%3==0)Detail("street-dumpster",5.9f,2.1f,new(1.32f,1.05f,.9f));
  if(stop.Index%3==1)Detail("street-lamp",5.2f,1.75f,new(.52f,4.1f,.52f));
 }

 void BatchHouseRoofDetails(Node3D root,int style,float halfWidth,float halfDepth){
  float roof=HouseWallHeight+HouseFloorTop+.16f;
  foreach(int side in new[]{-1,1})
   PlaceBatchPart(root,"roof-eaves",new(0,roof-.22f,side*(halfDepth+.04f)),new(halfWidth*2,.25f,.18f),side>0?180:0);
  if(style%4==0){
   PlaceBatchPart(root,"roof-hip",new(0,roof,0),new(halfWidth*2+.2f,1.45f,halfDepth*2+.2f));
   PlaceBatchPart(root,"roof-ridge",new(0,roof+1.4f,0),new(.45f,.14f,2.2f),90);
  }else{
   PlaceBatchPart(root,"roof-gutter",new(0,roof-.14f,halfDepth+.04f),new(halfWidth*2,.13f,.17f));
   PlaceBatchPart(root,"roof-chimney-steel",new(halfWidth*.58f,roof,-halfDepth*.45f),new(.33f,1.02f,.33f));
  }
 }

 void BatchWallCorners(Node3D root,string family,float halfWidth,float halfDepth,float bottom,float height){
  string key=$"wall-{family.ToLowerInvariant()}-corner";
  foreach(int x in new[]{-1,1})foreach(int z in new[]{-1,1})
   PlaceBatchPart(root,key,new(x*halfWidth,bottom,z*halfDepth),new(.24f,height,.24f),x*z<0?90:0);
 }
}
