using Godot;
public partial class MigrationScene {
 void WillowAccessRamp(Node3D root,float x,float start,float end,string name){float y=(float)(game.World.Height(FamilyX+x,FamilyZ+end)-familyFloor);var n=new Node3D{Name=name,Position=new(x,0,0)};root.AddChild(n);Vector3 a=new(-1.5f,0,start),b=new(1.5f,0,start),c=new(1.5f,y,end),d=new(-1.5f,y,end);var surface=new SurfaceTool();surface.Begin(Mesh.PrimitiveType.Triangles);foreach(var p in new[]{a,b,c,a,c,d})surface.AddVertex(p);surface.GenerateNormals();n.AddChild(new MeshInstance3D{Mesh=surface.Commit(),MaterialOverride=Textured(new(.36f,.32f,.25f),0,.45f)});PhysicsObstacle(n,new ConvexPolygonShape3D{Points=new[]{a,b,c,d,a+Vector3.Down*.15f,b+Vector3.Down*.15f,c+Vector3.Down*.15f,d+Vector3.Down*.15f}},Vector3.Zero);}
}
