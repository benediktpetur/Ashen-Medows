using Godot;
public partial class MigrationScene {
 void HuntingMaterialArt(Node3D n,int index){Color hide=new(.44f,.30f,.19f),bone=new(.75f,.71f,.56f);
  if(index==0){Ellipsoid(n,new(0,.12f,0),new(.25f,.10f,.16f),new(.48f,.16f,.13f));for(int i=0;i<3;i++)Tube(n,new(-.16f+i*.12f,.20f,-.08f),new(-.12f+i*.12f,.20f,.08f),.012f,new(.71f,.59f,.45f));}
  else if(index==1||index==2){Bevel(n,new(0,.04f,0),new(.4f,.04f,.5f),hide);if(index==2)for(int i=0;i<16;i++)Tube(n,new((i%4-1.5f)*.09f,.06f,(i/4-1.5f)*.12f),new((i%4-1.5f)*.09f+.02f,.12f,(i/4-1.5f)*.12f),.025f,new(.49f,.46f,.36f),.004f);}
  else if(index==3){Tube(n,new(-.2f,.09f,0),new(.2f,.09f,0),.035f,bone);for(int s=-1;s<=1;s+=2)for(int j=-1;j<=1;j+=2)Ellipsoid(n,new(s*.2f,.09f,j*.035f),new(.055f,.05f,.045f),bone);}
  else if(index==4){for(int i=0;i<4;i++)Tube(n,new(-.18f,.04f+i*.02f,-.08f),new(.18f,.04f+i*.02f,.08f),.012f,new(.68f,.56f,.35f));}
  else{Tube(n,new(-.23f,.12f,0),new(.23f,.12f,0),.12f,hide);for(int s=-1;s<=1;s+=2)Box(n,new(s*.13f,.24f,0),new(.04f,.025f,.16f),Cloth);}
 }
}
