# Image-conditioned model integration — 27 October 2026

Update: the 28 October world-art pass adds seven environment models to the original 159-model static registry (166 active entries total). See `WORLD-ART-PASS.md` for current coverage; the counts below describe the original static pass.

The completed local conversion batch contains 899 textured results from 918 input images; 19 failed. These counts include alternate concepts, not unique required game assets. Original images and model outputs are preserved.

The initial 225 static candidates were rendered in Godot and reviewed on 19 labelled contact sheets. **66 were rejected**, leaving **159 registered static models**. Examples include guns incorrectly generated for ammunition, armour with extra boots, filled door apertures and unrelated weapon silhouettes. `scripts/batch-model-rejections.json` records every exclusion. The procedural fallback remains active for those slots. Contact sheets intentionally include rejected candidates so the decisions can be reviewed. The importer applies these exclusions on subsequent runs.

`scripts/prepare-batch-static.py` imports one successful alternative per matching static catalogue key. It selects the first successful option deterministically; this is not a claim that an art director has approved the best alternative. The game copies and source SHA256 records are in `game/Assets/Hunyuan/Static/manifest.json`. Every copied GLB must contain geometry, embedded textures and no external file dependencies. `deferred.json` lists static catalogue entries without a new matching conversion. The full conversion inventory is outside the game in `Ashen Meadows - 3D Batch/Model Audit.json`.

## Runtime coverage

- Inventory and crafting previews use matching generated static assets, identified through `ExpeditionItemDefinition` keys, index and tier. Original procedural geometry remains as a fallback. Authored relic appearances are preserved.
- Harvestable material pickups use the generated material draft when available.
- Abandoned and burned wreck visuals use the selected generated car drafts. Gameplay and existing collision remain separate.
- Basic workbench visuals use the generated tier-one workbench. Upgrade details remain authored.
- Models are fitted proportionally to the existing visual envelope. This avoids arbitrarily stretching reconstructions. They are cached, with a 75-metre visibility limit. Imported materials are matte.

## Deliberately deferred

- Existing 22 articulated actor models and their tested adapter remain. New humanoids, bosses and wildlife require anatomy, rigging and motion review before replacing them.
- Fused house reconstructions cannot replace the game's walkable interiors, windows, floors and moving doors. They remain source drafts.
- Build-kit models are available in crafting previews. Runtime construction keeps its authored snapping dimensions, collision, doors, traps and functional nodes until each generated piece is fitted and tested individually.
- Trees and dense foliage need collision, LOD and forest performance review before replacing the streamed vegetation system.
- First-person weapons retain their authored grips, hands, moving parts and animation. Generated weapon models are used in matching item previews.
- Unmapped story props and furniture need explicit placement/identity mapping rather than filename guesses.

## Verification

`--batch-model-check` checks all imported scenes, materials and proportional scale, then exercises the existing item catalogue builder. It runs as part of `scripts/build-game.ps1`. `--batch-model-preview` captures labelled engine-rendered contact sheets to `Ingame Screenshots/Batch Models`. These are review aids, not proof of animation or collision quality.

This is a static-art integration pass. Deferred models remain available for further work; there is no claim that every generated result is production-ready or installed throughout the world.

Validation completed: C# build (zero warnings/errors), Godot asset import, all 225 candidate scene checks, repeated checks of the final 159-model registry, the build script's gameplay/actor checks, two-process ENet co-op checks, Windows release export, and the exported executable's 159-model check. Logs are in the sibling `Ashen Meadows - 3D Batch` folder: `game-validation.log`, `accepted-model-validation.log`, and `exported-model-validation.log`.

The updated local executable is `build/game/AshenMeadow.exe`. No GitHub publication or remote release was performed. The automatic batch-monitor follow-up can stop now; remaining exclusions need design correction, rigging, fitting or a separate art pass.
