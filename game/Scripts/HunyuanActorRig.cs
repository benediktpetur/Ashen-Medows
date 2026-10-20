using Godot;
using System;

// Keeps the existing gameplay articulation as the animation authority.
// Generated meshes have no rig: shared skin weights are built once per model.
public partial class HunyuanActorRig : Node3D {
 public Node3D Actor;
 public Skeleton3D Skeleton;
 public Node3D[] Anchors;
 public Transform3D[] Bind;
 public void UpdatePose(){
  if(Actor==null||Skeleton==null)return;
  for(int i=0;i<Anchors.Length;i++){
   var current=Relative(Anchors[i],Actor);
   Skeleton.SetBonePosePosition(i,Skeleton.GetBoneRest(i).Origin+current.Origin-Bind[i].Origin);
   Skeleton.SetBonePoseRotation(i,(current.Basis.Orthonormalized()*Bind[i].Basis.Orthonormalized().Inverse()).GetRotationQuaternion());
   Skeleton.SetBonePoseScale(i,new Vector3(current.Basis.X.Length()/Bind[i].Basis.X.Length(),current.Basis.Y.Length()/Bind[i].Basis.Y.Length(),current.Basis.Z.Length()/Bind[i].Basis.Z.Length()));
  }
 }
 public static Transform3D Relative(Node3D node,Node3D root){if(node==root)return Transform3D.Identity;var t=node.Transform;for(var p=node.GetParent();p!=null&&p!=root;p=p.GetParent())if(p is Node3D n)t=n.Transform*t;return t;}
 public override void _Process(double delta){if(Actor.IsVisibleInTree())UpdatePose();}
 public override void _ExitTree(){SetProcess(false);Actor=null;Skeleton=null;Anchors=Array.Empty<Node3D>();Bind=Array.Empty<Transform3D>();}
}
