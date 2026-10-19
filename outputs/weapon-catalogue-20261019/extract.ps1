$ErrorActionPreference='Stop'
$repo=Resolve-Path (Join-Path $PSScriptRoot '../..')
$asm=[Reflection.Assembly]::LoadFrom((Join-Path $repo 'game/.godot/mono/temp/bin/Debug/AshenMeadow.dll'))
$gt=$asm.GetType('ExpeditionGame')
function Call($name,[object[]]$arguments){$unwrapped=[object[]]::new($arguments.Length);for($n=0;$n -lt $arguments.Length;$n++){$unwrapped[$n]=$arguments[$n].PSObject.BaseObject};$gt.GetMethod($name).Invoke($script:g,$unwrapped)}
function Field($obj,$name){$obj.GetType().GetField($name).GetValue($obj)}
function Prop($name){$gt.GetProperty($name).GetValue($script:g)}
$ctor=$gt.GetConstructor([type[]]@([string],[int]))
$g=$ctor.Invoke([object[]]@([string](Join-Path $PSScriptRoot 'unused-profile.json'),[int]431))
$gt.GetField('NoDiskSave').SetValue($g,$true)
$craft=Prop 'Craft'
$names=$gt.GetField('GunNames').GetValue($null)
$recipes=$asm.GetType('ExpeditionRecipe').GetField('All').GetValue($null)
$weapons=@(for($id=0;$id -lt $names.Length;$id++){
 $craft.GetType().GetField('Weapon').SetValue($craft,[int]$id)
 $gun=Call 'IsGun' @([int]$id);$bow=Call 'IsBow' @([int]$id);$tier=Call 'GunTier' @([int]$id)
 $recipe=@($recipes | Where-Object {(Field $_ 'Kind') -eq 'weapon' -and (Field $_ 'Result') -eq $id}) | Select-Object -First 1
 $parent=Call 'WeaponParent' @([int]$id)
 $parentRecipe=if($recipe){Call 'ResearchParent' @($recipe)}else{$null}
 [ordered]@{id=$id;name=$names[$id];class=$(if($gun){'Gun'}elseif($bow){'Bow'}else{'Melee'});tier=$tier;damage=(Call 'DamageFor' @([int]$id,$false));cycle=$(if($gun){Prop 'FireDelay'}elseif($bow){Prop 'SoulBowDelay'}else{Prop 'SoulMeleeDelay'});reach=$(if(!$gun -and !$bow){Call 'WeaponReach' @([int]$id)}else{$null});magazine=$(if($gun){Prop 'Magazine'}else{$null});resource=$(if($gun){$gt.GetField('AmmoNames').GetValue($null)[(Call 'AmmoClassFor' @([int]$id))]}elseif($bow){'1 arrow'}elseif($id -eq 27){'1 fuel + 1 energy cell / burst'}elseif($id -eq 17){'1 fuel / burst'}else{'None'});stamina=$(if(!$gun -and !$bow){Prop 'MeleeCost'}else{$null});automatic=(Call 'Automatic' @([int]$id));shotgun=(Call 'Shotgun' @([int]$id));parent=$(if($parent -ge 0){$names[$parent]}else{'None'});recipeParent=$(if($parentRecipe){Field $parentRecipe 'Name'}else{'None'});station=$(if($recipe){Field $recipe 'Station'}else{$null});cost=$(if($recipe){@(Field $recipe 'Cost')}else{@()});rp=$(if($recipe){Call 'ResearchCost' @($recipe)}else{$null});family=$(if($recipe){Call 'ResearchFamily' @($recipe)}else{if($gun){'Ballistics'}elseif($bow){'Ranger'}else{'Brawler'}});recipeTier=$(if($recipe){Field $recipe 'Tier'}else{$null});starting=($id -in 8,9)}
})
$parts=@($asm.GetType('WeaponPart').GetField('All').GetValue($null) | ForEach-Object {
 $p=$_;$fit=Field $p 'Fits'
 [ordered]@{name=Field $p 'Name';slot=Field $p 'Slot';family=Field $p 'Family';tier=Field $p 'Tier';description=Field $p 'Description';fits=@(for($id=0;$id -lt $names.Length;$id++){if($fit.Invoke([int]$id)){$names[$id]}})}
})
$data=[ordered]@{date='2026-10-19';weapons=$weapons;parts=$parts;materials=$gt.GetField('ResourceNames').GetValue($null)}
$data | ConvertTo-Json -Depth 12 | Set-Content (Join-Path $PSScriptRoot 'weapons.json') -Encoding utf8
Write-Output "Extracted $($weapons.Count) weapons and $($parts.Count) attachments from current assembly."
