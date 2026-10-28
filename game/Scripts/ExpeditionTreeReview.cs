using Godot;
using System.IO;
using System.Text.Json;
public partial class MigrationScene {
 void PrepareTreeReview(){
  string audit=ProjectSettings.GlobalizePath("res://../../../Ashen Meadows - 3D Batch/Model Audit.json");
  using var doc=JsonDocument.Parse(File.ReadAllText(audit));int count=0;
  foreach(var row in doc.RootElement.GetProperty("queue").EnumerateArray()){
   string relative=row.GetProperty("relative").GetString();
   if(!relative.StartsWith("Nature/Trees/Oaks/")||row.GetProperty("status").GetString()!="complete")continue;
   var state=new GltfState();var loader=new GltfDocument();
   if(loader.AppendFromFile(row.GetProperty("model").GetString(),state)!=Error.Ok)continue;
   var scene=loader.GenerateScene(state);var packed=new PackedScene();packed.Pack(scene);scene.Free();
   string key="tree-review/"+Path.GetFileNameWithoutExtension(relative);batchPaths[key]="review";batchScenes[key]=packed;if(++count>=12)break;
  }
 }
}
