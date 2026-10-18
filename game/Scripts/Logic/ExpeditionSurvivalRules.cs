// Ported engine-independent Expedition rules. Rendering and input live in Godot adapters.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[DataContract] sealed class ExpeditionWound {
 [DataMember] public double Blood=600,Recovery=1200;[DataMember] public bool Patched;
}
[DataContract] sealed class ExpeditionBag { [DataMember] public string Id;[DataMember] public int Seed;[DataMember] public double X,Z; }
[DataContract] sealed partial class ExpeditionLife {
 [DataMember] public double Hunger=100,Hydration=100,Energy=100,Health=100,TravelCooldown;
 [DataMember] public int[] WeaponQuality=new int[ExpeditionGame.GunNames.Length];
 [DataMember] public int MillCores,LegendaryCores;
 [DataMember] public int Medicine;[DataMember] public double RecoveryBoost;
 [DataMember] public int Bandages=3,Food=3,Bags,NightEssence;
 [DataMember] public List<ExpeditionWound> Wounds=new List<ExpeditionWound>();
 [DataMember] public List<ExpeditionBag> Camps=new List<ExpeditionBag>();[DataMember] public string Respawn;
 [OnDeserializing] void Defaults(StreamingContext c){Hunger=Hydration=Energy=Health=100;Bandages=Food=3;}
 public void Normalize(){NormalizeConditions();LegendaryCores=Math.Max(0,LegendaryCores);if(WeaponQuality==null)WeaponQuality=new int[ExpeditionGame.GunNames.Length];else if(WeaponQuality.Length!=ExpeditionGame.GunNames.Length)Array.Resize(ref WeaponQuality,ExpeditionGame.GunNames.Length);if(Wounds==null)Wounds=new List<ExpeditionWound>();if(Camps==null)Camps=new List<ExpeditionBag>();Hunger=Clamp(Hunger);Hydration=Clamp(Hydration);Energy=Clamp(Energy);Health=Math.Max(1,Math.Min(200,Health));Bandages=Math.Max(0,Bandages);Food=Math.Max(0,Food);Bags=Math.Max(0,Bags);NightEssence=Math.Max(0,NightEssence);}
 static double Clamp(double v){return double.IsNaN(v)||double.IsInfinity(v)?100:Math.Max(0,Math.Min(100,v));}
 public bool TickWounds(double dt){if(!double.IsFinite(dt)||dt<=0)return Wounds.Exists(w=>!w.Patched&&w.Blood<=0);int open=Wounds.FindAll(w=>!w.Patched).Count;foreach(var w in Wounds){if(w.Patched)w.Recovery-=dt;else w.Blood-=dt*open;}Wounds.RemoveAll(w=>w.Patched&&w.Recovery<=0);return Wounds.Exists(w=>!w.Patched&&w.Blood<=0);}
 public bool Unpatch(int index){if(index<0||index>=Wounds.Count||!Wounds[index].Patched)return false;Wounds[index].Patched=false;Wounds[index].Recovery=1200;return true;}
 public bool Patch(){var w=Wounds.FindAll(a=>!a.Patched);w.Sort((a,b)=>a.Blood.CompareTo(b.Blood));if(Bandages<1||w.Count==0)return false;Bandages--;w[0].Patched=true;w[0].Recovery=1200;return true;}
}
