using Common;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Configs;
using UnityEngine;

namespace Dungeon_The_Ritual;

public class Rooms
{
    
    public static void AddAllRooms()
    {
        var assetBundle = Dungeon_The_RitualPlugin.assetBundle;
        
        RoomManager.AddRoom(
            assetBundle,
            "MWD_Grafstad_Room2",
            RoomConfigs.MWD_Grafstad_Room2
        );
        
        RoomManager.AddRoom(
            assetBundle,
            "MWD_Grafstad_Room3_8x8x8",
            RoomConfigs.MWD_Grafstad_Room3_8x8x8
        );
        
        RoomManager.AddRoom(
            assetBundle,
            "MWD_Grafstad_Room3_8x8x16",
            RoomConfigs.MWD_Grafstad_Room3_8x8x16
        );
        
        RoomManager.AddRoom(
            assetBundle,
            "MWD_Grafstad_Room4_16x8x16",
            RoomConfigs.MWD_Grafstad_Room4_16x8x16
        );
        
        RoomManager.AddRoom(
            assetBundle,
            "MWD_Grafstad_Room5",
            RoomConfigs.MWD_Grafstad_Room5
        );
        
        RoomManager.AddRoom(
            assetBundle,
            "MWD_Grafstad_Room6",
            RoomConfigs.MWD_Grafstad_Room6
        );
        
        RoomManager.AddRoom(
            assetBundle,
            "MWD_Grafstad_Room7",
            RoomConfigs.MWD_Grafstad_Room7
        );
        
        RoomManager.AddRoom(
            assetBundle,
            "MWD_Grafstad_Room8",
            RoomConfigs.MWD_Grafstad_Room8
        );
        
        RoomManager.AddRoom(
            assetBundle,
            "MWD_Grafstad_Room9",
            RoomConfigs.MWD_Grafstad_Room9
        );
        
        RoomManager.AddRoom(
            assetBundle,
            "MWD_Grafstad_EndCap1",
            RoomConfigs.MWD_Grafstad_EndCap1
        );
        
        RoomManager.AddRoom(
            assetBundle,
            "MWD_Grafstad_EndCap2",
            RoomConfigs.MWD_Grafstad_EndCap2
        );
        
        RoomManager.AddRoom(
            assetBundle,
            "MWD_Grafstad_EndCap3",
            RoomConfigs.MWD_Grafstad_EndCap3
        );
        
        RoomManager.AddRoom(
            assetBundle,
            "MWD_Grafstad_Entrance_24x24x24",
            RoomConfigs.MWD_Grafstad_Entrance_24x24x24
        );
        
        DungeonManager.OnVanillaRoomsAvailable -= AddAllRooms;
        
    }
    
}