using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 async void CaptureBlueprintGallery(){try{testing=true;CloseMenu();paused=true;hudRoot.Visible=false;terrainRoot.Visible=worldRoot.Visible=buildRoot.Visible=false;if(distantTerrain!=null)distantTerrain.Visible=false;weaponRoot.Visible=false;ClearEnemies();game.World.Changes.Time=900;ItemCatalogChecks();GD.Print($"CATALOGUE / {BuildDefinition.All.Length} buildables, {ExpeditionRecipe.All.Length} recipes, {ExpeditionItemDefinition.All().Count} model definitions");
  foreach(string tab in new[]{"Buildings","Defenses","Survival","Structural tiers"}){var indices=Enumerable.Range(0,BuildDefinition.All.Length).Where(i=>tab=="Structural tiers"?BuildDefinition.All[i].Function=="structure"&&BuildDefinition.All[i].BaseKind is 0 or 2:BuildDefinition.All[i].Tab==tab&&BuildDefinition.All[i].Function!="structure").ToArray();for(int page=0;page*8<indices.Length;page++){
   var stage=new Node3D();AddChild(stage);Box(stage,new(0,-.1f,0),new(22,.2f,14),new(.19f,.23f,.21f));stage.AddChild(new DirectionalLight3D{RotationDegrees=new(-35,130,0),LightEnergy=1,LightColor=new(.72f,.82f,.80f)});
   foreach(var entry in indices.Skip(page*8).Take(8).Select((kind,index)=>(kind,index))){float x=(entry.index%4-1.5f)*4.8f,z=(entry.index/4-.5f)*5.5f;var def=BuildDefinition.All[entry.kind];var node=Structure(new BuildPiece{Kind=entry.kind,Open=def.Function=="door"},true);node.Position=new(x,0,z);node.RotationDegrees=new(0,20,0);stage.AddChild(node);var label=new Label3D{Text=$"T{def.Tier} / {def.Name}",Position=new(x,Math.Max(2.8f,(float)def.H+.3f),z),FontSize=28,PixelSize=.012f,Width=280,AutowrapMode=TextServer.AutowrapMode.WordSmart,Billboard=BaseMaterial3D.BillboardModeEnum.Enabled,Modulate=new(.90f,.83f,.64f)};stage.AddChild(label);}
   camera.Fov=55;camera.GlobalPosition=new(5,10,-14);camera.LookAt(new Vector3(0,1,0));await ArtShot("blueprints-"+tab.Replace(" ","-").ToLowerInvariant()+"-"+(page+1).ToString("00"));stage.QueueFree();await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
  }}
  terrainRoot.Visible=worldRoot.Visible=buildRoot.Visible=true;if(distantTerrain!=null)distantTerrain.Visible=true;weaponRoot.Visible=true;camera.Fov=75;Teleport(ExpeditionWorld.SpawnX,ExpeditionWorld.SpawnZ);camera.Position=new(0,1.65f,0);camera.Rotation=Vector3.Zero;CaptureResearchTree();
 }catch(Exception e){GD.PrintErr(e);GetTree().Quit(1);}}
}
