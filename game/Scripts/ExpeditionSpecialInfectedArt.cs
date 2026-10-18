using Godot;
using System;
using System.Linq;

public partial class MigrationScene {
 void SpecialInfectedArt(Node3D root,string id,int tier,int night){
  string kind=ExpeditionCampaign.Kind(id,tier,night);if(ExpeditionThreat.Find(kind)!=null){NewThreatArt(root,kind,tier);return;}
  if(kind=="worm"){
   FigureMesh(root,"brood-worm-v1",b=>{for(int i=0;i<7;i++){float z=(i-3)*.09f;var color=i%2==0?new Color(.56f,.53f,.29f):new Color(.37f,.38f,.18f);b.Limb(color,new(0,.13f,z-.045f),new(0,.13f,z+.045f),.075f,.065f,.07f,.06f,12);}for(int s=-1;s<=1;s+=2)b.Limb(new Color(.25f,.12f,.08f),new(s*.025f,.14f,-.34f),new(s*.05f,.17f,-.42f),.015f,.015f,.002f,.002f,8);});return;
  }
  int model=kind=="bloater"?6:kind=="weeper"||kind=="stalker"?5:kind=="armored"?4:Math.Clamp(tier-1,0,4);
  HumanCharacter(root,tier,night,false,model);
  if(kind=="bloater"){
   root.Scale=new(1.25f,1.14f,1.25f);var torso=root.GetNode<Node3D>("Torso");torso.Scale=new(1.35f,1,1.3f);
   FigureMesh(torso,"brood-bloater-abdomen",b=>b.Loft(new Color(.44f,.46f,.29f),new[]{new Vector3(0,-.12f,-.06f),new Vector3(0,.06f,-.08f),new Vector3(0,.22f,-.03f)},new[]{new Vector2(.24f,.20f),new Vector2(.34f,.30f),new Vector2(.24f,.18f)},24));
   FigureMesh(torso,"brood-bloater-sacs",b=>{for(int i=0;i<6;i++){float a=i*2.4f;b.Limb(new Color(.43f,.52f,.19f),new(Mathf.Sin(a)*.18f,.05f+i*.04f,-.19f),new(Mathf.Sin(a)*.18f,.09f+i*.04f,-.25f),.055f,.06f,.04f,.05f,12);}});
  }else if(kind=="weeper"){
   root.Scale=new(1.05f,1.1f,1.05f);
   foreach(string side in new[]{"Left","Right"}){
    var hand=root.GetNode<Node3D>(side+"Arm/Elbow/Hand");
    FigureMesh(hand,"weeper-claws/"+side,b=>{for(int i=0;i<4;i++)b.Limb(new Color(.65f,.62f,.39f),new((i-1.5f)*.025f,-.07f,-.02f),new((i-1.5f)*.04f,-.43f,-.10f),.017f,.016f,.003f,.003f,10);});
   }
  }else if(kind=="runner"){root.Scale=new(.85f,1,.85f);root.GetNode<Node3D>("Torso").RotationDegrees=new(-18,0,0);}
  else if(kind=="bruiser"){root.Scale=new(1.4f,1.17f,1.3f);root.GetNode<Node3D>("Torso").Scale=new(1.2f,1,1.1f);}
  // Area clothing remains visible even on specialist silhouettes.
  if(tier==3&&kind!="weeper")Bevel(root.GetNode<Node3D>("Torso"),new(0,.24f,-.18f),new(.26f,.08f,.025f),new(.73f,.44f,.12f));
  if(tier>=5)for(int s=-1;s<=1;s+=2)Ellipsoid(root.GetNode<Node3D>("Torso"),new(s*.17f,.27f,-.12f),new(.045f,.06f,.035f),new(.27f,.61f,.41f));
 }
 void PoseWeeper(Infected e){
  if(!e.Visual.HasNode("Torso"))return;
  PoseFigure(e.Visual,(float)game.Clock*6,e.BossState=="weeper-hunt"?1:0,true);e.Visual.Position=Vector3.Zero;
  var torso=e.Visual.GetNode<Node3D>("Torso");
  if(DormantWeeper(e)){
   e.Visual.Position=new(0,-.52f,0);torso.RotationDegrees=new(-32,0,Mathf.Sin((float)game.Clock*2)*3);
   foreach(string side in new[]{"Left","Right"}){e.Visual.GetNode<Node3D>(side+"Leg").Rotation=new(-1.2f,0,0);e.Visual.GetNode<Node3D>(side+"Leg/Knee").Rotation=new(1.9f,0,0);e.Visual.GetNode<Node3D>(side+"Arm").Rotation=new(-2.3f,0,side=="Left"?.2f:-.2f);e.Visual.GetNode<Node3D>(side+"Arm/Elbow").Rotation=new(1.6f,0,0);}
  }else{
   torso.RotationDegrees=new(e.BossState=="weeper-charge"?-25:-10,0,0);
   foreach(string side in new[]{"Left","Right"})e.Visual.GetNode<Node3D>(side+"Arm").Rotation=new(e.BossState=="weeper-windup"?-2:e.BossState=="weeper-charge"?-1.2f:0,0,side=="Left"?-.3f:.3f);
  }
 }
 AudioStreamWav weeperCry;
 AudioStreamWav WeeperCry(){
  if(weeperCry!=null)return weeperCry;const int rate=22050,count=rate*6;var data=new byte[count*2];var random=new Random(981);double low=0;
  for(int i=0;i<count;i++){double t=i/(double)rate,phase=t%2;low=low*.9+(random.NextDouble()*2-1)*.1;double envelope=phase<1.3?Math.Sin(phase/1.3*Math.PI)*(.5+.5*Math.Sin(t*24)*Math.Sin(t*24)):0;double f=150+45*Math.Sin(t*2.4);short v=(short)((Math.Sin(t*f*2*Math.PI)*.16+low*.55)*envelope*12000);data[i*2]=(byte)v;data[i*2+1]=(byte)(v>>8);}
  return weeperCry=new(){Format=AudioStreamWav.FormatEnum.Format16Bits,MixRate=rate,Data=data,LoopMode=AudioStreamWav.LoopModeEnum.Forward,LoopEnd=count};
 }
 Label dangerLabel;
 void DrawSpecialInfected(){DrawNewThreats();
  foreach(var e in infected){
   string kind=InfectedKind(e);
   if(kind=="weeper"){
    if(CoopClient){PoseWeeper(e);var capsule=(CapsuleShape3D)e.Body.GetChild<CollisionShape3D>(0).Shape;capsule.Height=DormantWeeper(e)?1.2f:2;e.Body.GetChild<CollisionShape3D>(0).Position=new(0,capsule.Height/2,0);}
    if(!testing&&DisplayServer.GetName()!="headless"&&e.Visual.GetChildCount()>0){var cry=e.Visual.GetNodeOrNull<AudioStreamPlayer3D>("Crying");if(cry==null){cry=new(){Name="Crying",Stream=WeeperCry(),MaxDistance=55,UnitSize=6,VolumeDb=-13};e.Visual.AddChild(cry);cry.Play();}cry.StreamPaused=!DormantWeeper(e)||muted||(paused&&!CoopActive);}
   }
   if(kind=="bloater"&&e.Visual.HasNode("Torso"))e.Visual.GetNode<Node3D>("Torso").Scale=new(1.35f+(float)Math.Sin(game.Clock*3)*.025f,1,1.3f);
  }
  if(dangerLabel==null){dangerLabel=HudLabel(14);dangerLabel.Position=new(420,246);}
  var danger=infected.Where(e=>InfectedKind(e)=="weeper"&&Distance(e.Body.Position,player.Position)<25).OrderBy(e=>Distance(e.Body.Position,player.Position)).FirstOrDefault();
  dangerLabel.Text=danger==null?"":DormantWeeper(danger)?"SOBBING NEARBY / leave it undisturbed":danger.BossState=="weeper-windup"?"CLAWS RAISED / sidestep the rush!":danger.BossState=="weeper-recover"?"WEEPER EXPOSED / strike now":"WEEPER / keep your distance";
 }
}
