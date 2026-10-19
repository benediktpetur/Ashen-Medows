# The World You Leave

Twelve optional world projects reuse the existing South Block locations. These are persistent changes to a world seed, shared by everyone on that server. They do not replace the existing story encounters, addressed supply cases, settlement storylines or research choices.

Visit an abandoned location and press F at its notice board. The project entry explains both outcomes and their exact cost before commitment. The journal and map now open a World Changes ledger. Map squares mark committed work: amber in progress, green restored, red dismantled. Undiscovered projects are not revealed by the ledger.

| Project | Progression | Restore | Strip |
| --- | --- | --- | --- |
| The honesty pantry | T1 / Bracken Farm Stand | Transfer surplus from Emberstead into shared traveller supplies | Take the last emergency food and water |
| Save the beans | T1 / Hollow Creek Allotments | Growing beds supply food locally and to Emberstead | Take the frames and fiber |
| A roof for the next person | T1 / Miller's Picnic Shelter | A working rest stop and expedition departure refuge | Take timber and fabric; lose the refuge |
| Water belongs to someone | T2 / Broken Pump House | Clean-water collection and regular water for Emberstead | Salvage resin and alloy; lose the service |
| Keep the west trail open | T2 / Pinewatch Radio Tower | Working signal lamps and less future local spawn pressure | Electronics and fuel; a more dangerous local pocket |
| The names on the dressings | T2 / Saint Wren's Clinic | Dressings, water-illness treatment and gradual recovery support | Finite dressings and medical kits; no clinic |
| A spring worth waiting for | T2 / Foxglove Glasshouse | Food for travellers and Emberstead | Timber, fiber and resin; destroyed planting beds |
| A road home | T3 / East Spur Garage | A marked, persistent relief path and lower local pressure | Immediate alloy and fuel |
| Somebody has to deliver it | T3 / Rafi's Rail Locker | A relief depot supporting travellers and the recovery settlement | Empty its food, water and alloy reserves |
| One more warning | T3 / Cinder Signal Box | Working lamps and lower future local spawn pressure | Salvaged electronics and alloy; increased local pressure |
| The route out | T4 / Civic Evacuation Control | Persistent relief corridor and reduced local pressure | Alloy and electronics; broken evacuation markers |
| What the city gets back | T5 upgrade / Harbour Quarantine Works | Endgame treatment hub and water for Mercy Row | Bioalloy, electronics and medical kits; the hub is lost |

## Playing the projects

1. Clear the location's original guards and repair or power its original equipment where required.
2. Commit to restoration or dismantling. Tier materials, workshop level and actual supplies gate the choice. Committing pays once and permanently locks the branch for that world.
3. Work within 24m for 45 active seconds to restore or 20 to dismantle. Tier 2+ construction/dismantling attracts nearby infected. Local hammering provides an audible cue. Leaving pauses work; progress and committed materials are saved. Party members can continue the work.
4. Return to the changed site. Growing beds, water tanks, treatment cots, shelters, lit poles, stocked shelves or discarded boards represent the result. Relief paths also alter the actual terrain/path system.

There is no failure timer for leaving an opportunity undecided. Existing timed settlement rescues retain their original rules. Restoration and dismantling are separate from existing addressed community cases, so this system cannot silently consume or invalidate those quest items.

## What changes mechanically

- Linked settlements gain 5 trust for restoration or lose 6 for dismantling. Their existing reputation, service and progression rules consume that standing normally.
- Gardens add two provisions to shared site storage and two food to their linked living settlement every 600 active seconds. Waterworks similarly supply two water locally and to the settlement. Depots provide food and water locally and to their linked town. Town reserves cap at 40.
- The pantry transfers one food and one water from Emberstead only while both town reserves exceed four. It does not create supplies from nothing or drain the town to starvation.
- Shared food/water storage caps at six each. Clinic dressing storage caps at three, producing one per 600 active seconds. The T5 hub also provides water to Mercy Row. Evacuated settlements are never resurrected by production.
- Clinic treatment clears water illness and supports gradual recovery for five minutes; it does not grant instant health or patch bleeding. Refuge use restores 20 energy, adds 5 morale and provides five minutes of comfort. Both services have a shared ten-minute cooldown and require no hostile enemies within 20m.
- Restored warning sites multiply future normal spawn pressure by 0.65 within 85m. Refuges and relief trails use 0.75. Dismantled sites use 1.2. Overlaps are bounded between 0.5 and 1.5. The starting sanctuary stays at zero pressure. Bosses, scripted encounters and existing enemies are not deleted or weakened.
- Relief routes add up to 25 path patches along 48m, avoiding existing player structures and world boundaries. They do not open the closed river expansion.
- Production uses active simulation time, is capped, and cannot build up while the game/server is shut down. Shared collection transfers items to exactly one acting player and empties the claimed stock.

World state, timers, empty stock, irreversible choices, paths and resulting ecology survive saves and server reloads. Portable characters retain their private inventory; project choices belong to the visited world. Multiplayer protocol is 36, so every player needs the same build.

## Validation and captures

`--world-works-check` covers all 24 branches, guard/range/tier/material gates, paused and completed work, repeated commitment, stock caps/conservation, clinic cooldowns, actual paths/refuges, ecology and disk reloads. It is included in `scripts/build-game.ps1`.

The two-process co-op suite exercises a guest committing the choice, paying their own materials, receiving shared world state and dismantled artwork, collecting the finite stock and being rejected on a duplicate claim. Its completion timer is accelerated in that fixture; standalone checks exercise the actual work duration.

`--world-works-preview` captures the actual renderer with staged project states to compare restoration and dismantling. It does not modify a player's normal save. Captures illustrate outcomes, rather than documenting a normal playthrough.
