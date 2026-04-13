using Jotunn.Configs;

namespace More_World_Locations_AIO;

public class MWLLocation
{
    public string Name { get; set; }
    public string AssetPath { get; set; }
    public LocationConfig Config { get; set; }

    public void Register()
    {
        Config.Quantity = LocationQuantityManager.GetQuantity(Name);
        Common.LocationManager.AddLocation(Name, Config);
    }
}
