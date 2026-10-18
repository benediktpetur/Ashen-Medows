using Godot;
using System;
public partial class MigrationScene {
 void MegaArt(Node3D root,int variant){
  var torso=root.GetNode<Node3D>("Torso");var head=torso.GetNode<Node3D>("Head");
  foreach(var mesh in torso.GetChildren())if(mesh is MeshInstance3D m)m.Visible=false;
  Color skin=new(.39f,.43f,.30f),plate=new(.18f,.23f,.24f),bone=new(.70f,.64f,.42f),rust=new(.44f,.22f,.12f);
  if(variant==0){
   torso.Scale=new(2.0f,1.12f,1.7f);Ellipsoid(torso,new(0,.12f,0),new(.30f,.40f,.25f),skin);Ellipsoid(torso,new(0,-.07f,-.07f),new(.32f,.24f,.28f),new(.46f,.44f,.29f));
   for(int s=-1;s<=1;s+=2){Ellipsoid(torso,new(s*.24f,.32f,0),new(.15f,.18f,.22f),plate);for(int j=0;j<4;j++){var scab=Bevel(torso,new(s*.19f,.04f+j*.11f,-.21f),new(.18f,.09f,.09f),j%2==0?plate:rust);scab.RotationDegrees=new(0,s*22,s*12);}}
   Vector3[] points={new(-.22f,.27f,-.24f),new(.22f,.27f,-.24f),new(0,-.09f,-.29f)};
   for(int i=0;i<3;i++){var g=Ellipsoid(torso,points[i],new(.095f,.10f,.09f),new(.94f,.51f,.13f));g.Name="Growth"+i;Tube(torso,points[i]+new Vector3(0,.07f,0),points[i]+new Vector3(.02f,.13f,.03f),.016f,bone);}
   head.Scale=new(1.25f,1.12f,1.2f);Bevel(head,new(0,.08f,-.075f),new(.15f,.08f,.09f),plate);
   foreach(string side in new[]{"Left","Right"}){var a=root.GetNode<Node3D>(side+"Arm");a.Scale=new(1.6f,1.2f,1.6f);Ellipsoid(a,new(0,-.11f,.02f),new(.12f,.20f,.12f),plate);var hand=a.GetNode<Node3D>("Elbow/Hand");Bevel(hand,new(0,-.08f,-.04f),new(.20f,.23f,.20f),rust);}
  }else if(variant==2){
   torso.Scale=new(1.65f,1.1f,1.3f);Bevel(torso,new(0,.18f,-.07f),new(.43f,.48f,.26f),plate);Bevel(torso,new(0,.18f,-.22f),new(.31f,.28f,.055f),new(.31f,.34f,.30f));
   for(int i=0;i<5;i++)Bevel(torso,new(-.14f+i*.07f,.05f,-.26f),new(.035f,.13f,.045f),bone);
   Bevel(torso,new(0,.19f,.19f),new(.30f,.40f,.24f),rust);for(int i=0;i<4;i++)Tube(torso,new(-.11f+i*.075f,.37f,.24f),new(-.11f+i*.075f,.05f,.24f),.025f,Steel);
   Bevel(head,new(0,.18f,.005f),new(.19f,.15f,.19f),plate);Bevel(head,new(0,.12f,-.09f),new(.17f,.07f,.035f),Ink);Box(head,new(0,.13f,-.111f),new(.12f,.012f,.012f),new(1,.40f,.13f));
   for(int s=-1;s<=1;s+=2){var arm=root.GetNode<Node3D>(s<0?"LeftArm":"RightArm");Bevel(arm,new(0,-.03f,0),new(.22f,.19f,.25f),plate);var elbow=arm.GetNode<Node3D>("Elbow");Bevel(elbow,new(0,-.12f,-.12f),new(.19f,.32f,.24f),plate);
    for(int b=0;b<3;b++){float x=(b-1)*.055f;Tube(elbow,new(x,-.12f,-.12f),new(x,-.12f,-.49f),.027f,Steel);Tube(elbow,new(x,-.12f,-.45f),new(x,-.12f,-.52f),.037f,Ink);}
    for(int i=0;i<6;i++)Bevel(arm,new(s*.12f,-.02f-i*.05f,.07f),new(.055f,.04f,.08f),bone);
   }
  }else{
   torso.Scale=new(1.6f,1.0f,1.5f);Ellipsoid(torso,new(0,.15f,.07f),new(.27f,.34f,.28f),new(.33f,.28f,.33f));Ellipsoid(torso,new(0,.23f,.29f),new(.28f,.32f,.24f),new(.26f,.22f,.28f));
   for(int s=-1;s<=1;s+=2)for(int i=0;i<4;i++){var from=new Vector3(s*.17f,.03f+i*.12f,.14f);var tip=new Vector3(s*(.40f+i*.035f),.16f+i*.15f,.29f);Tube(torso,from,tip,.035f,bone,.01f);Tube(torso,tip,tip+new Vector3(s*.06f,-.18f,-.22f),.012f,bone,.003f);}
   for(int i=0;i<7;i++){float x=(i%2==0?-1:1)*(.11f+(i%3)*.02f);Ellipsoid(torso,new(x,.03f+i*.065f,.42f),new(.075f,.085f,.075f),new(.68f,.31f,.30f));}
   head.Scale=new(.9f,1.25f,1.0f);for(int s=-1;s<=1;s+=2){Tube(head,new(s*.045f,.10f,-.05f),new(s*.12f,-.03f,-.13f),.025f,bone,.003f);var arm=root.GetNode<Node3D>(s<0?"LeftArm":"RightArm");arm.Scale=new(.8f,1.4f,.8f);var hand=arm.GetNode<Node3D>("Elbow/Hand");for(int j=0;j<3;j++)Tube(hand,new((j-1)*.025f,-.07f,-.02f),new((j-1)*.04f,-.25f,-.09f),.01f,bone,.002f);}
  }
  for(int side=-1;side<=1;side+=2){var knee=root.GetNode<Node3D>((side<0?"LeftLeg":"RightLeg")+"/Knee");
   if(variant!=1){Bevel(knee,new(0,-.12f,-.025f),new(variant==0?.22f:.17f,.30f,.18f),plate);Bevel(knee,new(0,-.44f,-.07f),new(variant==0?.24f:.18f,.14f,.32f),plate);}
   else{Tube(knee,new(0,-.1f,.02f),new(side*.08f,-.38f,-.13f),.035f,bone,.008f);}
  }
  if(variant==2)for(int side=-1;side<=1;side+=2){var mount=new Vector3(side*.30f,.19f,-.20f);Bevel(torso,mount,new(.18f,.18f,.25f),plate);for(int j=0;j<3;j++){var from=mount+new Vector3((j-1)*.045f,0,-.10f);Tube(torso,from,from+new Vector3(0,0,-.40f),.023f,Steel);Tube(torso,from+new Vector3(0,0,-.36f),from+new Vector3(0,0,-.44f),.030f,Ink);}Box(torso,mount+new Vector3(0,.10f,-.02f),new(.12f,.025f,.12f),new(.69f,.51f,.17f));}
  Color light=variant==0?new(.8f,.46f,.12f):variant==1?new(.7f,.24f,.3f):new(.28f,.55f,.7f);
  root.AddChild(new OmniLight3D{Position=new(0,1.5f,-.5f),LightColor=light,LightEnergy=.8f,OmniRange=5});
 }
 void MegaNestArt(Node3D root){
  Ellipsoid(root,new(0,.65f,0),new(.65f,.8f,.6f),new(.31f,.23f,.29f));
  for(int i=0;i<5;i++){float a=i*1.25f;Tube(root,new(0,.4f,0),new(Mathf.Sin(a),0,Mathf.Cos(a)),.08f,new(.43f,.39f,.22f),.015f);Ellipsoid(root,new(Mathf.Sin(a)*.35f,.65f,Mathf.Cos(a)*.35f),new(.15f,.20f,.15f),new(.73f,.36f,.20f));}
 }
 void PoseMega(Infected boss){
  if(!IsMega(boss.Id)||!boss.Visual.HasNode("Torso"))return;int v=MegaVariant(boss.Id);var root=boss.Visual;
  PoseFigure(root,(float)game.Clock*(v==0?1.2f:3),boss.BossState=="hunt"?.35f:0,true);
  foreach(string side in new[]{"Left","Right"}){var leg=root.GetNode<Node3D>(side+"Leg");var at=leg.Position;at.X=(side=="Left"?-1:1)*(v==0?.20f:v==2?.16f:.14f);leg.Position=at;}
  var torso=root.GetNode<Node3D>("Torso");torso.RotationDegrees=new(v==1?-24:-6,0,0);
  for(int s=-1;s<=1;s+=2){var arm=root.GetNode<Node3D>(s<0?"LeftArm":"RightArm");arm.Position=new(s*(v==0?.38f:.30f),1.28f,-.04f);arm.RotationDegrees=new(v==2?-65:boss.BossState.EndsWith("windup")?-110:-18,0,s*12);}
  if(v==0)for(int i=0;i<3;i++){bool broken=(boss.MegaShell&(1<<i))!=0;torso.GetNode<Node3D>("Growth"+i).Visible=!broken;if(boss.Body.GetNodeOrNull<CollisionShape3D>("GrowthCollider"+i) is {} collider)collider.SetDeferred("disabled",broken);}
 }
}
