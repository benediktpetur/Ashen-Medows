# Graphic novel art pass

This pass applies art direction 3 to the CURRENT Godot scenes. It is an implemented procedural art pass, not the fidelity of the generated art-direction concept. It retains the existing gameplay/save format and separate Survival prototype.

| Module / scene | Work |
| --- | --- |
| Material system | Three-band lighting, ink contour pass, hatch/stipple surface treatment, real illustrated wood/masonry/asphalt/earth atlas with mipmaps |
| Starting valley | Abandoned tent, sleeping roll, fire ring, supply table/lantern and readable story; grass and richer resource silhouettes |
| Woodland | Branched tapered trunks, layered foliage clusters, surface leaf cards, instanced grass, rocks, simplified distant terrain |
| Outskirts / city | Facade window batches, masonry, shop signs/boarded glass, sidewalks/curbs, streetlights, planters and wreck detailing |
| Characters | Segmented infected/survivor meshes with facial/clothing details, night palette/bulk variants and limb movement; equipment portrait |
| Weapons | Beveled receivers/stocks, barrels/scopes, rails, trigger guard/bolt details, hands/forearms; pistol and tactical differences |
| Building | Timber/stone surfaces, plank seams, nail/metal details, support behavior preserved; world-shape checks prevent overlap with new street props |
| Dungeons / loot | Illustrated walls/floors, ceilings, warm point lights, medical beds, desk/equipment, ward signage and chest fittings |
| Day / night | Gradient sky, fog tint/sky influence, warm daytime vs dark blue night, flashlight; shared material system |
| HUD | Compact health/stamina/needs/ammo, interaction prompts, wound countdown, central crosshair and contextual building help |
| Menus | Shared dark ink/brass theme, two-column recipe/material UI, portrait/loadout, skills/camps/map/journal/pause styling |

Only 49 detailed terrain chunks are retained; grass/leaves use MultiMesh batches and distance limits, windows are batched, and the horizon is one coarse non-colliding mesh refreshed on chunk transitions. Earth edits rebuild nearby chunks. No whole-map detailed rendering is added. This is not a cross-hardware performance certification.

The atlas was generated with built-in image_gen and then wired into real Godot shaders. See game/Assets/ART-PROVENANCE.md for the final prompt. All gameplay screenshots are actual GPU viewport captures. Preview mode uses an isolated test profile and deliberately stages enemies, equipment and a small supported shelter to make the visual features visible; it does not alter the normal character save.

Validation: C# build zero warnings/errors; engine import; rule checks; engine integration (hitscan, physical occlusion, harvesting, wells, explosions, tier crafting, wounds/camps/death, dungeon loot, save behavior and menus); Windows .NET export; actual NVIDIA OpenGL preview captures. The Windows exported integration run is also checked. Multiplayer synchronization remains outstanding, and character animation remains procedural rather than a complete skeletal animation library.
