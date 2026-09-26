# Server-only diagnostics

Three things an operator or a tester can ask of a server-only server. None
of them changes what is generated; unset, none of them runs.

## Terrain readiness is paced, not counted per call

A location whose neighbouring ground is not built yet is held and tried
again. The hold budget is 30 checks, and since the fix in this branch a
check is charged **at most once per second per zone**, however often the
game polls. Fast polling previously spent all 30 within a few frames and
gave a placement up before the asynchronous terrain builder could answer.
Readiness itself is still tested on every call, so a zone recovers the
moment its ground is readable. Zones waiting on the height budget are never
charged. `mwl_terrain` lists zones currently held and placements given up.

## `mwl_plan <name> [skip] [take]` prints the template's terrain operations

Ahead of the paged child rows, one comment row per terrain operation:

```
# terrain <path> <x> <y> <z> level=0|1 <levelRadius> <levelOffset> <square> smooth=0|1 <smoothRadius> <smoothPower> paint=0|1 <paintRadius> <paintStrength> <paintType> <paintHeightCheck> enabled=0|1
```

Positions are relative to the template root, the frame vanilla spawns in.
The rows repeat on every page. A terrain check on a client samples the
ground at a level operation's centre and compares it with
`site y + y + levelOffset`; the writer's own completion status is not
evidence of the height a client sees.

## `MOREWORLDLOCATIONS_EMISSION_TRACE=<names|*>` records selection, emission and destruction

Set to a comma-separated list of exact template names, or `*` for every
template, before starting the server. The server then writes one
`[EMISSION]` line per event to the BepInEx log, tab separated, each carrying
the site's identity (definition, position, seed):

| row | after the identity | recorded at |
|---|---|---|
| `S` | mode, template fingerprint, networked objects considered | `ZoneSystem.SpawnLocation` begins |
| `R` | component, randomiser path, spawned 0/1 | each `RandomSpawn`/`RandomObject.Randomize` returns |
| `C` | randomiser path, member path, prefab, active 0/1, position relative to the root | same, one per networked object under the randomiser (itself included) |
| `E` | member path or `?`, prefab, ZDO id, world position | the emitted object's `ZNetView.Awake`, matched to a member by prefab and predicted position |
| `Z` | emitted count | `SpawnLocation` returns |
| `X` | member path, prefab, ZDO id, who (`peer <id>`, `this server, as owner`, `handled locally`) | `ZDOMan.DestroyZDO` / `HandleDestroyedZDO` for a traced ZDO |

What it answers: whether a missing object was left out by a randomiser, never
created, or created and destroyed later, and by which peer. What it does not
answer: why a peer destroyed it (collapse, damage or anything else is not
recorded), and anything after the emitting process ends, because the watched
set is not persisted. A member's `C` state is its state after THAT randomiser
ran; a member under two randomisers is off at emission if either turned it
off. The one `E` row with path `?` at every site is the `LocationProxy`.

Every hook body is guarded: a failing trace is logged and the spawn proceeds
untouched. Tracing every template on a busy world writes a large log.
