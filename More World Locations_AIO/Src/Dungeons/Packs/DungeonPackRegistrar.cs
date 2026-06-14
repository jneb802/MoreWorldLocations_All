using System.Collections.Generic;
using System.Linq;
using Jotunn;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.Dungeons.Packs;

public static class DungeonPackRegistrar
{
    public static void Register(MWLDungeonPack pack)
    {
        if (pack == null) return;

        ValidatePackDefinition(pack);

        AssetBundle? bundle = pack.Bundle?.Invoke();

        foreach (KitClone clone in pack.KitClones)
        {
            RegisterKitClone(clone);
        }

        foreach (CustomPrefabSpec custom in pack.CustomPrefabs)
        {
            RegisterCustomPrefab(bundle, custom, pack.Name);
        }

        if (pack.DungeonGeneratorPrefab != null)
        {
            RegisterCustomPrefab(bundle, pack.DungeonGeneratorPrefab, pack.Name);
        }

        VerifyRegisteredPrefabs(pack);
    }

    private static void RegisterKitClone(KitClone clone)
    {
        if (string.IsNullOrEmpty(clone.Name) || string.IsNullOrEmpty(clone.VanillaSource))
        {
            Debug.LogWarning("DungeonPackRegistrar: KitClone with missing Name or VanillaSource");
            return;
        }

        if (PrefabManager.Instance.GetPrefab(clone.Name) != null)
        {
            PrefabManager.Instance.RemovePrefab(clone.Name);
        }

        GameObject cloned = PrefabManager.Instance.CreateClonedPrefab(clone.Name, clone.VanillaSource);
        if (cloned == null)
        {
            Debug.LogWarning(
                $"DungeonPackRegistrar: failed to clone vanilla '{clone.VanillaSource}' for '{clone.Name}'");
            return;
        }

        KitStripper.Strip(cloned, clone.Strip);
        clone.Configure?.Invoke(cloned);

        CustomPrefab customPrefab = new CustomPrefab(cloned, fixReference: false);
        PrefabManager.Instance.AddPrefab(customPrefab);
    }

    private static void RegisterCustomPrefab(AssetBundle? bundle, CustomPrefabSpec spec, string packName)
    {
        if (string.IsNullOrEmpty(spec.Name))
        {
            Debug.LogWarning($"DungeonPackRegistrar: CustomPrefabSpec with empty Name in pack '{packName}'");
            return;
        }

        if (PrefabManager.Instance.GetPrefab(spec.Name) != null)
        {
            PrefabManager.Instance.RemovePrefab(spec.Name);
        }

        GameObject? prefab = null;

        switch (spec.Source)
        {
            case CustomPrefabSource.VanillaClone:
                if (string.IsNullOrEmpty(spec.VanillaSource))
                {
                    Debug.LogWarning(
                        $"DungeonPackRegistrar: VanillaClone '{spec.Name}' missing VanillaSource");
                    return;
                }
                prefab = PrefabManager.Instance.CreateClonedPrefab(spec.Name, spec.VanillaSource);
                if (prefab == null)
                {
                    Debug.LogWarning(
                        $"DungeonPackRegistrar: failed to clone '{spec.VanillaSource}' for '{spec.Name}'");
                    return;
                }
                break;

            case CustomPrefabSource.Bundled:
                if (bundle == null)
                {
                    Debug.LogWarning(
                        $"DungeonPackRegistrar: pack '{packName}' has no bundle; cannot load '{spec.Name}'");
                    return;
                }
                string assetName = string.IsNullOrEmpty(spec.BundleAsset) ? spec.Name : spec.BundleAsset;
                prefab = bundle.LoadAsset<GameObject>(assetName);
                if (prefab == null)
                {
                    Debug.LogWarning(
                        $"DungeonPackRegistrar: bundle has no asset '{assetName}' for '{spec.Name}'");
                    return;
                }
                break;
        }

        if (prefab == null)
        {
            Debug.LogWarning(
                $"DungeonPackRegistrar: prefab '{spec.Name}' resolved to null in pack '{packName}'");
            return;
        }

        KitStripper.Strip(prefab, spec.Strip);
        spec.Configure?.Invoke(prefab);

        bool clonedFromCustomPrefab = spec.Source == CustomPrefabSource.VanillaClone
                                      && !string.IsNullOrEmpty(spec.VanillaSource)
                                      && CustomPrefab.IsCustomPrefab(spec.VanillaSource);
        if (clonedFromCustomPrefab)
        {
            // Clones created from bundled custom prefabs inherit unresolved JVLmock_
            // references unless we fix the clone before it is registered.
            prefab.FixReferences(recursive: true);
        }

        // Bundled assets need fixReference=true so JVLmock_ children resolve.
        // Clones sourced from bundled custom prefabs are fixed above.
        bool fixReference = spec.Source == CustomPrefabSource.Bundled;

        CustomPrefab customPrefab = new CustomPrefab(prefab, fixReference);
        PrefabManager.Instance.AddPrefab(customPrefab);
    }

    private static void ValidatePackDefinition(MWLDungeonPack pack)
    {
        string packName = string.IsNullOrEmpty(pack.Name) ? "<unnamed>" : pack.Name;
        string[] definedNames = GetDefinedPrefabNames(pack);

        string[] duplicateNames = definedNames
            .Where(name => !string.IsNullOrEmpty(name))
            .GroupBy(name => name, System.StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(name => name)
            .ToArray();

        if (duplicateNames.Length > 0)
        {
            Debug.LogWarning(
                $"DungeonPackRegistrar: pack '{packName}' defines duplicate prefab names: {string.Join(", ", duplicateNames)}");
        }

        string[] expectedNames = NormalizeNames(pack.ExpectedPrefabNames);
        if (expectedNames.Length == 0)
        {
            return;
        }

        HashSet<string> definedSet = new(definedNames, System.StringComparer.Ordinal);
        HashSet<string> expectedSet = new(expectedNames, System.StringComparer.Ordinal);

        string[] missingFromDefinition = expectedSet
            .Where(name => !definedSet.Contains(name))
            .OrderBy(name => name)
            .ToArray();

        string[] extraInDefinition = definedSet
            .Where(name => !expectedSet.Contains(name))
            .OrderBy(name => name)
            .ToArray();

        if (missingFromDefinition.Length > 0)
        {
            Debug.LogWarning(
                $"DungeonPackRegistrar: pack '{packName}' is missing expected prefab definitions: {string.Join(", ", missingFromDefinition)}");
        }

        if (extraInDefinition.Length > 0)
        {
            Debug.LogWarning(
                $"DungeonPackRegistrar: pack '{packName}' defines unexpected prefabs: {string.Join(", ", extraInDefinition)}");
        }
    }

    private static void VerifyRegisteredPrefabs(MWLDungeonPack pack)
    {
        string[] expectedNames = NormalizeNames(pack.ExpectedPrefabNames);
        if (expectedNames.Length == 0)
        {
            return;
        }

        string packName = string.IsNullOrEmpty(pack.Name) ? "<unnamed>" : pack.Name;
        string[] missingPrefabs = expectedNames
            .Where(name => PrefabManager.Instance.GetPrefab(name) == null)
            .OrderBy(name => name)
            .ToArray();

        if (missingPrefabs.Length > 0)
        {
            Debug.LogWarning(
                $"DungeonPackRegistrar: pack '{packName}' failed to register expected prefabs: {string.Join(", ", missingPrefabs)}");
            return;
        }

        Debug.Log(
            $"DungeonPackRegistrar: verified {expectedNames.Length} prefabs for pack '{packName}'");
    }

    private static string[] GetDefinedPrefabNames(MWLDungeonPack pack)
    {
        List<string> names = new();

        if (pack.KitClones != null)
        {
            foreach (KitClone clone in pack.KitClones)
            {
                names.Add(clone.Name);
            }
        }

        if (pack.CustomPrefabs != null)
        {
            foreach (CustomPrefabSpec spec in pack.CustomPrefabs)
            {
                names.Add(spec.Name);
            }
        }

        if (pack.DungeonGeneratorPrefab != null)
        {
            names.Add(pack.DungeonGeneratorPrefab.Name);
        }

        return NormalizeNames(names);
    }

    private static string[] NormalizeNames(IEnumerable<string> names)
    {
        return names?
            .Where(name => !string.IsNullOrEmpty(name))
            .ToArray() ?? System.Array.Empty<string>();
    }
}
