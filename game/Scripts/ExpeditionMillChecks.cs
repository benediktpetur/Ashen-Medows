using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 async System.Threading.Tasks.Task MillChecks(){
  ClearEnemies();Teleport(MillX,MillZ+21);SpawnObjects();player.Position=new((float)MillX,(float)millFloor+.1f,(float)MillZ+21);StartMillFight();var boss=infected.Single(e=>IsMillBoss(e.Id));
  MigrationChecks.Assert(boss.MaxHp==1400,"Foreman solo scaling");
  boss.BossTime=0;TickMillBoss(boss,.01);MigrationChecks.Assert(boss.BossState=="slam","Foreman slam telegraph");
  player.Position=boss.Body.Position+Vector3.Right*3;game.Shield=0;double hp=game.Life.Health;boss.BossTime=0;TickMillBoss(boss,.01);MigrationChecks.Assert(game.Life.Health<hp&&game.Life.Wounds.Count==0,"Foreman slam damage without bite");
  boss.BossState="retch";boss.BossTime=0;boss.BossAim=boss.Body.Position+Vector3.Back*12;TickMillBoss(boss,.01);MigrationChecks.Assert(millPools.Count==3,"Foreman vomit pools");TickMillPools(10);MigrationChecks.Assert(millPools.Count==0,"Foreman pools expire");
  MigrationChecks.Assert(MillDamageMultiplier(boss,boss.Body.Position+Vector3.Up*2)==1.5,"Foreman recovery opening");boss.BossState="stunned";MigrationChecks.Assert(MillDamageMultiplier(boss,boss.Body.Position)==2,"Foreman charge stun opening");
  player.Position=new((float)MillX+60,(float)millFloor,(float)MillZ);TickMillBoss(boss,9);MigrationChecks.Assert(!infected.Contains(boss),"Foreman retreat resets encounter");
  player.Position=new((float)MillX,(float)millFloor+.1f,(float)MillZ+21);StartMillFight();boss=infected.Single(e=>IsMillBoss(e.Id));DefeatMillBoss(boss);MigrationChecks.Assert(game.World.Changes.MillDefeated&&game.Life.MillCores==1,"Foreman victory and core");StartMillFight();MigrationChecks.Assert(!infected.Any(e=>IsMillBoss(e.Id))&&game.Life.MillCores==1,"Foreman duplicate reward protection");
  var recipe=ExpeditionRecipe.All.First(r=>r.Kind=="mill");game.Craft.Researched.Add(ExpeditionGame.ResearchId(recipe));game.Craft.BenchTier=game.Craft.UnlockedTier=2;for(int i=0;i<7;i++)game.Craft.Materials[i]=100;game.CraftRecipe(recipe);MigrationChecks.Assert(game.Life.MillCores==0&&game.Life.WeaponQuality[game.Craft.Weapon]>=20,"Foreman core weapon crafting");
  var saved=new ExpeditionGame(game.PathName);MigrationChecks.Assert(saved.World.Changes.MillDefeated,"Foreman victory saved");game.World.Changes.MillDefeated=false;game.Life.Health=game.MaxHealth;Teleport(0,12);await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);GD.Print("PASS / Foreman encounter, telegraphs, hazards, retreat, victory, reward and tempering");
 }
 async void CaptureMill(){try{
  CloseMenu();ClearEnemies();game.World.Changes.Time=1050;Teleport(MillX,MillZ+21);SpawnObjects();player.Position=new((float)MillX,(float)millFloor+.1f,(float)MillZ+21);StartMillFight();var boss=infected.Single(e=>IsMillBoss(e.Id));boss.Body.Rotation=new(0,Mathf.Pi,0);camera.Fov=65;camera.GlobalPosition=new((float)MillX+5,(float)millFloor+2,(float)MillZ+14);camera.LookAt(boss.Body.Position+Vector3.Up*2);await ArtShot("foreman-01-mill-encounter");
  boss.BossState="slam";boss.BossStep=1;await ArtShot("foreman-02-ground-slam");boss.BossState="retch";boss.BossAim=player.Position;boss.BossStep=2;CreateMillVomit(boss);await ArtShot("foreman-03-corrosive-vomit");ShowMillNote(2);await ArtShot("foreman-04-survivor-notes");GD.Print("PASS / Foreman GPU captures");GetTree().Quit();
 }catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}
