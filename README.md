# Ashen Meadow Godot Expedition

This folder is the standalone Godot game and GitHub playtest repository. The old Expedition and Survival base-defence prototypes remain outside it.

## Upload to GitHub

1. Create a PUBLIC GitHub repository. Code and builds will be publicly visible. Private source plus public downloads requires a separate two-repository workflow that is not configured here.
2. Extract the clean gameplay upload ZIP and publish its CONTENTS using GitHub Desktop with main as the default branch. Do not upload the ZIP as one file: GitHub does not unpack it. Alternatively publish this folder directly using Git; its .gitignore excludes tools, builds, caches, saves and private server files.
3. Allow the included GitHub Actions workflow if prompted. Game/launcher/script changes pushed to main build a Windows release automatically; README-only pushes do not release. Actions also has a manual Run workflow option.
4. Download AshenLauncher.zip from the successful release and give that ZIP to testers ONCE. Extract both files together. GitHub Actions fills in your repository address automatically.
5. Subsequent releases use 0.1.<workflow run number>. The launcher checks GitHub's latest published release on startup. Playtest releases are ordinary releases labelled Playtest, not GitHub prereleases (which latest excludes).

The repository root is `AshenMeadow-Godot/Ashen-Medows`, the folder containing `.git`, `.github`, `game`, `launcher` and `scripts`. Run commands below from that folder and publish its contents. Open `game/project.godot` from this repository when editing the game. The parent folder is the previous working copy; edits there are outside this Git repository.

Locally built launchers default to `benediktpetur/Ashen-Medows`. GitHub Actions uses the current repository address automatically. GitHub publication needs its first run on your account; it has not been performed remotely.

## What is ported

The Expedition gameplay now runs in Godot: FPS movement/sprint/jump; gunfire, reload, headshots, armor and infected sensing/noise; tiered recipes/equipment; activity attributes and death XP penalty; hunger/water/fatigue, slow health recovery, persistent wounds and bandages; sleeping bags/respawn/restricted travel; streamed south-block terrain and progression; structural weight/support building; tree chopping, paths, wells/water and exploding wrecks; seven dungeons, city lower layers, guarded tiered loot and bioalloy; day/night threats and catalyst weapon tuning; map and journal.

Read GAMEPLAY.md for controls, timing, save import and current limits. Original engine-independent rules are in game/Scripts/Logic; Godot adapters handle physics, rendering, UI, input and audio.

The artwork/animation and room dressing are still simple procedural models/materials. This is a gameplay migration, not the visual fidelity of the style-3 concept. Cars remain explosive wrecks; drivable vehicles, NPC communities, bosses and river expansions are future features that were not implemented in the old Expedition.

The existing encrypted relay transport is retained on TCP 28747. F8 tests a connection only. Expedition multiplayer sessions and synchronized world state have NOT been implemented; this build is currently single-player. The relay server was not deployed or reconfigured.

## Run and build locally (PowerShell 7)

    ./scripts/setup-godot.ps1 -Templates
    ./scripts/build-game.ps1 -Export
    ./scripts/package-github.ps1 -Repository benediktpetur/Ashen-Medows -Version 0.2.0

Use Godot 4.6 .NET and .NET SDK 8. Open game/project.godot in the editor. The setup script downloads official pinned binaries/templates and verifies their SHA512 checksums. First setup downloads around 1.3 GB. Tools stay in .tools, excluded from Git. NuGet restores need network access.

Build-game compiles C#, imports in the real Godot editor, starts the game headlessly, runs rule and engine integration checks, then optionally exports Windows. Checks fail on ERROR output even when Godot returns zero. See VALIDATION.md. The Windows .NET export includes the EXE, PCK and runtime dependencies; share the complete package rather than only its EXE.

Package-github generates release/ashen-VERSION-windows-x64.zip, manifest.json and AshenLauncher.zip. The workflow creates a draft release, uploads all assets, then publishes it as latest. Failed builds/uploads preserve the previous published version. Versions cannot be reused: trigger a new workflow run rather than rerunning an already published release.

## Saves and legacy import

The launcher includes embedded Expedition artwork and a meadow crest. On first launch, use **Choose...** to select the game install folder, then **Check / Update**. Later launches automatically check for updates to an installed build. The folder choice is remembered in the stable LocalApplicationData/AshenMeadow/GodotPlaytest/launcher.json preferences file. Changing the folder does not move or delete previous downloads; selecting the original folder restores access. Game builds and temporary downloads use the selected folder, while Godot character saves retain their stable application-data location. Keep the supplied launcher.json beside AshenLauncher.exe for the initial GitHub configuration.

Godot user:// uses a stable AshenMeadowExpedition application-data folder, outside updater version folders. Character and per-seed world/build data live in expedition/profile.json, with atomic writes and a .bak. Corrupt profiles are protected from overwriting.

Esc -> Import Old Expedition Character reads a selected legacy profiles/expedition-loot.json and compatible structures from the sibling worlds folder. Import intentionally replaces the active Godot character and preserves the prior Godot save as .bak. Old files are only read. Skills, equipment, crafting, wounds/needs/camps, journal and world changes transfer; position resets to the meadow. Some decorative object placement differs between renderers, so imported removals may not line up visually. Import is explicit, never automatic.

Launcher builds live in %LOCALAPPDATA%/AshenMeadow/GodotPlaytest/versions, separate from the earlier legacy Playtest launcher. Updates verify byte size and SHA256, reject unsafe ZIP entries, extract to staging, and atomically change the active build. Play works offline after an unsuccessful check. Previous versions stay on disk; delta patches, automatic cleanup and launcher self-updates are not implemented.

## Hosting and costs

This configuration requires public GitHub Releases because the launcher has no private-repository login. It contains no deployment credentials. GitHub publication uses the short-lived workflow token with contents:write. HTTPS plus the manifest hash checks download integrity; this is not a separately signed release system. The launcher is not code-signed and Windows may show a reputation warning on first download.

Standard public GitHub-hosted build runners are free under GitHub's current policy. Releases hold the builds; the workflow does not retain large permanent Actions artifacts or use Git LFS. Each ZIP must stay under GitHub's 2 GiB release-asset limit. Never commit server certificates/configuration, passwords, saves, downloaded tools or export binaries.

The current graphic-novel art/UI pass is documented in ART-PASS.md. Generated material provenance is in game/Assets/ART-PROVENANCE.md.
