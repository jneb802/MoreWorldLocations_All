# MWL testing tools: agent entry point

For testing-framework, command-pack or adapter work, read the [local setup guide](MoreWorldLocations.TestAdapter/README.md), then the shared [agent workflow](https://github.com/tvongaza/ValheimTesting/blob/main/docs/agent-guide.md), [getting-started guide](https://github.com/tvongaza/ValheimTesting/blob/main/docs/getting-started.md) and [example index](https://github.com/tvongaza/ValheimTesting/blob/main/examples/README.md).

MWL-specific probes and scenarios stay in MWL. The optional port adapter needs full-mode gameplay for payment/delivery/ownership acceptance; a server-only world or main-menu smoke cannot establish it.

The agent workflow's rules apply here: the test pyramid, exact pins and discovered capabilities, mutations issued once and read-only state polled, and honest reports of unrun checks and teardown failures. Every native check follows the toolkit's [runtime hygiene checklist](https://github.com/tvongaza/ValheimTesting/blob/main/docs/runtime-hygiene.md). Run native checks only against a disposable game install, world and character; back up anything you change and restore it afterwards, and never test against a server or save that people play on.

The unit tests' game doubles (`MoreWorldLocations.Doubles`) are the toolkit's `Valheim.Testing.Doubles` plus MWL's additions. Add a missing game member there, not in the test project; replace a package file only when the package cannot model what a test needs (the project file lists the four it replaces, and why).

Do not infer deployment authorization from these instructions, and do not publish logs, credentials, game binaries or saves.

Use **ValheimCLI** as the product name in documentation. Preserve exact executable names, paths, package IDs, plugin GUIDs and code identifiers such as `valheim-cli`, `valheimCLI.dll` and `Valheim.Cli.Testing`.
