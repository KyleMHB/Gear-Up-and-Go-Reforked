param(
    [string]$DeployRoot = "E:\Steam\steamapps\common\RimWorld\Mods\Gear Up and Go Reforked"
)

$ErrorActionPreference = "Stop"

$sourceRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$runtimeDirs = @("About", "Defs", "Languages", "Textures")

foreach ($dir in $runtimeDirs) {
    $source = Join-Path $sourceRoot $dir
    $target = Join-Path $DeployRoot $dir

    if (-not (Test-Path -LiteralPath $source)) {
        throw "Missing runtime source directory: $source"
    }

    if (Test-Path -LiteralPath $target) {
        Remove-Item -LiteralPath $target -Recurse -Force
    }

    Copy-Item -LiteralPath $source -Destination $target -Recurse -Force
}

New-Item -ItemType Directory -Path (Join-Path $DeployRoot "Assemblies") -Force | Out-Null

Write-Host "Deployed runtime files to $DeployRoot"
