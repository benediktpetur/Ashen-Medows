param(
 [Parameter(Mandatory=$true)][string]$BuildDirectory,
 [Parameter(Mandatory=$true)][string]$Version,
 [Parameter(Mandatory=$true)][string]$PublicBaseUrl,
 [string]$Executable='AshenMeadow.exe',
 [string]$Notes='Playtest update',
 [string]$OutputDirectory=(Join-Path $PSScriptRoot 'release')
)
$ErrorActionPreference='Stop'
$parsedVersion=[version]$Version
if($Version -notmatch '^\d+\.\d+\.\d+(\.\d+)?$'){throw 'Use a numeric version such as 0.2.0'}
$baseUri=[uri]$PublicBaseUrl
if(!$baseUri.IsAbsoluteUri -or $baseUri.Scheme -ne 'https'){throw 'PublicBaseUrl must use HTTPS'}
$buildRoot=(Resolve-Path -LiteralPath $BuildDirectory).Path
if($Executable -match '(^/|\\|:|\.\.)' -or !(Test-Path -LiteralPath (Join-Path $buildRoot $Executable))){throw 'Executable must name a file inside the clean export folder'}
# Require a clean export, never accidentally distribute profiles or server certificates.
$files=Get-ChildItem -LiteralPath $buildRoot -Recurse -Force
foreach($file in $files){if($file.Name -match '^(profiles|saves|\.git|\.godot)$' -or $file.Extension -in '.pfx','.pem','.key' -or $file.Name -eq 'relay-server.json'){throw "Not a clean game export: $($file.Name)"}}
New-Item -ItemType Directory -Force $OutputDirectory | Out-Null
$outputRoot=(Resolve-Path -LiteralPath $OutputDirectory).Path
if($outputRoot.StartsWith($buildRoot.TrimEnd('\')+'\',[StringComparison]::OrdinalIgnoreCase) -or $outputRoot -eq $buildRoot){throw 'Output must be outside the build folder'}
$zipName="ashen-$Version-windows-x64.zip"
$zipPath=Join-Path $outputRoot $zipName
if(Test-Path -LiteralPath $zipPath){throw 'Version already packaged. Use a new version.'}
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory($buildRoot,$zipPath,[IO.Compression.CompressionLevel]::Optimal,$false)
$manifest=[ordered]@{Version=$Version;Url=$PublicBaseUrl.TrimEnd('/')+'/'+$zipName;Sha256=(Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash.ToLowerInvariant();Executable=$Executable;Bytes=(Get-Item -LiteralPath $zipPath).Length;Notes=$Notes}
$manifest | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $outputRoot 'manifest.json') -Encoding utf8
Write-Output "Upload $zipName first; replace manifest.json LAST."
