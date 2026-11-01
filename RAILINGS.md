# World railings

Roadside timber fences now follow the road shoulder in two-metre spans. Every span is sampled against paved roads, junctions, house approaches, authored locations and supply-stop access. Steep or uneven spans are omitted, with an end post closing each accepted run. This replaces the old fourteen-metre chords that crossed bends.

Posts and both horizontal rails have matching cylinder collision in visible and remote-player simulation chunks. Garden side fences follow terrain and use the same collision helper; front approaches and driveways remain open.

Validation: `--railing-check` checks route placement and actual physics ray intersections for visible and simulation rails. `--suburb-check` covers residential generation. The railing check is included in `scripts/build-game.ps1`.
