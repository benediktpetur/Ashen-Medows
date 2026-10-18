using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 double OperatingModule(int kind)=>Community.Established&&NearActiveSurface(Ground(Community.X,Community.Z),120)?game.ModulePower(kind):0;
 double WatchBonus(Vector3 point)=>Community.Established&&Distance(point,Ground(Community.X,Community.Z))<35?OperatingModule(2)/100.0:0;
 bool NearMegaCamp=>dungeon==null&&Community.Established&&Distance(player.Position,Ground(Community.X,Community.Z))<20;
 bool ModuleStation(int kind)=>kind==1?game.Craft.BenchTier>=5:kind==4?Community.Survivors.Any(n=>n.Member):game.Building.Pieces.Any(p=>p.Health>0&&ExpeditionGame.Distance(p.X,p.Z,Community.X,Community.Z)<30&&(kind==0?p.Def.Function=="water":kind==2?p.Def.Function=="alarm"||p.Def.Function=="turret":p.Def.Function=="farm"));
 bool MegaGearAction(string id){
  if(id.StartsWith("armor/")){string key=id.Substring(6);game.Change(()=>game.EquipGear(key));return true;}
  if(id.StartsWith("upgrade/")){string key=id.Substring(8);if(key=="none"||game.Life.SoulUpgrades.Any(m=>m.Id==key&&m.Owner==game.Life.SoulOwner))game.Change(()=>game.Life.ActiveUpgrade=key);return true;}
  if(id.StartsWith("companion/")){string key=id.Substring(10);if(game.Life.SoulCompanions.Any(m=>m.Id==key&&m.Owner==game.Life.SoulOwner))game.Change(()=>game.Life.ActiveCompanionGear=key);return true;}
  if(id.StartsWith("reinforce/")||id.StartsWith("ward/")){
   bool ward=id.StartsWith("ward/");var armor=game.Profile.Items.Find(a=>a.Id==id.Substring(ward?5:10)&&a.SoulOwner==game.Life.SoulOwner);
   if(armor==null)return true;if(ward?game.Life.ArmorMods<1||armor.Ward>0:game.Life.ArmorReinforcements<3||armor.Reinforced>=3){game.Notice="No materials or upgrade limit reached";return true;}
   game.Change(()=>{if(ward){game.Life.ArmorMods--;armor.Ward=1;armor.Armor+=8;armor.Recovery+=5;}else{game.Life.ArmorReinforcements-=3;armor.Reinforced++;armor.Armor+=3;}});return true;
  }
  if(id.StartsWith("install/")||id.StartsWith("remove/")||id.StartsWith("tune/")){
   if(!NearMegaCamp){game.Notice="Stand within 20m of your established camp";return true;}
   game.World.Changes.CampModules??=new();string key=id.Substring(id.IndexOf('/')+1);var m=game.Life.SoulModules.Find(x=>x.Id==key&&x.Owner==game.Life.SoulOwner);
   if(id.StartsWith("remove/")){game.Change(()=>game.World.Changes.CampModules.RemoveAll(x=>x.Id==key&&x.Owner==game.Life.SoulOwner));return true;}
   if(m==null)return true;
   if(id.StartsWith("tune/")){if(game.Life.CommunityParts<3||m.Power>=60){game.Notice="Needs 3 community parts / maximum module power 60%";return true;}game.Change(()=>{game.Life.CommunityParts-=3;m.Power=Math.Min(60,m.Power+5);m.Upkeep++;foreach(var active in game.World.Changes.CampModules.Where(x=>x.Id==key)){active.Power=m.Power;active.Upkeep=m.Upkeep;}});return true;}
   string gate=game.ModuleGate(m.Kind);if(gate!="Ready"||!ModuleStation(m.Kind)){game.Notice=gate!="Ready"?gate:"Build its matching water station, alarm/turret, farm or recruit a resident first";return true;}
   if(!game.Change(()=>{if(!game.InstallModule(key))game.Notice="Three slots maximum / one module of each type";}))return true;
   return true;
  }
  return false;
 }
 void TickCampModules(double dt){
  var list=game.World.Changes.CampModules;if(list==null||!Community.Established||!NearActiveSurface(Ground(Community.X,Community.Z),120))return;
  foreach(var m in list){if(!ModuleStation(m.Kind)){m.Charge=0;continue;}m.Charge=Math.Max(0,m.Charge-dt);if(m.Charge>0)continue;bool water=m.Kind==3;int stock=water?Community.Water:Community.Food;if(stock<m.Upkeep)continue;if(water)Community.Water-=m.Upkeep;else Community.Food-=m.Upkeep;m.Charge=300;}
 }
 double CampProduction(BuildPiece piece){if(!Community.Established||ExpeditionGame.Distance(piece.X,piece.Z,Community.X,Community.Z)>30)return 1;int kind=piece.Def.Function=="farm"?3:piece.Def.Function=="water"?0:1;return 1+OperatingModule(kind)/100.0;}
}
