using System;
using System.Runtime.Serialization;
[DataContract] sealed class ExpeditionCrash {
 [DataMember] public bool Asked,Opened,Toy,Given;
 [DataMember] public int Choice;
 public const double Z=-1060;
 public static double X(ExpeditionWorld w)=>w.RouteX(Z);
 public static bool InLot(ExpeditionWorld w,double x,double z,double margin=0)=>Math.Abs(x-X(w))<12+margin&&Math.Abs(z-Z)<16+margin;
}
partial class ExpeditionWorldChanges { [DataMember] public ExpeditionCrash Crash=new(); }

