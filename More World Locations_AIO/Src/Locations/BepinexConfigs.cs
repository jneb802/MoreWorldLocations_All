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
    public static ConfigEntry<PortInit.Toggle> UseCustomTraderConfigs = null!;
    public static ConfigEntry<PortInit.Toggle> UseCustomLocationYAML = null!;
    public static ConfigEntry<PortInit.Toggle> UseCustomLocalization = null!;
    public static ConfigEntry<PortInit.Toggle> ServerOnly = null!;

    public static void BindFeatureConfigs()
    {
        // Not synced: this entry decides which peers are admitted at all, so a
        // value that only arrives after admission would be no use, and a client
        // has no business overriding it. It is a config key rather than an
        // environment variable because it is an operating mode an operator sets
        // once, not a validation switch.
        ServerOnly = PortInit.plugin.Config.BindConfig("0 - Server Only", "Enable Server Only Mode", PortInit.Toggle.Off,
            "If On, this server serves an audited subset of MWL locations to players who have no mod installed: " +
            "peers that do not answer MWL's version check are admitted, and only approved templates are registered. " +
            "Ports, traders, trainers and dungeons are never registered in this mode.", synced: false);

        EnableShrines = PortInit.plugin.Config.BindConfig("0 - Features", "Enable Shrines", PortInit.Toggle.On,
            "If Off, shrine ward objects will be destroyed on load", synced: true);
        EnableWaystones = PortInit.plugin.Config.BindConfig("0 - Features", "Enable Waystones", PortInit.Toggle.On,
            "If Off, waystone ward objects will be destroyed on load", synced: true);
        EnableTraders = PortInit.plugin.Config.BindConfig("0 - Features", "Enable Traders", PortInit.Toggle.On,
            "If Off, trader locations (taverns, blacksmiths, material vendors) will not spawn", synced: true);
        EnableTrainers = PortInit.plugin.Config.BindConfig("0 - Features", "Enable Trainers", PortInit.Toggle.On,
            "If Off, trainer locations (skill book vendors) will not spawn", synced: true);
        UseCustomTraderConfigs = PortInit.plugin.Config.BindConfig("0 - Features", "Use Custom Trader Configs", PortInit.Toggle.Off,
            "If On, uses warpalicious.More_World_Locations_TraderItems.yml from config folder. Auto-extracts default if missing.", synced: true);
        UseCustomLocationYAML = PortInit.plugin.Config.BindConfig("0 - Features", "Use Custom Location YAML", PortInit.Toggle.Off,
            "If On, location spawn quantities will be loaded from warpalicious.More_World_Locations_LocationConfigs.yml in BepInEx config folder. Auto-extracts defaults if missing.", synced: true);
        UseCustomLocalization = PortInit.plugin.Config.BindConfig("0 - Features", "Use Custom Localization", PortInit.Toggle.Off,
            "If On, loads localization YAML files from BepInEx config folder. Place warpalicious.More_World_Locations_Localization.{Language}.yml in config folder. Auto-extracts English template if missing.", synced: false);
    }
}
