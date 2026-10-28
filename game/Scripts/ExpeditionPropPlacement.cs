using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 const float BriarCarSide=7.5f;
 void GroundVehicle(Node3D car){
  var p=car.GlobalPosition;var right=car.GlobalBasis.X.Normalized();var back=car.GlobalBasis.Z.Normalized();
  Vector3 Sample(Vector3 delta)=>SurfaceGround(p.X+delta.X,p.Z+delta.Z);
  var across=Sample(right*.9f)-Sample(-right*.9f);var along=Sample(back*1.35f)-Sample(-back*1.35f);
  var up=along.Cross(across).Normalized();if(up.Y<0)up=-up;
  right=up.Cross(back).Normalized();back=right.Cross(up).Normalized();
  var basis=new Basis(right,up,back);float height=float.NegativeInfinity;
  foreach(int sx in new[]{-1,1})foreach(int sz in new[]{-1,1}){var delta=basis*new Vector3(sx*.9f,0,sz*1.35f);height=Math.Max(height,Sample(delta).Y-delta.Y);}
  car.GlobalTransform=new Transform3D(basis,new(p.X,height,p.Z));
 }
 void PropPlacementChecks(){
  foreach(int kind in Enumerable.Range(0,4)){
   var root=new Node3D{Position=new(31,8,-29),RotationDegrees=new(0,90,0),Scale=new(1.1f,1,.9f)};AddChild(root);
   var target=new Aabb(new Vector3(-1,-.4f,-.6f),new Vector3(2,1.2f,1.2f));
   Box(root,target.GetCenter(),target.Size,Wood);ApplyWorldFurniture(root,kind);
   var model=root.GetNode<Node3D>("GeneratedWorldProp");Aabb fitted=default;bool found=false;BatchBounds(model,Transform3D.Identity,ref fitted,ref found);
   MigrationChecks.Assert(found&&Math.Abs(fitted.Position.Y-target.Position.Y)<.01f,"Furniture feet use local target floor / "+kind);
   MigrationChecks.Assert(fitted.Position.X>=target.Position.X-.01f&&fitted.End.X<=target.End.X+.01f&&fitted.Position.Z>=target.Position.Z-.01f&&fitted.End.Z<=target.End.Z+.01f,"Furniture remains in local envelope with transformed parent / "+kind);root.Free();
  }
  foreach(bool burned in new[]{false,true}){
   var root=new Node3D();AddChild(root);ArtWreck(root,burned);var model=root.GetNode<Node3D>("GeneratedWorldProp");Aabb b=default;bool found=false;BatchBounds(model,Transform3D.Identity,ref b,ref found);
   MigrationChecks.Assert(found&&Math.Abs(b.Position.Y)<.01f&&b.Size.X<=2.11f&&b.Size.Z<=4.21f,"Wreck wheels grounded and mesh inside collision envelope");
   MigrationChecks.Assert(BriarCarSide+b.End.X<8.9f&&BriarCarSide+b.Position.X>6.1f,"Parked car clears fence and house wall");root.Free();
  }
  var reference=new Node3D();ItemMaterial(reference,3);Aabb resourceTarget=default;bool resourceFound=false;LocalMeshBounds(reference,ref resourceTarget,ref resourceFound);
  var moved=new Node3D{Position=new(43,9,-32)};AddChild(moved);BatchResource(moved,3);Aabb resourceBounds=default;bool resourceLoaded=false;BatchBounds(moved.GetNode("BatchModel"),Transform3D.Identity,ref resourceBounds,ref resourceLoaded);
  MigrationChecks.Assert(resourceFound&&resourceLoaded&&resourceBounds.GetCenter().DistanceTo(resourceTarget.GetCenter())<.01f,"Resource replacement remains in local target with translated parent");reference.Free();moved.Free();
  GD.Print("PASS / prop placement / transformed furniture local bounds / grounded car models / fence and house clearance");
 }
}
