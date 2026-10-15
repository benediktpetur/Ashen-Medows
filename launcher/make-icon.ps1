$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$frames=@()
foreach($size in @(16,32,48,64,128,256)){
 $bitmap=[Drawing.Bitmap]::new($size,$size)
 $g=[Drawing.Graphics]::FromImage($bitmap)
 $g.SmoothingMode=[Drawing.Drawing2D.SmoothingMode]::AntiAlias
 $g.Clear([Drawing.Color]::FromArgb(12,17,16))
 $g.ScaleTransform($size/100.0,$size/100.0)
 $pen=[Drawing.Pen]::new([Drawing.Color]::FromArgb(223,178,100),4)
 $g.DrawEllipse($pen,9,9,82,82)
 $g.DrawArc($pen,30,25,40,40,180,180)
 for($i=0;$i -lt 3;$i++){
  $x=32+$i*18
  $g.DrawLine($pen,$x,74,$x+3,47)
  $g.DrawLine($pen,$x+3,53,$x-4,46)
 }
 $g.DrawLine($pen,25,74,76,74)
 $stream=[IO.MemoryStream]::new()
 $bitmap.Save($stream,[Drawing.Imaging.ImageFormat]::Png)
 $frames+=,@{Size=$size;Bytes=$stream.ToArray()}
 $stream.Dispose();$pen.Dispose();$g.Dispose();$bitmap.Dispose()
}
$output=[IO.File]::Create((Join-Path $PSScriptRoot 'assets/ashen-meadow.ico'))
$writer=[IO.BinaryWriter]::new($output)
try{
 $writer.Write([uint16]0);$writer.Write([uint16]1);$writer.Write([uint16]$frames.Count)
 $offset=6+16*$frames.Count
 foreach($frame in $frames){
  $dimension=if($frame.Size -eq 256){0}else{$frame.Size}
  $writer.Write([byte]$dimension);$writer.Write([byte]$dimension)
  $writer.Write([byte]0);$writer.Write([byte]0)
  $writer.Write([uint16]1);$writer.Write([uint16]32)
  $writer.Write([uint32]$frame.Bytes.Length);$writer.Write([uint32]$offset)
  $offset+=$frame.Bytes.Length
 }
 foreach($frame in $frames){$writer.Write([byte[]]$frame.Bytes)}
}finally{$writer.Dispose();$output.Dispose()}
