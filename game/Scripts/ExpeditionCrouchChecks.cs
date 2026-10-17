using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class MigrationScene {
 async Task CrouchChecks(){
  var old=game;var oldEye=camera.Transform;var roof=new StaticBody3D();
  try{
   game=new ExpeditionGame(ProjectSettings.GlobalizePath("user://tests/crouch-"+Guid.NewGuid()+".json")){NoDiskSave=true};
   ClearEnemies();player.Position=new(0,200,0);
   roof.Position=new(0,201.45f,-2);roof.AddChild(new CollisionShape3D{Shape=new BoxShape3D{Size=new(4,.3f,4)}});AddChild(roof);
   UpdateCrouch(1,true);
   MigrationChecks.Assert(game.Sneaking&&camera.Position.Y==CrouchingEye&&((CapsuleShape3D)player.GetChild<CollisionShape3D>(0).Shape).Height==CrouchingHeight,"Ctrl lowers viewpoint and physical capsule");
   await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
   var hit=player.MoveAndCollide(new Vector3(0,0,-2));
   MigrationChecks.Assert(hit==null&&player.Position.Z< -1.9f,"Crouched capsule fits below low ceiling");
   UpdateCrouch(1,false);MigrationChecks.Assert(game.Sneaking&&camera.Position.Y==CrouchingEye,"Cannot stand into a ceiling");
   player.Position=new(6,200,0);UpdateCrouch(1,false);
   MigrationChecks.Assert(!game.Sneaking&&camera.Position.Y==StandingEye,"Standing returns in clear space");
   player.Position=new(0,200,2);await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
   hit=player.MoveAndCollide(new Vector3(0,0,-4));MigrationChecks.Assert(hit!=null,"Standing capsule is blocked by same low passage");
   double walk=game.MovementSound(false),sprint=game.MovementSound(true);game.Sneaking=true;double crouch=game.MovementSound(false);
   MigrationChecks.Assert(crouch<walk&&walk<sprint&&game.MovementSound(false,2.4)>crouch&&game.MovementSound(false,1,.4)<crouch,"Speed tiers, vegetation and actual movement speed change noise");
   double z=-650,x=game.World.RouteX(z)+80;player.Position=Ground(x,z);
   MigrationChecks.Assert(FootstepSurface(player.Position).Name=="WOODLAND","Forest floor has leaf litter noise");
   StreamTerrain(true);var pair=chunks.First(p=>p.Value.HasMeta("noise_brush")&&p.Value.GetMeta("noise_brush").AsVector3Array().Length>0);
   var bushPoint=pair.Value.GetMeta("noise_brush").AsVector3Array()[0];player.Position=bushPoint;
   MigrationChecks.Assert(FootstepSurface(player.Position).Name=="BUSH"&&FootstepSurface(player.Position).Multiplier>1.5,"Actual rendered bush volume adds rustle noise");
   var positions=BrushPositions(pair.Key.Item1,pair.Key.Item2);
   MigrationChecks.Assert(positions.SequenceEqual(BrushPositions(pair.Key.Item1,pair.Key.Item2)),"Foliage positions are repeatable");
   player.Position=bushPoint+Vector3.Up*3;MigrationChecks.Assert(FootstepSurface(player.Position).Name=="FLOOR","Raised floors do not trigger bushes below them");
   GD.Print("PASS / physical crouch, low-passage collision, blocked standing, camera height, speed tiers, woodland and visible bush noise");
  }finally{RemoveChild(roof);roof.QueueFree();SetCrouch(player,false,false);game=old;camera.Transform=oldEye;Teleport(0,12);}
 }
 async void CaptureCrouch(){try{
  await CrouchChecks();
  ClearEnemies();double z=-650;Teleport(game.World.RouteX(z)+80,z);StreamTerrain(true);game.World.Changes.Time=900;
  var point=chunks.Values.Where(n=>n.HasMeta("noise_brush")).SelectMany(n=>n.GetMeta("noise_brush").AsVector3Array()).OrderBy(p=>Distance(p,player.Position)).First();
  player.Position=point+new Vector3(0,.1f,2);camera.Position=new(0,StandingEye,0);camera.Rotation=new(-.12f,0,0);game.FootstepGround="WOODLAND";game.MovementMode="MID";game.NoiseLevel=35;
  await ArtShot("crouch-01-standing");UpdateCrouch(1,true);player.Position=point+Vector3.Up*.1f;game.FootstepGround="BUSH";game.MovementMode="LOW";game.NoiseLevel=16;
  await ArtShot("crouch-02-in-bush");
  var stage=new Node3D{Position=point+Vector3.Up*18};worldRoot.AddChild(stage);Box(stage,new(0,-.1f,0),new(10,.2f,8),new Color(.3f,.32f,.27f),true);var rig=new Node3D();stage.AddChild(rig);ArtCharacter(rig,1,0,true);PoseFigure(rig,0,0,false);CrouchFigure(rig,true);
  camera.GlobalPosition=rig.GlobalPosition+new Vector3(2,1.5f,-2.5f);camera.LookAt(rig.GlobalPosition+Vector3.Up*.65f);weaponRoot.Visible=false;await ArtShot("crouch-03-character");GetTree().Quit();
 }catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}

}
