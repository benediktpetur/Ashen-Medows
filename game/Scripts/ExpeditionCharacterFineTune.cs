using Godot;
using System;
public partial class MigrationScene {
 static readonly float[] faceY={.018f,.045f,.075f,.10f,.125f,.145f,.16f,.18f,.205f,.235f,.265f,.29f,.298f};
 static readonly float[] faceWidth={.048f,.067f,.080f,.088f,.096f,.097f,.094f,.092f,.092f,.090f,.078f,.045f,.012f};
 static readonly float[] faceDepth={.058f,.068f,.079f,.084f,.087f,.086f,.083f,.085f,.087f,.086f,.074f,.044f,.018f};
 static float Bell(float x,float center,float width)=>Mathf.Exp(-Mathf.Pow((x-center)/width,2));
 static void FaceSection(float y,out float width,out float depth,out float center){int i=0;while(i<faceY.Length-2&&y>faceY[i+1])i++;float t=Math.Clamp((y-faceY[i])/(faceY[i+1]-faceY[i]),0,1);width=Mathf.Lerp(faceWidth[i],faceWidth[i+1],t);depth=Mathf.Lerp(faceDepth[i],faceDepth[i+1],t);center=Mathf.Lerp(-.014f,.024f,Math.Clamp(y/.28f,0,1));}
 static float FaceSurface(float x,float y){FaceSection(y,out var width,out var depth,out var center);float front=Mathf.Sqrt(Math.Max(0,1-x*x/(width*width)));float z=center-front*depth;
  z-=Bell(x,0,.022f)*Bell(y,.132f,.027f)*.026f; // continuous bridge and nose tip
  z-=Bell(Math.Abs(x),.060f,.025f)*Bell(y,.125f,.023f)*.007f; // cheekbone
  z+=Bell(Math.Abs(x),.043f,.022f)*Bell(y,.159f,.012f)*.006f; // eye socket
  z-=Bell(Math.Abs(x),.043f,.028f)*Bell(y,.181f,.010f)*.005f; // brow
  return z;
 }
 void SculptFaceShell(FigureSculpt mesh,Color skin){const int sides=24;var rings=new Vector3[faceY.Length][];for(int r=0;r<faceY.Length;r++){float y=faceY[r];FaceSection(y,out float width,out float depth,out float center);rings[r]=new Vector3[sides];for(int i=0;i<sides;i++){float a=i*Mathf.Tau/sides,x=Mathf.Sin(a)*width,z=center+Mathf.Cos(a)*depth;if(Mathf.Cos(a)<0)z=FaceSurface(x,y);rings[r][i]=new Vector3(x,y,z);}}
  for(int r=0;r<rings.Length-1;r++)for(int i=0;i<sides;i++){int j=(i+1)%sides;mesh.Triangle(rings[r][i],rings[r+1][i],rings[r+1][j],skin);mesh.Triangle(rings[r][i],rings[r+1][j],rings[r][j],skin);}
 }
 static Vector3 FacePoint(Vector3 p)=>new Vector3(p.X,p.Y,FaceSurface(p.X,p.Y)+(p.Z+.09f)*.2f-.0025f);
 void FacePanel(FigureSculpt b,Color color,params Vector3[] points){var center=Vector3.Zero;foreach(var p in points)center+=p;center/=points.Length;for(int i=0;i<points.Length;i++){var a=points[i];var c=points[(i+1)%points.Length];var mid=(a+c)*.5f;b.Panel(color,FacePoint(center),FacePoint(a),FacePoint(mid));b.Panel(color,FacePoint(center),FacePoint(mid),FacePoint(c));}}
 void FaceStitch(FigureSculpt b,Color color,Vector3 a,Vector3 c,float width=.002f){var d=(c-a).Cross(Vector3.Forward).Normalized()*width;FacePanel(b,color,a-d,c-d,c+d,a+d);}
 void PoseFigure(Node3D root,float phase,float stride,bool infected){if(!root.HasNode("LeftLeg/Knee"))return;float swing=Mathf.Sin(phase);var left=root.GetNode<Node3D>("LeftLeg");var right=root.GetNode<Node3D>("RightLeg");left.Rotation=new Vector3(swing*.30f*stride,0,.025f);right.Rotation=new Vector3(-swing*.30f*stride,0,-.025f);left.GetNode<Node3D>("Knee").Rotation=new Vector3(.055f+Math.Max(0,-swing)*.48f*stride,0,0);right.GetNode<Node3D>("Knee").Rotation=new Vector3(.055f+Math.Max(0,swing)*.48f*stride,0,0);
  var la=root.GetNode<Node3D>("LeftArm");var ra=root.GetNode<Node3D>("RightArm");la.Rotation=new Vector3((infected?.16f:.02f)-swing*.18f*stride,0,.045f);ra.Rotation=new Vector3((infected?.12f:.02f)+swing*.18f*stride,0,-.045f);la.GetNode<Node3D>("Elbow").Rotation=new Vector3(.12f+(infected?.30f:.04f)+swing*.04f*stride,0,0);ra.GetNode<Node3D>("Elbow").Rotation=new Vector3(.12f+(infected?.26f:.03f)-swing*.04f*stride,0,0);
  float lift=.91f+Math.Min(FigureSoleHeight(left),FigureSoleHeight(right));left.Position=new Vector3(-.095f,.91f-lift,0);right.Position=new Vector3(.095f,.91f-lift,0);la.Position=new Vector3(-.198f,1.335f-lift,infected?-.07f:0);ra.Position=new Vector3(.198f,1.335f-lift,infected?-.07f:0);
  var torso=root.GetNode<Node3D>("Torso");torso.Position=new Vector3(0,.96f-lift,0);torso.RotationDegrees=new Vector3(infected?-9:0,Mathf.Sin(phase)*2*stride,infected?-3+Mathf.Sin(phase)*1.4f:Mathf.Sin(phase)*.6f*stride);
 }
 static float FigureSoleHeight(Node3D leg){var knee=leg.GetNode<Node3D>("Knee");float toe=(leg.Basis*(knee.Position+knee.Basis*new Vector3(0,-.51f,-.18f))).Y;float heel=(leg.Basis*(knee.Position+knee.Basis*new Vector3(0,-.51f,.115f))).Y;return Math.Min(toe,heel);}
}


