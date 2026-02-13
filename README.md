![More World Locations AIO](https://i.imgur.com/Xd52Zfj.png)

# More World Locations
This mod massively enhances the world exploration component of Valheim by adding 153 new custom locations across all biomes and multiple new POI experiences.

## Table of Contents
- [Features](#features)
- [Location Previews](#location-previews)
- [Shipping Ports](#shipping-ports)
- [Shrines](#shrines)
- [Waystones](#waystones)
- [Traders](#traders)
  - [Trader Locations](#trader-locations)
  - [Trainers & Skill Books](#trainers--skill-books)
  - [Blacksmith Stones](#blacksmith-stones)
  - [Minimap Icons](#minimap-icons)
- [Custom Lore](#custom-lore)
- [Valheim SoftReferenceableAssets](#valheim-softreferenceableassets)
- [World Rings System](#world-rings-system)
- [Feature Toggle Configs](#feature-toggle-configs)
- [Instructions - Manually Installing Mod](#instructions---manually-installing-mod)
- [Instructions - Adding Locations to World](#instructions---adding-locations-to-world)
- [More World Locations](#more-world-locations-1)
- [All-in-One Pack vs. Individual Packs](#all-in-one-pack-vs-individual-packs)
- [FAQ](#faq)
- [Known Incompatibilities](#known-incompatibilities)
- [Mod Support & Feedback](#mod-support--feedback)
- [Donations/Tips](#donationstips)
- [Source Code](#source-code)
- [Credit & Thanks](#credit--thanks)

## Features
- Adds 153 new custom locations across all biomes. Each spawns up to 20 times.
- Adds Shipping Port locations to ship items and teleport between other discovered shipping port locations.
- Adds Shrines that provide temporary buffs with randomized effects and durations.
- Adds Waystones that mark dungeon locations on the map or reveal unexplored map area.
- Adds Trader NPCs across multiple biomes that sell unique items including Blacksmith Stones and Skill Books.
- Adds Trainer NPCs across multiple biomes that sell Skill Books for character progression.
- Adds custom lore and unique events to 2 locations.
- Location spawn quantities are configurable via YAML (`warpalicious.More_World_Locations_AIO.LocationConfigs.yml` in the BepInEx config folder). Customize how many of each location spawns in the world.
- Feature toggle configs allow you to disable shipping ports, traders, trainers, shrines, and waystones individually.
- Improved asset loading performance via Valheim SoftReferenceableAssets system to significantly improve client RAM usage. The assetBundle and assetBundleManifest files are shipped as separate files and they must be included.
- Anonymous analytics to help improve the mod (opt-out available in config).

## Location Previews
<details>
  <summary>Adds 27 locations to the Meadows.</summary>

  ![Meadows Pack 1](https://i.imgur.com/pPFkrDi.png)

</details>
<details>
  <summary>Adds 37 locations to the Blackforest.</summary>

  ![Blackforest Pack 1](https://i.imgur.com/5lKv9F0.png)

</details>
<details>
  <summary>Adds 29 locations to the Swamp.</summary>

  ![Swamp Pack 1](https://i.imgur.com/l62Do90.png)

</details>
<details>
  <summary>Adds 18 locations to the Mountain.</summary>

  ![Mountains Pack 1](https://i.imgur.com/BvdATdc.png)

</details>
<details>
  <summary>Adds 17 locations to the Plains.</summary>

  ![Plains Pack 1](https://i.imgur.com/APTsEfG.png)

</details>
<details>
  <summary>Adds 23 locations to the Mistlands.</summary>

  ![Mistlands Pack 1](https://i.imgur.com/uy5nOkT.png)

</details>
<details>
  <summary>Adds 3 locations to the Ashlands.</summary>

  ![Ashlands Pack 1](https://i.imgur.com/hnmWJXh.png)

</details>

## Shipping Ports
Shipping Ports are special coastal locations that introduce an item logistics system to Valheim. Four port locations spawn along coastlines in the Meadows, Blackforest, Plains, and Mistlands biomes, with each port spawning up to 5 times in a world.

**Discovering Ports:** Each port is staffed by an NPC dockmaster. You must interact with the NPC to discover the port and add it to your network. Only discovered ports can be used as shipping destinations.

**Sending Shipments:**
- Purchase a shipping manifest (functions as a chest) from the port NPC.
- Fill the manifest with the items you want to ship.
- Select a destination from your list of discovered ports.
- Pay the shipping cost in the configured currency (coins by default).

**Transit & Expiration:**
- Shipments take time to arrive based on distance. The default rate is 2 seconds per meter (configurable). Transit time uses in-game time, not real-world time.
- Shipments expire after 1 hour of in-game time if not collected. Expiration is configurable and can be disabled entirely.

**Teleportation:** Ports also support teleporting players between discovered ports. Teleport costs scale with distance. This feature is disabled by default and must be enabled in the config.

**Configuration:** All port settings (transit time, currency, expiration, teleport costs) are server-synced and configurable. The ports feature can be disabled entirely in the config if you just want the base locations. Port data is stored in a folder titled `MWL_Ports` in the `BepInEx/Configs` folder.

## Shrines
Shrines are interactive objects that can appear inside MWL locations. They use the vanilla Ward prefab as their model (temporarily until a custom model is available).

**Spawning:** Shrines only appear in specific eligible locations. When a location supports shrines, each instance of that location has a 5% chance to contain one. This means shrines are rare finds and no two worlds will have them in the same places.

**Activation & Buffs:**
- Interact with a Shrine to activate it and receive a temporary buff that applies to all players within a 5m radius.
- Buff duration is randomized: 5 min, 10 min, 20 min, or 30 min.
- Each shrine's buff is randomized with 1 to 4 effects (more effects being rarer).
- Possible buff stats: increased health regen, increased stamina regen, increased eitr regen, and increased skill gain. Each stat value is randomized at +5%, +10%, or +20%.

**Cost & Cooldown:**
- Shrines offer one free activation initially. After that, they require a sacrifice of monster trophies from the biome the shrine is in.
- Shrines can only be used once per in-game day.

**Raid Risk:** There is a 2% chance that activating a shrine triggers a raid event. Only raids appropriate to the current biome can be triggered.

## Waystones
Waystones are interactive objects that can appear inside MWL locations, similar to Shrines. They also use the vanilla Ward prefab temporarily.

**Spawning:** Like Shrines, Waystones can only spawn in certain eligible locations. Each qualifying location instance has a 5% chance to contain a Waystone.

**Effects:** When activated, a Waystone will do one of two things:
- **Pin a dungeon:** Adds a map pin for a dungeon in the current biome. Swamp Waystones pin SunkenCrypt, Mountain Waystones pin Frostcave, and Mistlands Waystones pin InfestedMine.
- **Reveal map area:** Uncovers a radius of the map around the Waystone. The revealed radius is randomized: 200m, 400m, or 800m.

Waystones are single-use, making each discovery a meaningful reward for exploration.

## Traders
MWL adds 7 unique trader NPCs spread across multiple biomes. Each trader is found at a dedicated location and sells a curated selection of items. Trader locations are unique — only one of each can spawn per world — making finding them a rewarding part of exploration.

### Trader Locations

| Trader | Location | Biome |
|--------|----------|-------|
| Torvin | MWL_PlainsTavern1 | Plains |
| Ragnir | MWL_PlainsCamp1 | Plains |
| Volund | MWL_BlackForestBlacksmith1 | Black Forest |
| Sindri | MWL_BlackForestBlacksmith2 | Black Forest |
| Thorgrim | MWL_MountainsBlacksmith1 | Mountains |
| Dvalin | MWL_MistlandsBlacksmith1 | Mistlands |
| Bjorn | MWL_OceanTavern1 | Ocean |

Traders function like Valheim's vanilla Haldor — interact with them to open a buy/sell interface. Their inventories are configured via YAML and support progressive unlocking based on boss defeats, so new items become available as you advance through the game.

### Trainers & Skill Books
MWL adds 4 trainer NPCs across different biomes. Trainers sell Skill Books — consumable items that raise a specific skill level when used. They are generated for every skill type in the game and come in 3 tiers.

| Trainer Location | Biome |
|-----------------|-------|
| MWL_MeadowsTrainer1 | Meadows |
| MWL_SwampTrainer1 | Swamp |
| MWL_PlainsTrainer1 | Plains |
| MWL_MistTrainer1 | Mistlands |

Skill book tiers are gated by boss progression:

| Tier | Skill Bonus | Unlocked After | Price |
|------|------------|----------------|-------|
| 1    | +1 level   | Immediately    | 100   |
| 2    | +3 levels  | Bonemass       | 300   |
| 3    | +5 levels  | Yagluth        | 500   |

Tier gating ensures trainers stay relevant to your current progression. Lower tier books disappear from the shop once you've progressed past them, keeping the inventory clean.

### Blacksmith Stones
Blacksmith Stones are a new consumable item sold by blacksmith traders. They allow you to upgrade weapons and armor **past their normal maximum quality level**.

**How to use:**
1. Place the weapon or armor you want to upgrade in the **top-left cell** (position 0,0) of your inventory.
2. Consume the Blacksmith Stone from your inventory.
3. If the item is the correct quality for the stone's tier, it will be upgraded by +1 quality. If not, the stone is returned to your inventory.

**Tiers:**

| Stone | Effect | Required Item Quality |
|-------|--------|----------------------|
| Blacksmith Stone (1) | Upgrades to quality 5 | Quality 4 (weapons/armor) or 3 (shields) |
| Blacksmith Stone (2) | Upgrades to quality 6 | Quality 5 (weapons/armor) or 4 (shields) |
| Blacksmith Stone (3) | Upgrades to quality 7 | Quality 6 (weapons/armor) or 5 (shields) |

Blacksmith Stones work on: one-handed weapons, two-handed weapons, bows, shields, helmets, chest armor, leg armor, shoulder capes, torches, and tools.

### Minimap Icons
Trader locations display custom icons on the minimap once discovered, making them easy to find again:

| Icon | Locations |
|------|-----------|
| Anvil | Blacksmith locations (Black Forest, Mountains, Mistlands) |
| Tankard | Tavern locations (Plains, Ocean) |
| Coin | Camp locations (Plains) |

## Custom Lore
2 locations have custom runestones and small environmental storytelling narratives: MWL_MarbleJail1 and MWL_MarbleCliffAltar1. Both of these locations also have custom prefabs associated with them in order to facilitate their storytelling and unique experiences.

## Valheim SoftReferenceableAssets
This mod utilizes the [Valheim SoftReferenceableAssets](https://www.valheimgame.com/support/modding-faq-for-the-asset-bundle-update-0-217-40/) system to dynamically load location prefabs. In previous versions of MWL mods, the location prefabs were always held in the client computer's memory, resulting in very high RAM usage. This caused slow startup times, slow world start times, and major stutters and even crashes during garbage collection events. The SoftReferenceableAssets system has been implemented via JotunnLib to massively improve performance — prefabs are now loaded on demand and released when no longer needed, significantly reducing memory footprint.

## World Rings System
The Valheim world is a circle with a default radius of 10,500 meters. In order to encourage and maintain exploration, I've defined a set of world rings that all locations in the More World Locations series will use when spawning in a world. Rings overlap intentionally to create natural variation in how far from the center each location type can appear. Below are the defined world rings...

| Ring # | Start Distance (m) | End Distance (m) |
|--------|---------------------|------------------|
| Ring 1 | 0                   | 500              |
| Ring 2 | 500                 | 2000             |
| Ring 3 | 1500                | 3000             |
| Ring 4 | 2500                | 4000             |
| Ring 5 | 3500                | 6000             |
| Ring 6 | 4500                | 8500             |
| Ring 7 | 5000                | 10500            |

## Feature Toggle Configs
As of version 4.1.0, you can disable individual features via the config file. Under the `0 - Features` section, toggle these on or off:
- **Enable Shrines** — Disable shrine spawning in locations
- **Enable Waystones** — Disable waystone spawning in locations
- **Enable Traders** — Disable all trader NPC locations
- **Enable Trainers** — Disable all trainer NPC locations
- **Enable Shipping Ports** — Disable shipping port functionality

This is useful if you want a simpler, purely POI-based experience without the interactive features.

## Instructions - Manually Installing Mod
- This mod has unique requirements if you are not using a mod manager (such as r2modman) or are manually placing files on your dedicated server so please read carefully. If you're using a mod manager you can safely ignore these unique instructions.
- When you unzip the file, there will be a "plugins" folder. Inside the "plugins" folder there is a "Bundles" folder. You must take the "Bundles" folder out of the "plugins" folder and put it in my mods folder. See the visual guide below.
- This mod requires additional files beyond the standard mod DLL file you are used to. It requires an asset bundle manifest file and also asset bundle files. The asset bundles must be inside a folder titled "Bundles".
- In my code I have written very specific file paths to locate these files on your storage disk. The file path I expect these files to be at is: `BepInEx.Paths.PluginPath\warpalicious-More_World_Locations_AIO`
- BepInEx.Paths.PluginPath is the "Plugins" folder inside your BepInEx installation.
- An example of this path on my computer is: `C:\Users\{username}\AppData\Roaming\r2modmanPlus-local\Valheim\profiles\ValheimModTesting\BepInEx\plugins\warpalicious-More_World_Locations_AIO`
- Common Mistake 1: Placing my mod files directly in your plugins without the `warpalicious-More_World_Locations_AIO` folder. You must put my mod files inside my folder and then put my folder in your plugins folder.
- Common Mistake 2: Downloading my mod manually and placing a folder with the version number in the folder name. When you download my mod from Thunderstore the folder name will include a version number like this `warpalicious-More_World_Locations_AIO-1.1.0`. This is wrong and won't work. Please remove the version number so that it looks like this: `warpalicious-More_World_Locations_AIO`

<details>
  <summary>Manual Installation Guide, click to view</summary>

  ![Manual Installation Guide](https://i.imgur.com/URjqsPQ.png)

</details>

## Instructions - Adding Locations to World
- To add these locations to a non-existing world, no action is required. Ensure the mod is installed and create a new world.
- To add these locations to an existing world, install the mod [Upgrade World](https://valheim.thunderstore.io/package/JereKuusela/Upgrade_World/). Then load into your existing world and use one of the commands below. Note, you must have access to the console either via enabling it via Steam or using a mod.

- Add all locations: `mwl_allbiomes`
- Add Meadows locations: `mwl_meadows`
- Add Blackforest locations: `mwl_blackforest`
- Add Swamp locations: `mwl_swamp`
- Add Mountain locations: `mwl_mountains`
- Add Plains locations: `mwl_plains`
- Add Mistlands locations: `mwl_mistlands`
- Add Ashlands locations: `mwl_ashlands`
- Add Ports locations: `mwl_ports`
- Add Trader locations: `mwl_traders`

## More World Locations
The goal of the More World Locations series is to solve Valheim's exploration problem. Valheim has a giant map but relatively few points of interest (POI) to find. Once a player learns that each biome is just a copy of what they've already seen, exploring the rest of the map feels unnecessary. The More World Locations series will fix this problem by adding ~~dozens~~ hundreds of handcrafted, unique, and interesting POIs to the Valheim world. I started developing the series in February 2024 and previously released mods as biome packs. In July 2025 I made a major performance improvement to the asset loading system and transitioned to this all-in-one (AIO). I don't plan to continue support for the individual biome packs.

## All-in-One Pack vs. Individual Packs
- This AIO pack includes many locations not present in any other pack.
- This AIO pack includes all biome packs including: Meadows Pack 1, Meadows Pack 2, BlackForest Pack 1, BlackForest Pack 2, Swamp Pack 1, Mountains Pack 1, Plains Pack 1, Mistlands Pack 1, Ashlands Pack 1, Adventure Map Pack 1.
- More World Traders has been fully integrated into the AIO mod as of 4.0.0.
- This AIO DOES NOT currently include these other MWL mods: Underground Ruins, Forbidden Catacombs.

## FAQ
Q: I'm seeing a lot of "Failed to place all X, placed Y out of Z" warning messages during world generation. Is something wrong?
A: No, this is completely normal! When Valheim generates a world, it tries to place each location type a set number of times. For each attempt, it checks many conditions: Is this the right biome? Is the ground flat enough? Is it far enough from similar locations? Is the altitude correct? Sometimes there simply isn't enough suitable conditions to hit the target spawn quantity.

## Known Incompatibilities
- Currently incompatible with the mod SeedTotem by MathiasDecrock. If SeedTotem is loaded alongside this mod, you will experience significant FPS drops.
- Previously incompatible with the mod RuneMagic by HyenaLegend. We fixed the incompatibility in version 2.0.7 of MWL_AIO.

## Mod Support & Feedback
Please feel free to share any and all feedback or ask questions. You can find me on my own modding Discord.
- [Warp Mods Discord](https://discord.gg/KjgZ63VZv5)

## Donations/Tips
I make mods because I enjoy it and want to make Valheim more enjoyable for everyone. If you feel like saying thanks you can tip me here.

| My Ko-fi: | [![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/warpalicious) |
|-----------|---------------|

## Source Code
Source code is available on Github.

| Github Repository: | <img height="18" src="https://github.githubassets.com/favicons/favicon-dark.svg"></img><a href="https://github.com/jneb802/MoreWorldLocations_All"> MoreWorldLocations</a> |
|-----------|---------------|

## Credit & Thanks
I greatly appreciate all the other mod developers that helped me while building the More World Locations series! If you're someone that's interested in making mods, please reach out to me on Discord and I will help you!

All the locations used in this mod were created by Valheim community members. If you're a Valheim builder and have some cool locations to share for a future More World Locations mod, please reach out on Discord!

A special thank you to the mod developer Rusty. He built the majority of the shipping port feature and taught me a lot in doing so. Thank you Rusty!

<details>
<summary><strong>Valheim Builder Credits</strong></summary>

| Location               | Creator     |
|------------------------|-------------|
| MWL_Ruins1 | H1lli               |
| MWL_Ruins2 | H1lli               |
| MWL_Ruins3 | H1lli               |
| MWL_Ruins6 | Jiroc                |
| MWL_Ruins7 | Jiroc                |
| MWL_Ruins8 | Jiroc                |
| MWL_RuinsArena1 | H1lli           |
| MWL_RuinsArena3 | Raaka           |
| MWL_RuinsChurch1 | H1lli           |
| MWL_RuinsWell1 | Jiroc           |
| MWL_DeerShrine1         | Ashenius             |
| MWL_DeerShrine2         | Ashenius             |
| MWL_MeadowsBarn1        | Aldhari              |
| MWL_MeadowsHouse2       | Aldhari              |
| MWL_MeadowsRuin1        | Aldhari              |
| MWL_MeadowsTomb4        | Aldhari              |
| MWL_MeadowsTower1       | Aldhari              |
| MWL_OakHut1             | Aldhari              |
| MWL_SmallHouse1         | Ashenius             |
| MWL_RuinsArena2 | H1lli               |
| MWL_RuinsCastle1 | H1lli               |
| MWL_RuinsCastle3 | H1lli               |
| MWL_RuinsTower3 | H1lli                |
| MWL_RuinsTower8 | H1lli                |
| MWL_Tavern1 | H1lli                |
| MWL_WoodTower1 | H1lli           |
| MWL_WoodTower2 | H1lli           |
| MWL_WoodTower3 | H1lli           |
| MWL_ForestForge1       | Aldhari     |
| MWL_ForestForge2       | Aldhari     |
| MWL_ForestGreatHouse2  | Aldhari     |
| MWL_ForestHouse2       | Aldhari     |
| MWL_ForestRuin1        | Aldhari     |
| MWL_ForestTower2       | Aldhari     |
| MWL_ForestTower3       | Aldhari     |
| MWL_MassGrave1         | Aldhari     |
| MWL_StoneFormation1    | Aldhari     |
| MWL_GuardTower1        | MaxFoxGaming|
| MWL_RootRuins1         | Hilli       |
| MWL_RootsTower1        | Hilli       |
| MWL_RootsTower2        | Hilli       |
| MWL_RuinedRootTower5   | Hilli       |
| MWL_ForestRuin2        | Ashenius    |
| MWL_ForestRuin3        | Ashenius    |
| MWL_ForestSkull1       | Ashenius    |
| MWL_ForestTower4       | Ashenius    |
| MWL_ForestTower5       | Ashenius    |
| MWL_GuckPit1 | SmittySurvival               |
| MWL_SwampAltar1 | SmittySurvival               |
| MWL_SwampAltar2 | SmittySurvival               |
| MWL_SwampAltar3 | SmittySurvival                |
| MWL_SwampAltar4 | SmittySurvival                |
| MWL_SwampCastle2 | Aldhari                |
| MWL_SwampGrave1 | Aldhari           |
| MWL_SwampHouse1 | Aldhari           |
| MWL_SwampRuin1 | SmittySurvival           |
| MWL_SwampTower1 | SmittySurvival           |
| MWL_SwampTower2 | Shigzula           |
| MWL_SwampTower3 | Aldhari           |
| MWL_SwampWell1 | SmittySurvival           |
| MWL_AbandonedHouse1 | MaxFoxGaming           |
| MWL_Shipyard1 | Insanity           |
| MWL_Treehouse1 | MaxFoxGaming           |
| MWL_FortBakkarhalt1 | MaxFoxGaming           |
| MWL_Belmont1 | FusterCluck           |
| MWL_StoneCastle1 | H1lli               |
| MWL_StoneFort1 | H1lli               |
| MWL_StoneHall1 | H1lli               |
| MWL_StoneTavern1 | H1lli                |
| MWL_StoneTower1 | H1lli                |
| MWL_StoneTower2 | H1lli                |
| MWL_WoodBarn1 | H1lli           |
| MWL_WoodFarm1 | H1lli           |
| MWL_WoodHouse1 | H1lli           |
| MWL_FulingRock1 | BatgirlXXRobin               |
| MWL_FulingTemple1 | Hilli               |
| MWL_FulingTemple2 | Hilli               |
| MWL_FulingTemple3 | Hilli                |
| MWL_FulingTower1 | Hilli                |
| MWL_FulingVillage1 | Mixeur666                |
| MWL_FulingVillage2 | Hilli           |
| MWL_FulingWall1 | Hilli                |
| MWL_GoblinFort1 | PUP           |
| MWL_PlainsPillar1 | Warpalicious           |
| MWL_GoblinCave1 | Warpalicious           |
| MWL_MistFort2 | HiccupTheHermit               |
| MWL_MistHut1 | edenekho               |
| MWL_MistTower1 | HiccupTheHermit               |
| MWL_MistTower2 | edenekho                |
| MWL_MistWall1 | HiccupTheHermit                |
| MWL_MistWorkshop1 | HiccupTheHermit                |
| MWL_SecretRoom1 | Warpalicious           |
| MWL_DvergrEitrSingularity1 | MaxFoxGaming           |
| MWL_DvergrHouse1 | MaxFoxGaming           |
| MWL_DvergrKnowledgeExtractor1 | MaxFoxGaming           |
| MWL_AshlandsFort1 | Insanity               |
| MWL_AshlandsFort2 | Insanity               |
| MWL_AshlandsFort3 | Insanity               |
| MWL_CastleCorner1       | Ninebyte & Dhakhar |
| MWL_ForestCamp1         | Ninebyte & Dhakhar |
| MWL_Misthut2            | Ninebyte & Dhakhar |
| MWL_MountainDvergrShrine1 | Ninebyte & Dhakhar |
| MWL_MountainShrine1     | Ninebyte & Dhakhar |
| MWL_RuinedTower1        | Ninebyte & Dhakhar |
| MWL_TreeTowers1         | Ninebyte & Dhakhar |
| MWL_Port1 | Warpalicious |
| MWL_Port2 | ArbbyM9er |
| MWL_Port3 | Hilli |
| MWL_Port4 | iNavite |
| MWL_DvergrHouseWood1 | Hilli |
| MWL_DvergrHouseWood2 | Hilli |
| MWL_MarbleJail1 | Bryn |
| MWL_FulingTempleBroken1 | Hilli |
| MWL_FulingTemple4 | Hilli |
| MWL_StoneCircle1 | Hilli |
| MWL_ForestGrove1 | Hilli |
| MWL_MountainDvergrShrine2 | Bryn |
| MWL_MarbleCliffAltar1 | Bryn |
| MWL_MistPond1 | Warpalicious |
| MWL_MaypoleHut1 | Bryn |
| MWL_MarbleHome1 | Bryn |
| MWL_MarbleCage1 | Hilli |
| MWL_SwampTemple1 | Hilli |
| MWL_MountainOverlook1 | Hilli |
| MWL_RockShrine1 | Hilli |
| MWL_MountainCultShrine1 | Hilli |
| MWL_RuinsChurch2 | Hilli |
| MWL_PlainsTavern1 | Olivanderr |
| MWL_PlainsCamp1 | MutantArtCat |
| MWL_BlackForestBlacksmith1 | Berserk The Builder |
| MWL_BlackForestBlacksmith2 | SmittySurvival |
| MWL_MountainsBlacksmith1 | JJ the builder |
| MWL_MistlandsBlacksmith1 | SmittySurvival |
| MWL_OceanTavern1 | guru_Lakhima |
| MWL_MeadowsTrainer1 | SmittySurvival |
| MWL_SwampTrainer1 | SmittySurvival |
| MWL_PlainsTrainer1 | SmittySurvival |
| MWL_MistTrainer1 | SmittySurvival |


</details>
