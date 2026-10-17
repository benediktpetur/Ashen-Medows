using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 // Species is encoded in the stable actor ID, shared by host and replicas.
 static string Species(string id)=>id.StartsWith("fauna/")?id.Split('/')[1]:"";
 static bool Peaceful(string id)=>Species(id) is "deer" or "rabbit";
 void ActorArt(Node3D root,string id,int tier,int night){if(IsCheckpointGuard(id)){CheckpointGuardArt(root,id);return;}if(IsSammy(id)){SammyArt(root);return;}if(IsMillBoss(id)){ForemanModel(root);return;}string kind=Species(id);if(kind!=""){AnimalArt(root,kind);return;}int model=night==1?5:night==2?6:Math.Clamp(tier-1,0,4);uint h=(uint)id.GetHashCodeStable();if(night==0&&tier==2&&h%4==0)model=0;if(night==0&&tier==4)model=h%3==0?4:3;HumanCharacter(root,tier,night,false,model);}
 void SpawnFauna(){if(dungeon!=null||CoopClient)return;var centers=CoopHost?ActiveSurfaceCenters():new System.Collections.Generic.List<Vector3>{player.Position};
  foreach(var center in centers){int cx=(int)Math.Floor(center.X/64),cz=(int)Math.Floor(center.Z/64);int local=infected.Count(e=>Species(e.Id)!=""&&Distance(e.Body.Position,center)<112);
   for(int dx=-1;dx<=1;dx++)for(int dz=-1;dz<=1;dz++){int x=cx+dx,z=cz+dz;uint h=game.World.Hash(x+1741,z-217);double px=x*64+10+h%42,pz=z*64+10+(h>>8)%42;int tier=game.World.Tier(px,pz);if(tier>2||!game.World.Playable(px,pz)||Distance(Ground(px,pz),center)>100||!ChunkLoaded(Ground(px,pz)))continue;
    string kind=tier==1?(h%3==0?"rabbit":"deer"):(Math.Abs(px-game.World.RouteX(pz))<65&&h%2==0?"dog":"boar");int count=kind=="rabbit"?2:kind=="dog"?2:1;
    for(int i=0;i<count&&local<14&&infected.Count<(CoopHost?CoopMaxEnemies:85);i++){string id=$"fauna/{kind}/{game.Save.Seed}/{x}:{z}/{i}";int before=infected.Count;AddEnemy(id,px+i*2,pz+i*2,tier);if(infected.Count>before)local++;}
   }
  }
 }
 bool AnimalSight(Infected e,float range){if(Distance(e.Body.Position,player.Position)>range)return false;var q=PhysicsRayQueryParameters3D.Create(e.Body.Position+Vector3.Up*.45f,player.Position+Vector3.Up*.9f);q.Exclude=new Godot.Collections.Array<Rid>{e.Body.GetRid()};var hit=GetWorld3D().DirectSpaceState.IntersectRay(q);return hit.Count==0||hit["collider"].AsGodotObject()==player;}
 void TickAnimal(Infected e,double dt){string kind=Species(e.Id);bool passive=Peaceful(e.Id);float d=Distance(e.Body.Position,player.Position);e.Cooldown=Math.Max(0,e.Cooldown-dt);e.Stagger=Math.Max(0,e.Stagger-dt);e.Memory=Math.Max(0,e.Memory-dt);
  if(AnimalSight(e,passive?12:kind=="dog"?22:13)){e.Memory=passive?5:7;e.Target=player.Position;}
  bool alert=e.Memory>0;var target=alert?(passive?e.Body.Position+(e.Body.Position-e.Target).Normalized()*14:e.Target):e.Home+new Vector3((float)Math.Sin(game.Clock*.10+e.Home.X)*7,0,(float)Math.Cos(game.Clock*.08+e.Home.Z)*7);
  var delta=target-e.Body.Position;delta.Y=0;float speed=alert?(kind=="rabbit"?4.3f:kind=="deer"?4.8f:kind=="dog"?3.5f:2.6f):kind=="rabbit"?.55f:.65f;var v=delta.Length()>.6f&&e.Stagger<=0?delta.Normalized()*speed:Vector3.Zero;v.Y=e.Body.IsOnFloor()?0:e.Body.Velocity.Y-18*(float)dt;e.Body.Velocity=v;e.Body.MoveAndSlide();
  if(e.Body.Position.Y<game.World.Height(e.Body.Position.X,e.Body.Position.Z)-8)e.Body.Position=Ground(e.Body.Position.X,e.Body.Position.Z);if(delta.LengthSquared()>.01)e.Body.Rotation=new(0,Mathf.Atan2(-delta.X,-delta.Z),0);AnimateAnimal(e);
  if(!passive&&d<1.35f&&e.Cooldown<=0&&AnimalSight(e,1.35f)&&Math.Abs(e.Body.Position.Y-player.Position.Y)<1){if(kind=="dog")game.Hurt(2);else{game.Life.Health-=9;game.LastDamage=game.Clock;game.Notice="Boar charge";}e.Cooldown=2.1;if(game.Life.Health<=0)Die();}
 }
 void AnimalLoot(Infected e){string kind=Species(e.Id);game.Change(()=>{if(!game.Skills.Defeated.Contains(e.Id)){game.Skills.Defeated.Add(e.Id);game.Skills.Survival.Gain(kind=="dog"?10:15);if(kind!="dog")game.Life.Food+=kind=="rabbit"?1:3;}});game.Notice=kind=="dog"?"Infected dog / contaminated meat discarded":"Hunted "+kind+" / food collected";}
 void AnimateAnimal(Infected e){if(e.Visual.GetChildCount()==0)return;float phase=(float)(game.Clock*(e.Memory>0?10:4)+e.Home.X);for(int i=0;i<4;i++){var leg=e.Visual.GetNodeOrNull<Node3D>("Leg"+i);if(leg!=null)leg.Rotation=new Vector3(Mathf.Sin(phase+(i==0||i==3?0:Mathf.Pi))*(e.Memory>0?.5f:.18f),0,0);}}
 void AnimalArt(Node3D root,string kind){bool rabbit=kind=="rabbit",deer=kind=="deer",dog=kind=="dog";float y=rabbit?.28f:deer?.95f:.57f,len=rabbit?.22f:deer?.60f:.46f,width=rabbit?.13f:deer?.22f:dog?.17f:.29f;Color fur=rabbit?new(.48f,.44f,.36f):deer?new(.49f,.35f,.22f):dog?new(.27f,.28f,.24f):new(.30f,.25f,.19f);
  FigureMesh(root,"fauna-v1/"+kind+"/body",b=>{
   var body=new FigureSculpt();body.Loft(fur,new[]{new Vector3(0,-len,0),new Vector3(0,-len*.65f,0),Vector3.Zero,new Vector3(0,len*.65f,0),new Vector3(0,len,0)},new[]{new Vector2(width*.45f,width*.6f),new Vector2(width,width*.95f),new Vector2(width,width*1.15f),new Vector2(width*.85f,width),new Vector2(width*.35f,width*.5f)},20);b.Append(body,new Transform3D(Basis.FromEuler(new Vector3(Mathf.Pi/2,0,0)),new Vector3(0,y,0)));
   b.Limb(fur,new(0,y,-len*.72f),new(0,y+(deer?.36f:rabbit?.12f:.12f),-len-.10f),width*.65f,width*.65f,width*.46f,width*.46f,16);
   var head=new Vector3(0,y+(deer?.40f:rabbit?.14f:.10f),-len-.12f);b.Limb(fur,head+new Vector3(0,0,.06f),head+new Vector3(0,-.035f,-.16f),width*.55f,width*.6f,width*.30f,width*.32f,16);
   b.Limb(new Color(.14f,.13f,.12f),head+new Vector3(0,-.035f,-.155f),head+new Vector3(0,-.035f,-.18f),width*.30f,width*.27f,width*.26f,width*.24f,12);
   for(int s=-1;s<=1;s+=2){var ear=head+new Vector3(s*width*.38f,.06f,.02f);b.Limb(fur,ear,ear+new Vector3(s*.035f,rabbit?.19f:deer?.15f:.11f,.025f),rabbit?.035f:.045f,.035f,.006f,.008f,12);b.Limb(new Color(.08f,.085f,.065f),head+new Vector3(s*width*.48f,.02f,-.035f),head+new Vector3(s*width*.48f,.024f,-.051f),.012f,.012f,.012f,.012f,10);
    if(deer){var ant=head+new Vector3(s*.065f,.14f,.05f);b.Limb(new Color(.58f,.53f,.39f),ant,ant+new Vector3(s*.07f,.24f,.06f),.017f,.017f,.006f,.006f,10);b.Limb(new Color(.58f,.53f,.39f),ant+new Vector3(s*.03f,.11f,.03f),ant+new Vector3(s*.12f,.19f,-.03f),.01f,.01f,.003f,.003f,10);}
    if(!rabbit&&!deer&&!dog)b.Limb(new Color(.72f,.68f,.49f),head+new Vector3(s*.08f,-.08f,-.10f),head+new Vector3(s*.10f,.01f,-.20f),.022f,.022f,.004f,.004f,12);
   }
   b.Limb(fur,new(0,y+.025f,len*.8f),new(0,y+(dog?.22f:rabbit?.03f:-.15f),len+(rabbit?.07f:.30f)),rabbit?.06f:.03f,rabbit?.06f:.03f,.01f,.01f,12);
   if(dog){b.Limb(new Color(.34f,.12f,.11f),new(-.17f,y+.025f,-.05f),new(-.16f,y+.06f,.16f),.025f,.05f,.025f,.06f,12);for(int i=0;i<4;i++)b.Limb(new Color(.60f,.56f,.43f),new(-.174f,y-.05f+i*.035f,-.05f),new(-.174f,y-.05f+i*.035f,.15f),.009f,.009f,.009f,.009f,10);}
  });
  for(int i=0;i<4;i++){int s=i%2==0?-1:1;var leg=new Node3D{Name="Leg"+i,Position=new(s*width*.70f,y-.08f,(i<2?-1:1)*len*.63f)};root.AddChild(leg);FigureMesh(leg,"fauna-v1/"+kind+"/leg/"+i,b=>{float h=y-.10f;b.Limb(fur,Vector3.Zero,new(0,-h*.5f,.045f),rabbit?.04f:.05f,rabbit?.045f:.055f,.028f,.03f,12);b.Limb(fur,new(0,-h*.5f,.045f),new(0,-h,-.025f),.028f,.03f,.02f,.023f,12);b.Limb(new Color(.19f,.17f,.14f),new(0,-h,-.025f),new(0,-h,-.09f),rabbit?.03f:.033f,.03f,.03f,.025f,12);});}
 }
}
