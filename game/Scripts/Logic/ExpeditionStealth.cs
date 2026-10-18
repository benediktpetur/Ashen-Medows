using System;

// Session-only sound and stance. Skill XP belongs to the portable survivor instead.
sealed partial class ExpeditionGame {
 public double NoiseLevel,FootstepTime;
 public bool Sneaking;public string FootstepGround="GRASS",MovementMode="STILL";
 public double SneakReduction=>Math.Min(.55,(Skills.Sneaking.Level-1)*.008);
 public double VisualRange=>Sneaking?24*(.6-SneakReduction*.5):24;
 public double MovementSound(bool sprint,double terrain=1,double speedScale=1)=>(Sneaking?3.5*(1-SneakReduction):sprint?(Talent("quiet")>0?14.4:24):10)*ArmorQuiet*Math.Clamp(terrain,.5,3)*Math.Clamp(speedScale,.25,1.15);
 public double RaiseNoise(double radius){NoiseLevel=Math.Clamp(NoiseLevel+radius*.4,0,100);return radius*(1+NoiseLevel*.006);}
 public void FadeNoise(double dt){NoiseLevel=Math.Max(0,NoiseLevel-Math.Max(0,dt)*6);}
 public void TrainSneaking(double dt,bool moving,bool unseenThreat){if(Sneaking&&moving&&unseenThreat)Skills.Sneaking.Gain(Math.Clamp(dt,0,.5)*3);}
}
