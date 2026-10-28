# World art rollout — 28 October 2026

## First world pass

Seven selected image-conditioned models now appear in the playable world:

- Oak 07 replaces the old broadleaf canopy in suburban gardens and streamed woodland. Deterministic rotation, height and existing density provide variation.
- Conifer 02 replaces procedural pines. A restrained material tint restores a natural muted green to the grey reconstruction.
- The cracked harvest boulder replaces the procedural streamed rock visual.
- Generated sofa, table, cupboard and chair models replace household furniture visuals, including in the starting home. Their existing collision, salvage identities and save/network removal remain authored.

The alternative Oak 01 was reviewed and excluded because of a disconnected ring/debris near its roots. Its original output remains intact. `scripts/world-model-selections.json` records the chosen references. `prepare-batch-static.py` reproduces the registered copies without editing source models. There are now 166 registered models: the earlier 159-model static integration plus these seven world assets.

Tree instances share imported meshes and cached matte materials, use generated Godot LODs with a lower LOD bias, and stop drawing beyond 190m. This is a performance-conscious implementation, not a claim of a completed frame-rate benchmark. Tree collision remains the existing authored trunk cylinder; foliage is not solid. If a selected model is unavailable, existing art remains available as fallback.

## Verification

The regional check now verifies tree height/ground contact, shared mesh and material resource IDs, one trunk collider, generated furniture visibility and retained collision. The ordinary build suite also checks residential salvage, streamed persistence and co-op furniture behaviour. Candidate models were rendered for inspection in `Ingame Screenshots/World Asset Candidates`.

## Building exterior pass

Suburban shells now use illustrated clapboard siding and staggered shingles, with cream, blue-grey and brown siding and slate, clay or charcoal roofs. City shells use brick or concrete panel finishes. These are shader materials rather than newly generated texture images; distance fading reduces fine-pattern aliasing. Existing doors, rooms and collision remain authored.

Industrial warehouses use six fitted instances of the selected `catalog-build/18` scrap-steel panel on their side and rear walls, with corrugated finishes on the existing shell and roadside garages. Loading-bay fronts remain clear. The generated timber-wall candidate was rejected for this use because its reconstruction is a solid block. Shared imported meshes and cached materials are retained.

The regional check verifies all six warehouse panels as well as the original collision footprint. Fresh engine-rendered exterior views are in `Ingame Screenshots/Exterior Pass - 2026-10-28`. This pass does not change the road layout or complete the terrain redesign.

Exterior validation passed: GPU-rendered regional previews without shader errors, the complete gameplay suite, two-process ENet co-op checks, Windows export and the exported executable's regional/world-model checks. Logs: `exterior-preview.log`, `exterior-build.log` and `exported-exterior-validation.log` in the sibling `Ashen Meadows - 3D Batch` folder. A representative frame-rate benchmark remains outstanding.

## Still to implement

The subsequent [ground/layout pass](GROUND-LAYOUT-PASS.md) completes the initial ground-material, road-connection, tree-selection and continuous-foundation work. Its two additional oak alternatives bring the registry to 168 entries. The list below records broader rollout work rather than claiming every region is finished.

This is the start of the world-art rollout, not the finished redesign.

1. Further exterior detailing and modular parts, preserving walkable rooms, openings, story placements and collision; do not substitute fused house reconstructions.
2. Additional approved tree variants after cleanup and a representative gameplay frame-rate check.
3. Distinct industrial yard dressing and selected vehicle/container variants, with explicit placement and collision rules.
4. Further site-specific terrain grading, yard dressing and road-end details after the initial ground/layout pass.
5. New actor/wildlife models only after anatomy and animation preparation.

The latest playable export is `build/game/AshenMeadow.exe`. A pre-pass executable/package copy is kept in `build/before-world-models-20261028/game`. Changes are local until committed and pushed for a launcher release.

Completed validation: C# build, imported scene/scale/material checks, world-model checks, the complete gameplay suite, two-process ENet co-op checks and Windows export. The exported executable also passed the regional/world-model checks. Logs are in the sibling `Ashen Meadows - 3D Batch` folder (`world-models-build.log`, `exported-world-models-validation.log`). Fresh rendered area views are in `Ingame Screenshots/World Art Pass - 2026-10-28`.
