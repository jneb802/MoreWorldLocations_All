# Private shipment synchronization validation

Validated on 2026-10-03 with Valdev and both Valnet clients.

The maintained `praetoris-season-8` profiles used Valheim l-1.0.16 and
the installed Season 8 production files, including overrides newer than the
8.0.31 package manifest. All 477 production server plugin and patcher files
matched the server baseline. Each client had 425 matching production files,
with the intended client and server differences. The candidate changed only
the MWL DLL to 5.1.8. Temporary command helpers exercised the real port UI,
shipment requests, delivery button, and container TakeAll methods.

The existing `mwlPortIconTest` world had 12,019 location entries. This test
used the production mod set, but did not use the complete production world.
The test manifest temporarily accepted the actual installed DLL hashes and
the client mods required by their own version handshakes. Hash enforcement
remained enabled. Production was not changed or restarted.

| Scenario | Result |
| --- | --- |
| 5.1.7 baseline: player 1 sends 17 Wood | Player 2 received one inaccessible shipment, including its item data. |
| 5.1.8: player 1 sends and collects 17 Wood | Player 1 received and recovered all 17 Wood. Player 2 kept an empty cache and received no shipment update. |
| Both players create private shipments | Each cache contained one accessible shipment and no foreign shipment. Creating player 2's shipment left player 1's view unchanged. |
| Client reconnect and server restart | Both private shipments remained saved. Each returning player received only their own shipment. Both recovered all 17 Wood. |
| Older record with all ownership fields empty | Both players received the shared shipment. Collection removed it from both views and preserved its 17 Wood. |
| Collect another player's shipment or a missing ID | The server rejected both requests. The saved shipment and client views remained unchanged. |
| Expiration enabled, five-second transit and three-second expiration | The server removed the shipment from disk and cleared its owner's cache. The unrelated client's update log remained unchanged. |
| Empty views | Clients correctly cleared their cache after collection and expiration. |

Valheim Monitor recorded four baseline port synchronization messages totaling
2,272 bytes for one creation and collection: both changes went to both clients.
Monitor did not expose the candidate's outbound Jotunn shipment message, even
with its capture expanded from five to 100 methods. Therefore, this test does
not establish a comparable byte reduction or an overall hourly traffic reduction.
Client cache contents and update callbacks establish that unrelated private
shipment updates were eliminated.

Follow-up inspection identified the measurement gap. The installed Network
Performance System DLL enables `RelaySendReuse`. Its `RelaySend.SendFrame`
updates the game's sent counters and sends directly through `m_socket.Send`,
without calling `ZRpc.Invoke`. Monitor counts named outbound messages at
`ZRpc.Invoke`, so it misses this optimized route. Monitor's total traffic
counter still reads the game's sent counters. Other outgoing routed messages
that use this optimization have the same gap in their per-message breakdown.

The release build passed with no errors. Server and client logs had no new
shipment errors. Existing headless shader errors and the unrelated
MoreVanillaBuildPrefabs Trailership warning remained. A test helper initially
called a private game method directly; the corrected helper used reflection,
and the unauthorized collection checks passed after that correction.

The original DLLs, configs, shipment files, test world, and prior profile
selections were restored and checked. Test helpers were removed. Both Valnet
machines were stopped. Valdev loaded the restored 5.1.7 mod.

Evidence is retained in the operator workspace under
`validation-artifacts/mwl-private-shipments-20261003`.

Deployment must update the server and clients together to 5.1.8 because the
shipment synchronization protocol changed. Existing shipment save files do
not need conversion.
