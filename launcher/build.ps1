param([string]$Repository="benediktpetur/Ashen-Medows")
$ErrorActionPreference='Stop'
$launcherRoot=$PSScriptRoot
New-Item -ItemType Directory -Force "$launcherRoot\dist" | Out-Null
& 'C:/Windows/Microsoft.NET/Framework64/v4.0.30319/csc.exe' /nologo /target:winexe "/win32icon:$launcherRoot\assets\ashen-meadow.ico" /optimize+ /out:"$launcherRoot\dist\AshenLauncher.exe" "/resource:$launcherRoot\assets\expedition-hero.png,LauncherArt" /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /reference:System.Runtime.Serialization.dll /reference:System.IO.Compression.dll /reference:System.IO.Compression.FileSystem.dll (Get-ChildItem -LiteralPath "$launcherRoot\src" -Filter *.cs | Select-Object -ExpandProperty FullName)
if($LASTEXITCODE -ne 0){throw 'Launcher compilation failed'}
$launcherCheck=Start-Process -FilePath "$launcherRoot\dist\AshenLauncher.exe" -ArgumentList '--check' -PassThru -Wait -WindowStyle Hidden
if($launcherCheck.ExitCode -ne 0){throw 'Launcher checks failed'}
@{Repository=$Repository;ManifestUrl=""} | ConvertTo-Json | Set-Content "$launcherRoot\dist\launcher.json" -Encoding utf8
