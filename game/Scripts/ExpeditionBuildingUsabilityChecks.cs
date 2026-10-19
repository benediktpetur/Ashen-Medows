using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 async void RunBuildingUsabilityChecks(){try{BuildingUsabilityChecks();SetProcess(false);SetPhysicsProcess(false);ClearChildren(worldRoot);ClearChildren(terrainRoot);ClearChildren(buildRoot);props.Clear();chunks.Clear();await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);GetTree().Quit();}catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}}
 void BuildingUsabilityChecks(){
  for(int turn=0;turn<4;turn++){
   double yaw=turn*90;var ground=new BuildPiece{Kind=0,Yaw=yaw};var b=new ExpeditionBuilding((x,z)=>0);b.Pieces.Add(ground);
   double a=yaw*Math.PI/180;var wall=ExpeditionBuildSnap.Connect(ground,2,Math.Sin(a)*1.4,Math.Cos(a)*1.4,0,1,0,0);
   MigrationChecks.Assert(b.Placement(wall).Valid,"Wall snaps supported at rotated deck edge / "+yaw);b.Pieces.Add(wall);
   var roof=ExpeditionBuildSnap.Connect(wall,7,wall.X,wall.Z,0,1,0,0,b.Pieces);MigrationChecks.Assert(b.Placement(roof).Valid,"Roof connects to wall top / "+yaw);MigrationChecks.Assert(Math.Abs(roof.X)<.001&&Math.Abs(roof.Z)<.001,"Roof centers over room rather than outside wall");
   var next=ExpeditionBuildSnap.Connect(ground,0,0,0,Math.Cos(a),0,-Math.Sin(a),0);MigrationChecks.Assert(b.Placement(next).Valid,"Foundations connect flush when rotated / "+yaw);
  }
  int[] costs=new int[7];foreach(int kind in new[]{0,2,2,7})for(int i=0;i<7;i++)costs[i]+=BuildDefinition.All[kind].Cost[i];
  MigrationChecks.Assert(costs[0]==13&&costs[1]==2&&costs[2]==4,"Starter shell is 13 timber, 2 stone, 4 fiber");
  for(int t=2;t<=5;t++){var d=BuildDefinition.All.First(d=>d.Function=="structure"&&d.Tier==t);MigrationChecks.Assert(d.Cost[t+1]>0,"Each tier requires its zone resource");}
  foreach(var d in BuildDefinition.All.Where(d=>d.Function.StartsWith("decor-"))){var piece=new BuildPiece{Kind=Array.IndexOf(BuildDefinition.All,d)};var visual=Structure(piece);MigrationChecks.Assert(visual.GetChildCount()>0,"Decoration has real model / "+d.Name);visual.Free();}
  GD.Print("PASS / building usability: rotated connections, supported roofs, starter material budget, tier gates and decoration models");
 }
}
