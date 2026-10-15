# Validation

Build: dotnet build, zero warnings/errors. Godot 4.6 .NET imports the project, runs the scene headlessly and exports Windows .NET builds.

--logic-check: original terrain/region, structural support/weight, recipe/tier, skill/death and relic serialization checks; real crafting costs/tier progression; bags/respawn/travel costs and cooldown/night/wound blocks; single/multiple bite countdown; non-healing bandages and full recovery timer; slow regen; night loot; reload ammo conservation; profile/build/camp roundtrip; legacy import; corrupt-save protection; failed-save rollback.

--integration-check: actual Godot raycast headshot and physical-wall occlusion; bounded streaming; live bag/structure nodes; path creation, well/water collection, tree felling and burned wreck persistence; all gameplay panels; dungeon geometry/guards, guarded chest lock, reward and duplicate protection; death and XP retention; runtime nocturnal spawns and persistence. Runs against an isolated user://tests profile.

scripts/build-game.ps1 runs both solo test modes and the two-process ENet checks in scripts/test-coop.ps1 before export; ERROR output fails the build even if Godot returns zero. --preview captures actual GPU-rendered gameplay and crafting screens in user://tests. Shader compilation and rendering are tested on the available NVIDIA OpenGL 3.3 device.

Launcher checks remain in launcher/build.ps1. Localhost Godot co-op tests cover independent crafting, shared structures/resources, enemy replication and host raycasts, wounds, death XP, party dungeons, reconnect and host shutdown, solo-world restoration and guest character persistence. GitHub publication, internet routing and eight-player load have not been tested remotely. See MULTIPLAYER.md.

Known diagnostic: the short ten-frame startup smoke can emit an ObjectDB shutdown-leak warning; the full solo integration checks exit cleanly, while the headless two-process co-op runs also emit this warning. This has not been characterized with a long-session memory test. No gameplay exceptions or compiler errors remain in the verified scenarios.

Graphic-novel art pass: actual GPU captures cover camp, woodland, outskirts, city, workshop, loadout, ward, night hunter, skills, camps, map and building. Shader/model surfaces were inspected and corrected; texture mipmaps enabled.
