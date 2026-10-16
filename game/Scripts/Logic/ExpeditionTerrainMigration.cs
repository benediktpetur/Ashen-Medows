using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
[DataContract]sealed class ExpeditionTerrainFootprint{[DataMember]public double X,Z,Radius;}
sealed partial class ExpeditionWorldChanges{
 [DataMember]public int TerrainRevision;
 [DataMember]public List<ExpeditionTerrainFootprint> LegacyTerrainAreas=new();
 Dictionary<(int,int),List<ExpeditionTerrainFootprint>> terrainFootprints;
 void IndexTerrainFootprints(){LegacyTerrainAreas??=new();terrainFootprints=new();foreach(var a in LegacyTerrainAreas){double r=a.Radius+12;for(int x=(int)Math.Floor((a.X-r)/64);x<=(int)Math.Floor((a.X+r)/64);x++)for(int z=(int)Math.Floor((a.Z-r)/64);z<=(int)Math.Floor((a.Z+r)/64);z++){var key=(x,z);if(!terrainFootprints.TryGetValue(key,out var list))terrainFootprints[key]=list=new();list.Add(a);}}}
 public double LegacyTerrainWeight(double x,double z){if(terrainFootprints==null)IndexTerrainFootprints();if(!terrainFootprints.TryGetValue((ExpeditionWorld.Cell(x),ExpeditionWorld.Cell(z)),out var list))return 0;double weight=0;foreach(var a in list){double t=Math.Clamp((Math.Sqrt((x-a.X)*(x-a.X)+(z-a.Z)*(z-a.Z))-a.Radius)/12,0,1);double w=1-t*t*(3-2*t);weight=Math.Max(weight,w);}return weight;}
 public void UpgradeTerrain(List<BuildPiece> pieces,List<ExpeditionBag> camps){if(TerrainRevision>=2)return;LegacyTerrainAreas??=new();foreach(var p in pieces)if(p!=null&&p.Kind>=0&&p.Kind<BuildDefinition.All.Length)LegacyTerrainAreas.Add(new(){X=p.X,Z=p.Z,Radius=Math.Max(p.Def.W,p.Def.D)*.75+3});foreach(var p in Paths)LegacyTerrainAreas.Add(new(){X=p.X,Z=p.Z,Radius=4});foreach(var p in Wells)LegacyTerrainAreas.Add(new(){X=p.X,Z=p.Z,Radius=4});foreach(var c in camps)if(c.Seed==Seed)LegacyTerrainAreas.Add(new(){X=c.X,Z=c.Z,Radius=3});TerrainRevision=2;IndexTerrainFootprints();}
}
