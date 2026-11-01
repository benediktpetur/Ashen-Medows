# November world atlas

The user-supplied `GAME MAP!!! LETS GOOOO!!!!.png` is now the geographic source for the playable world. North is negative Z. The source scale bar is interpreted as approximately 9 km across: X -2400..6600, Z -5000..4000. The original map is preserved in `game/WorldMap/reference.png`.

## Shared geography

`scripts/build-world-atlas.py` extracts connected streets and waterways, excludes detached roofs/parcel outlines, traces the main highway/interchange connections, and reserves the small existing starting lane. It generates a 2048-square signed-distance atlas, nearest-street coordinates and district masks. `atlas.bin` is embedded in the .NET assembly so exported games and co-op peers use the same data. Seed changes affect terrain variation, house styles and loot, not the authored street/water layout.

The northwest contains the river city and surrounding suburbs. Industry and rail yards occupy the north-central area. The highway sweeps southwest to northeast; rural farmland, wooded paths, quarry hills and the eastern lake chain occupy the rest of the map. The former south-to-north tier stripes and closed northern river boundary are replaced. Terrain, paved/dirt surfaces, vegetation clearance, house approaches, building exclusion, water surfaces, crossings and the map use the atlas. The normal chunk and remote simulation budgets remain bounded.

Housing is regenerated in suitable street-facing plots. Front doors and approaches point toward the nearest real street; foundations reach the terrain. Roads, banks, water, field sites and existing player bases reserve space. Farmland has sparse houses and fewer trees; forests retain their denser vegetation. City and industrial shells occupy clear land. Field locations are redistributed into their intended district tiers. Story houses seek clear plots, and the main settlement/mission corridor is repositioned geographically. Enterable residential/story houses retain their interaction and collision systems; ambient city shells retain their existing solid-shell behavior.

## Editable starting base and saves

The starting house is seeded once from ordinary foundations, wall/window/door modules, a support and roof tiles. It uses the same structure IDs, damage, owner assignment, snapping and support validation as player construction. Removed pieces do not respawn. No survivor beds are seeded. The yard and street approach remain scenery.

Before loading an existing profile the game preserves a `.before-world-atlas-v1` copy. Saved structures retain their positions and elevations using local ground pads; generated houses/blocks avoid those bases. Atlas and starting-house revisions are serialized. Character inventory and world records remain in the save. Co-op protocol 46 requires both peers to run the new build.

## Validation and scope

`--atlas-check` validates the full extent, terrain determinism through the logic checks, dry street-facing residential plots, district populations, field-site tiers, complete/supported starter-house modules, snapped expansion, legacy save/base preservation and persistence of house edits. `--railing-check` validates actual highway-adjacent rail spans and visible/simulation collision. `--building-check` validates connections and collider targeting. `--atlas-preview` captures the map, start, city, industry and lake.

This is a playable interpretation of the supplied plan, not a surveyed reconstruction of every roof or a multi-level traffic simulation. Street and water shapes come from the image; house footprints and architectural styles are procedural. Crossings are traversable graded decks/causeways; road junctions do not simulate grade-separated traffic. Existing art remains in use while the separate local AI asset batch continues. The source map and semantic preview allow subsequent layout refinements without repainting an unrelated world.
