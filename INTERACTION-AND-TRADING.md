# Interaction, construction hammer and player trading — 30 October 2026

## Controls

- **E** interacts: containers, workbenches, doors, resources, NPC conversations and nearby-player options. E also dismounts vehicles. F no longer triggers interaction or furniture salvage.
- **B** equips/puts away the construction hammer. It is a starting utility tool, also available through the inventory's construction-hammer button. Equipping it opens the building catalogue and shows a held hammer.
- With the hammer: left click places, right click opens the catalogue, Tab opens the catalogue, the wheel cycles pieces, Q/R rotates, Ctrl makes rotation precise, G toggles snapping, Page Up/Down changes height, Backspace resets orientation/height. E still interacts.
- The catalogue offers **Dismantle my build** (half the original material cost, rounded down) and **Destroy my build** (no refund). Delete toggles the removal preview. Both require an aimed structure owned by the acting builder.
- Switch to a weapon with 1/2/3 or the inventory, or to a harvesting tool with T, to put away the hammer.

## Ownership and damage

New structures save a builder identity. The host stamps that identity for guest placements, ignoring the identity sent in a placement packet. Ownership is checked again on the host when removing a structure. Another player's structures and world objects cannot be removed through the hammer. Stored contents, fuel, mounted vehicles and structural support dependencies block removal until dealt with.

Older saves have no builder history. Previously unlabelled builds are assigned to the local world owner on load; historical guest authorship cannot be reconstructed. Automatically migrated community cots remain world-owned.

Weapon strikes and gun/arrow hits damage player-built pieces, furniture, residential wall sections and ambient city/industrial building shells. Furniture has individual 90 HP; residential wall sections have 180 HP; ambient building shells have 600 HP. Player pieces retain their tiered durability. Destroyed furniture grants its existing finite salvage reward. Wall/building damage and destruction persist in the world's save data and synchronize to other players. Destroyed geometry loses its collision. Buildings and furniture do not use the hammer's owner-only removal path for weapon damage.

Authored story set pieces and the starting respawn bed retain their existing mission-specific behavior. This does not add destruction physics to every decorative prop. Ambient houses can lose wall sections without simulating roof collapse; player-built supports retain structural collapse behavior.

## Player trade

Look at a player within 3.5 m and press E, then choose Trade. The other player receives an invitation and uses E → Trade to accept. Both players review both offers and confirm them. Changing an offer resets both confirmations; confirmation packets carry the offer revision so a delayed click cannot approve a newer offer.

Trade supports crafting materials, food, clean/untreated water, bandages, medicine, fuel, scrap, basic arrows, all five ammunition classes, unequipped ordinary armor and spare ordinary weapons. Weapons retain their loaded magazines and quality. Starting stick/stone, equipped weapons, fitted weapons and soulbound/boss-bound equipment are excluded from ordinary trade. The existing original-kill-group boss-loot trading remains available in the Soulbound armory.

The host checks ownership, quantity, equipment state, binding, duplicate item IDs, inventory overflow, life state, distance and line of sight. Offered equipment descriptions include armor/vitality or weapon quality/magazine contents and must still match at settlement. Both inventories update together after validation; failed saves restore their snapshots. The underlying portable-character co-op save system remains unchanged; this does not claim crash-proof transactions across two computers.

Walking more than 4 m apart, obstruction, death, disconnect, cancellation or timeout closes a pending trade without moving items. Invitations expire after two minutes without activity. Closing the window alone leaves the invitation/session available until cancelled or expired.

## Validation

`--interaction-check` exercises physical furniture targeting, E without dismantling, individual furniture/wall health, destruction/collision removal, save serialization, owner-only hammer removal, stocked-container protection and material refunds. It also checks ordinary-trade conservation, unavailable/negative/duplicate offers, equipped/bound gear rejection, magazine/quality transfer and overflow protection.

Two-process ENet checks exercise a real guest invitation, offer changes, both confirmations, old-revision rejection, exact inventory transfer, range cancellation and attempted guest removal of a host wall. The existing initial guest placement now also carries a forged builder field to verify the host replaces it. Multiplayer protocol is 45; both players must use this build.

Current local executable: `build/game/AshenMeadow.exe`. No launcher release is published by this change.
