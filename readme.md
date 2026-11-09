# Gear Up and Go Reforked

Gear Up and Go Reforked adds a command that drafts selected colonists, equips gear allowed by the active outfit policy, and sends them to a chosen location. Optional Better Pawn Control integration can switch to a battle preset before the pawns gear up and restore the previous preset afterward.

This maintained refork supports RimWorld 1.6 and 1.7 and updates the original workflow for current gear-management mods.

## Features

- Draft, gear, and move one or more selected colonists with one command.
- Set a default Better Pawn Control preset or choose one from the command's right-click menu.
- Restore the previous Better Pawn Control preset after the battle.
- Continue gearing when Better Pawn Control is absent or the configured preset cannot be found.
- Use policy-relevant equipment from an assigned Outfit Stands Plus Forked stand when available.
- Work with Combat Extended loadouts and avoid competing with active Defensive Positions or outfit-stand jobs.
- Stop unreachable or repeated gear jobs instead of cycling indefinitely.

## Installation

### Steam Workshop

1. Subscribe to [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077) and [Gear Up and Go Reforked](https://steamcommunity.com/sharedfiles/filedetails/?id=3724312502).
2. Enable Harmony before Gear Up and Go Reforked in RimWorld's mod list.

### Manual installation

1. Download a release and extract the mod folder into RimWorld's `Mods` directory.
2. Enable Harmony before Gear Up and Go Reforked.
3. Restart RimWorld after changing the active mod list.

## Usage

1. Select one or more player-controlled colonists.
2. Select **Gear & Go** and choose a reachable destination.
3. The selected pawns draft, collect suitable gear, and move to that destination.

The command also supports these controls:

- **Right-click Gear & Go:** choose a Better Pawn Control preset for this order.
- **Shift-click Gear & Go while a preset is active:** restore the preset that was active before Gear & Go changed it.
- **Hold Alt while choosing the destination:** issue the order without changing the Better Pawn Control preset.
- **Hold Shift while choosing the destination:** queue the order.
- **Select the active Gear Up and Go alert:** restore the previous Better Pawn Control preset.

## Settings and configuration

Open **Options > Mod settings > Gear Up And Go**. If you use Better Pawn Control, enter the preset name Gear & Go should apply by default. The name match is case-insensitive.

The setting is optional. Gearing continues without a policy switch when Better Pawn Control is not active, the field is blank, or the named preset cannot be found.

## Requirements and compatibility

- Requires RimWorld 1.6 or 1.7 and Harmony.
- Supports Better Pawn Control and Better Pawn Control Forked as optional integrations.
- Supports Combat Extended loadouts when Combat Extended is active.
- Uses assigned Outfit Stands Plus Forked stands only when they contain equipment useful for the pawn's current policy.
- Avoids interrupting active Defensive Positions Forked and outfit-stand gear jobs.

## Building from source

The project targets .NET Framework 4.7.2. It compiles against the RimWorld 1.6 reference assemblies from the `Krafs.Rimworld.Ref` 1.6.4871 NuGet package and the `Microsoft.NETFramework.ReferenceAssemblies.net472` package in your NuGet cache. RimWorld 1.7 no longer ships a Unity managed folder, and the 1.6 build runs on both versions. If your packages or Harmony live elsewhere, pass `/p:RimWorldManagedPath=...` or `/p:HarmonyAssemblyPath=...`.

Build from the repository root:

```powershell
dotnet build .\Source\GearUpAndGo.csproj -c Release /p:UseSharedCompilation=false
```

The maintainer's `deploy.ps1` also depends on the local `..\_Shared\RimWorldModTools.ps1` helper and copies the runtime folders into the configured RimWorld installation.

## Testing and validation

Run the Better Pawn Control compatibility harnesses:

```powershell
dotnet run --project .\Tests\SetBetterPawnControlBindingTests\SetBetterPawnControlBindingTests.csproj --configuration Release
dotnet run --project .\Tests\SetBetterPawnControlLegacyBindingTests\SetBetterPawnControlLegacyBindingTests.csproj --configuration Release
```

The first harness covers the inherited-property shape used by Better Pawn Control Forked. The second covers the legacy static-field shape.

## Fork history and credits

Uuugggg created Gear Up And Go. A later fork kept it available through TDS Games. This refork maintains the command for RimWorld 1.6 and current integrations.

Compared with the earlier versions, this refork adds current package metadata, support for both Better Pawn Control package IDs and reflection shapes, Outfit Stands Plus Forked integration, Combat Extended handling, job-loop limits, and safeguards for Defensive Positions and stand-owned gear jobs.

## Links

Support me on Ko-fi. This does not imply endorsement by the original authors.

[![Support me on Ko-fi](https://img.shields.io/badge/Support_me_on_Ko--fi-72a4f2?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/I7L525WMJ6)
[![GitHub Repository](https://img.shields.io/badge/GitHub-Repository-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/KyleMHB/Gear-Up-and-Go-Reforked)

- [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3724312502)
- [Previous Gear Up And Go fork](https://steamcommunity.com/sharedfiles/filedetails/?id=3530234134)
- [TDS Games mod page](https://tdsgames.com/game-mods/)

- [Better Pawn Control Forked](https://steamcommunity.com/sharedfiles/filedetails/?id=3724294345)
- [Cargo Containers Expanded](https://steamcommunity.com/sharedfiles/filedetails/?id=3714050938)
- [Everything Has Quality Reforked](https://steamcommunity.com/sharedfiles/filedetails/?id=3710884766)
- [Outfit Stands Plus Forked](https://steamcommunity.com/sharedfiles/filedetails/?id=3724311713)
- [Stats Forked](https://steamcommunity.com/sharedfiles/filedetails/?id=3712317437)

## Contributing

> Contributions, issues, and feature requests are welcome.

## License

This refork inherits the original Gear Up And Go license. See the [original project information](https://tdsgames.com/game-mods/) for its terms and attribution.
