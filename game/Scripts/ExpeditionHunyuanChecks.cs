using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class MigrationScene {
 async void RunHunyuanChecks(){try{
  game.NoDiskSave=true;CloseMenu();paused=true;SetProcess(false);SetPhysicsProcess(false);ClearEnemies();
  bool preview=OS.GetCmdlineUserArgs().Contains("--hunyuan-preview");
  hudRoot.Visible=weaponRoot.Visible=terrainRoot.Visible=worldRoot.Visible=buildRoot.Visible=false;if(distantTerrain!=null)distantTerrain.Visible=false;
  var stage=new Node3D();AddChild(stage);Box(stage,new(0,-.1f,0),new(40,.2f,40),new(.25f,.27f,.23f));
  stage.AddChild(new DirectionalLight3D{RotationDegrees=new(-30,140,0),LightEnergy=.8f});
  string[] keys=new[]{"survivor-player-survivor"}.Concat(HunyuanZombies).Concat(HunyuanSpecial.Values).Distinct().ToArray();
  int checkedModels=0;
  for(int group=0;group<keys.Length;group+=4){
   var row=new Node3D();stage.AddChild(row);
   for(int k=group;k<Math.Min(group+4,keys.Length);k++){
    string key=keys[k];var actor=new Node3D{Position=new((k-group-1.5f)*2.4f,0,0)};row.AddChild(actor);
    bool humanoid=!key.Contains("worm")&&!key.StartsWith("brood-nest");
    if(k==0)ArtCharacter(actor,1,0,true);
    else if(Array.IndexOf(HunyuanZombies,key)>=0)HumanCharacter(actor,2,0,false,Array.IndexOf(HunyuanZombies,key));
    else if(HunyuanSpecial.ContainsValue(key)){string kind=HunyuanSpecial.First(p=>p.Value==key).Key;SpecialInfectedArt(actor,"special/"+kind+"/reference",3,0);}
    else throw new Exception("Unmapped Hunyuan model / "+key);
    var rig=actor.GetNodeOrNull<HunyuanActorRig>("HunyuanModel");MigrationChecks.Assert(rig!=null,"Generated model missing / "+key);
    if(humanoid){MigrationChecks.Assert(rig.Skeleton.GetBoneCount()==12,"Rig bones / "+key);PoseFigure(actor,.9f,1,k!=0);rig.UpdatePose();MigrationChecks.Assert(rig.Skeleton.GetBonePoseRotation(2)!=Quaternion.Identity,"Walking deforms skin / "+key);actor.GetNode<Node3D>("RightArm").RotationDegrees=new(-95,0,0);rig.UpdatePose();MigrationChecks.Assert(rig.Skeleton.GetBonePoseRotation(9)!=Quaternion.Identity,"Attack deforms skin / "+key);PoseFigure(actor,0,0,k!=0);rig.UpdatePose();}
    var mesh=hunyuanMeshes[key];MigrationChecks.Assert(mesh.GetAabb().Size.Y>0&&mesh.GetAabb().Size.Y<2,"Normalized model / "+key);
    if(humanoid){MigrationChecks.Assert(rig.Skeleton.GetBonePosePosition(1).Y>1.3f,"Head retains rest height / "+key);var arrays=mesh.SurfaceGetArrays(0);var verts=arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();var bones=arrays[(int)Mesh.ArrayType.Bones].AsInt32Array();var weights=arrays[(int)Mesh.ArrayType.Weights].AsFloat32Array();float min=float.MaxValue,max=float.MinValue;for(int i=0;i<verts.Length;i+=31){Vector3 point=Vector3.Zero;for(int w=0;w<4;w++){int bone=bones[i*4+w];point+=(rig.Skeleton.GetBoneGlobalPose(bone)*new Transform3D(Basis.Identity,-rig.Skeleton.GetBoneRest(bone).Origin)*verts[i])*weights[i*4+w];}min=Math.Min(min,point.Y);max=Math.Max(max,point.Y);}MigrationChecks.Assert(max-min>1.4f&&max-min<2,"Rendered skin retains human height / "+key);}
    for(int s=0;s<mesh.GetSurfaceCount();s++){MigrationChecks.Assert(mesh.SurfaceGetMaterial(s)!=null,"Textured material / "+key);if(humanoid){var arrays=mesh.SurfaceGetArrays(s);var weights=arrays[(int)Mesh.ArrayType.Weights].AsFloat32Array();for(int i=0;i<weights.Length;i+=4)MigrationChecks.Assert(Math.Abs(weights[i]+weights[i+1]+weights[i+2]+weights[i+3]-1)<.001,"Normalized skin weights / "+key);}}
    row.AddChild(new Label3D{Text=key.Replace("special-","").Replace("zombie-","").ToUpperInvariant(),Position=actor.Position+new Vector3(0,2,0),FontSize=22,PixelSize=.0025f,Billboard=BaseMaterial3D.BillboardModeEnum.Enabled});checkedModels++;
   }
   if(preview){camera.Fov=40;camera.GlobalPosition=new(0,2.3f,-8);camera.LookAt(new Vector3(0,1,0));await ArtShot($"hunyuan-actors-{group/4+1:00}");if(group==0){var actors=row.GetChildren().OfType<Node3D>().Where(n=>n.HasNode("HunyuanModel")).ToArray();foreach(var child in row.GetChildren().OfType<Node3D>())child.Visible=false;for(int i=0;i<2;i++){actors[i].Visible=true;actors[i].Position=new(i==0?.75f:-.75f,0,0);PoseFigure(actors[i],i==0?.9f:2.3f,1,i!=0);actors[i].GetNode<HunyuanActorRig>("HunyuanModel").UpdatePose();}camera.Fov=38;camera.GlobalPosition=new(.3f,1.55f,-4.2f);camera.LookAt(new Vector3(0,.95f,0));await ArtShot("hunyuan-player-and-drifter");actors[1].GetNode<Node3D>("RightArm").RotationDegrees=new(-95,0,-15);actors[1].GetNode<HunyuanActorRig>("HunyuanModel").UpdatePose();await ArtShot("hunyuan-attack-pose");}}
   row.QueueFree();await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
  }
  GD.Print($"PASS / Hunyuan actors / {checkedModels} textured models / 12-bone walk and attack skinning / normalized scale and weights");GetTree().Quit();
 }catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}