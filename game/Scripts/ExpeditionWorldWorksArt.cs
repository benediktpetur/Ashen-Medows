using Godot;
using System;
public partial class MigrationScene {
 void DrawWorldWorks(){if(!ContentEnabled)return;foreach(var pair in contentRoots){if(!GodotObject.IsInstanceValid(pair.Value))continue;var w=WorldWork.At(pair.Key);if(w==null)continue;var s=game.World.Changes.Work(w.Site);string signature=s.Choice+"/"+s.Finished;
  var old=pair.Value.GetNodeOrNull<Node3D>("WorldWork");if(old!=null&&old.GetMeta("stage","").AsString()==signature){var label=old.GetNodeOrNull<Label3D>("WorkLabel");if(!s.Finished&&s.Choice>0)WorkSound(old,s);if(label!=null&&!s.Finished&&s.Choice>0)label.Text=$"{w.Name}\nWORK / {s.Remaining:0}s / stay nearby";continue;}
  if(old!=null){pair.Value.RemoveChild(old);old.QueueFree();}var n=new Node3D{Name="WorldWork",Position=new(-6,0,9)};pair.Value.AddChild(n);n.SetMeta("stage",signature);
  n.AddChild(new Label3D{Name="WorkLabel",Text=w.Name+"\n"+(s.Choice==0?"F at the site's board / choose its future":!s.Finished?"WORK IN PROGRESS":s.Choice==1?"RESTORED / SHARED SERVICE":"DISMANTLED / STORES REMOVED"),Position=new(0,2.2f,0),FontSize=20,PixelSize=.008f,Billboard=BaseMaterial3D.BillboardModeEnum.Enabled,Modulate=s.Choice==2?new(.72f,.51f,.36f):new Color(.73f,.8f,.59f)});
  if(s.Choice==0){Box(n,new(0,.3f,0),new(1.5f,.6f,.8f),Wood);StoryPaper(n,new(0,.62f,0));continue;}
  if(!s.Finished){for(int i=0;i<4;i++)Box(n,new(-1+i*.5f,.12f,0),new(.4f,.24f,2),Wood);for(int side=-1;side<=1;side+=2)Tube(n,new(side*1.6f,0,0),new(side*1.6f,1.5f,0),.045f,Steel);Box(n,new(0,1.5f,0),new(3.5f,.14f,.2f),new(.63f,.43f,.12f));n.AddChild(new AudioStreamPlayer3D{Name="WorkSound",Stream=Tone(4,.12),VolumeDb=-15,MaxDistance=32,UnitSize=4});continue;}
  if(s.Choice==2){for(int i=0;i<7;i++){var plank=Box(n,new((i%3-1)*.6f,.08f+i*.015f,(i/3-1)*.6f),new(.18f,.08f,1.5f),new(.25f,.22f,.17f));plank.RotationDegrees=new(0,i*39,0);}Box(n,new(1.8f,.25f,0),new(.85f,.5f,.6f),Wood);continue;}
  switch(w.Kind){
   case "garden":for(int row=0;row<3;row++){Box(n,new(0,.15f,row*1.1f),new(3,.3f,.85f),Wood);Box(n,new(0,.32f,row*1.1f),new(2.8f,.07f,.65f),new(.15f,.12f,.07f));for(int i=0;i<6;i++){var p=new Vector3(-1.2f+i*.48f,.35f,row*1.1f);Tube(n,p,p+Vector3.Up*.6f,.018f,Leaf);Ellipsoid(n,p+new Vector3(.08f,.4f,0),new(.19f,.06f,.12f),new(.32f,.46f,.16f));}}break;
   case "water":for(int i=0;i<2;i++){Tube(n,new(i*.9f,0,0),new(i*.9f,1.3f,0),.38f,new(.16f,.38f,.43f));}Tube(n,new(0,1.1f,0),new(-.8f,1.1f,0),.045f,Steel);Box(n,new(-.8f,.2f,0),new(.6f,.4f,.6f),Steel);break;
   case "clinic":for(int i=0;i<2;i++){Box(n,new(i*1.5f,.55f,0),new(1,.16f,2),Steel,true);Box(n,new(i*1.5f,.67f,0),new(.9f,.13f,1.85f),new(.65f,.64f,.52f));}Box(n,new(-1.2f,.9f,0),new(.8f,1.8f,.6f),new(.53f,.57f,.47f),true);Box(n,new(-1.2f,1.25f,-.32f),new(.4f,.12f,.03f),new(.6f,.2f,.15f));break;
   case "refuge":foreach(int x in new[]{-1,1})foreach(int z in new[]{-1,1})Tube(n,new(x*1.8f,0,z*1.8f),new(x*1.8f,2.8f,z*1.8f),.07f,Wood);Box(n,new(0,2.8f,0),new(4.2f,.2f,4.2f),Wood,true);Box(n,new(0,.5f,0),new(2,.12f,.65f),Wood,true);break;
   case "warning":case "trail":for(int i=0;i<3;i++){var p=new Vector3(i*2,0,i*2);Tube(n,p,p+Vector3.Up*2.8f,.055f,Steel);Box(n,p+new Vector3(0,2.75f,0),new(.4f,.2f,.4f),new(.72f,.62f,.25f));n.AddChild(new OmniLight3D{Position=p+Vector3.Up*2.5f,LightColor=new(.9f,.72f,.36f),LightEnergy=.65f,OmniRange=9,ShadowEnabled=false});}break;
   default:for(int i=0;i<4;i++){Box(n,new(i%2*.95f,.3f+i/2*.65f,0),new(.85f,.6f,.7f),Wood);Box(n,new(i%2*.95f,.6f+i/2*.65f,0),new(.92f,.06f,.76f),Wood);}break;
  }
 }}
 void WorkSound(Node3D root,WorldWorkState state){if(muted||paused||testing||DisplayServer.GetName()=="headless"||!FieldReady(state.Site))return;var at=ExpeditionContentSite.Place(game.World,state.Site);if(ExpeditionGame.Distance(player.Position.X,player.Position.Z,at.X,at.Z)>24)return;double now=Time.GetTicksMsec()/1000.0;if(now<(double)root.GetMeta("next-beat",0d))return;root.SetMeta("next-beat",now+.8);root.GetNodeOrNull<AudioStreamPlayer3D>("WorkSound")?.Play();}
}
