using Godot;
using System;
public partial class MigrationScene {
 ColorRect menuShade;
 void ShowCleanPause(){
  var b=Panel(CoopActive?"ASHEN MEADOW / ONLINE":"ASHEN MEADOW / PAUSED");
  Text(b,CoopActive?"Your party’s world continues while this menu is open.":"Take a breath. Your expedition is paused.");
  Button(b,"RESUME EXPEDITION",CloseMenu);
  var navigation=new GridContainer{Columns=2};b.AddChild(navigation);
  Button(navigation,"INVENTORY / I",ShowEquipment);Button(navigation,"CRAFTING & RESEARCH / K",ShowCrafting);
  Button(navigation,"CHARACTER / J",ShowTalentTree);Button(navigation,"CAMP / F7",()=>ShowCommunity());
  Button(navigation,"FIELD NOTES / F1",()=>ShowFirstDay());Button(navigation,"EXPEDITIONS / F5",ShowJourneyPlanner);
  Button(navigation,"JOURNAL / N",ShowJournal);Button(navigation,"MAP / M",()=>{if(dungeon==null)ShowMap();else game.Notice="Return to the surface to use the world map";});
  b.AddChild(new HSeparator());
  var settings=new GridContainer{Columns=2};b.AddChild(settings);Button(settings,"OPTIONS",ShowOptions);Button(settings,"CONTROLS / F2",ShowControls);
  Button(settings,"MULTIPLAYER / F8",ShowMultiplayer);Button(settings,"WORLD & SAVES",ShowWorldManagement);
  Button(b,"SAVE PROGRESS",()=>{game.Notice=game.Persist()?"Progress saved":game.Notice;ShowCleanPause();});
  if(!string.IsNullOrWhiteSpace(game.Notice))Text(b,game.Notice);
  Button(b,"SAVE & QUIT",()=>{if(CoopActive)StopCoop("Session closed");game.Persist();GetTree().Quit();});
 }
 void ShowWorldManagement(){
  var b=Panel("WORLD & SAVES");Text(b,"Your character is saved outside the game’s installation. Each seed keeps its own buildings and world changes.");
  Button(b,"RETURN TO BRIAR LANE / STARTING SUBURB",()=>{if(CoopActive){game.Notice="Leave co-op before relocating.";ShowWorldManagement();return;}ExitDungeon(false);ClearEnemies();Teleport(ExpeditionWorld.HomeX,ExpeditionWorld.HomeZ);game.Persist();CloseMenu();});
  Text(b,"World seed");var seed=new SpinBox{MinValue=1,MaxValue=999999,Value=game.Save.Seed};b.AddChild(seed);
  Button(b,"OPEN SELECTED WORLD",()=>{if(CoopActive){game.Notice="Leave co-op before switching worlds.";ShowWorldManagement();return;}ExitDungeon(false);game.Change(()=>{game.Save.Seed=(int)seed.Value;game.Save.X=ExpeditionWorld.SpawnX;game.Save.Z=ExpeditionWorld.SpawnZ;game.Normalize();});ClearEnemies();ClearChildren(worldRoot);props.Clear();uniqueObjects.Clear();RebuildTerrain();SyncStructures();Teleport(ExpeditionWorld.SpawnX,ExpeditionWorld.SpawnZ);CloseMenu();});
  b.AddChild(new HSeparator());Text(b,"Legacy import replaces this character and keeps the previous save as a .bak backup.");Button(b,"IMPORT LEGACY CHARACTER",ImportDialog);
  Text(b,game.Notice);Button(b,"BACK",ShowPause);
 }
 void ShowControls(){
  var b=Panel("CONTROLS / FIELD REFERENCE");
  var scroll=new ScrollContainer{CustomMinimumSize=new(890,430)};b.AddChild(scroll);var list=new VBoxContainer{CustomMinimumSize=new(870,0)};list.AddThemeConstantOverride("separation",12);scroll.AddChild(list);
  foreach(var entry in new[]{
   ("MOVE","W A S D  Move     Shift  Sprint     Space  Jump     Ctrl  Hold to crouch"),
   ("INTERACT","F  Interact     G  Throw a stone distraction     L  Flashlight"),
   ("COMBAT","Left mouse  Attack     Right mouse  Aim     Shift while aiming  Hold breath\nR  Reload     1 / 2 / 3  Combat classes     Q  Chain-sword special"),
   ("TOOLS & SURVIVAL","T  Equip axe     H  Bandage     V  Eat     C  Drink clean water\nP  Make a path     O  Dig a well     Y  Health and water treatment"),
   ("BUILDING","B  Build     Tab  Catalogue     Mouse wheel  Change piece\nQ / E  Rotate (Ctrl for fine rotation)     G  Toggle snapping\nPage Up / Down  Height     + / −  Reach     R  Reset placement\nLeft mouse  Place     Right mouse  Remove"),
   ("YOUR EXPEDITION","I  Inventory     K  Crafting and research     J  Talents     N  Journal\nM  Map     F1  First-day field notes     F2  Controls\nF4  Boss field guide     F5  Expedition planner     F6  Sleeping bags\nF7  Camp community     F8  Multiplayer     U  Mute     Esc  Pause / close")}){
   PackLabel(list,entry.Item1,16);PackLabel(list,entry.Item2,13);
  }Button(b,"BACK TO WORLD",CloseMenu);
 }
 async void CaptureCleanInterface(){try{ShowCleanPause();await ArtShot("clean-01-pause");ShowControls();await ArtShot("clean-02-controls");ShowWorldManagement();await ArtShot("clean-03-world");ShowEquipment();await ArtShot("clean-04-inventory");ShowCrafting();await ArtShot("clean-05-crafting");CloseMenu();await ArtShot("clean-06-hud");GetTree().Quit();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}
