using Godot;
using System;
public partial class MigrationScene {
 void RefinedTorso(FigureSculpt b,Color coat,Color shirt,bool survivor,int night){
  float bulk=night==2?1.10f:1;float[] ys={survivor?-.13f:-.23f,-.055f,.06f,.18f,.29f,.36f,.405f,.46f};
  float[] widths={.178f,.172f,.152f,.160f,.188f,.205f,.185f,.065f};
  float[] depths={.108f,.108f,.096f,.103f,.115f,.105f,.085f,.062f};
  b.Loft(shirt,new[]{new Vector3(0,-.04f,0),new Vector3(0,.10f,0),new Vector3(0,.29f,.008f),new Vector3(0,.43f,.012f)},new[]{new Vector2(.145f,.075f),new Vector2(.136f,.074f),new Vector2(.165f,.082f),new Vector2(.060f,.045f)},16);
  const int sides=32;var rings=new Vector3[ys.Length][];
  for(int r=0;r<ys.Length;r++){rings[r]=new Vector3[sides];for(int i=0;i<sides;i++){float a=i*Mathf.Tau/sides;float wrinkle=1+.014f*Mathf.Sin(a*5+r*1.6f);float hem=!survivor&&r==0?(i%5)*.006f:0;rings[r][i]=new Vector3(Mathf.Sin(a)*widths[r]*bulk*wrinkle,ys[r]+hem,Mathf.Cos(a)*depths[r]*wrinkle+.012f);}}
  for(int r=0;r<rings.Length-1;r++)for(int i=0;i<sides;i++){int j=(i+1)%sides;float a=(i+.5f)*Mathf.Tau/sides;if(Math.Abs(a-Mathf.Pi)<.31f)continue;var shade=coat*(1+.015f*Mathf.Sin(i*1.3f+r));b.Triangle(rings[r][i],rings[r+1][i],rings[r+1][j],shade);b.Triangle(rings[r][i],rings[r+1][j],rings[r][j],shade);}
  for(int s=-1;s<=1;s+=2){
   b.Panel(coat*1.08f,new Vector3(s*.051f,.17f,-.098f),new Vector3(s*.114f,.318f,-.101f),new Vector3(s*.064f,.445f,-.062f),new Vector3(s*.025f,.30f,-.104f));
   b.Stitch(coat*.64f,new Vector3(s*.051f,.17f,-.101f),new Vector3(s*.114f,.318f,-.104f),.0014f);
   b.Panel(coat*.88f,new Vector3(s*.083f,.017f,-.083f),new Vector3(s*.143f,.029f,-.063f),new Vector3(s*.141f,.10f,-.069f),new Vector3(s*.081f,.092f,-.087f));
   b.Stitch(coat*.65f,new Vector3(s*.081f,.092f,-.089f),new Vector3(s*.141f,.10f,-.072f),.0012f);
   b.Stitch(coat*.76f,new Vector3(s*.07f,.125f,-.084f),new Vector3(s*.125f,.165f,-.074f),.0012f);
  }
  b.Loft(new Color(.17f,.15f,.12f),new[]{new Vector3(0,-.026f,0),new Vector3(0,.002f,0)},new[]{new Vector2(.159f,.098f),new Vector2(.159f,.098f)},16);
  b.Panel(new Color(.39f,.37f,.29f),new Vector3(-.018f,-.023f,-.102f),new Vector3(.018f,-.023f,-.102f),new Vector3(.018f,.005f,-.102f),new Vector3(-.018f,.005f,-.102f));
 }
 void RefinedSleeve(FigureSculpt b,Color coat,int side,bool survivor){float s=side,reach=survivor?-.055f:-.13f;
  b.Loft(coat,new[]{new Vector3(s*.04f,-.49f,reach),new Vector3(s*.045f,-.425f,reach+.01f),new Vector3(s*.05f,-.32f,-.07f),new Vector3(s*.045f,-.255f,-.035f),new Vector3(s*.035f,-.17f,-.005f),new Vector3(s*.012f,-.075f,.015f),new Vector3(-s*.018f,.005f,.012f),new Vector3(-s*.050f,.025f,.012f)},new[]{new Vector2(.038f,.040f),new Vector2(.047f,.048f),new Vector2(.054f,.056f),new Vector2(.056f,.060f),new Vector2(.060f,.064f),new Vector2(.066f,.069f),new Vector2(.060f,.065f),new Vector2(.027f,.035f)},16,ragged:!survivor);
  b.Loft(coat*.82f,new[]{new Vector3(s*.04f,-.495f,reach),new Vector3(s*.04f,-.469f,reach+.005f)},new[]{new Vector2(.040f,.042f),new Vector2(.042f,.044f)},12);
  b.Stitch(coat*.72f,new Vector3(s*.025f,-.25f,-.098f),new Vector3(s*.080f,-.278f,-.089f),.0013f);
  b.Stitch(coat*.79f,new Vector3(s*.04f,-.305f,-.12f),new Vector3(s*.075f,-.324f,-.111f),.001f);
 }
 void RefinedHand(FigureSculpt output,Vector3 wrist,Color skin,int side,bool gloved){var b=new FigureSculpt();var glove=new Color(.15f,.155f,.135f);var palm=gloved?glove:skin;var finger=gloved?glove:skin;
  b.Loft(palm,new[]{Vector3.Zero,new Vector3(0,-.025f,0),new Vector3(0,-.055f,-.003f),new Vector3(0,-.078f,-.005f)},new[]{new Vector2(.023f,.017f),new Vector2(.030f,.019f),new Vector2(.033f,.019f),new Vector2(.028f,.015f)},12);
  float[] xs={-.025f,-.008f,.009f,.024f},lengths={.055f,.066f,.061f,.046f},bases={-.072f,-.078f,-.076f,-.069f};
  for(int i=0;i<4;i++){float x=xs[i]*-side,len=lengths[i];var a=new Vector3(x,bases[i],-.005f);var knuckle=a+new Vector3((i-1.5f)*-.001f*side,-len*.43f,-.004f);var joint=knuckle+new Vector3(0,-len*.34f,-.007f);var tip=joint+new Vector3(0,-len*.23f,-.009f);float radius=i==3?.0067f:.008f;
   b.Limb(palm,a,knuckle,radius,.008f,radius*.92f,.007f,10);b.Limb(finger,knuckle,joint,radius*.92f,.007f,radius*.8f,.006f,10);b.Limb(finger,joint,tip,radius*.8f,.006f,.0048f,.0045f,10);
  }
  float thumb=-side;Vector3 root=new Vector3(thumb*.023f,-.025f,0),joint1=new Vector3(thumb*.041f,-.044f,-.003f),joint2=new Vector3(thumb*.046f,-.060f,-.010f),tipThumb=new Vector3(thumb*.044f,-.078f,-.017f);b.Limb(palm,root,joint1,.012f,.013f,.010f,.011f,10);b.Limb(finger,joint1,joint2,.010f,.011f,.008f,.008f,10);b.Limb(finger,joint2,tipThumb,.008f,.008f,.006f,.0055f,10);
  output.Append(b,new Transform3D(Basis.FromEuler(new Vector3(0,-side*1.05f,0)).Scaled(new Vector3(1.05f,1.1f,1.05f)),wrist));
 }
 void RefinedWeaponHands(Node3D root,bool pistol){
  Color cloth=new Color(.43f,.37f,.245f),skin=new Color(.18f,.18f,.155f),glove=new Color(.15f,.155f,.135f);
  FigureMesh(root,"refined-fps/sleeves/"+pistol,b=>{var rear=new Vector3(.034f,-.19f,.075f);var front=new Vector3(-.055f,pistol?-.18f:-.13f,pistol?.02f:-.33f);foreach(var pair in new[]{(new Vector3(.13f,-.43f,.30f),rear),(new Vector3(-.17f,-.42f,.07f),front)}){var mid=pair.Item1.Lerp(pair.Item2,.60f);b.Limb(cloth,pair.Item1,mid,.052f,.051f,.043f,.044f,14);b.Limb(cloth,mid,pair.Item2,.043f,.044f,.029f,.030f,14);var axis=(pair.Item2-pair.Item1).Normalized();b.Limb(cloth*.76f,pair.Item2-axis*.022f,pair.Item2,.032f,.033f,.031f,.031f,12);}});
  FigureMesh(root,"refined-fps/hand/right/"+pistol,b=>{
   b.Loft(glove,new[]{new Vector3(.034f,-.19f,.07f),new Vector3(.034f,-.143f,.043f),new Vector3(.033f,-.083f,.025f)},new[]{new Vector2(.020f,.022f),new Vector2(.015f,.029f),new Vector2(.014f,.024f)},12);
   for(int i=0;i<3;i++){float y=-.10f-i*.021f;var a=new Vector3(.032f,y,.002f);var mid=new Vector3(.005f,y-.004f,-.012f);var joint=new Vector3(-.024f,y-.006f,-.010f);var tip=new Vector3(-.028f,y-.008f,.020f);GripFinger(b,skin,a,mid,joint,tip);}
   GripFinger(b,skin,new Vector3(.032f,-.080f,.005f),new Vector3(.028f,-.073f,-.028f),new Vector3(.003f,-.073f,-.045f),new Vector3(-.010f,-.08f,-.041f));
   GripFinger(b,skin,new Vector3(.033f,-.09f,.05f),new Vector3(.015f,-.056f,.055f),new Vector3(-.008f,-.055f,.048f),new Vector3(-.018f,-.065f,.04f));
  });
  FigureMesh(root,"refined-fps/hand/left/"+pistol,b=>{float z=pistol?.024f:-.33f,y=pistol?-.135f:-.075f;
   b.Limb(glove,new Vector3(-.055f,y-.055f,z),new Vector3(-.02f,y-.005f,z),.024f,.026f,.029f,.037f,12);
   for(int i=0;i<4;i++){float zz=z+.029f-i*.019f;GripFinger(b,skin,new Vector3(-.032f,y,zz),new Vector3(-.005f,y+.022f,zz),new Vector3(.026f,y+.021f,zz),new Vector3(.034f,y+.005f,zz));}
   GripFinger(b,skin,new Vector3(-.045f,y-.012f,z+.025f),new Vector3(-.047f,y+.024f,z+.043f),new Vector3(-.023f,y+.043f,z+.047f),new Vector3(.002f,y+.040f,z+.037f));
  });
 }
 void GripFinger(FigureSculpt b,Color c,Vector3 a,Vector3 mid,Vector3 joint,Vector3 tip){b.Limb(c,a,mid,.008f,.008f,.0075f,.0075f,10);b.Limb(c,mid,joint,.0075f,.0075f,.0065f,.0065f,10);b.Limb(c,joint,tip,.0065f,.0065f,.005f,.005f,10);}
}
