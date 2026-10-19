using Godot;
public partial class MigrationScene {
 void HuntingToolArt(Node3D root,int tool){
  Tube(root,new(0,.03f,0),new(0,.25f,0),.035f,Wood);
  for(int i=0;i<5;i++)Tube(root,new(-.035f,.07f+i*.03f,0),new(.035f,.085f+i*.03f,0),.008f,new Color(.49f,.41f,.27f));
  if(tool==1){Bevel(root,new(.015f,.35f,0),new(.085f,.24f,.025f),new Color(.42f,.44f,.40f));Bevel(root,new(.025f,.47f,0),new(.045f,.08f,.022f),Steel);}
  else{Bevel(root,new(.10f,.37f,0),new(.26f,.23f,.035f),new Color(.37f,.40f,.38f));Box(root,new(.10f,.49f,0),new(.25f,.012f,.022f),Steel);}
 }
}
