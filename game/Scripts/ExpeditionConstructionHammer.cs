using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 bool dismantleRefund=true;
 bool HasConstructionHammer=>CoopHost&&coopActor>0?coopActors.TryGetValue(coopActor,out var a)&&a.Hammer:buildMode;
 void EquipConstructionHammer(bool equipped,bool catalogue=true){
  buildMode=equipped;demolitionMode=false;axeEquipped=axePending=aiming=false;ClearGhost();
  CoopRequest("hammer",equipped?"1":"0");BuildHeldTool();
  if(equipped&&catalogue)ShowBuildCatalogue();
 }
 void ConstructionHammerArt(){
  ClearChildren(weaponRoot);weaponRoot.Scale=Vector3.One;
  Tube(weaponRoot,new(0,-.3f,.05f),new(0,.15f,-.35f),.035f,Wood);
  Bevel(weaponRoot,new(0,.15f,-.35f),new(.32f,.13f,.13f),Steel);
  Box(weaponRoot,new(-.18f,.15f,-.35f),new(.05f,.16f,.16f),Steel);
  RebuiltFirstPersonArms(weaponRoot,false);
 }
 void AssignLegacyBuilders(){
  if(CoopClient)return;
  foreach(var p in game.Building.Pieces)if(string.IsNullOrEmpty(p.Builder))p.Builder=p.Id?.StartsWith("home-transfer/")==true?"world":CommunityOwner;
 }
 bool OwnBuild(BuildPiece p)=>p!=null&&!string.IsNullOrEmpty(p.Builder)&&p.Builder==CommunityOwner;
 void RemoveOwnStructure(bool refund){
  if(!HasConstructionHammer){game.Notice="Equip the construction hammer first / B";return;}
  var target=AimedBuildPiece();if(!OwnBuild(target)){game.Notice=target==null?"Aim at something you built":"You can only dismantle or destroy your own builds";return;}
  if(target.Stock?.Any(n=>n>0)==true||target.Charge>0){game.Notice="Empty stored items and use remaining fuel before removal";return;}
  if(vehicleDrivers.ContainsKey(target.Id??"")){game.Notice="Dismount before removing this structure";return;}
  int selected=game.Building.Pieces.IndexOf(target);var remaining=game.Building.Pieces.Where(p=>p!=target).ToList();var report=game.Building.Evaluate(remaining);
  if(!report.Valid){game.Notice="Remove supported pieces first / "+report.Reason;return;}
  string reason="";
  if(game.Change(()=>{if(!game.Building.Remove(selected,out reason))throw new InvalidOperationException(reason);if(refund)for(int i=0;i<7;i++)game.Craft.Materials[i]+=target.Def.Cost[i]/2;})){
   game.Notice=(refund?"Dismantled / half the material cost reclaimed / ":"Destroyed / ")+target.Def.Name;SyncStructures();
  }
 }
}
