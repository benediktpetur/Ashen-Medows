using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 async void RunCarChecks(){try{
  game.NoDiskSave=true;ClearEnemies();game.Building.Pieces.Clear();SyncStructures();
  player.Position=Ground(0,12);var wreck=new WorldProp{Id="car-check/wreck",Kind=2,Hp=100,Position=Ground(0,12),Node=new Node3D{Position=Ground(0,12)}};worldRoot.AddChild(wreck.Node);props.Add(wreck);
  game.Craft.ScrapMetal=12;game.Craft.Glass=4;game.Craft.ToolLevels[2]=0;RestoreWreck(wreck.Id);
  MigrationChecks.Assert(game.Building.Pieces.Count==0&&game.Craft.ScrapMetal==12,"Mechanic tool required without spending supplies");
  game.Craft.ToolLevels[2]=1;game.Craft.Glass=3;RestoreWreck(wreck.Id);
  MigrationChecks.Assert(game.Building.Pieces.Count==0&&game.Craft.ScrapMetal==12,"Insufficient parts preserve inventory");
  game.Craft.Glass=4;player.Position=Ground(20,12);RestoreWreck(wreck.Id);
  MigrationChecks.Assert(game.Building.Pieces.Count==0,"Remote restoration rejected");player.Position=Ground(0,12);RestoreWreck(wreck.Id);
  var car=game.Building.Pieces.Single();MigrationChecks.Assert(game.Craft.ScrapMetal==0&&game.Craft.Glass==0&&RestoredWreck(wreck.Id)&&!props.Contains(wreck),"Repair consumes parts once and replaces wreck");
  RestoreWreck(wreck.Id);MigrationChecks.Assert(game.Building.Pieces.Count==1,"Repeated restoration cannot duplicate cars");
  UseBlueprint(car.Id+"|mount");MigrationChecks.Assert(mountedBlueprint==car.Id&&vehicleDrivers.ContainsKey(car.Id),"Restored car mounts through normal vehicle controls");
  await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
  double z=car.Z;game.Clock+=1;DriveBlueprint(car.Id+"|0|1");MigrationChecks.Assert(car.Z<z,"Restored car drives forward with real collision");
  var barrier=new Node3D{Position=new((float)car.X,(float)car.Y,(float)car.Z-3.2f)};worldRoot.AddChild(barrier);PhysicsObstacle(barrier,new BoxShape3D{Size=new(4,3,.5f)},new(0,1.5f,0));
  await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);z=car.Z;game.Clock+=1;DriveBlueprint(car.Id+"|0|1");MigrationChecks.Assert(car.Z==z,"Solid obstacle blocks car motion");barrier.GetParent().RemoveChild(barrier);barrier.QueueFree();
  vehicleDrivers[car.Id]=99;game.Clock+=1;DriveBlueprint(car.Id+"|0|-1");MigrationChecks.Assert(car.Z==z,"Another player cannot drive an occupied car");vehicleDrivers[car.Id]=0;
  MigrationChecks.Assert(Json.Read<ExpeditionWorldChanges>(Json.Write(game.World.Changes)).Burned(car.Id),"Original wreck suppression marker is serialized");
  var copy=Json.Read<System.Collections.Generic.List<BuildPiece>>(Json.Write(game.Building.Pieces));MigrationChecks.Assert(copy.Single().Id==car.Id&&copy.Single().Z==car.Z,"Restored identity and position survive save serialization");
  Dismount();MigrationChecks.Assert(mountedBlueprint==null&&!vehicleDrivers.ContainsKey(car.Id),"Dismount releases driver seat");
  var spent=new WorldProp{Id="car-check/burned",Kind=2,Hp=0,Position=Ground(0,12),Node=new Node3D{Position=Ground(0,12)}};worldRoot.AddChild(spent.Node);props.Add(spent);game.World.Changes.Burn(spent.Id);game.Craft.ScrapMetal=12;game.Craft.Glass=4;RestoreWreck(spent.Id);MigrationChecks.Assert(game.Building.Pieces.Count==1&&game.Craft.ScrapMetal==12,"Burned wrecks cannot be restored");
  GD.Print("PASS / car restoration: tool, parts, proximity, single charge, replacement, mount, drive collision, saved identity/position, dismount and spent wreck rejection");GetTree().Quit();
 }catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}
