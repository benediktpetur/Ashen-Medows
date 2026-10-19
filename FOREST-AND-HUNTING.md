# Forest and hunting pass

## World

Woodland chunks now try 72 tree positions instead of 24; meadow and urban tree limits remain as before. At the screenshot viewpoint (seed 431, route X at Z -820 plus 170m), there are 166 trees within 64m, up from 50. Added trees have a taller overlapping canopy and use eight shared geometry/foliage variants. Leaf detail disappears at 75m, crowns at 190m; normal chunk streaming remains bounded. Ferns are batched per chunk and bushes are denser. Road corridors, authored locations, player structures and made paths retain clearance. Existing tree IDs and saved cuts are retained; clearing a build footprint must not shift the seeded positions of other trees.

## Wildlife

Meadows contain passive rabbits and deer, sometimes small groups. Woodland wildlife consists of tier 2 boars and pairs of healthy wolves. Infected dog packs remain a separate threat near the road and in higher zones. Deer and rabbits flee. Boars and wolves telegraph an attack for 0.75 seconds, then lunge briefly toward the last aimed position. They deal modest impact damage without zombie infection; dodging the lunge works. The local wildlife and global actor caps remain unchanged.

## Harvest and use

Kills leave a saved, harvestable carcass. F starts a three-second harvest; remain within 3m with a clear view. Moving away or disconnecting cancels progress. The world claim is shared, while the materials go to the harvesting character once. Unharvested carcasses spoil after 30 active minutes. Infected dogs provide no safe materials. Killing an animal does not instantly award cooked food.

| Animal | Raw meat | Hide | Fur | Bone | Sinew |
|---|---:|---:|---:|---:|---:|
| Rabbit | 1 | 0 | 1 | 1 | 1 |
| Deer | 4 | 2 | 0 | 3 | 2 |
| Boar | 3 | 2 | 0 | 2 | 2 |
| Wolf | 2 | 1 | 3 | 2 | 2 |

K -> Hunting opens cooking and processing. Hunting materials also appear in the inventory Materials filter, with a button opening that panel. Basic recipes are available immediately; advanced processing requires exploration tier 2 and workshop 2. Raw meat needs a nearby built campfire, field kitchen or communal hearth and consumes timber. It becomes safe trail provisions.

Eight recipes use the harvest: cooked meat, sinew cord (fiber), bone-tipped arrows, rabbit-fur hood, tanning hides into leather, leather boots, fur-lined vest and bone binding paste (binding fiber). The hood adds 10% insulation, boots 5%, vest 20%, while equipped; they use the existing gear slots. The overall warmth cap remains 70%. Unused materials remain in the portable character inventory.

World carcasses, harvesting claims and private materials are saved and replicated. No separate server setup is needed. Protocol 39 requires all co-op players to update together.

Checks cover measured forest density and clearance, unchanged seeded positions, saved tree felling, naturally seeded meadow/woodland fauna, attack warnings and damage, real timed harvesting, duplicate/spoil protection, cooking costs, all eight crafting recipes, equipped warmth, inventory and serialization. A real two-process guest test checks private yields, shared carcass claims, duplicate rejection and campfire cooking.

Screenshots are rendered from the game. The forest before/after uses the same seed and eye-level viewpoint. Animal screenshots show naturally seeded wildlife; inventory quantities in the crafting panel are staged in a disposable test profile.
