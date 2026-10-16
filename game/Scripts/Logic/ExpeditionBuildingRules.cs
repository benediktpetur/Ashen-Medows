// Ported engine-independent Expedition rules. Rendering and input live in Godot adapters.
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;


sealed class BuildDefinition {
 public int Tier=1,BaseKind;public string Tab="Buildings",Function="structure";public int[] Cost;public double Durability;
 public string Name;public double W,H,D,Weight,Capacity;public bool Ground;
 public BuildDefinition(string name,double w,double h,double d,double weight,double capacity,bool ground){Name=name;W=w;H=h;D=d;Weight=weight;Capacity=capacity;Ground=ground;}
 public static BuildDefinition[] All={
 new BuildDefinition("Foundation",3,.5,3,35,600,true),new BuildDefinition("Floor",3,.2,3,12,110,false),
 new BuildDefinition("Wall",3,2.5,.2,18,220,true),new BuildDefinition("Half wall",3,1.25,.2,10,150,true),
 new BuildDefinition("Pillar",.3,2.5,.3,8,260,true),new BuildDefinition("Beam",3,.3,.3,7,160,false),
 new BuildDefinition("Stairs",3,2.5,3,24,130,false),new BuildDefinition("Roof",3,.2,3,10,100,false)};
 static BuildDefinition(){for(int i=0;i<All.Length;i++)All[i].BaseKind=i;ExpeditionBlueprints.Extend();}
}
[DataContract] sealed class BuildPiece {
 [DataMember] public string Id;[DataMember] public bool Open;[DataMember] public double Health=-1,Charge,Progress;[DataMember] public int[] Stock;
 [DataMember] public int Kind;[DataMember] public double X,Y,Z,Yaw;
 [OnDeserializing] void Defaults(StreamingContext context){Health=-1;}
 public BuildDefinition Def{get{return BuildDefinition.All[Kind];}}
 public double CenterY{get{return Y+Def.H/2;}}
 public void Local(double x,double z,out double lx,out double lz){double a=Yaw*Math.PI/180,c=Math.Cos(a),s=Math.Sin(a);lx=(x-X)*c-(z-Z)*s;lz=(x-X)*s+(z-Z)*c;}
 public bool Contains(double x,double y,double z,double padding){double lx,lz;Local(x,z,out lx,out lz);return Math.Abs(lx)<Def.W/2+padding&&Math.Abs(lz)<Def.D/2+padding&&y>Y-padding&&y<Y+Def.H+padding;}
 public double Top(double x,double z){double lx,lz;Local(x,z,out lx,out lz);return Def.BaseKind==6?Y+Math.Max(0,Math.Min(1,(lz+Def.D/2)/Def.D))*Def.H:Y+Def.H;}
}
sealed class BuildReport {public bool Valid;public string Reason;public double[] Strength,Loads;public int[] Parent;}
sealed class ExpeditionBuilding {
 public List<BuildPiece> Pieces=new List<BuildPiece>();public Func<double,double,double> Ground;
 public ExpeditionBuilding(Func<double,double,double> ground){Ground=ground;}
 // Separating-axis tests use the actual rotated footprint, not a world-axis bounding box.
 public static bool Footprint(BuildPiece a,BuildPiece b,double tolerance){double[] angles={a.Yaw,a.Yaw+90,b.Yaw,b.Yaw+90};foreach(double angle in angles){double n=angle*Math.PI/180,nx=Math.Cos(n),nz=-Math.Sin(n),aa=a.Yaw*Math.PI/180,bb=b.Yaw*Math.PI/180;double ar=Math.Abs(nx*Math.Cos(aa)-nz*Math.Sin(aa))*a.Def.W/2+Math.Abs(nx*Math.Sin(aa)+nz*Math.Cos(aa))*a.Def.D/2,br=Math.Abs(nx*Math.Cos(bb)-nz*Math.Sin(bb))*b.Def.W/2+Math.Abs(nx*Math.Sin(bb)+nz*Math.Cos(bb))*b.Def.D/2;if(Math.Abs((b.X-a.X)*nx+(b.Z-a.Z)*nz)>ar+br+tolerance)return false;}return true;}
 static bool Touch(BuildPiece a,BuildPiece b){return a.Y<=b.Y+b.Def.H+.09&&b.Y<=a.Y+a.Def.H+.09&&Footprint(a,b,.09);}
 public bool Grounded(BuildPiece p){if(!p.Def.Ground)return false;double a=p.Yaw*Math.PI/180,c=Math.Cos(a),s=Math.Sin(a);for(int i=-1;i<=1;i+=2)for(int j=-1;j<=1;j+=2){double x=p.X+i*p.Def.W*.44*c+j*p.Def.D*.44*s,z=p.Z-i*p.Def.W*.44*s+j*p.Def.D*.44*c,g=Ground(x,z);if(p.Y>g+.15||p.Y<g-.65)return false;}return true;}
 public BuildReport Evaluate(List<BuildPiece> all){int n=all.Count;var r=new BuildReport{Valid=true,Reason="Supported",Strength=new double[n],Loads=new double[n],Parent=new int[n]};var done=new bool[n];for(int i=0;i<n;i++){r.Parent[i]=-1;r.Loads[i]=all[i].Def.Weight;if(Grounded(all[i]))r.Strength[i]=100;}
 // Highest remaining strength wins. Every edge loses strength, so parent chains cannot cycle.
 for(int step=0;step<n;step++){int best=-1;for(int i=0;i<n;i++)if(!done[i]&&r.Strength[i]>0&&(best<0||r.Strength[i]>r.Strength[best]))best=i;if(best<0)break;done[best]=true;for(int j=0;j<n;j++){if(done[j]||!Touch(all[best],all[j]))continue;double dx=all[j].X-all[best].X,dz=all[j].Z-all[best].Z;double loss=8+Math.Sqrt(dx*dx+dz*dz)*12+Math.Max(0,all[j].CenterY-all[best].CenterY)*12;double strength=r.Strength[best]-loss;if(strength>r.Strength[j]){r.Strength[j]=strength;r.Parent[j]=best;}}}
 for(int i=0;i<n;i++)if(r.Strength[i]<1){r.Valid=false;r.Reason="Needs a grounded foundation or a closer support";return r;}
 var order=new List<int>();for(int i=0;i<n;i++)order.Add(i);order.Sort((a,b)=>r.Strength[a].CompareTo(r.Strength[b]));foreach(int i in order)if(r.Parent[i]>=0)r.Loads[r.Parent[i]]+=r.Loads[i];for(int i=0;i<n;i++)if(r.Loads[i]>all[i].Def.Capacity){r.Valid=false;r.Reason=all[i].Def.Name+" overloaded ("+r.Loads[i].ToString("0")+" / "+all[i].Def.Capacity.ToString("0")+" load)";return r;}return r;}
 public BuildReport Placement(BuildPiece p){if(Math.Abs(p.X)+3>=7990||Math.Abs(p.Z)+3>=7990)return new BuildReport{Reason="Outside world boundary"};foreach(var b in Pieces)if(p.Y<b.Y+b.Def.H-.045&&b.Y<p.Y+p.Def.H-.045&&Footprint(p,b,-.045))return new BuildReport{Reason="Overlaps another building piece"};double lowest=Ground(p.X,p.Z);if(p.Y<lowest-.65)return new BuildReport{Reason="Buried too far into terrain"};var list=new List<BuildPiece>(Pieces);list.Add(p);return Evaluate(list);}
 public bool Remove(int index,out string reason){var copy=new List<BuildPiece>(Pieces);copy.RemoveAt(index);var r=Evaluate(copy);reason=r.Valid?"Removed": "Cannot remove: "+r.Reason;if(r.Valid)Pieces=copy;return r.Valid;}
 public bool Free(double x,double feet,double z){foreach(var p in Pieces){if(p.Open&&p.Def.Function=="door")continue;if(!p.Contains(x,feet+1,z,.30))continue;if(p.Def.BaseKind==6&&p.Top(x,z)<=feet+.38)continue;if(p.Y+p.Def.H<=feet+.35||p.Y>=feet+1.8)continue;return false;}return true;}
 public double Floor(double x,double z,double feet){double floor=Ground(x,z);foreach(var p in Pieces){double lx,lz;p.Local(x,z,out lx,out lz);if(Math.Abs(lx)>p.Def.W/2+.1||Math.Abs(lz)>p.Def.D/2+.1)continue;double top=p.Top(x,z);if(top<=feet+.38&&top>floor)floor=top;}return floor;}
 public double Ceiling(double x,double z,double oldHead){double ceiling=double.MaxValue;foreach(var p in Pieces)if(p.Contains(x,p.CenterY,z,.25)&&p.Y>=oldHead-.08)ceiling=Math.Min(ceiling,p.Y);return ceiling;}
 public static void Check(){var b=new ExpeditionBuilding((x,z)=>0);var foundation=new BuildPiece{Kind=0,Y=0};if(!b.Placement(foundation).Valid)throw new Exception("Ground foundation rejected");b.Pieces.Add(foundation);var floor=new BuildPiece{Kind=1,Y=.5};if(!b.Placement(floor).Valid)throw new Exception("Supported floor rejected");if(b.Placement(new BuildPiece{Kind=1,X=30,Y=10}).Valid)throw new Exception("Floating floor accepted");if(b.Placement(new BuildPiece{Kind=0,Y=20}).Valid)throw new Exception("Floating foundation accepted");if(b.Placement(new BuildPiece{Kind=0,Y=0}).Valid)throw new Exception("Overlap accepted");b.Pieces.Add(floor);string reason;if(b.Remove(0,out reason))throw new Exception("Unsafe support removal accepted");if(b.Floor(0,0,.4)<.69||b.Free(0,-.5,0))throw new Exception("Building collision error");var stairs=new ExpeditionBuilding((x,z)=>0);stairs.Pieces.Add(foundation);int built=0;for(int i=0;i<20;i++){var p=new BuildPiece{Kind=6,Z=(i+1)*3,Y=.5+i*2.5};if(!stairs.Placement(p).Valid)break;stairs.Pieces.Add(p);built++;}if(built<1||built>4)throw new Exception("Infinite stairway or no supported stairs");var overloaded=new List<BuildPiece>{foundation};for(int i=0;i<20;i++)overloaded.Add(new BuildPiece{Kind=0,Y=.5+i*.5});if(b.Evaluate(overloaded).Valid)throw new Exception("Unsupported tower accepted");var load=new List<BuildPiece>{new BuildPiece{Kind=4,Y=0}};for(int i=0;i<24;i++)load.Add(new BuildPiece{Kind=1,Y=2.5});if(b.Evaluate(load).Valid)throw new Exception("Overload accepted");var rotated=new BuildPiece{Kind=2,Yaw=45};double lx,lz;rotated.Local(1,1,out lx,out lz);if(Math.Abs(lx)>1e-6)throw new Exception("Rotation error");var roundtrip=Json.Read<List<BuildPiece>>(Json.Write(b.Pieces));if(roundtrip.Count!=b.Pieces.Count||roundtrip[1].Y!=b.Pieces[1].Y)throw new Exception("Building save roundtrip failed");if(!Footprint(new BuildPiece{Kind=1},new BuildPiece{Kind=1,X=3},.09)||Footprint(new BuildPiece{Kind=1},new BuildPiece{Kind=1,X=3},-.045))throw new Exception("Face connection error");}
}
