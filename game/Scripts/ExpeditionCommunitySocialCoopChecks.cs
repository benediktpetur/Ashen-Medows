using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
public partial class MigrationScene {
 int communitySocialAck;
 [Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]void CoopSocialAck(int stage){if(coopCheck&&CoopHost&&coopActors.ContainsKey(Multiplayer.GetRemoteSenderId()))communitySocialAck=Math.Max(communitySocialAck,stage);}
 async Task CommunitySocialCoopChecks(CoopActor actor){var oldCommunity=Community;var pieces=game.Building.Pieces.ToList();var oldPosition=actor.Body.Position;double oldTime=game.World.Changes.Time;bool fixture=communityFixture;try{
  ClearEnemies();foreach(var a in communityAvatars.Values)a.Mover.Body.QueueFree();communityAvatars.Clear();game.World.Changes.Community=new();game.World.Changes.Time=0;communityFixture=true;SocialCampFixture(HostPosition.X+10,HostPosition.Z);SpawnCommunity();var n=Community.Survivors[0];actor.Body.Position=NpcPoint(n)+Vector3.Right;actor.Game.Save.X=actor.Body.Position.X;actor.Game.Save.Z=actor.Body.Position.Z;int food=actor.Game.Life.Food,bandages=actor.Game.Life.Bandages;communitySocialAck=0;
  RpcId(actor.Id,nameof(CoopWorldUpdate),Json.Write(WorldSnapshot()));SendCoopPersonal(actor.Id,CoopProfileJson(actor.Game),"",actor.Body.Position,true);RpcId(actor.Id,nameof(CoopCheckStage),"community-social-talk",actor.Body.Position);await CoopWait(()=>communitySocialAck>=1,"Guest family conversation/quiet company failed",20);
  MigrationChecks.Assert(Community.Memories.Count(m=>m.Person==n.Id&&m.Kind=="family")==1&&n.LastConversation.Contains("Staying is enough")&&n.SupportUntil>0,"Guest talk is shared and cooldown prevents farming");MigrationChecks.Assert(actor.Game.Life.Food==food&&actor.Game.Life.Bandages==bandages&&n.GroupAcceptance<1,"Talk consumes no guest supplies and does not grant instant group acceptance");
  LoseCommunityMember(n);RpcId(actor.Id,nameof(CoopCheckStage),"community-social-loss",actor.Body.Position);await CoopWait(()=>communitySocialAck>=2,"Guest memorial/mourning snapshot missing",20);GD.Print("PASS / two-process camp social life: guest dialogue, shared history, private supplies, cooldown, neutral group standing, named death/grief and removed replica");
 }finally{foreach(var a in communityAvatars.Values)a.Mover.Body.QueueFree();communityAvatars.Clear();game.World.Changes.Community=oldCommunity;game.World.Changes.Time=oldTime;game.Building.Pieces.Clear();game.Building.Pieces.AddRange(pieces);actor.Body.Position=oldPosition;communityFixture=fixture;SyncStructures();RpcId(actor.Id,nameof(CoopWorldUpdate),Json.Write(WorldSnapshot()));}}
 async Task ClientCommunitySocialChecks(string stage){communityFixture=true;
  if(stage=="community-social-talk"){
   await CoopWait(()=>Community.Survivors.Count(n=>n.Member)==3,"Shared camp fixture missing");var n=Community.Survivors[0];CoopRequest("community",n.Id+"|talk-family");await CoopWait(()=>Community.Survivors[0].LastConversation.Contains("June"),"Family dialogue not replicated");CoopRequest("community",n.Id+"|talk-family");await CoopWait(()=>Community.Survivors[0].LastConversation.Contains("toast"),"Remembered family variant missing");CoopRequest("community",n.Id+"|talk-company");await CoopWait(()=>Community.Survivors[0].SupportUntil>0,"Quiet company result missing");CoopRequest("community",n.Id+"|talk-company");await CoopWait(()=>game.Notice.Contains("Let the moment stand"),"Comfort duplicate not rejected");RpcId(1,nameof(CoopSocialAck),1);
  }else{
   await CoopWait(()=>Community.Losses.Count==1&&Community.Survivors[0].State=="dead"&&Community.Survivors.Where(n=>n.Member).All(n=>n.GriefRemaining>10780),"Named loss/grief not replicated");SpawnCommunity();MigrationChecks.Assert(!communityAvatars.ContainsKey(Community.Survivors[0].Id),"Dead settler has no live client replica");ShowCampStories();CloseMenu();communityFixture=false;RpcId(1,nameof(CoopSocialAck),2);
  }
 }
}
