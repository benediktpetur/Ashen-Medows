# Hunyuan actor integration

The game uses the textured `model-1.glb` results from the workshop's **Models from text** batch: one player body and 21 distinct enemy meshes. These cover the regional/night variants and fifteen specialist/rare infected, with city security reusing the armored mesh. Original workshop files are untouched.

Game copies are in `game/Assets/Hunyuan/Actors`. `source-manifest.json` records source paths, original and game SHA256 checksums, triangle counts and resource paths. `scripts/prepare-hunyuan-actors.py` rebuilds these copies using NumPy. It removes distant disconnected reconstruction debris and needle-like faces, retaining UVs, materials, significant separate components and components close to the body. This conservative cleanup does not repair attached artifacts or missing anatomy.

## Runtime

`ExpeditionHunyuanActors.cs` maps existing actor identities to the generated art. Regional selection, spawning, stats, collision, damage, loot and network authority continue using their existing implementations. The procedural joints and boss weakpoint nodes remain as animation/gameplay anchors. Existing named story characters, human NPCs and bosses retain their authored designs. Existing wildlife and infected dogs are unchanged. The generated boss results were rejected because several are busts, duplicated figures or unrelated objects. The security output contains three fused people; its gameplay slot instead uses the valid armored model.

The player body is used for co-op avatars and the player model preview. First-person weapon arms remain the existing dedicated weapon rigs; a full-body reconstruction is not a replacement for first-person hands.

Hunyuan files arrived without skeletons, skin weights or animation clips. The adapter normalizes scale, aligns the shoulder axis, fits arm joints, builds shared skin weights, and drives a 12-bone skeleton from the game's existing walk, attack, stagger and special poses. Meshes and weights are cached per model. Existing boss art, scale and weakpoint indicators are preserved. The generated worm is a miniature humanoid rather than a worm, so the existing worm art is retained. The riot shield remains visible for readable counterplay.

This is a first playable art integration, not a finished character art pass. Automatic weighting can stretch garments around shoulders and hands, and source meshes still have some missing or malformed details. Manual topology repair and authored skin weights are needed for polished close-up animation. There is no claim that Hunyuan produced game-ready rigs.

## Verification

`--hunyuan-check` loads all 22 models through the actual actor builders and checks textures, normalized scale, skin weight sums, rest height, deformed mesh height, walk and attack bone motion. `--hunyuan-preview` also captures six roster images plus player/drifter walk and attack views. The regular build script includes the actor check, gameplay checks, and two-process ENet co-op checks.

Screenshots are copied to `Ingame Screenshots/Hunyuan Actors` after GPU capture. These are engine-rendered model review scenes, not AI mockups.
