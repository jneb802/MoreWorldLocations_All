using System;
using System.Collections.Generic;
using Common;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Configs;
using UnityEngine;
using CreatureManager = Common.CreatureManager;
using LootManager = Common.LootManager;

namespace Underground_Ruins;

public class Rooms
{
    public static Dictionary<string, RoomConfig> AllLimitRooms = new Dictionary<string, RoomConfig>
    {
        { "BFD_Modular8_Puzzle", new RoomConfig { ThemeName = "Underground Ruins", Weight = 0f } },
        { "BFD_Modular12_Solution", new RoomConfig { ThemeName = "Underground Ruins", Weight = 0f } },
        // { "BFD_Modular17_Boss", new RoomConfig { ThemeName = "Underground Ruins", Weight = 1.0f } },
    };
    
    public static Dictionary<string, RoomConfig> AllRoomConfigs = new Dictionary<string, RoomConfig>
    {
        { "BFD_MainEntrance2", new RoomConfig { ThemeName = "Underground Ruins", Entrance = true, Weight = 1.0f } },
        { "BFD_Modular3", new RoomConfig { ThemeName = "Underground Ruins", Weight = 1.0f } },
        { "BFD_Modular5", new RoomConfig { ThemeName = "Underground Ruins", Weight = 1.0f, MinPlaceOrder = 2 } },
        { "BFD_Modular6", new RoomConfig { ThemeName = "Underground Ruins", Weight = 1.0f } },
        { "BFD_Modular7", new RoomConfig { ThemeName = "Underground Ruins", Weight = 1.0f } },
        { "BFD_Modular9", new RoomConfig { ThemeName = "Underground Ruins", Weight = 1.0f } },
        { "BFD_Modular10", new RoomConfig { ThemeName = "Underground Ruins", Weight = 1.0f } },
        { "BFD_Modular11", new RoomConfig { ThemeName = "Underground Ruins", Weight = 1.0f } },
        { "BFD_Modular12", new RoomConfig { ThemeName = "Underground Ruins", Weight = 1.0f } },
        { "BFD_Modular13", new RoomConfig { ThemeName = "Underground Ruins", Weight = 1.0f } },
        { "BFD_Modular14", new RoomConfig { ThemeName = "Underground Ruins", Weight = 1.0f } },
        { "BFD_ModularElbow", new RoomConfig { ThemeName = "Underground Ruins", Weight = 1.0f } },
        { "BFD_ModularEnd3", new RoomConfig { ThemeName = "Underground Ruins", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_ModularEnd4", new RoomConfig { ThemeName = "Underground Ruins", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_ModularEnd5", new RoomConfig { ThemeName = "Underground Ruins", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_ModularEnd6", new RoomConfig { ThemeName = "Underground Ruins", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_ModularEnd7", new RoomConfig { ThemeName = "Underground Ruins", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_ModularEnd8", new RoomConfig { ThemeName = "Underground Ruins", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_ModularEnd9", new RoomConfig { ThemeName = "Underground Ruins", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_ModularDivider1", new RoomConfig { ThemeName = "Underground Ruins", Entrance = false, Endcap = false, Divider = true, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_Stairwell3", new RoomConfig { ThemeName = "Underground Ruins", Weight = 0.8f } },
        { "BFD_Stairwell5", new RoomConfig { ThemeName = "Underground Ruins", Weight = 0.9f } }
    };
    
    public static void AddAllRooms()
    {
        var assetBundle = Underground_RuinsPlugin.assetBundle;
        
        foreach (var room in AllRoomConfigs)
        {
            AddRoom(
                assetBundle, 
                room.Key, 
                room.Value,
                Underground_RuinsPlugin.dungeonBFDYamlManager.GetCreatureYamlContent(Underground_RuinsPlugin.MWD_UndergroundRuins_Configuration.CreatureYaml.Value),
                Underground_RuinsPlugin.MWD_UndergroundRuins_Configuration.CreatureList.Value,
                Underground_RuinsPlugin.dungeonBFDYamlManager.GetLootYamlContent(Underground_RuinsPlugin.MWD_UndergroundRuins_Configuration.LootYaml.Value),
                Underground_RuinsPlugin.MWD_UndergroundRuins_Configuration.LootList.Value,
                Underground_RuinsPlugin.dungeonBFDYamlManager.GetPickableItemContent(Underground_RuinsPlugin.MWD_UndergroundRuins_Configuration.PickableItemYaml.Value),
                Underground_RuinsPlugin.MWD_UndergroundRuins_Configuration.PickableItemList.Value);
        }
        
        foreach (var room in AllLimitRooms)
        {
            AddLimitRooms(
                assetBundle, 
                room.Key, 
                room.Value,
                Underground_RuinsPlugin.dungeonBFDYamlManager.GetCreatureYamlContent(Underground_RuinsPlugin.MWD_UndergroundRuins_Configuration.CreatureYaml.Value),
                Underground_RuinsPlugin.MWD_UndergroundRuins_Configuration.CreatureList.Value,
                Underground_RuinsPlugin.dungeonBFDYamlManager.GetLootYamlContent(Underground_RuinsPlugin.MWD_UndergroundRuins_Configuration.LootYaml.Value),
                Underground_RuinsPlugin.MWD_UndergroundRuins_Configuration.LootList.Value,
                Underground_RuinsPlugin.dungeonBFDYamlManager.GetPickableItemContent(Underground_RuinsPlugin.MWD_UndergroundRuins_Configuration.PickableItemYaml.Value),
                Underground_RuinsPlugin.MWD_UndergroundRuins_Configuration.PickableItemList.Value);
        }
        
        DungeonManager.OnVanillaRoomsAvailable -= AddAllRooms;
    }

    public static void RegisterTheme(GameObject dungeonGameObject, string themeName)
    {
        if (DungeonManager.Instance.RegisterDungeonTheme(dungeonGameObject, themeName))
        {
            WarpLogger.Logger.LogDebug("RoomTheme with name " + themeName + " was successfully registered to " + dungeonGameObject);
        }
        else
        {
            WarpLogger.Logger.LogError("RoomTheme with name " + themeName + " failed to register");
        }
        
    }
    
    public static void AddRoom(AssetBundle assetBundle, string prefabName, RoomConfig roomConfig, string creatureYAMLContent, string creatureListName,  string lootYAMLContent, string lootListName, string pickableItemYAMLContent, string pickableListName)
    {
        GameObject roomGameObject = assetBundle.LoadAsset<GameObject>(prefabName);
        GameObject roomContainer = ZoneManager.Instance.CreateLocationContainer(roomGameObject);
        
        AddTentaRoot(roomContainer);
        
        List<DropTable.DropData> dropDataList = LootManager.ParseContainerYaml_v2(lootListName, lootYAMLContent);
        List<Container> locationChestContainers = LootManager.GetLocationsContainers(roomContainer);
        LootManager.SetupChestLoot(locationChestContainers,dropDataList);
        LootManager_v2.SetupPickableItems(roomContainer,pickableItemYAMLContent,pickableListName);
        CreatureManager.SetupCreatures(creatureListName,roomContainer,creatureYAMLContent);
        
        CustomRoom customRoom = new CustomRoom(roomContainer, fixReference: true, roomConfig);
        
        DungeonManager.Instance.AddCustomRoom(customRoom);
    }
    
    public static void AddLimitRooms(AssetBundle assetBundle, string prefabName, RoomConfig roomConfig, string creatureYAMLContent, string creatureListName, string lootYAMLContent, string lootListName, string pickableItemYAMLContent, string pickableListName)
    {
        GameObject roomGameObject = assetBundle.LoadAsset<GameObject>(prefabName);
        GameObject roomContainer = ZoneManager.Instance.CreateLocationContainer(roomGameObject);

        roomContainer.AddComponent<RoomExtras>();
        roomContainer.GetComponent<RoomExtras>().PlacementLimit = 1;
        
        AddTentaRoot(roomContainer);
        
        List<DropTable.DropData> dropDataList = LootManager.ParseContainerYaml_v2(lootListName, lootYAMLContent);
        List<Container> locationChestContainers = LootManager.GetLocationsContainers(roomContainer);
        LootManager.SetupChestLoot(locationChestContainers,dropDataList);
        LootManager_v2.SetupPickableItems(roomContainer,pickableItemYAMLContent,pickableListName);
        CreatureManager.SetupCreatures(creatureListName,roomContainer,creatureYAMLContent);
        
        CustomRoom customRoom = new CustomRoom(roomContainer, fixReference: true, roomConfig);
        
        DungeonManager.Instance.AddCustomRoom(customRoom);
    }

    public static void AddTentaRoot(GameObject gameObject)
    {
        EggHatch[] eggHatches = gameObject.GetComponentsInChildren<EggHatch>();
        if (eggHatches.Length > 0)
        {
            foreach (EggHatch eggHatch in eggHatches)
            {
                eggHatch.m_spawnPrefab.GetComponent<CreatureSpawner>().m_creaturePrefab = CreatureManager.GetCreaturePrefab("TentaRoot");
            }
        }
    }
}