using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
sealed partial class SurvivorSettlement {
 [DataMember] public int StoryStage,Development,DeadResidents,MissionKills;
 [DataMember] public bool RescuedScout,Destroyed;
 [DataMember] public string Foundation="",Policy="",StoryResult="";
 [DataMember] public double StoryDue,RestockAt,QuestX,QuestZ;
 [DataMember] public int[] MarketStock=new int[]{2,1,1,1};
}
[DataContract] sealed class LedgerContract {
 [DataMember] public string State="unmet",Route="",Owner="",Outcome="";
 [DataMember] public int Target=-1,DefeatedRaiders;
 [DataMember] public bool WaterSabotaged,GateSabotaged,Burning,CacheTaken,RaidScheduled,RaidLocated,RaidStarted,RaidOver,Warned;
 [DataMember] public double Due,RaidDue,RaidX,RaidZ,RaidTime,FireTime;
}
sealed partial class ExpeditionWorldChanges {[DataMember] public LedgerContract Ledger=new();}

