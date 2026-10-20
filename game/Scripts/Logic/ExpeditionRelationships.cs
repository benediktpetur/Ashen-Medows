using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
[DataContract] sealed class RescueDog {
 [DataMember] public bool Initialized,Rescued,Dead;
 [DataMember] public int RescueVersion,KennelGuards;
 [DataMember] public string Name="",Owner="",Mode="follow";
 [DataMember] public double X,Y,Z,Health=100,Bond,Training,Together,CareAt;
 public bool NameReady=>Rescued&&!Dead&&Together>=900&&Bond>=15;
 public double NoiseRadius=>Math.Max(3,26-Training*.23);
 public void Hurt(double damage){if(Dead)return;Health=Math.Max(0,Health-damage);if(Health==0){Dead=true;Mode="dead";}}
}
[DataContract] sealed class PersonalStanding {
 [DataMember] public int Value;
 [DataMember] public string Memory="We have not earned each other's trust yet.";
 [DataMember] public double GiftAt;
 public string Rank=>Value<=-25?"Distrusted":Value<0?"Wary":Value<20?"Acquaintance":Value<50?"Trusted":"Close";
}
sealed partial class ExpeditionWorldChanges {
 [DataMember] public RescueDog Dog=new();
 [DataMember] public Dictionary<string,PersonalStanding> Relationships=new();
 public PersonalStanding Standing(string player,string person){Relationships??=new();string key=player+"|"+person;if(!Relationships.TryGetValue(key,out var r))Relationships[key]=r=new();return r;}
 public void Remember(string player,string person,int delta,string memory){var r=Standing(player,person);r.Value=Math.Clamp(r.Value+delta,-100,100);r.Memory=memory;}
}
