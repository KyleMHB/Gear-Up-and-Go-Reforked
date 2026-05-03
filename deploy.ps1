$ErrorActionPreference = "Stop"

$toolPath = Join-Path $PSScriptRoot "..\_Shared\RimWorldModTools.ps1"
. $toolPath

Invoke-RimWorldModDeploy `
    -ModName "Gear Up and Go Reforked" `
    -SourceRoot $PSScriptRoot `
    -BuildPath (Join-Path $PSScriptRoot "Source\GearUpAndGo.csproj") `
    -Configuration "Release" `
    -Folders @("About", "Assemblies", "Defs", "Languages", "Textures") `
    -RemoveFilePatterns @("*.pdb")
