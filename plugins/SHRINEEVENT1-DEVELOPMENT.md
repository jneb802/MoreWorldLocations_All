# ShrineEvent1 development bundle

This is a single-bundle development build, not a production release artifact.

- Source project: `/Users/benjmarston/Develop/valheimRipCombatArms/valheimRipCombat`
- Source prefab: `Assets/WarpProjects/Dungeons/MWL_ShrineEvent1.prefab`
- Bundle: `mwl_shrineevent1`
- Unity: `6000.0.46f1`
- Builder: modified AssetBundleBrowser, single-bundle mode
- Platform: StandaloneWindows; compression: LZMA
- External bundle dependencies: none
- SHA-256: `e73202ae40d4326f6fb93346c52a31eeb931c7667030b3d2f074ab6593a21f8f`

For a development installation, place `assetBundleManifest_mwl_shrineevent1`
beside the candidate MWL DLL. Place `Bundles/mwl_shrineevent1` and its Unity
`.manifest` in the existing MWL `Bundles` directory. Keep the existing full
manifest and other bundles. The optional manifest uses the prefab path's stable
hash, matching Jotunn's `GenerateAssetID(string)`.

The plugin loads the additional manifest and registers the location only when
`MidnightsFX.ValheimFortress` is loaded. Fortress is a soft dependency so it loads
first when present, but is not required for other MWL locations. The shrine mock
is `JVLmock_VF_wild_shrine_yellow1`.

Placement and quantity match Minddrip Hallow: Plains, `PlainsInterior` group,
quantity 15, minimum distance 500, minimum altitude 8, maximum terrain delta 4,
and minimum distance from similar locations 1024. The exterior remains a copied
Minddrip placeholder.

The bundle was loaded in the Editor to check its exported prefab and reciprocal
Teleport references. Automated checks cover Fortress present and absent using
substitutes for game APIs. Game startup, mock resolution, world generation, and
teleport travel still require validation with the current Praetoris mod set.

Before production packaging, rebuild all bundles together and use the resulting
dependency manifests. Update this location's soft-reference manifest to match
that build's dependencies; do not ship this single-bundle development output.
