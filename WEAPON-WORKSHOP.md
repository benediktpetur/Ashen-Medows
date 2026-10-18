# Research and weapon workshop

K opens the four research tabs. Branch filters reduce clutter; connections show actual blueprint prerequisites. Research spends points once; crafting consumes materials separately. The character keeps blueprints, fitted parts and spare parts through death and server travel.

The lifetime budget remains 48 RP. Tier 1–2 foundations do not commit a specialization. Your first advanced weapon blueprint commits Ballistics, Ranger or Brawler. Advanced armor has its own prerequisite chains and uses the shared point budget, so gun users can still research armor. Existing unlocks and commitments are retained. Armor upgrades follow the previous tier for the same slot. Building upgrades follow the previous matching structure/function; membrane purification and sealed water containers build on the charcoal filter.

## Weapon paths

- **Ballistics:** Trail pistol → salvage revolver, compact SMG, or scrap shotgun → pump shotgun. Woodland rifle → industrial carbine → precision → experimental rifle; the battle rifle branches from the woodland rifle. Guns still require character level 2 and tier 2 exploration.
- **Ranger:** Simple bow → woodland bow → recurve → crossbow or compound bow.
- **Brawler:** Stick → club → hammer/rod → chainsaw or heavier impact tools. Stone → stone spear → iron spear → polehammer. Stone knife → hunting knife → scrap sword → forged sword → alloy longsword. Chainsaw → Stormfire chain sword.

Research predecessors need to be learned, not consumed. Only the existing chain-sword recipe additionally requires an owned chainsaw. Each advanced recipe still needs its workshop, zone-exclusive materials and crafting skill. Sticks and hand stones are free foundational blueprints.

| New weapon | Tier | Role / ammunition |
|---|---|---|
| 9mm salvage revolver | 2 | Six rounds; heavier hits and slower follow-up than the pistol |
| 9mm compact SMG | 3 | Automatic, 24 rounds; low damage per round and fast fire |
| Polehammer | 3 | 3.3m reach, heavy impacts, deliberate 1.1s cycle |
| Pump-action shotgun | 4 | Six shells; six pellets per shot, 0.85s cycle |
| Battle rifle | 4 | Semi-automatic 7.62mm, 20 rounds, 0.45s cycle |
| Compound bow | 4 | High-energy arrows, 60m/s velocity, longer draw cycle |

## Fitting parts

Inventory → **Weapon Parts / Workshop**, or use the fitting button in research. Select an owned weapon. Craft a compatible part through research, then fit it at its required workshop tier. One part per named slot; fitting a replacement returns the previous part to the pack. Removing magazine parts returns excess loaded ammunition to the correct reserve. Neither fitting nor removing creates ammunition. Soulbound rolls and upgrades combine with fitted-part effects; parts are attached to the weapon type rather than an individual Soulbound roll.

| Part | Tier | Benefit and tradeoff |
|---|---|---|
| Cord grip | 1 | −15% melee stamina; ordinary melee weapons |
| Weighted head | 2 | +20% impact damage; +20% cycle time and stamina |
| Honed steel edge | 2 | +15% damage; steel blades and spearheads only |
| Fitted stock | 2 | −15% gun spread; +8% reload time |
| High-tension string | 2 | +20% arrow speed; +15% draw cycle |
| Suppressor | 3 | −50% gunshot noise; +10% spread |
| Muzzle brake | 3 | −30% recoil; +25% gunshot noise; replaces suppressor |
| Field optic | 3 | Magnified aim, −20% spread; +10% reload time |
| Extended magazine | 3 | +50% capacity; +20% reload time |
| Balanced limbs | 3 | −15% bow cycle time; −8% damage |
| Hardened chain | 4 | +18% saw damage; +20% stamina |
| Stormfire cooling jacket | 5 | −15% cutting cycle time; +15% stamina |

Compatibility is enforced in logic and in the host's multiplayer action handler. Revolvers do not accept suppressors or magazines. Shotguns do not accept either, and take stocks instead. Bow and melee parts cannot fit guns. Existing quality-tuning recipes remain as workshop calibration processes; quality no longer silently adds a suppressor model or reduces shot noise.

The score remains included. Multiplayer protocol is 33; all players need the same build.

Validation includes dependency cycles/reachability, zone material gates, all six physical models, part compatibility, stat effects, ammunition conservation, duplicate actions, old-save resizing, serialized parts, inventory entries and two-process research/crafting/fitting/removal with isolated player costs.
