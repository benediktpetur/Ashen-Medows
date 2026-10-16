using Godot;
using System.Collections.Generic;
internal partial class ResearchTreeCanvas:Control{
 internal List<(Vector2 From,Vector2 To,Color Color)> Edges=new();
 public override void _Draw(){DrawRect(new Rect2(Vector2.Zero,Size),new Color(.045f,.075f,.075f));foreach(var e in Edges)DrawLine(e.From,e.To,new Color(e.Color,.6f),1.5f,true);for(int i=1;i<5;i++)DrawLine(new(i*158,0),new(i*158,938),new Color(.2f,.27f,.25f),1);DrawLine(new(12,940),new(788,940),new Color(.42f,.39f,.27f),1);}
}
