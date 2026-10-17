using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
public partial class MigrationScene {
 async Task DogPackChecks(){
  var original=game;var location=player.Position;Node3D floor=null;
  try{
   ClearEnemies();game=new ExpeditionGame(ProjectSettings.GlobalizePath("user://tests/dogs-"+Guid.NewGuid()+".json")){NoDiskSave=true};
   MigrationChecks.Assert(DogPackSize(2,0)==3&&DogPackSize(3,0)==4&&DogPackSize(4,0)==5,"Pack tier sizes");
   floor=new Node3D();AddChild(floor);Box(floor,new(0,199.8f,0),new(30,.4f,30),Wood,true);player.Position=new(0,200,0);game.Shield=0;
   for(int i=0;i<4;i++){AddEnemy("fauna/dog/check/pack/"+i,0,7,2);var dog=infected[^1];dog.Body.Position=new(i*2,200,-7);dog.Home=dog.Body.Position;}
   var leader=infected[0];var outsider=infected[3];outsider.Id="fauna/dog/check/other/0";
   await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
   AlertDogPack(leader,new(1,200,2));MigrationChecks.Assert(infected.Take(3).All(e=>e.Memory==9&&e.Target==new Vector3(1,200,2))&&outsider.Memory==0,"Pack alert isolation");
   foreach(var dog in infected)dog.Memory=0;HearNoise(new(0,200,-5),20);MigrationChecks.Assert(leader.Memory>0,"Dogs investigate world noise");
   leader.Body.Position=new(0,200,-2);leader.Body.Rotation=new(0,Mathf.Pi,0);leader.Cooldown=0;
   TickDog(leader,.016);MigrationChecks.Assert(leader.AttackState=="dog-windup","Dog warns before lunging");
   var second=infected[1];second.Body.Position=new(1,200,-2);second.Body.Rotation=new(0,Mathf.Pi,0);TickDog(second,.016);MigrationChecks.Assert(second.AttackState=="","Pack prevents simultaneous lunges");
   double health=game.Life.Health;player.Position=new(6,200,0);TickDog(leader,.6);TickDog(leader,.3);MigrationChecks.Assert(game.Life.Health==health&&leader.AttackState=="dog-recover","Sidestep evades committed lunge");
   leader.Body.Position=new(0,200,-.6f);player.Position=new(0,200,0);leader.AttackState="dog-lunge";leader.AttackTime=.01;TickDog(leader,.02);MigrationChecks.Assert(game.Life.Health<health&&game.Life.Wounds.Count==1,"Physical dog bite");
   leader.AttackState="dog-lunge";leader.AttackTime=.01;TickDog(leader,.02);MigrationChecks.Assert(game.Life.Wounds.Count==1,"Pack bites do not stack unavoidable wound timers");
   MigrationChecks.Assert(leader.Visual.HasNode("DogJaw")&&leader.Visual.HasNode("Leg3")&&DogCue(false).Data.Length>1000,"Hound geometry and sound");
   int food=game.Life.Food;AnimalLoot(leader);AnimalLoot(leader);MigrationChecks.Assert(game.Life.Food==food,"Infected dogs never yield edible meat");
   GD.Print("PASS / zombie dog packs: tier sizes, alert isolation, noise investigation, staggered warning/lunge, dodge, bite/wound cap, mesh/audio and contaminated loot");
  }finally{ClearEnemies();floor?.QueueFree();game=original;player.Position=location;}
 }
 async void RunDogChecks(){try{await DogPackChecks();GetTree().Quit();}catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}}
 async void CaptureDogPack(){try{
  CloseMenu();ClearEnemies();game.World.Changes.Time=900;Teleport(ExpeditionWorld.SpawnX,ExpeditionWorld.SpawnZ);weaponRoot.Visible=false;
  var at=player.Position+new Vector3(0,0,-6);
  for(int i=0;i<5;i++){var rig=new Node3D{Position=Ground(at.X+(i-2)*1.2,at.Z+Math.Abs(i-2)*.6)};AddChild(rig);AnimalArt(rig,"dog");rig.RotationDegrees=new(0,180+(i-2)*12,0);}
  camera.GlobalPosition=at+new Vector3(3,1.3f,4);camera.LookAt(at+Vector3.Up*.4f);await ArtShot("zombie-dog-pack");GetTree().Quit();
 }catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}}
}
