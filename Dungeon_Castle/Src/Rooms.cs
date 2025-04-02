using System.Collections.Generic;
using Common;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Forbidden_Catacombs;

public class Rooms
{
    public static Dictionary<string, RoomConfig> UpperRoomConfigs = new Dictionary<string, RoomConfig>
    {
        { "CD_Entrance2", new RoomConfig { ThemeName = "CD_Catacomb", Entrance = true, Weight = 1.0f } },
        { "CD_Room1", new RoomConfig { ThemeName = "CD_Catacomb", Weight = 1.0f } },
        { "CD_Room2", new RoomConfig { ThemeName = "CD_Catacomb", Weight = 1.0f } },
        { "CD_Room3", new RoomConfig { ThemeName = "CD_Catacomb", Weight = 1.0f } },
        { "CD_Room4", new RoomConfig { ThemeName = "CD_Catacomb", Weight = 1.0f } },
        { "CD_RoomBig1", new RoomConfig { ThemeName = "CD_Catacomb", Weight = 1.0f } },
        { "CD_RoomBig2", new RoomConfig { ThemeName = "CD_Catacomb", Weight = 1.0f } },
        { "CD_RoomBig3", new RoomConfig { ThemeName = "CD_Catacomb", Weight = 1.0f } },
        { "CD_Hallway1", new RoomConfig { ThemeName = "CD_Catacomb", Weight = 1.0f } },
        { "CD_Hallway2", new RoomConfig { ThemeName = "CD_Catacomb", Weight = 1.0f } },
        { "CD_Hallway3", new RoomConfig { ThemeName = "CD_Catacomb", Weight = 1.0f } },
        // // { "CD_Stairwell1", new RoomConfig { ThemeName = "CD_Castle", Weight = 1.0f } },
        { "CD_Endcap1", new RoomConfig { ThemeName = "CD_Catacomb", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "CD_Endcap2", new RoomConfig { ThemeName = "CD_Catacomb", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "CD_Endcap4", new RoomConfig { ThemeName = "CD_Catacomb", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "CD_Endcap5", new RoomConfig { ThemeName = "CD_Catacomb", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "CD_Endcap6", new RoomConfig { ThemeName = "CD_Catacomb", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "CD_Endcap7", new RoomConfig { ThemeName = "CD_Catacomb", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "CD_Endcap8", new RoomConfig { ThemeName = "CD_Catacomb", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "CD_Endcap9", new RoomConfig { ThemeName = "CD_Catacomb", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "CD_Endcap10", new RoomConfig { ThemeName = "CD_Catacomb", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "CD_Endcap11", new RoomConfig { ThemeName = "CD_Catacomb", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "CD_Endcap12", new RoomConfig { ThemeName = "CD_Catacomb", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "CD_Endcap13", new RoomConfig { ThemeName = "CD_Catacomb", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "CD_Endcap14", new RoomConfig { ThemeName = "CD_Catacomb", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
    };
        
    public static Dictionary<string, RoomConfig> LowerRoomConfigs = new Dictionary<string, RoomConfig>
    {    
        { "CD_Lower_Hallway1", new RoomConfig { ThemeName = "CD_Catacomb", Weight = 1.0f } },
        { "CD_Lower_Hallway2", new RoomConfig { ThemeName = "CD_Catacomb", Weight = 1.0f } },
        { "CD_Lower_Hallway3", new RoomConfig { ThemeName = "CD_Catacomb", Weight = 1.0f } },
        { "CD_Lower_Hallway4", new RoomConfig { ThemeName = "CD_Catacomb", Weight = 1.0f } },
        { "CD_Lower_Room1", new RoomConfig { ThemeName = "CD_Catacomb", Weight = 1.0f } },
        { "CD_Lower_Room2", new RoomConfig { ThemeName = "CD_Catacomb", Weight = 1.0f } },
        { "CD_Lower_Room3", new RoomConfig { ThemeName = "CD_Catacomb", Weight = 1.0f } },
        { "CD_Lower_Cell1", new RoomConfig { ThemeName = "CD_Catacomb", Weight = 1.0f } },
        { "CD_Lower_Cell2", new RoomConfig { ThemeName = "CD_Catacomb", Weight = 1.0f } },
        { "CD_Lower_Cell3", new RoomConfig { ThemeName = "CD_Catacomb", Weight = 1.0f } },
        { "CD_Lower_Endcap1", new RoomConfig { ThemeName = "CD_Catacomb", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "CD_Lower_Endcap2", new RoomConfig { ThemeName = "CD_Catacomb", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "CD_Lower_Endcap3", new RoomConfig { ThemeName = "CD_Catacomb", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "CD_Lower_Endcap4", new RoomConfig { ThemeName = "CD_Catacomb", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
    };
    
    public static void AddAllRooms()
    {
        var assetBundle = Forbidden_CatacombsPlugin.assetBundle;
        
        foreach (var room in UpperRoomConfigs)
        {
            AddRoom(
                assetBundle, 
                room.Key, 
                room.Value,
                Forbidden_CatacombsPlugin.dungeonCastleYamlManager.GetCreatureYamlContent(Forbidden_CatacombsPlugin.MWD_CastleDungeon_CreatureYaml.Value),
                Forbidden_CatacombsPlugin.MWD_CastleDungeon_CreatureListUpper.Value,
                Forbidden_CatacombsPlugin.dungeonCastleYamlManager.GetLootYamlContent(Forbidden_CatacombsPlugin.MWD_CastleDungeon_LootYaml.Value),
                Forbidden_CatacombsPlugin.MWD_CastleDungeon_LootListUpper.Value,
                Forbidden_CatacombsPlugin.dungeonCastleYamlManager.GetPickableItemContent(Forbidden_CatacombsPlugin.MWD_CastleDungeon_PickableYaml.Value),
                Forbidden_CatacombsPlugin.MWD_CastleDungeon_PickableList.Value);
        }
        
        foreach (var room in LowerRoomConfigs)
        {
            AddRoom(
                assetBundle, 
                room.Key, 
                room.Value,
                Forbidden_CatacombsPlugin.dungeonCastleYamlManager.GetCreatureYamlContent(Forbidden_CatacombsPlugin.MWD_CastleDungeon_CreatureYaml.Value),
                Forbidden_CatacombsPlugin.MWD_CastleDungeon_CreatureListLower.Value,
                Forbidden_CatacombsPlugin.dungeonCastleYamlManager.GetLootYamlContent(Forbidden_CatacombsPlugin.MWD_CastleDungeon_LootYaml.Value),
                Forbidden_CatacombsPlugin.MWD_CastleDungeon_LootListLower.Value,
                Forbidden_CatacombsPlugin.dungeonCastleYamlManager.GetPickableItemContent(Forbidden_CatacombsPlugin.MWD_CastleDungeon_PickableYaml.Value),
                Forbidden_CatacombsPlugin.MWD_CastleDungeon_PickableList.Value);
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