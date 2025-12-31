Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent $PSScriptRoot
$AssetsDir = Join-Path $RootDir "Assets"
$ItemDatabasePath = Join-Path $RootDir "Assets\Scripts\Item Database.asset"
$EnemyDatabasePath = Join-Path $RootDir "Assets\Databases\Enemy Database.asset"
$ReportDir = Join-Path $RootDir "reports"

$ItemTypeNames = @{
    0 = "Montura"
    1 = "Casco"
    2 = "Collar"
    3 = "Arma"
    4 = "Armadura"
    5 = "Escudo"
    6 = "Guantes"
    7 = "Cinturón"
    8 = "Anillo"
    9 = "Botas"
    10 = "Otros"
}

$ItemNumericFields = @(
    "price",
    "hp",
    "mana",
    "ataque",
    "defensa",
    "velocidadAtaque",
    "ataqueCritico",
    "danoCritico",
    "suerte",
    "destreza",
    "nivel"
)

$ItemScaledFieldSuffix = "_scaled"

$EnemyNumericFields = @(
    "hp",
    "ataque",
    "defensa",
    "velocidadAtaque",
    "ataqueCritico",
    "danoCritico",
    "suerte",
    "destreza",
    "rewardCoins",
    "experienceReward",
    "requiredLevel",
    "level"
)

$CommonComparisonFields = @(
    "hp",
    "ataque",
    "defensa",
    "velocidadAtaque",
    "ataqueCritico",
    "danoCritico",
    "suerte",
    "destreza"
)

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
        throw "No se encontró el archivo: $AssetPath"
    }

    $content = Get-Content -Path $AssetPath -Raw
    $matches = [regex]::Matches($content, 'guid:\s*([0-9a-fA-F]{32})')
    $matches | ForEach-Object { $_.Groups[1].Value.ToLowerInvariant() }
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
    param([string] $RawValue)

    if ([string]::IsNullOrWhiteSpace($RawValue)) {
        return $null
    }

    $trimmed = $RawValue.Trim()
    if ($trimmed.StartsWith('"') -and $trimmed.EndsWith('"')) {
        $trimmed = $trimmed.Trim('"')
    }

    if ($trimmed -match '^-?\d+$') {
        return [int]$trimmed
    }

    if ($trimmed -match '^(true|false)$') {
        return [bool]::Parse($trimmed)
    }

    return $trimmed
}

function Get-IntField {
    param(
        [hashtable] $Parsed,
        [string] $Key
    )

    if (-not $Parsed.ContainsKey($Key)) {
        return $null
    }

    $value = Convert-ToValue $Parsed[$Key]
    if ($value -is [int]) {
        return $value
    }
    return $null
}

function Build-ItemRecords {
    param(
        [hashtable] $GuidMap
    )

    $records = @()
    $missing = @()

    $guids = Get-AssetGuids -AssetPath $ItemDatabasePath
    foreach ($guid in $guids) {
        if (-not $GuidMap.ContainsKey($guid)) {
            $missing += $guid
            continue
        }

        $assetPath = $GuidMap[$guid]
        $parsed = Parse-YamlFields -AssetPath $assetPath

        $itemTypeValue = Convert-ToValue $parsed["itemType"]
        $itemTypeName = $itemTypeValue
        if ($itemTypeValue -is [int]) {
            $itemTypeName = $ItemTypeNames[$itemTypeValue]
            if (-not $itemTypeName) {
                $itemTypeName = "Desconocido ($itemTypeValue)"
            }
        }

        $record = [ordered]@{
            guid = $guid
            asset_path = Get-RelativePath -BasePath $RootDir -FullPath $assetPath
            itemName = Convert-ToValue $parsed["itemName"]
            rareza = Convert-ToValue $parsed["rareza"]
            itemTypeIndex = $itemTypeValue
            itemType = $itemTypeName
        }

        foreach ($field in $ItemNumericFields) {
            $record[$field] = Get-IntField -Parsed $parsed -Key $field
        }

        $records += [pscustomobject]$record
    }

    $sorted = $records | Sort-Object -Property itemName
    return [pscustomobject]@{
        Records = $sorted
        MissingGuids = $missing
    }
}

function Build-EnemyRecords {
    param(
        [hashtable] $GuidMap
    )

    $records = @()
    $missing = @()

    $guids = Get-AssetGuids -AssetPath $EnemyDatabasePath
    foreach ($guid in $guids) {
        if (-not $GuidMap.ContainsKey($guid)) {
            $missing += $guid
            continue
        }

        $assetPath = $GuidMap[$guid]
        $parsed = Parse-YamlFields -AssetPath $assetPath

        $record = [ordered]@{
            guid = $guid
            asset_path = Get-RelativePath -BasePath $RootDir -FullPath $assetPath
            enemyName = Convert-ToValue $parsed["enemyName"]
            description = Convert-ToValue $parsed["description"]
        }

        foreach ($field in $EnemyNumericFields) {
            $record[$field] = Get-IntField -Parsed $parsed -Key $field
        }

        $records += [pscustomobject]$record
    }

    $sorted = $records | Sort-Object -Property enemyName
    return [pscustomobject]@{
        Records = $sorted
        MissingGuids = $missing
    }
}

function Get-NumericSummary {
    param(
        [System.Collections.IEnumerable] $Records,
        [string[]] $Fields
    )

    $summary = @{}
    foreach ($field in $Fields) {
        $values = @()
        foreach ($record in $Records) {
            $value = $record.$field
            if ($value -is [int]) {
                $values += $value
            }
        }

        if ($values.Count -eq 0) {
            continue
        }

        $positiveValues = $values | Where-Object { $_ -gt 0 }
        $positiveMin = $null
        if ($positiveValues.Count -gt 0) {
            $positiveMin = ($positiveValues | Measure-Object -Minimum).Minimum
        }

        $measure = $values | Measure-Object -Average -Minimum -Maximum
        $summary[$field] = @{
            count = $values.Count
            min = $measure.Minimum
            minPositive = $positiveMin
            max = $measure.Maximum
            avg = [Math]::Round($measure.Average, 2)
        }
    }

    return $summary
}

function Get-ScaledValue {
    param(
        [int] $Value,
        [hashtable] $Stats
    )

    if ($null -eq $Value) {
        return $null
    }

    if ($Value -le 0) {
        return 0
    }

    if (-not $Stats) {
        return $Value
    }

    $minPositive = $Stats.minPositive
    $max = $Stats.max

    if ($null -eq $minPositive -or $max -le $minPositive) {
        return [Math]::Min(666, [Math]::Max(1, $Value))
    }

    $scaled = 1 + (($Value - $minPositive) * 665.0 / ($max - $minPositive))
    return [int][Math]::Round([Math]::Min(666, [Math]::Max(1, $scaled)))
}

New-Item -ItemType Directory -Force -Path $ReportDir | Out-Null

Write-Host "Generando mapa de GUIDs..."
$guidMap = Build-GuidMap -AssetsDirectory $AssetsDir

Write-Host "Procesando Item Database..."
$itemResult = Build-ItemRecords -GuidMap $guidMap
$itemRecords = $itemResult.Records

Write-Host "Procesando Enemy Database..."
$enemyResult = Build-EnemyRecords -GuidMap $guidMap
$enemyRecords = $enemyResult.Records

$itemSummary = Get-NumericSummary -Records $itemRecords -Fields $ItemNumericFields
$enemySummary = Get-NumericSummary -Records $enemyRecords -Fields $EnemyNumericFields

$scaledFieldNames = @()
foreach ($field in $ItemNumericFields) {
    $stats = $itemSummary[$field]
    if (-not $stats) {
        continue
    }

    $scaledName = "$field$ItemScaledFieldSuffix"
    $scaledFieldNames += $scaledName

    foreach ($record in $itemRecords) {
        $value = $record.$field
        $scaledValue = Get-ScaledValue -Value $value -Stats $stats
        Add-Member -InputObject $record -NotePropertyName $scaledName -NotePropertyValue $scaledValue -Force
    }
}

$itemCsvHeaders = @(
    "itemName",
    "itemType",
    "itemTypeIndex",
    "rareza"
) + $ItemNumericFields + $scaledFieldNames + @(
    "asset_path",
    "guid"
)

$enemyCsvHeaders = @(
    "enemyName",
    "description"
) + $EnemyNumericFields + @(
    "asset_path",
    "guid"
)

$itemCsvPath = Join-Path $ReportDir "item_database_stats.csv"
$enemyCsvPath = Join-Path $ReportDir "enemy_database_stats.csv"
$summaryPath = Join-Path $ReportDir "database_stats_summary.json"

$itemRecords | Select-Object -Property $itemCsvHeaders | Export-Csv -Path $itemCsvPath -NoTypeInformation -Encoding UTF8
$enemyRecords | Select-Object -Property $enemyCsvHeaders | Export-Csv -Path $enemyCsvPath -NoTypeInformation -Encoding UTF8

$comparison = @{}
foreach ($field in $CommonComparisonFields) {
    $comparison[$field] = @{
        items = $itemSummary[$field]
        enemies = $enemySummary[$field]
    }
}

$payload = [ordered]@{
    item_count = $itemRecords.Count
    enemy_count = $enemyRecords.Count
    item_numeric_summary = $itemSummary
    enemy_numeric_summary = $enemySummary
    item_scaled_fields = $scaledFieldNames
    comparison = $comparison
    missing_item_guids = $itemResult.MissingGuids
    missing_enemy_guids = $enemyResult.MissingGuids
    item_csv = Get-RelativePath -BasePath $RootDir -FullPath $itemCsvPath
    enemy_csv = Get-RelativePath -BasePath $RootDir -FullPath $enemyCsvPath
}

$payload | ConvertTo-Json -Depth 6 | Set-Content -Path $summaryPath -Encoding UTF8

Write-Host "Items exportados:" $itemRecords.Count
Write-Host "Enemigos exportados:" $enemyRecords.Count
if ($itemResult.MissingGuids.Count -gt 0) {
    Write-Warning ("GUIDs de ítems no encontrados: " + ($itemResult.MissingGuids -join ", "))
}
if ($enemyResult.MissingGuids.Count -gt 0) {
    Write-Warning ("GUIDs de enemigos no encontrados: " + ($enemyResult.MissingGuids -join ", "))
}
Write-Host "CSV de ítems: $((Get-RelativePath -BasePath $RootDir -FullPath $itemCsvPath))"
Write-Host "CSV de enemigos: $((Get-RelativePath -BasePath $RootDir -FullPath $enemyCsvPath))"
Write-Host "Resumen JSON: $((Get-RelativePath -BasePath $RootDir -FullPath $summaryPath))"
