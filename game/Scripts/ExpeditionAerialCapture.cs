using Godot;
using System;
using System.IO;
public partial class MigrationScene {
 // Capture-only: render the normal streamed world in tiles, without loading the whole map.
 async void CaptureAerialWorld(){try{
  game.NoDiskSave=true;CloseMenu();paused=true;SetProcess(false);atmosphere.Environment.FogEnabled=false;game.World.Changes.Time=900;ClearEnemies();weaponRoot.Visible=false;
  var view=new SubViewport{Size=new(512,512),World3D=GetWorld3D(),RenderTargetUpdateMode=SubViewport.UpdateMode.Always};AddChild(view);
  var eye=new Camera3D{Projection=Camera3D.ProjectionType.Orthogonal,Size=400,Near=1,Far=3000,Current=true};view.AddChild(eye);
  sun.DirectionalShadowMaxDistance=1800;
  var result=Image.CreateEmpty(4096,4096,false,Image.Format.Rgba8);
  string folder=ProjectSettings.GlobalizePath("user://tests/art");Directory.CreateDirectory(folder);
  for(int row=0;row<8;row++)for(int col=0;col<8;col++){
   double x=ExpeditionWorld.MinX+200+col*400,z=ExpeditionWorld.MinZ+200+row*400;Teleport(x,z);ClearEnemies();
   for(int cx=ExpeditionWorld.Cell(x-200);cx<=ExpeditionWorld.Cell(x+200);cx++)for(int cz=ExpeditionWorld.Cell(z-200);cz<=ExpeditionWorld.Cell(z+200);cz++){
    if(!chunks.ContainsKey((cx,cz))&&game.World.InBlock(cx*64+32,cz*64+32))chunks[(cx,cz)]=CreateChunk(cx,cz);
   }
   eye.Position=new((float)x,1000,(float)z);eye.LookAt(new Vector3((float)x,0,(float)z),Vector3.Forward);
   for(int frame=0;frame<3;frame++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
   await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
   var tile=view.GetTexture().GetImage();tile.Convert(Image.Format.Rgba8);result.BlitRect(tile,new Rect2I(0,0,512,512),new Vector2I(col*512,row*512));
   if(row==4&&col==4||row==2&&col==3||row==7&&col==6)tile.SavePng(Path.Combine(folder,$"aerial-detail-{row}-{col}.png"));
   GD.Print($"AERIAL / {row*8+col+1}/64");
  }
  result.SavePng(Path.Combine(folder,"aerial-3d-world-north-up.png"));GD.Print("PASS / actual 3D world aerial capture / seed "+game.Save.Seed);GetTree().Quit();
 }catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}
