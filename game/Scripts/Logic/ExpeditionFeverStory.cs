using System.Runtime.Serialization;

// World-owned encounter. Children's visuals never enter the damage or loot systems.
[DataContract] sealed class ExpeditionFeverStory {
 [DataMember] public int Stage,Waypoint;
 [DataMember] public double X,Y,Z,Yaw,Timer,LineTime;
 [DataMember] public string Owner="",Line="";
}
