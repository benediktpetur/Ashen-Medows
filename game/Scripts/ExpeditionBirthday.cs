using Godot;
using System;
using System.Linq;

public partial class MigrationScene {
 ExpeditionBirthday Birthday=>game.World.Changes.Birthday;
 double BirthdayX=>ExpeditionBirthday.X(game.World);double birthdayFloor;
 string SammyId=>$"story/birthday/sammy/{game.Save.Seed}";
 static bool IsSammy(string id)=>id.StartsWith("story/birthday/sammy/");
 bool BirthdayEnabled=>!((testing||coopCheckRole!=null)&&!birthdayFixture);bool birthdayFixture;
 Node3D birthdayHouse,birthdayWholeCake,birthdayCutCake;AudioStreamPlayer3D birthdayMusic;AudioStreamWav birthdayTune;
 bool InBirthdayLot(double x,double z)=>ExpeditionBirthday.InLot(game.World,x,z);
 Vector3 BirthdayPoint(float x,float y,float z)=>new((float)BirthdayX+x,(float)birthdayFloor+y,(float)ExpeditionBirthday.Z+z);
 bool SammyDormant(Infected e)=>IsSammy(e.Id)&&e.BossState=="birthday-seated";
 void SpawnBirthday(){if(!BirthdayEnabled||dungeon!=null||!NearActiveSurface(Ground(BirthdayX,ExpeditionBirthday.Z),220))return;
  if(uniqueObjects.Add("story/birthday/house")){birthdayFloor=LetterFloor(BirthdayX,ExpeditionBirthday.Z,4,5);birthdayHouse=new Node3D{Name="SammysNinthBirthday",Position=BirthdayPoint(0,0,0)};worldRoot.AddChild(birthdayHouse);props.Add(new(){Id="story/birthday/house",Kind=24,Node=birthdayHouse,Position=birthdayHouse.Position});DressBirthdayHouse();}
  if(!CoopClient&&!game.Skills.Defeated.Contains(SammyId)&&!infected.Any(e=>e.Id==SammyId)&&NearActiveSurface(Ground(BirthdayX,ExpeditionBirthday.Z),130)){AddEnemy(SammyId,BirthdayX,ExpeditionBirthday.Z-2.35,2);var e=infected.FirstOrDefault(e=>e.Id==SammyId);if(e!=null){e.Body.Position=BirthdayPoint(Birthday.CakeTouched?.75f:0,.02f,-2.35f);e.Body.Rotation=new(0,Mathf.Pi,0);e.Home=e.Target=e.Body.Position;AnimateCharacter(e);}}
 }
 void SammyArt(Node3D root){root.Scale=new(.64f,.64f,.64f);HumanCharacter(root,1,0,true);var head=root.GetNode<Node3D>("Torso/Head");head.Scale*=1.12f;Ellipsoid(head,new(-.05f,.045f,-.143f),new(.022f,.024f,.013f),new(.12f,.14f,.13f));Ellipsoid(head,new(.05f,.045f,-.143f),new(.022f,.024f,.013f),new(.12f,.14f,.13f));var hat=new Node3D{Name="PartyHat"};head.AddChild(hat);Tube(hat,new(0,.21f,0),new(0,.53f,0),.145f,new(.46f,.36f,.22f),0);Tube(hat,new(0,.20f,0),new(0,.23f,0),.15f,new(.28f,.39f,.43f));Ellipsoid(hat,new(0,.54f,0),new(.035f,.035f,.035f),new(.64f,.55f,.32f));}
 bool TickBirthdayEnemy(Infected e,double dt){if(!IsSammy(e.Id))return false;
  if(e.BossState=="birthday-seated"){e.Memory=0;e.AttackState="";e.Stagger=Math.Max(0,e.Stagger-dt);e.Body.Velocity=Vector3.Zero;AnimateCharacter(e);return true;}
  if(e.BossState=="birthday-rise"){e.BossTime=Math.Max(0,e.BossTime-dt);AnimateCharacter(e);if(e.BossTime<=0){e.BossState="birthday-awake";e.Body.Position+=Vector3.Right*.75f;e.Visual.Position=Vector3.Zero;}return true;}return false;
 }
 bool SammyPose(Infected e){if(!IsSammy(e.Id))return false;e.Visual.Position=Vector3.Zero;if(e.BossState!="birthday-seated"&&e.BossState!="birthday-rise")return false;float seated=e.BossState=="birthday-rise"?(float)Math.Clamp(e.BossTime/.75,0,1):1;e.Visual.Position=new(0,.07f*seated,0);e.Visual.GetNode<Node3D>("Torso").RotationDegrees=new(8*seated,0,0);foreach(string side in new[]{"Left","Right"}){e.Visual.GetNode<Node3D>(side+"Leg").RotationDegrees=new(80*seated,0,0);e.Visual.GetNode<Node3D>(side+"Leg/Knee").RotationDegrees=new(-85*seated,0,0);e.Visual.GetNode<Node3D>(side+"Arm").RotationDegrees=new(32*seated,0,side=="Left"?8*seated:-8*seated);e.Visual.GetNode<Node3D>(side+"Arm/Elbow").RotationDegrees=new(-45*seated,0,0);}return true;}
 string BirthdayOperation(){if(!BirthdayEnabled||dungeon!=null||!LetterNodeLive(birthdayHouse)||Math.Abs(player.Position.Y-birthdayFloor)>1.5||Math.Abs(player.Position.X-BirthdayX)>3.9||Math.Abs(player.Position.Z-ExpeditionBirthday.Z)>4.9)return "";
  if(!Birthday.CakeTouched&&Distance(player.Position,BirthdayPoint(0,0,0))<2.15)return "cake";
  if(Distance(player.Position,BirthdayPoint(-3,0,2.7f))<1.8)return Birthday.MusicStopped?"silent":"music";
  if(Distance(player.Position,BirthdayPoint(0,0,-3.8f))<2)return "banner";return "";
 }
 string BirthdayPrompt()=>BirthdayOperation() switch{"cake"=>"F / TAKE A SLICE OF CAKE","music"=>"F / STOP THE MUSIC BOX","silent"=>"THE MUSIC BOX IS SILENT","banner"=>"F / READ THE BIRTHDAY BANNER",_=>""};
 bool BirthdayInteract(){string op=BirthdayOperation();if(op=="")return false;if(!CoopRequest("birthday",op))BirthdayAction(op);return true;}
 void BirthdayAction(string op){if(CoopClient||op!=BirthdayOperation())return;switch(op){
  case "cake":if(!game.Change(()=>{Birthday.CakeTouched=true;Birthday.MusicStopped=true;game.Life.Food++;}))return;var sammy=infected.FirstOrDefault(e=>e.Id==SammyId);if(sammy!=null){sammy.BossState="birthday-rise";sammy.BossTime=.75;sammy.Memory=8;sammy.Target=player.Position;sammy.Cooldown=1;}Noise(32);game.Notice=sammy!=null?"The music stops. A chair scrapes across the floor.":"One slice packed. The room is quiet.";Sound(4);break;
  case "music":game.Change(()=>Birthday.MusicStopped=true);game.Notice="The music box winds down. Sammy stays at the table.";break;
  case "silent":game.Notice="The music box is silent.";break;
  case "banner":game.Change(()=>{if(!game.Profile.Journal.Seen.Contains("birthday/sammy"))game.Profile.Journal.Seen.Add("birthday/sammy");});if(CoopHost&&coopActor>0)RpcId(coopActor,nameof(CoopBirthdayNote));else ShowBirthdayNote();break;
 }if(CoopHost&&coopActors.Count>0)Rpc(nameof(CoopWorldUpdate),Json.Write(WorldSnapshot()));}
 void ShowBirthdayNote(){var b=Panel("SAMMY'S NINTH BIRTHDAY");Text(b,"HAPPY 9TH BIRTHDAY, SAMMY.\n\nNine candles. Plates set for guests who never arrived. A paper hat at the head of the table.\n\nThe tune keeps returning to the same few notes.\n\nSammy has been waiting.");Button(b,"BACK",CloseMenu);}
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]void CoopBirthdayNote(){if(CoopClient)ShowBirthdayNote();}
 void EnemyImpact(Infected e,Vector3 at,Vector3 direction,int count){if(!IsSammy(e.Id))BloodImpact(at,direction,count);}
 void DrawBirthday(){if(!BirthdayEnabled||!LetterNodeLive(birthdayHouse))return;birthdayWholeCake.Visible=!Birthday.CakeTouched;birthdayCutCake.Visible=Birthday.CakeTouched;
  bool stopped=Birthday.MusicStopped||Birthday.CakeTouched||dungeon!=null;birthdayMusic.StreamPaused=muted||paused;if(stopped){if(birthdayMusic.Playing)birthdayMusic.Stop();}else if(!birthdayMusic.Playing&&!testing&&DisplayServer.GetName()!="headless")birthdayMusic.Play();
  birthdayHouse.GetNode<Node3D>("FallenHat").Visible=game.Skills.Defeated.Contains(SammyId);
 }
 AudioStreamWav BirthdayTune(){if(birthdayTune!=null)return birthdayTune;const int rate=22050;const double duration=12;int count=(int)(duration*rate);byte[] data=new byte[count*2];double[] notes={523.25,622.25,783.99,698.46,622.25,523.25,466.16,0,523.25,698.46,622.25,466.16,415.30,466.16,523.25,0};
  for(int i=0;i<count;i++){double t=i/(double)rate,beat=t/.75;int note=(int)beat%notes.Length;double local=t% .75,frequency=notes[note]*(1+.004*Math.Sin(t*3.1));double envelope=(1-Math.Exp(-local*300))*Math.Exp(-local*6);double sample=frequency==0?0:(Math.Sin(t*frequency*Math.Tau)+.23*Math.Sin(t*frequency*2.003*Math.Tau))*.18*envelope;short value=(short)(sample*22000);data[i*2]=(byte)value;data[i*2+1]=(byte)(value>>8);}
  return birthdayTune=new AudioStreamWav{Format=AudioStreamWav.FormatEnum.Format16Bits,MixRate=rate,Data=data,LoopMode=AudioStreamWav.LoopModeEnum.Forward,LoopEnd=count};
 }
 void DisposeBirthdayMusic(){if(GodotObject.IsInstanceValid(birthdayMusic)){birthdayMusic.Stop();birthdayMusic.Stream=null;}birthdayTune?.Dispose();birthdayTune=null;}
}
