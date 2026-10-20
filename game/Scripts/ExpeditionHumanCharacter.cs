using Godot;
using System;
public partial class MigrationScene {
 // Metres, Y up, -Z forward. Elbow hinge flexion is positive X.
 void BuildHumanCharacter(Node3D root,int tier,int night,bool survivor,int zombieModel=-1){
  int model=survivor?-1:zombieModel>=0?Math.Clamp(zombieModel,0,6):night==1?5:night==2?6:Math.Clamp(tier-1,0,4);
  string key="human-v3/"+(survivor?"survivor":$"infected-{model}");
  Color cloth=survivor?new(.48f,.40f,.27f):new(.36f,.32f,.25f),skin=survivor?new(.65f,.53f,.43f):new(.48f,.49f,.42f),pants=new(.24f,.26f,.25f);
  if(!survivor){cloth=ZombieCloth(model);skin=ZombieSkin(model);pants=model==3?new Color(.18f,.20f,.23f):model==2?new Color(.25f,.23f,.19f):pants;}
  var torso=new Node3D{Name="Torso",Position=new(0,.96f,0)};root.AddChild(torso);
  FigureMesh(torso,key+"/body",b=>{
   b.Loft(cloth,new[]{new Vector3(0,-.07f,0),new Vector3(0,.04f,0),new Vector3(0,.16f,.005f),new Vector3(0,.29f,.006f),new Vector3(0,.37f,.006f),new Vector3(0,.42f,.005f),new Vector3(0,.46f,0)},new[]{new Vector2(.175f,.11f),new Vector2(.16f,.10f),new Vector2(.165f,.105f),new Vector2(.198f,.12f),new Vector2(.205f,.105f),new Vector2(.16f,.085f),new Vector2(.065f,.055f)},24);
   b.Stitch(cloth*.65f,new(0,-.065f,-.113f),new(0,.39f,-.103f),.002f);
   for(int s=-1;s<=1;s+=2)b.Panel(cloth*.88f,new(s*.06f,.20f,-.114f),new(s*.145f,.20f,-.089f),new(s*.143f,.27f,-.108f),new(s*.06f,.27f,-.125f));
  });
  var head=new Node3D{Name="Head",Position=new(0,.49f,0)};torso.AddChild(head);
  FigureMesh(head,key+"/head",b=>{
   b.Loft(skin,new[]{new Vector3(0,-.045f,.012f),new Vector3(0,.035f,.012f)},new[]{new Vector2(.043f,.044f),new Vector2(.046f,.045f)},16);
   b.Loft(skin,new[]{new Vector3(0,.025f,-.018f),new Vector3(0,.055f,-.006f),new Vector3(0,.10f,0),new Vector3(0,.15f,.006f),new Vector3(0,.20f,.012f),new Vector3(0,.235f,.015f),new Vector3(0,.25f,.015f)},new[]{new Vector2(.037f,.045f),new Vector2(.061f,.059f),new Vector2(.077f,.072f),new Vector2(.078f,.076f),new Vector2(.072f,.073f),new Vector2(.046f,.050f),new Vector2(.008f,.012f)},24);
   var nose=new Color(skin.R*.95f,skin.G*.95f,skin.B*.95f);
   b.Loft(nose,new[]{new Vector3(0,.095f,-.072f),new Vector3(0,.11f,-.084f),new Vector3(0,.15f,-.065f)},new[]{new Vector2(.016f,.012f),new Vector2(.012f,.018f),new Vector2(.008f,.006f)},12);
   for(int s=-1;s<=1;s+=2){
    b.Loft(skin,new[]{new Vector3(s*.077f,.093f,.003f),new Vector3(s*.082f,.127f,.005f),new Vector3(s*.075f,.148f,.007f)},new[]{new Vector2(.011f,.012f),new Vector2(.013f,.015f),new Vector2(.008f,.01f)},12);
    b.Panel(new Color(.17f,.15f,.13f),new(s*.018f,.144f,-.070f),new(s*.052f,.143f,-.054f),new(s*.052f,.151f,-.056f),new(s*.018f,.152f,-.071f));
    b.Stitch(new Color(.20f,.17f,.13f),new(s*.018f,.164f,-.070f),new(s*.056f,.160f,-.055f),.002f);
   }
   b.Stitch(skin*.55f,new(-.022f,.071f,-.064f),new(.022f,.071f,-.064f),.002f);
   b.Loft(new Color(.12f,.10f,.075f),new[]{new Vector3(0,.195f,.014f),new Vector3(0,.231f,.016f),new Vector3(0,.253f,.016f)},new[]{new Vector2(.074f,.075f),new Vector2(.055f,.060f),new Vector2(.008f,.012f)},24);  });
  for(int s=-1;s<=1;s+=2){
   var leg=new Node3D{Name=s<0?"LeftLeg":"RightLeg",Position=new(s*.095f,.91f,0)};root.AddChild(leg);
   FigureMesh(leg,key+"/thigh/"+s,b=>b.Loft(pants,new[]{Vector3.Zero,new Vector3(s*.007f,-.12f,0),new Vector3(s*.008f,-.29f,-.012f),new Vector3(0,-.405f,-.015f)},new[]{new Vector2(.092f,.102f),new Vector2(.09f,.098f),new Vector2(.068f,.075f),new Vector2(.058f,.064f)},20));
   var knee=new Node3D{Name="Knee",Position=new(0,-.405f,-.015f)};leg.AddChild(knee);
   FigureMesh(knee,key+"/shin/"+s,b=>{
    b.Loft(pants,new[]{new Vector3(0,.012f,0),new Vector3(0,-.10f,.025f),new Vector3(0,-.28f,.023f),new Vector3(0,-.38f,.015f)},new[]{new Vector2(.059f,.064f),new Vector2(.065f,.072f),new Vector2(.048f,.054f),new Vector2(.043f,.047f)},20);
    b.Loft(new Color(.18f,.17f,.15f),new[]{new Vector3(0,-.495f,-.045f),new Vector3(0,-.46f,-.045f),new Vector3(0,-.40f,-.025f),new Vector3(0,-.345f,.015f)},new[]{new Vector2(.056f,.12f),new Vector2(.058f,.12f),new Vector2(.052f,.095f),new Vector2(.046f,.05f)},20);
   });
   var arm=new Node3D{Name=s<0?"LeftArm":"RightArm",Position=new(s*.198f,1.335f,0)};root.AddChild(arm);
   FigureMesh(arm,key+"/upper-arm/"+s,b=>b.Loft(cloth,new[]{new Vector3(-s*.015f,.025f,0),new Vector3(s*.015f,-.055f,0),new Vector3(s*.02f,-.16f,0),new Vector3(s*.018f,-.285f,0)},new[]{new Vector2(.048f,.055f),new Vector2(.066f,.069f),new Vector2(.055f,.061f),new Vector2(.043f,.047f)},20));
   var elbow=new Node3D{Name="Elbow",Position=new(s*.018f,-.285f,0)};arm.AddChild(elbow);
   FigureMesh(elbow,key+"/forearm/"+s,b=>b.Loft(cloth,new[]{new Vector3(0,.008f,0),new Vector3(0,-.065f,0),new Vector3(0,-.17f,0),new Vector3(0,-.245f,0)},new[]{new Vector2(.044f,.048f),new Vector2(.049f,.054f),new Vector2(.035f,.039f),new Vector2(.026f,.029f)},20));
   var hand=new Node3D{Name="Hand",Position=new(0,-.245f,0),Rotation=new(0,s*Mathf.Pi/2,0)};elbow.AddChild(hand);
   FigureMesh(hand,key+"/hand/"+s,b=>HumanPalm(b,skin,s,survivor));
  }
  if(!survivor)DressZombie(root,model,key,skin);
  PoseFigure(root,0,0,!survivor);
 }
 void HumanPalm(FigureSculpt b,Color skin,int side,bool glove){
  var c=glove?new Color(.24f,.245f,.22f):skin;
  b.Loft(c,new[]{Vector3.Zero,new Vector3(0,-.025f,0),new Vector3(0,-.062f,0),new Vector3(0,-.085f,0)},new[]{new Vector2(.023f,.017f),new Vector2(.032f,.02f),new Vector2(.037f,.019f),new Vector2(.033f,.016f)},20);
  // Index is on thumb side; all four fingers emerge from the distal palm edge.
  float[] x={.026f,.009f,-.009f,-.026f},len={.064f,.073f,.068f,.052f};
  for(int i=0;i<4;i++){var a=new Vector3(x[i]*side,-.078f-(i==1?.007f:0),0);var p=a+new Vector3(0,-len[i]*.45f,-.003f);var q=p+new Vector3(0,-len[i]*.33f,-.006f);var tip=q+new Vector3(0,-len[i]*.22f,-.008f);GripFinger(b,c,a,p,q,tip);}
  GripFinger(b,c,new(side*.027f,-.028f,0),new(side*.047f,-.048f,-.005f),new(side*.052f,-.068f,-.012f),new(side*.047f,-.083f,-.020f));
 }
}



