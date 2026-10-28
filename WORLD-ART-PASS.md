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

## Still to implement

This is the start of the world-art rollout, not the finished redesign.

1. House and industrial/city exterior materials and modular parts. Preserve walkable rooms, openings, story placements and collision; do not substitute fused house reconstructions.
2. Additional approved tree variants after cleanup and a representative gameplay frame-rate check.
3. Distinct industrial yard dressing and selected vehicle/container variants, with explicit placement and collision rules.
4. Terrain, road and ground-cover art consistency across all four surface regions.
5. New actor/wildlife models only after anatomy and animation preparation.

The latest playable export is `build/game/AshenMeadow.exe`. A pre-pass executable/package copy is kept in `build/before-world-models-20261028/game`. Changes are local until committed and pushed for a launcher release.

Completed validation: C# build, imported scene/scale/material checks, world-model checks, the complete gameplay suite, two-process ENet co-op checks and Windows export. The exported executable also passed the regional/world-model checks. Logs are in the sibling `Ashen Meadows - 3D Batch` folder (`world-models-build.log`, `exported-world-models-validation.log`). Fresh rendered area views are in `Ingame Screenshots/World Art Pass - 2026-10-28`.
