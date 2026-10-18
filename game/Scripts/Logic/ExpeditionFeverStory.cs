using System.Runtime.Serialization;

// World-owned encounter. Children's visuals never enter the damage or loot systems.
[DataContract] sealed class ExpeditionFeverStory {
 [DataMember] public int Stage,Waypoint;
 [DataMember] public int ArcVersion=1;
 [DataMember] public string Choice="",Disclosure="",Outcome="",GivenName="";
 [DataMember] public bool BiteKnown,GraveReady,GraveRead,CitySeen,Redemption,RedemptionClosed,Offscreen,Violence;
 [DataMember] public double GraveX,GraveZ,FledAt,FlightTime,SharedX,SharedZ;
 [DataMember] public double X,Y,Z,Yaw,Timer,LineTime;
 [DataMember] public string Owner="",Line="";
}
