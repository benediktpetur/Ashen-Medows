using Godot;
using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.Serialization;
sealed partial class ExpeditionWorldChanges {
 [DataMember] public Dictionary<string,double> ObjectHealth=new();
}
public partial class MigrationScene {
 Dictionary<string,double> ObjectHealth=>game.World.Changes.ObjectHealth??=new();
 string Coordinate(float n)=>Math.Round(n,3).ToString("0.###",CultureInfo.InvariantCulture);
 string ArchitectureId(Node3D parent,Vector3 center,Vector3 size){
  if(game==null||dungeon!=null||parent==null||size.Y<1)return null;
  Node3D house=null;bool chunk=false;
  for(Node cursor=parent;cursor!=null&&cursor!=this;cursor=cursor.GetParent()){
   if(cursor.Name.ToString().StartsWith("Furniture"))return null;
   if(cursor.HasMeta("world_structure_id"))return cursor.GetMeta("world_structure_id").AsString();
   if(cursor.HasMeta("house_id")||cursor.Name.ToString().StartsWith("BriarLaneHouse")){house=cursor as Node3D;break;}
   if(cursor.Name.ToString().StartsWith("Chunk_"))chunk=true;
  }
  if(house!=null){
   if(size.X>.5f&&size.Z>.5f)return null; // thin house wall sections, not furniture or foundations
   var p=house.ToLocal(parent.ToGlobal(center));string home=house.HasMeta("house_id")?house.GetMeta("house_id").AsString():"briar/"+house.Name;
   return $"wall/{home}/{Coordinate(p.X)},{Coordinate(p.Y)},{Coordinate(p.Z)}/{Coordinate(size.X)},{Coordinate(size.Y)},{Coordinate(size.Z)}";
  }
  if(chunk&&size.X>=6&&size.Z>=6&&size.Y>=3.4f){var p=parent.ToGlobal(center);return $"building/{game.Save.Seed}/{Coordinate(p.X)}/{Coordinate(p.Z)}";}
  return null;
 }
 void RegisterDamageNode(Node node,string id,double health){if(string.IsNullOrEmpty(id))return;node.SetMeta("world_damage_id",id);node.SetMeta("world_max_health",health);node.AddToGroup("damageable_world");if(ObjectHealth.TryGetValue(id,out double hp)&&hp<=0){if(node is Node3D visual)visual.Visible=false;node.QueueFree();}}
 void RegisterDamageBox(Node3D parent,MeshInstance3D mesh,Node body,Vector3 center,Vector3 size){
  string id=ArchitectureId(parent,center,size);if(id==null)return;double hp=id.StartsWith("wall/")?180:600;
  RegisterDamageNode(mesh,id,hp);RegisterDamageNode(body,id,hp);
 }
 void RegisterDamageObstacle(Node3D parent,Node body,Shape3D shape,Vector3 center){if(shape is BoxShape3D box){string id=ArchitectureId(parent,center,box.Size);if(id!=null)RegisterDamageNode(body,id,id.StartsWith("wall/")?180:600);}}
 void ApplyObjectDestruction(){foreach(var node in GetTree().GetNodesInGroup("damageable_world"))if(!node.IsQueuedForDeletion()&&node.HasMeta("world_damage_id")&&ObjectHealth.TryGetValue(node.GetMeta("world_damage_id").AsString(),out double hp)&&hp<=0){if(node is Node3D visual)visual.Visible=false;node.QueueFree();}SyncFurnitureSalvage(true);}
 void SyncDamageState(){ApplyObjectDestruction();if(CoopHost&&coopActors.Count>0)Rpc(nameof(CoopWorldUpdate),Json.Write(WorldSnapshot()));}
 BuildPiece BuildForCollider(Node node){while(node!=null){if(node.HasMeta("blueprint")){string id=node.GetMeta("blueprint").AsString();return game.Building.Pieces.FirstOrDefault(p=>p.Id==id);}node=node.GetParent();}return null;}
 bool WeaponStructureHit(Node collider,double damage){
  if(CoopClient||collider==null||!double.IsFinite(damage)||damage<=0)return false;
  var piece=BuildForCollider(collider);
  if(piece!=null){
   if(game.Change(()=>{piece.Health=Math.Max(0,piece.Health-damage);if(piece.Health<=0){game.Building.Pieces.Remove(piece);CollapseDamagedSupports();}})){game.Notice=piece.Health>0?$"{piece.Def.Name} / {piece.Health:0}/{piece.Def.Durability:0} HP":"Structure destroyed";SyncStructures();SyncDamageState();}return true;
  }
  for(Node node=collider;node!=null;node=node.GetParent()){
   if(node.HasMeta("salvage_furniture")){string id=node.GetMeta("salvage_furniture").AsString();var furniture=props.FirstOrDefault(p=>p.Kind==42&&p.Id==id);if(furniture!=null)DamageFurniture(furniture,damage);return true;}
   if(node.HasMeta("world_damage_id")){
    string id=node.GetMeta("world_damage_id").AsString();double max=node.GetMeta("world_max_health").AsDouble();double hp=ObjectHealth.TryGetValue(id,out var stored)?stored:max;
    if(hp<=0)return true;
    if(game.Change(()=>ObjectHealth[id]=Math.Max(0,hp-damage))){game.Notice=ObjectHealth[id]>0?$"STRUCTURE / {ObjectHealth[id]:0}/{max:0} HP":"World structure destroyed";SyncDamageState();}return true;
   }
  }
  return false;
 }
 void CollapseDamagedSupports(){
  int budget=game.Building.Pieces.Count+1;
  while(budget-->0){var report=game.Building.Evaluate(game.Building.Pieces);if(report.Valid)break;int i=Array.FindIndex(report.Strength,s=>s<1);if(i<0)i=Enumerable.Range(0,report.Loads.Length).FirstOrDefault(n=>report.Loads[n]>game.Building.Pieces[n].Def.Capacity,-1);if(i<0)break;game.Building.Pieces.RemoveAt(i);}
 }
 void DamageFurniture(WorldProp p,double damage){
  if(game.World.Changes.StrippedFurniture.Contains(p.Id))return;const double max=90;double hp=ObjectHealth.TryGetValue(p.Id,out var saved)?saved:max;
  if(hp<=0)return;
  bool destroyed=hp-damage<=0;
  if(game.Change(()=>{ObjectHealth[p.Id]=Math.Max(0,hp-damage);if(destroyed){game.World.Changes.StrippedFurniture.Add(p.Id);game.Craft.ScrapMetal+=p.Material==2?3:1;game.Craft.Materials[p.Material==0?2:0]+=p.Material==0?3:2;game.Skills.Strength.Gain(12);game.Career.Gain(5);}})){
   game.Notice=destroyed?"Furniture destroyed / reclaimed materials":$"FURNITURE / {ObjectHealth[p.Id]:0}/{max:0} HP";SyncDamageState();
  }
 }
 bool AxeStructureHit(){if((!game.Craft.HasAxe&&game.Craft.ToolLevels[game.Craft.HarvestTool]<1)||game.Clock<nextChop||game.Stamina<5)return false;var from=camera.GlobalPosition;var q=PhysicsRayQueryParameters3D.Create(from,from-camera.GlobalBasis.Z*3.5f);q.Exclude=new(){player.GetRid()};var hit=GetWorld3D().DirectSpaceState.IntersectRay(q);if(hit.Count==0||!WeaponStructureHit(hit["collider"].AsGodotObject() as Node,30))return false;game.Stamina-=5;Noise(18);return true;}
 string ObjectConditionPrompt(){
  var q=PhysicsRayQueryParameters3D.Create(camera.GlobalPosition,camera.GlobalPosition-camera.GlobalBasis.Z*3.5f);q.Exclude=new(){player.GetRid()};var hit=GetWorld3D().DirectSpaceState.IntersectRay(q);if(hit.Count==0)return "";
  var node=hit["collider"].AsGodotObject() as Node;var piece=BuildForCollider(node);if(piece!=null)return $"{piece.Def.Name.ToUpperInvariant()} / {piece.Health:0}/{piece.Def.Durability:0} HP";
  while(node!=null){if(node.HasMeta("world_damage_id")){string id=node.GetMeta("world_damage_id").AsString();double max=node.GetMeta("world_max_health").AsDouble();return $"STRUCTURE / {(ObjectHealth.TryGetValue(id,out double hp)?hp:max):0}/{max:0} HP / HIT WITH A WEAPON";}node=node.GetParent();}return "";
 }
}
