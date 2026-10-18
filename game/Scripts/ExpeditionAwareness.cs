using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
public partial class MigrationScene {
 sealed class StoneFlight{public string Room;public int Id;public Vector3 Position,Velocity;public Node3D Visual;public double Remaining=4;}
 readonly List<StoneFlight> distractions=new();int distractionSerial;
 sealed class TossClock{public double Ready;}
 readonly ConditionalWeakTable<ExpeditionGame,TossClock> tossClocks=new();
 Vector3 SearchLastPosition(Infected e,bool sees,double dt){if(!JourneyEnabled)return e.Target;if(sees||e.Memory<=0){e.Searching=false;e.SearchTime=0;return e.Target;}
  if(!e.Searching&&Distance(e.Body.Position,e.Target)<2.2f){e.Searching=true;e.SearchOrigin=e.Target;e.SearchTime=0;}
  if(!e.Searching)return e.Target;e.SearchTime+=dt;int step=(int)(e.SearchTime/1.5);float angle=step*2.4f+(e.Id.GetHashCodeStable()%100)*.1f;var point=e.SearchOrigin+new Vector3(Mathf.Sin(angle)*3,0,Mathf.Cos(angle)*3);return dungeon==null?Ground(point.X,point.Z):point;
 }
 void ThrowDistraction(){if(!JourneyEnabled)return;if(CoopRequest("distract"))return;var clock=tossClocks.GetOrCreateValue(game);if(game.Clock<clock.Ready||shotClock>0||reloadClock>0){game.Notice="Finish your current action before throwing another stone.";return;}if(game.Craft.Materials[1]<1||game.Stamina<3){game.Notice="Throwing a distraction needs one stone and three stamina.";return;}if(!game.Change(()=>{game.Craft.Materials[1]--;game.Stamina-=3;}))return;clock.Ready=game.Clock+3;int id=++distractionSerial;Vector3 start=camera.GlobalPosition-camera.GlobalBasis.Z*.5f,velocity=-camera.GlobalBasis.Z*13+Vector3.Up*3;SpawnDistraction(id,start,velocity);if(CoopHost&&coopActors.Count>0)Rpc(nameof(CoopDistraction),id,start,velocity);game.Notice="Stone thrown / infected investigate where it lands.";}
 void SpawnDistraction(int id,Vector3 start,Vector3 velocity){if(distractions.Count>=8){var old=distractions[0];old.Visual?.QueueFree();distractions.RemoveAt(0);}Node3D model=null;if(DisplayServer.GetName()!="headless"){model=new Node3D{Position=start};AddChild(model);Ellipsoid(model,Vector3.Zero,new(.055f,.04f,.06f),new(.43f,.44f,.39f));}distractions.Add(new(){Id=id,Position=start,Velocity=velocity,Visual=model,Room=dungeon?.Id??""});}
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]void CoopDistraction(int id,Vector3 position,Vector3 velocity){if(CoopClient)SpawnDistraction(id,position,velocity);}
 void TickDistractions(double dt){if(!JourneyEnabled)return;for(int i=distractions.Count-1;i>=0;i--){var stone=distractions[i];if(stone.Room!=(dungeon?.Id??"")){stone.Visual?.QueueFree();distractions.RemoveAt(i);continue;}stone.Remaining-=dt;Vector3 next=stone.Position+stone.Velocity*(float)dt;stone.Velocity+=Vector3.Down*(float)(9.8*dt);var ray=PhysicsRayQueryParameters3D.Create(stone.Position,next);ray.Exclude=PursuitExclusions();var hit=GetWorld3D().DirectSpaceState.IntersectRay(ray);if(hit.Count>0||stone.Remaining<=0){var at=hit.Count>0?(Vector3)hit["position"]:next;if(!CoopClient){HearNoise(at,24);EnemySound(at,2);}stone.Visual?.QueueFree();distractions.RemoveAt(i);}else{stone.Position=next;if(stone.Visual!=null)stone.Visual.Position=next;}}}
}
