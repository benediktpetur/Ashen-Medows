# Ground, roads and foundations — 28 October 2026

The shared tiled atlas has been removed from terrain shading. Grass uses muted olive, dry straw and earth patches; woodland uses brown leaf litter; gravel, soil, asphalt and concrete have separate restrained surface treatments. Fine detail fades with distance. These are local shader materials, not newly generated image textures.

The existing main route is now painted directly onto the terrain triangles, including its shoulders, urban sidewalks and lane markings. This removes separate road sheets and their depth interference. Markings leave gaps at cross streets, and sidewalks open where those streets join the route. Industrial service roads use a 64m grid, city streets a 32m grid, fitting between the existing building footprints. The existing residential street grid is retained, with the authored Briar Lane reservation excluded so unrelated grid streets no longer cross its gardens. Briar Lane connects back to the main route.

House approaches continue from each existing porch ramp to its facing street. Trees, grass and forest ferns use the shared surface-clearance rules, keeping routes and approaches readable. Road footsteps now match paved surfaces, and approaches use gravel footsteps.

Residential houses now have continuous concrete foundation volumes extending below the lowest sampled terrain under their footprint. The former four-post support remains replaced by a full foundation; side garages receive their own foundations too. House positions, floor elevations, interiors, lot IDs and natural terrain heights have not moved. Player earthworks and the legacy settlement terrain-preservation system remain intact.

Twelve completed oak reconstructions were rendered together for comparison. Oak 11 and Oak 05 now provide broadleaf variation, replacing Oak 07 in placement. Their source queue paths are recorded in `scripts/world-model-selections.json`; original outputs and the earlier registered copy are preserved. Garden trees are smaller; dense woodland retains taller trees and the existing conifer. All use shared imported geometry/material resources. There are now 168 registered static/environment models.

The paper map now shows the neighbourhood and urban street networks, the Briar Lane connector and the four surface region names. This is a coherent layout pass on the existing world, not a replacement of the world with a fully hand-authored town. Further site-specific yards, road-end treatment, terrain grading around authored story locations and a representative frame-rate benchmark remain worthwhile follow-ups.

Validation passed: C# build and asset import, 168 registered model checks, street-facing approach coverage across more than 100 residential lots, ground contact at every corner of the eight starting houses, regional collision/progression checks, the complete gameplay suite, two-process ENet co-op checks and Windows export. Saved-settlement migration, home access, furniture salvage, forest density and saved tree chopping passed. The exported executable itself passed the regional checks and rendered all four surface areas, house/warehouse exteriors and the updated map without shader errors. Logs are in the sibling `Ashen Meadows - 3D Batch` folder (`ground-build.log`, `exported-ground-validation.log`). Screenshots are in `Ingame Screenshots/Ground and Layout - 2026-10-28`.

Run the updated local build at `build/game/AshenMeadow.exe`. These changes have not been committed or published to the launcher release.

## Placement correction after close-up review

A close-up exposed furniture outside the house and a wreck intersecting a fence. Furniture replacement bounds had included the container's transform, then applied that offset again to the replacement child. Bounds now use the container's local space; the same correction applies to registered resource replacement. Furniture feet sit at the house floor level. Wreck visuals fit all three collider dimensions and align their lowest geometry with ground; Briar Lane cars move from the fence line at local X=9 to X=7.5 and follow the sampled ground slope.

Regression checks use translated, rotated and scaled furniture parents, a translated resource parent, both wreck variants and explicit fence/wall clearance. Close-up captures are in `Ingame Screenshots/Placement Fix - 2026-10-28`. The earlier wide shots were insufficient to catch these placement defects.
