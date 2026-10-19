using System;
using System.Linq;
using System.Collections.Generic;
sealed class OpeningPart {
 public double X,Y,Z,W,H,D,Yaw;public string Role;public bool Solid,Moving;
 public OpeningPart(string role,double x,double y,double z,double w,double h,double d,bool solid=true,double yaw=0,bool moving=false){Role=role;X=x;Y=y;Z=z;W=w;H=h;D=d;Solid=solid;Yaw=yaw;Moving=moving;}
 public bool Contains(double x,double y,double z,double pad){double a=Yaw*Math.PI/180,c=Math.Cos(a),s=Math.Sin(a),dx=x-X,dz=z-Z;return Math.Abs(dx*c-dz*s)<W/2+pad&&Math.Abs(dx*s+dz*c)<D/2+pad&&Math.Abs(y-Y)<H/2+pad;}
}
static class ExpeditionOpenings {
 public static bool IsWall(BuildDefinition d)=>d.BaseKind is 2 or 3||d.Opening is "doorway" or "window";
 static readonly Dictionary<(int,bool,int),List<OpeningPart>> cache=new();
 public static List<OpeningPart> Parts(BuildPiece p,double? angle=null){if(angle.HasValue)return MakeParts(p,angle);var key=(p.Kind,p.Open,p.DoorSwing<0?-1:1);if(!cache.TryGetValue(key,out var parts)){parts=MakeParts(p,null);cache[key]=parts;}return parts;}
 static List<OpeningPart> MakeParts(BuildPiece p,double? angle){var d=p.Def;var list=new List<OpeningPart>();double w=d.W,h=d.H,z=d.D;void Add(string role,double x,double y,double depth,double width,double height,double thick,bool solid=true)=>list.Add(new(role,x,y,depth,width,height,thick,solid));
  if(d.Opening=="window"){
   const double gap=1.65,bottom=1,top=2.1;for(int side=-1;side<=1;side+=2)Add("wall",side*(w+gap)/4,h/2,0,(w-gap)/2,h,z);
   Add("wall",0,bottom/2,0,w,bottom,z);Add("wall",0,(top+h)/2,0,w,h-top,z);
   for(int side=-1;side<=1;side+=2)Add("frame",side*(gap/2+.015),(bottom+top)/2,-z/2-.025,.09,top-bottom+.12,.07,false);
   Add("frame",0,bottom-.025,-.04,gap+.18,.10,z+.16,false);Add("frame",0,top+.025,-.04,gap+.18,.10,z+.10,false);
   if(d.Tier>=3)Add("frame",0,(top+bottom)/2,0,.045,top-bottom,z+.04);
   return list;
  }
  bool gate=d.Opening=="gate";double opening=gate?w-.6:1.2,openingTop=gate?h-.22:2.24;
  for(int side=-1;side<=1;side+=2)Add("wall",side*(w+opening)/4,h/2,0,(w-opening)/2,h,z);
  Add("frame",0,(openingTop+h)/2,0,w,h-openingTop,z);
  int leaves=gate?2:1;double leafW=opening/leaves-.045,leafH=openingTop-.06,rotation=(angle??(p.Open?90:0))*(p.DoorSwing<0?-1:1);
  for(int leaf=0;leaf<leaves;leaf++){
   int side=leaf==0?1:-1;double hinge=leaf==0?-opening/2:opening/2;double yaw=side*rotation,a=yaw*Math.PI/180;
   void LeafPart(string role,double x,double y,double depth,double width,double height,double thick,bool solid=false){double localX=side*x;list.Add(new(role,hinge+localX*Math.Cos(a)+depth*Math.Sin(a),y,-localX*Math.Sin(a)+depth*Math.Cos(a),width,height,thick,solid,yaw,true));}
   LeafPart("leaf",leafW/2,leafH/2+.03,0,leafW,leafH,z*.65,true);
   int ribs=d.Tier==1?5:6;for(int i=1;i<ribs;i++)LeafPart("seam",leafW*i/ribs,leafH/2+.03,-z*.34,.025,leafH,.025);
   for(int band=0;band<2;band++)LeafPart("brace",leafW/2,.38+band*(leafH-.65),-z*.4,leafW,.09,.06);
   LeafPart("handle",leafW-.12,1.05,-z*.52,.08,.18,.08);if(gate&&leaf==0)LeafPart("frame",leafW+.02,1.1,0,.16,1.6,z*.9,true);
   if(d.Tier>=3)for(int i=0;i<4;i++)LeafPart("bolt",.08+i*(leafW-.16)/3,.37,-z*.6,.035,.035,.025);
   if(d.Tier==5){LeafPart("warning",leafW/2,.62,-z*.43,leafW,.13,.04);LeafPart("plate",leafW*.55,1.7,-z*.43,.35,.22,.04);}
  }
  return list;
 }
 public static bool SolidAt(BuildPiece p,double x,double y,double z,double padding){p.Local(x,z,out double lx,out double lz);double depth=p.Open&&p.Def.Opening!="window"?p.Def.W/2:p.Def.D/2;if(Math.Abs(lx)>p.Def.W/2+padding||Math.Abs(lz)>depth+padding||y<p.Y-padding||y>p.Y+p.Def.H+padding)return false;return Parts(p).Any(part=>part.Solid&&part.Contains(lx,y-p.Y,lz,padding));}
}
sealed partial class ExpeditionGame {
 public bool KnownOpening(ExpeditionRecipe r){if(r.Kind!="build")return false;var d=BuildDefinition.All[r.Result];if(d.Opening is not ("doorway" or "window"))return false;var wall=ExpeditionRecipe.All.FirstOrDefault(w=>w.Kind=="build"&&w.Tier==d.Tier&&BuildDefinition.All[w.Result].Function=="structure"&&BuildDefinition.All[w.Result].BaseKind==2);return wall!=null&&(FreeBlueprint(wall)||Craft.Researched.Contains(ResearchId(wall)));}
}
