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
        SoftReference<GameObject> softReferencePrefab =
            AssetManager.Instance.GetSoftReference<GameObject>(Name);

        AssetManager.Instance.ResolveMocksOnLoad(
            softReferencePrefab.m_assetID,
            null,
            null);

        CustomRoom customRoom = new CustomRoom(softReferencePrefab, true, Config);
        DungeonManager.Instance.AddCustomRoom(customRoom);
    }
}
