# Expedition gameplay port

This ports the existing Expedition game logic into Godot .NET. Survival base defence in game2 remains separate. None of the deferred NPC-community ideas, drivable cars, boss systems, legendary boss loot or river expansions were present in the Expedition prototype, so those are not claimed as migrated features.

## Controls

- WASD: move; mouse: look; Shift: sprint; Space: jump; Esc: pause/resume.
- Left click: shoot; right click: aim toggle; R: reload (1.4 seconds).
- F: gather nearby resource, enter/exit dungeon, open a cleared chest, collect well water or read abandoned notes.
- K: crafting/workshop; I: guns/armor; J: six attributes; M: south-block map.
- H: bandage most urgent open wound; V: eat; C: drink; L: flashlight; U: mute generated audio.
- B: building mode; 1–8: foundation/floor/wall/half wall/pillar/beam/stairs/roof; Q/E: rotate (Ctrl fine); G: snap; Page Up/Down: elevation; +/-: reach; R: reset orientation/height; left click: place; right click: remove if supports remain valid.
- T: chop/hold to continue; P: clear/hold to extend paths; O: dig well (bench 1, 4 timber, 8 stone).
- F6: place crafted sleeping bags, bind respawn, travel and rest.
- N: journal; F8: existing encrypted relay connection probe.

Menus pause gameplay, needs and wound timers. Mouse capture resumes when you close a menu. Settings currently reset each launch; saved character/world state does not.

## Preserved rules

The original recipe table, crafting resource tier gates, skill XP curves and death penalty, wound timers/bandage recovery, armor stat/loot rolls, structural weight/support evaluator, south-block terrain equations, river boundary and dungeon layouts are copied as engine-independent C# logic. Game adapters use real Godot collision/raycasting and UI instead of Windows Forms/OpenGL calls.

T1 safe meadow → T2 woodland → T3 outskirts town → T4 south city; T5 city sewers/tower and a lower hospital ward. There are seven entrances, three room chests each, 3/5/7/9 guards by tier, and guarded deep bioalloy caches. Loot IDs preserve duplicate protection across saves and the legacy importer. Clear a T4 floor and interact at its northern lower-ward entrance to descend. North river remains closed.

Crafting includes all old benches, tiered guns/armor, rounds, sleeping bag kits, bandages, provisions and night weapon tuning. Building pieces retain free placement without a material-cost recipe, matching the prototype; unsupported towers and unsafe support removal are rejected. Stair collision uses a ramp with stepped visuals. Art and enemy animation are procedural placeholders; polished models/materials remain separate work.

Strength trains on trees; Agility/Stamina on actual sprinting; Marksmanship on hits/kills; Crafting on recipes; Survival on gathering. Higher Strength accesses harder trees. Death clears partial XP in all six attributes and retains earned levels/items. Night infected and gun drops provide catalysts/scavenged-quality bonuses. Gun quality is per weapon type, matching the prior loadout data model.

Bites add individual wounds. One open wound has 600 seconds of blood; each open wound drains all remaining blood timers N times faster. H patches one wound without healing health; patched wounds recover after 1200 active seconds. Timers/needs persist and don't advance offline. Health recovers at 0.08 HP/s (plus Survival) after 60 seconds without damage, sufficient hunger/water and no open bleeding. Health impact itself may kill sooner. Menus pause these timers.

Sleeping bags need workshop 2. Fast travel is same-seed, daylight, no wounds, safe camps, one provision, one water, 25 energy and ten-minute cooldown. Rest at a safe bag restores fatigue reserve. Respawn falls back to the meadow if a camp is blocked or nearby enemies make it unsafe. Camps, buildings, clock and terrain edits are per-seed; character gear/attributes travel between seeds via pause-menu world selection.

The world streams 64m chunks with a 7×7 active window (49 maximum), generates hills/valleys and increasingly dense towns/cities, and persists tree removals, paths, wells and burned wrecks. Only nearby affected terrain meshes/physics rebuild after earth edits. Trees/rocks/buildings block movement and shots. Enemy sight uses Godot line-of-sight queries, shots/chopping/sprinting alert infected, and night variants appear in T2+ while the meadow keeps low density. Static foliage/props differ visually from the old renderer.

## Persistence and import

Godot user:// uses the stable custom AshenMeadowExpedition application-data folder. Its expedition/profile.json contains the character, per-seed world edits and building lists. Writes are atomic with a .bak; corrupt files are protected from replacement. If the existing save is corrupted, repair/restore it outside the game rather than saving a replacement over it. Mutation failures roll back costs/state.

Esc → Import Old Expedition Character reads the selected legacy profile and compatible buildings in the sibling worlds directory. Import is explicit and replaces the active character; choose the legacy Expedition profile, not the unrelated Survival profile. Old source data remains untouched. Different procedural object geometry means some imported edits may have different visual placement. Standalone legacy and Godot save locations remain separate.

## Remaining migration work

The retained relay probe tests TLS transport only. Expedition multiplayer sessions, replicated world state, host authority and cross-server character validation have not been implemented. Full skeletal characters, cinematic animations, authored environmental art, the old detailed room dressing and richer ambient audio are not reproduced by this logic port. The journal retains the six stories with simplified abandoned-location interaction. No release is published remotely until you upload and run the GitHub workflow.
