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
- Live baseline reproduction and candidate validation are pending. Both Valnet
  clients were leased for community chest validation. No device was changed.

## Valnet test procedure

1. Claim an available Valnet client. Record its previous profile, game version,
   current Season 8 modpack version, and MWL version. Use a separate copy of the
   verified current Season 8 profile. Preserve the maintained baseline.
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

Do not treat the successful build as live validation. The candidate keeps the
5.1.4 version string for this test branch; identify it by checksum. Do not join
production with it. A release needs a separate version change after validation.

Current locations do not reference the removed prefabs. This does not prove
that older world saves contain none of them. Check a disposable copy of an
older save for the removed names before declaring broad upgrade compatibility.
