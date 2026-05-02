$ErrorActionPreference = "Stop"

$toolPath = Join-Path $PSScriptRoot "..\_Shared\RimWorldModTools.ps1"
. $toolPath

$repoRoot = $PSScriptRoot
$projectPath = Join-Path $repoRoot "Source\GearUpAndGo.csproj"
$targetRoot = Get-RimWorldModTargetPath -ModName "Gear Up and Go Reforked"

Invoke-RimWorldDotNetBuild -ProjectPath $projectPath -Configuration "Release"
Sync-RimWorldModFolders -SourceRoot $repoRoot -TargetRoot $targetRoot -Folders @("About", "Defs", "Languages", "Textures")

$stalePdb = Join-Path $targetRoot "Assemblies\GearUpAndGo.pdb"
if (Test-Path -LiteralPath $stalePdb) {
    Remove-Item -LiteralPath $stalePdb -Force
}

Write-Host "Deployed Gear Up and Go Reforked to $targetRoot"
