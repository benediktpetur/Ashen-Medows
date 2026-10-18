using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 async void CaptureThreatRoster(){try{
  game.NoDiskSave=true;game.World.Changes.Time=900;EnterDungeon(HeartDungeon());ClearEnemies();CloseMenu();paused=true;weaponRoot.Visible=false;foreach(var label in dungeonRoot.FindChildren("*","Label3D",true,false).OfType<Label3D>())label.Visible=false;
  var view=new Camera3D{Current=true,Fov=32};AddChild(view);AddChild(new OmniLight3D{Position=new(0,4,-3),LightColor=new(1,.83f,.66f),LightEnergy=4,OmniRange=20});AddChild(new OmniLight3D{Position=new(-6,3,-11),LightColor=new(.3f,.6f,.7f),LightEnergy=3,OmniRange=20});
  var layer=new CanvasLayer();AddChild(layer);var caption=new Label{Position=new(30,24)};caption.AddThemeFontSizeOverride("font_size",27);layer.AddChild(caption);
  for(int page=0;page<2;page++){ClearEnemies();for(int j=0;j<5;j++){var spec=ExpeditionThreat.All[page*5+j];string id="special/"+spec.Kind+"/preview";AddEnemy(id,(j-2)*2.3,-8,spec.Min);var e=infected.Last();e.Body.LookAt(new Vector3(e.Body.Position.X,0,3));PoseNewThreat(e);e.Body.AddChild(new Label3D{Text=spec.Name.ToUpperInvariant()+$"\nT{spec.Min}+",Position=new(0,2.55f,0),FontSize=28,PixelSize=.007f,Billboard=BaseMaterial3D.BillboardModeEnum.Enabled});}
   caption.Text=page==0?"NEW INFECTED / WOODLAND AND HIGHWAY":"NEW INFECTED / RARES AND ELITES";view.Position=new(0,2.1f,3.3f);view.LookAt(new Vector3(0,1.05f,-8));await ArtShot("enemies-new-"+(page+1).ToString("00")+"-roster");
   foreach(var e in infected.ToArray()){foreach(var other in infected)other.Visual.Visible=other==e;foreach(var label in infected.SelectMany(n=>n.Body.GetChildren().OfType<Label3D>()))label.Visible=false;var spec=ExpeditionThreat.Find(InfectedKind(e));caption.Text=spec.Name.ToUpperInvariant()+" / "+spec.Role.ToUpperInvariant();view.Position=e.Body.Position+new Vector3(1.8f,1.65f,3.8f);view.LookAt(e.Body.Position+new Vector3(0,.95f,0));await ArtShot("enemies-new-"+spec.Kind);}
  }
  GD.Print("PASS / ten new enemy in-engine model captures");GetTree().Quit();
 }catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}
