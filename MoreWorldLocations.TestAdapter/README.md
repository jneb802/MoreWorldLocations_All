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

The adapter compiles in the `Valheim.Testing.Adapter` 0.1.0-preview.2 source package
([what it provides](https://github.com/tvongaza/ValheimTesting/blob/main/docs/testing-toolkit.md#adapter-fixture-command-and-harmony-helpers-valheimtestingadapter-preview-2)):
`TestExtension` registers `mwl.testing` once ValheimCLI's extension API is ready,
`ConsoleAliases` adds the legacy console names, `InstalledPlugin` answers
`mwl.testing/runtime`, and `KeyValueReply` reads the probes' reply fields. The port
operations, their argument rules and the reply's completeness rules are MWL's.

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
`AllowOnServerClients`. If another plugin already owns one of the aliases, the
adapter withdraws its registration and logs why, and it unregisters only the exact
command objects it added. An old ValheimCLI with built-in MWL commands must be
replaced before loading this adapter.

Two more read-only capabilities are usable at the menu. `mwl.testing/runtime`
reports whether a live MWL plugin is registered (`installed`, `version`, `guid`).
`mwl.testing/session` is the toolkit's owned-session identity (token from
`MWL_TEST_SESSION_TOKEN`, process, save root, readiness), complete once a server
world is up with MWL loaded; see
[owned sessions](https://github.com/tvongaza/ValheimTesting/blob/main/docs/testing-toolkit.md#game-side-adapter-helpers-valheimtestingadapter-preview-1).
MWL need not be installed to test the adapter lifecycle; port operations require it
and a loaded world.

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

Use only disposable full-MWL worlds and test characters for these port probes
(the toolkit's [runtime hygiene checklist](https://github.com/tvongaza/ValheimTesting/blob/main/docs/runtime-hygiene.md) applies).
They can grant currency, create or clear shipments, open UI, load deliveries and
move the player. The payment, delivery and ownership-check probes clean up by
destroying the port's chests, so they refuse, without mutating anything, a port
whose chests already exist. Unloading the adapter removes commands; it does **not** roll back
world or inventory changes. Server-only MWL excludes ports, so these probes do not
validate server-only catalogue or terrain behavior.

## Test projects

- `MoreWorldLocations.Tests`: the MWL unit tests. They compile the shipped server-only
  sources against the game doubles in `MoreWorldLocations.Doubles`, which also consume
  pure `Valheim.Testing` (for example `SharedZoneConversionTests`). The shipped MWL
  plugin gains no dependency.
- `MoreWorldLocations.Doubles`: `Valheim.Testing.Doubles` built as its own assembly
  named `assembly_valheim`, because the template check asks whether a component's type
  comes from the game's assembly
  ([why](https://github.com/tvongaza/ValheimTesting/blob/main/docs/testing-toolkit.md#a-doubles-assembly-named-assembly_valheim)).
  MWL adds the game members the package does not declare, and leaves four package
  files out in favour of its own: a copy of `ValheimDoubles.cs` whose
  `TerrainComp.Save` writes the game's terrain bytes, a component `TerrainModifier`, a
  `HeightmapBuilder` that can leave a zone unbuilt, and Unity components whose settings
  are not public fields. The project file and each replacement file say why.
- `MoreWorldLocations.TestAdapter.Tests`: engine-free tests of the actual argument and
  reply rules, plus the external scenario assertions against the toolkit's
  [`ScriptedTransport`](https://github.com/tvongaza/ValheimTesting/blob/main/docs/testing-toolkit.md#test-fakes).
- `MoreWorldLocations.SystemTests`: `PortScenarios` uses shared `GameActor` and
  `ScenarioReport` for payment, delivery and ownership checks. The fixture host
  verifies environment pins, prepares the port/player, writes JSON/JUnit, and owns
  teardown. Effects are issued once. Existing monolithic probes are a migration
  stage; separating them into individual setup/action/observation operations is
  follow-up work before expanding the scenario coverage.

Pinned toolkit packages: `Valheim.Testing` 0.1.0-preview.7 and `Valheim.Testing.Doubles`
0.1.0-preview.6 (unit tests), `Valheim.Testing.Game` 0.1.0-preview.14 (net10.0 scenarios
and adapter tests), `Valheim.Testing.Adapter` 0.1.0-preview.2 (adapter). Where each version
is published, and how to add a local feed for an unpublished one, is in the toolkit's
[package versions and feeds](https://github.com/tvongaza/ValheimTesting/blob/main/docs/getting-started.md#package-versions-and-feeds).
To try another release, pass `-p:TestingPackageVersion=`, `-p:DoublesPackageVersion=`,
`-p:ToolkitPackageVersion=` (Game) or `-p:AdapterPackageVersion=`.

In Visual Studio 2026, Rider or VS Code, open `MoreWorldLocations.Testing.slnx` for the unit, scenario and adapter-logic tests ([IDE notes](https://github.com/tvongaza/ValheimTesting/blob/main/docs/getting-started.md#using-visual-studio-rider-or-vs-code)); the game-side adapter itself is built separately above.

```sh
# Unit tests on net10.0 and net48 (net48 under Mono on macOS/Linux); the runner is the toolkit's
MoreWorldLocations.Tests/run-tests.sh MoreWorldLocations.Tests/MoreWorldLocations.Tests.csproj
dotnet test MoreWorldLocations.TestAdapter.Tests/MoreWorldLocations.TestAdapter.Tests.csproj -c Release
# Unpublished toolkit build: add -p:RestoreAdditionalProjectSources=/absolute/ValheimTesting/.packages
```

See the toolkit's [test runner notes](https://github.com/tvongaza/ValheimTesting/blob/main/tools/test-runners/README.md) for its options and exit codes.

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

Start with the shared [getting-started guide](https://github.com/tvongaza/ValheimTesting/blob/main/docs/getting-started.md) and [example index](https://github.com/tvongaza/ValheimTesting/blob/main/examples/README.md). MWL-specific expectations stay in MWL; only generic lifecycle, transport and observation helpers are shared. [ReloadCheck](https://github.com/tvongaza/ValheimTesting/blob/main/examples/ReloadCheck/README.md) explains the reusable extension-replacement pattern. It is not a substitute for a full-mode port fixture or proof of shipment persistence.

## Shared terrain conversion fixture

`MoreWorldLocations.Tests/SharedZoneConversionTests.cs` runs MWL's real level/smooth/paint conversion across adjacent zones in either order on `Valheim.Testing`'s shared zone state, preserves untouched inputs and refuses repeated application by operation identity. Expectations stay in MWL; the shared state models no Unity, native save or replication behavior ([shared fixture guide](https://github.com/tvongaza/ValheimTesting/blob/main/docs/shared-world.md)).

## Strict smoke setup

The menu-smoke executable requires a fifth argument: `MoreWorldLocations.SystemTests <port> <adapter.dll> <empty-scripts-dir> <report.json> <pins-file>`. Supply the reviewed hashes of the installed ValheimCLI, ScriptEngine and every other loaded plugin. The driver explicitly requires MWL and its adapter absent at baseline; it pins the adapter's MD5 from the input DLL when installed and requires absence after removal. Every command gets a `cli_expect --strict` preflight, including expected-refusal probes. Only the expected new pins are polled while replacement settles. The persistent in-game expectation file, if enabled, must also match each intended phase; the driver never disables it. The adapter must list nine commands: the seven probes, `runtime` and `session`.

The strict driver passed its native menu smoke with Game preview.8 and the adapter before it moved to the toolkit's adapter helpers: aliases, no-world refusal, missing dependency, removal and stable core.

## Native port validation — 27 September 2026

A native run on 27 Sep 2026 on a disposable test machine used a disposable Valheim 1.0.16 world and full-mode MWL 5.1.4 client/server. Two authentic `MWL_Port1` templates were spawned through the game's location path; this is a functional shipping fixture, not a test of natural port placement. The client had five registered manifests and two loaded ports. All calls used explicit strict plugin/world pins and the persistent dispatch guard.

- Payment: the shipping fee (50 coins) was charged exactly once, matching MWL's computed cost (`m_containers.GetCost()`).
- Delivery: the test shipment loaded and the open-delivery flag was true.
- Ownership: one test character seeded a labelled shipment; a different character was denied at the loaded destination (`canAccess=False`, `loaded=False`). The original character rejoined and could open that same shipment (`canAccess=True`, `loaded=True`). No player identity was spoofed.
- ScriptEngine replaced the adapter in the loaded world with a new instance while the core stayed installed. Final removal refused the old capability, verified plugin absence and preserved core identity. The strict main-menu smoke is separate evidence.

The run first exposed two adapter defects. `ItemDrop` prefab data can lack its runtime `m_dropPrefab`, causing the detached shipment fixture to throw. `DetachedItem` now explicitly supplies the prefab on its clone. Current MWL stores the open-delivery flag in the port ZDO; the observer now reads that when the older instance field is absent. Delivery passed after these corrections. Both paths have focused local regressions.

The production MWL DLL (MD5 `c73a65fbfcb89febd57834e659706d9f`) was unchanged; the tested adapter was `8d4425964e8aa429795212122fe529e9`. This proves the optional adapter against the tested 5.1.4 build, not every MWL version, asynchronous failure mode, shipment restart/migration case or natural port placement. Human walking/appearance checks remain separate work.

Later adapter changes have not yet been run natively: currency resolution from MWL's configured currency, refusal of ports that already have chests, refusal of an out-of-range `goto-port` index, `unknown` for absent open-delivery and ownership-gate observations, removal of the selected-delivery field, and the move to the toolkit's registration, alias, runtime and reply helpers (which adds `mwl.testing/session`).
