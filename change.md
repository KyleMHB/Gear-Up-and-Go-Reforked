# Change Log

Always update this file when changing the mod. Add the newest entry at the top with the date, a short summary, and build/deploy status.

## 2026-05-02

- Hardened Gear Up and Go job flow with validation for pawns, targets, reachability, and final goto jobs.
- Added a max gear-up iteration guard to prevent repeated job queue churn and log spam.
- Refactored gearing into planner and queue helpers for maintainability.
- Added optional Outfit Stands Plus support:
  - Detects `khamenman.OutfitStandsPLus` by package id.
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
