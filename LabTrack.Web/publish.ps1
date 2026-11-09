# Genera el paquete listo para subir a MonsterASP.NET
# Uso:  .\publish.ps1
#       .\publish.ps1 -Zip          (también crea LabTrack.Web.zip)
#       .\publish.ps1 -Runtime win-x64

param(
    [string]$OutputPath = (Join-Path $PSScriptRoot 'publish'),
    [string]$Runtime = '',
    [switch]$Zip
)

$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'LabTrack.Web.csproj'

if (-not (Test-Path $project)) {
    throw "No se encontró el proyecto en $project"
}

if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force
}

$publishArgs = @(
    'publish', $project,
    '-c', 'Release',
    '-o', $OutputPath,
    '--no-self-contained'
)

if (-not [string]::IsNullOrWhiteSpace($Runtime)) {
    $publishArgs += @('-r', $Runtime)
}

Write-Host "Publicando en $OutputPath ..." -ForegroundColor Cyan
& dotnet @publishArgs

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish falló con código $LASTEXITCODE"
}

$webConfig = Join-Path $OutputPath 'web.config'
if (-not (Test-Path $webConfig)) {
    throw "La publicación no generó web.config. Revise el proyecto."
}

Write-Host "Publicación completada: $OutputPath" -ForegroundColor Green

if ($Zip) {
    $zipPath = Join-Path $PSScriptRoot 'LabTrack.Web.zip'
    if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
    Compress-Archive -Path (Join-Path $OutputPath '*') -DestinationPath $zipPath -Force
    Write-Host "ZIP listo para subir: $zipPath" -ForegroundColor Green
}
