using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using More_World_Locations_AIO.Managers;
using UnityEngine;

namespace More_World_Locations_AIO;

[PublicAPI]
public class Manifest
{
    public static readonly Dictionary<int, Manifest> Manifests = new();
    public string Name;
    public int ChestStableHashCode;
    public Container Chest;
    public ManifestRecipe Recipe = new();
    public string RequiredDefeatKey = "";
    public int CostToShip = 50;
    public Sprite? Icon;
    public EffectList? PlaceEffect;

    public bool IsPurchased;
    private static StringBuilder sb = new StringBuilder();

    private string _creatureName = string.Empty;
    private string CreatureName
    {
        get
        {
            if (string.IsNullOrEmpty(_creatureName) && DefeatKeyToCreatureMap.TryGetValue(RequiredDefeatKey, out var sharedName))
                _creatureName = sharedName ?? string.Empty;
            // cache result
            return _creatureName;
        }
    }

    private static Dictionary<string, string> _defeatKeyToCreatureMap = new();

    private static Dictionary<string, string> DefeatKeyToCreatureMap
    {
        get
        {
            if (_defeatKeyToCreatureMap.Count > 0 || !ZNetScene.instance) return _defeatKeyToCreatureMap;
            foreach (GameObject prefab in ZNetScene.instance.m_prefabs)
            {
                // find all characters
                if (!prefab.TryGetComponent(out Character component)) continue;
                // if they have a defeat key
                if (string.IsNullOrEmpty(component.m_defeatSetGlobalKey)) continue;
                // cache defeat key to shared name
                string sharedName = component.m_name;
                _defeatKeyToCreatureMap[component.m_defeatSetGlobalKey] = sharedName;
            }
            return _defeatKeyToCreatureMap;
        }
    }

    public Manifest(string name, Container container)
    {
        Name = name;
        Chest = container;
        ChestStableHashCode = container.name.GetStableHashCode();
        if (Manifests.ContainsKey(ChestStableHashCode))
        {
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug($"{Name}: {Chest.name} is already registered");
            return;
        }
        Manifests[ChestStableHashCode] = this;
    }
    
    public string GetTooltip()
    {
        int size = Chest.m_width * Chest.m_height;
        sb.Clear();
        if (!string.IsNullOrEmpty(CreatureName)) sb.Append($"\n{LocalKeys.RequiredToDefeat}: <color=yellow>{CreatureName}</color>");
        sb.Append($"\n{LocalKeys.Capacity}: <color=yellow>{size}</color>");
        sb.Append($"\n{LocalKeys.CostToShip}: <color=yellow>{CostToShip}</color>");
        return sb.ToString();
    }

    public static void ResetPurchasedManifests()
    {
        foreach (Manifest manifest in Manifests.Values) manifest.IsPurchased = false;
    }
    
    public class ManifestRecipe
    {
        public readonly List<Requirement> Requirements = new();

        public void Add(string itemName, int amount)
        {
            if (Requirements.Count >= 4) return;
            if (Helpers.GetPrefab(itemName) is not { } itemPrefab) return;
            if (!itemPrefab.TryGetComponent(out ItemDrop component)) return;
            Requirements.Add(new Requirement()
            {
                Item = component.m_itemData,
                Amount = amount
            });
        }
    }
    
    public record struct Requirement
    {
        public ItemDrop.ItemData Item;
        public int Amount;
    }
}

public static class ManifestHelpers
{
    public static Manifest.Requirement? GetRequirement(this Manifest manifest, int index)
    {
        if (index < 0 || index >= manifest.Recipe.Requirements.Count) return null;
        return manifest.Recipe.Requirements[index];
    }

    public static bool IsKnownManifest(this Player player, Manifest manifest)
    {
        if (player.NoCostCheat()) return true;
        return manifest.Recipe.Requirements.All(requirement => player.IsKnownMaterial(requirement.Item.m_shared.m_name));
    }

    public static void Purchase(this Player player, Manifest manifest)
    {
        if (!player.NoCostCheat())
        {
            Inventory inventory =  player.GetInventory();
            foreach (var requirement in manifest.Recipe.Requirements)
            {
                inventory.RemoveItem(requirement.Item.m_shared.m_name, requirement.Amount);
            }
        }
        manifest.IsPurchased = true;
    }

    public static bool HasRequirements(this Player player, Manifest manifest)
    {
        if (player.NoCostCheat()) return true;
        if (!string.IsNullOrEmpty(manifest.RequiredDefeatKey))
        {
            if (!ZoneSystem.instance.GetGlobalKey(manifest.RequiredDefeatKey) &&
                !player.GetUniqueKeys().Contains(manifest.RequiredDefeatKey))
                return false;
        }
        Inventory inventory = player.GetInventory();
        foreach (Manifest.Requirement requirement in manifest.Recipe.Requirements)
        {
            var count = inventory.CountItems(requirement.Item.m_shared.m_name);
            if (count >= requirement.Amount) continue;
            return false;
        }
        return true;
    }
}