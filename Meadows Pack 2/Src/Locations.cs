namespace Meadows_Pack_2;

public class Locations
{
    public static void AddAllLocations()
    {
        var assetBundle = Meadows_Pack_2Plugin.assetBundle;

        Common.LocationManager.AddLocation(assetBundle, "MWL_DeerShrine1", Meadows_Pack_2Plugin.MWL_DeerShrine1_Configuration, LocationConfigs.AllLocationConfigs["MWL_DeerShrine1"], Meadows_Pack_2Plugin.meadows2YAMLManager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_DeerShrine2", Meadows_Pack_2Plugin.MWL_DeerShrine2_Configuration, LocationConfigs.AllLocationConfigs["MWL_DeerShrine2"], Meadows_Pack_2Plugin.meadows2YAMLManager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_MeadowsBarn1", Meadows_Pack_2Plugin.MWL_MeadowsBarn1_Configuration, LocationConfigs.AllLocationConfigs["MWL_MeadowsBarn1"], Meadows_Pack_2Plugin.meadows2YAMLManager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_MeadowsHouse2", Meadows_Pack_2Plugin.MWL_MeadowsHouse2_Configuration, LocationConfigs.AllLocationConfigs["MWL_MeadowsHouse2"], Meadows_Pack_2Plugin.meadows2YAMLManager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_MeadowsRuin1", Meadows_Pack_2Plugin.MWL_MeadowsRuin1_Configuration, LocationConfigs.AllLocationConfigs["MWL_MeadowsRuin1"], Meadows_Pack_2Plugin.meadows2YAMLManager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_MeadowsTomb1", Meadows_Pack_2Plugin.MWL_MeadowsTomb1_Configuration, LocationConfigs.AllLocationConfigs["MWL_MeadowsTomb1"], Meadows_Pack_2Plugin.meadows2YAMLManager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_MeadowsTomb4", Meadows_Pack_2Plugin.MWL_MeadowsTomb4_Configuration, LocationConfigs.AllLocationConfigs["MWL_MeadowsTomb4"], Meadows_Pack_2Plugin.meadows2YAMLManager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_MeadowsTower1", Meadows_Pack_2Plugin.MWL_MeadowsTower1_Configuration, LocationConfigs.AllLocationConfigs["MWL_MeadowsTower1"], Meadows_Pack_2Plugin.meadows2YAMLManager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_OakHut1", Meadows_Pack_2Plugin.MWL_OakHut1_Configuration, LocationConfigs.AllLocationConfigs["MWL_OakHut1"], Meadows_Pack_2Plugin.meadows2YAMLManager);
        Common.LocationManager.AddLocation(assetBundle, "MWL_SmallHouse1", Meadows_Pack_2Plugin.MWL_SmallHouse1_Configuration, LocationConfigs.AllLocationConfigs["MWL_SmallHouse1"], Meadows_Pack_2Plugin.meadows2YAMLManager);

    }
}