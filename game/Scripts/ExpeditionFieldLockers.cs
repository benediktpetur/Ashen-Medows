using Godot;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Collections.Generic;

sealed partial class ContentSiteState {
 [DataMember] public bool LockerOpened,SearchLoud;
 [DataMember] public double SearchRemaining;
 [DataMember] public string SearchOwner="";
}
public partial class MigrationScene {
 bool journeyFixture;bool JourneyEnabled=>journeyFixture||!testing&&!coopCheck&&coopCheckRole==null;
 bool HasFieldLocker(int i)=>ExpeditionContentSite.All[i].Tier>=2&&ExpeditionContentSite.All[i].Theme is 1 or 2 or 3 or 5 or 6 or 11;
 string LockerContents(int i)=>ExpeditionContentSite.All[i].Theme switch{1=>ExpeditionContentSite.All[i].Tier>=3?"Salvaged field optic and arrows":"High-tension bowstring and arrows",2=>"Sterile dressings and medical supplies",3=>ExpeditionContentSite.All[i].Tier>=3?"Drive salvage, fuel and an extended magazine":"Fuel and charcoal-yard supplies",5=>ExpeditionContentSite.All[i].Tier>=3?"Relay salvage and a field optic":"Relay salvage and a fitted shoulder stock",6=>"Ammunition and transport salvage",_=>"Security muzzle brake and ammunition"};
 void DressFieldLocker(Node3D root,int i){if(!HasFieldLocker(i))return;var box=new Node3D{Name="SpecialistSupplyLocker",Position=new(3,.3f,7)};root.AddChild(box);Bevel(box,new(0,.4f,0),new(1.1f,.8f,.65f),ExpeditionContentSite.All[i].Theme==2?new(.61f,.64f,.56f):new Color(.26f,.34f,.31f));for(int j=0;j<4;j++)Box(box,new(0,.2f+j*.14f,-.34f),new(.96f,.025f,.03f),Steel);Box(box,new(.3f,.4f,-.36f),new(.1f,.14f,.04f),new(.68f,.54f,.28f));}
 bool FieldLockerAction(int i,bool loud){if(!JourneyEnabled||!HasFieldLocker(i))return false;var s=Field(i);if(s.LockerOpened){game.Notice="The specialist locker is empty.";return false;}if(s.SearchRemaining>0){game.Notice="Someone is already searching this locker.";return false;}if(!FieldReady(i)){game.Notice="Clear the site and restore its equipment before opening the specialist locker.";return false;}s.SearchOwner=game.Life.SoulOwner;s.SearchRemaining=loud?3:20;s.SearchLoud=loud;if(!loud)Noise(6);if(loud){Noise(70);EnemySound(player.Position,2);}game.Notice=loud?"Shutter forced / stay near the site for 3 seconds. That was loud.":"Carefully opening the shutter / stay within 6m of the entrance for 20 seconds.";FeverSync();return true;}
 void TickFieldLockers(double dt){if(!JourneyEnabled||CoopClient)return;foreach(var s in game.World.Changes.FieldSites.Where(s=>s.SearchRemaining>0).ToArray()){
  var person=MillPlayers().FirstOrDefault(p=>p.Game.Life.SoulOwner==s.SearchOwner);var prop=props.FirstOrDefault(p=>p.Kind==36&&p.Material==s.Index);
  if(person.Game==null||person.Game.Life.Health<=0||dungeon!=null||prop==null||Distance(person.Body.Position,prop.Position)>6){s.SearchRemaining=0;s.SearchOwner="";continue;}
  s.SearchRemaining=Math.Max(0,s.SearchRemaining-dt);if(s.SearchRemaining>0)continue;if(s.LockerOpened)continue;
  s.LockerOpened=true;RewardFieldLocker(person.Game,s.Index);person.Game.Notice="Specialist supplies recovered / "+LockerContents(s.Index);person.Game.Persist();FeverSync();
 }}
 void RewardFieldLocker(ExpeditionGame owner,int i){var d=ExpeditionContentSite.All[i];void Part(int id)=>owner.Craft.SpareParts[id]=owner.Craft.SpareParts.GetValueOrDefault(id)+1;
  switch(d.Theme){case 1:Part(d.Tier>=3?5:8);owner.Craft.Arrows+=12;break;case 2:owner.Life.Bandages+=4;owner.Life.Medicine++;break;case 3:owner.Craft.Fuel+=6;owner.Craft.Materials[Math.Min(6,d.Tier+1)]+=4;if(d.Tier>=3)Part(6);break;case 5:owner.Craft.Materials[Math.Min(6,d.Tier+1)]+=3;Part(d.Tier>=3?5:7);break;case 6:owner.Craft.GiveAmmo(2,24);owner.Craft.Materials[4]+=4;break;default:Part(4);owner.Craft.GiveAmmo(1,18);break;}
  owner.JourneyLoot(i);
 }
}
