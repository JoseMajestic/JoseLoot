param(
    [string]$ProjectRoot = (Get-Location).Path
)

$itemsFolder = Join-Path $ProjectRoot 'Assets/Items'

$template = @"
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 04862611ff0cbd941bb72b22ae5654f3, type: 3}
  m_Name: PLACEHOLDER_NAME
  m_EditorClassIdentifier: 
  itemName: PLACEHOLDER_ITEMNAME
  itemSprite: {fileID: -1565976107, guid: c88165515191c714f8d198e8fc38135c, type: 3}
  description: PLACEHOLDER_DESCRIPTION
  price: 100
  hp: 0
  mana: 0
  ataque: 0
  defensa: 0
  velocidadAtaque: 0
  ataqueCritico: 0
  danoCritico: 0
  suerte: 0
  destreza: 0
  nivel: 1
  rareza: Comun
  itemType: PLACEHOLDER_ITEMTYPE
"@

$assets = @(
    @{Name="Aprendiz Anillo I"; ItemName="Anillo de Tiro Rudimentum"; Description="Anillo simple que simboliza el inicio del camino marcial."; ItemType=8},
    @{Name="Aprendiz Anillo II"; ItemName="Anillo de Tiro Intermedio"; Description="Anillo que marca el progreso en el entrenamiento."; ItemType=8},
    @{Name="Aprendiz Anillo III"; ItemName="Anillo de Tiro Avanzado"; Description="Anillo que representa el dominio inicial."; ItemType=8},
    @{Name="Aprendiz Collar I"; ItemName="Collar de Tiro Rudimentum"; Description="Collar simple para principiantes."; ItemType=2},
    @{Name="Aprendiz Collar II"; ItemName="Collar de Tiro Intermedio"; Description="Collar que acompaña el crecimiento."; ItemType=2},
    @{Name="Aprendiz Collar III"; ItemName="Collar de Tiro Avanzado"; Description="Collar que refleja experiencia."; ItemType=2},
    @{Name="Aprendiz Montura I"; ItemName="Montura de Tiro Rudimentum"; Description="Montura básica para viajes iniciales."; ItemType=0},
    @{Name="Aprendiz Montura II"; ItemName="Montura de Tiro Intermedio"; Description="Montura que mejora la movilidad."; ItemType=0},
    @{Name="Aprendiz Montura III"; ItemName="Montura de Tiro Avanzado"; Description="Montura que simboliza perseverancia."; ItemType=0},
    @{Name="Cazador Anillo I"; ItemName="Anillo de Explorator Rudimentum"; Description="Anillo para cazadores novatos."; ItemType=8},
    @{Name="Cazador Anillo II"; ItemName="Anillo de Explorator Intermedio"; Description="Anillo que mejora la precisión."; ItemType=8},
    @{Name="Cazador Anillo III"; ItemName="Anillo de Explorator Avanzado"; Description="Anillo que marca a un cazador experimentado."; ItemType=8},
    @{Name="Cazador Collar I"; ItemName="Collar de Explorator Rudimentum"; Description="Collar para cazadores principiantes."; ItemType=2},
    @{Name="Cazador Collar II"; ItemName="Collar de Explorator Intermedio"; Description="Collar que protege en la caza."; ItemType=2},
    @{Name="Cazador Collar III"; ItemName="Collar de Explorator Avanzado"; Description="Collar que honra al cazador."; ItemType=2},
    @{Name="Cazador Montura I"; ItemName="Montura de Explorator Rudimentum"; Description="Montura para rastrear presas."; ItemType=0},
    @{Name="Cazador Montura II"; ItemName="Montura de Explorator Intermedio"; Description="Montura rápida para persecuciones."; ItemType=0},
    @{Name="Cazador Montura III"; ItemName="Montura de Explorator Avanzado"; Description="Montura que domina el terreno."; ItemType=0}
)

foreach ($asset in $assets) {
    $content = $template -replace 'PLACEHOLDER_NAME', $asset.Name -replace 'PLACEHOLDER_ITEMNAME', $asset.ItemName -replace 'PLACEHOLDER_DESCRIPTION', $asset.Description -replace 'PLACEHOLDER_ITEMTYPE', $asset.ItemType
    $fileName = $asset.Name + '.asset'
    $filePath = Join-Path $itemsFolder $fileName
    Set-Content -Path $filePath -Value $content -Encoding UTF8
    
    # Create .meta file
    $metaContent = @"
fileFormatVersion: 2
guid: $([guid]::NewGuid().ToString('N'))
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@  
    $metaPath = $filePath + '.meta'
    Set-Content -Path $metaPath -Value $metaContent -Encoding UTF8
}

Write-Host '18 missing assets recreated with basic stats.' 
