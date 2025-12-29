param(
    [string]$ProjectRoot = (Get-Location).Path,
    [string]$CsvPath = 'Assets/Databases/object_progression.csv'
)

$CsvPath = Join-Path $ProjectRoot $CsvPath

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$itemsFolder = Join-Path $ProjectRoot 'Assets/Items'
$itemDatabasePath = Join-Path $ProjectRoot 'Assets/Scripts/Item Database.asset'

if (-not (Test-Path $CsvPath)) { throw "CSV file not found: $CsvPath" }
if (-not (Test-Path $itemsFolder)) { throw "Items folder not found: $itemsFolder" }
if (-not (Test-Path $itemDatabasePath)) { throw "Item database not found: $itemDatabasePath" }

function Get-GuidMap {
    param([string]$root)
    $map = @{}
    Get-ChildItem -Path $root -Filter '*.meta' -Recurse | ForEach-Object {
        $guidLine = Select-String -Path $_.FullName -Pattern '^guid:\s*([0-9a-f]+)' -SimpleMatch:$false -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($guidLine) {
            $guid = ($guidLine.Matches[0].Groups[1].Value)
            $assetPath = $_.FullName.Substring(0, $_.FullName.Length - 5)
            if (-not $map.ContainsKey($guid)) {
                $map[$guid] = $assetPath
            }
        }
    }
    return $map
}

$guidMap = Get-GuidMap -root (Join-Path $ProjectRoot 'Assets')

function Get-GuidListFromDatabase {
    param([string]$databasePath)
    $content = Get-Content -LiteralPath $databasePath
    $guids = @()
    foreach ($line in $content) {
        if ($line -match 'guid:\s*([0-9a-f]+)') {
            $guids += $matches[1]
        }
    }
    return $guids
}

$itemGuids = Get-GuidListFromDatabase -databasePath $itemDatabasePath

function Resolve-AssetPaths {
    param([string[]]$guids)
    $paths = @()
    foreach ($guid in $guids) {
        if (-not $guidMap.ContainsKey($guid)) {
            Write-Warning "Cannot resolve asset for GUID $guid; skipping"
            continue
        }
        $paths += $guidMap[$guid]
    }
    return $paths
}

$itemPaths = Resolve-AssetPaths -guids $itemGuids | Where-Object {
    $_ -like "$itemsFolder*" -and $_.ToLower().EndsWith('.asset')
} | Sort-Object

if ($itemPaths.Count -eq 0) { throw "Item database did not resolve to any item assets in $itemsFolder" }

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

function Parse-ItemInfo {
    param([string]$filePath)
    $name = [System.IO.Path]::GetFileNameWithoutExtension($filePath)
    $parts = $name.Split(' ')
    $level = $parts[-1]
    $type = $parts[-2]
    $set = $parts[0..($parts.Length-3)] -join ' '
    return [pscustomobject]@{Set=$set;Type=$type;Level=$level}
}

foreach ($path in $itemPaths) {
    if (-not ($path -like "$itemsFolder*")) { continue }
    $info = Parse-ItemInfo -filePath $path
    if (-not $statTargets.ContainsKey($info.Set)) { continue }
    if (-not $statTargets[$info.Set].ContainsKey($info.Level)) { continue }
    $targets = $statTargets[$info.Set][$info.Level]
    $type = $info.Type
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
    }
}

Write-Host 'Object stats updated from table successfully.'
