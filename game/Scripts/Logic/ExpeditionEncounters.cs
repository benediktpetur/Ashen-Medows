using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
sealed class ExpeditionEncounter {
 public string Title,Clue,Action,After;public int Theme,Reward;
 public ExpeditionEncounter(string title,int theme,string clue,string action,string after,int reward){Title=title;Theme=theme;Clue=clue;Action=action;After=after;Reward=reward;}
 public static readonly ExpeditionEncounter[] All={
 new("The Last School Run",0,"The bus doors hang open. Backpacks and a lunchbox remain; there are no bodies. A teacher's note in the nearby farm shelter says she kept the children in the cellar when their bitten parents came calling. They are alive. She has not told them why their parents cannot come in.","Leave food by the cellar hatch / 1 food","A small voice counts the portions. The teacher whispers thank you. The hatch stays closed.",0),
 new("One More Ring",1,"A bell-ringer's notebook: Every morning I rang it. Someone will hear. If I am gone, please keep the promise. The chapel is empty now. There is a folded coat beneath the silent bell.","Ring the bell once","The note carries through the trees. No answer comes. You wait until the sound fades.",1),
 new("Borrowed Medicine",2,"The veterinary clinic smells of disinfectant. A note: I know animals, not people. She survived the operation. Now I need clean dressings. A patient rests behind a curtain; the vet's hands shake with exhaustion.","Leave two bandages / 2 bandages","The vet takes the dressings into the curtained room. For the first time, he says: We have a chance.",2),
 new("Till Death",1,"JUST MARRIED is fading on a parked car. Two wedding rings lie together at the altar. The last page: He was bitten at the reception. I will not leave him. We locked the door ourselves.","Set the rings together","You place both rings beneath the wilted bouquet. Two places, side by side.",1),
 new("Baby Girl Doe",2,"An empty crib rocks beneath a mobile. A chart reads Baby Girl Doe - No visitors. A nurse left a bundle of carefully folded blankets. The chart says the mother died in childbirth; no family ever arrived.","Stop the rocking crib","You steady the crib and fold the blanket. For a moment the room is still.",1),
 new("Prepared For Everything",3,"A bunker beneath an overgrown garden. An untouched ration tin and a single cartridge sit on the desk. The note: I prepared for everything. I did not prepare for being alone.","Take the sealed provisions","You take what you can carry. Behind the desk, the seated figure lifts its head. The note stays on the desk.",3),
 new("Twenty-Three Places",4,"Name cards line a mess-hall table. Twenty-three settings. A counselor's register has twenty-two children marked PRESENT. A locked freezer bears a warning: Do not open. I thought they would be safe. Leave them in peace. The children are represented only by their names.","Put the name cards in order","You straighten every card. At the head of the table, a chair scrapes across the floor. The counselor has noticed.",1),
 new("Sam Is Still Here",5,"Deflated balloons hang outside. HAPPY SEVENTH BIRTHDAY, SAM. Scratching comes from a boarded cupboard. A frightened boy answers: I hid. I could not get the door open. Please get my teacher.","Open the cupboard and leave food / 1 food","You free Sam and give him food. A marked route leads to the teacher's shelter; the journal records where he is waiting safely for help.",0),
 new("The Ferryman's Queue",6,"Two suitcases wait beside a dock. A timetable has every departure crossed out except the last. A note: I promised Mum I would wait here. If you see a blue boat, wave.","Raise the blue signal cloth","The cloth lifts above the dock. Someone coming by water might see it.",1),
 new("The Empty Bowl",7,"A dog's bowl sits outside a locked shed. The owner's diary: Every night I hear him outside. I keep filling it. I know it is not him. I just cannot stop.","Leave food in the bowl / 1 food","You fill the bowl. A kindness does not need a witness.",0),
 new("Weather Report",8,"A radio operator logged wind and rain long after the station went silent. The final broadcast reads: Clear skies tomorrow. If you are listening, I hope you get to see them.","Switch off the dead transmitter","The clicking stops. Outside, the wind moves through the aerial.",1),
 new("A Table For Two",4,"Two cups, one warm-looking ring on a cold table. A note: I will be late. Keep mine covered. Beneath it, in a different hand: I did. Every day.","Cover the second cup","You pull the small cloth over the cup, as someone did before you.",1),
 new("The Red Scarf",6,"A red scarf is tied to a fence at child height. A scratched message: Follow the red. I will leave another at the next turn. The next fence has only a torn thread.","Retie the scarf where it can be seen","The scarf becomes a signpost again. You record the trail in your journal.",1),
 new("The Night Shift",2,"A mechanic slept beneath the ambulance she was repairing. Her work order: One more belt, then we can collect them. The back seats are lined with clean blankets.","Salvage the spare repair parts","You keep the blankets folded and take the boxed parts from the bench.",4),
 new("Please Knock",5,"A locked door has a painted sign: PLEASE KNOCK. WE ARE STILL PEOPLE. On the reverse: They stopped knocking on Tuesday. No one answers now.","Knock three times","You knock, then wait. The silence is an answer, but you gave them the courtesy.",1),
 new("The Orchard Promise",7,"An orchard ledger lists jars promised to neighbours, with no prices. One line: Save the sweetest apples for Edith. She cannot climb the hill anymore.","Take a sealed jar","A label reads For Edith. You keep it with your provisions and remember the name.",3),
 new("The Last Lesson",0,"A makeshift classroom. Children practised writing their own addresses so someone could take them home. Their teacher corrected none of the spelling mistakes on the final day.","Collect the address book","You record the addresses. If their families are still out there, the names may matter.",1),
 new("Signal At The Crossing",8,"A railway signalman manually held the warning lever until the roads emptied. A chalk line: Just one more car. The line appears fourteen times.","Release the warning lever","The red lamp goes dark. There are no more cars coming today.",1),
 new("The Unsent Apology",5,"A sealed envelope lies beside an unplugged telephone. I was angry when you left. I wish I had spent that last minute differently. Please come home, even if you cannot forgive me.","Keep a copy of the letter","You fold the copy carefully. The original remains where its reader might find it.",1),
 new("The Little Library",7,"Books fill a roadside cabinet. A handwritten rule: Take what helps. Leave what you can. The last returned book has a bookmark that says I made it to chapter twelve.","Leave a provision for the next reader / 1 food","You place food beside a survival handbook. The little library has something new to offer.",0)
 };
 static readonly ConditionalWeakTable<ExpeditionWorld,(double X,double Z)[]> Places=new();
 static (double X,double Z)[] BuildPlaces(ExpeditionWorld w){var sites=new (double X,double Z)[All.Length];var portals=ExpeditionDungeon.Nearby(w,0,0);for(int i=0;i<sites.Length;i++){double z=-180-i*96;uint h=w.Hash(i+918,723);int side=i%2==0?-1:1;double x=w.RouteX(z)+side*(125+h%5*42);for(int step=0;step<8&&portals.Exists(d=>ExpeditionGame.Distance(d.X,d.Z,x,z)<42);step++)x+=side*35;sites[i]=(x,z);}return sites;}
 public static (double X,double Z) Place(ExpeditionWorld w,int index)=>Places.GetValue(w,BuildPlaces)[index];
 public static bool InLot(ExpeditionWorld w,double x,double z,double margin=0){foreach(var p in Places.GetValue(w,BuildPlaces))if(Math.Abs(x-p.X)<13+margin&&Math.Abs(z-p.Z)<12+margin)return true;return false;}
}
[DataContract] sealed class ExpeditionDroneEvent {
 [DataMember] public double Due=600,X,Z,Remaining;
 [DataMember] public int Serial,State,Waves; // 0 inactive, 1 beacon, 2 resolved
}
partial class ExpeditionWorldChanges {
 [DataMember] public List<int> EncounterChoices=new();
 [DataMember] public ExpeditionDroneEvent Drone=new();
}
