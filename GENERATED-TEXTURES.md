# Generated material integration

Integrated 2026-11-02: all 15 completed material jobs from the local house/texture batch. Source PNGs, metadata and checksums are under game/Assets/GeneratedTextures. The five unfinished texture jobs are not required by this build.

- Grass, soil, gravel, asphalt and concrete feed the shared world-space terrain shader.
- Wet mud and pebbles dress atlas shorelines. Silt colours submerged ground and shallow water.
- Siding, brick, plaster and concrete feed shared exterior materials. Tier-one editable walls and doors use siding; roofs retain their existing material because no generated roof texture was ready.
- Lake-water colour, the two approximate height-derived normal maps and the foam mask feed an animated water shader. Shore proximity and shallow depth are carried on the water mesh's vertex colours. Water is opaque with a shallow-bed colour blend; this does not add swimming or refraction.

Textures use repeating world-space coordinates and mipmaps. Colours use sRGB imports in the shaders; normals and masks are sampled as data. Textures and materials are shared across chunks rather than copied per object. The terrain shader now follows the CPU atlas surface codes instead of painting the old procedural road route over the new map.

Verification: clean .NET build; generated-resource/binding checks; region, suburb and atlas checks; actual OpenGL render captures with no shader errors. `--texture-check` is included in build-game.ps1. `--texture-preview` captures the starter house, neighborhood road and lake into an isolated profile's tests/art directory. Preview images are in Ingame Screenshots/Generated Textures - 2026-11-02.

Windows export: build/game/AshenMeadow.exe. This is a local build; no GitHub release is published by this change.
