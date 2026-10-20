using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MigrationScene {
 // The security output contains three fused people. Reuse the valid armored model.
 static readonly string[] HunyuanZombies={"zombie-0-meadow-drifter","zombie-1-woodland-logger","zombie-2-highway-worker","zombie-3-city-office-worker","special-armored-armored","zombie-5-night-stalker","zombie-6-bloated-infected"};
 static readonly string[] HunyuanJoints={"Torso","Torso/Head","LeftLeg","LeftLeg/Knee","RightLeg","RightLeg/Knee","LeftArm","LeftArm/Elbow","LeftArm/Elbow/Hand","RightArm","RightArm/Elbow","RightArm/Elbow/Hand"};
 static readonly Dictionary<string,string> HunyuanSpecial=new(){["runner"]="special-runner-runner",["stalker"]="special-stalker-stalker",["armored"]="special-armored-armored",["bloater"]="special-bloater-bloater",["weeper"]="special-weeper-weeper",["mossback"]="mossback-mossback",["howler"]="howler-bell-throat",["bilecaster"]="bilecaster-bilecaster",["gravecrawler"]="gravecrawler-gravecrawler",["roadbreaker"]="roadbreaker-roadbreaker",["cinder"]="cinder-cinder-host",["riot"]="riot-riot-husk",["wirebound"]="wirebound-wirebound",["palehunter"]="palehunter-pale-hunter",["blooddrinker"]="blooddrinker-blooddrinker"};
 readonly Dictionary<string,ArrayMesh> hunyuanMeshes=new();
 readonly Dictionary<string,Vector3[]> hunyuanRests=new();
 bool hunyuanBuilding;
 void ForemanModel(Node3D root){bool prev=hunyuanBuilding;hunyuanBuilding=true;try{BuildForemanModel(root);}finally{hunyuanBuilding=prev;}}
 void CampaignBossArt(Node3D root,int index){bool prev=hunyuanBuilding;hunyuanBuilding=true;try{BuildCampaignBossArt(root,index);}finally{hunyuanBuilding=prev;}}
 void NewThreatArt(Node3D root,string kind,int tier){bool prev=hunyuanBuilding;hunyuanBuilding=true;try{BuildNewThreatArt(root,kind,tier);}finally{hunyuanBuilding=prev;}if(!prev&&HunyuanSpecial.TryGetValue(kind,out var key))ApplyHunyuan(root,key);}
 void SpecialInfectedArt(Node3D root,string id,int tier,int night){bool prev=hunyuanBuilding;hunyuanBuilding=true;try{BuildSpecialInfectedArt(root,id,tier,night);}finally{hunyuanBuilding=prev;}string kind=ExpeditionCampaign.Kind(id,tier,night);if(!prev&&HunyuanSpecial.TryGetValue(kind,out var key))ApplyHunyuan(root,key,kind!="worm",kind=="worm"?.28f:1.7f);}
 string HunyuanPath(string key)=>"res://Assets/Hunyuan/Actors/"+key+".glb";
 void HumanCharacter(Node3D root,int tier,int night,bool survivor,int zombieModel=-1){
  BuildHumanCharacter(root,tier,night,survivor,zombieModel);
  if(!hunyuanBuilding&&!survivor)ApplyHunyuan(root,survivor?"survivor-player-survivor":HunyuanZombies[zombieModel>=0?Math.Clamp(zombieModel,0,6):night==1?5:night==2?6:Math.Clamp(tier-1,0,4)]);
 }
 void ActorArt(Node3D root,string id,int tier,int night){
  bool previous=hunyuanBuilding;hunyuanBuilding=true;
  try{ProceduralActorArt(root,id,tier,night);}finally{hunyuanBuilding=previous;}
  if(IsMegaNest(id)||IsCampaignBoss(id)||IsMillBoss(id))return;


  // Story children, named survivors and their equipment retain their authored designs.
  if(IsSettlementActor(id)||IsTownGuard(id)||IsCheckpointGuard(id)||IsSammy(id)||IsEncounterGuardian(id)||Species(id)!="")return;
  string kind=ExpeditionCampaign.Kind(id,tier,night);if(kind=="worm")return;
  if(HunyuanSpecial.TryGetValue(kind,out var special)){ApplyHunyuan(root,special,kind!="worm",kind=="worm"?.28f:1.70f);return;}
  int m=night==1?5:night==2?6:Math.Clamp(tier-1,0,4);uint h=(uint)id.GetHashCodeStable();if(night==0&&tier==2&&h%4==0)m=0;if(night==0&&tier==4)m=h%3==0?4:3;
  ApplyHunyuan(root,HunyuanZombies[m]);
 }
 void ApplyHunyuan(Node3D root,string key,bool articulated=true,float height=1.70f){
  if(!ResourceLoader.Exists(HunyuanPath(key)))return;
  if(root.GetNodeOrNull<Node3D>("HunyuanModel") is {} old){root.RemoveChild(old);old.QueueFree();}
  var anchors=articulated?HunyuanJoints.Select(p=>root.GetNodeOrNull<Node3D>(p)).ToArray():Array.Empty<Node3D>();
  if(anchors.Any(a=>a==null))return;
  var bind=anchors.Select(a=>HunyuanActorRig.Relative(a,root)).ToArray();
  // Bone rests are constant across instances so meshes and weights can be shared.
  if(!hunyuanMeshes.TryGetValue(key,out var mesh)){mesh=PrepareHunyuanMesh(key,articulated,height);if(mesh==null)return;hunyuanMeshes[key]=mesh;}
  var rest=hunyuanRests[key];
  HideHunyuanSource(root);
  var rig=new HunyuanActorRig{Name="HunyuanModel",Actor=root,Anchors=anchors,Bind=bind,ProcessPriority=100};root.AddChild(rig);
  var instance=new MeshInstance3D{Mesh=mesh};
  if(articulated){var skeleton=new Skeleton3D{Name="Skeleton"};rig.AddChild(skeleton);rig.Skeleton=skeleton;var skin=new Skin();for(int i=0;i<rest.Length;i++){skeleton.AddBone("Joint"+i);skeleton.SetBoneRest(i,new Transform3D(Basis.Identity,rest[i]));skin.AddBind(i,new Transform3D(Basis.Identity,-rest[i]));}skeleton.AddChild(instance);instance.Skeleton=new NodePath("..");instance.Skin=skin;}
  else rig.AddChild(instance);
  rig.UpdatePose();
 }
 static Vector3[] HunyuanRest()=>new[]{new Vector3(0,.96f,0),new Vector3(0,1.45f,0),new Vector3(-.095f,.91f,0),new Vector3(-.095f,.505f,-.015f),new Vector3(.095f,.91f,0),new Vector3(.095f,.505f,-.015f),new Vector3(-.198f,1.335f,0),new Vector3(-.216f,1.05f,0),new Vector3(-.216f,.805f,0),new Vector3(.198f,1.335f,0),new Vector3(.216f,1.05f,0),new Vector3(.216f,.805f,0)};
 void HideHunyuanSource(Node node){foreach(var c in node.GetChildren()){if(c is GeometryInstance3D g&&!c.Name.ToString().StartsWith("Growth")&&c.Name!="RiotShield")g.Visible=false;HideHunyuanSource(c);}}
 ArrayMesh PrepareHunyuanMesh(string key,bool articulated,float height){
  var scene=GD.Load<PackedScene>(HunyuanPath(key))?.Instantiate<Node3D>();if(scene==null)return null;
  try{
   var parts=new List<(MeshInstance3D node,Transform3D transform)>();
   void Gather(Node node,Transform3D t){if(node is Node3D n)t*=n.Transform;if(node is MeshInstance3D m&&m.Mesh!=null)parts.Add((m,t));foreach(var child in node.GetChildren())Gather(child,t);}
   Gather(scene,Transform3D.Identity);if(parts.Count==0)return null;
   bool first=true;Aabb bounds=default;foreach(var p in parts){var b=p.transform*p.node.Mesh.GetAabb();bounds=first?b:bounds.Merge(b);first=false;}
   float scale=height/Math.Max(.001f,bounds.Size.Y);var center=new Vector3(bounds.GetCenter().X,bounds.Position.Y,bounds.GetCenter().Z);
   var sourcePoints=parts.SelectMany(p=>Enumerable.Range(0,p.node.Mesh.GetSurfaceCount()).SelectMany(s=>p.node.Mesh.SurfaceGetArrays(s)[(int)Mesh.ArrayType.Vertex].AsVector3Array().Select(v=>p.transform*v))).ToArray();
   float Median(System.Collections.Generic.IEnumerable<float> numbers){var sorted=numbers.OrderBy(v=>v).ToArray();return sorted.Length==0?0:sorted[sorted.Length/2];}
   float alignment=0;
   if(articulated){var hips=sourcePoints.Where(v=>v.Y>bounds.Position.Y+bounds.Size.Y*.48f&&v.Y<bounds.Position.Y+bounds.Size.Y*.62f).ToArray();if(hips.Length>0){center.X=Median(hips.Select(v=>v.X));center.Z=Median(hips.Select(v=>v.Z));}var shoulders=sourcePoints.Where(v=>v.Y>bounds.Position.Y+bounds.Size.Y*.68f&&v.Y<bounds.Position.Y+bounds.Size.Y*.82f).ToArray();if(shoulders.Length>0){float mx=shoulders.Average(v=>v.X),mz=shoulders.Average(v=>v.Z);float xx=shoulders.Average(v=>(v.X-mx)*(v.X-mx)),zz=shoulders.Average(v=>(v.Z-mz)*(v.Z-mz)),xz=shoulders.Average(v=>(v.X-mx)*(v.Z-mz));alignment=Math.Clamp(.5f*Mathf.Atan2(2*xz,xx-zz),-.85f,.85f);}}
   // Hunyuan exports face +Z; the game faces -Z.
   var rotation=new Basis(Vector3.Up,Mathf.Pi+alignment);var result=new ArrayMesh();
   var rest=HunyuanRest();
   if(articulated){var points=sourcePoints.Select(v=>rotation*((v-center)*scale)).ToArray();
    Vector3 FitArm(float y,int side,Vector3 fallback){var candidates=points.Where(v=>Math.Abs(v.Y-y)<.065f&&v.X*side>.14f).ToArray();if(candidates.Length<20)return fallback;float edge=Median(candidates.Select(v=>v.X*side));var outer=candidates.Where(v=>v.X*side>=edge).ToArray();return new(Median(outer.Select(v=>v.X)),y,Median(outer.Select(v=>v.Z)));}
    for(int side=-1;side<=1;side+=2){int j=side<0?6:9;rest[j]=FitArm(1.335f,side,rest[j]);rest[j+1]=FitArm(1.05f,side,rest[j+1]);rest[j+2]=FitArm(.805f,side,rest[j+2]);}
   }
   hunyuanRests[key]=rest;
   foreach(var p in parts)for(int s=0;s<p.node.Mesh.GetSurfaceCount();s++){
    var arrays=p.node.Mesh.SurfaceGetArrays(s);var vertices=arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();var normals=arrays[(int)Mesh.ArrayType.Normal].AsVector3Array();
    var bones=new int[vertices.Length*4];var weights=new float[vertices.Length*4];
    for(int v=0;v<vertices.Length;v++){
     vertices[v]=rotation*((p.transform*vertices[v]-center)*scale);if(v<normals.Length)normals[v]=(rotation*p.transform.Basis*normals[v]).Normalized();
     if(articulated){var point=vertices[v];int a=0,b=0;float da=float.MaxValue,db=float.MaxValue;for(int j=0;j<rest.Length;j++){float d=HunyuanBoneDistance(point,j,rest);if(d<da){db=da;b=a;da=d;a=j;}else if(d<db){db=d;b=j;}}float w=Mathf.Pow((da+.012f)/(db+.012f),4);bones[v*4]=a;bones[v*4+1]=b;weights[v*4]=1/(1+w);weights[v*4+1]=w/(1+w);}
    }
    arrays[(int)Mesh.ArrayType.Vertex]=vertices;if(normals.Length>0)arrays[(int)Mesh.ArrayType.Normal]=normals;
    // Tangents are recomputed by Godot's importer, but their basis must follow normalization.
    if(arrays[(int)Mesh.ArrayType.Tangent].VariantType!=Variant.Type.Nil){var tangents=arrays[(int)Mesh.ArrayType.Tangent].AsFloat32Array();for(int t=0;t<tangents.Length;t+=4){var d=(rotation*p.transform.Basis*new Vector3(tangents[t],tangents[t+1],tangents[t+2])).Normalized();tangents[t]=d.X;tangents[t+1]=d.Y;tangents[t+2]=d.Z;}arrays[(int)Mesh.ArrayType.Tangent]=tangents;}
    if(articulated){arrays[(int)Mesh.ArrayType.Bones]=bones;arrays[(int)Mesh.ArrayType.Weights]=weights;}
    result.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles,arrays);
    var mat=p.node.GetActiveMaterial(s);if(mat is StandardMaterial3D standard){var copy=(StandardMaterial3D)standard.Duplicate();copy.Metallic=0;copy.Roughness=.92f;mat=copy;}result.SurfaceSetMaterial(result.GetSurfaceCount()-1,mat);
   }
   return result;
  }finally{scene.Free();}
 }
 static float HunyuanBoneDistance(Vector3 p,int j,Vector3[] r){
  Vector3 a=r[j],b=j switch{0=>new(0,1.36f,0),1=>new(0,1.68f,0),2=>r[3],3=>new(-.12f,.06f,-.04f),4=>r[5],5=>new(.12f,.06f,-.04f),6=>r[7],7=>r[8],8=>new(-.28f,.65f,0),9=>r[10],10=>r[11],_=>new(.28f,.65f,0)};
  // Keep torso/legs distinct even for wide coats, swollen bellies and backpacks.
  float t=Math.Clamp((p-a).Dot(b-a)/(b-a).LengthSquared(),0,1);float d=p.DistanceTo(a+(b-a)*t);
  if(j>=6&&Math.Abs(p.X)<Math.Min(Math.Abs(r[6].X),Math.Abs(r[9].X))*.6f)d+=.22f;if(j>=6&&p.X*a.X<0)d+=.4f;if(j==0&&p.Y<.82f)d+=.4f;if(j>=2&&j<=5&&p.Y>.95f)d+=.4f;
  return d;
 }
}
