using HarmonyLib;
using UnityEngine;

namespace More_World_Locations_AIO.Traders;

public class MinimapTraderIcons
{
    public static Sprite anvilSprite;
    public static Sprite tankardSprite;
    public static Sprite coinSprite;
    public static Sprite achorSprite;

    // Traders
    public static Minimap.LocationSpriteData blackForestBlacksmith1Icon;
    public static Minimap.LocationSpriteData blackForestBlacksmith2Icon;
    public static Minimap.LocationSpriteData mountainsBlacksmith1Icon;
    public static Minimap.LocationSpriteData mistlandsBlacksmith1Icon;
    public static Minimap.LocationSpriteData plainsTavern1Icon;
    public static Minimap.LocationSpriteData oceanTavern1Icon;
    public static Minimap.LocationSpriteData plainsCamp1Icon;
    
    // Trainers
    public static Minimap.LocationSpriteData meadowsTrainer1Icon;
    public static Minimap.LocationSpriteData swampTrainer1Icon;
    public static Minimap.LocationSpriteData plainsTrainer1Icon;
    public static Minimap.LocationSpriteData mistTrainer1Icon;

    // Ports
    public static Minimap.LocationSpriteData port1Icon;
    public static Minimap.LocationSpriteData port2Icon;
    public static Minimap.LocationSpriteData port3Icon;
    public static Minimap.LocationSpriteData port4Icon;
    public static Minimap.LocationSpriteData port5Icon;
    
    public static void LoadIcons()
    {
        var assetBundle = Prefabs.vendorsPrefabBundle;
        var portIconAssetBundle = Prefabs.portIconBundle;
        
        anvilSprite = assetBundle.LoadAsset<Sprite>("Assets/WarpProjects/More World Locations/More World Vendors/LocationsIcons/Anvil.png");
        tankardSprite = assetBundle.LoadAsset<Sprite>("Assets/WarpProjects/More World Locations/More World Vendors/LocationsIcons/Tankard.png");
        coinSprite = assetBundle.LoadAsset<Sprite>("Assets/WarpProjects/More World Locations/More World Vendors/LocationsIcons/Coin.png");
        achorSprite = portIconAssetBundle.LoadAsset<Sprite>("Assets/WarpProjects/MoreWorldTraders/LocationsIcons/Anchor.png");
    }

    public static void BuildLocationSpriteData()
    {
        blackForestBlacksmith1Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_BlackForestBlacksmith1",
            m_icon = anvilSprite
        };

        blackForestBlacksmith2Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_BlackForestBlacksmith2",
            m_icon = anvilSprite
        };

        mountainsBlacksmith1Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_MountainsBlacksmith1",
            m_icon = anvilSprite
        };

        mistlandsBlacksmith1Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_MistlandsBlacksmith1",
            m_icon = anvilSprite
        };

        plainsTavern1Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_PlainsTavern1",
            m_icon = tankardSprite
        };
        
        oceanTavern1Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_OceanTavern1",
            m_icon = tankardSprite
        };

        plainsCamp1Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_PlainsCamp1",
            m_icon = coinSprite
        };

        meadowsTrainer1Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_MeadowsTrainer1",
            m_icon = coinSprite
        };

        swampTrainer1Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_SwampTrainer1",
            m_icon = coinSprite
        };

        plainsTrainer1Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_PlainsTrainer1",
            m_icon = coinSprite
        };

        mistTrainer1Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_MistTrainer1",
            m_icon = coinSprite
        };
        
        mistTrainer1Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_MistTrainer1",
            m_icon = coinSprite
        };
        
        port1Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_Port1",
            m_icon = achorSprite
        };
        
        port2Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_Port2",
            m_icon = achorSprite
        };
        
        port3Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_Port3",
            m_icon = achorSprite
        };
        
        port4Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_Port4",
            m_icon = achorSprite
        };
        
        port5Icon = new Minimap.LocationSpriteData
        {
            m_name = "MWL_Port5",
            m_icon = achorSprite
        };
    }
    
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.Awake))]
    public static class Minimap_Awake_Patch
    {
        public static void Postfix(Minimap __instance)
        {
            if (__instance.GetLocationIcon(blackForestBlacksmith1Icon.m_name) == null)
            {
                __instance.m_locationIcons.Add(blackForestBlacksmith1Icon);
            }
            if (__instance.GetLocationIcon(blackForestBlacksmith2Icon.m_name) == null)
            {
                __instance.m_locationIcons.Add(blackForestBlacksmith2Icon);
            }
            if (__instance.GetLocationIcon(mountainsBlacksmith1Icon.m_name) == null)
            {
                __instance.m_locationIcons.Add(mountainsBlacksmith1Icon);
            }
            if (__instance.GetLocationIcon(mistlandsBlacksmith1Icon.m_name) == null)
            {
                __instance.m_locationIcons.Add(mistlandsBlacksmith1Icon);
            }
            if (__instance.GetLocationIcon(plainsTavern1Icon.m_name) == null)
            {
                __instance.m_locationIcons.Add(plainsTavern1Icon);
            }
            if (__instance.GetLocationIcon(oceanTavern1Icon.m_name) == null)
            {
                __instance.m_locationIcons.Add(oceanTavern1Icon);
            }
            if (__instance.GetLocationIcon(plainsCamp1Icon.m_name) == null)
            {
                __instance.m_locationIcons.Add(plainsCamp1Icon);
            }
            if (__instance.GetLocationIcon(meadowsTrainer1Icon.m_name) == null)
            {
                __instance.m_locationIcons.Add(meadowsTrainer1Icon);
            }
            if (__instance.GetLocationIcon(swampTrainer1Icon.m_name) == null)
            {
                __instance.m_locationIcons.Add(swampTrainer1Icon);
            }
            if (__instance.GetLocationIcon(plainsTrainer1Icon.m_name) == null)
            {
                __instance.m_locationIcons.Add(plainsTrainer1Icon);
            }
            if (__instance.GetLocationIcon(mistTrainer1Icon.m_name) == null)
            {
                __instance.m_locationIcons.Add(mistTrainer1Icon);
            }
            if (__instance.GetLocationIcon(port1Icon.m_name) == null)
            {
                __instance.m_locationIcons.Add(port1Icon);
            }
            if (__instance.GetLocationIcon(port2Icon.m_name) == null)
            {
                __instance.m_locationIcons.Add(port2Icon);
            }
            if (__instance.GetLocationIcon(port3Icon.m_name) == null)
            {
                __instance.m_locationIcons.Add(port3Icon);
            }
            if (__instance.GetLocationIcon(port4Icon.m_name) == null)
            {
                __instance.m_locationIcons.Add(port4Icon);
            }
            if (__instance.GetLocationIcon(port5Icon.m_name) == null)
            {
                __instance.m_locationIcons.Add(port5Icon);
            }
        }
    }
}
