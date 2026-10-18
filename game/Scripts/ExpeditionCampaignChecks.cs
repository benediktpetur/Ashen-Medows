using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class MigrationScene {
 async Task CampaignChecks(){
  var old=game;var oldEye=camera.Transform;Node3D art=null;
  try{
   if(dungeon!=null)ExitDungeon();ClearEnemies();
   game=new ExpeditionGame(ProjectSettings.GlobalizePath("user://tests/campaign-"+Guid.NewGuid()+".json")){NoDiskSave=true};
   Teleport(0,12);player.Position=new(0,200,0);camera.Position=new(0,StandingEye,0);
   AddEnemy("special/weeper/check",0,9,2);var weeper=infected.Last();weeper.Body.Position=new(6,200,0);weeper.Home=weeper.Body.Position;
   AddEnemy("listener/check",0,9,2);var listener=infected.Last();listener.Body.Position=new(16,200,0);
   await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
   double health=game.Life.Health;Noise(90);MigrationChecks.Assert(DormantWeeper(weeper)&&weeper.Memory==0&&game.Life.Health==health,"Weeper stays passive through nearby noise");
   listener.Memory=0;TickSpecialInfected(weeper,.1);MigrationChecks.Assert(listener.Memory>0&&listener.Target==weeper.Body.Position,"Crying attracts existing infected to its source");
   weeper.Hp-=1;ReactEnemyHit(weeper,false);MigrationChecks.Assert(!DormantWeeper(weeper)&&weeper.BossState=="weeper-windup"&&weeper.BossTime>=1,"Attacking weeper triggers warned claw rush");
   TickSpecialInfected(weeper,1.3);MigrationChecks.Assert(weeper.BossState=="weeper-charge","Weeper commits after warning");
   weeper.Body.Position=player.Position+Vector3.Right;weeper.BossHits.Clear();game.Shield=0;SpecialHit(weeper,2.1f,32,true);SpecialHit(weeper,2.1f,32,true);
   MigrationChecks.Assert(game.Life.Health==health-32&&game.Life.Wounds.Count==1,"Claw rush wounds once per target, not every frame");
   ClearEnemies();AddEnemy("special/bloater/check",0,9,2);var bloater=infected.Last();bloater.Body.Position=new(0,200,-4);KillEnemy(bloater);
   MigrationChecks.Assert(broodHatches.Count==1&&!infected.Any(e=>InfectedKind(e)=="worm"),"Bloater death warns before hatching");
   TickBrood(1.3);MigrationChecks.Assert(infected.Count(e=>InfectedKind(e)=="worm")==3,"Bloater hatches three killable worms");
   int food=game.Life.Food,ammo=game.Craft.Reserve;foreach(var worm in infected.ToArray())KillEnemy(worm);
   MigrationChecks.Assert(game.Life.Food==food&&game.Craft.Reserve==ammo,"Worms do not create ammunition/food farming loops");
   MigrationChecks.Assert(Enumerable.Range(0,300).All(i=>ExpeditionCampaign.Kind("habitat/check/"+i,1,0)=="walker"),"Starting valley has no specialist ambushes");
   var kinds=Enumerable.Range(0,1000).SelectMany(i=>Enumerable.Range(2,4).Select(t=>ExpeditionCampaign.Kind("habitat/check/"+i,t,0))).Distinct().ToArray();
   MigrationChecks.Assert(new[]{"runner","weeper","bloater","armored","bruiser","walker"}.All(k=>kinds.Contains(k)),"Regional population includes all specialist types");
   MigrationChecks.Assert(!IsCampaignBoss("boss/campaign/431/1/add/1/0")&&IsCampaignBoss(CampaignBossId(1)),"Boss minions do not become duplicate bosses");
   art=new Node3D();AddChild(art);foreach(var kind in kinds.Append("worm").Append("stalker")){var rig=new Node3D();art.AddChild(rig);SpecialInfectedArt(rig,"special/"+kind+"/rig",3,0);MigrationChecks.Assert(rig.GetChildCount()>0,"Special has actual geometry / "+kind);}
   foreach(int seed in new[]{1,431,777,981}){var map=new ExpeditionWorld(seed);for(int i=0;i<3;i++){var p=ExpeditionCampaign.Site(map,i);MigrationChecks.Assert(map.Playable(p.X,p.Z)&&!ExpeditionEncounter.InLot(map,p.X,p.Z,35)&&ExpeditionDungeon.Nearby(map,0,0).All(d=>ExpeditionGame.Distance(p.X,p.Z,d.X,d.Z)>=70),"Boss sites preserve stories and dungeon entrances across seeds");}}
   ClearEnemies();StartCampaignFight(0);MigrationChecks.Assert(!infected.Any(IsCampaignEnemy),"Boss sequence requires Foreman");game.World.Changes.MillDefeated=true;
   for(int i=0;i<2;i++){
    var site=ExpeditionCampaign.Site(game.World,i);Teleport(site.X,site.Z+15);SpawnCampaignSites();
    MigrationChecks.Assert(Math.Abs(game.World.Height(site.X,site.Z)-game.World.Height(site.X+18,site.Z-18))<.01,"Boss arena terrain is traversable");
    CampaignAction("site");var boss=infected.Single(e=>e.Id==CampaignBossId(i));
    boss.BossTime=0;TickCampaignBoss(boss,.1);MigrationChecks.Assert(boss.BossState.EndsWith("windup"),"Boss attack has wind-up");
    boss.BossState="recover";MigrationChecks.Assert(InfectedResistance(boss,false)==1.5,"Boss recovery rewards counterattacks");
    if(i==0){player.Position=boss.Home+Vector3.Right*90;TickCampaignBoss(boss,9);MigrationChecks.Assert(!infected.Contains(boss)&&!game.World.Changes.CampaignBosses.Contains(0),"Retreat resets boss without awarding a victory");Teleport(site.X,site.Z+15);CampaignAction("site");boss=infected.Single(e=>e.Id==CampaignBossId(i));}
    DefeatCampaignBoss(boss);int count=game.Profile.Items.Count;ClaimCampaignReward(game,i);
    MigrationChecks.Assert(game.World.Changes.CampaignBosses.Contains(i)&&game.Profile.Items.Count==count&&game.Craft.UnlockedTier>=i+3,"Boss reward persists once and supports next crafting tier");
   }
   var entry=ExpeditionCampaign.Site(game.World,2);Teleport(entry.X,entry.Z+15);SpawnCampaignSites();CampaignAction("site");
   MigrationChecks.Assert(InHeartRoom&&HeartWalkable(12,-25)&&!HeartWalkable(15,-25),"Final encounter has an entered physical dungeon");
   player.Position=new(0,.1f,-18.5f);CampaignAction("heart");var heart=infected.Single(e=>e.Id==CampaignBossId(2));
   heart.BossState="brood-windup";heart.BossTime=0;heart.BossStep=1;TickCampaignBoss(heart,.1);TickBrood(1.3);
   MigrationChecks.Assert(infected.Any(e=>InfectedKind(e)=="worm"),"Final boss summons real worms");
   DefeatCampaignBoss(heart);MigrationChecks.Assert(game.Life.LegendaryCores==1&&game.World.Changes.CampaignBosses.Count==3,"Final boss grants legendary heart and completion");
   ClaimCampaignReward(game,2);MigrationChecks.Assert(game.Life.LegendaryCores==1,"No duplicate legendary hearts");
   var recipe=ExpeditionRecipe.All.Single(r=>r.Kind=="legendary");game.Craft.Materials=Enumerable.Repeat(100,7).ToArray();game.Craft.BenchTier=5;game.Craft.Researched.Add(ExpeditionGame.ResearchId(recipe));
   game.Craft.Weapon=8;MigrationChecks.Assert(game.RecipeReason(recipe)!="Ready","Legendary upgrade requires maximum-tier weapon");game.Craft.Weapons.Add(19);game.Craft.Weapon=19;
   MigrationChecks.Assert(game.RecipeReason(recipe)=="Ready","Max-tier bow can become legendary");game.CraftRecipe(recipe);
   MigrationChecks.Assert(game.Life.LegendaryCores==0&&game.Life.WeaponQuality[19]==50&&game.ResearchComplete(recipe),"Legendary craft consumes heart and improves actual weapon");
   var world=Json.Read<ExpeditionWorldChanges>(Json.Write(game.World.Changes));world.Index();var life=Json.Read<ExpeditionLife>(Json.Write(game.Life));
   MigrationChecks.Assert(world.CampaignBosses.SequenceEqual(new[]{0,1,2})&&life.LegendaryCores==0&&life.WeaponQuality[19]==50,"Completion and legendary currency serialize");
   await SoulboundChecks();ShowCampaignGuide();CloseMenu();ExitDungeon();GD.Print("PASS / weeper passivity/cry/rush, once-only wound, bloater hatch/killable worms, safe starter, regional models, boss order/retry/telegraphs/rewards, final dungeon, legendary max-tier crafting and persistence");
  }finally{if(dungeon!=null)ExitDungeon();ClearEnemies();if(art!=null){RemoveChild(art);art.QueueFree();}game=old;camera.Transform=oldEye;Teleport(0,12);}
 }
 bool IsCampaignEnemy(Infected e)=>IsCampaignBoss(e.Id);
 async void RunCampaignChecks(){try{await CampaignChecks();GetTree().Quit();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
 async void CaptureCampaign(){try{
  ClearEnemies();game.NoDiskSave=true;game.World.Changes.Time=900;
  var stage=new Node3D{Position=player.Position+Vector3.Up*35};AddChild(stage);Box(stage,new(7,-.1f,0),new(24,.2f,12),stoneColor(),true);
  for(int i=0;i<5;i++){var rig=new Node3D{Position=new(i*3,0,0)};stage.AddChild(rig);HumanCharacter(rig,i+1,0,false,i);PoseFigure(rig,0,0,true);rig.AddChild(new Label3D{Text=new[]{"VALLEY DRIFTER","WOODLAND LOGGER","HIGHWAY CREW","CITY OFFICE","CITY SECURITY"}[i],Position=new(0,2.1f,0),FontSize=25,PixelSize=.007f,Billboard=BaseMaterial3D.BillboardModeEnum.Enabled});}
  camera.GlobalPosition=stage.Position+new Vector3(6,3,-11);camera.LookAt(stage.Position+new Vector3(6,1,0));weaponRoot.Visible=false;await ArtShot("campaign-01-regional-infected");
  ClearChildren(stage);Box(stage,new(3,-.1f,0),new(16,.2f,10),stoneColor(),true);
  foreach(var (kind,index) in new[]{("weeper",0),("bloater",1),("worm",2),("bruiser",3)}){var rig=new Node3D{Position=new(index*2.5f,0,0)};stage.AddChild(rig);SpecialInfectedArt(rig,"special/"+kind+"/preview",3,0);if(kind=="weeper")PoseWeeper(new(){Id="special/weeper/preview",Visual=rig,Tier=3,BossState="weeping"});rig.AddChild(new Label3D{Text=kind.ToUpperInvariant(),Position=new(0,2.4f,0),FontSize=27,PixelSize=.008f,Billboard=BaseMaterial3D.BillboardModeEnum.Enabled});}
  camera.GlobalPosition=stage.Position+new Vector3(3.8f,2.8f,-9);camera.LookAt(stage.Position+new Vector3(3.8f,1,0));await ArtShot("campaign-02-special-infected");var weeperRig=stage.GetChildren().OfType<Node3D>().First(n=>n.HasNode("Torso"));PoseWeeper(new(){Id="special/weeper/portrait",Tier=3,Visual=weeperRig,BossState="weeper-windup"});camera.GlobalPosition=stage.Position+new Vector3(.8f,1.6f,-3.2f);camera.LookAt(stage.Position+new Vector3(0,1.1f,0));await ArtShot("campaign-07-weeper-close");stage.QueueFree();
  game.World.Changes.MillDefeated=true;camera.Position=new(0,StandingEye,0);weaponRoot.Visible=true;
  for(int i=0;i<2;i++){var p=ExpeditionCampaign.Site(game.World,i);Teleport(p.X,p.Z+15);SpawnCampaignSites();StartCampaignFight(i);var boss=infected.Single(e=>IsCampaignBoss(e.Id));boss.BossState=i==0?"ram-windup":"alarm-windup";boss.BossAim=player.Position;boss.BossTime=2;boss.Body.LookAt(new Vector3(player.Position.X,boss.Body.Position.Y,player.Position.Z));camera.LookAt(boss.Body.Position+Vector3.Up*1.7f);DrawCampaign();await ArtShot("campaign-0"+(i+3)+"-"+(i==0?"wrecker":"bellkeeper"));DefeatCampaignBoss(boss);}
  var entry=ExpeditionCampaign.Site(game.World,2);Teleport(entry.X,entry.Z);EnterDungeon(HeartDungeon());player.Position=new(0,.1f,-18.5f);StartCampaignFight(2);var heart=infected.Single(e=>IsCampaignBoss(e.Id));heart.BossState="rupture-windup";heart.BossTime=2.5;player.Position=new(0,.1f,1);camera.Position=new(0,StandingEye,0);heart.Body.LookAt(new Vector3(player.Position.X,heart.Body.Position.Y,player.Position.Z));camera.LookAt(heart.Body.Position+Vector3.Up*1.8f);DrawCampaign();await ArtShot("campaign-05-broodheart");ShowCampaignGuide();await ArtShot("campaign-06-journey");GetTree().Quit();
 }catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}
