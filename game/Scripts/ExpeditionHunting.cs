using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
public partial class MigrationScene {
 readonly Dictionary<string,Node3D> huntingCarcasses=new();
 int huntingSeed=int.MinValue;
 bool AtHuntingFire=>dungeon==null&&(game.Building.Pieces.Any(p=>p.Def.Function=="cook"&&p.Health>0&&Distance(player.Position,new((float)p.X,(float)p.Y,(float)p.Z))<4)||Community.Established&&Community.Improvements>=2&&Distance(player.Position,CommunityFirePoint)<4);
 AnimalCarcass NearCarcass()=>dungeon==null?game.World.Changes.Carcasses.Where(c=>!c.Harvested&&game.World.Changes.Time-c.At<=1800&&Distance(player.Position,new((float)c.X,(float)c.Y,(float)c.Z))<3).OrderBy(c=>ExpeditionGame.Distance(c.X,c.Z,player.Position.X,player.Position.Z)).FirstOrDefault():null;
 string HuntingPrompt(){var c=NearCarcass();return c==null?"":string.IsNullOrEmpty(c.Worker)?"F / HARVEST "+c.Species.ToUpperInvariant()+" / "+ExpeditionGame.HuntingToolName(game.Craft.HuntingTool)+" / 3 seconds":"HARVESTING / "+c.Progress.ToString("0.0")+" / 3s";}
 bool HuntingInteract(){var c=NearCarcass();if(c==null)return false;if(!CoopRequest("hunting","harvest|"+c.Id))HuntingAction("harvest|"+c.Id);return true;}
 bool HuntingSight(AnimalCarcass c){var from=player.Position+Vector3.Up*1.3f;var to=new Vector3((float)c.X,(float)c.Y+.35f,(float)c.Z);var q=PhysicsRayQueryParameters3D.Create(from,to);q.Exclude=new(){player.GetRid()};return GetWorld3D().DirectSpaceState.IntersectRay(q).Count==0;}
 void HuntingAction(string data){if(CoopClient||dungeon!=null||data.Length>220)return;var bits=data.Split('|');if(bits.Length!=2)return;
  if(bits[0]=="tool"){if(int.TryParse(bits[1],out int tool))game.SelectHuntingTool(tool);}
  else if(bits[0]=="craft"){if(int.TryParse(bits[1],out int recipe))game.CraftHunting(recipe,AtHuntingFire);}
  else if(bits[0]=="harvest"){var c=game.World.Changes.Carcasses.FirstOrDefault(c=>c.Id==bits[1]);string why=game.HarvestReason(c,player.Position.X,player.Position.Z);if(why!="Ready"){game.Notice=why;return;}if(Distance(player.Position,new((float)c.X,(float)c.Y,(float)c.Z))>3||!HuntingSight(c)){game.Notice="Move beside the carcass with a clear view";return;}
   if(!string.IsNullOrEmpty(c.Worker)&&c.Worker!=CommunityOwner&&CommunityLeader(c.Worker)!=null){game.Notice="Another survivor is harvesting this animal";return;}if(c.Worker==CommunityOwner)return;c.Worker=CommunityOwner;c.HarvestTool=game.Craft.HuntingTool;c.Progress=0;game.Notice="Harvesting / stay beside the animal for 3 seconds";game.Persist();
  }
  if(CoopHost&&coopActors.Count>0)Rpc(nameof(CoopWorldUpdate),Json.Write(WorldSnapshot()));
 }
 void TickHunting(double dt){if(CoopClient||dungeon!=null)return;foreach(var c in game.World.Changes.Carcasses.Where(c=>!c.Harvested&&!string.IsNullOrEmpty(c.Worker)).ToArray()){
  var leader=CommunityLeader(c.Worker);if(leader==null||leader.Value.Game.Life.Health<=0||Distance(leader.Value.Body.Position,new((float)c.X,(float)c.Y,(float)c.Z))>3||game.World.Changes.Time-c.At>1800){c.Worker="";c.Progress=0;continue;}
  c.Progress+=dt*(c.HarvestTool==1?1+Math.Max(0,leader.Value.Game.Craft.ToolLevels[3]-1)*.3:1);if(c.Progress<3)continue;var actor=CoopHost?coopActors.Values.FirstOrDefault(a=>a.Identity==c.Worker):null;
  void Harvest(){if(HuntingSight(c))game.HarvestAnimal(c.Id,player.Position.X,player.Position.Z);c.Worker="";c.Progress=0;}
  if(actor!=null)WithCoopActor(actor,Harvest);else Harvest();
  if(CoopHost&&coopActors.Count>0)Rpc(nameof(CoopWorldUpdate),Json.Write(WorldSnapshot()));
 }
 }
 void DrawHuntingCarcasses(){if(game==null)return;if(huntingSeed!=game.Save.Seed){foreach(var root in huntingCarcasses.Values)if(GodotObject.IsInstanceValid(root))root.QueueFree();huntingCarcasses.Clear();huntingSeed=game.Save.Seed;}
  foreach(var pair in huntingCarcasses.ToArray()){var c=game.World.Changes.Carcasses.FirstOrDefault(c=>c.Id==pair.Key);if(c==null||c.Harvested||dungeon!=null||game.World.Changes.Time-c.At>1800||Distance(player.Position,pair.Value.Position)>140){pair.Value.QueueFree();huntingCarcasses.Remove(pair.Key);}}
  if(dungeon!=null)return;foreach(var c in game.World.Changes.Carcasses.Where(c=>!c.Harvested&&game.World.Changes.Time-c.At<=1800)){if(huntingCarcasses.ContainsKey(c.Id)||Distance(player.Position,new((float)c.X,(float)c.Y,(float)c.Z))>110)continue;var root=new Node3D{Name="HarvestableAnimal",Position=new((float)c.X,(float)c.Y+.20f,(float)c.Z),RotationDegrees=new(0,(float)c.Yaw,90)};AddChild(root);AnimalArt(root,c.Species);huntingCarcasses[c.Id]=root;}
 }
 void ShowHunting(){var b=Panel("HUNTING / NOTHING WASTED");coopMenu="hunting";PackLabel(b,"Hunt meadow rabbits and deer. Woodland boars and wolves are tier 2 threats. Press F beside a carcass and remain nearby for three seconds. Cook raw meat at your campfire; infected dogs cannot provide safe materials.",14);
  PackLabel(b,string.Join(" / ",Enumerable.Range(0,6).Select(i=>$"{ExpeditionGame.HuntNames[i]} {game.Craft.HuntStock[i]}")),14);
  PackLabel(b,"Skinning knife: 200% hide/fur, 50% meat/bone. Butcher cleaver: 200% meat/bone, 50% hide/fur. Sinew stays at 100%. Hides tan into leather. Half units accumulate across hunts. One tool per carcass, locked when you start.",14);
  for(int mode=0;mode<3;mode++){int selected=mode;var choose=new Button{Text=(game.Craft.HuntingTool==mode?"SELECTED / ":"SELECT / ")+ExpeditionGame.HuntingToolName(mode),Disabled=!game.HasHuntingTool(mode)};b.AddChild(choose);choose.Pressed+=()=>{if(!CoopRequest("hunting","tool|"+selected))HuntingAction("tool|"+selected);ShowHunting();};}
  var nearby=NearCarcass();if(nearby!=null)PackLabel(b,"Expected / "+ExpeditionGame.HarvestPreview(nearby,string.IsNullOrEmpty(nearby.Worker)?game.Craft.HuntingTool:nearby.HarvestTool),14);
  PackLabel(b,"Partial materials / "+string.Join(" / ",Enumerable.Range(0,6).Where(i=>game.Craft.HuntHalves[i]>0).Select(i=>"0.5 "+ExpeditionGame.HuntNames[i])),12);
  foreach(var pair in HuntingRecipe.All.Select((r,i)=>(r,i))){int index=pair.i;var r=pair.r;string cost=string.Join(" / ",Enumerable.Range(0,6).Where(i=>r.Stock[i]>0).Select(i=>$"{r.Stock[i]} {ExpeditionGame.HuntNames[i]}"));if(r.Product=="skinning-knife"||r.Product=="harvest-cleaver")cost+=" / 3 stone";if(r.Wood>0)cost+=$" / {r.Wood} timber";if(r.Fiber>0)cost+=$" / {r.Fiber} fiber";if(r.Resin>0)cost+=$" / {r.Resin} forest resin";
   PackLabel(b,$"T{r.Tier} / {r.Name} / {cost}",14);string reason=game.HuntingReason(index,AtHuntingFire);var button=new Button{Text=reason=="Ready"?"CRAFT / "+r.Name:reason,Disabled=reason!="Ready",CustomMinimumSize=new(0,36)};b.AddChild(button);button.Pressed+=()=>{if(!CoopRequest("hunting","craft|"+index))HuntingAction("craft|"+index);ShowHunting();};
  }PackLabel(b,"Hides become leather; sinew becomes cord; bones become arrowheads and binding paste. Fur clothing adds real warmth when equipped. Carcasses spoil after 30 active minutes. Collected hunting materials travel with your character.",13);PackLabel(b,game.Notice,13);Button(b,"BACK TO WORKSHOP",ResearchWorkshop);Button(b,"BACK TO WORLD",CloseMenu);
 }
}
