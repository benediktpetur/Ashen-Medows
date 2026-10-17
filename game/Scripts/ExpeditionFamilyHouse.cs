using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 const string FamilyRoom="story/family-house";const double FamilyZ=-930;double FamilyX=>game.World.RouteX(FamilyZ)+55;double familyFloor;Node3D familyHome,familyGarageDoor,familySafeDoor,familyShelf;OmniLight3D familyLamp;
 bool InFamilyRoom=>dungeon?.Id==FamilyRoom;string FamilyInfectedId=>$"story/family/frail/{game.Save.Seed}";
 bool InFamilyLot(double x,double z)=>x>FamilyX-9&&x<FamilyX+18&&z>FamilyZ-9&&z<FamilyZ+28;
 Vector3 FamilyPoint(float x,float y,float z)=>new((float)FamilyX+x,(float)familyFloor+y,(float)FamilyZ+z);
 ExpeditionDungeon FamilyDungeon()=>new(){Id=FamilyRoom,X=FamilyX-3,Z=FamilyZ-3,Tier=2,Name="THE HOUSE THAT WAITED / BASEMENT"};
 void SpawnFamilyHouse(){if(dungeon!=null)return;bool near=CoopHost?NearActiveSurface(Ground(FamilyX,FamilyZ),220):Distance(player.Position,Ground(FamilyX,FamilyZ))<220;if(!near||uniqueObjects.Contains("story/family-house"))return;familyFloor=Enumerable.Range(0,5).SelectMany(x=>Enumerable.Range(0,3).Select(z=>game.World.Height(FamilyX-5+x*4.5,FamilyZ-6+z*6))).Max()+.15;
  familyHome=new Node3D{Name="TheHouseThatWaited",Position=FamilyPoint(0,0,0)};worldRoot.AddChild(familyHome);props.Add(new(){Id="story/family-house",Kind=19,Node=familyHome,Position=familyHome.Position});uniqueObjects.Add("story/family-house");DressFamilyExterior(familyHome);DressFamilyLivingRoom(familyHome);familyGarageDoor=FamilyDoor(familyHome,"GarageDoor",new(7,0,4),6,2.8f);DressFamilyGarage(familyHome);FamilyPorchRamp(familyHome);FamilyExteriorDetails(familyHome);FamilyRoomDetails(familyHome,false);FamilyFinishMaterials(familyHome,false);
 }
 Node3D FamilyDoor(Node3D parent,string name,Vector3 at,float width,float height){var door=new Node3D{Name=name,Position=at};parent.AddChild(door);Box(door,new(width/2,height/2,0),new(width,height,.18f),name=="GarageDoor"?Steel:Wood,true);for(int i=1;i<5;i++)Box(door,new(width/2,height*i/5,-.10f),new(width-.1f,.025f,.02f),Ink);return door;}
 void FamilyPorchRamp(Node3D n){WillowAccessRamp(n,0,7,25,"WillowPorchAccess");WillowAccessRamp(n,10,4,20,"WillowGarageAccess");}
 void DressFamilyExterior(Node3D n){var paint=new Color(.42f,.47f,.39f);Box(n,new(0,-.25f,0),new(10,.5f,14),new(.34f,.35f,.31f),true);Box(n,new(-5,1.6f,0),new(.22f,3.2f,12),paint,true);Box(n,new(5,1.6f,0),new(.22f,3.2f,12),paint,true);Box(n,new(0,1.6f,-6),new(10,3.2f,.22f),paint,true);for(int side=-1;side<=1;side+=2)Box(n,new(side*3.1f,1.6f,6),new(3.8f,3.2f,.22f),paint,true);Box(n,new(0,2.9f,6),new(2.4f,.6f,.22f),paint,true);
  WillowWindow(n,-3.25f,1.65f,6.14f,true);WillowWindow(n,3.25f,1.65f,6.14f,true,true);
  for(int i=0;i<7;i++)Box(n,new(-4.5f+i*1.4f,.1f,7.1f),new(.9f,.16f,.5f),new(.28f,.30f,.25f));for(int side=-1;side<=1;side+=2){Tube(n,new(-6.8f+side*1.2f,0,12),new(-6.8f,2.4f,12),.055f,Steel);Tube(n,new(-6.8f+side*1.2f,0,15),new(-6.8f,2.4f,15),.055f,Steel);}Tube(n,new(-6.8f,2.4f,12),new(-6.8f,2.4f,15),.055f,Steel);for(int i=0;i<2;i++){Tube(n,new(-6.8f,2.35f,12.5f+i*1.7f),new(-6.8f,.65f,12.5f+i*1.7f),.01f,Steel);Box(n,new(-6.8f,.62f,12.5f+i*1.7f),new(.65f,.08f,.5f),Wood);}n.AddChild(new Label3D{Text="WILLOW HOUSE",Position=new(-3,1,7.2f),FontSize=24,PixelSize=.007f,Billboard=BaseMaterial3D.BillboardModeEnum.Enabled,Modulate=new(.70f,.69f,.54f)});
  Box(n,new(10,-.25f,0),new(6,.5f,8),new(.31f,.32f,.29f),true);Box(n,new(7,1.5f,0),new(.2f,3,8),paint,true);Box(n,new(13,1.5f,0),new(.2f,3,8),paint,true);Box(n,new(10,1.5f,-4),new(6,3,.2f),paint,true);Box(n,new(10,3.05f,0),new(6.4f,.2f,8.5f),new(.26f,.27f,.23f),true);var car=new Node3D{Position=new(14,(float)(game.World.Height(FamilyX+14,FamilyZ+9)-familyFloor),9)};n.AddChild(car);ArtWreck(car,false);PhysicsObstacle(car,new BoxShape3D{Size=new(2.1f,1.5f,4.2f)},new(0,.75f,0));for(int i=0;i<6;i++)Ellipsoid(car,new(Mathf.Sin(i)*.7f,1.2f,-1+i*.3f),new(.2f,.025f,.13f),new(.37f,.34f,.23f));
 }
}
