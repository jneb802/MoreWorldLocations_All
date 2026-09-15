# Template compatibility — preliminary

> **This is not the audit.** Every number below was read from
> `More World Locations_AIO/assets/moreworldlocations_assetbundle_1`
> (`29154ce12dbdabac17edd59bf0919f70f404ebc44742505cad52929ada1f8fdc`), a
> **legacy** bundle that ships in the repository and that no source file loads.
> Its asset paths (`.../MeadowsPack1/mwl_ruins1.prefab`) are not the ones
> `LocationDefinitions.cs` asks for (`.../Meadows/MWL_Ruins1.prefab`), so it is
> an older build of the same templates. It is used here because the 5.0.9
> content bundles are not in the repository at all (see
> [`BASELINE.md`](BASELINE.md)).
>
> Treat this as **the shape of the problem, measured**, not as an approval of
> any template. `ServerOnlyAllowlist.Approved` is empty and stays empty until
> the audit runs against the pinned 5.0.9 content with a captured stock prefab
> registry.

Assistant-written. Method and the source reading behind it:
[`SPAWN-PATH.md`](SPAWN-PATH.md). Measured by an offline auditor that reads the
prefab hierarchy straight out of the asset bundle and applies the four
predicates listed there; it lives with the validation tooling rather than in
this repository, since building the mod does not need it.

## What was measured

For each template, its children split into the half a stock client receives (an
enabled `ZNetView`, active up to the root) and the half only a client holding
the template can build. The walk stops at each networked child, because
everything below it belongs to that vanilla prefab and arrives with it.

| template | networked | proxy half | terrain mods | custom chests | spawners | other findings |
| --- | --- | --- | --- | --- | --- | --- |
| `MWL_Ruins1` | 52 | 4 | 1 | 1 | 0 | — |
| `MWL_Ruins2` | 135 | 3 | 1 | 1 | 3 | — |
| `MWL_Ruins3` | 52 | 3 | 1 | 1 | 2 | — |
| `MWL_Ruins6` | 287 | 4 | 1 | 1 | 3 | — |
| `MWL_Ruins7` | 103 | 4 | 1 | 1 | 3 | — |
| `MWL_Ruins8` | 244 | 4 | 1 | 1 | 3 | — |
| `MWL_RuinsArena1` | 447 | 7 | 4 | 1 | 5 | — |
| `MWL_RuinsArena2` | 505 | 5 | 2 | 2 | 10 | — |
| `MWL_RuinsArena3` | 161 | 4 | 1 | 1 | 4 | — |
| `MWL_RuinsCastle1` | 126 | 3 | 1 | 1 | 3 | — |
| `MWL_RuinsCastle3` | 811 | 3 | 1 | 3 | 5 | — |
| `MWL_RuinsChurch1` | 272 | 3 | 1 | 1 | 3 | — |
| `MWL_RuinsTower3` | 136 | 3 | 1 | 1 | 3 | — |
| `MWL_RuinsTower6` | 420 | 3 | 0 | 3 | 10 | — |
| `MWL_RuinsTower8` | 150 | 3 | 0 | 1 | 2 | — |
| `MWL_RuinsWell1` | 62 | 3 | 1 | 1 | 0 | — |
| `MWL_Tavern1` | 130 | 5 | 2 | 1 | 3 | scale_not_synced ×4 |
| `MWL_WoodTower1` | 95 | 2 | 0 | 1 | 5 | — |
| `MWL_WoodTower2` | 42 | 2 | 0 | 1 | 3 | — |
| `MWL_WoodTower3` | 254 | 3 | 0 | 1 | 5 | — |

## The result

**The first blocker the plan's review expected — static scenery stranded in the
proxy half — is not there.** In all twenty templates the proxy half is two to
seven objects: the root carrying `Location`, empty grouping transforms named
`Blueprint` and `Vegetation`, and the terrain modifiers. Not one carries a
renderer or a collider. These are blueprint-built structures, and they are
built out of networked pieces.

What does stand between them and a stock client is three named things, each with
a known conversion:

1. **Terrain.** Sixteen of twenty carry one to four `TerrainModifier` children
   with no `ZNetView` and `m_useTerrainCompiler = false` — a level of radius 3–4
   m, usually with a smooth and a `Dirt` paint. `Heightmap.ApplyModifiers` reads
   live instances, so a stock client stands on unshaped ground. The conversion
   is to apply the same operation on the server through a persistent
   `TerrainComp`, which is the route ProceduralRoads already writes roads
   through. Four templates carry no modifier at all and need nothing here.

2. **Chests.** Every template contains one to three `MWL_<name>_loot_chest_*`
   prefabs — MWL's own, registered by `Prefabs.cs`, not names a stock client can
   resolve. This is the plan's named conversion: emit a vanilla chest, fill it
   on the server, and set `ZDOVars.s_addedDefaultItems` before any client can
   own it, or vanilla's `Container.Awake` will roll the *vanilla* prefab's table
   on the first owner.

3. **Creature spawners.** `MWL_<name>_Spawner*`, likewise custom, two to ten per
   template. Out of scope for the first release by the plan's own boundary, so
   the first-release conversion is to omit them.

Everything else resolves to a vanilla piece: `stone_wall_1x1`, `wood_beam`,
`wood_floor`, `stone_floor`, `vines`, `Bush01`, `Pickable_Mushroom` and so on.
They appear in the bundle behind Jötunn's `JVLmock_` placeholders, which
`CustomLocation(…, fixReference: true)` swaps for the real prefabs at load, so
the ZDO the server writes carries the resolved name. The auditor resolves the
prefix before judging; that is also why these names cannot be confirmed against
a stock registry until one is captured.

`MWL_Tavern1` additionally has four `Greydwarf_Root` children scaled without
`ZNetView.m_syncInitialScale`: a stock client would rebuild them at scale 1.

## Candidates for the first milestone

On this evidence — and subject to re-running against 5.0.9 content —
`MWL_Ruins1` (52 pieces, one terrain modifier, one chest, no spawners) and
`MWL_RuinsWell1` (62 pieces, one terrain modifier, one chest, no spawners) are
the two smallest templates with nothing else to convert. They are what the
plan's review names as the one-site experiment, and the audit agrees with that
guess for reasons rather than by name.

## Still unknown

* Whether 5.0.9's versions of these templates have the same shape. **They must
  be re-read; this table cannot be carried over.**
* Whether every resolved name exists in the stock client's registry at the
  pinned game build. No registry has been captured.
* Everything runtime: collision, loot, ownership transfer, restart, and whether
  the converted terrain actually looks like the terrain the structure was
  authored for.
