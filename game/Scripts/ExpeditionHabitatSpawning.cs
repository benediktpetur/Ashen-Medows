using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 void SpawnSurfacePopulation(Vector3 center,int cap,float radius){int cx=(int)Math.Floor(center.X/64),cz=(int)Math.Floor(center.Z/64);int count=infected.Count(e=>Distance(e.Body.Position,center)<radius);for(int dx=-2;dx<=2;dx++)for(int dz=-2;dz<=2;dz++){int x=cx+dx,z=cz+dz;uint h=game.World.Hash(x+898,z-477);double xx=x*64+8+h%48,zz=z*64+8+(h>>8)%48;int tier=game.World.Tier(xx,zz),pack=game.World.EncounterSize(xx,zz,h,game.Night);for(int i=0;i<pack&&count<cap&&infected.Count<(CoopHost?CoopMaxEnemies:85);i++){double a=(h%628)*.01+i*2.4,ex=xx+Math.Sin(a)*(2+i),ez=zz+Math.Cos(a)*(2+i);var p=Ground(ex,ez);if(Distance(p,center)>radius||Distance(p,center)<22||game.World.SpawnHeat(ex,ez)==0||!ChunkLoaded(p))continue;int before=infected.Count;AddEnemy($"habitat/{game.Save.Seed}/{x}:{z}/{i}",ex,ez,game.World.Tier(ex,ez));if(infected.Count>before)count++;}}}
 void SpawnSoloHabitats(){foreach(var e in infected.ToArray())if(Distance(e.Body.Position,player.Position)>180||e.Night>0&&!game.Night)RemoveEnemy(e);SpawnSurfacePopulation(player.Position,60,112);if(game.Night&&game.World.Tier(player.Position.X,player.Position.Z)>=2&&game.Clock>=nightClock){nightClock=game.Clock+45;for(int i=0;i<4&&infected.Count<75;i++){double a=game.Clock+i*1.7;AddEnemy("night/"+(int)game.World.Changes.Time+"/"+i,player.Position.X+Math.Sin(a)*32,player.Position.Z+Math.Cos(a)*32,game.World.Tier(player.Position.X,player.Position.Z),i%2+1);}}}
}

