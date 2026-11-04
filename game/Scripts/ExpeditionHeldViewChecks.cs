using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
public partial class MigrationScene {
 void HeldViewChecks(){
  int equipped=game.Craft.Weapon;bool oldAxe=axeEquipped,oldBuild=buildMode,oldAim=aiming;
  try{
   axeEquipped=buildMode=aiming=false;
   for(int id=0;id<ExpeditionGame.GunNames.Length;id++){
    game.Craft.Weapon=id;RebuildHeldView();var profile=ProfileFor(id);
    MigrationChecks.Assert(rightGrip!=null&&(leftGrip!=null)==profile.Support,"Weapon-specific grip / "+ExpeditionGame.GunNames[id]);
    MigrationChecks.Assert(weaponRoot.FindChildren("*","GeometryInstance3D",true,false).Count>0,"First-person geometry / "+id);
    if(ExpeditionGame.IsMelee(id)){
     float duration=(float)ExpeditionGame.WeaponDelay(id),contact=id==17?.06f:.19f;
     var idle=AttackPose(profile,0,duration,contact);var impact=AttackPose(profile,contact,duration,contact);var end=AttackPose(profile,duration,duration,contact);
     MigrationChecks.Assert(idle.Position.IsEqualApprox(end.Position)&&idle.Angles.IsEqualApprox(end.Angles),"Attack recovers to idle / "+id);
     MigrationChecks.Assert(impact.Position.DistanceTo(idle.Position)>.01,"Attack has forward travel / "+id);
     for(float t=0;t<duration;t+=.005f){var a=AttackPose(profile,t,duration,contact);var b=AttackPose(profile,t+.005f,duration,contact);MigrationChecks.Assert(a.Position.IsFinite()&&a.Angles.IsFinite()&&a.Position.DistanceTo(b.Position)<.065f&&a.Angles.DistanceTo(b.Angles)<16,"Continuous attack pose / "+id);}
    }
   }
   game.Craft.Weapon=23;RebuildHeldView();shotClock=reloadClock=0;double stamina=game.Stamina;game.Stamina=0;heldAttackAt=-10;BeginMelee();MigrationChecks.Assert(heldAttackAt==-10,"Rejected melee does not animate");game.Stamina=stamina;
   RebuildHeldView();BuildHeldTool();StartHeldAttack();double active=heldAttackAt;var grip=rightGrip;BuildHeldTool();MigrationChecks.Assert(rightGrip==grip&&heldAttackAt==active,"Repeated co-op profile refresh preserves grip and animation");
   GD.Print("PASS / all 35 first-person weapons / grip layouts / continuous attacks / contact travel / recovery / rejected attacks");
  }finally{game.Craft.Weapon=equipped;axeEquipped=oldAxe;buildMode=oldBuild;aiming=oldAim;RebuildHeldView();}
 }
 async void CaptureHeldView(){try{
  CloseMenu();ClearEnemies();game.NoDiskSave=true;game.World.Changes.Time=900;SetProcess(false);SetPhysicsProcess(false);heldPreview=true;
  Teleport(ExpeditionWorld.SpawnX,ExpeditionWorld.SpawnZ);player.Rotation=Vector3.Zero;camera.Position=new(0,StandingEye,0);camera.Rotation=Vector3.Zero;pitch=0;camera.Fov=75;
  hudRoot.Visible=false;axeEquipped=buildMode=aiming=false;player.Velocity=Vector3.Zero;
  async Task Shot(string name){heldAimBlend=aiming?1:0;TickHeldView(0);await ArtShot(name);}
  for(int id=0;id<ExpeditionGame.GunNames.Length;id++){
   game.Craft.Weapon=id;RebuildHeldView();heldClock=10;game.Handling.SinceShot=10;heldAttackAt=-10;reloadClock=0;aiming=false;camera.Fov=75;
   await Shot($"held-{id:00}-idle");
   if(ExpeditionGame.IsMelee(id)){
    heldAttackAt=10;heldAttackWeapon=id;heldAttackDuration=ExpeditionGame.WeaponDelay(id);
    heldClock=10.09;await Shot($"held-{id:00}-windup");heldClock=10+(id==17?.06:.19);await Shot($"held-{id:00}-contact");heldClock=10+Math.Max(.28,heldAttackDuration*.55);await Shot($"held-{id:00}-follow");
   }else{
    aiming=true;camera.Fov=Scoped?32:52;await Shot($"held-{id:00}-aim");aiming=false;camera.Fov=75;
    if(ExpeditionGame.IsGun(id)){game.Handling.SinceShot=.025;await Shot($"held-{id:00}-fire");game.Handling.SinceShot=10;heldReloadDuration=1.4;reloadClock=.7;await Shot($"held-{id:00}-reload");reloadClock=0;}
    else{heldBowShotAt=10;heldClock=10.10;await Shot($"held-{id:00}-release");}
   }
  }
  axeEquipped=true;for(int family=0;family<3;family++){game.Craft.HarvestTool=family;RebuildHeldView();await Shot($"held-tool-{family}-idle");}
  axeEquipped=false;buildMode=true;RebuildHeldView();await Shot("held-builder-idle");
  GD.Print("PASS / first-person GPU review / all weapons, attacks, aim, recoil, reload and tools");GetTree().Quit();
 }catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}
