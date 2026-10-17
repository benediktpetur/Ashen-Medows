using Godot;
using System;
public partial class MigrationScene {
 AudioStreamPlayer effects,ambience;bool muted;double footstepClock;
 AudioStreamWav Tone(int seed,double seconds,bool wind=false){const int rate=22050;int count=(int)(rate*seconds);byte[] data=new byte[count*2];var random=new Random(seed);double low=0;for(int i=0;i<count;i++){double t=i/(double)rate,n=random.NextDouble()*2-1;low=low*.96+n*.04;double sample=wind?(low*.25+Math.Sin(t*63)*.009):Math.Exp(-t*25)*(n*.45+Math.Sin(t*170)*.28);short value=(short)(Math.Clamp(sample,-1,1)*18000);data[i*2]=(byte)value;data[i*2+1]=(byte)(value>>8);}return new AudioStreamWav{Format=AudioStreamWav.FormatEnum.Format16Bits,MixRate=rate,Data=data,LoopMode=wind?AudioStreamWav.LoopModeEnum.Forward:AudioStreamWav.LoopModeEnum.Disabled,LoopEnd=count};}
 void InitAudio(){effects=new AudioStreamPlayer{VolumeDb=-12};ambience=new AudioStreamPlayer{VolumeDb=-12,Stream=Tone(37,6,true)};AddChild(effects);AddChild(ambience);if(!testing)ambience.Play();}
 void Sound(int kind){if(muted||testing||effects==null)return;effects.VolumeDb=kind==3&&game.Sneaking?-24:-12;effects.Stream=kind==3?FootstepClip(game.FootstepGround):Tone(kind,kind==1?.18:.1);effects.Play();}
 void AudioTick(bool moving,double dt){if(effects==null)return;footstepClock-=dt;if(moving&&footstepClock<=0){Sound(3);footstepClock=game.Sneaking?.8:game.MovementMode=="HIGH"?.3:.5;}ambience.StreamPaused=paused||muted;}
}
