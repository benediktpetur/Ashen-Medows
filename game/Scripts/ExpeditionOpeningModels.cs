using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 Node3D OpeningStructure(BuildPiece p,bool preview){var root=new Node3D{Position=new((float)p.X,(float)p.Y,(float)p.Z),RotationDegrees=new(0,(float)p.Yaw,0)};var d=p.Def;
  Color surface=d.Tier<=2?Wood:d.Tier==3?new(.31f,.34f,.32f):d.Tier==4?new(.47f,.47f,.40f):new(.25f,.31f,.21f),frame=d.Tier<=2?Wood*.8f:Steel;
  foreach(var part in ExpeditionOpenings.Parts(p)){var position=new Vector3((float)part.X,(float)part.Y,(float)part.Z);var size=new Vector3((float)part.W,(float)part.H,(float)part.D);Color color=part.Role switch{"frame" or "brace"=>frame,"seam"=>d.Tier<=2?Ink:d.Tier==3?new(.41f,.26f,.15f):Steel*.65f,"bolt" or "handle"=>Steel,"warning"=>new(.64f,.55f,.25f),"plate"=>new(.75f,.74f,.62f),_=>surface};
   var mesh=part.Solid?Box(root,position,size,color):Bevel(root,position,size,color);mesh.RotationDegrees=new(0,(float)part.Yaw,0);
   if(d.Tier<=2&&part.Role is "wall" or "leaf")mesh.MaterialOverride=Textured(color,d.Tier==2&&d.Opening=="gate"&&part.Role=="wall"?1:0,.8f);
   if(!preview&&part.Solid){var body=new StaticBody3D{Position=position,RotationDegrees=new(0,(float)part.Yaw,0)};body.AddChild(new CollisionShape3D{Shape=new BoxShape3D{Size=size}});root.AddChild(body);}
  }
  if(d.Tier==2&&d.Opening!="gate")for(int side=-1;side<=1;side+=2)for(int row=0;row<3;row++)Bevel(root,new(side*1.14f,.11f+row*.2f,-.02f),new(.66f,.18f,.25f),new(.38f,.39f,.33f));
  if(d.Tier>=3){for(int side=-1;side<=1;side+=2)for(int i=0;i<4;i++)Ellipsoid(root,new(side*((float)d.W/2-.10f),.25f+i*.6f,-(float)d.D/2-.025f),new(.025f,.025f,.014f),Steel);}
  if(d.Opening=="gate")root.AddChild(new Label3D{Text=d.Tier==5?"CHECKPOINT":d.Tier==4?"KEEP GATE CLOSED":"",Position=new(0,(float)d.H-.12f,-(float)d.D/2-.025f),RotationDegrees=new(0,180,0),FontSize=22,PixelSize=.004f,Modulate=new(.78f,.73f,.56f)});
  return root;
 }
 BuildPiece AimedOpening(){if(dungeon!=null||buildMode)return null;var from=camera.GlobalPosition;var query=PhysicsRayQueryParameters3D.Create(from,from-camera.GlobalBasis.Z*4);query.Exclude=new(){player.GetRid()};var hit=GetWorld3D().DirectSpaceState.IntersectRay(query);if(hit.Count==0||hit["collider"].AsGodotObject() is not Node node)return null;while(node!=null){if(node.HasMeta("blueprint")){string id=node.GetMeta("blueprint").AsString();return game.Building.Pieces.FirstOrDefault(p=>p.Id==id&&p.Def.Opening!="");}node=node.GetParent();}return null;}
 bool OpeningSweepBlocked(BuildPiece p,int swing){var excluded=new Godot.Collections.Array<Rid>();foreach(var root in buildRoot.GetChildren().OfType<Node3D>())if(root.HasMeta("blueprint")&&root.GetMeta("blueprint").AsString()==p.Id)foreach(var body in root.FindChildren("*","StaticBody3D",true,false).OfType<StaticBody3D>())excluded.Add(body.GetRid());
  var probe=new BuildPiece{Kind=p.Kind,DoorSwing=swing};double rootAngle=p.Yaw*Math.PI/180;for(int step=0;step<=12;step++){double angle=p.Open?90-step*7.5:step*7.5;foreach(var part in ExpeditionOpenings.Parts(probe,angle).Where(part=>part.Moving&&part.Solid)){
   double x=p.X+part.X*Math.Cos(rootAngle)+part.Z*Math.Sin(rootAngle),z=p.Z-part.X*Math.Sin(rootAngle)+part.Z*Math.Cos(rootAngle);var query=new PhysicsShapeQueryParameters3D{Shape=new BoxShape3D{Size=new((float)part.W-.012f,(float)part.H-.012f,(float)part.D-.012f)},Transform=new Transform3D(Basis.FromEuler(new(0,(float)((p.Yaw+part.Yaw)*Math.PI/180),0)),new((float)x,(float)(p.Y+part.Y),(float)z)),Exclude=excluded,Margin=.001f};if(GetWorld3D().DirectSpaceState.IntersectShape(query,1).Count>0)return true;
  }}return false;
 }
}
