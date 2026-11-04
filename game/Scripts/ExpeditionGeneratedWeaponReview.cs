using Godot;
using System;
using System.Linq;
using System.IO;
public partial class MigrationScene {
 async void CaptureWeaponCandidates(){try{
  SetProcess(false);SetPhysicsProcess(false);CloseMenu();hudRoot.Visible=weaponRoot.Visible=false;if(viewArms!=null)viewArms.Visible=false;
  var rows=Godot.Json.ParseString(File.ReadAllText(ProjectSettings.GlobalizePath("res://../build/generated-weapon-review/candidates.json"))).AsGodotArray();
  string folder=ProjectSettings.GlobalizePath("res://../build/generated-weapon-review");
  GetWindow().ContentScaleSize=new(1440,1000);GetWindow().Size=new(1440,1000);
  for(int page=0;page<rows.Count;page+=12){
   var panel=new ColorRect{Color=new Color(.07f,.09f,.10f),Size=new(1440,1000)};ui.AddChild(panel);
   var grid=new GridContainer{Columns=4};panel.AddChild(grid);
   for(int i=page;i<Math.Min(page+12,rows.Count);i++){
    var row=rows[i].AsGodotDictionary();var cell=new VBoxContainer();grid.AddChild(cell);
    var container=new SubViewportContainer{CustomMinimumSize=new(350,285),Stretch=true};cell.AddChild(container);var viewport=new SubViewport{Size=new(350,285),OwnWorld3D=true,RenderTargetUpdateMode=SubViewport.UpdateMode.Always};container.AddChild(viewport);
    viewport.AddChild(new WorldEnvironment{Environment=new Godot.Environment{BackgroundMode=Godot.Environment.BGMode.Color,BackgroundColor=new Color(.14f,.18f,.20f),AmbientLightSource=Godot.Environment.AmbientSource.Color,AmbientLightColor=Colors.White,AmbientLightEnergy=.7f}});viewport.AddChild(new DirectionalLight3D{RotationDegrees=new(-35,-35,0),LightEnergy=1.2f});viewport.AddChild(new DirectionalLight3D{RotationDegrees=new(-20,145,0),LightEnergy=.7f});
    var doc=new GltfDocument();var state=new GltfState();if(doc.AppendFromFile(row["path"].AsString(),state)!=Error.Ok)throw new Exception("GLB failed");var model=(Node3D)doc.GenerateScene(state);var holder=new Node3D();viewport.AddChild(holder);holder.AddChild(model);Aabb bounds=default;bool found=false;BatchBounds(model,Transform3D.Identity,ref bounds,ref found);float longest=bounds.Size[(int)bounds.Size.MaxAxisIndex()];float scale=1.8f/longest;model.Scale*=scale;model.Position=-bounds.GetCenter()*scale;
    var camera=new Camera3D{Position=new(0,.5f,3),Projection=Camera3D.ProjectionType.Orthogonal,Size=2.3f,Current=true};viewport.AddChild(camera);camera.LookAt(Vector3.Zero);
    cell.AddChild(new Label{Text=i+" / "+row["name"].AsString(),CustomMinimumSize=new(350,42),AutowrapMode=TextServer.AutowrapMode.WordSmart});
   }
   for(int frame=0;frame<4;frame++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);GetViewport().GetTexture().GetImage().SavePng(folder+$"/candidates-{page/12:00}.png");panel.Free();GD.Print("Captured weapon candidate page "+page/12);
  }
  GetTree().Quit();
 }catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}
