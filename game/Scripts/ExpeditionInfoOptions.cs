using Godot;
using System;
public partial class MigrationScene {
 bool showDetailedInfo=true,showFirstPersonHands=true;
 float viewFov=75,viewMotion=.7f,viewDistance=0;
 const string InfoConfig="user://interface.cfg";
 void LoadInfoOptions(){var config=new ConfigFile();if(config.Load(InfoConfig)!=Error.Ok)return;handStyle=Math.Clamp(config.GetValue("view","hand_style",1).AsInt32(),0,2);showPlayerBody=config.GetValue("view","body",true).AsBool();showDetailedInfo=config.GetValue("interface","detailed_info",true).AsBool();showFirstPersonHands=config.GetValue("view","hands",true).AsBool();viewFov=Math.Clamp((float)config.GetValue("view","fov",75).AsDouble(),65,100);viewMotion=Math.Clamp((float)config.GetValue("view","motion",.7).AsDouble(),0,1);viewDistance=Math.Clamp((float)config.GetValue("view","distance",0).AsDouble(),0,.20f);}
 void SaveViewOptions(){var config=new ConfigFile();config.Load(InfoConfig);config.SetValue("view","hand_style",handStyle);config.SetValue("view","body",showPlayerBody);config.SetValue("view","hands",showFirstPersonHands);config.SetValue("view","fov",viewFov);config.SetValue("view","motion",viewMotion);config.SetValue("view","distance",viewDistance);if(config.Save(InfoConfig)!=Error.Ok)game.Notice="Could not save view options";}

 void SaveInfoOption(bool enabled){showDetailedInfo=enabled;var config=new ConfigFile();config.Load(InfoConfig);config.SetValue("interface","detailed_info",enabled);if(config.Save(InfoConfig)!=Error.Ok)game.Notice="Could not save interface options";}
 string InfoSuffix(string description,string separator="\n")=>showDetailedInfo?separator+description:"";
 Label InfoLabel(VBoxContainer box,string description,int font=13){var label=PackLabel(box,description,font);label.Visible=showDetailedInfo;return label;}
 void InfoText(VBoxContainer box,string description){if(showDetailedInfo)Text(box,description);}
 void ShowOptions(){
  var b=Panel("OPTIONS / VIEW, INTERFACE & AUDIO");
  void ViewSlider(string title,float min,float max,float step,float value,Action<float> apply,string suffix){var label=new Label();b.AddChild(label);var slider=new HSlider{MinValue=min,MaxValue=max,Step=step,Value=value,CustomMinimumSize=new(860,30)};b.AddChild(slider);void Change(float amount){label.Text=title+" / "+amount.ToString("0")+suffix;apply(amount);}Change(value);slider.ValueChanged+=amount=>{Change((float)amount);SaveViewOptions();};}
  ViewSlider("FIELD OF VIEW",65,100,1,viewFov,v=>viewFov=v,"°");
  ViewSlider("WEAPON DISTANCE",0,20,1,viewDistance*100,v=>viewDistance=v/100," cm further away");
  ViewSlider("WEAPON SWAY",0,100,5,viewMotion*100,v=>viewMotion=v/100,"%");
  var style=new OptionButton();style.AddItem("Natural bare hands");style.AddItem("Fingerless gloves");style.AddItem("Full gloves");style.Select(handStyle);b.AddChild(style);style.ItemSelected+=index=>{handStyle=(int)index;SaveViewOptions();BuildHeldTool();};
  var body=new CheckBox{Text="Show player body when looking down",ButtonPressed=showPlayerBody};b.AddChild(body);body.Toggled+=visible=>{showPlayerBody=visible;SaveViewOptions();};
  var hands=new CheckBox{Text="Show hands and sleeves",ButtonPressed=showFirstPersonHands};b.AddChild(hands);hands.Toggled+=v=>{showFirstPersonHands=v;SaveViewOptions();};
  b.AddChild(new HSeparator());
  var info=new CheckBox{Text="Show detailed info",ButtonPressed=showDetailedInfo,CustomMinimumSize=new(0,42)};b.AddChild(info);
  Text(b,"Show explanations for skills, talents, equipment, research and weapon parts. Turn off for a cleaner view. Names, stats, costs and requirements stay visible.");
  var preview=new Label{AutowrapMode=TextServer.AutowrapMode.WordSmart,CustomMinimumSize=new(880,70)};b.AddChild(preview);
  void Refresh(){preview.Text="PREVIEW\nStrength"+InfoSuffix("Improves tree cutting and unlocks harder wood."," — ");}Refresh();
  info.Toggled+=enabled=>{SaveInfoOption(enabled);Refresh();};
  Button(b,"MUSIC VOLUME",ShowMusicSettings);Button(b,"BACK",ShowPause);
 }
 async void CaptureInfoOptions(){try{ShowOptions();await ArtShot("info-01-options");showDetailedInfo=true;ShowProgression();await ArtShot("info-02-detailed");showDetailedInfo=false;ShowProgression();await ArtShot("info-03-compact");GetTree().Quit();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}
