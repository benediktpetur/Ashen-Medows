using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 void PlayerModelChecks(){
  MigrationChecks.Assert(localPlayerVisual?.HasNode("HunyuanModel/Skeleton")==true&&localPlayerShadowBody?.Skin!=null,"Local player has generated skinned body");
  var rig=localPlayerVisual.GetNode<HunyuanActorRig>("HunyuanModel");MigrationChecks.Assert(rig.Skeleton.GetBoneCount()==12,"Player body has animated skeleton");
  MigrationChecks.Assert(localPlayerLegs?.Skin==localPlayerShadowBody.Skin&&localPlayerLegs.Mesh.GetSurfaceCount()>0,"First-person legs share the player model's skin");
  MigrationChecks.Assert(localPlayerShadowBody.CastShadow==GeometryInstance3D.ShadowCastingSetting.ShadowsOnly,"Full player body casts a shadow without clipping the eye view");
  bool sneaking=game.Sneaking;var velocity=player.Velocity;bool body=showPlayerBody;int style=handStyle;int weapon=game.Craft.Weapon;
  try{
   game.Sneaking=false;player.Velocity=Vector3.Zero;UpdatePlayerModel(0,true);rig.UpdatePose();float standing=localPlayerVisual.Position.Y;
   game.Sneaking=true;UpdatePlayerModel(0,true);rig.UpdatePose();MigrationChecks.Assert(localPlayerVisual.Position.Y<standing-.1f,"Crouching lowers player body with feet planted");
   foreach(string side in new[]{"Left","Right"}){
    var hip=localPlayerVisual.GetNode<Node3D>(side+"Leg");
    var knee=hip.GetNode<Node3D>("Knee");
    var ankle=knee.ToGlobal(Vector3.Down*.48f);
    MigrationChecks.Assert(localPlayerVisual.ToLocal(knee.GlobalPosition).Z<hip.Position.Z&&
     localPlayerVisual.ToLocal(ankle).Z>localPlayerVisual.ToLocal(knee.GlobalPosition).Z,
     "Crouch puts the knee in front of the hip and folds the shin back / "+side);
   }
   showPlayerBody=false;UpdatePlayerModel(0,true);MigrationChecks.Assert(!localPlayerLegs.Visible,"Player body option hides local geometry");showPlayerBody=true;
   game.Craft.Weapon=0;for(int option=0;option<3;option++){handStyle=option;RebuildHeldView();var hand=rightGrip.GetNode<MeshInstance3D>("AnatomicalHand");MigrationChecks.Assert(hand.Mesh.GetAabb().Size.Length()<.3f&&hand.Mesh.SurfaceGetArrays(0)[(int)Mesh.ArrayType.Vertex].AsVector3Array().All(p=>p.IsFinite()),"Anatomical hand proportions and finite geometry / "+option);MigrationChecks.Assert(rightSleeve.Mesh is ArrayMesh&&rightSleeve.MaterialOverride is StandardMaterial3D{AlbedoTexture:not null},"Textured field jacket sleeves / "+option);}
   ShowPlayerCharacter();MigrationChecks.Assert(paused&&menu.FindChild("PlayerCharacterPreview",true,false)!=null,"Full player model preview is accessible");CloseMenu();
  }finally{game.Craft.Weapon=weapon;handStyle=style;game.Sneaking=sneaking;player.Velocity=velocity;showPlayerBody=body;RebuildHeldView();UpdatePlayerModel(0,true);}
  GD.Print("PASS / generated local player / animated first-person legs / crouch / visibility / three anatomical hand styles / character preview");
 }
 async void CapturePlayerModel(){try{
  CloseMenu();ClearEnemies();game.NoDiskSave=true;SetProcess(false);SetPhysicsProcess(false);heldPreview=true;handStyle=1;game.World.Changes.Time=900;
  Teleport(ExpeditionWorld.SpawnX,ExpeditionWorld.SpawnZ);player.Rotation=Vector3.Zero;camera.Position=new(0,StandingEye,0);pitch=0;camera.Rotation=Vector3.Zero;camera.Fov=75;aiming=axeEquipped=buildMode=false;player.Velocity=Vector3.Zero;
  foreach(int id in new[]{0,1,12,23,25}){game.Craft.Weapon=id;BuildHeldTool();heldClock=10;game.Handling.SinceShot=10;heldAimBlend=0;GunView(0,true);await ArtShot($"player-hands-{id:00}");}
  game.Craft.Weapon=0;BuildHeldTool();pitch=-1.3f;GunView(0,true);UpdatePlayerModel(0,true);localPlayerVisual.GetNode<HunyuanActorRig>("HunyuanModel").UpdatePose();await ArtShot("player-body-standing");
  game.Sneaking=true;camera.Position=new(0,CrouchingEye,0);UpdatePlayerModel(0,true);localPlayerVisual.GetNode<HunyuanActorRig>("HunyuanModel").UpdatePose();await ArtShot("player-body-crouching");game.Sneaking=false;
  ShowPlayerCharacter();await ArtShot("player-full-model");var preview=menu.FindChild("PlayerCharacterPreview",true,false);var animator=preview.GetParent().GetChildren().OfType<PlayerModelPreviewAnimator>().Single();animator.Pose=2;await ArtShot("player-full-model-crouched");ShowOptions();await ArtShot("player-view-options");
  GD.Print("PASS / new hands and local player GPU previews");GetTree().Quit();
 }catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
 void RunPlayerModelChecks(){try{game.NoDiskSave=true;SetPhysicsProcess(false);PlayerModelChecks();GetTree().Quit();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}
