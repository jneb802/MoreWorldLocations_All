# Shipment synchronization validation

This client-only helper exercises the installed MWL assembly through reflection. It is not part of the mod project or release package. Install it only on a leased test client and allow its exact hash in the test server's optional mod policy.

Use the maintained Praetoris mirror profiles. Back up affected files and policy before testing. Restore them and remove this helper afterward.

## Build and preparation

```sh
dotnet build tests/ShipmentSyncProbe/ShipmentSyncProbe.csproj -c Release
```

The project uses the local macOS Valheim managed and publicized assemblies. Override `GameRoot` and `Managed` for another installation. Put only `ShipmentSyncProbe.dll` in the client profile; use the game's existing Newtonsoft.Json assembly.

Join Valdev with a development character. Move within 10 metres of an existing port. ValheimMonitor must record fresh samples from the server, with the same settings for both builds and enough network methods retained to include the port synchronization method.

## Commands

- `mwl_probe_open`: calls the nearest port's normal `Interact` handler.
- `mwl_probe_seed`: opens that port and creates one empty test shipment through `Shipment.SendToServer`. This is a synchronization fixture, not an item-transfer or payment test.
- `mwl_probe_status`: reads the local shipment IDs and count without opening a port or requesting data.
- `mwl_probe_collect`: calls `Shipment.OnCollected` for one shipment created by this helper. Wait for the server reply before reading the count again.

## Comparison

1. On the original build, seed one shipment and wait for its synchronized count to reach one. Close the port interface.
2. Run the measurement below. It repeats the real interaction handler, closes the interface with Escape, and records server outbound bytes and calls for the port synchronization channel. Keep Valheim focused on display `:0`.
3. Restart server and client with the candidate build, retaining the same world and shipment file. Before opening a port, use `mwl_probe_status` to verify that the saved shipment arrived during login.
4. Repeat the measurement with the same count and monitor settings.
5. Create and collect test shipments. Verify that the client count changes after the server reply and that the monitor still records these updates. Reconnect to verify the final saved state.

```sh
python3 tests/ShipmentSyncProbe/measure.py \
  --client paperspace@CLIENT_IP --server warp@valdev \
  --source valdev-mwl-shipment --count 10 --output before.json
```

Use a separate `after.json` for the candidate. Compare each source separately; do not add server outgoing and client incoming bytes as if they were different transfers. These are monitor RPC byte counts, not a measurement of total wire traffic or frame-time improvement.

Restore the test world and profile backups, including generated shipment data, policy changes, and monitor settings. Remove the helper and temporary backups after verification.
