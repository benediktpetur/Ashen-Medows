using Godot;
using System;
using System.Linq;

public partial class MigrationScene {
 void NeighbourhoodYardDetails(Node3D house,int style){
  var detail=new Node3D{Name="NeighbourhoodYard"};house.AddChild(detail);
  Vector3 At(float x,float z){var p=house.Transform*new Vector3(x,0,z);p.Y=(float)game.World.Height(p.X,p.Z);return house.Transform.AffineInverse()*p;}
  // Mailboxes are off the central entrance path; beds stay outside the house shell.
  var post=At(-3.8f,9.2f);Tube(detail,post,post+Vector3.Up*.95f,.045f,Wood);
  Box(detail,post+Vector3.Up*1.02f,new(.45f,.3f,.55f),style%2==0?new(.31f,.37f,.34f):new(.42f,.29f,.23f));
  for(int row=0;row<2;row++){
   var bed=At(-8.2f,-2+row*2.2f);Box(detail,bed+Vector3.Up*.18f,new(1.5f,.36f,1.25f),Wood);
   Box(detail,bed+Vector3.Up*.38f,new(1.3f,.04f,1.05f),new(.22f,.16f,.11f));
   for(int j=0;j<3;j++)Ellipsoid(detail,bed+new Vector3(-.4f+j*.4f,.52f,0),new(.20f,.17f,.26f),new(.33f,.40f,.24f));
  }
  var bin=At(8.6f,-4);Box(detail,bin+Vector3.Up*.45f,new(.65f,.9f,.65f),new(.25f,.30f,.27f));Box(detail,bin+Vector3.Up*.93f,new(.72f,.06f,.72f),Ink);
  foreach(var mesh in detail.FindChildren("*","GeometryInstance3D",true,false).OfType<GeometryInstance3D>())mesh.VisibilityRangeEnd=100;
 }
 bool PopulationBuildClear(RoadsideStop stop)=>!game.Building.Pieces.Any(p=>ExpeditionGame.Distance(p.X,p.Z,stop.X,stop.Z)<12);
 void PopulationBox(Node3D n,Vector3 p,Vector3 size,Color color,bool simulation){if(simulation)PhysicsObstacle(n,new BoxShape3D{Size=size},p);else Box(n,p,size,color,true);}
 void RoadsidePopulation(Node3D root,int cx,int cz,bool simulation){
  foreach(var stop in game.World.RoadsideStops()){
   if(ExpeditionWorld.Cell(stop.X)!=cx||ExpeditionWorld.Cell(stop.Z)!=cz||!PopulationBuildClear(stop))continue;
   float floor=(float)game.World.Height(stop.X,stop.Z)+.08f;
   var n=new Node3D{Name="RoadsidePocket_"+stop.Index,Position=new((float)stop.X,floor,(float)stop.Z)};root.AddChild(n);
   // Ground every element separately: no floating decks or a raised lip across the entry.
   Vector3 At(float x,float z,float y=0)=>new(x,(float)game.World.Height(stop.X+x,stop.Z+z)-floor+y,z);
   Color paint=stop.Tier<=2?new(.30f,.37f,.29f):new(.28f,.36f,.39f);
   float roof=Math.Max(At(-2.8f,1.7f).Y,At(2.8f,1.7f).Y)+2.7f;
   foreach(int s in new[]{-1,1}){
    var foot=At(s*2.8f,1.7f);float height=roof-foot.Y;PopulationBox(n,foot+Vector3.Up*height/2,new(.16f,height,.16f),Steel,simulation);
   }
   PopulationBox(n,new(0,roof,1),new(6.4f,.16f,3.8f),paint,simulation);
   // Open front and sides preserve a walkable approach from the road.
   var seat=At(0,1.6f);
   PopulationBox(n,seat+Vector3.Up*.5f,new(3.6f,.12f,.65f),Wood,simulation);
   PopulationBox(n,seat+new Vector3(0,.95f,1.95f-1.6f),new(3.6f,.75f,.12f),Wood,simulation);
   foreach(int s in new[]{-1,1})PopulationBox(n,seat+new Vector3(s*1.4f,.22f,0),new(.12f,.44f,.5f),Steel,simulation);
   var bin=At(-3.8f,1.6f);PopulationBox(n,bin+Vector3.Up*.45f,new(.7f,.9f,.7f),paint,simulation);
   if(simulation)continue;
   Box(n,bin+Vector3.Up*.92f,new(.77f,.08f,.77f),Ink);
   var board=At(2.25f,1.9f);Box(n,board+Vector3.Up*1.5f,new(.9f,1.1f,.08f),Wood);
   StoryPaper(n,board+new Vector3(0,1.5f,-.06f));
   n.AddChild(new Label3D{Text=stop.Name.ToUpperInvariant(),Position=new(0,roof-.25f,-.9f),FontSize=24,PixelSize=.006f,Billboard=BaseMaterial3D.BillboardModeEnum.Enabled,Modulate=new(.82f,.78f,.64f)});
   // Scattered personal effects give each stop a different silhouette.
   if(stop.Tier==2){for(int j=0;j<3;j++){var p=At(-3.6f+j*.25f,-1.6f);Tube(n,p,p+new Vector3(.15f,.12f,1.4f),.12f,Wood);}}
   else if(stop.Tier>=3){var p=At(3.6f,1.6f);Box(n,p+Vector3.Up*.55f,new(.8f,1.1f,.7f),paint);Box(n,p+new Vector3(0,.75f,-.36f),new(.55f,.12f,.025f),Ink);}
   else{var p=At(-3.7f,-1.2f);Box(n,p+Vector3.Up*.2f,new(1,.4f,.8f),Wood);Box(n,p+Vector3.Up*.44f,new(.85f,.08f,.7f),new(.23f,.16f,.10f));}
   foreach(var mesh in n.FindChildren("*","GeometryInstance3D",true,false).OfType<GeometryInstance3D>())mesh.VisibilityRangeEnd=140;
  }
 }
 void SpawnRoadsideSupplies(){
  if(dungeon!=null)return;
  foreach(var stop in game.World.RoadsideStops()){
   if(!PopulationBuildClear(stop)||!NearActiveSurface(Ground(stop.X,stop.Z),150))continue;
   string id=$"supplies/roadside/{game.Save.Seed}/{stop.Index}";
   if(uniqueObjects.Contains(id)||game.Craft.Gathered.Contains(id))continue;
   // Outside the seating/collider footprint, at the open front of each shelter.
   double x=stop.X+2,z=stop.Z-1.6;
   var n=new Node3D{Name="RoadsideSupplies",Position=Ground(x,z)};worldRoot.AddChild(n);uniqueObjects.Add(id);
   Box(n,new(0,.25f,0),new(.8f,.5f,.6f),Wood);Box(n,new(0,.52f,0),new(.85f,.06f,.65f),Steel);
   n.AddChild(new Label3D{Text="SUPPLIES / E",Position=new(0,.85f,0),FontSize=20,PixelSize=.005f,Billboard=BaseMaterial3D.BillboardModeEnum.Enabled});
   props.Add(new(){Id=id,Kind=9,Tier=stop.Tier,Material=stop.Tier<=2?0:4,Amount=2,Position=n.Position,Node=n});
  }
 }
 void PopulationChecks(){
  var stops=game.World.RoadsideStops();
  MigrationChecks.Assert(stops.Count>=8,"Roadside destination coverage");
  foreach(var s in stops){MigrationChecks.Assert(game.World.Playable(s.X,s.Z)&&!game.World.MainRoad(s.X,s.Z,4),"Stops preserve carriageway");MigrationChecks.Assert(!ExpeditionContentSite.InLot(game.World,s.X,s.Z,18),"Stops preserve authored content lots");}
  foreach(var group in stops.GroupBy(s=>s.Tier))GD.Print($"POPULATION / tier {group.Key} / {group.Count()} stops");
  var stop=stops.First();var visual=new Node3D();var simulation=new Node3D();AddChild(visual);AddChild(simulation);
  int cx=ExpeditionWorld.Cell(stop.X),cz=ExpeditionWorld.Cell(stop.Z);RoadsidePopulation(visual,cx,cz,false);RoadsidePopulation(simulation,cx,cz,true);
  int a=visual.FindChildren("*","CollisionShape3D",true,false).Count,b=simulation.FindChildren("*","CollisionShape3D",true,false).Count;
  MigrationChecks.Assert(a>0&&a==b,"Shelter render and server collision parity");visual.Free();simulation.Free();
  Teleport(stop.X,stop.Z);SpawnRoadsideSupplies();var cache=props.FirstOrDefault(p=>p.Id==$"supplies/roadside/{game.Save.Seed}/{stop.Index}");
  MigrationChecks.Assert(cache!=null,"Roadside supplies spawn");player.Position=cache.Position;int food=game.Life.Food;
  MigrationChecks.Assert(SearchSupply()&&game.Life.Food==food+1,"Roadside cache uses existing loot interaction");SpawnRoadsideSupplies();
  MigrationChecks.Assert(!props.Any(p=>p.Id==cache.Id),"Claimed roadside supplies cannot respawn");
  GD.Print($"PASS / population / {stops.Count} roadside stops / collision parity / loot interaction and duplicate protection");
 }
 async void RunPopulationReview(){try{
  game.NoDiskSave=true;paused=true;SetProcess(false);SetPhysicsProcess(false);CloseMenu();ClearEnemies();PopulationChecks();
  if(OS.GetCmdlineUserArgs().Contains("--population-preview")){
   paused=true;hudRoot.Visible=weaponRoot.Visible=false;game.World.Changes.Time=900;camera.Fov=67;
   foreach(int tier in new[]{1,2,3,4}){
    var stop=game.World.RoadsideStops().Where(s=>s.Tier==tier).OrderBy(s=>Math.Abs(s.Z-(tier==1?40:tier==2?-700:tier==3?-1450:-2100))).FirstOrDefault();if(stop==null)continue;
    Teleport(stop.X,stop.Z);ClearEnemies();SpawnRoadsideSupplies();
    camera.GlobalPosition=Ground(stop.X-stop.Side*8,stop.Z-8)+Vector3.Up*2.4f;camera.LookAt(Ground(stop.X,stop.Z)+Vector3.Up*1.1f);
    for(int f=0;f<8;f++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
    await ArtShot("population-"+tier);GD.Print($"POPULATION VIEW / tier {tier} / {stop.X:F1}, {stop.Z:F1}");
   }
   Teleport(ExpeditionWorld.HomeX,ExpeditionWorld.HomeZ);ClearEnemies();
   var house=terrainRoot.FindChild("BriarLaneHouse0",true,false) as Node3D;
   if(house!=null){camera.GlobalPosition=house.ToGlobal(new Vector3(-17,5,18));camera.LookAt(house.ToGlobal(new Vector3(-2,1,3)));await ArtShot("population-neighbourhood");}
   ShowMap();await ArtShot("population-map");CloseMenu();
  }
  GetTree().Quit();
 }catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}}
}
