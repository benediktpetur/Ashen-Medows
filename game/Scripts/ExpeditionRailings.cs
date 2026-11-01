using Godot;
using System;
public partial class MigrationScene {
 async void RunRailingChecks(){try{
  game.NoDiskSave=true;paused=true;SetProcess(false);SetPhysicsProcess(false);
  int count=0;
  for(int z=-2600;z<240;z+=2)if(RailingSpan(z,out var a,out var b)){
   count++;
   for(int i=0;i<=40;i++){
    var p=a.Lerp(b,i/40f);
    MigrationChecks.Assert(!game.World.MainRoad(p.X,p.Z,1)&&!game.World.BriarRoad(p.X,p.Z,1)&&!game.World.ResidentialPaved(p.X,p.Z)&&!game.World.UrbanStreet(p.X,p.Z,1),"Railing stays outside road and junctions");
   }
  }
  MigrationChecks.Assert(count>30,"Roadside railings remain populated");
  foreach(bool simulation in new[]{false,true}){
   var root=new Node3D{Position=new(0,1000,0)};AddChild(root);
   RailingMember(root,new(0,0,-1),new(0,0,1),.05f,simulation);
   for(int i=0;i<3;i++)await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
   var hit=GetWorld3D().DirectSpaceState.IntersectRay(PhysicsRayQueryParameters3D.Create(new(-1,1000,0),new(1,1000,0)));
   MigrationChecks.Assert(hit.Count>0&&hit["collider"].AsGodotObject() is StaticBody3D body&&body.Name=="RailingCollision","Visible and simulation rail collision blocks crossing");
   root.Free();
  }
  GD.Print($"PASS / railings / {count} safe spans / road and junction clearance / visible and simulation physics");GetTree().Quit();
 }catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
 // Sample the actual paved surface along every span, rather than joining distant
 // route points with a chord that can cut through a bend or a junction.
 bool RailingSpan(int z,out Vector3 a,out Vector3 b){
  double x=ExpeditionWorldAtlas.HighwayX(z),next=ExpeditionWorldAtlas.HighwayX(z+2);a=b=Vector3.Zero;if(!double.IsFinite(x)||!double.IsFinite(next))return false;a=Ground(x+30,z);b=Ground(next+30,z+2);
  if(a.DistanceTo(b)>4||Math.Abs(a.Y-b.Y)>.6f)return false;
  for(int i=0;i<=16;i++){
   var p=a.Lerp(b,i/16f);
   if(game.World.StreetDistance(p.X,p.Z)>35||!game.World.Playable(p.X,p.Z)||game.World.Tier(p.X,p.Z)>2||
      game.World.SurfaceClearance(p.X,p.Z,2)||SuburbPaved(p.X,p.Z)||
      game.World.ResidentialInterior(p.X,p.Z)||game.World.NearRoadsideStop(p.X,p.Z,4)||
      InEncounterLot(p.X,p.Z)||InCrashLot(p.X,p.Z)||InCheckpointLot(p.X,p.Z)||
      InBirthdayLot(p.X,p.Z)||InLetterLot(p.X,p.Z)||
      Math.Abs(Ground(p.X,p.Z).Y-p.Y)>.25f)return false;
  }
  return true;
 }
 void RailingMember(Node3D root,Vector3 a,Vector3 b,float radius,bool simulation){
  var delta=b-a;
  if(!simulation)Tube(root,a,b,radius,Wood);
  // Use the same endpoints, rotation and radius in visible and simulation chunks.
  var body=new StaticBody3D{Name="RailingCollision",Position=(a+b)*.5f,
   Quaternion=new Quaternion(Vector3.Up,delta.Normalized())};
  body.AddChild(new CollisionShape3D{Shape=new CylinderShape3D{Radius=radius,Height=delta.Length()}});
  root.AddChild(body);
 }
 void RoadsideRailings(Node3D root,int cx,int cz,bool simulation){
  for(int z=cz*64;z<(cz+1)*64;z+=2){
   double x=ExpeditionWorldAtlas.HighwayX(z)+30;
   if(x<cx*64||x>=(cx+1)*64||!RailingSpan(z,out var a,out var b))continue;
   var section=new Node3D{Name="RoadsideRailing_"+z};root.AddChild(section);
   RailingMember(section,a,a+Vector3.Up*1.15f,.055f,simulation);
   // Adjacent accepted spans share a post; a rejected span ends with a post.
   if(!RailingSpan(z+2,out _,out _))RailingMember(section,b,b+Vector3.Up*1.15f,.055f,simulation);
   foreach(float height in new[]{.45f,.9f})RailingMember(section,a+Vector3.Up*height,b+Vector3.Up*height,.045f,simulation);
  }
 }
 void GardenRailings(Node3D house,bool simulation){
  // Side boundaries only: the front path and driveway remain open.
  foreach(int side in new[]{-1,1}){
   Vector3 previous=default;
   for(int j=0;j<9;j++){
    var point=new Vector3(side*9,0,-5+j*1.7f);var world=house.Transform*point;
    point.Y=SurfaceGround(world.X,world.Z).Y-house.Position.Y;
    RailingMember(house,point,point+Vector3.Up*1.2f,.055f,simulation);
    if(j>0)foreach(float h in new[]{.4f,.9f})RailingMember(house,previous+Vector3.Up*h,point+Vector3.Up*h,.05f,simulation);
    previous=point;
   }
  }
 }
}

