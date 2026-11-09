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

The harnesses compile `Source\SetBetterPawnControl.cs` with small local Verse and Better Pawn Control stand-ins. They exercise the public integration methods for package detection, policy listing, explicit policy selection, and active-policy lookup without requiring RimWorld to be running. They also check that a missing policy reports no switch and saves nothing, and that a switch first saves the colonists' current state under the active policy. The inherited-property stand-in returns `Colonists()` as an `IEnumerable<Pawn>`; the legacy one returns a `List<Pawn>`.

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

The cases are mechanical. They order the command's job directly and do not click the gizmo or the targeter. They run on the `default` mod set without Better Pawn Control, so no policy switch is covered.

Batch `guag-bpc` runs on the `bpc` mod set: Harmony, Core, Better Pawn Control Forked, then Gear Up and Go Reforked. It checks the policy switch:

- **`guag.bpc-bind`:** Gear Up and Go binds to BPC Forked and lists its Assign presets.
- **`guag.bpc-missing`:** asking for a preset that doesn't exist leaves BPC's active preset and the colonist's outfit unchanged, and the Gear & Go switch stays off.
- **`guag.bpc-switch`:** asking for the battle preset with a name in different case activates it, applies its outfit and remembers the previous preset.
- **`guag.bpc-gear`:** under the battle preset the colonist equips a duster, drafts and arrives. Only the battle outfit allows the duster.
- **`guag.bpc-reload`:** the switch, the remembered preset and BPC's active battle preset survive a save and reload.
- **`guag.bpc-restore`:** `Command_GearUpAndGo.End` (the alert click and Shift-click) returns BPC to the previous preset and turns the switch off. The colonist gets back the outfit it had at the switch, a change BPC hadn't saved yet, because Gear & Go saves the current state before switching.

These cases are also mechanical. `Command_GearUpAndGo.Go` reads `Event.current` and the selection, so the batch calls `GearUpPolicyComp.Set` with an explicit preset name, as the right-click menu does, and then orders the job. It doesn't exercise the targeter, the right-click menu, Alt or the mod-settings default preset. BPC's presets are set up through its own Assign-tab flow (save, then load), reached by reflection. BPC's installed copy must match its own repository; the launcher checks only the mod under test, so compare it before a run. `run-all` loads the union of both sets, so there `guag-gear` also runs with BPC loaded.

Run these from the repository root:

```powershell
dotnet build DevTests/GearUpAndGo.DevTests.csproj -c Release
python tools/guag_test.py run guag-gear --seed 1
python tools/guag_test.py run guag-bpc --seed 1
python tools/guag_test.py close-session
python tools/guag_test.py release-check
```

The installed mod must match the current `Assemblies/GearUpAndGo.dll`, so run `deploy.ps1` first if it has changed. `deploy.ps1` copies only the runtime folders, so `DevTests/` and `work/` never ship.

### 2026-11-09 run

- **Environment:** RimWorld 1.7.5000 rev1275, seed 1. Active mods: Harmony, Core and Gear Up and Go Reforked only. The user's `ModsConfig.xml` was backed up to `ModsConfig.xml.before-guag.bak`, replaced for the run and restored afterwards; its SHA-256 matched the original. The installed copy was updated from the working tree by copying the runtime folders. At that point `Source/GearUpAndGo.csproj` did not build, because its `HintPath`s pointed at `RimWorldWin64_Data\Managed`, which the 1.7 install no longer has. The DLL in `Assemblies/` already matched the installed DLL byte for byte.
- **First run, `20261109-174935-guag-gear-s1-9802bc8d`:** 2 passed, 3 failed. The failures were setup errors, not mod defects:
  - The duster lay loose on the ground. Vanilla `JobGiver_OptimizeApparel` only picks apparel that is in storage (see GAME.md), so the pawn went to the destination without gearing up. The reload case then failed as a consequence.
  - The iteration-cap case treated normal wandering after the job ended as movement.
- **Corrected run, `20261109-175116-guag-gear-s1-0f8fc768`:** 5 of 5 passed. In the stockpile setup the duster was worn at tick 240, the pawn was drafted at tick 241 and finished on the target cell. The capped pass never drafted the pawn, never ordered a goto and left no job queued. After the save and reload the pawn was still wearing the duster and still drafted.
- **Rebuilt DLL, `20261109-175820-guag-gear-s1`:** the csproj now builds against the cached RimWorld 1.6 and net472 reference assemblies, as Outfit Stands does. The Release build had 0 warnings and 0 errors, and `deploy.ps1` succeeded. On the same 3-mod list the rebuilt DLL passed 5 of 5 (fresh process, `--quit-on-finish`). The mod list was restored byte for byte.
- **Not covered at that point:** Better Pawn Control policy switching (see the BPC run below), Combat Extended loadouts, Outfit Stands Plus integration, clicking the actual gizmo and targeter, unreachable targets, and the full mod list.

### 2026-11-09 Better Pawn Control run

- **Environment:** RimWorld 1.7.5000 rev1275, seed 1, `bpc` mod set (4 mods), isolated save-data folder `RimWorldDevTests/work/userdata/guag`. Gear Up and Go was deployed from `fcac06d` with `deploy.ps1`. Better Pawn Control Forked was installed from its `devtests-harness` branch with the DLL rebuilt from `master`; all 27 runtime files matched the repository by SHA-256.
- **`20261109-191308-guag-bpc-s1`:** 6 of 6 passed. The switch activated `GUAG Test Battle` from the request `guag test BATTLE` and applied the battle outfit. The colonist then wore the duster, was drafted and stood on the target cell. After the reload the switch was still on and `lastPolicy` was `Default`. `End()` restored `Default` and the peace outfit.
- **`20261109-191336-guag-gear-s1`:** `guag-gear` still passed 5 of 5 on the `default` set after the new batch was registered.
- **Design question, observed and not asserted (fixed below):** after a request for a missing preset, BPC's preset and the outfit stayed unchanged as the README promises, but `GearUpPolicyComp.IsOn()` was `true` with `lastPolicy` `Default`. `Set` records the current preset before it checks that the requested one exists. So the "policy set" alert and the active icon appear although nothing switched, and restoring then reloads `Default`'s saved state.
- **Not covered:** the right-click menu, the targeter, Alt and the mod-settings default preset. At that point Gear Up and Go loaded the battle preset without first saving the current one, as BPC's own switch does, so restoring brought back the last saved state (fixed below). The original Better Pawn Control (`VouLT.BetterPawnControl`) wasn't loaded; only the offline harness covers its legacy field shape.

### 2026-11-09 policy-switch fixes

- **Fixes:** `SetBetterPawnControl.SetPawnControlPolicy` now returns whether it switched. `GearUpPolicyComp.Set` records the previous preset only after a successful switch, so a missing preset leaves the switch, the alert and the active icon off. Before loading a preset, Gear Up and Go calls BPC's `AssignManager.SaveCurrentState(Colonists())`, as BPC's own switch does; this applies to both the switch and the restore. Both BPC versions have those members (checked in the forked DLL and in `Source/ThirdParty/BetterPawnControl_publicized.dll`). If they are missing, the switch still happens without saving.
- **Batch change:** `guag.bpc-missing` now asserts that the switch stays off. Before the switch, the colonist's outfit is changed to `GUAG test changed outfit` without BPC saving it, and `guag.bpc-restore` expects that outfit back.
- **Unfixed mod, `20261109-192022-guag-bpc-s1`:** 4 passed, 2 failed as expected. `guag.bpc-missing` saw the switch on with `lastPolicy` `Default`. `guag.bpc-restore` got the peace outfit back instead of the changed one.
- **Fixed mod, deployed with `deploy.ps1`:** `20261109-192045-guag-bpc-s1` passed 6 of 6, and `20261109-192059-guag-gear-s1` passed 5 of 5. Both offline harnesses pass.
