using System;
using System.Collections.Generic;
using Common;
using Jotunn.Managers;
using UnityEngine;

namespace More_World_Locations_AIO.Utils
{
    public static class LootDB
    {
        public static Dictionary<Heightmap.Biome, DropTable> biomeLootTables;
        
        public static void InitializeLootTables()
        {
            biomeLootTables = new Dictionary<Heightmap.Biome, DropTable>
            {
                { Heightmap.Biome.Meadows, GetMeadowsLootTable() },
                { Heightmap.Biome.BlackForest, GetBlackForestLootTable() },
                { Heightmap.Biome.Swamp, GetSwampLootTable() },
                { Heightmap.Biome.Mountain, GetMountainsLootTable() },
                { Heightmap.Biome.Plains, GetPlainsLootTable() },
                { Heightmap.Biome.Mistlands, GetMistlandsLootTable() },
                { Heightmap.Biome.AshLands, GetAshlandsLootTable() }
            };

            PrefabManager.OnVanillaPrefabsAvailable -= InitializeLootTables;
        }

        public static void LoadFromYAML(string yamlContent)
        {
            if (string.IsNullOrEmpty(yamlContent)) return;

            // NOTE: The current LootDB structure uses biome-based loot tables (Dictionary<Heightmap.Biome, DropTable>)
            // while the YAML format uses named loot lists (e.g., "MeadowsLoot1", "MeadowsLoot2").
            // Since there's no clear mapping between named lists and biome tables, and locations currently
            // use the SetupLoot method with biome parameters, YAML override for loot is not currently implemented.
            // If named loot lists are needed, the location system would need to be refactored to reference
            // specific loot lists by name rather than by biome.

            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo("LootDB LoadFromYAML called, but biome-based loot table override from YAML is not currently implemented");
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo("Locations will continue to use biome-based hardcoded loot tables");

            // TODO: If needed, refactor location system to support named loot lists, or implement
            // a mapping system between YAML named lists and biome-based loot tables
        }

        private static DropTable GetMeadowsLootTable()
        {
            return CreateDropTable(new List<DropTable.DropData>
            {
                new DropTable.DropData { m_item = GetItemDrop("Flint"), m_stackMin = 1, m_stackMax = 4, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("Feathers"), m_stackMin = 2, m_stackMax = 4, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("Raspberry"), m_stackMin = 1, m_stackMax = 3, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("LeatherScraps"), m_stackMin = 1, m_stackMax = 3, m_weight = 1.2f },
                new DropTable.DropData { m_item = GetItemDrop("DeerHide"), m_stackMin = 1, m_stackMax = 3, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("FineWood"), m_stackMin = 1, m_stackMax = 4, m_weight = 1.0f },
            });
        }
        
        private static DropTable GetBlackForestLootTable()
        {
            return CreateDropTable(new List<DropTable.DropData>
            {
                new DropTable.DropData { m_item = GetItemDrop("Amber"), m_stackMin = 1, m_stackMax = 4, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("Ruby"), m_stackMin = 1, m_stackMax = 2, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("BoneFragments"), m_stackMin = 2, m_stackMax = 4, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("Blueberries"), m_stackMin = 2, m_stackMax = 4, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("CarrotSeeds"), m_stackMin = 2, m_stackMax = 4, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("AmberPearl"), m_stackMin = 1, m_stackMax = 3, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("TinOre"), m_stackMin = 1, m_stackMax = 3, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("CopperOre"), m_stackMin = 1, m_stackMax = 3, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("Bronze"), m_stackMin = 1, m_stackMax = 2, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("Copper"), m_stackMin = 1, m_stackMax = 3, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("SurtlingCore"), m_stackMin = 1, m_stackMax = 2, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("MinceMeatSauce"), m_stackMin = 1, m_stackMax = 2, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("DeerStew"), m_stackMin = 1, m_stackMax = 2, m_weight = 1.0f }
            });
        }
        
        private static DropTable GetSwampLootTable()
        {
            return CreateDropTable(new List<DropTable.DropData>
            {
                new DropTable.DropData { m_item = GetItemDrop("WitheredBone"), m_stackMin = 1, m_stackMax = 5, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("SurtlingCore"), m_stackMin = 2, m_stackMax = 5, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("AmberPearl"), m_stackMin = 1, m_stackMax = 2, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("Ruby"), m_stackMin = 1, m_stackMax = 5, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("ElderBark"), m_stackMin = 3, m_stackMax = 20, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("ArrowPoison"), m_stackMin = 3, m_stackMax = 10, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("Chain"), m_stackMin = 1, m_stackMax = 2, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("TurnipSeeds"), m_stackMin = 1, m_stackMax = 3, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("Guck"), m_stackMin = 1, m_stackMax = 4, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("Root"), m_stackMin = 3, m_stackMax = 5, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("IronScrap"), m_stackMin = 1, m_stackMax = 5, m_weight = 1.0f }
            });
        }
        
        private static DropTable GetMountainsLootTable()
        {
            return CreateDropTable(new List<DropTable.DropData>
            {
                new DropTable.DropData { m_item = GetItemDrop("OnionSeeds"), m_stackMin = 2, m_stackMax = 5, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("AmberPearl"), m_stackMin = 2, m_stackMax = 5, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("WolfPelt"), m_stackMin = 2, m_stackMax = 10, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("Crystal"), m_stackMin = 5, m_stackMax = 10, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("ArrowFrost"), m_stackMin = 4, m_stackMax = 10, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("Obsidian"), m_stackMin = 4, m_stackMax = 10, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("Coins"), m_stackMin = 20, m_stackMax = 50, m_weight = 1.0f },
            });
        }
        
        private static DropTable GetPlainsLootTable()
        {
            return CreateDropTable(new List<DropTable.DropData>
            {
                new DropTable.DropData { m_item = GetItemDrop("BlackMetalScrap"), m_stackMin = 2, m_stackMax = 4, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("LinenThread"), m_stackMin = 3, m_stackMax = 5, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("IronScrap"), m_stackMin = 2, m_stackMax = 4, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("Bread"), m_stackMin = 1, m_stackMax = 3, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("LoxPie"), m_stackMin = 1, m_stackMax = 2, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("FishWraps"), m_stackMin = 1, m_stackMax = 2, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("Coins"), m_stackMin = 30, m_stackMax = 50, m_weight = 1.0f },
            });
        }
        
        private static DropTable GetMistlandsLootTable()
        {
            return CreateDropTable(new List<DropTable.DropData>
            {
                new DropTable.DropData { m_item = GetItemDrop("WolfMeatSkewer"), m_stackMin = 2, m_stackMax = 3, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("ScaleHide"), m_stackMin = 2, m_stackMax = 5, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("Softtissue"), m_stackMin = 2, m_stackMax = 4, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("JuteBlue"), m_stackMin = 2, m_stackMax = 3, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("IronScrap"), m_stackMin = 3, m_stackMax = 5, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("RoyalJelly"), m_stackMin = 3, m_stackMax = 5, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("Mandible"), m_stackMin = 1, m_stackMax = 2, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("Bilebag"), m_stackMin = 1, m_stackMax = 2, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("Coins"), m_stackMin = 30, m_stackMax = 50, m_weight = 1.0f },
            });
        }
        
        private static DropTable GetAshlandsLootTable()
        {
            return CreateDropTable(new List<DropTable.DropData>
            {
                new DropTable.DropData { m_item = GetItemDrop("GemstoneBlue"), m_stackMin = 1, m_stackMax = 1, m_weight = 0.5f },
                new DropTable.DropData { m_item = GetItemDrop("GemstoneGreen"), m_stackMin = 1, m_stackMax = 1, m_weight = 0.5f },
                new DropTable.DropData { m_item = GetItemDrop("GemstoneRed"), m_stackMin = 1, m_stackMax = 1, m_weight = 0.5f },
                new DropTable.DropData { m_item = GetItemDrop("Coins"), m_stackMin = 200, m_stackMax = 500, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("CharredBone"), m_stackMin = 5, m_stackMax = 10, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("Charredskull"), m_stackMin = 1, m_stackMax = 2, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("CelestialFeather"), m_stackMin = 1, m_stackMax = 1, m_weight = 0.1f },
                new DropTable.DropData { m_item = GetItemDrop("FlametalOreNew"), m_stackMin = 10, m_stackMax = 20, m_weight = 1.0f },
                new DropTable.DropData { m_item = GetItemDrop("MoltenCore"), m_stackMin = 1, m_stackMax = 2, m_weight = 0.5f }
            });
        }
        
        private static DropTable CreateDropTable(List<DropTable.DropData> drops)
        {
            DropTable dropTable = new DropTable();
            dropTable.m_drops = drops;
            dropTable.m_dropMin = 2;
            dropTable.m_dropMax = 4;
            dropTable.m_dropChance = 1.0f;
            dropTable.m_oneOfEach = false;
            return dropTable;
        }
        
        private static GameObject GetItemDrop(string itemName)
        {
            GameObject prefab = PrefabManager.Instance.GetPrefab(itemName);
            if (prefab == null)
            {
                Debug.LogWarning($"LootDB: Could not find item prefab: {itemName}");
                return null;
            }
            return prefab;
        }
        
        public static DropTable GetLootTable(Heightmap.Biome biome)
        {
            if (!biomeLootTables.ContainsKey(biome))
            {
                Debug.LogWarning($"LootDB: No loot table found for biome: {biome}");
                return null;
            }
            
            return biomeLootTables[biome];
        }
        
        public static void SetupLoot(Heightmap.Biome biome, GameObject location)
        {
            foreach (Container container in location.GetComponentsInChildren<Container>())
            {
                container.m_defaultItems.m_drops = GetLootTable(biome).m_drops;
            }
        }
    }
}