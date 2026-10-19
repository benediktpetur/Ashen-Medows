using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class MigrationScene {
 readonly Dictionary<string,AudioStream> scoreStreams=new();
 readonly AudioStreamPlayer[] scorePlayers=new AudioStreamPlayer[2];
 readonly float[] scoreGain=new float[2];
 int scoreActive;string scoreName="";double scoreDecision,scoreDangerUntil,scoreElapsed;
 string scorePending="";double scorePendingAge;
 const float ScoreVolume=.24f;
 double musicLevel=65;float scoreIntensity=1;
 void SetMusicLevel(double value){musicLevel=Math.Clamp(value,0,100);int bus=AudioServer.GetBusIndex("Music");if(bus>=0)AudioServer.SetBusVolumeDb(bus,musicLevel<=0?-80:Mathf.LinearToDb((float)(musicLevel/100)));}
 void ShowMusicSettings(){var b=Panel("AUDIO / MUSIC");Text(b,"Quiet regional music follows the world, night and active danger. Music pauses in menus. U mutes all sound.");var value=new Label{Text="Music volume: "+(int)musicLevel+"%"};b.AddChild(value);var slider=new HSlider{MinValue=0,MaxValue=100,Step=1,Value=musicLevel,CustomMinimumSize=new Vector2(360,36)};b.AddChild(slider);slider.ValueChanged+=v=>{SetMusicLevel(v);value.Text="Music volume: "+(int)v+"%";var settings=new ConfigFile();settings.SetValue("audio","music",musicLevel);settings.Save("user://audio.cfg");};Button(b,"BACK",ShowOptions);}
 void InitMusic(){
  if(testing||coopCheckRole!=null||DisplayServer.GetName()=="headless")return;
  int bus=AudioServer.GetBusIndex("Music");
  if(bus<0){AudioServer.AddBus();bus=AudioServer.BusCount-1;AudioServer.SetBusName(bus,"Music");AudioServer.SetBusSend(bus,"Master");}
  var config=new ConfigFile();if(config.Load("user://audio.cfg")==Error.Ok)musicLevel=(double)config.GetValue("audio","music",65d);SetMusicLevel(musicLevel);
  foreach(string name in new[]{"meadow","woodland","outskirts","city","depths","home","night","combat","foreman","wrecker","bellkeeper","broodheart","mourner","colossus","matriarch"})
   scoreStreams[name]=GD.Load<AudioStream>("res://Audio/Music/"+name+".wav");
  for(int i=0;i<2;i++){scorePlayers[i]=new AudioStreamPlayer{Name="Score"+i,Bus="Music",VolumeDb=-80};AddChild(scorePlayers[i]);}
 }
 async void RunMusicChecks(){try{
  await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);ClearEnemies();
  foreach(string name in new[]{"meadow","woodland","outskirts","city","depths","home","night","combat","foreman","wrecker","bellkeeper","broodheart","mourner","colossus","matriarch"}){
   var clip=GD.Load<AudioStreamWav>("res://Audio/Music/"+name+".wav");MigrationChecks.Assert(clip!=null&&clip.GetLength()>30&&clip.LoopMode==AudioStreamWav.LoopModeEnum.Forward,"Looping exported score / "+name);
  }
  game.LastDamage=-100;scoreElapsed=0;scoreDangerUntil=0;game.World.Changes.Time=0;player.Position=Ground(ExpeditionWorld.SpawnX,ExpeditionWorld.SpawnZ);
  MigrationChecks.Assert(ScoreContext()=="meadow","Quiet starting region");
  var bossProbe=new Infected{Id=CampaignBossId(0),Body=player,Hp=100,MaxHp=100,Memory=0};infected.Add(bossProbe);string bossCue=ScoreContext();infected.Remove(bossProbe);MigrationChecks.Assert(bossCue=="wrecker","Active boss theme does not depend on ordinary zombie memory");scoreDangerUntil=0;
  game.World.Changes.Time=150*15;MigrationChecks.Assert(ScoreContext()=="night","Night variation");
  game.LastDamage=game.Clock;MigrationChecks.Assert(ScoreContext()=="combat","Damage escalates score");
  game.LastDamage=-100;scoreElapsed=5;MigrationChecks.Assert(ScoreContext()=="combat","Combat release holds through brief lulls");
  scoreElapsed=30;game.World.Changes.Time=0;MigrationChecks.Assert(ScoreContext()=="meadow","Calm returns after danger");
  scoreElapsed=170;MigrationChecks.Assert(ScoreContext()=="","Exploration includes silence");
  scoreElapsed=0;scoreDangerUntil=0;game.Life.Camps.Add(new(){Id="music-check",Seed=game.Save.Seed,X=player.Position.X,Z=player.Position.Z});MigrationChecks.Assert(ScoreContext()=="home","Camp warmth overrides region");
  for(int i=0;i<2;i++){scorePlayers[i]=new AudioStreamPlayer{Stream=GD.Load<AudioStream>("res://Audio/Music/home.wav")};AddChild(scorePlayers[i]);scorePlayers[i].Play();}scoreName="home";scoreGain[0]=.1f;paused=true;TickMusic(.1);MigrationChecks.Assert(scorePlayers[0].StreamPaused,"Menu pauses score");paused=false;muted=true;TickMusic(.1);MigrationChecks.Assert(scorePlayers[0].StreamPaused,"Global mute pauses score");
  foreach(var music in scorePlayers){music.Stop();music.Stream=null;music.QueueFree();}scorePlayers[0]=scorePlayers[1]=null;await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
  GD.Print("PASS / Adaptive music: 15 looping resources, regions, night, threat hold/release, camp, silence, pause and mute");GetTree().Quit();
 }catch(Exception ex){GD.PrintErr(ex);GetTree().Quit(1);}}
 // Only an engaged, audible/visible threat can escalate the score. Proximity alone never reveals ambushes.
 string ScoreContext(){
  var engaged=infected.Where(e=>e.Hp>0&&e.Memory>0&&(!JourneyEnabled||!e.Searching&&Distance(e.Target,player.Position)<4)&&Distance(player.Position,e.Body.Position)<32
   &&!Peaceful(e.Id)&&!PassiveStoryEnemy(e)&&!CheckpointPassive(e)
   &&(AnimalSight(e,32)||Distance(player.Position,e.Body.Position)<7)).ToArray();
  var boss=infected.FirstOrDefault(e=>e.Hp>0&&(IsMillBoss(e.Id)||IsCampaignBoss(e.Id))&&Distance(player.Position,e.Body.Position)<65&&AnimalSight(e,65));
  scoreIntensity=boss==null?1:boss.BossState=="recover"?.8f:boss.Hp<boss.MaxHp*.5?1.25f:1.1f;
  if(boss!=null){scoreDangerUntil=scoreElapsed+12;return IsMega(boss.Id)?new[]{"colossus","matriarch","mourner"}[MegaVariant(boss.Id)]:IsMillBoss(boss.Id)?"foreman":new[]{"wrecker","bellkeeper","broodheart"}[ExpeditionCampaign.BossIndex(boss.Id)];}
  if(engaged.Length>0||game.Clock-game.LastDamage<5)scoreDangerUntil=scoreElapsed+10;
  if(scoreElapsed<scoreDangerUntil)return "combat";
  if(birthdayMusic!=null&&GodotObject.IsInstanceValid(birthdayMusic)&&birthdayMusic.Playing&&Distance(player.Position,birthdayMusic.Position)<24)return "";
  if(dungeon!=null)return "depths";
  if(game.Life.Camps.Any(b=>b.Seed==game.Save.Seed&&ExpeditionGame.Distance(b.X,b.Z,player.Position.X,player.Position.Z)<24)
   ||game.World.Changes.Settlements.Any(t=>!t.Fallen&&t.Trust>=0&&t.Index<3&&ExpeditionGame.Distance(t.X,t.Z,player.Position.X,player.Position.Z)<32))return "home";
  // Leave breathing room rather than wallpapering every minute with music.
  if(scoreElapsed%200>160)return "";
  if(game.Night)return "night";
  return new[]{"meadow","meadow","woodland","outskirts","city","depths"}[Math.Clamp(game.World.Tier(player.Position.X,player.Position.Z),1,5)];
 }
 void TickMusic(double dt){
  if(scorePlayers[0]==null)return;
  bool stopped=paused||muted;
  if(!stopped){scoreElapsed+=dt;scoreDecision-=dt;if(scoreDecision<=0){scoreDecision=.5;string wanted=ScoreContext();
    if(wanted!=scorePending){scorePending=wanted;scorePendingAge=0;}else scorePendingAge+=.5;
    bool urgent=wanted is "combat" or "foreman" or "wrecker" or "bellkeeper" or "broodheart" or "mourner" or "colossus" or "matriarch";
    if(wanted!=scoreName&&(urgent||scorePendingAge>=3)){
     scoreName=wanted;int next=1-scoreActive;
     if(wanted!=""){scorePlayers[next].Stop();scorePlayers[next].Stream=scoreStreams[wanted];scoreGain[next]=0;scorePlayers[next].VolumeDb=-80;scorePlayers[next].Play();}
     scoreActive=next;
    }
   }}
  bool duck=FeverCinematic||game.Clock-game.LastDamage<1.5;
  for(int i=0;i<2;i++){
   scorePlayers[i].StreamPaused=stopped;
   float target=i==scoreActive&&scoreName!=""?(duck?ScoreVolume*.25f:ScoreVolume*scoreIntensity):0;
   scoreGain[i]=Mathf.MoveToward(scoreGain[i],target,(float)(dt*ScoreVolume/4));
   scorePlayers[i].VolumeDb=scoreGain[i]<=.0001f?-80:Mathf.LinearToDb(scoreGain[i]);
   if(scoreGain[i]<=.0001f&&i!=scoreActive)scorePlayers[i].Stop();
  }
 }
}
