using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

internal sealed class AtlasMarker {
 internal string Id,Name,Group;
 internal double X,Z;
 internal Color Color;
 internal AtlasMarker(string id,string name,string group,double x,double z,Color color){Id=id;Name=name;Group=group;X=x;Z=z;Color=color;}
}

public partial class MigrationScene {
 void ShowInteractiveAtlas(){
  var b=Panel("WORLD ATLAS / 9 KM / SEED "+game.Save.Seed);
  var map=new ExpeditionMapCanvas{World=game.World,Journey=game.Life.Journey,PlayerPosition=player.Position,PinName=HasLocationPin?locationPinName:"",PinX=locationPinX,PinZ=locationPinZ,CustomMinimumSize=new Vector2(590,400),SizeFlagsHorizontal=Control.SizeFlags.ExpandFill,ClipContents=true,MouseFilter=Control.MouseFilterEnum.Stop};
  map.LoadAtlasPreferences();
  var toolbar=new HBoxContainer();b.AddChild(toolbar);
  Button(toolbar,"−",()=>map.ZoomAt(map.Size/2,1/1.25f));
  Button(toolbar,"+",()=>map.ZoomAt(map.Size/2,1.25f));
  Button(toolbar,"FIT WORLD",()=>{map.Zoom=1;map.Pan=Vector2.Zero;map.QueueRedraw();});
  Button(toolbar,"FIND ME",()=>{map.Zoom=Math.Max(4,map.Zoom);map.Pan=Vector2.Zero;map.Pan=map.Size/2-map.WorldToAtlas(player.Position.X,player.Position.Z);map.QueueRedraw();});
  var heat=new CheckButton{Text="Danger heatmap",ButtonPressed=map.Heat};toolbar.AddChild(heat);heat.Toggled+=v=>{map.Heat=v;map.SaveAtlasPreferences();map.QueueRedraw();};
  var night=new CheckButton{Text="Night",ButtonPressed=map.NightHeat};toolbar.AddChild(night);night.Toggled+=v=>{map.NightHeat=v;map.SaveAtlasPreferences();map.QueueRedraw();};
  var mapBody=new HBoxContainer();b.AddChild(mapBody);mapBody.AddChild(map);
  Text(b,"Wheel: zoom • Drag: pan • Right-click: add named marker • Left-click a marker: details / remove");
  var pinTools=new HBoxContainer();b.AddChild(pinTools);
  var add=new CheckButton{Text="Place custom marker"};pinTools.AddChild(add);add.Toggled+=v=>map.PlacingMarker=v;
  var labels=new CheckButton{Text="Labels",ButtonPressed=map.ShowLabels};pinTools.AddChild(labels);labels.Toggled+=v=>{map.ShowLabels=v;map.SaveAtlasPreferences();map.QueueRedraw();};
  Button(pinTools,"MARK MY POSITION",()=>map.OpenMarkerEditor(player.Position.X,player.Position.Z));
  b.MoveChild(pinTools,3);
  var listScroll=new ScrollContainer{CustomMinimumSize=new Vector2(280,400),HorizontalScrollMode=ScrollContainer.ScrollMode.Disabled};mapBody.AddChild(listScroll);
  var list=new VBoxContainer{SizeFlagsHorizontal=Control.SizeFlags.ExpandFill};listScroll.AddChild(list);
  void Refresh(){
   foreach(Node child in list.GetChildren()){list.RemoveChild(child);child.QueueFree();}
   foreach(var group in map.AtlasMarkers().GroupBy(m=>m.Group)){
    var items=group.ToArray();
    var groupToggle=new CheckButton{Text=group.Key+" / show all",ButtonPressed=items.All(m=>!map.HiddenMarkers.Contains(m.Id))};list.AddChild(groupToggle);
    groupToggle.Toggled+=visible=>{foreach(var m in items){if(visible)map.HiddenMarkers.Remove(m.Id);else map.HiddenMarkers.Add(m.Id);}map.SaveAtlasPreferences();map.QueueRedraw();Refresh();};
    var grid=new GridContainer{Columns=1};list.AddChild(grid);
    foreach(var marker in items){var copy=marker;var toggle=new CheckBox{Text=copy.Name,ButtonPressed=!map.HiddenMarkers.Contains(copy.Id),CustomMinimumSize=new Vector2(250,30),ClipText=true,TooltipText=copy.Name};grid.AddChild(toggle);toggle.Toggled+=visible=>{if(visible)map.HiddenMarkers.Remove(copy.Id);else map.HiddenMarkers.Add(copy.Id);map.SaveAtlasPreferences();map.QueueRedraw();groupToggle.SetPressedNoSignal(items.All(m=>!map.HiddenMarkers.Contains(m.Id)));};}
   }
  }
  map.MarkersChanged=Refresh;Refresh();
  if(HasLocationPin){Button(b,"CLEAR LOCATION PIN",()=>{locationPinName="";ShowMap();});if(locationPinBack!=null)Button(b,"BACK TO LOCATION DETAILS",locationPinBack);}
  var routes=new HBoxContainer();b.AddChild(routes);Button(routes,"YOUR COMMUNITY",()=>ShowCommunity());Button(routes,"BOSS LOCATIONS",ShowCampaignGuide);Button(routes,"WORLD CHANGES",ShowWorldWorksLedger);Button(routes,"EXPEDITION PLANNER / F5",ShowJourneyPlanner);Button(b,"BACK",CloseMenu);
 }
}

internal partial class ExpeditionMapCanvas {
 internal float Zoom=1;
 internal Vector2 Pan;
 internal bool ShowLabels=true,PlacingMarker;
 internal Action MarkersChanged;
 internal readonly HashSet<string> HiddenMarkers=new();
 readonly List<AtlasMarker> customMarkers=new();
 bool dragging,moved;
 Vector2 dragStart;
 ConfigFile atlasPreferences;
 string AtlasSection=>"world_"+World.Seed;
 internal string AtlasPreferencesPath="user://world-atlas.cfg";
 internal Vector2 WorldToAtlas(double x,double z)=>Map(x,z);
 internal void LoadAtlasPreferences(){
  atlasPreferences=new ConfigFile();atlasPreferences.Load(AtlasPreferencesPath);
  ShowLabels=atlasPreferences.GetValue(AtlasSection,"labels",true).AsBool();Heat=atlasPreferences.GetValue(AtlasSection,"heat",false).AsBool();NightHeat=atlasPreferences.GetValue(AtlasSection,"night",false).AsBool();
  foreach(string id in atlasPreferences.GetValue(AtlasSection,"hidden",Array.Empty<string>()).AsStringArray())HiddenMarkers.Add(id);
  int count=Math.Clamp(atlasPreferences.GetValue(AtlasSection,"count",0).AsInt32(),0,10000);
  for(int i=0;i<count;i++){string key="marker_"+i;string id=atlasPreferences.GetValue(AtlasSection,key+"_id","").AsString();string name=atlasPreferences.GetValue(AtlasSection,key+"_name","").AsString();var pos=atlasPreferences.GetValue(AtlasSection,key+"_pos",Vector2.Zero).AsVector2();if(id!=""&&name!=""&&double.IsFinite(pos.X)&&double.IsFinite(pos.Y))customMarkers.Add(new(id,name,"Custom markers",pos.X,pos.Y,Colors.Yellow));}
 }
 internal void SaveAtlasPreferences(){
  atlasPreferences??=new ConfigFile();atlasPreferences.SetValue(AtlasSection,"hidden",HiddenMarkers.ToArray());atlasPreferences.SetValue(AtlasSection,"labels",ShowLabels);atlasPreferences.SetValue(AtlasSection,"heat",Heat);atlasPreferences.SetValue(AtlasSection,"night",NightHeat);atlasPreferences.SetValue(AtlasSection,"count",customMarkers.Count);
  for(int i=0;i<customMarkers.Count;i++){var m=customMarkers[i];string key="marker_"+i;atlasPreferences.SetValue(AtlasSection,key+"_id",m.Id);atlasPreferences.SetValue(AtlasSection,key+"_name",m.Name);atlasPreferences.SetValue(AtlasSection,key+"_pos",new Vector2((float)m.X,(float)m.Z));}
  if(atlasPreferences.Save(AtlasPreferencesPath)!=Error.Ok)GD.PushWarning("Could not save world atlas markers.");
 }
 internal void ZoomAt(Vector2 anchor,float factor){float next=Math.Clamp(Zoom*factor,1,16);Pan=anchor-Size/2-(anchor-Size/2-Pan)*(next/Zoom);Zoom=next;ClampAtlasPan();QueueRedraw();}
 void ClampAtlasPan(){float side=Math.Min(Size.X,Size.Y)*Zoom;Pan=new Vector2(Math.Clamp(Pan.X,-side/2,side/2),Math.Clamp(Pan.Y,-side/2,side/2));}
 public override void _Notification(int what){if(what==NotificationResized)QueueRedraw();}
 public override void _GuiInput(InputEvent e){
  if(e is InputEventMouseButton mouse){
   if(mouse.Pressed&&(mouse.ButtonIndex==MouseButton.WheelUp||mouse.ButtonIndex==MouseButton.WheelDown)){ZoomAt(mouse.Position,mouse.ButtonIndex==MouseButton.WheelUp?1.25f:1/1.25f);AcceptEvent();return;}
   if(mouse.ButtonIndex==MouseButton.Left){if(mouse.Pressed){dragging=true;moved=false;dragStart=mouse.Position;}else if(dragging){dragging=false;if(!moved)AtlasClick(mouse.Position,PlacingMarker);}AcceptEvent();}
   if(mouse.Pressed&&mouse.ButtonIndex==MouseButton.Right){AtlasClick(mouse.Position,true);AcceptEvent();}
  }
  if(e is InputEventMouseMotion motion&&dragging){if(motion.Position.DistanceTo(dragStart)>4)moved=true;if(moved){Pan+=motion.Relative;ClampAtlasPan();QueueRedraw();}AcceptEvent();}
 }
 void AtlasClick(Vector2 at,bool place){
  if(!place){var hit=AtlasMarkers().Where(m=>!HiddenMarkers.Contains(m.Id)).OrderBy(m=>Map(m.X,m.Z).DistanceSquaredTo(at)).FirstOrDefault();if(hit!=null&&Map(hit.X,hit.Z).DistanceTo(at)<=14){OpenMarkerEditor(hit.X,hit.Z,hit);return;}}
  if(!place||!AtlasMapRect.HasPoint(at))return;
  var uv=(at-AtlasMapRect.Position)/AtlasMapRect.Size;OpenMarkerEditor(ExpeditionWorld.MinX+uv.X*9000,ExpeditionWorld.MinZ+uv.Y*9000);
 }
 internal void OpenMarkerEditor(double x,double z,AtlasMarker existing=null){
  var dialog=new ConfirmationDialog{Title=existing==null?"Place custom marker":existing.Name,MinSize=new Vector2I(380,170),OkButtonText=existing==null?"Save marker":"Save"};
  var content=new VBoxContainer();dialog.AddChild(content);content.AddChild(new Label{Text=$"World coordinates: {x:0}, {z:0}"});
  bool editable=existing==null||customMarkers.Contains(existing);
  var name=new LineEdit{Text=existing?.Name??"",PlaceholderText="Marker name",MaxLength=64,Editable=editable};content.AddChild(name);
  dialog.Confirmed+=()=>{if(editable){string title=name.Text.Trim();if(title=="")title="My marker";if(existing==null)customMarkers.Add(new("custom/"+Guid.NewGuid().ToString("N"),title,"Custom markers",x,z,Colors.Yellow));else existing.Name=title;SaveAtlasPreferences();MarkersChanged?.Invoke();QueueRedraw();}dialog.QueueFree();};
  if(existing!=null){var remove=new Button{Text=editable?"Delete marker":"Hide this marker"};content.AddChild(remove);remove.Pressed+=()=>{if(editable){customMarkers.Remove(existing);HiddenMarkers.Remove(existing.Id);}else HiddenMarkers.Add(existing.Id);SaveAtlasPreferences();MarkersChanged?.Invoke();QueueRedraw();dialog.QueueFree();};}
  dialog.Canceled+=()=>dialog.QueueFree();AddChild(dialog);dialog.PopupCentered();name.GrabFocus();
 }
 internal List<AtlasMarker> AtlasMarkers(){
  var result=new List<AtlasMarker>();
  void Add(string id,string name,string group,double x,double z,Color color)=>result.Add(new(id,name,group,x,z,color));
  Add("player","You","Navigation",PlayerPosition.X,PlayerPosition.Z,Colors.White);
  Add("start","Briar Lane / start","Navigation",ExpeditionWorld.SpawnX,ExpeditionWorld.SpawnZ,Colors.LightGreen);
  Add("home","Your house / settlement","Navigation",ExpeditionWorld.HomeX,ExpeditionWorld.HomeZ,Colors.LightGreen);
  if(!string.IsNullOrEmpty(PinName))Add("location-pin",PinName,"Navigation",PinX,PinZ,Colors.Yellow);
  if(Journey?.Active==true&&Journey.Seed==World.Seed&&Journey.Site>=0&&Journey.Site<ExpeditionContentSite.All.Length){var site=ExpeditionContentSite.Place(World,Journey.Site);Add("journey",Journey.Reached?"Return refuge":"Expedition destination","Navigation",Journey.Reached?Journey.HomeX:site.X,Journey.Reached?Journey.HomeZ:site.Z,Colors.Gold);}
  Add("church","St. Brigid's","Story locations",ExpeditionLastLetter.ChurchX(World),ExpeditionLastLetter.ChurchZ,Colors.LightBlue);
  if(World.Changes?.LastLetter?.Stage==1)Add("letter","Letter / Samuel","Story locations",ExpeditionLastLetter.HomeX(World),ExpeditionLastLetter.HomeZ,Colors.Gold);
  Add("willow","Willow House","Story locations",World.SafeStoryX(World.RouteX(-930)+55,-930,32),-930,Colors.LightBlue);
  Add("mill","Lumber mill","Story locations",World.SafeStoryX(World.RouteX(-780)+90,-780,38),-780,Colors.Orange);
  if(World.Changes?.Community is ExpeditionCommunity camp){foreach(var n in camp.Survivors)if(!n.Member)Add("rescue/"+n.Name,"Rescue / "+n.Name,"Survivors",n.X,n.Z,Colors.Cyan);if(camp.Established)Add("community","Your community","Survivors",camp.X,camp.Z,Colors.LightGreen);}
  foreach(var d in ExpeditionDungeon.Nearby(World,0,0))Add($"dungeon/{d.X}/{d.Z}","Dungeon / "+$"{d.X:0}, {d.Z:0}","Dungeons",d.X,d.Z,Colors.Orange);
  if(World.Changes?.Settlements!=null)foreach(var t in World.Changes.Settlements){Add("town/"+t.Index,ExpeditionGame.SettlementNames[t.Index],"Settlements",t.X,t.Z,t.Fallen?Colors.Gray:Colors.LightGreen);if(t.StoryStage==1&&t.QuestZ!=0)Add("scout/"+t.Index,"Missing / "+MigrationScene.SettlementScoutMapName(t.Index),"Missions",t.QuestX,t.QuestZ,Colors.Gold);if(t.Index==3&&World.Changes.Ledger?.State=="debt"&&t.QuestZ!=0)Add("shipment","Seized shipment","Missions",t.QuestX,t.QuestZ,Colors.Gold);}
  var ledger=World.Changes?.Ledger;if(ledger?.RaidScheduled==true&&!ledger.RaidOver&&ledger.RaidZ!=0)Add("raid","Ledger retaliation","Missions",ledger.RaidX,ledger.RaidZ,Colors.Red);
  var fever=World.Changes?.FeverStory;if(fever?.GraveRead==true)Add("grave","Lily","Story locations",fever.GraveX,fever.GraveZ,Colors.Burlywood);
  if(World.Changes?.FieldSites!=null)foreach(var s in World.Changes.FieldSites.Where(s=>s.Seen||s.Accepted)){if(s.Index<0||s.Index>=ExpeditionContentSite.All.Length)continue;var at=ExpeditionContentSite.Place(World,s.Index);Add("field/"+s.Index,ExpeditionContentSite.All[s.Index].Name,"Field sites",at.X,at.Z,s.Delivered||s.Stripped?Colors.Gray:s.Accepted?Colors.Gold:Colors.LightSkyBlue);}
  var ev=World.Changes?.FieldEvent;if(ev?.Stage==1)Add("event",ev.Kind==0?"Migrating horde":ev.Kind==1?"Travelling trader":"Courier in distress","Events",ev.X,ev.Z,Colors.OrangeRed);
  if(World.Changes?.Works!=null)foreach(var s in World.Changes.Works){if(s.Choice==0||WorldWork.At(s.Site)==null)continue;var at=ExpeditionContentSite.Place(World,s.Site);Add("work/"+s.Site,WorldWork.At(s.Site).Name+" / "+(s.Finished?s.Choice==1?"Restored":"Stripped":"Work in progress"),"World projects",at.X,at.Z,s.Finished?Colors.LightGreen:Colors.Gold);}
  foreach(var region in new[]{(U:.20f,V:.12f,Name:"River City"),(U:.51f,V:.23f,Name:"Industry / rail yards"),(U:.29f,V:.53f,Name:"Suburbs"),(U:.76f,V:.18f,Name:"Eastern woodland"),(U:.60f,V:.68f,Name:"Rural farmland"),(U:.86f,V:.53f,Name:"Lake district"),(U:.20f,V:.87f,Name:"Southern forest")})Add("region/"+region.Name,region.Name,"Regions",ExpeditionWorld.MinX+region.U*9000,ExpeditionWorld.MinZ+region.V*9000,Colors.Beige);
  result.AddRange(customMarkers);return result;
 }
 internal void CheckAtlasInteractions(){
  var anchor=new Vector2(350,220);var before=(anchor-AtlasMapRect.Position)/AtlasMapRect.Size;
  ZoomAt(anchor,2);var after=(anchor-AtlasMapRect.Position)/AtlasMapRect.Size;
  MigrationChecks.Assert(before.DistanceTo(after)<.0001f,"Atlas zoom keeps cursor world position fixed");
  ZoomAt(Size/2,100);MigrationChecks.Assert(Zoom==16,"Atlas maximum zoom");ZoomAt(Size/2,.0001f);MigrationChecks.Assert(Zoom==1,"Atlas minimum zoom");
  customMarkers.Add(new("custom/test","Supplies","Custom markers",123,-456,Colors.Yellow));HiddenMarkers.Add("home");ShowLabels=false;SaveAtlasPreferences();
  var reload=new ExpeditionMapCanvas{World=World,AtlasPreferencesPath=AtlasPreferencesPath};reload.LoadAtlasPreferences();
  MigrationChecks.Assert(reload.customMarkers.Count==1&&reload.customMarkers[0].Name=="Supplies"&&reload.customMarkers[0].X==123&&reload.customMarkers[0].Z==-456&&reload.HiddenMarkers.Contains("home")&&!reload.ShowLabels,"Atlas custom markers and visibility survive reload");
  MigrationChecks.Assert(AtlasMarkers().Select(m=>m.Id).Distinct().Count()==AtlasMarkers().Count,"Atlas items have unique visibility IDs");
  reload.Free();customMarkers.Clear();HiddenMarkers.Clear();ShowLabels=true;Zoom=1;Pan=Vector2.Zero;
 }
 void DrawAtlasMarkers(){
  foreach(var marker in AtlasMarkers()){
   if(HiddenMarkers.Contains(marker.Id))continue;var p=Map(marker.X,marker.Z);if(!new Rect2(Vector2.Zero,Size).Grow(10).HasPoint(p))continue;
   if(marker.Id=="journey"&&!HiddenMarkers.Contains("player"))DrawLine(Map(PlayerPosition.X,PlayerPosition.Z),p,new Color(.9f,.75f,.3f,.65f),2);
   DrawCircle(p,6,Colors.Black);DrawCircle(p,4,marker.Color);
   if(marker.Group=="Custom markers"||marker.Id=="location-pin"){DrawLine(p,p-new Vector2(0,14),marker.Color,2);DrawCircle(p-new Vector2(0,16),5,marker.Color);}
   if(ShowLabels){var text=p+new Vector2(9,-8);DrawString(ThemeDB.FallbackFont,text+Vector2.One,marker.Name,HorizontalAlignment.Left,-1,11,Colors.Black);DrawString(ThemeDB.FallbackFont,text,marker.Name,HorizontalAlignment.Left,-1,11,marker.Color);}
  }
  DrawRect(new Rect2(0,0,Size.X,27),new Color(.03f,.06f,.06f,.9f));DrawString(ThemeDB.FallbackFont,new Vector2(10,19),$"NORTH / {Zoom:0.0}x"+(Heat?(NightHeat?" / DANGER: NIGHT":" / DANGER: DAY"):""),HorizontalAlignment.Left,-1,13,Colors.White);
 }
}

