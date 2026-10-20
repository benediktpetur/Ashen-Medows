using Godot;
using System;
using System.Linq;

public partial class MigrationScene {
 bool InCampaignLot(double x,double z)=>ExpeditionCampaign.InLot(game.World,x,z);
 string CampaignBossId(int index)=>$"boss/campaign/{game.Save.Seed}/{index}";
 ExpeditionDungeon HeartDungeon(){var p=ExpeditionCampaign.Site(game.World,2);return new(){Id="campaign-heart/"+game.Save.Seed,X=p.X,Z=p.Z,Tier=5,Deep=true,Name="THE PUMPING HEART"};}
 bool InHeartRoom=>dungeon!=null&&dungeon.Id=="campaign-heart/"+game.Save.Seed;
 static bool HeartWalkable(double x,double z)=>x> -13.6&&x<13.6&&z> -27.6&&z<5.6;
 void SpawnCampaignSites(){
  if(dungeon!=null)return;
  var starter=Ground(ExpeditionWorld.SpawnX+5,ExpeditionWorld.SpawnZ-4);
  if(NearActiveSurface(starter,190)&&uniqueObjects.Add("campaign/field-guide")){
   var sign=new Node3D{Position=starter};worldRoot.AddChild(sign);Box(sign,new(0,.6f,0),new(1.1f,1.2f,.5f),Wood,true);StoryPaper(sign,new(0,1.23f,0));
   sign.AddChild(new Label3D{Text="SURVIVOR FIELD GUIDE",Position=new(0,1.65f,0),FontSize=24,PixelSize=.006f,Billboard=BaseMaterial3D.BillboardModeEnum.Enabled});
   props.Add(new(){Id="campaign/field-guide",Kind=30,Node=sign,Position=starter});
  }
  for(int i=0;i<3;i++){
   var p=ExpeditionCampaign.Site(game.World,i);var ground=Ground(p.X,p.Z);string id="campaign/site/"+i;
   if(!NearActiveSurface(ground,210)||!uniqueObjects.Add(id))continue;
   var root=new Node3D{Position=ground};worldRoot.AddChild(root);
   Box(root,new(0,-.1f,0),new(44,.2f,44),i==0?new(.23f,.24f,.21f):new(.24f,.27f,.26f),true);
   for(int side=-1;side<=1;side+=2)for(int z=-16;z<=16;z+=8){Box(root,new(side*21,1.1f,z),new(.25f,2.2f,5),Steel,true);}
   if(i==0){
    for(int side=-1;side<=1;side+=2){var car=new Node3D{Position=new(side*11,0,-3),Rotation=new(0,side*.4f,0)};root.AddChild(car);ArtWreck(car,true);PhysicsObstacle(car,new BoxShape3D{Size=new(2.2f,1.5f,4.2f)},new(0,.75f,0));}
    Box(root,new(0,4,-17),new(12,.3f,5),Rust);for(int side=-1;side<=1;side+=2)Box(root,new(side*5,2,-17),new(.3f,4,.3f),Steel,true);
   }else if(i==1){
    for(int side=-1;side<=1;side+=2){Box(root,new(side*13,2,-10),new(2,4,2),stoneColor(),true);Tube(root,new(side*13,4,-10),new(side*13,6,-10),.15f,Steel);}
    Box(root,new(0,5,-17),new(8,.3f,3),Steel);for(int side=-1;side<=1;side+=2)Box(root,new(side*3,2.5f,-17),new(.3f,5,.3f),Steel,true);
   }else{
    Box(root,new(0,.3f,0),new(8,.6f,8),Steel,true);Box(root,new(0,.65f,0),new(3,.12f,3),Ink);for(int side=-1;side<=1;side+=2)Tube(root,new(side*2,.8f,-2),new(side*2,.8f,2),.08f,Steel);
   }
   var console=new Node3D{Position=new(0,0,15)};root.AddChild(console);Bevel(console,new(0,.75f,0),new(1,1.5f,.65f),Steel);Box(console,new(0,1.35f,-.34f),new(.75f,.24f,.03f),new(.52f,.24f,.12f));
   root.AddChild(new Label3D{Text=i==0?"ROAD RECOVERY / THE WRECKER":i==1?"CIVIC SQUARE / THE BELLKEEPER":"PUMP STATION / RESTRICTED DEPTHS",Position=new(0,3,15),FontSize=28,PixelSize=.009f,Billboard=BaseMaterial3D.BillboardModeEnum.Enabled});
   props.Add(new(){Id=id,Kind=29,Material=i,Node=root,Position=ground+new Vector3(0,0,15)});
  }
 }
 Color stoneColor()=>new(.32f,.34f,.31f);
 string CampaignGate(int index)=>index==0?!game.World.Changes.MillDefeated?"Defeat The Foreman at the lumber mill first":"Ready":!game.World.Changes.CampaignBosses.Contains(index-1)?"Defeat "+ExpeditionCampaign.Names[index-1]+" first":"Ready";
 string CampaignPrompt(){
  if(InHeartRoom){if(Distance(player.Position,new(0,0,4))<2)return "F / RETURN TO PUMP STATION";if(Distance(player.Position,new(0,0,-19))<3)return game.World.Changes.CampaignBosses.Contains(2)?"F / SECURE THE SOUTH BANK":"F / ACTIVATE PUMP / THE BROODHEART";return "";}
  if(dungeon!=null)return "";
  if(NearProp(30,3)!=null)return "F / SURVIVOR FIELD GUIDE / YOUR JOURNEY";
  var site=NearProp(29,4);if(site==null)return "";
  return game.World.Changes.CampaignBosses.Contains(site.Material)?"F / CLEARED SITE / YOUR JOURNEY":site.Material==2?"F / DESCEND TO THE PUMPING HEART":"F / CHALLENGE "+ExpeditionCampaign.Names[site.Material].ToUpperInvariant();
 }
 bool CampaignInteract(){
  string prompt=CampaignPrompt();if(prompt=="")return InHeartRoom;
  string action=InHeartRoom?Distance(player.Position,new(0,0,4))<2?"exit":game.World.Changes.CampaignBosses.Contains(2)?"claim":"heart":NearProp(30,3)!=null?"guide":"site";
  if(!CoopRequest("campaign",action))CampaignAction(action);return true;
 }
 void CampaignAction(string action){
  if(CoopClient)return;
  if(action.StartsWith("soul:")||action.StartsWith("mega:")){SoulAction(action);return;}
  if(InHeartRoom){
   if(action=="exit"&&Distance(player.Position,new(0,0,4))<2){if(coopActor>0){game.Notice="The host controls party travel.";return;}ExitDungeon();return;}
   if(Distance(player.Position,new(0,0,-19))>=3)return;
   if(action=="heart"){StartCampaignFight(2);return;}
   if(action=="claim"&&game.World.Changes.CampaignBosses.Contains(2)){ClaimCampaignReward(game,2);OpenCampaignGuide();}return;
  }
  if(dungeon!=null)return;
  if(action=="guide"&&NearProp(30,3)!=null){OpenCampaignGuide();return;}
  var site=NearProp(29,4);if(action!="site"||site==null)return;int index=site.Material;
  if(game.World.Changes.CampaignBosses.Contains(index)){ClaimCampaignReward(game,index);if(index==2){if(coopActor>0){game.Notice="The host controls party travel.";return;}EnterDungeon(HeartDungeon());}else OpenCampaignGuide();return;}
  if(CampaignGate(index)!="Ready"){game.Notice=CampaignGate(index);OpenCampaignGuide();return;}
  if(index==2){if(coopActor>0){game.Notice="Ask the host to enter the pumping station together.";return;}EnterDungeon(HeartDungeon());}else StartCampaignFight(index);
 }
 void OpenCampaignGuide(){if(CoopHost&&coopActor>0)RpcId(coopActor,nameof(CoopCampaignGuide));else ShowCampaignGuide();}
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]void CoopCampaignGuide(){if(CoopClient)ShowCampaignGuide();}
 void ShowCampaignGuide(){
  var box=Panel("THE SOUTH BANK / YOUR JOURNEY");var scroll=new ScrollContainer{CustomMinimumSize=new(900,450)};box.AddChild(scroll);var content=new VBoxContainer();scroll.AddChild(content);
  Text(content,"1 / MAKE TOMORROW POSSIBLE\nGather wood, stone and fibre. K crafts an axe, club, spear, bow and bandages by hand. Build a workbench and a small shelter. Practise combat in the quiet southeast. Avoid bites: bandages stop bleeding, not damage.");
  Text(content,"2 / THE WOODLAND\nGather resin to unlock workshop 2, guns and a sleeping bag. Place and bind a safe respawn before expeditions. Rescue a companion if you want help. Read the mill warnings and ring the yard bell when prepared.\nTHE FOREMAN / "+(game.World.Changes.MillDefeated?"DEFEATED":"Lumber mill, "+(int)MillX+", -780"));
  LocationButton(content,"Lumber mill / The Foreman",MillX,-780,ShowCampaignGuide);
  for(int i=0;i<3;i++){var p=ExpeditionCampaign.Site(game.World,i);Text(content,$"{i+3} / {ExpeditionCampaign.Names[i].ToUpperInvariant()} / "+(game.World.Changes.CampaignBosses.Contains(i)?"CLEARED":CampaignGate(i))+ $"\n{(int)p.X}, {(int)p.Z} / "+(i==0?"T3 highway recovery yard. Dodge the ram; punish recovery. Salvage alloy and upgrade your workshop.":i==1?"T4 civic square. The bell calls reinforcements. Read the orange target marker, move, then counterattack. Collect electronics.":"T5 pump station below the city. Clear the worms, avoid rupture rings and bile pools. The heart is your route to legendary equipment."));LocationButton(content,ExpeditionCampaign.Names[i],p.X,p.Z,ShowCampaignGuide);}
  Text(content,"AFTER THE HEART\nA legendary heart upgrades a maximum-tier weapon at workshop 5 (K / Weapons and armor). The heart also drops a Soulbound gun. Return to this pump station for repeatable Tier 5 mega hunts: open the Soulbound armory at the chamber console. Each hunt costs 8 night catalysts. The river remains the current world's edge.");
  Text(content,"FIELD WARNINGS\nSobbing: a seated weeper is avoidable. Attacking it unleashes a tanky clawed rush. Sidestep its committed charge, then strike during recovery.\nBloated infected: leave room after a kill. Worms hatch after a short green warning.\nHighway runners are fast; city armor resists body shots. Aim for the head or use recovery windows. Night catalysts improve weapon tuning.");
  Button(box,"INFECTED / TEN NEW FIELD THREATS",ShowThreatGuide);
  Button(box,"SOULBOUND ARMORY / T5 MEGA HUNTS",ShowSoulArmory);
  Button(box,"BACK TO EXPEDITION",CloseMenu);
 }
 void BuildHeartArena(){
  if(dungeonRoot!=null){RemoveChild(dungeonRoot);dungeonRoot.QueueFree();}dungeonRoot=new(){Name="PumpingHeartArena"};AddChild(dungeonRoot);
  Box(dungeonRoot,new(0,-.2f,-11),new(28,.4f,34),stoneColor(),true);Box(dungeonRoot,new(0,4.9f,-11),new(28,.4f,34),Steel,true);
  for(int side=-1;side<=1;side+=2){Box(dungeonRoot,new(side*14,2.4f,-11),new(.4f,4.8f,34),Steel,true);for(int z=-24;z<=0;z+=8){Tube(dungeonRoot,new(side*12,3,z),new(side*12,3,z+6),.18f,Rust);}}
  foreach(float z in new[]{-28f,6f})Box(dungeonRoot,new(0,2.4f,z),new(28,4.8f,.4f),Steel,true);
  for(int side=-1;side<=1;side+=2){Box(dungeonRoot,new(side*8,1.2f,-12),new(2.5f,2.4f,3),Steel,true);}
  var altar=new Node3D{Position=new(0,0,-20)};dungeonRoot.AddChild(altar);Bevel(altar,new(0,.7f,0),new(2,1.4f,.8f),Steel);Box(altar,new(0,1.3f,-.42f),new(1.6f,.3f,.04f),new(.29f,.65f,.40f));
  dungeonRoot.AddChild(new Label3D{Text="RESTORE PUMP PRESSURE / F",Position=new(0,2.7f,-20),FontSize=32,PixelSize=.01f});
  for(int side=-1;side<=1;side+=2)dungeonRoot.AddChild(new OmniLight3D{Position=new(side*9,3,-12),LightColor=new(.42f,.58f,.43f),LightEnergy=2,OmniRange=22});
  Box(dungeonRoot,new(0,1.3f,5.6f),new(3,2.6f,.1f),new(.18f,.5f,.36f));player.Position=new(0,.1f,3);player.Velocity=Vector3.Zero;lamp.Visible=true;
  if(CoopHost)CoopRoomChanged();
 }
}
