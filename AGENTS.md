# MWL testing tools: agent entry point

For testing-framework, command-pack or adapter work, read the [local setup guide](MoreWorldLocations.TestAdapter/README.md), then the shared [agent workflow](https://github.com/tvongaza/ValheimTesting/blob/main/docs/agent-guide.md), [getting-started guide](https://github.com/tvongaza/ValheimTesting/blob/main/docs/getting-started.md) and [example index](https://github.com/tvongaza/ValheimTesting/blob/main/examples/README.md).

MWL-specific probes and scenarios stay in MWL. The optional port adapter needs full-mode gameplay for payment/delivery/ownership acceptance; a server-only world or main-menu smoke cannot establish it.

Preserve the test pyramid: unit and controlled integration checks first, a small native check for runtime boundaries, and a separate human usability verdict. Run native checks only against a disposable game install, world and character; back up anything you change and restore it afterwards, and never test against a server or save that people play on. A connected ValheimCLI is not proof of world readiness; transport success is not proof an action succeeded. Require exact pins, discovered capabilities and complete observations. Issue mutations once and poll read-only state. Report unrun checks and teardown failures honestly.

Do not infer deployment authorization from these instructions, and do not publish logs, credentials, game binaries or saves.

Use **ValheimCLI** as the product name in documentation. Preserve exact executable names, paths, package IDs, plugin GUIDs and code identifiers such as `valheim-cli`, `valheimCLI.dll` and `Valheim.Cli.Testing`.
