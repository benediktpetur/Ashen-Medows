using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

[DataContract] sealed class ExpeditionJourney {
 [DataMember] public int Seed,Site=-1;
 [DataMember] public bool Active,Reached,Recovered;
 [DataMember] public double HomeX,HomeZ,Started,Travelled,LastX,LastZ;
 [DataMember] public string Outcome="No expedition planned.";
}
sealed partial class ExpeditionLife {
 [DataMember] public ExpeditionJourney Journey=new();
 [DataMember] public List<string> JourneyLog=new();
 [DataMember] public double Wetness,Chill;
}
sealed partial class ExpeditionCommunity {
 [DataMember] public int Improvements;
}
sealed partial class ExpeditionGame {
 // Fixtures opt in explicitly; dedicated servers and guests use the shared world's time.
 public bool ExpeditionEffects,WeatherSheltered,WeatherHeated;
 public int WeatherAt(double time){int period=(int)Math.Floor(Math.Max(0,time)/720);return period==0?0:(int)(World.Hash(period+4301,Save.Seed+861)%10) switch{<4=>0,<7=>1,<9=>2,_=>3};}
 public int Weather=>WeatherAt(World.Changes.Time);
 public static readonly string[] WeatherNames={"CLEAR","RAIN","FOG","COLD WIND"};
 public double WeatherHearing=>!ExpeditionEffects?1:Weather==1?.65:Weather==3?.85:1;
 public double WeatherSight=>!ExpeditionEffects?1:Weather==2?.6:Weather==1?.85:1;
 public string Forecast=>$"{WeatherNames[Weather]} / next {WeatherNames[WeatherAt(World.Changes.Time+720-World.Changes.Time%720)]} in {(720-World.Changes.Time%720)/60:0.0} active min";
 public double ClothingWarmth=>Math.Min(.7,Profile.Items.Where(i=>IsGearEquipped(i.Id)).Sum(i=>i.Warmth)+GearArmor/220.0+(Life.SurvivalTech.Contains(2)?.15:0));
 public void NormalizeJourneys(){Life.Journey??=new();Life.JourneyLog??=new();Life.Wetness=double.IsFinite(Life.Wetness)?Math.Clamp(Life.Wetness,0,100):0;Life.Chill=double.IsFinite(Life.Chill)?Math.Clamp(Life.Chill,0,100):0;World.Changes.Community.Improvements=Math.Clamp(World.Changes.Community.Improvements,0,3);}
 public void TickWeather(double dt){if(!ExpeditionEffects)return;
  bool rain=Weather==1&&!WeatherSheltered,cold=World.Tier(Save.X,Save.Z)>=2&&(Weather==3||Night&&Life.Wetness>40);
  Life.Wetness=Math.Clamp(Life.Wetness+dt*(rain?.35:WeatherHeated?-1:WeatherSheltered?-.5:-.12),0,100);
  Life.Chill=Math.Clamp(Life.Chill+dt*(WeatherHeated?-.9:WeatherSheltered?-.35:cold?.14*(1-ClothingWarmth)*(Life.Wetness>50?1.8:1):-.16),0,100);
  if(Life.Chill>60)Life.Energy=Math.Max(0,Life.Energy-dt/75);
 }
 public bool JourneyRefuge(double x,double z)=>WorksRefuge(x,z)||Distance(x,z,ExpeditionWorld.SpawnX,ExpeditionWorld.SpawnZ)<35||Life.Camps.Any(c=>c.Seed==Save.Seed&&Distance(x,z,c.X,c.Z)<20)||World.Changes.Community.Established&&Distance(x,z,World.Changes.Community.X,World.Changes.Community.Z)<20||World.Changes.Settlements.Any(t=>!t.Fallen&&t.Index<3&&t.Trust>=0&&Distance(x,z,t.X,t.Z)<25);
 public bool BeginJourney(int site){if(site<0||site>=ExpeditionContentSite.All.Length)return false;if(!JourneyRefuge(Save.X,Save.Z)){Notice="Plan departures from the trailhead, a sleeping bag, your camp or a friendly settlement.";return false;}if(Life.Journey?.Active==true){Notice="Finish or abandon your current expedition first.";return false;}return Change(()=>{Life.Journey=new(){Seed=Save.Seed,Site=site,Active=true,HomeX=Save.X,HomeZ=Save.Z,LastX=Save.X,LastZ=Save.Z,Started=World.Changes.Time,Outcome="Outbound / "+ExpeditionContentSite.All[site].Name};Notice="Expedition planned / F5 checks supplies, route and return.";});}
 public void EndJourney(string reason){var j=Life.Journey;if(j?.Active!=true)return;j.Active=false;j.Outcome=reason;Life.JourneyLog.Add(ExpeditionContentSite.All[Math.Clamp(j.Site,0,ExpeditionContentSite.All.Length-1)].Name+" / "+reason);while(Life.JourneyLog.Count>8)Life.JourneyLog.RemoveAt(0);}
 public void TickJourney(){var j=Life.Journey;if(j?.Active!=true||!ExpeditionEffects)return;if(j.Seed!=Save.Seed){EndJourney("Interrupted / travelled to another world.");return;}if(j.Site<0||j.Site>=ExpeditionContentSite.All.Length){EndJourney("Invalid destination.");return;}double moved=Distance(j.LastX,j.LastZ,Save.X,Save.Z);if(moved<15)j.Travelled+=moved;j.LastX=Save.X;j.LastZ=Save.Z;var at=ExpeditionContentSite.Place(World,j.Site);if(Distance(at.X,at.Z,Save.X,Save.Z)<28){j.Reached=true;j.Outcome="Destination reached / search or turn back.";}if(j.Reached&&j.Travelled>50&&Distance(Save.X,Save.Z,j.HomeX,j.HomeZ)<20){EndJourney(j.Recovered?"Returned with supplies.":"Scouted and returned safely.");Notice=j.Outcome;}}
 public void JourneyLoot(int site){if(Life.Journey?.Active==true&&Life.Journey.Seed==Save.Seed&&Life.Journey.Site==site)Life.Journey.Recovered=true;}
 public string JourneyChecklist(int site){var p=ExpeditionContentSite.Place(World,site);double distance=Distance(Save.X,Save.Z,p.X,p.Z)*2;bool planned=Life.Journey?.Active==true&&Life.Journey.Seed==Save.Seed&&Life.Journey.Site==site;if(planned)distance=Life.Journey.Reached?Distance(Save.X,Save.Z,Life.Journey.HomeX,Life.Journey.HomeZ):Distance(Save.X,Save.Z,p.X,p.Z)+Distance(p.X,p.Z,Life.Journey.HomeX,Life.Journey.HomeZ);double seconds=distance/4.8*1.35;double arrive=(Hour+seconds/150)%24;return $"Approx. {(planned?"remaining route":"round trip")} {distance:0}m / {seconds/60:0.0} active minutes before searches or detours.\nReturn estimate {arrive:00}:00 / {Forecast}\n"+string.Join("\n",new[]{Life.Food>=2?"Food packed":"PACK 2 FOOD",Craft.Water>=2?"Clean water packed":"PACK 2 CLEAN WATER",Life.Bandages>=2?"Dressings packed":"PACK 2 BANDAGES",Life.Energy>=45?"Rested enough":"REST FIRST / low energy",Life.Wounds.Any(w=>!w.Patched)?"PATCH OPEN WOUNDS BEFORE LEAVING":null,ExpeditionContentSite.All[site].Mode=="generator"&&Craft.Fuel<2?"PACK 2 GENERATOR FUEL":null,IsBow(Craft.Weapon)&&Craft.ArrowsOf(Craft.ArrowType)<12?"LOW ARROWS / craft a bundle":null,IsGun(Craft.Weapon)&&Craft.Ammo+Craft.Reserve<Magazine*2?"LOW AMMUNITION":null,Weather==3?"Cold wind / armor and shelter reduce exposure":null,arrive>=20||arrive<6?"Night return / consider a shorter trip":null}.Where(s=>s!=null));}
}
