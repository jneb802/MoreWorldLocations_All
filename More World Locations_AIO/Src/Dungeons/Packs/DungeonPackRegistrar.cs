using Jotunn.Entities;
using Jotunn.Managers;
using More_World_Locations_AIO.Utils;
using UnityEngine;

namespace More_World_Locations_AIO.Dungeons.Packs;

public static class DungeonPackRegistrar
{
    public static void Register(MWLDungeonPack pack)
    {
        if (pack == null) return;

        AssetBundle bundle = pack.Bundle?.Invoke();

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

    private static void RegisterCustomPrefab(AssetBundle bundle, CustomPrefabSpec spec, string packName)
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

        GameObject prefab = null;

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

        KitStripper.Strip(prefab, spec.Strip);
        spec.Configure?.Invoke(prefab);

        // Bundled assets need fixReference=true so JVLmock_ children resolve.
        // Vanilla clones have already-resolved references.
        bool fixReference = spec.Source == CustomPrefabSource.Bundled;

        CustomPrefab customPrefab = new CustomPrefab(prefab, fixReference);
        PrefabManager.Instance.AddPrefab(customPrefab);
    }
}
