using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
public partial class MigrationScene {
 async Task CaptureCampLifeScene(){
  CloseMenu();ClearEnemies();game.World.Changes.Time=900;weaponRoot.Visible=false;hudRoot.Visible=false;Teleport(ExpeditionWorld.SpawnX-40,ExpeditionWorld.SpawnZ+35);var c=Community;c.Established=true;c.X=player.Position.X;c.Z=player.Position.Z;c.Food=c.Water=20;c.Improvements=3;c.Survivors.Clear();game.Building.Pieces.Clear();
  int cot=Array.FindIndex(BuildDefinition.All,d=>d.Function=="bed");string[] names={"Mara Holt","Ivar Reed","Nora Vale"};
  for(int i=0;i<3;i++){double x=c.X+(i-1)*4,z=c.Z-4,y=game.World.Height(x,z);BuildPiece Add(int kind,double px,double pz,double py,double yaw=0){var piece=new BuildPiece{Id=Guid.NewGuid().ToString(),Kind=kind,X=px,Z=pz,Y=py,Yaw=yaw,Health=BuildDefinition.All[kind].Durability};game.Building.Pieces.Add(piece);return piece;}Add(0,x,z,y);Add(7,x,z,y+2.7);Add(2,x,z-1.5,y);Add(2,x-1.5,z,y,90);var bed=Add(cot,x,z,y+.25);c.Survivors.Add(new(){Id="preview/camp/"+i,Name=names[i],Encounter=i,State="member",BedId=bed.Id,Job=i==0?"wood":i==1?"supplies":"guard",X=x,Y=y+.35,Z=z+1.7,Morale=80});}
  SyncStructures();DrawCommunity(0);campLifeSignature="";campLifeScan=0;DrawCampLife(3);paused=true;hudRoot.Visible=false;camera.GlobalPosition=Ground(c.X,c.Z)+new Vector3(10,5.5f,12);camera.LookAt(Ground(c.X,c.Z)+new Vector3(0,1.4f,-1));await ArtShot("journeys-05-camp-life");
 }
}
