using Godot;
using System;
using System.Threading.Tasks;
public partial class MigrationScene {
 async Task CaptureSurvivalSystems(){try{
  game.Skills.Strength.Level=3;game.Skills.Strength.Xp=160;game.Skills.Agility.Level=2;game.Skills.Agility.Xp=110;game.Skills.Stamina.Level=2;game.Skills.Stamina.Xp=160;game.Skills.Crafting.Xp=108;
  foreach(string slot in new[]{"head","chest","hands","feet"}){game.Profile.Items.Add(new ExpeditionRelic{Id="demo/"+slot,Slot=slot,Name="Woodland "+slot+" armor",Origin="Field workshop",Level=2,Armor=slot=="chest"?22:6,Vitality=slot=="chest"?6:2});game.EquipGear("demo/"+slot);}
  ShowSkills();await ArtShot("leveling-01-skills");CloseMenu();game.Skills.Stamina.Gain(50);UpdateProgression();await ArtShot("leveling-02-level-up");inventorySelected="demo/head";ShowEquipment();await ArtShot("systems-03-gear");CloseMenu();game.Life.Wounds.Add(new ExpeditionWound{Blood=420,Patched=true,Recovery=960});game.Life.Wounds.Add(new ExpeditionWound{Blood=290});ShowWounds();await ArtShot("systems-04-wounds");CloseMenu();
  var tree=props.Find(p=>p.Kind==0&&p.Tier<=2);player.Position=tree.Position+new Vector3(1.5f,0,0);camera.LookAt(tree.Position+Vector3.Up*1.2f);axeEquipped=true;BuildHeldTool();AxeSwing();await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);ChopTree();axeSwingStarted=Time.GetTicksMsec()/1000.0-.25;await ArtShot("systems-05-woodcutting");axeEquipped=false;BuildHeldTool();
  EnterDungeon(ExpeditionDungeon.Nearby(game.World,0,0)[0]);ClearEnemies();player.Position=new(0,.2f,2);camera.Position=new(0,1.65f,0);camera.LookAt(new Vector3(0,1.5f,-3));AddEnemy("preview/combat",0,-3,1);game.Craft.Weapon=4;game.Craft.Weapons.Add(4);game.Craft.Ammo=3;BuildHeldTool();await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);Shoot();await ArtShot("systems-06-combat");GD.Print("PASS / survival systems GPU captures");GetTree().Quit();
 }catch(Exception e){GD.PrintErr(e);GetTree().Quit(1);}}
}
