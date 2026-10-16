using Godot;
public partial class MigrationScene {
 bool BlueprintSight(BuildPiece p,Infected target){var exclude=new Godot.Collections.Array<Rid>();foreach(var node in buildRoot.GetChildren())if(node is Node3D root&&root.HasMeta("blueprint")&&root.GetMeta("blueprint").AsString()==p.Id)foreach(var child in root.FindChildren("*","StaticBody3D",true,false))if(child is StaticBody3D body)exclude.Add(body.GetRid());var origin=new Vector3((float)p.X,(float)p.Y+(float)p.Def.H*.85f,(float)p.Z);var query=PhysicsRayQueryParameters3D.Create(origin,target.Body.Position+Vector3.Up);query.Exclude=exclude;var hit=GetWorld3D().DirectSpaceState.IntersectRay(query);return hit.Count==0||hit["collider"].AsGodotObject()==target.Body;}
}
