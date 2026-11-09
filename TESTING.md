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

## In-game developer tests

`DevTests/` uses the shared runner in the sibling `RimWorldDevTests` checkout. Batch `guag-gear` checks these behaviours:

- **`guag.defs`:** the job def, English text keys, command textures, the `Pawn.GetGizmos` patch and `GearUpPolicyComp` all load.
- **`guag.gizmo`:** a player-controlled colonist shows the Gear & Go command.
- **`guag.gear-and-go`:** a naked colonist ordered to a destination equips a nearby duster first, then drafts and arrives within 3 cells.
- **`guag.iteration-cap`:** a pass at the 16-iteration cap stops without drafting, moving or requeueing.
- **`guag.reload`:** the geared colonist and the policy component survive a save and reload.

The cases are mechanical. They order the command's job directly and do not click the gizmo or the targeter. They run without Better Pawn Control, so no policy switch is covered.

Run these from the repository root:

```powershell
dotnet build DevTests/GearUpAndGo.DevTests.csproj -c Release
python tools/guag_test.py run guag-gear --seed 1
python tools/guag_test.py close-session
python tools/guag_test.py release-check
```

The installed mod must match the current `Assemblies/GearUpAndGo.dll`, so run `deploy.ps1` first if it has changed. `deploy.ps1` copies only the runtime folders, so `DevTests/` and `work/` never ship.

### 2026-11-09 run

- **Environment:** RimWorld 1.7.5000 rev1275, seed 1. Active mods: Harmony, Core and Gear Up and Go Reforked only. The user's `ModsConfig.xml` was backed up to `ModsConfig.xml.before-guag.bak`, replaced for the run and restored afterwards; its SHA-256 matched the original. The installed copy was updated from the working tree by copying the runtime folders. `Source/GearUpAndGo.csproj` does not build on this machine because its `HintPath`s point at `RimWorldWin64_Data\Managed`, which the 1.7 install no longer has. The committed DLL already matched the installed DLL byte for byte.
- **First run, `20261109-174935-guag-gear-s1-9802bc8d`:** 2 passed, 3 failed. The failures were setup errors, not mod defects:
  - The duster lay loose on the ground. Vanilla `JobGiver_OptimizeApparel` only picks apparel that is in storage (see GAME.md), so the pawn went to the destination without gearing up. The reload case then failed as a consequence.
  - The iteration-cap case treated normal wandering after the job ended as movement.
- **Corrected run, `20261109-175116-guag-gear-s1-0f8fc768`:** 5 of 5 passed. In the stockpile setup the duster was worn at tick 240, the pawn was drafted at tick 241 and finished on the target cell. The capped pass never drafted the pawn, never ordered a goto and left no job queued. After the save and reload the pawn was still wearing the duster and still drafted.
- **Not covered:** Better Pawn Control policy switching, Combat Extended loadouts, Outfit Stands Plus integration, clicking the actual gizmo and targeter, unreachable targets, and the full mod list.
