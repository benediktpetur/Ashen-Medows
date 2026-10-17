using Godot;
using System;
using System.Linq;

public partial class MigrationScene {
 Label campaignBossLabel,journeyLabel;ProgressBar campaignHealth;Node3D campaignWarning;string campaignWarningKey="";
 void DrawCampaign(){
  if(journeyLabel==null){journeyLabel=HudLabel(12);journeyLabel.Position=new(970,72);}
  var clears=game.World.Changes.CampaignBosses;
  journeyLabel.Text=!game.World.Changes.MillDefeated?"JOURNEY / PREPARE FOR THE FOREMAN":!clears.Contains(0)?"JOURNEY / THE WRECKER":!clears.Contains(1)?"JOURNEY / THE BELLKEEPER":!clears.Contains(2)?"JOURNEY / THE BROODHEART":"JOURNEY / SOUTH BANK SECURED";
  if(campaignBossLabel==null){campaignBossLabel=HudLabel(16);campaignBossLabel.Position=new(380,160);campaignHealth=new(){Position=new(380,212),Size=new(520,12),ShowPercentage=false,MaxValue=1};hudRoot.AddChild(campaignHealth);campaignHealth.AddThemeStyleboxOverride("background",new StyleBoxFlat{BgColor=new(.08f,.10f,.09f)});campaignHealth.AddThemeStyleboxOverride("fill",new StyleBoxFlat{BgColor=new(.64f,.22f,.12f)});}
  var boss=infected.FirstOrDefault(e=>IsCampaignBoss(e.Id)&&Distance(e.Body.Position,player.Position)<75);
  campaignBossLabel.Visible=campaignHealth.Visible=boss!=null;
  if(boss==null){if(GodotObject.IsInstanceValid(campaignWarning))campaignWarning.Visible=false;return;}
  int index=ExpeditionCampaign.BossIndex(boss.Id);
  string cue=boss.BossState switch{"ram-windup"=>"RAM / dodge sideways; wrecks stop him","ram"=>"CHARGING!","sweep-windup"=>"SWEEP / leave the orange ring","alarm-windup"=>"ALARM / reinforcements incoming","knell-windup"=>"FALLING DEBRIS / leave the target circle","brood-windup"=>"BROOD HATCH / clear the worms","bile-windup"=>"BILE / move away from the target circle","rupture-windup"=>"RUPTURE / leave the green ring","recover"=>"EXPOSED / 50% bonus damage",_=>boss.Hp<boss.MaxHp*.5?"ENRAGED / faster warnings":"Watch the wind-up. Save stamina for a dodge."};
  campaignBossLabel.Text=$"{ExpeditionCampaign.Names[index].ToUpperInvariant()}  {boss.Hp:0}/{boss.MaxHp:0}\n{cue}";campaignHealth.Value=boss.Hp/boss.MaxHp;
  string key=boss.Id+"/"+boss.BossState+"/"+boss.BossStep;
  if(key!=campaignWarningKey){
   campaignWarningKey=key;if(GodotObject.IsInstanceValid(campaignWarning))campaignWarning.QueueFree();campaignWarning=new();AddChild(campaignWarning);
   if(boss.BossState=="ram-windup"){
    var dir=boss.BossAim-boss.Body.Position;dir.Y=0;dir=dir.Normalized();
    var lane=Box(campaignWarning,dir*9+Vector3.Up*.08f,new(3,.05f,18),new(.8f,.22f,.06f));lane.Rotation=new(0,Mathf.Atan2(-dir.X,-dir.Z),0);
   }else if(boss.BossState.EndsWith("windup")||boss.BossState=="recover")MillRing(campaignWarning,boss.BossState=="rupture-windup"?6.5f:boss.BossState=="sweep-windup"?5:boss.BossState is "knell-windup" or "bile-windup"?3.6f:2.2f,boss.BossState=="recover"?new(.23f,.75f,.83f,.9f):boss.BossState is "rupture-windup" or "brood-windup"?new(.5f,.8f,.13f,.9f):new Color(.9f,.48f,.08f,.9f));
  }
  campaignWarning.Visible=true;campaignWarning.Position=boss.BossState is "knell-windup" or "bile-windup"?boss.BossAim:boss.Body.Position;
  if(boss.Visual.HasNode("Torso")){
   PoseFigure(boss.Visual,(float)game.Clock*3,boss.BossState=="hunt"?.4f:0,true);
   foreach(string side in new[]{"Left","Right"})boss.Visual.GetNode<Node3D>(side+"Arm").RotationDegrees=new(boss.BossState.EndsWith("windup")?-120:boss.BossState=="recover"?10:-20,0,side=="Left"?-20:20);
  }
 }
}
