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
    public RoomConfig Config { get; set; }

    public void Register()
    {
        // Jotunn's AddCustomRoom rejects rooms whose ThemeName is neither a vanilla Room.Theme
        // value nor already in DungeonManager.themeList. The theme component on the dungeon
        // exterior prefab is attached later (when bfd_exterior resolves), which is too late
        // for this startup-time check. Pre-register the theme name here so the validation passes.
        EnsureThemeRegistered(Config.ThemeName);

        SoftReference<GameObject> softReferencePrefab =
            AssetManager.Instance.GetSoftReference<GameObject>(Name);

        AssetManager.Instance.ResolveMocksOnLoad(
            softReferencePrefab.m_assetID,
            null,
            null);

        CustomRoom customRoom = new CustomRoom(softReferencePrefab, true, Config);
        DungeonManager.Instance.AddCustomRoom(customRoom);
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
