using Godot;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
public partial class MigrationScene {
 sealed class CoverProbe{public double Due;}
 readonly ConditionalWeakTable<ExpeditionGame,CoverProbe> weatherProbes=new();
 MultiMeshInstance3D rainVisual;AudioStreamPlayer rainAudio;
 void ProbeWeatherCover(double dt){if(!JourneyEnabled)return;var probe=weatherProbes.GetOrCreateValue(game);probe.Due-=dt;if(probe.Due>0)return;probe.Due=.5;
  if(dungeon!=null){game.WeatherSheltered=true;game.WeatherHeated=false;return;}
  var ray=PhysicsRayQueryParameters3D.Create(player.Position+Vector3.Up*1.1f,player.Position+Vector3.Up*10);ray.Exclude=new(){player.GetRid()};game.WeatherSheltered=GetWorld3D().DirectSpaceState.IntersectRay(ray).Count>0;
  game.WeatherHeated=Community.Established&&Community.Improvements>=2&&Community.Food>0&&ExpeditionGame.Distance(player.Position.X,player.Position.Z,Community.X,Community.Z)<10;
  if(!game.WeatherHeated)game.WeatherHeated=game.World.Changes.FieldSites.Any(s=>s.Secured&&s.RestAt>game.World.Changes.Time&&ExpeditionContentSite.All[s.Index].Mode=="rest"&&ExpeditionContentSite.Place(game.World,s.Index) is var p&&ExpeditionGame.Distance(player.Position.X,player.Position.Z,p.X,p.Z)<12);
 }
 void DrawExpeditionWeather(double dt){if(!JourneyEnabled||sun==null)return;int weather=game.Weather;bool outside=dungeon==null;
  if(outside){if(weather!=0)sun.LightEnergy*=weather==1?.68f:weather==2?.78f:.86f;atmosphere.Environment.FogDensity=weather==2?.009f:weather==1?.0033f:.0016f;if(weather==2)atmosphere.Environment.FogLightColor=new(.34f,.39f,.39f);}
  bool rain=outside&&weather==1&&!game.WeatherSheltered;
  if(rainVisual==null&&rain&&DisplayServer.GetName()!="headless"){
   var mesh=new BoxMesh{Size=new(.012f,.5f,.012f)};var material=new StandardMaterial3D{AlbedoColor=new(.54f,.66f,.73f,.45f),Transparency=BaseMaterial3D.TransparencyEnum.Alpha,ShadingMode=BaseMaterial3D.ShadingModeEnum.Unshaded};rainVisual=new MultiMeshInstance3D{Name="LocalRain",Multimesh=new MultiMesh{TransformFormat=MultiMesh.TransformFormatEnum.Transform3D,Mesh=mesh,InstanceCount=100},MaterialOverride=material,CastShadow=GeometryInstance3D.ShadowCastingSetting.Off};AddChild(rainVisual);
   rainAudio=new AudioStreamPlayer{Name="Rainfall",Stream=RainfallClip(),VolumeDb=-25};AddChild(rainAudio);if(!testing)rainAudio.Play();
  }
  if(rainVisual!=null){rainVisual.Visible=rain;float time=(float)(Time.GetTicksMsec()/1000.0);for(int i=0;rain&&i<100;i++){float x=(i*17%97)/97f*18-9,z=(i*31%89)/89f*18-9,y=10-((time*11+i*.37f)%12);rainVisual.Multimesh.SetInstanceTransform(i,new Transform3D(Basis.FromEuler(new(.1f,0,.15f)),player.Position+new Vector3(x,y,z)));}rainAudio.StreamPaused=!rain||paused||muted;}
 }
 AudioStreamWav RainfallClip(){const int rate=22050,count=rate*4;var bytes=new byte[count*2];var random=new Random(197);double previous=0;for(int i=0;i<count;i++){double n=random.NextDouble()*2-1;previous=previous*.65+n*.35;short sample=(short)((n*.22+previous*.3)*12000);bytes[i*2]=(byte)sample;bytes[i*2+1]=(byte)(sample>>8);}return new(){Format=AudioStreamWav.FormatEnum.Format16Bits,MixRate=rate,Data=bytes,LoopMode=AudioStreamWav.LoopModeEnum.Forward,LoopEnd=count};}
}
