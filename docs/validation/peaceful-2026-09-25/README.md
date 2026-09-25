# Peaceful: player and creature damage

Validated on 2026-09-25 with Valdev and Valnet client 02. Both used temporary `mwl-peaceful-s8-8-0-27-20260925` profiles based on the deployed Praetoris Season 8 **8.0.27** mod set. Shared client/server DLL hashes matched. Version 8.0.28 was staged but not deployed.

The test used development character `Developer` (`Odev`) and the existing port in `mwlPortIconTest`, centered at `(648.97, 28.34, -1208.10)`. Its exterior radius was 16 metres. Valdev ran Valheim 1.0.12; the client ran 1.0.16. Both used network protocol 40 and connected successfully.

## Results

| Test | Before | Candidate |
|---|---|---|
| Four bronze-sword hits on a port beam | Health 400 → 330.5084 | Each hit dealt 0; health 330.5084 → 330.5084 |
| Greydwarf Brute melee hit on a port rug | Damage 27.73906; health 50 → 22.26094 | Damage 0; health 22.26094 → 22.26094 |
| Finewood bow with a wood arrow, fired inside the port | Not tested | One arrow consumed; damage 0; wall health 400 → 400 |
| First join after restarting both processes | Area existed, but Peaceful was absent from ObjectDB | Effect registered; player, Greydwarf, and Brute all received Peaceful |
| Move Brute outside, then back inside | Not tested | Peaceful expired outside and returned inside |

The baseline was `deedbca`: the old player-only branch merged with current `master`. Testing exposed a separate first-join defect: Jotunn's status-effect registration pass runs before `OnItemsRegistered`, where this branch creates Peaceful. The candidate adds Peaceful to the active ObjectDB as well as Jotunn's registry, with a duplicate check.

The tested candidate was **`90879fb`**. No runtime change to damage values was made by the test observer.

The Debug build passed with 152 existing warnings and zero errors. `git diff --check` passed, and the branch contains the current `master`.

## Evidence and method

- [Before/after hit, health, status, and boundary log excerpts](proof.log).
- [Inspected screenshot: Peaceful active inside the port, with a Greydwarf Brute](candidate.png).
- [Temporary observer source](PeacefulProof.cs.txt), retained to make the measurement method reviewable. It is not part of the mod build or release.

Player attacks used actual mouse input: a three-second sword attack hold, followed later by a three-second bow draw and release. The bow's arrow count changed from 20 to 19. Creature attacks used the creature's normal `Humanoid.StartAttack` animation and collision path after positioning it and stopping its movement AI. They did not inject synthetic damage into pieces.

The observer logged the input to `WearNTear.Damage` and read each affected piece's network health before and 0.5 seconds after the hit. Its Harmony prefix only observed the hit; it did not change damage or health. Separate repair commands ran after testing to restore the baseline-damaged beam and rug to 400 and 50 health.

The server's test-only allowed-mod list included the observer. ValheimEnforcer remained enabled. A leftover ValheimMonitor test DLL was disabled in the temporary server profile. Maintained profiles were unchanged.

Cleanup restored the beam and rug, removed the staged creatures, restored the previous profiles, and returned Valdev to its original stopped state. Valnet client 02 was confirmed off. Temporary profiles and local evidence were retained.

| DLL | SHA-256 |
|---|---|
| Baseline | `a3bcdcf74f36b1b523748f27e7dd72008ceebb70ce8b2de4e8c9b7c9457f984c` |
| Candidate, confirmed on both hosts | `29c8de6f5cae35ff431f287a880dd907561fb6a436fe3445c4c8fb2bbc3afc6d` |

## Log review and limits

Both candidate logs contained zero exceptions. Existing shader/content warnings remained, including `MWL_StoneOutlook1`, UndergroundRuins `8_PuzzleStand`, and Balrond fallback materials. ValheimEnforcer reported the intentionally modified MWL DLL, character-sync timeouts, and a character-save hitch; the same warning classes occurred in the baseline. StarLevelSystem also reported background location-reset counts in other zones. These are remaining profile issues; this is not a claim that the full profile logs are clean.

The damage results cover player melee, one player projectile, and Greydwarf Brute melee inside one existing port. They do not cover every port, trader, trainer, creature, creature projectile, area attack, or two-client ownership transfer. No new performance comparison was run.

Peaceful suppresses outgoing attack damage inside the area. It does not make pieces intrinsically indestructible. Attacks originating outside the area, weather, support loss, and scripted destruction remain outside this protection.
