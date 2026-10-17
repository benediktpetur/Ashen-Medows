using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
public partial class MigrationScene {
 string coopAimTarget;bool coopAimReady;
 [Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
 void CoopCheckFixtureAim(string id,float height){
  if(!coopCheck||!CoopHost||!float.IsFinite(height)||height<0||height>4||!coopActors.TryGetValue(Multiplayer.GetRemoteSenderId(),out var actor))return;
  var enemy=infected.FirstOrDefault(e=>e.Id==id);if(enemy==null)return;
  var direction=(enemy.Body.Position+Vector3.Up*height-(actor.Body.Position+Vector3.Up*(actor.Game.Sneaking?CrouchingEye:StandingEye))).Normalized();
  var forward=-Basis.FromEuler(new Vector3(actor.Pitch,actor.Yaw,0)).Z;
  if(forward.Dot(direction)>.9999f)RpcId(actor.Id,nameof(CoopCheckFixtureAimReady),id);
 }
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
 void CoopCheckFixtureAimReady(string id){if(coopCheck&&CoopClient&&coopAimTarget==id)coopAimReady=true;}
 async Task AimCoopFixture(Infected enemy,float height,bool ads=false){
  aiming=ads;coopAimTarget=enemy.Id;coopAimReady=false;double nextProbe=0;
  await CoopWait(()=>{
   camera.LookAt(enemy.Body.Position+Vector3.Up*height);pitch=camera.GlobalRotation.X;player.Rotation=new(0,camera.GlobalRotation.Y,0);camera.Rotation=new(pitch,0,0);
   double now=Time.GetTicksMsec()/1000.0;if(now>=nextProbe){nextProbe=now+.1;RpcId(1,nameof(CoopCheckFixtureAim),enemy.Id,height);}return coopAimReady;
  },"Host did not receive firing aim for "+enemy.Id);
 }
}
