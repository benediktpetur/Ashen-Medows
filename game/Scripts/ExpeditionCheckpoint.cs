using Godot;
using System;
using System.Linq;

public partial class MigrationScene {
 ExpeditionCheckpoint Checkpoint=>game.World.Changes.Checkpoint;
 double CheckpointX=>ExpeditionCheckpoint.X(game.World);double checkpointFloor;bool checkpointFixture;
 bool CheckpointEnabled=>!((testing||coopCheckRole!=null)&&!checkpointFixture);
 Node3D checkpointRoot,checkpointGate,checkpointTraveller,checkpointChildren,checkpointFood;
 CanvasLayer checkpointLayer;Label checkpointCaption,checkpointObjective;int checkpointShot;
 string CheckpointGuardId(int i)=>$"story/checkpoint/{game.Save.Seed}/guard/{i}";
 static bool IsCheckpointGuard(string id)=>id.StartsWith("story/checkpoint/")&&id.Contains("/guard/");
 bool CheckpointPassive(Infected e)=>IsCheckpointGuard(e.Id)&&!Checkpoint.Hostile;
 bool CheckpointCleared=>Enumerable.Range(0,3).All(i=>game.Skills.Defeated.Contains(CheckpointGuardId(i)));
 bool InCheckpointLot(double x,double z)=>ExpeditionCheckpoint.InLot(game.World,x,z);
 Vector3 CheckpointPoint(float x,float y,float z)=>new((float)CheckpointX+x,(float)checkpointFloor+y,(float)ExpeditionCheckpoint.Z+z);
 void CheckpointSay(string text){Checkpoint.Line=text;Checkpoint.LineTime=10;}
 void CheckpointSync(){Checkpoint.Revision++;if(CoopHost&&coopActors.Count>0)Rpc(nameof(CoopWorldUpdate),Json.Write(WorldSnapshot()));}
 Vector3 CheckpointPost(int i)=>CheckpointPoint(i==0?-2:i==1?2:2,0,i==0?5:i==1?0:-10);
 void SpawnCheckpoint(){if(!CheckpointEnabled||dungeon!=null||!NearActiveSurface(Ground(CheckpointX,ExpeditionCheckpoint.Z),220))return;
  if(uniqueObjects.Add("story/checkpoint")){checkpointFloor=Enumerable.Range(0,3).SelectMany(a=>new[]{-20,0,20}.Select(z=>game.World.Height(CheckpointX-3+a*3,ExpeditionCheckpoint.Z+z))).Max()+.45;checkpointRoot=new Node3D{Name="ThePriceOfCrossing",Position=CheckpointPoint(0,0,0)};worldRoot.AddChild(checkpointRoot);props.Add(new(){Id="story/checkpoint",Kind=25,Node=checkpointRoot,Position=checkpointRoot.Position});DressCheckpoint();}
  if(CoopClient||!NearActiveSurface(Ground(CheckpointX,ExpeditionCheckpoint.Z),130))return;for(int i=0;i<3;i++){string id=CheckpointGuardId(i);if(game.Skills.Defeated.Contains(id)||infected.Any(e=>e.Id==id))continue;var point=CheckpointPost(i);AddEnemy(id,point.X,point.Z,3);var guard=infected.FirstOrDefault(e=>e.Id==id);if(guard!=null){guard.Body.Position=point;guard.Body.Rotation=new(0,Mathf.Pi,0);guard.Home=point;guard.Cooldown=1;}}
 }
 Vector3 CheckpointCarrierPoint(double t){t%=24;Vector3 a=CheckpointPost(1),b=CheckpointPoint(2,0,-5),c=CheckpointPoint(8,0,-5);return t<3?a.Lerp(b,(float)(t/3)):t<7?b.Lerp(c,(float)((t-3)/4)):t<12?c:t<16?c.Lerp(b,(float)((t-12)/4)):t<20?b.Lerp(a,(float)((t-16)/4)):a;}
 bool CheckpointObserver(CharacterBody3D body){float distance=Distance(body.Position,CheckpointPoint(0,0,0));if(distance<8||distance>38||Math.Abs(body.Position.Y-checkpointFloor)>4)return false;var target=CheckpointPoint(8,1.3f,-6);var delta=target-body.Position;delta.Y=0;if((-body.GlobalBasis.Z).Dot(delta.Normalized())<.12)return false;var ray=PhysicsRayQueryParameters3D.Create(body.Position+Vector3.Up*1.5f,target);ray.Exclude=PursuitExclusions();var hit=GetWorld3D().DirectSpaceState.IntersectRay(ray);return hit.Count==0||((Vector3)hit["position"]).DistanceTo(target)<2.2f;}
 void TickCheckpoint(double dt){if(!CheckpointEnabled||CoopClient||dungeon!=null)return;Checkpoint.LineTime=Math.Max(0,Checkpoint.LineTime-dt);if(!NearActiveSurface(Ground(CheckpointX,ExpeditionCheckpoint.Z),80))return;
  if(!Checkpoint.Asked&&MillPlayers().Any(p=>Distance(p.Body.Position,CheckpointPoint(0,0,6))<14)){Checkpoint.Asked=true;CheckpointSay("RAE: Two portions of food. Then you cross. Don't make this harder than it is.");CheckpointSync();}
  if(!Checkpoint.Hostile&&!CheckpointCleared){Checkpoint.DeliveryClock=(Checkpoint.DeliveryClock+dt)%24;if(Checkpoint.DeliveryClock>=9)Checkpoint.DeliveredOnce=true;if(!Checkpoint.Revealed&&MillPlayers().Any(p=>CheckpointObserver(p.Body))){Checkpoint.Watch+=dt;if(Checkpoint.Watch>=12&&Checkpoint.DeliveredOnce){Checkpoint.Revealed=true;CheckpointSay("A SMALL VOICE: Is there enough for Lena?\nThe guard passes the food through the locked door. His own bowl is empty.");game.Persist();CheckpointSync();}}}
  if(Checkpoint.Paid&&!Checkpoint.Hostile&&Checkpoint.Robbery==0&&MillPlayers().Any(p=>Distance(p.Body.Position,CheckpointPoint(0,0,0))<60&&p.Body.Position.Z<ExpeditionCheckpoint.Z-13)){Checkpoint.Robbery=1;Checkpoint.RobberyTime=0;CheckpointSay("Behind you: 'Put the pack down.'\nTRAVELLER: Please. That's everything I have.");game.Persist();CheckpointSync();}
  if(Checkpoint.Robbery==1){Checkpoint.RobberyTime+=dt;if(Checkpoint.RobberyTime>=4){Checkpoint.Robbery=2;CheckpointSay("RAE: Leave the food. Keep walking.\nThe traveller backs away without his pack.");game.Persist();CheckpointSync();}}
 }
 string CheckpointOperation(){if(!CheckpointEnabled||dungeon!=null||!LetterNodeLive(checkpointRoot)||Math.Abs(player.Position.Y-checkpointFloor)>2)return "";if(Distance(player.Position,CheckpointPoint(8,0,-5.6f))<2.5)return "room";return !Checkpoint.Hostile&&!CheckpointCleared&&Distance(player.Position,CheckpointPoint(0,0,6))<5?"open":"";}
 string CheckpointPrompt()=>CheckpointOperation() switch{"open"=>Checkpoint.Paid?"F / SPEAK TO THE CHECKPOINT GUARDS":"F / CHECKPOINT / FOOD TOLL","room"=>"F / EXAMINE THE LOCKED ROOM",_=>""};
 bool CheckpointInteract(){string op=CheckpointOperation();if(op=="")return false;if(!CoopRequest("checkpoint",op))CheckpointAction(op);return true;}
 void CheckpointAction(string op){if(CoopClient)return;
  bool gate=CheckpointOperation()=="open",room=CheckpointOperation()=="room";
  if(op=="open"&&gate){Checkpoint.Asked=true;if(CoopHost&&coopActor>0){CheckpointSync();RpcId(coopActor,nameof(CoopCheckpointPanel),0);}else ShowCheckpointPanel(0);return;}
  if(op=="room"&&room){if(CheckpointCleared&&!Checkpoint.Revealed){Checkpoint.Revealed=true;CheckpointSync();}if(Checkpoint.Revealed)game.Change(()=>{if(!game.Profile.Journal.Seen.Contains("checkpoint/room"))game.Profile.Journal.Seen.Add("checkpoint/room");});if(CoopHost&&coopActor>0)RpcId(coopActor,nameof(CoopCheckpointPanel),1);else ShowCheckpointPanel(1);return;}
  if(op=="pay"&&gate&&!Checkpoint.Paid){if(game.Life.Food<2){game.Notice="The toll is two food portions. You can step back and watch, or leave.";return;}if(game.Change(()=>{game.Life.Food-=2;Checkpoint.Paid=true;}))CheckpointSay(Checkpoint.Revealed?"RAE: Thank you. They haven't eaten today. You're clear to cross.":"RAE: All right. Go through. Keep moving.");}
  else if(op=="refuse"&&gate&&!Checkpoint.Paid)StartCheckpointFight();
  else if(op=="watch"&&gate)CheckpointSay("Step back, keep the checkpoint in sight, and watch where the food goes.");
  else if(op=="donate"&&room&&Checkpoint.Revealed&&!Checkpoint.Donated){if(game.Life.Food<1){game.Notice="You need one food portion to leave at the door.";return;}if(game.Change(()=>{game.Life.Food--;Checkpoint.Donated=true;}))CheckpointSay("You leave food beside the feeding slot. A quiet 'thank you' comes from inside.");}
  else return;game.Persist();CheckpointSync();
 }
 void StartCheckpointFight(){if(Checkpoint.Hostile||CheckpointCleared)return;Checkpoint.Hostile=true;if(Checkpoint.Robbery==1){Checkpoint.Robbery=2;Checkpoint.RobberyTime=4;}CheckpointSay("RAE: Then get off the bridge. Now!\nThe guards raise their weapons.");foreach(var e in infected.Where(e=>IsCheckpointGuard(e.Id))){e.AttackState="";e.Memory=8;e.Cooldown=1;e.Target=player.Position;}game.Persist();CheckpointSync();}
 bool TickCheckpointGuard(Infected e,double dt){if(!IsCheckpointGuard(e.Id))return false;if(e.Hp<e.MaxHp&&!Checkpoint.Hostile)StartCheckpointFight();e.Stagger=Math.Max(0,e.Stagger-dt);e.Cooldown=Math.Max(0,e.Cooldown-dt);
  if(!Checkpoint.Hostile){e.Memory=0;e.AttackState="";int index=e.Id.EndsWith("/1")?1:e.Id.EndsWith("/2")?2:0;var next=index==1?CheckpointCarrierPoint(Checkpoint.DeliveryClock):CheckpointPost(index);var delta=next-e.Body.Position;if(delta.LengthSquared()>.001)e.Body.Rotation=new(0,Mathf.Atan2(-delta.X,-delta.Z),0);e.Body.Position=next;e.Body.Velocity=delta/(float)Math.Max(.001,dt);if(index==1&&Checkpoint.DeliveryClock>=7&&Checkpoint.DeliveryClock<12)e.Body.Rotation=Vector3.Zero;CheckpointPose(e);return true;}
  e.Memory=8;var target=player.Position+Vector3.Up*1.1f;float distance=Distance(e.Body.Position,player.Position);bool sight=distance<34&&CheckpointShotClear(e,target);var direction=target-e.Body.Position;direction.Y=0;if(direction.LengthSquared()>.01)e.Body.Rotation=new(0,Mathf.Atan2(-direction.X,-direction.Z),0);
  if(e.Stagger>0){e.AttackState="";e.Cooldown=Math.Max(.6,e.Cooldown);}else if(e.AttackState=="guard-aim"){e.AttackTime-=dt;if(e.AttackTime<=0){var from=e.Body.Position+Vector3.Up*1.35f;var q=PhysicsRayQueryParameters3D.Create(from,e.BossAim);q.Exclude=new(){e.Body.GetRid()};var hit=GetWorld3D().DirectSpaceState.IntersectRay(q);if(hit.Count>0&&hit["collider"].AsGodotObject()==player)MillDamage(game,player,9,false);CommunityTracer(from,e.BossAim);CheckpointGunshot(from);e.AttackState="";e.Cooldown=2.4;}}
  else if(sight&&e.Cooldown<=0){e.AttackState="guard-aim";e.AttackTime=.95;e.BossAim=target;}
  var velocity=Vector3.Zero;if(e.AttackState==""&&e.Stagger<=0&&distance>9&&distance<45&&!sight){var move=PursuitDirection(e,player.Position,dt);if(move.LengthSquared()>.1)velocity=move.Normalized()*2.5f;}if(distance>55){var move=PursuitDirection(e,e.Home,dt);if(move.Length()>.5)velocity=move.Normalized()*2;}velocity.Y=e.Body.IsOnFloor()?-.1f:e.Body.Velocity.Y-18*(float)dt;e.Body.Velocity=velocity;e.Body.MoveAndSlide();CheckpointPose(e);return true;
 }
 bool CheckpointShotClear(Infected e,Vector3 target){var q=PhysicsRayQueryParameters3D.Create(e.Body.Position+Vector3.Up*1.35f,target);q.Exclude=new(){e.Body.GetRid()};var hit=GetWorld3D().DirectSpaceState.IntersectRay(q);return hit.Count==0||hit["collider"].AsGodotObject()==player;}
 void CheckpointGunshot(Vector3 at){if(testing||muted||DisplayServer.GetName()=="headless"||Distance(player.Position,at)>85||enemySounds.Count>=8)return;var sound=new AudioStreamPlayer3D{Position=at,Stream=EnemyCue(3),VolumeDb=-7,MaxDistance=85,UnitSize=12};AddChild(sound);sound.Play();enemySounds.Add((sound,Time.GetTicksMsec()/1000.0+1));}
 void ShowCheckpointPanel(int kind){var b=Panel(kind==0?"THE PRICE OF CROSSING":"THE LOCKED ROOM");if(kind==0){Text(b,Checkpoint.Revealed?"Two children eat behind a locked door. The guard has carried a tin to them, then returned to his own empty bowl.\n\nRAE: 'They're all we've got left.'":"Three exhausted survivors hold the bridge. Empty bowls sit beside their weapons.\n\nRAE: 'Two portions of food. Then you cross.'");if(!Checkpoint.Paid){Button(b,$"{(Checkpoint.Revealed?"SHARE PROVISIONS":"PAY THE TOLL")} / 2 FOOD / YOU HAVE {game.Life.Food}",()=>CheckpointChoice("pay"));Button(b,"REFUSE THE TOLL",()=>CheckpointChoice("refuse"));}else Text(b,"Your party's passage is paid.");Button(b,"STEP BACK AND WATCH",()=>CheckpointChoice("watch"));}
  else{Text(b,Checkpoint.Revealed?(CheckpointCleared?"Children shelter inside the locked room, with ration tins stacked beside their beds. A small voice asks when Tom is coming back.\n\nThere is a feeding slot in the door. You can leave food here.":"Children shelter inside the old bridge keeper's room. The door is locked, with a narrow feeding slot cut into it. A guard passes in a tin, then returns to an empty bowl.\n\nThe toll is keeping them alive. The traveller's frightened voice still carries across the water."):"A padlock holds the door closed. There is a narrow slot beneath it. You hear small movements inside. Watch the checkpoint for longer to understand who is here.");if(Checkpoint.Revealed&&!Checkpoint.Donated)Button(b,$"LEAVE FOOD AT THE SLOT / 1 FOOD / YOU HAVE {game.Life.Food}",()=>CheckpointChoice("donate"));}
  Button(b,"LEAVE",CloseMenu);
 }
 void CheckpointChoice(string op){CloseMenu();if(!CoopRequest("checkpoint",op))CheckpointAction(op);}
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]void CoopCheckpointPanel(int kind){if(CoopClient)ShowCheckpointPanel(kind);}
}
