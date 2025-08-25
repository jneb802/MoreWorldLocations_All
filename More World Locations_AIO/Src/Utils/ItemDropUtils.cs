using UnityEngine;

namespace More_World_Locations_AIO.Utils;

public class ItemDropUtils
{
    public static ItemDrop Clone(ItemDrop sourceItemDrop, ItemDrop targetItemDrop)
    {
        targetItemDrop.m_myIndex = sourceItemDrop.m_myIndex;
        targetItemDrop.m_autoPickup = sourceItemDrop.m_autoPickup;
        targetItemDrop.m_autoDestroy = sourceItemDrop.m_autoDestroy;
        targetItemDrop.m_itemData.m_quality = sourceItemDrop.m_itemData.m_quality;
        targetItemDrop.m_itemData = sourceItemDrop.m_itemData.Clone();
        targetItemDrop.m_onDrop = sourceItemDrop.m_onDrop;
        targetItemDrop.m_pieceEnableObj = sourceItemDrop.m_pieceEnableObj;
        targetItemDrop.m_pieceDisabledObj = sourceItemDrop.m_pieceDisabledObj;
        targetItemDrop.m_nameHash = sourceItemDrop.m_nameHash;
        targetItemDrop.m_floating = sourceItemDrop.m_floating;
        targetItemDrop.m_body = sourceItemDrop.m_body;
        targetItemDrop.m_nview = sourceItemDrop.m_nview;
        targetItemDrop.m_pickupRequester = sourceItemDrop.m_pickupRequester;
        targetItemDrop.m_lastOwnerRequest = sourceItemDrop.m_lastOwnerRequest;
        targetItemDrop.m_ownerRetryCounter = sourceItemDrop.m_ownerRetryCounter;
        targetItemDrop.m_ownerRetryTimeout = sourceItemDrop.m_ownerRetryTimeout;
        targetItemDrop.m_spawnTime = sourceItemDrop.m_spawnTime;
        targetItemDrop.m_piece = sourceItemDrop.m_piece;
        targetItemDrop.m_wnt = sourceItemDrop.m_wnt;
        targetItemDrop.m_loadedRevision = sourceItemDrop.m_loadedRevision;
        targetItemDrop.m_haveAutoStacked = sourceItemDrop.m_haveAutoStacked;

        return targetItemDrop;
    }
}