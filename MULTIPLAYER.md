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

- Explore as a party within **120 metres of the host**, which preserves the existing 49-chunk streaming budget. The host can regroup stragglers automatically. Independent distant expeditions are not implemented.
- The host enters, descends and exits dungeons for the whole party. Guests can fight and loot inside. Dungeon death respawns at the room entrance; surface respawns use a safe personal bag or fall back near the host when the bag is outside the party area.
- Fast travel is disabled online. Menus do **not** pause the online world, needs or bleeding. Solo menus still pause.
- A host leaving ends the session. Guests return to their own solo world with the latest saved character progress. This first version does not provide an always-running dedicated world.
- Guest sleeping bags use the existing per-seed personal camp format; separate hosts running the same seed do not yet have distinct camp namespaces. Do not rely on bags as permanent cross-server camp bookmarks yet.
- LAN/localhost two-process behavior is tested. Real internet routing, high latency/packet loss and eight-person load still require a group playtest. ObjectDB shutdown-leak warnings remain in headless startup/co-op test runs; long-session memory behavior is not yet characterized.

## Repeatable tests

`scripts/build-game.ps1 -Export` runs solo logic/engine checks plus `scripts/test-coop.ps1`, which starts separate host/client Godot processes on localhost UDP 29748 with isolated test profiles. It verifies handshake, visible peer creation, host seed adoption, independent crafting, rotated shared building, resource gathering, authoritative gunfire, wound patching, death XP reset, party dungeon travel, reconnect, host shutdown, solo-world restoration and saved character progress. Test logs are under ignored `build/coop-checks`.

When reusing the original local Godot download, run `./scripts/build-game.ps1 -Export -ToolsRoot ../.tools` from this repository. CI uses its own default `.tools` location.
