> Historical 30 October layout and cleanup record. The playable geography is now the 9 km user-designed atlas described in [WORLD-ATLAS-IMPLEMENTATION.md](WORLD-ATLAS-IMPLEMENTATION.md). The old tier bands, closed river boundary and counts below no longer describe the current map.

# World structure and cleanup — 30 October 2026

The in-game M map now shows the four surface regions as numbered district cards, the actual curving main route with northbound progression arrows, static building footprints, and the new roadside stop markers. The north river boundary is labelled closed. A legend distinguishes roadside stops and buildings, and the distance scale is separate from the home marker. Terrain shading and minor street lines are quieter so the structure reads first. Heatmap, journey and location-pin overlays remain available. No terrain coordinates or story locations moved in this map update.

## Is the west side used?

Yes. An audit of the current seed 431 source gives the following static content distribution. House and urban-shell counts describe procedural placements; named field sites are the 28 exploration locations from the field guide. They do not count every story, dungeon, resource, roaming event or player structure.

| World X range | Procedural houses | Urban shells | Named field sites |
| --- | ---: | ---: | ---: |
| -1600 to -800 (far west) | 129 | 489 | 0 |
| -800 to 0 | 122 | 538 | 13 |
| 0 to 800 | 118 | 536 | 11 |
| 800 to 1600 (far east) | 104 | 464 | 4 |

The Wrecker and Bellkeeper are also west of X=0. The far western quarter is playable side exploration with generated districts, but has fewer authored reasons to visit. Empty-looking map space is not automatically unused land. Field locations retain their existing discovery rules rather than being revealed merely to fill the map.

Audit output: `build/map-content-audit.log`. The small audit harness under `build/map-audit` compiles the current engine-independent game rules without modifying the game.

## Cleanup

Four obsolete exported build folders and forty old source-upload ZIPs, totalling approximately 4.57 GiB, were moved to the Windows Recycle Bin. The exact original paths and sizes are recorded in `build/cleanup-2026-10-30.json`.

The current `build/game`, source repositories, shared tools, generated asset libraries, screenshots, saves and launcher-installed copies outside this workspace were retained. Recycle Bin storage is recoverable; this is not a claim that 4.57 GiB of disk space has been reclaimed.

The updated playable build remains `build/game/AshenMeadow.exe`. This local update has not been published to the launcher.
