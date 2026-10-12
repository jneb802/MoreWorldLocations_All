using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using SoftReferenceableAssets;
using UnityEngine;

namespace More_World_Locations_AIO.Dungeons;

/// <summary>Remove generated legacy stair records after static replacements are available.</summary>
[HarmonyPatch(typeof(ZNet), "Start")]
internal static class UndergroundRuinsStairMigration
{
    private const int Version = 1;
    private const float PositionTolerance = 0.05f;
    // Chunk saves truncate each Euler angle to half-degree steps. Combined error
    // can exceed half a degree after rebuilding the quaternion.
    private const float RotationTolerance = 2f;
    private static readonly int VersionKey = "mwl_underground_static_stairs".GetStableHashCode();
    private static readonly int StairHash = "stone_stair".GetStableHashCode();

    private readonly struct SavedRoom
    {
        public readonly int Hash;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;

        public SavedRoom(int hash, Vector3 position, Quaternion rotation)
        {
            Hash = hash;
            Position = position;
            Rotation = rotation;
        }
    }

    [HarmonyPostfix]
    private static void StartPostfix(ZNet __instance)
    {
        if (__instance.IsServer() && !ZNet.m_loadError)
            __instance.StartCoroutine(Migrate(__instance));
    }

    private static IEnumerator Migrate(ZNet net)
    {
        // All scene Awakes/Starts and Jotunn registrations must finish first.
        yield return null;
        while (DungeonDB.instance == null || ZNetScene.instance == null)
            yield return null;

        ZDOMan manager = ZDOMan.instance;
        List<ZDO> generators = new List<ZDO>();
        int index = 0;
        while (!manager.GetAllZDOsWithPrefabIterative("DG_BlackForestDungeon", generators, ref index))
            yield return null;

        foreach (ZDO generator in generators)
        {
            if (net == null || !net.IsServer() || ZDOMan.instance != manager) yield break;
            if (generator.GetInt(VersionKey) >= Version) continue;

            List<SavedRoom> rooms = ReadRooms(generator);
            if (rooms.Count == 0) continue;

            bool ready = true;
            foreach (int hash in rooms.Select(room => room.Hash).Distinct())
            {
                DungeonDB.RoomData data = DungeonDB.instance.GetRoom(hash);
                if (data == null || !UndergroundRuinsStairs.IsRoom(hash))
                {
                    ready = false;
                    break;
                }
                if (UndergroundRuinsStairs.Templates.ContainsKey(hash)) continue;

                bool completed = false;
                LoadResult result = default;
                SoftReference<GameObject> reference = data.m_prefab;
                reference.LoadAsync((asset, loaded) => { result = loaded; completed = true; });
                float deadline = Time.realtimeSinceStartup + 120f;
                while (!completed && Time.realtimeSinceStartup < deadline) yield return null;
                ready = completed && result == LoadResult.Succeeded
                        && UndergroundRuinsStairs.Templates.ContainsKey(hash);
                reference.Release();
                if (!ready) break;
            }
            if (!ready)
            {
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogWarning(
                    $"Underground Ruins stair migration deferred at {generator.GetPosition()}: room templates are unavailable.");
                continue;
            }

            List<UndergroundRuinsStairs.StairPose> expected = new List<UndergroundRuinsStairs.StairPose>();
            foreach (SavedRoom room in rooms)
                foreach (UndergroundRuinsStairs.StairPose stair in UndergroundRuinsStairs.Templates[room.Hash])
                    expected.Add(new UndergroundRuinsStairs.StairPose(room.Position + room.Rotation * stair.Position,
                        room.Rotation * stair.Rotation));

            List<ZDOID> removed = new List<ZDOID>();
            if (expected.Count > 0)
            {
                Vector2s center = ZoneSystem.GetZone(generator.GetPosition());
                int radius = 0;
                foreach (UndergroundRuinsStairs.StairPose pose in expected)
                {
                    Vector2s sector = ZoneSystem.GetZone(pose.Position);
                    radius = Math.Max(radius, Math.Max(Math.Abs(sector.x - center.x), Math.Abs(sector.y - center.y)));
                }
                // Refuse malformed or unexpectedly large saved layouts.
                if (radius > 16) continue;

                List<ZDO> nearby = new List<ZDO>();
                manager.FindSectorObjects(center, new SimulationDistance(radius, 0, true), nearby);
                Dictionary<Vector3Int, List<ZDO>> candidates = new Dictionary<Vector3Int, List<ZDO>>();
                foreach (ZDO zdo in nearby)
                {
                    if (!IsLegacyGeneratedStair(zdo)) continue;
                    Vector3Int key = Grid(zdo.GetPosition());
                    if (!candidates.TryGetValue(key, out List<ZDO> bucket))
                        candidates[key] = bucket = new List<ZDO>();
                    bucket.Add(zdo);
                }

                HashSet<ZDOID> scheduled = new HashSet<ZDOID>();
                foreach (UndergroundRuinsStairs.StairPose pose in expected)
                {
                    Vector3Int key = Grid(pose.Position);
                    for (int x = -1; x <= 1; x++)
                    for (int y = -1; y <= 1; y++)
                    for (int z = -1; z <= 1; z++)
                    {
                        if (!candidates.TryGetValue(key + new Vector3Int(x, y, z), out List<ZDO> bucket)) continue;
                        foreach (ZDO old in bucket)
                        {
                            if (!Matches(old, pose) || !scheduled.Add(old.m_uid)) continue;
                            // Only the server takes ownership and requests a persistent deletion.
                            old.SetOwner(ZDOMan.GetSessionID());
                            manager.DestroyZDO(old);
                            removed.Add(old.m_uid);
                        }
                    }
                }
            }

            // Do not save completion before the queued deletions have actually applied.
            float deletionDeadline = Time.realtimeSinceStartup + 30f;
            while (removed.Any(uid => manager.GetZDO(uid) != null) && Time.realtimeSinceStartup < deletionDeadline)
                yield return null;
            if (removed.Any(uid => manager.GetZDO(uid) != null)) continue;

            generator.Set(VersionKey, Version);
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
                $"Underground Ruins static stair migration at {generator.GetPosition()}: {expected.Count} authored stairs, {removed.Count} legacy records removed.");
            yield return null;
        }
    }

    internal static bool IsLegacyGeneratedStair(ZDO zdo)
    {
        return zdo.GetPrefab() == StairHash && zdo.Persistent
               && zdo.GetBool(ZDOVars.s_preSnow)
               && zdo.GetLong(ZDOVars.s_creator) == 0L
               && zdo.GetInt(ZDOVars.s_creatorIndex, -1) == -1;
    }

    private static bool Matches(ZDO zdo, UndergroundRuinsStairs.StairPose pose)
    {
        return Vector3.SqrMagnitude(zdo.GetPosition() - pose.Position) <= PositionTolerance * PositionTolerance
               && Quaternion.Angle(zdo.GetRotation(), pose.Rotation) <= RotationTolerance;
    }

    private static Vector3Int Grid(Vector3 point)
    {
        return new Vector3Int(Mathf.RoundToInt(point.x / PositionTolerance),
            Mathf.RoundToInt(point.y / PositionTolerance), Mathf.RoundToInt(point.z / PositionTolerance));
    }

    private static List<SavedRoom> ReadRooms(ZDO generator)
    {
        List<SavedRoom> rooms = new List<SavedRoom>();
        try
        {
            if (generator.GetByteArray(ZDOVars.s_roomData, out byte[] buffer))
            {
                using (BinaryReader reader = new BinaryReader(new MemoryStream(buffer)))
                {
                    int count = reader.ReadInt32();
                    if (count < 0 || count > 10000 || buffer.Length != 4 + count * 28) return rooms;
                    for (int i = 0; i < count; i++)
                        rooms.Add(new SavedRoom(reader.ReadInt32(), reader.ReadVector3(), reader.ReadQuaternion()));
                }
            }
            else
            {
                int count = generator.GetInt(ZDOVars.s_rooms);
                if (count < 0 || count > 10000) return rooms;
                for (int i = 0; i < count; i++)
                {
                    string key = "room" + i;
                    rooms.Add(new SavedRoom(generator.GetInt(key), generator.GetVec3(key + "_pos", Vector3.zero),
                        generator.GetQuaternion(key + "_rot", Quaternion.identity)));
                }
            }
            if (rooms.Any(room => !Finite(room.Position))) rooms.Clear();
        }
        catch (Exception error)
        {
            rooms.Clear();
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogWarning(
                $"Underground Ruins stair migration could not read a saved room layout: {error.Message}");
        }
        return rooms;
    }

    private static bool Finite(Vector3 point)
    {
        return !float.IsNaN(point.x) && !float.IsInfinity(point.x)
               && !float.IsNaN(point.y) && !float.IsInfinity(point.y)
               && !float.IsNaN(point.z) && !float.IsInfinity(point.z);
    }
}
