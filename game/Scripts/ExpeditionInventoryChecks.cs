using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 void InventoryActionChecks(){
  void Press(string text){var button=menu.FindChildren("*","Button",true,false).OfType<Button>().FirstOrDefault(b=>b.Text==text);MigrationChecks.Assert(button!=null&&!button.Disabled,"Inventory action unavailable / "+text);button.EmitSignal(Godot.Button.SignalName.Pressed);}
  game.Craft.Weapons=new(){0,1};game.Craft.Weapon=1;inventoryFilter="WEAPONS";inventorySearch="";inventorySelected="gun/0";SurvivorInventory();Press("EQUIP");MigrationChecks.Assert(game.Craft.Weapon==0,"Inventory weapon equip");
  game.Profile.Items.Add(new ExpeditionRelic{Id="inventory/action-plate",Name="Test chest",Origin="Checks",Level=2,Armor=21,Vitality=6});inventoryFilter="ARMOR";inventorySelected="inventory/action-plate";SurvivorInventory();Press("EQUIP");MigrationChecks.Assert(game.Profile.Equipped=="inventory/action-plate","Inventory armor equip");
  game.Life.Wounds.Clear();game.Life.Wounds.Add(new ExpeditionWound());game.Life.Bandages=2;game.Life.Health=60;inventoryFilter="SUPPLIES";inventorySelected="bandage";SurvivorInventory();Press("PATCH WOUND");MigrationChecks.Assert(game.Life.Bandages==1&&game.Life.Wounds[0].Patched&&game.Life.Health==60,"Inventory bandage healing/consumption");
  game.Life.Food=2;game.Life.Hunger=50;inventorySelected="food";SurvivorInventory();Press("USE ITEM");MigrationChecks.Assert(game.Life.Food==1&&game.Life.Hunger==90,"Inventory food consumption");game.Craft.Water=2;game.Life.Hydration=40;inventorySelected="water";SurvivorInventory();Press("USE ITEM");MigrationChecks.Assert(game.Craft.Water==1&&game.Life.Hydration==85,"Inventory water consumption");var saved=new ExpeditionGame(game.PathName);MigrationChecks.Assert(saved.Craft.Weapon==0&&saved.Profile.Equipped=="inventory/action-plate"&&saved.Life.Bandages==1&&saved.Craft.Water==1,"Inventory action persistence");
  game.Life.Wounds.Clear();game.Life.Wounds.Add(new ExpeditionWound{Patched=true,Recovery=60});game.Life.Wounds.Add(new ExpeditionWound{Blood=240});MigrationChecks.Assert(WoundStatus().Contains("HP per minute"),"Health drain is shown instead of a fatal countdown");inventorySearch="no-match-123";SurvivorInventory();MigrationChecks.Assert(menu.FindChildren("*","Label",true,false).OfType<Label>().Any(l=>l.Text=="NO ITEMS FOUND"),"Inventory empty-search state");inventoryFilter="ALL";inventorySearch="";CloseMenu();GD.Print("PASS / inventory UI: actual equip/use button actions, no bandage instant healing, consumption/save, empty search and mixed-wound countdown");
 }
}

