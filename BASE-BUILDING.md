# Research and base building

Open K. The four tabs share a five-tier workshop spine. Explore the appropriate zone, upgrade the workshop, research a blueprint using points, then manufacture its equipment or select it for placement. Research costs 1–5 points by tier. Basic shelter pieces and essential supply recipes start unlocked. Hover a node or select it for its full name, prerequisites, costs and model preview.

Points and unlocks are character progress. Death clears partial skill XP, but keeps research. First zone discoveries, manual categories, guarded dungeon loot and milestones award points once; their saved award IDs prevent duplication across seeds. Existing workshop saves retain their accessible tier blueprints.

## Build controls

- B: open building catalogue; select a blueprint to enter placement mode.
- Mouse wheel: cycle known, usable pieces. Tab: reopen the catalogue.
- 1–8: original foundation/floor/wall/half-wall/pillar/beam/stairs/roof shortcuts.
- Q/E: rotate; Ctrl makes fine adjustments. G toggles snapping.
- Page Up/Down: height; +/-: reach; R: reset height/rotation.
- Left click: place and spend materials. Right click: dismantle if remaining supports are valid. B exits placement.
- F near a base object: interact, repair or operate it.

Objects must fit clear space and have sufficient structural support. Advanced materials improve condition and carrying capacity. Unsupported construction is rejected. Infected can attack nearby structures while pursuing players. Destruction removes unsupported dependent pieces. Dismantling does not refund resources. Container contents are lost if the container is destroyed.

## What each tab contains

| Tab | Playable contents |
|---|---|
| Weapons and armor | Existing firearms, woodland bow, scrap shotgun, armor for four slots at all five tiers, weapon tuning and nocturnal tuning |
| Buildings | Five structural material tiers, doors, window walls, gates, storage, garage, repair station, mobile camp, generators, lights, powered doors, heavy support, rover |
| Defenses | Barricades, fences, spikes, snares, watch platform, reinforced gates, scrap barrier, noise lure, automatic turrets, electric fences, alarm, searchlight and shock trap |
| Survival | Bandages, provisions, ammunition, arrows, sleeping bags, campfire, kettle, water collectors, drying rack, crops, well, refrigerator, kitchen, medical stations, greenhouse, hydroponics and medical kits |

## Operating the base

**Storage:** F opens deposit/withdraw controls for seven materials, food, water, bandages and reserve ammunition. Each click moves up to ten. Storage is shared by players in the world. Standard storage loses one food portion every 30 active minutes; a powered refrigerator prevents this.

**Power:** fuel a generator with five fiber and one resin. A standard generator runs for ten active minutes; the efficient version runs for twenty. Powered devices must be within 24m. There are no wires yet. Electric lamps illuminate the base while powered. Powered doors need power to change state; they retain their last position when power fails.

**Production:** water takes two active minutes, drying four, farming six. Yield scales with tier and storage is capped at twenty times tier. F collects output. Tier 4+ production needs power. The starter kettle also consumes timber fuel. The campfire/kitchen can cook provisions from fiber or boil water with timber and stone. Solo menus pause production; online menus do not. There is no offline production.

**Medical:** powered stations manufacture dressings from fiber and improve slow natural recovery within 3m when needs and wounds permit it. Medical kits provide five minutes of improved slow recovery. Neither immediately restores HP nor shortens a wound's uninterrupted twenty-minute bandage requirement.

**Defenses:** spikes damage approaching hostile creatures. Snares need rearming with fiber after triggering. Electric defenses need nearby power. Turrets consume reserve ammunition loaded through F and require line of sight; Tier 5 turrets also need power. Noise lures use one electronic component for sixty seconds. Powered alarms warn of nearby threats. Peaceful wildlife is excluded from automated defense targets.

**Vehicles:** F enters the rover or mobile camp's driving mode. W/S moves, A/D steers, F exits. Vehicles stop for obstacles and steep changes. Use their menu to store/withdraw supplies. Repair stations restore nearby damaged vehicles for alloy scrap. The mobile camp offers safe rest when there are no open wounds or nearby hostile enemies.

## Prototype limits

These are usable first implementations. Vehicle movement is kinematic, with no suspension, towing, passenger seats or dashboard. Farming produces generic provisions on timers rather than simulating individual crop species. The bow uses hitscan rather than an arrow flight simulation. Tuning kits use the existing quality/noise system rather than independent attachment slots. Community research projects and legendary boss ascension are not implemented in this pass.

All multiplayer players need protocol 6 / the same updated build. Structures and station contents save per world seed; research and equipment travel with the character.
