using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 void SurfaceLayoutChecks(){
  var w=game.World;int checkedLots=0;
  for(int ix=-37;ix<103;ix++)for(int iz=-78;iz<63;iz++){
   var lot=w.Residential(ix,iz);if(lot==null)continue;
   double yaw=lot.Yaw*Math.PI/180,x=lot.X+Math.Sin(yaw)*10,z=lot.Z+Math.Cos(yaw)*10;
   MigrationChecks.Assert(w.ResidentialApproach(x,z)&&w.SurfaceClearance(x,z,2),"House approach connects to its facing atlas street / "+lot.Id);checkedLots++;
  }
  MigrationChecks.Assert(checkedLots>100,"Residential approach coverage");
  int roads=0;for(int x=-2300;x<6500;x+=80)for(int z=-4900;z<3900;z+=80)if(w.StreetDistance(x,z)<0){MigrationChecks.Assert(w.MainRoad(x,z),"Shared atlas road clearance");roads++;}MigrationChecks.Assert(roads>100,"Atlas street network is present");
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
