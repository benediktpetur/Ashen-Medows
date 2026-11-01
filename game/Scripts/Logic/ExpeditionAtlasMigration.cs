using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
[DataContract] sealed class AtlasGroundPad { [DataMember]public double X,Z,Y,Radius; }
sealed partial class ExpeditionWorldChanges {
 [DataMember]public int AtlasRevision;
 [DataMember]public double AtlasHomeFloor;
 [DataMember]public List<AtlasGroundPad> AtlasPads=new();
 Dictionary<(int,int),List<AtlasGroundPad>> atlasPadIndex;
 void IndexAtlasPads(){atlasPadIndex=new();foreach(var p in AtlasPads??new()){double r=p.Radius+6;for(int x=ExpeditionWorld.Cell(p.X-r);x<=ExpeditionWorld.Cell(p.X+r);x++)for(int z=ExpeditionWorld.Cell(p.Z-r);z<=ExpeditionWorld.Cell(p.Z+r);z++){if(!atlasPadIndex.TryGetValue((x,z),out var list))atlasPadIndex[(x,z)]=list=new();list.Add(p);}}}
 public bool AtlasHasBase(double x,double z,double margin=0){AtlasPads??=new();if(atlasPadIndex==null)IndexAtlasPads();return atlasPadIndex.TryGetValue((ExpeditionWorld.Cell(x),ExpeditionWorld.Cell(z)),out var list)&&list.Any(p=>ExpeditionGame.Distance(x,z,p.X,p.Z)<p.Radius+margin);}
 public double AtlasGround(double x,double z,double height){
  if(atlasPadIndex==null)IndexAtlasPads();
  if(AtlasHomeFloor>0&&Math.Abs(x-ExpeditionWorld.HomeX)<8&&Math.Abs(z-ExpeditionWorld.HomeZ)<8)return AtlasHomeFloor-.21;
  if(!atlasPadIndex.TryGetValue((ExpeditionWorld.Cell(x),ExpeditionWorld.Cell(z)),out var list))return height;
  AtlasGroundPad nearest=null;double best=double.MaxValue;
  foreach(var p in list){double d=ExpeditionGame.Distance(x,z,p.X,p.Z);if(d<p.Radius+6&&d<best){best=d;nearest=p;}}
  if(nearest==null)return height;double t=Math.Clamp((best-nearest.Radius)/6,0,1);t=t*t*(3-2*t);return nearest.Y*(1-t)+height*t;
 }
 public void UpgradeAtlas(ExpeditionWorld world,List<BuildPiece> pieces){
  AtlasPads??=new();if(AtlasRevision>0)return;
  PreservedResidentialLots??=new();
  foreach(var p in pieces){
   var lot=world.Residential((int)Math.Floor(p.X/64),(int)Math.Floor(p.Z/64));if(lot!=null&&!PreservedResidentialLots.Contains(lot.Id))PreservedResidentialLots.Add(lot.Id);
   if(!p.Def.Ground)continue;
   if(ExpeditionWorld.HomeInterior(p.X,p.Z)){if(AtlasHomeFloor==0||p.Y<AtlasHomeFloor)AtlasHomeFloor=p.Y;continue;}
   if(p.Def.BaseKind!=0&&pieces.Any(f=>f.Def.BaseKind==0&&ExpeditionGame.Distance(p.X,p.Z,f.X,f.Z)<12))continue;
   AtlasPads.Add(new(){X=p.X,Z=p.Z,Y=p.Y,Radius=Math.Sqrt(p.Def.W*p.Def.W+p.Def.D*p.Def.D)*.5+1});
  }
  AtlasRevision=1;IndexAtlasPads();
 }
}
