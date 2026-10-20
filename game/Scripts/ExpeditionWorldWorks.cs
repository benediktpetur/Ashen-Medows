using Godot;
using System;
using System.Linq;
public partial class MigrationScene {

 void WorkAction(string data){if(CoopClient||!ContentEnabled)return;var bits=data.Split(':');if(bits.Length!=2||!int.TryParse(bits[1],out int site)||WorldWork.At(site)==null)return;
  if(!game.NearWork(site,player.Position.X,player.Position.Z)){game.Notice="Return to the site's notice board";return;}
  if(bits[0]=="open"){if(CoopHost&&coopActor>0)RpcId(coopActor,nameof(CoopWorkPage),site);else ShowWorldWork(site);return;}
  if(!FieldReady(site)){game.Notice="Clear the site's guards and restore its original equipment first";return;}
  game.Save.X=player.Position.X;game.Save.Z=player.Position.Z;
  if(bits[0] is "restore" or "strip")game.BeginWork(site,bits[0]=="strip");
  else if(bits[0]=="collect")game.CollectWork(site);
  else if(bits[0]=="use"){
   if((dogPackSource??infected).Any(e=>!Peaceful(e.Id)&&Distance(e.Body.Position,player.Position)<20)){game.Notice="Threats are too close for treatment or rest";return;}game.UseWork(site);
  }else return;
  game.Persist();FeverSync();DrawWorldWorks();
 }
 [Rpc(MultiplayerApi.RpcMode.Authority,TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]void CoopWorkPage(int site){if(CoopClient&&WorldWork.At(site)!=null)ShowWorldWork(site);}
 void TickWorldWorks(double dt){
  game.SupplyWorks(dt);bool changed=false;
  foreach(var s in game.World.Changes.Works.Where(s=>s.Choice>0&&!s.Finished).ToArray()){
   var p=ExpeditionContentSite.Place(game.World,s.Site);
   if(!MillPlayers().Any(a=>ExpeditionGame.Distance(a.Body.Position.X,a.Body.Position.Z,p.X,p.Z)<24)||!FieldReady(s.Site))continue;
   s.Remaining=Math.Max(0,s.Remaining-dt);
   if(s.Remaining==0){game.FinishWork(s);changed=true;if(WorldWork.At(s.Site).Kind=="trail"&&s.Choice==1)RebuildTerrain();}
   else if(WorldWork.At(s.Site).Tier>=2){s.Noise-=dt;if(s.Noise<=0){s.Noise=8;HearNoise(Ground(p.X,p.Z),45);}}
  }
  if(changed){game.Persist();FeverSync();}DrawWorldWorks();
 }
 string WorkCost(WorldWork w,bool strip)=>string.Join(" / ",w.Cost(strip).Select((n,i)=>(n,i)).Where(x=>x.n>0).Select(x=>$"{x.n} {ExpeditionGame.ResourceNames[x.i]}"));
 void ShowWorldWork(int site){var w=WorldWork.At(site);if(w==null)return;var s=game.World.Changes.Work(site);var b=Panel("THE WORLD YOU LEAVE / "+w.Name.ToUpperInvariant());Text(b,ExpeditionContentSite.All[site].Name+" / T"+w.Tier+" project\n"+w.Reason);
  if(w.Town>=0)Text(b,"COMMUNITY AFFECTED / "+ExpeditionGame.SettlementNames[w.Town]);Text(b,FieldReady(site)?"SITE SECURED / ready for a project":"SECURE THIS SITE FIRST / clear guards and restore its original equipment");Text(b,"1. Secure the site and restore its original equipment.\n2. Choose which future to build; pay for the work.\n3. Stay within 24m for 45 active seconds to restore, or 20 to dismantle. Tier 2+ work makes noise. Leaving pauses work; it does not erase supplies.");
  void Act(string title,string op,bool enabled=true){var button=new Button{Text=title,Disabled=!enabled,CustomMinimumSize=new(0,38)};b.AddChild(button);button.Pressed+=()=>{CloseMenu();if(!CoopRequest("works",op+":"+site))WorkAction(op+":"+site);};}
  if(s.Choice==0){Text(b,"RESTORE / "+w.Restore+"\nCost: "+WorkCost(w,false)+"\n"+game.WorkReason(site,false));Text(b,"STRIP / "+w.Strip+"\nCost: "+WorkCost(w,true)+"\n"+game.WorkReason(site,true));Text(b,"Committing spends your materials and locks this site's choice for the whole world. Where a community is linked, restoring earns +5 trust and stripping loses 6. Existing story cases and quests remain separate. You may leave without penalty.");Act("COMMIT / RESTORE THE SITE","restore",FieldReady(site)&&game.WorkReason(site,false)=="Ready");Act("COMMIT / STRIP FOR SUPPLIES","strip",FieldReady(site)&&game.WorkReason(site,true)=="Ready");}
  else{Text(b,(s.Choice==1?"RESTORATION":"DISMANTLING")+" / "+(s.Finished?"COMPLETE":$"{s.Remaining:0}s of nearby work remaining")+"\n"+s.Outcome);if(s.Finished){bool stocked=s.Stock.Any(n=>n>0)||s.Medicine>0;if(stocked||s.Choice==1&&w.Kind is "garden" or "pantry" or "depot" or "water" or "clinic"){Text(b,"SHARED STORES / "+(stocked?"":"Nothing ready yet. Production takes ten active minutes. ")+string.Join(" / ",s.Stock.Select((n,i)=>(n,i)).Where(v=>v.n>0).Select(v=>$"{v.n} {(v.i<7?ExpeditionGame.ResourceNames[v.i]:new[]{"food","clean water","bandages","fuel"}[v.i-7])}")));if(s.Medicine>0)Text(b,$"Medical kits / {s.Medicine}");Act("COLLECT SHARED STORES","collect",stocked);}if(s.Choice==1&&w.Kind is "refuge" or "clinic")Act("USE "+(w.Kind=="refuge"?"REFUGE":"TREATMENT STATION"),"use");}}
  Text(b,"Ecology changes affect future normal spawns within 85m. Existing enemies, bosses and story encounters remain dangerous. Active-time production has storage caps; it does not accrue while the server is shut down.");Text(b,game.Notice);Button(b,"WORLD CHANGES / JOURNAL",ShowWorldWorksLedger);Button(b,"LEAVE",CloseMenu);
 }
 void ShowWorldWorksLedger(){var b=Panel("THE WORLD YOU LEAVE / WORLD CHANGES");Text(b,"These changes belong to this world and are shared by your party. Unvisited projects remain undiscovered. Read notice boards at abandoned sites to find opportunities.");var scroll=new ScrollContainer{CustomMinimumSize=new(890,420)};b.AddChild(scroll);var list=new VBoxContainer{CustomMinimumSize=new(870,0)};scroll.AddChild(list);
  foreach(var w in WorldWork.All){if(!Field(w.Site).Seen&&!Field(w.Site).Read&&!game.World.Changes.Works.Any(s=>s.Site==w.Site&&s.Choice>0))continue;var s=game.World.Changes.Work(w.Site);var p=ExpeditionContentSite.Place(game.World,w.Site);Text(list,$"{w.Name.ToUpperInvariant()} / {ExpeditionContentSite.All[w.Site].Name} / T{w.Tier}\n{ExpeditionGame.Distance(player.Position.X,player.Position.Z,p.X,p.Z):0}m / {p.X:0}, {p.Z:0}\n"+(s.Choice==0?"UNDECIDED / secure, supply and choose a future":s.Finished?s.Outcome:$"WORK PAUSED OR IN PROGRESS / {s.Remaining:0}s remaining"));LocationButton(list,w.Name,p.X,p.Z,ShowWorldWorksLedger);}
  Text(b,$"{game.World.Changes.Works.Count(s=>s.Finished)} / {WorldWork.All.Length} sites permanently changed.\nRestored services and shared reserves can make a return journey worthwhile.");Button(b,"BACK TO JOURNAL",ShowJournal);Button(b,"BACK TO WORLD",CloseMenu);
 }
}
