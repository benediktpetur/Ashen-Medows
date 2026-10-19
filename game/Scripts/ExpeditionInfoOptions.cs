using Godot;
using System;
public partial class MigrationScene {
 bool showDetailedInfo=true;
 const string InfoConfig="user://interface.cfg";
 void LoadInfoOptions(){var config=new ConfigFile();if(config.Load(InfoConfig)==Error.Ok)showDetailedInfo=(bool)config.GetValue("interface","detailed_info",true);}
 void SaveInfoOption(bool enabled){showDetailedInfo=enabled;var config=new ConfigFile();config.Load(InfoConfig);config.SetValue("interface","detailed_info",enabled);if(config.Save(InfoConfig)!=Error.Ok)game.Notice="Could not save interface options";}
 string InfoSuffix(string description,string separator="\n")=>showDetailedInfo?separator+description:"";
 Label InfoLabel(VBoxContainer box,string description,int font=13){var label=PackLabel(box,description,font);label.Visible=showDetailedInfo;return label;}
 void InfoText(VBoxContainer box,string description){if(showDetailedInfo)Text(box,description);}
 void ShowOptions(){
  var b=Panel("OPTIONS / INTERFACE & AUDIO");
  var info=new CheckBox{Text="Show detailed info",ButtonPressed=showDetailedInfo,CustomMinimumSize=new(0,42)};b.AddChild(info);
  Text(b,"Show explanations for skills, talents, equipment, research and weapon parts. Turn off for a cleaner view. Names, stats, costs and requirements stay visible.");
  var preview=new Label{AutowrapMode=TextServer.AutowrapMode.WordSmart,CustomMinimumSize=new(880,70)};b.AddChild(preview);
  void Refresh(){preview.Text="PREVIEW\nStrength"+InfoSuffix("Improves tree cutting and unlocks harder wood."," — ");}Refresh();
  info.Toggled+=enabled=>{SaveInfoOption(enabled);Refresh();};
  Button(b,"MUSIC VOLUME",ShowMusicSettings);Button(b,"BACK",ShowPause);
 }
 async void CaptureInfoOptions(){try{ShowOptions();await ArtShot("info-01-options");showDetailedInfo=true;ShowProgression();await ArtShot("info-02-detailed");showDetailedInfo=false;ShowProgression();await ArtShot("info-03-compact");GetTree().Quit();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}
