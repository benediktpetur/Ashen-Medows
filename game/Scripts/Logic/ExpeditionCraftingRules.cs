// Ported engine-independent Expedition rules. Rendering and input live in Godot adapters.
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;


[DataContract] sealed class ExpeditionCraftState {
 [DataMember] public int[] Materials=new int[7];[DataMember] public int BenchTier,UnlockedTier=1,Weapon=1,Ammo=6,Reserve=36;[DataMember] public int Water;
 [DataMember] public List<int> Weapons=new List<int>{1};[DataMember] public List<string> Gathered=new List<string>();
 public void Normalize(){if(Materials==null||Materials.Length!=7)Materials=new int[7];if(Weapons==null)Weapons=new List<int>{1};if(Gathered==null)Gathered=new List<string>();Weapons.RemoveAll(id=>id<0||id>5);if(Weapons.Count==0)Weapons.Add(1);if(!Weapons.Contains(Weapon))Weapon=Weapons[0];for(int i=0;i<Materials.Length;i++)Materials[i]=Math.Max(0,Materials[i]);BenchTier=Math.Max(0,Math.Min(5,BenchTier));Ammo=Math.Max(0,Ammo);Reserve=Math.Max(0,Reserve);UnlockedTier=Math.Max(1,Math.Min(5,UnlockedTier));}
 public bool Can(ExpeditionRecipe r){if(UnlockedTier<r.Tier||BenchTier<r.Station)return false;for(int i=0;i<7;i++)if(Materials[i]<r.Cost[i])return false;return true;}
 public bool Spend(ExpeditionRecipe r){if(!Can(r))return false;for(int i=0;i<7;i++)Materials[i]-=r.Cost[i];return true;}
}
sealed class ExpeditionRecipe {
 public string Name,Kind;public int Tier,Station,Result;public int[] Cost;
 public ExpeditionRecipe(string name,string kind,int tier,int station,int result,int wood,int stone,int fiber,int resin,int alloy,int electronics,int bio){Name=name;Kind=kind;Tier=tier;Station=station;Result=result;Cost=new[]{wood,stone,fiber,resin,alloy,electronics,bio};}
 public static readonly ExpeditionRecipe[] All={
 new ExpeditionRecipe("Starter workbench","bench",1,0,1,6,4,0,0,0,0,0),
 new ExpeditionRecipe("Workshop upgrade","bench",2,1,2,8,4,2,5,0,0,0),
 new ExpeditionRecipe("Industrial upgrade","bench",3,2,3,8,6,0,3,6,0,0),
 new ExpeditionRecipe("Advanced upgrade","bench",4,3,4,4,4,0,0,6,6,0),
 new ExpeditionRecipe("Experimental upgrade","bench",5,4,5,0,0,0,0,4,4,6),
 new ExpeditionRecipe("Trail pistol","weapon",1,1,0,4,4,2,0,0,0,0),
 new ExpeditionRecipe("Woodland rifle","weapon",2,2,2,6,3,2,4,0,0,0),
 new ExpeditionRecipe("Industrial carbine","weapon",3,3,3,3,2,0,2,5,0,0),
 new ExpeditionRecipe("Precision rifle","weapon",4,4,4,2,0,2,0,5,4,0),
 new ExpeditionRecipe("Experimental rifle","weapon",5,5,5,0,0,2,0,3,3,5),
 new ExpeditionRecipe("Field chest plate","armor",1,1,1,2,3,6,0,0,0,0),
 new ExpeditionRecipe("Woodland chest plate","armor",2,2,2,2,2,5,4,0,0,0),
 new ExpeditionRecipe("Industrial chest plate","armor",3,3,3,0,2,3,0,5,0,0),
 new ExpeditionRecipe("Advanced chest plate","armor",4,4,4,0,0,3,0,4,4,0),
 new ExpeditionRecipe("Experimental chest plate","armor",5,5,5,0,0,2,0,2,3,5),
 new ExpeditionRecipe("Trail hood","armor",1,1,11,0,0,5,0,0,0,0),
 new ExpeditionRecipe("Work gloves","armor",1,1,12,0,0,4,0,0,0,0),
 new ExpeditionRecipe("Trail boots","armor",1,1,13,2,0,5,0,0,0,0),
 new ExpeditionRecipe("Woodland helmet","armor",2,2,11,0,2,4,3,0,0,0),
 new ExpeditionRecipe("Reinforced gloves","armor",3,3,12,0,0,3,0,3,0,0),
 new ExpeditionRecipe("City patrol boots","armor",4,4,13,0,0,3,0,3,2,0),
 new ExpeditionRecipe("Experimental helmet","armor",5,5,11,0,0,2,0,2,2,3),
 new ExpeditionRecipe("24 rounds","ammo",1,1,24,0,3,2,0,0,0,0),
 new ExpeditionRecipe("Sleeping bag kit","bag",2,2,1,6,0,8,3,0,0,0),
 new ExpeditionRecipe("3 bandages","bandage",1,0,3,0,0,3,0,0,0,0),
 new ExpeditionRecipe("2 trail provisions","food",1,1,2,0,0,4,0,0,0,0),
 new ExpeditionRecipe("Nocturnal weapon tuning (8 catalysts / Crafting 2)","night",3,3,1,0,0,2,3,4,0,0)};
 public static void Check(){var state=new ExpeditionCraftState();state.Materials[0]=6;state.Materials[1]=4;if(!state.Spend(All[0])||state.Materials[0]!=0||state.Spend(All[0]))throw new Exception("Crafting cost error");for(int tier=2;tier<=5;tier++){state=new ExpeditionCraftState{UnlockedTier=tier,BenchTier=tier-1};for(int i=0;i<7;i++)state.Materials[i]=99;state.Materials[tier+1]=0;if(state.Can(All[tier-1]))throw new Exception("Craft tier bypassed exclusive material");state.Materials[tier+1]=99;if(!state.Can(All[tier-1]))throw new Exception("Craft progression inaccessible");state.UnlockedTier=tier-1;if(state.Can(All[tier-1]))throw new Exception("Locked tier accepted");}var copy=Json.Read<ExpeditionCraftState>(Json.Write(state));copy.Normalize();if(copy.Materials.Length!=7)throw new Exception("Craft save mismatch");}
}
