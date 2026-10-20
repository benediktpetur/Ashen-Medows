using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 Label firstDayHud;PanelContainer firstDayCard;int firstDayLast=-1;double firstDayFlash;
 void SpawnFirstDay(){
  if(dungeon!=null)return;double x=ExpeditionWorld.SpawnX,z=ExpeditionWorld.SpawnZ;
  if(!NearActiveSurface(Ground(x,z),190))return;
  string id="first-day/trailhead";
  if(uniqueObjects.Add(id)){
   var root=new Node3D{Position=Ground(x+13,z-7)};worldRoot.AddChild(root);
   // A sealed blue carrier on a weathered picnic table, with a paper left beneath it.
   Box(root,new(0,.75f,0),new(1.7f,.12f,.85f),Wood,true);
   foreach(float side in new[]{-.65f,.65f})Box(root,new(side,.36f,0),new(.1f,.72f,.65f),Wood,true);
   Box(root,new(0,1.08f,0),new(.48f,.55f,.4f),new(.12f,.34f,.43f),true);
   Box(root,new(0,1.39f,0),new(.17f,.07f,.15f),Steel);StoryPaper(root,new(.55f,.83f,0));
   props.Add(new(){Id=id,Kind=38,Position=root.Position,Node=root});
   var board=new Node3D{Position=Ground(x+7,z-4)};root.AddChild(board);board.GlobalPosition=Ground(x+7,z-4);
   Tube(board,Vector3.Zero,new(0,1.7f,0),.055f,Wood);
   Box(board,new(0,1.5f,0),new(1.8f,.7f,.09f),Wood);
   board.AddChild(new Label3D{Text="BRIAR LANE\nWATER  →\nF1 / FIELD NOTES",Position=new(0,1.5f,.065f),FontSize=30,PixelSize=.006f,Modulate=new(.85f,.8f,.62f)});
  }
  // Finite, identifiable resources on a short loop; no timers or respawning handouts.
  for(int i=0;i<9;i++){double a=i*2.4;AddResource($"first-day/{game.Save.Seed}/forage/{i}",x+Math.Cos(a)*(14+i*2),z-18+Math.Sin(a)*(12+i),i%3,i%3==0?20:16,1);}
  id="first-day/pump";
  if(uniqueObjects.Add(id)){
   var root=new Node3D{Position=Ground(x-19,z-13)};worldRoot.AddChild(root);
   Tube(root,Vector3.Zero,new(0,.95f,0),.13f,new(.25f,.35f,.3f));Tube(root,new(0,.9f,0),new(.45f,.9f,0),.055f,Steel);
   Tube(root,new(0,1,0),new(-.45f,1.25f,0),.035f,Steel);Box(root,new(.4f,.1f,0),new(.6f,.2f,.55f),Steel);
   props.Add(new(){Id=id,Kind=39,Position=root.Position,Node=root});
  }
 }
 void FirstDayTrees(Node3D chunk,int cx,int cz,bool simulation){for(int i=0;i<3;i++){
  double x=ExpeditionWorld.SpawnX-28-i*5,z=ExpeditionWorld.SpawnZ+8+i*5;
  if(ExpeditionWorld.Cell(x)!=cx||ExpeditionWorld.Cell(z)!=cz)continue;
  string id=$"first-day/{game.Save.Seed}/pine/{i}";if(game.World.Changes.Cut(id))continue;
  var pine=new Node3D{Position=Ground(x,z)};chunk.AddChild(pine);
  if(simulation)PhysicsObstacle(pine,new CylinderShape3D{Radius=.45f,Height=4},new(0,2,0));else ArtPine(pine,6+i,i);
  props.Add(new(){Id=id,Kind=0,Tier=1,Node=pine,Position=pine.Position});
 }}
 string FirstDayPrompt(){if(dungeon!=null||buildMode)return "";if(NearProp(38,3)!=null)return "F / TRAILHEAD WATER & FIELD NOTES";if(NearProp(39,2.5f)!=null)return "F / DRAW WELL WATER / BOIL BEFORE DRINKING";return "";}
 bool FirstDayInteract(){if(dungeon!=null||buildMode)return false;if(NearProp(38,3)!=null){if(CoopHost&&coopActor>0){game.FirstDayAction("water");return true;}ShowFirstDay(true);return true;}
  if(NearProp(39,2.5f)!=null){if(!CoopRequest("interact"))game.Change(()=>{if(game.Life.RawWater<20){game.Life.RawWater++;game.Notice="Well water / untreated. Boil at a workbench (Y) or your campfire (F).";}else game.Notice="Carrying enough untreated water / purify your supply first";});return true;}return false;}
 void FirstDayCraft(int index){if(index<0||index>=ExpeditionRecipe.All.Length)return;var r=ExpeditionRecipe.All[index];if(!game.Knows(r)){if(!CoopRequest("research",index.ToString()))game.Research(index);}else if(!CoopRequest("craft",index.ToString()))game.CraftRecipe(r);game.TickFirstDay();ShowFirstDay();}
 void ShowFirstDay(bool atCarrier=false){
  game.TickFirstDay();var b=Panel("FIELD NOTES / MAKE IT THROUGH TODAY");Button(b,"BRIAR LANE / NEIGHBORHOOD GUIDE",ShowSuburbGuide);coopMenu="firstday";
  Text(b,"Someone has written on the back of a neighborhood notice:\n“Search the street before you chase the road. Drink. Make something sharp. Find a dry patch and leave yourself enough for tomorrow. If you hear the mill, you’ve gone too far.”");
  LocationButton(b,"Sealed water carrier",ExpeditionWorld.SpawnX+13,ExpeditionWorld.SpawnZ-7,()=>ShowFirstDay());
  if(atCarrier)Button(b,"SEARCH SEALED EMERGENCY CARRIER",()=>{if(!CoopRequest("firstday","water"))game.FirstDayAction("water");ShowFirstDay(true);});
  int step=game.FirstDayStep;var scroll=new ScrollContainer{CustomMinimumSize=new(895,step==5?150:270)};b.AddChild(scroll);var list=new VBoxContainer{CustomMinimumSize=new(870,0)};scroll.AddChild(list);
  for(int i=0;i<7;i++){bool done=(game.Life.FirstDayMarks&(1<<i))!=0;Text(list,(done?"✓  ":i==game.FirstDayStep?"→  ":"○  ")+ExpeditionGame.FirstDayTitles[i].ToUpperInvariant());if(!done||i==game.FirstDayStep)Text(list,ExpeditionGame.FirstDayHints[i]);}
  ExpeditionRecipe recipe=step==1?ExpeditionRecipe.All.First(r=>r.Kind=="weapon"&&r.Result==11):step==2?ExpeditionRecipe.All.First(r=>r.Kind=="tool"):step==4?ExpeditionRecipe.All[0]:null;
  if(recipe!=null){int index=Array.IndexOf(ExpeditionRecipe.All,recipe);Text(b,recipe.Name+" / "+(game.Knows(recipe)?game.RecipeReason(recipe):game.ResearchReason(recipe)));Button(b,(game.Knows(recipe)?"CRAFT / ":"RESEARCH / ")+recipe.Name,()=>FirstDayCraft(index));}
  if(step==5){
   Text(b,"Your Briar Lane house already provides shelter. Place the cot inside, with a chest nearby and a fire outside. Expand its yard with foundations, walls and roofs as you progress. G disables snapping for free placement; Page Up/Down fine-tunes height. Every piece uses normal costs and structural support.");
   var grid=new GridContainer{Columns=2};b.AddChild(grid);
   foreach(int kind in new[]{0,2,7,Array.FindIndex(BuildDefinition.All,d=>d.Function=="bed"),Array.FindIndex(BuildDefinition.All,d=>d.Function=="cook"),Array.FindIndex(BuildDefinition.All,d=>d.Function=="storage")}){
    int target=kind;var r=ExpeditionRecipe.All.First(r=>r.Kind=="build"&&r.Result==kind);bool known=game.Knows(r);
    Button(grid,(known?"PLACE / ":"RESEARCH / ")+r.Name,()=>{if(!game.Knows(r)){if(!CoopRequest("research",Array.IndexOf(ExpeditionRecipe.All,r).ToString()))game.Research(Array.IndexOf(ExpeditionRecipe.All,r));ShowFirstDay();}else{CloseMenu();selectedBuild=target;buildMode=true;}});
   }
  }
  if(step==6){Text(b,$"Carrying / food {game.Life.Food}, clean water {game.Craft.Water}, untreated {game.Life.RawWater}. F at a storage chest deposits supplies.");Button(b,"WATER & HEALTH",ShowWounds);}
  if(step==7)Text(b,"YOU HAVE A FOOTHOLD. A drink, a weapon, tools, shelter and tomorrow’s supplies. Nothing makes you invincible. But you have somewhere to return to. F5 plans your next expedition; the woodland leads northwest.");
  Text(b,game.Notice);Button(b,game.Life.FirstDayHidden?"SHOW FIRST-DAY GUIDANCE":"TUCK GUIDANCE AWAY / NO PENALTY",()=>{string action=game.Life.FirstDayHidden?"show":"hide";if(!CoopRequest("firstday",action))game.FirstDayAction(action);ShowFirstDay();});Button(b,"BACK TO WORLD",CloseMenu);
 }
 void DrawFirstDay(){
  if(firstDayHud==null){firstDayCard=new PanelContainer{MouseFilter=Control.MouseFilterEnum.Ignore,CustomMinimumSize=new(350,0)};firstDayCard.AddThemeStyleboxOverride("panel",Style(new(.045f,.065f,.06f,.88f),new(.35f,.37f,.25f),1,12));hudRoot.AddChild(firstDayCard);firstDayHud=new Label{MouseFilter=Control.MouseFilterEnum.Ignore,AutowrapMode=TextServer.AutowrapMode.WordSmart,CustomMinimumSize=new(325,0)};firstDayHud.AddThemeFontSizeOverride("font_size",13);firstDayCard.AddChild(firstDayHud);}
  int step=game.FirstDayStep;if(firstDayLast>=0&&step>firstDayLast)firstDayFlash=game.Clock+6;firstDayLast=step;
  firstDayCard.Position=new(GetViewport().GetVisibleRect().Size.X-380,205);firstDayCard.Visible=dungeon==null&&!game.Life.FirstDayHidden&&(step<7||game.Clock<firstDayFlash);
  firstDayHud.Text=step==7?"A FOOTHOLD / FIRST DAY COMPLETE\nYou have somewhere to return to.\nF5 / plan the next expedition":$"FIRST DAY / {step+1} OF 7\n{ExpeditionGame.FirstDayTitles[step].ToUpperInvariant()}\n"+(step==0?$"Blue water carrier / {ExpeditionGame.Distance(player.Position.X,player.Position.Z,ExpeditionWorld.SpawnX+13,ExpeditionWorld.SpawnZ-7):0}m from you\nC drinks clean water":step==1?"Loose branches, stones and flax.\nF gathers / K crafts a spear":step==2?"2 timber + 3 stone + 2 fiber\nK crafts / T equips axe":step==3?"Aim at a trunk, click to swing.\nRest between hits.":step==4?"6 timber + 4 stone / K crafting":step==5?"Cot and chest inside your house.\nFire outside / B opens building":"Store 2 food + 2 clean water.\nF uses your fire and chest")+"\nF1 / field notes & next steps";
 }
}
