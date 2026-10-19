using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

sealed record WorldWork(int Site,int Tier,string Name,string Kind,int Town,string Reason,string Restore,string Strip){
 public static readonly WorldWork[] All={
  new(0,1,"The honesty pantry","pantry",0,"The empty baskets still have names on them. Someone used to leave food here for people too proud to ask.","Reopen the pantry: transfer one food and one water from Emberstead every 10 active minutes while both town reserves exceed four; shared capacity six each.","Take the last emergency reserve: six food, three water. The broken stand draws larger future packs nearby."),
  new(3,1,"Save the beans","garden",0,"The seedlings can feed strangers for a season. The frames would also keep your own fire going tonight.","Grow food: two shared provisions and two food for Emberstead every 10 active minutes, up to storage limits.","Dismantle the beds: sixteen timber and eight fiber. Only dry stalks and broken frames remain."),
  new(4,1,"A roof for the next person","refuge",-1,"A trail ledger has a blank line after every name. The last person wrote: Leave the roof. Somebody will need it.","Make a refuge: rest here, plan expeditions, and reduce future normal spawn pressure within 85m by 25%.","Take twelve timber and eight fiber. The marked refuge stays abandoned; future local packs grow."),
  new(7,2,"Water belongs to someone","water",0,"The pump can fill your bottles or supply the families waiting downhill. Fixing it means sharing the work.","Clean-water station: two shared water and two water for Emberstead every 10 active minutes.","Strip the pump: eight resin and eight alloy. The pipes are left dry; there is no clean-water service."),
  new(8,2,"Keep the west trail open","warning",0,"A watch roster ends halfway through a sentence. Someone needs to finish the warning network.","Restore the watch relay: future normal spawn pressure within 85m falls by 35%; a light marks the working tower.","Take six electronics and six fuel. The silent, stripped tower becomes a higher-pressure pocket."),
  new(10,2,"The names on the dressings","clinic",0,"There are no spare beds. There is a clean table, a working tap and a list of people who tried.","Open a field clinic: one shared bandage every 10 active minutes, plus treatment for water illness and slow recovery support.","Empty the treatment store: six bandages and two medical kits. The treatment bench remains dismantled."),
  new(13,2,"A spring worth waiting for","garden",0,"The greenhouse is labelled for next spring. Nothing outside suggests there will be one.","Replant the glasshouse: two shared provisions and two food for Emberstead every 10 active minutes.","Take twelve timber, twelve fiber and six resin. The remaining planting beds are lost."),
  new(14,3,"A road home","trail",1,"A painted arrow points toward the recovery camp. The delivery crew ran out of road before they ran out of hope.","Mark a relief trail: a persistent 48m path, route lights and 25% less future normal spawn pressure within 85m.","Strip the road crew's stores: sixteen alloy and eight fuel. The relief trail never gets marked."),
  new(16,3,"Somebody has to deliver it","depot",1,"The rail ledger records who needed food, not what they could pay. You could erase those destinations.","Run a relief depot: shared food and water, plus two food and two water for the recovery settlement every 10 active minutes.","Empty the reserve: eight food, six water and twelve alloy. The depot's delivery shelves stay empty."),
  new(17,3,"One more warning","warning",1,"A signal technician left a chalk arrow beside a broken fuse. Below it: Give them time to run.","Reconnect early warning: future normal spawn pressure within 85m falls by 35%; the signal lamps work again.","Salvage ten electronics and twelve alloy. Local future packs increase around the dark signal box."),
  new(21,4,"The route out","trail",2,"The evacuation map has been redrawn so many times the paper is almost transparent. A route can still help the people left.","Mark a relief corridor: a persistent path, lamps and 25% less future normal spawn pressure within 85m.","Take eighteen alloy and twelve electronics. The evacuation markers stay broken."),
  new(24,5,"What the city gets back","clinic",2,"The quarantine works have enough equipment for one proper treatment station. The hardware is valuable. So are the people still waiting.","Build an endgame treatment hub: shared dressings, water-illness treatment and recovery support; Mercy Row receives clean water regularly.","Take six bioalloy, eight electronics and four medical kits. A working treatment hub is permanently lost.")
 };
 public static WorldWork At(int site)=>All.FirstOrDefault(w=>w.Site==site);
 public int[] Cost(bool strip){var c=new int[7];if(strip){c[0]=2;c[1]=2;return c;}c[0]=Tier<4?8+Tier*2:4;c[1]=Tier<3?6:0;c[2]=4;if(Tier>=2)c[Tier+1]=4+Tier*2;if(Tier>=3)c[4]=12;if(Tier>=4)c[5]=8;return c;}
}
[DataContract] sealed class WorldWorkState {
 [DataMember] public int Site,Choice,Medicine;
 [DataMember] public double Remaining,Supply,ServiceAt,Noise;
 [DataMember] public bool Finished;
 [DataMember] public int[] Stock=new int[11];
 [DataMember] public string By="",Outcome="";
}
sealed partial class ExpeditionWorldChanges {
 [DataMember] public List<WorldWorkState> Works=new();
 public WorldWorkState Work(int site){Works??=new();var s=Works.FirstOrDefault(s=>s.Site==site);if(s==null){s=new(){Site=site};Works.Add(s);}s.Stock??=new int[11];if(s.Stock.Length!=11)Array.Resize(ref s.Stock,11);return s;}
}
partial class ExpeditionWorld {
 public double WorkPressure(double x,double z){double factor=1;if(Changes?.Works==null)return factor;foreach(var s in Changes.Works){if(!s.Finished)continue;var w=WorldWork.At(s.Site);if(w==null)continue;var p=ExpeditionContentSite.Place(this,s.Site);if(ExpeditionGame.Distance(x,z,p.X,p.Z)>85)continue;factor*=s.Choice==2?1.2:w.Kind=="warning"?.65:w.Kind is "trail" or "refuge"?.75:1;}return Math.Clamp(factor,.5,1.5);}
}
sealed partial class ExpeditionGame {
 public bool WorksRefuge(double x,double z)=>World.Changes.Works?.Any(s=>s.Finished&&s.Choice==1&&WorldWork.At(s.Site)?.Kind=="refuge"&&NearWork(s.Site,x,z,15))==true;
 public bool NearWork(int site,double x,double z,double radius=5){var p=ExpeditionContentSite.Place(World,site);return Distance(x,z,p.X,p.Z+5.5)<radius;}
 public string WorkReason(int site,bool strip){var w=WorldWork.At(site);if(w==null)return "Unknown project";var s=World.Changes.Work(site);if(!NearWork(site,Save.X,Save.Z))return "Return to this site's notice board";if(s.Choice!=0)return "This world's decision is already committed";if(Craft.UnlockedTier<w.Tier||Craft.BenchTier<Math.Max(0,w.Tier-1))return $"Needs tier {w.Tier} materials and workshop {Math.Max(0,w.Tier-1)}";var cost=w.Cost(strip);for(int i=0;i<7;i++)if(Craft.Materials[i]<cost[i])return "Needs "+cost[i]+" "+ResourceNames[i];return "Ready";}
 public bool BeginWork(int site,bool strip){string reason=WorkReason(site,strip);if(reason!="Ready"){Notice=reason;return false;}var w=WorldWork.At(site);return Change(()=>{var s=World.Changes.Work(site);var cost=w.Cost(strip);for(int i=0;i<7;i++)Craft.Materials[i]-=cost[i];s.Choice=strip?2:1;s.Remaining=strip?20:45;s.By=Life.SoulOwner;s.Outcome=strip?"Dismantling committed / stay nearby to finish.":"Restoration committed / stay nearby to finish.";WorldDecision("work/commit/"+site,w.Name+" / "+s.Outcome);Notice=s.Outcome;});}
 public void FinishWork(WorldWorkState s){if(s.Finished||s.Choice==0)return;var w=WorldWork.At(s.Site);if(w==null)return;s.Finished=true;s.Remaining=0;s.Outcome=s.Choice==1?w.Restore:w.Strip;
  var town=World.Changes.Settlements.FirstOrDefault(t=>t.Index==w.Town&&!t.Fallen);if(town!=null){town.Trust+=s.Choice==1?5:-6;town.LastEvent=w.Name+" / "+(s.Choice==1?"The restored site is part of our route now.":"The supplies were taken. There will be no service there.");}
  if(s.Choice==2){switch(s.Site){case 0:s.Stock[7]=6;s.Stock[8]=3;break;case 3:s.Stock[0]=16;s.Stock[2]=8;break;case 4:s.Stock[0]=12;s.Stock[2]=8;break;case 7:s.Stock[3]=8;s.Stock[4]=8;break;case 8:s.Stock[5]=6;s.Stock[10]=6;break;case 10:s.Stock[9]=6;s.Medicine=2;break;case 13:s.Stock[0]=12;s.Stock[2]=12;s.Stock[3]=6;break;case 14:s.Stock[4]=16;s.Stock[10]=8;break;case 16:s.Stock[7]=8;s.Stock[8]=6;s.Stock[4]=12;break;case 17:s.Stock[5]=10;s.Stock[4]=12;break;case 21:s.Stock[4]=18;s.Stock[5]=12;break;case 24:s.Medicine=4;s.Stock[6]=6;s.Stock[5]=8;break;}}
  else if(w.Kind=="trail"){var p=ExpeditionContentSite.Place(World,s.Site);for(int i=0;i<25;i++){double x=p.X+14,z=p.Z+8+i*2;if(!World.Playable(x,z)||Building.Pieces.Any(b=>Distance(x,z,b.X,b.Z)<5))continue;World.Changes.Path(new(){X=x,Z=z,Height=World.Height(x,z)});}}
  WorldDecision("work/finish/"+s.Site,w.Name+" / "+s.Outcome);Notice="WORLD CHANGED / "+w.Name;
 }
 public void SupplyWorks(double dt){if(dt<=0||World.Changes.Works==null)return;foreach(var s in World.Changes.Works.Where(s=>s.Finished&&s.Choice==1)){var w=WorldWork.At(s.Site);if(w==null)continue;s.Supply+=dt;if(s.Supply<600)continue;s.Supply%=600;var town=World.Changes.Settlements.FirstOrDefault(t=>t.Index==w.Town&&!t.Fallen);switch(w.Kind){
   case "garden":s.Stock[7]=Math.Min(6,s.Stock[7]+2);if(town!=null)town.Food=Math.Min(40,town.Food+2);break;
   case "water":s.Stock[8]=Math.Min(6,s.Stock[8]+2);if(town!=null)town.Water=Math.Min(40,town.Water+2);break;
   case "pantry":if(town!=null&&town.Food>4&&town.Water>4&&s.Stock[7]<6&&s.Stock[8]<6){town.Food--;town.Water--;s.Stock[7]++;s.Stock[8]++;}break;
   case "depot":s.Stock[7]=Math.Min(6,s.Stock[7]+1);s.Stock[8]=Math.Min(6,s.Stock[8]+1);if(w.Kind=="depot"&&town!=null){town.Food=Math.Min(40,town.Food+2);town.Water=Math.Min(40,town.Water+2);}break;
   case "clinic":s.Stock[9]=Math.Min(3,s.Stock[9]+1);if(w.Tier==5&&town!=null)town.Water=Math.Min(40,town.Water+2);break;
  }}}
 public bool CollectWork(int site){var w=WorldWork.At(site);if(w==null||!NearWork(site,Save.X,Save.Z))return false;var s=World.Changes.Work(site);if(!s.Finished||!s.Stock.Any(n=>n>0)&&s.Medicine==0){Notice="Nothing stored here yet";return false;}return Change(()=>{for(int i=0;i<7;i++)Craft.Materials[i]+=s.Stock[i];Life.Food+=s.Stock[7];Craft.Water+=s.Stock[8];Life.Bandages+=s.Stock[9];Craft.Fuel+=s.Stock[10];Life.Medicine+=s.Medicine;s.Medicine=0;Array.Clear(s.Stock);Notice="Collected the shared site stores";});}
 public bool UseWork(int site){var w=WorldWork.At(site);if(w==null||!NearWork(site,Save.X,Save.Z))return false;var s=World.Changes.Work(site);if(!s.Finished||s.Choice!=1||w.Kind is not ("refuge" or "clinic")){Notice="No working service here";return false;}if(World.Changes.Time<s.ServiceAt){Notice="Service restocking / "+(int)Math.Ceiling(s.ServiceAt-World.Changes.Time)+" active seconds";return false;}return Change(()=>{s.ServiceAt=World.Changes.Time+600;if(w.Kind=="clinic"){Life.Sickness=0;Life.RecoveryBoost=300;Notice="Clean treatment / water illness cleared; gradual recovery supported for five minutes";}else{Life.Energy=Math.Min(100,Life.Energy+20);Life.Morale=Math.Min(100,Life.Morale+5);Life.Comfort=300;Notice="A dry stop / energy and morale improved. F5 can plan your next expedition here.";}});}
}
