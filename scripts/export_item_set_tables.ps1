<#
.SYNOPSIS
    Genera tablas de stats proyectados por set/tier para niveles clave.
.DESCRIPTION
    - Lee todos los ItemData referenciados en Item Database.
    - Determina el set/tier y multiplicador según ItemImprovementCurves.
    - Usa la curva base (reports/item_stat_growth.csv) para calcular stats
      en los niveles simbólicos solicitados.
    - Escribe un CSV por set con todos sus objetos y los 9 parámetros.
#>

param(
    [int[]] $Levels = @(1,5,10,25,50,100,150,200,250,500,600,750,900,999)
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent $PSScriptRoot
$AssetsDir = Join-Path $RootDir "Assets"
$ReportDir = Join-Path $RootDir "reports"
$GrowthCsv = Join-Path $ReportDir "item_stat_growth.csv"
$ItemDatabasePath = Join-Path $RootDir "Assets\Scripts\Item Database.asset"
$OutputDir = Join-Path $ReportDir "item_set_tables"

New-Item -ItemType Directory -Force -Path $ReportDir | Out-Null
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

function Get-RelativePath {
    param(
        [string] $BasePath,
        [string] $FullPath
    )

    $base = [System.IO.Path]::GetFullPath($BasePath)
    $full = [System.IO.Path]::GetFullPath($FullPath)
    if ($full.StartsWith($base)) {
        return $full.Substring($base.Length).TrimStart('\')
    }
    return $FullPath
}

if (-not (Test-Path -LiteralPath $GrowthCsv)) {
    Write-Host "No se encontró $GrowthCsv. Generando curva base..."
    $growthScript = Join-Path $PSScriptRoot "export_item_stat_growth.ps1"
    & powershell -ExecutionPolicy Bypass -File $growthScript
}

if (-not (Test-Path -LiteralPath $GrowthCsv)) {
    throw "No se pudo generar item_stat_growth.csv. Aborta."
}

function Import-GrowthData {
    param([string] $Path, [int[]] $TargetLevels)

    $growthRows = Import-Csv -Path $Path
    $map = @{}
    foreach ($row in $growthRows) {
        $level = [int]$row.Level
        if ($TargetLevels -contains $level) {
            $map[$level] = $row
        }
    }

    $missing = @($TargetLevels | Where-Object { -not $map.ContainsKey($_) })
    if ($missing.Count -gt 0) {
        throw "Faltan niveles en la curva base: $($missing -join ', ')"
    }

    return $map
}

$GrowthMap = Import-GrowthData -Path $GrowthCsv -TargetLevels $Levels

$StatNames = @("hp","mana","ataque","defensa","velocidadAtaque","ataqueCritico","danoCritico","suerte","destreza")

function Build-GuidMap {
    param([string] $AssetsDirectory)

    $map = @{}
    Get-ChildItem -Path $AssetsDirectory -Filter "*.meta" -Recurse | ForEach-Object {
        $metaPath = $_.FullName
        $assetPath = $metaPath.Substring(0, $metaPath.Length - 5)
        if (-not (Test-Path -LiteralPath $assetPath)) {
            return
        }

        $guid = $null
        foreach ($line in Get-Content -Path $metaPath -TotalCount 20) {
            if ($line -match '^\s*guid:\s*([0-9a-fA-F]+)') {
                $guid = $matches[1].ToLowerInvariant()
                break
            }
        }

        if ([string]::IsNullOrWhiteSpace($guid)) {
            return
        }

        if (-not $map.ContainsKey($guid)) {
            $map[$guid] = $assetPath
        }
    }

    return $map
}

function Get-AssetGuids {
    param([string] $AssetPath)

    if (-not (Test-Path -LiteralPath $AssetPath)) {
        throw "No se encontró Item Database en $AssetPath"
    }

    $content = Get-Content -Path $AssetPath -Raw
    $matches = [regex]::Matches($content, 'guid:\s*([0-9a-fA-F]{32})')
    return $matches | ForEach-Object { $_.Groups[1].Value.ToLowerInvariant() }
}

function Parse-YamlFields {
    param([string] $AssetPath)

    $result = @{}
    Get-Content -Path $AssetPath | ForEach-Object {
        if ($_ -notlike "*:*") { return }
        $parts = $_.Split(":", 2)
        $key = $parts[0].Trim()
        if ([string]::IsNullOrWhiteSpace($key)) { return }
        $value = ""
        if ($parts.Count -gt 1) {
            $value = $parts[1].Trim()
        }
        $result[$key] = $value
    }
    return $result
}

function Convert-ToValue {
    param([string] $Raw)

    if ([string]::IsNullOrWhiteSpace($Raw)) {
        return $null
    }

    $trimmed = $Raw.Trim()
    if ($trimmed.StartsWith('"') -and $trimmed.EndsWith('"')) {
        $trimmed = $trimmed.Trim('"')
    }

    if ($trimmed -match '^-?\d+$') {
        return [int]$trimmed
    }

    return $trimmed
}

function Get-IntField {
    param([hashtable] $Parsed, [string] $Key)

    if (-not $Parsed.ContainsKey($Key)) {
        return 0
    }

    $value = Convert-ToValue $Parsed[$Key]
    if ($value -is [int]) {
        return $value
    }
    return 0
}

function Normalize-Text {
    param([string] $Text)

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return ""
    }

    $normalized = $Text.Normalize([System.Text.NormalizationForm]::FormD)
    $builder = New-Object System.Text.StringBuilder
    foreach ($char in $normalized.ToCharArray()) {
        $category = [System.Globalization.CharUnicodeInfo]::GetUnicodeCategory($char)
        if ($category -eq [System.Globalization.UnicodeCategory]::NonSpacingMark) {
            continue
        }
        $builder.Append([char]::ToLowerInvariant($char)) | Out-Null
    }
    return $builder.ToString()
}

$OrderedSetDefinitions = @(
    @{ Key = "Aprendiz (Tiro)"; Token = "tiro"; Tiers = @("rudimentum","veteranum","imperium") },
    @{ Key = "Arcano (Augur)"; Token = "augur"; Tiers = @("omen","ritus","oraculum") },
    @{ Key = "Cazador (Explorator)"; Token = "explorator"; Tiers = @("peregrinus","venator","praedator") },
    @{ Key = "Conquistador (Legionarius)"; Token = "legionarius"; Tiers = @("cohors","centuria","legio") },
    @{ Key = "Héroe (Roma)"; Token = "roma"; Tiers = @("virtus","honor","gloria") },
    @{ Key = "Fantasma (Speculator)"; Token = "speculator"; Tiers = @("umbra","silentium","occultus") },
    @{ Key = "Titán (Colossus)"; Token = "colossus"; Tiers = @("moles","titanus","colossus") },
    @{ Key = "Serafín (Pontifex)"; Token = "pontifex"; Tiers = @("sacrum","divinus","sanctus") },
    @{ Key = "Abismo (Infernum)"; Token = "infernum"; Tiers = @("limbus","inferus","abyssus") },
    @{ Key = "Apocalipsis (Ultima Roma)"; Token = "ultima roma"; Tiers = @("exitium","cataclysmus","aeternitas") }
)

$SetDefinitions = @()
$multiplierBase = 1
foreach ($def in $OrderedSetDefinitions) {
    $SetDefinitions += [pscustomobject]@{
        DisplayName = $def.Key
        SetToken = Normalize-Text $def.Token
        TierTokens = $def.Tiers | ForEach-Object { Normalize-Text $_ }
        BaseMultiplier = $multiplierBase
    }
    $multiplierBase += $def.Tiers.Count
}

function Resolve-SetInfo {
    param(
        [string] $ItemName,
        [string] $AssetName
    )

    $normalized = Normalize-Text ("$ItemName|$AssetName")

    foreach ($definition in $SetDefinitions) {
        if (-not $normalized.Contains($definition.SetToken)) {
            continue
        }

        for ($tierIndex = 0; $tierIndex -lt $definition.TierTokens.Count; $tierIndex++) {
            $tierToken = $definition.TierTokens[$tierIndex]
            if ($normalized.Contains($tierToken)) {
                return [pscustomobject]@{
                    SetDisplay = $definition.DisplayName
                    Tier = $definition.TierTokens[$tierIndex]
                    TierIndex = $tierIndex
                    Multiplier = $definition.BaseMultiplier + $tierIndex
                }
            }
        }

        return [pscustomobject]@{
            SetDisplay = $definition.DisplayName
            Tier = "desconocido"
            TierIndex = 0
            Multiplier = $definition.BaseMultiplier
        }
    }

    return [pscustomobject]@{
        SetDisplay = "Otros"
        Tier = "desconocido"
        TierIndex = 0
        Multiplier = 1
    }
}

$guidMap = Build-GuidMap -AssetsDirectory $AssetsDir
$itemGuids = Get-AssetGuids -AssetPath $ItemDatabasePath

$items = @()
$missingAssets = @()

foreach ($guid in $itemGuids) {
    if (-not $guidMap.ContainsKey($guid)) {
        $missingAssets += $guid
        continue
    }

    $assetPath = $guidMap[$guid]
    $parsed = Parse-YamlFields -AssetPath $assetPath
    if (-not $parsed.ContainsKey("itemName")) {
        continue
    }

    $itemName = [string](Convert-ToValue $parsed["itemName"])
    $assetName = [System.IO.Path]::GetFileNameWithoutExtension($assetPath)
    $setInfo = Resolve-SetInfo -ItemName $itemName -AssetName $assetName

    $item = [pscustomobject]@{
        ItemName = $itemName
        AssetName = $assetName
        AssetPath = Get-RelativePath -BasePath $RootDir -FullPath $assetPath
        ItemType = Convert-ToValue $parsed["itemType"]
        SetDisplay = $setInfo.SetDisplay
        Tier = $setInfo.Tier
        Multiplier = $setInfo.Multiplier
        Stats = @{
            "hp" = Get-IntField -Parsed $parsed -Key "hp"
            "mana" = Get-IntField -Parsed $parsed -Key "mana"
            "ataque" = Get-IntField -Parsed $parsed -Key "ataque"
            "defensa" = Get-IntField -Parsed $parsed -Key "defensa"
            "velocidadAtaque" = Get-IntField -Parsed $parsed -Key "velocidadAtaque"
            "ataqueCritico" = Get-IntField -Parsed $parsed -Key "ataqueCritico"
            "danoCritico" = Get-IntField -Parsed $parsed -Key "danoCritico"
            "suerte" = Get-IntField -Parsed $parsed -Key "suerte"
            "destreza" = Get-IntField -Parsed $parsed -Key "destreza"
        }
    }

    $items += $item
}

if ($missingAssets.Count -gt 0) {
    Write-Warning ("GUIDs sin asset: " + ($missingAssets -join ", "))
}

$rowsBySet = @{}

foreach ($item in $items) {
    foreach ($level in $Levels) {
        $bonusRow = $GrowthMap[$level]
        $row = [ordered]@{
            ItemName = $item.ItemName
            AssetName = $item.AssetName
            ItemType = $item.ItemType
            Set = $item.SetDisplay
            Tier = $item.Tier
            SetMultiplier = $item.Multiplier
            Level = $level
        }

        foreach ($stat in $StatNames) {
            $baseValue = $item.Stats[$stat]
            $bonusProperty = "${stat}_bonus"
            $bonus = 0
            if ($bonusRow.PSObject.Properties.Match($bonusProperty).Count -gt 0) {
                $bonus = [int]$bonusRow.$bonusProperty
            }
            $row[$stat] = $baseValue + ($bonus * $item.Multiplier)
        }

        if (-not $rowsBySet.ContainsKey($item.SetDisplay)) {
            $rowsBySet[$item.SetDisplay] = @()
        }
        $rowsBySet[$item.SetDisplay] += [pscustomobject]$row
    }
}

$outputSummary = @()

foreach ($setName in $rowsBySet.Keys) {
    $fileSafe = $setName.ToLowerInvariant().Replace(" ", "_").Replace("(", "").Replace(")", "").Replace(".", "")
    $filePath = Join-Path $OutputDir "set_${fileSafe}.csv"
    $rowsBySet[$setName] | Sort-Object ItemName, Level | Export-Csv -Path $filePath -NoTypeInformation -Encoding UTF8
    $outputSummary += [pscustomobject]@{
        Set = $setName
        File = Get-RelativePath -BasePath $RootDir -FullPath $filePath
        ItemCount = ($rowsBySet[$setName] | Select-Object -ExpandProperty ItemName -Unique).Count
    }
}

$summaryPath = Join-Path $OutputDir "summary.csv"
$outputSummary | Export-Csv -Path $summaryPath -NoTypeInformation -Encoding UTF8

Write-Host "Tablas generadas en $OutputDir"
