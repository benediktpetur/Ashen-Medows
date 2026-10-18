using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 void FeverArcAction(string op){
  string context=FeverOperation();bool meeting=context=="accept";bool house=context is "door" or "tend" or "notebook"||context=="boy"&&Fever.Stage==3;
  if(op=="refuse"&&meeting){Fever.Choice="refused";Fever.Outcome="You declined. Noah went to find another adult. Your camp and reputation were not penalized.";FeverStage(7,"NOAH: Okay. I'll ask someone else. / No reputation or resource penalty.");return;}
  if(op.StartsWith("accept")&&meeting){
   Fever.Disclosure=op=="accept-camp"&&game.Life.Camps.Any(c=>c.Seed==game.Save.Seed)?"camp":op=="accept-alone"?"alone":"name";
   if(Fever.Disclosure=="camp"){var camp=game.Life.Camps.First(c=>c.Seed==game.Save.Seed);Fever.SharedX=camp.X;Fever.SharedZ=camp.Z;}
   Fever.GivenName=coopActor>0&&coopActors.TryGetValue(coopActor,out var speaker)?speaker.Name:coopName;Fever.Owner=CommunityOwner;FeverStage(2,"NOAH: I'm Noah. Lily's at home. Dad left three days ago. Thank you. / He remembers what you told him.");return;
  }
  if(op=="resume"&&context=="resume"){Fever.Owner=CommunityOwner;FeverSay("NOAH: Stay close. Please.");return;}
  if(op=="boy"&&context=="boy"){game.Notice="Follow Noah / he waits when you fall behind";return;}
  if(op=="notebook"&&context=="notebook"){ReadFeverNotebook();return;}
  if(op=="tend"&&context=="tend"){Fever.BiteKnown=true;FeverSay("Lily grips a worn teddy. Beneath the cloth are bite marks. Noah looks away: Mum didn't mean it.");game.Persist();FeverSync();return;}
  if(op=="door"&&context=="door"){FeverSay("Claw marks inside the feeding slit. A sour smell. Moaning stops when Noah speaks, then starts again. The children have kept feeding her.");return;}
  if(!house||Fever.Stage!=3)return;
  if(op=="leave"){Fever.Choice="left";Fever.Outcome="You left the door closed. Noah sought another adult. He does not accuse you of killing his mother.";FeverStage(8,"You leave. Nobody takes your supplies or marks your reputation.");}
  else if(op=="reinforce"){
   if(game.Craft.Materials[0]<8||game.Craft.Materials[2]<6){game.Notice="Needs 8 timber and 6 fiber";return;}
   game.Change(()=>{game.Craft.Materials[0]-=8;game.Craft.Materials[2]-=6;Fever.Choice="reinforce";Fever.FledAt=game.World.Changes.Time+1200;Fever.Outcome="The stronger door bought time. Lily's untreated bite was still there. Noah eventually left with her.";Fever.Stage=8;});FeverSay("NOAH: You fixed it. So she'll be alright? / The barricade holds. It cannot cure Lily.");
  }else if(op is "open" or "truth"){
   Fever.Choice=op;if(op=="open"){ChooseFeverGrave();Fever.FledAt=Math.Max(1,game.World.Changes.Time);Fever.Outcome="Noah fled with Lily during the breach, whether or not you stayed to fight.";}FeverStage(5,op=="truth"?"You tell Noah his mother has turned. He refuses to believe you. The children retreat as you unbrace the door.":"You remove the bracing. The mother lunges. Noah pulls Lily out through the porch.");SpawnFeverMother();
  }
  game.Persist();FeverSync();
 }
 void FinishFeverArc(){
  if(Fever.Choice=="truth")Fever.Outcome="NOAH: You killed her. Don't say her name. / He takes Lily and runs. The truth did not make him trust you.";
  else if(Fever.Choice=="reinforce")Fever.Outcome="The barricade held. Lily remained feverish. Noah carried her into the woods at dawn; their mother stayed behind the reinforced door.";else Fever.Outcome="Noah got Lily out during the breach. He cannot forget the door opening. The teddy disappeared with them.";
  ChooseFeverGrave();if(Fever.FledAt<=0||Fever.Choice=="reinforce")Fever.FledAt=Math.Max(1,game.World.Changes.Time);Fever.FlightTime=Fever.CitySeen?14:0;FeverStage(9,Fever.Outcome);
 }
 void ChooseFeverGrave(){
  if(Fever.GraveX!=0||Fever.GraveZ!=0)return;
  for(int attempt=0;attempt<120;attempt++){uint h=game.World.Hash(attempt+3191,821);double z=-650-h%430,x=game.World.RouteX(z)+(h%2==0?-1:1)*(180+(h>>8)%500);string key=$"{ExpeditionWorld.Cell(x)}:{ExpeditionWorld.Cell(z)}";if(game.World.Tier(x,z)!=2||!game.World.Playable(x,z)||game.World.Changes.Explored.Contains(key)||!OutdoorFree(x,z))continue;Fever.GraveX=x;Fever.GraveZ=z;return;}
  // Search the remaining woodland cells rather than placing the grave in an explored lot.
  for(double z=-1100;z<-500;z+=64)for(double x=-1450;x<1450;x+=64)if(game.World.Tier(x,z)==2&&!game.World.Changes.Explored.Contains($"{ExpeditionWorld.Cell(x)}:{ExpeditionWorld.Cell(z)}")&&OutdoorFree(x,z)){Fever.GraveX=x;Fever.GraveZ=z;return;}
 }
 void TickFeverArc(double dt){
  if(Fever.ArcVersion==0)return;
  if(Fever.Choice=="reinforce"&&Fever.Stage==8&&game.World.Changes.Time>=Fever.FledAt)FinishFeverArc();
  if(Fever.Stage==9&&Fever.FlightTime<14){Fever.FlightTime+=dt;var target=Ground(FeverX+35,FeverZ-35);FeverMove(target,dt,3.2f,false);}
 }
 void ShowFeverChoices(bool meeting){
  var box=Panel(meeting?"JUST A FEVER / NOAH":"JUST A FEVER / THE DOOR");
  Text(box,meeting?"An eleven-year-old asks for help. His mother is sick behind a locked door. His father left three days ago. Lily is waiting at home. You can refuse without a penalty.":"Soup dries in the feeding slit. Scratches cross the frame. Lily is frightened and feverish, clutching a teddy. Noah insists Mum just needs medicine.\n"+(Fever.BiteKnown?"You have seen Lily's bite.":"You have not examined Lily's wrapped hand."));
  void Choice(string title,string op){Button(box,title,()=>{CloseMenu();if(!CoopRequest("fever",op))FeverAction(op);});}
  if(meeting){Choice("HELP / TELL HIM YOUR NAME","accept-name");Choice("HELP / SAY YOU TRAVEL ALONE","accept-alone");if(game.Life.Camps.Any(c=>c.Seed==game.Save.Seed))Choice("HELP / SHARE YOUR CAMP LOCATION","accept-camp");Choice("I CAN'T HELP / WALK AWAY","refuse");}
  else{Text(box,"Opening the door begins an adult infected fight. Telling the truth does not guarantee gratitude. Reinforcing the door costs supplies and buys time; it does not cure an infection.");Choice("OPEN THE DOOR","open");Choice("REINFORCE / 8 TIMBER + 6 FIBER","reinforce");Choice("CONFRONT THE MOTHER / TELL THE CHILDREN THE TRUTH","truth");Choice("LEAVE THE DOOR CLOSED / WALK AWAY","leave");}Button(box,"NOT YET",CloseMenu);
 }
 string FeverArcNotebook()=>FeverBefore+"\n\n"+(Fever.Outcome==""?"Lily has a threadbare teddy in her lap. Noah will remember the person who answers his plea.":Fever.Outcome)+"\n\n"+(Fever.GraveRead?"The woodland grave was Lily's. Noah's note: I promised I wouldn't leave her alone. I couldn't carry her any further. I kept her warm until she stopped asking for Mum. The bear can stay. I need both hands now.":"")+(Fever.Redemption?"\nMercy Row's clinic runs again. Noah accepted the supplies. He said: Help them. It doesn't make us alright.":"");
}
