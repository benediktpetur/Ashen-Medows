param(
 [Parameter(Mandatory=$true)][string]$Repository,
 [Parameter(Mandatory=$true)][string]$Version,
 [string]$Notes='Godot playtest update'
)
$ErrorActionPreference='Stop'
if($Repository -notmatch '^[A-Za-z0-9][A-Za-z0-9-]*/[A-Za-z0-9_.-]+$'){throw 'Repository must be owner/repository'}
$repoRoot=Split-Path $PSScriptRoot -Parent
$tag="v$Version"
& (Join-Path $repoRoot 'launcher/build.ps1') -Repository $Repository
& (Join-Path $repoRoot 'launcher/package-release.ps1') -BuildDirectory (Join-Path $repoRoot 'build/game') -Version $Version -PublicBaseUrl "https://github.com/$Repository/releases/download/$tag" -Executable AshenMeadow.exe -Notes $Notes -OutputDirectory (Join-Path $repoRoot 'release')
$gameZip=Join-Path $repoRoot "release/ashen-$Version-windows-x64.zip"
if((Get-Item -LiteralPath $gameZip).Length -ge 2GB){throw 'GitHub release asset must be under 2 GiB'}
$launcherZip=Join-Path $repoRoot "release/AshenLauncher.zip"
if(Test-Path $launcherZip){throw 'Launcher ZIP already exists; use a fresh build workspace'}
Compress-Archive -LiteralPath (Join-Path $repoRoot 'launcher/dist/AshenLauncher.exe'),(Join-Path $repoRoot 'launcher/dist/launcher.json') -DestinationPath $launcherZip
Write-Output "Ready: release/ashen-$Version-windows-x64.zip, release/manifest.json, release/AshenLauncher.zip"
