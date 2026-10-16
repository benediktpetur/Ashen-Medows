# Item catalogue

The current supported catalogue contains 48 visual definitions. These are authored Godot meshes, integrated with existing item rules rather than a new inventory/save format. All 27 crafting recipes resolve to an appropriate item model. Materials use these models in the world; crafting and inventory detail panels render them in a lit 3D preview. Weapon variants are also used in first person. Equipped chest armor is shown on the local inventory portrait.

| Group | Entries |
| --- | --- |
| Weapons (6) | Trail pistol; Trail rifle; Woodland rifle; Industrial carbine; Precision rifle; Experimental rifle |
| Armor (5) | Field; Woodland; Industrial; Advanced; Experimental chest plates |
| Materials (7) | Timber; Stone; Fiber; Forest resin; Alloy scrap; Electronics; Bioalloy |
| Survival and tuning (7) | Bandage roll; Trail provisions; Drinking water; Sleeping bag kit; Ammunition box; Night catalyst; Nocturnal tuning module |
| Workshops (5) | Starter workbench; Workshop upgrade; Industrial upgrade; Advanced upgrade; Experimental upgrade |
| Building parts (8) | Foundation; Floor; Wall; Half wall; Pillar; Beam; Stairs; Roof |
| Existing utility tools (3) | Field axe (T); Earthwork shovel (P/O); Builder hammer (B) |

Crafted chest plates and dungeon relics share tier-appropriate armor geometry; saved relic IDs provide small stable palette variation. This does not introduce 30 separate armor rules for the six random origins. The sleeping bag kit has a rolled model, while placed sleeping bags have bedding, pillow and stitching.

The utility tools are models for existing key-driven actions. They are not new equippable inventory items. Workbench upgrades remain progression state, ammunition remains a shared reserve and nocturnal tuning remains an upgrade; their visual representations do not add extra tradable stacks. Building costs/support remain as before. No unimplemented legendary items, boss drops, vehicle parts, new damage rules or multiplayer remote-avatar gear replication are claimed.

Model coverage is checked for all definitions and every recipe. `--item-preview` renders seven category galleries plus crafting and inventory captures using isolated saves. Review images are copied into ignored `build/item-catalogue`.

Survival update adds seven head/hand/foot recipes and their preview models, bringing the catalogue to 48 definitions. T equips the axe and each left click performs a deliberate swing.
