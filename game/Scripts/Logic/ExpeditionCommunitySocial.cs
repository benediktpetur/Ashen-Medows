using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

sealed partial class CampSurvivor {
 [DataMember] public int ArrivalOrder;
 [DataMember] public double JoinedAt,HomeTime,GroupAcceptance,GriefRemaining,SupportUntil;
 [DataMember] public string GrievingFor="",LastConversation="";
 public string Belonging=>GroupAcceptance>=80?"Family by choice":GroupAcceptance>=55?"Belongs here":GroupAcceptance>=20?"Finding a place":"Neutral / getting acquainted";
}
[DataContract] sealed class CampMemory {
 [DataMember] public string Person,Name,Text,Kind;
 [DataMember] public int Day;
}
[DataContract] sealed class CampLoss {
 [DataMember] public string Id,Name;
 [DataMember] public int Encounter,Day;
}
sealed partial class ExpeditionCommunity {
 public const double MourningDuration=10800,RescueWindow=180;
 [DataMember] public int ArrivalSequence,WelcomedArrival,LastGatherDay,LastSundayDay,GatherDay,GatherLine;
 [DataMember] public string NewestMember="",GatherKind="";
 [DataMember] public bool Gathering,GatherPaused;
 [DataMember] public double GatherElapsed;
 [DataMember] public List<string> GatherScript=new(),HeardStories=new();
 [DataMember] public List<CampMemory> Memories=new();
 [DataMember] public List<CampLoss> Losses=new();
 public static int DayAt(double time)=>1+(int)(Math.Max(0,time)/3600);
 public static string Weekday(double time)=>new[]{"Monday","Tuesday","Wednesday","Thursday","Friday","Saturday","Sunday"}[(DayAt(time)-1)%7];
 public void NormalizeSocial(){GatherScript??=new();Memories??=new();Losses??=new();HeardStories??=new();NewestMember??="";GatherKind??="";
  foreach(var n in Survivors){n.GrievingFor??="";n.LastConversation??="";n.GroupAcceptance=Math.Clamp(n.GroupAcceptance,0,100);if(n.Member&&n.ArrivalOrder==0)n.ArrivalOrder=++ArrivalSequence;ArrivalSequence=Math.Max(ArrivalSequence,n.ArrivalOrder);}
  var newest=Survivors.Where(n=>n.Member).OrderByDescending(n=>n.ArrivalOrder).FirstOrDefault();NewestMember=newest?.Id??"";GatherLine=Math.Clamp(GatherLine,0,Math.Max(0,GatherScript.Count-1));
 }
 public void Welcome(CampSurvivor n,double time){n.ArrivalOrder=++ArrivalSequence;n.JoinedAt=time;n.HomeTime=n.GroupAcceptance=0;n.SupportUntil=0;n.GriefRemaining=0;n.GrievingFor="";NewestMember=n.Id;Remember(n,"arrival","Arrived with a bed, a meal and no promises of belonging yet.",time);}
 public void Remember(CampSurvivor n,string kind,string text,double time){Memories.Add(new(){Person=n?.Id??"",Name=n?.Name??"Camp",Kind=kind,Text=text,Day=DayAt(time)});while(Memories.Count>96)Memories.RemoveAt(0);}
 public bool RecordLoss(CampSurvivor n,double time){if(n==null||!n.Member||Losses.Any(l=>l.Id==n.Id))return false;
  Losses.Add(new(){Id=n.Id,Name=n.Name,Encounter=n.Encounter,Day=DayAt(time)});n.State="dead";n.Health=0;n.Downed=false;n.Owner=n.EscortOwner="";n.Pose="idle";n.Treatment=0;
  foreach(var other in Survivors.Where(s=>s.Member)){other.GriefRemaining=MourningDuration;other.GrievingFor=n.Name;other.Morale=Math.Max(5,other.Morale-15);}
  Gathering=GatherPaused=false;Remember(n,"loss","Died. Their place in this camp is not erased.",time);NormalizeSocial();return true;
 }
 public CampSurvivor[] AtHome()=>Survivors.Where(n=>n.Member&&!n.Downed&&n.Injury<70&&string.IsNullOrEmpty(n.Owner)&&ExpeditionGame.Distance(n.X,n.Z,X,Z)<20).OrderBy(n=>n.ArrivalOrder).ToArray();
 public void TickSocial(double dt,double time,List<BuildPiece> pieces,bool safe){if(!Established||dt<=0)return;NormalizeSocial();
  var home=AtHome();foreach(var n in Survivors.Where(n=>n.Member)){n.GriefRemaining=Math.Max(0,n.GriefRemaining-dt);bool cared=home.Contains(n)&&Food>0&&Sheltered(Bed(n,pieces),pieces);if(cared){n.HomeTime+=dt;if(home.Length>=2)n.GroupAcceptance=Math.Min(100,n.GroupAcceptance+dt*(n.GriefRemaining>0?.003:.006));}}
  int day=DayAt(time);double hour=(6+time/150)%24;bool evening=hour>=18&&hour<21;bool sunday=day%7==0;
  if(Gathering){if(!evening||day!=GatherDay){Gathering=GatherPaused=false;return;}GatherPaused=!safe||home.Length<2;if(GatherPaused)return;GatherElapsed+=dt;int line=Math.Min(GatherScript.Count-1,(int)(GatherElapsed/18));if(line>GatherLine){for(int i=GatherLine+1;i<=line;i++){Remember(null,"fireside",GatherScript[i],time);foreach(var speaker in Survivors)if(GatherScript[i]==speaker.Name+": "+CampVoice.Family(speaker,false)&&!HeardStories.Contains(speaker.Id+"/family"))HeardStories.Add(speaker.Id+"/family");}GatherLine=line;}return;}
  bool welcome=home.Any(n=>n.Id==NewestMember&&n.ArrivalOrder>WelcomedArrival);
  if(!evening||!safe||Improvements<2||home.Length<2||Food<home.Length||LastGatherDay==day||!(sunday&&LastSundayDay!=day||welcome))return;
  Gathering=true;GatherPaused=false;GatherDay=LastGatherDay=day;GatherKind=sunday?"Sunday remembrance":"A place for someone new";if(sunday)LastSundayDay=day;WelcomedArrival=home.Max(n=>n.ArrivalOrder);Food-=home.Length;GatherElapsed=0;GatherLine=0;GatherScript.Clear();
  GatherScript.Add("The tools are put aside. A meal is passed around the fire. Nobody is required to speak.");
  foreach(var n in home){string familyKey=n.Id+"/family";bool known=HeardStories.Contains(familyKey);GatherScript.Add(n.Name+": "+CampVoice.Family(n,known));
   if(n.GriefRemaining>0)GatherScript.Add(n.Name+": "+CampVoice.Grief(n));
   var other=home.FirstOrDefault(s=>s.Id!=n.Id);if(other!=null)GatherScript.Add(other.Name+": "+CampVoice.Response(other,n));
   GatherScript.Add(n.Name+": "+CampVoice.Home(n,true));n.GroupAcceptance=Math.Min(100,n.GroupAcceptance+4);n.Morale=Math.Min(95,n.Morale+4);
  }
  GatherScript.Add("Someone puts another log on the fire. For a while, the woods are not the only thing you can hear.");Remember(null,"fireside",GatherScript[0],time);
 }
 public string CurrentFireside=>Gathering&&GatherScript.Count>0?GatherScript[Math.Clamp(GatherLine,0,GatherScript.Count-1)]:"";
 public string Conversation(CampSurvivor n,string topic,double time){string text;
  if(topic=="family"){string key=n.Id+"/family";text=CampVoice.Family(n,HeardStories.Contains(key));if(!HeardStories.Contains(key)){HeardStories.Add(key);Remember(n,"family",text,time);}}
  else if(topic=="home")text=CampVoice.Home(n,Food>0);
  else if(topic=="feelings")text=n.GriefRemaining>0?CampVoice.Grief(n):n.Injury>40?"I keep listening for footsteps behind me. Even here. My body still thinks we're out there.":Food==0?"It's hard to feel safe when the cupboards are empty. I know you're trying.":n.GroupAcceptance<20?"I still fold everything into my bag before I sleep. Don't take it personally. I'm learning I can unpack.":"Today I caught myself thinking about next spring. That scared me more than the dead. I want to be here for it.";
  else{if(time<n.SupportUntil)return "You have already spent some quiet time together. Let the moment stand.";n.SupportUntil=time+600;n.Morale=Math.Min(95,n.Morale+3);text=n.GriefRemaining>0?"You sit together without trying to fix it. After a while: 'Thank you for not telling me to move on.'":"You stay and listen. 'You don't have to find the right words. Staying is enough.'";}
  n.LastConversation=text;return text;
 }
}
static class CampVoice {
 public static string Family(CampSurvivor n,bool known)=>n.Encounter switch {
  0=>known?"June used to burn the toast and scrape it over the sink so Elsie wouldn't notice. Funny what you miss. Not the big things. The smell of that kitchen.":"My sister June had a little girl, Elsie. We lost each other at the evacuation stairs. I went back twice. I still count the windows in every building we pass.",
  1=>known?"Signe would have hated these benches. She'd have fixed them before dinner, then pretended it was no trouble. I can almost hear her complaining.":"My wife Signe died before I reached the road. Thirty-one years together, and all I brought out was her house key. I can't bring myself to throw it away.",
  _=>known?"Theo wanted to know whether fish sleep. I never looked it up. If you find a book about rivers, leave it by my bed. I'd like to know the answer.":"Daniel took our boy Theo toward the ferry. I stayed to help at the clinic. We had a meeting place. They never came. I don't know whether to say 'missing' or 'gone'."
 };
 public static string Home(CampSurvivor n,bool fed)=>!fed?"I'd like to tell you this feels like home. Right now I just need us to eat tomorrow.":n.GroupAcceptance<20?"Someone left a cup beside my bed. I don't know whose it was. I keep thinking they'll ask for it back.":n.Encounter switch {
  0=>n.GroupAcceptance>=55?"We laughed over dinner yesterday. Then I felt guilty. I think June would be furious with me for feeling guilty.":"I planted something without wondering if I'd be here to see it grow. That's new.",
  1=>n.GroupAcceptance>=55?"I used to repair things because they were broken. Now I do it because someone's going to use them tomorrow. That's a different feeling.":"Somebody saved me the dry seat. No bargain, no favor owed. Just a seat.",
  _=>n.GroupAcceptance>=55?"I still watch the road for them. But I no longer feel like I'm betraying them when I'm glad to come back here.":"I slept through the rain last night. When I woke up, I could hear you all arguing about breakfast. I wanted to stay in that sound."
 };
 public static string Grief(CampSurvivor n)=>$"I almost called for {n.GrievingFor}. Just now. There's a job I'd have asked them to help with. I don't know when that stops.";
 public static string Response(CampSurvivor speaker,CampSurvivor subject)=>subject.GroupAcceptance<20?$"{subject.Name.Split(' ')[0]}, keep the cup. It's yours. You don't have to earn a place at this fire.":speaker.Encounter==1?"We can miss them and still make something worth staying for. Some days I believe that more than others.":"You can tell that story again next week. I want to remember their name.";
}
