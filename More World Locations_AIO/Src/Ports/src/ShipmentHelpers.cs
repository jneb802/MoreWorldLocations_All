using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace More_World_Locations_AIO;

[PublicAPI]
public static class ShipmentHelpers
{
    public static void Add(this List<ShipmentItem> list, int chestID, ItemDrop.ItemData item)
    {
        list.Add(new ShipmentItem(chestID, item));
    }

    public static void Add(this List<ShipmentItem> list, params Container[] containers)
    {
        foreach (Container container in containers) list.Add(container);
    }

    public static void Add(this List<ShipmentItem> list, Container container)
    {
        int chestID = container.m_nview.GetZDO().GetPrefab();
        List<ItemDrop.ItemData> items = container.GetInventory().GetAllItems();
        if (items.Count <= 0) return;
        foreach (ItemDrop.ItemData item in items)
        {
            list.Add(chestID, item);
        }
    }

    [Obsolete]
    public static void EmptyAll(this Container[] containers)
    {
        foreach (Container container in containers)
        {
            container.GetInventory().RemoveAll();
        }
    }

    public static void Add<T>(this List<T> list, params T[] values) => list.AddRange(values);

    public static bool HasItems(this Inventory inventory) => inventory.GetAllItems().Count > 0;
    
    public static void CopySpriteAndMaterial(this GameObject prefab, GameObject source, string childName, string sourceChildName = "")
    {
        Transform to = prefab.transform.Find(childName);
        if (to == null)
        {
            Debug.LogError($"CopySpriteAndMaterial: couldn't find child {childName} on {prefab.name}");
            return;
        }

        if (!to.TryGetComponent(out Image toImage))
        {
            Debug.LogError($"CopySpriteAndMaterial: couldn't find image on {to.name}");
            return;
        }
        
        Transform from = string.IsNullOrWhiteSpace(sourceChildName) ? source.transform : source.transform.Find(sourceChildName);
        if (from == null)
        {
            Debug.LogError($"CopySpriteAndMaterial: couldn't find child {sourceChildName} on {source.name}");
            return;
        }

        if (!from.TryGetComponent(out Image fromImage))
        {
            Debug.LogError($"CopySpriteAndMaterial: couldn't find image on {from.name}");
            return;
        }
        toImage.sprite = fromImage.sprite;
        toImage.material = fromImage.material;
        toImage.color = fromImage.color;
        toImage.type = fromImage.type;
    }
    
    public static void CopyButtonState(this GameObject prefab, GameObject source, string childName, string sourceChildName = "")
    {
        Transform? target = prefab.transform.Find(childName);
        if (target == null)
        {
            Debug.LogError($"CopyButtonState failed to find {childName} on {prefab.name}");
            return;
        }

        if (!target.TryGetComponent(out Button button))
        {
            Debug.LogError($"CopyButtonState failed to find Button component on {target.name}");
            return;
        }

        Transform sourceChild;
        if (!string.IsNullOrWhiteSpace(sourceChildName))
        {
            sourceChild = source.transform.Find(sourceChildName);
            if (sourceChild == null)
            {
                Debug.LogError($"CopyButtonState failed to find {sourceChildName} on {source.name}");
                return;
            }
        }
        else
        {
            sourceChild = source.transform;
        }

        if (!sourceChild.TryGetComponent(out Button sourceButton))
        {
            Debug.LogError($"CopyButtonSprite {sourceChild} missing Button component");
            return;
        }
        button.spriteState = sourceButton.spriteState;
    }

    public static void AddRange<T, V>(this Dictionary<T, V> dict, Dictionary<T, V> otherDict)
    {
        foreach (KeyValuePair<T, V> kvp in otherDict)
        {
            dict[kvp.Key] = kvp.Value;
        }
    }

    /// <summary>
    /// Check if item has icons, since ItemData <exception cref="ItemDrop.ItemData.GetIcon"></exception> does not check
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static bool IsValid(this ItemDrop.ItemData item)
    {
        return item.m_shared.m_icons.Length > 0;
    }
}