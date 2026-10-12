using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.Dungeons;

/// <summary>Convert legacy room stairs after mock resolution, before room placement.</summary>
public static class UndergroundRuinsStairs
{
    public const string PrefabName = "BFD_Kit_stone_stair";
    internal static readonly Dictionary<int, StairPose[]> Templates = new Dictionary<int, StairPose[]>();

    internal readonly struct StairPose
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;

        public StairPose(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }

    internal static bool IsRoom(string name)
    {
        return RoomDefinitions.UndergroundRuins.Any(room => room.Name == name);
    }

    internal static bool IsRoom(int hash)
    {
        return RoomDefinitions.UndergroundRuins.Any(room => room.Name.GetStableHashCode() == hash);
    }

    public static void PrepareRoom(string name, GameObject room)
    {
        if (!IsRoom(name)) return;

        GameObject stairs = PrefabManager.Instance.GetPrefab(PrefabName);
        if (stairs == null)
            throw new InvalidOperationException($"Underground Ruins stair template '{PrefabName}' is not registered.");

        List<StairPose> poses = new List<StairPose>();
        // Snapshot the transforms because replacements change the hierarchy.
        foreach (Transform transform in room.GetComponentsInChildren<Transform>(true))
        {
            if (transform == null) continue;
            string objectName = transform.name;
            bool legacy = objectName == "stone_stair" || objectName == "JVLmock_stone_stair"
                          || objectName.StartsWith("stone_stair (", StringComparison.Ordinal)
                          || objectName.StartsWith("JVLmock_stone_stair (", StringComparison.Ordinal);
            if (!legacy && objectName != PrefabName) continue;

            // Use the same world offset as DungeonGenerator.PlaceRoom.
            poses.Add(new StairPose(Quaternion.Inverse(room.transform.rotation) * (transform.position - room.transform.position),
                Quaternion.Inverse(room.transform.rotation) * transform.rotation));
            if (!legacy) continue;

            GameObject replacement = UnityEngine.Object.Instantiate(stairs, transform.parent, false);
            replacement.name = PrefabName;
            replacement.transform.localPosition = transform.localPosition;
            replacement.transform.localRotation = transform.localRotation;
            replacement.transform.localScale = transform.localScale;
            replacement.SetActive(transform.gameObject.activeSelf);
            UnityEngine.Object.DestroyImmediate(transform.gameObject);
        }

        Templates[name.GetStableHashCode()] = poses.ToArray();
    }
}
