using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
[DataContract] sealed class ExpeditionJournal { [DataMember] public List<string> Seen=new List<string>(); }
static class Json { public static string Write<T>(T value){return NetJson.Write(value);}public static T Read<T>(string s){return NetJson.Read<T>(s);} }
sealed partial class ExpeditionWorld {
 public const int Size=3200,ChunkSize=64,Radius=4;public int Seed;
 public ExpeditionWorld(int seed){Seed=seed;InitializeRegions();}
 public static int Cell(double n){return (int)Math.Floor(n/64);}
 public uint Hash(int x,int z){unchecked{uint n=(uint)Seed^(uint)x*374761393u^(uint)z*668265263u;n=(n^(n>>13))*1274126177u;return n^(n>>16);}}
 public int Tier(double x,double z){return OrganicTier(x,z);}
 public static void Check(){CheckOrganic(new ExpeditionWorld(431),new ExpeditionWorld(431),new ExpeditionWorld(432));}
}
