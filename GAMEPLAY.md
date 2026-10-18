# Expedition gameplay port

This ports the existing Expedition game logic into Godot .NET. Survival base defence in game2 remains separate. None of the deferred NPC-community ideas, drivable cars, boss systems, legendary boss loot or river expansions were present in the Expedition prototype, so those are not claimed as migrated features.

## Controls

- WASD: move; mouse: look; Shift: sprint; Space: jump; Esc: pause/resume.
- Left click: shoot; right click: aim toggle; R: reload (1.4 seconds).
- F: gather nearby resource, enter/exit dungeon, open a cleared chest, collect well water or read abandoned notes.
- K: crafting/workshop; I: guns/armor; J: six attributes; M: south-block map.
- H: bandage most urgent open wound; V: eat; C: drink; L: flashlight; U: mute generated audio.
- B: building mode; 1â€“8: foundation/floor/wall/half wall/pillar/beam/stairs/roof; Q/E: rotate (Ctrl fine); G: snap; Page Up/Down: elevation; +/-: reach; R: reset orientation/height; left click: place; right click: remove if supports remain valid.
- T: chop/hold to continue; P: clear/hold to extend paths; O: dig well (bench 1, 4 timber, 8 stone).
- F6: place crafted sleeping bags, bind respawn, travel and rest.
- N: journal; F8: Godot co-op host/join/session menu (see MULTIPLAYER.md).

Solo menus pause gameplay, needs and wound timers. Online menus keep the world and timers running. Mouse capture resumes when you close a menu. Settings currently reset each launch; saved character/world state does not.

## Preserved rules

The original recipe table, crafting resource tier gates, skill XP curves and death penalty, wound timers/bandage recovery, armor stat/loot rolls, structural weight/support evaluator, south-block terrain equations, river boundary and dungeon layouts are copied as engine-independent C# logic. Game adapters use real Godot collision/raycasting and UI instead of Windows Forms/OpenGL calls.

T1 safe meadow â†’ T2 woodland â†’ T3 outskirts town â†’ T4 south city; T5 city sewers/tower and a lower hospital ward. There are seven entrances, three room chests each, 3/5/7/9 guards by tier, and guarded deep bioalloy caches. Loot IDs preserve duplicate protection across saves and the legacy importer. Clear a T4 floor and interact at its northern lower-ward entrance to descend. North river remains closed.

Crafting includes all old benches, tiered guns/armor, rounds, sleeping bag kits, bandages, provisions and night weapon tuning. Building pieces consume materials and advanced blueprints require research; unsupported towers and unsafe support removal are rejected. Stair collision uses a ramp with stepped visuals. Art and enemy animation are procedural placeholders; polished models/materials remain separate work.

Strength trains on trees; Agility/Stamina on actual sprinting; Marksmanship on hits/kills; Crafting on recipes; Survival on gathering. Higher Strength accesses harder trees. Death clears partial XP in all six attributes and retains earned levels/items. Night infected and gun drops provide catalysts/scavenged-quality bonuses. Gun quality is per weapon type, matching the prior loadout data model.

Bites add individual wounds. One open wound has 600 seconds of blood; each open wound drains all remaining blood timers N times faster. H patches one wound without healing health; patched wounds recover after 1200 active seconds. Timers/needs persist and don't advance offline. Health recovers at 0.08 HP/s (plus Survival) after 60 seconds without damage, sufficient hunger/water and no open bleeding. Health impact itself may kill sooner. Solo menus pause these timers; online menus do not.

Sleeping bags need workshop 2. Fast travel is same-seed, daylight, no wounds, safe camps, one provision, one water, 25 energy and ten-minute cooldown. Rest at a safe bag restores fatigue reserve. Respawn falls back to the meadow if a camp is blocked or nearby enemies make it unsafe. Camps, buildings, clock and terrain edits are per-seed; character gear/attributes travel between seeds via pause-menu world selection.

The world streams 64m chunks with a 7Ã—7 active window (49 maximum), generates hills/valleys and increasingly dense towns/cities, and persists tree removals, paths, wells and burned wrecks. Only nearby affected terrain meshes/physics rebuild after earth edits. Trees/rocks/buildings block movement and shots. Enemy sight uses Godot line-of-sight queries, shots/chopping/sprinting alert infected, and night variants appear in T2+ while the meadow keeps low density. Static foliage/props differ visually from the old renderer.

## Persistence and import

Godot user:// uses the stable custom AshenMeadowExpedition application-data folder. Its expedition/profile.json contains the character, per-seed world edits and building lists. Writes are atomic with a .bak; corrupt files are protected from replacement. If the existing save is corrupted, repair/restore it outside the game rather than saving a replacement over it. Mutation failures roll back costs/state.

Esc â†’ Import Old Expedition Character reads the selected legacy profile and compatible buildings in the sibling worlds directory. Import is explicit and replaces the active character; choose the legacy Expedition profile, not the unrelated Survival profile. Old source data remains untouched. Different procedural object geometry means some imported edits may have different visual placement. Standalone legacy and Godot save locations remain separate.

## Remaining migration work

Godot direct-connect co-op and shared world state are implemented; read MULTIPLAYER.md for usage and current party/session limits. Cross-server character validation, public matchmaking, relay traversal and dedicated-server mode are not implemented. Full skeletal characters, cinematic animations, authored environmental art, the old detailed room dressing and richer ambient audio are not reproduced by this logic port. The journal retains the six stories with simplified abandoned-location interaction. No release is published remotely until you upload and run the GitHub workflow.

Wildlife and biome population: seeded starter-valley deer and rabbit pairs are peaceful and flee nearby survivors or gunfire. Woodland (tier 2) adds boars and infected dog pairs alongside level-2 logger/drifter packs. Drifters occupy the valley, road workers the highway/town approach, office revenants and security the city, security the tier-5 depths, and stalkers/bloaters the existing night waves. Animal spawn IDs include species, seed and cell; only loaded nearby areas are populated, with a local wildlife cap of 14 per player area. Co-op uses host-authoritative movement/combat and the existing actor replication. Deer/boar award 3 food, rabbits 1, infected dogs no edible food; rewards are saved once per ID and do not roll firearm loot. Collection is automatic on a kill, using the current food inventory. Deer/rabbits do not invalidate safe sleeping bags. Boars deal 9 direct damage per charge (2.1s cooldown); infected dogs use existing tier-2 bite wounds. Animals remain stylized, with basic four-leg gait animation.

Restored world storytelling: the six old Expedition site types now populate nearby terrain (campsite, evacuation wreck, triage, memorial, ruined shelter, checkpoint), using the old hash layout and original journal text. The existing starter camp remains. Fixed early memorial/evacuation/shack sites and forest triage/roadside checkpoint anchors supplement procedural sites. Sites reject steep ground, buildings, nearby trees/rocks, dungeon entrances and player structures. They load within 190m and use the existing 300m cleanup; scans are throttled, with cell changes checked immediately. F reads the visible paper by belongings, N reopens recovered fragments. Story wrecks use the existing noise/explosion/persistent burned-state system. Dungeon reception, triage and refuge records now appear at the old locations with extra beds and shelving. Journal discoveries persist per character; reading online opens the guest's own letter UI without pausing the host. This pass restores world stories and some room dressing, not the deferred community, vehicles, bosses or expansion systems.

Terrain relief revision 2: visible rolling meadow hills near spawn, seeded multi-scale woodland slopes, forest ridges and valleys. The spawn pad is small and level; the central route follows a gentler grade and city building footprints blend onto level pads. Rendering retains the 49-chunk local window and a coarse distant mesh; no whole-world geometry is loaded. Existing pre-revision saves preserve the original ground around saved structures, paths, wells and sleeping bags, blending into the new hills beyond their footprints. This protection uses an indexed, per-world list and persists across saves and co-op snapshots. Existing seed, biome boundaries, edits and character progress remain. Terrain screenshots are produced by --terrain-preview.

Southeast progression and population pass: new characters and unbound respawns start at X=1100, Z=160. Existing saved positions stay intact; Esc > Return to Southeast Trailhead relocates solo players to the new starting camp. Fresh seed selection also uses that start. The trail bends northwest and joins the original route at Z=-550; northern terrain, tier bands, river boundary and dungeon positions remain. The old terrain grading equation is retained, preserving saved settlement support. A new camp and timber/stone/fiber starters support this entrance.

Spawn heat now combines tier, woodland cover, road proximity, nearby generated buildings and dungeon approaches. The initial 90m around the southeast trailhead suppresses enemy spawning, while wildlife remains. Habitat-selected packs use seeded dispersed positions outside a 22m player exclusion radius; solo and host players use the same population function and existing streaming/caps. Night increases ordinary pack pressure alongside the existing special night waves. Woodland dogs favour roads; boars inhabit deeper woodland. M offers terrain/progression, daytime heat and nighttime heat views. Heat represents spawn pressure rather than a live count of enemies or a guaranteed safe area against pursuing enemies.

Content/art: thin curved wind-moving grass replaces the large triangle clumps, mixed conifers and round-canopy trees use softer leaf shading, and nearby chunks gain wildflowers, fallen timber, undergrowth, trail fencing and street debris. Seeded supply containers (F) award zone-appropriate materials, provisions and higher-zone ammunition, persist once per seed and synchronize through host-owned gathered state. Scans are throttled and decor remains attached to streamed chunks. Supply collection preserves well interaction priority. No vehicle-driving, new bosses, NPC community or river expansion is included in this pass.

Workshop presentation: K opens the research/crafting flow. Each tier connects biome-material discovery to its workshop upgrade and equipment branch. Select a workshop node to inspect that tier's recipes; recipe details show material counts, station/discovery requirements and lock reasons. Completed upgrades are marked, and unavailable craft actions are disabled. Existing upgrade costs, discovery gates, crafting XP and host-authoritative co-op crafting remain; no separate research currency or unlock-save format is added. A detailed wood-and-steel worktable with vise, tool rack, drawers and blueprint stands beside the southeast camp; approach and press F to open the same screen. Portable K access remains. --workshop-preview captures the model, available upgrade, sleeping-bag lock and experimental-tier lock using isolated saves.

HUD/inventory pass: compact survival panels, compass, weapon/ammo/chest status and key-labelled quick slots (H/V/C/L/I/K) replace the old text HUD. I opens a searchable, sortable inventory with category filters, distinct material/gear/supply glyphs, saved primary/chest equipment, item details and actual equip/use actions. Bandage/food/water buttons use the existing saved counts and server-authoritative co-op operations. Camps/workshop actions open their existing menus. No new capacity, weight, durability, drag/drop or additional armor-slot rules are implied. Open bleeding countdown now excludes patched recovery timers; online inventory status keeps updating while the world continues. --inventory-preview renders HUD, bleeding, gear and supplies from isolated test saves.

Supported item-model pass: ITEM-CATALOGUE.md lists 41 definitions spanning six weapon variants, five armor tiers, seven materials, seven supply/upgrade visuals, five workshop tiers, eight building parts and three existing utility tools. Current recipes all have matching 3D previews, material nodes use distinct world models, first-person weapons use the shared tier-variant builder, placed sleeping bags have bedding detail and local inventory portraits show equipped chest armor. Unlocks, quantities and combat rules are unchanged; catalogue inclusion does not grant items. Utility-tool models do not introduce new equipping rules.

## Stats, gear, wounds and impact feedback
- J opens six activity skills, exact XP bars, permanent levels, effective health/stamina/armor/speed/damage and training guidance. Levels cap at 100; death clears partial XP in every skill and keeps levels and belongings.
- I includes primary weapon plus head, chest, hands and feet. Seven new armor recipes provide actual slot equipment. All equipped armor/vitality bonuses stack; empty older slots remain empty and legacy plates remain chest gear. Click occupied small slot buttons to unequip. Equipment saves in the portable character.
- Y opens wound management; H patches the most urgent open wound. Each untreated wound starts with 600 blood seconds, draining at the number of open wounds per second. One fresh bite is fatal in ten minutes; two in five. Each bandage pauses one wound's remaining blood and cures it after 1,200 uninterrupted active seconds, without restoring HP. Removing a bandage resumes blood loss and resets healing. Solo pause/offline time does not advance healing; online menus do. Food/water support slow natural HP recovery, never instant bandage healing.
- T equips/puts away the axe. Click once per swing, aim at a trunk within 3.5m, and keep the trunk in sight at contact. Solid objects block the strike. Successful strikes consume eight stamina, make noise and chips; felling awards timber and Strength XP. Trees visibly fall and their colliders are disabled. Holding T no longer auto-harvests.
- Enemy hits show confirmation, headshot markers, stagger and blood fragments. Deaths leave a posed fallen model and blood pool for 30 seconds. Effects are bounded to 160 fragments and 16 remains; remains have no collision. Incoming damage adds blood, an impact sound, short view shake and existing red damage tint. These are stylized effects, not dismemberment or physics ragdolls.
- Multiplayer protocol is now 3: everyone needs this build. Host validates gear ownership and axe rays; character/wound state remains authoritative. Localhost two-process checks passed.
- Collision checks cover solid wall ray occlusion, player capsule obstruction, trunk aiming/rear rejection, hill grounding and existing world furniture/dungeon tests. This is automated coverage, not an exhaustive traversal of every generated seed.
Four-tab research and building pass: K opens Weapons and armor / Buildings / Defenses / Survival. The catalogue now has 126 recipes, 83 placeable objects and 139 modeled definitions. Basic structural pieces, bandages, food, ammunition and arrows start known; other blueprints cost their tier in research points. Workshops still consume their existing material costs and grant eight points on first construction. Research does not consume crafting materials; manufacturing/placement does. Research, points and reward-history IDs travel with the character and survive death. Existing saves retain their workshop-tier unlocks.
Points come from first zone material discovery (15/20/25/30/35), each of six manual categories (25), first guarded dungeon loot per dungeon ID (25), first shelter/camp/tree/harvest/drive/vehicle repair (8 each), and first nocturnal kill (10). Award IDs are independent of world seed, so reading duplicate notes or switching seeds does not farm points. The initial valley counts as the first discovered zone.
B opens the build catalogue. Select an unlocked blueprint, aim, rotate/snap/adjust elevation, then left click to place. Mouse wheel cycles usable blueprints; Tab reopens the selected piece's tab. Keys 1-8 retain the original structural shortcuts. Green placement also requires sufficient materials. F opens station interactions; storage is shared in multiplayer. Walls and supports can be attacked by nearby pursuing infected. Destroyed supports remove dependent unsupported pieces; placed stock is lost when its container is destroyed. Manual dismantling still rejects unsupported remaining structures and does not refund materials.
There are five structural material tiers with increasing capacity/condition, opening doors/gates, window walls, storage, fueled generators, lamps, garage gates, repair stations, mobile camp and rover. Defenses include barriers, fences, spikes, rearmable snares, lures, alarms, powered fences/shock traps, watch platforms and ammunition-fed automatic turrets. Turrets use line of sight. Survival includes cooking/boiling, water collectors, drying racks, crop beds, refrigeration, medical stations, greenhouse and hydroponics. Production is timed during active gameplay; powered Tier 4+ production requires a generator within 24m. A Tier 1 kettle also requires timber fuel. F shows costs and operation details.
Woodland bows consume crafted arrows and use a quiet hitscan attack; scrap shotguns resolve six pellets per shell. Armor covers head/chest/hands/feet at each tier. Tuning kits improve the equipped weapon's quality and reduce noise; nocturnal tuning remains the 25% quality endpoint. Medical kits double slow recovery for five active minutes, without restoring HP immediately or shortening the 20-minute bandage requirement. Powered medical stations improve slow recovery nearby when wounds are patched and needs permit it.
Vehicles/mobile camp are kinematic prototypes: F enters driving mode, WASD steers/drives, F exits; they check obstructions and slope, and carry deposited supplies. They do not have suspension, towing, passenger seats or a vehicle dashboard. Generators use proximity power rather than wiring. Farming uses timed generic provisions rather than seed/crop/weather simulation. Research projects/NPCs, boss-specific legendary ascension and community management remain future content. See BASE-BUILDING.md for the operational guide. Multiplayer protocol is 5; update every tester together.

Main character levels and talents: J now opens the character talent tree, with an ACTIVITY SKILLS button for the six existing action-trained attributes. Main level starts at one and caps at thirty. XP required for the next level is 300 + current level Ã— 100. Each earned level gives one talent point. There are twenty talents across Combat, Toughness, Mobility and Survival: four two-rank talents plus a one-rank capstone per branch. Rows require main levels 2, 4, 8, 12 and 20, the previous talent, and six branch points for a capstone. Thirty levels award 29 points; fully filling every branch costs 36, so characters specialize. Reset is free during playtesting and refunds the earned budget without granting levels or XP.
Main XP comes from infected kills (20 + eight per enemy tier), gathering (10), tree felling (15), crafting (12 per recipe tier), construction (three per tier) and first discovery/milestone rewards (four XP per research point awarded). Main XP has its own save field and is separate from activity XP and research points. Death zeros current main XP and all activity XP, retaining earned levels, talent ranks and available points. Legacy characters with no main-level record receive a level for every three already-earned activity levels, capped at thirty, and may allocate those points freely. Talents and main progress travel with the saved character across seeds/co-op.
Talent effects apply to damage/headshots/reload/melee, maximum health and effective armor, slow recovery and enemy attack mitigation, stamina/sprint/speed/recovery/noise, needs drain, harvest yields and limited bag travel. They do not prevent bite wounds, extend fatal bleed timers, shorten bandage healing, or restore HP instantly when purchased. Wayfarer reduces safe bag travel to fifteen energy and seven minutes, retaining the other travel restrictions. Online talent purchases and refunds are host-authoritative. Protocol is now 6; everyone needs the same updated build.

## First woodland boss: The Foreman
Four discoverable warning notes lead from the southeast trailhead to Ashen Lumber Mill (woodland road, z -780, 90m east of its center). Read with F; revisit through N. The mill is marked on M. Ring the southern entrance bell with F to start the optional encounter.

The oversized bloated logger has 1,400 solo health, plus 50% per extra nearby player. Orange slam circles demand distance; green vomit lanes leave nine-second corrosive pools; red charge lanes can be baited into the physical log piles for a five-second stun. Recovery grants 1.5x damage, stun 2x. Below half health, warnings shorten by 25%. Retreat is always possible; eight seconds without nearby living players resets the fight. Building is blocked inside the arena.

First victory per character awards 15 research points, bonus character XP, a tier-two reinforced apron, bandages, ammunition and one Foreman core. Research Foreman weapon tempering in Weapons and armor, then craft it at a tier-two workshop with the core, fiber and resin. Victory persists per world seed; the reward cannot be farmed by moving between seeds. All players need networking protocol 7 builds.

## Scavenging surprises
Twenty-two percent of seeded outdoor supply crates are eligible for a hidden radio alarm, falling can rig, or distress flare. Tier-one crates are excluded, as are crates within 80m of the mill and 40m of the searching character's saved camp. Opening an eligible crate keeps its normal loot and starts a noisy, flashing source. Existing infected within 110m investigate it.

The first small wave arrives after six seconds; reinforcements arrive at eighteen seconds. Spawn attempts use valid streamed outdoor positions 30â€“45m from the source, at least 18m from every player, with bounded enemy counts. Press F within three metres of the source to silence it and cancel future waves, or retreat. Already approaching enemies remain dangerous. The source automatically expires after thirty seconds. There is one active incident per world, a four-minute active-play cooldown, and permanent spent-trap records per seed. Cooldown and active incident progress survive saves. Entering a party dungeon cancels the incident. These are surface loot surprises; no random instant damage is applied.

Co-op uses the host's shared incident, enemies and spent records. Protocol 8 is required for all players.

## Combat pursuit and salvage (protocol 9)
Humanoid zombies use bounded local obstacle routes with physical line checks and body-width clearance. Cached routes refresh around changing structures; route searches are limited to one per physics frame, 180 search nodes and an 18m local radius. Long routes and inaccessible upper floors are not a full navigation-mesh solution. Zombie bites now have a 0.6-second visible wind-up and a recovery period: escape melee range before impact, or interrupt with a hit. The impact checks range, height and actual sight again. Heavy hits visibly stagger the torso and cancel attacks. Alert growls, attack snarls and impact sounds play spatially nearby, respect mute, and have a bounded voice count. Co-op snapshots include attack/recovery, memory and stagger state.

Six contextual salvage models accompany existing abandoned sites: provisions, evacuee travel packs, medical lockers, memorial emergency tins, workshop chests and checkpoint ammunition. Medical caches provide bandages and medicine; workshops provide building materials; checkpoints supply ammunition; other caches provide food, water and a bandage. Each also provides zone-appropriate materials. Eighteen percent contain a named rare armor piece in one of four equipment slots, matching the zone tier. Gear is added to inventory rather than auto-equipped. Search with F. Claims are shared per world and persist; only the searching character receives the inventory reward. Eligible caches can trigger existing scavenging surprises.

Seeded hunting shelters and roadside repair stops now appear off the progression road where terrain, trees, buildings and dungeon clearance permit. Their geometry and nearby supplies use the existing graphic-novel style. Searchable supplies remain separate from story notes.

## Survivor community (F7 / protocol 10)
Three seeded rescue encounters introduce Mara Holt, Ivar Reed and Nora Vale along the southeast-to-northwest route, near z -240, -560 and -1080. M marks the encounters and your established community. Clear each encounter's two, three or four infected, then stand beside the survivor and use F to talk and rescue. Rescued survivors follow their rescuer; another nearby player can resume the escort through FOLLOW ME TO CAMP.

Research the tier-one Survivor cot under Survival, then place it through B. Each recruit needs a separate cot with a roof overhead and at least two nearby full walls, window walls or doors. Bring the rescued survivor beside you near the bed and recruit for three food portions, which enter camp stores. The first cot establishes the shared camp; subsequent cots must be within 20m of it. Maximum three residents per world seed. Destroying a cot or shelter stops that resident's work; build a replacement and use ASSIGN FREE SHELTERED COT.

F7 opens management from anywhere. Job changes require standing beside the resident or within 20m of camp; supply transfers require being at camp. Deadwood workers collect two basic timber per two effective work minutes into shared stores (TAKE 5 TIMBER). Supply tenders use one stored water and one fiber to produce two food per three effective work minutes. Guards patrol near home and attack visible threats. Each resident consumes one food every five active minutes; food, shelter and injuries affect morale. Low morale slows work; injuries at 70 or above stop work and companion duty. Fed, sheltered residents at home recover 12 health and injury points per active minute. Stock capacities: 200 timber/fiber, 100 food/water, 200 ammunition. No offline progress or offscreen raids are simulated.

One active companion is allowed for the whole community. COME WITH ME binds them to the requesting player, including a guest; RETURN TO CAMP JOB releases them. Companions and guards fire at visible hostiles using shared ammunition (ADD 12 AMMO), and use melee if an enemy comes close after ammo runs out. They can assist against The Foreman and are affected by his attacks and sludge. Hits can injure or down survivors. One bandage revives a downed survivor and provides slow first aid over a minute; otherwise after 90 active seconds they recover at camp with serious injuries. Residents do not permanently die in this first version. NPCs can step onto low foundations and use the same bounded obstacle routes as zombies; unobstructed beds/entrances are recommended. Unobserved escorts can catch up in safe conditions; unobserved residents return home after being released. Companions wait outside dungeons. Community ownership, positions, stocks, jobs, morale and injuries persist in the server world's save, separately from portable player characters. Networking protocol 10 is required for everyone.

## Willow House / The House That Waited (protocol 11)
A unique authored woodland home stands 55m east of the progression road near z -930. M marks WILLOW HOUSE. The exterior has a pitched roof, chimney, neglected flowers, broken fence, front-yard swing set, boarded windows and a dusty car beside the garage. Physical ramps connect the raised house and garage floors to terrain. Enter through the open front doorway; inspect the family portrait and kitchen note with F. The family portrait depicts two adults, a child and an infant. Their fate is implied through abandoned belongings, ration entries, sparse old stains and a covered adult-sized form; no graphic injuries to children or suicide method are shown.

Use F at the marked basement hatch to enter the authored basement through the existing party-room transition. A fallen shelf physically blocks the safe-room door. Equip the axe with T, aim and swing three times (8 stamina each), or shoot the shelf. F opens the cleared door, triggering a single frail adult infected with 1 HP and a brief non-damaging lunge before ordinary attack wind-ups. The surprise happens once per world. If left alive, the frail infected remains on subsequent visits without replaying the lunge.

Inside are a sofa, TV, double bed, bunk beds, tiny kitchen, toilet, empty tins, abandoned restraint and ration table. F at the table takes the shared garage keys; F again reads the ration book. N revisits the portrait, kitchen note and final ration entries. Mild warm-light fluctuation and dust provide atmosphere. Return upstairs with F near the basement entrance, unlock the garage with F, then search the stores inside.

The garage awards 96 reserve rounds, 6 bandages, 4 food, 4 water, 12 timber, 8 stone, 10 fiber, 8 resin and a guaranteed tier-two pair of reinforced mechanic gloves (10 armor, 5 vitality). Garage rewards are claimed once per world, by the searching character. Shelf damage, door opening, key possession, garage unlocking, enemy defeat and cache claims persist. In co-op, the host leads basement transitions with the party regrouped; guests can break the shelf, open the safe room, take shared keys and claim garage supplies. Everyone needs protocol 11. Player construction and ordinary surface resource/enemy spawning are kept outside the story lot.

## Willow House architecture update (protocol 12)
Willow House now follows the supplied ageing Victorian farmhouse reference: two storeys, a projecting front gable, tall sash windows, distressed grey clapboard siding, mossy shingle roofs, eave trim, turned porch posts, decorative brackets and broken railings. Dedicated procedural siding and roof materials keep the geometry light while showing weathering. The TV branding and people from the reference are not reproduced.

A real internal staircase reaches furnished upstairs sleeping areas. Its open stairwell, upper landing and railings have collision. The family portrait moved to the rear-left downstairs wall; the kitchen note, basement entrance, safe-room encounter, shared garage keys and garage reward are unchanged. Front and garage access ramps now use solid convex collision surfaces. Everyone must update to protocol 12 for matching house geometry.


### The First Fish — woodland story camp
A handcrafted camp lies west of the woodland route at Z=-690, about 65m off the road. Two tents, a cold cooking fire, a fallen pot, fishing equipment and broken defenses tell its story. Three visible spike traps can be disarmed with F from a safe distance; stepping on one causes a small armor-mitigated hit and alerts nearby infected. Traps stay spent for the world. Two infected lurk outside; a third kneels beside the torn tent and reacts to approach, injury or noise. Clear that infected, then press F at the backpack to recover the weathered journal; revisit it with N. The child is represented through a blanket and belongings, without graphic injury. The journal follows the rescue, first fish, survival lessons, broken foot and last defense. Authored enemies do not return after being killed in that world. Camps stream with nearby terrain; their traps and infected are host-authoritative. Everyone needs protocol 13.


### Just a Fever — side mission
Noah approaches travellers on the woodland road around Z=-425. Press F to help, follow his route, and stay within 16m; he waits when you fall behind. If the original escort leaves the session, another player can resume with F. He leads to a weathered cottage east of the road at Z=-470. His walking dialogue describes his missing father, soup through a locked-door slit, and his sister Lily's wrapped hand. Check Lily on the sofa with F to trigger a three-second, non-graphic door-breaking sequence and a fight with their infected mother, Mara. Nearby players receive short damage protection during the fade. Children have no hit colliders, cannot be attacked or looted, and are represented only through implied loss in the aftermath. Defeat Mara, then read the notebook on the table; the completed record is portable and available through N. This is a fixed tragic narrative outcome, with no failed-quest penalty or required consumable. Encounter stage, escort position, door state and completion persist per world; movement and actions are host-authoritative in co-op. Protocol 14 requires everyone to update together.


### The Door I Closed — church letter side mission
At Saint Brigid's Church on the east side of the outskirts town (Z=-1680), Daniel is dying on a cot near the altar. F accepts his request and opens an unsealed confession: he barred his healthy wife Ruth outside to keep the remaining food. His brother Samuel's safehouse is across town to the west (Z=-1776); accepting the request adds a gold delivery marker to M and a distance objective to the HUD. Deliver with F beside Samuel or Ruth. The wife survived and lives with Samuel. She reads the letter, thanks you quietly and asks you to leave.

The later scene waits at least three active minutes after delivery, requires leaving the safehouse, and only changes when every player is more than 70m from both landmarks. This prevents the scene changing while anyone is watching in co-op. Return to find Samuel's body in a church pew beside the folded letter. The safehouse is empty, and Ruth is missing. There is no explanation of the death or a named culprit. The mission has no combat gate or consumable cost. F examines the aftermath; recovered pages stay in N / Journal. Encounter stage, delay and discovery persist per seed. Both enterable buildings stream near active players and reserve their lots against generated buildings, trees, loot sites and player construction. Network protocol 15 requires everyone to update together.


### Sammy's Ninth Birthday — environmental ambush
A house east of the late woodland route (Z=-1216) contains a spatial, original music-box tune, a set table, nine candles, wrapped gifts and a birthday banner. Sammy sits at the head of the table in a paper hat. He stays seated despite proximity, noise or injury; companions and automated defenses do not target him while dormant. F at the cake takes one slice and grants one food to that player, cuts the music, then starts a short rise animation before normal pursuit and attacks. The table and chairs are real obstacles. The slice is shared and cannot duplicate in co-op. You can safely stop the music box with F, or deal with Sammy before touching the cake. His fight uses no blood particles, gore, or corpse remains; a fallen party hat marks the quiet aftermath. Reading the banner with F adds a portable journal entry. Cake, music and defeat persist per seed. Geometry and the encounter stream near active players; everyone needs protocol 16.


### The Price of Crossing — bridge checkpoint
Three exhausted, armed survivors hold a bridge over a small creek on the outskirts road at Z=-1520. F at the south barrier opens their demand: two food portions for passage. Paying charges the acting player once and opens the crossing for the whole party. After crossing north, a one-time scene plays behind you: gunshots, a frightened traveller surrendering his pack, and the guards taking the food. The traveller remains alive. Refusing, or attacking a guard, starts a firefight with visible aiming wind-ups, physical bullets, cover and dodgeable captured aim. Retreat remains possible.

Choose to step back and watch, or simply wait with the feeding door in view from 8–38m. Twelve active seconds of observation reveals a guard carrying food through the slot to children in a locked keeper's room. Empty bowls and the guards' thin figures support the discovery. Inspect the room with F to recover a journal entry and optionally leave one food portion; this donation can happen only once. The room can also be understood after the guards are defeated. Children are protected story figures with no damage colliders. Decisions, payment, discovery, donation and robbery state save per world; guests keep their own costs and journals. Bridge, rails, barrier, approaches and room have physical collision. The existing expansion river stays closed. Network protocol 17 requires everyone to update together.

## Primitive combat and roadside stories

Fresh survivors begin with a sturdy stick and a hand stone, no firearm and no ammunition. Gather loose timber, stone and fiber; craft a stone chopping axe in Survival before using T to chop trees. Existing saves retain their old axe access and owned weapons. Firearms require both character level 2 and discovery of tier 2 materials, including when equipping an already-owned gun. Tier 1 enemies cannot drop firearms. Pistol and ammunition crafting now start at tier 2.

| Stage | Craftable weapons |
| --- | --- |
| T1, hand crafting | Sticks, stones, wooden clubs, stone spears, simple bows, baseball bats, arrows, stone chopping axe |
| T2 workshop | Woodland bow, claw hammer, iron rod, iron spear, pistol and woodland rifle |
| T3 workshop | Chainsaw, recurve bow, machete, scrap shotgun and industrial carbine |
| T4 workshop | Crossbow, sledgehammer, precision rifle |
| T5 workshop | Experimental rifle and existing endgame tuning |

Left click swings the equipped melee weapon or fires the equipped bow/gun. T switches to a crafted chopping axe; I selects weapons. Spears have 3.6m reach; most clubs/bats/rods 2.6m; heavy weapons hit harder and swing more slowly. Melee swings have a contact delay, stamina costs and physical obstruction checks. Chainsaw bursts also spend fuel, crafted from resin and salvage at workshop 2 after discovering T3 materials. Bows use crafted arrows with visible flight, gravity and physical segment collision. R reloads firearms; arrows and melee never consume firearm ammunition.

Melee, bow and gun proficiency are separate portable character skills. Successful hits grant 4 XP, ranged headshots grant 8 XP, and direct infected kills grant 18 XP in the relevant class. Misses and building strikes award none. Each proficiency level adds 1% damage to its own class. Melee proficiency also reduces stamina cost by 1% per level, capped at 25%. The existing gun skill is retained as gun proficiency; old gun levels are preserved. Death resets partial XP in all three classes, preserving earned levels. Defenses and companion kills do not train the weapon in your hands.

Twenty additional environmental encounters occupy deterministic, seed-dependent plots down the progression route. They stream near active players, reserve their plots from procedural trees/buildings, and avoid dungeon entrances. These are short interactive story scenes with props, a journal entry and a persistent choice, rather than twenty multi-stage questlines. Supplies and gifts resolve only once per world; in co-op, the acting player spends or receives the supplies. Protected narrative people have no combat damage collider.

1. The Last School Run: crashed school bus, backpacks, farmhouse teacher and cellar provisions.
2. One More Ring: an abandoned bell-ringer's promise.
3. Borrowed Medicine: a veterinary clinic needing clean dressings.
4. Till Death: wedding rings and an unanswered vow.
5. Baby Girl Doe: an empty crib and a forgotten chart.
6. Prepared For Everything: untouched bunker provisions and a lonely note.
7. Twenty-Three Places: a camp mess-hall register and twenty-two remembered names.
8. Sam Is Still Here: a boarded cupboard, food and a protected child waiting for further help.
9. The Ferryman's Queue: luggage, a dock and a blue signal cloth.
10. The Empty Bowl: a missing dog's food bowl.
11. Weather Report: a radio operator's final forecast.
12. A Table For Two: an untouched second cup.
13. The Red Scarf: a signpost left along an evacuation route.
14. The Night Shift: an unfinished ambulance repair and boxed parts.
15. Please Knock: a request for one last courtesy.
16. The Orchard Promise: a jar saved for a neighbour.
17. The Last Lesson: a teacher's address book.
18. Signal At The Crossing: a railway worker's vigil.
19. The Unsent Apology: a letter beside a dead telephone.
20. The Little Library: leave food for the next reader.

Random event: DISTRESS DRONE. After ten active world minutes, a survivor exploring T2+ has a 45% chance at each three-minute opportunity to encounter a fallen delivery drone about 35m away. Its blinking beacon and original looping alarm warn of danger. An unsilenced beacon can draw three waves of up to three infected, placed away from players, before its three-minute battery expires. Salvage it for timber, resin and bandages, or destroy the beacon and forfeit those supplies. Silencing stops further waves; already-arrived enemies remain. The host runs the event, the outcome is shared, the acting player receives supplies once, and event state/retry time persist. The next opportunity follows a seeded 12â€“18 minute recovery interval. No offline timer and no automatic starter-valley event.

Quick combat selection: 1 cycles owned melee weapons, 2 cycles bows/crossbows, and 3 cycles usable firearms. These shortcuts leave the chopping-axe mode; number keys still choose blueprints while building. An already-fired arrow retains its source weapon damage/proficiency if you switch weapons during flight. The journal's encounter list scrolls to keep all recovered entries accessible. The bunker occupant and adult camp counselor remain seated until their associated supplies/table are disturbed; companion and automated defenses ignore them while passive.

### Noise and sneaking
Hold **Ctrl** outside building mode to sneak at a slower pace. The HUD noise meter rises with footsteps, chopping and weapon use, and fades while quiet. Walking carries farther than sneaking; sprinting, chainsaws and gunfire are much louder. Repeated activity expands hearing range. Walls muffle sound; explosions and the distress beacon draw existing zombies toward their actual source. Sound does not create extra zombies by itself.

Sneaking is a separate portable skill. Moving within 24m of an unaware zombie on a similar floor earns 3 XP per second; standing still or sneaking without nearby threats earns none. Higher levels reduce sneak footstep noise (up to 55%) and initial visual detection distance. Sneaking also reduces peripheral detection, but touching-distance enemies can still see you, and already alerted enemies remain dangerous. Death clears current sneak XP without removing earned levels.

### Physical crouching and noisy ground
**Hold Ctrl to crouch.** Your capsule becomes 1.1m tall and your viewpoint lowers to 1m. Releasing Ctrl stands you up only if the full 1.75m body fits; leave a low ceiling before standing. Sprinting and jumping are disabled while crouched. Other players see your crouched stance.

Movement noise uses actual distance moved, with low crouching, mid walking and high sprinting levels. Surface multipliers: grass x1, woodland leaf litter x1.5, visible bushes x2.4, dirt trail x0.9, player-made path x0.85, road x1.2, raised/indoor floor x1.1. Bushes use the same streamed locations for their visuals and rustling. Floors above vegetation do not rustle the plants below. Leaf litter and bushes have crunchier footstep audio; muting audio does not disable zombie hearing.

### South Bank campaign and new infected
**F4 opens Your Journey**, also available from the journal, map and pause menu. A survivor field guide stands beside the southeast trailhead. Prepare primitive tools, shelter and bandages, then resin, workshop 2, a sleeping bag and woodland weapons. The open world remains explorable; the challenge consoles follow this sequence:

1. **The Foreman** at Ashen Lumber Mill (T2): existing slam, bile and log-stopping charge encounter; core tempering.
2. **The Wrecker** in a T3 highway recovery yard: committed ram, broad sweep, wreck obstacles and long recovery windows. Rewards a road helmet, alloy, ammunition/arrows, provisions and research.
3. **The Bellkeeper** at a T4 civic square: calls bounded reinforcements, marks a falling-debris target and sweeps nearby players. Rewards armored gloves, electronics, provisions and research.
4. **The Broodheart** in the T5 pumping-station dungeon: hatches worms, marks bile pools and releases a warned rupture ring. Rewards a cuirass, bioalloy and one legendary heart per character/world. A final worm hatch still needs clearing.

Use F at each console. The pumping station is entered as a party by the host, with a usable exit throughout. Boss health scales with nearby players when the encounter starts. Warnings shorten below half health; recovery grants 50% bonus damage. Retreating beyond the encounter for eight seconds allows a fresh attempt. Rewards and cleared sites save per world; personal claims are deduplicated. A late-arriving player can claim their own reward at an already-cleared console. The river remains closed for future expansions.

**Legendary ascension** is researched/crafted at workshop 5 under Weapons and armor. It consumes one legendary heart, 6 alloy, 6 electronics and 8 bioalloy. Equip a maximum-tier weapon: T5 gun, T4 crossbow or T4 sledgehammer. It raises actual weapon quality to +50%, with a legendary inventory/HUD name.

Regional models retain valley civilian clothes, woodland logger workwear, highway reflective gear, city office/security uniforms and contaminated details in the depths. Habitat populations add runners, heavy bruisers and armored infected (body damage resistance; headshots remain effective). Starter-valley habitat spawns remain basic drifters.

**Weeper:** sits sobbing with long claws. Its cry attracts nearby infected without waking the weeper. Leave it alone or deliberately provoke it with damage. It has substantial health, a visible committed claw rush, damaging bleeding slices and a recovery opening; walls stop its charge. Camp defenses and companions leave a passive weeper alone.

**Bloater:** slow, swollen silhouette with green sacs. Death leaves a 1.25-second green hatch warning, then 3–5 segmented worms. Worms are physical, killable targets, bite for small direct damage with a wind-up, and do not produce food/ammunition loot. They cannot recursively hatch more worms. Night bloaters also retain catalyst rewards. Hatch warnings and worms replicate to co-op clients.

## Gun handling and blade progression

Right click toggles aiming. Hold Shift while stationary and aimed to steady the sight, consuming stamina; the breath window starts at five seconds and grows with gun proficiency. Moving or running out of breath breaks the hold and requires recovery. Precision and experimental rifles use a scope overlay. Gunfire follows a repeatable recoil pattern, with bloom increasing during rapid fire and recovering between shots. Movement and jumping worsen accuracy; crouching helps. Hold left click for the carbine or powered saws.

Ammunition is separate: 9mm pistol rounds, 7.62mm rifle rounds (including precision rifles), 5.56mm carbine rounds, 12-gauge shells and experimental energy cells. Craft the matching recipe at the required tier/workshop. Magazines remain loaded when switching weapons. Older shared ammunition migrates once into the equipped gun's ammunition class, preserving its quantity. Shared storage and vehicle cargo preserve each type independently.

New craftable melee progression: T1 stone knife; T2 hunting knife and scrap sword; T3 forged sword; T4 alloy longsword; T5 Stormfire chain sword, requiring an owned chainsaw. Swords can cleave up to three nearby enemies with physical wall checks. Stormfire contact uses one fuel and one energy cell, with electrical stagger. Press Q outside building mode to reverse the sword and fire its rear flame exhaust: 20 stamina, eight fuel, four cells, 1.4-second burst, ten-second cooldown. Flame range is 5.5 metres, blocked by walls, with a short lingering burn. The sword and ammunition have inventory/crafting models.

## Infected hound packs

Zombie dogs roam in seeded packs: three in T2 woodland near roads, four along T3 highways, and five near T4+ city roads. They do not spawn in the T1 starting valley, indoors, or within 28 metres of an active player. Streaming and existing fauna/enemy caps bound their numbers; defeated dogs retain their stable saved IDs.

Seeing or being struck by a player alerts nearby members of the same pack. Dogs also investigate the noise system, while crouching and sneak proficiency reduce their visual detection range. They run, spread out around their target and use existing obstacle routing and physical collision. A crouch and growl precede a short committed lunge; packmates wait their turn before lunging, giving the player a chance to sidestep or interrupt. Dog bites deal damage but add a bleeding wound only when the player has no existing wounds. Infected meat is discarded. Dogs have visible teeth, damaged flanks, collars and animated jaws, with spatial barks and growls replicated from their shared attack state.

## The Waterline / current health and consequence rules (2026-10-18)

See LIVING-WORLD.md for the full current systems, timings, research branches and Noah/Lily outcomes. This update supersedes the older fatal-bite countdown descriptions above. Use Y for health/conditions/purification, F7 -> The Waterline for neighboring communities and the world record. Research now has a 48-point lifetime award cap and permanent advanced specializations. Existing learned blueprints are preserved; older completed fever outcomes are not rewritten. Wells supply untreated water rather than clean drinks. Co-op protocol 25 requires matching builds.

The unique rescue dog and personal relationships are documented in LIVING-WORLD.md. Use F beside the dog; F7 opens care/commands and the People reputation ledger. The dog's death is permanent within that world.

Settlement progression now includes three branching chapters per friendly community, visible development and finite reputation-gated markets. A fourth city faction, the Black Ledger, offers a reputation-selected assault/infiltration order against an early settlement, with a costly warning/defense alternative and actual camp retaliation. See LIVING-WORLD.md for deadlines, costs, consequences and controls.

The field atlas content expansion adds the places, activities, field requests, dungeon caches and roaming events documented in CONTENT-ATLAS.md. Site choices/loot persist in the world; the field guide is available through F7 and the journal. Multiplayer protocol 28.

## Tier 5 Soulbound hunts

Defeat the Broodheart, then return to the pump station and descend again. At the chamber console, open **Soulbound armory / T5 mega hunts**. The same management page is available from Inventory. Each summon costs eight night catalysts; the world cooldown is five active minutes. Retreat consumes the summon cost and resets the fight. Each living player within 65m at victory receives private, character-bound loot. Rewards persist across death and servers; duplicate requests do not reroll them.

| Boss | Mechanics | Guaranteed personal loot | Extra roll |
| --- | --- | --- | --- |
| Iron Mourner | 6,000 base HP; telegraphed bursts of moving scrap projectiles, cover blocks shots; targeted explosive canisters; five-second overheat window | One rolled Soulbound weapon + one weapon upgrade | 20% second weapon upgrade |
| Sump Colossus | 8,400 base HP; slow pursuit, rupture slam and short charge; three amber growths break under targeted hits, opening five-second exposure windows; broken shell stays weaker | One rolled Soulbound armor piece + three reinforcement plates | 25% rare armor ward |
| Brood Matriarch | 5,000 base HP; three destroyable nests; waves of 3 + 2 per surviving nest; clear a wave for twelve seconds of vulnerability; heavily resistant during waves | One rolled camp module + three community parts | 25% companion harness |

Health gains 40% per additional nearby player. The three bosses share the existing pumping chamber, with distinct procedural models and combat behavior. They do not add three new regions. Armor weakpoint state, enemy waves and projectile visuals replicate to co-op clients. Compatible builds use protocol 30.

### Weapon table

Each base type has equal weight: Trail rifle, Woodland rifle, Industrial carbine, Precision rifle, Experimental rifle, Scrap shotgun, Crossbow, Iron spear, Sledgehammer, Alloy longsword, Stormfire chain sword, Hunting knife. Each copy stores a unique ID, permanent stat rolls and its owner. Gun stats affect damage, magazine capacity, spread and reload time. Bow stats affect damage, arrow velocity, draw recovery and noise. Melee stats affect damage, stamina cost, attack recovery and noise. Existing proficiency, talents and workshop quality still apply. Switching preserves ammunition.

Weapon upgrades roll one of five types with equal weight, at 15–30% power: armor penetration, recoil reduction, faster bow draw, chance to save chainsaw fuel, or reduced melee stamina cost. One upgrade is active per character, applies only to its matching action, and can be changed in the armory. No research is granted by equipping loot.

### Armor table

Head, chest, hands and feet have equal weight. Each piece rolls a Bulwark, Mirestalker or Lifekeeper profile, varying armor/vitality, movement, stamina, footstep noise and recovery. Heavy pieces have movement/stamina/noise drawbacks. Rolled gear uses the real four equipment slots; bound armor cannot be equipped by another owner. Three reinforcement plates add three armor, up to three times per piece. A rare ward adds eight armor and five percent recovery once per piece. These do not stop bleeding or provide instant healing.

### Community table

Each module type has equal weight and rolls +20–45% effectiveness with 1–3 upkeep per five active minutes. Three slots per world camp; equal types cannot stack. Install/remove/tune within 20m of camp. Only the owner can remove their module. Camp stores pay upkeep when the camp is occupied; unfunded modules stop, and modules provide no bonus to unobserved camps. Destroyed required stations also stop their modules.

- Waterworks: faster water-station production; needs a built water station and an already crafted researched charcoal/membrane filter. Food upkeep.
- Workshop heart: faster food-station production; requires workshop 5. Crafting is currently instantaneous, so this does not invent a crafting-time bonus. Food upkeep.
- Watchtower relay: stronger camp guard damage/range and additional raid preparation seconds; requires a built alarm or turret. Food upkeep.
- Greenhouse regulator: faster crop production; requires a built farm. Water upkeep.
- Expedition board: faster resident timber/supply work; requires a recruited resident. It improves the existing jobs rather than adding a separate expedition-mission system. Food upkeep.

Three community parts tune a module by +5 percentage points, capped at 60%, while increasing upkeep by one. A companion harness rolls +15–30% damage and +10–20% attack speed for the owning character's following human companion; one active harness. Existing camp shelter, food, injury, ammunition and research requirements remain.

Automated checks cover actual projectile damage and wall occlusion, nest-dependent waves, shell breaks, specialist rewards and duplicate protection, equipment stats, save roundtrips, research/station/location gates, module upkeep and production. Solo balance and the strongest roll combinations still need playtesting.

## Ten additional infected types

These replace up to one fifth of eligible habitat, night and ordinary dungeon population slots when a matching local habitat exists; existing streaming, heatmaps and enemy caps remain. Story actors and authored field-site guard types are preserved. Tier 1 receives none of the new types. Enemy selection is deterministic from its saved ID and tier/night category. F4 > Infected / ten new field threats lists their counters.

| Enemy | Tier | Behavior / counter |
| --- | --- | --- |
| Mossback | 2–3 | Moss and branches disguise a crouched woodland ambusher; close approach, noise or damage wakes it. |
| Bell-throat | 2+ | A 1.8-second raised-head warning precedes a scream that alerts existing zombies within 80m, reduced by walls. Hit it to interrupt. |
| Bilecaster | 2+ | Marks your current position before leaving a six-second acid patch. Move out of the green marker or interrupt the cast. |
| Gravecrawler | 2+ | Low ambusher with an actual 1.1m collider; short-range activation and a crouched crawling silhouette. Look down. |
| Roadbreaker | 3+ | Reinforced roadworker charges toward a fixed point after a warning. Sidestep it; collisions/end of rush cause long recovery. |
| Cinder Host | 3+ | On lethal damage, ignites for 1.8 seconds before an explosion and five-second burning patch. Repeated hits do not reset/duplicate the death. |
| Riot Husk | 4+ | Shield resists frontal body damage. Flank or aim above it; penetration upgrades help. |
| Wirebound | 3+ | A purple target marker precedes an eight-damage lash draining 35 stamina. It does not root or stun-lock the player. |
| Blooddrinker | 4+ | Actual successful melee damage restores 30 health, capped at its maximum. Dodged/shielded strikes do not heal it. |
| Pale Hunter | 4+ | Appears in night encounters and Tier 5 depths. Flanks before a committed, telegraphed claw rush. |

All have distinct additions to the existing adult infected rigs, server-authoritative combat, attack warning/recovery states and matching co-op replicas. New specialist kills add one zone-appropriate crafting material; the four high-health elites/brutes add two and bonus career XP. Pale Hunters add two night catalysts. Defeated IDs prevent duplicate rewards. Fire/acid patches share the existing bounded hazard system, including player, dog and survivor damage; colors replicate to clients.

Validation: --threat-check exercises seeding, all ten models and mechanics, interruption, hearing, crawler collider, shield facing, life-steal, finite death/fire and saved rewards. --threat-preview captures two five-enemy galleries and ten individual models in the engine. Co-op checks exercise all ten replicas, a guest-interrupted howl, shared hearing and Cinder self-removal/fire replication. Multiplayer protocol is 31. Balance still needs live playtesting.

## Habitat-based enemy placement

The ten added infected now require both a suitable tier and matching surroundings at their spawn point. The in-game F4 infected guide lists each habitat. They can pursue players beyond that habitat after spawning.

| Enemy | Spawn surroundings |
| --- | --- |
| Mossback | Dense Tier 2 woodland more than 45m from the main route |
| Bell-throat | Woods, roads, abandoned city housing and ruins |
| Bilecaster | Clinics, hospitals, drains/sewers, pump works and contaminated sites |
| Gravecrawler | Church/grave surroundings, discovered woodland grave sites and ruins |
| Roadbreaker | Tier 3+ main roads, garages and industrial works |
| Cinder Host | Tier 3+ garages, rail/utility sites, factories and mine interiors |
| Riot Husk | Tier 4+ evacuation/security checkpoints, civic square, hospital security and tower interiors |
| Wirebound | Tier 3+ utility/industrial sites, checkpoints and sewers |
| Blooddrinker | Tier 4+ medical sites and abandoned city housing |
| Pale Hunter | Suitable Tier 4+ city/ruin habitats at night, or suitable Tier 5 underground habitats |

Surface context uses actual route distance, woodland density, building clusters, authored content-site themes (within 90m), dungeon entrances (75m), checkpoints/civic security (85m), church surroundings (80m), and the discovered woodland grave (65m). These are area affinities, not extra spawn generators. Existing authored story encounters and reserved lots retain their protections. Dungeon pools follow their identity: mines favor industrial/ruin types; clinics and hospitals favor medical types; drains and sewers favor sewer types; towers favor residential/security types.

A deterministic kind tag travels with the spawn ID, so clients and rewards retain the server's selected type even when the enemy moves elsewhere. Old defeated slot IDs still suppress new tagged spawns; kills save both the kind-tagged ID and original slot ID, preventing repeat rewards if local conditions later change. Brood children retain unique IDs. Night spawns use their actual landing-point tier and skip Tier 1/sanctuary points. Population and streaming limits are unchanged. Protocol 32.


## Weapon research and removable parts

The current build has 34 weapons, 179 recipes and 193 item models. Research branch filters show actual predecessors for weapons, armor, building upgrades and water purification. The 48-point lifetime budget and advanced weapon specializations remain. Armor has a separate chain, so Ballistics and Ranger characters can research protection. See WEAPON-WORKSHOP.md for the six additional weapons, twelve compatible parts and fitting instructions. Spare parts appear in inventory; fitting and removal return replaced parts and conserve ammunition. Existing quality calibration remains separate from fitted parts.
