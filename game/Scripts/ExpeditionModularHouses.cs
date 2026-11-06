using Godot;

public partial class MigrationScene {
 const float HouseBay=3, HouseWallHeight=2.5f, HouseWallDepth=.2f, HouseFloorTop=.16f;
 const string ModularWallArt="res://Assets/WorldModular/Walls/";
 enum HouseWallOpening { Solid, Door, Window }
 static readonly string[] HouseWallFamilies={"Clapboard","Brick","Timber","Plaster"};
 static readonly (int Width,int Depth)[] HouseFootprints={(3,3),(3,4),(4,3),(4,4)};
 readonly System.Collections.Generic.Dictionary<string,PackedScene> modularWallScenes=new();
 StandardMaterial3D modularWallPlaceholder;
 BoxMesh modularWallCube;
 StandardMaterial3D ModularWallPlaceholder()=>modularWallPlaceholder??=new StandardMaterial3D{
  Transparency=BaseMaterial3D.TransparencyEnum.Alpha,AlbedoColor=new(.37f,.74f,.81f,.38f),
  ShadingMode=BaseMaterial3D.ShadingModeEnum.Unshaded
 };

 bool AddModularWallArt(Node3D root,string asset,Vector3 size)=>
  AddModularArt(root,ModularWallArt+asset+".glb",size/new Vector3(HouseBay,HouseWallHeight,HouseWallDepth));

 bool AddModularArt(Node3D root,string path,Vector3 scale,Vector3 position=default,float yaw=0){
  if(!modularWallScenes.TryGetValue(path,out var art)){
   art=ResourceLoader.Exists(path,"PackedScene")?GD.Load<PackedScene>(path):null;
   modularWallScenes[path]=art;
  }
  if(art==null)return false;
  var model=art.Instantiate<Node3D>();
  model.Name=System.IO.Path.GetFileNameWithoutExtension(path);
  model.Scale*=scale;
  model.Position+=position;
  model.RotateY(Mathf.DegToRad(yaw));
  foreach(var child in model.FindChildren("*","MeshInstance3D",true,false))
   if(child is MeshInstance3D mesh){mesh.LodBias=.18f;mesh.VisibilityRangeEnd=190;}
  root.AddChild(model);
  return true;
 }

 bool PlaceBatchPart(Node3D root,string key,Vector3 bottom,Vector3 size,float yaw=0)=>
  AddModularArt(root,"res://Assets/WorldModular/Parts/"+key+".glb",size,bottom,yaw);

 void BatchPartBox(Node3D root,string key,Vector3 center,Vector3 size,Color fallback,bool simulation=false,bool solid=false){
  if(!simulation&&!PlaceBatchPart(root,key,center-Vector3.Up*size.Y/2,size)){
   Box(root,center,size,fallback,solid);
   return;
  }
  if(solid)PhysicsObstacle(root,new BoxShape3D{Size=size},center);
 }

 void ModularHouse(Node3D house,int style,bool simulation){
  var (columns,rows)=HouseFootprints[style%HouseFootprints.Length];
  float halfWidth=columns*HouseBay/2,halfDepth=rows*HouseBay/2;
  string family=HouseWallFamilies[style%HouseWallFamilies.Length];
  string home=house.HasMeta("house_id")?house.GetMeta("house_id").AsString():"briar/"+house.Name;
  HouseFoundation(house,new(2*halfWidth,2*halfDepth),simulation);
  void Slab(Vector3 center,Vector3 size,Color color){
   if(simulation)PhysicsObstacle(house,new BoxShape3D{Size=size},center);
   else Box(house,center,size,color,true);
  }
  float ceiling=HouseWallHeight+HouseFloorTop;
  bool ceilingArt=!simulation&&PlaceBatchPart(house,"ceiling-plaster",new(0,ceiling,0),new(2*halfWidth,.16f,2*halfDepth));
  for(int x=0;x<columns;x++)for(int z=0;z<rows;z++){
   var center=new Vector3(-halfWidth+(x+.5f)*HouseBay,.08f,-halfDepth+(z+.5f)*HouseBay);
   if(simulation)Slab(center,new(HouseBay,.16f,HouseBay),Wood);
   else PhysicsObstacle(house,new BoxShape3D{Size=new(HouseBay,.16f,HouseBay)},center);
   var roofCenter=center+Vector3.Up*ceiling;
   if(ceilingArt)PhysicsObstacle(house,new BoxShape3D{Size=new(HouseBay,.16f,HouseBay)},roofCenter);
   else Slab(roofCenter,new(HouseBay,.16f,HouseBay),Ink);
  }
  if(!simulation)BatchPartBox(house,"floor-timber",new(0,.08f,0),new(2*halfWidth,.16f,2*halfDepth),Wood);
  void Wall(string slot,Vector3 at,float yaw,float width,HouseWallOpening opening)=>
   ModularHouseWall(house,home,slot,at,yaw,width,family,opening,simulation);
  // Keep the central entrance aligned with the map's existing front approach.
  Wall("front-entry",new(0,HouseFloorTop,halfDepth),180,HouseBay,HouseWallOpening.Door);
  for(int side=-1;side<=1;side+=2){
   float rest=halfWidth-HouseBay/2;
   Wall($"front-{side}-window",new(side*HouseBay,HouseFloorTop,halfDepth),180,HouseBay,HouseWallOpening.Window);
   if(rest>HouseBay)Wall($"front-{side}-end",new(side*(halfWidth-(rest-HouseBay)/2),HouseFloorTop,halfDepth),180,rest-HouseBay,HouseWallOpening.Solid);
   for(int z=0;z<rows;z++)
    Wall($"side-{side}-{z}",new(side*halfWidth,HouseFloorTop,-halfDepth+(z+.5f)*HouseBay),-90*side,HouseBay,z%2==0?HouseWallOpening.Window:HouseWallOpening.Solid);
  }
  for(int x=0;x<columns;x++)
   Wall($"rear-{x}",new(-halfWidth+(x+.5f)*HouseBay,HouseFloorTop,-halfDepth),0,HouseBay,x%2==0?HouseWallOpening.Window:HouseWallOpening.Solid);

  var end=house.Transform*new Vector3(0,0,13);
  float rampEnd=(float)game.World.Height(end.X,end.Z)-house.Position.Y+.03f;
  var a=new Vector3(-1.1f,HouseFloorTop,halfDepth-.12f);
  var b=new Vector3(1.1f,HouseFloorTop,halfDepth-.12f);
  var c=new Vector3(1.1f,rampEnd,13);
  var d=new Vector3(-1.1f,rampEnd,13);
  var under=Vector3.Up*.12f;
  PhysicsObstacle(house,new ConvexPolygonShape3D{Points=new[]{a,b,c,d,a-under,b-under,c-under,d-under}},Vector3.Zero);

  var furniture=new Node3D{Name=house.Name,Scale=Vector3.One*.7f};
  if(house.HasMeta("house_id"))furniture.SetMeta("house_id",house.GetMeta("house_id"));
  house.AddChild(furniture);
  HouseFurniture(furniture,style,simulation);
  GardenRailings(house,simulation);
  string yardWall=$"wall-{family.ToLowerInvariant()}-half";
  if(family is "Clapboard" or "Timber" or "Plaster"){
   var foot=house.Transform*new Vector3(-7.25f,0,-7.2f);
   float y=(float)game.World.Height(foot.X,foot.Z)-house.Position.Y;
   BatchPartBox(house,yardWall,new(-7.25f,y+.48f,-7.2f),new(2.4f,.96f,.18f),Wood,simulation,true);
  }
  if(simulation)return;
  BatchHouseRoofDetails(house,style,halfWidth,halfDepth);
  BatchWallCorners(house,family,halfWidth,halfDepth,HouseFloorTop,HouseWallHeight);
  var ramp=new SurfaceTool();
  ramp.Begin(Mesh.PrimitiveType.Triangles);
  foreach(var point in new[]{a,b,c,a,c,d})ramp.AddVertex(point);
  ramp.GenerateNormals();
  house.AddChild(new MeshInstance3D{Mesh=ramp.Commit(),MaterialOverride=Textured(Wood,0,.6f)});
  HouseApproach(house);
  StoryPaper(house,new(.42f,.581f,-.84f));
  house.AddChild(new Label3D{
   Text=house.HasMeta("house_address")?house.GetMeta("house_address").AsString():(12+style*2).ToString(),
   Position=new(.93f,2.12f,halfDepth+.13f),PixelSize=.004f,FontSize=26
  });
 }

 void ModularHouseWall(Node3D house,string home,string slot,Vector3 at,float yaw,float width,string family,HouseWallOpening opening,bool simulation,float height=HouseWallHeight,float depth=HouseWallDepth,bool protectedWall=false){
  string asset=$"Wall-{family}-{(int)opening+1}";
  var body=new StaticBody3D{Name="Wall_"+slot.Replace('/','_'),Position=at,RotationDegrees=new(0,yaw,0)};
  house.AddChild(body);
  body.SetMeta("wall_asset",asset);
  body.SetMeta("wall_salvage_material",family is "Brick" or "Plaster" or "Concrete" or "Industrial"?1:0);
  if(!protectedWall)RegisterDamageNode(body,$"wall/{home}/modular/{slot}",180);
  if(body.IsQueuedForDeletion())return;

  var dimensions=new Vector3(width,height,depth);
  bool hasArt=false;
  if(!simulation){
   string material=family.ToLowerInvariant();
   if(opening==HouseWallOpening.Solid&&height is >=.75f and <=1.4f)
    hasArt=PlaceBatchPart(body,$"wall-{material}-half",Vector3.Zero,dimensions);
   if(!hasArt&&opening==HouseWallOpening.Solid&&(slot=="2/0/1"||slot=="rear-1"))
    hasArt=PlaceBatchPart(body,$"wall-{material}-damaged",Vector3.Zero,dimensions);
   if(!hasArt)hasArt=AddModularWallArt(body,asset,dimensions);
   if(!hasArt&&opening==HouseWallOpening.Solid)
    hasArt=PlaceBatchPart(body,$"wall-{material}-damaged",Vector3.Zero,dimensions);
  }

  void Panel(float x,float bottom,float panelWidth,float panelHeight){
   var size=new Vector3(panelWidth,panelHeight,depth);
   var center=new Vector3(x,bottom+panelHeight/2,0);
   body.AddChild(new CollisionShape3D{Shape=new BoxShape3D{Size=size},Position=center});
   if(simulation||hasArt)return;
   body.AddChild(new MeshInstance3D{
    Mesh=modularWallCube??=new BoxMesh{Size=Vector3.One},Scale=size,Position=center,MaterialOverride=ModularWallPlaceholder(),
    CastShadow=GeometryInstance3D.ShadowCastingSetting.Off
   });
  }
  if(opening==HouseWallOpening.Solid){Panel(0,0,width,height);return;}
  float holeWidth=(opening==HouseWallOpening.Door?1.2f:1.65f)*width/HouseBay;
  float sill=opening==HouseWallOpening.Door?0:height/HouseWallHeight;
  float top=(opening==HouseWallOpening.Door?2.24f:2.1f)*height/HouseWallHeight;
  float jamb=(width-holeWidth)/2;
  Panel(-(holeWidth+jamb)/2,0,jamb,height);
  Panel((holeWidth+jamb)/2,0,jamb,height);
  Panel(0,top,holeWidth,height-top);
  if(sill>0){
   Panel(0,0,holeWidth,sill);
   if(!simulation&&opening==HouseWallOpening.Window){
    string insert=family is "Industrial" or "Concrete"?"window-steel-frame":slot=="rear-2"?"window-broken-pane":null;
    if(insert!=null)PlaceBatchPart(body,insert,new(0,sill,0),new(holeWidth,top-sill,Mathf.Min(.12f,depth)));
   }
  }
 }
}
