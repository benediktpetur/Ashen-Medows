using Godot;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
[DataContract] sealed class CommunityNpcFrame {
 [DataMember] public string Id,Pose;
 [DataMember] public double X,Y,Z,Yaw,Health,Injury;
}
public partial class MigrationScene {
 List<CommunityNpcFrame> CommunityFrame(Vector3 viewer)=>Community.Survivors.Where(n=>n.State!="dead"&&NpcPoint(n).DistanceTo(viewer)<180).Select(n=>new CommunityNpcFrame{Id=n.Id,Pose=n.Pose,X=n.X,Y=n.Y,Z=n.Z,Yaw=n.Yaw,Health=n.Health,Injury=n.Injury}).ToList();
 void ApplyCommunityFrame(List<CommunityNpcFrame> frames){if(frames==null)return;foreach(var f in frames){var n=Community.Survivors.FirstOrDefault(s=>s.Id==f.Id);if(n==null)continue;n.X=f.X;n.Y=f.Y;n.Z=f.Z;n.Yaw=f.Yaw;n.Health=f.Health;n.Injury=f.Injury;n.Pose=f.Pose??"idle";}}
}
