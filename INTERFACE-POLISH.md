# Interface and feel pass

- Pause menu groups inventory, research, character, camp, field notes, expeditions, journal and map. Options, controls, multiplayer and world/save management have their own entries.
- Seed switching, trailhead relocation and legacy import are collected under World & Saves. Existing online restrictions and save behavior remain in place.
- F2 opens a dedicated controls reference. The normal HUD shows a short set of essential shortcuts; build mode retains its relevant placement shortcuts.
- Shared menu panels have a dim background, a short opening fade, larger standard buttons and overflow scrolling. Existing panel sizes and inner research/inventory scrolling are retained.
- Inventory uses a taller layout, a smaller portrait and the existing four gear slots instead of a second chest-only control. Equip/use actions sit beneath item details rather than below the longest column. Zero-value weapon bonus text is omitted.
- Research and talent tree viewports leave room for bottom actions. The talent tree retains every node, connection, requirement and cost.
- HUD notices wrap near the top center and fade at the end of their display time. Persistent connection text no longer repeats in the notification area. Multiplayer status remains available in its panel.
- Health and stamina bar interpolation affects presentation only. Their labels and gameplay values remain immediate. Aim zoom and held-weapon transitions interpolate with a frame-rate-independent blend; recoil, hit detection, spread and stamina rules are unchanged.
- Canvas-item stretching preserves the authored 1280×720 interface when the window changes size.
- HUD panel styles and building-preview mesh/material resources are reused instead of allocated each frame. This removes unnecessary work; no frame-rate improvement is claimed without profiling.
- Routine notices yield to level-up messages so the two do not overlap.

The saved detailed-info preference remains supported. Story text, action costs, research commitment warnings and gameplay requirements are not removed by this cleanup.

`--clean-ui-preview` captures pause, controls, world management, inventory, crafting/research and HUD through the game renderer. `scripts/build-game.ps1 -Export` runs the existing logic, music, journey, first-day, full integration and two-process ENet checks before exporting Windows.

During validation, the workshop co-op fixture exposed a scene-timer pacing race. Its requests now wait against the same monotonic clock used by the host limiter. The deliberately exhausted burst-budget fixture is retained, with no mutation retries or change to the production rate limit. Full co-op checks also pass at a capped 30 FPS.
