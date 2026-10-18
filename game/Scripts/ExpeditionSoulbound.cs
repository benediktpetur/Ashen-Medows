using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class MigrationScene {
 static readonly string[] MegaNames={"The Sump Colossus","The Brood Matriarch","The Iron Mourner"};
 static bool IsMega(string id)=>id?.Contains("/mega/")==true&&ExpeditionCampaign.BossIndex(id)==2;
 static int MegaVariant(string id)=>IsMega(id)&&int.TryParse(id.Split('/')[6],out int n)?Math.Clamp(n,0,2):0;
 void SoulAction(string action){
  if(reloadClock>0||shotClock>0){game.Notice="Finish the current weapon action first.";return;}
  if(action.StartsWith("soul:")){
   string id=action.Substring(5);if(MegaGearAction(id))return;game.Change(()=>{if(id=="base"){game.Life.ActiveSoul=null;game.TrimSoulAmmo();}else game.EquipSoul(id);});axeEquipped=false;BuildGun();return;
  }
  if(!InHeartRoom||Distance(player.Position,new(0,0,-19))>=3||!game.World.Changes.CampaignBosses.Contains(2))return;
  if(!int.TryParse(action.Substring(5),out int variant)||variant<0||variant>2)return;
  if(infected.Any(e=>IsCampaignBoss(e.Id))){game.Notice="A hunt is already active.";return;}
  var world=game.World.Changes;
  if(world.Time<world.MegaReady){game.Notice=$"Pressure recovering / {Math.Ceiling(world.MegaReady-world.Time)} seconds";return;}
  if(game.Life.NightEssence<8){game.Notice="Hunt requires 8 night catalysts / spent when summoned";return;}
  if(!game.Change(()=>{game.Life.NightEssence-=8;world.MegaRuns++;world.MegaVariant=variant;world.MegaReady=world.Time+300;}))return;
  string idBoss=$"boss/campaign/{game.Save.Seed}/2/mega/{world.MegaRuns}/{variant}";
  AddEnemy(idBoss,0,-9,5);var boss=infected.FirstOrDefault(e=>e.Id==idBoss);if(boss!=null)campaignParticipants[idBoss]=new HashSet<ExpeditionGame>();
  CloseMenu();game.Notice=MegaNames[variant]+" / TIER 5 MEGA HUNT / retreat loses the catalyst cost";
 }
 void DefeatMega(Infected boss){
  if(!infected.Contains(boss))return;
  var authority=CoopHost?coopHostGame:game;
  if(!authority.Change(()=>authority.World.Changes.MegaVictories++)){boss.Hp=1;return;}
  if(!campaignParticipants.TryGetValue(boss.Id,out var participants))participants=new();
  foreach(var p in MillPlayers().Where(p=>p.Game.Life.Health>0&&Distance(p.Body.Position,boss.Home)<65))participants.Add(p.Game);
  foreach(var character in participants.Where(c=>MillPlayers().Any(p=>p.Game==c&&c.Life.Health>0&&Distance(p.Body.Position,boss.Home)<65)))
   character.ClaimMegaLoot(boss.Id+"/soul",MegaVariant(boss.Id),MegaNames[MegaVariant(boss.Id)],Guid.NewGuid().GetHashCode());
  ClearMegaChildren(boss);campaignParticipants.Remove(boss.Id);ClearBrood();ClearMillPools();BloodImpact(boss.Body.Position+Vector3.Up,Vector3.Up,40);DeathRemains(boss);RemoveEnemy(boss);
  foreach(var add in infected.Where(e=>e.Id.StartsWith(boss.Id+"/add/")).ToArray())RemoveEnemy(add);
  authority.Notice="MEGA BOSS DEFEATED / Soulbound drop in Inventory > Soulbound armory";
  if(CoopHost&&coopActors.Count>0)Rpc(nameof(CoopWorldUpdate),Json.Write(WorldSnapshot()));
 }
 void ShowSoulArmory(){
  var box=Panel("SOULBOUND / ARMORY AND CAMP RELICS");var scroll=new ScrollContainer{CustomMinimumSize=new(900,400)};box.AddChild(scroll);var list=new VBoxContainer{CustomMinimumSize=new(860,0)};scroll.AddChild(list);
  void Action(string label,string op){Button(list,label,()=>{if(!CoopRequest("campaign","soul:"+op))SoulAction("soul:"+op);ShowSoulArmory();});}
  Text(list,"TIER 5 / THREE DISTINCT LOOT TABLES\n8 night catalysts per hunt; five-minute world cooldown. Summon at the pumping chamber console after defeating the Broodheart. One guaranteed specialist drop for each living player present at victory. All relics belong to your character across death and servers.");
  foreach(int i in new[]{2,0,1}){int n=i;Text(list,MegaNames[i].ToUpperInvariant()+" / "+(i==2?"WEAPONS: legendary gun, bow or melee weapon + one upgrade; 20% second upgrade. Dodge scrap volleys and canisters; counterattack during overheat.":i==0?"ARMOR: one legendary rolled armor piece + 3 reinforcement plates; 25% rare ward. Break the amber growths to expose its shell. Slow slams and charges.":"COMMUNITY: one rolled camp module + 3 community parts; 25% companion harness. Destroy nests to shrink each wave, then attack during the recovery break."));Button(list,"SUMMON / "+MegaNames[i],()=>{if(!CoopRequest("campaign","mega:"+n))SoulAction("mega:"+n);});}
  Text(list,"WEAPON COPIES / positive damage and capacity improve stats; negative spread/reload is better. Bows roll arrow speed, draw time and noise; melee rolls stamina efficiency, attack recovery and noise.");
  foreach(var item in game.Life.Souls){Text(list,item.Name+" / "+item.Source+"\n"+item.Stats);Action(game.ActiveSoul?.Id==item.Id?"EQUIPPED":"EQUIP THIS COPY",item.Id);}
  Action("USE STANDARD WEAPON STATS","base");
  Text(list,"WEAPON UPGRADES / one active upgrade. Each applies to its matching class only.");foreach(var m in game.Life.SoulUpgrades)Action((game.Life.ActiveUpgrade==m.Id?"ACTIVE / ":"")+m.Description,"upgrade/"+m.Id);Action("REMOVE WEAPON UPGRADE","upgrade/none");
  Text(list,$"LEGENDARY ARMOR / reinforcement plates {game.Life.ArmorReinforcements} / rare wards {game.Life.ArmorMods}");
  foreach(var a in game.Profile.Items.Where(a=>a.SoulOwner==game.Life.SoulOwner)){Text(list,a.Name+" / "+a.SoulStats);Action(game.IsGearEquipped(a.Id)?"EQUIPPED":"EQUIP "+a.GearSlot,"armor/"+a.Id);Action($"REINFORCE / 3 PLATES / +3 ARMOR ({a.Reinforced}/3)","reinforce/"+a.Id);Action("APPLY RARE WARD / +8 ARMOR +5% RECOVERY / ONCE","ward/"+a.Id);}
  game.World.Changes.CampModules??=new();Text(list,$"CAMP MODULES / {game.World.Changes.CampModules.Count}/3 slots / {game.Life.CommunityParts} tuning parts\nInstall/remove/tune within 20m of your camp. Matching stations and existing research are required. Modules draw upkeep from camp stores every five active minutes while camp is occupied. Unfunded modules stop. Equal types do not stack. Tuning costs 3 parts for +5% power and +1 upkeep.");
  foreach(var m in game.World.Changes.CampModules){Text(list,"INSTALLED / "+m.Description+$" / funded {m.Charge:0}s");if(m.Owner==game.Life.SoulOwner)Action("REMOVE MODULE","remove/"+m.Id);}
  foreach(var m in game.Life.SoulModules){Text(list,m.Description+" / "+game.ModuleGate(m.Kind));Action("INSTALL MODULE","install/"+m.Id);Action("TUNE MODULE / 3 PARTS","tune/"+m.Id);}
  foreach(var m in game.Life.SoulCompanions)Action((game.Life.ActiveCompanionGear==m.Id?"ACTIVE / ":"")+m.Description,"companion/"+m.Id);
  Text(box,game.Notice);Button(box,"BACK TO EXPEDITION",CloseMenu);
 }
}
