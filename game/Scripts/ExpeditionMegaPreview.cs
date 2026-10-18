using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 async void CaptureMegaBosses(){try{
  game.NoDiskSave=true;ClearEnemies();CloseMenu();game.World.Changes.Time=900;game.World.Changes.CampaignBosses=new(){0,1,2};game.World.Changes.MillDefeated=true;game.Craft.UnlockedTier=5;game.Career.Level=10;
  EnterDungeon(HeartDungeon());foreach(var label in dungeonRoot.FindChildren("*","Label3D",true,false).OfType<Label3D>())label.Visible=false;ClearEnemies();paused=true;weaponRoot.Visible=false;hudRoot.Visible=false;var overlay=new CanvasLayer();AddChild(overlay);var caption=new Label{Position=new(36,28)};caption.AddThemeFontSizeOverride("font_size",28);overlay.AddChild(caption);var subtitle=new Label{Position=new(38,68)};subtitle.AddThemeFontSizeOverride("font_size",16);overlay.AddChild(subtitle);player.Position=new(3,.1f,0);var view=new Camera3D{Current=true,Fov=55};AddChild(view);
  var light=new OmniLight3D{Position=new(1,4,-4),LightColor=new(1,.82f,.61f),LightEnergy=3.3f,OmniRange=18};AddChild(light);
  var rim=new OmniLight3D{Position=new(-6,4,-14),LightColor=new(.26f,.56f,.72f),LightEnergy=3,OmniRange=16};AddChild(rim);
  for(int j=0;j<3;j++){int v=new[]{2,0,1}[j];ClearEnemies();string id=$"boss/campaign/{game.Save.Seed}/2/mega/900/{v}";AddEnemy(id,0,-10,5);var boss=infected.Single(e=>e.Id==id);boss.BossState=v==2?"volley-windup":v==0?"rupture-windup":"hatch-windup";boss.BossTime=2.5;boss.BossStep=1;boss.BossAim=new(2,0,-1);boss.Body.LookAt(new Vector3(3,0,0));
   view.Position=new(4,2.7f,-3.4f);view.LookAt(boss.Body.Position+new Vector3(0,1.65f,0));caption.Text=MegaNames[v].ToUpperInvariant();subtitle.Text=v==2?"TIER 5 / WEAPONS / SCRAP VOLLEYS + EXPLOSIVE CANISTERS":v==0?"TIER 5 / ARMOR / BREAK THE SHELL, DODGE THE CHARGE":"TIER 5 / COMMUNITY / DESTROY NESTS TO WEAKEN THE HORDE";PoseMega(boss);
   if(v==1){MegaNests(boss);for(int i=0;i<4;i++){string add=id+"/wave/1/"+i;AddEnemy(add,-4+i*2,-13-i%2*2,2);var e=infected.FirstOrDefault(e=>e.Id==add);if(e!=null){e.Body.LookAt(new Vector3(2,0,0));AnimateCharacter(e);}}}
   DrawCampaign();await ArtShot(j==0?"mega-01-iron-mourner":j==1?"mega-02-sump-colossus":"mega-03-brood-matriarch");
  }
  GD.Print("PASS / three mega-boss in-engine screenshots");GetTree().Quit();
 }catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}}
}
