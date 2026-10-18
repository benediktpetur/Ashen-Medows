# Prepare, scavenge, come home

## Specialist scavenging

The existing South Block and its 28 named destinations remain in place. Tier 2+ hunting, medical, industrial, relay, freight and security sites have an additional specialist shutter, separate from any addressed community quest case. Clear the site's guards and satisfy its existing generator/repair/night requirement first.

- Open carefully: 20 active seconds within 6m of the entrance, with a small rustling sound.
- Force it: 3 active seconds, with a 70m base noise event. Walls and weather reduce effective hearing. It attracts existing infected rather than creating an extra wave.
- Walking away, dying, disconnecting or changing dungeon cancels the search. No loot is paid before completion. The player who started it receives the items; the emptied locker is shared and remains empty in that world.

Hunting lodges provide bowstrings and arrows. Clinics supply dressings and medical kits. Garages supply fuel, drive salvage and, at tier 3+, extended magazines. Relay sites supply zone-appropriate salvage and stocks or optics. Freight sites offer ammunition and transport salvage; security sites offer muzzle hardware and ammunition. Parts retain their workshop and weapon compatibility requirements. Existing community cases and their moral consequences remain separate.

## Awareness and distractions

Ordinary zombies remember the position they saw or heard. When they reach that position without reacquiring a target, they search a small area, moving more slowly and checking different directions before memory expires. They do not update this search from the player's unseen position. New noises redirect the investigation; visual contact restores pursuit. Specialist enemy and boss abilities retain their own behavior.

**G**, outside building mode, throws a physical stone. It costs one gathered stone and three stamina, with a three-second cooldown. The impact creates a 24m base noise event. It cannot distract an enemy that can plainly see and reacquire you. Throwing is host-authoritative; projectile visuals are replicated, bounded and cleared on room changes. Search state and target positions are included in enemy snapshots so guests see consistent behavior and music does not escalate just because an enemy is searching elsewhere.

## Expeditions

**F5**, the map or the field guide opens the planner. Depart from the southeast trailhead, a sleeping bag, the resident camp or a friendly settlement. Rumoured destinations remain labelled as rumours until discovered. Plans track one destination and the actual departure refuge; they do not teleport the player or provide a navigable path around obstacles.

Preparation checks show food, clean water, dressings, fatigue, open wounds, ammunition, generator fuel, the weather forecast and a rough walking/return estimate. The HUD and map track the outbound destination, then the return refuge. Reaching it and walking home records the result, including whether supplies were recovered. Turning back costs no gear or levels. Death interrupts the expedition while retaining the existing character/equipment death rules. Recent results are saved to the character; no repeatable research or XP reward is attached to walking the same route.

## Weather

Weather follows the shared seed and active world clock in twelve-minute periods. The first period is clear. Later periods can be clear, rainy, foggy or cold and windy. Forecasts give the next period and time remaining. All players share the same conditions; exposure and cover are personal.

- Rain has visible local drops and a quiet rain loop, reduces hearing range to 65%, reduces sight to 85%, and gradually wets uncovered players.
- Fog increases world fog and reduces infected sight to 60%.
- Cold wind reduces hearing to 85%. Outside the starting region, cold wind or a wet night builds chill. Armor and the comfort kit reduce its buildup.
- Chill above 60 reduces movement by 10% and slowly increases fatigue. Exposure does not directly inflict health damage or guarantee sickness.
- Actual overhead collision provides cover. Dry weather, shelter and warmth remove wetness/chill at different rates. Existing lit rest sites and a stocked upgraded camp hearth provide warmth.

Music, effects and rain retain pause/mute behavior. Weather does not load additional terrain chunks or increase enemy population limits.

## A camp worth returning to

Recruited residents have personal corners by their assigned beds, with colored blankets, mugs, storage and belongings. Residents gather near the developed hearth in the evening; guards patrol a wider camp perimeter. Returning home produces a short written greeting reflecting food, injuries, the player's condition and the expedition result.

F7 → **Camp life / improvements** offers three shared stages, paid from the acting player's materials. Residents need sheltered beds and stocked food/clean water first.

| Stage | Minimum workshop/exploration | Cost | Visible change |
|---|---|---|---|
| Personal spaces | 1 | 8 timber, 6 fiber | Personal storage and belongings |
| Communal hearth | 2 | 12 timber, 8 stone, 4 resin | Hearth, benches, warm light; warmth while stocked |
| Organized workyard | 3 | 12 timber, 8 alloy, 2 electronics | Worktable and stocked supplies |

Each stage adds 3 to the resident morale target and 5% to work rate. These are gradual/economy benefits; hunger, injuries, damaged beds and missing shelter still matter. Decoration supplies no structural support, automatic repairs or zombie immunity. Players still construct and maintain their buildings. Nearby player-built structures receive visible wear marks as their real condition falls; repairing the structure removes that deterioration.

## Validation and builds

`--journey-check` covers seeded weather, actual cover, exposure/recovery, real sight/hearing effects, searching, physical resource-cost distractions, travel/return records, finite timed themed loot, forced-shutter alerts, camp costs/art and save round trips. The normal build runs it before integration. Two-process tests cover private plans, distraction resource isolation, timed shared lockers, private loot, duplicate rejection and cancellation. `--journey-preview` captures planner, preparation, loot, rain and camp scenes. Multiplayer protocol is 34; all players need the same build.
