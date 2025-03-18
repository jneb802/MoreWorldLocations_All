using BepInEx;
using BepInEx.Configuration;
using JetBrains.Annotations;
using UnityEngine;
using Jotunn;
using Jotunn.Extensions;

namespace Common;

public class LocationConfiguration
{
    public ConfigEntry<int> Quantity { get; set; }
    public ConfigEntry<ConfigurationManager.Toggle> CreatureYaml { get; set; }
    public ConfigEntry<string> CreatureList { get; set; }
    public ConfigEntry<ConfigurationManager.Toggle> LootYaml { get; set; }
    public ConfigEntry<string> LootList { get; set; }
    
    public ConfigEntry<ConfigurationManager.Toggle> PickableItemYaml { get; set; }
    
    public ConfigEntry<string> PickableItemList { get; set; }

    public LocationConfiguration(
        ConfigFile config,
        string locationName,
        int spawnQuantity)
    {
        Quantity = ConfigFileExtensions.BindConfigInOrder(config, locationName, "Spawn Quantity", spawnQuantity, 
            "Amount of this location the game will attempt to place during world generation");
    }
    
    public LocationConfiguration(
        ConfigFile config,
        string locationName,
        int spawnQuantity,
        string customCreatureListName,
        string customLootListName)
    {
        Quantity = ConfigFileExtensions.BindConfigInOrder(config, locationName, "Spawn Quantity", spawnQuantity, 
            "Amount of this location the game will attempt to place during world generation");

        CreatureYaml = ConfigFileExtensions.BindConfigInOrder(config, locationName, "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off, 
            "When Off, location will spawn default creatures. When On, location will select creatures from the list in the warpalicious.More_World_Locations_CreatureLists.yml file in the BepInEx config folder");

        CreatureList = ConfigFileExtensions.BindConfigInOrder(config, locationName, "Name of Creature List", customCreatureListName, 
            "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");

        LootYaml = ConfigFileExtensions.BindConfigInOrder(config, locationName, "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off, 
            "When Off, location will use default loot. When On, location will select loot from the list in the warpalicious.More_World_Locations_LootLists.yml file in the BepInEx config folder");

        LootList = ConfigFileExtensions.BindConfigInOrder(config, locationName, "Name of Loot List", customLootListName, 
            "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
    }
    
    public LocationConfiguration(
        ConfigFile config,
        string locationName,
        int spawnQuantity,
        string customCreatureListName,
        string customLootListName,
        string pickableItemListName)
    {
        Quantity = ConfigFileExtensions.BindConfigInOrder(config, locationName, "Spawn Quantity", spawnQuantity, 
            "Amount of this location the game will attempt to place during world generation");

        CreatureYaml = ConfigFileExtensions.BindConfigInOrder(config, locationName, "Use Custom Creature YAML file", ConfigurationManager.Toggle.Off, 
            "When Off, location will spawn default creatures. When On, location will select creatures from the list in the warpalicious.More_World_Locations_CreatureLists.yml file in the BepInEx config folder");

        CreatureList = ConfigFileExtensions.BindConfigInOrder(config, locationName, "Name of Creature List", customCreatureListName, 
            "The name of the creature list to use from warpalicious.More_World_Locations_CreatureLists.yml file");

        LootYaml = ConfigFileExtensions.BindConfigInOrder(config, locationName, "Use Custom Loot YAML file", ConfigurationManager.Toggle.Off, 
            "When Off, location will use default loot. When On, location will select loot from the list in the warpalicious.More_World_Locations_LootLists.yml file in the BepInEx config folder");

        LootList = ConfigFileExtensions.BindConfigInOrder(config, locationName, "Name of Loot List", customLootListName, 
            "The name of the loot list to use from warpalicious.More_World_Locations_LootLists.yml file");
        
        PickableItemYaml = ConfigFileExtensions.BindConfigInOrder(config, locationName, "Use Custom PickableItem YAML file", ConfigurationManager.Toggle.Off, 
            "When Off, location will use default loot. When On, dungeon will select loot from list in the custom YAML file in BepinEx config folder");

        PickableItemList = ConfigFileExtensions.BindConfigInOrder(config, locationName, "Name of PickableItem List", pickableItemListName, 
            "The name of the loot list to use from YAML file");
    }
    
}