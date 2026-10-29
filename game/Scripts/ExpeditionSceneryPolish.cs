using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MigrationScene {
 Mesh scrubMesh,flowerMesh;ShaderMaterial undergrowthMaterial;
 bool SceneryClear(double x,double z,float radius=.8f){
  var w=game.World;if(!w.Playable(x,z)||w.IsRiver(x,z)||w.SurfaceClearance(x,z,radius)||w.BriarRoad(x,z,3.6)||w.MainRoad(x,z,w.Tier(x,z)>=3?3.1:1.4)||w.UrbanStreet(x,z,2.1)||InSuburbHome(x,z)||ForestBuildClearance(x,z))return false;
  if(InLivingLot(x,z)||InEncounterLot(x,z)||InCrashLot(x,z)||InCheckpointLot(x,z)||InBirthdayLot(x,z)||InLetterLot(x,z)||InFeverLot(x,z)||InWoodlandCamp(x,z)||InFamilyLot(x,z)||InMill(x,z,35))return false;
  if(w.Tier(x,z)>=3){double bx=Math.Floor(x/32)*32+16,bz=Math.Floor(z/32)*32+16;if(w.BuildingCell(bx,bz)&&Math.Abs(x-bx)<w.BuildingHalf(bx,bz)+radius&&Math.Abs(z-bz)<w.BuildingHalf(bx,bz)+radius)return false;
   double shopZ=Math.Round((z-8)/64)*64+8;if(Math.Abs(z-shopZ)<4&&Math.Abs(Math.Abs(x-w.RouteX(shopZ))-14)<5)return false;
  }
  // Explicitly keep Briar Lane's parked cars and their narrow driveways clear.
  if(w.BriarReserve(x,z))for(int i=0;i<8;i++){double hx=ExpeditionWorld.SpawnX+(i%2==0?-26:26),hz=ExpeditionWorld.SpawnZ-30-(i/2)*42;double qx=(i%2==0?-1:1)*(z-hz),qz=(i%2==0?1:-1)*(x-hx);if(qx>6.2&&qx<8.9&&qz> -4&&qz<20)return false;}
  return true;
 }
 float SceneryPatch(double x,double z)=>.5f+.25f*Mathf.Sin((float)(x*.12+z*.07))+.25f*Mathf.Sin((float)(z*.19-x*.04));
 void ScatterDetail(Node3D root,string name,Mesh mesh,List<Vector3> points,Color[] colors,int seed,float distance,float scaleLow,float scaleHigh){
  if(points.Count==0)return;var rng=new Random(seed);var mm=new MultiMesh{TransformFormat=MultiMesh.TransformFormatEnum.Transform3D,Mesh=mesh,UseColors=true,InstanceCount=points.Count};
  for(int i=0;i<points.Count;i++){float scale=scaleLow+(float)rng.NextDouble()*(scaleHigh-scaleLow);mm.SetInstanceTransform(i,new Transform3D(Basis.FromEuler(new(0,(float)rng.NextDouble()*Mathf.Tau,0)).Scaled(Vector3.One*scale),points[i]));mm.SetInstanceColor(i,colors[i%colors.Length]*(.88f+(float)rng.NextDouble()*.23f));}
  undergrowthMaterial??=new ShaderMaterial{Shader=GD.Load<Shader>("res://Shaders/undergrowth.gdshader")};
  root.AddChild(new MultiMeshInstance3D{Name=name,Multimesh=mm,MaterialOverride=undergrowthMaterial,VisibilityRangeEnd=distance,CastShadow=GeometryInstance3D.ShadowCastingSetting.Off});
 }
 Mesh SmallPlant(bool flower){
  var st=new SurfaceTool();st.Begin(Mesh.PrimitiveType.Triangles);
  void Tri(Vector3 a,Vector3 b,Vector3 c,bool petal=false){foreach(var p in new[]{a,b,c}){st.SetColor(flower&&!petal?new Color(.40f,.55f,.28f):Colors.White);st.SetNormal(Vector3.Up);st.AddVertex(p);}}
  for(int frond=0;frond<(flower?5:9);frond++){
   float angle=frond*2.4f;var outwards=new Vector3(Mathf.Sin(angle),0,Mathf.Cos(angle));var side=new Vector3(outwards.Z,0,-outwards.X);float height=flower?.36f:.35f+(frond%3)*.12f;
   var tip=outwards*(flower?.13f:.37f)+Vector3.Up*height;
   Tri(Vector3.Zero,tip-side*.025f,tip+side*.025f);
   var center=tip*.63f;var leaf=outwards*.21f;Tri(center,center+side*.11f+leaf*.5f,center+leaf);Tri(center,center+leaf,center-side*.11f+leaf*.5f);
   if(flower)for(int petal=0;petal<5;petal++){float a=petal*Mathf.Tau/5;var d=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a))*.065f;Tri(tip,tip+d+Vector3.Up*.025f,tip+d.Rotated(Vector3.Up,.9f),true);}
  }
  return st.Commit();
 }
 void SceneryUndergrowth(Node3D root,int cx,int cz){
  scrubMesh??=SmallPlant(false);flowerMesh??=SmallPlant(true);var shrubs=new List<Vector3>();var flowers=new List<Vector3>();var rng=new Random(unchecked((int)game.World.Hash(cx+6281,cz-901)));
  for(int i=0;i<190;i++){double x=cx*64+rng.NextDouble()*64,z=cz*64+rng.NextDouble()*64;int tier=game.World.Tier(x,z);
   if(!SceneryClear(x,z,1.1f)||SceneryPatch(x,z)<.48f||tier>=3&&i>25)continue;
   var p=SurfaceGround(x,z);if(tier==2||i%3==0)shrubs.Add(p);else flowers.Add(p);
  }
  ScatterDetail(root,"SceneryShrubs",scrubMesh,shrubs,new[]{new Color(.27f,.34f,.17f),new Color(.38f,.37f,.22f),new Color(.24f,.31f,.21f)},cx*7919+cz,82,.7f,1.65f);
  ScatterDetail(root,"SceneryWildflowers",flowerMesh,flowers,new[]{new Color(.71f,.65f,.40f),new Color(.69f,.67f,.57f),new Color(.46f,.39f,.47f)},cx*1777+cz,65,.65f,1.1f);
 }
 void GroundedFencePost(Node3D house,Vector3 point){
  var world=house.Transform*point;float bottom=Math.Min(0,SurfaceGround(world.X,world.Z).Y-house.Position.Y-.12f),height=1.2f-bottom;
  Box(house,new(point.X,bottom+height/2,point.Z),new(.10f,height,.13f),Wood);
 }
 void HouseSceneryDetails(Node3D house,int style){
  var detail=new Node3D{Name="HouseFinishDetails"};house.AddChild(detail);var trim=new Color(.29f,.30f,.29f);
  foreach(int side in new[]{-1,1}){
   Box(detail,new(side*6.35f,3.35f,0),new(.14f,.14f,12.1f),trim);
   var foot=house.Transform*new Vector3(side*6.3f,0,-5.6f);float bottom=SurfaceGround(foot.X,foot.Z).Y-house.Position.Y+.12f;
   Tube(detail,new(side*6.3f,3.35f,-5.6f),new(side*6.3f,bottom,-5.6f),.045f,trim);
   var porch=house.Transform*new Vector3(side*2.9f,0,7.1f);float depth=Math.Max(.2f,house.Position.Y-SurfaceGround(porch.X,porch.Z).Y+.16f);
   Box(detail,new(side*2.9f,-depth/2,7.1f),new(.16f,depth,.16f),Wood);
   Box(detail,new(side*5.88f,1.7f,0),new(.025f,3.1f,10.7f),new(.51f,.49f,.43f));
   Box(detail,new(side*5.84f,.28f,0),new(.07f,.22f,10.7f),new(.30f,.27f,.22f));
  }
  Box(detail,new(0,1.7f,-5.38f),new(11.75f,3.1f,.025f),new(.51f,.49f,.43f));
  Box(detail,new(0,.28f,-5.33f),new(11.75f,.22f,.07f),new(.30f,.27f,.22f));
  Box(detail,new(-1.8f,2.25f,5.68f),new(.25f,.35f,.09f),trim);Box(detail,new(-1.8f,2.25f,5.75f),new(.17f,.23f,.08f),new(.71f,.64f,.43f));
  if(style%3==0){Box(detail,new(-2.9f,5.25f,-2.5f),new(.65f,1.8f,.75f),new(.36f,.26f,.21f)).MaterialOverride=ExteriorFinish(2,new(.36f,.26f,.21f));Box(detail,new(-2.9f,6.19f,-2.5f),new(.82f,.15f,.92f),trim);}
  foreach(var mesh in detail.FindChildren("*","GeometryInstance3D",true,false).OfType<GeometryInstance3D>())mesh.VisibilityRangeEnd=115;
 }
 void WarehouseSceneryDetails(Node3D warehouse,float size){
  var detail=new Node3D{Name="WarehouseFinishDetails"};warehouse.AddChild(detail);
  // Fixed wall-mounted dressing stays within the existing solid shell envelope.
  Box(detail,new(size*.5f+.08f,1.4f,size*.20f),new(.16f,1.1f,.9f),Steel);
  foreach(int side in new[]{-1,1}){Box(detail,new(side*(size*.5f-.3f),2.8f,-size*.5f-.13f),new(.35f,.18f,.25f),Ink);Box(detail,new(side*(size*.5f-.3f),2.76f,-size*.5f-.28f),new(.26f,.08f,.06f),new(.75f,.69f,.48f));}
  for(int i=0;i<4;i++)Box(detail,new(size*.5f+.18f,.99f+i*.2f,size*.20f),new(.025f,.035f,.7f),Ink);
  foreach(var mesh in detail.FindChildren("*","GeometryInstance3D",true,false).OfType<GeometryInstance3D>())mesh.VisibilityRangeEnd=110;
 }
 Color CityWallPaint(uint hash)=>new[]{new Color(.47f,.43f,.36f),new Color(.39f,.44f,.46f),new Color(.48f,.38f,.30f),new Color(.43f,.45f,.40f)}[hash%4];
 void CitySceneryDetails(Node3D root,Vector3 position,float size,float height,uint hash){
  var detail=new Node3D{Name="CityFinishDetails",Position=position};root.AddChild(detail);
  // One shared mesh per silhouette/style, rather than dozens of extra draws.
  FigureMesh(detail,$"city-finish/{size}/{height}/{hash%3}",b=>{
   void Part(Vector3 p,Vector3 dimensions,Color color){var radius=new Vector2(dimensions.X/Mathf.Sqrt(2),dimensions.Z/Mathf.Sqrt(2));b.Loft(color,new[]{p-Vector3.Up*dimensions.Y/2,p+Vector3.Up*dimensions.Y/2},new[]{radius,radius},4);}
   var trim=new Color(.25f,.28f,.28f);
   foreach(int side in new[]{-1,1}){
    Part(new(side*(size/2+.045f),3.45f,0),new(.16f,.28f,size+.2f),trim);
    Part(new(0,3.45f,side*(size/2+.045f)),new(size+.2f,.28f,.16f),trim);
    Part(new(side*size/2,height+.52f,0),new(.22f,.55f,size+.3f),trim);
    Part(new(0,height+.52f,side*size/2),new(size+.3f,.55f,.22f),trim);
   }
   Part(new(0,2.55f,-size/2-.38f),new(3,.18f,.9f),new(.36f,.30f,.24f));
   Part(new(size*.22f,height+.75f,size*.15f),new(2.2f,1,1.6f),new(.36f,.38f,.37f));
   for(int i=0;i<5;i++)Part(new(size*.22f-.8f+i*.4f,height+1.26f,size*.15f),new(.06f,.03f,1.3f),Ink);
   b.Limb(trim,new(size/2+.12f,.2f,size*.3f),new(size/2+.12f,height+.4f,size*.3f),.065f,.065f,.065f,.065f,6);
  });
  detail.GetChild<MeshInstance3D>(0).VisibilityRangeEnd=145;
 }
 void IndustrialYardDetails(Node3D root,int x,int z,float size,int tier,bool simulation){
  if(tier!=3)return;
  for(int item=0;item<3;item++){
   double px=x+size/2+1.35,pz=z+size*.2-item*1.4;
   if(!SceneryClear(px,pz,.55f))continue;
   var n=new Node3D{Name="IndustrialYardProp",Position=SurfaceGround(px,pz)};root.AddChild(n);
   if(item<2){PhysicsObstacle(n,new CylinderShape3D{Radius=.31f,Height=.92f},new(0,.46f,0));if(!simulation)FigureMesh(n,"scenery/steel-drum/"+(item%2),b=>{
     var paint=item==0?new Color(.31f,.37f,.37f):new Color(.45f,.32f,.21f);
     b.Loft(paint,new[]{Vector3.Zero,new Vector3(0,.92f,0)},new[]{new Vector2(.3f,.3f),new Vector2(.3f,.3f)},10);
     foreach(float y in new[]{.08f,.3f,.65f,.86f})b.Loft(Steel,new[]{new Vector3(0,y-.025f,0),new Vector3(0,y+.025f,0)},new[]{new Vector2(.315f,.315f),new Vector2(.315f,.315f)},10);
    });
   }else{PhysicsObstacle(n,new BoxShape3D{Size=new(1.4f,.42f,1.1f)},new(0,.21f,0));if(!simulation)FigureMesh(n,"scenery/wooden-pallet-stack",b=>{
     for(int level=0;level<2;level++)for(int plank=0;plank<5;plank++)b.Limb(Wood,new(-.65f,.12f+level*.2f,-.44f+plank*.22f),new(.65f,.12f+level*.2f,-.44f+plank*.22f),.07f,.04f,.07f,.04f,4);
     foreach(float xx in new[]{-.5f,.5f})b.Limb(Wood,new(xx,.05f,-.5f),new(xx,.05f,.5f),.08f,.06f,.08f,.06f,4);
    });}
   if(!simulation)foreach(var mesh in n.GetChildren().OfType<GeometryInstance3D>())mesh.VisibilityRangeEnd=90;
  }
 }
}
