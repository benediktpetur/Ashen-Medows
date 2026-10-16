# Human character replacement / concept 03

The sole visual reference remains `art-reference/03-cel.png`, copied unchanged from the original concept. No external character assets or additional visual references were used.

`game/Scripts/ExpeditionHumanCharacter.cs` now builds the active survivor and infected character meshes. It replaces the previous body, head, arm, leg and hand geometry with a new human proportion layout. Shared mesh construction and the existing ink shaders are retained. The old character assembly is no longer called by ArtCharacter.

Coordinates are metres, Y up, -Z forward. Upper arms and forearms are independently authored at their actual joint origins. Positive X elbow flexion moves a hanging forearm forward, and the preview checks a 90-degree bend. Knees flex in the opposite anatomical direction. Hands have four fingers at the distal palm edge, an index-side thumb, mirrored handedness and inward-facing resting palms. Full-body survivor hands wear gloves. The existing separate FPS weapon grip geometry remains, with corrected surface winding from the shared mesh helper.

The preview captures standing, walking, 90-degree elbow flexion, hand detail, body profile, face, inventory and FPS weapon views in Godot. These are actual rendered meshes. This remains a simplified stylized mesh with articulated segment joints, rather than a professionally sculpted and skin-weighted character. It does not reproduce all the concept's painted detail.

Gameplay capsules, portable character saves, stats and multiplayer transport are retained. Cached appearance meshes are shared between instances. Local reviewed captures are in ignored `build/human-character`.

Seven infected variants are defined in ExpeditionZombieModels.cs and use the approved human rig: tier 1 valley drifter (scarf and pack), tier 2 woodland logger (checked work shirt, suspenders, knit cap), tier 3 highway worker (hi-vis vest and hardhat), tier 4 office revenant (lapels and tie), tier 5 security guard (vest, cap and belt pouches), night type 1 stalker (narrow torso, cowl and pale scars), night type 2 bloater (distended abdomen, stretched seams and shoulder growths). Model selection is deterministic from the existing tier/night fields, so clients and streamed instances match without new replication data. These are visual variants, with existing enemy stats and behaviour retained. The survivor is unchanged. --zombie-preview renders all seven plus individual views using isolated test saves. Cached mesh keys include the variant index.
