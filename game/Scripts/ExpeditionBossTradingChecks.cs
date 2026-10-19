using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
public partial class MigrationScene {
 int bossTradeAck;
 [Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]void CoopBossTradeAck(int step){if(coopCheck&&CoopHost&&coopActors.ContainsKey(Multiplayer.GetRemoteSenderId()))bossTradeAck=Math.Max(bossTradeAck,step);}
 void BossTradingLogicChecks(){
  ExpeditionGame Fresh()=>new ExpeditionGame(System.IO.Path.Combine(System.IO.Path.GetTempPath(),"boss-trade-"+Guid.NewGuid()+".json")){NoDiskSave=true};var a=Fresh();var b=Fresh();var outsider=Fresh();
  for(int variant=0;variant<3;variant++)a.ClaimMegaLoot("trade-test/"+variant,variant,"Trade test",variant+10);
  a.Life.SoulCompanions.Add(new(){Id=Guid.NewGuid().ToString("N"),Owner=a.Life.SoulOwner,Damage=25,Speed=15});a.BindBossRewards(new(),"unique-kill",new[]{a.Life.SoulOwner,b.Life.SoulOwner});
  var items=a.BossTradeItems().ToArray();MigrationChecks.Assert(items.Length>=5,"All five unique boss item families present");foreach(var item in items){string id=item.Id,name=item.Name;MigrationChecks.Assert(!a.TransferBossItem(id,outsider),"Outsider rejected for every item family");MigrationChecks.Assert(a.TransferBossItem(id,b),"Eligible transfer / "+name);MigrationChecks.Assert(!a.TransferBossItem(id,b),"Repeated transfer rejected");var copy=b.BossTradeItems().Single(i=>i.Id==id);MigrationChecks.Assert(copy.Name==name&&copy.Bond.Kill=="unique-kill"&&copy.Bond.Eligible.Count==2,"Item stats and fixed group retained");MigrationChecks.Assert(!b.TransferBossItem(id,outsider)&&b.TransferBossItem(id,a),"No laundering through eligible recipient");}
  var reload=Json.Read<ExpeditionGameSave>(Json.Write(a.Save));a.Save=reload;a.Normalize();MigrationChecks.Assert(a.BossTradeItems().Count==items.Length&&a.BossTradeItems().All(i=>i.Bond.Allows(b.Life.SoulOwner)),"All five item families keep eligibility after profile reload");GD.Print("PASS / boss trade logic: weapon, armor, upgrade, module and harness transfers, outsider rejection, duplicate rejection, fixed group, rolls and save reload");
 }
 async Task BossTradingCoopChecks(CoopActor actor){
  player.Position=new(0,.1f,-18);actor.Body.Position=player.Position+Vector3.Right*2;
  var item=game.Life.Souls[0];MigrationChecks.Assert(item.Bond!=null&&item.Bond.Allows(actor.Game.Life.SoulOwner),"Boss records kill-group eligibility");string id=item.Id,stats=item.Stats,kill=item.Bond.Kill;int hostCount=game.Life.Souls.Count,guestCount=actor.Game.Life.Souls.Count;
  var outsider=new ExpeditionGame(System.IO.Path.Combine(System.IO.Path.GetTempPath(),"trade-outsider-"+Guid.NewGuid()+".json")){NoDiskSave=true};MigrationChecks.Assert(game.BossTradeReason(id,outsider).Contains("original"),"Outsiders cannot receive boss items");
  game.Life.ActiveSoul=id;MigrationChecks.Assert(game.BossTradeReason(id,actor.Game).Contains("Unequip"),"Equipped boss item cannot be given");game.Life.ActiveSoul=null;
  var saved=Json.Read<SoulboundWeapon>(Json.Write(item));MigrationChecks.Assert(saved.Bond.Kill==kill&&saved.Bond.Allows(actor.Game.Life.SoulOwner)&&saved.Stats==stats,"Eligibility and rolls survive saves");
  BossTradeAction("offer|"+id+"|"+actor.Game.Life.SoulOwner);MigrationChecks.Assert(game.Life.Souls.Count==hostCount,"Offer does not transfer before consent");
  SendCoopPersonal(actor.Id,CoopProfileJson(actor.Game),"Trade test",actor.Body.Position,true);RpcId(actor.Id,nameof(CoopCheckStage),"campaign-trade",actor.Body.Position);
  await CoopWait(()=>bossTradeAck>=1,"Guest did not accept boss item",20);
  MigrationChecks.Assert(!game.Life.Souls.Any(i=>i.Id==id)&&actor.Game.Life.Souls.Count==guestCount+1,"Accepted trade moves exactly one item");var received=actor.Game.Life.Souls.Single(i=>i.Id==id);MigrationChecks.Assert(received.Stats==stats&&received.Owner==actor.Game.Life.SoulOwner&&received.Bond.Kill==kill&&received.Bond.Eligible.Count==2,"Trade preserves rolls and original two-person group");
  await CoopWait(()=>bossOffers.Any(o=>o.From==actor.Game.Life.SoulOwner&&o.Item==id),"Return offer missing",15);var offer=bossOffers.Single(o=>o.Item==id);BossTradeAction("accept|"+offer.Id);BossTradeAction("accept|"+offer.Id);
  MigrationChecks.Assert(game.Life.Souls.Count==hostCount&&actor.Game.Life.Souls.Count==guestCount&&game.Life.Souls.Single(i=>i.Id==id).Owner==game.Life.SoulOwner,"Return trade and duplicate accept conserve ownership");await CoopWait(()=>bossTradeAck>=2,"Guest return transfer not replicated",15);
  GD.Print("PASS / two-process boss trades: saved kill-group eligibility, outsider/equipped rejection, consent, host-to-guest and return, exact rolls, owner rebinding and duplicate acceptance protection");
 }
 async Task ClientBossTradingChecks(){
  CoopRequest("bosstrade","view");await CoopWait(()=>bossTradeView.Rows.Any(r=>r.Action?.StartsWith("accept|")==true),"Incoming boss offer missing");var row=bossTradeView.Rows.First(r=>r.Action?.StartsWith("accept|")==true);int count=game.Life.Souls.Count;CoopRequest("bosstrade",row.Action);await CoopWait(()=>game.Life.Souls.Count==count+1,"Accepted boss item missing");RpcId(1,nameof(CoopBossTradeAck),1);
  await CoopWait(()=>bossTradeView.Rows.Any(r=>r.Action?.StartsWith("offer|")==true),"Return trade option missing");var give=bossTradeView.Rows.First(r=>r.Action?.StartsWith("offer|")==true);CoopRequest("bosstrade",give.Action);await CoopWait(()=>game.Life.Souls.Count==count,"Return trade missing");RpcId(1,nameof(CoopBossTradeAck),2);
 }
}
