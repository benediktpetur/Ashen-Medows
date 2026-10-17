using Godot;
using System;
using System.Linq;

public partial class MigrationScene {
 ExpeditionLastLetter LastLetter=>game.World.Changes.LastLetter;
 bool letterFixture;bool LetterEnabled=>!((testing||coopCheckRole!=null)&&!letterFixture);
 Node3D letterChurch,letterHome,letterDyingMan,letterBrother,letterWife,letterBrotherBody;
 double letterChurchFloor,letterHomeFloor;
 CanvasLayer letterLayer;Label letterCaption,letterObjective;
 double LetterChurchX=>ExpeditionLastLetter.ChurchX(game.World);double LetterHomeX=>ExpeditionLastLetter.HomeX(game.World);
 bool InLetterLot(double x,double z)=>ExpeditionLastLetter.InLots(game.World,x,z);
 Vector3 LetterChurchPoint(float x,float y,float z)=>new((float)LetterChurchX+x,(float)letterChurchFloor+y,(float)ExpeditionLastLetter.ChurchZ+z);
 Vector3 LetterHomePoint(float x,float y,float z)=>new((float)LetterHomeX+x,(float)letterHomeFloor+y,(float)ExpeditionLastLetter.HomeZ+z);
 bool LetterNodeLive(Node3D n)=>GodotObject.IsInstanceValid(n)&&n.IsInsideTree();
 void SpawnLastLetter(){if(!LetterEnabled||dungeon!=null)return;
  if(NearActiveSurface(Ground(LetterChurchX,ExpeditionLastLetter.ChurchZ),220)&&uniqueObjects.Add("story/letter/church")){
   letterChurchFloor=LetterFloor(LetterChurchX,ExpeditionLastLetter.ChurchZ,6,12);
   letterChurch=new Node3D{Name="SaintBrigidsChurch",Position=LetterChurchPoint(0,0,0)};worldRoot.AddChild(letterChurch);props.Add(new(){Id="story/letter/church",Kind=22,Node=letterChurch,Position=letterChurch.Position});DressLetterChurch();
  }
  if(NearActiveSurface(Ground(LetterHomeX,ExpeditionLastLetter.HomeZ),220)&&uniqueObjects.Add("story/letter/home")){
   letterHomeFloor=LetterFloor(LetterHomeX,ExpeditionLastLetter.HomeZ,4,5);
   letterHome=new Node3D{Name="SamuelAndRuthsSafehouse",Position=LetterHomePoint(0,0,0)};worldRoot.AddChild(letterHome);props.Add(new(){Id="story/letter/home",Kind=23,Node=letterHome,Position=letterHome.Position});DressLetterHome();
  }
 }
 double LetterFloor(double x,double z,double halfX,double halfZ)=>Enumerable.Range(0,3).SelectMany(a=>Enumerable.Range(0,3).Select(b=>game.World.Height(x-halfX+a*halfX,z-halfZ+b*halfZ))).Max()+.16;
 void LetterSync(){if(CoopHost&&coopActors.Count>0)Rpc(nameof(CoopWorldUpdate),Json.Write(WorldSnapshot()));}
 void LetterStage(int stage,string line=""){LastLetter.Stage=stage;LastLetter.Line=line;LastLetter.LineTime=12;game.Persist();LetterSync();}
 void TickLastLetter(double dt){if(!LetterEnabled||CoopClient||dungeon!=null)return;LastLetter.LineTime=Math.Max(0,LastLetter.LineTime-dt);
  if(LastLetter.Stage!=2)return;LastLetter.Elapsed+=dt;var people=MillPlayers();
  if(people.All(p=>Distance(p.Body.Position,Ground(LetterHomeX,ExpeditionLastLetter.HomeZ))>45))LastLetter.Departed=true;
  if(LastLetter.Elapsed>=180&&LastLetter.Departed&&people.All(p=>Distance(p.Body.Position,Ground(LetterHomeX,ExpeditionLastLetter.HomeZ))>70&&Distance(p.Body.Position,Ground(LetterChurchX,ExpeditionLastLetter.ChurchZ))>70))LetterStage(3);
 }
 string LetterOperation(){if(!LetterEnabled||dungeon!=null)return "";
  if(LetterNodeLive(letterChurch)&&Math.Abs(player.Position.Y-letterChurchFloor)<1.8){
   if(Distance(player.Position,LetterChurchPoint(-3,0,-8))<2.3)return LastLetter.Stage==0?"accept":"man";
   if(LastLetter.Stage>=3&&Distance(player.Position,LetterChurchPoint(2.6f,0,-1.8f))<2.3)return "body";
  }
  if(LetterNodeLive(letterHome)&&Math.Abs(player.Position.Y-letterHomeFloor)<1.8){
   if(LastLetter.Stage>=3&&Distance(player.Position,LetterHomePoint(0,0,1))<2.4)return "empty";
   if(LastLetter.Stage<3&&Distance(player.Position,LetterHomePoint(-2,0,0))<2.5)return LastLetter.Stage==1?"deliver":"brother";
   if(LastLetter.Stage<3&&Distance(player.Position,LetterHomePoint(2.3f,0,-1.2f))<2.5)return LastLetter.Stage==1?"deliver":"wife";
  }return "";
 }
 string LetterPrompt()=>LetterOperation() switch{"accept"=>"F / HEAR THE DYING MAN / TAKE HIS LETTER","man"=>LastLetter.Stage>=2?"F / EXAMINE DANIEL'S COT":"F / REREAD DANIEL'S LETTER","brother"=>"F / SPEAK TO SAMUEL","deliver"=>"F / DELIVER DANIEL'S LETTER","wife"=>"F / SPEAK TO RUTH","body"=>"F / EXAMINE THE MAN IN THE PEW","empty"=>"F / SEARCH THE QUIET SAFEHOUSE",_=>""};
 bool LetterInteract(){string op=LetterOperation();if(op=="")return false;if(!CoopRequest("lastletter",op))LetterAction(op);return true;}
 void LetterAction(string op){if(CoopClient||op!=LetterOperation())return;
  switch(op){
   case "accept":LetterStage(1,"DANIEL: My brother. Samuel. Across town, west of here. Please take this to him.");ReadLastLetter(0);break;
   case "man":if(LastLetter.Stage<2)ReadLastLetter(0);else{LastLetter.Line="Daniel is still. The blanket has been pulled up to his shoulders.";LastLetter.LineTime=9;game.Notice=LastLetter.Line;}break;
   case "brother":LastLetter.Line=LastLetter.Stage==1?"SAMUEL: Daniel sent you? Ruth, you should read this. We thought you knew what he'd done.":LastLetter.Stage==0?"SAMUEL: Just the two of us here. We keep the door open for people who need it.":"Samuel watches Ruth fold the letter. Neither of them says anything more.";LastLetter.LineTime=12;game.Notice=LastLetter.Line;break;
   case "wife":LastLetter.Line=LastLetter.Stage==0?"RUTH: Samuel found me outside that night. I live here now.":"RUTH: Thank you. Please leave us now.";LastLetter.LineTime=10;game.Notice=LastLetter.Line;break;
   case "deliver":LastLetter.Elapsed=0;LastLetter.Departed=false;LetterStage(2,"RUTH: Thank you for bringing it. ...I'd like you to leave now, please.");ReadLastLetter(1);break;
   case "body":if(LastLetter.Stage==3)LetterStage(4);ReadLastLetter(2);break;
   case "empty":ReadLastLetter(3);break;
  }game.Persist();LetterSync();
 }
 static readonly string[] LastLetterTitles={"THE DOOR I CLOSED / DANIEL'S LETTER","THE DOOR I CLOSED / DELIVERY","THE DOOR I CLOSED / RETURN TO THE CHURCH","THE DOOR I CLOSED / EMPTY SAFEHOUSE"};
 const string DanielLetter="Samuel,\n\nI told you Ruth was taken in the street. That was a lie.\n\nShe went out to help a woman calling for water. I watched from the window. When she came back, I put the bolt across. She wasn't bitten. She wasn't sick. I knew it was her.\n\nWe had six tins left. I counted them while she knocked. I told myself another mouth meant we would both starve. She said my name until her voice gave out. I sat on the other side of the door and ate.\n\nDon't make this into fear. I knew what I was doing.\n\nI'm in Saint Brigid's now. I won't last much longer. I don't expect you to forgive me. I just couldn't leave you with the lie.\n\nDaniel";
 string LastLetterText(int page)=>page switch{
  0=>$"Daniel lies on a cot beneath the church windows. He presses an envelope into your hand.\n\n'My brother, Samuel. He has a place across town. Take it to him. I need him to know.'\n\nThe envelope is unsealed.\n\n{DanielLetter}\n\nAddress: Samuel's safehouse / west side / X {(int)LetterHomeX}, Z {(int)ExpeditionLastLetter.HomeZ}. Marked on your map [M].",
  1=>"Samuel is living with Ruth. She survived the night Daniel locked her outside.\n\nHe looks toward her when you mention the letter.\n\nRuth reads slowly, then reads the final lines again. She folds the paper along its old crease.\n\n'Thank you for bringing it.'\n\nA long silence.\n\n'I'd like you to leave now, please.'\n\nSamuel does not look at you as you go.",
  2=>"The man in the pew is Samuel. The same dark blue coat. The same repaired cuff. He is dead.\n\nDaniel is still on the cot at the back of the church.\n\nThe letter lies folded beside Samuel's hand. Someone has straightened the candles.\n\nThere is no note. Nothing here tells you what happened.\n\nRuth is not here. No one can tell you where she went.",
  _=>"Two bowls are set on the table. A chair has been pushed neatly back in.\n\nSamuel's coat is gone. So is Ruth. The letter is no longer here.\n\nThe door stands open.\n\nThere is no forwarding address."};
 void ReadLastLetter(int page){if(page<0||page>3)return;game.Change(()=>{string id="last-letter/"+page;if(!game.Profile.Journal.Seen.Contains(id))game.Profile.Journal.Seen.Add(id);});if(CoopHost&&coopActor>0)RpcId(coopActor,nameof(CoopLastLetterPage),page);else ShowLastLetter(page);}
 void ShowLastLetter(int page){if(page<0||page>3)return;var b=Panel(LastLetterTitles[page]);var scroll=new ScrollContainer{CustomMinimumSize=new(880,440)};b.AddChild(scroll);var text=new VBoxContainer();scroll.AddChild(text);Text(text,LastLetterText(page));Button(b,"BACK",CloseMenu);}
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]void CoopLastLetterPage(int page){if(CoopClient)ShowLastLetter(page);}
}
