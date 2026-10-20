using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 double KennelX=>game.World.RouteX(-610)+95;const double KennelZ=-610;
 bool KennelClear=>Dog.KennelGuards==7;
 string KennelGuardId(int i)=>$"story/kennel/{game.Save.Seed}/{i}";
 void KennelGuardKilled(string id){for(int i=0;i<3;i++)if(id==KennelGuardId(i)){Dog.KennelGuards|=1<<i;game.Persist();break;}}
 void SpawnDogRescue(){if(dungeon!=null)return;if(!CoopClient&&!Dog.Rescued&&!Dog.Dead&&Dog.RescueVersion<1){Dog.Initialized=true;Dog.RescueVersion=1;Dog.X=KennelX;Dog.Z=KennelZ;Dog.Y=game.World.Height(Dog.X,Dog.Z);if(DogPresent){rescueDogActor.Body.QueueFree();rescueDogActor=null;}game.Persist();}
  if(Dog.Rescued){foreach(var p in props.Where(p=>p.Id=="story/kennel").ToArray()){p.Node.QueueFree();props.Remove(p);uniqueObjects.Remove(p.Id);}return;}
  if(Dog.Dead||!NearActiveSurface(Ground(KennelX,KennelZ),175))return;
  string id="story/kennel";if(uniqueObjects.Add(id)){var n=new Node3D{Name="WoodlandRescueKennel",Position=Ground(KennelX,KennelZ)};worldRoot.AddChild(n);foreach(int side in new[]{-1,1}){PhysicsObstacle(n,new BoxShape3D{Size=new(.1f,2.2f,4)},new(side*2,1.1f,0));for(int bar=0;bar<9;bar++)Box(n,new(side*2,1.1f,-2+bar*.5f),new(.04f,2.2f,.04f),Steel);PhysicsObstacle(n,new BoxShape3D{Size=new(1.5f,2.2f,.12f)},new(side*1.2f,1.1f,2));}Box(n,new(0,1.1f,-2),new(4,2.2f,.1f),Wood,true);Box(n,new(0,2.3f,0),new(4.5f,.15f,4.4f),Rust);PhysicsObstacle(n,new BoxShape3D{Size=new(.9f,2.2f,.1f)},new(0,1.1f,2));for(int i=0;i<8;i++)Box(n,new(-1.8f+i*.5f,1.1f,2.08f),new(.035f,2,.03f),new(.42f,.43f,.38f));Bevel(n,new(-.9f,.14f,.8f),new(.5f,.2f,.5f),Steel);n.AddChild(new Label3D{Text="MAINTENANCE KENNELS\nA DOG IS STILL ALIVE / CLEAR THE YARD",Position=new(0,2.9f,2.1f),FontSize=24,PixelSize=.006f});props.Add(new(){Id=id,Kind=40,Node=n,Position=n.Position});}
  if(CoopClient)return;for(int i=0;i<3;i++){string guard=KennelGuardId(i);if((Dog.KennelGuards&(1<<i))!=0||infected.Any(e=>e.Id==guard))continue;AddEnemy(guard,KennelX-5+i*5,KennelZ+8+i,2);}
 }
 void OpenRescueKennel(){foreach(var p in props.Where(p=>p.Id=="story/kennel").ToArray()){p.Node.QueueFree();props.Remove(p);uniqueObjects.Remove(p.Id);}Dog.Z+=3;Dog.Y=game.World.Height(Dog.X,Dog.Z);if(DogPresent)rescueDogActor.Body.Position=Ground(Dog.X,Dog.Z);}
}
