using Godot;
using System;
public partial class MigrationScene {
 string locationPinName=""; double locationPinX,locationPinZ; int locationPinSeed; Action locationPinBack;
 bool HasLocationPin=>locationPinName!=""&&locationPinSeed==game.Save.Seed;
 void LocationButton(Container parent,string name,double x,double z,Action back){Button(parent,"SHOW ON MAP / "+name,()=>{locationPinName=name;locationPinX=x;locationPinZ=z;locationPinSeed=game.Save.Seed;locationPinBack=back;ShowMap();});}
}
internal partial class ExpeditionMapCanvas {
 internal string PinName=""; internal double PinX,PinZ;
 void DrawLocationPin(){if(string.IsNullOrEmpty(PinName))return;var p=Map(PinX,PinZ);var color=Colors.Yellow;DrawLine(p-new Vector2(0,20),p,color,3);DrawCircle(p-new Vector2(0,24),8,color);DrawCircle(p-new Vector2(0,24),3,Colors.Black);DrawArc(p,12,0,Mathf.Tau,32,color,2,true);var label=new Vector2(Mathf.Clamp(p.X+15,4,Math.Max(4,Size.X-230)),Mathf.Clamp(p.Y-18,38,Size.Y-8));DrawString(ThemeDB.FallbackFont,label,PinName,HorizontalAlignment.Left,225,14,color);}
}
