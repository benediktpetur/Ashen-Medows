# Ingame Screenshots

These images are fresh captures from the current exported Windows game in `build/game`, copied into this folder. Open `Gallery.html` for thumbnails, or open any PNG directly. `Build Information.json` identifies the executable and game pack by SHA-256; `Screenshot Manifest.csv` records each image's hash and capture time.

The older screenshot collection was reviewed in `build` and the local Godot capture directory. Old anatomy iterations, superseded UI views and previous-build images were left in their original locations. This folder contains the refreshed current-build collection only.

## What is currently in the game

- First-person solo and direct-connect Godot ENet co-op, with portable saved characters and per-seed worlds.
- A streamed world with terrain elevation, a safe starting meadow, woodland, highways/outskirts, southern city areas and tiered dungeons; day/night changes and denser threats in dangerous zones.
- Gathering and physical tree chopping, crafting workshops, four research branches, character talents, skill progression, equipment slots and inventory.
- Customizable building with structural support, camp/sleeping-bag respawns, restricted fast travel, storage, utilities and defenses.
- Hunger, hydration, fatigue and timer-based bleeding; bandages pause bleeding while wounds recover. Death removes progress within earned skill levels.
- Primitive melee and bows, tiered firearms and class-specific ammo, gun recoil/bloom, scoped breath holding, knives/swords and the Stormfire electric chain sword with flame exhaust.
- Regional infected, weepers, bloaters and worms, wildlife, coordinated zombie dog packs, the Foreman and the South Bank boss progression.
- Environmental story locations, journals, loot, traps, a drone event, survivor rescue/recruitment, camp jobs, morale and a companion.

## How to read the collection

- World, HUD, inventory, research and story images render their current in-game scenes and panels using prepared capture saves.
- Character, weapon, item and structure lineups are staged in-engine showcases of current assets, not evidence that a player has unlocked every item at once.
- Boss and encounter previews deliberately position actors and cameras to show their appearance. The dog-pack preview is similarly staged; normal pack spawning begins outside the T1 starting zone.
- Screenshots document the current prototype. They are not concept art or a claim that every gameplay path has been manually played from start to finish in this capture session.

Capture automation: `scripts/capture-current-screenshots.ps1`. It launches the exported game with isolated capture data, without using the normal player profile. Capture logs and the inventory of older build screenshots are in `build/screenshot-capture`.
