param([switch]$Export,[string]$ToolsRoot)
$ErrorActionPreference='Stop'
$repoRoot=Split-Path $PSScriptRoot -Parent
if(!$ToolsRoot){$ToolsRoot=Join-Path $repoRoot '.tools'};$ToolsRoot=[IO.Path]::GetFullPath($ToolsRoot)
$gameRoot=Join-Path $repoRoot 'game'
$editorFolder=Join-Path $ToolsRoot 'Godot_v4.6-stable_mono_win64'
$godotExe=Get-ChildItem -LiteralPath $editorFolder -Filter '*console.exe' -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName
if(!$godotExe){throw 'Run scripts/setup-godot.ps1 first (add -Templates to export).'}
$priorAppData=$env:APPDATA
try{
 $env:APPDATA=Join-Path $ToolsRoot 'appdata'
 New-Item -ItemType Directory -Force $env:APPDATA | Out-Null
 & dotnet build (Join-Path $gameRoot 'AshenMeadow.csproj') --configuration Debug --nologo
 if($LASTEXITCODE -ne 0){throw 'Godot C# build failed'}
 & $godotExe --headless --path $gameRoot --editor --import
 if($LASTEXITCODE -ne 0){throw 'Godot import failed'}
 & $godotExe --headless --path $gameRoot --quit-after 10 2>&1 | Tee-Object -Variable smokeOutput
 if($LASTEXITCODE -ne 0 -or ($smokeOutput -join "`n") -match '(?m)^ERROR:'){throw 'Godot smoke test failed'}
 foreach($testMode in @('--logic-check','--music-check','--journey-check','--first-day-check','--world-works-check','--community-social-check','--forest-check','--openings-check','--integration-check')){
  & $godotExe --headless --path $gameRoot -- $testMode 2>&1 | Tee-Object -Variable testOutput
  if($LASTEXITCODE -ne 0 -or ($testOutput -join "`n") -match '(?m)^ERROR:'){throw "Godot gameplay checks failed: $testMode"}
 }
 & (Join-Path $PSScriptRoot 'test-coop.ps1') -GodotExecutable $godotExe
 if($Export){
  $buildFolder=Join-Path $repoRoot 'build/game'
  New-Item -ItemType Directory -Force $buildFolder | Out-Null
  & $godotExe --headless --path $gameRoot --quit --export-release 'Windows Desktop' (Join-Path $buildFolder 'AshenMeadow.exe') 2>&1 | Tee-Object -Variable exportOutput
  if($LASTEXITCODE -ne 0 -or ($exportOutput -join "`n") -match '(?m)^ERROR:'){throw 'Godot export failed'}
  if(!(Test-Path (Join-Path $buildFolder 'AshenMeadow.exe')) -or !(Test-Path (Join-Path $buildFolder 'AshenMeadow.pck'))){throw 'Incomplete Windows export'}
 }
}finally{$env:APPDATA=$priorAppData}
