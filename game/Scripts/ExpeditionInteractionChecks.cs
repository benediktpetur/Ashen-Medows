using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
public partial class MigrationScene {
 void TradeRuleChecks(){
  var a=new ExpeditionGame(System.IO.Path.Combine(System.IO.Path.GetTempPath(),Guid.NewGuid()+".json")){NoDiskSave=true};var b=new ExpeditionGame(System.IO.Path.Combine(System.IO.Path.GetTempPath(),Guid.NewGuid()+".json")){NoDiskSave=true};
  a.Craft.Materials[0]=10;b.Life.Food=5;var timber=new List<PlayerTradeLine>{new(){Key="s:0",Amount=7}};var food=new List<PlayerTradeLine>{new(){Key="s:7",Amount=2}};
  MigrationChecks.Assert(ExpeditionTrading.Exchange(a,b,timber,food,out _)&&a.Craft.Materials[0]==3&&b.Craft.Materials[0]==7&&a.Life.Food==5&&b.Life.Food==3,"Trade conserves both offers");
  MigrationChecks.Assert(!ExpeditionTrading.Exchange(a,b,timber,food,out _)&&a.Craft.Materials[0]==3,"Stale offers do not partially transfer");
  MigrationChecks.Assert(ExpeditionTrading.Validate(a,b,new(){new(){Key="s:0",Amount=-1}})!="Ready","Negative quantities rejected");
  MigrationChecks.Assert(ExpeditionTrading.Validate(a,b,new(){new(){Key="s:0",Amount=1},new(){Key="s:0",Amount=1}})!="Ready","Duplicate offer keys rejected");
  var armor=new ExpeditionRelic{Id="trade-test-armor",Name="Test coat",Slot="chest",Level=1};a.Profile.Items.Add(armor);a.Profile.Equipped=armor.Id;
  MigrationChecks.Assert(!ExpeditionTrading.GearAllowed(a,armor),"Equipped armor cannot trade");a.Profile.Equipped=null;armor.Bond=new();MigrationChecks.Assert(!ExpeditionTrading.GearAllowed(a,armor),"Boss binding cannot bypass kill-group trading");armor.Bond=null;
  a.Craft.Weapons.Add(2);a.Craft.Magazines[2]=11;a.Life.WeaponQuality[2]=17;
  MigrationChecks.Assert(ExpeditionTrading.Exchange(a,b,new(){new(){Key="w:2",Amount=1},new(){Key="g:"+armor.Id,Amount=1}},new(),out _)&&b.Craft.Weapons.Contains(2)&&!a.Craft.Weapons.Contains(2)&&b.Craft.Magazines[2]==11&&a.Craft.Magazines[2]==0&&b.Life.WeaponQuality[2]==17&&b.Profile.Items.Any(i=>i.Id==armor.Id)&&!a.Profile.Items.Any(i=>i.Id==armor.Id),"Weapon quality, magazine and armor transfer without duplication");
  b.Craft.Materials[0]=int.MaxValue;MigrationChecks.Assert(ExpeditionTrading.Validate(a,b,new(){new(){Key="s:0",Amount=1}})!="Ready","Inventory integer overflow rejected");
  GD.Print("PASS / ordinary trade / conservation / stale offers / negative and duplicate rejection / gear binding / weapon magazine and quality / overflow");
 }
 async void RunInteractionChecks(){try{
  game.NoDiskSave=true;paused=true;SetProcess(false);SetPhysicsProcess(false);CloseMenu();ClearEnemies();TradeRuleChecks();
  Teleport(ExpeditionWorld.SpawnX,ExpeditionWorld.SpawnZ);var furniture=props.First(p=>p.Id=="briar/BriarLaneHouse0/furniture/0");
  player.Position=furniture.Position+furniture.Node.GlobalBasis.X.Normalized()*2;camera.GlobalPosition=player.Position+Vector3.Up;camera.LookAt(furniture.Position);
  for(int i=0;i<3;i++)await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
  MigrationChecks.Assert(AimedFurniture()==furniture,"Furniture target uses actual collider");
  paused=false;_UnhandledInput(new InputEventKey{PhysicalKeycode=Key.E,Pressed=true});
  MigrationChecks.Assert(!game.World.Changes.StrippedFurniture.Contains(furniture.Id)&&!ObjectHealth.ContainsKey(furniture.Id),"E cannot dismantle furniture");CloseMenu();paused=true;
  var ray=PhysicsRayQueryParameters3D.Create(camera.GlobalPosition,furniture.Position);ray.Exclude=new(){player.GetRid()};var hit=GetWorld3D().DirectSpaceState.IntersectRay(ray);
  MigrationChecks.Assert(WeaponStructureHit(hit["collider"].AsGodotObject() as Node,30)&&ObjectHealth[furniture.Id]==60&&!game.World.Changes.StrippedFurniture.Contains(furniture.Id),"Furniture has persistent per-item health");
  WeaponStructureHit(hit["collider"].AsGodotObject() as Node,30);int scrap=game.Craft.ScrapMetal;WeaponStructureHit(hit["collider"].AsGodotObject() as Node,30);
  MigrationChecks.Assert(game.World.Changes.StrippedFurniture.Contains(furniture.Id)&&game.Craft.ScrapMetal==scrap+1,"Weapon damage destroys furniture and grants salvage once");
  var changes=Json.Read<ExpeditionWorldChanges>(Json.Write(game.World.Changes));MigrationChecks.Assert(changes.ObjectHealth[furniture.Id]==0,"Object health survives save serialization");
  var wall=GetTree().GetNodesInGroup("damageable_world").OfType<StaticBody3D>().First(n=>n.GetMeta("world_damage_id").AsString().StartsWith("wall/"));string wallId=wall.GetMeta("world_damage_id").AsString();
  WeaponStructureHit(wall,30);MigrationChecks.Assert(ObjectHealth[wallId]==150,"Residential wall has independent health");WeaponStructureHit(wall,200);await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
  MigrationChecks.Assert(!GetTree().GetNodesInGroup("damageable_world").Any(n=>n.GetMeta("world_damage_id").AsString()==wallId),"Destroyed wall loses render and collision");
  game.Building.Pieces.Clear();var piece=new BuildPiece{Id="interaction-test-wall",Builder="another-builder",Kind=2,X=0,Y=200,Z=0,Health=BuildDefinition.All[2].Durability,Stock=new int[15]};game.Building.Pieces.Add(piece);SyncStructures();player.Position=new(0,200,3);camera.GlobalPosition=new(0,201.2f,3);camera.LookAt(new Vector3(0,201.2f,0));
  for(int i=0;i<3;i++)await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
  EquipConstructionHammer(true,false);RemoveOwnStructure(false);MigrationChecks.Assert(game.Building.Pieces.Contains(piece),"Hammer cannot destroy another builder's wall");
  piece.Builder=CommunityOwner;piece.Stock[0]=1;RemoveOwnStructure(true);MigrationChecks.Assert(game.Building.Pieces.Contains(piece),"Stocked structures cannot discard stored items");piece.Stock[0]=0;
  int materials=game.Craft.Materials[0];RemoveOwnStructure(true);MigrationChecks.Assert(!game.Building.Pieces.Contains(piece)&&game.Craft.Materials[0]==materials+piece.Def.Cost[0]/2,"Owner dismantle refunds half materials");
  MigrationChecks.Assert(Json.Read<BuildPiece>(Json.Write(piece)).Builder==CommunityOwner,"Builder ownership survives serialization");
  GD.Print("PASS / interaction / E never salvages / furniture and wall health / persistent destruction / hammer ownership and stock protection / refunds");
  if(OS.GetCmdlineUserArgs().Contains("--interaction-preview")){Teleport(ExpeditionWorld.HomeX,ExpeditionWorld.HomeZ);EquipConstructionHammer(true);await ArtShot("interaction-hammer-catalogue");CloseMenu();}
  GetTree().Quit();
 }catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}}
}
