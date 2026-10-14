# ShrineEvent1 development bundle

This is a single-bundle development build, not a production release artifact.

- Source project: `/Users/benjmarston/Develop/valheimRipCombatArms/valheimRipCombat`
- Source prefab: `Assets/WarpProjects/Dungeons/MWL_ShrineEvent1.prefab`
- Bundle: `mwl_shrineevent1`
- Unity: `6000.0.46f1`
- Builder: modified AssetBundleBrowser, single-bundle mode
- Platform: StandaloneWindows; compression: LZMA
- External bundle dependencies: none
- SHA-256: `7b2eb7c67c8160c6cb094aa869042eebaeca1a4c5fa2a767d9b8bdb6d2c85dee`

For a development installation, place `assetBundleManifest_mwl_shrineevent1`
beside the candidate MWL DLL. Place `Bundles/mwl_shrineevent1` and its Unity
`.manifest` in the existing MWL `Bundles` directory. Keep the existing full
manifest and other bundles. The optional manifest uses the prefab path's stable
hash, matching Jotunn's `GenerateAssetID(string)`.

The plugin loads the additional manifest and registers the location only when
`MidnightsFX.ValheimFortress` is loaded. Fortress is a soft dependency so it loads
first when present, but is not required for other MWL locations. The shrine mock
is `JVLmock_VF_wild_shrine_yellow1`.

If `BuildCombinedManifest` regenerates the full manifest with ShrineEvent1,
the plugin skips its separate manifest. This prevents duplicate asset IDs in
SoftRef, which otherwise stop scene loading. Disable generation again after
the first launch, then restart the game to use the generated manifest.

Mock object names must match their source prefab names exactly. The bundle
removes Unity copy suffixes such as ` (1)` from 386 stoneblock mock objects;
those suffixes prevented Jotunn from resolving the walls and floor.

After mock resolution, MWL replaces the embedded shrine's `WildShrine` and
`Spawner` behaviours with `FortressTributeShrine`. Its appearance, colliders,
and effects remain. This callback only applies to ShrineEvent1.

Fortress 0.39.0 or newer is required for the offering interaction. The copied
`Src/Integrations/FortressAPI.cs` comes from v0.39.0 (schema 3). The component
looks up `VF_wild_shrine_yellow1` / the offered item prefab from Fortress's currently loaded
wild-shrine configuration, then runs that definition with the location's own
spawn points. It does not duplicate the event definition or parse Fortress YAML.

The payment is fixed at 50 of the selected item because the API does not expose tribute
amounts. Normal interaction selects `Resin` (Black Forest). Using a `Resin`
or `Guck` inventory item selects the matching Fortress entry; `Guck` selects
the Swamp test. Both entries must configure a tribute amount of 50. The
component checks inventory and the event definition, validates the floor below
each spawn marker, and consumes only the selected item across stacks after a
successful API start. Failed checks leave the inventory unchanged. Both event
types share the same reservation and active-runner lock. Other tribute items
are rejected. Wave definitions, creature stats, modifiers and rewards remain in
Fortress configuration; no Unity bundle rebuild is needed to change them.

`Interior/ShrineEventSpawnPoints` contains four markers, 20 metres from the
centre. Floor casts are limited to six metres around each marker. They never
search down to exterior terrain. Pillars and blue torches use exact mock names.
The Location component and the interior environment zone both use `Interior`,
so they do not repeatedly override each other's environment.

The location's network owner grants control for an offering. The component
stores the API runner's network ID on the location, preventing another offering
while that runner exists, including while its GameObject is unloaded. Fortress
0.39 returns only a success flag, so MWL identifies the runner created by the
synchronous API call. This depends on the `VF_api_challenge_runner` prefab name.
The offering unlocks when Fortress removes that runner after completion.

Placement and quantity match Minddrip Hallow: Plains, `PlainsInterior` group,
quantity 15, minimum distance 500, minimum altitude 8, maximum terrain delta 4,
and minimum distance from similar locations 1024. The exterior remains a copied
Minddrip placeholder.

The bundle was loaded in the Editor to check its exported prefab, four spawn
markers, mock names and reciprocal Teleport references. Automated checks cover
location registration with Fortress present and absent using substitutes for
game APIs. Live API validation is recorded in the feature pull request.

On Valnet01 with the Season 8 production mod set (8.0.42) and Fortress 0.39.0,
the Resin definition completed all four phases in the interior. Spawn points
were at Y=5048.64 and creatures stood on the floor at approximately Y=5048.44.
The configured 39-star troll appeared in phase four. Completion produced three
each of DustRare, ReagentRare and EssenceRare beside the shrine. An offering
of 49 resin did not start the event; 50 resin started it and was consumed once.
A second offering during the event consumed nothing. After completion, another
50-resin offering started a new event. Test damage commands advanced the waves;
this check does not assess combat balance. Multiplayer ownership transfer and
reconnection during a challenge have not been validated.

Before production packaging, rebuild all bundles together and use the resulting
dependency manifests. Update this location's soft-reference manifest to match
that build's dependencies; do not ship this single-bundle development output.

The Resin/Guck selection change was tested on Valnet01 with the Season 8
8.0.43 mod set and the same development bundle. The original DLL rejected
Guck without payment. The updated DLL rejected Wood and insufficient Guck,
started Swamp for exactly 50 Guck while preserving Resin, and blocked both
offering types during the event without further payment. Swamp completed four
phases, with the Oozer in the finale, and produced rewards in the room. Normal
interaction then started Black Forest for 50 Resin while preserving Guck.
Tests used the real `UseItem` and `Interact` methods through valheimCLI.
Damage commands advanced combat; this is interaction and progression proof,
not combat-balance proof. A Wraith received the random SLS Fast modifier;
future balance measurements must account for random SLS modifiers as well as
the explicitly configured Fortress rules. Evidence is in
`valheim-validation-evidence/shrine-tribute-20261014` beside the repositories.
