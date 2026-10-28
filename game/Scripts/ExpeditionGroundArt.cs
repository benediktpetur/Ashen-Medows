using Godot;
using System;

public partial class MigrationScene {
 // Alpha carries a surface code; the opaque shader never uses it as opacity.
 static Color SurfaceTone(int kind)=>kind switch {
  1=>new(.32f,.26f,.18f,.125f),2=>new(.19f,.20f,.20f,.25f),
  3=>new(.47f,.45f,.39f,.375f),4=>new(.28f,.25f,.17f,.5f),
  5=>new(.38f,.35f,.28f,.625f),6=>new(.12f,.25f,.31f,.75f),
  _=>new(.26f,.30f,.16f,0)
 };
 Color GroundTone(double x,double z){
  var w=game.World;int tier=w.Tier(x,z);
  if(w.IsRiver(x,z))return SurfaceTone(6);
  if(w.Changes.OnPath(x,z))return SurfaceTone(1);
  if(w.BriarRoad(x,z)||w.UrbanStreet(x,z))return SurfaceTone(2);
  if(w.ResidentialPaved(x,z))return SurfaceTone(ExpeditionWorld.GridStreetDistance(x)<5||ExpeditionWorld.GridStreetDistance(z)<5?2:3);
  if(w.ResidentialApproach(x,z)||w.BriarApproach(x,z))return SurfaceTone(5);
  if(w.UrbanStreet(x,z,2)||w.BriarRoad(x,z,3))return SurfaceTone(3);
  if(tier>=3)return SurfaceTone(tier==3?5:3);
  return SurfaceTone(tier==2?4:0);
 }
 void GroundQuad(SurfaceTool st,Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color color){
  foreach(var p in new[]{a,b,c,a,c,d}){st.SetColor(color);st.AddVertex(p);}
 }
 Vector3 SurfaceGround(double x,double z){
  // Match the two rendered/collision triangles of the 4m terrain cell exactly.
  // Sampling the analytic hill at finer intervals would cut through that mesh.
  double ax=Math.Floor(x/4)*4,az=Math.Floor(z/4)*4,u=(x-ax)/4,v=(z-az)/4;
  double a=game.World.Height(ax,az),b=game.World.Height(ax+4,az),c=game.World.Height(ax+4,az+4),d=game.World.Height(ax,az+4);
  double h=u>=v?a+(b-a)*u+(c-b)*v:a+(c-d)*u+(d-a)*v;
  return new((float)x,(float)h,(float)z);
 }
 void HouseApproach(Node3D house){
  // All residential houses face along local +Z. Briar Lane meets its central
  // lane; other homes meet the existing 128m neighbourhood street grid.
  var direction=house.Basis*Vector3.Back;double street;
  if(house.Name.ToString().StartsWith("BriarLaneHouse"))street=ExpeditionWorld.SpawnX;
  else street=Math.Round((house.Position.X+Math.Sign(direction.X)*32)/128)*128;
  float end=(float)Math.Abs(street-house.Position.X)/Math.Max(.01f,Math.Abs(direction.X))-7;
  if(end<=13)return;
  var st=new SurfaceTool();st.Begin(Mesh.PrimitiveType.Triangles);
  for(float z=13;z<end;z+=1){float next=Math.Min(z+1,end);
   Vector3 At(float x,float zz){var p=house.Transform*new Vector3(x,0,zz);return SurfaceGround(p.X,p.Z)+Vector3.Up*.09f;}
   GroundQuad(st,At(-1.4f,z),At(-1.4f,next),At(1.4f,next),At(1.4f,z),SurfaceTone(5));
  }
  st.GenerateNormals();house.GetParent().AddChild(new MeshInstance3D{Name="StreetApproach",Mesh=st.Commit(),MaterialOverride=TerrainInk(),CastShadow=GeometryInstance3D.ShadowCastingSetting.Off});
 }
 void HouseFoundation(Node3D n,Vector2 size,bool simulation,Vector3 offset=default){
  float lowest=n.Position.Y;for(float x=-size.X/2;x<=size.X/2+.01f;x+=size.X/4)for(float z=-size.Y/2;z<=size.Y/2+.01f;z+=size.Y/4){var p=n.Transform*(new Vector3(x,0,z)+offset);lowest=Math.Min(lowest,(float)game.World.Height(p.X,p.Z));}
  float bottom=lowest-n.Position.Y-.35f,top=-.08f,depth=Math.Max(.6f,top-bottom);
  var center=new Vector3(0,top-depth/2,0)+offset;var dimensions=new Vector3(size.X,depth,size.Y);
  if(simulation)PhysicsObstacle(n,new BoxShape3D{Size=dimensions},center);
  else{var foundation=Box(n,center,dimensions,new(.36f,.35f,.31f),true);foundation.Name="ContinuousFoundation";foundation.MaterialOverride=ExteriorFinish(4,new(.36f,.35f,.31f));}
 }
}
