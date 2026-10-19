# First day: a foothold

The opening is an optional, saved survival sequence. F1 opens the field notes; the small HUD card tracks the next unfinished action. There is no deadline, forced combat, failure penalty or extra research currency. Guidance can be tucked away and restored on F1. Actions performed out of order still count.

1. Search the blue emergency carrier at the southeast trailhead and drink clean water (C). Each character receives three water, two food and two dressings once per world seed. The claim record travels with that character, including through co-op.
2. Gather branches, stone and fiber. Research and craft the crude spear using the normal recipe and research budget. F1 offers the appropriate research/craft action.
3. Craft the stone axe: two timber, three stone, two fiber. T equips it.
4. Aim and swing at an actual trunk. Normal stamina costs, collision, hit count, timber rewards, falling animation and saved stump apply. Three additional Tier 1 trees stream with the nearby terrain chunks.
5. Craft the starter workbench using six timber and four stone.
6. Build a sheltered survivor cot, with a campfire and storage chest within 12m. Shelter uses the existing roof and two-wall checks. F1 offers the six essential building blueprints; purchases, costs, supports and placement rules remain in effect.
7. Store two food and two clean water in a nearby chest. The hand pump supplies untreated water, capped at 20 carried; boiling uses normal fuel and workbench/campfire rules. Actual conserved deposits complete the opening.

The nine finite forage piles provide 60 timber, 48 stone and 48 fiber before existing starting resources and harvested trees. They use normal gathering, shared depletion and streaming. This is enough for the tested opening camp while allowing some experimentation. The carrier gives each guest their own small ration; gathered world resources and built camps remain shared.

Completing a milestone records it permanently. Death and travel do not erase it, and dismantling a camp does not revoke previously learned steps. A cot does **not** grant respawn: Tier 2 sleeping bags retain that purpose. Completion adds no invulnerability or permanent safe zone. F5 becomes the suggested next step for a planned woodland expedition.

## Validation

`--first-day-check` creates a fresh test character, collects finite supplies, researches and crafts the spear/axe, fells a real tree through aimed collision, pays for and places a supported camp, draws and boils water, deposits provisions, and reloads the saved result. `scripts/build-game.ps1` runs this check before the existing full integration suite.

The two-process co-op suite also checks guest claim range, duplicate protection, private supplies/milestones and the actual personal-profile envelope. Multiplayer protocol is 35; all players must update together.

`--first-day-preview` captures the trailhead, field notes and camp guidance using the actual game renderer. The camp guidance capture shows a staged UI progression state; it is a UI demonstration, not a completed player save.
