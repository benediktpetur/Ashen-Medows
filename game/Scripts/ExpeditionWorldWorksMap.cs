using Godot;
internal partial class ExpeditionMapCanvas {
 void DrawWorkMap(){if(World.Changes?.Works==null)return;foreach(var s in World.Changes.Works){if(s.Choice==0||WorldWork.At(s.Site)==null)continue;var at=ExpeditionContentSite.Place(World,s.Site);var p=Map(at.X,at.Z);var color=s.Finished?s.Choice==1?new Color(.48f,.85f,.58f):new Color(.88f,.43f,.27f):new Color(.95f,.76f,.33f);DrawRect(new Rect2(p-new Vector2(6,6),new Vector2(12,12)),color,false,2);DrawString(ThemeDB.FallbackFont,p+new Vector2(9,13),s.Finished?s.Choice==1?"RESTORED":"STRIPPED":"WORK IN PROGRESS",HorizontalAlignment.Left,-1,9,color);}}
}
