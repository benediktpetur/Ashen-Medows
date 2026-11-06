using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MigrationScene {
 const int TrailRifle=1;
 const float RifleDrawDuration=.42f;
 static readonly Vector3 RifleMagazineSeat=new(0,-.108f,.004f);
 static readonly Vector3 RifleMagazineGrip=new(-.036f,-.079f,.004f);
 HeldArm rightHeldArm,leftHeldArm;
 Node3D rifleMagazine;
 ArrayMesh trailReceiverMesh,trailMagazineMesh;
 double rifleDrawAt;
 int rifleDrawShotSerial;

 sealed class HeldArm {
  public Skeleton3D Sleeve,Hand;
  public Vector3 Shoulder;
  public bool Generated;
 }

 void BuildRifleView(){
  var model=weaponRoot.GetNodeOrNull<Node3D>("GeneratedHeldWeapon");
  var source=model?.FindChildren("*","MeshInstance3D",true,false).OfType<MeshInstance3D>().SingleOrDefault();
  if(source==null||source.Mesh.GetSurfaceCount()!=1)return;
  if(trailMagazineMesh==null&&!SplitTrailMagazine(source))return;

  rifleMagazine=new Node3D{Name="TrailRifleMagazine",Position=RifleMagazineSeat};
  weaponRoot.AddChild(rifleMagazine);
  var fitted=weaponRoot.GetNodeOrNull<Node3D>("FittedMagazine");
  if(fitted!=null){
   fitted.Reparent(rifleMagazine,false);
   fitted.Position=new(0,-.082f,0);
   fitted.Scale=new(.77f,.68f,.88f);
  }else{
   var mount=HunyuanActorRig.Relative(source,weaponRoot);
   mount.Origin-=RifleMagazineSeat;
   rifleMagazine.AddChild(new MeshInstance3D{
    Mesh=trailMagazineMesh,Transform=mount,
    MaterialOverride=source.GetActiveMaterial(0),CastShadow=GeometryInstance3D.ShadowCastingSetting.Off
   });
   // Close the magazine where it was separated from the receiver scan.
   Bevel(rifleMagazine,new Vector3(0,.002f,.012f),new Vector3(.045f,.012f,.075f),new Color(.13f,.15f,.16f));
  }
  source.Mesh=trailReceiverMesh;

  rightGrip.Position=new(.036f,-.132f,.127f);
  rightGrip.RotationDegrees=new(0,0,-10);
  leftGrip.Position=leftGripRest=new(-.039f,-.092f,-.255f);
  leftGrip.RotationDegrees=new(-67,0,0);
  rifleDrawAt=heldClock;
  rifleDrawShotSerial=game.Handling.Serial;
 }

 bool SplitTrailMagazine(MeshInstance3D source){
  var arrays=source.Mesh.SurfaceGetArrays(0);
  var vertices=arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();
  var indices=arrays[(int)Mesh.ArrayType.Index].AsInt32Array();
  var transform=HunyuanActorRig.Relative(source,weaponRoot);
  var receiver=new List<int>();
  var magazine=new List<int>();
  // The scan's detachable magazine is the section below the receiver, forward of the trigger guard.
  for(int i=0;i<indices.Length;i+=3){
   var center=transform*((vertices[indices[i]]+vertices[indices[i+1]]+vertices[indices[i+2]])/3);
   float rearEdge=Mathf.Lerp(.053f,.069f,Ease((-.105f-center.Y)/.065f));
   var part=center.Y<-.101f&&center.Z>-.052f&&center.Z<rearEdge?magazine:receiver;
   part.Add(indices[i]);part.Add(indices[i+1]);part.Add(indices[i+2]);
  }
  if(magazine.Count==0||receiver.Count==0)return false;
  ArrayMesh MakePart(List<int> triangles){
   arrays[(int)Mesh.ArrayType.Index]=triangles.ToArray();
   var mesh=new ArrayMesh();
   mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles,arrays);
   return mesh;
  }
  trailReceiverMesh=MakePart(receiver);
  trailMagazineMesh=MakePart(magazine);
  return true;
 }

 static void AddViewBone(Skeleton3D skeleton,Skin skin,string name,Vector3 pivot){
  int bone=skeleton.GetBoneCount();
  skeleton.AddBone(name);
  skeleton.SetBoneRest(bone,new Transform3D(Basis.Identity,pivot));
  skeleton.SetBonePosePosition(bone,pivot);
  skin.AddBind(bone,new Transform3D(Basis.Identity,-pivot));
 }

 static ArrayMesh ViewSkinMesh(Mesh mesh,Func<Vector3,(int A,int B,float Blend)> weightsFor){
  var arrays=mesh.SurfaceGetArrays(0);
  var vertices=arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();
  var indices=new int[vertices.Length*4];
  var weights=new float[indices.Length];
  for(int i=0;i<vertices.Length;i++){
   var (a,b,blend)=weightsFor(vertices[i]);
   int slot=i*4;
   indices[slot]=a;indices[slot+1]=b;
   weights[slot]=1-blend;weights[slot+1]=blend;
  }
  arrays[(int)Mesh.ArrayType.Bones]=indices;
  arrays[(int)Mesh.ArrayType.Weights]=weights;
  var result=new ArrayMesh();
  result.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles,arrays);
  return result;
 }

 HeldArm BuildRifleArm(Node3D grip,bool left){
  var hand=grip.GetNode<MeshInstance3D>("AnatomicalHand");
  var handRig=new Skeleton3D{Name="RifleHandRig"};
  grip.AddChild(handRig);
  var handSkin=new Skin();
  float mirror=left?-1:1;
  AddViewBone(handRig,handSkin,"Palm",Vector3.Zero);
  for(int finger=0;finger<4;finger++)
   AddViewBone(handRig,handSkin,"Finger"+finger,new Vector3(-.028f*mirror,RifleFingerRows[finger],.007f));
  AddViewBone(handRig,handSkin,"Thumb",new Vector3(.025f*mirror,.037f,.018f));
  string handKey=$"rifle-skin/{left}/{handStyle}";
  if(!characterMeshes.TryGetValue(handKey,out var handMesh)){
   handMesh=ViewSkinMesh(hand.Mesh,p=>{
    float x=p.X*mirror;
    if(p.Y>.040f)return(0,5,Ease((p.Y-.040f)/.009f));
    int digit=0;
    for(int i=1;i<4;i++)if(Math.Abs(p.Y-RifleFingerRows[i])<Math.Abs(p.Y-RifleFingerRows[digit]))digit=i;
    return (0,digit+1,Ease((-.018f-x)/.020f));
   });
   characterMeshes[handKey]=handMesh;
  }
  hand.Reparent(handRig,false);
  hand.Mesh=handMesh;hand.Skin=handSkin;hand.Skeleton=new NodePath("..");

  var sleeve=FieldJacketSleeve();
  var sleeveRig=new Skeleton3D{Name="RifleSleeveRig"};
  viewArms.AddChild(sleeveRig);
  var sleeveSkin=new Skin();
  AddViewBone(sleeveRig,sleeveSkin,"UpperArm",new Vector3(0,-.5f,0));
  AddViewBone(sleeveRig,sleeveSkin,"Forearm",new Vector3(0,-.08f,0));
  if(!characterMeshes.TryGetValue("rifle-sleeve-skin",out var sleeveMesh)){
   sleeveMesh=ViewSkinMesh(sleeve.Mesh,p=>(0,1,Ease((p.Y+.16f)/.16f)));
   characterMeshes["rifle-sleeve-skin"]=sleeveMesh;
  }
  sleeve.Reparent(sleeveRig,false);
  sleeve.Mesh=sleeveMesh;sleeve.Skin=sleeveSkin;sleeve.Skeleton=new NodePath("..");
  sleeve.CustomAabb=new Aabb(new Vector3(-2,-2,-3),new Vector3(4,4,5));
  return new HeldArm{Sleeve=sleeveRig,Hand=handRig,Shoulder=new Vector3(.32f*mirror,-.49f,.10f)};
 }

 static readonly float[] RifleFingerRows={.032f,.011f,-.010f,-.029f};
 void PoseRifleArm(HeldArm arm,Node3D grip,bool left,float open,float trigger){
  if(arm.Generated){PoseGeneratedHeldArm(arm,grip,left,open,trigger);return;}
  var wrist=camera.ToLocal(grip.ToGlobal(HeldWrist(left)));
  var shoulder=arm.Shoulder;
  // Bias the elbow outward and down while keeping the cuff on the animated wrist.
  var elbow=shoulder.Lerp(wrist,.48f)+new Vector3(left?-.10f:.10f,-.11f,.05f);
  var forearm=wrist-elbow;
  var cuff=wrist+forearm.Normalized()*.032f;
  void Segment(int bone,Vector3 start,Vector3 end,float restLength){
   var delta=end-start;
   arm.Sleeve.SetBonePosePosition(bone,start);
   arm.Sleeve.SetBonePoseRotation(bone,new Quaternion(Vector3.Up,delta.Normalized()));
   arm.Sleeve.SetBonePoseScale(bone,new Vector3(1,delta.Length()/restLength,1));
  }
  Segment(0,shoulder,elbow,.42f);
  Segment(1,elbow,cuff,.58f);

  float direction=left?-1:1;
  for(int i=1;i<=4;i++){
   float angle=open*(.56f+i*.05f);
   if(!left&&i==1)angle=.19f-trigger*.29f;
   arm.Hand.SetBonePoseRotation(i,new Quaternion(Vector3.Up,direction*angle));
  }
  arm.Hand.SetBonePoseRotation(5,new Quaternion(Vector3.Forward,direction*open*.24f));
 }

 HeldPose RifleActionPose(HeldPose pose,float reload){
  float draw=1-Ease((float)((heldClock-rifleDrawAt)/RifleDrawDuration));
  float roll=reload<0?0:Ease(Math.Min(reload/.20f,(1-reload)/.27f));
  return new HeldPose(pose.Position+new Vector3(.07f,-.38f,.14f)*draw+new Vector3(-.065f,.078f,-.22f)*roll,
   pose.Angles+new Vector3(-38,-14,19)*draw+new Vector3(14,40,-24)*roll);
 }

 void PoseRifleHands(float reload,float trigger){
  var support=new HeldPose(leftGripRest,new Vector3(-67,0,0));
  var hand=support;
  var magazine=new HeldPose(RifleMagazineSeat,Vector3.Zero);
  float open=0;
  if(reload>=0){
   var extracted=new HeldPose(RifleMagazineSeat+new Vector3(-.057f,-.155f,.025f),new Vector3(9,0,-19));
   var emptyOut=new HeldPose(RifleMagazineSeat+new Vector3(-.16f,-.85f,.07f),new Vector3(12,0,-27));
   var freshIn=new HeldPose(RifleMagazineSeat+new Vector3(-.23f,-.85f,.09f),new Vector3(-11,15,27));
   var liningUp=new HeldPose(RifleMagazineSeat+new Vector3(-.025f,-.135f,.018f),new Vector3(-5,7,15));
   if(reload>=.19f&&reload<.33f)magazine=BlendPose(magazine,extracted,(reload-.19f)/.14f);
   else if(reload>=.33f&&reload<.46f)magazine=BlendPose(extracted,emptyOut,(reload-.33f)/.13f);
   else if(reload>=.46f&&reload<.49f)magazine=BlendPose(emptyOut,freshIn,(reload-.46f)/.03f);
   else if(reload>=.49f&&reload<.62f)magazine=BlendPose(freshIn,liningUp,(reload-.49f)/.13f);
   else if(reload>=.62f&&reload<.77f)magazine=BlendPose(liningUp,magazine,(reload-.62f)/.15f);

   var rotation=Basis.FromEuler(magazine.Angles*Mathf.Pi/180);
   var grasp=new HeldPose(magazine.Position+rotation*RifleMagazineGrip,magazine.Angles+new Vector3(-12,0,7));
   if(reload<.19f)hand=BlendPose(support,grasp,reload/.19f);
   else if(reload<.77f)hand=grasp;
   else hand=BlendPose(grasp,support,(reload-.77f)/.23f);
   open=reload<.19f?Mathf.Sin(reload/.19f*Mathf.Pi):reload>.77f?Mathf.Sin((reload-.77f)/.23f*Mathf.Pi):0;
  }
  rifleMagazine.Position=magazine.Position;rifleMagazine.RotationDegrees=magazine.Angles;
  leftGrip.Position=hand.Position;leftGrip.RotationDegrees=hand.Angles;
  PoseRifleArm(rightHeldArm,rightGrip,false,0,trigger);
  PoseRifleArm(leftHeldArm,leftGrip,true,open,0);
 }
}
