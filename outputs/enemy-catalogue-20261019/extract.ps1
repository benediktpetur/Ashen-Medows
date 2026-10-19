$ErrorActionPreference='Stop'
$repo=Resolve-Path (Join-Path $PSScriptRoot '../..')
$asm=[Reflection.Assembly]::LoadFrom((Join-Path $repo 'game/.godot/mono/temp/bin/Debug/AshenMeadow.dll'))
function F($obj,$name){$obj.GetType().GetField($name).GetValue($obj)}
$threats=$asm.GetType('ExpeditionThreat').GetField('All').GetValue($null)
$campaign=$asm.GetType('ExpeditionCampaign')
$ecology=$asm.GetType('ExpeditionThreatEcology')
$special=@($threats|ForEach-Object {[ordered]@{kind=F $_ 'Kind';name=F $_ 'Name';min=F $_ 'Min';max=F $_ 'Max';health=F $_ 'Health';speed=F $_ 'Speed';night=F $_ 'NightOnly';role=F $_ 'Role';counter=F $_ 'Counter';habitat=$ecology.GetMethod('Description').Invoke($null,@([string](F $_ 'Kind')))}})
$health=@(for($tier=1;$tier -le 5;$tier++){foreach($kind in @('walker','runner','stalker','bloater','bruiser','armored','weeper','worm')+$special.kind){[ordered]@{tier=$tier;kind=$kind;hp=$campaign.GetMethod('Hp').Invoke($null,@([string]$kind,[int]$tier))}}})
$gt=$asm.GetType('ExpeditionGame');$g=$gt.GetConstructor([type[]]@([string],[int])).Invoke([object[]]@([string](Join-Path $PSScriptRoot 'unused.json'),[int]431));$gt.GetField('NoDiskSave').SetValue($g,$true);$world=$gt.GetField('World').GetValue($g)
$stories=@(foreach($index in @(5,6)){$p=$asm.GetType('ExpeditionEncounter').GetMethod('Place').Invoke($null,@($world,[int]$index));$x=F $p 'Item1';$z=F $p 'Item2';[ordered]@{index=$index;tier=[Math]::Max(2,$world.GetType().GetMethod('Tier').Invoke($world,@([double]$x,[double]$z)))}})
[ordered]@{date='2026-10-19';seed=431;special=$special;health=$health;storyGuardians=$stories}|ConvertTo-Json -Depth 10|Set-Content (Join-Path $PSScriptRoot 'enemies.json') -Encoding utf8
Write-Output "Extracted $($special.Count) special archetypes and $($health.Count) tier-specific health values."
