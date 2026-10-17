using System;
using System.Runtime.Serialization;

[DataContract] sealed class ExpeditionCheckpoint {
 [DataMember] public bool Asked,Paid,Hostile,Revealed,DeliveredOnce,Donated;
 [DataMember] public double Watch,DeliveryClock,RobberyTime,LineTime;
 [DataMember] public int Robbery,Revision;
 [DataMember] public string Line="";
 public const double Z=-1520;
 public static double X(ExpeditionWorld world)=>world.RouteX(Z);
 public static bool InLot(ExpeditionWorld world,double x,double z,double margin=0)=>Math.Abs(x-X(world))<15+margin&&Math.Abs(z-Z)<39+margin;
 public static double CreekCut(ExpeditionWorld world,double x,double z){double dx=Math.Abs(x-X(world)),dz=Math.Abs(z-Z);return dx>32||dz>36?0:2.8*Math.Pow(Math.Cos(dz/36*Math.PI/2),2)*Math.Min(1,(32-dx)/8);}
}
