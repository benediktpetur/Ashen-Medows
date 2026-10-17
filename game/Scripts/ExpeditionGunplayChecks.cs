using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class MigrationScene {
 async Task GunplayChecks(){
  var original=game;var eye=camera.Transform;Node3D wall=null;bool oldView=viewGunApplied;
  try{
   game=new ExpeditionGame(ProjectSettings.GlobalizePath("user://tests/gunplay-"+Guid.NewGuid()+".json")){NoDiskSave=true};viewGunApplied=false;ClearEnemies();
   var legacy=Json.Read<ExpeditionCraftState>("{\"Weapon\":2,\"Weapons\":[2],\"Ammo\":7,\"Reserve\":37}");legacy.Normalize();
   MigrationChecks.Assert(legacy.Ammo==7&&legacy.Stock(1)==37&&legacy.Stock(0)==0&&legacy.Stock(4)==0,"Legacy rounds migrate once to equipped gun class");legacy.Normalize();MigrationChecks.Assert(legacy.Stock(1)==37,"Ammo migration cannot duplicate reserves");
   game.Career.Level=2;game.Craft.UnlockedTier=5;game.Craft.Weapons.AddRange(new[]{0,2,3,4,5,7});
   game.Equip(0);game.Craft.GiveAmmo(0,20);game.Craft.GiveAmmo(1,15);game.Craft.GiveAmmo(3,12);game.Reload();
   MigrationChecks.Assert(game.Craft.Ammo==12&&game.Craft.Stock(0)==8,"Pistol loads only 9mm");
   game.Equip(2);game.Reload();MigrationChecks.Assert(game.Craft.Ammo==6&&game.Craft.Stock(1)==9&&game.Craft.Stock(0)==8,"Rifle has separate rounds");
   game.Equip(7);game.Reload();MigrationChecks.Assert(game.Craft.Ammo==2&&game.Craft.Stock(3)==10,"Shotgun uses shells");
   game.Equip(0);MigrationChecks.Assert(game.Craft.Ammo==12&&game.Craft.Reserve==8,"Switching preserves each loaded magazine");
   game.Craft.Ammo=0;game.Craft.GiveAmmo(0,-8);game.Reload();MigrationChecks.Assert(game.Craft.Ammo==0&&game.Craft.Stock(3)==10,"Wrong ammunition cannot reload a gun");
   game.Craft.CommitAmmo();var copy=Json.Read<ExpeditionCraftState>(Json.Write(game.Craft));copy.Normalize();MigrationChecks.Assert(copy.Magazines[2]==6&&copy.Stock(3)==10,"Typed stocks and magazines survive save roundtrip");
   Teleport(0,12);var storage=new BuildPiece{Kind=Array.FindIndex(BuildDefinition.All,d=>d.Function=="storage"),Id="gunplay/storage",X=0,Z=12,Y=Ground(0,12).Y,Health=100,Stock=new int[11]};game.Building.Pieces.Add(storage);
   game.Craft.GiveAmmo(1,20);int rifle=game.Craft.Stock(1);UseBlueprint(storage.Id+"|deposit|11");UseBlueprint(storage.Id+"|withdraw|11");MigrationChecks.Assert(game.Craft.Stock(1)==rifle&&game.Craft.Stock(0)==0&&storage.Stock.Length==15,"Shared storage preserves ammunition classes");game.Building.Pieces.Clear();
   game.Craft.BenchTier=5;game.Skills.Crafting.Level=4;game.Craft.Materials=Enumerable.Repeat(100,7).ToArray();
   for(int type=0;type<5;type++){string kind=new[]{"ammo","ammo-rifle","ammo-carbine","ammo-shell","ammo-cell"}[type];var recipe=ExpeditionRecipe.All.Single(r=>r.Kind==kind);int before=game.Craft.Stock(type);game.CraftRecipe(recipe);MigrationChecks.Assert(game.Craft.Stock(type)==before+recipe.Result,"Crafts correct ammo / "+kind);}
   game.Craft.Weapon=4;game.Stamina=100;var h=game.Handling;h.Tick(game,.1,0,true,false);double sway=h.Sway;
   h.Tick(game,.1,0,true,true);double stamina=game.Stamina;game.Tick(.1,false);
   MigrationChecks.Assert(h.HoldingBreath&&h.Sway<sway*.2&&game.Stamina==stamina,"Breath steadies real aim and prevents passive stamina refill");
   h.Tick(game,.1,2,true,true);MigrationChecks.Assert(!h.HoldingBreath,"Movement interrupts steady aim");
   game.Stamina=100;for(int i=0;i<60;i++)h.Tick(game,.1,0,true,true);
   MigrationChecks.Assert(!h.HoldingBreath&&h.NeedRelease,"Holding breath has a duration cap and needs release");
   for(int i=0;i<50;i++)h.Tick(game,.1,0,true,false);h.Tick(game,.1,0,true,true);MigrationChecks.Assert(h.HoldingBreath,"Breath returns after recovery and release");
   game.Craft.Weapon=3;var a=new ExpeditionGunHandling();var b=new ExpeditionGunHandling();a.Tick(game,.01,0,false,false);b.Tick(game,.01,0,false,false);double first=0;
   for(int i=0;i<7;i++){a.Shot(3);b.Shot(3);if(i==0)first=a.Spread(game);MigrationChecks.Assert(a.ShotOffset(game,0)==b.ShotOffset(game,0),"Repeatable spray pattern and bloom seed");a.Tick(game,.14,0,false,false);b.Tick(game,.14,0,false,false);}
   MigrationChecks.Assert(a.Bloom>1&&a.Spread(game)>first&&a.RecoilY>.5,"Repeated fire builds bloom and vertical recoil");for(int i=0;i<6;i++)a.Tick(game,.5,0,false,false);MigrationChecks.Assert(a.Bloom==0&&a.Streak==0,"Burst control recovers accuracy");
   game.Craft.Weapons.AddRange(new[]{17,22,23,24,25,26,27});game.Craft.Weapon=27;game.Craft.Fuel=100;game.Craft.GiveAmmo(4,40);game.Stamina=100;game.Handling.SpecialCooldown=0;game.Handling.HoldingBreath=false;
   ClearEnemies();player.Position=new(0,200,0);camera.Position=new(0,StandingEye,0);camera.LookAt(new Vector3(0,201,-4));
   AddEnemy("gunplay/open",0,9,1);var open=infected.Last();open.Body.Position=new(-1,200,-3);open.Hp=open.MaxHp=500;
   AddEnemy("gunplay/covered",0,9,1);var covered=infected.Last();covered.Body.Position=new(1,200,-3);covered.Hp=covered.MaxHp=500;
   wall=new Node3D();AddChild(wall);Box(wall,new(1,201,-1.5f),new(1.3f,3,.4f),Steel,true);await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
   int fuel=game.Craft.Fuel,cells=game.Craft.Stock(4);StartChainFlame();StartChainFlame();TickChainCombat(.1);
   MigrationChecks.Assert(game.Craft.Fuel==fuel-8&&game.Craft.Stock(4)==cells-4&&game.Stamina==80&&flameBursts.Count==1,"Flame special spends costs once and enforces cooldown");
   MigrationChecks.Assert(open.Hp<500&&covered.Hp==500,"Flame cone damages exposed enemies but cannot pass through walls");
   RemoveChild(wall);wall.QueueFree();wall=null;ClearEnemies();AddEnemy("gunplay/chain-contact",0,9,1);var contact=infected.Last();contact.Body.Position=new(0,200,-2.2f);contact.Hp=contact.MaxHp=500;camera.LookAt(contact.Body.Position+Vector3.Up);await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
   shotClock=reloadClock=0;cells=game.Craft.Stock(4);fuel=game.Craft.Fuel;BeginMelee();TickPrimitiveCombat(.25);
   MigrationChecks.Assert(contact.Hp<500&&contact.Stagger>=.55&&game.Craft.Stock(4)==cells-1&&game.Craft.Fuel==fuel-1,"Electric chain contact consumes power, damages and staggers");
   game.Craft.GiveAmmo(4,-game.Craft.Stock(4));shotClock=0;fuel=game.Craft.Fuel;BeginMelee();MigrationChecks.Assert(game.Craft.Fuel==fuel&&primitiveStrikes.Count==0,"Unpowered chain sword cannot consume fuel or attack");
   for(int id=22;id<=27;id++){var rig=new Node3D();AddChild(rig);BladeModel(rig,id,false);MigrationChecks.Assert(rig.GetChildCount()>0&&ExpeditionRecipe.All.Any(r=>r.Kind=="weapon"&&r.Result==id),"Blade has recipe and geometry");rig.QueueFree();}
   GD.Print("PASS / typed ammo migration, distinct reloads/magazines/storage/crafting/save, breath/sway/stamina/recovery, repeatable recoil/bloom, flame costs/cooldown/wall occlusion, electric chain contact and six blade models");
  }finally{if(wall!=null){RemoveChild(wall);wall.QueueFree();}ClearEnemies();primitiveStrikes.Clear();game=original;viewGunApplied=oldView;camera.Transform=eye;Teleport(0,12);}
 }
 async void RunGunplayChecks(){try{await GunplayChecks();GetTree().Quit();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}

