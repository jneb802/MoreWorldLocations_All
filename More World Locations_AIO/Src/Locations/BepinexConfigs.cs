using BepInEx.Configuration;
using Jotunn.Extensions;

namespace More_World_Locations_AIO;

public class BepinexConfigs
{
    public static BepInEx.Configuration.ConfigFile Config;

    public static ConfigEntry<PortInit.Toggle> EnableShrines = null!;
    public static ConfigEntry<PortInit.Toggle> EnableWaystones = null!;
    public static ConfigEntry<PortInit.Toggle> EnableTraders = null!;
    public static ConfigEntry<PortInit.Toggle> EnableTrainers = null!;

    public static void BindFeatureConfigs()
    {
        EnableShrines = PortInit.plugin.Config.BindConfig("0 - Features", "Enable Shrines", PortInit.Toggle.On,
            "If Off, shrine ward objects will be destroyed on load", synced: true);
        EnableWaystones = PortInit.plugin.Config.BindConfig("0 - Features", "Enable Waystones", PortInit.Toggle.On,
            "If Off, waystone ward objects will be destroyed on load", synced: true);
        EnableTraders = PortInit.plugin.Config.BindConfig("0 - Features", "Enable Traders", PortInit.Toggle.On,
            "If Off, trader locations (taverns, blacksmiths, material vendors) will not spawn", synced: true);
        EnableTrainers = PortInit.plugin.Config.BindConfig("0 - Features", "Enable Trainers", PortInit.Toggle.On,
            "If Off, trainer locations (skill book vendors) will not spawn", synced: true);
    }
}
