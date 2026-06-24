using UnityEngine;

namespace More_World_Locations_AIO.Dungeons;

internal static class DraugrPileSpawnerZdoPatch
{
    private const string PrefabName = "Spawner_DraugrPile";

    internal static bool SkipPersistedDraugrPileSpawnerZDO(ZDO zdo, ref GameObject? __result)
    {
        if (zdo.GetPrefab() != PrefabName.GetStableHashCode())
        {
            return true;
        }

        __result = null;
        Debug.Log($"DraugrPileSpawnerZdoPatch: skipped persisted '{PrefabName}' ZDO {zdo.m_uid}");
        return false;
    }
}
