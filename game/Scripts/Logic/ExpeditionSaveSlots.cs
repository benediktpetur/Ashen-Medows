using System;
using System.Linq;

sealed partial class ExpeditionGame {
 public int[] SavedWorldSeeds()=>Save.Buildings.Select(world=>world.Seed)
  .Concat(Profile.Worlds.Select(world=>world.Seed)).Append(Save.Seed).Distinct().OrderBy(seed=>seed).ToArray();

 public string SavedWorldName(int seed){
  var world=Save.Buildings.Find(world=>world.Seed==seed);
  return string.IsNullOrWhiteSpace(world?.Name)?"World "+seed:world.Name;
 }

 public bool SelectSavedWorld(int seed,string newName=null){
  if(seed==Save.Seed&&newName==null)return Healthy;
  return Change(()=>{
   var current=Save.Buildings.Find(world=>world.Seed==Save.Seed);
   current.X=Save.X;
   current.Z=Save.Z;
   var next=Save.Buildings.Find(world=>world.Seed==seed);
   Save.Seed=seed;
   Save.X=next?.X??ExpeditionWorld.HomeX;
   Save.Z=next?.Z??ExpeditionWorld.HomeZ;
   Normalize();
   if(newName!=null)Save.Buildings.Find(world=>world.Seed==seed).Name=newName;
  });
 }
}
