using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 void ThreatEcologyChecks(){
  foreach(ThreatHabitat habitat in Enum.GetValues<ThreatHabitat>())for(int tier=1;tier<=5;tier++)for(int night=0;night<=1;night++)for(int i=0;i<250;i++){
   string id="habitat/ecology-check/"+i;string kind=ExpeditionThreatEcology.Choose(id,tier,night,habitat);if(kind==null)continue;
   MigrationChecks.Assert(ExpeditionThreatEcology.Eligible(ExpeditionThreat.Find(kind),tier,night,habitat,false),"Environmental pool never leaks an incompatible enemy");
   string stamped=ExpeditionThreatEcology.Stamp(id,tier,night,habitat);MigrationChecks.Assert(ExpeditionCampaign.Kind(stamped,tier,night)==kind,"Stamped kind is identical for host, replicas and rewards");
  }
  foreach(var spec in ExpeditionThreat.All){var habitat=ExpeditionThreatEcology.ForKind(spec.Kind);MigrationChecks.Assert(Enumerable.Range(0,3000).Any(i=>ExpeditionThreatEcology.Choose("habitat/find/"+i,spec.Min,1,habitat)==spec.Kind),"Every specialist still reachable in its habitat");}
  for(int i=0;i<500;i++)MigrationChecks.Assert(ExpeditionThreatEcology.Choose("night/day/"+i,5,0,ThreatHabitat.Ruins,false)!="palehunter","Tier 5 surface does not bypass night-only rules");
  MigrationChecks.Assert(Enumerable.Range(0,3000).Any(i=>ExpeditionThreatEcology.Choose("vault/deep/"+i,5,0,ThreatHabitat.Sewer,true)=="palehunter"),"Depths allow the hunter without surface night");
  foreach(int seed in new[]{1,431,2026}){
   var world=new ExpeditionWorld(seed);var forest=ExpeditionThreatEcology.Surface(world,world.RouteX(-600)+180,-600);MigrationChecks.Assert((forest&ThreatHabitat.Forest)!=0,"Dense off-route woodland marked as forest");
   var highway=ExpeditionThreatEcology.Surface(world,world.RouteX(-1450),-1450);MigrationChecks.Assert((highway&ThreatHabitat.Road)!=0,"Actual highway marked as road");
   foreach(int index in new[]{10,20}){var p=ExpeditionContentSite.Place(world,index);MigrationChecks.Assert((ExpeditionThreatEcology.Surface(world,p.X+50,p.Z)&ThreatHabitat.Medical)!=0,"Actual clinic/dispensary has medical surroundings");}
   var checkpoint=ExpeditionContentSite.Place(world,21);MigrationChecks.Assert((ExpeditionThreatEcology.Surface(world,checkpoint.X+50,checkpoint.Z)&ThreatHabitat.Checkpoint)!=0,"Actual evacuation control marked as checkpoint");
  }
  MigrationChecks.Assert(ExpeditionThreatEcology.Dungeon("city-sewer")==ThreatHabitat.Sewer&&(ExpeditionThreatEcology.Dungeon("city-hospital")&ThreatHabitat.Medical)!=0&&(ExpeditionThreatEcology.Dungeon("forest-mine")&ThreatHabitat.Industrial)!=0,"Real dungeon IDs select themed pools");
  string worm="brood/worm/block1/431/vault/forest-mine/guard/0/ecology/native/1";MigrationChecks.Assert(ExpeditionThreat.SpawnKey(worm)==worm&&!ExpeditionThreatEcology.Natural(worm),"Brood children retain separate IDs and never inherit parent habitat tags");
  string key="habitat/431/legacy-defeated/0";game.Skills.Defeated.Add(key);int count=infected.Count;AddEnemy(key,0,-7,2);MigrationChecks.Assert(infected.Count==count,"Saved legacy kills block new tagged spawns");
  string reward="habitat/431/ecology-reward/0",tagged=reward+"/ecology/bilecaster";game.Kill(tagged,2,0);int resin=game.Craft.Materials[3];game.Kill(reward+"/ecology/howler",2,0);MigrationChecks.Assert(game.Craft.Materials[3]==resin&&game.Skills.Defeated.Contains(reward),"Changing habitat cannot duplicate a saved slot's loot");
  GD.Print("PASS / enemy ecology: environmental exclusion matrix, reachable pools, tier/night/depth gates, actual woodland/highway/medical/checkpoint sites across seeds, dungeon themes, canonical old-save kills and duplicate loot protection");
 }
}
