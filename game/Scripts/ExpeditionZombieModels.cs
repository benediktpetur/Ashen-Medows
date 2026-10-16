using Godot;
using System;
public partial class MigrationScene {
 static readonly string[] ZombieNames={"Valley drifter","Woodland logger","Highway worker","Office revenant","Security guard","Night stalker","Night bloater"};
 static Color ZombieCloth(int m)=>m switch{0=>new(.39f,.36f,.27f),1=>new(.42f,.20f,.16f),2=>new(.40f,.39f,.30f),3=>new(.25f,.29f,.34f),4=>new(.20f,.27f,.25f),5=>new(.17f,.20f,.22f),_=>new(.36f,.30f,.21f)};
 static Color ZombieSkin(int m)=>m switch{0=>new(.55f,.54f,.44f),1=>new(.50f,.49f,.40f),2=>new(.53f,.51f,.40f),3=>new(.55f,.56f,.49f),4=>new(.43f,.48f,.43f),5=>new(.39f,.45f,.49f),_=>new(.48f,.49f,.32f)};
 void DressZombie(Node3D root,int m,string key,Color skin){
  var torso=root.GetNode<Node3D>("Torso");var head=torso.GetNode<Node3D>("Head");var dark=new Color(.15f,.13f,.12f);var wound=new Color(.29f,.12f,.105f);
  // Outfit geometry is attached to the same articulated rig as the approved human.
  FigureMesh(torso,key+"/outfit",b=>{
   if(m==0){ // Ragged scarf and a small pack: the quiet valley wanderer.
    b.Loft(new Color(.29f,.25f,.19f),new[]{new Vector3(0,.405f,0),new Vector3(0,.46f,0)},new[]{new Vector2(.087f,.071f),new Vector2(.075f,.064f)},16);
    b.Panel(new Color(.29f,.25f,.19f),new(-.055f,.42f,-.08f),new(.005f,.42f,-.09f),new(-.025f,.19f,-.125f),new(-.075f,.23f,-.115f));
    b.Loft(new Color(.24f,.23f,.18f),new[]{new Vector3(0,.09f,.13f),new Vector3(0,.30f,.14f),new Vector3(0,.35f,.12f)},new[]{new Vector2(.10f,.05f),new Vector2(.12f,.06f),new Vector2(.08f,.04f)},16);
   }
   if(m==1){ // Lumber workwear: check pattern and suspenders.
    for(int i=0;i<5;i++){float y=.02f+i*.073f;float d=y>.20f?.12f:.11f;b.Panel(new Color(.20f,.16f,.12f),new(-.145f,y,-d),new(.145f,y,-d),new(.145f,y+.022f,-d),new(-.145f,y+.022f,-d));}
    for(int s=-1;s<=1;s+=2)b.Panel(dark,new(s*.085f,-.035f,-.112f),new(s*.108f,.37f,-.105f),new(s*.083f,.37f,-.116f),new(s*.06f,-.035f,-.116f));
   }
   if(m==2){ // Road crew high visibility vest, broad reflective bands.
    for(int s=-1;s<=1;s+=2){var orange=new Color(.66f,.37f,.12f);b.Panel(orange,new(s*.035f,-.06f,-.116f),new(s*.16f,-.03f,-.083f),new(s*.183f,.34f,-.085f),new(s*.074f,.42f,-.075f));
     b.Panel(new Color(.64f,.64f,.47f),new(s*.048f,.10f,-.12f),new(s*.164f,.10f,-.10f),new(s*.166f,.135f,-.11f),new(s*.048f,.135f,-.124f));}
   }
   if(m==3){ // Once an office worker: pale shirt, long lapels, hanging tie.
    b.Panel(new Color(.62f,.61f,.52f),new(-.06f,.40f,-.09f),new(.06f,.40f,-.09f),new(.035f,.15f,-.13f),new(-.035f,.15f,-.13f));
    b.Panel(new Color(.29f,.14f,.13f),new(-.012f,.37f,-.105f),new(.014f,.37f,-.105f),new(.025f,.12f,-.135f),new(0,.09f,-.13f),new(-.018f,.13f,-.135f));
    for(int s=-1;s<=1;s+=2)b.Panel(ZombieCloth(m)*1.2f,new(s*.065f,.42f,-.087f),new(s*.142f,.32f,-.11f),new(s*.045f,.19f,-.13f),new(s*.028f,.32f,-.125f));
   }
   if(m==4){ // Security armour and utility belt.
    b.Loft(new Color(.12f,.17f,.17f),new[]{new Vector3(0,.12f,0),new Vector3(0,.29f,0),new Vector3(0,.365f,0)},new[]{new Vector2(.18f,.126f),new Vector2(.207f,.139f),new Vector2(.185f,.12f)},20);
    b.Panel(new Color(.48f,.48f,.39f),new(-.10f,.29f,-.143f),new(.10f,.29f,-.143f),new(.10f,.32f,-.143f),new(-.10f,.32f,-.143f));
    for(int s=-1;s<=1;s+=2)b.Loft(dark,new[]{new Vector3(s*.15f,-.065f,-.06f),new Vector3(s*.15f,.025f,-.06f)},new[]{new Vector2(.035f,.065f),new Vector2(.035f,.065f)},12);
   }
   if(m==5){ // Lean nocturnal figure with a torn cowl and pale rib scars.
    b.Loft(ZombieCloth(m),new[]{new Vector3(0,.32f,.03f),new Vector3(0,.47f,.035f),new Vector3(0,.55f,.04f)},new[]{new Vector2(.19f,.12f),new Vector2(.115f,.095f),new Vector2(.085f,.08f)},20);
    for(int i=0;i<4;i++)b.Stitch(skin*.78f,new(-.075f,.12f+i*.035f,-.115f),new(.06f,.13f+i*.035f,-.122f),.007f);
   }
   if(m==6){ // Bloated abdomen and stretched seams, not a scaled-up head/hand.
    b.Loft(ZombieCloth(m),new[]{new Vector3(0,-.075f,0),new Vector3(0,.06f,-.025f),new Vector3(0,.18f,-.035f),new Vector3(0,.30f,0),new Vector3(0,.38f,0)},new[]{new Vector2(.18f,.13f),new Vector2(.23f,.18f),new Vector2(.25f,.19f),new Vector2(.23f,.15f),new Vector2(.19f,.10f)},24);
    for(int i=0;i<5;i++){float y=.02f+i*.046f;b.Stitch(dark,new(-.035f,y,-.221f),new(.035f,y+.008f,-.221f),.003f);}
    for(int s=-1;s<=1;s+=2)b.Loft(new Color(.40f,.44f,.22f),new[]{new Vector3(s*.18f,.32f,.025f),new Vector3(s*.20f,.40f,.025f),new Vector3(s*.18f,.46f,.025f)},new[]{new Vector2(.035f,.04f),new Vector2(.055f,.06f),new Vector2(.012f,.015f)},12);
   }
  });
  FigureMesh(head,key+"/head-detail",b=>{
   // Asymmetric damage and hollow eyes distinguish the infected from survivors.
   b.Panel(wound,new(-.064f,.08f,-.05f),new(-.028f,.066f,-.064f),new(-.035f,.115f,-.07f),new(-.068f,.12f,-.045f));
   b.Stitch(dark,new(-.026f,.064f,-.066f),new(.025f,.060f,-.066f),.006f);
   if(m==1){b.Loft(new Color(.19f,.18f,.16f),new[]{new Vector3(0,.185f,.015f),new Vector3(0,.24f,.015f),new Vector3(0,.27f,.015f)},new[]{new Vector2(.082f,.084f),new Vector2(.065f,.071f),new Vector2(.015f,.02f)},20);}
   if(m==2){var yellow=new Color(.67f,.52f,.19f);b.Loft(yellow,new[]{new Vector3(0,.18f,.012f),new Vector3(0,.195f,.012f),new Vector3(0,.25f,.012f),new Vector3(0,.28f,.012f)},new[]{new Vector2(.105f,.105f),new Vector2(.09f,.09f),new Vector2(.073f,.08f),new Vector2(.015f,.02f)},20);}
   if(m==4){b.Loft(new Color(.14f,.19f,.19f),new[]{new Vector3(0,.18f,.01f),new Vector3(0,.245f,.01f)},new[]{new Vector2(.087f,.086f),new Vector2(.06f,.06f)},20);b.Panel(dark,new(-.08f,.18f,-.04f),new(.08f,.18f,-.04f),new(.075f,.177f,-.12f),new(-.075f,.177f,-.12f));}
   if(m==5)for(int s=-1;s<=1;s+=2)b.Stitch(new Color(.64f,.73f,.57f),new(s*.022f,.146f,-.072f),new(s*.048f,.146f,-.06f),.0025f);
  });
  if(m==5){torso.Scale=new(.87f,1,1);foreach(string n in new[]{"LeftArm","RightArm"})root.GetNode<Node3D>(n).Scale=new(.84f,1, .84f);}
  if(m==6){foreach(string n in new[]{"LeftArm","RightArm"})root.GetNode<Node3D>(n).Scale=new(1.16f,1,1.12f);}
  head.RotationDegrees=new Vector3(m==5?8:3,m*2-5,m==0?-9:m==3?7:0);
 }
 async void CaptureZombieLineup(){try{
  testing=true;CloseMenu();paused=true;hudRoot.Visible=false;terrainRoot.Visible=worldRoot.Visible=buildRoot.Visible=false;if(distantTerrain!=null)distantTerrain.Visible=false;ClearEnemies();weaponRoot.Visible=false;
  var stage=new Node3D();AddChild(stage);Box(stage,new Vector3(0,-.08f,0),new Vector3(16,.16f,8),new Color(.22f,.25f,.23f));
  stage.AddChild(new DirectionalLight3D{RotationDegrees=new(-30,145,0),LightColor=new(.8f,.85f,.9f),LightEnergy=.85f});
  var figures=new Node3D[7];var labels=new Label3D[7];
  for(int i=0;i<7;i++){var figure=new Node3D{Position=new((3-i)*1.12f,0,0),RotationDegrees=new(0,i%2==0?-8:8,0)};figures[i]=figure;stage.AddChild(figure);HumanCharacter(figure,i+1,i==5?1:i==6?2:0,false,i);PoseFigure(figure,i*.73f,.25f,true);var label=new Label3D{Text=$"{i+1}. {ZombieNames[i]}",Position=new((3-i)*1.12f,2.05f,0),FontSize=28,PixelSize=.003f,Billboard=BaseMaterial3D.BillboardModeEnum.Enabled,Modulate=new(.86f,.86f,.73f)};labels[i]=label;stage.AddChild(label);}
  camera.Fov=39;camera.GlobalPosition=new(0,2.0f,-7.2f);camera.LookAt(new Vector3(0,1.05f,0));await ArtShot("zombies-01-seven-models");
  for(int i=0;i<7;i++){for(int j=0;j<7;j++){figures[j].Visible=j==i;labels[j].Visible=j==i;}camera.Fov=40;camera.GlobalPosition=new((3-i)*1.12f+.62f,1.35f,-2.8f);camera.LookAt(new Vector3((3-i)*1.12f,1.05f,0));await ArtShot($"zombies-{i+2:00}-{i+1}");}
  GD.Print("PASS / seven zombie model GPU captures");GetTree().Quit();
 }catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}

