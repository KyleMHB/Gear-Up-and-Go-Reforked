[h1]Description[/h1]
Gear Up and Go Reforked adds a command that drafts selected colonists, equips gear allowed by the active outfit policy, and sends them to a chosen location.

Optional Better Pawn Control integration can switch to a battle preset before the pawns gear up and restore the previous preset afterward. The command still works when Better Pawn Control is absent or the configured preset cannot be found.

[h1]Features[/h1]
[list]
[*]Draft, gear, and move one or more selected colonists with one command.
[*]Use a default Better Pawn Control preset or choose one from the command's right-click menu.
[*]Restore the previous Better Pawn Control preset after the battle.
[*]Use policy-relevant equipment from an assigned Outfit Stands Plus Forked stand when available.
[*]Support Combat Extended loadouts and avoid competing with active Defensive Positions or outfit-stand jobs.
[*]Stop unreachable or repeated gear jobs instead of cycling indefinitely.
[/list]

[h1]How to Use[/h1]
[list=1]
[*]Select one or more player-controlled colonists.
[*]Select [b]Gear & Go[/b] and choose a reachable destination.
[*]The selected pawns draft, collect suitable gear, and move to that destination.
[/list]

[b]Command controls[/b]
[list]
[*][b]Right-click Gear & Go:[/b] choose a Better Pawn Control preset for this order.
[*][b]Shift-click Gear & Go while a preset is active:[/b] restore the preset that was active before Gear & Go changed it.
[*][b]Hold Alt while choosing the destination:[/b] issue the order without changing the Better Pawn Control preset.
[*][b]Hold Shift while choosing the destination:[/b] queue the order.
[*][b]Select the active Gear Up and Go alert:[/b] restore the previous Better Pawn Control preset.
[/list]

[h1]Settings and Configuration[/h1]
Open [b]Options > Mod settings > Gear Up And Go[/b]. If you use Better Pawn Control, enter the preset name Gear & Go should apply by default. The name match is case-insensitive.

The setting is optional. Gearing continues without a policy switch when Better Pawn Control is not active, the field is blank, or the named preset cannot be found.

[h1]Requirements and Dependencies[/h1]
[list]
[*]RimWorld 1.6.
[*][url=https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077]Harmony[/url].
[/list]

[h1]Compatibility and Load Order[/h1]
[list]
[*][b]Better Pawn Control:[/b] Supports both Better Pawn Control and Better Pawn Control Forked as optional integrations.
[*][b]Combat Extended:[/b] Uses Combat Extended loadout jobs when Combat Extended is active.
[*][b]Outfit Stands Plus Forked:[/b] Uses an assigned stand only when it contains equipment useful for the pawn's current policy.
[*][b]Defensive Positions Forked:[/b] Avoids interrupting active movement and gear jobs.
[*][b]Load order:[/b] Load Harmony before Gear Up and Go Reforked.
[/list]

[h1]Fork History[/h1]
Uuugggg created Gear Up And Go. A later fork kept it available through TDS Games. This refork maintains the command for RimWorld 1.6 and current integrations.

Compared with the earlier versions, this refork adds current package metadata, support for both Better Pawn Control package IDs and reflection shapes, Outfit Stands Plus Forked integration, Combat Extended handling, job-loop limits, and safeguards for Defensive Positions and stand-owned gear jobs.

[h1]Credits[/h1]
Original Gear Up And Go by Uuugggg. Previous maintenance and project information are available through TDS Games. This refork is maintained by KyleMHB.

[h1]License and Forking Policy[/h1]
This refork inherits the original Gear Up And Go license.

If your fork primarily consists of bug fixes or feature additions that align with the core vision of this mod, I reserve the right to request that your changes be submitted as a Pull Request to my existing codebase rather than being published as a completely separate standalone release.

This is a project request, not an additional license restriction.

[h1]Links[/h1]
Support me on Ko-fi. This does not imply endorsement by the original authors.

[url=https://ko-fi.com/I7L525WMJ6][img]https://img.shields.io/badge/Support_me_on_Ko--fi-72a4f2?style=for-the-badge&logo=kofi&logoColor=white[/img][/url]
[url=https://github.com/KyleMHB/Gear-Up-and-Go-Reforked][img]https://img.shields.io/badge/GitHub-Repository-181717?style=for-the-badge&logo=github&logoColor=white[/img][/url]
[list]
[*][url=https://steamcommunity.com/sharedfiles/filedetails/?id=3724312502]Gear Up and Go Reforked on Steam Workshop[/url]
[*][url=https://steamcommunity.com/sharedfiles/filedetails/?id=3530234134]Previous Gear Up And Go fork[/url]
[*][url=https://tdsgames.com/game-mods/]TDS Games mod page[/url]
[/list]
