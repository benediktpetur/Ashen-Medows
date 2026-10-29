using Godot;
using System;
using System.Collections.Generic;
public partial class MigrationScene {
 readonly Dictionary<int,MultiMesh> forestCrowns=new(),forestLeafDetails=new();
 Mesh fernMesh;
 void ForestFloor(Node3D chunk,int cx,int cz){var random=new Random(unchecked((int)game.World.Hash(cx+5519,cz-971)));var points=new List<Vector3>();for(int i=0;i<440;i++){double x=cx*64+random.NextDouble()*64,z=cz*64+random.NextDouble()*64;if(game.World.Tier(x,z)!=2||!SceneryClear(x,z,1.4f)||InEncounterLot(x,z)||InLivingLot(x,z)||InFamilyLot(x,z)||InWoodlandCamp(x,z)||InFeverLot(x,z)||InMill(x,z,35)||game.World.Changes.OnPath(x,z)||ForestBuildClearance(x,z))continue;points.Add(SurfaceGround(x,z));}if(points.Count==0)return;
  if(fernMesh==null){var surface=new SurfaceTool();surface.Begin(Mesh.PrimitiveType.Triangles);for(int frond=0;frond<7;frond++){float a=frond*Mathf.Tau/7;var outward=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));var side=new Vector3(Mathf.Cos(a),0,-Mathf.Sin(a));for(int j=1;j<6;j++){float t=j/6f;var center=outward*t*.65f+Vector3.Up*(Mathf.Sin(t*Mathf.Pi)*.42f+.06f);float width=(1-t)*.19f;foreach(var v in new[]{center-outward*.09f,center+side*width,center+outward*.10f,center-outward*.09f,center+outward*.10f,center-side*width}){surface.SetNormal(Vector3.Up);surface.AddVertex(v);}}}fernMesh=surface.Commit();}
  var mm=new MultiMesh{TransformFormat=MultiMesh.TransformFormatEnum.Transform3D,Mesh=fernMesh,InstanceCount=points.Count};for(int i=0;i<points.Count;i++)mm.SetInstanceTransform(i,new Transform3D(Basis.FromEuler(new(0,(float)random.NextDouble()*6,0)).Scaled(Vector3.One*(.7f+(float)random.NextDouble()*.7f)),points[i]));chunk.AddChild(new MultiMeshInstance3D{Name="ForestFerns",Multimesh=mm,MaterialOverride=Material(new Color(.19f,.30f,.12f)),VisibilityRangeEnd=75,CastShadow=GeometryInstance3D.ShadowCastingSetting.Off});
 }
 bool ForestBuildClearance(double x,double z){foreach(var p in game.Building.Pieces)if(p.Contains(x,p.CenterY,z,1.2))return true;return false;}
 // Eight shared mesh variants keep the denser woodland from creating unique geometry for every tree.
 void ForestTree(Node3D root,float height,int seed){if(TryWorldTree(root,height,seed,(uint)seed%7==0))return;int variant=(int)((uint)seed%8);var visual=new Node3D{Scale=Vector3.One*(height/14)};root.AddChild(visual);
  FigureMesh(visual,"forest-canopy/v1/trunk/"+variant,b=>{b.Limb(Wood,Vector3.Zero,new(.12f,11,0),.29f,.29f,.075f,.075f,10);for(int i=0;i<5;i++){float a=i*2.4f+variant*.6f;var start=new Vector3(.05f,6+i*.6f,0);b.Limb(Wood,start,new(Mathf.Sin(a)*2.9f,9+i*.7f,Mathf.Cos(a)*2.9f),.12f,.12f,.025f,.025f,8);}});
  visual.GetChild<MeshInstance3D>(0).VisibilityRangeEnd=175;
  if(!forestCrowns.TryGetValue(variant,out var crown)){var random=new Random(variant*317+19);crown=new MultiMesh{TransformFormat=MultiMesh.TransformFormatEnum.Transform3D,UseColors=true,Mesh=new SphereMesh{Radius=1,Height=2,RadialSegments=10,Rings=5},InstanceCount=13};
   for(int i=0;i<13;i++){float angle=i*2.4f+variant*.6f;var center=new Vector3(Mathf.Sin(angle)*(i==12?0:2.5f),9.6f+(float)random.NextDouble()*3,Mathf.Cos(angle)*(i==12?0:2.5f));crown.SetInstanceTransform(i,new Transform3D(Basis.Identity.Scaled(new(2.6f,1.6f,2.5f)),center));float tint=.75f+(float)random.NextDouble()*.2f;crown.SetInstanceColor(i,new(tint,tint,.72f,1));}forestCrowns[variant]=crown;
   var leaf=new SurfaceTool();leaf.Begin(Mesh.PrimitiveType.Triangles);foreach(var p in new[]{new Vector3(-.14f,0,0),new Vector3(0,.04f,.12f),new Vector3(.14f,0,0),new Vector3(-.14f,0,0),new Vector3(.14f,0,0),new Vector3(0,-.03f,-.12f)})leaf.AddVertex(p);leaf.GenerateNormals();var detail=new MultiMesh{TransformFormat=MultiMesh.TransformFormatEnum.Transform3D,Mesh=leaf.Commit(),InstanceCount=260};
   for(int i=0;i<260;i++){float angle=(float)random.NextDouble()*Mathf.Tau,lat=(float)random.NextDouble()*Mathf.Pi;var cluster=crown.GetInstanceTransform(i%13);var unit=new Vector3(Mathf.Sin(angle)*Mathf.Sin(lat),Mathf.Cos(lat),Mathf.Cos(angle)*Mathf.Sin(lat));detail.SetInstanceTransform(i,new Transform3D(Basis.FromEuler(new(.3f,angle,0)),cluster.Origin+unit*cluster.Basis.Scale));}forestLeafDetails[variant]=detail;
  }
  visual.AddChild(new MultiMeshInstance3D{Multimesh=crown,MaterialOverride=WoodlandLeaves(),VisibilityRangeEnd=190});visual.AddChild(new MultiMeshInstance3D{Multimesh=forestLeafDetails[variant],MaterialOverride=WoodlandLeaves(),VisibilityRangeEnd=75,CastShadow=GeometryInstance3D.ShadowCastingSetting.Off});PhysicsObstacle(root,new CylinderShape3D{Radius=.45f,Height=height*.65f},new(0,height*.325f,0));
 }
}
