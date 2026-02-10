Run a pre-publish validation check for the MWL_AIO Thunderstore package. Check each item below and report results as PASS, WARN, or FAIL. Do NOT auto-publish — just report and suggest commands.

## Validation Checks

### 1. DLL exists
Check if `./More World Locations_AIO/bin/Release/More_World_Locations_AIO.dll` exists.
- FAIL if missing.

### 2. DLL recently built
Check the modification date of the DLL.
- WARN if older than 1 hour.

### 3. DLL size reasonable
Check the DLL file size.
- WARN if less than 1MB (suspiciously small for MWL_AIO).

### 4. Version consistency
Compare the version string across these three sources:
- **C# source**: `ModVersion` constant in `./More World Locations_AIO/Src/MoreWorldLocations.cs`
- **thunderstore.toml**: `versionNumber` field
- **CHANGELOG.md**: First version entry in the table
- FAIL if any mismatch. Offer to update `thunderstore.toml` to match the C# source version.

### 5. CHANGELOG updated
Verify that the version from the C# source has a matching entry in `CHANGELOG.md`.
- FAIL if the current version has no changelog entry.
- Prompt the user to add one.

### 6. README freshness
Check if `README_thunderstore.md` exists and report its last modification date.
- WARN if not modified in the last 7 days (just informational).

### 7. Icon valid
Check that `./icon.png` exists. Use `file` command to verify it is a PNG and check dimensions.
- FAIL if missing or not PNG.
- WARN if not 256x256.

### 8. Dependencies
Read `thunderstore.toml` and list the dependency packages. For each dependency, use `curl` to check if it exists on the Thunderstore API: `https://thunderstore.io/api/experimental/package/{namespace}/{name}/`
- WARN if any dependency cannot be verified (API may be unavailable).

### 9. Bundles present (MWL_AIO-specific)
Check if `./plugins/Bundles/` exists and has files in it.
- WARN if empty or missing (bundles must be staged from Unity before building).
- Report the file count.

### 10. Bundle count
Count files in `./plugins/Bundles/`.
- WARN if count differs from expected 158 (informational only — count may change with updates).

### 11. assetBundleManifest_full exists
Check if `./assetBundleManifest_full` exists at the repo root.
- WARN if missing (must be copied from Unity build before packaging).

### 12. plugins/ folder structure
Verify that `./plugins/Bundles/` nesting is correct (Bundles must be inside plugins, not at root).
- FAIL if `./Bundles/` exists at root instead of inside `./plugins/`.

## Output Format

Print a summary table:

```
Pre-Publish Check Results
=========================
  1. DLL exists              [PASS/FAIL]
  2. DLL recently built      [PASS/WARN]  (age: Xm)
  3. DLL size                [PASS/WARN]  (size: X MB)
  4. Version consistency     [PASS/FAIL]  (version: X.Y.Z)
  5. CHANGELOG entry         [PASS/FAIL]
  6. README freshness        [PASS/WARN]  (modified: date)
  7. Icon valid              [PASS/FAIL]  (256x256 PNG)
  8. Dependencies            [PASS/WARN]
  9. Bundles present         [PASS/WARN]  (count: N)
 10. Bundle count            [PASS/WARN]  (expected: 158, found: N)
 11. Manifest file           [PASS/WARN]
 12. Folder structure        [PASS/FAIL]
```

## Fix Capabilities
- If version mismatch (check 4): offer to update `thunderstore.toml` versionNumber to match the C# source.
- If CHANGELOG missing entry (check 5): tell the user what version needs an entry.
- If README is stale (check 6): note it as informational.

## Final Output
After the summary:
- If any FAIL: list what must be fixed before building.
- If only WARN or PASS: print the build and publish commands:
  ```
  tcli build --config-path ./thunderstore.toml
  tcli publish --config-path ./thunderstore.toml
  ```
- Remind the user: "Never auto-publish. Review the build output in ./build/ before publishing."
