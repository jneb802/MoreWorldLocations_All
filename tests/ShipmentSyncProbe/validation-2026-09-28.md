# Live validation — 2026-09-28

## Environment

- Valdev and Valnet client 02; one connected development character.
- Existing `praetoris-season-8` profiles, temporarily aligned for the test. No new profiles were created.
- Client package versions matched Praetoris 8.0.29. Server plugin and patcher files came from live production; MWL settings and mod policy also came from production. Other server configs retained test-device settings.
- Valheim 1.0.16; MWL baseline 5.1.4; ValheimMonitor 0.5.0 on the server, source `valdev-mwl-shipment`.
- Identical monitor settings for both builds, including `TopNRpcs = 250`.
- Candidate server and client DLL SHA-256: `9ad74bf402240b88826288b3c2affb199f207ce508a85fc5e2739fb48a6d268d`.

## Measurement

The fixture contained one unchanged, empty shipment. Each run performed ten port interactions through `Port.Interact` and closed the interface between interactions. The collector had fresh samples beyond each measurement window before querying it.

| Build | Interactions | Port ConfigSync outbound calls | Port ConfigSync outbound bytes | Window |
|---|---:|---:|---:|---:|
| Original | 10 | 10 | 6,750 | 38.076 s |
| Candidate | 10 | 0 | 0 | 42.877 s |

Compare traffic per interaction; the window duration includes SSH and command latency. This is a 100% reduction for the repeated-browsing operation, not for all MWL traffic or total server traffic.

## Correctness

- After restarting server and client with the candidate, the saved shipment appeared before any port interaction.
- Creating another fixture shipment changed the synchronized client count from one to two.
- Collecting a fixture shipment changed the synchronized count from two to one.
- After stopping the server and removing the test world's shipment file, the client retained one cached shipment while disconnected. Reconnecting to the restarted server changed the count to zero without opening a port.
- A screenshot confirmed the candidate port interface opened.
- The original and candidate server runs each logged the same 14 error lines: headless graphics/video startup errors and a `DiscordConnector.Records.Database.Dispose` exception during shutdown. No shipment-operation exceptions appeared in the candidate command output or client log.

## Limits and rollout

This validates synchronization with one client and an empty shipment fixture. It does not measure multi-client load, physical item delivery, payment, automatic expiration, CPU improvement, or allocation improvement. Changes to shipment contents still broadcast the complete collection; this patch removes the unnecessary browsing requests and initializes the login snapshot.

Deploy matching server and client builds together. Older servers do not initialize the shipment snapshot during startup. Shipment file and payload formats are unchanged.

The Release build completed with zero errors and 152 warnings. The test helper build completed with zero warnings or errors. The measurement script completed against the live monitor for both runs.

## Cleanup

The pre-test server and client profile contents and the test world were restored and compared against their archives. Original profile selections and Valdev's stopped state were restored. Candidate DLLs, the probe, temporary policy changes, and backup directories were removed from the devices. Valdev's base game update from 1.0.12 to 1.0.16 remains; profile restoration does not downgrade game binaries.
