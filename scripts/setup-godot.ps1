param([switch]$Templates)
$ErrorActionPreference='Stop'
$repoRoot=Split-Path $PSScriptRoot -Parent
$toolsRoot=Join-Path $repoRoot '.tools'
New-Item -ItemType Directory -Force $toolsRoot | Out-Null
$releaseBase='https://github.com/godotengine/godot-builds/releases/download/4.6-stable'
$sumsPath=Join-Path $toolsRoot 'SHA512-SUMS.txt'
Invoke-WebRequest "$releaseBase/SHA512-SUMS.txt" -OutFile $sumsPath
function Get-VerifiedAsset([string]$Name){
 $target=Join-Path $toolsRoot $Name
 $line=Get-Content $sumsPath | Where-Object { $_ -match ('\s\*?'+[regex]::Escape($Name)+'$') } | Select-Object -First 1
 if(!$line){throw "No official checksum for $Name"}
 $expected=($line -split '\s+')[0]
 if(!(Test-Path $target) -or (Get-FileHash $target -Algorithm SHA512).Hash -ne $expected){Invoke-WebRequest "$releaseBase/$Name" -OutFile $target}
 if((Get-FileHash $target -Algorithm SHA512).Hash -ne $expected){throw "Checksum mismatch for $Name"}
 return $target
}
Add-Type -AssemblyName System.IO.Compression.FileSystem
$editorFolder=Join-Path $toolsRoot 'Godot_v4.6-stable_mono_win64'
if(!(Test-Path $editorFolder)){
 $editorZip=Get-VerifiedAsset 'Godot_v4.6-stable_mono_win64.zip'
 [IO.Compression.ZipFile]::ExtractToDirectory($editorZip,$toolsRoot)
}
if($Templates){
 $templateFolder=Join-Path $toolsRoot 'appdata/Godot/export_templates/4.6.stable.mono'
 if(!(Test-Path (Join-Path $templateFolder 'windows_release_x86_64.exe'))){
  $templateZip=Get-VerifiedAsset 'Godot_v4.6-stable_mono_export_templates.tpz'
  $extractFolder=Join-Path $toolsRoot ('templates-'+[guid]::NewGuid().ToString('N'))
  [IO.Compression.ZipFile]::ExtractToDirectory($templateZip,$extractFolder)
  New-Item -ItemType Directory -Force $templateFolder | Out-Null
  Copy-Item (Join-Path $extractFolder 'templates/*') -Destination $templateFolder -Recurse -Force
 }
}
$godotExe=Get-ChildItem -LiteralPath $editorFolder -Filter '*console.exe' -Recurse | Select-Object -First 1 -ExpandProperty FullName
if(!$godotExe){throw 'Godot .NET console executable not found'}
Write-Output $godotExe

