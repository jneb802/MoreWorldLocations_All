# Underground Ruins static stairs

Underground Ruins used the vanilla `stone_stair` build piece inside its rooms.
These stairs had `WearNTear` and `ZNetView`. Their saved `preSnow` flag allowed
support checks before the room finished loading. The game could destroy the
stairs and delete their saved network records.

## Room loading

The pack registers `BFD_Kit_stone_stair` as a clone of `stone_stair`.
The clone retains the mesh and colliders. It removes `Piece`, `WearNTear`, and
`ZNetView`, including those components on children.

After Jotunn resolves a registered Underground Ruins room, `PrepareRoom`
replaces its legacy stairs with this clone. It keeps each stair's parent,
position, rotation, scale, and active state. The existing bundle therefore
works without an asset rebuild. The same room template serves new generation
and saved room reloads.

The scope is interior room stairs. Exterior stairs, other build pieces, and
functional objects are outside this change.

## Saved worlds

The server reads saved `DG_BlackForestDungeon` room layouts after world load.
It loads each room template before it removes any old records.

A removable record must satisfy all of these conditions:

- Its prefab is `stone_stair` and it is persistent.
- It has the generation flag `preSnow`.
- It has no player creator or creator index.
- Its position is within 5 cm of an authored stair position in that layout.
- Its rotation is within 2 degrees of that authored stair rotation. Chunk saves
  truncate each Euler angle to half-degree steps, so a half-degree quaternion
  tolerance is too small.

The server confirms the queued deletions before it records migration version 1
on the dungeon generator. Unavailable rooms defer the migration. Invalid or
unexpectedly large layouts cause no deletions.

Previously missing stairs return from the authored room template when the
client loads the room. The migration does not reconstruct network records.

Update the server and all clients together when releasing this change. An old
client still uses the networked room stairs and cannot display their static
replacements after migration. Apply the normal release version increment and
client version enforcement before a production deployment.

Keep a world backup before deployment. After migration, reverting the DLL alone
does not restore the removed network records. A rollback to the old room stairs
requires the corresponding world backup.

## Validation on 2026-10-12

The original failure was reproduced in the production mod set before this
change. All 230 stairs in one saved dungeon disappeared during reload.

The candidate was tested on Valdev and a Valnet client. The maintained Season 8
profiles were backed up. Their mod DLLs were aligned with the deployed 8.0.41
release, then the candidate and a temporary diagnostic plugin were added.
The world was a private copy of the intact October 3 production backup.
Production files were unchanged.

| Check | Result |
| --- | --- |
| Release rebuild | 0 errors; 148 existing warnings |
| Intact saved dungeon migration | All 230 generated stair records removed |
| Client room load | 230 static stairs; 230 enabled colliders; no build, wear, or network component on stair roots |
| Player in the interior | Player settled on the interior floor at about height 5061.8 |
| Leave the area | Static stair objects unloaded |
| Return to the area | All 230 static stairs loaded again |
| Fresh generation through `DungeonGenerator.Generate` | 82 static stairs; 82 enabled colliders; no networked stair records |
| Damaged saved dungeon migration | Layout contained 297 authored stairs; 13 remaining legacy records removed |
| Full backup migration | 28 dungeons; 6,095 matching legacy records removed |
| Preservation fixtures | Player-built stair at an authored position and unrelated generated stair both retained |
| Logout and rejoin | 230 static stairs and both protected records remained |
| Save and server restart | Migration version 1 persisted; both protected records remained; migration did not repeat |

The full backup test server stopped after the migration and its SSH access
temporarily timed out. The cause of that server stop is unconfirmed. The
preservation, logout/rejoin, and restart checks used a 37,860-record slice of
the same backup with the same mod set. Full-world restart validation remains
incomplete.
