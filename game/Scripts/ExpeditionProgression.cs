using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 static readonly string[] SkillNames={"STRENGTH","AGILITY","STAMINA","MARKSMANSHIP","CRAFTING","SURVIVAL"};
 static readonly string[] SkillTraining={"Chop trees / 80 XP per tree","Sprint / 2 XP per second","Sprint / 2 XP per second","Hit enemies / 4 XP, headshots / 8 XP, kills / 18 XP","Craft recipes / 12 XP per recipe tier","Gather resources / 15 XP; search supplies / 20 XP"};
 ExpeditionSkill[] LocalSkills()=>new[]{game.Skills.Strength,game.Skills.Agility,game.Skills.Stamina,game.Skills.Marksmanship,game.Skills.Crafting,game.Skills.Survival};
 Label[] skillReadouts;ProgressBar[] skillBars;Label levelToast;int[] seenSkillLevels;double levelToastUntil;
 string SkillBenefit(int i,int level){int n=level-1;return i switch{
  0=>$"Tree cutting: {Math.Max(2,4-n/5)} swings. T3 hardwood at level 3; T4 at level 5.",
  1=>$"Run speed +{Math.Min(20,n)*.5:0.#}% / maximum stamina +{n*2}.",
  2=>$"Maximum stamina +{n*5} from this skill.",
  3=>$"Weapon damage +{n}% from training.",
  4=>"T3 guns need level 2; T4 need level 3; T5 need level 4. Night tuning needs level 2.",
  _=>$"Natural health recovery +{Math.Min(20,n)*2}% / improved provision finding."
 };}
 void ShowProgression(){
  var box=Panel("SURVIVOR / LEARN BY DOING");menu.Position=new(40,45);menu.Size=new(1200,610);
  var intro=new Label{Text=$"HEALTH {game.Life.Health:0}/{game.MaxHealth:0}  /  STAMINA {game.Stamina:0}/{game.MaxStamina:0}  /  ARMOR {game.GearArmor}  /  RUN x{game.Speed:0.00}  /  DAMAGE {game.Damage(false):0}"};box.AddChild(intro);
  var grid=new GridContainer{Columns=2,SizeFlagsHorizontal=Control.SizeFlags.ExpandFill};grid.AddThemeConstantOverride("h_separation",18);grid.AddThemeConstantOverride("v_separation",12);box.AddChild(grid);
  skillReadouts=new Label[6];skillBars=new ProgressBar[6];var skills=LocalSkills();
  for(int i=0;i<6;i++){
   var panel=new PanelContainer{CustomMinimumSize=new(565,142),SizeFlagsHorizontal=Control.SizeFlags.ExpandFill};grid.AddChild(panel);
   var margin=new MarginContainer();foreach(var k in new[]{"margin_left","margin_right","margin_top","margin_bottom"})margin.AddThemeConstantOverride(k,12);panel.AddChild(margin);var card=new VBoxContainer();margin.AddChild(card);
   skillReadouts[i]=new Label();skillReadouts[i].AddThemeFontSizeOverride("font_size",19);skillReadouts[i].AddThemeColorOverride("font_color",new Color(.87f,.76f,.49f));card.AddChild(skillReadouts[i]);
   skillBars[i]=new ProgressBar{MaxValue=1,ShowPercentage=false,CustomMinimumSize=new(0,8)};card.AddChild(skillBars[i]);
   card.AddChild(new Label{Text=SkillTraining[i]});card.AddChild(new Label{Text=SkillBenefit(i,skills[i].Level),AutowrapMode=TextServer.AutowrapMode.WordSmart,CustomMinimumSize=new(530,35)});
  }
  box.AddChild(new Label{Text=$"DEATH / All six skills lose only their current XP. Earned levels stay.  •  Deaths: {game.Skills.Deaths}\nSkills save with your portable character, including when you join a friend's world. Maximum skill level: 100."});
  Button(box,"BACK TO EXPEDITION",CloseMenu);UpdateProgression();
 }
 void UpdateProgression(){UpdateWoundPanel();
  var skills=LocalSkills();
  if(seenSkillLevels==null)seenSkillLevels=skills.Select(s=>s.Level).ToArray();
  for(int i=0;i<6;i++){
   if(skills[i].Level>seenSkillLevels[i]){
    if(levelToast==null){levelToast=new Label{Position=new(410,90),MouseFilter=Control.MouseFilterEnum.Ignore};levelToast.AddThemeFontSizeOverride("font_size",24);levelToast.AddThemeColorOverride("font_color",new Color(.96f,.83f,.48f));hudRoot.AddChild(levelToast);}
    levelToast.Text=$"LEVEL UP / {SkillNames[i]} {skills[i].Level}\n{SkillBenefit(i,skills[i].Level)}";levelToastUntil=Time.GetTicksMsec()/1000.0+6;
   }
   seenSkillLevels[i]=skills[i].Level;
   if(skillReadouts!=null&&GodotObject.IsInstanceValid(skillReadouts[i])&&skillReadouts[i].IsInsideTree()){
    skillReadouts[i].Text=$"{SkillNames[i]}   /   LV {skills[i].Level:00}   /   "+(skills[i].Level>=100?"MASTERED":$"{skills[i].Xp:0} / {skills[i].Need:0} XP");skillBars[i].Value=skills[i].Progress;
   }
  }
  if(levelToast!=null)levelToast.Visible=Time.GetTicksMsec()/1000.0<levelToastUntil;
 }
 async void CaptureProgression(){await CaptureSurvivalSystems();}
}
