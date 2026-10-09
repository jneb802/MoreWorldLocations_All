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

## Verified Unity mapping: stone_2x1_high (2026-10-09)

Room objects must retain `JVLmock_` and reference the intended game prefab. The earlier
removal of 3,413 mock prefixes bypassed resolution and was not a valid fix. Those prefixes
have been restored from the original backups. Its live screenshots and absence of mock
warnings do not validate the corrected mapping below.

The first scoped replacement is:

`JVLmock_stone_2x1_high` → `JVLmock_SunkenKit_int_wall_1x2`

The Unity rip's `SunkenKit_int_wall_1x2/stone_hgih` uses the exact same mesh asset as
`stone_2x1_high/default`: `GameElements/Pieces/_res/stone/default_3.asset`, GUID
`1a0d8db346766234696718939c5be8d0`. Its transform relative to the prefab root is also
identical. This mesh occurs 140 times across 27 vanilla room prefabs, including Sunken
Crypt rooms and Hildir tower rooms. This establishes geometry reuse, not that the name
`stone_2x1_high` itself is a valid runtime mock target.

`tools/unity/replace-crypts-stone-2x1-high.cs` replaces only this model type with connected
instances of `world/Props/CastleBuildingKit/SunkenKit_int_wall_1x2.prefab`, named with the
mock prefix. It preserves each instance's position, rotation, scale, sibling order and
active state. It rejects unexpected meshes or nonvisual source components and checks
the complete high-detail mesh transformation before saving. Explicit component references
are remapped to the replacement. Backups and a report are written to a unique temporary
directory.

The replacement supplies the Sunken Kit interior material, its lower-detail mesh and its
collider. The material is `sunkenkit_stone_mat_interior 2`; its `_AddSnow` setting is zero.
The target prefab's authoring root offset and rotation are not copied into placed instances.

Unity validation confirmed 538 replacements across 21 room prefabs, with unchanged
high-detail mesh placement. Each replacement remains connected to the intended prefab
and has its level-of-detail component and collision. Mock counts match the original
rooms exactly, except for this explicit name mapping. The open authoring scene reflects
all 538 replacements with no remaining old target names and no unsaved scene changes.
The updated material inventory is in [crypts-caverns-room-materials.csv](crypts-caverns-room-materials.csv).

The other model types remain mocked with their original names pending individual review.
The earlier development bundles are obsolete: rebuild before testing this mapping.
No new bundle deployment or runtime validation has been performed for this change.
Valnet remains off. The exterior entrance, remaining model mappings, room connections
and multiplayer still require validation.
