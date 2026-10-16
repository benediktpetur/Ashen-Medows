using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 void ComicCrafting(){ResearchWorkshop();}
 void Portrait(VBoxContainer parent){var holder=new SubViewportContainer{CustomMinimumSize=new Vector2(250,140),Stretch=true};parent.AddChild(holder);var viewport=new SubViewport{Size=new Vector2I(250,140),TransparentBg=true,OwnWorld3D=true,RenderTargetUpdateMode=SubViewport.UpdateMode.Always};holder.AddChild(viewport);var model=new Node3D{RotationDegrees=new Vector3(0,15,0)};viewport.AddChild(model);ArtCharacter(model,1,0,true);var equipped=game.Profile.Items.FirstOrDefault(a=>a.Id==game.Profile.Equipped);if(equipped!=null){var worn=new Node3D{Scale=new Vector3(1.02f,1,1.08f)};model.GetNode<Node3D>("Torso").AddChild(worn);ItemArmor(worn,equipped.Level,equipped.Id);}var key=new DirectionalLight3D{RotationDegrees=new Vector3(-35,-30,0),LightColor=new Color(1,.86f,.65f),LightEnergy=1.2f};viewport.AddChild(key);var fill=new DirectionalLight3D{RotationDegrees=new Vector3(20,160,0),LightColor=new Color(.53f,.67f,.74f),LightEnergy=.6f};viewport.AddChild(fill);var cam=new Camera3D{Position=new Vector3(0,1.1f,-2.3f),Current=true,Fov=50};viewport.AddChild(cam);cam.LookAt(new Vector3(0,.95f,0));}
 void ComicEquipment(){SurvivorInventory();}
}
