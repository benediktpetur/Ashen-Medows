using System;
using System.Runtime.Serialization;

sealed partial class ExpeditionCraftState {
 [DataMember] public int AmmoVersion=1;
 [DataMember] public int[] AmmoStocks=new int[5],Magazines=new int[ExpeditionGame.GunNames.Length];
 public void NormalizeAmmo(){
  if(AmmoStocks==null||AmmoStocks.Length!=5)AmmoStocks=new int[5];
  if(Magazines==null)Magazines=new int[ExpeditionGame.GunNames.Length];else if(Magazines.Length!=ExpeditionGame.GunNames.Length)Array.Resize(ref Magazines,ExpeditionGame.GunNames.Length);
  if(AmmoVersion==0){Array.Clear(AmmoStocks);Array.Clear(Magazines);if(!ExpeditionGame.IsGun(Weapon)){Reserve+=Ammo;Ammo=0;}AmmoVersion=1;}
  for(int i=0;i<AmmoStocks.Length;i++)AmmoStocks[i]=Math.Max(0,AmmoStocks[i]);
  for(int i=0;i<Magazines.Length;i++)Magazines[i]=Math.Max(0,Magazines[i]);
  CommitAmmo();
 }
 public int AmmoClass=>ExpeditionGame.AmmoClassFor(Weapon);
 public void CommitAmmo(){if(AmmoStocks==null||Magazines==null)return;AmmoStocks[AmmoClass]=Math.Max(0,Reserve);if(ExpeditionGame.IsGun(Weapon)&&Weapon<Magazines.Length)Magazines[Weapon]=Math.Max(0,Ammo);}
 public int Stock(int type)=>type==AmmoClass?Reserve:AmmoStocks[type];
 public void GiveAmmo(int type,int amount){CommitAmmo();AmmoStocks[type]=Math.Max(0,AmmoStocks[type]+amount);if(type==AmmoClass)Reserve=AmmoStocks[type];}
 public bool SpendAmmo(int type,int amount){if(amount<0||Stock(type)<amount)return false;GiveAmmo(type,-amount);return true;}
 public void LoadAmmo(){Ammo=ExpeditionGame.IsGun(Weapon)?Magazines[Weapon]:0;Reserve=AmmoStocks[AmmoClass];}
}
sealed partial class ExpeditionGame {
 public static readonly string[] AmmoNames={"9mm pistol rounds","7.62mm rifle rounds","5.56mm carbine rounds","12-gauge shells","Energy cells"};
 public static int AmmoClassFor(int weapon)=>weapon is 1 or 2 or 4?1:weapon==3?2:weapon==7?3:weapon==5?4:0;
 public static bool IsPoweredSaw(int id)=>id is 17 or 27;
 public static bool IsSword(int id)=>id is >=24 and <=26;
 public bool HasChainPower=>Craft.Fuel>0&&Craft.Stock(4)>0;
}
