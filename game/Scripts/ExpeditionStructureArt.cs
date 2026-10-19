using Godot;
using System;
public partial class MigrationScene {
 void ArtStructure(Node3D node,BuildPiece p){var d=p.Def;if(d.Tier>1){TierStructureArt(node,p);return;}foreach(Node c in node.GetChildren())if(c is MeshInstance3D mesh)mesh.MaterialOverride=Textured(Wood,0,.65f);if(p.Kind==0){foreach(Node c in node.GetChildren())if(c is MeshInstance3D m)m.MaterialOverride=Material(new Color(.30f,.34f,.29f));Box(node,new Vector3(0,(float)d.H+.015f,0),new Vector3((float)d.W,.03f,(float)d.D),Wood);}if(p.Kind is 2 or 3){for(float y=.25f;y<d.H;y+=.25f)Box(node,new Vector3(0,y,-(float)d.D/2-.01f),new Vector3((float)d.W,.02f,.02f),new Color(.15f,.12f,.085f));for(int side=-1;side<=1;side+=2){Box(node,new Vector3(side*((float)d.W/2-.07f),(float)d.H/2,-(float)d.D/2-.02f),new Vector3(.12f,(float)d.H,.05f),Wood*.7f);for(float y=.2f;y<d.H;y+=.5f)Ellipsoid(node,new Vector3(side*((float)d.W/2-.07f),y,-(float)d.D/2-.055f),new Vector3(.018f,.018f,.008f),Steel);}}if(p.Kind is 1 or 7){for(float x=-(float)d.W/2+.25f;x<d.W/2;x+=.25f)Box(node,new Vector3(x,(float)d.H+.015f,0),new Vector3(.015f,.015f,(float)d.D),Ink);}if(p.Kind==4){for(float y=.15f;y<d.H;y+=.65f)Bevel(node,new Vector3(0,y,0),new Vector3((float)d.W+.025f,.07f,(float)d.D+.025f),Steel);}}
 void TierStructureArt(Node3D n,BuildPiece p){var d=p.Def;int t=d.Tier;float w=(float)d.W,h=(float)d.H,z=(float)d.D;
  Color stone=new(.38f,.39f,.33f),steel=new(.29f,.32f,.29f),olive=new(.24f,.29f,.20f);
  foreach(var child in n.FindChildren("*","MeshInstance3D",true,false))if(child is MeshInstance3D m)m.MaterialOverride=t==2?Textured(Wood,0,.85f):Material(t==3?steel:t==4?new Color(.48f,.47f,.40f):olive);
  if(d.BaseKind is 2 or 3){
   if(t==2){for(int row=0;row<3;row++)for(int col=0;col<6;col++)Box(n,new((col-2.5f)*w/6,row*.22f+.11f,0),new(w/6-.025f,.20f,z+.055f),stone*(.86f+(col%3)*.07f));for(int side=-1;side<=1;side+=2)Box(n,new(side*(w/2-.09f),h/2,0),new(.18f,h,z+.10f),Wood);}
   if(t>=3){for(int i=0;i<6;i++){Box(n,new((i-2.5f)*w/6,h/2,-z/2-.018f),new(.035f,h,.035f),t==3?new Color(.40f,.24f,.14f):steel);for(int j=0;j<3;j++)Ellipsoid(n,new((i-2.5f)*w/6,.2f+j*(h-.4f)/2,-z/2-.05f),new(.025f,.025f,.015f),Steel);}}
   if(t==5){Box(n,new(0,.30f,-z/2-.04f),new(w,.15f,.06f),new(.60f,.53f,.26f));Box(n,new(w*.29f,h*.65f,-z/2-.05f),new(.45f,.28f,.035f),new(.70f,.72f,.60f));for(int i=0;i<3;i++)Box(n,new(w*.29f,h*.65f-.07f+i*.07f,-z/2-.075f),new(.30f,.018f,.015f),olive);}
  }
 }
 void DressChests(){for(int i=0;i<3;i++){var p=new Vector3((float)ExpeditionDungeon.ChestX[i],0,(float)ExpeditionDungeon.ChestZ[i]);Bevel(dungeonRoot,p+new Vector3(0,.78f,0),new Vector3(.94f,.15f,.84f),Wood);for(int s=-1;s<=1;s+=2)Box(dungeonRoot,p+new Vector3(s*.3f,.4f,-.425f),new Vector3(.045f,.75f,.025f),Steel);Bevel(dungeonRoot,p+new Vector3(0,.62f,-.44f),new Vector3(.12f,.13f,.04f),new Color(.63f,.53f,.27f));}}
}
