using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 void SurfaceLayoutChecks(){
  var w=game.World;int checkedLots=0;
  for(int ix=-20;ix<20;ix++)for(int iz=-3;iz<3;iz++){
   var lot=w.Residential(ix,iz);if(lot==null)continue;
   double street=(ix&1)==0?ix*64:(ix+1)*64;
   double x=street+Math.Sign(lot.X-street)*12;
   MigrationChecks.Assert(w.ResidentialApproach(x,lot.Z)&&w.SurfaceClearance(x,lot.Z,2),"House approach connects to its facing street / "+lot.Id);checkedLots++;
  }
  MigrationChecks.Assert(checkedLots>100,"Residential approach coverage");
  foreach(int z in new[]{-700,-1450,-2100})MigrationChecks.Assert(w.MainRoad(w.RouteX(z),z)&&!w.MainRoad(w.RouteX(z)+12,z),"Continuous main road and bounded width");
  MigrationChecks.Assert(w.UrbanStreet(64,-1478)&&w.UrbanStreet(93,-1472)&&w.UrbanStreet(32,-2100),"Industrial service roads and city block streets");
  foreach(int i in Enumerable.Range(0,8)){
   var house=SuburbHouseRoot(i);AddChild(house);HouseFoundation(house,new(12.2f,11.2f),false);
   var mesh=house.GetNode<MeshInstance3D>("ContinuousFoundation");var box=(BoxMesh)mesh.Mesh;
   for(int a=-1;a<=1;a+=2)for(int b=-1;b<=1;b+=2){var p=house.Transform*new Vector3(a*6,0,b*5.5f);float terrain=(float)w.Height(p.X,p.Z)-house.Position.Y;
    MigrationChecks.Assert(mesh.Position.Y-box.Size.Y/2<terrain&&mesh.Position.Y+box.Size.Y/2<=0,"Continuous foundation reaches terrain at every corner / "+i);
   }
   MigrationChecks.Assert(mesh.FindChildren("*","CollisionShape3D",true,false).Count+house.FindChildren("*","CollisionShape3D",true,false).Count>0,"Foundation has physical support");house.Free();
  }
  GD.Print("PASS / surface layout / connected house approaches / industrial and city streets / eight continuous foundations with ground contact");
 }
}
