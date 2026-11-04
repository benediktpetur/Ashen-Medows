# Generated skyscrapers — 2026-11-04

All ordinary Tier 4 city building exteriors now use four reviewed textured skyscraper models from the November 3 batch. Existing world-seeded lot positions, footprints, heights and building damage identifiers are retained. Industrial warehouses and authored story landmarks keep their own structures.

Sixteen completed conversions were inspected in actual Godot renders. Four have usable full-building geometry. Twelve are deferred because of missing facades, detached strips, collapsed geometry or large untextured white faces. `scripts/skyscraper-selections.json` records each decision; `game/Assets/GeneratedSkyscrapers/deferred.json` links the rejected candidates to their original files. Source generation outputs are unchanged. The initial all-candidate contact sheets remain in `Ingame Screenshots/Generated Skyscrapers` for review.

Run `scripts/prepare-skyscrapers.py` to stage the approved, self-contained textured GLBs and write the source paths and SHA-256 manifest. Godot imports the four assets. Runtime caches share the imported meshes, LODs, textures and adjusted materials across instances. Each lot chooses a model deterministically from the existing world hash and fits it to the original city block envelope. A concrete foundation gives the model ground contact.

These are exterior replacements. Ordinary city blocks retain their solid box collision and 600 HP whole-building damage behavior, including saved destruction and the host's simulation-only chunks. Destroyed old city lots remain destroyed because their identifiers are unchanged. There are no new enterable skyscraper floors, wall harvesting rewards or collapse mechanics in this pass.

`--skyscraper-check` verifies every approved model, dimensions, centering, ground contact, shared mesh/material resources, city-wide variant selection, collider parity, damage and destruction after streaming. `--skyscraper-preview` renders model contact sheets and two city views. The build script includes the new check. The skyscraper, region and atlas checks passed, followed by the Windows export and exported-build verification recorded in `VALIDATION.md`.
