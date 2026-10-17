using Godot;
using System;

public partial class MigrationScene {
 const float StandingHeight=1.75f,CrouchingHeight=1.1f,StandingEye=1.65f,CrouchingEye=1.0f;
 bool CanStand(CharacterBody3D body){
  var query=new PhysicsShapeQueryParameters3D{
   Shape=new CapsuleShape3D{Radius=.29f,Height=StandingHeight-.02f},
   Transform=new Transform3D(Basis.Identity,body.GlobalPosition+Vector3.Up*.885f),
   Exclude=new Godot.Collections.Array<Rid>{body.GetRid()},Margin=.001f};
  return GetWorld3D().DirectSpaceState.IntersectShape(query,1).Count==0;
 }
 bool SetCrouch(CharacterBody3D body,bool requested,bool checkHeadroom=true){
  var collision=body.GetChild<CollisionShape3D>(0);var capsule=(CapsuleShape3D)collision.Shape;
  bool crouched=requested||(checkHeadroom&&capsule.Height<1.5f&&!CanStand(body));
  capsule.Height=crouched?CrouchingHeight:StandingHeight;
  collision.Position=Vector3.Up*(crouched?.555f:.88f);
  return crouched;
 }
 void UpdateCrouch(double dt,bool requested){
  game.Sneaking=SetCrouch(player,requested);
  var eye=camera.Position;eye.Y=Mathf.MoveToward(eye.Y,game.Sneaking?CrouchingEye:StandingEye,(float)dt*5);camera.Position=eye;
 }
 void CrouchFigure(Node3D figure,bool crouched){
  figure.Position=Vector3.Zero;
  if(!figure.HasNode("LeftLeg/Knee"))return;
  var head=figure.GetNodeOrNull<Node3D>("Torso/Head");if(head!=null)head.Rotation=Vector3.Zero;
  if(!crouched)return; // PoseFigure restores the standing pose before this call.
  foreach(string side in new[]{"Left","Right"}){
   var leg=figure.GetNode<Node3D>(side+"Leg");
   leg.Rotation=new(-1.22f,0,side=="Left"?.04f:-.04f);
   leg.GetNode<Node3D>("Knee").Rotation=new(1.92f,0,0);
  }
  var left=figure.GetNode<Node3D>("LeftLeg");var right=figure.GetNode<Node3D>("RightLeg");
  float sole=Math.Min(left.Position.Y+FigureSoleHeight(left),right.Position.Y+FigureSoleHeight(right));
  figure.Position=Vector3.Down*sole;
  figure.GetNode<Node3D>("Torso").Rotation=new(-1.05f,0,0);
  if(head!=null)head.Rotation=new(1.05f,0,0);
  foreach(string side in new[]{"Left","Right"}){
   var arm=figure.GetNode<Node3D>(side+"Arm");arm.Position+=new Vector3(0,-.18f,-.2f);arm.Rotation=new(-.5f,0,side=="Left"?.08f:-.08f);
   arm.GetNode<Node3D>("Elbow").Rotation=new(.7f,0,0);
  }
 }
}
