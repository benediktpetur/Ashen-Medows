using Godot;
public partial class MigrationScene {
 void UpdateFriendlyTool(CampSurvivor n,FriendlyAvatar a){string mode=n.Pose=="fireside"?"":n.Member?(!string.IsNullOrEmpty(n.Owner)||n.Job=="guard"?"rifle":n.Job):"";if(a.ToolMode==mode)return;if(GodotObject.IsInstanceValid(a.Tool))a.Tool.QueueFree();a.Tool=null;a.ToolMode=mode;if(mode=="")return;var hand=a.Mover.Visual.GetNode<Node3D>("RightArm/Elbow/Hand");a.Tool=new Node3D{Position=new(0,-.03f,-.08f),Scale=Vector3.One*.65f};hand.AddChild(a.Tool);if(mode=="rifle"){a.Tool.RotationDegrees=new(80,0,0);ItemWeapon(a.Tool,1,false);}else if(mode=="wood")ItemTool(a.Tool,0);else Box(a.Tool,new(0,-.2f,0),new(.35f,.3f,.3f),Wood);}
}
