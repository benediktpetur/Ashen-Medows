using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
public partial class MigrationScene {
 bool axeEquipped,axePending;double hurtShake;double axeSwingStarted=-10,hitUntil;Label hitMarker;double feedbackHealth=-1;
 readonly List<(Node3D Node,Vector3 Velocity,double Until)> impactBits=new();
 readonly List<(Node3D Node,double Until)> remains=new();
 void BuildHeldTool(){weaponRoot.Rotation=Vector3.Zero;if(!axeEquipped){ArtGun();return;}ClearChildren(weaponRoot);ItemTool(weaponRoot,0);weaponRoot.Scale=Vector3.One;}
 void BeginAxeSwing(){if(axePending||game.Clock<nextChop)return;AxeSwing();axePending=true;}
 void AxeSwing(){if(coopExecuting)return;axeSwingStarted=Time.GetTicksMsec()/1000.0;}
 WorldProp AimedTree(){
  var q=PhysicsRayQueryParameters3D.Create(camera.GlobalPosition,camera.GlobalPosition-camera.GlobalBasis.Z*3.5f);q.Exclude=new Godot.Collections.Array<Rid>{player.GetRid()};var hit=GetWorld3D().DirectSpaceState.IntersectRay(q);if(hit.Count==0)return null;
  var collider=hit["collider"].AsGodotObject() as Node;return props.FirstOrDefault(p=>p.Kind==0&&collider!=null&&(p.Node==collider||p.Node.IsAncestorOf(collider)));
 }
 void WoodImpact(WorldProp p){ImpactParticles(p.Position+Vector3.Up*1.3f,new Color(.62f,.43f,.22f),10,.06f);game.Notice="AXE IMPACT / "+(p.Hits+1)+" strikes";}
 void FallingTree(WorldProp p){if(DisplayServer.GetName()=="headless"){p.Node.GetParent().RemoveChild(p.Node);p.Node.QueueFree();return;}p.Node.Reparent(this,true);foreach(var shape in p.Node.FindChildren("*","CollisionShape3D",true,false))if(shape is CollisionShape3D collision)collision.SetDeferred(CollisionShape3D.PropertyName.Disabled,true);var tween=CreateTween();tween.TweenProperty(p.Node,"rotation_degrees:z",85f,1.15).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);tween.TweenInterval(2);tween.TweenCallback(Callable.From(()=>{if(GodotObject.IsInstanceValid(p.Node))p.Node.QueueFree();}));}
 void BloodImpact(Vector3 point,Vector3 direction,int count){ImpactParticles(point,new Color(.38f,.025f,.025f),count,.045f);}
 void ImpactParticles(Vector3 point,Color color,int count,float size){
  if(DisplayServer.GetName()=="headless")return;var random=new Random();for(int i=0;i<count;i++){while(impactBits.Count>=160){impactBits[0].Node.QueueFree();impactBits.RemoveAt(0);}var node=new Node3D{Position=point};AddChild(node);if(color.R>.5f)Box(node,Vector3.Zero,Vector3.One*size,color);else Ellipsoid(node,Vector3.Zero,new Vector3(size*.65f,size*.35f,size),color);var velocity=new Vector3((float)(random.NextDouble()-.5)*3,(float)random.NextDouble()*2+.7f,(float)(random.NextDouble()-.5)*3);impactBits.Add((node,velocity,Time.GetTicksMsec()/1000.0+1.2));}
 }
 void ConfirmHit(bool head){if(coopExecuting){if(coopActor>0)RpcId(coopActor,nameof(CoopHitFeedback),head);return;}if(hitMarker==null){hitMarker=new Label{MouseFilter=Control.MouseFilterEnum.Ignore};hitMarker.AddThemeFontSizeOverride("font_size",26);hudRoot.AddChild(hitMarker);}hitMarker.Text=head?"✕":"×";hitMarker.AddThemeColorOverride("font_color",head?new Color(.95f,.30f,.18f):new Color(.95f,.83f,.63f));hitUntil=Time.GetTicksMsec()/1000.0+.22;}
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)] void CoopHitFeedback(bool head){if(CoopClient)ConfirmHit(head);}
 void DeathRemains(Infected e){if(IsSammy(e.Id))return;if(DisplayServer.GetName()=="headless")return;while(remains.Count>=16){remains[0].Node.QueueFree();remains.RemoveAt(0);}var n=new Node3D{Position=e.Body.Position+Vector3.Up*.07f};AddChild(n);Ellipsoid(n,Vector3.Zero,new Vector3(.65f,.025f,.4f),new Color(.23f,.018f,.018f));var body=new Node3D{RotationDegrees=new(90,e.Body.RotationDegrees.Y,0),Position=new(0,.12f,0)};n.AddChild(body);ActorArt(body,e.Id,e.Tier,e.Night);remains.Add((n,Time.GetTicksMsec()/1000.0+30));}
 void TickImpactFeedback(double dt){double now=Time.GetTicksMsec()/1000.0;if(axePending&&now-axeSwingStarted>=.24&&!paused){axePending=false;ChopTree();}
  if(feedbackHealth>=0&&game.Life.Health<feedbackHealth&&!paused&&!coopExecuting){hurtShake=.28;ImpactParticles(camera.GlobalPosition-camera.GlobalBasis.Z*.5f,new Color(.42f,.035f,.025f),5,.025f);Sound(4);}feedbackHealth=game.Life.Health;hurtShake=Math.Max(0,hurtShake-dt);camera.HOffset=(float)(Math.Sin(now*83)*hurtShake*.025);camera.VOffset=(float)(Math.Sin(now*97)*hurtShake*.018);
  if(hitMarker!=null){hitMarker.Position=GetViewport().GetVisibleRect().Size*.5f-new Vector2(9,17);hitMarker.Visible=now<hitUntil;}
  if(axeEquipped){float t=(float)(now-axeSwingStarted);weaponRoot.RotationDegrees=t<.65f?new Vector3(-25*Mathf.Sin(t/.65f*Mathf.Pi),0,65*Mathf.Sin(t/.65f*Mathf.Pi)):Vector3.Zero;}
  for(int i=impactBits.Count-1;i>=0;i--){var bit=impactBits[i];if(now>bit.Until){bit.Node.QueueFree();impactBits.RemoveAt(i);continue;}bit.Velocity+=Vector3.Down*8*(float)dt;bit.Node.Position+=bit.Velocity*(float)dt;impactBits[i]=bit;}
  for(int i=remains.Count-1;i>=0;i--)if(now>remains[i].Until){remains[i].Node.QueueFree();remains.RemoveAt(i);}
 }
 void GearSlots(VBoxContainer profile){var grid=new GridContainer{Columns=2};profile.AddChild(grid);foreach(string slot in new[]{"head","chest","hands","feet"}){string copy=slot;var item=game.Profile.Items.FirstOrDefault(i=>i.Id==game.GearId(slot));var b=new Godot.Button{Text=slot.ToUpperInvariant()+"\n"+(item==null?"EMPTY":"T"+item.Level),TooltipText=item==null?"Craft this slot at your workbench":item.Name+" / click to unequip",CustomMinimumSize=new(120,30)};grid.AddChild(b);b.Pressed+=()=>{if(item==null){inventoryFilter="ARMOR";SurvivorInventory();}else{if(!CoopRequest("unequip",copy))game.Change(()=>game.UnequipGear(copy));SurvivorInventory();}};}}
}
