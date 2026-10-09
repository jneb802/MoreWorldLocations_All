# Crypts & Caverns dungeon setup

This integrates the rooms and a temporary `Crypt4` exterior clone for manual generation tests.
The final display name, biome, exterior, creatures and loot are not decided yet.

## Temporary test exterior

`MWL_CryptsCaverns_Test` clones the vanilla `Crypt4` location when all room asset references
are available. Its quantity is zero, so it does not enter natural world generation.
Use the `location MWL_CryptsCaverns_Test` developer command in a disposable test world.

The clone keeps Crypt4's entrance, interior environment and generator settings. The generator
uses `MWL_CryptsCaverns`, excludes the vanilla room theme, and clears Crypt4's required vanilla
room list. Its unique network prefab name, `MWL_DG_CryptsCaverns_Test`, prevents Expand World
Data from substituting the vanilla generator when it spawns objects by prefab name.
The original `Crypt4` location is unchanged.

## Room assets

`CryptsCavernsDungeon.Rooms` lists the 22 prefabs from the Crypts & Caverns authoring scene.
The current asset directory is `Assets/WarpProjects/CryptRecovery/Rooms/`.
The working custom theme is `MWL_CryptsCaverns`.

Room registration uses the existing `MWLRoom` soft-reference path and reference resolution.
It requires all 22 room references in the installed asset manifest. If none are present,
registration skips the set with an information message. If only some are present,
registration skips the entire set and logs the missing names.

The code sets only the custom theme. Unity remains the source for each Room component's
entrance, endcap, divider, enabled state, weight and placement settings. Several numbered
rooms are endcaps, so their role must not be inferred from their names.
Scene grid positions are for editing only; dungeon generation uses the prefab assets.

`RoomDB` includes the new rooms in its name/theme lookups and asset-path list for manifest
generation. The existing room sets keep their registration behavior.

## Remaining integration

1. Finish and save the room prefab assets in Unity, including room bounds and connections.
2. Decide the dungeon name, biome, exterior prefab and generation settings.
3. Add the generator and exterior using the same custom theme. Add the exterior to
   `LocationDefinitions`, location quantity configuration and localization.
4. Configure any required custom props, creatures and loot through the existing systems.
5. Build the room bundle and regenerate the combined soft-reference manifest with the
   new `RoomDB.GetAllAssetPaths()` entries. Package all referenced assets and dependencies.
6. Test the absent, partial and complete asset cases. Generate several dungeon layouts
   and check room connections, collision, entrances, exits and multiplayer persistence.
   Complete final validation with the current production season's mod set before release.

The Unity project and built bundles are maintained outside this repository. A valid manifest
reference does not prove that a prefab or all of its dependencies can load; live validation is still required.

## Local manifest generation build

The feature branch temporarily enables `AssetBundles.BuildCombinedManifest` in
`MoreWorldLocations.cs`. Install the candidate DLL and each individual room bundle with
its Unity `.manifest` file in the local mod's `Bundles` directory before launching.
Startup writes `assetBundleManifest_full` beside the DLL using the location and room catalogs.
Keep the previous DLL, manifest and bundles in a backup outside the loaded profile.

This call runs on every launch while enabled. After the first successful generation,
disable the call and rebuild the DLL before using this as a normal test build or release.
The generated manifest must stay beside that DLL.

## Initial generation test (2026-10-09)

On Valnet client 01, the Season 8 profile used the production 8.0.39 dependency versions,
the candidate DLL, the 22 individual room bundles and the valheimCLI test helper.
Manual placement in `TestingWorld` found all 22 custom rooms. Seeds 1041, 1042 and 1043
placed 46, 33 and 27 rooms respectively, including endcaps. The test exterior had no
naturally placed instances before manual placement.

An initial test retained the name `DG_ForestCrypt` and generated vanilla rooms because
Expand World Data resolves spawned objects by prefab name. Giving the cloned generator
its own network prefab name fixed that behavior in the repeated test.

This is generation proof, not release validation. Jotunn reported unresolved model
references, including `stone_1x1_high`, `stone_1x1`, `stone_4x2`, `stone_2x1_high`,
`stone_4x2_high` and `stonepillar`. The Linux client also reported missing shader-platform
data. Room rendering, collision, entrance/exit traversal, saved-world reload and multiplayer
still require validation after the asset issues are resolved.

## Visual model reference repair (2026-10-09)

The recovered room prefabs marked embedded visual models as `JVLmock_` objects. For example,
`JVLmock_stone_1x1_high` refers to an internal model, while the game building prefab is
`stone_wall_1x1`. Jotunn searches its asset and loaded-object caches for the target name.
These model targets can be absent on one load and available after other locations load.
When found, Jotunn replaces the authored object, including its material. This made room
appearance depend on which other assets had loaded. The white surfaces occurred in that
replacement path; the authored textures were present in the bundles.

The repair removes the mock prefix from embedded visual models whose complete component
hierarchy contains only Transform, MeshFilter and MeshRenderer components. It preserves
their meshes, materials, transforms and hierarchy. Gameplay mocks, including spawners,
chests and pickables, keep their references. Shader and material mocks are unchanged.
The authoring repair changed 3,413 object names across 21 rooms. The endcap needed no edit.
All 22 individual development bundles were rebuilt with the existing Windows/LZMA settings.

`tools/unity/repair-crypts-visual-mocks.cs` reproduces the repair through Unity Pipeline's
`eval_file` command. It backs up the prefabs and their metadata to a unique temporary directory
and writes a change report there. Run it only in the Crypts & Caverns authoring project.
A second run against the repaired prefabs reported zero changes. Byte comparison against
the backups confirmed that only the mock prefixes changed.

The full per-room material inventory is in [crypts-caverns-room-materials.csv](crypts-caverns-room-materials.csv).
The rooms use Standard, Standard (Specular setup), Standard TwoSided, Particles/Standard Surface,
Sprites/Default and mocked Valheim Custom/StaticRock, Custom/Creature and Custom/Piece shaders.
All 3,478 inspected meshes had nonempty, nonconstant texture coordinates. The live diagnostic
found texture bindings on the visible geometry. No blanket shader replacement is included.

Final visual check used the Season 8 8.0.39 client profile, the rebuilt feature DLL and all
22 rebuilt bundles. The temporary material diagnostic plugin was removed before launch.
The saved 12-room test dungeon reloaded, and screenshots showed textured arches and walls
at player position approximately `(-59.9, 5055.3, -229.8)`. The session registered all 22
custom rooms and reported no mock-resolution warnings. All 45 deployed files (22 bundles,
22 Unity manifests and the DLL) matched their build hashes. The DLL build completed with
zero errors and 148 existing warnings.

This proves the visual repair at the tested interior position and successful saved-room
loading. It does not prove every room's appearance, all room connections or multiplayer.
An additional generator object spawned away from the player loaded zero rooms, so it
does not count as another generation test. The exterior entrance is still inaccessible
in the interactive test and needs separate work. Shader-platform messages remain during
startup; the inspected room surfaces render with their textures despite those messages.
