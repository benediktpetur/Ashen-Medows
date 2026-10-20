using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
public partial class MigrationScene {
 readonly Dictionary<string,double> furnitureStrikes=new();double furnitureSync;
 void ResidentialChunk(Node3D root,int cx,int cz,bool simulation){
  int x0=(int)Math.Floor(cx*64/64.0)-1,x1=(int)Math.Floor((cx+1)*64/64.0)+1,z0=(int)Math.Floor(cz*64/64.0)-1,z1=(int)Math.Floor((cz+1)*64/64.0)+1;
  for(int ix=x0;ix<=x1;ix++)for(int iz=z0;iz<=z1;iz++){var p=game.World.Residential(ix,iz);if(p==null||ExpeditionWorld.Cell(p.X)!=cx||ExpeditionWorld.Cell(p.Z)!=cz)continue;if(game.World.Changes.PreservedResidentialLots?.Contains(p.Id)==true)continue;
   double floor=game.World.Height(p.X,p.Z);foreach(int xx in new[]{-9,9})foreach(int zz in new[]{-9,9})floor=Math.Max(floor,game.World.Height(p.X+xx,p.Z+zz));var n=new Node3D{Name="Residential_"+ix+"_"+iz,Position=new((float)p.X,(float)floor+.05f,(float)p.Z),RotationDegrees=new(0,(float)p.Yaw,0),Scale=new((float)p.ScaleX,1,(float)p.ScaleZ)};n.SetMeta("house_id",p.Id);n.SetMeta("house_address",p.Address);root.AddChild(n);SuburbHouse(n,p.Style,simulation);ResidentialDetails(n,p.Style,simulation);
   double tx=p.X+((ix&1)==0?17:-17),tz=p.Z+16;string treeId=p.Id+"/yard-tree";if(!game.World.Changes.Cut(treeId)){var tree=new Node3D{Position=Ground(tx,tz)};root.AddChild(tree);if(simulation)PhysicsObstacle(tree,new CylinderShape3D{Radius=.45f,Height=5},new(0,2.5f,0));else ArtTree(tree,8+p.Style%4,ix*91+iz);props.Add(new(){Id=treeId,Kind=0,Tier=1,Position=tree.Position,Node=tree});}
   if(!simulation&&p.Style%2==0){double sx=((ix&1)==0?ix*64:(ix+1)*64)+((ix&1)==0?9:-9);var pole=Ground(sx,p.Z-20);Tube(root,pole,pole+Vector3.Up*4,.045f,Steel);Box(root,pole+Vector3.Up*4,new(.5f,.15f,.35f),new(.62f,.53f,.35f));}
  }
  if(simulation)return;BatchResidentialMeshes(root);
  var st=new SurfaceTool();st.Begin(Mesh.PrimitiveType.Triangles);int count=0;for(int x=cx*64;x<(cx+1)*64;x+=4)for(int z=cz*64;z<(cz+1)*64;z+=4){double xx=x+2,zz=z+2;if(!game.World.ResidentialPaved(xx,zz)||InBriarLane(xx,zz)&&Math.Abs(xx-1152)>7)continue;bool walk=ExpeditionWorld.GridStreetDistance(xx)>5&&ExpeditionWorld.GridStreetDistance(zz)>5;Color c=walk?new(.39f,.40f,.37f):new(.13f,.14f,.135f);foreach(var v in new[]{Ground(x,z),Ground(x+4,z),Ground(x+4,z+4),Ground(x,z),Ground(x+4,z+4),Ground(x,z+4)}){st.SetColor(c);st.AddVertex(v+Vector3.Up*.055f);}count++;}if(count>0){st.GenerateNormals();root.AddChild(new MeshInstance3D{Mesh=st.Commit(),MaterialOverride=TerrainInk()});}
  for(int t=0;t<4;t++){double z=cz*64+8+t*16,x=Math.Round((cx*64+32)/128.0)*128;if(x>=cx*64&&x<(cx+1)*64&&game.World.ResidentialPaved(x,z)&&!InBriarLane(x,z))Box(root,Ground(x,z)+Vector3.Up*.085f,new(.12f,.025f,3),new(.53f,.47f,.31f));double hz=Math.Round((cz*64+32)/128.0)*128,hx=cx*64+8+t*16;if(hz>=cz*64&&hz<(cz+1)*64&&game.World.ResidentialPaved(hx,hz)&&!InBriarLane(hx,hz))Box(root,Ground(hx,hz)+Vector3.Up*.085f,new(3,.025f,.12f),new(.53f,.47f,.31f));}

 }
 void BatchResidentialMeshes(Node3D root){
  var groups=new Dictionary<(Vector3,ulong),List<MeshInstance3D>>();foreach(var mesh in root.FindChildren("*","MeshInstance3D",true,false).OfType<MeshInstance3D>()){if(mesh.Mesh is not BoxMesh box||mesh.MaterialOverride==null||mesh.GetParent().HasMeta("salvage_furniture"))continue;Node owner=mesh.GetParent();while(owner!=null&&owner!=root&&!owner.HasMeta("house_id"))owner=owner.GetParent();if(owner==null||owner==root)continue;var key=(box.Size,mesh.MaterialOverride.GetInstanceId());if(!groups.TryGetValue(key,out var list))groups[key]=list=new();list.Add(mesh);}
  foreach(var pair in groups){var list=pair.Value;if(list.Count<3)continue;var mm=new MultiMesh{TransformFormat=MultiMesh.TransformFormatEnum.Transform3D,Mesh=new BoxMesh{Size=pair.Key.Item1},InstanceCount=list.Count};var material=list[0].MaterialOverride;for(int i=0;i<list.Count;i++){var m=list[i];mm.SetInstanceTransform(i,root.GlobalTransform.AffineInverse()*m.GlobalTransform);m.GetParent().RemoveChild(m);m.Free();}root.AddChild(new MultiMeshInstance3D{Multimesh=mm,MaterialOverride=material,VisibilityRangeEnd=175});}
 }
 void ResidentialDetails(Node3D n,int style,bool simulation){
  // Additions change each home's outline, not only its wall color.
  if(style%4==0){var size=new Vector3(4,2.8f,5);var pos=new Vector3(-7.8f,1.4f,-1);if(simulation)PhysicsObstacle(n,new BoxShape3D{Size=size},pos);else{Box(n,pos,size,new(.35f,.36f,.31f),true);Box(n,new(-7.8f,2.9f,-1),new(4.5f,.2f,5.5f),Rust);Box(n,new(-7.8f,1.25f,1.55f),new(3,2.3f,.06f),new(.20f,.24f,.22f));}}
  if(simulation)return;
  if(style%4==1){for(int k=0;k<5;k++){float x=-4+k*1.7f;Box(n,new(x,.4f,-8),new(1.3f,.8f,1.1f),Wood);Ellipsoid(n,new(x,.95f,-8),new(.6f,.35f,.5f),Leaf);}}
  if(style%4==2){Tube(n,new(-6,0,8),new(-6,2.7f,8),.035f,Steel);Tube(n,new(-6,2.7f,8),new(-5,2.7f,8),.025f,Steel);Box(n,new(-5,2.7f,8),new(.6f,.3f,.1f),new(.53f,.44f,.29f));}
  if(style%4==3){Box(n,new(3,.13f,8),new(3,.26f,2),new(.25f,.22f,.17f));for(int j=0;j<6;j++){Tube(n,new(2+j*.4f,.2f,8),new(2+j*.4f,.8f,8),.012f,Leaf);}}
  n.AddChild(new Label3D{Text=new[]{"THE GARDENER","REPAIR SHOP","EVACUATED","FIRST AID","NO ANSWER","KEEP QUIET","RADIO SILENT","BACK SOON"}[style%8],Position=new(-2,1.2f,5.65f),PixelSize=.003f,FontSize=21});
 }
 void HouseFurniture(Node3D house,int style,bool simulation){
  string key=house.HasMeta("house_id")?house.GetMeta("house_id").AsString():"briar/"+house.Name;bool home=house.Name=="BriarLaneHouse1";game.World.Changes.StrippedFurniture??=new();
  if(home){
   var bed=new Vector3(0,.28f,-4.1f);PhysicsObstacle(house,new BoxShape3D{Size=new(2.4f,.65f,2.1f)},bed);if(!simulation){Box(house,bed,new(2.4f,.3f,2.1f),Wood);Bevel(house,bed+Vector3.Up*.25f,new(2.2f,.25f,1.95f),new(.45f,.47f,.38f));Box(house,new(0,.65f,-4.8f),new(1.8f,.14f,.4f),new(.67f,.63f,.51f));Box(house,new(0,.015f,.3f),new(3.4f,.03f,2.2f),new(.37f,.28f,.20f));Box(house,new(0,2,-5.37f),new(.9f,.7f,.06f),Wood);Box(house,new(0,2,-5.32f),new(.75f,.55f,.02f),new(.47f,.49f,.39f));house.AddChild(new OmniLight3D{Position=new(0,2.7f,.5f),LightColor=new(1,.81f,.58f),LightEnergy=.85f,OmniRange=10,ShadowEnabled=false});Box(house,new(0,2.95f,.5f),new(.35f,.15f,.35f),new(.74f,.63f,.39f));}
  }
  var positions=new[]{new Vector3(-4,.45f,-2.7f),new Vector3(.7f,.75f,-1.2f),new Vector3(4,.85f,-3.8f),new Vector3(-3,.45f,1.6f)};
  var sizes=new[]{new Vector3(2.8f,.85f,1.1f),new Vector3(2,.1f,1.4f),new Vector3(2.8f,1.7f,1.2f),new Vector3(.7f,.9f,.7f)};
  if(style%3==1)positions[0].Z=-3.6f;else if(style%3==2){positions[0].Z=.4f;positions[1].Z=-2.5f;positions[3].Z=3.2f;}
  for(int i=0;i<4;i++){string id=key+"/furniture/"+i;if(game.World.Changes.StrippedFurniture.Contains(id))continue;var pos=positions[i];if(style%2==1)pos.X=-pos.X;var n=new Node3D{Name="Furniture"+i,Position=pos};house.AddChild(n);PhysicsObstacle(n,new BoxShape3D{Size=sizes[i]},Vector3.Zero);
   if(!simulation){Box(n,Vector3.Zero,sizes[i],i==0?Cloth:i==2?new(.44f,.44f,.37f):Wood);if(i==0){Box(n,new(0,.55f,-.4f),new(2.8f,.5f,.25f),Cloth);foreach(float x in new[]{-.8f,0,.8f})Box(n,new(x,.45f,0),new(.7f,.14f,.75f),new(.38f,.39f,.30f));}if(i==1)foreach(float x in new[]{-.8f,.8f})foreach(float z in new[]{-.5f,.5f})Box(n,new(x,-.35f,z),new(.08f,.7f,.08f),Wood);if(i==2)for(int j=0;j<3;j++){Box(n,new(0,-.55f+j*.55f,.63f),new(2.65f,.02f,.03f),Ink);Tube(n,new(-.15f,-.3f+j*.5f,.65f),new(.15f,-.3f+j*.5f,.65f),.018f,Steel);}}
   if(home)continue;n.SetMeta("salvage_furniture",id);props.Add(new(){Id=id,Kind=42,Material=i,Node=n,Position=n.GlobalPosition,Tier=1});
  }
 }
 WorldProp AimedFurniture(){if(dungeon!=null)return null;var from=camera.GlobalPosition;var q=PhysicsRayQueryParameters3D.Create(from,from-camera.GlobalBasis.Z*3.5f);q.Exclude=new Godot.Collections.Array<Rid>{player.GetRid()};var hit=GetWorld3D().DirectSpaceState.IntersectRay(q);if(hit.Count==0)return null;var n=hit["collider"].AsGodotObject() as Node;while(n!=null){if(n.HasMeta("salvage_furniture")){string id=n.GetMeta("salvage_furniture").AsString();return props.FirstOrDefault(p=>p.Kind==42&&p.Id==id);}n=n.GetParent();}return null;}
 bool SalvageFurniture(){var p=AimedFurniture();if(p==null)return false;if(CoopRequest("furniture",p.Id))return true;SalvageFurnitureAction(p.Id);return true;}
 void SalvageFurnitureAction(string id){if(CoopClient||id.Length>160)return;var p=AimedFurniture();if(p==null||p.Id!=id||(furnitureStrikes.TryGetValue(CommunityOwner,out double nextStrike)&&game.Clock<nextStrike))return;if(!game.Craft.HasAxe&&game.Craft.ToolLevels[2]<1){game.Notice="Use a crafted axe or mechanic's tool to dismantle furniture";return;}if(game.Stamina<5){game.Notice="Rest before dismantling";return;}furnitureStrikes[CommunityOwner]=game.Clock+.75;game.Stamina-=5;Sound(4);Noise(18);p.Hits+=game.Craft.ToolLevels[2]>=2?2:1;if(p.Hits<3){game.Notice="DISMANTLING / "+p.Hits+" / 3 / the noise carries";return;}
  game.World.Changes.StrippedFurniture??=new();if(game.World.Changes.StrippedFurniture.Contains(id))return;game.Change(()=>{game.World.Changes.StrippedFurniture.Add(id);game.Craft.ScrapMetal+=p.Material==2?3:1;game.Craft.Materials[p.Material==0?2:0]+=p.Material==0?3:2;game.Skills.Strength.Gain(12);game.Career.Gain(5);});SyncFurnitureSalvage(true);game.Notice="Furniture dismantled / scrap and reclaimed materials";if(CoopHost&&coopActors.Count>0)Rpc(nameof(CoopWorldUpdate),Json.Write(WorldSnapshot()));
 }
 void SyncFurnitureSalvage(bool force=false){if(!force&&game.Clock<furnitureSync)return;furnitureSync=game.Clock+.2;game.World.Changes.StrippedFurniture??=new();foreach(var p in props.Where(p=>p.Kind==42&&game.World.Changes.StrippedFurniture.Contains(p.Id)).ToArray()){p.Node.QueueFree();props.Remove(p);}}
 void RemoveChunkFurniture(Node root){props.RemoveAll(p=>p.Kind==42&&GodotObject.IsInstanceValid(p.Node)&&root.IsAncestorOf(p.Node));}
 Vector3 CoopArrivalPosition(){if(dungeon==null&&ExpeditionWorld.HomeInterior(player.Position.X,player.Position.Z)){
  var candidates=Enumerable.Range(-4,9).SelectMany(x=>Enumerable.Range(-4,9).Select(z=>new Vector3((float)ExpeditionWorld.HomeX+x*1.05f,(float)game.World.HomeFloor+.15f,(float)ExpeditionWorld.HomeZ+z*1.05f))).OrderBy(p=>p.DistanceSquaredTo(player.Position));
  foreach(var p in candidates){if(!ExpeditionWorld.HomeInterior(p.X,p.Z))continue;var q=new PhysicsShapeQueryParameters3D{Shape=new CapsuleShape3D{Radius=.32f,Height=1.75f},Transform=new Transform3D(Basis.Identity,p+Vector3.Up*.9f),CollisionMask=1};if(GetWorld3D().DirectSpaceState.IntersectShape(q,1).Count==0)return p;}}
  return player.Position+new Vector3(3+coopActors.Count,0,0);
 }
 void SettleAtHome(){if(testing||coopCheck)return;var c=Community;bool first=!c.HomeBound;bool relocate=first&&Distance(player.Position,Ground(ExpeditionWorld.SpawnX,ExpeditionWorld.SpawnZ))<100;c.X=ExpeditionWorld.HomeX;c.Z=ExpeditionWorld.HomeZ;c.HomeBound=true;
  if(first&&c.Established){int index=0;foreach(var person in c.Survivors.Where(p=>p.Member)){var old=game.Building.Pieces.FirstOrDefault(p=>p.Id==person.BedId);int kind=old?.Kind??Array.FindIndex(BuildDefinition.All,d=>d.Function=="bed");var cot=new BuildPiece{Id="home-transfer/"+person.Id,Kind=kind,X=c.X-3.2,Z=c.Z-3+index*3,Y=game.World.HomeFloor,Yaw=-90,Health=BuildDefinition.All[kind].Durability,Stock=new int[15]};if(!game.Building.Pieces.Any(p=>p.Id==cot.Id))game.Building.Pieces.Add(cot);person.BedId=cot.Id;person.X=cot.X;person.Z=cot.Z;person.Y=cot.Y;person.Owner=person.EscortOwner="";index++;}}
  if(relocate){player.Position=new((float)c.X,(float)game.World.HomeFloor+.15f,(float)c.Z);game.Save.X=c.X;game.Save.Z=c.Z;}else if(ExpeditionWorld.HomeInterior(player.Position.X,player.Position.Z))player.Position=new(player.Position.X,(float)game.World.HomeFloor+.15f,player.Position.Z);if(first)game.Persist();
 }
}
