using Godot;
using System;
public partial class MigrationScene {
 async void CaptureCharacterRebuild(){try{
  testing=true;CloseMenu();paused=true;hudRoot.Visible=false;terrainRoot.Visible=worldRoot.Visible=buildRoot.Visible=false;if(distantTerrain!=null)distantTerrain.Visible=false;ClearEnemies();weaponRoot.Visible=false;game.World.Changes.Time=900;
  var stage=new Node3D();AddChild(stage);Box(stage,new Vector3(0,-.08f,0),new Vector3(12,.16f,8),new Color(.22f,.25f,.23f));
  var survivor=new Node3D{Position=new Vector3(-.64f,0,0)};stage.AddChild(survivor);ArtCharacter(survivor,1,0,true);
  var zombie=new Node3D{Position=new Vector3(.64f,0,0),RotationDegrees=new Vector3(0,-8,0)};stage.AddChild(zombie);ArtCharacter(zombie,2,0);
  var fill=new DirectionalLight3D{RotationDegrees=new Vector3(-25,145,0),LightColor=new Color(.75f,.80f,.85f),LightEnergy=.7f};stage.AddChild(fill);
  camera.Fov=32;camera.GlobalPosition=new Vector3(1.2f,1.55f,-4.8f);camera.LookAt(new Vector3(0,1,0));await ArtShot("character-rebuild-01-full-models");PoseFigure(survivor,1.1f,1,false);PoseFigure(zombie,2.3f,1,true);await ArtShot("character-finetune-05-walking-pose");PoseFigure(survivor,.7f,0,false);PoseFigure(zombie,.7f,0,true);
  survivor.GetNode<Node3D>("LeftArm/Elbow").Rotation=new Vector3(Mathf.Pi/2,0,0);survivor.GetNode<Node3D>("RightArm/Elbow").Rotation=new Vector3(Mathf.Pi/2,0,0);camera.GlobalPosition=new Vector3(-2.4f,1.3f,-2.3f);camera.LookAt(survivor.GlobalPosition+new Vector3(0,1.15f,0));await ArtShot("character-human-09-elbow-flexion");PoseFigure(survivor,0,0,false);zombie.Visible=false;camera.Fov=38;camera.GlobalPosition=new Vector3(-.43f,1.71f,-.72f);camera.LookAt(new Vector3(-.64f,1.68f,0));await ArtShot("character-rebuild-02-face");var handNode=survivor.GetNode<Node3D>("LeftArm/Elbow/Hand");survivor.GetNode<Node3D>("LeftArm").Rotation=new Vector3(0,0,-.4f);handNode.Rotation=Vector3.Zero;var wrist=handNode.GlobalPosition;camera.Fov=32;camera.GlobalPosition=wrist+new Vector3(.035f,.00f,-.45f);camera.LookAt(wrist+new Vector3(0,-.08f,-.015f));await ArtShot("character-anatomy-06-hand-detail");handNode.Rotation=new Vector3(0,-Mathf.Pi/2,0);camera.Fov=42;camera.GlobalPosition=new Vector3(1.4f,1.2f,-1.7f);camera.LookAt(survivor.GlobalPosition+new Vector3(0,1,0));await ArtShot("character-anatomy-07-body-profile");
  stage.QueueFree();terrainRoot.Visible=worldRoot.Visible=buildRoot.Visible=true;if(distantTerrain!=null)distantTerrain.Visible=true;paused=false;weaponRoot.Visible=true;Teleport(0,12);camera.Position=new Vector3(0,1.65f,0);camera.Fov=65;camera.Rotation=Vector3.Zero;player.Rotation=Vector3.Zero;AddEnemy("character/field",-1.2,-.5,2);infected[0].Body.Rotation=new Vector3(0,Mathf.Pi,0);await ArtShot("character-rebuild-03-in-game");game.Craft.Weapon=0;BuildGun();await ArtShot("character-anatomy-08-pistol-grip");game.Craft.Weapon=1;BuildGun();ShowEquipment();await ArtShot("character-rebuild-04-loadout");GD.Print("PASS / rebuilt character GPU captures");GetTree().Quit();
 }catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}


