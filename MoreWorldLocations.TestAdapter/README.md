# Optional MWL testing adapter

This project owns the seven MWL port/shipment commands extracted from valheimCLI.
It is development tooling, separate from the shipped MWL plugin. Building or
installing MWL normally does not build, install, or reference this adapter or ValheimCLI.

The ValheimCLI core remains in `BepInEx/plugins`. The adapter can be placed in
`BepInEx/scripts` for ScriptEngine replacement. Do not copy ValheimCLI, Unity, or game
assemblies into scripts. Build against the exact ValheimCLI extension-host candidate you
install. Preview API v1 is not a compatibility promise with an older upstream ValheimCLI.

```sh
dotnet build MoreWorldLocations.TestAdapter/MoreWorldLocations.TestAdapter.csproj \
  -c Release -p:CliDll=/absolute/path/to/valheimCLI.dll
```

`CliDll` is required and has no default; the build stops with an error if it is
missing or does not exist.

`GameReferences.props` contains compile references only: it deliberately does not
import MWL's production packaging/deployment targets. Symbols are embedded for
ScriptEngine. Reflection binds to the **currently registered MWL plugin**, never
to an arbitrary old assembly left behind by a reload.

| Extension command | Existing compatibility alias | Access |
|---|---|---|
| `mwl.testing/port-status` | `cli_mwl_port_status [radius]` | Read-only, loaded world |
| `mwl.testing/goto-port` | `cli_mwl_goto_port [index]` | Client mutation |
| `mwl.testing/clear-shipments` | `cli_mwl_clear_shipments` | Server mutation |
| `mwl.testing/payment-probe` | `cli_mwl_port_payment_regression [item] [count]` | Client mutation |
| `mwl.testing/delivery-probe` | `cli_mwl_port_delivery_regression [item] [count]` | Client mutation |
| `mwl.testing/ownership-seed` | `cli_mwl_port_ownership_seed [item] [count]` | Client mutation |
| `mwl.testing/ownership-check` | `cli_mwl_port_ownership_check <id> [blocked\|allowed]` | Client mutation |

Use `cli_extension <extension-command> ...` for the structured result. Aliases go
through the **same** extension dispatcher, shared operation gate, cancellation,
and permission checks. Mutations require devcommands; a joined client also needs
`AllowOnServerClients`. The adapter refuses to register if another plugin already
owns one of its aliases, and unregisters only the exact command objects it owns.
An old ValheimCLI with built-in MWL commands must be replaced before loading this adapter.

`mwl.testing/runtime` is an additional read-only capability usable at the menu. It
reports whether a live MWL plugin is registered. MWL need not be installed to test
the adapter lifecycle; port operations require it and a loaded world.

## Results and scope

The original probe implementations move with their MIT notice; their world/player
effects are not rewritten as part of extraction. Successful replies retain their
console lines and also carry structured `fields`, `source` and `complete`. Errors,
silence, duplicate fields and ambiguous multiple replies fail. Invalid arguments
are refused instead of quietly turning into a zero, default, or different action.

Transport success is not a test pass. `result=BUG_PRESENT` is a valid observation
which **fails** the external scenario. A `retry=true` ownership response is
incomplete, never auto-replayed. Teleport, shipment submission and clearing actions
are also marked incomplete: accepting their request does not prove arrival or
server-side completion. Missing collection counts remain incomplete. The delivery
probe calls `Port.LoadDelivery` directly, so it does not observe the port UI's
selected-delivery state.

Use only disposable full-MWL worlds and test characters for these port probes.
They can grant currency, create or clear shipments, open UI, load deliveries and
move the player. The payment, delivery and ownership-check probes clean up by
destroying the port's chests, so they refuse, without mutating anything, a port
whose chests already exist. Unloading the adapter removes commands; it does **not** roll back
world or inventory changes. Server-only MWL excludes ports, so these probes do not
validate server-only catalogue or terrain behavior.

## Test pyramid

- `MoreWorldLocations.Tests`: the existing MWL unit tests, plus
  `SharedZoneConversionTests`, which adds a test-only `Valheim.Testing` package
  reference to that project. The shipped MWL plugin gains no dependency.
- `MoreWorldLocations.TestAdapter.Tests`: engine-free tests of actual argument and
  reply rules, plus fake-transport tests of the external scenario assertions.
- `MoreWorldLocations.SystemTests`: `PortScenarios` uses shared `GameActor` and
  `ScenarioReport` for payment, delivery and ownership checks. The fixture host
  verifies environment pins, prepares the port/player, writes JSON/JUnit, and owns
  teardown. Effects are issued once. Existing monolithic probes are a migration
  stage; separating them into individual setup/action/observation operations is
  follow-up work before expanding the scenario coverage.

In Visual Studio 2026, Rider or VS Code, open `MoreWorldLocations.Testing.slnx` for the unit, scenario and adapter-logic tests ([IDE notes](https://github.com/tvongaza/ValheimTesting/blob/main/docs/getting-started.md#using-visual-studio-rider-or-vs-code)); the game-side adapter itself is built separately below.

External scenarios consume pinned `Valheim.Testing.Game` 0.1.0-preview.10 (net10.0), with `Valheim.Testing` 0.1.0-preview.5 and ValheimCLI transport `Valheim.Testing.Cli` 0.1.0-preview.4 dependencies, from [ValheimTesting](https://github.com/tvongaza/ValheimTesting). `MoreWorldLocations.SystemTests` and `MoreWorldLocations.TestAdapter.Tests` target net10.0. All three restore from nuget.org; to try an unpublished ValheimTesting build, build its local feed (see its bootstrap/validate guide) and add its `.packages` directory as an extra source. No sibling ValheimCLI source checkout is required; `CliDll` still selects the game adapter's exact ValheimCLI extension API.

```sh
dotnet restore MoreWorldLocations.TestAdapter.Tests/MoreWorldLocations.TestAdapter.Tests.csproj
# Unpublished build: add -p:RestoreAdditionalProjectSources=/absolute/ValheimTesting/.packages
dotnet test MoreWorldLocations.TestAdapter.Tests/MoreWorldLocations.TestAdapter.Tests.csproj -c Release --no-restore
```

The executable in `MoreWorldLocations.SystemTests` is a **main-menu boundary
smoke**, not a port gameplay test. It refuses a loaded world or installed MWL,
then verifies alias registration, shared world preconditions, missing-dependency
observation, cleanup, and the unchanged core on the same connection. Supply an
empty scripts directory reserved for this run, on the same machine as the ValheimCLI port:

```sh
dotnet run --project MoreWorldLocations.SystemTests -- \
  5555 /absolute/path/MoreWorldLocations.TestAdapter.dll \
  /absolute/disposable-game/BepInEx/scripts /absolute/results/adapter.json \
  /absolute/pins.txt
```

Full MWL port scenarios require a prepared game fixture. The native run below covers payment, delivery and character ownership. Fast tests alone do not establish Harmony timing, RPC settlement, or persistence.

To try another toolkit release, pass `-p:ToolkitPackageVersion=<version>` to the
system/adapter test projects. The game adapter still builds against the exact
installed ValheimCLI extension API via `CliDll`.

Start with the shared [getting-started guide](https://github.com/tvongaza/ValheimTesting/blob/main/docs/getting-started.md) and [example index](https://github.com/tvongaza/ValheimTesting/blob/main/examples/README.md). MWL-specific expectations stay in MWL; only generic lifecycle, transport and observation helpers are shared. [ReloadCheck](https://github.com/tvongaza/ValheimTesting/blob/main/examples/ReloadCheck/README.md) explains the reusable extension-replacement pattern. It is not a substitute for a full-mode port fixture or proof of shipment persistence.

## Shared terrain conversion fixture

`MoreWorldLocations.Tests/SharedZoneConversionTests.cs` consumes pure `Valheim.Testing` 0.1.0-preview.5. Restore that unit project from nuget.org (or the same local feed for a pre-release build), then run `MoreWorldLocations.Tests/run-tests.sh` for .NET and Mono. Its real conversion runs level/smooth/paint across adjacent zones in either order, preserves untouched inputs and refuses repeated application by operation identity. Expectations stay in MWL; shared state models no Unity, native save or replication behavior. See the [shared fixture guide](https://github.com/tvongaza/ValheimTesting/blob/main/docs/shared-world.md). Production MWL has no new dependency.

Game preview.8 added reusable [TerrainCapture](https://github.com/tvongaza/ValheimTesting/blob/main/examples/TerrainCapture/README.md) and [SessionControl](https://github.com/tvongaza/ValheimTesting/blob/main/examples/SessionControl/README.md) consumers. They do not replace MWL-specific port assertions or close the full-mode gameplay gate. The shared capability surface passed its [bounded native validation](https://github.com/tvongaza/ValheimTesting/blob/main/docs/native-validation-20260927.md).

## Strict smoke setup

The menu-smoke executable requires a fifth argument: `MoreWorldLocations.SystemTests <port> <adapter.dll> <empty-scripts-dir> <report.json> <pins-file>`. Supply the reviewed hashes of the installed ValheimCLI, ScriptEngine and every other loaded plugin. The driver explicitly requires MWL and its adapter absent at baseline; it pins the adapter's MD5 from the input DLL when installed and requires absence after removal. Every command gets a `cli_expect --strict` preflight, including expected-refusal probes. Only the expected new pins are polled while replacement settles. The persistent in-game expectation file, if enabled, must also match each intended phase; the driver never disables it.

The strict driver passed its native menu smoke with Game preview.8: aliases, no-world refusal, missing dependency, removal and stable core.

## Native port validation — 27 September 2026

A native run on 27 Sep 2026 on a disposable test machine used a disposable Valheim 1.0.16 world and full-mode MWL 5.1.4 client/server. Two authentic `MWL_Port1` templates were spawned through the game's location path; this is a functional shipping fixture, not a test of natural port placement. The client had five registered manifests and two loaded ports. All calls used explicit strict plugin/world pins and the persistent dispatch guard.

- Payment: the shipping fee (50 coins) was charged exactly once, matching MWL's computed cost (`m_containers.GetCost()`).
- Delivery: the test shipment loaded and the open-delivery flag was true.
- Ownership: one test character seeded a labelled shipment; a different character was denied at the loaded destination (`canAccess=False`, `loaded=False`). The original character rejoined and could open that same shipment (`canAccess=True`, `loaded=True`). No player identity was spoofed.
- ScriptEngine replaced the adapter in the loaded world with a new instance while the core stayed installed. Final removal refused the old capability, verified plugin absence and preserved core identity. The strict main-menu smoke is separate evidence.

The run first exposed two adapter defects. `ItemDrop` prefab data can lack its runtime `m_dropPrefab`, causing the detached shipment fixture to throw. `DetachedItem` now explicitly supplies the prefab on its clone. Current MWL stores the open-delivery flag in the port ZDO; the observer now reads that when the older instance field is absent. Delivery passed after these corrections. Both paths have focused local regressions.

The production MWL DLL (MD5 `c73a65fbfcb89febd57834e659706d9f`) was unchanged; the tested adapter was `8d4425964e8aa429795212122fe529e9`. This proves the optional adapter against the tested 5.1.4 build, not every MWL version, asynchronous failure mode, shipment restart/migration case or natural port placement. Human walking/appearance checks remain separate work.

Later adapter changes have not yet been run natively: currency resolution from MWL's configured currency, refusal of ports that already have chests, refusal of an out-of-range `goto-port` index, `unknown` for absent open-delivery and ownership-gate observations, and removal of the selected-delivery field.
