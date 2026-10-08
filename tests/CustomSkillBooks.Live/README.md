# Custom skill book live checks

This is a temporary BepInEx test plugin. Do not ship it with MWL.

1. Build with `dotnet build tests/CustomSkillBooks.Live -c Release` on the configured Mac build host.
2. Install the test DLL alongside the candidate MWL DLL in a backed-up test profile with ImpactfulSkills 0.21.0.
3. Enter a local world with a test character and free inventory space.
4. Run `mwl_skillbooks` and check that all four ImpactfulSkills skills have three book prefab names.
5. Run `mwl_test_skillbooks`. Read the BepInEx log through `MWL_BOOK_TEST_COMPLETE failures=0`.
6. Save a custom book in the character inventory, log out, and rejoin. Confirm the item name and prefab persist.
7. Remove the test plugin and restore the profile after testing.

The 25 consumption checks use `Player.UseItem`, rather than invoking the book effect directly. They cover all tiers for four custom skills and vanilla Run, the level-100 cap, consuming a book at level 100, translated names, item consumption, and preserved experience progress. Original skill levels and progress are restored after each skill. Use a disposable test character because normal game hooks can also run during these checks.

Verified on Valnet client 02 on 2026-10-08 with Valheim l-1.0.16, the deployed Season 8 8.0.38 plugin set, Jotunn 2.30.2, and ImpactfulSkills 0.21.0. All 25 checks passed, including existing inventory stacks. A temporary trainer configuration displayed all 12 ImpactfulSkills books. Buying a Forging book deducted one coin; the book remained in the inventory after a full client restart. Discovery listed five Jotunn skills, including Deathlink, without duplicate books. The embedded SkillManager collector and dedicated-server behavior were not exercised by this client profile.
