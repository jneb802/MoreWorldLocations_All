using BepInEx.Configuration;
using Jotunn.Configs;

namespace Common;

public class WarpLocation
{
   public string LocationName { get; set; }
   public int SpawnQuantity { get; set; }
   public string CreatureYaml { get; set; }
   public string LootYaml { get; set; }
   public LocationConfiguration BepinExConfig { get; set; } 
   
   
   
   public LocationConfig JotunnLocationConfig { get; set; }


   public WarpLocation(
       ConfigFile config,
       string locationName,
       int spawnQuantity,
       string creatureYaml,
       string lootYaml)
   {
       BepinExConfig = new LocationConfiguration(config, locationName, spawnQuantity, creatureYaml, lootYaml);
       
   }
}
