using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Jotunn.Managers;
using More_World_Locations_AIO.RPCs;
using UnityEngine;

namespace More_World_Locations_AIO.Waystones;

public class Waystone : MonoBehaviour, Interactable, Hoverable
{
    public WaystoneConfig waystoneConfig;
    public Heightmap.Biome biome;
    public ZNetView znetView;

    public const string WaystoneConfigKey = "MWL_WaystoneConfigName";

    public void Awake()
    {
        znetView = GetComponent<ZNetView>();
        ZDO zdo = znetView.GetZDO();
        if (znetView.IsOwner())
        {
            if (!zdo.GetString(WaystoneConfigKey, out string storedName) || string.IsNullOrEmpty(storedName))
            {
                biome = WorldGenerator.instance.GetBiome(this.transform.position);
                
                waystoneConfig = WaystoneDB.GetRandomWaystoneConfig(biome);
                
                zdo.Set(WaystoneConfigKey, waystoneConfig.internalName);
                zdo.Set("MWL_Waystone_Location", waystoneConfig.locationInternalName);
                zdo.Set("MWL_Waystone_Vegetation", waystoneConfig.vegetationInternalName);
                zdo.Set("MWL_Waystone_Radius", waystoneConfig.mapRevealRadius);
                zdo.Set("MWL_Waystone_Biome", biome.ToString());
                
                Debug.Log($"Waystone: Set random config '{waystoneConfig.internalName}' to ZDO");
                switch (waystoneConfig.waystoneType)
                {
                    case WaystoneConfig.WaystoneType.location:
                        Debug.Log($"Waystone Type: Location | Target Location: {waystoneConfig.locationInternalName}");
                        break;

                    case WaystoneConfig.WaystoneType.mapReveal:
                        Debug.Log($"Waystone Type: MapReveal | Reveal Radius: {waystoneConfig.mapRevealRadius} | Biome: {biome}");
                        break;

                    case WaystoneConfig.WaystoneType.vegetation:
                        Debug.Log($"Waystone Type: Vegetation | Target Vegetation: {waystoneConfig.vegetationInternalName}");
                        break;
                }
            }
        }
        
        string configName = zdo.GetString(WaystoneConfigKey, "");
        waystoneConfig = WaystoneDB.GetWaystoneConfig(configName);
        
        string biomeString = zdo.GetString("MWL_Waystone_Biome", "");
        Enum.TryParse<Heightmap.Biome>(biomeString, out Heightmap.Biome parsedBiome);
        biome = parsedBiome;
        
        Debug.Log("Waystone Awake: Loaded waystone config '" + waystoneConfig.internalName + "'");
    }

    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        if (waystoneConfig.waystoneType == WaystoneConfig.WaystoneType.location)
        {
            Game.instance.DiscoverClosestLocation(waystoneConfig.locationInternalName, this.transform.position, "Pin", (int) Minimap.PinType.Icon3, true, false);
            return true;
        }
        
        if (waystoneConfig.waystoneType == WaystoneConfig.WaystoneType.mapReveal)
        {
            Minimap.instance.Explore(this.transform.position, waystoneConfig.mapRevealRadius);
            
            user.Message(MessageHud.MessageType.Center, $"Map revealed in radius of {waystoneConfig.mapRevealRadius} meters");
            return true;
        }

        return false;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item)
    {
        return true;
    }

    public string GetHoverText()
    {
        string hoverText;
        string waystoneDetails = "";

        waystoneDetails = WaystoneUtils.GetWaystoneDetails(waystoneConfig);
            
        hoverText = Localization.instance.Localize(
            $"{waystoneConfig.displayName}\n" +
            $"[<color=yellow><b>$KEY_Use</b></color>] Use Waystone\n" +
            $"{waystoneDetails}");
            
            return hoverText;
    }

    public string GetHoverName()
    {
        return Localization.instance.Localize(this.name);
    }


}
    
