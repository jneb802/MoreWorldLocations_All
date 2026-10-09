# Crypts & Caverns dungeon setup

This is the initial room integration. It does not add a world location or a dungeon generator.
The display name, biome, exterior, creatures and loot are not decided yet.

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

No Unity assets or bundle files are changed by this code setup. A valid manifest reference
does not prove that a prefab or all of its dependencies can load; live validation is still required.
