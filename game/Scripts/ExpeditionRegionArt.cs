using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class MigrationScene {
 readonly Dictionary<(int,int,int),bool> regionPavedLots=new();
 bool OnRegionalRoute(double x,double z)=>Math.Abs(x-game.World.RouteX(z))<(game.World.Tier(x,z)==3?7:5);
 void IndustrialLaneMarks(Node3D root,Vector3 p){
  foreach(int side in new[]{-1,1}){
   Box(root,p+new Vector3(side*3.3f,.035f,0),new(.12f,.025f,5),new(.67f,.66f,.58f));
   Box(root,p+new Vector3(side*6.6f,.035f,0),new(.12f,.025f,16),new(.64f,.63f,.55f));
  }
 }
 void ArtBuilding(Node3D root,int x,int z,float size,float h,int tier,bool simulation=false){
  string id=$"building/{game.Save.Seed}/{x}/{z}";
  if(ObjectHealth.TryGetValue(id,out double legacyHealth)&&legacyHealth<=0)return;
  var street=game.World.NearestStreet(x,z);
  double facing=Math.Atan2(x-street.X,z-street.Z)*180/Math.PI;
  float yaw=(float)(Math.Round(facing/90)*90);
  var shell=new Node3D{
   Name=tier==3?"IndustrialWarehouse":$"AmbientBlock_{x}_{z}",Position=Ground(x,z),
   RotationDegrees=new(0,yaw,0)
  };
  root.AddChild(shell);
  string family=tier==3?"Industrial":game.World.Hash(x,z)%3==0?"Brick":"Concrete";
  ModularMapShell(shell,id,size,size,h,family,simulation);
  if(simulation)return;
  if(tier==3){
   WarehouseSceneryDetails(shell,size);
   shell.AddChild(new Label3D{
    Text=new[]{"FREIGHT DEPOT","MACHINE WORKS","METAL FABRICATION","WAREHOUSE / LOADING"}[game.World.Hash(x,z)%4],
    Position=new(0,Math.Min(h-.5f,4),-size/2-.24f),RotationDegrees=new(0,180,0),
    FontSize=30,PixelSize=.009f,Modulate=new(.81f,.78f,.63f)
   });
  }else CitySceneryDetails(shell,Vector3.Zero,size,h,game.World.Hash(x,z));
 }
 Color RegionGround(double x,double z){
  int tier=game.World.Tier(x,z);
  if(tier==4)return new(.19f,.20f,.20f);
  if(tier>=3){
   double bx=Math.Floor(x/32)*32+16,bz=Math.Floor(z/32)*32+16;
   var key=(game.Save.Seed,(int)bx,(int)bz);
   if(!regionPavedLots.TryGetValue(key,out bool occupied)){occupied=game.World.BuildingCell(bx,bz);regionPavedLots[key]=occupied;}
   bool lot=occupied&&Math.Abs(x-bx)<15&&Math.Abs(z-bz)<15;
   bool crossStreet=Math.Abs(z-Math.Round(z/128)*128)<9;
   if(lot||crossStreet)return tier==3?new(.24f,.24f,.21f):new(.19f,.20f,.20f);
  }
  return new(.12f,.18f,.095f);
 }
 async void RunRegionChecks(){try{
  game.NoDiskSave=true;paused=true;SetProcess(false);SetPhysicsProcess(false);CloseMenu();ClearEnemies();
  WorldModelChecks();SurfaceLayoutChecks();PropPlacementChecks();SceneryPolishChecks();
  foreach(int tier in new[]{3,4}){
   var root=new Node3D();AddChild(root);int z=tier==3?-1456:-2096,x=(int)(Math.Floor(game.World.RouteX(z)/32)*32+48);
   float size=(float)game.World.BuildingHalf(x,z)*2,h=(float)game.World.BuildingHeight(x,z);
   ArtBuilding(root,x,z,size,h,tier);
   var simulation=new Node3D();AddChild(simulation);
   ArtBuilding(simulation,x,z,size,h,tier,true);
   var walls=root.FindChildren("Wall_*","StaticBody3D",true,false).OfType<StaticBody3D>().ToArray();
   var physics=simulation.FindChildren("Wall_*","StaticBody3D",true,false).OfType<StaticBody3D>().ToArray();
   MigrationChecks.Assert(walls.Length>4&&walls.Select(w=>w.GetMeta("world_damage_id").AsString()).SequenceEqual(physics.Select(w=>w.GetMeta("world_damage_id").AsString())),$"Regional wall slots agree with simulation / {tier} / {walls.Length} visible, {physics.Length} simulated");
   MigrationChecks.Assert(walls.Any(w=>w.GetMeta("wall_asset").AsString().EndsWith("-2")),"Regional building has an entrance / "+tier);
   if(tier==3)MigrationChecks.Assert(root.HasNode("IndustrialWarehouse")&&root.FindChildren("*","Label3D",true,false).Count>0,"Industrial identity is readable");
   simulation.Free();
   var shell=root.GetChild<Node3D>(0);
   await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
   await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
   Godot.Collections.Dictionary Probe(float offset){
    var start=shell.ToGlobal(new Vector3(offset,1.2f,-size/2-1));
    var end=shell.ToGlobal(new Vector3(offset,1.2f,-size/2+1));
    return GetWorld3D().DirectSpaceState.IntersectRay(PhysicsRayQueryParameters3D.Create(start,end));
   }
   MigrationChecks.Assert(Probe(0).Count==0&&Probe(size*.47f).Count>0,"Regional doorway is open and neighboring wall is solid / "+tier);
   root.Free();
  }
  MigrationChecks.Assert(game.World.Tier(game.World.RouteX(-700),-700)==2&&game.World.Tier(game.World.RouteX(-1450),-1450)==3&&game.World.Tier(game.World.RouteX(-2100),-2100)==4,"Surface progression and encounter tiers retained");
  MigrationChecks.Assert(RegionGround(0,-2100).IsEqualApprox(new Color(.19f,.20f,.20f)),"Dense city uses urban paving");
  MigrationChecks.Assert(OnRegionalRoute(game.World.RouteX(-1450)+6,-1450)&&!OnRegionalRoute(game.World.RouteX(-1450)+8,-1450),"Industrial highway has a wider paved carriageway");
  GD.Print("PASS / suburbs, woodland, highway-industrial and dense city / regional modular wall physics parity / paved city and highway width / existing progression");
  if(OS.GetCmdlineUserArgs().Contains("--region-preview")){
   hudRoot.Visible=weaponRoot.Visible=false;game.World.Changes.Time=900;
   var points=new[]{new Vector2((float)ExpeditionWorld.HomeX,(float)ExpeditionWorld.HomeZ),new Vector2((float)game.World.RouteX(-700),-700),new Vector2((float)game.World.RouteX(-1450),-1450),new Vector2((float)game.World.RouteX(-2100),-2100)};
   for(int i=0;i<points.Length;i++){var p=points[i];Teleport(p.X,p.Y);ClearEnemies();var eye=Ground(p.X+35,p.Y-43)+Vector3.Up*38;camera.GlobalPosition=eye;camera.LookAt(Ground(p.X,p.Y)+Vector3.Up*3);await ArtShot($"region-{i+1:00}");}
   Teleport(ExpeditionWorld.HomeX,ExpeditionWorld.HomeZ);ClearEnemies();var interior=terrainRoot.FindChild("BriarLaneHouse0",true,false) as Node3D;if(interior==null)throw new Exception("Interior preview house not streamed");camera.GlobalPosition=interior.ToGlobal(new Vector3(1.5f,1.8f,4.6f));camera.LookAt(interior.ToGlobal(new Vector3(-1,.8f,-2)));await ArtShot("world-interior");
   camera.Fov=65;camera.GlobalPosition=interior.ToGlobal(new Vector3(15,5,18));camera.LookAt(interior.ToGlobal(new Vector3(0,1.7f,0)));await ArtShot("exterior-suburban-house");
   Teleport(game.World.RouteX(-1450),-1450);ClearEnemies();var factory=terrainRoot.FindChildren("IndustrialWarehouse*","Node3D",true,false).OfType<Node3D>().OrderBy(n=>n.GlobalPosition.DistanceSquaredTo(player.GlobalPosition)).FirstOrDefault();if(factory==null)throw new Exception("Industrial preview warehouse missing");camera.GlobalPosition=factory.ToGlobal(new Vector3(15,5,-20));camera.LookAt(factory.ToGlobal(new Vector3(0,2.5f,0)));await ArtShot("exterior-warehouse");ShowMap();await ArtShot("ground-layout-map");CloseMenu();Teleport(ExpeditionWorld.HomeX,ExpeditionWorld.HomeZ);ClearEnemies();var inspect=terrainRoot.FindChild("BriarLaneHouse0",true,false) as Node3D;camera.GlobalPosition=inspect.ToGlobal(new Vector3(12,2.4f,8));camera.LookAt(inspect.ToGlobal(new Vector3(7.5f,.8f,-1)));await ArtShot("placement-car-fence");camera.GlobalPosition=inspect.ToGlobal(new Vector3(-1,1.9f,3));camera.LookAt(inspect.ToGlobal(new Vector3(3,1,-3)));await ArtShot("placement-furniture-room");await SceneryStreetReview();
  }
  GetTree().Quit();
 }catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}}
}
