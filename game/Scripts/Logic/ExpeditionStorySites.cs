using System;
using System.Collections.Generic;
// Restores the seeded six-scene layout from ExpeditionAmbience.cs without renderer dependencies.
sealed class ExpeditionStorySite {
 public string Id;public double X,Z,Yaw;public int Kind;
 public static List<ExpeditionStorySite> Nearby(ExpeditionWorld w,double x,double z){var result=new List<ExpeditionStorySite>();int cx=(int)Math.Floor(x/160),cz=(int)Math.Floor(z/160);for(int a=cx-2;a<=cx+2;a++)for(int b=cz-2;b<=cz+2;b++){uint h=w.Hash(a+618,b-923);if(h%5==0)continue;double px=a*160+30+h%100,pz=b*160+30+(h>>9)%100;if(px*px+pz*pz<2500)continue;bool portal=false;foreach(var d in ExpeditionDungeon.Nearby(w,px,pz))if(ExpeditionGame.Distance(px,pz,d.X,d.Z)<16){portal=true;break;}if(!portal)result.Add(new(){Id=w.Seed+"/scene/"+a+":"+b,X=px,Z=pz,Yaw=(h>>16)%360,Kind=(int)((h>>12)%6)});}
  result.Add(new(){Id=w.Seed+"/scene/evacuation",X=-25,Z=20,Yaw=45,Kind=1});result.Add(new(){Id=w.Seed+"/scene/memorial",X=-32,Z=-35,Yaw=12,Kind=3});result.Add(new(){Id=w.Seed+"/scene/shack",X=45,Z=-30,Yaw=60,Kind=4});result.Add(new(){Id=w.Seed+"/scene/triage",X=w.RouteX(-640)+34,Z=-640,Yaw=12,Kind=2});result.Add(new(){Id=w.Seed+"/scene/checkpoint",X=w.RouteX(-1370)+12,Z=-1370,Yaw=0,Kind=5});return result;
 }
}
