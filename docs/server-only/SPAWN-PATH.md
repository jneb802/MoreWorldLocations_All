# What a client without the mod receives

Read from the decompiled Valheim 1.0 assemblies recorded in
[`BASELINE.md`](BASELINE.md) (`assembly_valheim.dll` `bb32454276…`,
`assembly_utils.dll` `e5fb9f228c…`). Source-only: every statement below is a
reading of the shipped IL, and none of it has been confirmed against a running
server yet. Assistant-written.

This is the mechanism the whole server-only question turns on, so it is written
down once, here, rather than re-derived per experiment.

## A location is spawned in two halves

`ZoneSystem.SpawnLocation(location, seed, pos, rot, mode, …)` branches on the
mode. A dedicated server generating a zone for a peer uses `SpawnMode.Ghost`
(`SpawnZone` → `PlaceLocations`); a client rebuilding a location it already has
a proxy for uses `SpawnMode.Client`.

### Ghost / Full — what the server writes

1. `Utils.GetEnabledComponentsInChildren<ZNetView>(prefab)` collects every
   `ZNetView` below the root whose own `activeSelf` and every ancestor's
   `activeSelf` up to the root are true (`Utils.IsEnabledInheirarcy`).
2. `RandomSpawn.Randomize` and `RandomObject.Randomize` run under
   `Random.InitState(seed)`, which can only **switch children off**
   (`RandomSpawn.SetSpawned` deactivates; `Reset` turns everything back on
   afterwards).
3. Each collected view whose `gameObject.activeSelf` is still true is
   `Instantiate`d inside `StartGhostInit`/`FinishGhostInit`, so each becomes its
   own **persistent ZDO**. Its network identity is
   `ZNetView.GetPrefabName().GetStableHashCode()` — the **GameObject's name**
   (`ZNetView.Awake`).
4. `WearNTear.m_randomInitialDamage` is set from `Location.m_applyRandomDamage`
   for the duration, so initial damage is rolled on the server and persists.
5. `SnapToGround.SnappAll()` runs, so snapped positions are the ones saved.
6. `CreateLocationProxy` adds one more ZDO: a `LocationProxy` carrying
   `s_location` (the location name's hash) and `s_seed`.

Because the view array is captured **before** randomising, while the hierarchy
is in its default all-active state, the set of objects any seed can produce is a
subset of one fixed set. That is what makes an offline audit of the template
sound: no seed can introduce a child the audit did not see.

### Client — what only a client with the template builds

The `else` branch deactivates every networked child and then instantiates the
**whole prefab**. So the client half is precisely *the location minus its
networked children*: static meshes, colliders with no view, `TerrainModifier`
children with no view, `EffectArea`, and the `Location` component's own
behaviour.

## A stock client meeting an unknown location

`LocationProxy.Awake` → `SpawnLocation()`:

* `ZoneSystem.ShouldDelayProxyLocationSpawning(hash)` finds no `ZoneLocation`
  for the hash, logs `Missing location:<hash>` and returns **false** — so the
  proxy does **not** enter the retry branch and `m_locationNeedsSpawn` stays
  false. `Update` does nothing afterwards; there is no per-frame loop.
* `ZoneSystem.SpawnProxyLocation(hash, …)` logs `Missing location:<hash>` a
  second time and returns null.
* `SpawnLocation()` returns false. Nothing local is created, nothing throws,
  the peer is not disconnected.

**Expected warning budget: exactly two `Missing location:<hash>` lines per
location proxy per client session.** That is a new warning class the acceptance
gate has to account for rather than tolerate silently, and it is the cheapest
signal that a site actually reached the client.

So a stock client sees a server-placed MWL location as *its networked children
and nothing else*.

## The consequence that decides the first release

The audit rule follows directly: **a template can be served to stock clients
only if everything a player needs is a networked child whose prefab name the
stock client already has.** Necessary, checkable from the asset — and not
sufficient: loot, collision, ownership transfer and restart still have to be
proven in game.

Four predicates fall out of the same reading, and the offline auditor checks all
four:

| predicate | why | where |
| --- | --- | --- |
| the child's name resolves in the stock registry | the ZDO's prefab is the **name**, hashed | `ZNetView.Awake` |
| enabled `ZNetView`, active up to the root | otherwise Ghost never instantiates it | `Utils.GetEnabledComponentsInChildren` |
| `m_persistent` is true | a non-persistent ZDO is not saved, so the object is gone after a restart | `ZNetView.Awake` |
| `m_syncInitialScale` is true when the child is scaled | otherwise the client rebuilds it at scale 1 | `ZNetView.Awake` |

## Terrain is the part that does not come for free

`Heightmap.ApplyModifiers()` walks the **live `TerrainModifier` instances in the
scene** and applies level/smooth/paint to the heightmap, then separately applies
whatever a `TerrainComp` holds. The first of those is not persisted at all: it is
re-derived on every machine from modifier objects that are present.

A location's terrain modifiers are in the proxy half. A client with the template
instantiates them and gets the shaped ground; a stock client does not, and
stands on unshaped terrain under a structure that was placed for shaped terrain.

The persistent half, `TerrainComp`, is the supported route — the same one
ProceduralRoads writes its roads through. Converting a template's modifiers into
a `TerrainComp` operation on the server is therefore the conversion the first
milestone needs, and it is a small one: the sampled templates carry one to four
modifiers, each a level/smooth/paint of 2–4 m radius with
`m_useTerrainCompiler = false`.

## Clearing comes for free

`PlaceLocations` adds a `ClearArea(position, exteriorRadius)` for a location with
`m_clearArea`, and `PlaceVegetation` — which runs in the same `SpawnZone` call,
in the same mode — skips vegetation inside it. In Ghost mode the vegetation is
written as ZDOs too, so a ghost-generated zone already has the clearing baked
into what the client receives. No extra work, and no re-clearing on later
visits.

One limit: `m_tempClearAreas` is per zone. A location whose exterior radius
spills into the next zone does not suppress that zone's vegetation. That is
vanilla behaviour, not something this mode introduces, but it is a thing to look
at for any site near a zone boundary.

## Pre-baking has to generate, not wait

`ZoneSystem.SpawnZone` refuses to generate until
`HeightmapBuilder.IsTerrainReady` says the zone's terrain has been built, and
returns false without generating. A pre-bake loop therefore has to call
`SpawnZone(zone, SpawnMode.Ghost, out _)` per zone from a bounded, frame-sliced
loop and retry the refusals — it cannot assume one pass is enough.

It also cannot rely on the server's own live zones. Measured on ProceduralRoads:
a dedicated server parks `m_referencePosition` at the sentinel
`1000000,0,1000000` (zone 15625,15625), so its live zones never coincide with a
site.
