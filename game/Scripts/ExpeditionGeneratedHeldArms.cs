using Godot;
using System;
using System.Linq;

public partial class MigrationScene {
 const string GeneratedHeldArmPath="res://Assets/Hunyuan/Actors/generated-fps-arm.glb";
 readonly ArrayMesh[] generatedHeldArmMeshes=new ArrayMesh[2];
 Material generatedHeldArmMaterial;

 // The landmarks are measured on the front orthographic projection of the generated mesh.
 static readonly Vector2 ArmWrist=new(354,316);
 static readonly Vector2 ArmProximal=new(97,458);
 static readonly Vector2 ArmPalm=new(407,224);
 static readonly Vector2[][] ArmDigits={
  new Vector2[]{new(454,189),new(473,152),new(495,109)},
  new Vector2[]{new(418,177),new(424,125),new(431,74)},
  new Vector2[]{new(379,182),new(373,137),new(366,96)},
  new Vector2[]{new(349,208),new(336,179),new(320,150)},
  new Vector2[]{new(457,262),new(498,246),new(534,225)}
 };
 static readonly Vector2 PalmAxis=(ArmPalm-ArmWrist).Normalized();
 static readonly Vector2 ThumbAxis=new(-PalmAxis.Y,PalmAxis.X);
 const float ArmPixelScale=.00078f;

 static Vector3 ArmLocal(Vector2 pixel,float depth,bool left){
  var delta=pixel-ArmWrist;
  return new Vector3((left?1:-1)*delta.Dot(PalmAxis)*ArmPixelScale,delta.Dot(ThumbAxis)*ArmPixelScale,depth*.185f);
 }
 static Vector2 ArmProjection(Vector3 v)=>new(295+(v.X+.001892f)*250.74f,285-(v.Y-.00134f)*250.74f);

 ArrayMesh LoadGeneratedHeldArm(bool left){
  int side=left?0:1;
  if(generatedHeldArmMeshes[side]!=null)return generatedHeldArmMeshes[side];
  var scene=GD.Load<PackedScene>(GeneratedHeldArmPath)?.Instantiate<Node3D>();
  if(scene==null)return null;
  try{
   var source=scene.FindChildren("*","MeshInstance3D",true,false).OfType<MeshInstance3D>().FirstOrDefault();
   if(source==null||source.Mesh.GetSurfaceCount()!=1)return null;
   var arrays=source.Mesh.SurfaceGetArrays(0);
   var vertices=arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();
   var normals=arrays[(int)Mesh.ArrayType.Normal].AsVector3Array();
   var indices=arrays[(int)Mesh.ArrayType.Index].AsInt32Array();
   var bones=new int[vertices.Length*4];
   var weights=new float[bones.Length];
   var sleeveAxis=(ArmProximal-ArmWrist).Normalized();
   for(int i=0;i<vertices.Length;i++){
    var pixel=ArmProjection(vertices[i]);
    vertices[i]=ArmLocal(pixel,vertices[i].Z-.04f,left);
    if(i<normals.Length){
     var n=normals[i];
     normals[i]=(ArmLocal(ArmWrist+new Vector2(n.X,-n.Y)*250.74f,n.Z,left)).Normalized();
    }
    int a=0,b=1;
    float blend=1-Ease(((pixel-ArmWrist).Dot(sleeveAxis)+16)/34);
    float best=19*19;
    for(int finger=0;finger<ArmDigits.Length;finger++){
     var digit=ArmDigits[finger];
     var axis=digit[2]-digit[0];
     float t=(pixel-digit[0]).Dot(axis)/axis.LengthSquared();
     if(t<0)continue;
     float dist=pixel.DistanceSquaredTo(digit[0]+axis*Math.Clamp(t,0,1));
     if(dist>=best)continue;
     best=dist;
     int joint=2+finger*2;
     a=t<.38f?1:joint;b=t<.38f?joint:joint+1;
     blend=t<.38f?Ease(t/.30f):Ease((t-.40f)/.26f);
    }
    int offset=i*4;
    bones[offset]=a;bones[offset+1]=b;weights[offset]=1-blend;weights[offset+1]=blend;
   }
   // The second hand reflects the generated surface, including its triangle winding.
   if(left)for(int i=0;i<indices.Length;i+=3)(indices[i+1],indices[i+2])=(indices[i+2],indices[i+1]);
   arrays[(int)Mesh.ArrayType.Vertex]=vertices;
   arrays[(int)Mesh.ArrayType.Normal]=normals;
   arrays[(int)Mesh.ArrayType.Index]=indices;
   arrays[(int)Mesh.ArrayType.Tangent]=default(Variant);
   arrays[(int)Mesh.ArrayType.Bones]=bones;
   arrays[(int)Mesh.ArrayType.Weights]=weights;
   var mesh=new ArrayMesh();
   mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles,arrays);
   if(generatedHeldArmMaterial==null&&source.GetActiveMaterial(0) is StandardMaterial3D material){
    var copy=(StandardMaterial3D)material.Duplicate();copy.Metallic=0;copy.Roughness=.88f;
    generatedHeldArmMaterial=copy;
   }
   return generatedHeldArmMeshes[side]=mesh;
  }finally{scene.Free();}
 }

 HeldArm BuildGeneratedHeldArm(Node3D grip,bool left){
  if(handStyle!=1||!ResourceLoader.Exists(GeneratedHeldArmPath))return null;
  var mesh=LoadGeneratedHeldArm(left);
  if(mesh==null)return null;
  var skeleton=new Skeleton3D{Name="GeneratedHeldArm",Position=HeldWrist(left)};
  grip.AddChild(skeleton);
  var skin=new Skin();
  AddViewBone(skeleton,skin,"Sleeve",Vector3.Zero);
  var sleeveRest=new Transform3D(new Basis(new Quaternion(Vector3.Up,ArmLocal(ArmProximal,0,left).Normalized())),Vector3.Zero);
  skeleton.SetBoneRest(0,sleeveRest);
  skin.SetBindPose(0,sleeveRest.AffineInverse());
  AddViewBone(skeleton,skin,"Palm",Vector3.Zero);
  for(int finger=0;finger<ArmDigits.Length;finger++){
   var points=ArmDigits[finger];
   var basePivot=ArmLocal(points[0],0,left);
   var midPivot=ArmLocal(points[1],0,left);
   int parent=skeleton.GetBoneCount();
   AddViewBone(skeleton,skin,"Digit"+finger,basePivot);
   skeleton.SetBoneParent(parent,1);
   AddViewBone(skeleton,skin,"Digit"+finger+"Tip",midPivot);
   skeleton.SetBoneParent(parent+1,parent);
   skeleton.SetBoneRest(parent+1,new Transform3D(Basis.Identity,midPivot-basePivot));
   skeleton.SetBonePosePosition(parent+1,midPivot-basePivot);
  }
  skeleton.AddChild(new MeshInstance3D{
   Name="GeneratedSleeveAndHand",Mesh=mesh,MaterialOverride=generatedHeldArmMaterial,Skin=skin,Skeleton=new NodePath(".."),
   CastShadow=GeometryInstance3D.ShadowCastingSetting.Off,CustomAabb=new Aabb(new Vector3(-2,-2,-2),new Vector3(4,4,4))
  });
  return new HeldArm{Hand=skeleton,Shoulder=new Vector3(left?-.33f:.33f,-.53f,.075f),Generated=true};
 }

 void PoseGeneratedHeldArm(HeldArm arm,Node3D grip,bool left,float open,float trigger){
  var skeleton=arm.Hand;
  var wrist=HeldWrist(left);
  var target=grip.ToLocal(camera.ToGlobal(arm.Shoulder))-wrist;
  var rest=ArmLocal(ArmProximal,0,left);
  // The bind frame points down the sleeve, so scale acts only along its length.
  skeleton.SetBonePoseRotation(0,new Quaternion(Vector3.Up,target.Normalized()));
  skeleton.SetBonePoseScale(0,new Vector3(1,target.Length()/rest.Length(),1));
  float fingers=heldProfile.Motion switch{
   HeldMotion.Stone=>.72f,
   HeldMotion.Rifle or HeldMotion.Shotgun or HeldMotion.Crossbow or HeldMotion.Flame when left=>.90f,
   HeldMotion.Bow when !left=>1.22f,
   _=>1.12f
  };
  for(int i=0;i<ArmDigits.Length;i++){
   var digit=ArmDigits[i];
   var direction=ArmLocal(digit[2],0,left)-ArmLocal(digit[0],0,left);
   var axis=direction.Normalized().Cross(Vector3.Forward).Normalized();
   float curl=i==4?.58f:fingers;
   if(!left&&i==0&&!axeEquipped&&!buildMode&&ExpeditionGame.IsGun(game.Craft.Weapon))curl=.85f+trigger*.24f;
   curl*=1-open*.76f;
   skeleton.SetBonePoseRotation(2+i*2,new Quaternion(axis,curl));
   skeleton.SetBonePoseRotation(3+i*2,new Quaternion(axis,curl*.72f));
  }
 }
}
