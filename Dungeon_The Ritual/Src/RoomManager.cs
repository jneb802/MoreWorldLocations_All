using Common;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Configs;
using UnityEngine;

namespace Dungeon_The_Ritual;

public class RoomManager
{

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