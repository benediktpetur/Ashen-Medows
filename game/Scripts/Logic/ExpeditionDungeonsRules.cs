// Ported engine-independent Expedition rules. Rendering and input live in Godot adapters.
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;


sealed class ExpeditionDungeon {
 public string Id;public double X,Z;public int Tier;
 public string Name;public bool Deep;
 public static List<ExpeditionDungeon> Nearby(ExpeditionWorld world,double x,double z){var list=new List<ExpeditionDungeon>();string[] ids={"0:0","forest-mine","outskirts-clinic","town-drain","city-hospital","city-sewer","city-tower"};double[] zs={-470,-940,-1400,-1630,-1940,-2140,-2300},offsets={65,-200,-130,180,200,90,-220};string[] names={"WOODLAND SHELTER","OLD FOREST MINE","OUTSKIRTS CLINIC","TOWN DRAIN","CITY HOSPITAL","CITY SEWERS","THE LAST HIGHRISE"};for(int i=0;i<ids.Length;i++){double px=world.RouteX(zs[i])+offsets[i];list.Add(new ExpeditionDungeon{Id=ids[i],X=px,Z=zs[i],Tier=i>=5?5:world.Tier(px,zs[i]),Deep=i>=5,Name=names[i]});}return list;} public static bool Tile(int x,int z){return (x>=-3&&x<3&&z>=-4&&z<2)||(x>=-1&&x<1&&z>=-7&&z<-4)||(x>=-3&&x<4&&z>=-14&&z<-7)||(x>=3&&x<8&&z>=-3&&z<-1)||(x>=8&&x<13&&z>=-5&&z<1);}
 public static bool Free(double x,double z){for(int i=-1;i<=1;i+=2)for(int j=-1;j<=1;j+=2)if(!Tile((int)Math.Floor((x+i*.3)/2),(int)Math.Floor((z+j*.3)/2)))return false;return true;}
 public static readonly double[] ChestX={-4,0,21},ChestZ={-3,-23,-5};
}
[DataContract] sealed partial class ExpeditionRelic {
 [DataMember] public string Name,Origin,Id;[DataMember] public string Slot; public string GearSlot=>string.IsNullOrEmpty(Slot)?"chest":Slot;[DataMember] public int Level,Armor,Vitality;
 public static ExpeditionRelic Roll(int seed,ExpeditionDungeon dungeon,int chest){var random=new Random(unchecked(seed*397+dungeon.Id.GetHashCodeStable()*17+chest*7919));string[] origins={"the Lost Colony","the Old Guard","the Ashen Foundry","the Highway Wardens","the Forgotten Expedition","the Sunken Vault"};string origin=origins[random.Next(origins.Length)];return new ExpeditionRelic{Name="Chest plate of "+origin,Origin=origin,Id="block1/"+seed+"/"+dungeon.Id+"/"+chest,Level=dungeon.Tier,Armor=dungeon.Tier*10+random.Next(0,5),Vitality=dungeon.Tier*3+random.Next(0,3)};}
}
static class DungeonString {public static int GetHashCodeStable(this string text){unchecked{int h=23;foreach(char c in text)h=h*31+c;return h;}}}
[DataContract] sealed class ExpeditionLootSave {
 [DataMember] public ExpeditionLife Life;[DataMember] public string Head,Hands,Feet;
 [DataMember] public List<ExpeditionRelic> Items=new List<ExpeditionRelic>();[DataMember] public List<string> Claimed=new List<string>();[DataMember] public string Equipped;[DataMember] public ExpeditionCraftState Crafting;[DataMember] public ExpeditionSurvivor Survivor;[DataMember] public ExpeditionJournal Journal;[DataMember] public List<ExpeditionWorldChanges> Worlds;
 public bool Claim(ExpeditionRelic item){if(Claimed.Contains(item.Id))return false;Claimed.Add(item.Id);Items.Add(item);return true;}
 public static void Check(){var w=new ExpeditionWorld(431);var a=ExpeditionDungeon.Nearby(w,0,0).Find(d=>d.Id=="0:0");if(a==null||a.Tier!=2||a.Z!=-470)throw new Exception("Starter dungeon missing");for(int level=1;level<=5;level++){var d=new ExpeditionDungeon{Id="test",Tier=level};for(int chest=0;chest<3;chest++){var item=ExpeditionRelic.Roll(431,d,chest);var again=ExpeditionRelic.Roll(431,d,chest);if(item.Level!=level||item.Name!=again.Name||item.Armor!=again.Armor||item.Armor<level*10)throw new Exception("Dungeon loot tier or seed mismatch");}}if(!ExpeditionDungeon.Free(0,2)||!ExpeditionDungeon.Free(0,-12)||!ExpeditionDungeon.Free(0,-23)||!ExpeditionDungeon.Free(21,-5)||ExpeditionDungeon.Free(7,3))throw new Exception("Dungeon traversal error");var save=new ExpeditionLootSave();var loot=ExpeditionRelic.Roll(431,a,0);if(!save.Claim(loot)||save.Claim(loot)||save.Items.Count!=1)throw new Exception("Duplicate dungeon loot");save.Equipped=loot.Id;var copy=Json.Read<ExpeditionLootSave>(Json.Write(save));if(copy.Claim(loot)||copy.Equipped!=loot.Id||copy.Items[0].Level!=2)throw new Exception("Loot persistence error");}
}
