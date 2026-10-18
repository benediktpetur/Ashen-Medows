using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
public partial class MigrationScene {
 bool livingCoopAck;
 [Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]void CoopLivingCheckAck(){if(coopCheck&&CoopHost&&coopActors.ContainsKey(Multiplayer.GetRemoteSenderId()))livingCoopAck=true;}
 async Task LivingCoopChecks(CoopActor actor){
  livingFixture=true;var town=Town(0);Teleport(town.X,town.Z);ClearEnemies();actor.Body.Position=Ground(town.X+1,town.Z)+Vector3.Up*.1f;
  actor.Game.Life.Food=6;actor.Game.Craft.Water=4;actor.Game.Life.RawWater=2;actor.Game.Life.Sickness=100;actor.Game.Life.Medicine=1;actor.Game.Craft.Materials[0]=10;actor.Game.Craft.Materials[4]=8;actor.Game.Craft.Materials[5]=2;actor.Game.Craft.ResearchPoints=20;actor.Game.Craft.ResearchFocus.Clear();actor.Game.Craft.BenchTier=actor.Game.Craft.UnlockedTier=5;actor.Game.Skills.Crafting.Level=5;actor.Game.Career.Level=Math.Max(2,actor.Game.Career.Level);
  int hostFood=game.Life.Food,hostWood=game.Craft.Materials[0];SendCoopPersonal(actor.Id,CoopProfileJson(actor.Game),"",actor.Body.Position,true);FeverSync();RpcId(actor.Id,nameof(CoopCheckStage),"living-community",actor.Body.Position);
  await CoopWait(()=>livingCoopAck,"Guest living-world actions did not finish",25);
  MigrationChecks.Assert(town.Trust==16&&town.WaterFixed&&actor.Game.Life.Food==3&&actor.Game.Craft.Water==6&&actor.Game.Life.RawWater==0&&actor.Game.Life.Sickness==0&&actor.Game.Life.Medicine==0,"Community/water/illness authority or conservation failed");
  MigrationChecks.Assert(game.Life.Food==hostFood&&game.Craft.Materials[0]==hostWood&&game.Craft.ResearchFocus.Count==0&&actor.Game.Craft.ResearchFocus["Weapons and armor"]=="Ranger","Living systems affected host inventory or specialization");
  await RescueDogCoopChecks(actor);actor.Game.Craft.BenchTier=1;actor.Game.Craft.UnlockedTier=2;CloseMenu();ClearEnemies();livingFixture=false;GD.Print("PASS / two-process living world: shared trust/filter repair, personal supply conservation, raw-to-clean water, illness treatment and permanent research branch isolation");
 }
 async Task ClientLivingChecks(Vector3 target){
  livingFixture=true;await CoopWait(()=>Distance(player.Position,target)<1&&game.Craft.BenchTier==5,"Living test profile/teleport missing");SpawnLivingWorld();CoopRequest("living","0|visit");await CoopWait(()=>paused,"Community dialogue missing");CloseMenu();
  CoopRequest("living","0|food");await CoopWait(()=>Town(0).Trust==4&&game.Life.Food==3,"Community donation not authoritative");CloseMenu();CoopRequest("living","0|repair");await CoopWait(()=>Town(0).WaterFixed&&game.Craft.Materials[4]==0,"Shared permanent filter repair missing");CloseMenu();
  CoopRequest("purify","boil");await CoopWait(()=>game.Craft.Water==6&&game.Life.RawWater==0,"Guest purification conservation failed");CoopRequest("medicine");await CoopWait(()=>game.Life.Sickness==0&&game.Life.Medicine==0,"Guest illness treatment missing");
  int expectedPoints=game.Craft.ResearchPoints;foreach(int foundation in new[]{12,6}){var recipe=ExpeditionRecipe.All.First(r=>r.Kind=="weapon"&&r.Result==foundation);if(!game.Knows(recipe)){expectedPoints-=game.ResearchCost(recipe);CoopRequest("research",Array.IndexOf(ExpeditionRecipe.All,recipe).ToString());await CoopWait(()=>game.Knows(recipe),"Archery foundation missing");}}expectedPoints-=3;
  int bow=Array.FindIndex(ExpeditionRecipe.All,r=>r.Kind=="weapon"&&r.Result==18),gun=Array.FindIndex(ExpeditionRecipe.All,r=>r.Kind=="weapon"&&r.Result==3);CoopRequest("research",bow.ToString());await CoopWait(()=>game.Craft.ResearchFocus.TryGetValue("Weapons and armor",out var focus)&&focus=="Ranger","Guest specialization missing");CoopRequest("research",gun.ToString());await CoopWait(()=>game.Notice.Contains("SPECIALIZATION LOCK"),"Incompatible research was not rejected");
  if(game.Knows(ExpeditionRecipe.All[gun])||game.Craft.ResearchPoints!=expectedPoints)throw new Exception("Guest specialization bypass or extra point spend");livingFixture=false;RpcId(1,nameof(CoopLivingCheckAck));
 }
}
