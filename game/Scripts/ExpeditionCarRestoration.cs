using Godot;
using System;
using System.Linq;

public partial class MigrationScene {
 bool RestoredWreck(string id)=>game.World.Changes.Burned("restored/"+id);
 bool WreckInteract(){
  if(buildMode||dungeon!=null)return false;
  var wreck=NearProp(2,4);if(wreck==null||game.World.Changes.Burned(wreck.Id))return false;
  var box=Panel("RESTORE ABANDONED CAR");
  Text(box,"Save this car instead of stripping it for parts.\nRequires a mechanic tool, 12 raw scrap metal and 4 glass.\nRestored cars keep their position, condition and cargo in this world.\nW/S drive and reverse, A/D steer, E exits. Repair later with timber or a vehicle repair station.");
  Text(box,$"RAW SCRAP {game.Craft.ScrapMetal}/12 / GLASS {game.Craft.Glass}/4");
  Button(box,"REPAIR CAR / 12 SCRAP + 4 GLASS",()=>{if(!CoopRequest("restorecar",wreck.Id))RestoreWreck(wreck.Id);CloseMenu();});
  Button(box,"LEAVE CAR",CloseMenu);return true;
 }
 void RestoreWreck(string id){
  if(dungeon!=null)return;
  var wreck=props.FirstOrDefault(p=>p.Kind==2&&p.Id==id&&Distance(p.Position,player.Position)<4);
  if(wreck==null||wreck.Hp<=0||game.World.Changes.Burned(id)||RestoredWreck(id)){game.Notice="This wreck cannot be restored";return;}
  if(game.Craft.ToolLevels[2]<1){game.Notice="Craft a mechanic tool first / F3";return;}
  if(game.Craft.ScrapMetal<12||game.Craft.Glass<4){game.Notice="Car repair needs 12 raw scrap metal and 4 glass";return;}
  int kind=Array.FindIndex(BuildDefinition.All,d=>d.Function=="vehicle");
  var car=new BuildPiece{Id="restored/"+id,Kind=kind,Builder=coopActor.ToString(),X=wreck.Position.X,Y=game.World.Height(wreck.Position.X,wreck.Position.Z),Z=wreck.Position.Z,Yaw=wreck.Node.RotationDegrees.Y,Health=BuildDefinition.All[kind].Durability,Stock=new int[15]};
  if(!game.Change(()=>{game.Craft.ScrapMetal-=12;game.Craft.Glass-=4;game.World.Changes.Burn(car.Id);game.Building.Pieces.Add(car);game.AwardResearch("milestone/vehicle-repair",8);}))return;
  RemoveRestoredWrecks();SyncStructures();HearNoise(new((float)car.X,(float)car.Y,(float)car.Z),30);
  game.Notice="Car restored / E opens driver seat and cargo";
  if(CoopHost)Rpc(nameof(CoopWorldUpdate),Json.Write(WorldSnapshot()));
 }
 void RemoveRestoredWrecks(){foreach(var p in props.Where(p=>p.Kind==2&&RestoredWreck(p.Id)).ToArray()){p.Node.GetParent()?.RemoveChild(p.Node);p.Node.QueueFree();props.Remove(p);}}
 Node3D RestoredCarArt(BuildPiece p,bool preview){
  var root=new Node3D{Position=new((float)p.X,(float)p.Y,(float)p.Z),RotationDegrees=new(0,(float)p.Yaw,0)};
  var model=new Node3D{Scale=new(1,1,(float)(p.Def.D/4.2))};root.AddChild(model);ArtWreck(model,false);
  // Fresh panels and bright headlights distinguish the restored car from salvage.
  Box(model,new(0,.99f,-1.48f),new(1.75f,.08f,1.05f),new(.23f,.39f,.34f));
  for(int side=-1;side<=1;side+=2)Box(model,new(side*.57f,.73f,-2.14f),new(.35f,.17f,.04f),new(.98f,.91f,.65f));
  if(!preview)PhysicsObstacle(root,new BoxShape3D{Size=new((float)p.Def.W,1.5f,(float)p.Def.D)},new(0,.75f,0));
  return root;
 }
}
