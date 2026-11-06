using System.Linq;
sealed partial class ExpeditionGame {
 static ExpeditionRelic WorkshopArmor(ExpeditionRecipe r,string id)=>new ExpeditionRelic{Id=id,Name=r.Name,Origin="Player workshop",Slot=r.Result==11?"head":r.Result==12?"hands":r.Result==13?"feet":"chest",Level=r.Tier,Armor=r.Result>=11?r.Tier*3:r.Tier*10+2,Vitality=r.Result>=11?r.Tier:r.Tier*3};
 public string GearId(string slot)=>slot switch{"head"=>Profile.Head,"hands"=>Profile.Hands,"feet"=>Profile.Feet,_=>Profile.Equipped};
 public bool IsGearEquipped(string id)=>Profile.Items.Any(i=>i.Id==id&&GearId(i.GearSlot)==id);
 public int GearArmor=>Profile.Items.Where(i=>IsGearEquipped(i.Id)).Sum(i=>i.Armor);
 public int GearVitality=>Profile.Items.Where(i=>IsGearEquipped(i.Id)).Sum(i=>i.Vitality);
 public void EquipGear(string id){var item=Profile.Items.Find(i=>i.Id==id);if(item==null||!string.IsNullOrEmpty(item.SoulOwner)&&item.SoulOwner!=Life.SoulOwner)return;switch(item.GearSlot){case "head":Profile.Head=id;break;case "hands":Profile.Hands=id;break;case "feet":Profile.Feet=id;break;default:Profile.Equipped=id;break;}Life.Health=System.Math.Min(Life.Health,MaxHealth);}
 public void UnequipGear(string slot){switch(slot){case "head":Profile.Head=null;break;case "hands":Profile.Hands=null;break;case "feet":Profile.Feet=null;break;case "chest":Profile.Equipped=null;break;}Life.Health=System.Math.Min(Life.Health,MaxHealth);}
}
