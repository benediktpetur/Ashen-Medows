using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
public partial class MigrationScene {
 Node3D localPlayerVisual;
 MeshInstance3D localPlayerShadowBody;
 readonly List<GeometryInstance3D> localPlayerBodyCore=new();
 bool showPlayerBody=true;
 float localStride,localWalkPhase;
 void InitPlayerModel(){
  if(localPlayerVisual!=null&&GodotObject.IsInstanceValid(localPlayerVisual))localPlayerVisual.QueueFree();
  localPlayerVisual=new Node3D{Name="LocalPlayerModel"};player.AddChild(localPlayerVisual);ArtCharacter(localPlayerVisual,1,0,true);
  localPlayerBodyCore.Clear();
  // A fitted inner outfit closes the reconstruction's open seams from the eye view.
  foreach(string path in new[]{"LeftLeg","RightLeg"})foreach(var geometry in localPlayerVisual.GetNode<Node3D>(path).FindChildren("*","GeometryInstance3D",true,false).OfType<GeometryInstance3D>()){geometry.Visible=true;geometry.MaterialOverride=new StandardMaterial3D{VertexColorUseAsAlbedo=true,AlbedoColor=new(.63f,.66f,.65f),Roughness=.94f};geometry.CastShadow=GeometryInstance3D.ShadowCastingSetting.Off;localPlayerBodyCore.Add(geometry);}
  foreach(var geometry in localPlayerVisual.GetNode<Node3D>("Torso").GetChildren().OfType<GeometryInstance3D>())geometry.Visible=false;
  var rig=localPlayerVisual.GetNodeOrNull<HunyuanActorRig>("HunyuanModel");if(rig?.Skeleton==null)return;
  var source=rig.FindChildren("*","MeshInstance3D",true,false).OfType<MeshInstance3D>().FirstOrDefault();if(source==null)return;
  localPlayerShadowBody=source;source.CastShadow=GeometryInstance3D.ShadowCastingSetting.ShadowsOnly;
  UpdatePlayerModel(0,true);
 }
 void UpdatePlayerModel(double dt,bool force=false){
  if(coopExecuting||localPlayerVisual==null||!GodotObject.IsInstanceValid(localPlayerVisual))return;
  if(!paused||force){float speed=new Vector2(player.Velocity.X,player.Velocity.Z).Length();localStride=Mathf.Lerp(localStride,Math.Clamp(speed/4.8f,0,1.25f),(float)(1-Math.Exp(-dt*12)));localWalkPhase+=(float)dt*(speed>6?10:7)*Math.Clamp(speed/4.8f,.2f,1.4f);PoseFigure(localPlayerVisual,localWalkPhase,localStride,false);CrouchFigure(localPlayerVisual,game.Sneaking);localPlayerVisual.Position+=new Vector3(0,0,.20f);if(!player.IsOnFloor()&&Math.Abs(player.Velocity.Y)>.5f)foreach(string side in new[]{"Left","Right"})localPlayerVisual.GetNode<Node3D>(side+"Leg/Knee").Rotation=new(.28f,0,0);}
  if(localPlayerShadowBody!=null)localPlayerShadowBody.Visible=mountedBlueprint==null;foreach(var core in localPlayerBodyCore)core.Visible=showPlayerBody&&mountedBlueprint==null;
 }
 sealed partial class PlayerModelPreviewAnimator:Node {
  internal MigrationScene Scene;internal Node3D Visual;internal int Pose;float phase;
  public override void _Process(double dt){if(Scene==null||!GodotObject.IsInstanceValid(Visual))return;phase+=(float)dt*6;Scene.PoseFigure(Visual,phase,Pose==1?1:0,false);Scene.CrouchFigure(Visual,Pose==2);}
 }
 void ShowPlayerCharacter(){
  var b=Panel("YOUR CHARACTER / SURVIVOR");
  var container=new SubViewportContainer{CustomMinimumSize=new(880,370),Stretch=true};b.AddChild(container);
  var viewport=new SubViewport{Size=new(1100,460),OwnWorld3D=true,RenderTargetUpdateMode=SubViewport.UpdateMode.Always};container.AddChild(viewport);
  viewport.AddChild(new WorldEnvironment{Environment=new Godot.Environment{BackgroundMode=Godot.Environment.BGMode.Color,BackgroundColor=new(.075f,.095f,.105f),AmbientLightSource=Godot.Environment.AmbientSource.Color,AmbientLightColor=new(.85f,.88f,.90f),AmbientLightEnergy=.7f}});
  viewport.AddChild(new DirectionalLight3D{RotationDegrees=new(-30,-30,0),LightEnergy=1.4f});viewport.AddChild(new DirectionalLight3D{RotationDegrees=new(-15,140,0),LightEnergy=.7f,LightColor=new(.75f,.85f,1)});
  var model=new Node3D{Name="PlayerCharacterPreview"};viewport.AddChild(model);ArtCharacter(model,1,0,true);
  var eye=new Camera3D{Position=new(2.0f,1.1f,-3.8f),Projection=Camera3D.ProjectionType.Orthogonal,Size=2.05f,Current=true};viewport.AddChild(eye);eye.LookAt(new Vector3(0,.87f,0));
  var animator=new PlayerModelPreviewAnimator{Scene=this,Visual=model};viewport.AddChild(animator);
  var controls=new HBoxContainer();b.AddChild(controls);Button(controls,"TURN LEFT",()=>model.RotateY(-Mathf.Pi/6));Button(controls,"TURN RIGHT",()=>model.RotateY(Mathf.Pi/6));
  var pose=new OptionButton();pose.AddItem("Standing");pose.AddItem("Walking");pose.AddItem("Crouching");controls.AddChild(pose);pose.ItemSelected+=index=>animator.Pose=(int)index;
  Text(b,"Your survivor is visible in co-op and casts a full-body shadow. Look down during play to see your body and feet.");
  Button(b,"VIEW & HAND OPTIONS",ShowOptions);Button(b,"BACK",ShowPause);
 }
}
