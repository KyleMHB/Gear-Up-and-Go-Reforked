# Changelog

Always update this file when changing the mod. Add the newest entry at the top with the date, a short summary, and build/deploy status.

## 2026-09-05

- Fixed Better Pawn Control integration when the current fork exposes `AssignManager.policies` as an inherited static property instead of a field.
- Retained compatibility with the original static `policies` field and existing policy selection and discovery behavior.
- Added dependency-free regression harnesses for both reflection shapes.
- Bumped assembly version to `1.0.4.0`.

Build status:
- Dependency-free .NET 8 inherited-property harness: passed.
- Dependency-free .NET 8 legacy-field harness: passed.
- Patch applicability check against the Gear Up and Go repository: passed.
- Gear Up and Go Release build: passed with 0 warnings and 0 errors.
- Runtime delivery: passed to `E:\Steam\steamapps\common\RimWorld\Mods\Gear Up and Go Reforked`.
- Source and runtime `GearUpAndGo.dll` versions: `1.0.4.0`; SHA-256 hashes match.
- Runtime deployment contains no PDB files.

## 2026-05-03

- Restored right-click Better Pawn Control policy selection for the renamed Better Pawn Control Forked package.
- Reworked Better Pawn Control integration to support both original and forked package ids without a compile-time BPC assembly reference.
- Bumped assembly version to `1.0.3.0`.

Build status:
- Debug build: passed.

## 2026-05-03

- Stopped disabling outfit stand hauling when Gear Up and Go queues assigned stand use, allowing returned gear to go back to the stand.
- Bumped assembly version to `1.0.2.0`.

Build status:
- Release build: passed.
- `deploy.ps1`: passed.

## 2026-05-03

- Updated optional Outfit Stands Plus Forked detection to use the renamed package id `KyleMHB.OutfitStandsPlusForked`.
- Updated documentation references to the renamed Outfit Stands Plus Forked integration.
- Bumped assembly version to `1.0.1.0`.

Build status:
- Debug build: passed.

## 2026-05-02

- Added updated README and Steam Workshop description files for the documentation refresh.
- Cleaned the coding repository by removing generated build/cache artifacts.
- Removed tracked `Assemblies` output from source control now that Release deploys directly to the Steam runtime target.
- Removed stale `Files.csproj` packaging project and simplified `GearUpAndGo.sln` to the real code project.
- Ignored root `Assemblies` so deployed/runtime assemblies do not return to source control.

Build status:
- Debug solution build from cleaned repo: passed.
- Release solution build from cleaned repo: passed and deployed `GearUpAndGo.dll`.
- `deploy.ps1`: passed.
- Ignored build/cache artifacts removed after verification.

## 2026-05-02

- Moved the source-controlled repository layout to `E:\Coding\Rimworld\Gear Up and Go Reforked`.
- Converted the Steam mod folder at `E:\Steam\steamapps\common\RimWorld\Mods\Gear Up and Go Reforked` to a runtime-only deploy target.
- Updated project references to absolute RimWorld and Harmony install paths so builds work from the coding workspace.
- Updated Release output to deploy `GearUpAndGo.dll` directly to the Steam runtime `Assemblies` folder.
- Updated Debug output to ignored `bin\Debug` so local debug builds do not overwrite tracked runtime assemblies.
- Added `deploy.ps1` to copy runtime metadata/content folders to the Steam mod target while Release builds produce assemblies.

Build status:
- Debug build from `E:\Coding\Rimworld\Gear Up and Go Reforked`: passed.
- Release build from `E:\Coding\Rimworld\Gear Up and Go Reforked`: passed and deployed `GearUpAndGo.dll` to the Steam runtime target.
- `deploy.ps1`: passed and refreshed runtime metadata/content folders.

## 2026-05-02

- Hardened Gear Up and Go job flow with validation for pawns, targets, reachability, and final goto jobs.
- Added a max gear-up iteration guard to prevent repeated job queue churn and log spam.
- Refactored gearing into planner and queue helpers for maintainability.
- Added optional Outfit Stands Plus Forked support:
  - Detects `KyleMHB.OutfitStandsPlusForked` by package id.
  - Finds a pawn's assigned outfit stand.
  - Uses the stand first when it contains at least one policy-useful apparel or weapon.
  - Skips the stand and falls back to normal gearing when it is not useful.
- Kept Defensive Positions Forked compatibility target-based so its integration can pass saved destinations.
- Improved Better Pawn Control compatibility:
  - Case-insensitive package detection.
  - Case-insensitive policy lookup.
  - Warns once when a configured policy is missing.
  - Keeps gearing functional when BPC is missing or misconfigured.
- Guarded `GearUpPolicyComp` access to avoid null-state failures in gizmos and alerts.
- Removed verbose GearUp job logging from Release builds.
- Updated project references to build against the local RimWorld and Harmony install paths.
- Built and deployed Release `GearUpAndGo.dll` to `Assemblies`.

Build status:
- Debug build: passed.
- Release build: passed and deployed.

Notes:
- Could not commit this change set because the mod folder is not a git repository.
