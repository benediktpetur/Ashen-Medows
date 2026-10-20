using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 bool demolitionMode;
 BuildPiece HitBlueprint(Godot.Collections.Dictionary hit){if(hit.Count==0)return null;var node=hit["collider"].AsGodotObject() as Node;while(node!=null){if(node.HasMeta("blueprint")){string id=node.GetMeta("blueprint").AsString();return game.Building.Pieces.FirstOrDefault(p=>p.Id==id);}node=node.GetParent();}return null;}
 BuildPiece AimedBuildPiece(){var from=camera.GlobalPosition;var q=PhysicsRayQueryParameters3D.Create(from,from-camera.GlobalBasis.Z*(float)reach);q.Exclude=new Godot.Collections.Array<Rid>{player.GetRid()};return HitBlueprint(GetWorld3D().DirectSpaceState.IntersectRay(q));}
 void DemolitionPreview(){ClearGhost();var target=AimedBuildPiece();placement=new BuildReport{Reason=target==null?"DEMOLISH / aim at your structure / Delete returns to placement":"DEMOLISH / "+target.Def.Name+" / left click removes / Delete returns to placement"};if(target==null)return;ghostMesh=new MeshInstance3D{Mesh=new BoxMesh{Size=new((float)target.Def.W+.04f,(float)target.Def.H+.04f,(float)target.Def.D+.04f)},Position=new((float)target.X,(float)target.CenterY,(float)target.Z),RotationDegrees=new(0,(float)target.Yaw,0),CastShadow=GeometryInstance3D.ShadowCastingSetting.Off,MaterialOverride=new StandardMaterial3D{Transparency=BaseMaterial3D.TransparencyEnum.Alpha,ShadingMode=BaseMaterial3D.ShadingModeEnum.Unshaded,AlbedoColor=new(1,.2f,.12f,.3f)}};AddChild(ghostMesh);}
}
