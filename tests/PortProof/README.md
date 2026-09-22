# Shipping port multiplayer regression fixture

This development-only plugin calls the existing MWL port methods and Valheim
inventory operations. Do not include it in a release or install it on production.
It creates test items and moves development characters.

## Build

```sh
dotnet build tests/PortProof/PortProof.csproj -c Release
```

The project uses the local Valheim and BepInEx assemblies. Override `GameDir` and
`Managed` with MSBuild properties if the game is installed elsewhere. Copy the
resulting `PortProof.dll` into the isolated test profiles. Permit its plugin ID
`local.mwl.portproof` in the test server's ValheimEnforcer manifest.

## Before / after test

Use a dedicated server and two clients with the same current Season 8 mod set.
Use development characters. Generate a normal MWL port in the test world.

1. Move client A to the port with `portproof_go X Y Z`.
2. Run `portproof_seed` once. This calls `Port.SpawnContainer` and adds 17 Wood
   through the chest inventory. It does not test purchase material costs.
3. Move client B to the same port. Run `portproof_status` on both clients.
4. Run `portproof_take INDEX` on client B. This uses Valheim's normal TakeAll RPC.
5. Check both inventories. The combined increase must be exactly 17 Wood.
6. Move away and return. Check that items and chests do not reappear.
7. Repeat with 17 Wood left in a chest. Restart the server normally, reconnect
   both clients, and check that there is only one chest containing 17 Wood.

For the fixed version, run `portproof_open` before `portproof_seed` if the local
client does not own the port. Wait for the port panel to open. The normal owner
request must complete; the fixture does not force port ownership.

## Additional checks

- Upgrade with old `PortItems` data still present. Both clients must see one
  persistent chest. The old snapshot must be cleared only after successful import.
- `portproof_open`, then `portproof_buy`, buys a wooden manifest through the normal
  UI button. The fixture supplies the required 10 Wood and 5 Resin first.
- `portproof_add` adds 17 Wood to an existing locally owned chest.
- `portproof_hide` closes the port panel. Another player must then be able to
  request port control and use the panel.
- `portproof_chest_open` and `portproof_chest_close` use the normal chest
  interaction and inventory close methods. While one player has a chest open,
  the other player's port panel must remain closed.
- `portproof_roundtrip_send` supplies 500 Coins and selects the current port as
  the destination. This is a same-port test fixture, not a destination the normal
  UI offers. It exercises the normal send button, costs, and server shipment RPC
  without a distance-based travel delay.
- After the shipment arrives, run `portproof_open`, wait for the shipment list,
  then `portproof_deliver`. This uses the normal delivery button. Collect its
  contents with `portproof_take` and verify that the emptied delivery is removed.
- Check both client logs and the server log for new errors during the test window.

The fixture does not change MWL code, bypass network ownership checks, or replace
inventory save callbacks. Record every use of its item-creation commands so that
test items cannot be mistaken for duplicated items.
