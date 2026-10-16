param([string]$OutputPath)
$ErrorActionPreference='Stop'
$repoRoot=Split-Path $PSScriptRoot -Parent
if(!$OutputPath){$OutputPath=Join-Path (Split-Path $repoRoot -Parent) 'AshenMeadow-Godot-upload.zip'}
if(Test-Path -LiteralPath $OutputPath){throw 'Upload ZIP already exists; choose a new OutputPath.'}
Add-Type -AssemblyName System.IO.Compression.FileSystem
$files=@()
foreach($name in @('.gitignore','README.md','VALIDATION.md','GAMEPLAY.md','ART-PASS.md','MULTIPLAYER.md','CHARACTER-ART.md','ITEM-CATALOGUE.md','BASE-BUILDING.md')){$files+=Get-Item -LiteralPath (Join-Path $repoRoot $name)}
foreach($folder in @('game','launcher','scripts','.github','art-reference')){
 $files+=Get-ChildItem -LiteralPath (Join-Path $repoRoot $folder) -Recurse -Force -File | Where-Object {
  $relative=$_.FullName.Substring($repoRoot.Length+1).Replace('\','/')
  $relative -notmatch '(^|/)(\.godot|\.packages|bin|obj|dist|tests|release|build|profiles|worlds|saves)/' -and $_.Extension -notin '.pfx','.pem','.key' -and $_.Name -notin 'relay-client.json','relay-server.json'
 }
}
$archive=[IO.Compression.ZipFile]::Open($OutputPath,[IO.Compression.ZipArchiveMode]::Create)
try{foreach($file in $files){$relative=$file.FullName.Substring($repoRoot.Length+1).Replace('\','/');[IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive,$file.FullName,$relative,[IO.Compression.CompressionLevel]::Optimal) | Out-Null}}finally{$archive.Dispose()}
Write-Output "Clean source upload: $OutputPath"
