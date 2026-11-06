using Godot;
using System;

public partial class MigrationScene {
 // Authored runs preserve the original bounds and every opening between runs.
 void AuthoredWall(Node3D root,string site,Vector3 center,Vector3 size,string family,bool protectedWall=false){
  bool acrossX=size.X>=size.Z;
  float yaw=acrossX?(center.Z>0?180:0):(center.X>0?-90:90);
  float length=acrossX?size.X:size.Z;
  int bays=Math.Max(1,(int)Math.Ceiling(length/HouseBay));
  float width=length/bays;
  string run=$"{Coordinate(center.X)},{Coordinate(center.Y)},{Coordinate(center.Z)}";
  for(int bay=0;bay<bays;bay++){
   float offset=(bay+.5f)*width-length/2;
   var at=center+(acrossX?Vector3.Right:Vector3.Back)*offset-Vector3.Up*size.Y/2;
   ModularHouseWall(root,site,run+"/"+bay,at,yaw,width,family,HouseWallOpening.Solid,false,size.Y,acrossX?size.Z:size.X,protectedWall);
  }
 }

 void ModularMapShell(Node3D root,string site,float width,float depth,float height,string family,bool simulation,bool foundation=true){
  if(foundation)HouseFoundation(root,new(width,depth),simulation);
  int bands=Math.Max(1,(int)Math.Ceiling((height-2.5f)/5f)+1);
  for(int band=0;band<bands;band++){
   float y=band==0?0:HouseWallHeight+(band-1)*(height-HouseWallHeight)/(bands-1);
   float bandHeight=band==0?Math.Min(height,HouseWallHeight):(height-HouseWallHeight)/(bands-1);
   for(int side=0;side<4;side++){
    float length=side%2==0?width:depth;
    float setback=((side%2==0?depth:width)-HouseWallDepth)/2;
    int count=Math.Max(1,(int)Math.Ceiling(length/(band==0?HouseBay:6)));
    if(side==0&&band==0&&count%2==0)count++;
    float bayWidth=length/count;
    float yaw=90*side;
    for(int bay=0;bay<count;bay++){
     var at=new Vector3((bay+.5f)*bayWidth-length/2,y,-setback).Rotated(Vector3.Up,Mathf.DegToRad(yaw));
     var opening=side==0&&band==0&&bay==count/2?HouseWallOpening.Door:
      band==0&&bay%2==0?HouseWallOpening.Window:HouseWallOpening.Solid;
     ModularHouseWall(root,site,$"{side}/{band}/{bay}",at,yaw,bayWidth,family,opening,simulation,bandHeight);
    }
   }
  }
  // The upper facade gives the lot its original skyline. The ground floor is walkable.
  var slab=new Vector3(width,.18f,depth);
  BatchPartBox(root,"floor-concrete",new(0,-.09f,0),slab,Ink,simulation,true);
  if(simulation)PhysicsObstacle(root,new BoxShape3D{Size=slab},new(0,height+.09f,0));
  else{
   BatchPartBox(root,"roof-flat",new(0,height+.09f,0),slab,Ink,solid:true);
   BatchWallCorners(root,family,width/2,depth/2,0,Math.Min(height,HouseWallHeight));
  }
 }
}
