// Ported engine-independent Expedition rules. Rendering and input live in Godot adapters.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;


[DataContract] sealed class ExpeditionSkill {
 [DataMember] public int Level=1;[DataMember] public double Xp;
 public double Need{get{return 100+Level*50;}}
 public void Gain(double xp){Xp+=Math.Max(0,xp);while(Xp>=Need&&Level<100){Xp-=Need;Level++;}}
}
[DataContract] sealed class ExpeditionSurvivor {
 [DataMember] public ExpeditionSkill Stamina=new ExpeditionSkill(),Marksmanship=new ExpeditionSkill(),Crafting=new ExpeditionSkill(),Strength=new ExpeditionSkill(),Agility=new ExpeditionSkill(),Survival=new ExpeditionSkill();
 [DataMember] public List<string> Defeated=new List<string>();[DataMember] public int Deaths;
 public void Normalize(){if(Strength==null)Strength=new ExpeditionSkill();if(Agility==null)Agility=new ExpeditionSkill();if(Survival==null)Survival=new ExpeditionSkill();if(Stamina==null)Stamina=new ExpeditionSkill();if(Marksmanship==null)Marksmanship=new ExpeditionSkill();if(Crafting==null)Crafting=new ExpeditionSkill();if(Defeated==null)Defeated=new List<string>();foreach(var s in new[]{Stamina,Marksmanship,Crafting,Strength,Agility,Survival}){s.Level=Math.Max(1,Math.Min(100,s.Level));if(double.IsNaN(s.Xp)||double.IsInfinity(s.Xp))s.Xp=0;s.Xp=Math.Max(0,Math.Min(s.Need-.001,s.Xp));}}
 public void Die(){Stamina.Xp=Marksmanship.Xp=Crafting.Xp=Strength.Xp=Agility.Xp=Survival.Xp=0;Deaths++;}
 public static void Check(){var s=new ExpeditionSurvivor();s.Stamina.Level=2;s.Stamina.Xp=s.Stamina.Need*.8;s.Marksmanship.Level=7;s.Marksmanship.Xp=35;s.Crafting.Level=4;s.Crafting.Xp=80;s.Die();if(s.Stamina.Level!=2||s.Stamina.Xp!=0||s.Marksmanship.Level!=7||s.Marksmanship.Xp!=0||s.Crafting.Level!=4||s.Crafting.Xp!=0)throw new Exception("Death removed earned levels or retained partial XP");s.Stamina.Gain(s.Stamina.Need+7);if(s.Stamina.Level!=3||s.Stamina.Xp!=7)throw new Exception("Skill level-up error");var saved=Json.Read<ExpeditionSurvivor>(Json.Write(s));if(saved.Deaths!=1||saved.Stamina.Level!=3)throw new Exception("Survivor save error");if(ExpeditionCombatMath.Sphere(0,0,0,0,0,-1,0,0,-10,.5)<9||ExpeditionCombatMath.Sphere(0,0,0,0,0,-1,5,0,-10,.5)>=0)throw new Exception("Shot intersection error");}
}
static class ExpeditionCombatMath {
 public static double Sphere(double x,double y,double z,double dx,double dy,double dz,double cx,double cy,double cz,double radius){double ox=cx-x,oy=cy-y,oz=cz-z,along=ox*dx+oy*dy+oz*dz,dist=ox*ox+oy*oy+oz*oz-along*along;if(along<0||dist>radius*radius)return -1;return Math.Max(0,along-Math.Sqrt(radius*radius-dist));}
}
