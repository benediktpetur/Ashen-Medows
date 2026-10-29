# Model work still needed — 2026-10-29

This list separates genuinely absent conversions, rejected drafts, reusable existing models and procedural art that could benefit from dedicated assets. A model being present does not mean it is approved, rigged, optimized or placed in the world.

- Conversion inventory: 899 completed textured outputs from 918 inputs; 19 failed inputs, including variants.
- Current static/environment registry: 168 keys. Many are inventory/crafting previews; registration does not imply world placement.
- Catalogue entries without an exact textured draft: 14. Of those, 6 already have explicit working game replacements.
- Default runtime preview lookups without a registered imported model: 96 unique lookup slots across 254 item definitions. Procedural visuals remain functional.
- Importer deferrals: 97; 66 rejected conversions, the rest missing a matching new conversion. Deferrals overlap the runtime list; do not add these counts together.

Runtime coverage is exported by the game and uses the same first matching kind/index/tier lookup as ItemModel, so aliases are not counted as separate missing meshes. Authored relic appearances, first-person views, procedural world structures and animation rigs are separate paths.

## Best next art tasks

| Priority | Asset group | What remains |
|---|---|---|
| P1 | Grass / tall dry grass / fern / low shrub LOD set | Procedural ground cover is now present; dedicated optimized meshes would improve silhouettes and draw-distance transitions. |
| P1 | Enterable house modular kit | Open door/window wall sections, interior faces, roof, foundation, porch and corners. Fused house conversions cannot replace usable rooms. |
| P1 | Garden fence and gate kit | Straight, corner, sloped, damaged sections and working gate apertures. Review existing build drafts before creating more. |
| P1 | Roadside and pavement kit | Curbs, dropped curbs, drains, manholes, road signs and guardrail endings. Current roads are terrain materials, not missing terrain models. |
| P1 | Industrial yard set | Pallets, cable reels, crates, bins, drums and scrap piles. Existing catalogue drafts need selection/scale review before any new batch. |
| P2 | Fallen logs / stumps / branches / leaf-litter set | Review completed Nature input conversions; optimize and add LODs instead of generating duplicate oak trees. |
| P2 | City frontage kit | Shopfronts, doors, broken/boarded windows, awnings, vents, pipes and rooftop units. Current facade trim is procedural. |
| P2 | Suburban garden and household dressing | Planters, mailbox, bins, bicycle, clothesline and porch light. Use approved current furniture; a full duplicate furniture batch is unnecessary. |
| P2 | Street furniture and debris | Benches, bus stop, utility cabinets, litter, broken masonry and damaged traffic barriers. Review existing props first. |
| P2 | Wildlife and dog animation-ready assets | Textured drafts exist; anatomy checks, rigging, LODs and animation are the remaining work. Static GLBs are not animation-ready replacements. |
| P3 | Variant survivor / story NPC assets | Use current survivor/actor rigs as the baseline; different clothing and identity variants need rig-compatible preparation. |
| P3 | Dungeon and landmark modular sets | Doors with real openings, stairs, pipe junctions, sewer/medical/industrial dressing. Preserve existing colliders and authored story rooms. |

## No exact converted catalogue model

These entries have no exact matching textured GLB in the audited folders. Review the reuse note before generating anything.

| Asset | Key | Next action |
|---|---|---|
| Stone | `catalog-material/1` | Review world-rock reuse for stone; no new generation required if that adaptation is suitable. |
| Antibiotic field procedure | `catalog-survival-tech/3/3` | Create a matching model or explicitly map a suitable existing draft. |
| Field axe / T | `catalog-axe` | Create a matching model or explicitly map a suitable existing draft. |
| Untanned hide | `catalog-hunting-material/1` | Create a matching model or explicitly map a suitable existing draft. |
| Leather | `catalog-hunting-material/5` | Create a matching model or explicitly map a suitable existing draft. |
| Iron chopping axe | `catalog-field-tool/0/2` | Create a matching model or explicitly map a suitable existing draft. |
| Iron skinning knife | `catalog-field-tool/3/3` | Create a matching model or explicitly map a suitable existing draft. |
| Abandoned provision chest | `salvage-0` | Create a matching model or explicitly map a suitable existing draft. |

### Previously labelled missing, already covered

- Bone skinning knife: `catalog-field-tool/3/2` is registered through the corresponding model lookup; the source may be a selected-collection alternative outside the catalogue audit match.
- Abandoned car: `wreck-car` is registered through the corresponding model lookup; the source may be a selected-collection alternative outside the catalogue audit match.
- Broadleaf tree: `world-oak-c` is registered through the corresponding model lookup; the source may be a selected-collection alternative outside the catalogue audit match.
- Pine tree: `world-pine` is registered through the corresponding model lookup; the source may be a selected-collection alternative outside the catalogue audit match.
- Forest canopy tree: `world-oak-c` is registered through the corresponding model lookup; the source may be a selected-collection alternative outside the catalogue audit match.
- Harvest rock: `world-rock` is registered through the corresponding model lookup; the source may be a selected-collection alternative outside the catalogue audit match.

## Every unfilled default runtime model slot

These are imported-model gaps, not invisible/missing gameplay objects. Rejected models need replacement or cleanup; existing unregistered drafts need review, not an automatic generation job.

| Asset | Key | Next action | Detail |
|---|---|---|
| 9mm pistol rounds | `catalog-ammo` | Replace rejected conversion | Rendered result is a pistol rather than ammunition |
| 5.56mm carbine rounds | `catalog-ammo/2` | Replace rejected conversion | Rendered result is a rifle rather than ammunition |
| Field chest plate | `catalog-armor/1/1` | Replace rejected conversion | Torso includes unwanted detached boots |
| Trail hood | `catalog-armor/1/11` | Replace rejected conversion | Hood includes unwanted detached boots |
| Woodland helmet | `catalog-armor/2/11` | Replace rejected conversion | Helmet includes unwanted detached boots |
| Woodland chest plate | `catalog-armor/2/2` | Replace rejected conversion | Torso includes unwanted detached boots |
| T3 helmet | `catalog-armor/3/11` | Replace rejected conversion | Helmet includes unwanted detached boots and debris |
| Industrial chest plate | `catalog-armor/3/3` | Replace rejected conversion | Torso includes unwanted detached boots |
| T4 helmet | `catalog-armor/4/11` | Replace rejected conversion | Helmet includes unwanted detached boots |
| Advanced chest plate | `catalog-armor/4/4` | Replace rejected conversion | Torso includes unwanted detached boots |
| Experimental helmet | `catalog-armor/5/11` | Replace rejected conversion | Helmet includes unwanted detached boots |
| Experimental chest plate | `catalog-armor/5/5` | Replace rejected conversion | Torso includes unwanted detached boots |
| 12 flint arrows | `catalog-arrow/1/12` | Replace rejected conversion | Arrow bundle is a malformed debris-covered bundle |
| Barrel lapping / +10% quality | `catalog-attachment/2/10` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Action bedding / +15% quality | `catalog-attachment/3/15` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Precision calibration / +20% quality | `catalog-attachment/4/20` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Field axe / T | `catalog-axe` | Create or reuse model | No matching textured draft found in the audited catalogue |
| Bandage roll | `catalog-bandage` | Replace rejected conversion | Rendered bandage resembles a wooden cylinder rather than cloth |
| Workshop upgrade | `catalog-bench/2` | Replace rejected conversion | Kitchen counter rather than upgraded workbench |
| Industrial upgrade | `catalog-bench/3` | Replace rejected conversion | Kitchen counter rather than upgraded workbench |
| Advanced upgrade | `catalog-bench/4` | Replace rejected conversion | Kitchen counter rather than upgraded workbench |
| Experimental upgrade | `catalog-bench/5` | Replace rejected conversion | Closed cabinet rather than upgraded workbench |
| Stone-braced wall | `catalog-build/10` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Reinforced concrete window wall | `catalog-build/100` | Replace rejected conversion | Window opening is filled |
| Military door wall | `catalog-build/102` | Replace rejected conversion | Door opening is filled |
| Military window wall | `catalog-build/103` | Replace rejected conversion | Window opening is filled |
| Stone-braced stairs | `catalog-build/14` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Wall | `catalog-build/2` | Replace rejected conversion | Wall reconstructed as a solid deep box |
| Scrap steel pillar | `catalog-build/20` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Pillar | `catalog-build/4` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Storage chest | `catalog-build/41` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Stone-braced window wall | `catalog-build/42` | Replace rejected conversion | Window aperture is filled |
| Generator | `catalog-build/48` | Replace rejected conversion | Generator reconstructed as a timber cabinet |
| Fence section | `catalog-build/56` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Snare trap | `catalog-build/58` | Replace rejected conversion | Rope snare reconstructed as a large cage |
| Heavy defensive turret | `catalog-build/66` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Field kitchen | `catalog-build/75` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Medical station | `catalog-build/76` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Advanced medical station | `catalog-build/79` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Efficient generator | `catalog-build/80` | Replace rejected conversion | Generator reconstructed as a timber cabinet |
| Hydroponic farm | `catalog-build/82` | Replace rejected conversion | Hydroponic rack has substantial floating reconstruction debris |
| Keepsake shelf | `catalog-build/87` | Replace rejected conversion | Display shelf reconstructed as a closed cabinet |
| Rough timber door wall | `catalog-build/91` | Replace rejected conversion | Door opening is filled |
| Stone-braced door wall | `catalog-build/94` | Replace rejected conversion | Door opening is filled |
| Scrap steel door wall | `catalog-build/96` | Replace rejected conversion | Door opening is filled |
| Reinforced concrete door wall | `catalog-build/99` | Replace rejected conversion | Door opening is filled |
| Night catalyst | `catalog-catalyst` | Replace rejected conversion | Generic wooden crate does not represent the catalyst |
| Flint arrow | `catalog-field-arrow/1` | Replace rejected conversion | Bent and malformed arrow shaft |
| Crude Molotov | `catalog-field-throw/0` | Replace rejected conversion | Molotov lacks readable wick and resembles a closed supply bottle |
| Thickened Molotov | `catalog-field-throw/1` | Replace rejected conversion | Molotov reconstructed as a solid wooden cylinder |
| Iron chopping axe | `catalog-field-tool/0/2` | Create or reuse model | No matching textured draft found in the audited catalogue |
| Iron skinning knife | `catalog-field-tool/3/3` | Create or reuse model | No matching textured draft found in the audited catalogue |
| Trail provisions | `catalog-food` | Replace rejected conversion | Generic wood crate without a readable provisions design |
| Chainsaw fuel / 40 cuts | `catalog-fuel/3/40` | Replace rejected conversion | Chainsaw rather than its fuel supply |
| Fur-lined woodland vest | `catalog-hunting-gear/vest` | Replace rejected conversion | Vest includes detached boots and malformed opening |
| Untanned hide | `catalog-hunting-material/1` | Create or reuse model | No matching textured draft found in the audited catalogue |
| Bone | `catalog-hunting-material/3` | Replace rejected conversion | Bone reconstructed as a wooden cylinder |
| Sinew | `catalog-hunting-material/4` | Replace rejected conversion | Sinew reconstructed as a wooden barrel |
| Leather | `catalog-hunting-material/5` | Create or reuse model | No matching textured draft found in the audited catalogue |
| Legendary heart | `catalog-legendary` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Legendary ascension | `catalog-legendary-upgrade` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Timber | `catalog-material/0` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Stone | `catalog-material/1` | Review reuse candidate | The integrated world-rock model is available for a stone inventory/pickup adaptation |
| Fiber | `catalog-material/2` | Replace rejected conversion | Fiber reconstructed as a solid stump-like cylinder |
| Alloy scrap | `catalog-material/4` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Electronics | `catalog-material/5` | Replace rejected conversion | Electronics reconstructed as a wooden crate |
| Bioalloy | `catalog-material/6` | Replace rejected conversion | Bioalloy reconstructed as a wooden stump |
| Foreman weapon tempering | `catalog-mill-temper` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Foreman core | `catalog-millcore` | Replace rejected conversion | Generic barrel does not read as an industrial core |
| Camp comfort kit | `catalog-survival-tech/3/2` | Replace rejected conversion | Comfort kit reconstructed as an unrelated panel and stakes |
| Antibiotic field procedure | `catalog-survival-tech/3/3` | Create or reuse model | No matching textured draft found in the audited catalogue |
| Weighted striking head | `catalog-weapon-mod/2/1` | Replace rejected conversion | Weighted striking head reconstructed as a wooden barrel |
| Fitted shoulder stock | `catalog-weapon-mod/2/7` | Replace rejected conversion | Shoulder stock reconstructed as torso armour |
| High-tension bowstring | `catalog-weapon-mod/2/8` | Replace rejected conversion | Bowstring reconstructed as a complete bow |
| Stormfire cooling jacket | `catalog-weapon-mod/5/11` | Replace rejected conversion | Cooling jacket reconstructed as a human jacket |
| Trail pistol | `catalog-weapon/0` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Trail rifle | `catalog-weapon/1` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Claw hammer | `catalog-weapon/14` | Replace rejected conversion | Claw hammer lacks the correct hammer silhouette |
| Iron rod | `catalog-weapon/15` | Replace rejected conversion | Iron rod reconstructed as a wooden staff |
| Chainsaw | `catalog-weapon/17` | Replace rejected conversion | Chainsaw reconstructed as a firearm |
| Recurve bow | `catalog-weapon/18` | Replace rejected conversion | Recurve bow reconstructed as a firearm |
| Crossbow | `catalog-weapon/19` | Replace rejected conversion | Crossbow reconstructed as a firearm |
| Woodland rifle | `catalog-weapon/2` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Machete | `catalog-weapon/20` | Replace rejected conversion | Machete reconstructed as a wooden staff |
| Sledgehammer | `catalog-weapon/21` | Replace rejected conversion | Sledgehammer lacks its hammer head |
| Hunting knife | `catalog-weapon/23` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Forged sword | `catalog-weapon/25` | Replace rejected conversion | Forged sword has an excessively long staff-like handle |
| Alloy longsword | `catalog-weapon/26` | Replace rejected conversion | Longsword has an excessively long staff-like handle |
| Stormfire chain sword | `catalog-weapon/27` | Replace rejected conversion | Chain sword lacks the defining chain blade |
| Industrial carbine | `catalog-weapon/3` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Compound bow | `catalog-weapon/32` | Replace rejected conversion | Compound bow reconstructed as a blade |
| Polehammer | `catalog-weapon/33` | Replace rejected conversion | Polehammer reconstructed as a gun stock |
| Salvaged flamethrower | `catalog-weapon/34` | Replace rejected conversion | Flamethrower lacks its fuel system and resembles a compact firearm |
| Precision rifle | `catalog-weapon/4` | Review existing older conversion | Older textured draft exists; do not regenerate before reviewing it |
| Woodland bow | `catalog-weapon/6` | Replace rejected conversion | Bow reconstructed as a rifle |
| Hand stone | `catalog-weapon/9` | Replace rejected conversion | Hand stone reconstructed as a gun stock |

## All rejected static conversions

The files exist but were rejected after rendering. Existing procedural art remains active. A new conversion is only one option; cleanup or a reviewed alternative may be sufficient.

| Asset | Key | Rejection |
|---|---|---|
| 9mm pistol rounds | `catalog-ammo` | Rendered result is a pistol rather than ammunition |
| 5.56mm carbine rounds | `catalog-ammo/2` | Rendered result is a rifle rather than ammunition |
| Field chest plate | `catalog-armor/1/1` | Torso includes unwanted detached boots |
| Trail hood | `catalog-armor/1/11` | Hood includes unwanted detached boots |
| Woodland helmet | `catalog-armor/2/11` | Helmet includes unwanted detached boots |
| Woodland chest plate | `catalog-armor/2/2` | Torso includes unwanted detached boots |
| T3 helmet | `catalog-armor/3/11` | Helmet includes unwanted detached boots and debris |
| Industrial chest plate | `catalog-armor/3/3` | Torso includes unwanted detached boots |
| T4 helmet | `catalog-armor/4/11` | Helmet includes unwanted detached boots |
| Advanced chest plate | `catalog-armor/4/4` | Torso includes unwanted detached boots |
| Experimental helmet | `catalog-armor/5/11` | Helmet includes unwanted detached boots |
| Experimental chest plate | `catalog-armor/5/5` | Torso includes unwanted detached boots |
| 12 flint arrows | `catalog-arrow/1/12` | Arrow bundle is a malformed debris-covered bundle |
| Bandage roll | `catalog-bandage` | Rendered bandage resembles a wooden cylinder rather than cloth |
| Workshop upgrade | `catalog-bench/2` | Kitchen counter rather than upgraded workbench |
| Industrial upgrade | `catalog-bench/3` | Kitchen counter rather than upgraded workbench |
| Advanced upgrade | `catalog-bench/4` | Kitchen counter rather than upgraded workbench |
| Experimental upgrade | `catalog-bench/5` | Closed cabinet rather than upgraded workbench |
| Reinforced concrete window wall | `catalog-build/100` | Window opening is filled |
| Military door wall | `catalog-build/102` | Door opening is filled |
| Military window wall | `catalog-build/103` | Window opening is filled |
| Wall | `catalog-build/2` | Wall reconstructed as a solid deep box |
| Stone-braced window wall | `catalog-build/42` | Window aperture is filled |
| Generator | `catalog-build/48` | Generator reconstructed as a timber cabinet |
| Snare trap | `catalog-build/58` | Rope snare reconstructed as a large cage |
| Efficient generator | `catalog-build/80` | Generator reconstructed as a timber cabinet |
| Hydroponic farm | `catalog-build/82` | Hydroponic rack has substantial floating reconstruction debris |
| Keepsake shelf | `catalog-build/87` | Display shelf reconstructed as a closed cabinet |
| Rough timber door wall | `catalog-build/91` | Door opening is filled |
| Stone-braced door wall | `catalog-build/94` | Door opening is filled |
| Scrap steel door wall | `catalog-build/96` | Door opening is filled |
| Reinforced concrete door wall | `catalog-build/99` | Door opening is filled |
| Night catalyst | `catalog-catalyst` | Generic wooden crate does not represent the catalyst |
| Flint arrow | `catalog-field-arrow/1` | Bent and malformed arrow shaft |
| Crude Molotov | `catalog-field-throw/0` | Molotov lacks readable wick and resembles a closed supply bottle |
| Thickened Molotov | `catalog-field-throw/1` | Molotov reconstructed as a solid wooden cylinder |
| Trail provisions | `catalog-food` | Generic wood crate without a readable provisions design |
| Chainsaw fuel / 40 cuts | `catalog-fuel/3/40` | Chainsaw rather than its fuel supply |
| Rabbit-fur hood | `catalog-hunting-gear/hood` | Hood includes detached boots |
| Fur-lined woodland vest | `catalog-hunting-gear/vest` | Vest includes detached boots and malformed opening |
| Bone | `catalog-hunting-material/3` | Bone reconstructed as a wooden cylinder |
| Sinew | `catalog-hunting-material/4` | Sinew reconstructed as a wooden barrel |
| Fiber | `catalog-material/2` | Fiber reconstructed as a solid stump-like cylinder |
| Electronics | `catalog-material/5` | Electronics reconstructed as a wooden crate |
| Bioalloy | `catalog-material/6` | Bioalloy reconstructed as a wooden stump |
| Foreman core | `catalog-millcore` | Generic barrel does not read as an industrial core |
| Camp comfort kit | `catalog-survival-tech/3/2` | Comfort kit reconstructed as an unrelated panel and stakes |
| Weighted striking head | `catalog-weapon-mod/2/1` | Weighted striking head reconstructed as a wooden barrel |
| Fitted shoulder stock | `catalog-weapon-mod/2/7` | Shoulder stock reconstructed as torso armour |
| High-tension bowstring | `catalog-weapon-mod/2/8` | Bowstring reconstructed as a complete bow |
| Stormfire cooling jacket | `catalog-weapon-mod/5/11` | Cooling jacket reconstructed as a human jacket |
| Claw hammer | `catalog-weapon/14` | Claw hammer lacks the correct hammer silhouette |
| Iron rod | `catalog-weapon/15` | Iron rod reconstructed as a wooden staff |
| Chainsaw | `catalog-weapon/17` | Chainsaw reconstructed as a firearm |
| Recurve bow | `catalog-weapon/18` | Recurve bow reconstructed as a firearm |
| Crossbow | `catalog-weapon/19` | Crossbow reconstructed as a firearm |
| Machete | `catalog-weapon/20` | Machete reconstructed as a wooden staff |
| Sledgehammer | `catalog-weapon/21` | Sledgehammer lacks its hammer head |
| Forged sword | `catalog-weapon/25` | Forged sword has an excessively long staff-like handle |
| Alloy longsword | `catalog-weapon/26` | Longsword has an excessively long staff-like handle |
| Stormfire chain sword | `catalog-weapon/27` | Chain sword lacks the defining chain blade |
| Compound bow | `catalog-weapon/32` | Compound bow reconstructed as a blade |
| Polehammer | `catalog-weapon/33` | Polehammer reconstructed as a gun stock |
| Salvaged flamethrower | `catalog-weapon/34` | Flamethrower lacks its fuel system and resembles a compact firearm |
| Woodland bow | `catalog-weapon/6` | Bow reconstructed as a rifle |
| Hand stone | `catalog-weapon/9` | Hand stone reconstructed as a gun stock |

## Existing textured drafts to review before generating more

These catalogue entries have a textured draft but no matching registered static key or recognized actor key. They may need cleanup, rigging, a modular remake or integration. This is a review queue, not a claim that every file is usable or every world object is absent. Rejected conversions are listed separately above.

| Category | Asset | Key | Draft files |
|---|---|---|---|
| Bosses | Brood nest | `brood-nest` | 2 |
| Bosses | The Bellkeeper | `campaign-1` | 2 |
| Bosses | The Brood Matriarch | `mega-1` | 2 |
| Bosses | The Broodheart | `campaign-2` | 2 |
| Bosses | The Foreman | `foreman` | 2 |
| Bosses | The Iron Mourner | `mega-2` | 2 |
| Bosses | The Sump Colossus | `mega-0` | 2 |
| Bosses | The Wrecker | `campaign-0` | 2 |
| Buildings | Advanced medical station | `catalog-build/79` | 2 |
| Buildings | Fence section | `catalog-build/56` | 2 |
| Buildings | Field kitchen | `catalog-build/75` | 2 |
| Buildings | Heavy defensive turret | `catalog-build/66` | 2 |
| Buildings | Medical station | `catalog-build/76` | 2 |
| Buildings | Pillar | `catalog-build/4` | 2 |
| Buildings | Scrap steel pillar | `catalog-build/20` | 2 |
| Buildings | Stone-braced stairs | `catalog-build/14` | 2 |
| Buildings | Stone-braced wall | `catalog-build/10` | 2 |
| Buildings | Storage chest | `catalog-build/41` | 2 |
| Enemies | City security | `zombie-4` | 2 |
| Enemies | worm | `special-worm` | 4 |
| Houses | Residential house variant 1 | `house-0` | 2 |
| Houses | Residential house variant 10 | `house-9` | 2 |
| Houses | Residential house variant 11 | `house-10` | 2 |
| Houses | Residential house variant 12 | `house-11` | 2 |
| Houses | Residential house variant 13 | `house-12` | 2 |
| Houses | Residential house variant 14 | `house-13` | 2 |
| Houses | Residential house variant 15 | `house-14` | 2 |
| Houses | Residential house variant 16 | `house-15` | 2 |
| Houses | Residential house variant 17 | `house-16` | 2 |
| Houses | Residential house variant 18 | `house-17` | 2 |
| Houses | Residential house variant 19 | `house-18` | 2 |
| Houses | Residential house variant 2 | `house-1` | 2 |
| Houses | Residential house variant 20 | `house-19` | 2 |
| Houses | Residential house variant 21 | `house-20` | 2 |
| Houses | Residential house variant 22 | `house-21` | 2 |
| Houses | Residential house variant 23 | `house-22` | 2 |
| Houses | Residential house variant 24 | `house-23` | 2 |
| Houses | Residential house variant 3 | `house-2` | 2 |
| Houses | Residential house variant 4 | `house-3` | 2 |
| Houses | Residential house variant 5 | `house-4` | 2 |
| Houses | Residential house variant 6 | `house-5` | 2 |
| Houses | Residential house variant 7 | `house-6` | 2 |
| Houses | Residential house variant 8 | `house-7` | 2 |
| Houses | Residential house variant 9 | `house-8` | 2 |
| Items | Action bedding / +15% quality | `catalog-attachment/3/15` | 2 |
| Items | Alloy scrap | `catalog-material/4` | 2 |
| Items | Barrel lapping / +10% quality | `catalog-attachment/2/10` | 2 |
| Items | Foreman weapon tempering | `catalog-mill-temper` | 2 |
| Items | Legendary ascension | `catalog-legendary-upgrade` | 2 |
| Items | Legendary heart | `catalog-legendary` | 2 |
| Items | Precision calibration / +20% quality | `catalog-attachment/4/20` | 2 |
| Items | Timber | `catalog-material/0` | 2 |
| Props | Checkpoint ammunition case | `salvage-5` | 2 |
| Props | Evacuee's travel pack | `salvage-1` | 2 |
| Props | Memorial emergency tin | `salvage-3` | 2 |
| Props | Placed sleeping bag | `sleeping-bag` | 2 |
| Props | Teddy bear | `teddy` | 2 |
| Props | Triage medical locker | `salvage-2` | 2 |
| Props | Workshop tool chest | `salvage-4` | 2 |
| Weapons | Hunting knife | `catalog-weapon/23` | 2 |
| Weapons | Industrial carbine | `catalog-weapon/3` | 2 |
| Weapons | Precision rifle | `catalog-weapon/4` | 2 |
| Weapons | Trail pistol | `catalog-weapon/0` | 2 |
| Weapons | Trail rifle | `catalog-weapon/1` | 2 |
| Weapons | Woodland rifle | `catalog-weapon/2` | 2 |
| Wildlife | Rescue dog | `companion` | 4 |
| Wildlife | boar | `boar` | 4 |
| Wildlife | deer | `deer` | 4 |
| Wildlife | dog | `dog` | 4 |
| Wildlife | rabbit | `rabbit` | 2 |
| Wildlife | wolf | `wolf` | 4 |

## Failed input variants

These are failed individual input jobs, not necessarily missing asset types; another successful variant may already cover the asset.

- `Natural Colour Concepts/items/catalog-bandage-bandage-roll-option-01.jpg`
- `Natural Colour Concepts/items/catalog-field-throw-6-green-smoke-option-02.jpg`
- `Natural Colour Concepts/items/catalog-field-throw-7-blue-smoke-option-02.jpg`
- `Natural Colour Concepts/items/catalog-food-trail-provisions-option-02.jpg`
- `Natural Colour Concepts/items/catalog-hunting-material-0-raw-game-meat-option-01.jpg`
- `Natural Colour Concepts/items/catalog-hunting-material-1-untanned-hide-option-01.jpg`
- `Natural Colour Concepts/items/catalog-hunting-material-1-untanned-hide-option-02.jpg`
- `Natural Colour Concepts/items/catalog-hunting-material-2-fur-option-02.jpg`
- `Natural Colour Concepts/items/catalog-hunting-material-4-sinew-option-02.jpg`
- `Natural Colour Concepts/items/catalog-legendary-legendary-heart-option-01.jpg`
- `Natural Colour Concepts/items/catalog-legendary-legendary-heart-option-02.jpg`
- `Natural Colour Concepts/items/catalog-material-3-forest-resin-option-02.jpg`
- `Natural Colour Concepts/items/catalog-material-4-alloy-scrap-option-01.jpg`
- `Natural Colour Concepts/items/catalog-material-4-alloy-scrap-option-02.jpg`
- `Natural Colour Concepts/items/catalog-material-6-bioalloy-option-02.jpg`
- `Natural Colour Concepts/items/catalog-medicine-4-1-field-medical-kit-option-01.jpg`
- `Natural Colour Concepts/buildings/catalog-build-0-foundation-option-02.jpg`
- `Natural Colour Concepts/buildings/catalog-build-1-floor-option-01.jpg`
- `Natural Colour Concepts/buildings/catalog-build-63-electric-fence-option-02.jpg`

## Named story art and assembly work outside the default item list

These are source-backed art tasks without a one-to-one conversion catalogue slot. They are not added to the 96 default item-lookup gaps or treated as proven absent files. Existing procedural assemblies work; dedicated replacements need a separate shape/rig/interaction review.

- Mara Holt, Ivar Reed and Nora Vale: distinct faces, clothing and accessories sharing the working survivor rig (`ExpeditionCommunityRules.cs`).
- Foreman reinforced apron, Wrecker road helmet, Bellkeeper armored gloves and Brood-sealed cuirass: rig-compatible wearable variants, preserving their authored relic appearances (`ExpeditionMillBoss.cs`, `ExpeditionCampaignBosses.cs`).
- Birthday room: whole/cut cake, candles, music box with lid/crank, party hat, presents, bunting/banner, tablecloth, plates/cups/cutlery and balloon debris (`ExpeditionBirthdayArt.cs`). Reuse current tables/chairs; lettering can remain a texture.
- Willow/story house: reusable stairs, floor/upper-floor/roof sections, open wall/window sections, doors and porch trim (`ExpeditionWillowArchitecture.cs`, `ExpeditionWillowUpperFloor.cs`, `ExpeditionWillowAccess.cs`). Preserve the functional layout; a fused house GLB is insufficient.
- Dungeon/campaign rooms: gates, supports, room-specific pipework and encounter mechanisms (`ExpeditionContentDungeons.cs`, `ExpeditionCampaignWorld.cs`). A room-by-room modular art pass remains; no exhaustive unique-mesh count is claimed.
- First-person weapons/tools and animated wildlife: pivots, grips, separated moving components, rigging, retopology and animation are additional tasks even when a static inventory GLB exists.


## Reproduce

Refresh the source conversion audit with the workspace `Ashen Meadows - 3D Batch/audit_models.py`; run the game with `--region-check` to export runtime coverage; then run `scripts/audit-model-backlog.py --date YYYY-MM-DD`. The script writes this report and `model-backlog.json`; it does not launch generation or modify source assets.
