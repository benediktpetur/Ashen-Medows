using System;

sealed class ExpeditionGunHandling {
 public bool Aiming,HoldingBreath,NeedRelease,Airborne;
 public double BreathTime,BreathCooldown,Bloom,SinceShot=10,Age,Speed,RecoilX,RecoilY,SpecialCooldown;
 public int Serial,Streak,Weapon=-1;
 public double BreathLimit(int skill)=>5+Math.Min(2,(skill-1)*.025);
 public void Tick(ExpeditionGame game,double dt,double speed,bool aiming,bool hold,bool grounded=true){
  dt=Math.Clamp(dt,0,.5);Age+=dt;Speed=Math.Max(0,speed);SinceShot+=dt;SpecialCooldown=Math.Max(0,SpecialCooldown-dt);BreathCooldown=Math.Max(0,BreathCooldown-dt);
  if(Weapon!=game.Craft.Weapon){Weapon=game.Craft.Weapon;Bloom=RecoilX=RecoilY=0;Streak=0;HoldingBreath=false;}
  Airborne=!grounded;Aiming=aiming&&ExpeditionGame.IsGun(Weapon);
  if(!hold){NeedRelease=false;BreathTime=Math.Max(0,BreathTime-dt*2);}
  HoldingBreath=Aiming&&hold&&!NeedRelease&&BreathCooldown<=0&&grounded&&Speed<.35&&game.Stamina>12;
  if(HoldingBreath){game.Stamina=Math.Max(0,game.Stamina-dt*11);BreathTime+=dt;if(BreathTime>=BreathLimit(game.Skills.Marksmanship.Level)||game.Stamina<=12){HoldingBreath=false;NeedRelease=true;BreathCooldown=4;}}
  else if(hold&&BreathTime>0){BreathTime=Math.Max(0,BreathTime-dt*.3);}
  Bloom=Math.Max(0,Bloom-dt*(game.Sneaking?2.8:2));
  if(SinceShot>.2){double decay=Math.Exp(-dt*7);RecoilX*=decay;RecoilY*=decay;}
  if(SinceShot>1.1)Streak=0;
 }
 public double Sway=>!Aiming?0:(HoldingBreath?.012:BreathCooldown>0?.28:.14)*(1+Speed*.08);
 public double Spread(ExpeditionGame game)=>((Aiming?.10:.55)+Bloom*.30+Speed*.10+(Airborne?1.5:0))*(game.Sneaking?.72:1)*(HoldingBreath?.6:1)*game.SoulSpread*game.PartFactor(game.Craft.Weapon,p=>p.Spread);
 public (double X,double Y) AimOffset()=> (RecoilX+Math.Sin(Age*1.9)*Sway,RecoilY+Math.Sin(Age*2.7+.7)*Sway);
 public void Shot(int weapon){
  if(Weapon!=weapon){Weapon=weapon;Streak=0;Bloom=0;}
  if(SinceShot>1.1)Streak=0;
  double scale=ExpeditionGame.Shotgun(weapon)?1.5:weapon==29?.5:weapon is 28 or 31?1.2:weapon is 1 or 2 or 4?1.1:weapon==3?.8:.65;
  RecoilX=Math.Sin(Streak*.9)*Math.Min(1.4,Streak*.13)*scale;
  RecoilY=(.15+Math.Min(3.5,Streak*.24))*scale;
  Streak++;Serial++;SinceShot=0;Bloom=Math.Min(4,Bloom+(ExpeditionGame.Shotgun(weapon)?1.15:weapon==29?.35:weapon==3?.55:.7));
 }
 public (double X,double Y) ShotOffset(ExpeditionGame game,int pellet){
  uint h=game.World.Hash(Serial*31+pellet*71,game.Craft.Weapon+627);
  double angle=(h%65536)/65536.0*Math.PI*2,r=Math.Sqrt((h>>16)/65535.0)*Spread(game);
  var aim=AimOffset();return(aim.X-RecoilX*(1-game.PartFactor(game.Craft.Weapon,p=>p.Recoil)*(1-game.UpgradePower(1)/100.0))+Math.Cos(angle)*r,aim.Y-RecoilY*(1-game.PartFactor(game.Craft.Weapon,p=>p.Recoil)*(1-game.UpgradePower(1)/100.0))+Math.Sin(angle)*r);
 }
}
sealed partial class ExpeditionGame {public readonly ExpeditionGunHandling Handling=new();}
