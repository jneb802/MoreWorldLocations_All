using System.Collections.Generic;
using System.IO;
using Jotunn.Utils;
using YamlDotNet.RepresentationModel;
using Paths = BepInEx.Paths;

namespace Meadows_Pack_1;

public class WarpYAMLManager
{
    public static string creatureYAMLContent;
    public static string lootYAMLContent;
    
    public static string defaultCreatureYamlContent;
    public static string customCreatureYamlContent;
    public static string defaultlootYamlContent;
    public static string customlootYamlContent;
    
    
    public static void ParseDefaultYamls()
    { 
        defaultCreatureYamlContent = AssetUtils.LoadTextFromResources("warpalicious.More_World_Locations_CreatureLists.yml");
        defaultlootYamlContent = AssetUtils.LoadTextFromResources("warpalicious.More_World_Locations_LootLists.yml");
    }
    
    public static void ParseCustomYamls()
    { 
        var customCreatureListYamLFilePath = Path.Combine(Paths.ConfigPath, "warpalicious.More_World_Locations_CreatureLists.yml");
        
        if (File.Exists(customCreatureListYamLFilePath))
        {
            customCreatureYamlContent = File.ReadAllText(customCreatureListYamLFilePath);
            WarpLogger.Logger.LogInfo("Successfully loaded warpalicious.More_World_Locations_CreatureLists.yml file from BepinEx config folder");
        }
        
        var customLootListYamLFilePath = Path.Combine(Paths.ConfigPath, "warpalicious.More_World_Locations_LootLists.yml");
        if (File.Exists(customLootListYamLFilePath))
        {
            customlootYamlContent = File.ReadAllText(customLootListYamLFilePath);
            WarpLogger.Logger.LogInfo("Successfully loaded warpalicious.More_World_Locations_LootLists.yml file from BepinEx config folder");
        }
    }

    public static string GetCreatureYamlContent(Meadows_Pack_1Plugin.Toggle useCustomCreatureYaml)
    {

        if (useCustomCreatureYaml == Meadows_Pack_1Plugin.Toggle.On)
        {
            return customCreatureYamlContent;
        }
        
        return defaultCreatureYamlContent;
    }
    
    public static string GetLootYamlContent(Meadows_Pack_1Plugin.Toggle useCustomLootYaml)
    {

        if (useCustomLootYaml == Meadows_Pack_1Plugin.Toggle.On)
        {
            return customlootYamlContent;
        }
        return defaultlootYamlContent;
        
    }
    
    
    /*public static void ParseCreatureYAML()
    { 
        if (Meadows_Pack_1Plugin.UseCustomLocationCreatureListYAML.Value ==  Meadows_Pack_1Plugin.Toggle.On)
        {
            var creatureYAMLFilePath = Path.Combine(Paths.ConfigPath, "warpalicious.MWL_Meadows_Pack_1_Creatures.yml");
            creatureYAMLContent = File.ReadAllText(creatureYAMLFilePath);
            WarpLogger.Logger.LogInfo("Successfully loaded warpalicious.MWL_Meadows_Pack_1_Creatures.yml file from BepinEx config folder");
        }
        else
        {
            creatureYAMLContent = AssetUtils.LoadTextFromResources("warpalicious.MWL_Meadows_Pack_1_Creatures.yml");
        }
    }
        
    public static void ParseLootYAML()
    { 
        if (Meadows_Pack_1Plugin.UseCustomLocationLootListYAML.Value == Meadows_Pack_1Plugin.Toggle.On)
        {
            var lootYAMLFilePath = Path.Combine(Paths.ConfigPath, "warpalicious.MWL_Meadows_Pack_1_LootLists.yml");
            lootYAMLContent = File.ReadAllText(lootYAMLFilePath);
            WarpLogger.Logger.LogInfo("Successfully loaded warpalicious.MWL_Meadows_Pack_1_LootLists.yml file from BepinEx config folder");
        }
        else
        {
            lootYAMLContent = AssetUtils.LoadTextFromResources("warpalicious.MWL_Meadows_Pack_1_LootLists.yml");
        }
    }*/
}