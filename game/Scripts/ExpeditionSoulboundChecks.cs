using System;
using System.Linq;
using Godot;
public partial class MigrationScene {
 async System.Threading.Tasks.Task SoulboundChecks(){
  MigrationChecks.Assert(game.Life.Souls.Count==1,"First Broodheart grants exactly one saved Soulbound drop");
  game.Craft.UnlockedTier=5;game.Career.Gain(1000);game.Craft.Weapons.Add(5);
  var a=SoulboundWeapon.Roll(game.Life.SoulOwner,5,"Test",1);var b=SoulboundWeapon.Roll(game.Life.SoulOwner,5,"Test",2);
  MigrationChecks.Assert(a.Stats!=b.Stats,"Same base gun has different stat rolls");game.Life.Souls.Add(a);game.Life.Souls.Add(b);
  game.Craft.Weapon=5;game.Life.ActiveSoul=null;double damage=game.Damage(false),spread=game.Handling.Spread(game);int mag=game.Magazine;
  MigrationChecks.Assert(game.EquipSoul(a.Id)&&Math.Abs(game.Damage(false)/damage-(1+a.Damage/100.0))<.001,"Rolled damage changes real combat damage");
  MigrationChecks.Assert(Math.Abs(game.Handling.Spread(game)/spread-(1+a.Accuracy/100.0))<.001&&game.Magazine==Math.Max(1,(int)Math.Round(mag*(1+a.Capacity/100.0))),"Accuracy and magazine rolls affect live stats");
  game.Craft.Ammo=game.Magazine;game.Craft.Reserve=50;int total=game.Craft.Ammo+game.Craft.Reserve;
  game.EquipSoul(b.Id);MigrationChecks.Assert(game.Craft.Ammo+game.Craft.Reserve==total&&game.Craft.Ammo<=game.Magazine,"Switching copies preserves ammo without overfilled magazines");
  var foreign=SoulboundWeapon.Roll("another-character",5,"Test",3);game.Life.Souls.Add(foreign);MigrationChecks.Assert(!game.EquipSoul(foreign.Id),"Foreign character cannot equip bound gear");game.Life.NormalizeSouls();
  int count=game.Life.Souls.Count;game.ClaimSoul("test/soul-once","Test",4);game.ClaimSoul("test/soul-once","Test",5);MigrationChecks.Assert(game.Life.Souls.Count==count+1,"Drop claims cannot reroll or duplicate");
  var copy=Json.Read<ExpeditionLife>(Json.Write(game.Life));copy.Normalize();MigrationChecks.Assert(copy.Souls.Count==count+1&&copy.Souls.Any(s=>s.Id==a.Id&&s.Stats==a.Stats)&&copy.SoulOwner==game.Life.SoulOwner,"Rolls and ownership survive save roundtrip");
  ClearEnemies();shotClock=reloadClock=0;player.Position=new(0,.1f,-18.5f);game.Life.NightEssence=40;game.Life.Health=game.MaxHealth;
  for(int i=0;i<3;i++){
   game.World.Changes.Time=Math.Max(game.World.Changes.Time,game.World.Changes.MegaReady);int before=game.MegaLootCount,price=game.Life.NightEssence;
   SoulAction("mega:"+i);var boss=infected.Single(e=>IsMega(e.Id));MigrationChecks.Assert(boss.MaxHp>=4800&&game.Life.NightEssence==price-8,"Mega hunt consumes catalysts and spawns a physical boss");
   int runs=game.World.Changes.MegaRuns;SoulAction("mega:"+i);MigrationChecks.Assert(game.World.Changes.MegaRuns==runs,"Active hunts cannot be duplicated");
   boss.BossTime=0;TickCampaignBoss(boss,.01);MigrationChecks.Assert(boss.BossState.EndsWith("windup"),"Mega boss has visible attack warning");
   DefeatCampaignBoss(boss);DefeatCampaignBoss(boss);MigrationChecks.Assert(game.MegaLootCount==before+1,"Mega victory grants one private drop and rejects duplicate death");
   SoulAction("mega:"+i);MigrationChecks.Assert(!infected.Any(e=>IsMega(e.Id)),"World cooldown prevents immediate repeat summons");
  }
  await MegaSpecialistChecks();ShowSoulArmory();CloseMenu();GD.Print("PASS / Soulbound rolls, damage/spread/capacity, ownership, ammo conservation, persistence, three repeatable mega hunts, costs/cooldown and duplicate protection");
 }
}
