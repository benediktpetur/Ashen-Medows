using System;
using System.Linq;
using System.Collections.Generic;
static class ExpeditionBuildSnap {
 public static BuildPiece Connect(BuildPiece target,int kind,double x,double z,double nx,double ny,double nz,double yaw,IEnumerable<BuildPiece> pieces=null){
  var d=BuildDefinition.All[kind];target.Local(x,z,out double lx,out double lz);double px=0,pz=0,y=target.Y;double angle=target.Yaw;
  bool panel=ExpeditionOpenings.IsWall(d)||d.BaseKind is 2 or 3;
  bool targetDeck=target.Def.BaseKind is 0 or 1 or 7;
  if(targetDeck&&(d.BaseKind is 0 or 1)){
   // Aiming at a deck top chooses an adjacent socket, never stacks a second tile.
   double r=target.Yaw*Math.PI/180;double ax=ny>.5?lx:nx*Math.Cos(r)-nz*Math.Sin(r),az=ny>.5?lz:nx*Math.Sin(r)+nz*Math.Cos(r);
   if(Math.Abs(ax)>Math.Abs(az))px=(ax<0?-1:1)*(target.Def.W+d.W)/2;else pz=(az<0?-1:1)*(target.Def.D+d.D)/2;
   y=target.Y+target.Def.H-d.H;
   return new BuildPiece{Kind=kind,X=target.X+px*Math.Cos(r)+pz*Math.Sin(r),Z=target.Z-px*Math.Sin(r)+pz*Math.Cos(r),Y=y,Yaw=target.Yaw};
  }
  if(targetDeck&&panel){
   double r=target.Yaw*Math.PI/180;double ax=ny>.5?lx:nx*Math.Cos(r)-nz*Math.Sin(r),az=ny>.5?lz:nx*Math.Sin(r)+nz*Math.Cos(r);
   if(Math.Abs(ax)>Math.Abs(az)){px=(ax<0?-1:1)*target.Def.W/2;angle+=90;}else pz=(az<0?-1:1)*target.Def.D/2;
   return new BuildPiece{Kind=kind,X=target.X+px*Math.Cos(r)+pz*Math.Sin(r),Z=target.Z-px*Math.Sin(r)+pz*Math.Cos(r),Y=target.Y+target.Def.H,Yaw=angle};
  }
  if(ny>.5){y+=target.Def.H;
   if(panel&&target.Def.W>1&&target.Def.D>1){bool side=Math.Abs(lx)>Math.Abs(lz);if(side){px=(lx<0?-1:1)*(target.Def.W/2);angle+=90;}else pz=(lz<0?-1:1)*(target.Def.D/2);}
   else if((d.BaseKind is 1 or 7)&&(ExpeditionOpenings.IsWall(target.Def))){pz=-(d.D/2-target.Def.D/2);}
   else if(d.Function!="structure"){px=Math.Round(lx/.25)*.25;pz=Math.Round(lz/.25)*.25;angle=yaw;}
  }else{double r=target.Yaw*Math.PI/180;double localNx=nx*Math.Cos(r)-nz*Math.Sin(r),localNz=nx*Math.Sin(r)+nz*Math.Cos(r);
   if(Math.Abs(localNx)>Math.Abs(localNz))px=Math.Sign(localNx)*(target.Def.W+d.W)/2;
   else pz=Math.Sign(localNz)*(target.Def.D+d.D)/2;
  }
  if(ny>.5&&(d.BaseKind is 1 or 7)&&(ExpeditionOpenings.IsWall(target.Def))&&pieces!=null){var deck=pieces.Where(p=>(p.Def.BaseKind is 0 or 1)&&p.Y<target.Y+.1&&Math.Abs(p.Y+p.Def.H-target.Y)<.15&&Math.Sqrt(Math.Pow(p.X-target.X,2)+Math.Pow(p.Z-target.Z,2))<2).OrderBy(p=>Math.Pow(p.X-x,2)+Math.Pow(p.Z-z,2)).FirstOrDefault();if(deck!=null)return new BuildPiece{Kind=kind,X=deck.X,Z=deck.Z,Y=y,Yaw=deck.Yaw};}
  double a=target.Yaw*Math.PI/180;return new BuildPiece{Kind=kind,X=target.X+px*Math.Cos(a)+pz*Math.Sin(a),Z=target.Z-px*Math.Sin(a)+pz*Math.Cos(a),Y=y,Yaw=angle};
 }
}
