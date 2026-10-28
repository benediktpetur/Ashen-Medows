using Godot;
internal partial class ExpeditionMapCanvas {
 void DrawSurfaceMap(){
  var minor=new Color(.53f,.51f,.44f,.6f);
  for(int x=-1568;x<1568;x+=32)for(int z=-2400;z<288;z+=32){
   bool H(double a,double b)=>World.Tier(a,b)>=3?World.UrbanStreet(a,b):World.ResidentialPaved(a,b);
   if(H(x+16,z))DrawLine(Map(x,z),Map(x+32,z),minor,1);
   if(H(x,z+16))DrawLine(Map(x,z),Map(x,z+32),minor,1);
  }
  double sx=ExpeditionWorld.SpawnX,sz=ExpeditionWorld.SpawnZ,cross=sz-173;
  DrawLine(Map(sx,sz+40),Map(sx,sz-185),new Color(.71f,.67f,.54f),2);
  DrawLine(Map(sx,cross),Map(World.RouteX(cross),cross),new Color(.71f,.67f,.54f),2);
  if(!Heat)foreach(var p in new[]{new Vector2(700,120),new Vector2(700,-780),new Vector2(700,-1460),new Vector2(700,-2130)})
   DrawString(ThemeDB.FallbackFont,Map(p.X,p.Y),ExpeditionWorld.TierName(World.Tier(p.X,p.Y)),HorizontalAlignment.Left,-1,11,new Color(.85f,.82f,.70f));
 }
}
