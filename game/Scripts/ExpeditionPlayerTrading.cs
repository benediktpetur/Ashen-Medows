using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
[DataContract] sealed class PlayerTradeView {
 [DataMember] public string Id,Other,Notice;[DataMember] public int Revision;
 [DataMember] public bool Invited,Incoming,Accepted,OtherAccepted;
 [DataMember] public List<PlayerTradeLine> Inventory=new(),Mine=new(),Theirs=new();
}
public partial class MigrationScene {
 sealed class PlayerTrade {public string Id;public int Revision;public long A,B;public double Until;public bool Invited=true,ReadyA,ReadyB;public List<PlayerTradeLine> GiveA=new(),GiveB=new();}
 readonly List<PlayerTrade> playerTrades=new();PlayerTradeView playerTradeView=new();long playerOptionTarget;
 long TradeSelf=>CoopClient?Multiplayer.GetUniqueId():coopActor>0?coopActor:1;
 (ExpeditionGame Game,CharacterBody3D Body,string Name) TradePerson(long id){if(id==1)return(coopHostGame??game,GetNode<CharacterBody3D>("Survivor"),coopName);if(coopActors.TryGetValue(id,out var actor))return(actor.Game,actor.Body,actor.Name);return(default,default,default);}
 CoopActor NearbyPlayer(){if(!CoopActive||coopExecuting)return null;return coopActors.Values.Where(a=>a.Body!=player&&a.Body.Visible&&a.Body.Position.DistanceTo(player.Position)<=3.5f&&(-camera.GlobalBasis.Z).Dot((a.Body.Position+Vector3.Up-camera.GlobalPosition).Normalized())>.65f).Where(a=>{var ray=PhysicsRayQueryParameters3D.Create(camera.GlobalPosition,a.Body.Position+Vector3.Up);ray.Exclude=new(){player.GetRid()};var hit=GetWorld3D().DirectSpaceState.IntersectRay(ray);return hit.Count>0&&hit["collider"].AsGodotObject()==a.Body;}).OrderBy(a=>a.Body.Position.DistanceSquaredTo(player.Position)).FirstOrDefault();}
 bool PlayerInteract(){var person=NearbyPlayer();if(person==null)return false;playerOptionTarget=person.Id;var panel=Panel(person.Name.ToUpperInvariant()+" / PLAYER OPTIONS");coopMenu="player-options";Button(panel,"TRADE",()=>{PlayerTradeRequest("open|"+person.Id);});Button(panel,"CLOSE",CloseMenu);return true;}
 void PlayerTradeRequest(string action){coopMenu="playertrade";if(!CoopRequest("playertrade",action))PlayerTradeAction(action);}
 bool TradeNear(long a,long b){var x=TradePerson(a);var y=TradePerson(b);if(x.Game==null||y.Game==null||x.Game.Life.Health<=0||y.Game.Life.Health<=0||x.Body.Position.DistanceTo(y.Body.Position)>4)return false;var q=PhysicsRayQueryParameters3D.Create(x.Body.Position+Vector3.Up,y.Body.Position+Vector3.Up);q.Exclude=new(){x.Body.GetRid(),y.Body.GetRid()};return GetWorld3D().DirectSpaceState.IntersectRay(q).Count==0;}
 PlayerTradeView TradeView(long self){var t=playerTrades.FirstOrDefault(t=>t.A==self||t.B==self);if(t==null)return new(){Notice=TradePerson(self).Game?.Notice??"No active trade. Approach another player and press E."};bool first=t.A==self;var own=TradePerson(self);var other=TradePerson(first?t.B:t.A);return new(){Id=t.Id,Revision=t.Revision,Other=other.Name,Invited=t.Invited,Incoming=t.Invited&&t.B==self,Accepted=first?t.ReadyA:t.ReadyB,OtherAccepted=first?t.ReadyB:t.ReadyA,Inventory=ExpeditionTrading.Inventory(own.Game),Mine=first?t.GiveA:t.GiveB,Theirs=first?t.GiveB:t.GiveA,Notice=own.Game.Notice};}
 void SendTradeViews(){if(!CoopHost)return;foreach(var a in coopActors.Values){RpcId(a.Id,nameof(PlayerTradeSnapshot),Json.Write(TradeView(a.Id)));SendCoopPersonal(a.Id,CoopProfileJson(a.Game),a.Game.Notice,a.Body.Position,false);}CallDeferred(nameof(RefreshHostTradeView));}
 void RefreshHostTradeView(){if(!CoopHost)return;playerTradeView=TradeView(1);if(paused&&coopMenu=="playertrade")DrawPlayerTrade();else if(playerTradeView.Incoming)coopHostGame.Notice="Trade invitation / approach the survivor and press E, then Trade";}
 double playerTradeScan;
 void TickPlayerTrades(){if(!CoopHost||TradeTime<playerTradeScan)return;playerTradeScan=TradeTime+.25;if(playerTrades.RemoveAll(t=>t.Until<TradeTime||!TradeNear(t.A,t.B))>0){coopHostGame.Notice="Trade cancelled / expired, out of range, blocked, dead or disconnected";SendTradeViews();}}
 void ClosePlayerTrades(long id){if(playerTrades.RemoveAll(t=>t.A==id||t.B==id)>0)SendTradeViews();}
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]void PlayerTradeSnapshot(string json){if(!CoopClient)return;playerTradeView=Json.Read<PlayerTradeView>(json);if(paused&&coopMenu=="playertrade")DrawPlayerTrade();else if(playerTradeView.Incoming)game.Notice="Trade invitation from "+playerTradeView.Other+" / approach and press E, then Trade";}
 void DrawPlayerTrade(){var b=Panel("TRADE / "+(playerTradeView.Other??"NEARBY SURVIVOR"));coopMenu="playertrade";Text(b,playerTradeView.Notice??"");var id=playerTradeView.Id;
  if(string.IsNullOrEmpty(id)){Button(b,"CLOSE",CloseMenu);return;}
  if(playerTradeView.Invited){Text(b,playerTradeView.Incoming?"Accept this invitation to review offers. Nothing moves until both players confirm.":"Waiting for the other player to accept the invitation.");if(playerTradeView.Incoming)Button(b,"ACCEPT INVITATION",()=>PlayerTradeRequest("join|"+id));}
  else{
   Text(b,"Stay within 4m. Changing either offer clears both confirmations. Equipped gear, fitted weapons and bound boss loot cannot use ordinary trade.");
   Text(b,"YOUR OFFER: "+string.Join(" / ",playerTradeView.Mine.Select(l=>$"{l.Amount} {l.Name}"))+"\nTHEIR OFFER: "+string.Join(" / ",playerTradeView.Theirs.Select(l=>$"{l.Amount} {l.Name}")));
   Text(b,$"You: {(playerTradeView.Accepted?"CONFIRMED":"REVIEWING")} / Them: {(playerTradeView.OtherAccepted?"CONFIRMED":"REVIEWING")}");
   var scroll=new ScrollContainer{CustomMinimumSize=new(880,245)};b.AddChild(scroll);var rows=new VBoxContainer{SizeFlagsHorizontal=Control.SizeFlags.ExpandFill};scroll.AddChild(rows);
   foreach(var entry in playerTradeView.Inventory){string key=entry.Key;var row=new HBoxContainer();rows.AddChild(row);var count=new SpinBox{MinValue=0,MaxValue=Math.Min(100000,entry.Owned),Step=1,Value=playerTradeView.Mine.FirstOrDefault(l=>l.Key==key)?.Amount??0,CustomMinimumSize=new(100,0)};row.AddChild(count);Button(row,entry.Name+" / OWNED "+entry.Owned+" / SET OFFER",()=>PlayerTradeRequest("set|"+id+"|"+key+"|"+(int)count.Value));}
   foreach(var stale in playerTradeView.Mine.Where(l=>!playerTradeView.Inventory.Any(i=>i.Key==l.Key))){string key=stale.Key;Button(b,"REMOVE UNAVAILABLE / "+stale.Name,()=>PlayerTradeRequest("set|"+id+"|"+key+"|0"));}
   Button(b,playerTradeView.Accepted?"WITHDRAW CONFIRMATION":"CONFIRM THESE OFFERS",()=>PlayerTradeRequest((playerTradeView.Accepted?"unready|":"ready|")+id+"|"+playerTradeView.Revision));
  }
  Button(b,"REFRESH",()=>PlayerTradeRequest("view"));Button(b,"CANCEL TRADE",()=>PlayerTradeRequest("cancel|"+id));Button(b,"CLOSE WINDOW",CloseMenu);
 }
 void PlayerTradeAction(string data){if(!CoopHost||data.Length>300)return;long self=TradeSelf;var bits=data.Split('|');bool expired=playerTrades.RemoveAll(t=>t.Until<TradeTime||!TradeNear(t.A,t.B))>0;
  if(bits[0]=="open"&&bits.Length==2&&long.TryParse(bits[1],out long other)&&self!=other&&TradeNear(self,other)){
   var existing=playerTrades.FirstOrDefault(t=>(t.A==self&&t.B==other)||(t.B==self&&t.A==other));
   if(existing==null&&!playerTrades.Any(t=>t.A==self||t.B==self||t.A==other||t.B==other))playerTrades.Add(new(){Id=Guid.NewGuid().ToString("N"),A=self,B=other,Until=TradeTime+120});
  }else if(bits.Length>=2){var t=playerTrades.FirstOrDefault(t=>t.Id==bits[1]&&(t.A==self||t.B==self));if(t!=null){
   if(bits[0]=="cancel")playerTrades.Remove(t);
   else if(bits[0]=="join"&&t.B==self){t.Invited=false;t.Revision++;}
   else if(!t.Invited){var own=TradePerson(self).Game;var partner=TradePerson(t.A==self?t.B:t.A).Game;var list=t.A==self?t.GiveA:t.GiveB;
    if(bits[0]=="set"&&bits.Length==4&&int.TryParse(bits[3],out int amount)&&amount>=0&&amount<=100000){var inventory=ExpeditionTrading.Inventory(own).FirstOrDefault(l=>l.Key==bits[2]);if(amount==0||(inventory!=null&&amount<=inventory.Owned)){list.RemoveAll(l=>l.Key==bits[2]);if(amount>0)list.Add(new(){Key=inventory.Key,Name=inventory.Name,Amount=amount});t.ReadyA=t.ReadyB=false;t.Revision++;}}
    else if(bits[0]=="unready"||(bits[0]=="ready"&&bits.Length==3&&int.TryParse(bits[2],out int revision)&&revision==t.Revision)){if(t.A==self)t.ReadyA=bits[0]=="ready";else t.ReadyB=bits[0]=="ready";}
    if(t.ReadyA&&t.ReadyB){var a=TradePerson(t.A).Game;var b=TradePerson(t.B).Game;if(ExpeditionTrading.Exchange(a,b,t.GiveA,t.GiveB,out string reason)){a.Notice=b.Notice=reason;playerTrades.Remove(t);}else{a.Notice=b.Notice=reason;t.ReadyA=t.ReadyB=false;}}
   }t.Until=TradeTime+120;
  }}
  if(expired)game.Notice="Trade closed: expired, disconnected, out of range or blocked";
  if(coopActor>0)RpcId(coopActor,nameof(PlayerTradeSnapshot),Json.Write(TradeView(self)));else{playerTradeView=TradeView(self);DrawPlayerTrade();}SendTradeViews();
 }
}
