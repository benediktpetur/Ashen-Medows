using Godot;
using System;
using System.Linq;

public partial class MigrationScene {
 const double FeverZ=-470,FeverMeetZ=-425;
 const string FeverJournal="fever/remembered";
 double FeverX=>game.World.RouteX(FeverZ)+38;
 string FeverMotherId=>$"story/fever/mother/{game.Save.Seed}";
 ExpeditionFeverStory Fever=>game.World.Changes.FeverStory;
 Node3D feverHouse,feverDoor,feverBoy,feverSister,feverMotherVisual,feverAftermath;
 double feverFloor,feverRattle;int feverSeed=int.MinValue;bool feverFixture;
 CanvasLayer feverLayer;ColorRect feverFade;Label feverCaption,feverObjective;
 bool FeverEnabled=>!((testing||coopCheckRole!=null)&&!feverFixture);
 bool InFeverLot(double x,double z)=>Math.Abs(x-FeverX)<7&&z>FeverZ-7&&z<FeverZ+23;
 Vector3 FeverPoint(float x,float y,float z)=>new((float)FeverX+x,(float)feverFloor+y,(float)FeverZ+z);
 Vector3 FeverBoyPoint=>new((float)Fever.X,(float)Fever.Y,(float)Fever.Z);
 bool FeverCinematic=>Fever.ArcVersion==0&&Fever.Stage==4&&dungeon==null&&Distance(player.Position,FeverPoint(0,0,0))<22;
 void SetFeverBoy(Vector3 p){Fever.X=p.X;Fever.Y=p.Y;Fever.Z=p.Z;}
 void FeverSay(string text){Fever.Line=text;Fever.LineTime=9;}
 void FeverSync(){if(CoopHost&&coopActors.Count>0)Rpc(nameof(CoopWorldUpdate),Json.Write(WorldSnapshot()));}
 void FeverStage(int stage,string line){Fever.Stage=stage;Fever.Timer=0;FeverSay(line);game.Persist();FeverSync();}

 void SpawnFeverStory(){if(!FeverEnabled||dungeon!=null)return;
  if(feverSeed!=game.Save.Seed){if(GodotObject.IsInstanceValid(feverBoy))feverBoy.QueueFree();feverBoy=null;feverSeed=game.Save.Seed;}
  if(Fever.Stage==0&&Fever.X==0&&Fever.Z==0&&!CoopClient)SetFeverBoy(Ground(game.World.RouteX(FeverMeetZ)+6,FeverMeetZ));
  bool near=NearActiveSurface(Ground(FeverX,FeverZ),200);
  if(near&&uniqueObjects.Add("story/fever-house")){feverFloor=Enumerable.Range(0,3).SelectMany(x=>Enumerable.Range(0,3).Select(z=>game.World.Height(FeverX-4+x*4,FeverZ-5+z*5))).Max()+.18;
   feverHouse=new Node3D{Name="JustAFever",Position=FeverPoint(0,0,0)};worldRoot.AddChild(feverHouse);props.Add(new(){Id="story/fever-house",Kind=21,Node=feverHouse,Position=feverHouse.Position});DressFeverHouse();
  }
  if(!near){if(GodotObject.IsInstanceValid(feverBoy)){feverBoy.QueueFree();feverBoy=null;}return;}
  if((Fever.Stage<4||Fever.Stage==9&&Fever.FlightTime<14)&&!GodotObject.IsInstanceValid(feverBoy)){feverBoy=new Node3D{Name="Noah_NoDamageCollider",Position=FeverBoyPoint};AddChild(feverBoy);FeverChild(feverBoy,false);}
  if(Fever.Stage==5&&!CoopClient&&!game.Skills.Defeated.Contains(FeverMotherId)&&!infected.Any(e=>e.Id==FeverMotherId))SpawnFeverMother();
 }
 Vector3 FeverWaypoint(int index){return index switch{
  0=>Ground(game.World.RouteX(-440),-440),1=>Ground(game.World.RouteX(FeverZ+21),FeverZ+21),
  2=>Ground(FeverX,FeverZ+21),3=>FeverPoint(0,.06f,5.8f),_=>FeverPoint(.8f,.06f,2.4f)};}
 void TickFeverStory(double dt){if(!FeverEnabled||CoopClient||dungeon!=null)return;SpawnFeverStory();Fever.LineTime=Math.Max(0,Fever.LineTime-dt);TickFeverArc(dt);
  if(Fever.Stage<=1){var candidates=MillPlayers().Where(p=>p.Game.Life.Health>0&&Distance(p.Body.Position,FeverBoyPoint)<24).OrderBy(p=>Distance(p.Body.Position,FeverBoyPoint)).ToArray();
   if(candidates.Length>0){var target=candidates[0].Body.Position;float distance=Distance(target,FeverBoyPoint);if(Fever.Stage==0)FeverStage(1,"BOY: Please! Mum's sick. We can't get her fever down. Will you help?");if(distance>2.5f)FeverMove(target,dt,2.4f,false);}
  }else if(Fever.Stage==2){var leader=CommunityLeader(Fever.Owner);if(leader!=null&&Distance(leader.Value.Body.Position,FeverBoyPoint)<16){var destination=FeverWaypoint(Fever.Waypoint);FeverMove(destination,dt,2.25f,Fever.Waypoint>=3);if(Distance(FeverBoyPoint,destination)<.2f){Fever.Waypoint++;switch(Fever.Waypoint){case 1:FeverSay("NOAH: Dad locked Mum in her room. He said not to open it. Then he went for medicine.");break;case 2:FeverSay("NOAH: He's been gone for days. We keep heating soup and sliding it through the gap.");break;case 3:FeverSay("NOAH: My sister tried to reach in. Mum hurt her hand. Now she feels hot too.");break;case 5:FeverStage(3,"NOAH: She's on the sofa. Please look at her. I'll stay right here.");break;}}}
  }else if(Fever.Stage==4){if(!MillPlayers().Any(p=>Distance(p.Body.Position,FeverPoint(0,0,0))<30))return;Fever.Timer+=dt;
   if(Fever.Timer>=3){FeverStage(5,"The room falls quiet. The infected is still moving. Keep your distance.");SpawnFeverMother();}
  }else if(Fever.Stage==5&&game.Skills.Defeated.Contains(FeverMotherId)){if(Fever.ArcVersion>0)FinishFeverArc();else FeverStage(6,"JUST A FEVER / The house is quiet. Read the notebook on the table.");}
 }
 void FeverMove(Vector3 destination,double dt,float speed,bool raised){var current=FeverBoyPoint;var delta=destination-current;delta.Y=0;float step=Math.Min(delta.Length(),speed*(float)dt);if(step<=.001)return;var next=current+delta.Normalized()*step;
  if(raised){float end=(float)(game.World.Height(FeverX,FeverZ+21)-feverFloor);next.Y=next.Z>FeverZ+7.3?(float)feverFloor+Mathf.Lerp(0,end,Math.Clamp((next.Z-(float)FeverZ-7.3f)/13.7f,0,1)):(float)feverFloor+.06f;}else next.Y=(float)game.World.Height(next.X,next.Z);
  Fever.Yaw=Mathf.Atan2(-delta.X,-delta.Z);SetFeverBoy(next);
 }
 void SpawnFeverMother(){AddEnemy(FeverMotherId,FeverX,FeverZ-1.2,2);var mother=infected.FirstOrDefault(e=>e.Id==FeverMotherId);if(mother==null)return;mother.Body.Position=FeverPoint(0,.1f,-1.2f);mother.Home=mother.Body.Position;mother.Hp=mother.MaxHp=130;mother.Memory=8;mother.Target=player.Position;mother.Cooldown=1;}
 string FeverOperation(){if(!FeverEnabled||dungeon!=null)return "";
  if(Fever.Stage<=2&&GodotObject.IsInstanceValid(feverBoy)&&Distance(player.Position,FeverBoyPoint)<3.2&&Math.Abs(player.Position.Y-FeverBoyPoint.Y)<2)return Fever.Stage<2?"accept":CommunityLeader(Fever.Owner)==null?"resume":"boy";
  if(!GodotObject.IsInstanceValid(feverHouse)||!feverHouse.IsInsideTree()||Math.Abs(player.Position.Y-feverFloor)>1.5)return "";
  if(Fever.Stage<=3&&Distance(player.Position,FeverPoint(0,0,-1.9f))<2)return "door";
  if(Fever.Stage==3&&Distance(player.Position,FeverPoint(-2.6f,0,1.4f))<2)return "tend";
  return Distance(player.Position,FeverPoint(2.4f,0,1.4f))<1.8?"notebook":"";
 }
 string FeverPrompt()=>FeverOperation() switch{"accept"=>"F / HELP THE BOY","resume"=>"F / CONTINUE TO THE HOUSE","boy"=>"FOLLOW NOAH / HE WAITS IF YOU FALL BEHIND","door"=>"F / EXAMINE THE BOARDED DOOR","tend"=>"F / CHECK HIS SISTER","notebook"=>"F / READ THE HOUSEHOLD NOTEBOOK",_=>""};
 bool FeverInteract(){string op=FeverOperation();if(op=="")return false;if(Fever.ArcVersion>0&&op is "accept" or "door"){ShowFeverChoices(op=="accept");return true;}if(!CoopRequest("fever",op))FeverAction(op);return true;}
 void FeverAction(string op){if(CoopClient)return;if(Fever.ArcVersion>0){FeverArcAction(op);return;}if(op!=FeverOperation())return;
  switch(op){case "accept":Fever.Owner=CommunityOwner;FeverStage(2,"NOAH: Thank you. I'm Noah. It's this way. Please stay close.");break;
   case "resume":Fever.Owner=CommunityOwner;FeverSay("NOAH: You're back. Please, we have to get home.");break;
   case "boy":game.Notice="Follow Noah / he waits when you fall behind";break;
   case "door":FeverSay("A narrow feeding slit. Dried soup on the sill. Something throws its weight against the boards.");game.Notice="The door is braced from this side. Find the children first.";break;
   case "tend":foreach(var pair in MillPlayers().Where(p=>Distance(p.Body.Position,FeverPoint(0,0,0))<22))pair.Game.Shield=Math.Max(pair.Game.Shield,3.5);FeverStage(4,"Her hand is wrapped in an old dishcloth. Her fever is dangerously high. Behind you, the boards begin to split.");EnemySound(FeverPoint(0,1,-2),1);break;
   case "notebook":ReadFeverNotebook();break;
  }game.Persist();FeverSync();
 }
 const string FeverBefore="A note in an adult's handwriting:\n\n'Door stays shut. No matter what she says. I've gone to find medicine. Keep your sister away from the gap. I'll come back.'\n\nBelow it, two children have kept a careful list:\nMonday: soup, water, cold cloth.\nTuesday: soup. Mum didn't talk.\nWednesday: soup. Too much banging to sleep.\nThursday: Lily's hand hurts.\n\nThe last bowl on the table is cold.";
 const string FeverAfter="Noah asked for help because he thought his mother had a fever. He and Lily kept feeding her through the slit, waiting for their father to return.\n\nLily's wrapped hand and the heat of her skin told the story they could not understand. When the bracing gave way, their mother was already gone.\n\nNoah ran toward her voice. The light went out. Neither child survived.\n\nThe infected fell during the fight. Three names remain on the first page of the notebook: Mara, Noah, Lily.\n\nTheir father never returned. The soup is still on the table.";
 void ReadFeverNotebook(){bool completed=Fever.Stage>=6;if(completed)game.Change(()=>{if(!game.Profile.Journal.Seen.Contains(FeverJournal))game.Profile.Journal.Seen.Add(FeverJournal);});if(CoopHost&&coopActor>0)RpcId(coopActor,nameof(CoopFeverNotebook),completed);else ShowFeverNotebook(completed);}
 void ShowFeverNotebook(bool completed){var b=Panel(completed?"JUST A FEVER / REMEMBERED":"JUST A FEVER / HOUSEHOLD NOTEBOOK");Text(b,Fever.ArcVersion>0?FeverArcNotebook():completed?FeverBefore+"\n\n"+FeverAfter:FeverBefore);Button(b,"BACK",CloseMenu);}
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]void CoopFeverNotebook(bool completed){if(CoopClient)ShowFeverNotebook(completed);}
}
