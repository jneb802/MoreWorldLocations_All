using HarmonyLib;
using UnityEngine;

namespace More_World_Locations_AIO.Traders;

public class MinimapTraderIcons
{
    public static Sprite anvilSprite;
    public static Sprite tankardSprite;
    public static Sprite coinSprite;

    public static Minimap.LocationSpriteData blackForestBlacksmith1Icon;
    public static Minimap.LocationSpriteData blackForestBlacksmith2Icon;
    public static Minimap.LocationSpriteData mountainsBlacksmith1Icon;
    public static Minimap.LocationSpriteData mistlandsBlacksmith1Icon;
    public static Minimap.LocationSpriteData plainsTavern1Icon;
    public static Minimap.LocationSpriteData oceanTavern1Icon;
    public static Minimap.LocationSpriteData plainsCamp1Icon;

    public static void LoadIcons()
    {
        var assetBundle = Prefabs.vendorsPrefabBundle;
        
        anvilSprite = assetBundle.LoadAsset<Sprite>("Assets/WarpProjects/More World Locations/More World Vendors/LocationsIcons/Anvil.png");
        tankardSprite = assetBundle.LoadAsset<Sprite>("Assets/WarpProjects/More World Locations/More World Vendors/LocationsIcons/Tankard.png");
        coinSprite = assetBundle.LoadAsset<Sprite>("Assets/WarpProjects/More World Locations/More World Vendors/LocationsIcons/Coin.png");
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
        }
    }
}
