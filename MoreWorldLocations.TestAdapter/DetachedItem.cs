using System;
using UnityEngine;

namespace MoreWorldLocations.TestAdapter;

// A prefab's unspawned ItemData can lack m_dropPrefab. Inventory.AddItem assigns
// it for real inventory items; detached shipment fixtures must do the same.
internal static class DetachedItem
{
    internal static ItemDrop.ItemData Create(ItemDrop.ItemData prototype, GameObject prefab, int count)
    {
        if (prototype == null || prefab == null || count < 1) throw new ArgumentException("A prefab, prototype and positive stack are required.");
        var item = prototype.Clone();
        item.m_dropPrefab = prefab;
        item.m_stack = count;
        return item;
    }
}
