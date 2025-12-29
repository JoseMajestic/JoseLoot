param(
    [string]$ProjectRoot = (Get-Location).Path,
    [string]$CsvPath = 'Assets/Databases/object_progression.csv'
)

$CsvPath = Join-Path $ProjectRoot $CsvPath

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$itemsFolder = Join-Path $ProjectRoot 'Assets/Items'

if (-not (Test-Path $CsvPath)) { throw "CSV file not found: $CsvPath" }
if (-not (Test-Path $itemsFolder)) { throw "Items folder not found: $itemsFolder" }

function Update-YamlValue {
    param(
        [string[]]$lines,
        [string]$field,
        [string]$value,
        [switch]$WarnOnly
    )
    $pattern = "^(\s*){0}:\s*" -f [regex]::Escape($field)
    for ($i = 0; $i -lt $lines.Length; $i++) {
        if ($lines[$i] -match $pattern) {
            $indent = $matches[1]
            $lines[$i] = "${indent}${field}: $value"
            return [pscustomobject]@{Success=$true;Lines=$lines}
        }
    }
    if ($WarnOnly) {
        Write-Warning "Field '$field' not found; skipping"
        return [pscustomobject]@{Success=$false;Lines=$lines}
    }
    throw "Field '$field' not found in file"
}

$csvData = Import-Csv -Path $CsvPath

$statTargets = @{}
foreach ($row in $csvData) {
    $set = $row.SetName
    $level = $row.Level
    if (-not $statTargets.ContainsKey($set)) { $statTargets[$set] = @{} }
    $statTargets[$set][$level] = @{
        attack = [int]$row.Attack
        defense = [int]$row.Defense
        hp = [int]$row.HP
        crit = [int]$row.CritChance
        critDamage = [int]$row.CritDamage
        attackSpeed = [int]$row.AttackSpeed
        dexterity = [int]$row.Dexterity
        luck = [int]$row.Luck
    }
}

$attackDistribution = @{'Arma'=0.4;'Guantes'=0.15;'Anillo'=0.15;'Cinturon'=0.1;'Montura'=0.2}
$defenseDistribution = @{'Armadura'=0.3;'Escudo'=0.2;'Casco'=0.15;'Botas'=0.1;'Collar'=0.1;'Montura'=0.15}
$hpDistribution = $defenseDistribution
$critDistribution = @{'Arma'=0.5;'Guantes'=0.3;'Collar'=0.1;'Anillo'=0.1}
$critDmgDistribution = @{'Arma'=0.4;'Guantes'=0.2;'Collar'=0.2;'Anillo'=0.2}
$attackSpeedDistribution = @{'Guantes'=0.5;'Botas'=0.3;'Arma'=0.2}
$dexDistribution = @{'Casco'=0.25;'Guantes'=0.2;'Botas'=0.2;'Anillo'=0.15;'Collar'=0.1;'Cinturon'=0.1}
$luckDistribution = @{'Collar'=0.4;'Anillo'=0.3;'Montura'=0.2;'Guantes'=0.1}

function Get-DistributionValue {
    param($dist, $type)
    if ($dist.ContainsKey($type)) { return $dist[$type] }
    return 0
}

$recreatedAssets = @(
    'Aprendiz Anillo I.asset',
    'Aprendiz Anillo II.asset',
    'Aprendiz Anillo III.asset',
    'Aprendiz Collar I.asset',
    'Aprendiz Collar II.asset',
    'Aprendiz Collar III.asset',
    'Aprendiz Montura I.asset',
    'Aprendiz Montura II.asset',
    'Aprendiz Montura III.asset',
    'Cazador Anillo I.asset',
    'Cazador Anillo II.asset',
    'Cazador Anillo III.asset',
    'Cazador Collar I.asset',
    'Cazador Collar II.asset',
    'Cazador Collar III.asset',
    'Cazador Montura I.asset',
    'Cazador Montura II.asset',
    'Cazador Montura III.asset'
)

foreach ($assetName in $recreatedAssets) {
    $path = Join-Path $itemsFolder $assetName
    if (-not (Test-Path $path)) {
        Write-Warning "Asset $assetName not found; skipping"
        continue
    }
    $name = [System.IO.Path]::GetFileNameWithoutExtension($path)
    $parts = $name.Split(' ')
    $set = $parts[0]
    $type = $parts[1]
    $level = $parts[2]
    if (-not $statTargets.ContainsKey($set) -or -not $statTargets[$set].ContainsKey($level)) {
        Write-Warning "No stats for $set $level; skipping"
        continue
    }
    $targets = $statTargets[$set][$level]
    $attack = [math]::Round($targets.attack * (Get-DistributionValue $attackDistribution $type))
    $defense = [math]::Round($targets.defense * (Get-DistributionValue $defenseDistribution $type))
    $hp = [math]::Round($targets.hp * (Get-DistributionValue $hpDistribution $type))
    $crit = [math]::Round($targets.crit * (Get-DistributionValue $critDistribution $type))
    $critDmg = [math]::Round($targets.critDamage * (Get-DistributionValue $critDmgDistribution $type))
    $atkSpeed = [math]::Round($targets.attackSpeed * (Get-DistributionValue $attackSpeedDistribution $type))
    $dex = [math]::Round($targets.dexterity * (Get-DistributionValue $dexDistribution $type))
    $luck = [math]::Round($targets.luck * (Get-DistributionValue $luckDistribution $type))
    $lines = Get-Content -LiteralPath $path
    $changed = $false
    foreach ($pair in @(
        @{field='hp';value=[int]$hp},
        @{field='ataque';value=[int]$attack},
        @{field='defensa';value=[int]$defense},
        @{field='velocidadAtaque';value=[int]$atkSpeed},
        @{field='ataqueCritico';value=[int]$crit},
        @{field='danoCritico';value=[int]$critDmg},
        @{field='suerte';value=[int]$luck},
        @{field='destreza';value=[int]$dex}
    )) {
        $update = Update-YamlValue -lines $lines -field $pair.field -value $pair.value -WarnOnly
        if ($update.Success) { $changed = $true }
        $lines = $update.Lines
    }
    if ($changed) {
        Set-Content -LiteralPath $path -Value $lines -Encoding UTF8
        Write-Host "Updated $assetName"
    }
}

Write-Host 'Recreated assets updated successfully.'
