using Godot;
using System;
using System.Collections.Generic;

public partial class MigrationScene {
 // New mesh construction, solely art-directed from expedition/art-directions/03-cel.png.
 // Each articulated segment batches its sculpted surfaces into one cached mesh.
 readonly Dictionary<string,ArrayMesh> characterMeshes=new();ShaderMaterial characterPigment,characterFacePigment;
 sealed class FigureSculpt {
  readonly List<Vector3> vertices=new(),normals=new();readonly List<Color> colors=new();
  public void Triangle(Vector3 a,Vector3 b,Vector3 c,Color color){var normal=(c-a).Cross(b-a).Normalized();vertices.AddRange(new[]{a,b,c});normals.AddRange(new[]{normal,normal,normal});colors.AddRange(new[]{color,color,color});}
  public void Panel(Color c,params Vector3[] points){for(int i=1;i<points.Length-1;i++){if((points[i+1]-points[0]).Cross(points[i]-points[0]).Z>0)Triangle(points[0],points[i+1],points[i],c);else Triangle(points[0],points[i],points[i+1],c);}}
  // Elliptical cross-sections describe anatomy and garment silhouettes, not primitive limbs.
  public void Loft(Color color,Vector3[] centers,Vector2[] radii,int sides=12,float phase=0,bool ragged=false){
   if(centers[^1].Y<centers[0].Y){centers=(Vector3[])centers.Clone();radii=(Vector2[])radii.Clone();Array.Reverse(centers);Array.Reverse(radii);}var rings=new Vector3[centers.Length][];for(int r=0;r<centers.Length;r++){rings[r]=new Vector3[sides];for(int i=0;i<sides;i++){float a=(i+.5f)*Mathf.Tau/sides+phase;float notch=ragged&&r==0?((i*7)%5)*.009f:0;rings[r][i]=centers[r]+new Vector3(Mathf.Sin(a)*radii[r].X,notch,Mathf.Cos(a)*radii[r].Y);}}
   for(int r=0;r<centers.Length-1;r++)for(int i=0;i<sides;i++){int j=(i+1)%sides;var c=color*(1+(((i*3+r*7)%7)-3)*.018f);Triangle(rings[r][i],rings[r+1][i],rings[r+1][j],c);Triangle(rings[r][i],rings[r+1][j],rings[r][j],c);}
   for(int i=0;i<sides;i++){int j=(i+1)%sides;Triangle(centers[0],rings[0][i],rings[0][j],color);Triangle(centers[^1],rings[^1][j],rings[^1][i],color);}
  }
  public void Limb(Color c,Vector3 a,Vector3 b,float aWidth,float aDepth,float bWidth,float bDepth,int sides=10){
   var axis=(b-a).Normalized();var right=Vector3.Right;if(Math.Abs(axis.Dot(right))>.9)right=Vector3.Forward;var u=axis.Cross(right).Normalized();var v=axis.Cross(u).Normalized();var low=new Vector3[sides];var high=new Vector3[sides];for(int i=0;i<sides;i++){float t=i*Mathf.Tau/sides;low[i]=a+u*Mathf.Cos(t)*aWidth+v*Mathf.Sin(t)*aDepth;high[i]=b+u*Mathf.Cos(t)*bWidth+v*Mathf.Sin(t)*bDepth;}
   for(int i=0;i<sides;i++){int j=(i+1)%sides;Triangle(low[i],high[i],high[j],c);Triangle(low[i],high[j],low[j],c);Triangle(a,low[i],low[j],c);Triangle(b,high[j],high[i],c);}
  }
  public void Stitch(Color c,Vector3 a,Vector3 b,float width=.002f){var offset=(b-a).Cross(Vector3.Forward).Normalized()*width;Panel(c,a-offset,b-offset,b+offset,a+offset);}
  public void Append(FigureSculpt other,Transform3D transform){for(int i=0;i<other.vertices.Count;i+=3)Triangle(transform*other.vertices[i],transform*other.vertices[i+1],transform*other.vertices[i+2],other.colors[i]);}
  public FigureSculpt Section(float minY,float maxY,Vector3 offset){var result=new FigureSculpt();for(int i=0;i<vertices.Count;i+=3){var polygon=new List<Vector3>{vertices[i],vertices[i+1],vertices[i+2]};foreach(var plane in new[]{(minY,true),(maxY,false)}){if(float.IsInfinity(plane.Item1))continue;var clipped=new List<Vector3>();for(int j=0;j<polygon.Count;j++){var a=polygon[j];var b=polygon[(j+1)%polygon.Count];bool aIn=plane.Item2?a.Y>=plane.Item1:a.Y<=plane.Item1,bIn=plane.Item2?b.Y>=plane.Item1:b.Y<=plane.Item1;if(aIn)clipped.Add(a);if(aIn!=bIn)clipped.Add(a.Lerp(b,(plane.Item1-a.Y)/(b.Y-a.Y)));}polygon=clipped;}for(int j=1;j<polygon.Count-1;j++)result.Triangle(polygon[0]+offset,polygon[j]+offset,polygon[j+1]+offset,colors[i]);}return result;}
  public ArrayMesh Mesh(){var sums=new Dictionary<Vector3,Vector3>();for(int i=0;i<vertices.Count;i++){sums.TryGetValue(vertices[i],out var n);sums[vertices[i]]=n+normals[i];}for(int i=0;i<normals.Count;i++)normals[i]=sums[vertices[i]].Normalized();var arrays=new Godot.Collections.Array();arrays.Resize((int)Godot.Mesh.ArrayType.Max);arrays[(int)Godot.Mesh.ArrayType.Vertex]=vertices.ToArray();arrays[(int)Godot.Mesh.ArrayType.Normal]=normals.ToArray();arrays[(int)Godot.Mesh.ArrayType.Color]=colors.ToArray();var mesh=new ArrayMesh();mesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles,arrays);return mesh;}
 }
 Color FigureInk=>new Color(.065f,.065f,.057f);
 void FigureMesh(Node3D parent,string key,Action<FigureSculpt> sculpt,float minY=float.NegativeInfinity,float maxY=float.PositiveInfinity,Vector3 offset=default){if(!characterMeshes.TryGetValue(key,out var mesh)){var builder=new FigureSculpt();sculpt(builder);characterMeshes[key]=mesh=builder.Section(minY,maxY,offset).Mesh();}if(characterPigment==null){characterPigment=new ShaderMaterial{Shader=GD.Load<Shader>("res://Shaders/character-ink.gdshader"),NextPass=new ShaderMaterial{Shader=GD.Load<Shader>("res://Shaders/character-outline.gdshader")}};}if((key.EndsWith("/head")||key.Contains("/hand/"))&&characterFacePigment==null){var outline=new ShaderMaterial{Shader=GD.Load<Shader>("res://Shaders/character-outline.gdshader")};outline.SetShaderParameter("ink_width",.0006f);characterFacePigment=new ShaderMaterial{Shader=characterPigment.Shader,NextPass=outline};}parent.AddChild(new MeshInstance3D{Mesh=mesh,MaterialOverride=(key.EndsWith("/head")||key.Contains("/hand/"))?characterFacePigment:characterPigment});}
 void RebuiltCharacter(Node3D root,int tier,int night,bool survivor){
  string variant=survivor?"survivor":night==2?"heavy":night==1?"nocturnal":"infected";
  Color coat=survivor?new Color(.47f,.40f,.25f):night>0?new Color(.29f,.33f,.31f):new Color(.43f,.33f,.23f),pants=new Color(.24f,.25f,.23f),skin=survivor?new Color(.57f,.49f,.40f):new Color(.48f,.46f,.40f),shirt=new Color(.26f,.29f,.24f);
  skin.A=.12f;var body=new Node3D{Name="Torso",Position=new Vector3(0,.96f,0),RotationDegrees=survivor?Vector3.Zero:new Vector3(-9,0,-4)};root.AddChild(body);
  FigureMesh(body,variant+"/body",b=>SculptCoat(b,coat,shirt,survivor,night));
  var head=new Node3D{Name="Head",Position=new Vector3(0,.525f,survivor?-.015f:-.06f),RotationDegrees=survivor?new Vector3(0,-3,0):new Vector3(8,0,9)};body.AddChild(head);
  FigureMesh(head,variant+"/head",b=>SculptHead(b,skin,survivor,night));
  for(int side=-1;side<=1;side+=2){int s=side;var leg=new Node3D{Name=s<0?"LeftLeg":"RightLeg",Position=new Vector3(s*.108f,.91f,0)};root.AddChild(leg);FigureMesh(leg,variant+"/thigh/"+s,b=>SculptLeg(b,pants,s,survivor),minY:-.39f);var knee=new Node3D{Name="Knee",Position=new Vector3(0,-.39f,-.02f)};leg.AddChild(knee);FigureMesh(knee,variant+"/shin/"+s,b=>SculptLeg(b,pants,s,survivor),maxY:-.385f,offset:-knee.Position);
   var arm=new Node3D{Name=s<0?"LeftArm":"RightArm",Position=new Vector3(s*.200f,1.34f,survivor?0:-.07f)};root.AddChild(arm);FigureMesh(arm,variant+"/upper-arm/"+s,b=>SculptArm(b,coat,skin,s,survivor),minY:-.255f);var elbow=new Node3D{Name="Elbow",Position=new Vector3(s*.065f,-.255f,-.035f)};arm.AddChild(elbow);FigureMesh(elbow,variant+"/forearm/"+s,b=>SculptArm(b,coat,skin,s,survivor),maxY:-.25f,offset:-elbow.Position);var hand=new Node3D{Name="Hand",Position=new Vector3(s*.04f,-.49f,survivor?-.055f:-.13f)-elbow.Position};elbow.AddChild(hand);FigureMesh(hand,variant+"/hand/"+s,b=>SculptHand(b,Vector3.Zero,skin,s,survivor));
  }
  PoseFigure(root,.7f,0,!survivor);
 }
 void SculptCoat(FigureSculpt b,Color coat,Color shirt,bool survivor,int night){RefinedTorso(b,coat,shirt,survivor,night);}
 void SculptHead(FigureSculpt b,Color skin,bool survivor,int night){
  skin.A=.12f;var shadow=skin*(survivor?.91f:.81f);var bone=skin*1.10f;var ink=FigureInk;
  b.Loft(skin,new[]{new Vector3(0,-.08f,.01f),new Vector3(0,.015f,0),new Vector3(0,.06f,0)},new[]{new Vector2(.052f,.055f),new Vector2(.047f,.052f),new Vector2(.062f,.058f)},10);
  SculptFaceShell(b,skin);
  for(int s=-1;s<=1;s+=2){
   
   FacePanel(b,ink,new Vector3(s*.019f,.151f,-.094f),new Vector3(s*.064f,.153f,-.089f),new Vector3(s*.060f,.165f,-.093f),new Vector3(s*.020f,.164f,-.096f));
   FacePanel(b,survivor?new Color(.73f,.71f,.62f):new Color(.57f,.56f,.47f),new Vector3(s*.025f,.154f,-.097f),new Vector3(s*.060f,.155f,-.096f),new Vector3(s*.058f,.162f,-.096f),new Vector3(s*.027f,.163f,-.098f));FacePanel(b,night>0?new Color(.51f,.63f,.44f):new Color(.16f,.16f,.12f),new Vector3(s*.037f,.154f,-.099f),new Vector3(s*.046f,.155f,-.098f),new Vector3(s*.046f,.162f,-.098f),new Vector3(s*.037f,.163f,-.100f));
   FaceStitch(b,skin*.75f,new Vector3(s*.018f,.177f,-.091f),new Vector3(s*.070f,.174f,-.084f),.003f);
   
   b.Loft(skin,new[]{new Vector3(s*.095f,.10f,.005f),new Vector3(s*.102f,.145f,.006f),new Vector3(s*.092f,.18f,.01f)},new[]{new Vector2(.015f,.018f),new Vector2(.020f,.023f),new Vector2(.013f,.016f)},8);
   if(!survivor)FaceStitch(b,shadow,new Vector3(s*.024f,.105f,-.097f),new Vector3(s*.037f,.072f,-.097f),.002f);
  }
  FacePanel(b,survivor?skin*.59f:ink,new Vector3(-.027f,.071f,-.095f),new Vector3(.027f,.069f,-.095f),new Vector3(.022f,survivor?.073f:.077f,-.099f),new Vector3(-.025f,survivor?.074f:.078f,-.100f));
  FaceStitch(b,skin*.78f,new Vector3(-.022f,.058f,-.092f),new Vector3(.020f,.057f,-.092f),.003f);
  for(int side=-1;side<=1;side+=2)FaceStitch(b,skin*.80f,new Vector3(side*.011f,.120f,-.096f),new Vector3(side*.009f,.158f,-.095f),.001f);
  Color hair=new Color(.085f,.073f,.05f);
  b.Loft(hair,new[]{new Vector3(0,.229f,.012f),new Vector3(0,.27f,.02f),new Vector3(0,.298f,.03f),new Vector3(-.015f,.308f,.025f)},new[]{new Vector2(.095f,.094f),new Vector2(.084f,.086f),new Vector2(.050f,.046f),new Vector2(.008f,.008f)},14,ragged:true);
  for(int i=0;i<5;i++)b.Panel(hair*.8f,new Vector3(-.075f+i*.031f,.245f,-.095f),new Vector3(-.057f+i*.029f,.234f-(i%3)*.002f,-.092f),new Vector3(-.040f+i*.029f,.245f,-.097f));
  if(!survivor){b.Stitch(new Color(.28f,.24f,.16f),new Vector3(-.07f,.12f,-.091f),new Vector3(-.045f,.083f,-.094f),.004f);b.Panel(shadow,new Vector3(-.033f,.045f,-.078f),new Vector3(.03f,.043f,-.079f),new Vector3(.048f,.067f,-.084f),new Vector3(-.046f,.070f,-.088f));}
 }
 void SculptLeg(FigureSculpt b,Color pants,int s,bool survivor){
  b.Loft(pants,new[]{new Vector3(s*.015f,-.77f,.012f),new Vector3(s*.02f,-.64f,.025f),new Vector3(s*.018f,-.42f,-.02f),new Vector3(s*.008f,-.35f,-.025f),new Vector3(0,-.12f,.008f),Vector3.Zero},new[]{new Vector2(.058f,.065f),new Vector2(.071f,.071f),new Vector2(.077f,.078f),new Vector2(.087f,.088f),new Vector2(.104f,.105f),new Vector2(.103f,.11f)},12,ragged:!survivor);
  b.Panel(pants*.67f,new Vector3(-.065f,-.43f,-.081f),new Vector3(.065f,-.44f,-.081f),new Vector3(.06f,-.38f,-.10f),new Vector3(-.06f,-.37f,-.10f));
  for(int i=0;i<4;i++)b.Stitch(pants*.55f,new Vector3(-.06f,-.46f-i*.035f,-.070f),new Vector3(.05f,-.475f-i*.028f,-.070f),.002f);
  Color leather=new Color(.18f,.17f,.135f);
  b.Loft(leather,new[]{new Vector3(s*.015f,-.87f,-.045f),new Vector3(s*.015f,-.83f,-.048f),new Vector3(s*.015f,-.77f,-.027f),new Vector3(s*.015f,-.70f,.016f)},new[]{new Vector2(.073f,.15f),new Vector2(.078f,.147f),new Vector2(.069f,.12f),new Vector2(.062f,.070f)},12);
  b.Loft(FigureInk,new[]{new Vector3(s*.015f,-.90f,-.048f),new Vector3(s*.015f,-.867f,-.048f)},new[]{new Vector2(.079f,.153f),new Vector2(.079f,.153f)},12);
  for(int i=0;i<4;i++)b.Stitch(new Color(.42f,.37f,.27f),new Vector3(-.037f,-.77f+i*.014f,-.108f),new Vector3(.05f,-.76f+i*.014f,-.11f),.002f);
 }
 void SculptArm(FigureSculpt b,Color coat,Color skin,int s,bool survivor){RefinedSleeve(b,coat,s,survivor);}
 void SculptHand(FigureSculpt b,Vector3 wrist,Color skin,int side,bool gloved){RefinedHand(b,wrist,skin,side,gloved);}
 void RebuiltFirstPersonArms(Node3D root,bool pistol){RefinedWeaponHands(root,pistol);}
}

