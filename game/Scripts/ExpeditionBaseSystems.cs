using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]void CoopStation(string id){if(CoopClient){var p=game.Building.Pieces.FirstOrDefault(p=>p.Id==id);if(p!=null)ShowBaseStation(p);}}
 double defenseClock;string basePanelId;bool baseVisualPower;
 bool Powered(BuildPiece p)=>p.Def.Function=="power"?p.Charge>0:game.Building.Pieces.Any(g=>g.Def.Function=="power"&&g.Charge>0&&ExpeditionGame.Distance(g.X,g.Z,p.X,p.Z)<24);
 BuildPiece NearBlueprint()=>dungeon==null?game.Building.Pieces.Where(p=>(p.Def.Function!="structure"||p.Health<p.Def.Durability)&&ExpeditionGame.Distance(p.X,p.Z,player.Position.X,player.Position.Z)<3.2).OrderBy(p=>ExpeditionGame.Distance(p.X,p.Z,player.Position.X,player.Position.Z)).FirstOrDefault():null;
 void TickBaseSystems(double dt){if(CoopClient||dungeon!=null)return;TickCampModules(dt);foreach(var p in game.Building.Pieces){if(p.Stock==null)p.Stock=new int[15];if(p.Def.Function=="power")p.Charge=Math.Max(0,p.Charge-dt);if(p.Def.Function is "water" or "food" or "farm"){
   if(p.Def.Function=="water"&&p.Def.Tier==1){if(p.Charge<=0)continue;p.Charge=Math.Max(0,p.Charge-dt);}bool active=p.Def.Tier<4||Powered(p);if(!active)continue;p.Progress+=dt*CampProduction(p);double duration=p.Def.Function=="water"?120:p.Def.Function=="farm"?360:240;if(p.Progress>=duration){p.Progress-=duration;int slot=p.Def.Function=="water"?8:7;p.Stock[slot]=Math.Min(20*p.Def.Tier,p.Stock[slot]+p.Def.Tier);}}
   if(p.Def.Function=="fridge"||p.Def.Function=="storage"){if(p.Stock[7]>0&&!(p.Def.Function=="fridge"&&Powered(p))){p.Progress+=dt;if(p.Progress>=1800){p.Progress=0;p.Stock[7]--;}}}
  }
  bool power=game.Building.Pieces.Any(p=>p.Def.Function=="power"&&p.Charge>0);if(power!=baseVisualPower){baseVisualPower=power;SyncStructures();}
  defenseClock-=dt;if(defenseClock>0)return;defenseClock=1;foreach(var child in buildRoot.GetChildren())if(child is Node3D n)n.Visible=dungeon==null&&Distance(n.Position,player.Position)<220;
  foreach(var p in game.Building.Pieces.ToArray()){
   string f=p.Def.Function;if(f=="lure"&&p.Charge>0){p.Charge-=1;foreach(var e in infected)if(!Peaceful(e.Id)&&Distance(e.Body.Position,new((float)p.X,(float)p.Y,(float)p.Z))<45){e.Target=new((float)p.X,(float)p.Y,(float)p.Z);e.Memory=7;}}
   var targets=infected.Where(e=>!CheckpointPassive(e)&&!PassiveStoryEnemy(e)&&!Peaceful(e.Id)&&Math.Abs(e.Body.Position.Y-p.Y)<3&&ExpeditionGame.Distance(e.Body.Position.X,e.Body.Position.Z,p.X,p.Z)<(f=="turret"?24:f=="alarm"?18:Math.Max(p.Def.W,p.Def.D)/2+.8)).ToArray();
   if(f=="alarm"&&Powered(p)&&targets.Length>0)game.Notice="PERIMETER ALARM / threats near your base";
   if(f is "spikes" or "trap" or "electric" or "electrictrap" or "turret"){
    if((f is "electric" or "electrictrap"||f=="turret"&&p.Def.Tier>=5)&&!Powered(p))continue;if(f=="turret"&&p.Stock[10]<1)continue;if(f=="trap"&&p.Charge<1)continue;
    foreach(var e in targets.Take(f=="turret"?1:3)){if(f=="turret"&&!BlueprintSight(p,e))continue;if(f=="turret")p.Stock[10]--;if(f=="trap")p.Charge=0;e.Hp-=f=="turret"?35*p.Def.Tier:8*p.Def.Tier;e.Stagger=.5;EnemyImpact(e,e.Body.Position+Vector3.Up*.5f,Vector3.Up,5);if(e.Hp<=0)KillEnemy(e);if(f=="trap")break;}
   }
  }
 }
 void ShowBaseStation(BuildPiece p){if(p.Stock==null)p.Stock=new int[15];else if(p.Stock.Length<15)Array.Resize(ref p.Stock,15);basePanelId=p.Id;var box=Panel(p.Def.Name.ToUpperInvariant());coopMenu="base";Text(box,$"TIER {p.Def.Tier} / CONDITION {p.Health:0}/{p.Def.Durability:0}\n"+(p.Def.Opening=="window"?"WINDOW FRAME / open aperture":p.Def.Opening!=""?"MANUAL HARDWARE / no generator needed":Powered(p)?"POWERED / generator within 24m":"UNPOWERED / powered stations need a fueled generator within 24m"));
  void Action(string title,string operation){Button(box,title,()=>{string data=p.Id+"|"+operation;if(!CoopRequest("baseuse",data))UseBlueprint(data);var current=game.Building.Pieces.FirstOrDefault(x=>x.Id==basePanelId);if(current!=null)ShowBaseStation(current);else CloseMenu();});}
  string f=p.Def.Function;switch(f){
   case "bed":Text(box,"One cot per resident. Shelter needs a roof overhead and two nearby walls. F7 opens community management.");Button(box,"MANAGE COMMUNITY",()=>ShowCommunity());break;case "structure":Text(box,$"Structural capacity {p.Def.Capacity:0}. Repair restores this piece without changing its placement or supports.");break;
   case "door":Action(p.Open?"CLOSE DOOR":"OPEN DOOR","door");break;case "window":Text(box,"Solid wall and sill with a real window opening. Fire through the aperture; higher tiers have a metal mullion. This section also supports the roof.");break;
   case "storage":case "fridge":Text(box,"Storage is shared by this world's players. Stored food spoils one portion per 30 active minutes; a powered refrigerator prevents spoilage.");var sc=new ScrollContainer{CustomMinimumSize=new(880,260)};box.AddChild(sc);var rows=new VBoxContainer();sc.AddChild(rows);for(int i=0;i<15;i++){int index=i;string name=i<7?ExpeditionGame.ResourceNames[i]:i<10?new[]{"Food","Water","Bandages"}[i-7]:ExpeditionGame.AmmoNames[i-10];Button(rows,$"{name} / STORED {p.Stock[i]} / DEPOSIT 10",()=>{if(!CoopRequest("baseuse",p.Id+"|deposit|"+index))UseBlueprint(p.Id+"|deposit|"+index);ShowBaseStation(game.Building.Pieces.First(x=>x.Id==p.Id));});Button(rows,"WITHDRAW 10 / "+name,()=>{if(!CoopRequest("baseuse",p.Id+"|withdraw|"+index))UseBlueprint(p.Id+"|withdraw|"+index);ShowBaseStation(game.Building.Pieces.First(x=>x.Id==p.Id));});}break;
   case "water":case "food":case "farm":Text(box,$"Stored production: {p.Stock[f=="water"?8:7]} / progress {p.Progress:0} seconds. Water: 2 minutes; drying: 4; crops: 6. Tier 4+ stations need power. No offline production.");if(f=="water"&&p.Def.Tier==1)Action("FUEL KETTLE / 1 TIMBER","kettle");Action("COLLECT PRODUCE","collect");break;
   case "power":Text(box,$"Fuel remaining: {p.Charge:0} seconds. Supplies nearby powered equipment. Efficient generators run twice as long.");Action("FUEL / 5 FIBER + 1 RESIN","fuel");break;
   case "cook":Text(box,"Cook trail provisions for 4 fiber, or boil water using 2 timber. Boiling consumes untreated water. Collect it from a well first.");Action("COOK / 2 PROVISIONS","cook");Action("BOIL / 2 WATER","boil");break;
   case "medical":Text(box,"Stand within 3m, patch all wounds and stay fed/hydrated for improved slow recovery. Bandages still need the full 20 minutes. Powered stations make field dressings.");Action("MAKE DRESSINGS / 3 FIBER","dressings");break;
   case "repair":Text(box,"Repair nearby vehicles and mobile structures for 3 alloy scrap, or repair this station for 2 timber.");Action("REPAIR NEARBY VEHICLES","repairvehicle");break;
   case "lure":Action("ACTIVATE FOR 60 SECONDS / 1 ELECTRONIC","lure");break;
   case "turret":Text(box,$"Ammunition: {p.Stock[10]}. Fires at hostile targets within 24m. Tier 5 also needs power.");Action("LOAD 30 PISTOL ROUNDS","load");break;
   case "trap":Action("ARM SNARE / 1 FIBER","arm");break;
   case "mobile":case "vehicle":Text(box,"W/S drive or reverse, A/D steer. F dismounts. Repair at a vehicle station. Trailer also provides a portable rest point; supplies travel with it.");Action("ENTER DRIVER SEAT","mount");Action("STORE MATERIALS IN VEHICLE","cargo-store");Action("WITHDRAW MATERIALS","cargo-take");if(f=="mobile")Action("REST / RESTORE ENERGY","rest");break;
   case "platform":Text(box,"Jump onto the lower ladder rungs and climb using repeated jumps; the deck and posts have collision.");break;
   default:Text(box,"Passive defense / operates when hostile enemies approach. Powered equipment needs a fueled generator within 24m.");break;
  }Action("REPAIR / 2 TIMBER","repair");Button(box,"BACK",CloseMenu);
 }
 void UseBlueprint(string data){var parts=data.Split('|');if(parts.Length<2)return;var p=game.Building.Pieces.FirstOrDefault(p=>p.Id==parts[0]);if(p==null||dungeon!=null||ExpeditionGame.Distance(p.X,p.Z,player.Position.X,player.Position.Z)>4)return;if(p.Stock==null)p.Stock=new int[15];else if(p.Stock.Length<15)Array.Resize(ref p.Stock,15);string op=parts[1],f=p.Def.Function;
  if(op=="mount"&&(f is "mobile" or "vehicle")){if(vehicleDrivers.TryGetValue(p.Id,out long current)&&current!=coopActor){game.Notice="Vehicle already has a driver";return;}vehicleDrivers[p.Id]=coopActor;player.Position=new((float)p.X,(float)(p.Y+p.Def.H+.2),(float)p.Z);if(CoopHost&&coopActor>0)RpcId(coopActor,nameof(CoopMount),p.Id);else{mountedBlueprint=p.Id;CloseMenu();}return;}
  int swing=p.DoorSwing;if(op=="door"&&p.Def.Opening!=""){if(!p.Open){p.Local(player.Position.X,player.Position.Z,out _,out double localZ);swing=localZ<0?-1:1;}if(OpeningSweepBlocked(p,swing)){game.Notice="Opening blocked / clear the doorway and swing space";return;}}
  game.Change(()=>{
   int Owned(int index)=>index<7?game.Craft.Materials[index]:index==7?game.Life.Food:index==8?game.Craft.Water:index==9?game.Life.Bandages:game.Craft.Stock(index-10);
   void Add(int index,int delta){if(index<7)game.Craft.Materials[index]+=delta;else if(index==7)game.Life.Food+=delta;else if(index==8)game.Craft.Water+=delta;else if(index==9)game.Life.Bandages+=delta;else game.Craft.GiveAmmo(index-10,delta);}
   if(op=="door"&&f=="door"){if(p.Def.Tier>=4&&p.Def.Opening==""&&!Powered(p)){game.Notice="Door requires power";return;}p.DoorSwing=swing;p.Open=!p.Open;game.Notice=(p.Open?"Opened ":"Closed ")+p.Def.Name;}
   if(op is "deposit" or "withdraw"&&(f is "storage" or "fridge")&&parts.Length==3&&int.TryParse(parts[2],out int index)&&index>=0&&index<15){int count=Math.Min(10,op=="deposit"?Math.Min(Owned(index),200*p.Def.Tier-p.Stock[index]):p.Stock[index]);if(count>0){Add(index,op=="deposit"?-count:count);p.Stock[index]+=op=="deposit"?count:-count;}}
   if(op=="kettle"&&f=="water"&&p.Def.Tier==1&&game.Craft.Materials[0]>0){game.Craft.Materials[0]--;p.Charge+=120;}
   if(op=="collect"&&(f is "water" or "food" or "farm")){int i=f=="water"?8:7;Add(i,p.Stock[i]);p.Stock[i]=0;game.AwardResearch("milestone/harvest",8);}
   if(op=="fuel"&&f=="power"&&game.Craft.Materials[2]>=5&&game.Craft.Materials[3]>=1){game.Craft.Materials[2]-=5;game.Craft.Materials[3]--;p.Charge+=p.Def.Tier==5?1200:600;}
   if(op=="cook"&&f=="cook"&&game.Craft.Materials[2]>=4){game.Craft.Materials[2]-=4;game.Life.Food+=2;}
   if(op=="boil"&&f=="cook"&&game.Craft.Materials[0]>=2&&game.Life.RawWater>0){int n=Math.Min(2,game.Life.RawWater);game.Craft.Materials[0]-=2;game.Life.RawWater-=n;game.Craft.Water+=n;}
   if(op=="dressings"&&f=="medical"&&Powered(p)&&game.Craft.Materials[2]>=3){game.Craft.Materials[2]-=3;game.Life.Bandages+=p.Def.Tier;}
   if(op=="load"&&f=="turret"){int n=Math.Min(30,game.Craft.Stock(0));game.Craft.SpendAmmo(0,n);p.Stock[10]+=n;}
   if(op=="arm"&&f=="trap"&&game.Craft.Materials[2]>0){game.Craft.Materials[2]--;p.Charge=1;}
   if(op=="lure"&&f=="lure"&&game.Craft.Materials[5]>0){game.Craft.Materials[5]--;p.Charge=60;}
   if(op=="repair"&&game.Craft.Materials[0]>=2&&p.Health<p.Def.Durability){game.Craft.Materials[0]-=2;p.Health=p.Def.Durability;}
   if(op=="repairvehicle"&&f=="repair"&&game.Craft.Materials[4]>=3&&game.Building.Pieces.Any(x=>x.Def.Function is "vehicle" or "mobile"&&x.Health<x.Def.Durability&&ExpeditionGame.Distance(x.X,x.Z,p.X,p.Z)<12)){game.Craft.Materials[4]-=3;foreach(var car in game.Building.Pieces.Where(x=>x.Def.Function is "vehicle" or "mobile"&&ExpeditionGame.Distance(x.X,x.Z,p.X,p.Z)<12))car.Health=car.Def.Durability;game.AwardResearch("milestone/vehicle-repair",8);}
   if((op is "cargo-store" or "cargo-take")&&(f is "vehicle" or "mobile")){for(int i=0;i<15;i++){int n=op=="cargo-store"?Math.Min(10,Math.Min(Owned(i),200-p.Stock[i])):Math.Min(10,p.Stock[i]);if(n<=0)continue;Add(i,op=="cargo-store"?-n:n);p.Stock[i]+=op=="cargo-store"?n:-n;}}
   if(op=="rest"&&f=="mobile"&&!game.Life.Wounds.Any(w=>!w.Patched)&&!infected.Any(e=>!Peaceful(e.Id)&&Distance(e.Body.Position,player.Position)<30))game.Life.Energy=100;
  });SyncStructures();
 }
 void DamageBaseNear(Infected e){if(e.Cooldown>0||e.Memory<=0)return;var p=game.Building.Pieces.FirstOrDefault(p=>(!p.Open||p.Def.Opening!="")&&p.Def.Function is not ("trap" or "spikes" or "electrictrap")&&(p.Def.Opening!=""?ExpeditionOpenings.SolidAt(p,e.Body.Position.X,e.Body.Position.Y+.8,e.Body.Position.Z,.8):p.Contains(e.Body.Position.X,e.Body.Position.Y+.8,e.Body.Position.Z,.8)));if(p==null)return;e.Cooldown=1.3;p.Health-=5+e.Tier*3;if(p.Health>0)return;game.Building.Pieces.Remove(p);int collapseBudget=game.Building.Pieces.Count+1;while(collapseBudget-->0){var report=game.Building.Evaluate(game.Building.Pieces);if(report.Valid)break;int remove=Array.FindIndex(report.Strength,s=>s<1);if(remove<0)remove=Enumerable.Range(0,report.Loads.Length).FirstOrDefault(i=>report.Loads[i]>game.Building.Pieces[i].Def.Capacity,-1);if(remove<0)break;game.Building.Pieces.RemoveAt(remove);}game.Persist();SyncStructures();game.Notice="A structure was destroyed / unsupported pieces collapsed";}
}
