# Version 5.1.2 validation — 2026-09-22

## Environment

- Dedicated server: Valdev.
- Players: Valnet client 01 and Valnet client 02, development characters.
- Baseline: Praetoris Season 8 8.0.24 dependency versions, including MWL 5.1.1.
- Candidate: the same mod set with MWL 5.1.2 on all three machines.
- Isolated mmcli profiles copied from the existing Season 8 profiles. The old
  profiles required dependency updates to match the current production manifest.
- Server MWL settings match production; only a generated default-value comment
  differs. Test-only connection settings and developer tools remain separate.
- Test helpers: valheimCLI and this PortProof plugin, allowed in the isolated
  ValheimEnforcer manifest. Upgrade World generated the test port normally.
- Test world: `mwlPortIconTest`; port trader at `(651.67, 33.97, -1201.24)`.

## Confirmed failure before the change

1. Client A created a wooden manifest and added 17 Wood.
2. Client B entered the port area.
3. Both clients saw two different network chest objects at the same position.
   Client A reported a combined 34 Wood.
4. Client B collected 17 Wood from its chest through the native TakeAll RPC.
5. Client A also collected 17 Wood. The original 17 Wood produced 34 collectible
   Wood, so this was not only a stale display.
6. A fresh 17-Wood snapshot was left in the port. After a normal server restart
   and both clients reconnecting, both clients again reported two chests and
   34 Wood.

The baseline chests reported `persistent=False`. The second client created a
new chest instead of attaching to the first client's chest. Remote inventories
also disagreed because the port replaced Valheim's inventory-save callback.

## Candidate results

- Legacy migration: one chest, 17 Wood, `persistent=True`,
  `persistentStorage=True`, and `snapshotBytes=0` on client A.
- Second client arrival: both clients reported one chest, the same network ID,
  and 17 Wood. The server also reported exactly one wooden shipment chest.
- Normal manifest purchase: the supplied 10 Wood and 5 Resin were consumed;
  one empty persistent chest was created. Adding 17 Wood synchronized to both
  clients.
- Normal server and client restart, with client B joining first: one chest and
  17 Wood remained. The chest's network ID changed, but both clients reported
  `LINKED chests=1`, proving the saved link survived the ID change.
- Port panel ownership: client A could not take control while client B's panel
  was open. After B closed it, A became the owner and opened the panel.
- Cross-player TakeAll: A collected 17 Wood from B's network-owned chest. Both
  clients then reported zero Wood. B's subsequent TakeAll added zero items.
- Both clients left the area and returned: one empty chest remained; no collected
  items returned, and both clients reattached to the same chest.
- Same-port shipment fixture: the server registered the 17-Wood shipment; the
  normal send button charged 50 Coins. The shipment survived a server restart.
  Client A opened the delivery, client B collected its 17 Wood, and the port
  removed the empty delivery chest. Both clients then reported zero chests.
  A second delivery attempt found no delivery and created no items.
- A new manifest could be purchased after delivery cleanup, confirming the slot
  was released.
- The final build adds a check for the native replicated chest-in-use flag.
  Its restart pass again produced one linked persistent chest and exactly
  17 Wood on both clients. With B's normal chest inventory open (`inUse=1`),
  A remained the port owner but its panel stayed closed. After B closed the
  inventory, A could open the panel. The final build also repeated the send and
  delivery flow with 17 Wood and the expected 50-Coin charge. B collected exactly
  17 Wood; both clients and the server then reported zero shipment chests, and
  another delivery attempt created nothing.

Final candidate SHA-256, verified on Valdev and both clients:

```text
8280c1cbc73894ba50d2803814a280338f72cf4716d8902c31eca00b8230344e
```

Release build: zero errors, 152 existing compiler/build warnings. Test fixture:
zero errors and zero warnings.

Selected command output from the final build (the players already held 34 test
Wood each before this fixture):

```text
After restart, client A:
PORTPROOF chests=1 totalWood=17 playerWood=34
LINKED chests=1 uiVisible=False playerCoins=450

After restart, client B:
PORTPROOF chests=1 totalWood=17 playerWood=34
LINKED chests=1 uiVisible=False playerCoins=0

After final delivery collection, client B:
PORTPROOF chests=0 totalWood=0 playerWood=51

After final delivery collection, client A:
PORTPROOF chests=0 totalWood=0 playerWood=34
DELIVERY not arrived

Server chest lookup after collection:
No objects found matching the provided criteria.
```

## Log review

The completed gameplay pass has no port migration, inventory, or RPC exceptions.
Startup still reports Unity shader-platform errors on both Linux clients and
intro-video/shader errors on the headless server. These do not occur in the port
operation window. The port UI was captured and inspected after restart.

Other environment warnings remain: unused location/recipe prefab references,
Balrond fallback requirements, Jotunn asset-name ambiguity, Deathlink's unspecified
fallback selection, and a slow character save. The test server intentionally has
no Discord webhook. These settings/assets were not changed by this fix. Duplicate
older dependency copies are skipped; BepInEx loads the newer required versions.

Early fixture errors were corrected before the completed gameplay pass: the
interact method's normal `false` return was incorrectly treated as command failure;
the fixture inspected a chest during deferred destruction; and a reflection lookup
used the wrong visibility for the public delivery-selection field. None required
changes to the production port behavior.
An additional chest-open attempt ran after a readiness timeout while the client
was still at the menu. The final open-chest check ran only after successful world
entry; the fixture now also rejects missing-chest requests without throwing.

The final client gameplay windows, from successful world entry through the last
collection, contain no warnings or errors. The final server log contains no port
migration or RPC exceptions. Full raw logs and command transcripts are retained
locally in the dated proof directory, outside the release package.

## Scope

The round-trip shipment fixture selects the same port as its destination to
avoid a travel delay. It exercises the normal purchase/send/delivery button code,
inventory changes, and server shipment RPCs. It does not test travel between two
different ports.

Normal server restarts are covered by this procedure. Forced process crashes,
power loss, and transaction consistency between the separate shipment file and
the world save are not covered. The fix does not remove items players duplicated
before upgrading. Back up the world before upgrading; update server and clients
together. The existing exact-version handshake rejects mixed MWL versions.

## Cleanup

The original active profiles were restored on Valdev and both clients. Valdev
was returned to its original stopped state. Both Valnet machines were stopped.
The isolated test profiles, test world, and local evidence were retained.
