using System;
using System.IO;
using System.Reflection;
// The raster is embedded in the game assembly, including exported builds.
static class ExpeditionWorldAtlas {
 public const double MinX=-2400,MinZ=-5000,Span=9000;
 static readonly int n;static readonly float[] road,water,nx,nz;static readonly byte[] district;
 static ExpeditionWorldAtlas(){
  using var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("AshenMeadow.WorldAtlas")??throw new InvalidOperationException("Embedded world atlas missing");
  using var r=new BinaryReader(stream);if(new string(r.ReadChars(4))!="AMAP")throw new InvalidDataException("World atlas header");
  n=r.ReadInt32();for(int i=0;i<4;i++)r.ReadSingle();int count=n*n;road=new float[count];water=new float[count];nx=new float[count];nz=new float[count];district=new byte[count];
  for(int i=0;i<count;i++){road[i]=r.ReadSingle();water[i]=r.ReadSingle();nx[i]=r.ReadSingle();nz[i]=r.ReadSingle();district[i]=r.ReadByte();}
 }
 static int Index(double x,double z)=>Math.Clamp((int)Math.Round((z-MinZ)/Span*(n-1)),0,n-1)*n+Math.Clamp((int)Math.Round((x-MinX)/Span*(n-1)),0,n-1);
 static double Sample(float[] values,double x,double z){double xx=Math.Clamp((x-MinX)/Span*(n-1),0,n-1.001),zz=Math.Clamp((z-MinZ)/Span*(n-1),0,n-1.001);int ix=(int)xx,iz=(int)zz;double u=xx-ix,v=zz-iz;int a=iz*n+ix;return (values[a]*(1-u)+values[a+1]*u)*(1-v)+(values[a+n]*(1-u)+values[a+n+1]*u)*v;}
 public static int District(double x,double z)=>district[Index(x,z)];
 public static double Road(double x,double z)=>Sample(road,x,z);
 public static double Water(double x,double z)=>Sample(water,x,z);
 public static (double X,double Z) Street(double x,double z){int i=Index(x,z);return(nx[i],nz[i]);}
 public static bool Dry(double x,double z,double margin=0)=>Water(x,z)>margin;
 public static bool Bridge(double x,double z)=>Water(x,z)<3&&Road(x,z)<1;
 static readonly double[] highwayU={0,.13,.26,.40,.49,.56,.60,.63,.66,.71,.78,.80,.83};
 static readonly double[] highwayV={.74,.69,.64,.61,.56,.48,.40,.31,.23,.16,.12,.07,0};
 public static double HighwayX(double z){double v=(z-MinZ)/Span;for(int i=0;i<highwayV.Length-1;i++)if(v<=highwayV[i]&&v>=highwayV[i+1])return MinX+Span*(highwayU[i]+(highwayU[i+1]-highwayU[i])*(highwayV[i]-v)/(highwayV[i]-highwayV[i+1]));return double.NaN;}
 public static bool Highway(double x,double z)=>double.IsFinite(HighwayX(z))&&Math.Abs(x-HighwayX(z))<65;
 public static string Name(double x,double z)=>District(x,z) switch{0=>"RIVER / LAKES",1=>"SUBURBS",2=>"WOODLAND",3=>"INDUSTRIAL DISTRICT",4=>"RIVER CITY",5=>"RURAL FARMLAND",6=>"CITY PARK",7=>"RAIL YARDS",8=>"QUARRY / HIGHLANDS",_=>"WOODLAND"};
}
partial class ExpeditionWorld {
 public int District(double x,double z)=>ExpeditionWorldAtlas.District(x,z);
 public bool ChunkIntersects(int cx,int cz)=>(cx+1)*64>MinX&&cx*64<MaxX&&(cz+1)*64>MinZ&&cz*64<MaxZ;
 public double StreetDistance(double x,double z)=>ExpeditionWorldAtlas.Road(x,z);
 public double WaterDistance(double x,double z)=>ExpeditionWorldAtlas.Water(x,z);
 public (double X,double Z) NearestStreet(double x,double z)=>ExpeditionWorldAtlas.Street(x,z);
 double AtlasRawHeight(double x,double z){
  int d=District(x,z);double amp=d is 3 or 4 or 7?2:d is 1 or 6?4:d==5?9:18;
  double h=15+amp*TerrainNoise(x,z,260,91)+amp*.25*TerrainNoise(x,z,100,217);
  // South-west quarry hills and the wooded southern rim in the supplied map.
  double u=(x-MinX)/(MaxX-MinX),v=(z-MinZ)/(MaxZ-MinZ);
  h+=155*Math.Exp(-((u-.115)*(u-.115)/.0018+(v-.805)*(v-.805)/.005));
  if(d==2&&v>.76)h+=30*TerrainSmooth((v-.76)/.20);
  return h;
 }
 double AtlasHeight(double x,double z){
  double h=AtlasRawHeight(x,z),water=WaterDistance(x,z),road=StreetDistance(x,z);
  if(water<35){double blend=1-TerrainSmooth(Math.Max(0,water)/35);double bed=WaterLevel-4-Math.Min(8,Math.Max(0,-water)*.06);h=h*(1-blend)+bed*blend;}
  if(road<20){var at=NearestStreet(x,z);double grade=AtlasRawHeight(at.X,at.Z);if(water<70)grade=WaterLevel+5+(grade-WaterLevel-5)*TerrainSmooth(Math.Max(0,water)/70);double blend=1-TerrainSmooth(Math.Max(0,road)/20);h=h*(1-blend)+grade*blend;}
  return h;
 }
 public string RegionName(double x,double z)=>ExpeditionWorldAtlas.Name(x,z);
 readonly System.Collections.Generic.Dictionary<(double,double,double),double> storyPlotX=new();
 public double SafeStoryX(double preferred,double z,double radius=28){
  var key=(preferred,z,radius);if(storyPlotX.TryGetValue(key,out double cached))return cached;int tier=Tier(preferred,z);
  for(int i=0;i<256;i++){double x=preferred+(i==0?0:((i+1)/2)*16*(i%2==0?-1:1));if(!InBlock(x,z)||Tier(x,z)!=tier||District(x,z)==7||WaterDistance(x,z)<radius+8||StreetDistance(x,z)<radius+3)continue;return storyPlotX[key]=x;}
  throw new InvalidOperationException("No safe story plot at "+preferred+","+z);
 }
}
