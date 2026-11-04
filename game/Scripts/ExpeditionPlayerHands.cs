using Godot;
using System;
using System.Collections.Generic;
public partial class MigrationScene {
 int handStyle=1; // Bare, fingerless, full gloves.
 MeshInstance3D rightUpperSleeve,leftUpperSleeve;
 static Vector3 HeldWrist(bool left)=>new(left?-.102f:.102f,-.016f,.010f);
 void NaturalHeldHand(Node3D grip,bool left,HeldMotion motion){
  string key=$"held-hands-v2/{handStyle}/{left}/{motion}";
  if(!characterMeshes.TryGetValue(key,out var mesh)){
   var hand=new FigureSculpt();var skin=new Color(.57f,.38f,.27f);var leather=new Color(.20f,.21f,.19f);var palmColor=handStyle==0?skin:leather;
   // Palm runs from the wrist toward the knuckles; fingers emerge from its distal edge.
   var palm=new FigureSculpt();palm.Loft(palmColor,new[]{new Vector3(0,-.108f,0),new Vector3(0,-.085f,.002f),new Vector3(0,-.055f,.004f),new Vector3(0,-.015f,.002f),new Vector3(0,.021f,0)},new[]{new Vector2(.021f,.017f),new Vector2(.025f,.019f),new Vector2(.037f,.022f),new Vector2(.042f,.020f),new Vector2(.037f,.016f)},24);
   hand.Append(palm,new Transform3D(Basis.FromEuler(new(0,0,Mathf.Pi/2)),new(0,0,.009f)));
   float[] row={.032f,.011f,-.010f,-.029f};float[] length={.94f,1,.94f,.78f};
   for(int finger=0;finger<4;finger++){
    float y=row[finger],scale=length[finger];bool bowDraw=motion==HeldMotion.Bow&&!left;
    var path=new[]{new Vector3(-.018f,y,.009f),new Vector3(-.037f,y,.003f),new Vector3(-.058f*scale,y,-.011f),new Vector3(-.064f*scale,y,-.033f),new Vector3(-.048f*scale,y-.002f,-.054f),new Vector3(-.023f*scale,y-.003f,bowDraw?-.045f:-.057f)};
    SculptHeldDigit(hand,path,finger==3?.0074f:.0086f,skin,leather,handStyle);
   }
   var thumb=new[]{new Vector3(.034f,.023f,.021f),new Vector3(.022f,.048f,.025f),new Vector3(-.003f,.054f,.017f),new Vector3(-.031f,.047f,-.002f),new Vector3(-.044f,.030f,-.017f)};
   SculptHeldDigit(hand,thumb,.011f,skin,leather,handStyle);
   // Thenar pad blends the thumb into the palm without detached cylindrical joints.
   hand.Limb(palmColor,new(.045f,.014f,.015f),new(.012f,.034f,.019f),.018f,.014f,.016f,.013f,16);
   if(left){var mirrored=new FigureSculpt();mirrored.Append(hand,new Transform3D(Basis.Identity.Scaled(new Vector3(-1,1,1)),Vector3.Zero));hand=mirrored;}
   characterMeshes[key]=mesh=hand.Mesh();
  }
  grip.AddChild(new MeshInstance3D{Name="AnatomicalHand",Mesh=mesh,MaterialOverride=new StandardMaterial3D{VertexColorUseAsAlbedo=true,Roughness=.78f,Metallic=0},CastShadow=GeometryInstance3D.ShadowCastingSetting.Off});
 }
 static void SculptHeldDigit(FigureSculpt output,Vector3[] controls,float radius,Color skin,Color glove,int style){
  var points=new List<Vector3>();
  for(int i=0;i<controls.Length-1;i++)for(int step=0;step<4;step++){
   float t=step/4f;var a=controls[Math.Max(0,i-1)];var b=controls[i];var c=controls[i+1];var d=controls[Math.Min(controls.Length-1,i+2)];
   points.Add(.5f*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t));
  }
  points.Add(controls[^1]);const int sides=14;var rings=new Vector3[points.Count][];
  for(int i=0;i<points.Count;i++){
   var tangent=(points[Math.Min(i+1,points.Count-1)]-points[Math.Max(0,i-1)]).Normalized();var width=Vector3.Up;if(Math.Abs(tangent.Dot(width))>.9)width=Vector3.Right;var normal=tangent.Cross(width).Normalized();width=normal.Cross(tangent).Normalized();float t=i/(float)(points.Count-1);float r=radius*(1-.30f*t);if(i==points.Count-1)r*=.30f;
   rings[i]=new Vector3[sides];for(int j=0;j<sides;j++){float angle=j*Mathf.Tau/sides;rings[i][j]=points[i]+width*Mathf.Cos(angle)*r+normal*Mathf.Sin(angle)*r*.91f;}
  }
  for(int i=0;i<rings.Length-1;i++){
   float progress=i/(float)(rings.Length-1);Color color=style==2||style==1&&progress<.48f?glove:skin;
   for(int j=0;j<sides;j++){int next=(j+1)%sides;output.Triangle(rings[i][j],rings[i+1][j],rings[i+1][next],color);output.Triangle(rings[i][j],rings[i+1][next],rings[i][next],color);}
  }
  Color tip=style==2?glove:skin;for(int j=0;j<sides;j++)output.Triangle(points[^1],rings[^1][j],rings[^1][(j+1)%sides],tip);
 }
 MeshInstance3D AnatomicalSleeve(float bottom,float top,bool cuff){
  string key=$"held-sleeve-v2/{bottom}/{top}/{cuff}";
  if(!characterMeshes.TryGetValue(key,out var mesh)){
   var sculpt=new FigureSculpt();var centers=new Vector3[7];var radii=new Vector2[7];
   for(int i=0;i<7;i++){float t=i/6f;centers[i]=new(0,t-.5f,0);float r=Mathf.Lerp(bottom,top,t)*(i is 1 or 4?1.035f:1);radii[i]=new(r,r*.92f);}
   sculpt.Loft(Colors.White,centers,radii,20);characterMeshes[key]=mesh=sculpt.Mesh();
  }
  var instance=new MeshInstance3D{Mesh=mesh,MaterialOverride=new StandardMaterial3D{AlbedoColor=cuff?new(.18f,.21f,.18f):new(.31f,.34f,.29f),Roughness=.94f},CastShadow=GeometryInstance3D.ShadowCastingSetting.Off};viewArms.AddChild(instance);return instance;
 }
}
