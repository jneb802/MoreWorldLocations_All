using System.Collections.Generic;
using Common;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Configs;
using UnityEngine;
using CreatureManager = Common.CreatureManager;

namespace Dungeon_The_Ritual;

public class RoomManager
{
    
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
        { "BFD_Modular17", new RoomConfig { ThemeName = "Underground Ruins", Weight = 1.0f } },
        { "BFD_Modular8_Puzzle", new RoomConfig { ThemeName = "Underground Ruins", Weight = 0f } },
        { "BFD_Modular12_Solution", new RoomConfig { ThemeName = "Underground Ruins", Weight = 0f } },
        { "BFD_ModularElbow", new RoomConfig { ThemeName = "Underground Ruins", Weight = 1.0f } },
        { "BFD_ModularEnd3", new RoomConfig { ThemeName = "Underground Ruins", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_ModularEnd4", new RoomConfig { ThemeName = "Underground Ruins", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_ModularEnd5", new RoomConfig { ThemeName = "Underground Ruins", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_ModularEnd6", new RoomConfig { ThemeName = "Underground Ruins", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_ModularEnd7", new RoomConfig { ThemeName = "Underground Ruins", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_ModularEnd8", new RoomConfig { ThemeName = "Underground Ruins", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_ModularEnd9", new RoomConfig { ThemeName = "Underground Ruins", Endcap = true, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_ModularDivider1", new RoomConfig { ThemeName = "Underground Ruins", Entrance = false, Endcap = false, Divider = true, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_Stairwell3", new RoomConfig { ThemeName = "Underground Ruins", Weight = 1.0f } },
        { "BFD_Stairwell5", new RoomConfig { ThemeName = "Underground Ruins", Weight = 1.0f } }
    };
    
    public static void AddAllRooms()
    {
        var assetBundle = Dungeon_The_RitualPlugin.assetBundle;
        
        foreach (var room in AllRoomConfigs)
        {
            AddRoom(
                assetBundle, 
                room.Key, 
                room.Value,
                Dungeon_The_RitualPlugin.dungeonBFDYamlManager.GetCreatureYamlContent(Dungeon_The_RitualPlugin.MWD_TheRitual_CreatureYaml_Config.Value),
                Dungeon_The_RitualPlugin.MWD_TheRitual_CreatureList_Config.Value);
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
    
    public static void AddRoom(AssetBundle assetBundle, string prefabName, RoomConfig roomConfig, string creatureYAMLContent, string creatureListName)
    {
        GameObject roomGameObject = assetBundle.LoadAsset<GameObject>(prefabName);
        GameObject roomContainer = ZoneManager.Instance.CreateLocationContainer(roomGameObject);
        
        AddTentaRoot(roomContainer);
        CreatureManager.SetupCreatures(creatureListName,roomContainer,creatureYAMLContent);
        
        CustomRoom customRoom = new CustomRoom(roomContainer, fixReference: true, roomConfig);
        
        DungeonManager.Instance.AddCustomRoom(customRoom);
    }
    
    public static void AddPuzzleRoom(AssetBundle assetBundle, string prefabName, RoomConfig roomConfig, string creatureYAMLContent, string creatureListName)
    {
        GameObject roomGameObject = assetBundle.LoadAsset<GameObject>(prefabName);
        GameObject roomContainer = ZoneManager.Instance.CreateLocationContainer(roomGameObject);
        
        
        
        AddTentaRoot(roomContainer);
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