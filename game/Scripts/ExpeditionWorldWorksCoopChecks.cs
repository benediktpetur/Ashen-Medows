using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
public partial class MigrationScene {
 int workCoopAck;
 [Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]void CoopWorkAck(int step){if(coopCheck&&CoopHost&&coopActors.ContainsKey(Multiplayer.GetRemoteSenderId()))workCoopAck=Math.Max(workCoopAck,step);}
 async Task WorldWorksCoopChecks(CoopActor actor){bool old=contentFixture;try{contentFixture=true;workCoopAck=0;ClearEnemies();var at=ExpeditionContentSite.Place(game.World,0);Teleport(at.X,at.Z+5.5);SpawnContentAtlas();var point=props.First(p=>p.Kind==36&&p.Material==0).Position;actor.Body.Position=point;actor.Game.Save.X=point.X;actor.Game.Save.Z=point.Z;player.Position=point+Vector3.Right*2;
  actor.Game.Craft.Materials[0]=actor.Game.Craft.Materials[1]=10;actor.Game.Craft.UnlockedTier=Math.Max(1,actor.Game.Craft.UnlockedTier);int hostWood=game.Craft.Materials[0],hostFood=game.Life.Food,guestFood=actor.Game.Life.Food;
  SendCoopPersonal(actor.Id,CoopProfileJson(actor.Game),"",actor.Body.Position,true);FeverSync();RpcId(actor.Id,nameof(CoopCheckStage),"world-works-start",point);await CoopWait(()=>workCoopAck>=1,"Guest project commitment missing",20);
  var state=game.World.Changes.Work(0);MigrationChecks.Assert(state.Choice==2&&actor.Game.Craft.Materials[0]==8&&actor.Game.Craft.Materials[1]==8&&game.Craft.Materials[0]==hostWood,"Shared choice must spend guest's resources only");
  // The standalone checks cover real active-time work; finish this fixture to keep the full network suite bounded.
  game.FinishWork(state);FeverSync();SendCoopPersonal(actor.Id,CoopProfileJson(actor.Game),"",actor.Body.Position,false);RpcId(actor.Id,nameof(CoopCheckStage),"world-works-collect",point);await CoopWait(()=>workCoopAck>=2,"Shared project collection missing",20);
  MigrationChecks.Assert(state.Stock[7]==0&&actor.Game.Life.Food==guestFood+6&&game.Life.Food==hostFood,"Project collection must pay one acting guest without duplicating shared supplies");
  GD.Print("PASS / two-process world works: actual guest commitment, private cost, shared irreversible result/art, conservation and duplicate rejection");
 }finally{contentFixture=old;}}
 async Task ClientWorldWorksChecks(string stage,Vector3 point){contentFixture=true;if(stage=="world-works-start"){
  await CoopWait(()=>Distance(player.Position,point)<1&&game.Craft.Materials[0]==10,"Project fixture missing");CoopRequest("works","strip:0");await CoopWait(()=>game.World.Changes.Work(0).Choice==2&&game.Craft.Materials[0]==8,"World choice or guest cost did not replicate");RpcId(1,nameof(CoopWorkAck),1);
 }else{await CoopWait(()=>game.World.Changes.Work(0).Finished,"Completed project snapshot missing");SpawnContentAtlas();DrawWorldWorks();MigrationChecks.Assert(contentRoots[0].GetNode<Node3D>("WorldWork").GetMeta("stage").AsString()=="2/True","Guest sees dismantled world art");int food=game.Life.Food;CoopRequest("works","collect:0");await CoopWait(()=>game.Life.Food==food+6&&game.World.Changes.Work(0).Stock[7]==0,"Guest shared supply transfer failed");CoopRequest("works","collect:0");await CoopWait(()=>game.Notice=="Nothing stored here yet","Duplicate project claim not rejected");contentFixture=false;RpcId(1,nameof(CoopWorkAck),2);}}
}
