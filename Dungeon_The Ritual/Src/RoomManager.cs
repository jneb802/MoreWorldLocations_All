using System.Collections.Generic;
using Common;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Configs;
using UnityEngine;

namespace Dungeon_The_Ritual;

public class RoomManager
{
    public static Dictionary<string, RoomConfig> AllRoomConfigs = new Dictionary<string, RoomConfig>
    {
        { "BFD_EndCap1", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = true, Divider = false, EndcapPrio = 1, Weight = 1.0f } },
        { "BFD_Hall1", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = false, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_Hall2", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = false, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_Hall3", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = false, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_Hall4", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = false, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_MainEntrance", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = true, Endcap = false, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_Modular1", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = false, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_Modular2", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = false, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_Modular4", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = false, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_Modular5", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = false, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_Modular6", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = false, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_Modular7", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = false, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_ModularElbow", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = false, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_ModularEnd1", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = true, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_ModularEnd2", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = true, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_ModularEnd3", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = true, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_ModularEnd4", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = true, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_ModularEnd6", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = true, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_ModularDivider1", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = false, Divider = true, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_Room1", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = false, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_Room2", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = false, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_Room3", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = false, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_Room4", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = false, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_Room5", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = false, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_Room6", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = false, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_Stairwell1", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = false, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_Stairwell2", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = false, Divider = false, EndcapPrio = 0, Weight = 1.0f } },
        { "BFD_Stairwell3", new RoomConfig { ThemeName = "Grafstad", Enabled = true, Entrance = false, Endcap = false, Divider = false, EndcapPrio = 0, Weight = 1.0f } }
    };
    
    public static void AddAllRooms()
    {
        var assetBundle = Dungeon_The_RitualPlugin.assetBundle;
        
        foreach (var room in AllRoomConfigs)
        {
            AddRoom(assetBundle, room.Key, room.Value);
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
    
    public static void AddRoom(AssetBundle assetBundle, string prefabName, RoomConfig roomConfig)
    {
        GameObject roomGameObject = assetBundle.LoadAsset<GameObject>(prefabName);
        
        CustomRoom customRoom = new CustomRoom(roomGameObject, fixReference: true, roomConfig);

        DungeonManager.Instance.AddCustomRoom(customRoom);
    }
}