# Scenery and model review — 29 October 2026

This pass reviews the four surface regions at street level, their wider layout, house interiors and the previously faulty furniture/parking placements. It is not a claim of manual visual approval of every dungeon, NPC, conversion variant or save location.

## What changed

- Grass sampling increases from 900 to 2,600 candidates per chunk, with clustered density and varied scale. Forest fern sampling increases from 160 to 440 candidates. Actual counts depend on roads, buildings, story reservations and player structures; the representative forest check produces 1,753 grass clumps and 440 ferns.
- New shared low-polygon shrub and flower meshes provide undergrowth, dry plants and muted cream, straw and lavender flower accents. Leaves keep their natural green/earth tones. Decorative plants have no collision. Rustling bushes retain matching seeded visual/hearing positions on rendered and remote simulation chunks, with more samples.
- Clearance excludes house interiors, sidewalk widths, road shoulders, porch approaches, Briar Lane parking spaces, reserved story locations and player building footprints. New visual ground cover samples the same triangle heights as terrain collision. Grass/flower/fern draw ranges are deliberately limited; imported tree density is retained rather than multiplying expensive tree meshes.
- Houses gain gutters, grounded downspouts, porch foundation supports, selected chimneys, porch light housings, interior paint and skirting. Fence posts extend into the actual terrain instead of hanging from the house's raised floor elevation. Existing entrances, furniture and salvage identifiers remain.
- Warehouses gain electrical fittings and exterior light housings. Industrial yards gain small procedural drum/pallet groups with matching collision in both rendered and remote simulation chunks, kept away from doors, roads and reserved sites.
- City shells vary between muted cream, blue-grey, brown and stone finishes and gain parapets, lower facade bands, door canopies, downpipes and roof units. These additions use one cached composite mesh per silhouette/style instead of individual draw calls for every trim piece.
- Reversed warehouse, garage and roadside shop text is corrected to face the road.

New trim, yard props and plants are locally constructed geometry using the existing graphic-novel style. No new AI generation or paid service was run. Dedicated optimized art replacements remain in the model backlog.

## Model backlog

[MODEL-BACKLOG.md](MODEL-BACKLOG.md) is the readable list; `model-backlog.json` records keys, source concepts, existing draft paths, rejection reasons and required next actions. The report reconciles conversion output, the static registry, explicit environment replacements, recognized actor files and actual default item/crafting lookup aliases. It distinguishes absent conversions from rejected or unregistered existing drafts.

Regenerate it with `scripts/audit-model-backlog.py --date YYYY-MM-DD` after refreshing the conversion audit and running a regional check. It never starts a generation batch or modifies source images/GLBs.

## Verification

The regional checks cover increased foliage counts and sampled road/path/interior clearance in addition to the existing local-transform regression, vehicle clearance, foundation contact, building collision and regional progression checks. The ordinary build suite also exercises home access, furniture salvage, player construction clearance, saved felling, forest density, hearing volumes, world streaming and two-process ENet co-op.

GPU review captures include all four regions, close-up furniture/parking checks and four street-level views. The capture mode also logs a 45-frame mean for each paused static view at its viewport resolution. Those figures are a rendering sanity check, not a moving gameplay or combat frame-rate benchmark.

Build/review logs are in the sibling `Ashen Meadows - 3D Batch` folder (`scenery-build.log`, `scenery-preview.log`, `exported-scenery-validation.log`). Final screenshots are in `Ingame Screenshots/Scenery Polish - 2026-10-29`. The local executable is `build/game/AshenMeadow.exe`; no Git commit, push or launcher publication is part of this pass.

Completed validation: C# build, asset import, all registered model checks, the full gameplay suite, two-process ENet co-op and Windows export passed. The exported executable passed the regional/scenery/placement checks and rendered the saved close-ups without shader errors. Its paused 1280×720 static-view means were 113.4 fps (suburb), 114.8 (woodland), 164.8 (industrial) and 164.8 (city), sampled over only 45 frames each. These are not minimum gameplay frame rates.

## Remaining art work

Dedicated foliage LODs, a coherent modular architecture kit, road-end and drainage details, richer industrial props, landmark-specific dressing and animation-ready wildlife are the next visual priorities. Existing textured drafts should be reviewed before generating duplicates. See the backlog for exact item keys and unavailable/rejected conversions.
