<#
.SYNOPSIS
    Aplica los valores escalados (campo *_scaled) del reporte a los ItemData.
.DESCRIPTION
    Lee reports/item_database_stats.csv, busca los campos terminados en "_scaled"
    y reemplaza los valores numéricos correspondientes en cada archivo .asset.
    Solo modifica los ítems presentes en el CSV (actualmente 283). Los que no tengan
    fila en el reporte permanecerán sin cambios.
#>

param(
    [string] $ReportCsv = "reports\item_database_stats.csv",
    [string] $ScaledSuffix = "_scaled"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent $PSScriptRoot
$ReportPath = Join-Path $RootDir $ReportCsv

if (-not (Test-Path -LiteralPath $ReportPath)) {
    throw "No se encontró el archivo de reporte: $ReportPath. Ejecuta primero export_database_stats.ps1."
}

$records = Import-Csv -Path $ReportPath
if ($records.Count -eq 0) {
    throw "El CSV está vacío. Asegúrate de haber generado el reporte correctamente."
}

# Campos que deben normalizarse (coinciden con los generados en el reporte).
$statFields = @(
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

$updatedCount = 0
$skippedCount = 0
$missingAssets = @()
$fieldsWithoutScaled = @()

foreach ($row in $records) {
    $relativePath = $row.asset_path
    if ([string]::IsNullOrWhiteSpace($relativePath)) {
        $skippedCount++
        continue
    }

    $assetPath = Join-Path $RootDir $relativePath
    if (-not (Test-Path -LiteralPath $assetPath)) {
        $missingAssets += $relativePath
        continue
    }

    $lines = Get-Content -Path $assetPath -Encoding UTF8
    $modified = $false

    foreach ($field in $statFields) {
        $scaledColumn = "$field$ScaledSuffix"
        $scaledValue = $row.$scaledColumn

        if ([string]::IsNullOrWhiteSpace($scaledValue)) {
            $fieldsWithoutScaled += "$relativePath::$scaledColumn"
            continue
        }

        $pattern = "^\s*$field\s*:"
        $newLine = $null

        for ($i = 0; $i -lt $lines.Count; $i++) {
            if ($lines[$i] -match $pattern) {
                $indentMatch = [regex]::Match($lines[$i], "^(\s*)")
                $indent = $indentMatch.Groups[1].Value
                $lines[$i] = "$indent${field}: $scaledValue"
                $modified = $true
                $newLine = $true
                break
            }
        }

        if (-not $newLine) {
            Write-Warning "No se encontró el campo '$field' en $relativePath"
        }
    }

    if ($modified) {
        Set-Content -Path $assetPath -Value $lines -Encoding UTF8
        $updatedCount++
    }
}

Write-Host "Items actualizados:" $updatedCount
Write-Host "Items omitidos (sin fila válida):" $skippedCount

if ($missingAssets.Count -gt 0) {
    Write-Warning "No se encontraron los siguientes archivos:"
    $missingAssets | Sort-Object -Unique | ForEach-Object { Write-Warning "  $_" }
}

if ($fieldsWithoutScaled.Count -gt 0) {
    Write-Warning "Campos sin valor escalado en el CSV:"
    $fieldsWithoutScaled | Sort-Object -Unique | ForEach-Object { Write-Warning "  $_" }
}
