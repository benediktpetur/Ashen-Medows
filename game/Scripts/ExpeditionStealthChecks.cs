using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class MigrationScene {
 async Task StealthChecks(){
  var original=game;var eye=camera.Transform;Node3D wall=null;
  try{
   game=new ExpeditionGame(ProjectSettings.GlobalizePath("user://tests/stealth-"+Guid.NewGuid()+".json")){NoDiskSave=true};
   game.Sneaking=true;double low=game.MovementSound(false),range=game.VisualRange;
   game.Skills.Sneaking.Level=100;
   MigrationChecks.Assert(game.MovementSound(false)<low&&game.VisualRange<range&&game.VisualRange>5,"Sneak levels reduce sound and detection without invisibility");
   game.TrainSneaking(.5,false,true);game.TrainSneaking(.5,true,false);
   MigrationChecks.Assert(game.Skills.Sneaking.Xp==0,"No idle or safe-zone sneak XP");
   game.Skills.Sneaking.Level=2;game.TrainSneaking(.5,true,true);
   MigrationChecks.Assert(game.Skills.Sneaking.Xp==1.5,"Sneak earns XP near unaware enemies");
   var saved=Json.Read<ExpeditionSurvivor>(Json.Write(game.Skills));saved.Normalize();saved.Die();
   MigrationChecks.Assert(saved.Sneaking.Level==2&&saved.Sneaking.Xp==0,"Sneak skill travels with character and loses only partial XP on death");
   saved.Sneaking=null;saved.Normalize();MigrationChecks.Assert(saved.Sneaking.Level==1,"Legacy character gains new sneak skill");
   double first=game.RaiseNoise(20),next=game.RaiseNoise(20);MigrationChecks.Assert(next>first,"Repeated actions carry farther");game.FadeNoise(100);MigrationChecks.Assert(game.NoiseLevel==0,"Noise fades completely");
   ClearEnemies();Teleport(0,12);AddEnemy("stealth/check",0,9,1);var enemy=infected.Last();
   player.Position=new(0,200,0);camera.Position=new(0,1.65f,0);enemy.Body.Position=new(0,200,-30);
   await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
   Noise(5);MigrationChecks.Assert(enemy.Memory==0,"Quiet actions do not alert distant zombies");
   Noise(55);MigrationChecks.Assert(enemy.Memory>0&&enemy.Target==player.Position,"Gun noise draws distant zombies to its origin");
   enemy.Memory=0;enemy.Body.Position=new(0,200,-15);
   wall=new Node3D();AddChild(wall);Box(wall,new(0,201,-7),new(5,4,1),new Color(.3f,.3f,.3f),true);
   await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
   HearNoise(player.Position,20);MigrationChecks.Assert(enemy.Memory==0,"Solid walls muffle noise");
   HearNoise(player.Position,80);MigrationChecks.Assert(enemy.Memory>0,"Loud sound still carries through walls");
   RemoveChild(wall);wall.QueueFree();wall=null;await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
   enemy.Memory=0;enemy.Body.Position=new(0,200,-10);enemy.Body.Rotation=Vector3.Zero;game.Sneaking=true;
   MigrationChecks.Assert(!Sight(enemy,10),"Sneaking out of peripheral vision avoids initial detection");
   enemy.Body.Position=new(0,200,-1);MigrationChecks.Assert(Sight(enemy,1),"Sneaking cannot hide a player at touching distance");
   HearNoise(new Vector3(20,200,0),60);MigrationChecks.Assert(enemy.Target==new Vector3(20,200,0),"World sound investigates its source, not player location");
   GD.Print("PASS / noise accumulation, decay, hearing distance, wall muffling, sound origin, sneak sight/progression, portable save and death");
  }finally{if(wall!=null){RemoveChild(wall);wall.QueueFree();}ClearEnemies();game=original;camera.Transform=eye;Teleport(0,12);}
 }
}
