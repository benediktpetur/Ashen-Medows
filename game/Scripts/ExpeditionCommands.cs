using Godot;
using System;

public partial class MigrationScene {
 void ShowCommandPanel(){
  var panel=Panel("SOLO COMMANDS");
  Text(panel,"/godmode or /godmode on — invulnerability, flight and a full inventory.\n/godmode off — resume normal movement and damage. Items are saved.\nWASD to fly, Space to rise, Ctrl to descend, Shift to fly faster.");
  var input=new LineEdit{PlaceholderText="/godmode",MaxLength=64};
  var feedback=new Label{Text=game.GodMode?"God mode is active.":""};
  panel.AddChild(input);
  panel.AddChild(feedback);
  void Submit(string command){
   if(!RunSoloCommand(command)){feedback.Text=game.Notice;return;}
   CloseMenu();
  }
  input.TextSubmitted+=Submit;
  Button(panel,"RUN COMMAND",()=>Submit(input.Text));
  Button(panel,"BACK TO WORLD",CloseMenu);
  input.GrabFocus();
 }

 bool RunSoloCommand(string command){
  if(CoopActive){game.Notice="God mode is available in solo play.";return false;}
  switch(command.Trim().ToLowerInvariant()){
   case "/godmode":
   case "/godmode on":
    if(!game.Change(game.GrantGodModeItems))return false;
    Dismount();
    game.GodMode=true;
    game.Notice="God mode ON / items saved / Space up, Ctrl down";
    break;
   case "/godmode off":
    game.GodMode=false;
    game.Notice="God mode OFF";
    break;
   default:
    game.Notice="Unknown command. Use /godmode or /godmode off.";
    return false;
  }
  player.Velocity=Vector3.Zero;
  return true;
 }
}
