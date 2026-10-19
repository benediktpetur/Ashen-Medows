using Godot;
using System;
using System.Linq;

public partial class MigrationScene {
 bool viewGunApplied;InkCrosshair weaponCrosshair;Label handlingLabel,ammoTypeLabel;
 readonly System.Collections.Generic.List<(Node3D Node,double Until)> bulletMarks=new();
 bool Scoped=>aiming&&!axeEquipped&&(game.Craft.Weapon is 4 or 5||game.PartsFor(game.Craft.Weapon).Any(p=>p.Slot=="Optic"));
 bool GunActorGrounded(CharacterBody3D body){var query=PhysicsRayQueryParameters3D.Create(body.Position+Vector3.Up*.08f,body.Position-Vector3.Up*.25f);query.Exclude=new(){body.GetRid()};return GetWorld3D().DirectSpaceState.IntersectRay(query).Count>0;}
 void TickGunplay(double dt,bool moving,bool sprint){
  double speed=new Vector2(player.Velocity.X,player.Velocity.Z).Length();
  bool hold=!paused&&Input.MouseMode==Input.MouseModeEnum.Captured&&(Input.IsPhysicalKeyPressed(Key.Shift)||coopCheck&&coopCheckHold);
  game.Handling.Tick(game,dt,speed,aiming&&!buildMode&&!axeEquipped&&!paused,hold,player.IsOnFloor());
  if(!paused&&!buildMode&&!axeEquipped&&(ExpeditionGame.Automatic(game.Craft.Weapon)||ExpeditionGame.IsPoweredSaw(game.Craft.Weapon))&&Input.MouseMode==Input.MouseModeEnum.Captured&&Input.IsMouseButtonPressed(MouseButton.Left)&&shotClock<=0&&reloadClock<=0)Shoot();
 }
 Vector3 GunDirection(int pellet){
  var basis=viewGunApplied&&!coopExecuting?player.GlobalBasis*Basis.FromEuler(new Vector3(pitch,0,0)):camera.GlobalBasis;
  var offset=game.Handling.ShotOffset(game,pellet);
  if(ExpeditionGame.Shotgun(game.Craft.Weapon)){offset.X+=(pellet%3-1)*2.1;offset.Y+=(pellet/3-.5)*2.1;}
  return(-basis.Z+basis.X*(float)Math.Tan(offset.X*Math.PI/180)+basis.Y*(float)Math.Tan(offset.Y*Math.PI/180)).Normalized();
 }
 void PredictGunShot(){if(ExpeditionGame.IsGun(game.Craft.Weapon))game.Handling.Shot(game.Craft.Weapon);}
 void GunView(double dt,bool force=false){
  if((testing||coopCheck)&&!force||paused||coopExecuting)return;
  var h=game.Handling;bool gun=ExpeditionGame.IsGun(game.Craft.Weapon)&&!axeEquipped&&!buildMode;
  viewGunApplied=gun;
  if(gun){var offset=h.AimOffset();double brake=game.PartFactor(game.Craft.Weapon,p=>p.Recoil);offset.X-=h.RecoilX*(1-brake);offset.Y-=h.RecoilY*(1-brake);camera.Rotation=new(pitch+(float)(offset.Y*Math.PI/180),(float)(-offset.X*Math.PI/180),0);float kick=(float)Math.Max(0,1-h.SinceShot/.16);weaponRoot.RotationDegrees=new(kick*6,0,kick*2);}
  else camera.Rotation=new(pitch,0,0);
  float blend=(float)(1-Math.Exp(-dt*14));camera.Fov=Mathf.Lerp(camera.Fov,Scoped?32:aiming?52:75,blend);weaponRoot.Position=weaponRoot.Position.Lerp(aiming?new Vector3(0,-.15f,-.65f):new Vector3(.23f,-.23f,-.65f),blend);
  if(weaponCrosshair!=null){weaponCrosshair.Scoped=Scoped;weaponCrosshair.Spread=gun?(float)Math.Min(55,h.Spread(game)*6):0;weaponCrosshair.QueueRedraw();}
  if(handlingLabel==null){handlingLabel=HudLabel(13);ammoTypeLabel=HudLabel(11);}
  var size=GetViewport().GetVisibleRect().Size;
  handlingLabel.Position=new(size.X*.5f-175,size.Y-240);
  handlingLabel.Text=gun&&aiming?h.HoldingBreath?$"BREATH HELD / {Math.Max(0,h.BreathLimit(game.Skills.Marksmanship.Level)-h.BreathTime):0.0}s / STAMINA {game.Stamina:0}":h.BreathCooldown>0?$"CATCH YOUR BREATH / {h.BreathCooldown:0.0}s":h.Speed>=.35?"STOP MOVING / SHIFT TO STEADY AIM":"HOLD SHIFT / STEADY AIM":game.Craft.Weapon==27?$"Q / FLAME EXHAUST / {(h.SpecialCooldown>0?$"{h.SpecialCooldown:0.0}s":"READY")} / 8 FUEL + 4 CELLS":"";
  ammoTypeLabel.Position=new(size.X-270,size.Y-48);ammoTypeLabel.Text=gun?ExpeditionGame.AmmoNames[game.Craft.AmmoClass].ToUpperInvariant():game.Craft.Weapon==27?$"ENERGY CELLS {game.Craft.Stock(4)} / ELECTRIC CHAIN":"";
 }
 void AmmoPackModel(Node3D root,int type){
  Color color=type==0?new(.45f,.38f,.19f):type==1?new(.25f,.37f,.25f):type==2?new(.33f,.37f,.43f):type==3?new(.56f,.19f,.13f):new(.12f,.45f,.55f);
  Bevel(root,new(0,.1f,0),new(.42f,.2f,.29f),color);
  Box(root,new(0,.13f,-.151f),new(.25f,.09f,.015f),new(.65f,.61f,.44f));
  for(int i=0;i<4;i++){
   float x=(i-1.5f)*.085f;Tube(root,new(x,.21f,0),new(x,type==3?.37f:.4f,0),type==3?.028f:.018f,type==4?new(.23f,.72f,.83f):type==3?color:new Color(.68f,.51f,.24f),type is 3 or 4?.018f:.006f);
  }
 }
 void BulletMark(Vector3 point,Vector3 normal){
  if(DisplayServer.GetName()=="headless")return;
  while(bulletMarks.Count>=100){bulletMarks[0].Node.QueueFree();bulletMarks.RemoveAt(0);}
  var mark=new MeshInstance3D{Position=point+normal*.012f,Mesh=new QuadMesh{Size=new(.035f,.035f)},MaterialOverride=new StandardMaterial3D{AlbedoColor=new(.055f,.045f,.035f),CullMode=BaseMaterial3D.CullModeEnum.Disabled,ShadingMode=BaseMaterial3D.ShadingModeEnum.Unshaded},CastShadow=GeometryInstance3D.ShadowCastingSetting.Off};AddChild(mark);
  mark.LookAt(mark.Position+normal,Math.Abs(normal.Dot(Vector3.Up))>.95?Vector3.Forward:Vector3.Up);bulletMarks.Add((mark,Time.GetTicksMsec()/1000.0+40));
  ImpactParticles(point,new(.57f,.52f,.41f),3,.02f);
 }
 void TickBulletMarks(){double now=Time.GetTicksMsec()/1000.0;for(int i=bulletMarks.Count-1;i>=0;i--)if(now>=bulletMarks[i].Until){bulletMarks[i].Node.QueueFree();bulletMarks.RemoveAt(i);}}
}
