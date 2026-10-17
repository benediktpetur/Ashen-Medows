using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class MigrationScene {
 bool coopCheckHold;int gunplayAck;
 [Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]void CoopGunplayAck(int step){if(coopCheck&&CoopHost&&coopActors.ContainsKey(Multiplayer.GetRemoteSenderId()))gunplayAck=Math.Max(gunplayAck,step);}
 async Task GunplayCoopChecks(CoopActor actor){
  ClearEnemies();Teleport(0,12);player.Position=Ground(6,14);actor.Body.Position=Ground(0,12)+Vector3.Up*.1f;
  actor.Game.Craft.Weapons.AddRange(new[]{0,2,4,17,27});actor.Game.Craft.CommitAmmo();actor.Game.Craft.Weapon=4;actor.Game.Craft.Ammo=0;actor.Game.Craft.Reserve=30;actor.Game.Craft.Magazines[0]=actor.Game.Craft.Magazines[2]=0;actor.Game.Craft.GiveAmmo(0,24-actor.Game.Craft.Stock(0));actor.Game.Stamina=100;actor.Game.Shield=90;actor.Game.Craft.UnlockedTier=5;
  SendCoopPersonal(actor.Id,CoopProfileJson(actor.Game),"",actor.Body.Position,true);RpcId(actor.Id,nameof(CoopCheckStage),"gunplay-breath",actor.Body.Position);
  await CoopWait(()=>gunplayAck>=1&&actor.Game.Handling.HoldingBreath,"Guest breath input was not host-authoritative",15);
  MigrationChecks.Assert(actor.Game.Stamina<100&&!game.Handling.HoldingBreath,"Guest breath drain stays on guest");
  RpcId(actor.Id,nameof(CoopCheckStage),"gunplay-ammo",actor.Body.Position);await CoopWait(()=>gunplayAck>=2,"Typed magazine switch/reload missing",15);
  MigrationChecks.Assert(actor.Game.Craft.Weapon==0&&actor.Game.Craft.Ammo==12&&actor.Game.Craft.Stock(0)==12&&actor.Game.Craft.Stock(1)==24,"Guest ammunition did not conserve class stocks");
  actor.Game.Equip(27);actor.Game.Craft.Fuel=30;actor.Game.Craft.GiveAmmo(4,40-actor.Game.Craft.Stock(4));actor.Game.Stamina=100;actor.Game.Handling.SpecialCooldown=0;actor.Shot=0;
  AddEnemy("gunplay/coop/flame-target",0,9,1);var enemy=infected.Single(e=>e.Id=="gunplay/coop/flame-target");enemy.Hp=enemy.MaxHp=500;enemy.Stagger=30;
  int hostFuel=game.Craft.Fuel,hostCells=game.Craft.Stock(4);SendCoopPersonal(actor.Id,CoopProfileJson(actor.Game),"",actor.Body.Position,true);RpcId(actor.Id,nameof(CoopCheckStage),"gunplay-chain",actor.Body.Position);
  await CoopWait(()=>gunplayAck>=3,"Guest chain sword electric/flame attack missing",20);
  MigrationChecks.Assert(actor.Game.Craft.Fuel==21&&actor.Game.Craft.Stock(4)==35&&game.Craft.Fuel==hostFuel&&game.Craft.Stock(4)==hostCells,"Chain sword authority/cooldown/resource isolation failed");
  ClearEnemies();actor.Aim=actor.HoldBreath=false;GD.Print("PASS / two-process gunplay: host-validated breath/stamina, typed ammunition and saved magazines, electric chain contact, flame/cooldown and personal fuel/cell isolation");
 }
 async Task ClientGunplayChecks(string stage,Vector3 target){
  await CoopWait(()=>Distance(player.Position,target)<1,"Gunplay position missing",15);
  if(stage=="gunplay-breath"){
   await CoopWait(()=>game.Craft.Weapon==4,"Precision rifle missing");aiming=true;coopCheckHold=true;await CoopDelay(.5);await CoopWait(()=>game.Handling.HoldingBreath&&game.Stamina<100,"Breath/stamina not replicated");RpcId(1,nameof(CoopGunplayAck),1);return;
  }
  coopCheckHold=false;aiming=false;
  if(stage=="gunplay-ammo"){
   CoopRequest("equip","0");await CoopWait(()=>game.Craft.Weapon==0,"Pistol switch missing");CoopRequest("reload");await CoopWait(()=>game.Craft.Ammo==12&&game.Craft.Stock(0)==12,"Pistol reload used wrong stock");
   CoopRequest("equip","2");await CoopWait(()=>game.Craft.Weapon==2,"Rifle switch missing");CoopRequest("reload");await CoopWait(()=>game.Craft.Ammo==6&&game.Craft.Stock(1)==24,"Rifle reload used wrong stock");
   CoopRequest("equip","0");await CoopWait(()=>game.Craft.Weapon==0&&game.Craft.Ammo==12,"Loaded pistol magazine was lost");RpcId(1,nameof(CoopGunplayAck),2);return;
  }
  await CoopWait(()=>game.Craft.Weapon==27&&infected.Any(e=>e.Id=="gunplay/coop/flame-target"),"Chain sword/target replica missing");var enemy=infected.Single(e=>e.Id=="gunplay/coop/flame-target");
  camera.LookAt(enemy.Body.Position+Vector3.Up);pitch=camera.GlobalRotation.X;player.Rotation=new(0,camera.GlobalRotation.Y,0);await CoopDelay(.3);CoopRequest("shoot");
  await CoopWait(()=>enemy.Hp<500&&game.Craft.Fuel==29&&game.Craft.Stock(4)==39,"Electric contact not authoritative");double hp=enemy.Hp;CoopRequest("chainflame");
  await CoopWait(()=>enemy.Hp<hp&&game.Craft.Fuel==21&&game.Craft.Stock(4)==35&&game.Handling.SpecialCooldown>0,"Flame attack did not resolve");CoopRequest("chainflame");await CoopDelay(.4);
  if(game.Craft.Fuel!=21||game.Craft.Stock(4)!=35)throw new Exception("Repeated special bypassed cooldown");RpcId(1,nameof(CoopGunplayAck),3);
 }
}
