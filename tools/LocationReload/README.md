# MWL location reload development plugin

This separate BepInEx plugin adds `mwl_reload <location name>` for local development worlds. It is not part of the public MWL package.

## Install

Build `LocationReload.csproj` with `dotnet build -c Release`. Copy `bin/Release/net48/MWL.LocationReload.dll` into the development profile's `BepInEx/plugins` directory. Start Valheim once to load the plugin. MWL and Jotunn 2.30.2 must already be installed.

The project defaults to the macOS Steam installation for assembly references. Override `GameManaged` and `BepInExCore` with MSBuild properties for another installation.

## Use

1. Enter a disposable local development world.
2. Enable `devcommands` if it is not already enabled.
3. Spawn the initial test instance with `location MWL_Ruins1`.
4. Edit the prefab in Unity and build its bundle.
5. Replace the installed `Bundles/mwl_ruins1` file after the build finishes.
6. Stand within 100 metres of the test instance and run `mwl_reload MWL_Ruins1`.

The command identifies the preview's networked pieces through their retained asset references, then removes those pieces and the location proxy. This also identifies vegetation after it snaps to terrain. It releases the target's temporary cache and Jotunn reference, waits for the bundle to unload, then loads and spawns the replacement. It retains the original position, rotation, and random seed. It does not clear all objects in a radius.

## Scope

- Local worlds only, with no connected peers.
- One loaded instance of the selected location.
- A preview spawned during the current session. Saved-world pieces do not retain a reliable location ownership record, so the command refuses those instances.
- Existing asset path, bundle name, and dependency mappings.
- No generated dungeon interiors.
- No player-created target pieces or unknown asset references. Unrelated objects are retained, including nearby player builds.
- The bundle must not be shared with another loaded asset.
- Pieces must remain within the preview's exterior radius plus five metres (at least 15 metres). Vertical movement from terrain snapping is supported.
- The command currently requires Jotunn 2.30.2 because it releases the specific reference retained by that version's `PrepareLocation` callback.
- Changes to separately registered custom network prefabs, SoftRef manifests, or bundle dependencies are outside this workflow. The validated edit changes a location's arrangement of existing vanilla mock pieces.

Use the ordinary `location` command without `SAVE` to set up the preview. That command disables world saving for the session. Changes belong in the Unity prefab, not in a saved development world.

If an error occurs after removal, the command reports failure rather than forcing remaining reference counts to zero. Restore a valid bundle, return to the menu, and enter the test world again before spawning a new preview.

## Live validation: 2026-10-13

Tested on Valnet client 01 in the local `TestingWorld`, with Valheim `l-1.0.16`, MWL 5.2.0, Jotunn 2.30.2, and client package versions aligned with Praetoris Season 8 8.0.42. The BepInEx startup banner reported 5.4.2350 rather than the manifest's 5.4.2351. Exact bootstrap-package parity was not established. A read-only observer recorded prefab identity, marker count, and wall count. Valdev was not changed; the requested environment was clarified to a local Valnet world.

The edited bundle adds eight stone walls under `MWL_RELOAD_MARKER_V2`. All successful cycles below used the same game process, PID 8455, started at 12:50:50 local time. The character remained in the world throughout.

| Step | Prefab instance | Marker count | `stone_wall_2x1` count |
|---|---:|---:|---:|
| Initial preview | -93456 | 0 | 16 |
| Reload original bundle | -114874 | 0 | 16 |
| Copy edited bundle and reload | -136438 | 1 | 24 |
| Reload edited bundle again | -161756 | 1 | 24 |
| Restore original bundle and reload | -186722 | 0 | 16 |

Each cycle retained position `(0, 30, 10)` and seed `42295`. All 40 original stone-wall positions matched after the edited reload; eight new positions appeared. Restoring the original returned to exactly the original 40 wall positions. A separately spawned `Rock_3` at X=14, Z=10 survived all cycles, including its normal terrain snapping.

Missing-file and invalid-file checks left the preview intact. With two loaded previews, the command refused to remove either one. The test was staged above water to isolate geometry changes; it does not validate natural terrain placement or saved-world replacement.

The first prototype missed the references stored directly on `ZNetView`; its reference check refused removal. The second prototype refused a repeated reload after a bush snapped to terrain. The final implementation counts both holder types and identifies pieces by reference ownership. The table records the final implementation.

The final reload window had no reload exceptions. Loading the Windows-built edited bundle on Linux emitted 14 shader-platform errors per load, the same error category present during baseline startup. Screenshots showed the expected geometry. Separate `PieceManager.BuildPiece.SnapshotPiece` errors occurred during logout in the full mod profile. This is not a clean-log claim or a multiplayer validation.

Local evidence: `/Users/benjmarston/Develop/mwl-command-evidence-20261013`. The `client` subdirectory contains command transcripts, screenshots, package-version checks, and logs. No Unity project was copied or edited for this test; it reused the reversible bundle edit from the earlier menu/re-entry validation.

After testing, all 664 original profile files and 468 world files matched their backup SHA-256 hashes, with no added files. The restored profile reached the main menu. Files changed by that verification launch were restored and checked again. OBS configuration matched its backup, the temporary character was removed, and the previous `praetoris-season-8-8.0.15` profile selection was restored. Temporary backups and deployed test helpers were removed.
