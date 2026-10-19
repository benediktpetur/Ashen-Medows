using Godot;
using System;
using System.Collections.Generic;

public partial class MigrationScene {
 readonly Dictionary<string,AudioStreamWav> footstepClips=new();
 AudioStreamWav FootstepClip(string surface){
  if(footstepClips.TryGetValue(surface,out var clip))return clip;
  const int rate=22050;var data=new byte[rate/4*2];var random=new Random(91);double low=0;
  bool leaves=surface is "BUSH" or "WOODLAND";
  for(int i=0;i<data.Length/2;i++){
   double t=i/(double)rate,n=random.NextDouble()*2-1;low=low*.83+n*.17;
   double crunch=leaves?(n-low)*(.16+.12*Math.Sin(t*180))*(surface=="BUSH"?1.5:1):low*.16;
   short value=(short)(Math.Clamp((crunch+Math.Sin(t*110)*.14)*Math.Exp(-t*(leaves?13:28)),-1,1)*18000);
   data[i*2]=(byte)value;data[i*2+1]=(byte)(value>>8);
  }
  return footstepClips[surface]=new AudioStreamWav{Format=AudioStreamWav.FormatEnum.Format16Bits,MixRate=rate,Data=data};
 }
 // Render and hearing use the same seeded foliage positions, including remote simulation chunks.
 List<Vector3> BrushPositions(int cx,int cz){
  var result=new List<Vector3>();var random=new Random(unchecked((int)game.World.Hash(cx+8041,cz-2303)));
  for(int i=0;i<28;i++){
   double x=cx*64+3+random.NextDouble()*58,z=cz*64+3+random.NextDouble()*58;
   int tier=game.World.Tier(x,z);
   if(tier>2||tier==1&&i>1||!game.World.Playable(x,z)||Math.Abs(x-game.World.RouteX(z))<9||game.World.Changes.OnPath(x,z))continue;
   if(InEncounterLot(x,z)||InCrashLot(x,z)||InCheckpointLot(x,z)||InBirthdayLot(x,z)||InLetterLot(x,z)||InFeverLot(x,z)||InWoodlandCamp(x,z)||InFamilyLot(x,z)||InMill(x,z,35))continue;
   if(props.Exists(p=>(p.Kind==0||p.Kind==7)&&Distance(p.Position,Ground(x,z))<2))continue;
   result.Add(Ground(x,z));
  }
  return result;
 }
 void SpawnNoiseBrush(Node3D chunk,int cx,int cz,bool simulation){
  var positions=BrushPositions(cx,cz);
  // Metadata stays on streamed chunk roots so clearing chunks also clears their sound volumes.
  chunk.SetMeta("noise_brush",positions.ToArray());
  if(simulation)return;
  if(positions.Count==0)return;
  var leaves=new MultiMesh{TransformFormat=MultiMesh.TransformFormatEnum.Transform3D,UseColors=true,Mesh=new SphereMesh{Radius=1,Height=2,RadialSegments=12,Rings=6},InstanceCount=positions.Count*7};
  int index=0;
  foreach(var point in positions)for(int i=0;i<7;i++){
   float angle=i*2.4f;var tip=new Vector3(Mathf.Sin(angle)*.7f,.45f+(i%3)*.14f,Mathf.Cos(angle)*.7f);
   leaves.SetInstanceTransform(index,new Transform3D(Basis.Identity.Scaled(new Vector3(.48f,.33f,.48f)),point+tip));
   leaves.SetInstanceColor(index++,i%2==0?new Color(.9f,.93f,.9f):new Color(1.15f,1.1f,1));
  }
  chunk.AddChild(new MultiMeshInstance3D{Name="RustlingBushes",Multimesh=leaves,MaterialOverride=WoodlandLeaves(),VisibilityRangeEnd=150});

 }
 (string Name,double Multiplier) FootstepSurface(Vector3 position){
  if(dungeon!=null||Math.Abs(position.Y-game.World.Height(position.X,position.Z))>1.2)return("FLOOR",1.1);
  if(game.World.Changes.OnPath(position.X,position.Z))return("PATH",.85);
  if(Math.Abs(position.X-game.World.RouteX(position.Z))<5)return game.World.Tier(position.X,position.Z)>=3?("ROAD",1.2):("DIRT",.9);
  int cx=ExpeditionWorld.Cell(position.X),cz=ExpeditionWorld.Cell(position.Z);
  for(int x=cx-1;x<=cx+1;x++)for(int z=cz-1;z<=cz+1;z++){
   if(!chunks.TryGetValue((x,z),out var chunk)||!chunk.HasMeta("noise_brush"))continue;
   foreach(var point in chunk.GetMeta("noise_brush").AsVector3Array())
    if(Distance(point,position)<1.25f&&Math.Abs(point.Y-position.Y)<1.2)return("BUSH",2.4);
  }
  return game.World.Tier(position.X,position.Z)==2?("WOODLAND",1.5):("GRASS",1);
 }
}
