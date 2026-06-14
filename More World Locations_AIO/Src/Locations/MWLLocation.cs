using Jotunn.Configs;

namespace More_World_Locations_AIO;

public class MWLLocation
{
    public string Name { get; set; }
    public string AssetPath { get; set; }
    public string DungeonTheme { get; set; }
    public string InteriorPrefabName { get; set; }
    public LocationConfig Config { get; set; }

    public void Register()
    {
        Config.Quantity = LocationQuantityManager.GetQuantity(Name);

        if (!string.IsNullOrEmpty(DungeonTheme) || !string.IsNullOrEmpty(InteriorPrefabName))
        {
            Common.LocationManager.AddLocation(Name, Config, DungeonTheme, InteriorPrefabName);
        }
        else
        {
            Common.LocationManager.AddLocation(Name, Config);
        }
    }
}
