using System.Collections.Generic;
using Jotunn.Managers;
using More_World_Locations_AIO.Dungeons;
using UnityEngine;

namespace More_World_Locations_AIO.Dungeons.Packs;

public static class UndergroundRuinsPack
{
    private const string Theme = "Underground Ruins";
    private const string ExteriorLocation = "BFD_Exterior";
    private const string PuzzleStandTemplateName = "BFD_Kit_PuzzleStand";
    private const string PuzzleLeverName = "BFD_PuzzleLever";
    private const string PuzzleDoorName = "BFD_PuzzleSecretDoor";
    private const string TentaSpawnerName = "BFD_Kit_Spawner_TentaRoot";
    private const string TentaSpawnerChestName = "BFD_chest_spawner1";
    private const string TentaCreatureName = "TentaRoot";
    private const int PuzzleCount = 8;

    public static readonly MWLDungeonPack Pack = new MWLDungeonPack
    {
        Name = "UndergroundRuins",
        ThemeName = Theme,
        ExteriorLocationName = ExteriorLocation,
        Bundle = () => Prefabs.dungeonBlackforest,

        KitClones = new[]
        {
            new KitClone { Name = "BFD_Kit_dirtfloor",        VanillaSource = "dirtfloor" },
            new KitClone { Name = "BFD_Kit_Stoneblock",       VanillaSource = "Stoneblock" },
            new KitClone { Name = "BFD_Kit_StoneblockSmall",  VanillaSource = "StoneblockSmall" },
            new KitClone { Name = "BFD_Kit_StonePillar",      VanillaSource = "StonePillar" },
            new KitClone { Name = "BFD_Kit_MineRock_Copper",  VanillaSource = "MineRock_Copper", Strip = StripFlags.StaticPhysics },
            new KitClone { Name = "BFD_chest_loot",           VanillaSource = "TreasureChest_fCrypt" },
            new KitClone { Name = TentaSpawnerName,           VanillaSource = "Spawner_Skeleton", Configure = ConfigureTentaSpawner },
            new KitClone { Name = TentaSpawnerChestName,      VanillaSource = "crypt_skeleton_chest", Configure = ConfigureTentaSpawnerChest },
        },

        CustomPrefabs = BuildCustomPrefabs(),

        DungeonGeneratorPrefab = new CustomPrefabSpec
        {
            Name = "DG_BlackForestDungeon",
            Source = CustomPrefabSource.Bundled,
        },

        ExpectedPrefabNames = BuildExpectedPrefabNames(),
    };

    private static CustomPrefabSpec[] BuildCustomPrefabs()
    {
        List<CustomPrefabSpec> prefabs = new()
        {
            new CustomPrefabSpec { Name = PuzzleStandTemplateName, Source = CustomPrefabSource.Bundled },
            new CustomPrefabSpec { Name = PuzzleLeverName, Source = CustomPrefabSource.Bundled, Configure = ConfigurePuzzleLever },
            new CustomPrefabSpec { Name = PuzzleDoorName, Source = CustomPrefabSource.Bundled },
            new CustomPrefabSpec { Name = "BFD_CryptKey", Source = CustomPrefabSource.Bundled },
            new CustomPrefabSpec { Name = "BFD_Modular6_Light", Source = CustomPrefabSource.Bundled },
            new CustomPrefabSpec { Name = "BFD_Modular8_Puzzle_Light", Source = CustomPrefabSource.Bundled },
            // Keep this explicit because the current AIO bundle still contains it and the
            // old Prefabs.cs path registered it through the bundle sweep.
            new CustomPrefabSpec { Name = "8_PuzzleStand", Source = CustomPrefabSource.Bundled },
        };

        for (int i = 0; i < PuzzleCount; i++)
        {
            prefabs.Add(new CustomPrefabSpec
            {
                Name = $"{i}_PuzzleStand",
                Source = CustomPrefabSource.VanillaClone,
                VanillaSource = PuzzleStandTemplateName,
            });
        }

        for (int i = 0; i < PuzzleCount; i++)
        {
            prefabs.Add(new CustomPrefabSpec
            {
                Name = $"{i}_PuzzlePickable",
                Source = CustomPrefabSource.VanillaClone,
                VanillaSource = "Pickable_SurtlingCoreStand",
            });
        }

        return prefabs.ToArray();
    }

    private static string[] BuildExpectedPrefabNames()
    {
        List<string> names = new()
        {
            "BFD_Kit_dirtfloor",
            "BFD_Kit_Stoneblock",
            "BFD_Kit_StoneblockSmall",
            "BFD_Kit_StonePillar",
            "BFD_Kit_MineRock_Copper",
            "BFD_chest_loot",
            TentaSpawnerName,
            TentaSpawnerChestName,
            PuzzleStandTemplateName,
            PuzzleLeverName,
            PuzzleDoorName,
            "BFD_CryptKey",
            "BFD_Modular6_Light",
            "BFD_Modular8_Puzzle_Light",
            "8_PuzzleStand",
            "DG_BlackForestDungeon",
        };

        for (int i = 0; i < PuzzleCount; i++)
        {
            names.Add($"{i}_PuzzleStand");
        }

        for (int i = 0; i < PuzzleCount; i++)
        {
            names.Add($"{i}_PuzzlePickable");
        }

        return names.ToArray();
    }

    private static void ConfigureTentaSpawner(GameObject prefab)
    {
        CreatureSpawner creatureSpawner = prefab.GetComponent<CreatureSpawner>();
        if (creatureSpawner == null)
        {
            Debug.LogWarning($"UndergroundRuinsPack: '{prefab.name}' is missing a CreatureSpawner component");
            return;
        }

        GameObject tentaRootPrefab = PrefabManager.Cache.GetPrefab<GameObject>(TentaCreatureName);
        if (tentaRootPrefab == null)
        {
            Debug.LogWarning($"UndergroundRuinsPack: Could not find creature prefab '{TentaCreatureName}'");
            return;
        }

        creatureSpawner.m_creaturePrefab = tentaRootPrefab;
    }

    private static void ConfigureTentaSpawnerChest(GameObject prefab)
    {
        EggHatch eggHatch = prefab.GetComponent<EggHatch>();
        if (eggHatch == null)
        {
            Debug.LogWarning($"UndergroundRuinsPack: '{prefab.name}' is missing an EggHatch component");
            return;
        }

        GameObject tentaSpawnerPrefab = PrefabManager.Instance.GetPrefab(TentaSpawnerName);
        if (tentaSpawnerPrefab == null)
        {
            Debug.LogWarning(
                $"UndergroundRuinsPack: Could not assign '{TentaSpawnerName}' to '{TentaSpawnerChestName}'");
            return;
        }

        eggHatch.m_spawnPrefab = tentaSpawnerPrefab;
    }

    private static void ConfigurePuzzleLever(GameObject prefab)
    {
        if (prefab.GetComponent<AttachPuzzle>() == null)
        {
            prefab.AddComponent<AttachPuzzle>();
        }
    }
}
