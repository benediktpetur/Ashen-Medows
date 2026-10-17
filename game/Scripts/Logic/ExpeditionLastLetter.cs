using System;
using System.Runtime.Serialization;

[DataContract] sealed class ExpeditionLastLetter {
 [DataMember] public int Stage;
 [DataMember] public double Elapsed,LineTime;
 [DataMember] public bool Departed;
 [DataMember] public string Line="";
 public const double ChurchZ=-1680,HomeZ=-1776;
 public static double ChurchX(ExpeditionWorld world)=>Math.Floor((world.RouteX(ChurchZ)+105)/32)*32+16;
 public static double HomeX(ExpeditionWorld world)=>Math.Floor((world.RouteX(HomeZ)-145)/32)*32+16;
 public static bool InLots(ExpeditionWorld world,double x,double z,double margin=0)=>
  Math.Abs(x-ChurchX(world))<10+margin&&z>ChurchZ-14-margin&&z<ChurchZ+32+margin||
  Math.Abs(x-HomeX(world))<7+margin&&z>HomeZ-7-margin&&z<HomeZ+23+margin;
}
