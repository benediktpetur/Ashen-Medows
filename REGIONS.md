# Area progression

1. **Suburbs:** residential streets, gardens and abandoned homes. The player's house and existing starter stories belong here. Named farm stands and allotments are neighbourhood landmarks, not a separate meadow biome.
2. **Woodland outside the city:** forest, trails, logging and rural approach sites.
3. **Highway corridor and industrial zone:** a wider marked highway, warehouses, machine works, freight depots, loading bays and truck-service garages. These replace the generic outskirts-town storefront appearance. Industrial lots have worn paved surfaces.
4. **Dense city:** closely packed taller buildings, shops, civic/medical facilities and paved cross streets.
5. **City depths:** existing sewers, tower/endgame encounters and the lower hospital ward.

## Map implementation

Existing organic region boundaries, main route, residential blocks, forest, reserved story lots and dungeon locations remain compatible. This pass makes each area's identity readable, particularly by replacing tier-three town facades with industrial buildings. Tier-three highway paving is 14 metres wide with lane and edge markings. Building footprints and matching host simulation collisions are retained. Warehouse doors are visual loading bays on the existing solid ambient buildings; this pass does not add enterable factory interiors.

`ExpeditionWorld.TierName` supplies canonical UI/map labels. `ExpeditionRegionArt.cs` implements industrial art and regional paving. `--region-check` verifies progression and industrial visual/simulation collision parity. `--region-preview` also captures four in-engine area views.

The World Writer's accepted region fact and source snapshot use this progression. Existing drafts are not rewritten automatically.

Verification: C# build, gameplay and two-process ENet checks passed; the Windows release was rebuilt. The exported executable passed `--region-check`, including city paving and highway width. Four engine-rendered views are in `Ingame Screenshots/Regions`. Validation logs are in the sibling `Ashen Meadows - 3D Batch` folder (`regional-game-validation.log` and `exported-region-validation.log`). The previous local build is preserved in `build/before-region-map-20261027/game`.
