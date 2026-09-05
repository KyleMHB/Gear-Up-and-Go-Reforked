# Testing

## Better Pawn Control compatibility

The dependency-free regression harnesses cover both Better Pawn Control reflection shapes used by Gear Up and Go:

- `SetBetterPawnControlBindingTests` exposes `BetterPawnControlForked.AssignManager.policies` as an inherited static property.
- `SetBetterPawnControlLegacyBindingTests` exposes `BetterPawnControl.AssignManager.policies` as the legacy static field.

Run both harnesses from the repository root:

```powershell
dotnet run --project .\Tests\SetBetterPawnControlBindingTests\SetBetterPawnControlBindingTests.csproj --configuration Release
dotnet run --project .\Tests\SetBetterPawnControlLegacyBindingTests\SetBetterPawnControlLegacyBindingTests.csproj --configuration Release
```

Expected result:

```text
PASS: Better Pawn Control inherited-property binding
PASS: Better Pawn Control legacy-field binding
```

The harnesses compile `Source\SetBetterPawnControl.cs` with small local Verse and Better Pawn Control stand-ins. They exercise the public integration methods for package detection, policy listing, explicit policy selection, and active-policy lookup without requiring RimWorld to be running.

Run the normal mod build separately when RimWorld reference files are available:

```powershell
dotnet build .\Source\GearUpAndGo.csproj -c Release /p:UseSharedCompilation=false
```

The compatibility patch was deployed successfully on 2026-09-05. The Release build completed with 0 warnings and 0 errors; source and runtime `GearUpAndGo.dll` files are version `1.0.4.0` with matching SHA-256 hashes, and no runtime PDB files were present. The user-confirmed RimWorld Dev Quicktest passed for this change set.
