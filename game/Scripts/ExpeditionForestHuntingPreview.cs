using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 async void CaptureForestHunting(){try{ClearEnemies();game.World.Changes.Time=900;weaponRoot.Visible=false;camera.Fov=70;
  double z=-820,x=game.World.RouteX(z)+170;Teleport(x,z);ClearEnemies();paused=true;hudRoot.Visible=false;camera.GlobalPosition=Ground(x,z)+Vector3.Up*1.65f;camera.LookAt(Ground(x+16,z-30)+Vector3.Up*1.65f);await ArtShot("forest-01-interior");
  GD.Print("FOREST / "+props.Count(p=>p.Kind==0&&Distance(p.Position,player.Position)<64)+" trees within 64m");SpawnFauna();var animal=infected.FirstOrDefault(e=>Species(e.Id) is "boar" or "wolf");if(animal!=null){camera.GlobalPosition=animal.Body.Position+new Vector3(4,1.3f,5);camera.LookAt(animal.Body.Position+Vector3.Up*.5f);await ArtShot("forest-02-woodland-wildlife");}
  ClearEnemies();Teleport(ExpeditionWorld.SpawnX-100,ExpeditionWorld.SpawnZ-50);ClearEnemies();SpawnFauna();animal=infected.FirstOrDefault(e=>Species(e.Id)=="deer");if(animal!=null){camera.GlobalPosition=animal.Body.Position+new Vector3(4,1.7f,7);camera.LookAt(animal.Body.Position+Vector3.Up*.7f);await ArtShot("forest-03-meadow-deer");}animal=infected.FirstOrDefault(e=>Species(e.Id)=="rabbit");if(animal!=null){camera.GlobalPosition=animal.Body.Position+new Vector3(2,1.0f,3);camera.LookAt(animal.Body.Position+Vector3.Up*.25f);await ArtShot("forest-04-meadow-rabbits");}
  game.Craft.HuntStock=new[]{4,2,3,3,2,1};ShowHunting();await ArtShot("forest-05-hunting-crafting");GetTree().Quit();
 }catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}}
}
