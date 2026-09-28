# Server-only baseline

Step 0 of the server-only plan: the exact source, toolchain and artifacts every
later result is measured against. Assistant-written; every hash below was taken
on this machine on 14 September 2026, and nothing here is carried over from an
earlier session.

## Source

| | |
| --- | --- |
| upstream | `jneb802/MoreWorldLocations_All` |
| pinned commit | `5546c481847e3f169e5a22c12b402db8e20c5acf` ("Merge pull request #107 from jneb802/fix/valheim-1-console-commands") |
| plugin version at that commit | 5.0.9 |
| branch | `feature/server-only`, cut from the pinned commit |
| worktree | a separate checkout, so the main one stays on `master` |

The pinned commit is `origin/master` as of this session. An earlier checkout of
this repository sat on `c228662` (5.0.7), 14 commits behind; anything read
against that checkout has to be re-read here.

## Game and dependencies

| | | |
| --- | --- | --- |
| Valheim | Steam buildid `25253764` | macOS install used for compilation |
| `assembly_valheim.dll` | `bb32454276…f4f38b18` | |
| `assembly_utils.dll` | `e5fb9f228c…d5a0e086` | |
| `SoftReferenceableAssets.dll` | `9f9f816bd0…5c6eab8c` | |
| `assembly_valheim_publicized.dll` | `129703cd3c…23b0f49b` | publicized 12 Sep, one day after the game assembly it derives from |
| BepInEx | 5.4.23.5 | |
| Jötunn (compile reference, `Libs/Jotunn.dll`) | 2.27.1 | `a2ed2b32ec…ea24c558` |
| Jötunn (installed on this machine) | 2.29.0 | `BepInEx/plugins/Jotunn/plugins/Jotunn.dll` |
| ServerSync (`Libs/ServerSync.dll`) | — | `7451b2f499…8de60573` |
| ILRepack (`tools/ILRepack.exe`) | — | `09acb91db9…9e66e2ac` |

**The bundled compile reference is older than the mod's own runtime
requirement.** `More_World_Locations_AIOPlugin.MinJotunnVersion` is 2.28.0 and
`thunderstore.toml` declares `ValheimModding-Jotunn = "2.29.1"`, but
`Libs/Jotunn.dll` is 2.27.1. The build succeeds because nothing it references
was added after 2.27.1; it means a compiling build is not evidence that the API
it will meet at runtime is the one it was compiled against. Any server-only
work that calls a newer Jötunn API has to raise the bundled reference first.

## Build

```
dotnet build "More World Locations_AIO/More World Locations_AIO.csproj" -c Release
```

Exit 0, 0 errors, 152 warnings (all pre-existing nullable-annotation warnings in
untouched files). The post-build target merges ServerSync through ILRepack under
Mono and replaces the output in place, so the artifact is the merged assembly:

    More World Locations_AIO/bin/Release/More_World_Locations_AIO.dll

Its hash changes with every source change and is recorded per experiment, not
here. The unmodified pinned source produced
`8482e6ef3ae3f4e2be45d46fe77f6003a98e9c66797357b511ea9de08aed8a9b`.

One benign warning comes from the merge step itself: *"Did not write source
server data to output assembly. Source server data is only writeable on
Windows."*

## Tests

There was no test project in this repository. `MoreWorldLocations.Tests` is new:
it compiles the shipped server-only sources directly against shims for the
Unity, Valheim and BepInEx types, so the tests exercise the code that ships
rather than a copy of it. The shims and the synthetic world are reused from
ProceduralRoads' harness (`feature/server-only`), which already stands in for
`ZoneSystem`, `ZDO`, `ZDOMan`, `TerrainComp`, `Heightmap` and `WorldGenerator`
— the same surface the pre-bake work will need.

```
MoreWorldLocations.Tests/run-tests.sh
```

runs both runtimes and exits with the runner's own status:

| runtime | why | result |
| --- | --- | --- |
| net10.0 | fast local loop | 20 passed, 0 failed |
| net48 under Mono | the mod's actual target, closest to Valheim's runtime | 20 total, 0 failed |

Verified by exit code, not by reading the output. A negative control was run
once: removing the excluded-pack guard from `ServerOnlyAllowlist` fails 5 of the
20 with exit 1, and restoring it returns to 20/20 with exit 0.

## What this repository cannot build on its own

Recorded here because it gates every runtime experiment, and a build that exits
0 hides it.

1. **The location content is not in the repository.** MWL's locations are loaded
   by soft reference from a `Bundles/` folder next to the plugin, indexed by an
   `assetBundleManifest_full` file. `plugins/Bundles/` contains only a
   `.gitkeep`, `assetBundleManifest_full` is absent, and both are `.gitignore`d.
   `thunderstore.toml` copies them into the release package, so they exist only
   in the published Thunderstore build (or in the author's Unity project), and
   no MWL package was installed on this machine.

   Resolved for the audit by fetching the published package:
   `warpalicious/More_World_Locations_AIO` **5.0.9**,
   `b7790876e86eb161d9e4b123e92e396ee167f5e44cea620b6ebe28bdfde8f3c5`, 280,263,578
   bytes, published 9 Sep 2026 — 264 bundles and the manifest
   (`879003ecd4e9…`). It is kept with the validation tooling, unpacked and
   uncommitted; it is someone else's release, not ours to vendor. A build of
   this repository still cannot produce a runnable install on its own.

2. **`assets/moreworldlocations_assetbundle_2` is an unresolved Git LFS
   pointer** (134 bytes standing in for 153,754,436), and `git lfs` is not
   installed here. It is not a blocker: no source file names any of the four
   `moreworldlocations_assetbundle_*` files, they are copied to the output
   rather than embedded, and the three that are present hold an older layout
   (`.../MeadowsPack1/mwl_ruins1.prefab`) than the paths
   `LocationDefinitions.cs` asks for (`.../Meadows/MWL_Ruins1.prefab`). They are
   legacy content, useful only as a structural sample.

3. **No stock prefab registry has been captured yet.** The audit's central
   question — can the client resolve this prefab name? — needs the name and hash
   set from a vanilla `ZNetScene` at the exact game build. Step 0 item 5 of the
   plan is therefore outstanding.

## Provenance rules kept from ProceduralRoads

* Verify by exit code, never by grepping output.
* Compare identities, not counts.
* Raw logs first, under a unique immutable name; summaries afterwards.
* Validation switches live in the environment. Config keys are for the product:
  a config key cannot be taken back once it is on a user's disk.
