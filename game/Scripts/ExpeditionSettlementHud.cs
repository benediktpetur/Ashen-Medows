using Godot;
using System;
using System.Linq;
public partial class MigrationScene {
 Label settlementObjectiveHud;
 void DrawSettlementObjectiveHud(){if(!LivingEnabled)return;if(settlementObjectiveHud==null){settlementObjectiveHud=HudLabel(12);settlementObjectiveHud.Size=new(300,115);settlementObjectiveHud.AutowrapMode=TextServer.AutowrapMode.WordSmart;}settlementObjectiveHud.Position=new(GetViewport().GetVisibleRect().Size.X-330,225);settlementObjectiveHud.Modulate=new(.94f,.74f,.40f);string text="";if(Ledger.RaidScheduled&&!Ledger.RaidOver)text=Ledger.RaidStarted?"BLACK LEDGER / CAMP UNDER ATTACK\nDefeat four armed raiders before they overrun it.":$"BLACK LEDGER / RETALIATION\n{Math.Max(0,Ledger.RaidDue-game.World.Changes.Time)/60:0.0} active minutes to prepare. Marked on M.";else if(Ledger.Due>0&&Ledger.State is "debt" or "offer" or "assault" or "infiltration" or "burning")text=$"BLACK LEDGER / {Ledger.State.ToUpperInvariant()}\n{Math.Max(0,Ledger.Due-game.World.Changes.Time)/60:0} active minutes / F7 > Waterline > Story";else{var next=game.World.Changes.Settlements.Where(t=>!t.Fallen&&t.StoryStage>0&&t.StoryStage<4&&t.StoryDue>0).OrderBy(t=>t.StoryDue).FirstOrDefault();if(next!=null)text=$"{StoryTitle(next.Index)} / CHAPTER {next.StoryStage}\n{Math.Max(0,next.StoryDue-game.World.Changes.Time)/60:0} active minutes / F7 > Waterline > Story";}settlementObjectiveHud.Text=text;
 }
}
