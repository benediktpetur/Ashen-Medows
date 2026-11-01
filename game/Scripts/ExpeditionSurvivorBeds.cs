using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 string SurvivorBedReason(BuildPiece bed,CampSurvivor survivor){
  if(bed==null||bed.Def.Function!="bed"||bed.Health<=0||!game.Building.Pieces.Contains(bed))return "This bed is no longer available";
  if(survivor==null||survivor.State is "trapped" or "dead")return "Rescue this survivor first";
  if(survivor.BedId==bed.Id&&survivor.Member)return "Already assigned here";
  if(Community.Survivors.Any(n=>n.Member&&n.BedId==bed.Id))return "This bed already has a survivor";
  if(!ExpeditionCommunity.Sheltered(bed,game.Building.Pieces))return "Needs a roof and two walls";
  if((Community.HomeBound||Community.Established)&&ExpeditionGame.Distance(bed.X,bed.Z,Community.X,Community.Z)>=35)return "Build this bed at your settlement home";
  if(survivor.Downed)return "Give this survivor first aid before assigning a bed";
  if((!string.IsNullOrEmpty(survivor.Owner)&&survivor.Owner!=CommunityOwner)||(!string.IsNullOrEmpty(survivor.EscortOwner)&&survivor.EscortOwner!=CommunityOwner))return "Accompanying another player";
  if(!survivor.Member){
   if(survivor.State!="rescued")return "Rescue this survivor first";
   if(Community.Survivors.Count(n=>n.Member)>=3)return "Camp is full (three survivors)";
   if(game.Life.Food<3)return "Needs three food portions to join camp";
  }
  return "Ready";
 }
 void SurvivorBedPanel(VBoxContainer box,BuildPiece bed,Action<string,string> action){
  var occupant=Community.Survivors.FirstOrDefault(n=>n.Member&&n.BedId==bed.Id);
  Text(box,"ASSIGNED SURVIVOR / "+(occupant?.Name??"Unassigned"));
  Text(box,"One survivor per bed. Place under a roof with two nearby walls. Assign a rescued survivor to invite them home for 3 food, or move an existing resident here for free.");
  var found=Community.Survivors.Where(n=>n.Member||n.State=="rescued").ToArray();
  if(found.Length==0)Text(box,"No rescued survivors yet. Find survivors in the world and rescue them first.");
  foreach(var survivor in found){
   string reason=SurvivorBedReason(bed,survivor);
   if(reason=="Ready")action("ASSIGN "+survivor.Name.ToUpperInvariant()+(survivor.Member?"":" / JOIN CAMP / 3 FOOD"),"assign|"+survivor.Id);
   else Text(box,survivor.Name+" / "+reason);
  }
 }
 void AssignSurvivorBed(BuildPiece bed,string survivorId){
  if(CoopClient||dungeon!=null||Distance(player.Position,new((float)bed.X,(float)bed.Y,(float)bed.Z))>4)return;
  var survivor=Community.Survivors.FirstOrDefault(n=>n.Id==survivorId);
  string reason=SurvivorBedReason(bed,survivor);
  if(reason!="Ready"){game.Notice=reason;return;}
  bool recruited=!survivor.Member;
  game.Change(()=>{
   if(recruited)Community.Recruit(survivor,game,bed.X,bed.Z,bed);
   else survivor.BedId=bed.Id;
  });
  game.Notice=survivor.Name+" assigned to Survivor Bed";
  if(CoopHost&&coopActors.Count>0)Rpc(nameof(CoopWorldUpdate),Json.Write(WorldSnapshot()));
 }
}
