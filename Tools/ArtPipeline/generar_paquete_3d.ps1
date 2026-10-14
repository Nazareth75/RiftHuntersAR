param([int]$Samples=16,[int]$Resolution=800,[string]$Blender='C:\Program Files\Blender Foundation\Blender 5.2\blender.exe')
$ErrorActionPreference='Stop'
$projectRoot=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
Push-Location -LiteralPath $projectRoot
try {
    if((git branch --show-current) -ne 'diseno-visual') { throw 'Ejecutar solamente en diseno-visual' }
    $logRoot=Join-Path $projectRoot 'ArtRenders\PipelineLogs'
    New-Item -ItemType Directory -Path $logRoot -Force | Out-Null
    foreach($asset in @('enemy_fast','enemy_tank','rift_guardian','portal','plasma_blaster','electric_cannon','energy_launcher')) {
        & $Blender --background --factory-startup --python-exit-code 1 --python "Tools/ArtPipeline/generar_$asset.py" -- --samples $Samples --resolution $Resolution *> (Join-Path $logRoot "$asset.log")
        if($LASTEXITCODE -ne 0) { throw "Falló generación: $asset. Ver log." }
    }
    & $Blender --background --factory-startup --python-exit-code 1 --python Tools/ArtPipeline/validar_paquete_3d.py *> (Join-Path $logRoot 'validacion.log')
    if($LASTEXITCODE -ne 0) { throw 'Falló validación del paquete' }
    & $Blender --background --factory-startup --python-exit-code 1 --python Tools/ArtPipeline/generar_lineups.py -- --samples 20 --resolution 1800 *> (Join-Path $logRoot 'lineups.log')
    if($LASTEXITCODE -ne 0) { throw 'Falló generación de lineups' }
    Write-Output 'Paquete 3D generado y validado. EnemyBasic aprobado conservado. No integración Unity.'
} finally { Pop-Location }
