using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 void ShowWorldLedger(){var b=Panel("THE WATERLINE / NAMES, SUPPLIES AND PROMISES");Text(b,"Communities remember what reached their tables. A repaired filter is permanent. Stolen food comes out of someone's ration. Your own camp is only one of the places trying to survive.");var sc=new ScrollContainer{CustomMinimumSize=new(880,400)};b.AddChild(sc);var rows=new VBoxContainer{CustomMinimumSize=new(850,0)};sc.AddChild(rows);foreach(var t in game.World.Changes.Settlements){Text(rows,$"{ExpeditionGame.SettlementNames[t.Index]} / {(t.Fallen?"EVACUATED":t.Visits==0?"NOT YET MET":$"TRUST {t.Trust} / FOOD {t.Food} / WATER {t.Water}")} / {(int)t.X}, {(int)t.Z}\n"+(t.Visits>0?t.LastEvent:"A marked survivor camp. Visit to learn what they need."));if(t.Visits>0){int index=t.Index;Text(rows,$"Development {t.Development}/3 / {t.StoryResult}");Button(rows,"STORY / "+ExpeditionGame.SettlementNames[index],()=>ShowSettlementStory(index));}}Text(rows,"YOUR WORLD RECORD");foreach(var entry in game.Profile.Journal.Seen.Where(s=>s.StartsWith("world/")).TakeLast(18))Text(rows,entry.Substring(6));Button(b,"BACK TO CAMP",()=>ShowCommunity());}
 void RemoveLivingActor(Infected e){var local=infected;try{infected=dogPackSource??infected;RemoveEnemy(e);}finally{infected=local;}}
 void GuardShot(Infected guard,Infected target){
  var query=PhysicsRayQueryParameters3D.Create(guard.Body.Position+Vector3.Up*1.3f,target.Body.Position+Vector3.Up*.8f);query.Exclude=PursuitExclusions();if(GetWorld3D().DirectSpaceState.IntersectRay(query).Count>0)return;
  target.Hp-=25;guard.Cooldown=2;ReactEnemyHit(target,false);CheckpointGunshot(guard.Body.Position);
  if(target.Hp<=0){if(IsSettlementActor(target.Id)||IsSettlementMission(target.Id)){SettlementCombatDeath(target);return;}if(InfectedKind(target)=="bloater")QueueBrood(target);game.Skills.Defeated.Add(target.Id);RemoveLivingActor(target);}
 }
}
