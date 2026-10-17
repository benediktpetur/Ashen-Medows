using Godot;
using System;
public partial class MigrationScene {
 async void CaptureGunplay(){try{
  CloseMenu();ClearEnemies();game.NoDiskSave=true;game.World.Changes.Time=900;
  hudRoot.Visible=false;weaponRoot.Visible=false;terrainRoot.Visible=worldRoot.Visible=buildRoot.Visible=false;if(distantTerrain!=null)distantTerrain.Visible=false;
  var stage=new Node3D();AddChild(stage);Box(stage,new(0,-.08f,0),new(13,.16f,5),new(.16f,.21f,.20f));
  stage.AddChild(new DirectionalLight3D{RotationDegrees=new(-40,140,0),LightEnergy=1.1f});
  for(int i=0;i<6;i++){var n=new Node3D{Position=new((2.5f-i)*1.8f,.85f,0),RotationDegrees=new(-20,30,-25)};stage.AddChild(n);BladeModel(n,22+i,false);stage.AddChild(new Label3D{Text="T"+ExpeditionGame.GunTier(22+i)+" / "+ExpeditionGame.GunNames[22+i],Position=new((2.5f-i)*1.8f,.15f,-.8f),FontSize=26,PixelSize=.0026f,Billboard=BaseMaterial3D.BillboardModeEnum.Enabled});}
  camera.Fov=48;camera.GlobalPosition=new(0,5,-10);camera.LookAt(new Vector3(0,.5f,0));await ArtShot("gunplay-01-blades");stage.QueueFree();
  terrainRoot.Visible=worldRoot.Visible=buildRoot.Visible=true;if(distantTerrain!=null)distantTerrain.Visible=true;hudRoot.Visible=weaponRoot.Visible=true;
  Teleport(ExpeditionWorld.SpawnX,ExpeditionWorld.SpawnZ);game.Craft.CommitAmmo();game.Craft.Weapon=4;game.Craft.LoadAmmo();BuildGun();camera.Position=new(0,1.65f,0);player.Rotation=Vector3.Zero;pitch=0;camera.Rotation=Vector3.Zero;aiming=true;game.Handling.Tick(game,.1,0,true,true);GunView(1,true);await ArtShot("gunplay-02-steady-scope");
  game.Craft.CommitAmmo();game.Craft.Weapon=27;game.Craft.LoadAmmo();game.Craft.Fuel=40;game.Craft.GiveAmmo(4,40);BuildGun();aiming=false;GunView(1,true);StartChainFlame();DrawChainEffects(.01);await ArtShot("gunplay-03-flame-exhaust");
  GD.Print("PASS / gunplay GPU captures");GetTree().Quit();
 }catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}
