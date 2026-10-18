using Godot;
internal partial class ExpeditionMapCanvas {
 internal ExpeditionJourney Journey;
 void DrawJourneyMap(){if(Journey?.Active!=true||Journey.Seed!=World.Seed||Journey.Site<0||Journey.Site>=ExpeditionContentSite.All.Length)return;var site=ExpeditionContentSite.Place(World,Journey.Site);var target=Journey.Reached?Map(Journey.HomeX,Journey.HomeZ):Map(site.X,site.Z);DrawLine(Map(PlayerPosition.X,PlayerPosition.Z),target,new Color(.9f,.75f,.3f,.65f),2);DrawCircle(target,8,Colors.Gold,false,2);DrawString(ThemeDB.FallbackFont,target+new Vector2(-120,-12),Journey.Reached?"RETURN REFUGE":"EXPEDITION DESTINATION",HorizontalAlignment.Left,-1,11,Colors.Gold);}
}
