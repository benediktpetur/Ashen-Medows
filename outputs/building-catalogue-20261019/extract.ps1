$ErrorActionPreference='Stop'
$repo=Resolve-Path (Join-Path $PSScriptRoot '../..')
$asm=[Reflection.Assembly]::LoadFrom((Join-Path $repo 'game/.godot/mono/temp/bin/Debug/AshenMeadow.dll'))
$gt=$asm.GetType('ExpeditionGame')
function Call($name,[object[]]$arguments){$unwrapped=[object[]]::new($arguments.Length);for($n=0;$n -lt $arguments.Length;$n++){$unwrapped[$n]=$arguments[$n].PSObject.BaseObject};$gt.GetMethod($name).Invoke($script:g,$unwrapped)}
function Field($obj,$name){$obj.GetType().GetField($name).GetValue($obj)}
$g=$gt.GetConstructor([type[]]@([string],[int])).Invoke([object[]]@([string](Join-Path $PSScriptRoot 'unused-profile.json'),[int]431))
$gt.GetField('NoDiskSave').SetValue($g,$true)
$defs=$asm.GetType('BuildDefinition').GetField('All').GetValue($null)
$recipes=$asm.GetType('ExpeditionRecipe').GetField('All').GetValue($null)
$buildings=@(for($id=0;$id -lt $defs.Length;$id++){
 $d=$defs[$id];$r=@($recipes|Where-Object {(Field $_ 'Kind') -eq 'build' -and (Field $_ 'Result') -eq $id})[0]
 $parent=Call 'ResearchParent' @($r)
 [ordered]@{id=$id;name=Field $d 'Name';tier=Field $d 'Tier';category=Field $d 'Tab';function=Field $d 'Function';opening=Field $d 'Opening';baseKind=Field $d 'BaseKind';width=Field $d 'W';height=Field $d 'H';depth=Field $d 'D';weight=Field $d 'Weight';capacity=Field $d 'Capacity';ground=Field $d 'Ground';durability=Field $d 'Durability';cost=@(Field $d 'Cost');station=Field $r 'Station';rp=Call 'ResearchCost' @($r);free=Call 'FreeBlueprint' @($r);family=Call 'ResearchFamily' @($r);parent=$(if($parent){Field $parent 'Name'}else{'None'});focus=Call 'FocusWarning' @($r)}
})
$benches=@($recipes|Where-Object {(Field $_ 'Kind') -eq 'bench'}|ForEach-Object {[ordered]@{name=Field $_ 'Name';tier=Field $_ 'Tier';station=Field $_ 'Station';cost=@(Field $_ 'Cost')}})
[ordered]@{date='2026-10-19';buildings=$buildings;benches=$benches;materials=$gt.GetField('ResourceNames').GetValue($null)}|ConvertTo-Json -Depth 10|Set-Content (Join-Path $PSScriptRoot 'buildings.json') -Encoding utf8
Write-Output "Extracted $($buildings.Count) buildables and $($benches.Count) workshop upgrades."
