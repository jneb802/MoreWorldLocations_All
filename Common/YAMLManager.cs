using System.Collections.Generic;
using System.IO;
using Jotunn.Utils;
using YamlDotNet.RepresentationModel;
using Paths = BepInEx.Paths;

namespace Common;

public class YAMLManager
{
    public string creatureYAMLContent;
    public string lootYAMLContent;
    
    public string defaultCreatureYamlContent;
    public string customCreatureYamlContent;
    
    public string defaultlootYamlContent;
    public string customlootYamlContent;
    
    public string defaultPickableItemContent;
    public string customPickableItemContent;
    
    public string defaultTraderYamlContent;
    public string customTraderYamlContent;
    
    public enum Toggle
    {
        On = 1,
        Off = 0 
    }
    
    public void ParseDefaultYamls()
    { 
        defaultCreatureYamlContent = AssetUtils.LoadTextFromResources("warpalicious.More_World_Locations_CreatureLists.yml");
        defaultlootYamlContent = AssetUtils.LoadTextFromResources("warpalicious.More_World_Locations_LootLists.yml");
    }
    
    public void ParseCustomYamls()
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
    
    public void ParseCreatureYaml(string filename)
    { 
        defaultCreatureYamlContent = AssetUtils.LoadTextFromResources(filename + "_CreatureLists.yml");
        
        var customCreatureListYamLFilePath = Path.Combine(Paths.ConfigPath, filename + "_CreatureLists.yml");
        if (File.Exists(customCreatureListYamLFilePath))
        {
            customCreatureYamlContent = File.ReadAllText(customCreatureListYamLFilePath);
            WarpLogger.Logger.LogInfo("Successfully loaded + " + filename + "_CreatureLists.yml file from BepinEx config folder");;
        }
    }
    
    public void ParseTraderYaml(string filename)
    {
        defaultTraderYamlContent = AssetUtils.LoadTextFromResources(filename);
        
        string customTraderListYamLFilePath = Path.Combine(Paths.ConfigPath, filename);
        if (File.Exists(customTraderListYamLFilePath))
        {
            customTraderYamlContent = File.ReadAllText(customTraderListYamLFilePath);
            WarpLogger.Logger.LogInfo("Successfully loaded + " + filename + " file from BepinEx config folder");;
        }
    }
    
    public void ParseContainerYaml(string filename)
    { 
        defaultlootYamlContent = AssetUtils.LoadTextFromResources(filename + "_ContainerLists.yml");
        
        var customCreatureListYamLFilePath = Path.Combine(Paths.ConfigPath, filename + "_ContainerLists.yml");
        if (File.Exists(customCreatureListYamLFilePath))
        {
            customlootYamlContent = File.ReadAllText(customCreatureListYamLFilePath);
            WarpLogger.Logger.LogInfo("Successfully loaded + " + filename + "_ContainerLists.yml file from BepinEx config folder");;
        }
    }
    
    public void ParsePickableItemYaml(string filename)
    { 
        defaultPickableItemContent = AssetUtils.LoadTextFromResources(filename + "_PickableItemLists.yml");
        
        var customCreatureListYamLFilePath = Path.Combine(Paths.ConfigPath, filename + "_PickableItemLists.yml");
        if (File.Exists(customCreatureListYamLFilePath))
        {
            customPickableItemContent = File.ReadAllText(customCreatureListYamLFilePath);
            WarpLogger.Logger.LogInfo("Successfully loaded + " + filename + "_PickableItemLists.yml file from BepinEx config folder");;
        }
    }

    public string GetCreatureYamlContent(ConfigurationManager.Toggle useCustomCreatureYaml)
    {

        if (useCustomCreatureYaml == ConfigurationManager.Toggle.On)
        {
            return customCreatureYamlContent;
        }
        
        return defaultCreatureYamlContent;
    }
    
    public string GetLootYamlContent(ConfigurationManager.Toggle useCustomLootYaml)
    {

        if (useCustomLootYaml == ConfigurationManager.Toggle.On)
        {
            return customlootYamlContent;
        }
        return defaultlootYamlContent;
        
    }
    
    public string GetPickableItemContent(ConfigurationManager.Toggle useCustomPickableItemYAML)
    {

        if (useCustomPickableItemYAML == ConfigurationManager.Toggle.On)
        {
            return customPickableItemContent;
        }
        return defaultPickableItemContent;
        
    }
    
}