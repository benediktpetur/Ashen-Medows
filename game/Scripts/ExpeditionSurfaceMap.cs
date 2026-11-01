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
  foreach(var region in new[]{(U:.20f,V:.12f,Name:"RIVER CITY"),(U:.51f,V:.23f,Name:"INDUSTRY / RAIL YARDS"),(U:.29f,V:.53f,Name:"SUBURBS"),(U:.76f,V:.18f,Name:"EASTERN WOODLAND"),(U:.60f,V:.68f,Name:"RURAL FARMLAND"),(U:.86f,V:.53f,Name:"LAKE DISTRICT"),(U:.20f,V:.87f,Name:"SOUTHERN FOREST")}){
   var p=Map(ExpeditionWorld.MinX+region.U*9000,ExpeditionWorld.MinZ+region.V*9000);DrawRect(new Rect2(p-new Vector2(5,14),new Vector2(region.Name.Length*6.5f+10,20)),new Color(.04f,.08f,.07f,.85f));DrawString(ThemeDB.FallbackFont,p,region.Name,HorizontalAlignment.Left,-1,11,Colors.Beige);
  }
 }
 Texture2D atlasMapTexture;
 void DrawAtlasMap(){
  atlasMapTexture??=GD.Load<Texture2D>("res://WorldMap/reference.png");DrawTextureRect(atlasMapTexture,AtlasMapRect,false);
  if(Heat)for(int x=(int)ExpeditionWorld.MinX;x<ExpeditionWorld.MaxX;x+=128)for(int z=(int)ExpeditionWorld.MinZ;z<ExpeditionWorld.MaxZ;z+=128){float heat=(float)World.SpawnHeat(x,z,NightHeat);DrawRect(new Rect2(Map(x,z),new Vector2(AtlasMapRect.Size.X*128/9000+1,AtlasMapRect.Size.Y*128/9000+1)),new Color(.85f,.15f,.08f,heat*.55f));}
 }
}
