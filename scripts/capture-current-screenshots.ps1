param([string]$GameExecutable,[string[]]$Modes=@('preview','character-preview','zombie-preview','wildlife-preview','world-systems-preview','terrain-preview','world-polish-preview','workshop-preview','inventory-preview','item-preview','leveling-preview','research-preview','blueprint-preview','talent-preview','mill-preview','trap-preview','combat-loot-preview','community-preview','family-preview','woodland-preview','fever-preview','letter-preview','birthday-preview','checkpoint-preview','crash-preview','expansion-preview','crouch-preview','campaign-preview','gunplay-preview','dog-preview'))
$ErrorActionPreference='Stop'
$taskRepo=Split-Path $PSScriptRoot -Parent
if(!$GameExecutable){$GameExecutable=Join-Path $taskRepo 'build/game/AshenMeadow.exe'}
$taskOutput=Join-Path $taskRepo 'Ingame Screenshots'
$taskLogs=Join-Path $taskRepo 'build/screenshot-capture'
$taskData=Join-Path $taskRepo 'build/screenshot-capture/appdata'
New-Item -ItemType Directory -Force $taskOutput,$taskLogs,$taskData | Out-Null
$taskPriorData=$env:APPDATA
$taskFailures=@()
$taskInventory=Get-ChildItem -LiteralPath (Join-Path $taskRepo 'build') -Recurse -File -Filter *.png | Where-Object {$_.FullName -notlike '*screenshot-capture*'} | Select-Object @{n='OriginalPath';e={$_.FullName.Substring($taskRepo.Length+1)}},Length,LastWriteTime
$taskInventory | Export-Csv -LiteralPath (Join-Path $taskLogs 'existing-screenshots.csv') -NoTypeInformation
try{
 $env:APPDATA=$taskData
 foreach($taskMode in $Modes){
  $taskLog=Join-Path $taskLogs ($taskMode+'.log');$taskError=Join-Path $taskLogs ($taskMode+'-error.log')
  $taskProcess=Start-Process -FilePath $GameExecutable -ArgumentList '--',('--'+$taskMode) -WindowStyle Hidden -PassThru -RedirectStandardOutput $taskLog -RedirectStandardError $taskError
  if(!$taskProcess.WaitForExit(55000)){$taskProcess.Kill();$taskFailures+=$taskMode;Write-Output "FAIL / timeout / $taskMode";continue}
  $taskText=(Get-Content -LiteralPath $taskLog -Raw)+(Get-Content -LiteralPath $taskError -Raw)
  if($taskProcess.ExitCode -ne 0 -or $taskText -match '(?m)^ERROR:' -or $taskText -notmatch 'Captured '){$taskFailures+=$taskMode;Write-Output "FAIL / $taskMode / see $taskError"}else{Write-Output "PASS / $taskMode"}
 }
 $taskArt=Join-Path $taskData 'AshenMeadowExpedition/tests/art'
 $taskEntries=@(Get-ChildItem -LiteralPath $taskArt -File -Filter *.png | Sort-Object Name | ForEach-Object {Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $taskOutput $_.Name);[pscustomobject]@{File=$_.Name;SHA256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash;CapturedUtc=$_.LastWriteTimeUtc.ToString('o');Source='Exported Windows game'}})
 $taskEntries | Export-Csv -LiteralPath (Join-Path $taskOutput 'Screenshot Manifest.csv') -NoTypeInformation
 [ordered]@{Executable=$GameExecutable;ExecutableSHA256=(Get-FileHash -LiteralPath $GameExecutable).Hash;PackSHA256=(Get-FileHash -LiteralPath (Join-Path (Split-Path $GameExecutable) 'AshenMeadow.pck')).Hash;CapturedUtc=[DateTime]::UtcNow.ToString('o');Screenshots=$taskEntries.Count;FailedModes=$taskFailures} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $taskOutput 'Build Information.json')
 Write-Output "COPIED / $($taskEntries.Count) current-build screenshots / $taskOutput"
 if($taskFailures.Count){throw ('Capture failures: '+($taskFailures -join ', '))}
}finally{$env:APPDATA=$taskPriorData}
