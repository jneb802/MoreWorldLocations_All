# More World Locations AIO

This repository builds a BepInEx Valheim mod that adds custom world locations,
ports, traders, shrines, waystones, totems, dungeons, loot, creatures, and YAML
configuration.

## Review Context

- The main plugin project is `More World Locations_AIO/More World Locations_AIO.csproj`.
- Shared code lives in `Common/`.
- Dungeon subprojects live in `Dungeon_Castle/` and `Dungeon_The Ritual/`.
- The main plugin targets `.NET Framework 4.8`, uses C# 10, and has nullable reference types enabled.
- The build depends on local Valheim, BepInEx, publicized Valheim assemblies, Jotunn, ServerSync, and ILRepack.
- GitHub-hosted runners usually do not have the Valheim install or publicized assemblies required for a full build.

## Coding Rules

- Prefer explicit C# type declarations over `var`.
- Keep changes scoped to the requested behavior.
- Do not remove or rename embedded assets, YAML resources, Thunderstore metadata, or bundle paths unless the PR is explicitly about packaging or assets.
- Treat server/client synchronization, ZDO ownership, localization keys, YAML schema changes, and world-upgrade behavior as high-risk areas.
- For user-facing config changes, verify defaults and migration behavior.

## Review Expectations

- Focus on behavior bugs introduced by the PR diff.
- Cite specific files and lines for any finding.
- Do not flag style-only issues unless they obscure behavior or maintainability.
- Do not ask the author to run a full GitHub Actions build unless the workflow provides the required Valheim assemblies.
