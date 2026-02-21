using System.Collections.Generic;
using System.IO;
using Jotunn.Managers;
using Jotunn.Utils;
using More_World_Locations_AIO;
using UnityEngine;
using YamlDotNet.RepresentationModel;
using Paths = BepInEx.Paths;

namespace Common;

public class YAMLManager
{
    public string creatureYAMLContent;
    public string lootYAMLContent;
    
    public string defaultCreatureYamlContent;
    public string customCreatureYamlContent;
    public Dictionary<string, List<GameObject>> creatureListDictionary = new Dictionary<string, List<GameObject>>();
    
    public string defaultlootYamlContent;
    public string customlootYamlContent;
    public Dictionary<string, List<DropTable.DropData>> lootListDictionary = new Dictionary<string, List<DropTable.DropData>>();
    
    public string defaultPickableItemContent;
    public string customPickableItemContent;
    public List<PickableItem.RandomItem> pickableList;
    
    public string defaultTraderYamlContent;
    public string customTraderYamlContent;
    
    public enum Toggle
    {
        On = 1,
        Off = 0 
    }
    
    public void ParseDefaultYamls()
    { 
        Debug.Log("Calling parse yamls");
        defaultCreatureYamlContent = AssetUtils.LoadTextFromResources("warpalicious.More_World_Locations_CreatureLists.yml");
        defaultlootYamlContent = AssetUtils.LoadTextFromResources("warpalicious.More_World_Locations_LootLists.yml");
    }
    
    public void ParseCustomYamls(ConfigurationManager.Toggle useCustomLocationYAML)
    {
        var customCreatureListYamlFilePath = Path.Combine(Paths.ConfigPath, "warpalicious.More_World_Locations_CreatureLists.yml");
        var customLootListYamlFilePath = Path.Combine(Paths.ConfigPath, "warpalicious.More_World_Locations_LootLists.yml");

        if (File.Exists(customCreatureListYamlFilePath))
        {
            try
            {
                customCreatureYamlContent = File.ReadAllText(customCreatureListYamlFilePath);
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo("Successfully loaded warpalicious.More_World_Locations_CreatureLists.yml from BepInEx config folder");
            }
            catch (System.Exception ex)
            {
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogError("Failed to load custom creature YAML: " + ex.Message);
            }
        }

        if (File.Exists(customLootListYamlFilePath))
        {
            try
            {
                customlootYamlContent = File.ReadAllText(customLootListYamlFilePath);
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo("Successfully loaded warpalicious.More_World_Locations_LootLists.yml from BepInEx config folder");
            }
            catch (System.Exception ex)
            {
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogError("Failed to load custom loot YAML: " + ex.Message);
            }
        }
    }
    
    public void ParseCreatureYaml(string filename)
    { 
        defaultCreatureYamlContent = AssetUtils.LoadTextFromResources(filename + "_CreatureLists.yml");
        
        var customCreatureListYamLFilePath = Path.Combine(Paths.ConfigPath, filename + "_CreatureLists.yml");
        if (File.Exists(customCreatureListYamLFilePath))
        {
            customCreatureYamlContent = File.ReadAllText(customCreatureListYamLFilePath);
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo("Successfully loaded + " + filename + "_CreatureLists.yml file from BepinEx config folder");;
        }
    }
    
    public void ParseTraderYaml(string filename, ConfigurationManager.Toggle useCustomTraderYaml)
    {
        defaultTraderYamlContent = AssetUtils.LoadTextFromResources(filename);

        string customTraderListYamlFilePath = Path.Combine(Paths.ConfigPath, filename);

        // Auto-extract if toggle On and file doesn't exist
        if (useCustomTraderYaml == ConfigurationManager.Toggle.On && !File.Exists(customTraderListYamlFilePath))
        {
            try
            {
                File.WriteAllText(customTraderListYamlFilePath, defaultTraderYamlContent);
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo("Auto-extracted " + filename + " to BepInEx config folder");
            }
            catch (System.Exception ex)
            {
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogError("Failed to extract " + filename + ": " + ex.Message);
            }
        }

        if (File.Exists(customTraderListYamlFilePath))
        {
            try
            {
                customTraderYamlContent = File.ReadAllText(customTraderListYamlFilePath);
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo("Successfully loaded " + filename + " from BepInEx config folder");
            }
            catch (System.Exception ex)
            {
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogError("Failed to load custom trader YAML: " + ex.Message);
            }
        }
    }
    
    public void ParseContainerYaml(string filename)
    { 
        defaultlootYamlContent = AssetUtils.LoadTextFromResources(filename + "_ContainerLists.yml");
        
        var customCreatureListYamLFilePath = Path.Combine(Paths.ConfigPath, filename + "_ContainerLists.yml");
        if (File.Exists(customCreatureListYamLFilePath))
        {
            customlootYamlContent = File.ReadAllText(customCreatureListYamLFilePath);
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo("Successfully loaded + " + filename + "_ContainerLists.yml file from BepinEx config folder");;
        }
    }
    
    public void ParsePickableItemYaml(string filename)
    { 
        defaultPickableItemContent = AssetUtils.LoadTextFromResources(filename + "_PickableItemLists.yml");
        
        var customCreatureListYamLFilePath = Path.Combine(Paths.ConfigPath, filename + "_PickableItemLists.yml");
        if (File.Exists(customCreatureListYamLFilePath))
        {
            customPickableItemContent = File.ReadAllText(customCreatureListYamLFilePath);
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo("Successfully loaded + " + filename + "_PickableItemLists.yml file from BepinEx config folder");;
        }
    }

    public string GetCreatureYamlContent(ConfigurationManager.Toggle useCustomCreatureYaml)
    {
        if (useCustomCreatureYaml == ConfigurationManager.Toggle.On && !string.IsNullOrEmpty(customCreatureYamlContent))
        {
            return customCreatureYamlContent;
        }

        return defaultCreatureYamlContent;
    }

    public string GetLootYamlContent(ConfigurationManager.Toggle useCustomLootYaml)
    {
        if (useCustomLootYaml == ConfigurationManager.Toggle.On && !string.IsNullOrEmpty(customlootYamlContent))
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

    public string GetTraderYamlContent(ConfigurationManager.Toggle useCustomTraderYaml)
    {
        if (useCustomTraderYaml == ConfigurationManager.Toggle.On && !string.IsNullOrEmpty(customTraderYamlContent))
        {
            return customTraderYamlContent;
        }
        return defaultTraderYamlContent;
    }

    public void BuildCreatureLists(ConfigurationManager.Toggle useCustomCreatureYAML, string creatureListName)
    {
        // Debug.Log("Creature list built");
        if (useCustomCreatureYAML == ConfigurationManager.Toggle.On)
        {
            List<GameObject> creatureList = Common.CreatureManager_v2.CreateCreatureList(creatureListName, customCreatureYamlContent);
            creatureListDictionary.Add(creatureListName,creatureList);
        }
        else
        { 
            List<GameObject> creatureList = Common.CreatureManager_v2.CreateCreatureList(creatureListName, defaultCreatureYamlContent);
            creatureListDictionary.Add(creatureListName,creatureList);
        }
    }
    
    public void BuildLootList(ConfigurationManager.Toggle useCustomLootYAML, string lootListName)
    {
        // Debug.Log("Loot list built");
        if (useCustomLootYAML == ConfigurationManager.Toggle.On)
        {
            List<DropTable.DropData> list = Common.LootManager.ParseContainerYaml_v2(lootListName, customlootYamlContent);
            lootListDictionary.Add(lootListName,list);
        }
        else
        {
            List<DropTable.DropData> list = Common.LootManager.ParseContainerYaml_v2(lootListName, defaultlootYamlContent);
            lootListDictionary.Add(lootListName,list);
        }
    }
    
    public void BuildPickableList(ConfigurationManager.Toggle useCustomPickableYAML, string pickableListName)
    {
        // Debug.Log("Pickable list built");
        if (useCustomPickableYAML == ConfigurationManager.Toggle.On)
        {
            pickableList = Common.LootManager_v2.ParsePickableYaml(pickableListName,customPickableItemContent);
        }
        else
        {
            pickableList = Common.LootManager_v2.ParsePickableYaml(pickableListName,defaultPickableItemContent);
        }
    }
}