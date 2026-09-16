# Server-only startup and recovery

The server verifies its location catalogue before loading a world. Loading
starts only after the approved locations have been registered successfully.
World saves are blocked until that initial load succeeds.

Every defined template in the supported packs is evaluated after its assets
are resolved. All templates that pass the current validator are eligible;
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

These guards apply to server-only mode. Vanilla's own save-error checks remain
in effect.
