using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
sealed class ExpeditionContentSite {
 public string Name,Mode,Lore,Request;public int Tier,Theme,Town,Giver;
 public ExpeditionContentSite(string name,int tier,int theme,string mode,string lore,int town=-1,int giver=0,string request=""){Name=name;Tier=tier;Theme=theme;Mode=mode;Lore=lore;Town=town;Giver=giver;Request=request;}
 public static readonly ExpeditionContentSite[] All={
 new("Bracken Farm Stand",1,0,"cache","The price board says PAY WHAT YOU CAN. Someone crossed out the prices, then filled the basket anyway."),
 new("The Wayfarer's Stove",1,9,"rest","A kettle, two cups and a ledger of names. KEEP THE FIRE SMALL. LEAVE WOOD FOR THE NEXT PERSON."),
 new("South Meadow Post Office",1,8,"cache","Every undelivered envelope has been sorted by street. The carrier circled the farms she meant to reach first."),
 new("Hollow Creek Allotments",1,4,"repair","The seed trays are dry. A note asks the next owner to save the beans. They were somebody's father's favourite."),
 new("Miller's Picnic Shelter",1,9,"rest","Four painted stones hold down a map. The route north is labelled IF WE GET SPLIT UP, NOT IF WE DIE."),
 new("The Orchard Tool Shed",1,1,"cache","The owner laid the sharp tools on a clean cloth. The hammer still has his daughter's repair ribbon around its handle."),
 new("Coldwater Ranger Lodge",2,1,"cache","Tessa marked a seed case with Emberstead's name. The remaining supplies could feed one traveller, or restart a garden.",0,0,"Iris: Bring back the sealed seed case. Do not strip it for yourself. We need something worth planting."),
 new("The Broken Pump House",2,4,"repair","A brass impeller is wrapped in oilcloth. The farm cooperative's repair slip has been signed by everyone but its last mechanic.",0,1,"Mal: Recover the clean pump parts after restoring the station. Thin soup is one thing. Dirty water is another."),
 new("Pinewatch Radio Tower",2,5,"generator","Jules has been waiting for this relay's battery. A taped message repeats: Tell them the west trail is still open.",0,2,"Jules: Power the relay and recover its battery pack. Expect the generator to draw attention."),
 new("Moonrise Hunting Lodge",2,1,"night","Tessa's trail camera caught eyes around the porch after dark. Her survey case is still beneath the receiver.",0,3,"Tessa: Retrieve the night survey case. It only unlocks when the night receiver gets its signal."),
 new("Saint Wren's Woodland Clinic",2,2,"generator","A nurse kept the cold cabinet running long after her patients left. The labels are names, not bed numbers."),
 new("The Last Fisher's Jetty",2,6,"rest","A repaired rod waits beside two chairs. The second chair faces the trail, not the water."),
 new("Alder Charcoal Yard",2,3,"cache","A kiln ledger lists every bag given away that winter. The only unpaid account belongs to the man who owned the yard."),
 new("Foxglove Glasshouse",2,0,"repair","The roof is broken but the pots are labelled for spring. Someone expected to come back and argue about the tomatoes."),
 new("East Spur Recovery Garage",3,3,"cache","Gus's parts manifest is stapled to an unfinished work order: GET THE AMBULANCE MOVING BEFORE YOU FIX MY TRUCK.",1,0,"Gus: Recover the boxed drive parts. Taking loose scrap instead will not bring our truck back."),
 new("Redline Ambulance Depot",3,2,"generator","Nia left empty dressing tins on the recovery bench so she would not forget how many people were still waiting.",1,1,"Nia: Restart the cold store and bring its sealed dressings. Please do not trade away the case."),
 new("Rafi's Rail Locker",3,6,"cache","A rail worker's locker has Rafi's route book inside. On the first page: If you miss a delivery, people do not eat.",1,2,"Rafi: Bring the original route ledger. It tells us which shelters still have people in them."),
 new("Cinder Signal Box",3,5,"repair","Len's relay fuse sits beside a dead signal board. All but one line is marked NO REPLY.",1,3,"Len: Restore the signal box and recover the relay fuse. We need warning before the highway fills up."),
 new("Turnpike Roadhouse",3,9,"rest","The menu has been replaced by a list of people expected for supper. None of the names have been crossed out."),
 new("The Last Petrol Stop",3,3,"night","The owner hid the night-shift locker from daytime looters. A pencilled warning says the lights bring the hungry ones."),
 new("Saint Orla's Dispensary",4,2,"generator","Ada's request is tucked behind a patient chart: Keep the sterile case sealed. We can work with almost anything else.",2,0,"Ada: Restore power and bring the sterile surgical case. Its contents can treat a whole ward."),
 new("Civic Evacuation Control",4,11,"repair","Rook's street plan has four safe routes and twelve amendments. The newest amendment says THERE ARE NO SAFE ROUTES.",2,1,"Rook: Recover the evacuation maps after repairing the control panel. We need a route, not another promise."),
 new("June's Community Kitchen",4,9,"cache","Every table has been pushed together. June wrote the portions in pencil so she could change the number when someone new arrived.",2,2,"June: Bring the pantry inventory. We can combine the kitchens if we know what was left behind."),
 new("Eli's Rooftop Receiver",4,5,"night","Eli left a receiver beneath a sheet of plastic. The frequency card says HOME CALLS AT MIDNIGHT.",2,3,"Eli: Recover the night receiver's contact book. Someone may still be answering."),
 new("Harbour Quarantine Works",4,10,"generator","A shift board shows twenty-seven names and one signature for the final shift. The signature is almost too tired to read."),
 new("The Rooftop Canteen",4,7,"rest","A courtyard kitchen served the office blocks after the lifts stopped. A handwritten sign asks visitors to carry a meal to someone who cannot climb."),
 new("South Bank Records Office",4,8,"cache","The archive clerk copied the missing-person register by hand when the computers failed. The final page has blank lines for names she had not learned yet."),
 new("Ashfall Freight Terminal",4,6,"night","The freight clerk refused to open the relief containers without a destination. Someone finally painted one across the doors: ANYONE STILL ALIVE.")
 };
 static readonly ConditionalWeakTable<ExpeditionWorld,(double X,double Z)[]> Places=new();
 static readonly double[] Zs={145,60,-20,-105,-185,-255,-405,-515,-625,-745,-855,-965,-1075,-1150,-1290,-1370,-1450,-1530,-1610,-1690,-1830,-1900,-1970,-2040,-2110,-2180,-2250,-2330};
 static bool Reserved(ExpeditionWorld w,double x,double z){if(!w.Playable(x,z)||ExpeditionEncounter.InLot(w,x,z,24)||ExpeditionCampaign.InLot(w,x,z,24)||ExpeditionCrash.InLot(w,x,z,24)||ExpeditionCheckpoint.InLot(w,x,z,24)||ExpeditionBirthday.InLot(w,x,z,24)||ExpeditionLastLetter.InLots(w,x,z,24))return true;foreach(var p in ExpeditionDungeon.Nearby(w,0,0))if(ExpeditionGame.Distance(x,z,p.X,p.Z)<48)return true;foreach(var a in new[]{(Z:-470d,Offset:38d),(Z:-690d,Offset:-65d),(Z:-930d,Offset:55d),(Z:-790d,Offset:130d),(Z:-1460d,Offset:-105d),(Z:-2050d,Offset:130d),(Z:-2290d,Offset:130d),(Z:-780d,Offset:90d)})if(ExpeditionGame.Distance(x,z,w.RouteX(a.Z)+a.Offset,a.Z)<65)return true;return false;}
 static (double X,double Z)[] Build(ExpeditionWorld w){var sites=new (double X,double Z)[All.Length];for(int i=0;i<sites.Length;i++){int sign=i%2==0?-1:1;double z=Zs[i],x=w.RouteX(z)+sign*(85+w.Hash(i,641)%100);for(int a=0;a<160;a++){uint h=w.Hash(i+701,a+211);z=Zs[i]+(int)(h%61)-30;x=w.RouteX(z)+sign*(70+h%380);if(w.Tier(x,z)!=All[i].Tier||Reserved(w,x,z)||sites.Take(i).Any(s=>ExpeditionGame.Distance(x,z,s.X,s.Z)<52))continue;break;}sites[i]=(x,z);}return sites;}
 public static (double X,double Z) Place(ExpeditionWorld w,int i)=>Places.GetValue(w,Build)[i];
 public static bool InLot(ExpeditionWorld w,double x,double z,double margin=0){foreach(var p in Places.GetValue(w,Build))if(Math.Abs(x-p.X)<18+margin&&Math.Abs(z-p.Z)<27+margin)return true;return false;}
}
[DataContract] sealed class ContentSiteState {
 [DataMember] public int Index,DeadGuards;
 [DataMember] public bool Seen,Read,Recovered,Stripped,Delivered,Accepted,Repaired,Secured;
 [DataMember] public double PowerLeft,RestAt,NoiseAt;
 [DataMember] public string Outcome="";
}
[DataContract] sealed class ContentRoamingEvent {
 [DataMember] public int Serial,Kind,Stage,Dead;
 [DataMember] public double Health=90;
 [DataMember] public double Due=720,X,Z,GoalX,GoalZ,Remaining;
 [DataMember] public bool Traded;
 [DataMember] public string Outcome="";
}
sealed partial class ExpeditionWorldChanges {
 [DataMember] public List<ContentSiteState> FieldSites=new();
 [DataMember] public ContentRoamingEvent FieldEvent=new();
 public ContentSiteState FieldSite(int i){FieldSites??=new();var s=FieldSites.FirstOrDefault(s=>s.Index==i);if(s==null)FieldSites.Add(s=new(){Index=i});return s;}
}
