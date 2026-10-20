using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
public partial class MigrationScene {
 void BuildNewThreatArt(Node3D root,string kind,int tier){
  int model=kind=="riot"?4:kind is "howler" or "palehunter"?5:kind is "cinder" or "bilecaster"?6:Math.Clamp(tier-1,0,4);HumanCharacter(root,tier,0,false,model);
  var torso=root.GetNode<Node3D>("Torso");var head=torso.GetNode<Node3D>("Head");Color bone=new(.69f,.64f,.44f),steel=new(.24f,.29f,.28f),rust=new(.49f,.27f,.12f);
  if(kind=="mossback"){
   for(int i=0;i<12;i++){float a=i*2.4f;Ellipsoid(torso,new(Mathf.Sin(a)*.21f,.03f+(i%4)*.11f,Mathf.Cos(a)*.15f),new(.08f,.09f,.07f),i%2==0?new(.23f,.34f,.16f):new(.35f,.39f,.19f));}for(int s=-1;s<=1;s+=2)Tube(torso,new(s*.16f,.30f,.08f),new(s*.35f,.65f,.1f),.025f,bone,.005f);
  }else if(kind=="howler"){
   head.Scale=new(1.1f,1.25f,1);Ellipsoid(head,new(0,-.01f,-.04f),new(.085f,.11f,.075f),new(.55f,.23f,.20f));Bevel(head,new(0,.07f,-.095f),new(.10f,.09f,.04f),new(.14f,.10f,.09f));for(int s=-1;s<=1;s+=2)Tube(torso,new(s*.08f,.10f,-.12f),new(s*.05f,.45f,-.08f),.025f,new(.52f,.34f,.25f));
  }else if(kind=="bilecaster"){
   torso.Scale=new(1.15f,1,1.1f);for(int i=0;i<5;i++)Ellipsoid(torso,new((i%2==0?-1:1)*.15f,.08f+i*.065f,.14f),new(.075f,.09f,.10f),new(.44f,.59f,.19f));Tube(head,new(0,.06f,-.06f),new(0,-.08f,-.17f),.035f,new(.43f,.50f,.22f),.016f);
  }else if(kind=="gravecrawler"){
   for(int s=-1;s<=1;s+=2){var hand=root.GetNode<Node3D>((s<0?"Left":"Right")+"Arm/Elbow/Hand");for(int i=0;i<3;i++)Tube(hand,new((i-1)*.025f,-.06f,0),new((i-1)*.04f,-.19f,-.05f),.012f,bone,.002f);Bevel(torso,new(s*.15f,.20f,-.10f),new(.09f,.22f,.04f),new(.34f,.25f,.17f));}
  }else if(kind=="roadbreaker"){
   root.Scale=new(1.30f,1.14f,1.2f);Bevel(head,new(0,.19f,0),new(.20f,.13f,.20f),new(.76f,.49f,.12f));Bevel(torso,new(0,.20f,-.13f),new(.33f,.28f,.08f),rust);for(int s=-1;s<=1;s+=2){var a=root.GetNode<Node3D>((s<0?"Left":"Right")+"Arm");Bevel(a,new(0,-.03f,-.02f),new(.21f,.17f,.20f),steel);}for(int i=0;i<3;i++)Box(torso,new(0,.12f+i*.075f,-.18f),new(.29f,.025f,.02f),new(.86f,.66f,.18f));
  }else if(kind=="cinder"){
   for(int i=0;i<8;i++)Ellipsoid(torso,new((i%2==0?-1:1)*.15f,.02f+(i%4)*.1f,-.10f),new(.05f,.07f,.05f),new(.92f,.32f,.06f));Bevel(torso,new(0,.2f,.15f),new(.25f,.32f,.14f),new(.20f,.16f,.12f));root.AddChild(new OmniLight3D{Position=new(0,1.15f,0),LightColor=new(1,.3f,.06f),LightEnergy=.7f,OmniRange=4});
  }else if(kind=="riot"){
   Bevel(head,new(0,.19f,0),new(.20f,.14f,.21f),steel);Box(head,new(0,.125f,-.09f),new(.17f,.07f,.025f),new(.13f,.22f,.23f));var shield=Bevel(torso,new(-.17f,.02f,-.30f),new(.46f,.73f,.075f),steel);shield.Name="RiotShield";Box(torso,new(-.17f,.24f,-.345f),new(.30f,.12f,.02f),new(.14f,.22f,.23f));Box(torso,new(-.17f,-.13f,-.345f),new(.34f,.06f,.02f),new(.69f,.66f,.48f));
  }else if(kind=="wirebound"){
   for(int i=0;i<9;i++){float y=.02f+i*.045f;Tube(torso,new(-.19f,y,-.12f),new(.19f,y+.03f,-.12f),.008f,steel);}var hand=root.GetNode<Node3D>("RightArm/Elbow/Hand");for(int i=0;i<6;i++)Tube(hand,new(.03f,-i*.10f,-.02f),new(i%2==0?-.06f:.08f,-.12f-i*.10f,-.04f),.008f,steel);
  }else if(kind=="blooddrinker"){
   root.Scale=new(1.05f,1.09f,1.05f);for(int s=-1;s<=1;s+=2){Bevel(torso,new(s*.09f,.20f,-.12f),new(.08f,.31f,.035f),new(.35f,.12f,.12f));Tube(head,new(s*.022f,.07f,-.07f),new(s*.023f,-.015f,-.09f),.009f,bone,.002f);}Ellipsoid(torso,new(0,.09f,-.12f),new(.08f,.10f,.04f),new(.46f,.16f,.16f));
  }else{
   root.Scale=new(.88f,1.17f,.9f);for(int s=-1;s<=1;s+=2){var hand=root.GetNode<Node3D>((s<0?"Left":"Right")+"Arm/Elbow/Hand");for(int i=0;i<4;i++)Tube(hand,new((i-1.5f)*.022f,-.07f,0),new((i-1.5f)*.035f,-.32f,-.08f),.01f,bone,.002f);Ellipsoid(head,new(s*.035f,.15f,-.06f),new(.015f,.01f,.014f),new(.68f,.86f,.78f));}for(int i=0;i<4;i++)Tube(torso,new(0,.03f+i*.1f,.12f),new(0,.12f+i*.1f,.28f),.018f,bone,.004f);
  }
 }
 void PoseNewThreat(Infected e){
  if(!e.Visual.HasNode("Torso"))return;string kind=InfectedKind(e);bool hidden=e.BossState=="threat-hidden",crawl=kind=="gravecrawler";PoseFigure(e.Visual,(float)game.Clock*5,e.BossState=="threat-rush"?1:e.Memory>0?.5f:0,true);e.Visual.Position=Vector3.Zero;
  var torso=e.Visual.GetNode<Node3D>("Torso");if(hidden||crawl){e.Visual.Position=new(0,-.45f,0);torso.RotationDegrees=new(-38,0,0);foreach(string side in new[]{"Left","Right"}){e.Visual.GetNode<Node3D>(side+"Leg").Rotation=new(-1.0f,0,0);e.Visual.GetNode<Node3D>(side+"Leg/Knee").Rotation=new(1.65f,0,0);}}
  if(e.BossState.EndsWith("-windup")||e.AttackState=="windup"){torso.RotationDegrees=new(kind=="howler"?18:-22,0,0);foreach(string side in new[]{"Left","Right"})e.Visual.GetNode<Node3D>(side+"Arm").RotationDegrees=new(-105,0,side=="Left"?-20:20);}
  if(kind=="cinder"&&e.BossState=="cinder-death")torso.RotationDegrees=new(22,0,0);
 }
 readonly Dictionary<string,Node3D> threatCues=new();
 void DrawNewThreats(){
  foreach(var old in threatCues.Keys.Where(id=>!infected.Any(e=>e.Id==id&&(e.BossState.EndsWith("-windup")||e.BossState=="cinder-death"))).ToArray()){threatCues[old].QueueFree();threatCues.Remove(old);}
  foreach(var e in infected){var spec=ExpeditionThreat.Find(InfectedKind(e));if(spec==null)continue;PoseNewThreat(e);if(!(e.BossState.EndsWith("-windup")||e.BossState=="cinder-death"))continue;
   if(!threatCues.TryGetValue(e.Id,out var root)){root=new Node3D();AddChild(root);threatCues[e.Id]=root;MillRing(root,spec.Kind=="cinder"?3.2f:spec.Kind=="howler"?2:2.2f,spec.Kind=="wirebound"?new(.68f,.35f,.86f,.9f):spec.Kind=="bilecaster"?new(.45f,.8f,.13f,.9f):new(1,.48f,.12f,.9f));root.AddChild(new Label3D{Text=spec.Name.ToUpperInvariant()+" / "+(spec.Kind=="howler"?"SCREAM / INTERRUPT":spec.Kind=="cinder"?"IGNITING / MOVE AWAY":"ATTACK / MOVE"),Position=new(0,2.3f,0),FontSize=23,PixelSize=.006f,Billboard=BaseMaterial3D.BillboardModeEnum.Enabled});}
   root.Position=spec.Kind is "bilecaster" or "wirebound"?e.BossAim:e.Body.Position;
  }
 }
 void ShowThreatGuide(){var box=Panel("FIELD GUIDE / INFECTED THREATS");var scroll=new ScrollContainer{CustomMinimumSize=new(900,430)};box.AddChild(scroll);var list=new VBoxContainer();scroll.AddChild(list);foreach(var t in ExpeditionThreat.All)Text(list,t.Name.ToUpperInvariant()+$" / T{t.Min}+ / "+t.Role+"\nHABITAT / "+ExpeditionThreatEcology.Description(t.Kind)+"\n"+t.Counter);Button(box,"BACK",CloseMenu);}
}
