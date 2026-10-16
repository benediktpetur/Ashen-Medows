using Godot;
using System;
using System.IO;
using System.Threading.Tasks;
static class ExpeditionNewSystemsChecks {
 internal static void Run(){
  var skill=new ExpeditionSkill();skill.Gain(360);MigrationChecks.Assert(skill.Level==3&&skill.Xp==10,"Multiple skill levels / XP carry");skill.Gain(double.PositiveInfinity);MigrationChecks.Assert(skill.Xp==10,"Nonfinite XP");skill.Level=99;skill.Gain(20000);MigrationChecks.Assert(skill.Level==100&&skill.Xp==0&&skill.Progress==1,"Mastery cap");
  var life=new ExpeditionLife();life.Wounds.Add(new ExpeditionWound());life.TickWounds(180);life.Patch();life.TickWounds(600);MigrationChecks.Assert(life.Wounds[0].Blood==420&&life.Wounds[0].Recovery==600,"Bandage pauses blood countdown");life.Unpatch(0);life.TickWounds(20);MigrationChecks.Assert(life.Wounds[0].Blood==400,"Removing bandage resumes remaining blood");life.Patch();life.TickWounds(1199);MigrationChecks.Assert(life.Wounds.Count==1,"Full uninterrupted bandage healing");var saved=Json.Read<ExpeditionLife>(Json.Write(life));saved.TickWounds(1);MigrationChecks.Assert(saved.Wounds.Count==0,"Wound timer save / final healing");life.Wounds.Clear();life.Wounds.Add(new ExpeditionWound());life.Wounds.Add(new ExpeditionWound());MigrationChecks.Assert(life.TickWounds(300),"Two bites fatal in five minutes");
  var path=ProjectSettings.GlobalizePath("user://tests/gear-"+Guid.NewGuid()+".json");try{var g=new ExpeditionGame(path);g.Profile.Items.Add(new ExpeditionRelic{Id="old",Armor=12,Vitality=3});g.EquipGear("old");foreach(string slot in new[]{"head","hands","feet"}){g.Profile.Items.Add(new ExpeditionRelic{Id=slot,Slot=slot,Armor=3,Vitality=1});g.EquipGear(slot);}MigrationChecks.Assert(g.GearArmor==21&&g.MaxHealth==106,"Distinct gear slots and legacy chest migration");g.Persist();var copy=new ExpeditionGame(path);MigrationChecks.Assert(copy.GearArmor==21,"Gear slot persistence");copy.UnequipGear("head");MigrationChecks.Assert(copy.GearArmor==18&&copy.Profile.Equipped=="old","Unequip affects only its slot");}finally{if(File.Exists(path))File.Delete(path);if(File.Exists(path+".bak"))File.Delete(path+".bak");}
  GD.Print("PASS / skill cap, XP carry, bandage pause/resume/20-minute healing, saved wound timers, fatal multi-bite timing, gear slots and compatibility");
 }
}
public partial class MigrationScene {
 async Task CollisionGameplayChecks(){
  var body=new StaticBody3D{Position=player.Position+new Vector3(0,1,-2)};body.AddChild(new CollisionShape3D{Shape=new BoxShape3D{Size=new(4,3,.3f)}});AddChild(body);await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
  var from=player.Position+Vector3.Up;var ray=PhysicsRayQueryParameters3D.Create(from,from+Vector3.Forward*4);ray.Exclude=new Godot.Collections.Array<Rid>{player.GetRid()};var hit=GetWorld3D().DirectSpaceState.IntersectRay(ray);MigrationChecks.Assert(hit.Count>0&&hit["collider"].AsGodotObject()==body,"Physical wall blocks weapon / tool ray");
  var collision=player.MoveAndCollide(Vector3.Forward*4);MigrationChecks.Assert(collision!=null&&collision.GetCollider()==body,"Player capsule cannot cross solid wall");player.Position-=Vector3.Forward*.1f;body.Free();
  var tree=props.Find(p=>p.Kind==0&&p.Tier<=2);MigrationChecks.Assert(tree!=null,"Collision tree fixture");var previous=player.Position;player.Position=tree.Position+new Vector3(1.5f,0,0);camera.LookAt(tree.Position+Vector3.Up*1.2f);await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);MigrationChecks.Assert(AimedTree()==tree,"Aimed axe ray hits trunk collider");camera.RotateY(Mathf.Pi);MigrationChecks.Assert(AimedTree()!=tree,"Axe cannot hit tree behind survivor");player.Position=previous;camera.Rotation=Vector3.Zero;GD.Print("PASS / wall hitscan occlusion, player capsule collision, aimed trunk collision and rear-target rejection");
 }
}
