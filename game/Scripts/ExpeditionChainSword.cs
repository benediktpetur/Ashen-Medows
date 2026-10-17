using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class MigrationScene {
 sealed class FlameBurst {public long Owner;public double Time=1.4,Tick;public HashSet<string> Trained=new();}
 sealed class Burn {public string Id,Room;public long Owner;public double Time=3,Tick=1;}
 sealed class ChainFx {public Node3D Node;public double Time;public long Owner;public Vector3 From,Direction;public bool Flame;}
 readonly List<FlameBurst> flameBursts=new();readonly List<Burn> burns=new();readonly List<ChainFx> chainFx=new();
 double chainFlameVisual;
 Vector3 FlameNozzle()=>camera.GlobalPosition+camera.GlobalBasis*new Vector3(.23f,-.17f,-.9f);
 void StartChainFlame(){
  if(mountedBlueprint!=null||buildMode||axeEquipped||game.Craft.Weapon!=27||reloadClock>0)return;
  if(CoopRequest("chainflame"))return;
  if(game.Handling.SpecialCooldown>0){game.Notice="Exhaust cooling / "+game.Handling.SpecialCooldown.ToString("0.0")+"s";return;}
  if(game.Stamina<20||game.Craft.Fuel<8||game.Craft.Stock(4)<4){game.Notice="Flame exhaust needs 20 stamina, 8 fuel and 4 energy cells";return;}
  if(!game.Change(()=>{game.Stamina-=20;game.Craft.Fuel-=8;game.Craft.SpendAmmo(4,4);}))return;
  game.Handling.SpecialCooldown=10;flameBursts.Add(new(){Owner=coopActor});Noise(80);
  long visualOwner=coopActor>0?coopActor:CoopHost?1:0;CreateFlameFx(visualOwner,FlameNozzle(),-camera.GlobalBasis.Z);
  if(CoopHost&&coopActors.Count>0)Rpc(nameof(CoopFlameFx),visualOwner,FlameNozzle(),-camera.GlobalBasis.Z);
 }
 void TickChainCombat(double dt){
  if(CoopClient)return;
  foreach(var burst in flameBursts.ToArray()){
   burst.Time-=dt;burst.Tick-=dt;if(burst.Time<=0){flameBursts.Remove(burst);continue;}if(burst.Tick>0)continue;burst.Tick=.2;
   PrimitiveOwner(burst.Owner,()=>{
    if(game.Craft.Weapon!=27||game.Life.Health<=0){burst.Time=0;return;}
    var from=FlameNozzle();var direction=-camera.GlobalBasis.Z;
    foreach(var enemy in infected.ToArray()){
     if(IsSammy(enemy.Id))continue;var target=enemy.Body.Position+Vector3.Up*(InfectedKind(enemy)=="worm"?.15f:.9f);var delta=target-from;
     if(delta.Length()>5.5||delta.Length()<.01||direction.Dot(delta.Normalized())<.78)continue;
     var q=PhysicsRayQueryParameters3D.Create(from,target);q.Exclude=PursuitExclusions();if(GetWorld3D().DirectSpaceState.IntersectRay(q).Count>0)continue;
     enemy.Hp-=game.DamageFor(27,false)*.2*InfectedResistance(enemy,false);ReactEnemyHit(enemy,false);EnemyImpact(enemy,target,direction,3);
     if(burst.Trained.Add(enemy.Id))game.Skills.Melee.Gain(4);
     if(enemy.Hp<=0){game.Skills.Melee.Gain(18);KillEnemy(enemy);continue;}
     var burn=burns.FirstOrDefault(b=>b.Id==enemy.Id);if(burn==null)burns.Add(new(){Id=enemy.Id,Owner=burst.Owner,Room=dungeon?.Id??""});else{burn.Time=3;burn.Owner=burst.Owner;}
    }
   });
  }
  foreach(var burn in burns.ToArray()){
   burn.Time-=dt;burn.Tick-=dt;var enemy=infected.FirstOrDefault(e=>e.Id==burn.Id);
   if(enemy==null||burn.Time<=0||burn.Room!=(dungeon?.Id??"")){burns.Remove(burn);continue;}if(burn.Tick>0)continue;burn.Tick=1;
   PrimitiveOwner(burn.Owner,()=>{enemy.Hp-=6;if(enemy.Hp<=0){game.Skills.Melee.Gain(18);KillEnemy(enemy);}});
  }
 }
 void ChainElectric(Infected enemy,Vector3 point){
  if(!IsCampaignBoss(enemy.Id)&&!IsMillBoss(enemy.Id))enemy.Stagger=Math.Max(enemy.Stagger,.55);
  ElectricFx(point);if(CoopHost&&coopActors.Count>0)Rpc(nameof(CoopElectricFx),point);
 }
 void ElectricFx(Vector3 point){
  if(DisplayServer.GetName()=="headless")return;var root=new Node3D{Position=point};AddChild(root);
  for(int i=0;i<5;i++){var p=new Vector3((i%2==0?1:-1)*.13f,.12f*i,0);var mesh=Tube(root,p,p+new Vector3(i%2==0?-.2f:.2f,.16f,.05f),.012f,new(.25f,.72f,1));mesh.MaterialOverride=new StandardMaterial3D{AlbedoColor=new(.25f,.72f,1),EmissionEnabled=true,Emission=new(.15f,.6f,1)};}
  chainFx.Add(new(){Node=root,Time=.28});
 }
 void CreateFlameFx(long owner,Vector3 from,Vector3 direction){
  if(owner==0||!CoopHost&&owner==Multiplayer.GetUniqueId()||CoopHost&&owner==1)chainFlameVisual=1.4;
  if(DisplayServer.GetName()=="headless")return;var root=new Node3D();AddChild(root);
  for(int i=0;i<9;i++){float distance=.35f+i*.5f;var mesh=Ellipsoid(root,new(0,0,-distance),new(.1f+i*.055f,.1f+i*.055f,.38f),i<2?new(.2f,.65f,1):new(1,.35f,.05f));mesh.MaterialOverride=new StandardMaterial3D{AlbedoColor=i<2?new(.2f,.65f,1,.8f):new(1,.35f,.04f,.65f),Transparency=BaseMaterial3D.TransparencyEnum.Alpha,EmissionEnabled=true,Emission=i<2?new(.2f,.5f,1):new Color(1,.2f,.02f),ShadingMode=BaseMaterial3D.ShadingModeEnum.Unshaded};}
  root.AddChild(new OmniLight3D{LightColor=new(1,.35f,.08f),LightEnergy=2,OmniRange=7});chainFx.Add(new(){Node=root,Time=1.4,Owner=owner,From=from,Direction=direction,Flame=true});
 }
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]void CoopFlameFx(long owner,Vector3 from,Vector3 direction){if(CoopClient)CreateFlameFx(owner,from,direction);}
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]void CoopElectricFx(Vector3 point){if(CoopClient)ElectricFx(point);}
 void DrawChainEffects(double dt){
  if(paused&&!CoopActive)return;chainFlameVisual=Math.Max(0,chainFlameVisual-dt);
  if(chainFlameVisual>0&&game.Craft.Weapon==27)weaponRoot.RotationDegrees=new(0,180,12);
  foreach(var fx in chainFx.ToArray()){
   fx.Time-=dt;if(fx.Time<=0){fx.Node.QueueFree();chainFx.Remove(fx);continue;}
   if(!fx.Flame)continue;
   var from=fx.From;var dir=fx.Direction;
   bool local=fx.Owner==0||CoopHost&&fx.Owner==1||CoopClient&&fx.Owner==Multiplayer.GetUniqueId();
   if(local){from=FlameNozzle();dir=-camera.GlobalBasis.Z;}
   else if(coopActors.TryGetValue(fx.Owner,out var actor)){from=actor.Body.Position+Vector3.Up*1.3f;dir=-(Basis.FromEuler(new Vector3(actor.Pitch,actor.Yaw,0))).Z;}
   fx.Node.Position=from;fx.Node.LookAt(from+dir);fx.Node.Scale=Vector3.One*(.93f+.07f*(float)Math.Sin(game.Clock*42));
  }
 }
 void ClearChainCombat(){foreach(var mark in bulletMarks)mark.Node.QueueFree();bulletMarks.Clear();flameBursts.Clear();burns.Clear();foreach(var fx in chainFx)fx.Node.QueueFree();chainFx.Clear();chainFlameVisual=0;}
 void BladeModel(Node3D root,int id,bool hands){
  bool knife=id is 22 or 23;var metal=id==22?new Color(.4f,.42f,.37f):Steel;
  Tube(root,new(0,-.05f,.15f),new(0,0,-.12f),.035f,Cloth);Bevel(root,new(0,0,-.13f),new(knife?.15f:.3f,.04f,.05f),metal);
  if(id==27){
   Bevel(root,new(0,.02f,-.19f),new(.21f,.24f,.31f),new(.22f,.3f,.31f));Bevel(root,new(0,.02f,-.77f),new(.05f,.15f,.9f),Steel);
   for(int i=0;i<19;i++)Box(root,new(0,(i%2==0?1:-1)*.10f,-.35f-i*.043f),new(.07f,.05f,.035f),Ink);
   for(int i=0;i<5;i++)Box(root,new(.033f,.04f,-.42f-i*.15f),new(.015f,.05f,.07f),new(.2f,.7f,.9f));
   Tube(root,new(0,.06f,-.03f),new(0,.06f,.23f),.065f,Steel,.08f);Tube(root,new(0,.06f,.22f),new(0,.06f,.25f),.055f,Ink);
  }else{
   float length=knife?.38f:id==26?1.13f:.9f;
   FigureMesh(root,"blade/"+id,b=>{b.Panel(metal,new(-.04f,0,-.16f),new(.04f,0,-.16f),new(.035f,0,-length),new(0,0,-length-.16f),new(-.035f,0,-length));b.Panel(metal*.7f,new(-.04f,.025f,-.16f),new(.04f,.025f,-.16f),new(.035f,.025f,-length),new(0,.01f,-length-.16f),new(-.035f,.025f,-length));});
   if(id>=25)for(int i=0;i<3;i++)Box(root,new(0,.028f,-.27f-i*.13f),new(.015f,.005f,.04f),id==26?new(.66f,.57f,.30f):Ink);
  }
  if(hands)RebuiltFirstPersonArms(root,false);
 }
}
