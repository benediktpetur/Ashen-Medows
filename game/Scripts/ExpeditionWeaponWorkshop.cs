using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class MigrationScene {
 string WeaponResearchDescription(int id)=>id switch{28=>"Six-shot 9mm revolver: harder hits than the pistol, slower follow-up shots. Uses existing 9mm ammunition. Its cylinder accepts neither extended magazines nor suppressors.",29=>"Compact automatic 9mm weapon: rapid close-range fire, lower damage per round. Can fit a suppressor, brake and extended magazine.",30=>"Six-shell pump shotgun: six pellets per shell, deliberate cycling and a 45m maximum trace. Takes a fitted stock, not a suppressor or detachable magazine.",31=>"20-round semi-automatic 7.62mm battle rifle: controlled heavy fire. Takes optics, muzzle devices, a stock and larger magazines.",32=>"Compound bow: high-energy arrows and a longer draw cycle. Takes bowstrings and balanced limbs; shares crafted arrows with other bows.",33=>"Two-handed polehammer: long reach and heavy impacts. Grip wraps and a weighted striking head offer stamina or damage choices.",_=>"Permanent "+(ExpeditionGame.IsGun(id)?"firearm":ExpeditionGame.IsBow(id)?"archery":"melee")+" blueprint. Unlock its predecessor, then craft it from this zone's materials. Compatible parts appear in the weapon workshop."};
 void ShowWeaponParts(){
  var b=Panel("WEAPON WORKSHOP / "+ExpeditionGame.GunNames[game.Craft.Weapon].ToUpperInvariant());coopMenu="weaponparts";
  InfoText(b,"Parts belong to your character and remain fitted when you travel or die. One part per slot. Replacements return the old part to your pack. Fit/remove at the required workshop tier; crafting spends materials separately.");
  var weapons=new OptionButton();foreach(int w in game.Craft.Weapons)weapons.AddItem(ExpeditionGame.GunNames[w],w);weapons.Select(game.Craft.Weapons.IndexOf(game.Craft.Weapon));b.AddChild(weapons);weapons.ItemSelected+=i=>{int w=weapons.GetItemId((int)i);if(!CoopRequest("equip",w.ToString()))game.Change(()=>game.Equip(w));BuildGun();ShowWeaponParts();};
  Text(b,$"Workshop {game.Craft.BenchTier} / Damage {game.Damage(false):0} / "+(ExpeditionGame.IsGun(game.Craft.Weapon)?$"Magazine {game.Magazine} / Reload x{game.PartFactor(game.Craft.Weapon,p=>p.Reload):0.00}":$"Attack cycle {(ExpeditionGame.IsBow(game.Craft.Weapon)?game.SoulBowDelay:game.SoulMeleeDelay):0.00}s")+"\n"+(showDetailedInfo?game.WeaponPartSummary(game.Craft.Weapon):string.Join("\n",game.PartsFor(game.Craft.Weapon).Select(p=>p.Slot+": "+p.Name))));
  var scroll=new ScrollContainer{CustomMinimumSize=new(800,330)};b.AddChild(scroll);var list=new VBoxContainer{CustomMinimumSize=new(770,0)};scroll.AddChild(list);
  for(int id=0;id<WeaponPart.All.Length;id++){int part=id;var p=WeaponPart.All[id];if(!p.Fits(game.Craft.Weapon))continue;bool fitted=game.Craft.FittedParts.TryGetValue(game.Craft.Weapon,out var slots)&&slots.Contains(id);string reason=game.PartReason(id,fitted);Text(list,$"T{p.Tier} / {p.Slot.ToUpperInvariant()} / {p.Name} / SPARES {game.Craft.SpareParts.GetValueOrDefault(id)}"+InfoSuffix(p.Description));var row=new HBoxContainer();list.AddChild(row);var fit=new Button{Text=(fitted?"REMOVE":"FIT")+" / "+reason,Disabled=reason!="Ready"||shotClock>0||reloadClock>0};row.AddChild(fit);fit.Pressed+=()=>{string data=part+"|"+(fitted?"remove":"fit");if(!CoopRequest("weaponpart",data))game.FitPart(part,fitted);BuildGun();ShowWeaponParts();};var craft=new Button{Text="RESEARCH / CRAFT PART"};row.AddChild(craft);craft.Pressed+=()=>{researchTab="Weapons and armor";researchFamilyFilter=p.Family;researchChoice=Array.FindIndex(ExpeditionRecipe.All,r=>r.Kind=="weapon-mod"&&r.Result==part);ShowResearchTree();};}
  Button(b,"RESEARCH TREE",ShowResearchTree);Button(b,"BACK",SurvivorInventory);
 }
 void NewWorkshopWeapon(Node3D n,int id,bool hands){
  int basis=id switch{28=>0,29=>3,30=>7,31=>3,32=>18,_=>21};var body=new Node3D{Name="WorkshopWeapon"};n.AddChild(body);ItemWeapon(body,basis,false);
  if(id==28){Tube(body,new(-.06f,.005f,-.12f),new(.06f,.005f,-.12f),.062f,Steel);for(int i=0;i<6;i++){float angle=i*Mathf.Tau/6;Tube(body,new(-.062f,Mathf.Sin(angle)*.04f, -.12f+Mathf.Cos(angle)*.04f),new(-.064f,Mathf.Sin(angle)*.04f,-.12f+Mathf.Cos(angle)*.04f),.012f,Ink);}}
  if(id==29){body.Scale=new(1,1,.7f);Bevel(body,new(0,-.17f,-.15f),new(.055f,.29f,.075f),Steel);Tube(body,new(-.05f,-.02f,.22f),new(-.05f,-.02f,.42f),.012f,Steel);}
  if(id==30){Bevel(body,new(0,-.045f,-.48f),new(.11f,.09f,.24f),Wood);for(int i=0;i<7;i++)Box(body,new(0,-.045f,-.38f-i*.035f),new(.12f,.095f,.008f),Ink);Tube(body,new(0,-.055f,-.3f),new(0,-.055f,-.8f),.019f,Steel);}
  if(id==31){body.Scale=new(1.08f,1.05f,1.18f);Bevel(body,new(0,-.16f,-.12f),new(.075f,.25f,.12f),Steel);Bevel(body,new(0,-.025f,.24f),new(.075f,.13f,.27f),Wood);}
  if(id==32){foreach(int sign in new[]{-1,1}){Tube(body,new(-.15f,sign*.62f,-.36f),new(-.15f,sign*.62f,-.32f),.085f,Ink);Tube(body,new(-.15f,sign*.62f,-.32f),new(-.15f,sign*.62f,-.31f),.055f,Steel);}Tube(body,new(.08f,0,-.35f),new(.08f,0,-.65f),.025f,Ink);}
  if(id==33){body.Scale=new(.85f,1,1.3f);Bevel(body,new(0,.04f,-.98f),new(.33f,.15f,.18f),Steel);Bevel(body,new(.19f,.04f,-.98f),new(.15f,.04f,.06f),Steel);}
  if(hands)RebuiltFirstPersonArms(n,id is 28);
 }
 void WeaponPartModel(Node3D n,int id){var p=WeaponPart.All[id];switch(p.Slot){
  case "Muzzle":Tube(n,Vector3.Zero,new(0,0,-.22f),.035f,Ink);for(int i=0;i<5;i++)Tube(n,new(0,0,-i*.04f),new(0,0,-i*.04f-.008f),.038f,Steel);break;
  case "Optic":Tube(n,Vector3.Zero,new(0,0,-.2f),.035f,Ink);Tube(n,new(0,0,-.201f),new(0,0,-.205f),.03f,new(.19f,.48f,.55f));Bevel(n,new(0,-.04f,-.08f),new(.06f,.04f,.07f),Steel);break;
  case "Magazine":Bevel(n,Vector3.Zero,new(.07f,.25f,.1f),Steel);for(int i=0;i<4;i++)Box(n,new(.036f,-.08f+i*.05f,0),new(.004f,.025f,.07f),Ink);break;
  case "String":for(int i=0;i<5;i++)Tube(n,new(-.02f+i*.01f,-.13f,0),new(-.02f+i*.01f,.13f,0),.004f,Cloth);break;
  case "Chain":for(int i=0;i<8;i++)Bevel(n,new(0,0,-i*.03f),new(.06f,.025f,.02f),Steel);break;
  case "Grip":for(int i=0;i<7;i++)Tube(n,new(-.035f,-.06f+i*.018f,0),new(.035f,-.06f+i*.018f,0),.009f,Cloth);break;
  default:Bevel(n,Vector3.Zero,new(.1f,.09f,.19f),Steel);for(int i=0;i<4;i++)Box(n,new(0,.048f,-.06f+i*.04f),new(.11f,.012f,.008f),Ink);break;
 }}
 void FittedWeaponArt(Node3D target,int weapon){foreach(int id in game.Craft.FittedParts.GetValueOrDefault(weapon)??new()){var part=WeaponPart.All[id];var mount=new Node3D{Name="Fitted"+part.Slot,Position=part.Slot switch{"Muzzle"=>new(0,0,weapon is 0 or 28?-.36f:-.79f),"Optic"=>new(0,.15f,-.1f),"Magazine"=>new(0,-.22f,-.12f),"Stock"=>new(0,-.02f,.3f),"Grip"=>new(0,-.1f,.02f),"String"=>new(-.14f,.35f,-.35f),"Head"=>new(0,.05f,weapon==33?-1.3f:weapon is 14 or 21?-1.0f:-.8f),_=>new(0,.05f,-.65f)}};target.AddChild(mount);WeaponPartModel(mount,id);}}
}
