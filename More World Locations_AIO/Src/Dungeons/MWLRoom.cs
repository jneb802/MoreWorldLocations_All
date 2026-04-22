using System.Collections.Generic;
using System.Reflection;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using SoftReferenceableAssets;
using UnityEngine;

namespace More_World_Locations_AIO.Dungeons;

public class MWLRoom
{
    public string Name { get; set; }
    public string AssetPath { get; set; }
    public int PlacementLimit { get; set; } = 0;
    public RoomConfig Config { get; set; }

    public void Register()
    {
        // Jotunn's AddCustomRoom rejects rooms whose ThemeName is neither a vanilla Room.Theme
        // value nor already in DungeonManager.themeList. The dungeon-exterior prefab's Theme
        // component is attached later when bfd_exterior resolves (too late for this check),
        // so we pre-register the theme name here to satisfy validation.
        EnsureThemeRegistered(Config.ThemeName);

        SoftReference<GameObject> softRef = AssetManager.Instance.GetSoftReference<GameObject>(Name);
        if (!softRef.IsValid)
        {
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogError(
                $"SoftReference invalid for room prefab: {Name}");
            return;
        }

        int placementLimit = PlacementLimit;
        AssetManager.Instance.ResolveMocksOnLoad<GameObject>(softRef, null, prefab =>
        {
            if (prefab == null) return;

            if (placementLimit > 0 && prefab.GetComponent<RoomExtras>() == null)
            {
                RoomExtras extras = prefab.AddComponent<RoomExtras>();
                extras.PlacementLimit = placementLimit;
            }

            SwapEggHatchCreature(prefab, "TentaRoot");
        });

        CustomRoom customRoom = new CustomRoom(softRef, fixReference: true, Config);
        DungeonManager.Instance.AddCustomRoom(customRoom);
    }

    // EggHatch.m_spawnPrefab points to a prefab that carries a CreatureSpawner; swap that
    // spawner's creature so hatched eggs in this room spawn the chosen creature.
    private static void SwapEggHatchCreature(GameObject roomPrefab, string creatureName)
    {
        EggHatch[] eggHatches = roomPrefab.GetComponentsInChildren<EggHatch>();
        if (eggHatches.Length == 0) return;

        GameObject creaturePrefab = PrefabManager.Cache.GetPrefab<GameObject>(creatureName);
        if (creaturePrefab == null)
        {
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogError(
                $"Could not find creature prefab '{creatureName}' for EggHatch swap");
            return;
        }

        foreach (EggHatch eggHatch in eggHatches)
        {
            CreatureSpawner spawner = eggHatch.m_spawnPrefab?.GetComponent<CreatureSpawner>();
            if (spawner != null) spawner.m_creaturePrefab = creaturePrefab;
        }
    }

    private static FieldInfo _themeListField;

    private static void EnsureThemeRegistered(string themeName)
    {
        if (string.IsNullOrEmpty(themeName)) return;
        if (CustomRoom.IsVanillaTheme(themeName)) return;

        if (_themeListField == null)
        {
            _themeListField = typeof(DungeonManager).GetField(
                "themeList", BindingFlags.Instance | BindingFlags.NonPublic);
        }
        if (_themeListField == null) return;

        List<string> themeList = _themeListField.GetValue(DungeonManager.Instance) as List<string>;
        if (themeList == null) return;

        if (!themeList.Contains(themeName)) themeList.Add(themeName);
    }
}
