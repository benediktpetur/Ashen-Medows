# Godot co-op playtest

Multiplayer uses Godot 4.6 ENet over UDP. The player who hosts runs the world and plays normally; no legacy relay installation or account is needed. There are eight slots including the host. This is direct-connect co-op for friends, with no public matchmaking, authentication, relay, automatic router setup, host migration or dedicated-server mode.

## Play together

1. Everyone installs the same new game build. Launch it and press **F8**, or **Esc → Multiplayer / Host or Join**.
2. The host enters a survivor name, leaves port **28748**, and clicks **Host Current World**. The current solo seed, buildings and world changes become the shared world.
3. Friends enter a name and the host's address, then click **Join Host**. On the same LAN, use the host's LAN IPv4 address. `127.0.0.1` works only when both game processes run on the same computer.
4. For internet play, the host forwards **UDP 28748** to their gaming PC and allows the game through Windows Firewall; friends use the host's public address. An existing TCP 28747 relay rule does not cover this new UDP port. The router/firewall has not been changed by this work. Carrier-grade NAT can prevent direct hosting; a reachable server or VPN/relay is needed in that situation.
5. **Play Expedition** in the launcher starts the game; hosting/joining happens inside the game. Push the source changes to `main` to let the existing GitHub workflow build and publish an update for launchers. No remote release has been published by this implementation.

## Shared gameplay and saves

Players have visible named survivor models. The host simulates infected, gun hit detection, damage and bleeding, death penalties, resource claims, terrain changes, structures and dungeon loot. Clients send gameplay requests; range/support/crafting rules are checked by the host. Character movement is client-predicted with host distance/speed limits. Friendly fire does not damage survivors.

Crafting, equipment, inventories, attributes and wounds remain individual. The host's world records harvested nodes, defeated enemies, claimed dungeon chests, trees, paths, wells, burned wrecks and buildings. Resources and dungeon chests are first-come shared claims, not duplicated for every player. Buildings still have the prototype's free material cost and structural support rules.

Guest character progress saves to their own normal profile. Solo seed, position, world edits, structures and world claim records are preserved while visiting and restored on leave/disconnect; guests do not take the host's terrain/buildings into their solo save. Saves remain outside downloaded version folders. The host keeps its shared world in its own normal profile. A disconnected character is cached for rejoining the same running session. There is no server-side cross-server character validation or cheat protection for imported profiles: use this with trusted friends.

## First-version limits

- Players can explore the surface independently across the whole playable map. Each client renders at most 49 terrain chunks. The host keeps its own 49 full-detail chunks plus physics-only windows around other players: overlapping windows are shared, with at most 392 total chunks for eight widely separated players. Remote simulation omits scenery, foliage and character artwork outside the host view. Chunks unload when nobody needs them.
- Outdoor infected spawn around every player, capped at 48 per area and 384 globally. Empty areas unload their enemies; enemy snapshots only include threats within 150m of each recipient (up to 96) and use compressed packets below the network MTU. Partly damaged enemies outside every active area can reset when later respawned, matching the prototype despawn behavior.
- The host enters, descends and exits dungeons for the whole party. Surface entry requires everyone within 24m of the host; a distant explorer is never pulled into a dungeon automatically. Guests can fight and loot inside. Dungeon death respawns at the room entrance; surface respawns use a safe personal bag regardless of host distance, or fall back to the starting meadow when no safe bag is bound.
- Fast travel is disabled online. Menus do **not** pause the online world, needs or bleeding. Solo menus still pause.
- A host leaving ends the session. Guests return to their own solo world with the latest saved character progress. This first version does not provide an always-running dedicated world.
- Guest sleeping bags use the existing per-seed personal camp format; separate hosts running the same seed do not yet have distinct camp namespaces. Do not rely on bags as permanent cross-server camp bookmarks yet.
- LAN/localhost two-process behavior is tested. Real internet routing, high latency/packet loss and eight-person load still require a group playtest. ObjectDB shutdown-leak warnings remain in headless startup/co-op test runs; long-session memory behavior is not yet characterized.

## Repeatable tests

`scripts/build-game.ps1 -Export` runs solo logic/engine checks plus `scripts/test-coop.ps1`, which starts separate host/client Godot processes on localhost UDP 29748 with isolated test profiles. It additionally separates two players by over 600m and verifies bounded physics-only streaming, per-player enemy sensing and snapshot filtering, remote combat/gather/build/path changes, distant bag respawn, and refusal of party dungeon entry while split. It verifies handshake, visible peer creation, host seed adoption, independent crafting, rotated shared building, resource gathering, authoritative gunfire, wound patching, death XP reset, party dungeon travel, reconnect, host shutdown, solo-world restoration and saved character progress. Test logs are under ignored `build/coop-checks`.

When reusing the original local Godot download, run `./scripts/build-game.ps1 -Export -ToolsRoot ../.tools` from this repository. CI uses its own default `.tools` location.

Networking protocol is now version 20. Update everyone together; older co-op builds are rejected at handshake.

Base extension: the host validates research spending, blueprint unlocks, construction costs, station proximity, storage transfers and driving authority. World snapshots include station stocks, door state, durability, fuel, production timers and mobile positions. Two-process tests cover guest research costs and deposit/withdraw conservation without changing the host's personal inventory.

Character talents: main XP and talents are serialized in the portable survivor profile. The host validates talent prerequisites and point budgets, handles respecs, and retains talents when current main XP resets on death. Two-process checks verify guest purchases do not affect host talents and guest death retains the purchased talent.

Position corrections now carry a host-issued revision. Client movement packets echo that revision, so packets sent before a teleport or respawn cannot overwrite the corrected position. All players must use protocol 20 builds.

Protocol 20 adds host-resolved melee contacts, visible arrow projectile replication, portable melee/bow proficiency, shared seeded encounter choices and a host-controlled distress-drone event. Guest supplies, proficiency and dialogue remain isolated to the acting character. Everyone must update together.

Noise and sneak stance are simulated per character on the host. Protocol 22 carries sneak stance with position updates and returns the authoritative noise meter in personal snapshots; all players need matching builds. Position correction revisions still reject stale poses.

Physical crouching uses protocol 22: host-validated capsule/headroom and crouch pose are included in personal state and nearby player snapshots. Update every player's build together.

Campaign/infected expansion uses protocol 23. Shared snapshots carry boss state, weeper aggression, brood hatch warnings and campaign completion; personal profiles carry legendary hearts and upgrade quality. The custom pumping-station dungeon has its own host-validated movement bounds. All players must use the same build.

Gunplay uses protocol 24; all players must update together. Aiming/held-breath input is validated and stamina, ammunition, loaded magazines, recoil state and special cooldown are synchronized per character. The host resolves electric contacts, flame cones, wall occlusion and burns; clients receive visual effects. Each player's fuel and energy cells remain independent.

The Waterline update uses protocol 25. Communities, the Noah/Lily arc, graves, explored cells and consequences belong to the hosted world. Research specializations, water quality inventory and health conditions belong to each character; guests cannot spend the host's supplies. See LIVING-WORLD.md.

The rescue dog/reputation update uses protocol 26. Dog state and individual standings belong to the host world, keyed by the acting player's identity; care and rescue spend only that player's food. All players must update together.

Settlement campaigns use protocol 27. Development, casualties, quest outcomes, stock, sabotage and armed retaliation are shared host-world state. Individual purchases and rebuilding costs are applied to the requesting character. Update all players together.

Field atlas content uses protocol 28. Location depletion, reserved cases, deliveries and event health/timers are host-world state. The host checks site/settlement proximity and gives loot or payments only to the actor. Update all clients together.

Soulbound update: protocol 30. Mega-hunt summons and equipment selection use server-validated campaign actions. Each eligible character receives its own bound roll; world completion and cooldowns remain shared.

Mega specialist update: protocol 30. Separate weapon/armor/community rewards, replicated breakable shells and nests, and server-authoritative moving projectile damage. Installed camp modules are shared world state; personal drops and upgrades remain character-owned.

Ten-threat update: protocol 31. Fire-pool visual type replicates alongside position/lifetime. Server enemy ticks merge self-removing actors back into the shared population; sound propagation reads the full population during per-player simulation. These are covered by Cinder death and Bell-throat co-op checks.

Habitat update: protocol 32. The server stamps selected enemy kinds onto natural spawn IDs; clients use that selection rather than classifying the enemy from its current/chase position. Old defeated slot IDs remain honored, and tagged kills record the canonical slot to prevent respawn/reward duplication.


Weapon workshop update: current protocol 33. Research/crafting and fitting/removal use host-validated actions. Parts belong to each character; save/profile snapshots carry spare stock and per-weapon slots. Invalid compatibility, missing parts, active weapon actions and duplicate fitting are rejected. Two-process coverage checks private costs, actual stamina effects and remove/refit conservation. All players must use this build.
