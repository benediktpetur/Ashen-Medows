# Residential wall assets

Put the finished visual-only GLB files in this folder and let Godot import them.
The game resolves each slot by filename when it loads the map. A missing file
uses a translucent placeholder. Re-export the game to include new art in a
standalone build.

| Slot | Example filename |
| --- | --- |
| Solid wall | Wall-Brick-1.glb |
| Doorway | Wall-Brick-2.glb |
| Window opening | Wall-Brick-3.glb |

The same numbers apply to Clapboard, Timber, Plaster, Concrete, and Industrial. Model
each piece at 3 m wide, 2.5 m tall, and 0.2 m deep, with its origin at the
bottom center, Y up and the exterior facing −Z. Center the 1.2 × 2.24 m doorway
horizontally. Center the 1.65 × 1.1 m window horizontally with its sill 1 m up.
Keep the holes clear in the mesh and omit collision nodes. The game defines
collision separately and scales the narrow end pieces to fit their slots.

Residential plans use 9 × 9, 9 × 12, 12 × 9, and 12 × 12 m layouts before
existing lot scaling. All have one floor, a centered entrance facing the road,
window bays, and a tiled floor and roof. City and industrial shells retain the
atlas footprint and height, with a walkable ground level and wider bays on
upper facades. Authored mission buildings keep the dimensions and gaps of
their original wall runs. The bridge keeper's locked room keeps its protected
collision, and the starter home still uses the player's build-piece rules.
Other wall slots have their own saved damage state and yield two wood or stone
when destroyed.
