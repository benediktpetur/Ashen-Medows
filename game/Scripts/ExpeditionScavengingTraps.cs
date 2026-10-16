using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
[DataContract] sealed class ScavengeAlarm {
 [DataMember] public string Id;
 [DataMember] public float X,Y,Z;
 [DataMember] public int Kind,Tier,Waves;
 [DataMember] public double Age;
 public Vector3 Position=>new(X,Y,Z);
}
public partial class MigrationScene {
 Node3D scavengeSource;AudioStreamPlayer3D scavengeAudio;string scavengeVisualId;Label scavengeLabel;
 static int TrapRoll(ExpeditionWorld world,string id)=> (int)(world.Hash(id.GetHashCodeStable(),491)%100);
 bool TrapEligible(WorldProp p)=>p.Tier>=2&&TrapRoll(game.World,p.Id)<22&&!InMill(p.Position.X,p.Position.Z,80)&&!game.Life.Camps.Any(c=>c.Seed==game.Save.Seed&&ExpeditionGame.Distance(c.X,c.Z,p.Position.X,p.Position.Z)<40);
 void TryScavengeTrap(WorldProp p){if(CoopClient||!TrapEligible(p))return;var world=game.World.Changes;if(world.TrapCooldown>0||world.ScavengeAlarm!=null||world.SpentTraps.Contains(p.Id))return;
  if(!game.Change(()=>{world.SpentTraps.Add(p.Id);world.TrapCooldown=240;world.ScavengeAlarm=new(){Id=p.Id,X=p.Position.X,Y=p.Position.Y,Z=p.Position.Z,Tier=p.Tier,Kind=(int)(game.World.Hash(p.Id.GetHashCodeStable(),857)%3)};}))return;
  game.Notice="OH SHIT / "+TrapName(world.ScavengeAlarm.Kind)+"! Silence it with F or get away!";if(CoopHost&&coopActors.Count>0)Rpc(nameof(CoopWorldUpdate),Json.Write(WorldSnapshot()));
 }
 static string TrapName(int kind)=>kind==0?"HIDDEN RADIO ALARM":kind==1?"FALLING CAN RIG":"DISTRESS FLARE";
 void TickScavengeTraps(double dt){if(CoopClient)return;var w=game.World.Changes;w.TrapCooldown=Math.Max(0,w.TrapCooldown-dt);var a=w.ScavengeAlarm;if(a==null)return;if(dungeon!=null){w.ScavengeAlarm=null;return;}a.Age+=dt;
  foreach(var e in infected)if(!Peaceful(e.Id)&&!IsMillBoss(e.Id)&&Distance(e.Body.Position,a.Position)<110){e.Memory=8;e.Target=a.Position;}
  if(a.Waves==0&&a.Age>=6){a.Waves=1;SpawnScavengeWave(a,0);}if(a.Waves==1&&a.Age>=18){a.Waves=2;SpawnScavengeWave(a,1);}if(a.Age>=30)w.ScavengeAlarm=null;
 }
 void SpawnScavengeWave(ScavengeAlarm a,int wave){int count=wave==0?Math.Min(7,2+a.Tier):Math.Min(4,a.Tier);int spawnedCount=0;for(int attempt=0;attempt<48&&spawnedCount<count&&infected.Count<(CoopHost?CoopMaxEnemies:60);attempt++){
  uint hash=game.World.Hash(a.Id.GetHashCodeStable()+attempt*71,wave+192);float angle=hash%6283/1000f,radius=30+(hash>>12)%16;var p=Ground(a.X+Mathf.Sin(angle)*radius,a.Z+Mathf.Cos(angle)*radius);
  if(!ChunkLoaded(p)||!OutdoorFree(p.X,p.Z)||MillPlayers().Any(pair=>Distance(pair.Body.Position,p)<18))continue;
  string id=$"ambush/{a.Id}/{wave}/{spawnedCount}";AddEnemy(id,p.X,p.Z,a.Tier);var enemy=infected.FirstOrDefault(e=>e.Id==id);if(enemy==null)continue;enemy.Memory=12;enemy.Target=a.Position;spawnedCount++;
 }}
 bool SilenceScavengeTrap(){var a=game.World.Changes.ScavengeAlarm;if(a==null||Distance(player.Position,a.Position)>3)return false;if(game.Change(()=>game.World.Changes.ScavengeAlarm=null)){game.Notice="Noise stopped / approaching infected are still out there";if(CoopHost&&coopActors.Count>0)Rpc(nameof(CoopWorldUpdate),Json.Write(WorldSnapshot()));}return true;}
 AudioStreamWav TrapSiren(int kind){const int rate=22050;byte[] data=new byte[rate*2];for(int i=0;i<rate;i++){double t=i/(double)rate;double f=kind==0?650+220*Math.Sin(t*Math.PI*2):kind==1?180:1100;double envelope=kind==1?Math.Exp(-(t%.25)*25):.6;short v=(short)(Math.Sin(t*f*Math.PI*2)*envelope*9500);data[i*2]=(byte)v;data[i*2+1]=(byte)(v>>8);}return new(){Format=AudioStreamWav.FormatEnum.Format16Bits,MixRate=rate,Data=data,LoopMode=AudioStreamWav.LoopModeEnum.Forward,LoopEnd=rate};}
 void DrawScavengeTrap(){var a=game.World.Changes.ScavengeAlarm;if(a==null||dungeon!=null){if(GodotObject.IsInstanceValid(scavengeSource))scavengeSource.QueueFree();scavengeSource=null;scavengeVisualId=null;if(scavengeLabel!=null)scavengeLabel.Visible=false;return;}
  if(scavengeVisualId!=a.Id||!GodotObject.IsInstanceValid(scavengeSource)){if(GodotObject.IsInstanceValid(scavengeSource))scavengeSource.QueueFree();scavengeVisualId=a.Id;scavengeSource=new Node3D{Position=a.Position};AddChild(scavengeSource);Box(scavengeSource,new(0,.2f,0),new(.42f,.4f,.28f),Steel);for(int i=0;i<4;i++)Box(scavengeSource,new(-.13f+i*.08f,.22f,-.15f),new(.025f,.21f,.015f),Ink);Tube(scavengeSource,new(.16f,.35f,0),new(.25f,.85f,0),.012f,Steel);MillRing(scavengeSource,1.1f,new(1,.16f,.05f,.8f));var lamp=new OmniLight3D{Position=new(0,.65f,0),LightColor=new(1,.12f,.025f),OmniRange=12,LightEnergy=3};scavengeSource.AddChild(lamp);if(a.Kind==2){var smokeMaterial=new StandardMaterial3D{AlbedoColor=new(.65f,.20f,.10f,.25f),Transparency=BaseMaterial3D.TransparencyEnum.Alpha,ShadingMode=BaseMaterial3D.ShadingModeEnum.Unshaded,EmissionEnabled=true,Emission=new(.3f,.035f,.005f)};var smoke=new CpuParticles3D{Position=new(0,.5f,0),Amount=45,Lifetime=3,Preprocess=2,Direction=Vector3.Up,Spread=18,InitialVelocityMin=1.1f,InitialVelocityMax=2.2f,Gravity=new(0,.15f,0),ScaleAmountMin=.6f,ScaleAmountMax=2,Mesh=new SphereMesh{Radius=.12f,Height=.24f,RadialSegments=8,Rings=4,Material=smokeMaterial}};scavengeSource.AddChild(smoke);Ellipsoid(scavengeSource,new(0,.5f,0),new(.07f,.12f,.07f),new(1,.4f,.08f));}else if(a.Kind==1){for(int i=0;i<5;i++)Tube(scavengeSource,new(i*.18f-.4f,.12f,.4f),new(i*.18f-.4f,.38f,.4f),.09f,Steel);}
   scavengeAudio=new AudioStreamPlayer3D{Stream=TrapSiren(a.Kind),MaxDistance=110,UnitSize=16,VolumeDb=-10};scavengeSource.AddChild(scavengeAudio);scavengeAudio.Play();
  }scavengeAudio.VolumeDb=muted?-80:-10;scavengeSource.Visible=Distance(player.Position,a.Position)<160;foreach(Node child in scavengeSource.GetChildren())if(child is OmniLight3D light)light.LightEnergy=1+2*Mathf.Abs(Mathf.Sin((float)a.Age*8));
  scavengeLabel??=HudLabel(17);scavengeLabel.Visible=Distance(player.Position,a.Position)<110;scavengeLabel.Position=new(380,226);scavengeLabel.Text=TrapName(a.Kind)+" / "+(a.Age<6?$"INFECTED INBOUND / {Math.Max(1,6-(int)a.Age)}s":"NOISE IS DRAWING INFECTED")+"\nF near source / silence it, or run!";
 }
}
