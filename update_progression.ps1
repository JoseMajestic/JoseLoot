param(
    [string]$ProjectRoot = (Get-Location).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$itemsFolder = Join-Path $ProjectRoot 'Assets/Items'
$enemiesFolder = Join-Path $ProjectRoot 'Assets/Enemies'
$itemDatabasePath = Join-Path $ProjectRoot 'Assets/Scripts/Item Database.asset'
$enemyDatabasePath = Join-Path $ProjectRoot 'Assets/Databases/Enemy Database.asset'

if (-not (Test-Path $itemsFolder)) { throw "Items folder not found: $itemsFolder" }
if (-not (Test-Path $enemiesFolder)) { throw "Enemies folder not found: $enemiesFolder" }
if (-not (Test-Path $itemDatabasePath)) { throw "Item database not found: $itemDatabasePath" }
if (-not (Test-Path $enemyDatabasePath)) { throw "Enemy database not found: $enemyDatabasePath" }

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

$enemyGuids = Get-GuidListFromDatabase -databasePath $enemyDatabasePath
$itemGuids = Get-GuidListFromDatabase -databasePath $itemDatabasePath

function Resolve-AssetPaths {
    param([string[]]$guids)
    $paths = @()
    foreach ($guid in $guids) {
        if (-not $guidMap.ContainsKey($guid)) {
            throw "Cannot resolve asset for GUID $guid"
        }
        $paths += $guidMap[$guid]
    }
    return $paths
}

$enemyPaths = Resolve-AssetPaths -guids $enemyGuids | Where-Object {
    $_ -like "$enemiesFolder*" -and $_.ToLower().EndsWith('.asset')
}
$itemPaths = Resolve-AssetPaths -guids $itemGuids | Where-Object {
    $_ -like "$itemsFolder*" -and $_.ToLower().EndsWith('.asset')
} | Sort-Object

if ($enemyPaths.Count -eq 0) { throw "Enemy database did not resolve to any enemy assets in $enemiesFolder" }
if ($itemPaths.Count -eq 0) { throw "Item database did not resolve to any item assets in $itemsFolder" }

$enemyStats = @(
    @{hp=7200; ataque=880; defensa=720; velocidad=100; crit=5; critDmg=120; destreza=200; suerte=10},
    @{hp=12000; ataque=1300; defensa=1000; velocidad=100; crit=6; critDmg=125; destreza=270; suerte=12},
    @{hp=21000; ataque=1900; defensa=1300; velocidad=100; crit=7; critDmg=130; destreza=340; suerte=15},
    @{hp=33000; ataque=2600; defensa=1700; velocidad=105; crit=8; critDmg=135; destreza=430; suerte=18},
    @{hp=48000; ataque=3900; defensa=2500; velocidad=105; crit=9; critDmg=140; destreza=520; suerte=20},
    @{hp=66000; ataque=5200; defensa=3200; velocidad=110; crit=10; critDmg=150; destreza=620; suerte=25},
    @{hp=90000; ataque=7000; defensa=4500; velocidad=110; crit=12; critDmg=160; destreza=750; suerte=28},
    @{hp=120000; ataque=9200; defensa=5800; velocidad=115; crit=14; critDmg=170; destreza=900; suerte=30},
    @{hp=161000; ataque=12000; defensa=7800; velocidad=115; crit=16; critDmg=185; destreza=1050; suerte=35},
    @{hp=210000; ataque=15500; defensa=10000; velocidad=120; crit=18; critDmg=200; destreza=1200; suerte=38},
    @{hp=330000; ataque=27500; defensa=15000; velocidad=125; crit=20; critDmg=220; destreza=1500; suerte=42},
    @{hp=570000; ataque=50000; defensa=27000; velocidad=130; crit=22; critDmg=250; destreza=1900; suerte=48},
    @{hp=840000; ataque=78000; defensa=42000; velocidad=135; crit=24; critDmg=280; destreza=2300; suerte=52},
    @{hp=1200000; ataque=112000; defensa=60000; velocidad=140; crit=26; critDmg=320; destreza=2800; suerte=55},
    @{hp=1650000; ataque=154000; defensa=84000; velocidad=145; crit=28; critDmg=360; destreza=3400; suerte=60},
    @{hp=2100000; ataque=210000; defensa=115000; velocidad=150; crit=30; critDmg=400; destreza=4100; suerte=65},
    @{hp=2400000; ataque=255000; defensa=138000; velocidad=155; crit=32; critDmg=440; destreza=4600; suerte=70},
    @{hp=2700000; ataque=302000; defensa=170000; velocidad=160; crit=34; critDmg=480; destreza=5100; suerte=75},
    @{hp=2850000; ataque=332000; defensa=186000; velocidad=165; crit=35; critDmg=500; destreza=5400; suerte=78},
    @{hp=3000000; ataque=385000; defensa=300000; velocidad=170; crit=38; critDmg=550; destreza=6000; suerte=80}
)

if ($enemyStats.Count -ne $enemyPaths.Count) {
    throw "Enemy stats count ($($enemyStats.Count)) does not match enemy database entries ($($enemyPaths.Count))"
}

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

for ($i = 0; $i -lt $enemyPaths.Count; $i++) {
    $path = $enemyPaths[$i]
    if ($i -ge $enemyStats.Count) {
        Write-Warning "No predefined stats for enemy index $i ($path); skipping"
        continue
    }
    $stats = $enemyStats[$i]
    Write-Host "Updating enemy $path"
    $lines = Get-Content -LiteralPath $path
    $changed = $false
    foreach ($field in @('hp','ataque','defensa','velocidadAtaque','ataqueCritico','danoCritico','suerte','destreza')) {
        $value = [int]$stats[$field.Replace('velocidadAtaque','velocidad').Replace('ataqueCritico','crit').Replace('danoCritico','critDmg')]
        $result = Update-YamlValue -lines $lines -field $field -value $value -WarnOnly
        if ($result.Success) { $changed = $true }
        $lines = $result.Lines
    }
    if ($changed) {
        Set-Content -LiteralPath $path -Value $lines -Encoding UTF8
    }
}

$setNames = @('Aprendiz','Arcano','Cazador','Conquistador','Heroe','Fantasma','Titan','Serafin','Abismo','Apocalipsis')
$levels = @('I','II','III')
$totalRanks = $setNames.Count * $levels.Count

function Get-GeometricValue {
    param($start,$end,[int]$rank,[int]$maxRank)
    if ($start -le 0) { return 0 }
    $ratio = [math]::Pow($end / $start, $rank / [double]$maxRank)
    return $start * $ratio
}

function Get-LinearValue {
    param($start,$end,[int]$rank,[int]$maxRank)
    return $start + ($end - $start) * ($rank / [double]$maxRank)
}

$statTargets = @{}
$maxRank = $totalRanks - 1
for ($s = 0; $s -lt $setNames.Count; $s++) {
    $set = $setNames[$s]
    $statTargets[$set] = @{}
    for ($l = 0; $l -lt $levels.Count; $l++) {
        $level = $levels[$l]
        $rank = $s * $levels.Count + $l
        $targets = [ordered]@{}
        $targets.attack = [math]::Round((Get-GeometricValue -start 1200 -end 500000 -rank $rank -maxRank $maxRank))
        $targets.defense = [math]::Round((Get-GeometricValue -start 800 -end 350000 -rank $rank -maxRank $maxRank))
        $targets.hp = [math]::Round((Get-GeometricValue -start 5000 -end 1600000 -rank $rank -maxRank $maxRank))
        $targets.crit = [math]::Round((Get-LinearValue -start 5 -end 38 -rank $rank -maxRank $maxRank))
        $targets.critDamage = [math]::Round((Get-LinearValue -start 120 -end 550 -rank $rank -maxRank $maxRank))
        $targets.attackSpeed = [math]::Round((Get-LinearValue -start 100 -end 170 -rank $rank -maxRank $maxRank))
        $targets.dexterity = [math]::Round((Get-GeometricValue -start 200 -end 6000 -rank $rank -maxRank $maxRank))
        $targets.luck = [math]::Round((Get-LinearValue -start 10 -end 80 -rank $rank -maxRank $maxRank))
        $statTargets[$set][$level] = $targets
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
    $set = $parts[0]
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
    $lines = Update-YamlValue -lines $lines -field 'hp' -value ([int]$hp) -WarnOnly
    foreach ($pair in @(
        @{field='ataque';value=[int]$attack},
        @{field='defensa';value=[int]$defense},
        @{field='velocidadAtaque';value=[int]$atkSpeed},
        @{field='ataqueCritico';value=[int]$crit},
        @{field='danoCritico';value=[int]$critDmg},
        @{field='suerte';value=[int]$luck},
        @{field='destreza';value=[int]$dex}
    )) {
        $update = Update-YamlValue -lines $lines -field $pair.field -value $pair.value -WarnOnly
        $lines = $update.Lines
    }
    Set-Content -LiteralPath $path -Value $lines -Encoding UTF8
}

Write-Host 'Enemy and item stats updated successfully.'
