using System.Collections.Generic;
using Common;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Dungeon_Castle;

public class Rooms
{
    public static Dictionary<string, RoomConfig> AllRoomConfigs = new Dictionary<string, RoomConfig>
    {
        { "CD_Entrance", new RoomConfig { ThemeName = "CD_Castle", Entrance = true, Weight = 1.0f } },
        { "CD_Hallway1", new RoomConfig { ThemeName = "CD_Castle", Weight = 1.0f } },
        { "CD_Endcap1", new RoomConfig { ThemeName = "CD_Castle", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "CD_Endcap2", new RoomConfig { ThemeName = "CD_Castle", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "CD_Endcap3", new RoomConfig { ThemeName = "CD_Castle", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "CD_Endcap4", new RoomConfig { ThemeName = "CD_Castle", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "CD_Endcap5", new RoomConfig { ThemeName = "CD_Castle", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
    };
    
    public static void AddAllRooms()
    {
        var assetBundle = Dungeon_CastlePlugin.assetBundle;
        
        foreach (var room in AllRoomConfigs)
        {
            AddRoom(
                assetBundle, 
                room.Key, 
                room.Value,
                Dungeon_CastlePlugin.dungeonCastleYamlManager.GetCreatureYamlContent(Dungeon_CastlePlugin.MWD_CastleDungeon_Configuration.CreatureYaml.Value),
                Dungeon_CastlePlugin.MWD_CastleDungeon_Configuration.CreatureList.Value,
                Dungeon_CastlePlugin.dungeonCastleYamlManager.GetLootYamlContent(Dungeon_CastlePlugin.MWD_CastleDungeon_Configuration.LootYaml.Value),
                Dungeon_CastlePlugin.MWD_CastleDungeon_Configuration.LootList.Value,
                Dungeon_CastlePlugin.dungeonCastleYamlManager.GetPickableItemContent(Dungeon_CastlePlugin.MWD_CastleDungeon_Configuration.PickableItemYaml.Value),
                Dungeon_CastlePlugin.MWD_CastleDungeon_Configuration.PickableItemList.Value);
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
        
        List<DropTable.DropData> dropDataList = LootManager.ParseContainerYaml_v2(lootListName, lootYAMLContent);
        List<Container> locationChestContainers = LootManager.GetLocationsContainers(roomContainer);
        LootManager.SetupChestLoot(locationChestContainers,dropDataList);
        LootManager_v2.SetupPickableItems(roomContainer,pickableItemYAMLContent,pickableListName);
        Common.CreatureManager.SetupCreatures(creatureListName,roomContainer,creatureYAMLContent);
        
        CustomRoom customRoom = new CustomRoom(roomContainer, fixReference: true, roomConfig);
        
        DungeonManager.Instance.AddCustomRoom(customRoom);
    }
}