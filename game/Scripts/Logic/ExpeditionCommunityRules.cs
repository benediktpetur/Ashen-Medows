using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
[DataContract] sealed partial class CampSurvivor {
 [DataMember] public string Id,Name,State="trapped",Job="wood",BedId="",Owner="",EscortOwner="",Pose="idle";
 [DataMember] public int Encounter;
 [DataMember] public double X,Y,Z,Yaw,Health=100,Injury,Morale=65,Work,DownTime,Treatment,ShotTime,ReturnTime;
 [DataMember] public bool Downed;
 public bool Member=>State=="member";
}
[DataContract] sealed partial class ExpeditionCommunity {
 [DataMember] public List<CampSurvivor> Survivors=new();
 [DataMember] public bool Established;
 [DataMember] public double X,Z,MealTime;
 [DataMember] public int Food,Water,Ammo;
 [DataMember] public int[] Stock=new int[7];
 public void Normalize(){Improvements=Math.Clamp(Improvements,0,3);Survivors??=new();if(Stock==null||Stock.Length!=7)Stock=new int[7];NormalizeSocial();foreach(var n in Survivors){n.Owner??="";n.EscortOwner??="";n.BedId??="";n.Job??="wood";}}
 public void Seed(ExpeditionWorld world){if(Survivors.Count!=0)return;double[] zs={-240,-560,-1080};string[] names={"Mara Holt","Ivar Reed","Nora Vale"};for(int i=0;i<3;i++){double z=zs[i],x=world.RouteX(z)+(i==1?-14:14);Survivors.Add(new(){Id=$"survivor/{world.Seed}/{i}",Name=names[i],Encounter=i,X=x,Z=z+2.6,Y=world.Height(x,z+2.6)+.1});}}
 public static bool Sheltered(BuildPiece bed,List<BuildPiece> pieces){if(bed==null||bed.Health==0)return false;bool roof=pieces.Any(p=>p.Health!=0&&(p.Def.BaseKind==7||p.Def.BaseKind==1)&&p.Y>bed.Y+1.7&&p.Y<bed.Y+4.8&&Covers(p,bed.X,bed.Z));int walls=pieces.Count(p=>p.Health!=0&&(p.Def.BaseKind==2||p.Def.Function is "window" or "door")&&ExpeditionGame.Distance(p.X,p.Z,bed.X,bed.Z)<4&&p.Y+p.Def.H>bed.Y+1.5);return roof&&walls>=2;}
 static bool Covers(BuildPiece p,double x,double z){p.Local(x,z,out double lx,out double lz);return Math.Abs(lx)<p.Def.W*.5&&Math.Abs(lz)<p.Def.D*.5;}
 public BuildPiece Bed(CampSurvivor n,List<BuildPiece> pieces)=>pieces.FirstOrDefault(p=>p.Id==n.BedId&&p.Def.Function=="bed"&&p.Health!=0);
 public BuildPiece FreeBed(List<BuildPiece> pieces,double x,double z)=>pieces.Where(p=>p.Def.Function=="bed"&&p.Health!=0&&Sheltered(p,pieces)&&ExpeditionGame.Distance(p.X,p.Z,x,z)<25&&(!Established||ExpeditionGame.Distance(p.X,p.Z,X,Z)<20)&&!Survivors.Any(n=>n.Member&&n.BedId==p.Id)).OrderBy(p=>ExpeditionGame.Distance(p.X,p.Z,x,z)).FirstOrDefault();
 public string RecruitReason(CampSurvivor n,ExpeditionGame game,double x,double z){if(n.Member)return "Already a community member";if(n.State!="rescued")return "Clear the rescue encounter first";if(n.Downed)return "Give first aid before recruiting";if(ExpeditionGame.Distance(n.X,n.Z,x,z)>4)return "Stand beside the survivor";if(Survivors.Count(s=>s.Member)>=3)return "Camp is full (three survivors)";if(FreeBed(game.Building.Pieces,x,z)==null)return "Needs a free survivor cot under a roof with two walls";if(game.Life.Food<3)return "Bring three food portions for their camp stores";return "Ready";}
 public void Recruit(CampSurvivor n,ExpeditionGame game,double x,double z){var bed=FreeBed(game.Building.Pieces,x,z);if(bed==null)return;n.State="member";Welcome(n,game.World.Changes.Time);n.BedId=bed.Id;n.Owner=n.EscortOwner="";n.Job="wood";n.Work=0;game.Life.Food-=3;Food+=3;if(!Established){Established=true;X=bed.X;Z=bed.Z;}}
 public void TickEconomy(double dt,List<BuildPiece> pieces,double workBonus=1){var members=Survivors.Where(n=>n.Member).ToArray();if(members.Length==0)return;MealTime+=dt;while(MealTime>=300){MealTime-=300;Food=Math.Max(0,Food-members.Length);}
  foreach(var n in members){var bed=Bed(n,pieces);bool housed=Sheltered(bed,pieces),home=ExpeditionGame.Distance(n.X,n.Z,X,Z)<20;double target=Math.Clamp((Food>0?65:25)+Improvements*3+(housed?15:-20)-n.Injury*.3+(n.GroupAcceptance/100*8)-(n.GriefRemaining>0?18:0),5,95);n.Morale+=Math.Clamp(target-n.Morale,-dt*.06,dt*.06);if(home&&Food>0&&housed&&!n.Downed){n.Injury=Math.Max(0,n.Injury-dt*.2);n.Health=Math.Min(100,n.Health+dt*.2);}if(n.Pose=="fireside"||n.Downed||!home||!string.IsNullOrEmpty(n.Owner)||Food<=0||!housed||n.Injury>=70)continue;
   n.Work+=dt*workBonus*(n.GriefRemaining>0?.8:1)*(.4+n.Morale*.006);if(n.Job=="wood"&&n.Work>=120){n.Work=0;Stock[0]=Math.Min(200,Stock[0]+2);}else if(n.Job=="supplies"&&n.Work>=180){n.Work=0;if(Water>0&&Stock[2]>0&&Food<=98){Water--;Stock[2]--;Food+=2;}}else if(n.Job=="guard")n.Work=0;
  }
 }
}
