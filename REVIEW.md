# Review Instructions

## Important Findings

Mark a finding Important only when the PR likely introduces a real behavior bug,
data loss, broken multiplayer behavior, asset/config breakage, release packaging
breakage, or a compatibility problem with Valheim, BepInEx, Jotunn, or
Thunderstore.

Important examples:

- Server/client state can diverge, duplicate, or fail to sync.
- ZDO ownership, RPC routing, or network cleanup can run on the wrong peer.
- A location, asset bundle, embedded resource, YAML file, localization key, or
  Thunderstore dependency is renamed or moved without updating all references.
- Config defaults or migrations can break existing worlds.
- A release metadata change makes the package version, changelog, manifest, or
  dependencies inconsistent.

## Nit Findings

Use Nit for small maintainability issues only when they are clearly introduced by
the PR. Do not post more than five Nit comments. If there are more, summarize the
extra items in the final comment instead of posting each one inline.

## Do Not Report

- Missing CI coverage for the full Valheim build. GitHub-hosted runners do not
  have the local game assemblies this repo needs.
- Formatting-only concerns unless they make the code hard to read.
- Pre-existing issues outside the PR diff unless the PR makes them worse.
- Speculative risks without a concrete code path and file/line evidence.

## Final Comment

Always post one top-level PR comment. Start with either `No blocking issues
found.` or `Blocking issues found.` Then summarize the reviewed areas and list
any Important findings first.
