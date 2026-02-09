using System.Collections.Generic;
using UnityEngine;

namespace More_World_Locations_AIO.Sealstone;

public static class SealstoneHelper
{
    public static GameObject FindGameObjectInSector(string prefabName, Vector3 position)
    {
        int prefabHash = prefabName.GetStableHashCode();

        Vector2i sector = ZoneSystem.GetZone(position);
        int sectorIndex = ZDOMan.instance.SectorToIndex(sector);

        if (sectorIndex < 0 || sectorIndex >= ZDOMan.instance.m_objectsBySector.Length)
            return null;

        List<ZDO> sectorList = ZDOMan.instance.m_objectsBySector[sectorIndex];
        if (sectorList == null)
            return null;

        foreach (ZDO zdo in sectorList)
        {
            if (zdo.GetPrefab() != prefabHash)
                continue;

            GameObject instance = ZNetScene.instance.FindInstance(zdo)?.gameObject;
            if (instance != null)
                return instance;
        }

        return null;
    }
}
