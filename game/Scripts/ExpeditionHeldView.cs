using Godot;
using System;
using System.Linq;

public partial class MigrationScene {
 // Camera-space rig. Hands follow the grip; sleeves reach back below the viewport.
 enum HeldMotion { Pistol,Rifle,Shotgun,Bow,Crossbow,Knife,Spear,Club,Hammer,Sword,Saw,Flame,Stone }
 readonly record struct HeldPose(Vector3 Position,Vector3 Angles);
 readonly record struct HeldProfile(HeldMotion Motion,Vector3 Position,Vector3 Angles,Vector3 RightGrip,Vector3 LeftGrip,bool Support,float Kick);
 HeldProfile heldProfile;
 Node3D viewArms,rightGrip,leftGrip;
 MeshInstance3D rightSleeve,leftSleeve,rightCuff,leftCuff,bowStringTop,bowStringBottom;
 Node3D bowArrow;
 Vector3 leftGripRest,viewSway;
 float heldAimBlend;
 double heldClock,heldAttackAt=-10,heldAttackDuration=.7,heldReloadDuration,heldBowShotAt=-10;
 int heldAttackWeapon=-1;
 bool heldPreview;

 static HeldProfile ProfileFor(int id){
  var right=new Vector3(.035f,-.105f,.025f);var support=new Vector3(-.045f,-.06f,-.39f);
  HeldProfile P(HeldMotion motion,Vector3 pos,Vector3 angles,Vector3 r,Vector3 l,bool two,float kick=1)=>new(motion,pos,angles,r,l,two,kick);
  var result=id switch {
   0=>P(HeldMotion.Pistol,new(.20f,-.20f,-.46f),new(0,-3,0),right,new(-.04f,-.12f,.04f),true,.8f),
   1=>P(HeldMotion.Rifle,new(.22f,-.23f,-.46f),new(0,-3,0),right,support,true,1.2f),
   2=>P(HeldMotion.Rifle,new(.21f,-.23f,-.47f),new(0,-3,0),right,support,true,1.25f),
   3=>P(HeldMotion.Rifle,new(.22f,-.23f,-.44f),new(0,-3,0),right,support,true,.65f),
   4=>P(HeldMotion.Rifle,new(.22f,-.24f,-.47f),new(0,-3,0),right,new(-.045f,-.06f,-.43f),true,1.4f),
   5=>P(HeldMotion.Rifle,new(.21f,-.24f,-.47f),new(0,-3,0),right,support,true,1),
   7=>P(HeldMotion.Shotgun,new(.23f,-.24f,-.44f),new(0,-3,0),new(.04f,-.09f,-.07f),new(-.06f,-.07f,-.43f),true,1.8f),
   28=>P(HeldMotion.Pistol,new(.20f,-.21f,-.45f),new(0,-3,0),right,new(-.04f,-.12f,.04f),true,1.25f),
   29=>P(HeldMotion.Rifle,new(.21f,-.22f,-.44f),new(0,-3,0),right,new(-.045f,-.06f,-.28f),true,.5f),
   30=>P(HeldMotion.Shotgun,new(.23f,-.24f,-.46f),new(0,-3,0),new(.04f,-.09f,-.07f),new(-.06f,-.09f,-.49f),true,1.65f),
   31=>P(HeldMotion.Rifle,new(.23f,-.24f,-.48f),new(0,-3,0),right,new(-.05f,-.07f,-.47f),true,1.15f),
   34=>P(HeldMotion.Flame,new(.25f,-.25f,-.48f),new(0,-4,0),new(.04f,-.13f,.01f),new(-.08f,-.08f,-.34f),true,.18f),
   6 or 12 or 18 or 32=>P(HeldMotion.Bow,new(-.23f,-.16f,-.49f),new(0,4,-7),new(.13f,-.025f,.13f),new(-.035f,-.04f,-.35f),true,.25f),
   19=>P(HeldMotion.Crossbow,new(.23f,-.22f,-.45f),new(0,-3,0),new(.04f,-.12f,.04f),new(-.04f,-.09f,-.37f),true,.45f),
   8=>P(HeldMotion.Club,new(.27f,-.29f,-.47f),new(48,-12,-10),new(.035f,-.15f,-.025f),Vector3.Zero,false),
   9=>P(HeldMotion.Stone,new(.25f,-.27f,-.47f),new(18,-12,-8),new(.06f,-.015f,-.15f),Vector3.Zero,false),
   10=>P(HeldMotion.Club,new(.27f,-.29f,-.49f),new(48,-12,-12),new(.04f,-.13f,-.035f),Vector3.Zero,false),
   11=>P(HeldMotion.Spear,new(.26f,-.28f,-.40f),new(15,-8,-7),new(.04f,-.13f,-.10f),new(-.04f,-.07f,-.49f),true),
   13=>P(HeldMotion.Club,new(.27f,-.30f,-.49f),new(52,-12,-14),new(.045f,-.13f,-.06f),new(-.04f,-.08f,-.25f),true),
   14=>P(HeldMotion.Hammer,new(.28f,-.29f,-.47f),new(48,-12,-9),new(.035f,-.13f,-.04f),Vector3.Zero,false),
   15=>P(HeldMotion.Club,new(.27f,-.29f,-.48f),new(48,-12,-11),new(.04f,-.13f,-.06f),Vector3.Zero,false),
   16=>P(HeldMotion.Spear,new(.26f,-.28f,-.41f),new(15,-8,-7),new(.04f,-.13f,-.10f),new(-.04f,-.07f,-.49f),true),
   17=>P(HeldMotion.Saw,new(.26f,-.26f,-.46f),new(8,-8,-4),new(.075f,-.04f,.0f),new(-.14f,.13f,-.14f),true),
   20=>P(HeldMotion.Sword,new(.27f,-.29f,-.47f),new(48,-10,-12),new(.035f,-.13f,-.04f),Vector3.Zero,false),
   21=>P(HeldMotion.Hammer,new(.29f,-.32f,-.48f),new(48,-12,-14),new(.04f,-.13f,-.05f),new(-.04f,-.08f,-.31f),true),
   22=>P(HeldMotion.Knife,new(.24f,-.24f,-.43f),new(27,-14,-12),new(.035f,-.02f,.045f),Vector3.Zero,false),
   23=>P(HeldMotion.Knife,new(.24f,-.24f,-.44f),new(30,-12,-10),new(.035f,-.02f,.045f),Vector3.Zero,false),
   24=>P(HeldMotion.Sword,new(.28f,-.29f,-.49f),new(52,-12,-12),new(.035f,-.02f,.04f),Vector3.Zero,false),
   25=>P(HeldMotion.Sword,new(.28f,-.29f,-.50f),new(55,-12,-12),new(.035f,-.02f,.04f),Vector3.Zero,false),
   26=>P(HeldMotion.Sword,new(.28f,-.31f,-.51f),new(55,-12,-14),new(.035f,-.02f,.04f),new(-.035f,-.035f,.13f),true),
   27=>P(HeldMotion.Saw,new(.27f,-.28f,-.49f),new(13,-9,-5),new(.04f,-.02f,.04f),new(-.045f,.06f,.17f),true),
   33=>P(HeldMotion.Hammer,new(.29f,-.32f,-.50f),new(42,-10,-12),new(.04f,-.13f,-.06f),new(-.04f,-.08f,-.34f),true),
   _=>throw new ArgumentOutOfRangeException(nameof(id),id,"Missing first-person weapon profile")
  };
  return result with {Position=result.Position+new Vector3(0,0,-.13f)};
 }
 void RebuildHeldView(){
  heldViewKey="";
  heldProfile=ProfileFor(game.Craft.Weapon);
  if(buildMode)heldProfile=new(HeldMotion.Hammer,new(.27f,-.27f,-.43f),new(35,-12,-8),new(.035f,-.16f,-.06f),Vector3.Zero,false,1);
  else if(axeEquipped)heldProfile=new(HeldMotion.Hammer,new(.28f,-.29f,-.46f),new(45,-12,-10),new(.04f,-.15f,-.07f),new(-.04f,-.05f,-.35f),true,1);
  ClearChildren(weaponRoot);weaponRoot.Scale=Vector3.One;
  if(viewArms!=null){camera.RemoveChild(viewArms);viewArms.QueueFree();}
  viewArms=new Node3D{Name="FirstPersonSleeves"};camera.AddChild(viewArms);
  if(buildMode){Tube(weaponRoot,new(0,-.3f,.05f),new(0,.15f,-.35f),.035f,Wood);Bevel(weaponRoot,new(0,.15f,-.35f),new(.32f,.13f,.13f),Steel);Box(weaponRoot,new(-.18f,.15f,-.35f),new(.05f,.16f,.16f),Steel);}
  else if(axeEquipped)FieldHarvestToolArt(weaponRoot,game.Craft.HarvestTool,game.Craft.ToolLevels[game.Craft.HarvestTool]);
  else if(heldProfile.Motion==HeldMotion.Bow)FirstPersonBow();
  else if(heldProfile.Motion==HeldMotion.Crossbow)FirstPersonCrossbow();
  else {ItemWeapon(weaponRoot,game.Craft.Weapon,false);FittedWeaponArt(weaponRoot,game.Craft.Weapon);}
  rightGrip=BuildGrip("Right",heldProfile.RightGrip,false);leftGrip=heldProfile.Support?BuildGrip("Left",heldProfile.LeftGrip,true):null;leftGripRest=heldProfile.LeftGrip;
  rightSleeve=SleeveSegment(.055f,.030f);rightCuff=SleeveSegment(.033f,.029f,true);leftSleeve=leftGrip!=null?SleeveSegment(.052f,.030f):null;leftCuff=leftGrip!=null?SleeveSegment(.033f,.029f,true):null;
  foreach(var node in weaponRoot.FindChildren("*","GeometryInstance3D",true,false))if(node is GeometryInstance3D geometry){geometry.CastShadow=GeometryInstance3D.ShadowCastingSetting.Off;if(geometry.MaterialOverride is ShaderMaterial original){var material=(ShaderMaterial)original.Duplicate();var outline=new ShaderMaterial{Shader=GD.Load<Shader>("res://Shaders/character-outline.gdshader")};outline.SetShaderParameter("ink_width",.00065f);material.NextPass=outline;geometry.MaterialOverride=material;}}

  heldAttackAt=-10;heldAttackWeapon=-1;heldBowShotAt=-10;heldReloadDuration=0;viewSway=Vector3.Zero;heldAimBlend=0;
  weaponRoot.Position=heldProfile.Position;weaponRoot.RotationDegrees=heldProfile.Angles;UpdateHeldSleeves();
 }
 Node3D BuildGrip(string side,Vector3 at,bool left){
  var grip=new Node3D{Name=side+"Grip",Position=at};weaponRoot.AddChild(grip);
  bool shaft=heldProfile.Motion is HeldMotion.Knife or HeldMotion.Spear or HeldMotion.Club or HeldMotion.Hammer or HeldMotion.Sword or HeldMotion.Saw;
  bool support=left&&heldProfile.Motion is HeldMotion.Rifle or HeldMotion.Shotgun or HeldMotion.Crossbow or HeldMotion.Flame;
  grip.RotationDegrees=shaft||support?new(-70,0,0):Vector3.Zero;
  if(left&&heldProfile.Motion==HeldMotion.Saw&&game.Craft.Weapon==17)grip.RotationDegrees=new(0,0,90);
  if(heldProfile.Motion==HeldMotion.Bow)grip.RotationDegrees=left?Vector3.Zero:new(0,-15,10);
  // Closed leather gloves: continuous palm, separate curved fingers, covered wrists.
  FigureMesh(grip,"held-v1/hand/"+side+"/"+heldProfile.Motion,b=>{
   var leather=new Color(.29f,.255f,.20f);var seam=leather*.68f;
   Vector3 Mirror(Vector3 point)=>new(left?-point.X:point.X,point.Y,point.Z);
   b.Loft(leather,new[]{new Vector3(0,-.08f,.015f),new Vector3(0,-.04f,.008f),new Vector3(0,.005f,0),new Vector3(0,.035f,-.003f)},new[]{new Vector2(.024f,.025f),new Vector2(.032f,.027f),new Vector2(.035f,.027f),new Vector2(.028f,.023f)},16);
   for(int i=0;i<4;i++){float y=.026f-i*.022f;var a=new Vector3(-.018f,y,-.022f);var mid=new Vector3(-.043f,y,-.04f);var joint=new Vector3(-.061f,y-.005f,-.015f);var tip=new Vector3(-.052f,y-.008f,.008f);GripFinger(b,leather,Mirror(a),Mirror(mid),Mirror(joint),Mirror(tip));b.Stitch(seam,Mirror(a+new Vector3(0,.004f,0)),Mirror(mid+new Vector3(0,.004f,0)),.0012f);}
   GripFinger(b,leather,Mirror(new(.018f,.008f,.015f)),Mirror(new(.01f,.045f,.012f)),Mirror(new(-.021f,.048f,.005f)),Mirror(new(-.035f,.03f,-.008f)));
   b.Stitch(seam,Mirror(new(.023f,-.045f,.025f)),Mirror(new(.025f,.018f,.024f)),.0014f);
  });return grip;
 }
 Material SleeveMaterial(bool cuff){var material=(ShaderMaterial)Material(cuff?new Color(.26f,.28f,.24f):new Color(.38f,.40f,.32f)).Duplicate();var outline=new ShaderMaterial{Shader=GD.Load<Shader>("res://Shaders/character-outline.gdshader")};outline.SetShaderParameter("ink_width",.00065f);material.NextPass=outline;return material;}
 MeshInstance3D SleeveSegment(float bottom,float top,bool cuff=false){
  var mesh=new MeshInstance3D{Mesh=new CylinderMesh{Height=1,BottomRadius=bottom,TopRadius=top,RadialSegments=16},MaterialOverride=SleeveMaterial(cuff),CastShadow=GeometryInstance3D.ShadowCastingSetting.Off};viewArms.AddChild(mesh);return mesh;
 }
 static void ConnectSegment(MeshInstance3D mesh,Vector3 start,Vector3 end){var delta=end-start;float length=delta.Length();mesh.Position=(start+end)*.5f;mesh.Quaternion=new Quaternion(Vector3.Up,delta.Normalized());mesh.Scale=new(1,Math.Max(.001f,length),1);}
 void UpdateHeldSleeves(){
  if(viewArms==null||rightGrip==null)return;
  void Sleeve(Node3D grip,MeshInstance3D sleeve,MeshInstance3D cuff,int side){var wrist=camera.ToLocal(grip.ToGlobal(new Vector3(0,-.072f,.015f)));var elbow=new Vector3(side*.34f,-.61f,-.10f);var axis=(wrist-elbow).Normalized();ConnectSegment(sleeve,elbow,wrist-axis*.027f);ConnectSegment(cuff,wrist-axis*.044f,wrist+axis*.005f);}
  Sleeve(rightGrip,rightSleeve,rightCuff,1);if(leftGrip!=null)Sleeve(leftGrip,leftSleeve,leftCuff,-1);
  viewArms.Visible=weaponRoot.Visible&&!Scoped&&showFirstPersonHands;rightGrip.Visible=showFirstPersonHands;if(leftGrip!=null)leftGrip.Visible=showFirstPersonHands;
 }
 void FirstPersonBow(){
  int id=game.Craft.Weapon;float radius=id==12?.022f:.029f;Color limb=id is 6 or 12?Wood:Steel;
  for(int i=0;i<18;i++){float a=(i/18f-.5f)*2.5f,b=((i+1)/18f-.5f)*2.5f;Tube(weaponRoot,new(Mathf.Cos(a)*.24f-.22f,Mathf.Sin(a)*.65f,-.35f),new(Mathf.Cos(b)*.24f-.22f,Mathf.Sin(b)*.65f,-.35f),radius,limb);}
  for(int i=0;i<6;i++)Tube(weaponRoot,new(-.026f,-.065f+i*.022f,-.35f),new(.026f,-.065f+i*.022f,-.35f),.008f,Cloth);
  if(id==32)foreach(int sign in new[]{-1,1}){Tube(weaponRoot,new(-.145f,sign*.616f,-.38f),new(-.145f,sign*.616f,-.32f),.067f,Ink);Tube(weaponRoot,new(.07f,sign*.1f,-.35f),new(.07f,sign*.1f,-.58f),.012f,Steel);}
  bowStringTop=Tube(weaponRoot,Vector3.Zero,Vector3.Up,.003f,Cloth);bowStringBottom=Tube(weaponRoot,Vector3.Zero,Vector3.Up,.003f,Cloth);
  // Unit cylinders allow moving the nocking point without rebuilding geometry.
  bowStringTop.Mesh=new CylinderMesh{Height=1,TopRadius=.0025f,BottomRadius=.0025f,RadialSegments=8};bowStringBottom.Mesh=bowStringTop.Mesh;
  bowArrow=new Node3D{Name="NockedArrow"};weaponRoot.AddChild(bowArrow);Tube(bowArrow,Vector3.Zero,new(0,0,-.85f),.005f,Wood);Bevel(bowArrow,new(0,0,-.88f),new(.025f,.018f,.07f),Steel);for(int i=0;i<2;i++)Box(bowArrow,new(0,0,-.10f-i*.035f),new(.035f,.012f,.06f),Cloth);
 }
 void FirstPersonCrossbow(){
  Bevel(weaponRoot,new(0,-.025f,.18f),new(.065f,.12f,.28f),Wood);Bevel(weaponRoot,new(0,0,-.20f),new(.10f,.09f,.55f),Steel);Bevel(weaponRoot,new(0,-.115f,.025f),new(.045f,.16f,.075f),Wood);
  for(int side=-1;side<=1;side+=2){Tube(weaponRoot,new(0,.015f,-.43f),new(side*.18f,.02f,-.50f),.025f,Steel);Tube(weaponRoot,new(side*.18f,.02f,-.50f),new(side*.35f,.02f,-.41f),.021f,Steel);Tube(weaponRoot,new(side*.35f,.02f,-.41f),new(0,.025f,-.10f),.003f,Cloth);}
  Bevel(weaponRoot,new(0,-.06f,-.35f),new(.075f,.10f,.18f),Wood);Box(weaponRoot,new(0,.065f,-.40f),new(.01f,.025f,.02f),Ink);Box(weaponRoot,new(0,.065f,-.04f),new(.04f,.02f,.025f),Ink);
  bowArrow=new Node3D{Name="LoadedBolt",Position=new(0,.055f,-.09f)};weaponRoot.AddChild(bowArrow);Tube(bowArrow,Vector3.Zero,new(0,0,-.60f),.005f,Wood);Bevel(bowArrow,new(0,0,-.62f),new(.025f,.014f,.06f),Steel);
 }
 void StartHeldAttack(){if(coopExecuting)return;heldAttackAt=heldClock;heldAttackWeapon=game.Craft.Weapon;heldAttackDuration=Math.Clamp(game.SoulMeleeDelay,.22,1.6);}
 void BowViewShot(){if(!coopExecuting)heldBowShotAt=heldClock;}
 static float Ease(float t){t=Math.Clamp(t,0,1);return t*t*(3-2*t);}
 static HeldPose BlendPose(HeldPose a,HeldPose b,float t)=>new(a.Position.Lerp(b.Position,Ease(t)),a.Angles.Lerp(b.Angles,Ease(t)));
 static HeldPose AttackPose(HeldProfile profile,float seconds,float duration,float contact){
  var idle=new HeldPose(profile.Position,profile.Angles);if(seconds<0||seconds>=duration)return idle;
  bool thrust=profile.Motion is HeldMotion.Spear or HeldMotion.Knife or HeldMotion.Stone;
  if(profile.Motion==HeldMotion.Saw){float pulse=seconds<=contact?Ease(seconds/contact):1-Ease((seconds-contact)/Math.Max(.01f,duration-contact));return new(profile.Position+new Vector3(0,.006f*pulse,-.075f*pulse),profile.Angles+new Vector3(1.5f*pulse,0,0));}
  var wind=thrust?new HeldPose(profile.Position+new Vector3(.045f,.005f,.07f),profile.Angles+new Vector3(7,-8,8)):new HeldPose(profile.Position+new Vector3(.07f,.025f,.025f),new(75,-28,14));
  var hit=thrust?new HeldPose(profile.Position+new Vector3(-.10f,.045f,-.27f),new(0,0,-4)):new HeldPose(profile.Position+new Vector3(-.12f,.015f,-.10f),new(8,14,-30));
  var follow=thrust?new HeldPose(profile.Position+new Vector3(-.09f,.03f,-.24f),new(-2,2,-6)):new HeldPose(profile.Position+new Vector3(-.17f,-.06f,-.08f),new(-13,25,-42));
  if(profile.Motion==HeldMotion.Hammer){wind=new(profile.Position+new Vector3(.04f,.065f,.035f),new(78,-8,6));hit=new(profile.Position+new Vector3(-.075f,-.015f,-.12f),new(-5,5,-7));follow=new(profile.Position+new Vector3(-.08f,-.10f,-.08f),new(-28,8,-12));}
  if(profile.Motion==HeldMotion.Sword){wind=new(profile.Position+new Vector3(.09f,.025f,.025f),new(62,-35,10));hit=new(profile.Position+new Vector3(-.14f,.025f,-.10f),new(12,22,-34));follow=new(profile.Position+new Vector3(-.20f,-.035f,-.065f),new(-7,38,-45));}
  if(profile.Motion==HeldMotion.Stone){wind=new(profile.Position+new Vector3(.035f,0,.055f),profile.Angles+new Vector3(0,-8,0));hit=new(profile.Position+new Vector3(-.12f,.065f,-.22f),new(8,2,-7));follow=new(hit.Position+new Vector3(0,-.02f,.02f),hit.Angles);}
  float windEnd=contact*.55f,followEnd=Math.Min(Math.Max(contact+.01f,duration-.02f),contact+.09f);
  if(seconds<windEnd)return BlendPose(idle,wind,seconds/windEnd);
  if(seconds<contact)return BlendPose(wind,hit,(seconds-windEnd)/(contact-windEnd));
  if(seconds<followEnd)return BlendPose(hit,follow,(seconds-contact)/(followEnd-contact));
  return BlendPose(follow,idle,(seconds-followEnd)/Math.Max(.01f,duration-followEnd));
 }
 void TickHeldView(double dt){
  if(coopExecuting||paused&&!heldPreview||testing&&!heldPreview||coopCheck&&!heldPreview)return;
  if(!heldPreview)heldClock+=dt;
  var profile=heldProfile;var pose=new HeldPose(profile.Position,profile.Angles);
  bool melee=!axeEquipped&&!buildMode&&ExpeditionGame.IsMelee(game.Craft.Weapon);
  if(melee&&heldAttackWeapon==game.Craft.Weapon)pose=AttackPose(profile,(float)(heldClock-heldAttackAt),(float)heldAttackDuration,game.Craft.Weapon==17?.06f:.19f);
  if(axeEquipped){float t=(float)(Time.GetTicksMsec()/1000.0-axeSwingStarted);pose=AttackPose(profile,t,.65f,.24f);}
  bool sight=aiming&&!axeEquipped&&!buildMode;
  heldAimBlend=Mathf.Lerp(heldAimBlend,sight?1:0,(float)(1-Math.Exp(-dt*13)));
  if(heldAimBlend>.001f&&!melee){var target=profile.Motion==HeldMotion.Bow?new Vector3(-.13f,-.12f,-.62f):new Vector3(0,Scoped?-.09f:-.07f,-.65f);if(profile.Motion==HeldMotion.Flame)target=new(.18f,-.21f,-.68f);pose=new(pose.Position.Lerp(target,heldAimBlend),pose.Angles.Lerp(Vector3.Zero,heldAimBlend));}
  bool gun=ExpeditionGame.IsGun(game.Craft.Weapon)&&!axeEquipped&&!buildMode;
  if(gun){float recoil=(float)Math.Exp(-game.Handling.SinceShot*22)*profile.Kick;pose=new(pose.Position+new Vector3(0,recoil*.007f,recoil*.035f),pose.Angles+new Vector3(recoil*3.8f,0,recoil*.6f));}
  if(reloadClock>0&&!axeEquipped&&!buildMode){heldReloadDuration=Math.Max(heldReloadDuration,reloadClock);float t=1-(float)(reloadClock/heldReloadDuration);float lower=Ease(Math.Min(t/.18f,(1-t)/.22f));pose=new(pose.Position+new Vector3(.04f,-.15f,.08f)*lower,pose.Angles+new Vector3(-12,-14,-24)*lower);if(leftGrip!=null)leftGrip.Position=leftGripRest+new Vector3(.04f,-.12f,.20f)*Mathf.Sin(t*Mathf.Pi);}else{heldReloadDuration=0;if(leftGrip!=null)leftGrip.Position=leftGripRest;}
  float speed=new Vector2(player.Velocity.X,player.Velocity.Z).Length();float moving=Math.Clamp(speed/4.8f,0,1);float bob=(sight?.001f:.004f)*moving*viewMotion;pose=new(pose.Position+new Vector3(Mathf.Sin((float)heldClock*7)*bob,Mathf.Cos((float)heldClock*14)*bob*.65f,0),pose.Angles);
  var mouse=Input.GetLastMouseVelocity();var targetSway=new Vector3(Math.Clamp(mouse.X*.000008f,-.012f,.012f),Math.Clamp(mouse.Y*.000008f,-.009f,.009f),0);viewSway=viewSway.Lerp(targetSway,(float)(1-Math.Exp(-dt*10)));pose=new(pose.Position-viewSway*(sight?.15f:1)*viewMotion,pose.Angles);
  if(speed>6&&!sight){pose=new(pose.Position+new Vector3(.025f,-.07f,.025f),pose.Angles+new Vector3(-8,6,7));}
  if(chainFlameVisual>0&&game.Craft.Weapon==27)pose=new(pose.Position+new Vector3(0,-.02f,-.07f),new(0,-5,-4));
  pose=pose with {Position=pose.Position+new Vector3(0,0,-viewDistance)};
  weaponRoot.Position=pose.Position;weaponRoot.RotationDegrees=pose.Angles;weaponRoot.Visible=!Scoped;
  if(profile.Motion==HeldMotion.Bow){float release=(float)(heldClock-heldBowShotAt);float draw=sight?.30f:.12f;float settle=release<0?1:Ease((release-.08f)/.28f);var nock=new Vector3(-.075f,0,-.35f+draw*settle);ConnectSegment(bowStringTop,new(-.145f,.616f,-.35f),nock);ConnectSegment(bowStringBottom,new(-.145f,-.616f,-.35f),nock);bowArrow.Position=nock;bowArrow.Visible=release<0||release>.20f;rightGrip.Position=new(-.015f,-.025f,nock.Z+.015f);}
  if(profile.Motion==HeldMotion.Crossbow)bowArrow.Visible=heldClock-heldBowShotAt>.20;
  UpdateHeldSleeves();
 }
}
