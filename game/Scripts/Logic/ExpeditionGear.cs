using System.Linq;
sealed partial class ExpeditionGame {
 public string GearId(string slot)=>slot switch{"head"=>Profile.Head,"hands"=>Profile.Hands,"feet"=>Profile.Feet,_=>Profile.Equipped};
 public bool IsGearEquipped(string id)=>Profile.Items.Any(i=>i.Id==id&&GearId(i.GearSlot)==id);
 public int GearArmor=>Profile.Items.Where(i=>IsGearEquipped(i.Id)).Sum(i=>i.Armor);
 public int GearVitality=>Profile.Items.Where(i=>IsGearEquipped(i.Id)).Sum(i=>i.Vitality);
 public void EquipGear(string id){var item=Profile.Items.Find(i=>i.Id==id);if(item==null)return;switch(item.GearSlot){case "head":Profile.Head=id;break;case "hands":Profile.Hands=id;break;case "feet":Profile.Feet=id;break;default:Profile.Equipped=id;break;}Life.Health=System.Math.Min(Life.Health,MaxHealth);}
 public void UnequipGear(string slot){switch(slot){case "head":Profile.Head=null;break;case "hands":Profile.Hands=null;break;case "feet":Profile.Feet=null;break;case "chest":Profile.Equipped=null;break;}Life.Health=System.Math.Min(Life.Health,MaxHealth);}
}
