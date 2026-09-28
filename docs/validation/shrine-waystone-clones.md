# Shrine and waystone clone validation

## Change

Create `MWL_Shrine` and `MWL_Waystone` from the installed game's `guard_stone`.
Keep their names, persistent network component, damage handling, and visual
children. Remove `PrivateArea`, `AreaMarker`, and `PlayerBase`. Keep `WayEffect`
active and add `RandomSpawn` with `m_chanceToSpawn = 5`. Add the existing
`Shrine` or `Waystone` component before registering the prefab.

The production 5.1.4 bundle audit found 176 obsolete chest prefabs, 545 obsolete
spawner prefabs, and one unreferenced totem across the three removed bundles.
The shrine and waystone were the only prefabs referenced by the shipped
location bundles. The production DLL SHA-256 was
`4f44034478172fe46e847a0b5ae32fe0e8d46d7b045d9f5357e86e1fbd7485a4`.

## Local checks on 2026-09-28

- Release build and ServerSync merge passed: 0 errors, 149 warnings.
- The merged DLL contains none of the three removed embedded bundle resources.
- No C# or project references remain to the removed loaders or bundle fields.
- Candidate DLL: 60,848,128 bytes; production DLL: 79,247,872 bytes.
- Candidate DLL SHA-256:
  `ccee7760d0b6ab21d5f9369d2608659f93b2808876c7a946d83c7b7a8e53c713`.

## Valnet results on 2026-09-28

Tested production 5.1.4 and the candidate on `valnet-client-01`, using the
`praetoris-season-8` profile aligned with deployed modpack 8.0.29. The client
ran Valheim `l-1.0.16`, Unity 6000.0.75f1, and Linux BepInEx 5.4.2350. All
manifest plugin versions matched; the manifest's BepInEx 5.4.2351 framework
was not substituted for the installed Linux framework. This is a coverage
limit. No production deployment was made.

| Check | Production baseline | Candidate |
|---|---|---|
| Registered MWL names containing `_Spawner` or `_loot` | 710 | 0 |
| Loaded `moreworldlocations_prefabs_*` bundles | 3 | 0 |
| Shrine and waystone resolve by their original names | Yes | Yes |
| Persistent network component, scale 1, spawn chance 5 | Yes | Yes |
| Ward protection, area marker, player-base child | Absent | Absent |
| Correct MWL interaction component, collider, renderer, active effects | Present | Present |
| Vanilla `guard_stone` retains ward protection and area children | Yes | Yes |
| CoastTower1 references | 1 shrine, 1 waystone, 2 containers, 5 spawners | Same |

The 710 runtime count is a name-filtered registration count, not the 721
obsolete assets counted in the bundle audit. CoastTower1's five creature
references remained `Skeleton`. Both container loot lists retained their ten
item references, including feathers, arrows, coins, amber, and upgrade items.

Controlled placements exercised the real `Shrine.Interact` and
`Waystone.Interact` methods. The first shrine use applied `MWL_SE_Neck`; the
second use refused access due to the daily cooldown. The upgraded waystone
increased explored map cells from 81 to 3,625. A newly created waystone
increased them from 3,625 to 14,073.

Objects saved by production 5.1.4 loaded with the candidate at the same
positions and retained their configuration names and shrine cooldown day.
Candidate-created objects also retained these values after save, logout, and
reload. The saved map reveal remained present. These checks prove this test
world's shrine/waystone upgrade path; they do not prove compatibility with all
older saves.

With both feature settings set to `Off` while the game was stopped, the next
load removed all nearby existing shrine/waystone instances. Controlled new
spawns were also removed by the next inspection. Evidence:
`disabled-final-existing.txt`, `disabled-final-spawn.txt`, and
`disabled-final-after-spawn.txt`. Earlier files named `candidate-disabled-*`
are setup attempts, not passing evidence: the running process retained the
enabled settings and saved them again at exit.

Evidence is stored locally under
`/Users/benjmarston/Develop/artifacts/mwl-clones-20260928/`. The `baseline-*`
and `candidate-*` text files contain probe output. The probe inspects live
objects and invokes their existing interaction methods. Its source is
`MWLCloneProbe.cs`; `run_probe.py` reads multiline CLI output without truncation.
The test world and full logs are retained with the evidence.

## Cleanup

The production DLL and original CLI DLL were restored with matching SHA-256
hashes. Test helper DLLs were removed. The original configuration directory
and original world database/metadata were restored and compared with their
backups. The restored profile reached the main menu with MWL 5.1.4 loaded and
without the probe or world-generation helper. Configuration was compared again
after shutdown. The maintained profile retains its verified 8.0.29 plugin
updates; temporary candidate files are absent. The previous selected profile,
`praetoris-season-8-8.0.15`, was selected again. See `restoration.txt`,
`restored-hashes.txt`, `restored-status.txt`, and `restored-complete.log`.

## Observed limitations and separate issues

- Shrine status-effect values use shared mutable objects. Reloading or creating
  another shrine can increase the displayed bonus values or change the shared
  duration. `Shrine.Awake` adds saved values to that shared object. This code is
  unchanged by this branch and needs a separate fix.
- Baseline and candidate both emit Linux shader warnings and other mod
  warnings. Removing these three bundles does not remove all shader problems.
- The candidate's main run logged no exceptions, missing prefab hashes, or
  clone failures. ValheimEnforcer reported missing saved status-effect IDs
  `-532501972` (`MWL_SE_Boar`) and `65647743` (`MWL_SE_Neck`) on first character
  load. Jotunn registered those effects on the next world load. The unchanged
  `OnItemsRegistered` registration order is the likely cause; the initial
  baseline used a new character and did not exercise that saved-buff path.
  Saved buff restoration on the first join remains a separate open check.
- Other candidate-only messages included a 152 ms character-save warning and
  a death-screenshot upload error caused by an empty webhook URL. These are
  save timing and test-client notification configuration, respectively.
- Initial world generation used a temporary accelerator. The cached 1.0.0
  helper failed against the current game API; 1.0.1 completed generation. The
  helper was removed before candidate validation. This was a test setup issue.
- Location references were inspected in the live loaded CoastTower1 prefab.
  This run does not prove every location, natural 5% spawn distribution,
  dedicated-server authority, or multiplayer synchronization.
- `candidate-flat.png` was captured through OBS and inspected. It shows the
  glowing shrine/waystone objects alongside ordinary wards. The earlier
  screenshots are poorly framed and are not the visual proof reference.

## Valnet test procedure

1. Claim an available Valnet client. Record its previous profile, game version,
   current Season 8 modpack version, and MWL version. Use a verified current
   Season 8 test environment. Back up affected files and restore them afterward.
2. Run production MWL 5.1.4 first. Record the registered shrine, waystone, and
   obsolete prefab names, loaded bundle names, screenshots, and the log window.
   Use a disposable world with a location containing both objects, such as
   `MWL_CoastTower1`. Directly spawning the objects is an additional check, not a
   substitute for checking location reference resolution.
3. Save and close the game. Preserve a copy of that world for the upgrade test.
   Replace only the MWL DLL in the test profile with the candidate. Record its
   checksum. Keep the shipped location bundles and manifest unchanged.
4. Confirm both MWL prefab names resolve. Confirm the 721 obsolete chest/spawner
   names and `MWL_Totem_wood_8` are absent. Confirm the three old bundles are not
   loaded and that the vanilla ward remains unchanged.
5. Inspect both clones: persistent `ZNetView`; existing `Piece` and `WearNTear`;
   exactly one correct MWL interaction component; `RandomSpawn` chance 5; no
   `PrivateArea`, `AreaMarker`, or `PlayerBase`; active `WayEffect`. Confirm they
   do not become ordinary wards or add a player-base area.
6. Inspect their placement, scale, mesh, materials, light, particles, and
   collision. Capture and inspect screenshots. Check interaction text and use
   both objects. Confirm shrine effects and cooldown, and waystone map reveal
   or location marking. Check normal location loot and creature spawning.
7. Test a newly generated location. Account for the existing 5% random-spawn
   setting: one absent shrine or waystone does not prove a failure. Inspect the
   resolved prefab and use controlled placement as well as natural generation.
8. Load the world saved with 5.1.4. Check the existing shrine and waystone,
   including stored configuration and shrine cooldown. Save, exit, and reload
   with the candidate, then repeat the checks. Test feature-disabled cleanup.
9. Inspect the complete log window for missing prefabs, unresolved references,
   clone failures, exceptions, and unexpected warnings. Record any unrelated
   baseline warnings separately. If dedicated-server behavior is tested, use
   Valdev and matching candidate profiles on client and server.
10. Restore the previous profile, stop the client when required by its lease,
    and release the lease. Keep the test world and evidence for review.

## Release limitations

The candidate keeps the 5.1.4 version string for this test branch; identify it
by checksum. Do not join
production with it. A release needs a separate version change after validation.

Current locations do not reference the removed prefabs. This does not prove
that older world saves contain none of them. Check a disposable copy of an
older save for the removed names before declaring broad upgrade compatibility.
