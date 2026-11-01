using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
public partial class MigrationScene {
 bool interactionTradeAck,interactionOwnerAck;
 [Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]void InteractionAck(string step){if(!coopCheck||!CoopHost||!coopActors.ContainsKey(Multiplayer.GetRemoteSenderId()))return;if(step=="trade")interactionTradeAck=true;if(step=="owner")interactionOwnerAck=true;}
 async Task InteractionCoopChecks(CoopActor a){
  int ownWood=game.Craft.Materials[0],ownFood=game.Life.Food,theirWood=a.Game.Craft.Materials[0],theirFood=a.Game.Life.Food;
  try{
   GD.Print("INTERACTION COOP / setup");ClearEnemies();CloseMenu();Teleport(0,12);a.Body.Position=HostPosition+Vector3.Right*2;game.Craft.Materials[0]=20;game.Life.Food=4;a.Game.Craft.Materials[0]=5;a.Game.Life.Food=9;game.Life.Health=a.Game.Life.Health=100;
   SendCoopPersonal(a.Id,CoopProfileJson(a.Game),"Interaction check",a.Body.Position,true);await CoopDelay(.4);
   MigrationChecks.Assert(TradeNear(1,a.Id),"Trade fixtures have clear sight and close range");PlayerTradeAction("open|"+a.Id);GD.Print("INTERACTION COOP / invitation sent");var trade=playerTrades.Single();interactionTradeAck=false;
   RpcId(a.Id,nameof(CoopCheckStage),"interaction-trade",a.Body.Position);
   await CoopWait(()=>!trade.Invited&&trade.ReadyB,"Guest could not accept invitation and confirm offer",15);
   GD.Print("INTERACTION COOP / guest offer confirmed");PlayerTradeAction("set|"+trade.Id+"|s:0|3");MigrationChecks.Assert(!trade.ReadyA&&!trade.ReadyB,"Changing host offer resets both confirmations");WithCoopActor(a,()=>PlayerTradeAction("ready|"+trade.Id+"|"+(trade.Revision-1)));MigrationChecks.Assert(!trade.ReadyB,"Delayed confirmation of an old offer cannot approve a changed trade");
   PlayerTradeAction("ready|"+trade.Id+"|"+trade.Revision);
   await CoopWait(()=>interactionTradeAck&&playerTrades.Count==0,"Mutual-confirmation trade did not complete",15);
   MigrationChecks.Assert(game.Craft.Materials[0]==17&&a.Game.Craft.Materials[0]==8&&game.Life.Food==5&&a.Game.Life.Food==8,"Network trade transfers exactly both offers");CloseMenu();
   PlayerTradeAction("open|"+a.Id);a.Body.Position=HostPosition+Vector3.Right*20;playerTradeScan=0;TickPlayerTrades();MigrationChecks.Assert(playerTrades.Count==0,"Leaving trade range cancels session");CloseMenu();
   a.Body.Position=HostPosition+Vector3.Right*2;var p=new BuildPiece{Id="interaction-owner-wall",Builder=CommunityOwner,Kind=2,X=a.Body.Position.X,Z=a.Body.Position.Z-2.5,Y=a.Body.Position.Y,Health=180,Stock=new int[15]};game.Building.Pieces.Add(p);SyncStructures();interactionOwnerAck=false;
   RpcId(a.Id,nameof(CoopWorldUpdate),Json.Write(WorldSnapshot()));SendCoopPersonal(a.Id,CoopProfileJson(a.Game),"Owner check",a.Body.Position,true);RpcId(a.Id,nameof(CoopCheckStage),"interaction-owner",new Vector3((float)p.X,(float)p.CenterY,(float)p.Z));
   await CoopWait(()=>interactionOwnerAck,"Guest owner restriction check missing",15);MigrationChecks.Assert(game.Building.Pieces.Contains(p)&&p.Health==180,"Guest hammer cannot delete host build");game.Building.Pieces.Remove(p);SyncStructures();
   GD.Print("PASS / two-process interaction / invitation / both confirmations / changed offer reset / exact exchange / distance cancellation / guest cannot remove host builds");
  }finally{playerTrades.Clear();game.Building.Pieces.RemoveAll(p=>p.Id=="interaction-owner-wall");game.Craft.Materials[0]=ownWood;game.Life.Food=ownFood;a.Game.Craft.Materials[0]=theirWood;a.Game.Life.Food=theirFood;a.Hammer=false;CloseMenu();if(GodotObject.IsInstanceValid(a.Body)&&coopActors.ContainsKey(a.Id))SendCoopPersonal(a.Id,CoopProfileJson(a.Game),"Interaction checks complete",a.Body.Position,true);}
 }
 async Task ClientInteractionChecks(string stage,Vector3 target){
  if(stage=="interaction-trade"){
   await CoopWait(()=>playerTradeView.Incoming&&!string.IsNullOrEmpty(playerTradeView.Id),"Trade invitation missing",15);string id=playerTradeView.Id;coopMenu="playertrade";paused=true;
   CoopRequest("playertrade","join|"+id);await CoopWait(()=>!playerTradeView.Invited,"Invitation accept missing");CoopRequest("playertrade","set|"+id+"|s:7|1");await CoopWait(()=>playerTradeView.Mine.Any(l=>l.Key=="s:7"&&l.Amount==1),"Guest offer missing");CoopRequest("playertrade","ready|"+id+"|"+playerTradeView.Revision);
   await CoopWait(()=>playerTradeView.Theirs.Any(l=>l.Key=="s:0"&&l.Amount==3)&&!playerTradeView.Accepted,"Offer change did not reset guest acceptance",15);CoopRequest("playertrade","ready|"+id+"|"+playerTradeView.Revision);
   await CoopWait(()=>game.Craft.Materials[0]==8&&game.Life.Food==8&&string.IsNullOrEmpty(playerTradeView.Id),"Trade inventories were not synchronized",15);CloseMenu();RpcId(1,nameof(InteractionAck),"trade");
  }else{
   await CoopWait(()=>game.Building.Pieces.Any(p=>p.Id=="interaction-owner-wall"),"Owner wall replica missing");player.Rotation=Vector3.Zero;camera.LookAt(target);var rot=camera.Rotation;player.Rotation=new(0,rot.Y,0);pitch=rot.X;camera.Rotation=new(pitch,0,0);await CoopDelay(.5);
   MigrationChecks.Assert(AimedBuildPiece()?.Id=="interaction-owner-wall","Guest targets actual host wall collider");CoopRequest("hammer","1");CoopRequest("remove","destroy");await CoopDelay(.5);MigrationChecks.Assert(game.Building.Pieces.Any(p=>p.Id=="interaction-owner-wall"),"Owner wall disappeared on guest");RpcId(1,nameof(InteractionAck),"owner");
  }
 }
}
