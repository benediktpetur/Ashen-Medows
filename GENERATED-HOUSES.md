# Generated house batch — 2026-11-04

Fourteen completed textured exteriors from `Ashen Meadows - AI Houses - 2026-11-01` replace the procedural neighborhood houses across Briar Lane and the residential district. Selection uses the existing saved lot style, position, rotation and scale, so it is deterministic. Future batches can extend the manifest and repeat preparation.

The editable starter home, authored story buildings and lots preserved around existing player construction retain their dedicated structures. The eight generated kit components remain source assets; they are not complete houses.

## Preparation

Run `scripts/prepare-houses.py`, import the project, build C#, then run Godot with `-- --house-prepare`. The staging script copies successful textured GLBs and records source paths and SHA-256 hashes in `game/Assets/GeneratedHouses/manifest.json`. Original generated files are unchanged. The preparation mode saves textured ArrayMesh resources alongside the manifest.

Prepared exteriors fit the existing lots with individual roof heights. Their texture coordinates are retained. An uneven reconstructed base is removed and the existing gameplay floor/foundation replaces it. A clear 2.7m-wide front opening is cut through sealed geometry, with a timber frame and a collision ramp. The shell supplies matching visual and physical walls, rather than hiding an unrelated box collider behind the new model. Furniture uses a smaller layout to fit cottage rooms. Existing furniture IDs, salvage, caches, garden railings and street approaches remain.

## Damage and limits

Ground-floor triangles are grouped into independently damageable wall sections. Each section uses the existing 180 HP architecture damage system and stable per-house identifiers. Destruction removes both its mesh and collision and persists through the existing world-health save and multiplayer snapshot. Upper shells remain in place when walls are destroyed; there is no new structural-collapse simulation. No new wall material rewards or tool-specific harvesting rules have been added. Original procedural-wall damage identifiers remain in saves but do not identify the replacement geometry.

These are prepared exterior reconstructions with usable ground-floor access, not newly authored room plans. Upper-storey windows are scenery; this batch adds no stairs or upstairs gameplay. Double-sided shell materials provide an interior surface. Some source models retain asymmetry and baked shading.

## Verification

`--house-check` checks all fourteen resources, lot dimensions, clear entrance geometry, collision generation and independent wall sections. `--house-preview` produces rendered model contact sheets in `Ingame Screenshots/Generated Houses`. The normal build script now includes `--house-check`; suburb checks cover entrances, home, furniture salvage and streamed persistence. Interaction checks cover independent wall health and removal of render/collision on destruction.

Every multiplayer participant needs this build for matching residential collision and wall identifiers.
