# Doors, windows and perimeter gates

There is now a matching door wall, window wall and six-metre perimeter gate in each of the five tiers: 15 variants. The existing cabin doorway and tier 2 window retain their blueprint indices; 13 variants are appended, so older builds and research IDs remain valid.

| Tier | Door/window construction | Perimeter gate |
|---|---|---|
| 1 | Rough timber planks and bracing | Timber palisade gate |
| 2 | Timber with stone bracing | Stone-post timber gate |
| 3 | Riveted scrap steel | Scrap steel perimeter gate |
| 4 | Reinforced concrete and steel | Concrete bastion gate |
| 5 | Olive military plating and hardware | Military checkpoint gate |

Door and window walls are 3m wide and 2.5m high, matching standard walls. Door openings are 1.2m wide with 2.24m headroom. Windows have a 1.65m-wide open aperture above a 1m sill; advanced variants have a metal mullion. Gates span two wall modules and use two hinged leaves with a 5.4m clear opening.

B -> Structure contains door and window walls. B -> Defenses contains the perimeter gates. Tier 1 openings are known immediately. Researching a wall tier also unlocks its door and window walls at no additional research-point cost. Perimeter gates have a separate tier-by-tier Defenses research chain and use the Fortifier specialization at advanced tiers. All retain tier workshop and regional material requirements. A T1 door wall costs 5 timber + 1 fiber; its window wall costs 4 timber + 1 fiber; its perimeter gate costs 12 timber + 3 fiber.

Aim at a new door/gate and press F to toggle it. New manual hardware operates without electricity at every tier. Legacy powered doors and powered gates retain their existing power requirement. The leaves open away from the acting player. The full swept arc is checked against world structures, actors and other obstacles before a toggle; a blocked action leaves the saved state unchanged. Window frames have no glass collider across the aperture.

Rendering and engine-independent passage checks use the same part geometry. Side walls and posts remain solid when open, and open leaves still collide outside their closed footprint. Frames support roofs. Rotated edge snapping and roof centering recognize the door and window sections. Opening state and swing direction are saved and replicated.

Checks cover all 15 variants, wall-linked unlocks, the gate research chain and zone materials, rotated deck/roof snapping, solid posts and open leaves, real Godot hitscan through a door or window aperture, blocked closing, direct aimed interaction, manual tier 5 operation and saved swing state. A two-process guest test checks actual gate open/close requests and the client's physical collision geometry. Multiplayer protocol is 40; everyone must update together.

Screenshots show actual exported Godot models arranged on a display stage in a disposable test profile, rather than a player-built base.
