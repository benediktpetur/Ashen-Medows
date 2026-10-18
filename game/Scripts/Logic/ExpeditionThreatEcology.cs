using System;
using System.Linq;
[Flags] enum ThreatHabitat {None=0,Forest=1,Road=2,Ruins=4,Graves=8,Medical=16,Industrial=32,Checkpoint=64,Residential=128,Sewer=256,Contaminated=512}
static class ExpeditionThreatEcology {
 public static ThreatHabitat ForKind(string kind)=>kind switch{
  "mossback"=>ThreatHabitat.Forest,
  "howler"=>ThreatHabitat.Forest|ThreatHabitat.Road|ThreatHabitat.Ruins|ThreatHabitat.Residential,
  "bilecaster"=>ThreatHabitat.Sewer|ThreatHabitat.Medical|ThreatHabitat.Contaminated,
  "gravecrawler"=>ThreatHabitat.Graves|ThreatHabitat.Ruins,
  "roadbreaker"=>ThreatHabitat.Road|ThreatHabitat.Industrial,
  "cinder"=>ThreatHabitat.Industrial,
  "riot"=>ThreatHabitat.Checkpoint,
  "wirebound"=>ThreatHabitat.Industrial|ThreatHabitat.Checkpoint|ThreatHabitat.Sewer,
  "blooddrinker"=>ThreatHabitat.Medical|ThreatHabitat.Residential,
  "palehunter"=>ThreatHabitat.Forest|ThreatHabitat.Residential|ThreatHabitat.Ruins|ThreatHabitat.Sewer,
  _=>ThreatHabitat.None};
 public static ThreatHabitat Dungeon(string id){
  id=(id??"").ToLowerInvariant();
  if(id.Contains("sewer")||id.Contains("drain"))return ThreatHabitat.Sewer;
  if(id.Contains("clinic"))return ThreatHabitat.Medical|ThreatHabitat.Ruins;
  if(id.Contains("hospital"))return ThreatHabitat.Medical|(id.Contains("/deep")?ThreatHabitat.Contaminated:ThreatHabitat.Checkpoint)|ThreatHabitat.Ruins;
  if(id.Contains("mine"))return ThreatHabitat.Industrial|ThreatHabitat.Ruins;
  if(id.Contains("tower"))return ThreatHabitat.Residential|ThreatHabitat.Checkpoint|ThreatHabitat.Ruins;
  return ThreatHabitat.Forest|ThreatHabitat.Ruins;
 }
 public static ThreatHabitat Surface(ExpeditionWorld world,double x,double z){
  if(!world.Playable(x,z)||world.Tier(x,z)<2)return ThreatHabitat.None;
  int tier=world.Tier(x,z);double road=Math.Abs(x-world.RouteX(z));var flags=ThreatHabitat.None;
  if(world.Woodland(x,z)>=.7&&road>45)flags|=ThreatHabitat.Forest;
  if(tier>=3&&road<70)flags|=ThreatHabitat.Road;
  for(int i=0;i<ExpeditionContentSite.All.Length;i++){
   var p=ExpeditionContentSite.Place(world,i);if(ExpeditionGame.Distance(x,z,p.X,p.Z)>90)continue;
   flags|=ExpeditionContentSite.All[i].Theme switch{2=>ThreatHabitat.Medical,3 or 5 or 6=>ThreatHabitat.Industrial,4=>ThreatHabitat.Contaminated,8=>ThreatHabitat.Ruins,10=>ThreatHabitat.Industrial|ThreatHabitat.Contaminated,11=>ThreatHabitat.Checkpoint,_=>ThreatHabitat.None};
  }
  foreach(var d in ExpeditionDungeon.Nearby(world,x,z))if(ExpeditionGame.Distance(x,z,d.X,d.Z)<75)flags|=Dungeon(d.Id);
  if(ExpeditionGame.Distance(x,z,ExpeditionLastLetter.ChurchX(world),ExpeditionLastLetter.ChurchZ)<80)flags|=ThreatHabitat.Graves|ThreatHabitat.Ruins;
  if(ExpeditionGame.Distance(x,z,ExpeditionCheckpoint.X(world),ExpeditionCheckpoint.Z)<85)flags|=ThreatHabitat.Checkpoint;
  var square=ExpeditionCampaign.Site(world,1);if(ExpeditionGame.Distance(x,z,square.X,square.Z)<85)flags|=ThreatHabitat.Checkpoint;
  if(tier>=4){double bx=Math.Floor(x/32)*32+16,bz=Math.Floor(z/32)*32+16;for(int a=-1;a<=1;a++)for(int b=-1;b<=1;b++)if(world.BuildingCell(bx+a*32,bz+b*32))flags|=ThreatHabitat.Residential|ThreatHabitat.Ruins;}
  var fever=world.Changes?.FeverStory;if(fever!=null&&fever.GraveReady&&ExpeditionGame.Distance(x,z,fever.GraveX,fever.GraveZ)<65)flags|=ThreatHabitat.Graves;
  return flags;
 }
 public static string Description(string kind)=>kind switch{
  "mossback"=>"Dense woodland away from the main route",
  "howler"=>"Woodland, roads, abandoned homes and ruins",
  "bilecaster"=>"Medical sites, drains, sewers and contaminated works",
  "gravecrawler"=>"Grave sites and ruined buildings",
  "roadbreaker"=>"Highways, garages and industrial works",
  "cinder"=>"Garages, rail yards, utility sites and industrial works",
  "riot"=>"Checkpoints, civic security sites, hospital security and tower interiors",
  "wirebound"=>"Utility sites, industrial works, checkpoints and sewers",
  "blooddrinker"=>"Hospitals, medical sites and abandoned city housing",
  "palehunter"=>"City homes/ruins at night; suitable Tier 5 underground areas",
  _=>"Ordinary infected"};
 public static bool Eligible(ExpeditionThreat t,int tier,int night,ThreatHabitat habitat,bool underground)=>tier>=t.Min&&tier<=t.Max&&(!t.NightOnly||night>0||underground&&tier>=5)&&(ForKind(t.Kind)&habitat)!=0;
 public static string Choose(string id,int tier,int night,ThreatHabitat habitat,bool underground=false){
  if(tier<2)return null;uint hash=(uint)(id+"/threat-ecology").GetHashCodeStable();if(hash%5!=0)return null;
  var pool=ExpeditionThreat.All.Where(t=>Eligible(t,tier,night,habitat,underground)).ToArray();return pool.Length==0?null:pool[(hash>>8)%(uint)pool.Length].Kind;
 }
 public static string Stamp(string id,int tier,int night,ThreatHabitat habitat,bool underground=false)=>id+"/ecology/"+(Choose(id,tier,night,habitat,underground)??"native");
 public static bool Natural(string id)=>id.StartsWith("habitat/")||id.StartsWith("night/")||id.StartsWith("block1/")&&id.Contains("/vault/");
}
