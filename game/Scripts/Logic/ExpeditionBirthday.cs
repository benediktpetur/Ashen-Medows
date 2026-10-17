using System;
using System.Runtime.Serialization;

[DataContract] sealed class ExpeditionBirthday {
 [DataMember] public bool CakeTouched,MusicStopped;
 public const double Z=-1216;
 public static double X(ExpeditionWorld world)=>Math.Floor((world.RouteX(Z)+75)/32)*32+16;
 public static bool InLot(ExpeditionWorld world,double x,double z,double margin=0)=>Math.Abs(x-X(world))<7+margin&&z>Z-7-margin&&z<Z+23+margin;
}
