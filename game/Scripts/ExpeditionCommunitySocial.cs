using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 bool communityGatherSafe;
 Label communityFireCaption;
 Vector3 CommunityFirePoint=>Ground(Community.X+4,Community.Z+4);
 bool CommunityGathering(CampSurvivor n)=>Community.Gathering&&!Community.GatherPaused&&communityGatherSafe&&n.Member&&!n.Downed&&n.Injury<70&&string.IsNullOrEmpty(n.Owner)&&ExpeditionGame.Distance(n.X,n.Z,Community.X,Community.Z)<20;
 Vector3 CommunityFireSeat(CampSurvivor n){double angle=n.Encounter*Math.PI*2/3+.3;var p=Ground(CommunityFirePoint.X+Math.Sin(angle)*2.3,CommunityFirePoint.Z+Math.Cos(angle)*2.3);return p;}
 void TickCommunitySocial(double dt){communityGatherSafe=!infected.Any(e=>!Peaceful(e.Id)&&!PassiveStoryEnemy(e)&&!CheckpointPassive(e)&&Distance(e.Body.Position,CommunityFirePoint)<25);bool was=Community.Gathering;
  Community.TickSocial(dt,game.World.Changes.Time,game.Building.Pieces,communityGatherSafe);
  if(!was&&Community.Gathering){game.Notice=Community.GatherKind+" / the fire is lit. Join them if you want.";game.Persist();}
 }
 void LoseCommunityMember(CampSurvivor n){if(!Community.RecordLoss(n,game.World.Changes.Time))return;
  if(communityAvatars.Remove(n.Id,out var avatar))avatar.Mover.Body.QueueFree();campLifeSignature="";game.Notice=n.Name+" has died. Their name will stay with the camp.";game.Persist();if(CoopHost&&coopActors.Count>0)Rpc(nameof(CoopWorldUpdate),Json.Write(WorldSnapshot()));
 }
 void CommunitySocialAction(CampSurvivor n,string operation,bool near){if(n==null||CoopClient)return;
  if(operation=="vigil"){
   if(n.State!="dead"||!AtCommunityCamp){game.Notice="Return to camp to remember them.";return;}string key=n.Id+"/vigil/"+ExpeditionCommunity.DayAt(game.World.Changes.Time);if(Community.HeardStories.Contains(key)){game.Notice="You have already held a quiet vigil today.";return;}
   game.Change(()=>{Community.HeardStories.Add(key);Community.Remember(n,"vigil","Their name was spoken aloud. No one hurried the silence afterward.",game.World.Changes.Time);foreach(var member in Community.Survivors.Where(s=>s.Member))member.Morale=Math.Min(95,member.Morale+2);});game.Notice="You remember "+n.Name+" together. Grief takes its own time.";
  }else{
   string topic=operation.Substring(5);if(topic is not ("family" or "home" or "feelings" or "company"))return;
   if(!n.Member||!near||n.Downed){game.Notice="Stand beside an able resident to talk.";return;}
   if(infected.Any(e=>!Peaceful(e.Id)&&!PassiveStoryEnemy(e)&&Distance(e.Body.Position,NpcPoint(n))<15)){game.Notice="They need to get safe before talking.";return;}
   game.Change(()=>game.Notice=n.Name+": "+Community.Conversation(n,topic,game.World.Changes.Time));
  }
  game.Persist();if(CoopHost&&coopActors.Count>0)Rpc(nameof(CoopWorldUpdate),Json.Write(WorldSnapshot()));
 }
 void DrawCommunitySocial(){if(!Community.Established){if(communityFireCaption!=null)communityFireCaption.Visible=false;return;}communityFireCaption??=HudLabel(15);communityFireCaption.Position=new(220,460);communityFireCaption.Size=new(840,90);communityFireCaption.AutowrapMode=TextServer.AutowrapMode.WordSmart;
  communityFireCaption.Visible=!paused&&dungeon==null&&Community.Gathering&&Distance(player.Position,CommunityFirePoint)<12;
  communityFireCaption.Text=!Community.GatherPaused?Community.CurrentFireside:"The conversation stops. There is something in the dark.";
 }
 void ShowCampStories(){Community.NormalizeSocial();var b=Panel("THE PEOPLE WHO MAKE THIS HOME");coopMenu="campstories";
  PackLabel(b,$"DAY {ExpeditionCommunity.DayAt(game.World.Changes.Time)} / {ExpeditionCommunity.Weekday(game.World.Changes.Time)} / {(int)game.Hour:00}:00",16);
  PackLabel(b,"Sunday, 18:00–21:00, at the communal hearth. The first evening with someone new can bring everyone together sooner. Needs two able residents at home, a hearth improvement and a meal each. Threats interrupt the conversation. You can stay away without a penalty.",13);
  var newest=Community.Survivors.FirstOrDefault(n=>n.Id==Community.NewestMember);if(newest!=null)PackLabel(b,"NEWEST ARRIVAL / "+newest.Name+" / "+newest.Belonging,15);
  if(Community.Gathering)PackLabel(b,Community.GatherKind+"\n"+Community.CurrentFireside,16);
  foreach(var n in Community.Survivors.Where(s=>s.Member)){PackLabel(b,n.Name+" / "+n.Belonging+(n.GriefRemaining>0?$" / mourning {n.GrievingFor} / {n.GriefRemaining/3600:0.0} active days remaining":""),14);}
  foreach(var loss in Community.Losses){PackLabel(b,$"IN MEMORY / {loss.Name} / day {loss.Day}",16);string id=loss.Id;Button(b,"SIT IN MEMORY OF "+loss.Name.ToUpperInvariant(),()=>{if(!CoopRequest("community",id+"|vigil"))CommunityAction(id+"|vigil");ShowCampStories();});}
  PackLabel(b,"THE CAMP REMEMBERS",18);foreach(var memory in Community.Memories.AsEnumerable().Reverse().Take(40))PackLabel(b,$"Day {memory.Day} / {memory.Name}\n{memory.Text}",13);
  if(Community.Memories.Count==0)PackLabel(b,"Stories begin when someone arrives. Talk beside your residents, and come back for the fire.",14);Button(b,"BACK TO COMMUNITY",()=>ShowCommunity());Button(b,"BACK TO WORLD",CloseMenu);
 }
}
