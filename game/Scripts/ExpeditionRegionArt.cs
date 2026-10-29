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
 // The regional shell keeps the existing solid footprint and collider. Doors are
 // visual loading bays; enterable/story buildings retain their authored layouts.
 void ArtBuilding(Node3D root,int x,int z,float size,float h,int tier){
  if(tier!=3){DefaultRegionBuilding(root,x,z,size,h,tier);return;}
  var n=new Node3D{Name="IndustrialWarehouse",Position=Ground(x,z)};root.AddChild(n);
  var wall=new Color(.39f,.40f,.38f);var metal=new Color(.27f,.30f,.31f);
  Box(n,new(0,h/2,0),new(size,h,size),wall,true).MaterialOverride=ExteriorFinish(3,wall);
  Box(n,new(0,.35f,0),new(size+.15f,.7f,size+.15f),new(.28f,.29f,.28f));
  Box(n,new(0,h+.12f,0),new(size+.5f,.24f,size+.5f),metal);
  WarehouseCladding(n,size,h);WarehouseSceneryDetails(n,size);float front=-size/2-.07f;
  for(int side=-1;side<=1;side+=2){
   Box(n,new(side*(size/2-.12f),h/2,front),new(.24f,h,.18f),metal);
   Box(n,new(side*size*.22f,1.7f,front-.05f),new(size*.34f,3.4f,.12f),new(.20f,.23f,.24f));
   for(int row=0;row<9;row++)Box(n,new(side*size*.22f,.25f+row*.36f,front-.13f),new(size*.33f,.035f,.03f),metal);
   Box(n,new(side*size*.22f,.10f,front-.16f),new(size*.37f,.2f,.3f),new(.65f,.53f,.21f));
  }
  for(float height=4.2f;height<h-.6f;height+=2.4f)Box(n,new(0,height,front-.02f),new(size*.74f,.65f,.08f),new(.17f,.24f,.26f));
  Box(n,new(0,Math.Min(h-.5f,4),front-.16f),new(size*.84f,.55f,.12f),metal);
  n.AddChild(new Label3D{Text=new[]{"FREIGHT DEPOT","MACHINE WORKS","METAL FABRICATION","WAREHOUSE / LOADING"}[(int)(game.World.Hash(x,z)%4)],Position=new(0,Math.Min(h-.5f,4),front-.24f),RotationDegrees=new(0,180,0),FontSize=30,PixelSize=.009f,Modulate=new(.81f,.78f,.63f)});
  for(int i=0;i<3;i++){
   float px=(i-1)*size*.25f;
   Box(n,new(px,h+.6f,size*.15f),new(1.2f,.9f,1.8f),metal);
   for(int slot=0;slot<4;slot++)Box(n,new(px-.43f+slot*.28f,h+1.06f,size*.15f),new(.09f,.025f,1.4f),Ink);
  }
  if(game.World.Hash(x+7,z)%3==0){Tube(n,new(size*.32f,h,-size*.15f),new(size*.32f,h+2.5f,-size*.15f),.28f,Rust);Tube(n,new(size*.32f,h+2.45f,-size*.15f),new(size*.32f,h+2.6f,-size*.15f),.36f,metal);}
 }
 void IndustrialRoadside(Node3D shop,int side){
  Box(shop,new(0,1.75f,0),new(7,3.5f,5),new(.38f,.39f,.37f),true).MaterialOverride=ExteriorFinish(3,new(.38f,.39f,.37f));
  Box(shop,new(0,3.6f,0),new(7.4f,.2f,5.4f),Steel);
  Box(shop,new(-.7f,1.45f,-2.55f),new(3.6f,2.9f,.08f),new(.22f,.25f,.26f));
  for(int i=0;i<8;i++)Box(shop,new(-.7f,.2f+i*.35f,-2.61f),new(3.5f,.035f,.03f),Steel);
  Box(shop,new(2.3f,1.1f,-2.55f),new(1,2.2f,.1f),Rust);
  Box(shop,new(0,3.12f,-2.6f),new(6,.45f,.1f),new(.20f,.24f,.25f));
  shop.AddChild(new Label3D{Text=side<0?"TYRE & TRUCK SERVICE":"HAULAGE / DISPATCH",Position=new(0,3.12f,-2.68f),RotationDegrees=new(0,180,0),FontSize=28,PixelSize=.010f,Modulate=new(.82f,.76f,.55f)});
  Box(shop,new(-2.8f,3.9f,1.3f),new(.8f,.5f,.9f),Steel);
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
   var shapes=root.FindChildren("*","CollisionShape3D",true,false).OfType<CollisionShape3D>().Where(s=>s.Shape is BoxShape3D).ToArray();
   MigrationChecks.Assert(shapes.Any(s=>((BoxShape3D)s.Shape).Size.IsEqualApprox(new Vector3(size,h,size))),"Regional shell retains authored collision footprint / "+tier);
   if(tier==3){MigrationChecks.Assert(root.HasNode("IndustrialWarehouse"),"Highway zone uses industrial buildings");MigrationChecks.Assert(root.FindChildren("GeneratedSteelCladding*","Node3D",true,false).Count==6,"Six fitted generated steel panels / warehouse");MigrationChecks.Assert(root.FindChildren("*","Label3D",true,false).OfType<Label3D>().Any(l=>l.Text.Contains("DEPOT")||l.Text.Contains("WORKS")||l.Text.Contains("FABRICATION")||l.Text.Contains("WAREHOUSE")),"Industrial identity is readable");}
   root.Free();
  }
  var garage=new Node3D();AddChild(garage);IndustrialRoadside(garage,-1);
  MigrationChecks.Assert(garage.FindChildren("*","CollisionShape3D",true,false).OfType<CollisionShape3D>().Any(s=>s.Shape is BoxShape3D box&&box.Size.IsEqualApprox(new Vector3(7,3.5f,5))),"Industrial roadside agrees with simulation collider");garage.Free();
  MigrationChecks.Assert(game.World.Tier(game.World.RouteX(-700),-700)==2&&game.World.Tier(game.World.RouteX(-1450),-1450)==3&&game.World.Tier(game.World.RouteX(-2100),-2100)==4,"Surface progression and encounter tiers retained");
  MigrationChecks.Assert(RegionGround(0,-2100).IsEqualApprox(new Color(.19f,.20f,.20f)),"Dense city uses urban paving");
  MigrationChecks.Assert(OnRegionalRoute(game.World.RouteX(-1450)+6,-1450)&&!OnRegionalRoute(game.World.RouteX(-1450)+8,-1450),"Industrial highway has a wider paved carriageway");
  GD.Print("PASS / suburbs, woodland, highway-industrial and dense city / warehouse and garage physics parity / paved city and highway width / existing progression");
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
