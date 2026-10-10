# Crypts & Caverns dungeon setup

This integrates the rooms and a temporary `Crypt4` exterior clone for manual generation tests.
The final display name, biome, exterior, creatures and loot are not decided yet.

Latest Unity state: 2,849 additional prefab replacements have been applied and checked,
including the scaled 1x1 SunkenKit mapping. Only 4 coffin models and 24 slab LOD models
remain pending from the mapping review. See [Applied reviewed mappings](#applied-reviewed-mappings).
Bundles have not been rebuilt after these changes.

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

The complete read-only inventory and remaining candidates are in the
[prefab mapping CSV](crypts-caverns-prefab-mapping.csv). The
[mapping evidence](crypts-caverns-prefab-mapping-evidence.json) records source variants,
mesh GUIDs, root-relative mesh matrices, native materials, components and example
vanilla room hierarchies. See the mapping review below before any further replacements.

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

At this stage the other model types still used their original names pending review.
The earlier development bundles are obsolete: rebuild before testing this mapping.
No new bundle deployment or runtime validation has been performed for this change.
Valnet remains off. The exterior entrance, remaining model mappings, room connections
and multiplayer still require validation.


## Remaining prefab mapping review (2026-10-09)

This audit covers all 41 current mock names and 3,491 instances in the 22 room assets.
It made no Unity asset changes. The 538 previously applied SunkenKit replacements are
included for completeness. There are 2,953 other mock instances.

The table lists native candidates, not an executable replacement plan. Preserve the
`JVLmock_` prefix. `__` separates a native prefab root from an exact child path; Jotunn's
MockManager supports this syntax. Child targets must still resolve in the live game.

- `KEEP`: existing native target and geometry match; retain its behavior.
- `MATCH`: matching native geometry and mesh transform; native components or materials can differ.
- `CHILD`: exact visual subtree found in a native prefab. Check runtime lookup, collision and level-of-detail behavior.
- `OFFSET`, `SCALE`, `TRANSFORM`: shared mesh, but native internal placement differs. A name-only swap changes room geometry.
- `VARIANT`: model-only and complete native instances need different treatment.
- `REVIEW`: candidate changes geometry or behavior, or does not satisfy the SunkenKit requirement. Hold replacement.
- `PAIR_REVIEW`: two model names belong to one native level-of-detail prop. Inspect colocated pairs before replacement.
- `APPLIED`: the earlier approved replacement, still awaiting runtime validation.

| Current target (without `JVLmock_`) | Count | Native candidate (without `JVLmock_`) | Status |
|---|---:|---|---|
| `CircleStone` | 1 | `StartPlatform` | OFFSET |
| `CurvedRock` | 17 | `caverock_curvedrock` | OFFSET |
| `FloorBig` | 1 | `caverock_floorbig` | MATCH |
| `Pickable_ForestCryptRandom` | 39 | `Pickable_ForestCryptRandom` | KEEP |
| `Pickable_SurtlingCoreStand` | 5 | `Pickable_SurtlingCoreStand` | KEEP |
| `Skull2` | 1 | `Skull2` | KEEP |
| `Spawner_Skeleton` | 7 | `Spawner_Skeleton` | KEEP |
| `Stone throne_broken` | 2 | `piece_throne02__Broken__high` | CHILD |
| `SunkenKit_int_wall_1x2` | 538 | `SunkenKit_int_wall_1x2` | APPLIED |
| `TraderRune` | 1 | `TraderRune` | KEEP |
| `TreasureChest_forestcrypt` | 4 | `TreasureChest_forestcrypt` | KEEP |
| `altar` | 3 | `altar` | SCALE |
| `caverock_curvedrock` | 3 | `caverock_curvedrock` | KEEP |
| `crypt_skeleton_chest` | 2 | `crypt_skeleton_chest` | KEEP |
| `crypt_skeleton_laying` | 2 | `crypt_skeleton_laying` | KEEP |
| `dirtwall` | 36 | `dirtwall` | MATCH |
| `fi_vil_cath_decor_swords_cross` | 2 | `fi_vil_cath_decor_swords_cross` | MATCH |
| `fi_vil_combs_props_bone_coffin` | 4 | `crypt_skeleton_laying` | REVIEW |
| `fi_vil_combs_props_bone_hang_01a` | 7 | `crypt_hangingskeleton` | MATCH |
| `fi_vil_combs_props_bone_skull` | 28 | `Skull1` | MATCH |
| `fi_vil_combs_props_bonepile_01e` | 16 | `BogWitch_Camp__Hut__fi_vil_combs_props_bonepile_01e` | CHILD |
| `fi_vil_combs_props_bonepile_02b` | 6 | `BogWitch_Camp__Hut__fi_vil_combs_props_bonepile_02b` | CHILD |
| `fi_vil_combs_props_bonepile_02c` | 7 | `Pickable_ForestCryptRemains03__crypt_skeleton_pile03` | CHILD |
| `fi_vil_combs_props_bonepile_04a` | 4 | `Pickable_ForestCryptRemains02__crypt_skeleton_pile02__fi_vil_combs_props_bonepile_04a` | CHILD |
| `fi_vil_combs_props_bonepile_04b` | 6 | `BogWitch_Camp__Hut__fi_vil_combs_props_bonepile_04b` | CHILD |
| `fi_vil_combs_props_bonepile_04d` | 8 | `BogWitch_Camp__Hut__fi_vil_combs_props_bonepile_04d` | CHILD |
| `fi_vil_shield05_a` | 5 | `fi_vil_shield05_a` | MATCH |
| `skeleton_arm` | 3 | `Pickable_ForestCryptRemains02__crypt_skeleton_pile02__skeleton_arm` | CHILD |
| `skeleton_pelvis` | 7 | `crypt_skeleton_laying__skeleton_pelvis` | CHILD |
| `skeleton_ribcage` | 11 | `Pickable_ForestCryptRemains01__crypt_skeleton_pile01__skeleton_ribcage` | CHILD |
| `stair1` | 44 | `SunkenKit_int_stair` | TRANSFORM |
| `stone_1x1` | 6 | `SunkenKit_int_wall_1x2` | TRANSFORM |
| `stone_1x1_high` | 1906 | `SunkenKit_int_wall_1x2` | TRANSFORM |
| `stone_4x2` | 3 | `SunkenKit_int_wall_2x4` | TRANSFORM |
| `stone_4x2_high` | 235 | `SunkenKit_int_wall_2x4` | TRANSFORM |
| `stonepillar` | 80 | `StonePillar` | MATCH |
| `stoneslab_lod0` | 12 | `StoneSlab` | PAIR_REVIEW |
| `stoneslab_lod1` | 12 | `StoneSlab` | PAIR_REVIEW |
| `stonewall` | 3 | `stonewall` | SCALE |
| `stonewall_1` | 403 | `stonewall_1` | SCALE |
| `widestone` | 11 | `widestone` | VARIANT |

The user's scaled-1x2 hypothesis resolves the 1x1 geometry question. Comparing only mesh
GUIDs missed this relationship. The SunkenKit 1x2 meshes reproduce the 1x1 geometry when
scaled by (0.5, 1, 1). The high-detail mesh also needs a local X translation of
+0.00973847482 after scaling; the low-detail mesh needs no translation. Both comparisons
have maximum vertex and triangle-position errors below 0.000001 local units. High-detail
meshes have 420 triangles each but different vertex counts (332 versus 331), which does
not change their matching triangle geometry. Low-detail meshes both have 12 triangles.

The candidate for these 1,912 instances is now `JVLmock_SunkenKit_int_wall_1x2` with the
placement correction above. Preserve existing room transforms and multiply the local
X scale by 0.5. Apply the high-detail offset through the existing instance transform,
not directly along world X. One native root cannot align both detail levels exactly:
aligning the high mesh leaves the low mesh/collider about 0.00974 local units offset.
Check this during replacement validation. UVs, native material appearance, collision and
runtime resolution are not established by the geometry comparison. No replacement was
performed during that comparison. The building prefab `stone_wall_1x1` is not the proposed replacement.

For the 4x2 models, `SunkenKit_int_wall_2x4` uses the same two mesh GUIDs, with children
rotated 90 degrees around Z. `SunkenKit_int_wall_4x4` also shares the mesh but doubles
its Y dimension. This is why matching only a mesh or a name is insufficient. The stairs
also have a native child rotation and nonuniform scale. Any later conversion must
preserve each mesh's world matrix, including room instances with nonuniform scale.

Other placement differences include native `stonewall`, `stonewall_1` and `altar` child
scale 1.5; `caverock_curvedrock` child X offset -1; model-only `widestone` child scale 2
and X offset -0.2; and `StartPlatform` child Y offset -0.27. The evidence separates the
8 model-only widestones from the 3 complete native widestones. Existing complete cave
rock instances already include their offset and must not receive it twice.

`CircleStone` uses a recovered mesh with a different asset GUID. Unity comparison found
identical vertices, triangle indices, UVs and normals to the native StartPlatform mesh
(320 vertices). The four bonepile types found only inside `BogWitch_Camp/Hut` have native
prop usage, but no room mesh usage was found. The coffin mesh is part of
`crypt_skeleton_laying`, which adds two other meshes; it is not an exact whole-prop swap.

The evidence's room counts mean serialized MeshFilter uses of the source mesh GUIDs.
They are not counts of prefab references. Zero does not mean an asset is absent from
the game, and this working rip does not prove provenance against an untouched rip.
Source materials under CryptRecovery can differ from native materials. The native
StonePillar material is itself named `JVLmock_stonepillar` in this working project;
its live material remains unverified. No runtime resolution, texture or collision proof
is claimed by this list. No bundles were built, no assets were swapped, and Valnet
remained off during this audit.

## Applied reviewed mappings

Applied 2,849 further replacements across all 22 room prefab assets. The earlier 538
`stone_2x1_high` replacements remain in place. Every object retains its `JVLmock_` name
and uses the reviewed native root or explicit child path. The conversion includes:

- 1,912 1x1 models to `SunkenKit_int_wall_1x2`, with local X scale halved and the verified high-detail offset.
- 238 4x2 models to `SunkenKit_int_wall_2x4`, with rotation compensation.
- 44 stair models to `SunkenKit_int_stair`, with rotation and scale compensation.
- 655 other models to the reviewed props and visual children, including walls, pillars, rocks and bones.

The [swap results](crypts-caverns-prefab-swap-results.json) contain per-room totals,
replacement counts, verification results and saved prefab hashes. Original room assets
and metadata are backed up outside the Unity project; their location is in that report.
The [material inventory](crypts-caverns-room-materials.csv) now reflects the saved assets.

Validation:

- A full dry run passed before saving. It checked current meshes and placement, and rejected transforms that could not be represented without changing shape.
- The conversion remapped 2,761 component references. Largest mesh-matrix difference was 0.000002862.
- Reloading the saved prefabs verified all 2,849 placements, native meshes, material assignments, collider counts and native prefab connections where applicable.
- Per-room mock counts exactly match the expected renames; total remains 3,491. Serialized local object references have no missing targets.
- A repeated dry run proposed zero further changes.
- The Crypts & Caverns authoring scene is open, reflects the new prefab contents and has no unsaved changes. Its scene file was not edited.
- The backups contain the same 33 empty LOD renderer entries as the saved rooms, including entries inherited from the existing skeleton props. The swap introduced none; these existing entries remain for separate review.

The unresolved 4 `fi_vil_combs_props_bone_coffin`, 12 `stoneslab_lod0` and 12
`stoneslab_lod1` instances remain unchanged. A full skeleton adds geometry to the coffin
model; the slab LOD pairs require placement review before combining them into `StoneSlab`.
No bundles or DLLs were built or deployed for this swap. Valnet remains off. Correct
native material assignments in Unity are not proof of runtime mock resolution, shader
rendering, collision or traversal; those checks remain required before release.

### Conversion tools

`tools/unity/replace-crypts-reviewed-prefabs.cs` runs through official Unity Pipeline
`eval_file`. Set `/tmp/mwl-prefab-swap-config.json` to an object with `apply` (boolean),
`mapping` (absolute path to the mapping evidence JSON), and `output` (a new absolute
evidence directory). Run with `apply=false` first, then `apply=true` using the same output
directory. Apply requires unchanged preflight input hashes, backs up every room and
restores saved rooms if the conversion fails. It rejects Play mode, Prefab mode and
unsaved scenes. Whole native props stay connected to their prefab sources; child mocks
copy the selected native subtree and retain its explicit runtime lookup path.

`tools/unity/verify-crypts-reviewed-prefabs.cs` uses that same configuration to reload and
check the saved results. It briefly imports uniquely named copies of the backups to
compare inherited LOD entries, then removes those temporary assets in a `finally` block.
