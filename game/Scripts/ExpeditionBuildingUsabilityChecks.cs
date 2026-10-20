using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 async void RunBuildingUsabilityChecks(){try{BuildingUsabilityChecks();
 game.NoDiskSave=true;game.Building.Pieces.Clear();var anchor=new BuildPiece{Id="check/anchor",Kind=1,Y=200};game.Building.Pieces.Add(anchor);SyncStructures();player.Position=new(0,200,4);camera.Position=new(0,1.65f,0);camera.LookAt(new Vector3(0,200.2f,1.2f));
 await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
 MigrationChecks.Assert(AimedBuildPiece()==anchor,"Real floor collider resolves its owning blueprint");snap=true;heightBuild=3;selectedBuild=1;UpdateGhost();MigrationChecks.Assert(Math.Abs(ghost.Y-200)<.001&&Math.Abs(ghost.Z-3)<.001,"Actual placement ghost anchors adjacent floors despite stale height adjustment");
 heightBuild=0;selectedBuild=2;UpdateGhost();MigrationChecks.Assert(Math.Abs(ghost.Z-1.5)<.001&&Math.Abs(ghost.Y-200.2)<.001,"Actual wall ghost attaches at floor edge");
 demolitionMode=true;UpdateGhost();MigrationChecks.Assert(placement.Reason.Contains("DEMOLISH")&&ghostMesh!=null,"Demolition highlights real aimed structure");RemoveStructure();MigrationChecks.Assert(game.Building.Pieces.Count==0,"Demolition removes aimed structure and persists change");demolitionMode=false;ClearGhost();CloseMenu();GD.Print("PASS / real collider targeting, anchored ghost elevation, wall edge preview and demolition");SetProcess(false);SetPhysicsProcess(false);ClearChildren(worldRoot);ClearChildren(terrainRoot);ClearChildren(buildRoot);props.Clear();chunks.Clear();await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);GetTree().Quit();}catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}}
 void BuildingUsabilityChecks(){
  for(int turn=0;turn<4;turn++){
   double yaw=turn*90;var ground=new BuildPiece{Kind=0,Yaw=yaw};var b=new ExpeditionBuilding((x,z)=>0);b.Pieces.Add(ground);
   double a=yaw*Math.PI/180;var wall=ExpeditionBuildSnap.Connect(ground,2,Math.Sin(a)*1.4,Math.Cos(a)*1.4,0,1,0,0);
   MigrationChecks.Assert(b.Placement(wall).Valid,"Wall snaps supported at rotated deck edge / "+yaw);b.Pieces.Add(wall);
   var roof=ExpeditionBuildSnap.Connect(wall,7,wall.X,wall.Z,0,1,0,0,b.Pieces);MigrationChecks.Assert(b.Placement(roof).Valid,"Roof connects to wall top / "+yaw);MigrationChecks.Assert(Math.Abs(roof.X)<.001&&Math.Abs(roof.Z)<.001,"Roof centers over room rather than outside wall");
   var next=ExpeditionBuildSnap.Connect(ground,0,0,0,Math.Cos(a),0,-Math.Sin(a),0);MigrationChecks.Assert(b.Placement(next).Valid,"Foundations connect flush when rotated / "+yaw);
  }
  foreach(double yaw in new[]{0.0,37.0,90.0}){
   var anchor=new BuildPiece{Kind=1,Y=4,Yaw=yaw};double a=yaw*Math.PI/180;
   foreach(int side in new[]{-1,1}){
    var next=ExpeditionBuildSnap.Connect(anchor,1,Math.Sin(a)*side*1.3,Math.Cos(a)*side*1.3,0,1,0,0);
    MigrationChecks.Assert(Math.Abs(next.Y-anchor.Y)<.001&&Math.Abs(next.Yaw-yaw)<.001&&Math.Abs(ExpeditionGame.Distance(next.X,next.Z,anchor.X,anchor.Z)-3)<.001,"Top face floor sockets preserve elevation and rotation");
    var wall=ExpeditionBuildSnap.Connect(anchor,2,Math.Sin(a)*side*1.3,Math.Cos(a)*side*1.3,0,1,0,0);
    MigrationChecks.Assert(Math.Abs(ExpeditionGame.Distance(wall.X,wall.Z,0,0)-1.5)<.001&&Math.Abs(wall.Y-4.2)<.001,"Wall centered on floor perimeter");
   }
   var center=ExpeditionBuildSnap.Connect(anchor,2,0,0,0,1,0,0);MigrationChecks.Assert(ExpeditionGame.Distance(center.X,center.Z,0,0)>1.4,"Center aim still selects an edge");
  }
  int[] costs=new int[7];foreach(int kind in new[]{0,2,2,7})for(int i=0;i<7;i++)costs[i]+=BuildDefinition.All[kind].Cost[i];
  MigrationChecks.Assert(costs[0]==13&&costs[1]==2&&costs[2]==4,"Starter shell is 13 timber, 2 stone, 4 fiber");
  for(int t=2;t<=5;t++){var d=BuildDefinition.All.First(d=>d.Function=="structure"&&d.Tier==t);MigrationChecks.Assert(d.Cost[t+1]>0,"Each tier requires its zone resource");}
  foreach(var d in BuildDefinition.All.Where(d=>d.Function.StartsWith("decor-"))){var piece=new BuildPiece{Kind=Array.IndexOf(BuildDefinition.All,d)};var visual=Structure(piece);MigrationChecks.Assert(visual.GetChildCount()>0,"Decoration has real model / "+d.Name);visual.Free();}
  GD.Print("PASS / building usability: rotated connections, supported roofs, starter material budget, tier gates and decoration models");
 }
}
