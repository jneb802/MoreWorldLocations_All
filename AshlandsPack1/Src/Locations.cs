using Common;

namespace AshlandsPack1;

public class Locations
{
    public enum LocationPosition
    {
        Interior,
        Exterior
    }

    public static void AddAllLocations()
    {
        var assetBundle = AshlandsPack1Plugin.assetBundle;
        var creatureYAMLContent = AshlandsPack1Plugin.ashlandsYAMLManager.creatureYAMLContent;
        var lootYAMLContent = AshlandsPack1Plugin.ashlandsYAMLManager.lootYAMLContent;

        LocationManager.AddLocation(assetBundle,
            "MWL_AshlandsFort1",
            AshlandsPack1Plugin.ashlandsYAMLManager.GetCreatureYamlContent(AshlandsPack1Plugin.MWL_AshlandsFort1_Configuration.CreatureYaml.Value),
            AshlandsPack1Plugin.MWL_AshlandsFort1_Configuration.CreatureList.Value,
            AshlandsPack1Plugin.ashlandsYAMLManager.GetLootYamlContent(AshlandsPack1Plugin.MWL_AshlandsFort1_Configuration.LootYaml.Value),
            AshlandsPack1Plugin.MWL_AshlandsFort1_Configuration.LootList.Value,
            LocationConfigs.MWL_AshlandsFort1_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_AshlandsFort2",
            AshlandsPack1Plugin.ashlandsYAMLManager.GetCreatureYamlContent(AshlandsPack1Plugin.MWL_AshlandsFort2_Configuration.CreatureYaml.Value),
            AshlandsPack1Plugin.MWL_AshlandsFort2_Configuration.CreatureList.Value,
            AshlandsPack1Plugin.ashlandsYAMLManager.GetLootYamlContent(AshlandsPack1Plugin.MWL_AshlandsFort2_Configuration.LootYaml.Value),
            AshlandsPack1Plugin.MWL_AshlandsFort2_Configuration.LootList.Value,
            LocationConfigs.MWL_AshlandsFort2_Config);
        
        LocationManager.AddLocation(assetBundle,
            "MWL_AshlandsFort3",
            AshlandsPack1Plugin.ashlandsYAMLManager.GetCreatureYamlContent(AshlandsPack1Plugin.MWL_AshlandsFort3_Configuration.CreatureYaml.Value),
            AshlandsPack1Plugin.MWL_AshlandsFort3_Configuration.CreatureList.Value,
            AshlandsPack1Plugin.ashlandsYAMLManager.GetLootYamlContent(AshlandsPack1Plugin.MWL_AshlandsFort3_Configuration.LootYaml.Value),
            AshlandsPack1Plugin.MWL_AshlandsFort3_Configuration.LootList.Value,
            LocationConfigs.MWL_AshlandsFort3_Config);
    }
}