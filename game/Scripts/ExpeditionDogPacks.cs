using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class MigrationScene {
 List<Infected> dogPackSource;
 static int DogPackSize(int tier,uint seed)=>Math.Clamp(tier+1,3,5);
 static string DogPackId(string id)=>Species(id)=="dog"?id[..id.LastIndexOf('/')]:"";
 IEnumerable<Infected> DogMates(Infected dog)=>(dogPackSource??infected).Where(e=>e!=dog&&Species(e.Id)=="dog"&&DogPackId(e.Id)==DogPackId(dog.Id)&&Distance(e.Body.Position,dog.Body.Position)<35);
 void AlertDogPack(Infected dog,Vector3 target){dog.Memory=9;dog.Target=target;foreach(var mate in DogMates(dog)){mate.Memory=9;mate.Target=target;}}
 bool DogSees(Infected dog){if(FieldSmoke(dog.Body.Position+Vector3.Up,player.Position+Vector3.Up))return false;float distance=Distance(dog.Body.Position,player.Position);float range=dog.Memory>0?26:Math.Min(23,(float)game.VisualRange);if(distance>range)return false;var delta=player.Position-dog.Body.Position;if(distance>3&&(-dog.Body.Basis.Z).Dot(delta.Normalized())<-.15)return false;return AnimalSight(dog,range);}
 Vector3 DogGoal(Infected dog){
  if(dog.Memory<=0)return dog.Home+new Vector3((float)Math.Sin(game.Clock*.11+DogPackId(dog.Id).GetHashCodeStable())*5,0,(float)Math.Cos(game.Clock*.11)*5);
  if(Distance(dog.Body.Position,dog.Target)<2.8f)return dog.Target;
  uint slot=(uint)dog.Id.GetHashCodeStable();float angle=(slot%7-3f)*.38f;var toward=dog.Body.Position-dog.Target;toward.Y=0;if(toward.LengthSquared()<.01)return dog.Target;
  return dog.Target+toward.Normalized().Rotated(Vector3.Up,angle)*1.7f;
 }
 void TickDog(Infected dog,double dt){
  dog.Cooldown=Math.Max(0,dog.Cooldown-dt);dog.Stagger=Math.Max(0,dog.Stagger-dt);dog.Memory=Math.Max(0,dog.Memory-dt);
  bool companionTarget=DogCanBeTarget(dog,Distance(dog.Body.Position,player.Position));bool sees=companionTarget||DogSees(dog);if(sees)AlertDogPack(dog,companionTarget?DogPoint:player.Position);
  if(dog.Stagger>0){dog.AttackState="";dog.AttackTime=0;dog.Cooldown=Math.Max(.45,dog.Cooldown);}
  float distance=Distance(dog.Body.Position,companionTarget?DogPoint:player.Position);
  if(dog.AttackState!=""){
   dog.AttackTime-=dt;
   if(dog.AttackTime<=0){
    if(dog.AttackState=="dog-windup"){dog.AttackState="dog-lunge";dog.AttackTime=.28;}
    else if(dog.AttackState=="dog-lunge"){
     if(companionTarget&&distance<1.5f){HurtRescueDog(18);}else if(!companionTarget&&distance<1.5f&&Math.Abs(dog.Body.Position.Y-player.Position.Y)<1&&AnimalSight(dog,1.5f)){game.Hurt(1,game.Life.Wounds.Count==0);game.Notice="Infected hound bite / dodge the crouch before it lunges";if(game.Life.Health<=0){Die();return;}}
     dog.AttackState="dog-recover";dog.AttackTime=.9;dog.Cooldown=1.6;
    }else dog.AttackState="";
   }
  }else if(dog.Stagger<=0&&dog.Cooldown<=0&&distance<2.7&&sees&&!DogMates(dog).Any(e=>e.AttackState is "dog-windup" or "dog-lunge")){
   dog.AttackState="dog-windup";dog.AttackTime=.55;dog.BossAim=companionTarget?DogPoint:player.Position;
  }
  var goal=DogGoal(dog);var direction=dog.AttackState=="dog-lunge"?dog.BossAim-dog.Body.Position:dog.AttackState==""&&dog.Stagger<=0?PursuitDirection(dog,goal,dt):Vector3.Zero;direction.Y=0;
  float speed=dog.AttackState=="dog-lunge"?6:dog.Memory>0?4.1f:.85f;
  var separation=Vector3.Zero;foreach(var mate in DogMates(dog)){var away=dog.Body.Position-mate.Body.Position;away.Y=0;if(away.LengthSquared()>.001&&away.LengthSquared()<1.4f)separation+=away.Normalized()*.7f;}
  var velocity=direction.Length()>.35f?direction.Normalized()*speed+separation:Vector3.Zero;velocity.Y=dog.Body.IsOnFloor()?-.1f:dog.Body.Velocity.Y-18*(float)dt;dog.Body.Velocity=velocity;dog.Body.MoveAndSlide();
  if(dog.Body.Position.Y<game.World.Height(dog.Body.Position.X,dog.Body.Position.Z)-8)dog.Body.Position=Ground(dog.Body.Position.X,dog.Body.Position.Z);
  var face=dog.AttackState!=""?dog.BossAim-dog.Body.Position:direction;if(face.LengthSquared()>.01)dog.Body.Rotation=new(0,Mathf.Atan2(-face.X,-face.Z),0);AnimateAnimal(dog);
 }
 void DogDetails(Node3D root){
  var jaw=new Node3D{Name="DogJaw",Position=new(0,.57f,-.68f)};root.AddChild(jaw);
  Bevel(jaw,new(0,0,0),new(.14f,.055f,.22f),new(.25f,.12f,.11f));
  for(int side=-1;side<=1;side+=2){
   for(int i=0;i<4;i++)Tube(jaw,new(side*.055f,.02f,-.08f+i*.045f),new(side*.055f,.065f,-.08f+i*.045f),.012f,new(.75f,.69f,.45f),.003f);
   Ellipsoid(root,new(side*.085f,.70f,-.60f),new(.018f,.016f,.015f),new(.88f,.24f,.07f));
  }
  Tube(root,new(-.13f,.60f,-.32f),new(.13f,.60f,-.32f),.025f,new(.30f,.17f,.12f));
  Bevel(root,new(0,.53f,-.34f),new(.045f,.07f,.025f),new(.58f,.48f,.24f));
 }
 void PoseDog(Infected dog){var jaw=dog.Visual.GetNodeOrNull<Node3D>("DogJaw");if(jaw!=null)jaw.RotationDegrees=new(dog.AttackState=="dog-lunge"?-28:dog.Memory>0?-14:-3,0,0);dog.Visual.RotationDegrees=new(dog.AttackState=="dog-windup"?-12:dog.AttackState=="dog-lunge"?12:0,0,dog.Stagger>0?14:0);}
 AudioStreamWav DogCue(bool growl){
  int key=growl?5:4;if(enemyCueClips.TryGetValue(key,out var cached))return cached;
  const int rate=22050;double seconds=growl?.52:.42;int count=(int)(rate*seconds);var data=new byte[count*2];var random=new Random(key*171);double noise=0;
  for(int i=0;i<count;i++){double t=i/(double)rate;noise=noise*.8+(random.NextDouble()*2-1)*.2;double envelope=growl?Math.Sin(t/seconds*Math.PI):Math.Exp(-t*12)+.6*Math.Exp(-Math.Pow((t-.21)*26,2));double f=growl?65+12*Math.Sin(t*45):150-90*t;double sample=(Math.Sin(t*f*Math.PI*2)*.35+noise*.65)*envelope;short v=(short)(Math.Clamp(sample,-1,1)*20000);data[i*2]=(byte)v;data[i*2+1]=(byte)(v>>8);}
  return enemyCueClips[key]=new(){Format=AudioStreamWav.FormatEnum.Format16Bits,MixRate=rate,Data=data};
 }
}
