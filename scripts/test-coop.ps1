param([Parameter(Mandatory=$true)][string]$GodotExecutable,[switch]$Exported)
$ErrorActionPreference='Stop'
$repoRoot=Split-Path $PSScriptRoot -Parent
$project=Join-Path $repoRoot 'game'
$logs=Join-Path $repoRoot 'build/coop-checks'
New-Item -ItemType Directory -Force $logs | Out-Null
$processes=@()
try{
 foreach($role in @('host','client')){
  $arguments=if($Exported){@('--headless','--',"--coop-check-$role")}else{@('--headless','--path',('"'+$project+'"'),'--',"--coop-check-$role")}
  $processes+=Start-Process -FilePath $GodotExecutable -ArgumentList $arguments -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $logs "$role.log") -RedirectStandardError (Join-Path $logs "$role-error.log")
 }
 $deadline=[DateTime]::UtcNow.AddSeconds(150)
 while(@($processes | Where-Object {!$_.HasExited}).Count -gt 0){
  if([DateTime]::UtcNow -gt $deadline){throw 'Co-op process tests timed out.'}
  Start-Sleep -Milliseconds 200
 }
 foreach($role in @('host','client')){
  $output=Get-Content -LiteralPath (Join-Path $logs "$role.log"),(Join-Path $logs "$role-error.log") -Raw
  if(($output -join "`n") -match '(?m)^ERROR:' -or ($output -join "`n") -notmatch "PASS / Godot ENet $role"){
   foreach($logRole in @('host','client')){
    foreach($suffix in @('','-error')){
     $logFile=Join-Path $logs "$logRole$suffix.log"
     Write-Output "Co-op diagnostic: $logFile"
     Get-Content -LiteralPath $logFile -Tail 80
    }
   }
   throw "Co-op $role failed. See diagnostics above and $logs"
  }
 }
 foreach($process in $processes){if($process.ExitCode -ne 0){throw 'Co-op test process failed.'}}
 Write-Output 'PASS / two-process Godot ENet co-op checks'
}finally{
 foreach($process in $processes){if(!$process.HasExited){Stop-Process -Id $process.Id -Force};$process.Dispose()}
}
