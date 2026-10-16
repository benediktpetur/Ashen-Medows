using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
[DataContract] sealed class ExpeditionEarthPatch { [DataMember] public double X,Z,Height; }
[DataContract] sealed partial class ExpeditionWorldChanges {
 [DataMember] public double Time;
 [DataMember] public int Seed;[DataMember] public string Version="south-block-1";
 [DataMember] public List<string> CutTrees=new List<string>(),BurnedCars=new List<string>();
 [DataMember] public List<ExpeditionEarthPatch> Paths=new List<ExpeditionEarthPatch>(),Wells=new List<ExpeditionEarthPatch>(),Stumps=new List<ExpeditionEarthPatch>();
 Dictionary<long,List<ExpeditionEarthPatch>> pathIndex,wellIndex;HashSet<string> cutIndex,burnedIndex;
 static long CellKey(double x,double z){return ((long)(int)Math.Floor(x/32)<<32)|(uint)(int)Math.Floor(z/32);}
 public void Index(){IndexTerrainFootprints();if(CutTrees==null)CutTrees=new List<string>();if(BurnedCars==null)BurnedCars=new List<string>();if(Paths==null)Paths=new List<ExpeditionEarthPatch>();if(Wells==null)Wells=new List<ExpeditionEarthPatch>();if(Stumps==null)Stumps=new List<ExpeditionEarthPatch>();cutIndex=new HashSet<string>(CutTrees);burnedIndex=new HashSet<string>(BurnedCars);pathIndex=new Dictionary<long,List<ExpeditionEarthPatch>>();wellIndex=new Dictionary<long,List<ExpeditionEarthPatch>>();foreach(var p in Paths)AddIndex(pathIndex,p,3);foreach(var p in Wells)AddIndex(wellIndex,p,2);}
 static void AddIndex(Dictionary<long,List<ExpeditionEarthPatch>> index,ExpeditionEarthPatch p,double r){for(int x=(int)Math.Floor((p.X-r)/32);x<=(int)Math.Floor((p.X+r)/32);x++)for(int z=(int)Math.Floor((p.Z-r)/32);z<=(int)Math.Floor((p.Z+r)/32);z++){long k=((long)x<<32)|(uint)z;if(!index.ContainsKey(k))index[k]=new List<ExpeditionEarthPatch>();index[k].Add(p);}}
 public bool Cut(string id){if(cutIndex==null)Index();return id!=null&&cutIndex.Contains(id);}
 public bool Burned(string id){if(burnedIndex==null)Index();return burnedIndex.Contains(id);}
 public void Fell(string id){if(!Cut(id)){CutTrees.Add(id);cutIndex.Add(id);}}
 public void Burn(string id){if(!Burned(id)){BurnedCars.Add(id);burnedIndex.Add(id);}}
 public void Path(ExpeditionEarthPatch p){Paths.Add(p);if(pathIndex==null)Index();else AddIndex(pathIndex,p,3);}
 public void Well(ExpeditionEarthPatch p){Wells.Add(p);if(wellIndex==null)Index();else AddIndex(wellIndex,p,2);}
 public bool NearWell(double x,double z){if(wellIndex==null)Index();List<ExpeditionEarthPatch> list;if(wellIndex.TryGetValue(CellKey(x,z),out list))foreach(var p in list)if((x-p.X)*(x-p.X)+(z-p.Z)*(z-p.Z)<36)return true;return false;}
 public bool OnPath(double x,double z){if(pathIndex==null)Index();List<ExpeditionEarthPatch> list;if(pathIndex.TryGetValue(CellKey(x,z),out list))foreach(var p in list)if((x-p.X)*(x-p.X)+(z-p.Z)*(z-p.Z)<3.2)return true;return false;}
 public double Height(double x,double z,double h){if(pathIndex==null)Index();List<ExpeditionEarthPatch> list;if(pathIndex.TryGetValue(CellKey(x,z),out list))foreach(var p in list){double d=(x-p.X)*(x-p.X)+(z-p.Z)*(z-p.Z),blend=Math.Max(0,1-d/5)*.75;h+=Math.Max(-.45,Math.Min(.45,p.Height-h))*blend;}if(wellIndex.TryGetValue(CellKey(x,z),out list))foreach(var p in list){double d=(x-p.X)*(x-p.X)+(z-p.Z)*(z-p.Z);h-=1.25*Math.Exp(-d/.35);}return h;}
}
partial class ExpeditionWorld {
 public ExpeditionWorldChanges Changes;
 public double Height(double x,double z){double h=BaseHeight(x,z);if(Changes!=null){double w=Changes.LegacyTerrainWeight(x,z);if(w>0)h=h*(1-w)+LegacyBaseHeight(x,z)*w;}return Changes==null?h:Changes.Height(x,z,h);}
}
