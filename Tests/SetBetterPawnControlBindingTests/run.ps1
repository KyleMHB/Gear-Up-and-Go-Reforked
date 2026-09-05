$ErrorActionPreference = 'Stop'

$project = Join-Path $PSScriptRoot 'SetBetterPawnControlBindingTests.csproj'
dotnet run --project $project --configuration Release --no-restore
