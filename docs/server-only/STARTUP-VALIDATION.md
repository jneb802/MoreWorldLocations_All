# Startup failure validation

For disposable station worlds only. The environment variable
`MOREWORLDLOCATIONS_FAULT_STARTUP_ONCE` is read once per process and accepts:

| Value | Injection point |
| --- | --- |
| unset or empty | No fault; ordinary startup. |
| `registration` | Throws after the first successful late-registration callback. The named definition has already been inserted; the initial registration transaction remains incomplete. |
| `load-throw` | Throws from the world-load prefix after the gate enters `Loading`. The ordinary Harmony finalizer receives the exception. |
| `load-error` | Sets the real `ZNet.m_loadError` flag and skips the load body after the gate enters `Loading`. The ordinary finalizer receives a normal return with the error flag set. |

Each selected fault fires once per process, even if its announcement cannot be
logged. A resweep does not rearm it. Unknown values throw rather than silently
producing a healthy validation run. Full mode does not consult this switch.

The load faults deliberately avoid reading or corrupting a world file. They
exercise the patched invocation and finalizer, not a real corrupt-file decoder
or a partially completed vanilla load. Do not describe them as proving those
behaviours.

## One bounded station campaign

Use a disposable COPY of a saved world already containing approved MWL sites.
Keep the original immutable. Give every arm a separate save directory and log
name. Before launching, record the actual game version, installed DLL SHA-256,
the save-file manifest (including chunk files), and the existing MWL site
identities (template, position and placed status). Back up the station first.
No player connection or character changes are needed.

1. **Partial registration and retry, same process.** Set `registration`.
   Require exactly one `[STARTUP FAULT]` line naming the inserted definition,
   a failed catalogue, `registration not ready`, and `world load Waiting`.
   Issue a save explicitly and require refusal. Verify saved bytes have not
   changed and no world-load release occurred. Run `mwl_memory resweep` without
   changing the environment. Require `registration ready`, `world load Loaded`,
   one world-load release, no duplicate definitions, and all expected saved
   MWL site identities present. Save normally, then restart without injection;
   verify the same site identities again.
2. **Thrown load failure.** Fresh disposable copy, `load-throw`. Require one
   fault, an ERROR naming the failure, `world load Failed`, refused explicit
   saving and unchanged world files. Run `mwl_memory resweep`; require an
   explicit restart-required refusal, no new audit or second load release,
   and saving still refused. Stop
   without waiting for a successful save. Restart with the switch unset and
   verify the original site's identities and normal saving.
3. **Returned game-load error.** Repeat step 2 on another fresh copy with
   `load-error`. If the game returns to the menu and creates a new ZoneSystem
   in the same process, require the original failure and restart-required
   status to survive that scene change. `mwl_memory resweep` must refuse,
   without starting another audit, re-registering, or releasing a second load.
   Confirm no new successful save and byte-identical saved files throughout
   the blocked phase. Restart the process without the fault and verify the
   saved site identities and normal saving. Keep this arm's evidence separate
   from the exception arm. Do not call a scene change a process restart.

A printed registration count can precede the fault. Use `registration ready`
and world-load state as the committed outcome, not that count alone. A save
refusal line is corroboration: file hashes are the check against overwriting
an unloaded world. Compare hashes while still in the blocked phase, before
permitting recovery and legitimate saves.

Never wait for `World save ... done` when the gate intentionally refuses it.
Archive complete BepInEx and game logs before another arm starts. Preserve the
real driver exit status. Remove the fault environment and restore/hash-verify
the station even when an assertion fails.

## Local coverage and its boundary

`StartupFaultTests` compiles the production `GenerationHold` prefixes and
finalizer, invokes them directly, and uses the real registration transaction.
It checks one-shot behaviour, unset/full-mode behaviour, logging failures,
partial-registration recovery, both load failure forms, blocked saves, and
restart-only load recovery. The doubles do not apply Harmony patches or load
a real save. The station campaign above is still needed for that integration.
