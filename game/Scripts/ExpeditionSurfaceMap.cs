using Godot;
internal partial class ExpeditionMapCanvas {
 void DrawSurfaceMap(){
  var minor=new Color(.67f,.66f,.57f,.10f);
  for(int x=-1568;x<1568;x+=32)for(int z=-2400;z<288;z+=32){
   bool H(double a,double b)=>World.Tier(a,b)>=3?World.UrbanStreet(a,b):World.ResidentialPaved(a,b);
   if(H(x+16,z))DrawLine(Map(x,z),Map(x+32,z),minor,1);
   if(H(x,z+16))DrawLine(Map(x,z),Map(x,z+32),minor,1);
  }
  double sx=ExpeditionWorld.SpawnX,sz=ExpeditionWorld.SpawnZ,cross=sz-173;
  DrawLine(Map(sx,sz+40),Map(sx,sz-185),new Color(.71f,.67f,.54f),2);
  DrawLine(Map(sx,cross),Map(World.RouteX(cross),cross),new Color(.71f,.67f,.54f),2);
  if(!Heat)DrawMapDistricts();
 }
 void DrawMapDistricts(){
  // Actual static lot footprints, not a city grid painted across empty woodland.
  for(int x=-1552;x<1552;x+=32)for(int z=-2384;z<300;z+=32){
   if(!World.BuildingCell(x,z))continue;double half=World.BuildingHalf(x,z);
   var a=Map(x-half,z-half);DrawRect(new Rect2(a,Map(x+half,z+half)-a),new Color(.13f,.17f,.17f,.55f));
  }
  for(int x=-24;x<25;x++)for(int z=-4;z<5;z++){
   var lot=World.Residential(x,z);if(lot==null)continue;var p=Map(lot.X,lot.Z);DrawRect(new Rect2(p-new Vector2(1.5f,1),new Vector2(3,2)),new Color(.20f,.26f,.19f,.65f));
  }
  foreach(var stop in World.RoadsideStops()){var at=Map(stop.X,stop.Z);DrawRect(new Rect2(at-new Vector2(3,3),new Vector2(6,6)),new Color(.08f,.13f,.13f));DrawRect(new Rect2(at-new Vector2(2,2),new Vector2(4,4)),new Color(.94f,.85f,.53f));}
 }
 void DrawMapStructure(){
  if(Heat)return;
  var gold=new Color(.96f,.78f,.37f);
  foreach(var item in new[]{(Tier:4,Z:-2130d,Title:"4  DENSE CITY",Detail:"Tall blocks / communities / city depths"),(Tier:3,Z:-1460d,Title:"3  HIGHWAY & INDUSTRY",Detail:"Freight / workshops / roadside stops"),(Tier:2,Z:-780d,Title:"2  WOODLAND",Detail:"Trails / shelters / field locations"),(Tier:1,Z:-220d,Title:"1  SUBURBS",Detail:"Briar Lane / homes / shared supplies")}){
   var p=new Vector2(22,Map(-1470,item.Z).Y);DrawRect(new Rect2(p-new Vector2(8,17),new Vector2(244,39)),new Color(.055f,.09f,.085f,.90f));
   DrawRect(new Rect2(p-new Vector2(8,17),new Vector2(3,39)),gold);
   DrawString(ThemeDB.FallbackFont,p,item.Title,HorizontalAlignment.Left,-1,12,new Color(.94f,.91f,.79f));
   DrawString(ThemeDB.FallbackFont,p+new Vector2(0,15),item.Detail,HorizontalAlignment.Left,-1,10,new Color(.70f,.76f,.70f));
  }
  // Arrowheads follow the real curving route; north is towards later progression.
  foreach(double z in new[]{-200d,-550,-1150,-1740,-2350}){
   var at=Map(World.RouteX(z),z);var forward=(Map(World.RouteX(z-32),z-32)-at).Normalized();var side=new Vector2(-forward.Y,forward.X);
   DrawColoredPolygon(new[]{at+forward*6,at-forward*4+side*3,at-forward*4-side*3},gold);
  }
  var note=new Vector2(Size.X-265,14);DrawRect(new Rect2(note-new Vector2(7,14),new Vector2(258,47)),new Color(.05f,.08f,.075f,.92f));
  DrawString(ThemeDB.FallbackFont,note,"NORTH / RIVER CROSSING CLOSED",HorizontalAlignment.Left,-1,11,new Color(.83f,.88f,.85f));
  DrawString(ThemeDB.FallbackFont,note+new Vector2(0,17),"Gold route: 1 > 2 > 3 > 4 > city depths",HorizontalAlignment.Left,-1,10,gold);
  DrawRect(new Rect2(new Vector2(10,Size.Y-48),new Vector2(380,43)),new Color(.05f,.08f,.075f,.93f));
  DrawString(ThemeDB.FallbackFont,new Vector2(18,Size.Y-33),"WEST SIDE: PLAYABLE DISTRICTS / FEWER NAMED SITES",HorizontalAlignment.Left,-1,10,new Color(.80f,.85f,.77f));
  DrawString(ThemeDB.FallbackFont,new Vector2(18,Size.Y-18),"Square: roadside stop   /   Dark blocks: buildings",HorizontalAlignment.Left,-1,10,new Color(.88f,.84f,.69f));
  float scale=Size.X*400/3200;DrawLine(new Vector2(410,Size.Y-14),new Vector2(410+scale,Size.Y-14),Colors.Beige,2);
  DrawString(ThemeDB.FallbackFont,new Vector2(410,Size.Y-21),"400 m",HorizontalAlignment.Left,-1,10,Colors.Beige);
 }
}
