using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 const double MillZ=-780;double MillX=>game.World.RouteX(MillZ)+90;string MillBossId=>"boss/mill/"+game.Save.Seed;
 Node3D millRoot;int millSeed;double millFloor;
 bool IsMillBoss(string id)=>id!=null&&id.StartsWith("boss/mill/");
 bool InMill(double x,double z,double radius=32)=>ExpeditionGame.Distance(x,z,MillX,MillZ)<radius;
 static readonly string[] MillNoteTitles={"THE ROAD CREW'S WARNING","A LOGGER'S LAST SHIFT","SCAVENGER'S FIGHTING NOTES","THE MILL BELL"};
 static readonly string[] MillNotes={
  "We stopped sending crews to the lumber mill northwest of the trailhead. The foreman is still there. Bigger than a truck cab, stomach hanging through his apron. He crushed the logging crew's shelter and dragged their supplies back into the yard. Follow the woodland road north, past the shelter, then look east for the old mill signs. Don't go there with three rounds and a prayer. Bring a woodland weapon, reserve ammunition, water and several bandages.",
  "He drums those huge fists against his belly before he pounds the ground. The dust starts shaking in a wide circle. GET OUT OF THE CIRCLE. His jaw and the split in his apron are exposed while he catches his breath. Our rifle barely scratched him while he was moving, but those recovery shots hurt him. You can leave the yard if you need to. He doesn't follow far, but he'll recover before you return.",
  "The green swelling in his throat is your warning: he's about to vomit. Move sideways, away from where he's looking, and stay off the glowing sludge. When he lowers his shoulder, let him choose his line, then dodge across it. The heavy log piles stop him cold. That's your chance to shoot. Keep your bandages for his claws; patching won't give your health back. Below half health he turns furious and the warnings get shorter.",
  "The evacuation bell still works at the south entrance. He comes out when it rings. We left gaps around the yard: nobody should be trapped in there. Don't try fortifying the mill itself; the whole deck belongs to him. Outside the fence you can prepare a camp. If everybody runs or dies, you'll have to ring again and start over. There is something hardened inside his ruined chest. It might be useful at the woodland workbench."
 };
 void SpawnMillSites(){if(dungeon!=null)return;
  var locations=new[]{new Vector2((float)ExpeditionWorld.SpawnX+12,(float)ExpeditionWorld.SpawnZ-9),new Vector2((float)game.World.RouteX(-370)+7,-370),new Vector2((float)game.World.RouteX(-500)+12,-500),new Vector2((float)MillX, (float)MillZ+53)};
  for(int i=0;i<locations.Length;i++){var location=locations[i];string id="mill-note/"+i;if(uniqueObjects.Contains(id)||Distance(player.Position,Ground(location.X,location.Y))>190)continue;var n=new Node3D{Position=Ground(location.X,location.Y)};worldRoot.AddChild(n);Bevel(n,new(0,.35f,0),new(.8f,.7f,.65f),Wood);StoryPaper(n,new(0,.72f,0));n.AddChild(new Label3D{Text="WARNING / LUMBER MILL",Position=new(0,1.2f,0),FontSize=24,PixelSize=.007f,Billboard=BaseMaterial3D.BillboardModeEnum.Enabled,Modulate=new(.85f,.72f,.43f)});props.Add(new WorldProp{Id=id,Kind=12,Material=i,Node=n,Position=n.Position});uniqueObjects.Add(id);}
  bool near=CoopHost?ActiveSurfaceCenters().Any(p=>InMill(p.X,p.Z,220)):InMill(player.Position.X,player.Position.Z,220);
  if(GodotObject.IsInstanceValid(millRoot)&&millRoot.IsInsideTree()&&millSeed==game.Save.Seed){millRoot.Visible=InMill(player.Position.X,player.Position.Z,230);if(!near){millRoot.QueueFree();millRoot=null;props.RemoveAll(p=>p.Kind==13);uniqueObjects.Remove("mill-bell");}return;}if(!near)return;
  millSeed=game.Save.Seed;millFloor=game.World.Height(MillX,MillZ);for(int a=-1;a<=1;a++)for(int b=-1;b<=1;b++)millFloor=Math.Max(millFloor,game.World.Height(MillX+a*24,MillZ+b*24));millFloor+=.25;
  millRoot=new Node3D{Position=new((float)MillX,(float)millFloor,(float)MillZ)};worldRoot.AddChild(millRoot);Box(millRoot,new(0,-.25f,0),new(50,.5f,50),new(.24f,.22f,.16f),true);for(int i=-6;i<=6;i++)Box(millRoot,new(i*3.6f,.01f,0),new(.04f,.02f,48),Wood*.65f);
  // Open-sided saw shed, real log-pile obstacles and a traversable southern entrance.
  for(int side=-1;side<=1;side+=2){Box(millRoot,new(side*24,1.2f,0),new(.3f,2.4f,46),Wood,true);for(int z=-18;z<=18;z+=12)Box(millRoot,new(side*24,2.4f,z),new(.35f,4.8f,.35f),Wood);}
  for(int side=-1;side<=1;side+=2){var pile=new Node3D{Position=new(side*12,0,-6)};millRoot.AddChild(pile);for(int row=0;row<3;row++)for(int i=0;i<4-row;i++)Tube(pile,new((i-(3-row)*.5f)*.65f,.35f+row*.60f,-3),new((i-(3-row)*.5f)*.65f,.35f+row*.60f,3),.34f,Wood);PhysicsObstacle(pile,new BoxShape3D{Size=new(2.5f,1.9f,6)},new(0,.95f,0));}
  Box(millRoot,new(0,5.4f,-19),new(24,.25f,10),new(.26f,.29f,.25f));for(int side=-1;side<=1;side+=2)Box(millRoot,new(side*10,2.7f,-19),new(.3f,5.4f,.3f),Wood,true);Bevel(millRoot,new(0,.8f,-19),new(8,.4f,2),Steel);for(int i=-3;i<=3;i++)Tube(millRoot,new(i,.9f,-20),new(i,.9f,-18),.12f,Steel);millRoot.AddChild(new Label3D{Text="ASHEN LUMBER / KEEP OUT",Position=new(0,4.5f,-14),FontSize=50,PixelSize=.02f,Modulate=new(.86f,.72f,.42f)});
  MillEntranceRamp();
  var bell=new Node3D{Position=new(0,0,21)};millRoot.AddChild(bell);for(int side=-1;side<=1;side+=2)Box(bell,new(side*.6f,1.4f,0),new(.12f,2.8f,.12f),Wood);Box(bell,new(0,2.8f,0),new(1.5f,.15f,.2f),Wood);Tube(bell,new(0,2,0),new(0,2.5f,0),.30f,new(.57f,.43f,.21f));props.Add(new WorldProp{Id="mill-bell",Kind=13,Node=bell,Position=millRoot.Position+bell.Position});uniqueObjects.Add("mill-bell");
 }
 void ShowMillNote(int index){if(index<0||index>=MillNotes.Length)return;var b=Panel(MillNoteTitles[index]);Text(b,MillNotes[index]);Text(b,$"Lumber mill / woodland / approximate map coordinates {(int)MillX}, {(int)MillZ}");Button(b,"BACK",CloseMenu);}
 void ReadMillNote(int index){var p=NearProp(12,3);if(p==null||p.Material!=index)return;if(!game.Change(()=>{if(!game.Profile.Journal.Seen.Contains(p.Id))game.Profile.Journal.Seen.Add(p.Id);game.AwardResearch(p.Id,3);}))return;if(CoopHost&&coopActor>0)RpcId(coopActor,nameof(CoopMillNote),index);else ShowMillNote(index);}
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]void CoopMillNote(int index){if(CoopClient)ShowMillNote(index);}
}
