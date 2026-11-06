using Godot;
using System;
using System.IO;
using System.Linq;

public partial class MigrationScene {
 string SavesDirectory=>ProjectSettings.GlobalizePath("user://expedition");
 Action<ExpeditionGame> startSelectedExpedition;

 void ShowStartupPicker(Action<ExpeditionGame> start){
  startSelectedExpedition=start;
  Input.MouseMode=Input.MouseModeEnum.Visible;
  InitTheme();
  ui=new CanvasLayer();
  AddChild(ui);
  hudRoot=new Control();
  ui.AddChild(hudRoot);
  ShowCharacterPicker();
 }

 void ShowCharacterPicker(){
  var panel=Panel("ASHEN MEADOW / CHOOSE CHARACTER");
  Text(panel,"Choose a survivor or start a new expedition. Each survivor has their own progress and worlds.");
  var feedback=new Label();
  string legacy=Path.Combine(SavesDirectory,"profile.json");
  if(File.Exists(legacy))Button(panel,"EXISTING SURVIVOR",()=>PickCharacter(legacy,feedback));
  string slots=Path.Combine(SavesDirectory,"characters");
  try{
   if(Directory.Exists(slots)){
    foreach(string path in Directory.EnumerateFiles(slots,"*.json").OrderByDescending(File.GetLastWriteTimeUtc)){
     string name;
     try{
      name=Json.Read<ExpeditionGameSave>(File.ReadAllText(path))?.CharacterName;
     }catch{ name=null; }
     string label=string.IsNullOrWhiteSpace(name)?"Unreadable character ("+Path.GetFileNameWithoutExtension(path)+")":name;
     Button(panel,label,()=>PickCharacter(path,feedback));
    }
   }
  }catch(Exception ex){feedback.Text="Could not list characters / "+ex.Message;}

  panel.AddChild(new HSeparator());
  var nameInput=new LineEdit{PlaceholderText="New survivor name",MaxLength=32};
  panel.AddChild(nameInput);
  void Create(string text){
   string name=text.Trim();
   if(name.Length==0){feedback.Text="Enter a name for your survivor.";return;}
   string path=Path.Combine(slots,Guid.NewGuid().ToString("N")+".json");
   var character=new ExpeditionGame(path);
   character.Save.CharacterName=name;
   character.Save.Buildings.Find(world=>world.Seed==character.Save.Seed).Name="Briar Lane";
   if(!character.Persist()){feedback.Text=character.Notice;return;}
   ShowWorldPicker(character);
  }
  nameInput.TextSubmitted+=Create;
  Button(panel,"CREATE CHARACTER",()=>Create(nameInput.Text));
  panel.AddChild(feedback);
  Button(panel,"QUIT",()=>GetTree().Quit());
 }

 void PickCharacter(string path,Label feedback){
  var character=new ExpeditionGame(path);
  if(!character.Healthy){feedback.Text=character.Notice;return;}
  ShowWorldPicker(character);
 }

 void ShowWorldPicker(ExpeditionGame character){
  string name=character.Save.CharacterName??"Existing Survivor";
  var panel=Panel("CHOOSE WORLD / "+name);
  Text(panel,"Inventory and skills travel with this survivor. Each world keeps its own buildings and position.");
  var feedback=new Label();
  int[] seeds=character.SavedWorldSeeds();
  void Open(int seed,string newName=null){
   if(!character.SelectSavedWorld(seed,newName)){feedback.Text=character.Notice;return;}
   CloseMenu();
   RemoveChild(ui);
   ui.QueueFree();
   ui=null;
   hudRoot=null;
   menuShade=null;
   startSelectedExpedition(character);
   startSelectedExpedition=null;
  }
  foreach(int seed in seeds){
   string last=seed==character.Save.Seed?" / LAST PLAYED":"";
   Button(panel,character.SavedWorldName(seed)+" / SEED "+seed+last,()=>Open(seed));
  }
  panel.AddChild(new HSeparator());
  var nameInput=new LineEdit{PlaceholderText="New world name (optional)",MaxLength=32};
  var seedInput=new SpinBox{MinValue=1,MaxValue=999999,Value=Random.Shared.Next(1,1000000),Prefix="Seed"};
  panel.AddChild(nameInput);
  panel.AddChild(seedInput);
  Button(panel,"CREATE WORLD & PLAY",()=>{
   int seed=(int)seedInput.Value;
   if(seeds.Contains(seed)){feedback.Text="This survivor already has a world with that seed.";return;}
   string worldName=nameInput.Text.Trim();
   Open(seed,worldName.Length==0?"World "+seed:worldName);
  });
  panel.AddChild(feedback);
  Button(panel,"BACK TO CHARACTERS",ShowCharacterPicker);
 }
}
