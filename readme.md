# Gear Up and Go Reforked

Gear Up and Go Reforked is a RimWorld mod that drafts a pawn, gets the gear for the current outfit, and sends them to a targeted location.

This refork exists to keep the original mod alive and compatible with current RimWorld builds.

## Features

- **Gear-up command** for drafting a pawn, equipping the needed outfit, and moving them to a target location.
- **Better Pawn Control and Better Pawn Control Forked integration** through the configured policy name in mod settings.
- **Optional Outfit Stands Plus Forked support** for safer gear flow when the stand integration is available.
- **Defensive Positions compatibility checks** to avoid interrupting active movement or gear jobs.
- **Localized content** through language files.

## Installation

### Steam Workshop

Subscribe to the Workshop item and enable **Harmony** and **Gear Up and Go Reforked** in RimWorld's mod list.

### Manual Installation

1. Download or clone the repository.
2. Place the mod folder in your RimWorld `Mods` directory.
3. Enable **Harmony** and **Gear Up and Go Reforked**.

## Usage

Set the Better Pawn Control policy name in the mod settings.

When you use the Gear Up And Go command, the pawn is drafted, switches to the configured gear policy, and heads to the selected target.

## Configuration

The mod exposes a settings window in RimWorld.

The main setting is the Better Pawn Control policy name used when the button is clicked.

## Building from Source

Prerequisites:

- .NET SDK compatible with the project
- RimWorld reference files configured for the source project

Build from the repository root:

```powershell
dotnet build .\Source\GearUpAndGo.csproj -c Release /p:UseSharedCompilation=false
```

Use `deploy.ps1` to copy runtime metadata and content folders when preparing a local mod package.

## Testing and Validation

The primary validation command is the Release build:

```powershell
dotnet build .\Source\GearUpAndGo.csproj -c Release /p:UseSharedCompilation=false
```

The repository also includes `deploy.ps1` for runtime packaging checks.

## Contributing and Forking Policy

> Contributions, issues, and feature requests are welcome.
>
> **Forking Policy:** If your fork primarily consists of bug fixes or feature additions that align with the core vision of this project, I reserve the right to request that your changes be submitted as a Pull Request to this existing codebase rather than being published as a completely separate standalone release, package, listing, or distribution.

## Links


Support me on Ko-fi. This does not imply endorsement by the original authors.

[![Support me on Ko-fi](https://img.shields.io/badge/Support_me_on_Ko--fi-72a4f2?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/I7L525WMJ6)
[![GitHub Repository](https://img.shields.io/badge/GitHub-Repository-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/KyleMHB/Gear-Up-and-Go-Reforked)
- **Steam Workshop:** <https://steamcommunity.com/sharedfiles/filedetails/?id=3530234134>
- **TDS Games mod page:** <https://tdsgames.com/game-mods/>

## License

> This project is a fork of **Gear Up And Go** and inherits the original project's license. See the original project for license terms: <https://tdsgames.com/game-mods/>.

## Credits

- Original mod by Uuugggg.
- Current maintenance and refork work by the current maintainer.
