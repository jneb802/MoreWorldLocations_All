# Server-only startup and recovery

The server verifies its location catalogue before loading a world. Loading
starts only after the approved locations have been registered successfully.
World saves are blocked until that initial load succeeds.

Every defined template in the supported packs is evaluated after its assets
are resolved, or its verdict is reused from a previous start with the same
inputs (see below). All templates that pass the current validator are eligible;
there is no separate approval list. Blocked,
unresolved and excluded templates remain out, with their reasons available
through `mwl_catalogue` and `mwl_location <name>`. A compatible verdict is a
capability check, not a claim that every location has been play-tested.

For station testing, the legacy `MOREWORLDLOCATIONS_APPROVE` environment
variable is now only a comma-separated subset filter. It cannot override a
failed verdict. Leave it unset to include all compatible templates. Use a
subset only on disposable test worlds, and keep that subset unchanged when
reloading a test save; omitted definitions cannot resolve saved locations.

Use `mwl_memory` in the server console to see audit progress, registration
readiness, and world-load status. If verification or registration fails, the
server keeps the world unloaded and reports the cause. Correct that cause and
run `mwl_memory resweep` to retry verification and registration.

If world loading itself fails, saving remains blocked. Correct the cause and
restart the server process. Returning to the menu, changing scenes, or running
`mwl_memory resweep` cannot clear that failure. The original reason remains in
`mwl_memory`; loading, generation and saving stay blocked until restart.

Once registration has succeeded, `mwl_memory resweep` is diagnostic: it reports
a fresh audit without changing the current registration or loading the world
again. A diagnostic failure leaves the existing world in place. Applying a
different selection of locations requires a new server session.

## Reusing verdicts across restarts

Assistant-written section.

Auditing every template takes minutes on a dedicated server. After a start
whose audit completed, the server stores the verdicts, and the next start
reuses them without opening any template, but only when nothing they depend
on has changed. Otherwise it runs the full audit, exactly as it would with no
stored verdicts, and replaces them.

The stored verdicts are reused only when all of the following are unchanged:

* the cache format, the validator's rule and content versions, and the rules'
  digest (stock snapshot build, forgiven components, excluded packs);
* the names to judge, in order, with their packs and interiors, and the
  `MOREWORLDLOCATIONS_APPROVE` subset;
* every file in MWL's own plugin folder (DLL, `Bundles/`, manifest), and in
  the bundle manifest's folder if the two differ;
* MWL's own settings under `BepInEx/config`: its `.cfg` and its
  `warpalicious.More_World_Locations_*` YAML files;
* the game's `assembly_valheim.dll`, its version text, network version, and
  whether it runs headless;
* the loaded Jötunn DLL and every file in `BepInEx/core`;
* the names of the other loaded plugins, which the verdicts cite;
* every `MOREWORLDLOCATIONS_*` environment variable except the two below;
* every live stock prefab the verdicts were reached against. The file records
  each name the audit compared against, resolved a mock from, or found
  referenced. A name in the stock prefab snapshot is recorded with a digest
  of the prefab the comparison uses for it. It is checked again before reuse,
  so a prefab another mod edited, removed or added under that name causes a
  miss, and the log names it. Any other name is recorded as `not-stock` and
  never examined. Such names are not stock prefabs: a lookup finds whatever
  object of that name happens to be loaded, such as a snap point or a piece
  of an open template, and that changes as templates load and unload.

Other plugins' DLLs and settings are not part of the key. Updating or
reconfiguring a plugin that touches none of those prefabs does not force
a new audit.

The file is `BepInEx/cache/MoreWorldLocations/server-only-audit.txt`. It is
plain text, checksummed, and written atomically. Anything wrong with it
causes a miss. The log records the outcome in one line:
`catalogue audit: reused N verdicts from …` or
`catalogue audit: not reusing stored verdicts (<reason>)`. The reason names
the key parts or prefabs that changed. Verdicts are not stored while any
template is unresolved, because an unresolved verdict records what one
start could not see.

Two environment variables control the cache:

| Variable | Effect |
| --- | --- |
| `MOREWORLDLOCATIONS_AUDIT_CACHE=off` | Never read or write stored verdicts (`0` and `false` work too; an unrecognised value also counts as off). |
| `MOREWORLDLOCATIONS_AUDIT_CACHE_PATH=<file>` | Keep the file somewhere else. |

`mwl_memory resweep` never uses stored verdicts. A diagnostic resweep
neither reads nor writes them. A resweep that retries a failed registration
runs the full audit. To force a full audit at the next start, delete the file
or set the switch to `off`.

These guards apply to server-only mode. Vanilla's own save-error checks remain
in effect.
