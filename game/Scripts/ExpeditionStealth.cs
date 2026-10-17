using Godot;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class MigrationScene {
 sealed class SoundTrail {public Vector3 Position;public bool Ready;public double Elapsed;}
 readonly ConditionalWeakTable<ExpeditionGame,SoundTrail> soundTrails=new();
 Label noiseLabel;
 void Noise(double range){if(CoopClient)return;HearNoise(player.Position,game.RaiseNoise(range));}
 void HearNoise(Vector3 position,double radius){
  if(CoopClient)return;
  foreach(var e in infected){
   if(PassiveStoryEnemy(e)||Species(e.Id)!=""&&Species(e.Id)!="dog")continue;
   double distance=e.Body.Position.DistanceTo(position);if(distance>radius)continue;
   var query=PhysicsRayQueryParameters3D.Create(position+Vector3.Up,e.Body.Position+Vector3.Up);
   query.Exclude=PursuitExclusions();
   double audible=GetWorld3D().DirectSpaceState.IntersectRay(query).Count>0?radius*.45:radius;
   if(distance>audible)continue;
   e.Memory=Math.Max(e.Memory,Math.Clamp(4+radius*.06,4,14));e.Target=position;
  }
 }
 void TickStealth(double dt,bool sprint){
  game.FadeNoise(dt);var trail=soundTrails.GetOrCreateValue(game);
  trail.Elapsed+=dt;
  double moved=trail.Ready?Distance(trail.Position,player.Position):0;
  trail.Ready=true;trail.Position=player.Position;
  bool moving=moved>.005&&moved<Math.Max(3,dt*14);
  double movementTime=coopActor>0?Math.Clamp(trail.Elapsed,.001,Math.Max(.25,dt)):dt;
  double speed=moving?moved/Math.Max(.001,movementTime):0;
  bool fast=!game.Sneaking&&sprint&&speed>6;
  double scale=Math.Clamp(speed/((game.Sneaking?2.2:fast?9:4.8)*game.Speed),.25,1.15);
  var surface=FootstepSurface(player.Position);game.FootstepGround=surface.Name;
  game.MovementMode=moving?(game.Sneaking?"LOW":fast?"HIGH":"MID"):"STILL";
  game.FootstepTime-=dt;
  if(!CoopClient&&moving&&game.FootstepTime<=0){Noise(game.MovementSound(fast,surface.Multiplier,scale));game.FootstepTime=(game.Sneaking?.8:fast?.3:.5)/scale;}
  if(!CoopClient)game.TrainSneaking(movementTime,moving,infected.Any(e=>(Species(e.Id)==""||Species(e.Id)=="dog")&&!PassiveStoryEnemy(e)&&e.Memory<=0&&Distance(e.Body.Position,player.Position)<24&&Math.Abs(e.Body.Position.Y-player.Position.Y)<3));
  if(moving)trail.Elapsed=0;

 }
 void DrawNoiseHud(){
  if(noiseLabel==null){noiseLabel=HudLabel(13);}
  noiseLabel.Position=new(34,68);
  int bars=(int)Math.Ceiling(game.NoiseLevel/10);
  noiseLabel.Text=$"{(game.Sneaking?"CROUCH":"NOISE")} [{new string('|',bars)}{new string('.',10-bars)} ] {game.MovementMode} / {game.FootstepGround}";
  noiseLabel.Modulate=game.NoiseLevel>65?new Color(1,.48f,.3f):game.Sneaking?new Color(.65f,.85f,.66f):new Color(.8f,.8f,.72f);
 }
}
