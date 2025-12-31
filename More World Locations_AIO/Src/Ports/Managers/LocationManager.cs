using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using JetBrains.Annotations;
using Jotunn.Extensions;
using SoftReferenceableAssets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace More_World_Locations_AIO.Managers;

[PublicAPI]
public class LocationManager
{
	private static BaseUnityPlugin? _plugin;
	internal static BaseUnityPlugin plugin
	{
		get
		{
			if (_plugin is null)
			{
				IEnumerable<TypeInfo> types;
				try
				{
					types = Assembly.GetExecutingAssembly().DefinedTypes.ToList();
				}
				catch (ReflectionTypeLoadException e)
				{
					types = e.Types.Where(t => t != null).Select(t => t.GetTypeInfo());
				}
				_plugin = (BaseUnityPlugin)BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent(types.First(t => t.IsClass && typeof(BaseUnityPlugin).IsAssignableFrom(t)));
			}
			return _plugin;
		}
	}
	
    private static Dictionary<string, Sprite>? _inGameOptions;
    private static Dictionary<string, Sprite> InGameOptions
    {
        get
        {
            if (_inGameOptions != null) return _inGameOptions;
            if (Minimap.instance == null) return new();
            _inGameOptions = new Dictionary<string, Sprite>();
            foreach (var icon in Minimap.instance.m_icons)
            {
                _inGameOptions[icon.m_name.ToString()] = icon.m_icon;
            }

            foreach (var icon in Minimap.instance.m_locationIcons)
            {
                _inGameOptions[icon.m_name] = icon.m_icon;
            }

            return _inGameOptions;
        }
    }
    
    internal static void Patch_SetupLocations(ZoneSystem __instance)
    {
	    foreach (CustomLocation location in CustomLocation.locations.Values)
	    {
		    ZoneSystem.ZoneLocation data = location.GetLocation();
		    if (data.m_prefab.IsValid)
		    {
			    __instance.m_locations.Add(data);
			    Debug.Log("[Location Manager] registered custom location: " + data.m_prefabName);
		    }
		    else
		    {
			    Debug.LogWarning("[Location Manager] " + data.m_prefabName + " is not valid " + ", is available: " + Runtime.Loader.IsAvailable(data.m_prefab.m_assetID));
		    }
	    }
    }
    
    internal static bool Patch_Location_Awake(Location __instance)
    {
        if (!CustomLocation.locations.TryGetValue(Helpers.GetNormalizedName(__instance.name), out CustomLocation? data)) return true;
        Location.s_allLocations.Add(__instance);
        if (!__instance.m_hasInterior || !data.InteriorEnvironment.Enabled) return false;
        Vector3 zoneCenter = __instance.GetZoneCenter();
        GameObject environment = Object.Instantiate(__instance.m_interiorPrefab, new Vector3(zoneCenter.x, __instance.transform.position.y + data.InteriorEnvironment.Altitude, zoneCenter.z), Quaternion.identity, __instance.transform);
        environment.transform.localScale = data.InteriorEnvironment.Scale;
        environment.GetComponent<EnvZone>().m_environment = data.InteriorEnvironment.Environment;

        return false;
    }
    
    internal static void Patch_Minimap_Awake(Minimap __instance)
    {
        foreach (CustomLocation? location in CustomLocation.locations.Values)
        {
            if (location.Icon.GetSprite() is not {} sprite) continue;
            __instance.m_locationIcons.Add(new Minimap.LocationSpriteData()
            {
                m_name = location.Prefab.name,
                m_icon = sprite
            });
        }
    }
    
    [PublicAPI]
    public class CustomLocation
    {
	    internal static Dictionary<string, CustomLocation> locations = new ();
	    
	    public readonly GameObject Prefab;
	    public readonly AssetID AssetID;
	    public Heightmap.Biome Biome = Heightmap.Biome.None;
	    public Heightmap.BiomeArea BiomeArea = Heightmap.BiomeArea.Everything;
	    public GroupSettings Group = new();
	    public PlacementSettings Placement = new();
	    public IconSettings Icon = new();
	    public InteriorEnvironmentSettings InteriorEnvironment = new();
	    public Configs configs = new();

	    public CustomLocation(string assetBundleName, string prefabName) : this(AssetBundleManager.GetAssetBundle(assetBundleName), prefabName){}
	    
	    public CustomLocation(AssetBundle assetBundle, string prefabName) : this(assetBundle.LoadAsset<GameObject>(prefabName)){}

	    public CustomLocation(GameObject prefab)
	    {
		    Prefab = prefab;
		    Load();
		    AssetID = SoftAssetLoader.AddAsset(prefab);
		    locations[prefab.name] = this;
	    }

	    public CustomLocation(GameObject prefab, bool isBlueprint)
	    {
		    Prefab = prefab;
		    Load();
		    AssetID = SoftAssetLoader.AddBlueprintAsset(prefab);
		    locations[prefab.name] = this;
	    }

	    internal void Setup()
	    {
		    configs.Enabled = ConfigFileExtensions.BindConfigInOrder(plugin.Config, Prefab.name, "Enabled", PortInit.Toggle.On, $"If on, {Prefab.name} will load");
		    configs.Enabled.SettingChanged += (_, _) =>
		    {
			    if (registeredLocation == null) return;
			    registeredLocation.m_enable = configs.Enabled.Value is PortInit.Toggle.On;
		    };
		    configs.Quantity = ConfigFileExtensions.BindConfigInOrder(plugin.Config, Prefab.name, "Quantity", Placement.Quantity, "Set initial amount to spawn in world");
		    configs.Quantity.SettingChanged += (_, _) =>
		    {
			    Placement.Quantity = configs.Quantity.Value;
			    if (registeredLocation == null) return;
			    registeredLocation.m_quantity = configs.Quantity.Value;
		    };
	    }
	    
	    private void Load()
	    {
		    if (!Prefab.TryGetComponent(out Location component)) return;
		    Placement.ClearArea = component.m_clearArea;
		    Placement.ExteriorRadius = component.m_exteriorRadius;
		    Placement.InteriorRadius =  component.m_interiorRadius;
	    }
	    
		internal ZoneSystem.ZoneLocation? registeredLocation;
		
	    internal ZoneSystem.ZoneLocation GetLocation()
	    {
		    var data = new ZoneSystem.ZoneLocation()
		    {
				m_enable = configs.Enabled!.Value is PortInit.Toggle.On,
                m_prefabName = Prefab.name,
                m_prefab = SoftAssetLoader.GetSoftReference(AssetID),
                m_biome = Biome,
                m_biomeArea = BiomeArea,
                m_quantity = Placement.Quantity,
                m_prioritized = Placement.Prioritized,
                m_centerFirst = Placement.CenterFirst,
                m_unique = Placement.Unique,
                m_group = Group.Name,
                m_groupMax = Group.MaxName,
                m_minDistanceFromSimilar = Placement.DistanceFromSimilar.Min,
                m_maxDistanceFromSimilar = Placement.DistanceFromSimilar.Max,
                m_iconAlways = Icon.Always,
                m_iconPlaced = Icon.Enabled,
                m_randomRotation = Placement.RandomRotation,
                m_slopeRotation = Placement.SlopeRotation,
                m_snapToWater = Placement.SnapToWater,
                m_interiorRadius = Placement.InteriorRadius,
                m_exteriorRadius = Placement.ExteriorRadius,
                m_clearArea = Placement.ClearArea,
                m_minTerrainDelta = Placement.TerrainDeltaCheck.Min,
                m_maxTerrainDelta = Placement.TerrainDeltaCheck.Max,
                m_minimumVegetation = Placement.VegetationCheck.Radius.Min,
                m_maximumVegetation = Placement.VegetationCheck.Radius.Max,
                m_surroundCheckVegetation = Placement.VegetationCheck.Check,
                m_surroundCheckDistance = Placement.VegetationCheck.CheckDistance,
                m_surroundCheckLayers = Placement.VegetationCheck.Layers,
                m_surroundBetterThanAverage = Placement.VegetationCheck.BetterThanAverage,
                m_inForest = Placement.InForest,
                m_forestTresholdMin = Placement.ForestThreshold.Min,
                m_forestTresholdMax = Placement.ForestThreshold.Max,
                m_minDistance = Placement.Distance.Min,
                m_maxDistance = Placement.Distance.Max,
                m_minAltitude = Placement.Altitude.Min,
                m_maxAltitude = Placement.Altitude.Max,
                m_foldout = Placement.Foldout,
		    };

			registeredLocation = data;
		    return data;
	    }
    }

    [PublicAPI]
    public class InteriorEnvironmentSettings
    {
	    public float Altitude = 5000f;
	    public Vector3 Scale = new(200f, 500f, 200f);
	    public string Environment = string.Empty;
	    public bool Enabled;
    }

    
    //TODO add location quantity here
    [PublicAPI]
    public class Configs
    {
	    internal ConfigEntry<PortInit.Toggle>? Enabled;
	    internal ConfigEntry<int> Quantity;
    }

    [PublicAPI]
    public class MinMaxSettings
    {
	    public float Min;
	    public float Max;

	    public MinMaxSettings(float min, float max)
	    {
		    Min = min;
		    Max = max;
	    }
    }
    [PublicAPI]
    public class GroupSettings
    {
	    public string Name = "";
		public string MaxName = "";
    }
    [PublicAPI]
    public class PlacementSettings
    {
	    public int Quantity;
		public bool Prioritized;
		public bool CenterFirst;
		public bool Unique;
		public MinMaxSettings DistanceFromSimilar = new(0f, 0f);
		public bool RandomRotation;
		public bool SlopeRotation;
		public bool SnapToWater;
		public float InteriorRadius;
		public float ExteriorRadius = 50f;
		public bool ClearArea;
		public MinMaxSettings TerrainDeltaCheck = new(0f, 100f);
		public VegetationSettings VegetationCheck = new();
		public bool InForest;
		public MinMaxSettings ForestThreshold = new(0f, 1f);
		public MinMaxSettings Distance = new(0f, 10000f);
		public MinMaxSettings Altitude = new(0f, 1000f);
		public bool Foldout;
    }

    [PublicAPI]
    public class VegetationSettings
    {
	    public bool Check;
	    public float CheckDistance;
	    public int Layers = 2;
		public float BetterThanAverage;
	    public MinMaxSettings Radius = new(0f, 1f);
    }

    [PublicAPI]
    public class IconSettings
    {
	    public bool Always;
	    public bool Enabled;
	    public Sprite? Icon;
	    public LocationIcon InGameIcon = LocationIcon.None;
	    
	    public void Set(Sprite sprite) => Icon = sprite;
	    public void Set(LocationIcon icon) => InGameIcon = icon;

	    internal Sprite? GetSprite()
	    {
		    if (Icon != null) return Icon;
		    if (InGameIcon == LocationIcon.None) return null;
		    return InGameOptions.TryGetValue(InGameIcon.GetInternalName(), out Sprite sprite) ? sprite : null;
	    }

	    [PublicAPI]
	    public enum LocationIcon
	    {
		    [InternalName("None")]None,
		    [InternalName("StartTemple")]StartTemple,
		    [InternalName("Vendor_BlackForest")]Haldor,
		    [InternalName("Hildir_camp")]Hildir,
		    [InternalName("BogWitch_Camp")]BogWitch,
		    [InternalName("Icon 0")]Fire,
		    [InternalName("Icon 1")]House,
		    [InternalName("Icon 2")]Hammer,
		    [InternalName("Icon 3")]Pin,
		    [InternalName("Icon 4")]Portal,
		    [InternalName("Death")]Death,
		    [InternalName("Bed")]Bed,
		    [InternalName("Should")]Shout,
		    [InternalName("Boss")]Boss,
		    [InternalName("Player")]Player,
		    [InternalName("RandomEvent")]Event,
		    [InternalName("EventArea")]EventArea,
		    [InternalName("Ping")]Ping,
		    [InternalName("Hildir1")]QuestionMark,
	    }
	    
	    internal class InternalName : Attribute
	    {
		    public readonly string internalName;
		    public InternalName(string internalName) => this.internalName = internalName;
	    }
    }
}