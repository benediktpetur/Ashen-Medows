using Godot;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

[DataContract] sealed class CoopWorld {
 [DataMember] public int Seed;
 [DataMember] public ExpeditionWorldChanges Changes;
 [DataMember] public List<BuildPiece> Pieces=new();
 [DataMember] public List<string> Gathered=new(),Defeated=new(),Claimed=new();
}
[DataContract] sealed class CoopPersonalState {
 [DataMember] public ExpeditionLootSave Profile;
 [DataMember] public double Stamina,Shield,Reload;
}
[DataContract] sealed class CoopActorState {
 [DataMember] public long Id;
 [DataMember] public string Name;
 [DataMember] public float X,Y,Z,Yaw,Pitch;
}
[DataContract] sealed class CoopEnemyState {
 [DataMember] public string Id;
 [DataMember] public float X,Y,Z,Yaw;
 [DataMember] public int Tier,Night;
 [DataMember] public double Hp,MaxHp,BossTime;[DataMember] public string BossState;[DataMember] public int BossStep;[DataMember] public float AimX,AimY,AimZ;
}
[DataContract] sealed class CoopFrame {
 [DataMember] public string Room="";
 [DataMember] public List<CoopActorState> Players=new();
 [DataMember] public List<CoopEnemyState> Enemies=new();[DataMember] public List<MillPoolState> MillPools=new();
}

public partial class MigrationScene {
 const int CoopProtocol=8,CoopPort=28748;
 ENetMultiplayerPeer coopPeer;bool coopReady,coopApplying,coopExecuting,coopHosting;
 bool CoopActive=>coopPeer!=null;
 bool CoopClient=>CoopActive&&!coopHosting;
 bool CoopHost=>CoopActive&&coopHosting;
 string coopStatus="Offline",coopOfflineSave,coopIdentity,coopName="Survivor";
 double coopFrameClock,coopWorldClock,coopConnectClock;
 int coopSequence;long coopActor;
 readonly Dictionary<long,CoopActor> coopActors=new();
 readonly Dictionary<string,ExpeditionGame> coopCharacters=new();
 readonly Dictionary<long,double> coopPending=new();
 readonly Dictionary<string,Infected> coopGhostEnemies=new();
 CoopWorld coopWorld;ExpeditionGame coopHostGame;string coopMenu="";int coopRecipeChoice;
 sealed class CoopActor {
  public long Id;public string Identity,Name;public CharacterBody3D Body;public Node3D Visual;
  public ExpeditionGame Game;public Vector3 Target;public float Yaw,Pitch;public int Sequence;
  public double Shot,Reload,Chop,Path,LastPose,LastAction,ActionTokens=8;public bool Sprint;
 }
 void InitCoop(){
  Multiplayer.PeerConnected+=id=>{if(CoopHost)coopPending[id]=Time.GetTicksMsec()/1000.0;};
  Multiplayer.PeerDisconnected+=id=>{foreach(string key in vehicleDrivers.Where(p=>p.Value==id).Select(p=>p.Key).ToArray())vehicleDrivers.Remove(key);coopPending.Remove(id);if(coopActors.Remove(id,out var a)){if(a.Game!=null)coopCharacters[a.Identity]=a.Game;RemoveChild(a.Body);a.Body.QueueFree();}coopStatus="Co-op / player disconnected";};
  Multiplayer.ConnectedToServer+=()=>{RpcId(1,nameof(CoopHello),CoopProtocol,coopIdentity,coopName,Json.Write(game.Profile));};
  Multiplayer.ConnectionFailed+=()=>StopCoop("Could not connect. Check the address and UDP port.");
  Multiplayer.ServerDisconnected+=()=>StopCoop("Host disconnected. Character saved; returned to your solo world.");
  string identityPath=ProjectSettings.GlobalizePath(coopCheckRole!=null?"user://tests/coop-"+coopCheckRole+"-id.txt":"user://expedition/coop-id.txt");Directory.CreateDirectory(Path.GetDirectoryName(identityPath));
  coopIdentity=File.Exists(identityPath)?File.ReadAllText(identityPath).Trim():Guid.NewGuid().ToString("N");
  if(!Guid.TryParse(coopIdentity,out _))coopIdentity=Guid.NewGuid().ToString("N");File.WriteAllText(identityPath,coopIdentity);
 }
 void ShowMultiplayer(){var b=Panel("EXPEDITION / GODOT CO-OP");Text(b,coopStatus+" / "+(CoopActive?(coopActors.Count+1)+" players":"single-player"));
  Text(b,"Up to 8 players. Host owns the world; everyone brings their own character. Menus do not pause online play. Explore the surface independently. Regroup within 24m for party dungeon trips.");
  if(CoopActive){Button(b,"LEAVE SESSION / RETURN TO SOLO",()=>{StopCoop("Session closed / returned to solo");CloseMenu();});}
  else {var name=new LineEdit{Text=coopName,PlaceholderText="Survivor name",MaxLength=24};b.AddChild(name);var address=new LineEdit{Text="127.0.0.1",PlaceholderText="Host IP address or DNS name"};b.AddChild(address);var port=new SpinBox{MinValue=1024,MaxValue=65535,Value=CoopPort};b.AddChild(port);
   Button(b,"HOST CURRENT WORLD",()=>{coopName=CleanName(name.Text);StartCoop(true,"",(int)port.Value);CloseMenu();});
   Button(b,"JOIN HOST",()=>{coopName=CleanName(name.Text);StartCoop(false,address.Text.Trim(),(int)port.Value);CloseMenu();});
   Text(b,"LAN: use the host's local IP. Internet: forward UDP "+CoopPort+" to the host and allow the game through Windows Firewall. No changes to the old relay server are required. Both players need the same game build.");}
  Button(b,"BACK",CloseMenu);
 }
 static string CleanName(string text)=>new string((text??"Survivor").Where(c=>char.IsLetterOrDigit(c)||c==' '||c=='_'||c=='-').Take(24).ToArray()) is string s&&s.Length>0?s:"Survivor";
 void StartCoop(bool host,string address,int port){
  if(CoopActive)return;if(!game.Healthy){game.Notice="Repair the protected character save before joining co-op.";return;}
  ExitDungeon();game.Persist();coopPeer=new ENetMultiplayerPeer();Error error=host?coopPeer.CreateServer(port,7):coopPeer.CreateClient(address,port);
  if(error!=Error.Ok){coopPeer.Dispose();coopPeer=null;game.Notice="Networking failed / "+error;GD.Print(game.Notice);return;}
  coopHosting=host;Multiplayer.MultiplayerPeer=coopPeer;coopSequence=0;coopWireSequence=0;coopReceivedSequence=-1;coopFrameParts=null;coopConnectClock=0;coopReady=host;
  if(host){coopHostGame=game;coopWorld=WorldSnapshot();coopStatus="HOST / UDP "+port;}
  else {coopOfflineSave=Json.Write(game.Save);game.GuestOriginal=Json.Read<ExpeditionGameSave>(coopOfflineSave);coopStatus="Connecting to "+address+":"+port;}
  game.Notice=coopStatus;GD.Print("COOP / "+coopStatus);
 }
 CoopWorld WorldSnapshot()=>new CoopWorld{Seed=game.Save.Seed,Changes=game.World.Changes,Pieces=game.Building.Pieces,Gathered=game.Craft.Gathered.ToList(),Defeated=game.Skills.Defeated.ToList(),Claimed=game.Profile.Claimed.ToList()};
 void BindWorld(ExpeditionGame target){target.Save.Seed=game.Save.Seed;target.World=game.World;target.Building=new ExpeditionBuilding(game.World.Height);target.Building.Pieces=game.Building.Pieces;target.Craft.Gathered=game.Craft.Gathered;target.Skills.Defeated=game.Skills.Defeated;target.Profile.Claimed=game.Profile.Claimed;}
 [Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
 void CoopHello(int protocol,string identity,string name,string profile){
  if(!CoopHost)return;long id=Multiplayer.GetRemoteSenderId();
  if(protocol!=CoopProtocol||profile.Length>524288||!Guid.TryParse(identity,out _)||coopActors.ContainsKey(id)||coopActors.Values.Any(a=>a.Identity==identity)){RejectCoop(id,"Incompatible build or duplicate character.");return;}
  try{if(!coopCharacters.TryGetValue(identity,out var character)){character=new ExpeditionGame(ProjectSettings.GlobalizePath("user://tests/coop-transient/"+identity+".json"));character.NoDiskSave=true;character.Save.Profile=Json.Read<ExpeditionLootSave>(profile);character.Normalize();character.Save.X=player.Position.X+3;character.Save.Z=player.Position.Z;}
   BindWorld(character);var actor=MakeCoopActor(id,CleanName(name));actor.Identity=identity;actor.Game=character;actor.Body.Position=player.Position+new Vector3(3+coopActors.Count,0,0);actor.Target=actor.Body.Position;actor.LastPose=Time.GetTicksMsec()/1000.0;coopActors[id]=actor;coopPending.Remove(id);
   RpcId(id,nameof(CoopWelcome),Json.Write(WorldSnapshot()),CoopProfileJson(character),actor.Body.Position, dungeon?.Id??"");coopStatus="HOST / "+(coopActors.Count+1)+" players";GD.Print("COOP / joined "+id);
  }catch(Exception ex){GD.Print("COOP / rejected profile: "+ex.Message);RejectCoop(id,"Character could not be loaded.");}
 }
 void RejectCoop(long id,string reason){RpcId(id,nameof(CoopRejected),reason);coopPending[id]=-100;}
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
 void CoopRejected(string reason){StopCoop(reason);}
 CoopActor MakeCoopActor(long id,string name){var body=new CharacterBody3D{Name="Peer_"+id};body.AddChild(new CollisionShape3D{Shape=new CapsuleShape3D{Radius=.3f,Height=1.75f},Position=new Vector3(0,.88f,0)});var visual=new Node3D();body.AddChild(visual);ArtCharacter(visual,1,0,true);body.AddChild(new Label3D{Text=name,Position=new Vector3(0,2.1f,0),FontSize=26,Billboard=BaseMaterial3D.BillboardModeEnum.Enabled,Modulate=new Color(.8f,.9f,.65f)});AddChild(body);return new CoopActor{Id=id,Name=name,Body=body,Visual=visual};}
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
 void CoopWelcome(string world,string profile,Vector3 spawn,string room){if(!CoopClient)return;ApplyCoopWorld(world,true);ApplyCoopProfile(profile);coopReady=true;player.Position=spawn;player.Velocity=Vector3.Zero;if(room!="")ApplyCoopRoom(room);coopStatus="CONNECTED / GODOT CO-OP";game.Notice=coopStatus;GD.Print("COOP / "+coopStatus);GD.Print("COOP / welcome");}
 void ApplyCoopProfile(string profile){var state=Json.Read<CoopPersonalState>(profile);var incoming=state.Profile;if(incoming.Life.Health<game.Life.Health)game.LastDamage=game.Clock;game.Stamina=state.Stamina;game.Shield=state.Shield;reloadClock=state.Reload;incoming.Worlds=game.Profile.Worlds;game.Save.Profile=incoming;game.Normalize();if(coopWorld!=null){game.World.Changes=coopWorld.Changes;game.Building.Pieces=coopWorld.Pieces;}BuildGun();game.Persist();}
 void ApplyCoopWorld(string json,bool initial=false){var w=Json.Read<CoopWorld>(json);if(w==null||w.Changes==null)return;bool terrainChanged=initial||coopWorld==null||Json.Write(w.Changes.Paths)!=Json.Write(coopWorld.Changes.Paths)||Json.Write(w.Changes.Wells)!=Json.Write(coopWorld.Changes.Wells);bool structures=initial||coopWorld==null||Json.Write(w.Pieces)!=Json.Write(coopWorld.Pieces);coopWorld=w;game.Save.Seed=w.Seed;game.Normalize();game.World.Changes=w.Changes;w.Changes.Index();game.Building.Pieces=w.Pieces;game.Craft.Gathered=w.Gathered;game.Skills.Defeated=w.Defeated;game.Profile.Claimed=w.Claimed;
  foreach(var p in props.ToArray())if((p.Kind==0&&w.Changes.Cut(p.Id))||(p.Kind==1&&w.Gathered.Contains(p.Id))){p.Node.GetParent().RemoveChild(p.Node);p.Node.QueueFree();props.Remove(p);uniqueObjects.Remove(p.Id);}else if(p.Kind==2&&w.Changes.Burned(p.Id)&&p.Hp>0){p.Hp=0;foreach(var child in p.Node.GetChildren())if(child is MeshInstance3D m)m.MaterialOverride=Material(new Color(.09f,.10f,.09f));}
  if(terrainChanged){ClearChildren(worldRoot);props.RemoveAll(p=>p.Kind!=0&&p.Kind!=7);uniqueObjects.Clear();RebuildTerrain();SpawnObjects();}if(structures)SyncStructures();SyncCampObjects();
 }
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
 void CoopWorldUpdate(string json){if(CoopClient&&coopReady)ApplyCoopWorld(json);}
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
 void CoopPersonal(string json,string message,Vector3 correction,bool teleport){if(!CoopClient||!coopReady)return;ApplyCoopProfile(json);if(message!=""){game.Notice=message;if(paused){if(coopMenu=="talents")ShowTalentTree();else if(coopMenu=="research")ShowResearchTree();else if(coopMenu=="craft")ResearchWorkshop();else if(coopMenu=="equipment")ComicEquipment();else if(coopMenu=="camps")ShowCamps();else if(coopMenu=="wounds")ShowWounds();else if(coopMenu=="base"){var station=game.Building.Pieces.FirstOrDefault(p=>p.Id==basePanelId);if(station!=null)ShowBaseStation(station);}}}if(teleport){player.Position=correction;player.Velocity=Vector3.Zero;}}
 [Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode=MultiplayerPeer.TransferModeEnum.UnreliableOrdered,TransferChannel=1)]
 void CoopPose(Vector3 position,float yaw,float look,bool sprint){if(!CoopHost||!coopActors.TryGetValue(Multiplayer.GetRemoteSenderId(),out var a)||!Finite(position)||!float.IsFinite(yaw+look))return;double now=Time.GetTicksMsec()/1000.0,elapsed=Math.Clamp(now-a.LastPose,.02,.5);if(position.DistanceTo(a.Body.Position)>Math.Max(3,elapsed*13)||dungeon==null&&!game.World.Playable(position.X,position.Z)||dungeon!=null&&!ExpeditionDungeon.Free(position.X,position.Z)){RpcId(a.Id,nameof(CoopPersonal),CoopProfileJson(a.Game),"Movement corrected / invalid position or excessive speed",a.Body.Position,true);return;}a.Body.Position=position;a.Target=position;a.Yaw=yaw;a.Pitch=Math.Clamp(look,-1.4f,1.4f);a.Body.Rotation=new Vector3(0,yaw,0);a.LastPose=now;a.Sprint=sprint;}
 static bool Finite(Vector3 v)=>float.IsFinite(v.X)&&float.IsFinite(v.Y)&&float.IsFinite(v.Z);
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.UnreliableOrdered,TransferChannel=1)]
 void CoopFrameUpdate(string json){if(!CoopClient||!coopReady)return;var frame=Json.Read<CoopFrame>(json);if(frame.Room!=(dungeon?.Id??""))return;ApplyMillPools(frame.MillPools);var ids=new HashSet<long>();foreach(var p in frame.Players){if(p.Id==Multiplayer.GetUniqueId())continue;ids.Add(p.Id);if(!coopActors.TryGetValue(p.Id,out var a))coopActors[p.Id]=a=MakeCoopActor(p.Id,p.Name);a.Target=new Vector3(p.X,p.Y,p.Z);a.Yaw=p.Yaw;a.Pitch=p.Pitch;}
  foreach(var id in coopActors.Keys.ToArray())if(!ids.Contains(id)){var a=coopActors[id];RemoveChild(a.Body);a.Body.QueueFree();coopActors.Remove(id);}var enemies=new HashSet<string>();foreach(var e in frame.Enemies){enemies.Add(e.Id);if(!coopGhostEnemies.TryGetValue(e.Id,out var local)){AddEnemy(e.Id,e.X,e.Z,e.Tier,e.Night);local=infected.Find(x=>x.Id==e.Id);if(local==null)continue;coopGhostEnemies[e.Id]=local;}local.Body.Position=new Vector3(e.X,e.Y,e.Z);local.Body.Rotation=new Vector3(0,e.Yaw,0);if(e.Hp<local.Hp)BloodImpact(local.Body.Position+Vector3.Up,Vector3.Up,8);local.Hp=e.Hp;local.MaxHp=e.MaxHp>0?e.MaxHp:local.MaxHp;local.BossState=e.BossState??"";local.BossTime=e.BossTime;local.BossStep=e.BossStep;local.BossAim=new(e.AimX,e.AimY,e.AimZ);local.Memory=2;AnimateCharacter(local);}foreach(var key in coopGhostEnemies.Keys.ToArray())if(!enemies.Contains(key)){var e=coopGhostEnemies[key];if(infected.Contains(e)){if(game.Skills.Defeated.Contains(e.Id)){BloodImpact(e.Body.Position+Vector3.Up,Vector3.Up,20);DeathRemains(e);}RemoveEnemy(e);}coopGhostEnemies.Remove(key);}
 }
 bool CoopRequest(string operation,string data=""){if(!CoopClient||coopExecuting)return false;if(!coopReady){game.Notice="Waiting for host...";return true;}RpcId(1,nameof(CoopAction),++coopSequence,operation,data);return true;}
 [Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
 void CoopAction(int sequence,string operation,string data){if(!CoopHost||!coopActors.TryGetValue(Multiplayer.GetRemoteSenderId(),out var a)||sequence<=a.Sequence||data.Length>4096)return;a.Sequence=sequence;double now=Time.GetTicksMsec()/1000.0;a.ActionTokens=Math.Min(8,a.ActionTokens+(now-a.LastAction)*20);a.LastAction=now;if(a.ActionTokens<1)return;a.ActionTokens--;Vector3 before=a.Body.Position;try{WithCoopActor(a,()=>{if(dungeon==null)SpawnObjects();ExecuteCoopAction(operation,data);});}catch(Exception ex){a.Game.Notice="Action rejected / "+ex.Message;}game.Persist();RpcId(a.Id,nameof(CoopPersonal),CoopProfileJson(a.Game),a.Game.Notice,a.Body.Position,a.Body.Position.DistanceTo(before)>.5f);}
 void WithCoopActor(CoopActor a,Action action){var ownGame=game;var ownPlayer=player;var transform=camera.GlobalTransform;var ownPitch=pitch;double ownShot=shotClock,ownReload=reloadClock,ownChop=nextChop,ownPath=nextPath;long oldActor=coopActor;
  try{BindWorld(a.Game);game=a.Game;player=a.Body;camera.GlobalPosition=a.Body.Position+new Vector3(0,1.65f,0);camera.GlobalRotation=new Vector3(a.Pitch,a.Yaw,0);pitch=a.Pitch;shotClock=a.Shot;reloadClock=a.Reload;nextChop=a.Chop;nextPath=a.Path;coopActor=a.Id;coopExecuting=true;action();a.Shot=shotClock;a.Reload=reloadClock;a.Chop=nextChop;a.Path=nextPath;}
  finally{game=ownGame;player=ownPlayer;camera.GlobalTransform=transform;pitch=ownPitch;shotClock=ownShot;reloadClock=ownReload;nextChop=ownChop;nextPath=ownPath;coopActor=oldActor;coopExecuting=false;}
 }
 void ExecuteCoopAction(string operation,string data){switch(operation){
  case "shoot":Shoot();break;case "reload":StartReload();break;case "chop":ChopTree();break;case "path":PaintPath();break;case "well":DigWell();break;
  case "interact":CoopInteract();break;
  case "millnote":if(int.TryParse(data,out int note))ReadMillNote(note);break;case "millbell":StartMillFight();break;
  case "talent":game.LearnTalent(data);break;case "talentreset":game.ResetTalents();break;
  case "research":if(int.TryParse(data,out int research))game.Research(research);break;
  case "drive":DriveBlueprint(data);break;
  case "dismount":if(vehicleDrivers.TryGetValue(data,out long driver)&&driver==coopActor)vehicleDrivers.Remove(data);break;
  case "baseuse":UseBlueprint(data);break;
  case "researchgoal":if(int.TryParse(data,out int goal))game.TrackResearch(goal);break;
  case "craft":if(int.TryParse(data,out int r)&&r>=0&&r<ExpeditionRecipe.All.Length)game.CraftRecipe(ExpeditionRecipe.All[r]);break;
  case "equip":if(int.TryParse(data,out int gun)&&gun>=0&&gun<ExpeditionGame.GunNames.Length)game.Change(()=>game.Equip(gun));break;
  case "armor":if(game.Profile.Items.Any(i=>i.Id==data))game.Change(()=>game.EquipGear(data));break;
  case "unpatch":if(int.TryParse(data,out int woundIndex))game.Change(()=>game.Life.Unpatch(woundIndex));break;
  case "unequip":game.Change(()=>game.UnequipGear(data));break;
  case "medicine":game.Change(()=>{if(game.Life.Medicine>0){game.Life.Medicine--;game.Life.RecoveryBoost=300;}});break;
  case "bandage":game.Change(()=>game.Notice=game.Life.Patch()?"Bandaged one wound":"No open wound or no bandages");break;
  case "eat":game.Change(()=>{if(game.Life.Food>0){game.Life.Food--;game.Life.Hunger=Math.Min(100,game.Life.Hunger+40);}});break;
  case "drink":game.Change(()=>{if(game.Craft.Water>0){game.Craft.Water--;game.Life.Hydration=Math.Min(100,game.Life.Hydration+45);}});break;
  case "place":CoopPlace(Json.Read<BuildPiece>(data));break;
  case "remove":RemoveStructure();break;
  case "bag":game.AddBag(player.Position.X,player.Position.Z,CampSafe);SyncCampObjects();break;
  case "bind":if(game.Life.Camps.Any(c=>c.Id==data&&c.Seed==game.Save.Seed))game.Change(()=>game.Life.Respawn=data);break;
  case "rest":var camp=game.Life.Camps.Find(c=>c.Seed==game.Save.Seed&&ExpeditionGame.Distance(c.X,c.Z,player.Position.X,player.Position.Z)<4);if(camp!=null&&CampSafe(camp)&&!game.Life.Wounds.Exists(w=>!w.Patched))game.Change(()=>game.Life.Energy=100);break;
  case "travel":game.Notice="Fast travel is disabled during co-op; explore together.";break;
 }}
 void CoopPlace(BuildPiece p){if(p==null||p.Kind<0||p.Kind>=BuildDefinition.All.Length||!double.IsFinite(p.X+p.Y+p.Z+p.Yaw)||ExpeditionGame.Distance(p.X,p.Z,player.Position.X,player.Position.Z)>12||Math.Abs(p.Y-player.Position.Y)>12||dungeon!=null||InMill(p.X,p.Z,32)||!game.World.Playable(p.X,p.Z))return;var valid=game.Building.Placement(p);if(!valid.Valid||StaticPlacementBlocked(p)){game.Notice=valid.Reason;return;}if(game.PlaceBlueprint(p))SyncStructures();}
 void CoopInteract(){if(dungeon==null){var portal=ExpeditionDungeon.Nearby(game.World,0,0).Find(d=>Distance(player.Position,Ground(d.X,d.Z))<3.5);if(portal!=null){game.Notice="Ask the host to enter the dungeon; the party enters together.";return;}}
  if(dungeon!=null&&(Distance(player.Position,new Vector3(0,0,3))<2||dungeon.Tier==4&&!dungeon.Deep&&Distance(player.Position,new Vector3(0,0,-26))<2.2)){game.Notice="The host controls party dungeon travel.";return;}Interact();if(paused)CloseMenu();}
 string CoopProfileJson(ExpeditionGame character){var profile=Json.Read<ExpeditionLootSave>(Json.Write(character.Profile));profile.Worlds=new List<ExpeditionWorldChanges>();var actor=coopActors.Values.FirstOrDefault(a=>ReferenceEquals(a.Game,character));return Json.Write(new CoopPersonalState{Profile=profile,Stamina=character.Stamina,Shield=character.Shield,Reload=ReferenceEquals(character,game)?reloadClock:actor?.Reload??0});}void CoopTick(double dt){if(!CoopActive)return;coopConnectClock+=dt;if(CoopClient&&!coopReady){if(coopConnectClock>15)StopCoop("Connection timed out. Check the host address and UDP port.");return;}
  if(CoopClient){foreach(var a in coopActors.Values){float speed=a.Body.Position.DistanceTo(a.Target);a.Body.Position=a.Body.Position.Lerp(a.Target,(float)Math.Min(1,dt*14));a.Body.Visible=dungeon!=null||Distance(a.Body.Position,player.Position)<220;a.Body.Rotation=new Vector3(0,Mathf.LerpAngle(a.Body.Rotation.Y,a.Yaw,(float)Math.Min(1,dt*14)),0);float swing=speed>.04f?(float)Math.Sin(game.Clock*7)*.25f:0;PoseFigure(a.Visual,(float)game.Clock*7,speed>.04f?1:0,false);}
   coopFrameClock+=dt;if(coopFrameClock>=.1){coopFrameClock=0;RpcId(1,nameof(CoopPose),player.Position,player.Rotation.Y,pitch,!paused&&Input.IsPhysicalKeyPressed(Key.Shift)&&player.Velocity.Length()>5);}return;}
  double now=Time.GetTicksMsec()/1000.0;foreach(var id in coopPending.Keys.ToArray())if(now-coopPending[id]>10){coopPeer.DisconnectPeer((int)id);coopPending.Remove(id);}
  foreach(var a in coopActors.Values.ToArray()){WithCoopActor(a,()=>{shotClock=Math.Max(0,shotClock-dt);double time=game.World.Changes.Time;if(game.Tick(dt,a.Sprint))CoopDie();if(a.Sprint)Noise(game.Talent("quiet")>0?8.4:14);game.World.Changes.Time=time;if(reloadClock>0){reloadClock-=dt;if(reloadClock<=0)game.Reload();}game.Save.X=player.Position.X;game.Save.Z=player.Position.Z;});}
  coopFrameClock+=dt;coopWorldClock+=dt;if(coopFrameClock>=.1){coopFrameClock=0;foreach(var a in coopActors.Values)SendCoopFrame(a.Id,FrameFor(a.Body.Position));}
  if(coopWorldClock>=1){coopWorldClock=0;if(coopActors.Count>0)Rpc(nameof(CoopWorldUpdate),Json.Write(WorldSnapshot()));foreach(var a in coopActors.Values){WithCoopActor(a,SpawnObjects);RpcId(a.Id,nameof(CoopPersonal),CoopProfileJson(a.Game),"",a.Body.Position,false);}}
 }
 static CoopActorState ActorState(long id,string name,CharacterBody3D body,float look)=>new(){Id=id,Name=name,X=body.Position.X,Y=body.Position.Y,Z=body.Position.Z,Yaw=body.Rotation.Y,Pitch=look};
 void CoopDie(){game.Die(CampSafe);if(dungeon!=null)player.Position=new Vector3(0,.2f,2);else{var point=Ground(game.Save.X,game.Save.Z)+Vector3.Up*.3f;player.Position=point;}player.Velocity=Vector3.Zero;reloadClock=0;if(coopActor>0)RpcId(coopActor,nameof(CoopPersonal),CoopProfileJson(game),game.Notice,player.Position,true);}
 void TickCoopEnemies(double dt){foreach(var enemy in infected.ToArray()){if(IsMillBoss(enemy.Id)){RefreshCoopEnemyVisual(enemy);TickMillBoss(enemy,dt);continue;}RefreshCoopEnemyVisual(enemy);
  CoopActor nearest=null;float distance=Distance(enemy.Body.Position,player.Position);
  foreach(var a in coopActors.Values){float d=Distance(enemy.Body.Position,a.Body.Position);if(d<distance){distance=d;nearest=a;}}
  if(dungeon==null&&!ChunkLoaded(enemy.Body.Position))continue;var all=infected;infected=new List<Infected>{enemy};try{if(nearest==null)TickSoloEnemies(dt);else WithCoopActor(nearest,()=>TickSoloEnemies(dt));}finally{infected=all;}
 }}
 void CoopBlast(Vector3 point){if(!CoopHost)return;
  foreach(var a in coopActors.Values.ToArray())if(a.Id!=coopActor){float d=Distance(a.Body.Position,point);if(d<9)WithCoopActor(a,()=>{game.Life.Health-=100*(1-d/9);game.LastDamage=game.Clock;if(game.Life.Health<=0)CoopDie();});}
  if(coopActor>0&&coopHostGame!=null){var host=GetNode<CharacterBody3D>("Survivor");float d=Distance(host.Position,point);if(d<9){coopHostGame.Life.Health-=100*(1-d/9);coopHostGame.LastDamage=coopHostGame.Clock;if(coopHostGame.Life.Health<=0){coopHostGame.Die(CampSafe);host.Position=dungeon==null?Ground(coopHostGame.Save.X,coopHostGame.Save.Z)+Vector3.Up*.3f:new Vector3(0,.2f,2);host.Velocity=Vector3.Zero;}}}
 }
 void CoopRoomChanged(){if(!CoopHost)return;string room=dungeon?.Id??"";foreach(var a in coopActors.Values){a.Body.Position=dungeon==null?player.Position+new Vector3(3,0,0):new Vector3(2,.2f,2);a.Target=a.Body.Position;}if(coopActors.Count>0)Rpc(nameof(CoopRoom),room,player.Position);}
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
 void CoopRoom(string room,Vector3 hostPosition){if(!CoopClient||!coopReady)return;ApplyCoopRoom(room);player.Position=room==""?hostPosition+new Vector3(3,0,0):new Vector3(2,.2f,2);player.Velocity=Vector3.Zero;}
 void ApplyCoopRoom(string room){coopApplying=true;if(room!="")coopCheckSawDungeon=true;try{if(room==""){ExitDungeon();return;}bool deep=room.EndsWith("/deep");string id=deep?room.Substring(0,room.Length-5):room;var d=ExpeditionDungeon.Nearby(game.World,0,0).Find(x=>x.Id==id);if(d==null)return;if(deep)d=new ExpeditionDungeon{Id=room,X=d.X,Z=d.Z,Tier=5,Deep=true,Name="SEALED LOWER WARD"};if(dungeon!=null)ExitDungeon(false);EnterDungeon(d);ClearEnemies();coopGhostEnemies.Clear();}finally{coopApplying=false;}}
 void StopCoop(string message){mountedBlueprint=null;vehicleDrivers.Clear();if(!CoopActive)return;bool guest=CoopClient;coopReady=false;coopPeer.Close();Multiplayer.MultiplayerPeer=new OfflineMultiplayerPeer();coopPeer.Dispose();coopPeer=null;foreach(var a in coopActors.Values){RemoveChild(a.Body);a.Body.QueueFree();}coopActors.Clear();coopCharacters.Clear();coopPending.Clear();coopGhostEnemies.Clear();coopApplying=true;ExitDungeon();coopApplying=false;ClearEnemies();
  if(guest&&coopOfflineSave!=null){var original=Json.Read<ExpeditionGameSave>(coopOfflineSave);var profile=game.Profile;profile.Worlds=original.Profile.Worlds;profile.Crafting.Gathered=original.Profile.Crafting.Gathered;profile.Survivor.Defeated=original.Profile.Survivor.Defeated;profile.Claimed=original.Profile.Claimed;original.Profile=profile;game.Save=original;game.GuestOriginal=null;game.Normalize();coopOfflineSave=null;}
  coopNightTimes.Clear();coopWorld=null;ClearChildren(worldRoot);props.Clear();uniqueObjects.Clear();RebuildTerrain();SyncStructures();Teleport(game.Save.X,game.Save.Z);game.Persist();coopStatus=message;game.Notice=message;
 }
}
