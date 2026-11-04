using Godot;
using System;
using System.Linq;

public partial class MigrationScene {
 int storageQuantity=1;
 static string StorageName(int i)=>i<7?ExpeditionGame.ResourceNames[i]:i<10?new[]{"Trail provisions","Drinking water","Bandages"}[i-7]:ExpeditionGame.AmmoNames[i-10];
 static string StorageGlyph(int i)=>i<7?new[]{"timber","stone","fiber","resin","alloy","electronics","bio"}[i]:i<10?new[]{"food","water","bandage"}[i-7]:"ammo";
 int BackpackStock(int i)=>i<7?game.Craft.Materials[i]:i==7?game.Life.Food:i==8?game.Craft.Water:i==9?game.Life.Bandages:game.Craft.Stock(i-10);
 void StorageInventory(VBoxContainer box,BuildPiece p){
  Text(box,"Drag an item to the opposite container, or click it to transfer. Choose how much to move below.\nShared storage / food spoils every 30 active minutes unless refrigerated and powered.");
  var toolbar=new HBoxContainer();box.AddChild(toolbar);
  foreach(int amount in new[]{1,10,int.MaxValue}){int n=amount;var choice=new Button{Text=n==int.MaxValue?"FULL STACK":"MOVE "+n,ToggleMode=true,ButtonPressed=storageQuantity==n,CustomMinimumSize=new(130,34)};toolbar.AddChild(choice);choice.Pressed+=()=>{storageQuantity=n;ShowBaseStation(p);};}
  Text(box,game.Notice);
  var columns=new HBoxContainer();columns.AddThemeConstantOverride("separation",16);box.AddChild(columns);
  void Move(int index,bool fromStorage){string data=p.Id+"|"+(fromStorage?"withdraw":"deposit")+"|"+index+"|"+storageQuantity;if(!CoopRequest("baseuse",data))UseBlueprint(data);var current=game.Building.Pieces.FirstOrDefault(x=>x.Id==p.Id);if(current!=null)ShowBaseStation(current);else CloseMenu();}
  void Container(bool stored){
   var side=new VBoxContainer{CustomMinimumSize=new(434,0),SizeFlagsHorizontal=Control.SizeFlags.ExpandFill};columns.AddChild(side);
   int occupied=Enumerable.Range(0,15).Count(i=>(stored?p.Stock[i]:BackpackStock(i))>0);
   PackLabel(side,(stored?p.Def.Name.ToUpperInvariant():"BACKPACK")+" / "+occupied+" STACKS",17);
   PackLabel(side,stored?"Capacity: "+(200*p.Def.Tier)+" per item type":"Materials, provisions, water, dressings and reserve ammunition",11);
   var target=new StorageDropZone{Stored=stored,Transfer=Move,CustomMinimumSize=new(434,290)};side.AddChild(target);
   var grid=new GridContainer{Columns=5,MouseFilter=Control.MouseFilterEnum.Pass};target.AddChild(grid);
   for(int i=0;i<15;i++){int index=i,count=stored?p.Stock[i]:BackpackStock(i);var card=new StorageItemSlot{ItemIndex=i,Stored=stored,Amount=count,Text="\n\n"+StorageName(i)+"\nx"+count,CustomMinimumSize=new(83,90),TooltipText=StorageName(i)+" / "+count+(stored?" stored":" carried")+"\nClick or drag to "+(stored?"withdraw":"store"),AutowrapMode=TextServer.AutowrapMode.WordSmart,Transfer=Move};card.AddThemeFontSizeOverride("font_size",10);card.Modulate=count>0?Colors.White:new Color(.55f,.59f,.55f);grid.AddChild(card);card.AddChild(new InventoryGlyph{Kind=StorageGlyph(i),Position=new(28,6),Size=new(28,28),MouseFilter=Control.MouseFilterEnum.Ignore});card.Pressed+=()=>{if(count>0)Move(index,stored);};}
  }
  Container(false);Container(true);
  Text(box,"Equipment and specialist supplies remain in your backpack. Open the backpack to equip gear or use supplies.");
  Button(box,"VIEW FULL BACKPACK",SurvivorInventory);
 }
}
internal partial class StorageDropZone:PanelContainer {
 internal bool Stored;internal Action<int,bool> Transfer;
 public override bool _CanDropData(Vector2 position,Variant data)=>StorageItemSlot.Accept(data,Stored);
 public override void _DropData(Vector2 position,Variant data){var d=data.AsGodotDictionary();Transfer((int)d["item"],(bool)d["stored"]);}
}
internal partial class StorageItemSlot:Button {
 internal int ItemIndex,Amount;internal bool Stored;internal Action<int,bool> Transfer;
 internal static bool Accept(Variant data,bool stored){if(data.VariantType!=Variant.Type.Dictionary)return false;var d=data.AsGodotDictionary();return d.ContainsKey("storage-item")&&d.ContainsKey("item")&&d.ContainsKey("stored")&&(bool)d["stored"]!=stored;}
 public override Variant _GetDragData(Vector2 position){if(Amount<=0)return default;var preview=new Label{Text=TooltipText.Split('\n')[0]};SetDragPreview(preview);return new Godot.Collections.Dictionary{{"storage-item",true},{"item",ItemIndex},{"stored",Stored}};}
 public override bool _CanDropData(Vector2 position,Variant data)=>Accept(data,Stored);
 public override void _DropData(Vector2 position,Variant data){var d=data.AsGodotDictionary();Transfer((int)d["item"],(bool)d["stored"]);}
}
