using Godot;
using System;
public partial class MigrationScene {
 void AtlasTransportDetails(Node3D root,int cx,int cz){
  for(int z=cz*64;z<(cz+1)*64;z+=12){double x=ExpeditionWorldAtlas.HighwayX(z);if(!double.IsFinite(x)||x<cx*64||x>=(cx+1)*64||!game.World.MainRoad(x,z))continue;
   var stripe=Box(root,SurfaceGround(x,z)+Vector3.Up*.07f,new(.14f,.025f,4),new(.76f,.72f,.53f));stripe.Rotation=new(0,(float)Math.Atan2(ExpeditionWorldAtlas.HighwayX(z+2)-ExpeditionWorldAtlas.HighwayX(z-2),4),0);
  }
  var st=new SurfaceTool();st.Begin(Mesh.PrimitiveType.Triangles);int count=0;
  for(int z=cz*64;z<(cz+1)*64;z+=4)for(int track=-3;track<=3;track++){
   double x=2100-.18*(z+3920)+track*6;
   if(x<cx*64||x>=(cx+1)*64||game.World.District(x,z)!=7||game.World.IsRiver(x,z))continue;
   Vector3 At(double xx,double zz)=>SurfaceGround(xx,zz)+Vector3.Up*.10f;
   foreach(double offset in new[]{-.72,.72})GroundQuad(st,At(x+offset-.05,z),At(x+offset+.05,z),At(x+offset-.72+.05,z+4),At(x+offset-.72-.05,z+4),Steel);
   foreach(double dz in new[]{0d,2})GroundQuad(st,At(x-1.15-.18*dz,z+dz),At(x+1.15-.18*dz,z+dz),At(x+1.15-.18*dz,z+dz+.2),At(x-1.15-.18*dz,z+dz+.2),Wood);
   count++;
  }
  if(count>0){st.GenerateNormals();root.AddChild(new MeshInstance3D{Name="RailYardTracks",Mesh=st.Commit(),MaterialOverride=new StandardMaterial3D{VertexColorUseAsAlbedo=true,Roughness=.85f}});}
 }
 void EditableHomeYard(Node3D root,bool simulation){
  GardenRailings(root,simulation);
  root.AddChild(new Label3D{Text="14 / HOME",Position=new(0,2.1f,4.65f),FontSize=26,PixelSize=.006f});
  var at=game.World.NearestStreet(ExpeditionWorld.HomeX,ExpeditionWorld.HomeZ);var a=new Vector3((float)ExpeditionWorld.HomeX-4.5f,(float)game.World.HomeFloor-.02f,(float)ExpeditionWorld.HomeZ);var b=Ground(at.X,at.Z)+Vector3.Up*.05f;
  var st=new SurfaceTool();st.Begin(Mesh.PrimitiveType.Triangles);var forward=(b-a).Normalized();var across=new Vector3(-forward.Z,0,forward.X)*1.1f;
  GroundQuad(st,a-across,b-across,b+across,a+across,SurfaceTone(5));st.GenerateNormals();if(!simulation)root.GetParent().AddChild(new MeshInstance3D{Name="HomeStreetApproach",Mesh=st.Commit(),MaterialOverride=TerrainInk()});
  var body=new StaticBody3D();body.AddChild(new CollisionShape3D{Shape=new ConvexPolygonShape3D{Points=new[]{a-across,b-across,b+across,a+across,a-across-Vector3.Up*.1f,b-across-Vector3.Up*.1f,b+across-Vector3.Up*.1f,a+across-Vector3.Up*.1f}}});root.GetParent().AddChild(body);
 }
 ShaderMaterial atlasWaterMaterial;
 void AtlasWater(Node3D root,int cx,int cz,bool simulation){
  if(simulation)return;var st=new SurfaceTool();st.Begin(Mesh.PrimitiveType.Triangles);int count=0;
  for(int x=cx*64;x<(cx+1)*64;x+=4)for(int z=cz*64;z<(cz+1)*64;z+=4){
   if(game.World.WaterDistance(x+2,z+2)>2)continue;
   float y=(float)ExpeditionWorld.WaterLevel;
   foreach(var p in new[]{new Vector3(x,y,z),new Vector3(x+4,y,z),new Vector3(x+4,y,z+4),new Vector3(x,y,z),new Vector3(x+4,y,z+4),new Vector3(x,y,z+4)}){float shore=(float)Math.Clamp(1+game.World.WaterDistance(p.X,p.Z)/10,0,1);float shallow=(float)Math.Clamp(1-(ExpeditionWorld.WaterLevel-game.World.Height(p.X,p.Z))/5,0,1);st.SetColor(new Color(shore,shallow,0));st.AddVertex(p);}
   count++;
  }
  if(count==0)return;st.GenerateNormals();atlasWaterMaterial??=GeneratedLakeMaterial();
  root.AddChild(new MeshInstance3D{Name="AtlasRiverAndLakeSurface",Mesh=st.Commit(),MaterialOverride=atlasWaterMaterial});
 }
}
