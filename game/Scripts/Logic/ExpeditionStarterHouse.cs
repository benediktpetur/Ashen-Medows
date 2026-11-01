using System;
using System.Runtime.Serialization;
sealed partial class ExpeditionWorldChanges { [DataMember]public int StarterHouseRevision; }
sealed partial class ExpeditionGame {
 void SeedEditableStarterHouse(){
  if(World.Changes.StarterHouseRevision>0)return;
  double floor=World.HomeFloor;World.Changes.AtlasHomeFloor=floor;
  int window=Array.FindIndex(BuildDefinition.All,d=>d.Tier==1&&d.Function=="window"),door=Array.FindIndex(BuildDefinition.All,d=>d.Tier==1&&d.Opening=="doorway");
  int index=0;
  void Part(int kind,double x,double z,double y,double yaw=0){var p=new BuildPiece{Id=$"starter-house/{Save.Seed}/{index++}",Kind=kind,X=ExpeditionWorld.HomeX+x,Z=ExpeditionWorld.HomeZ+z,Y=y,Yaw=yaw,Health=BuildDefinition.All[kind].Durability,Stock=new int[15]};if(Building.Placement(p).Valid)Building.Pieces.Add(p);}
  for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z++)Part(0,x*3,z*3,floor-.5);
  foreach(int side in new[]{-1,1})for(int i=-1;i<=1;i++){
   Part(i==0&&window>=0?window:2,i*3,side*4.5,floor);
   Part(side==-1&&i==0?door:2,side*4.5,i*3,floor,90);
  }
  Part(4,0,1,floor);
  for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z++)Part(7,x*3,z*3,floor+2.5);
  World.Changes.StarterHouseRevision=1;
 }
}
