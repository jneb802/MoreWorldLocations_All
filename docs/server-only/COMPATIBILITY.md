# Template compatibility

Every MWL 5.0.9 location template, measured against the four predicates in
[`SPAWN-PATH.md`](SPAWN-PATH.md): which of its parts a client without the mod
actually receives. Assistant-written.

**Nothing here is an approval.** `ServerOnlyAllowlist.Approved` is empty and
stays empty until a template has been through the runtime dump and the one-site
experiment as well. This is the audit's structural half — necessary, checkable
from the assets, and not sufficient.

## What was read

The content is not in this repository (see [`BASELINE.md`](BASELINE.md)); it
ships only in the Thunderstore package. Read from
`warpalicious/More_World_Locations_AIO` **5.0.9**
(`b7790876e86eb161d9e4b123e92e396ee167f5e44cea620b6ebe28bdfde8f3c5`, published
9 Sep 2026), which matches the pinned source commit's version, unpacked to the
validation station:

* 264 bundles under `plugins/Bundles`, one per prefab, indexed by
  `assetBundleManifest_full` (`879003ecd4e9…`).
* 193 of them contain a GameObject with a `Location` component. The rest are
  dungeon rooms and other non-location prefabs.
* `mwl_ruins1` `c465607de57e…`, `mwl_ruinswell1` `649d9436d598…`.

Measured by an offline auditor that walks the prefab hierarchy straight out of
the bundle. It lives with the validation tooling rather than in this repository,
since building the mod does not need it.

## Result

| verdict | templates | meaning |
| --- | --- | --- |
| candidate | 16 | nothing found: every part is a networked child |
| needs conversion | 169 | terrain, or a scale whose fate the bundle cannot settle |
| blocked | 8 | something a player would notice is in the proxy half |

| finding | count |
| --- | --- |
| `terrain_in_proxy_half` | 220 across 166 templates |
| `mock_scale_unresolved` | 352 |
| `structure_in_proxy_half` | 16 across 7 templates |
| `inactive_root` | 1 |

**The blocker the plan's review expected is almost absent.** The prediction was
that hand-authored scenery — walls and floors that are plain meshes — would
strand most templates. Across 193 templates only 16 objects in 7 templates are
non-networked structure, and they are named things rather than whole buildings:
`Gateway`, `ExteriorGateway` and `Cube` in the two dungeon exteriors and
`MWL_MinddripHallow1`, and a `Property Area` marker in four Mistlands
templates. These are blueprint-built structures made of the game's own pieces.

### 1. Terrain — the real work

220 `TerrainModifier` children across 166 templates, and **every one of them has
`m_useTerrainCompiler = false`** and no `ZNetView`. `Heightmap.ApplyModifiers`
reads live instances, so this shaping exists only on a machine that has the
template: a stock client stands on unshaped ground under a structure that was
placed for shaped ground.

What they do: 202 level (radius 1–42 m, mostly 3–6), 182 smooth, 127 paint (118
`Dirt`, 5 `Cultivate`, 3 `ClearVegetation`, 1 `Paved`). The conversion is to
apply the same operation on the server through a persistent `TerrainComp`, which
is the route ProceduralRoads already writes roads through.

### 2. Mock scale — cannot be settled from the bundle

Most children are Jötunn mocks. `MockManager.ReplaceMockGameObject`
**instantiates the real prefab under the same parent, copies the mock's
position, rotation, scale and `activeSelf` onto it, and destroys the mock.** So
a mock's own `ZNetView` fields are not the ones that reach the world, and its
children never exist.

That matters twice. It is why an earlier pass of this audit was wrong — it read
view-less mocks such as `JVLmock_Bush01` as lost scenery when they resolve to
prefabs that do carry a view — and it is why 352 scaled mocks are reported as
unresolved rather than as failures: whether a scaled piece survives to a stock
client depends on the **real** prefab's `m_syncInitialScale`, which this bundle
does not contain. Settling those needs a runtime dump or the game's own assets.

### 3. Prefabs the stock client may not have

66 of 193 templates emit at least one mod-local prefab. The recurring ones:

| name | instances | what it is |
| --- | --- | --- |
| `MD_Kit_widestone` | 135 | a `widestone` clone with `Destructible` removed |
| `MWL_Shrine` | 38 | shrine ward, excluded by the plan |
| `MWL_Waystone` | 14 | excluded by the plan |
| `MWL_*_Vendor`, `MWL_*_Trainer`, `MWL_*_Runestone1` | 1 each | ports and traders, excluded packs |
| `MWL_SwampChurch1_Spawner1..11` | 1 each | custom creature spawners |

The 127 remaining templates emit only names that look vanilla. *Look* is the
operative word: this cannot be confirmed until a stock prefab registry is
captured at the pinned game build, which is the outstanding Step 0 item. All
`Spawner_Draugr`-style names are vanilla spawners, not MWL's.

### 4. Loot arrives, but not MWL's loot

The starter candidates carry `TreasureChest_meadows` — a **vanilla** chest, not
one of MWL's own. That is better than expected and does not solve the problem.
`LootDB.SetupLoot` writes MWL's drop table into the resolved object's
`Container` **component fields**, which are not ZDO state. A stock client that
becomes the chest's owner runs `Container.Awake` → `AddDefaultItems` against the
**vanilla** `TreasureChest_meadows` table. So the chest is there and openable,
and it holds vanilla meadows loot instead of MWL's — a silent content
difference, not a crash. The fix is the one the plan names: fill the inventory
on the server and set `ZDOVars.s_addedDefaultItems` before any client can own it.

### 5. One template spawns nothing

`MWL_StoneBeacon1`'s root GameObject is inactive in the bundle. Vanilla's
`Utils.IsEnabledInheirarcy` checks the root as well as the children, so as read
here Ghost mode would instantiate none of its 60 objects. Either the runtime
activates the loaded asset before `SpawnLocation` sees it, or this template
places an empty site on any server, modded or not. The dump will say which; it
is the only template of the 193 like this.

## The starter set

Ten templates have **no findings at all and emit no mod-local prefab**:

`MWL_AshWallPost1`, `MWL_MarbleCage1`, `MWL_MeadowsTomb4`, `MWL_RuinsTower8`,
`MWL_StoneCastle1`, `MWL_SwampHouse1`, `MWL_SwampRuin1`, `MWL_TreeTowers1`,
`MWL_WoodTower1`, `MWL_WoodTower2`.

A further 78 need only the terrain conversion.

For the first milestone the plan wants one small Meadows site made of vanilla
pieces. Two fit, and both need only the terrain conversion:

| | `MWL_Ruins1` | `MWL_RuinsWell1` |
| --- | --- | --- |
| networked children | 52 | 62 |
| proxy half | 4 objects | 3 objects |
| prefabs | `stone_wall_1x1` ×24, `stone_wall_2x1` ×16, `Bush01` ×6, `Pickable_Mushroom` ×3, `FirTree_oldLog`, `RaspberryBush`, `TreasureChest_meadows` | `wood_beam` ×25, `wood_floor` ×20, `stone_floor` ×9, `vines` ×7, `TreasureChest_meadows` |
| mod-local prefabs | none | none |
| terrain | one modifier: level r=3 square, smooth r=3 power 4, paint Dirt r=3 | one modifier: level r=4 round, offset −2, no smooth, no paint |
| creature spawners | none | none |

`MWL_RuinsWell1` is the narrower experiment: one operation, no paint, no
vegetation, and a −2 m offset that makes the terrain change unmistakable when it
is missing. `MWL_Ruins1` exercises paint and smoothing as well.

## Still unknown

* Every name's presence in the stock registry. No registry captured.
* The real prefabs' `m_syncInitialScale` for the 352 scaled mocks.
* Whether the runtime hierarchy after soft-reference and mock resolution matches
  what the bundle says. **The runtime dump is what makes this audit
  authoritative**; this pass narrows what it has to look at.
* Everything in game: collision, loot, ownership transfer, restart, and whether
  converted terrain looks like the terrain the structure was authored for.
