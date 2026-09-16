# Server-only startup and recovery

The server verifies its location catalogue before loading a world. Loading
starts only after the approved locations have been registered successfully.
World saves are blocked until that initial load succeeds.

Use `mwl_memory` in the server console to see audit progress, registration
readiness, and world-load status. If verification or registration fails, the
server keeps the world unloaded and reports the cause. Correct that cause and
run `mwl_memory resweep` to retry verification and registration.

If world loading itself fails, saving remains blocked. Correct the cause and
restart the server. A resweep cannot retry a partially loaded world.

Once registration has succeeded, `mwl_memory resweep` is diagnostic: it reports
a fresh audit without changing the current registration or loading the world
again. A diagnostic failure leaves the existing world in place. Applying a
different selection of locations requires a new server session.

These guards apply to server-only mode. Vanilla's own save-error checks remain
in effect.
