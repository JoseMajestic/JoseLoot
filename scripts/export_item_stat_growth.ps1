Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent $PSScriptRoot
$ReportDir = Join-Path $RootDir "reports"
New-Item -ItemType Directory -Force -Path $ReportDir | Out-Null

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

$OutputCsv = Join-Path $ReportDir "item_stat_growth.csv"
$OutputJson = Join-Path $ReportDir "item_stat_growth_summary.json"

$MaxLevel = 1000
$BaseCurveLength = $MaxLevel - 1
$TargetTotalBonus = 1000
$MaxIncrementPerLevel = 3
$DefaultMaxZeroRun = 2
$ExtraZeroRunForFirstStat = 3
$RandomSeed = 20251230
$StatCount = 9

$StatNames = @(
    "hp",
    "mana",
    "ataque",
    "defensa",
    "velocidadAtaque",
    "ataqueCritico",
    "danoCritico",
    "suerte",
    "destreza"
)

function Get-RandomRangeInclusive {
    param(
        [System.Random] $Rng,
        [int] $Min,
        [int] $Max
    )

    if ($Min -gt $Max) {
        return $Min
    }

    return $Rng.Next($Min, $Max + 1)
}

function Generate-IncrementCurve {
    param([int] $StatIndex)

    $increments = New-Object int[] $BaseCurveLength
    $statSeed = $RandomSeed + ($StatIndex + 1) * 9973
    $rng = [System.Random]::new($statSeed)
    $remainingSum = $TargetTotalBonus
    $zeroRun = 0
    $maxZeroRun = if ($StatIndex -eq 0) { $ExtraZeroRunForFirstStat } else { $DefaultMaxZeroRun }

    for ($i = 0; $i -lt $BaseCurveLength; $i++) {
        $slotsLeft = $BaseCurveLength - $i
        $minValue = [Math]::Max(0, $remainingSum - ($slotsLeft - 1) * $MaxIncrementPerLevel)
        $maxValue = [Math]::Min($MaxIncrementPerLevel, $remainingSum)

        $adjustedMin = if ($zeroRun -ge $maxZeroRun) {
            [Math]::Max(1, $minValue)
        } else {
            $minValue
        }

        $preferred = ($i + $StatIndex) % ($MaxIncrementPerLevel + 1)
        $preferred = [Math]::Max($adjustedMin, [Math]::Min($preferred, $maxValue))

        $value = Get-RandomRangeInclusive -Rng $rng -Min $adjustedMin -Max $maxValue

        if ([Math]::Abs($value - $preferred) -gt 1 -and
            $preferred -ge $adjustedMin -and
            $preferred -le $maxValue -and
            $rng.NextDouble() -lt 0.35) {
            $value = $preferred
        }

        $maxFuture = ($slotsLeft - 1) * $MaxIncrementPerLevel
        while ($remainingSum - $value -gt $maxFuture -and $value -gt $adjustedMin) {
            $value--
        }

        $increments[$i] = $value
        $remainingSum -= $value
        if ($value -eq 0) {
            $zeroRun++
        } else {
            $zeroRun = 0
        }
    }

    if ($remainingSum -ne 0) {
        $increments[$BaseCurveLength - 1] += $remainingSum
    }

    return $increments
}

function Build-CumulativeCurve {
    param([int[]] $Increments)

    $cumulative = New-Object int[] ($MaxLevel + 1)
    $cumulative[1] = 0

    for ($level = 2; $level -le $MaxLevel; $level++) {
        $cumulative[$level] = $cumulative[$level - 1] + $Increments[$level - 2]
    }

    return $cumulative
}

$cumulativeByStat = @{}

for ($statIndex = 0; $statIndex -lt $StatCount; $statIndex++) {
    $increments = Generate-IncrementCurve -StatIndex $statIndex
    $cumulativeByStat[$statIndex] = Build-CumulativeCurve -Increments $increments
}

$rows = @()
for ($level = 1; $level -le 999; $level++) {
    $row = [ordered]@{ Level = $level }
    for ($statIndex = 0; $statIndex -lt $StatCount; $statIndex++) {
        $statName = $StatNames[$statIndex]
        $row["${statName}_bonus"] = $cumulativeByStat[$statIndex][$level]
    }
    $rows += [pscustomobject]$row
}

$rows | Export-Csv -Path $OutputCsv -NoTypeInformation -Encoding UTF8

$summary = [ordered]@{
    base_curve_csv = Get-RelativePath -BasePath $RootDir -FullPath $OutputCsv
    description = "Bonificación acumulada base por nivel (sin multiplicador de set). Multiplica cada columna por el SetMultiplier correspondiente para obtener la bonificación final de un ItemData."
    stat_names = $StatNames
    max_level = 999
}

$summary | ConvertTo-Json -Depth 4 | Set-Content -Path $OutputJson -Encoding UTF8

Write-Host "Tabla generada en $($summary.base_curve_csv)"
