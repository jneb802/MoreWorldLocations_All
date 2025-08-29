using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Common;
using HarmonyLib;
using Jotunn.Utils;
using SoftReferenceableAssets;
using UnityEngine;
using BepInEx;

namespace More_World_Locations_AIO;

public class AssetBundles
{
    public static string bundle1 = "moreworldlocations_assetbundle_1";
    public static string bundle2 = "moreworldlocations_assetbundle_2";
    public static string bundle3 = "moreworldlocations_assetbundle_3";
    public static string bundle4 = "moreworldlocations_assetbundle_4";
    
    [HarmonyPatch(typeof(EntryPointSceneLoader), "Start")]
    public static class EntryPatch
    {
        public static void Prefix()
        {
            string manifestPath1 = GetManifest(Path.Combine(BepInEx.Paths.PluginPath, "warpalicious-More_World_Locations_AIO", "assetBundleManifest_1"));
            string manifestPath2 = GetManifest(Path.Combine(BepInEx.Paths.PluginPath, "warpalicious-More_World_Locations_AIO", "assetBundleManifest_2"));
            string manifestPath3 = GetManifest(Path.Combine(BepInEx.Paths.PluginPath, "warpalicious-More_World_Locations_AIO", "assetBundleManifest_3"));
            string manifestPath4 = GetManifest(Path.Combine(BepInEx.Paths.PluginPath, "warpalicious-More_World_Locations_AIO", "assetBundleManifest_4"));


            if (manifestPath1 != null) Runtime.AddManifest(manifestPath1);
            if (manifestPath2 != null) Runtime.AddManifest(manifestPath2);
            if (manifestPath3 != null) Runtime.AddManifest(manifestPath3);
            if (manifestPath4 != null) Runtime.AddManifest(manifestPath4);
        }
    }
    
    public static string GetManifest(string path)
    {
        if (File.Exists(path))
        {
            return path;
        }
        else
        {
            Debug.LogError($"[More_World_Locations_AIO] Manifest missing at: {path}");
            return null;
        }
    }


    public static void BuildManifest(string bundleName, string manifestPath, string[] assetPathsInBundle, string suffix)
    {
        Debug.Log($"Calling build manifest: {bundleName}");
        string softRefmanifestPath = Path.Combine(BepInEx.Paths.PluginPath, "assetBundleManifest_" + suffix);
        string bundleRelativeDir = ".";
            
        SoftReferenceableAssets.AssetBundleManifest manifest = new SoftReferenceableAssets.AssetBundleManifest(bundleRelativeDir);

        foreach (string assetPath in assetPathsInBundle)
        {
            AssetID assetId = Jotunn.Managers.AssetManager.Instance.GenerateAssetID(assetPath);
            var location = new AssetLocation(bundleName, assetPath);
            manifest.AddAssetLocation(assetId, location);
        }
        
        string[] dependencies = ExtractDependenciesFromTextManifest(manifestPath);
        manifest.AddBundleDependencies(bundleName, dependencies);
            
        manifest.SerializeToDisk(softRefmanifestPath, SerializationFormat.Text);
    }
    
    private static string[] ExtractDependenciesFromTextManifest(string manifestPath)
    {
        if (!File.Exists(manifestPath)) 
        {
            Debug.LogWarning($"Manifest file not found: {manifestPath}");
            return new string[0];
        }
    
        try
        {
            string[] lines = File.ReadAllLines(manifestPath);
            bool inDependencies = false;
            List<string> dependencies = new List<string>();
        
            foreach (string line in lines)
            {
                if (line.StartsWith("Dependencies:"))
                {
                    inDependencies = true;
                    continue;
                }
                if (inDependencies && line.StartsWith("- "))
                {
                    // Extract just the filename from the full path
                    string fullPath = line.Substring(2).Trim();
                    string dep = Path.GetFileName(fullPath);
                    dependencies.Add(dep);
                }
                else if (inDependencies && !line.StartsWith("- ") && !string.IsNullOrWhiteSpace(line))
                {
                    break; // End of dependencies section
                }
            }
        
            Debug.Log($"Found {dependencies.Count} dependencies: {string.Join(", ", dependencies)}");
            return dependencies.ToArray();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse manifest file {manifestPath}: {e.Message}");
            return new string[0];
        }
    }
    
    public static string[] assetPathsInBundle1 = new[]
    {
        // Meadows Pack 1
        "Assets/WarpProjects/More World Locations/MeadowsPack1/MWL_Ruins1.prefab",
        "Assets/WarpProjects/More World Locations/MeadowsPack1/MWL_Ruins2.prefab",
        "Assets/WarpProjects/More World Locations/MeadowsPack1/MWL_Ruins3.prefab",
        "Assets/WarpProjects/More World Locations/MeadowsPack1/MWL_Ruins6.prefab",
        "Assets/WarpProjects/More World Locations/MeadowsPack1/MWL_Ruins7.prefab",
        "Assets/WarpProjects/More World Locations/MeadowsPack1/MWL_Ruins8.prefab",
        "Assets/WarpProjects/More World Locations/MeadowsPack1/MWL_RuinsArena1.prefab",
        "Assets/WarpProjects/More World Locations/MeadowsPack1/MWL_RuinsArena3.prefab",
        "Assets/WarpProjects/More World Locations/MeadowsPack1/MWL_RuinsChurch1.prefab",
        "Assets/WarpProjects/More World Locations/MeadowsPack1/MWL_RuinsWell1.prefab",
        
        // Black Forest Pack 1
        "Assets/WarpProjects/More World Locations/Blackforest Pack 1/MWL_RuinsArena2.prefab",
        "Assets/WarpProjects/More World Locations/Blackforest Pack 1/MWL_RuinsCastle1.prefab",
        "Assets/WarpProjects/More World Locations/Blackforest Pack 1/MWL_RuinsCastle3.prefab",
        "Assets/WarpProjects/More World Locations/Blackforest Pack 1/MWL_RuinsTower3.prefab",
        "Assets/WarpProjects/More World Locations/Blackforest Pack 1/MWL_RuinsTower6.prefab",
        "Assets/WarpProjects/More World Locations/Blackforest Pack 1/MWL_RuinsTower8.prefab",
        "Assets/WarpProjects/More World Locations/Blackforest Pack 1/MWL_Tavern1.prefab",
        "Assets/WarpProjects/More World Locations/Blackforest Pack 1/MWL_WoodTower1.prefab",
        "Assets/WarpProjects/More World Locations/Blackforest Pack 1/MWL_WoodTower2.prefab",
        "Assets/WarpProjects/More World Locations/Blackforest Pack 1/MWL_WoodTower3.prefab",
        
    };
    
    public static string[] assetPathsInBundle2 = new[]
    {

        // Meadows Pack 2
        "Assets/WarpProjects/More World Locations/Meadows Pack 2/MWL_DeerShrine1.prefab",
        "Assets/WarpProjects/More World Locations/Meadows Pack 2/MWL_DeerShrine2.prefab",
        "Assets/WarpProjects/More World Locations/Meadows Pack 2/MWL_MeadowsBarn1.prefab",
        "Assets/WarpProjects/More World Locations/Meadows Pack 2/MWL_MeadowsHouse2.prefab",
        "Assets/WarpProjects/More World Locations/Meadows Pack 2/MWL_MeadowsRuin1.prefab",
        "Assets/WarpProjects/More World Locations/Meadows Pack 2/MWL_MeadowsTomb4.prefab",
        "Assets/WarpProjects/More World Locations/Meadows Pack 2/MWL_MeadowsTower1.prefab",
        "Assets/WarpProjects/More World Locations/Meadows Pack 2/MWL_OakHut1.prefab",
        "Assets/WarpProjects/More World Locations/Meadows Pack 2/MWL_SmallHouse1.prefab",
        "Assets/WarpProjects/More World Locations/Meadows Pack 2/MWL_MeadowsFarm1.prefab",
        "Assets/WarpProjects/More World Locations/Meadows Pack 2/MWL_MeadowsLighthouse1.prefab",
        "Assets/WarpProjects/More World Locations/Meadows Pack 2/MWL_MeadowsSawmill1.prefab",
        "Assets/WarpProjects/More World Locations/Meadows Pack 2/MWL_MeadowsWall1.prefab",
        "Assets/WarpProjects/More World Locations/Meadows Pack 2/MWL_MeadowsTavern1.prefab",

        // Black Forest Pack 2
        "Assets/WarpProjects/More World Locations/BlackForest Pack 2/MWL_ForestForge1.prefab",
        "Assets/WarpProjects/More World Locations/BlackForest Pack 2/MWL_ForestForge2.prefab",
        "Assets/WarpProjects/More World Locations/BlackForest Pack 2/MWL_ForestGreatHouse2.prefab",
        "Assets/WarpProjects/More World Locations/BlackForest Pack 2/MWL_ForestHouse2.prefab",
        "Assets/WarpProjects/More World Locations/BlackForest Pack 2/MWL_ForestRuin1.prefab",
        "Assets/WarpProjects/More World Locations/BlackForest Pack 2/MWL_ForestTower2.prefab",
        "Assets/WarpProjects/More World Locations/BlackForest Pack 2/MWL_ForestTower3.prefab",
        "Assets/WarpProjects/More World Locations/BlackForest Pack 2/MWL_MassGrave1.prefab",
        "Assets/WarpProjects/More World Locations/BlackForest Pack 2/MWL_StoneFormation1.prefab",
        "Assets/WarpProjects/More World Locations/BlackForest Pack 2/MWL_GuardTower1.prefab",
        "Assets/WarpProjects/More World Locations/BlackForest Pack 2/MWL_RootRuins1.prefab",
        "Assets/WarpProjects/More World Locations/BlackForest Pack 2/MWL_RootsTower1.prefab",
        "Assets/WarpProjects/More World Locations/BlackForest Pack 2/MWL_RootsTower2.prefab",
        "Assets/WarpProjects/More World Locations/BlackForest Pack 2/MWL_ForestRuin2.prefab",
        "Assets/WarpProjects/More World Locations/BlackForest Pack 2/MWL_ForestRuin3.prefab",
        "Assets/WarpProjects/More World Locations/BlackForest Pack 2/MWL_ForestSkull1.prefab",
        "Assets/WarpProjects/More World Locations/BlackForest Pack 2/MWL_ForestTower4.prefab",
        "Assets/WarpProjects/More World Locations/BlackForest Pack 2/MWL_ForestTower5.prefab",
        "Assets/WarpProjects/More World Locations/BlackForest Pack 2/MWL_ForestPillar1.prefab",
        "Assets/WarpProjects/More World Locations/BlackForest Pack 2/MWL_CoastTower1.prefab",

        // Swamp Pack 1
        "Assets/WarpProjects/More World Locations/Swamp Pack 1/MWL_GuckPit1.prefab",
        "Assets/WarpProjects/More World Locations/Swamp Pack 1/MWL_SwampAltar1.prefab",
        "Assets/WarpProjects/More World Locations/Swamp Pack 1/MWL_SwampAltar2.prefab",
        "Assets/WarpProjects/More World Locations/Swamp Pack 1/MWL_SwampAltar3.prefab",
        "Assets/WarpProjects/More World Locations/Swamp Pack 1/MWL_SwampAltar4.prefab",
        "Assets/WarpProjects/More World Locations/Swamp Pack 1/MWL_SwampCastle2.prefab",
        "Assets/WarpProjects/More World Locations/Swamp Pack 1/MWL_SwampGrave1.prefab",
        "Assets/WarpProjects/More World Locations/Swamp Pack 1/MWL_SwampHouse1.prefab",
        "Assets/WarpProjects/More World Locations/Swamp Pack 1/MWL_SwampRuin1.prefab",
        "Assets/WarpProjects/More World Locations/Swamp Pack 1/MWL_SwampTower1.prefab",
        "Assets/WarpProjects/More World Locations/Swamp Pack 1/MWL_SwampTower2.prefab",
        "Assets/WarpProjects/More World Locations/Swamp Pack 1/MWL_SwampTower3.prefab",
        "Assets/WarpProjects/More World Locations/Swamp Pack 1/MWL_SwampWell1.prefab",
        "Assets/WarpProjects/More World Locations/Swamp Pack 1/MWL_AbandonedHouse1.prefab",
        "Assets/WarpProjects/More World Locations/Swamp Pack 1/MWL_Treehouse1.prefab",
        "Assets/WarpProjects/More World Locations/Swamp Pack 1/MWL_Shipyard1.prefab",
        "Assets/WarpProjects/More World Locations/Swamp Pack 1/MWL_FortBakkarhalt1.prefab",
        "Assets/WarpProjects/More World Locations/Swamp Pack 1/MWL_Belmont1.prefab",
        "Assets/WarpProjects/More World Locations/Swamp Pack 1/MWL_SwampCourtyard1.prefab",
        "Assets/WarpProjects/More World Locations/Swamp Pack 1/MWL_SwampBrokenTower1.prefab",
        "Assets/WarpProjects/More World Locations/Swamp Pack 1/MWL_SwampBrokenTower3.prefab",
        

        // Mountains Pack 1
        "Assets/WarpProjects/More World Locations/Mountain Pack 1/MWL_StoneCastle1.prefab",
        "Assets/WarpProjects/More World Locations/Mountain Pack 1/MWL_StoneFort1.prefab",
        "Assets/WarpProjects/More World Locations/Mountain Pack 1/MWL_StoneHall1.prefab",
        "Assets/WarpProjects/More World Locations/Mountain Pack 1/MWL_StoneTavern1.prefab",
        "Assets/WarpProjects/More World Locations/Mountain Pack 1/MWL_StoneTower1.prefab",
        "Assets/WarpProjects/More World Locations/Mountain Pack 1/MWL_StoneTower2.prefab",
        "Assets/WarpProjects/More World Locations/Mountain Pack 1/MWL_WoodBarn1.prefab",
        "Assets/WarpProjects/More World Locations/Mountain Pack 1/MWL_WoodFarm1.prefab",
        "Assets/WarpProjects/More World Locations/Mountain Pack 1/MWL_WoodHouse1.prefab",
        "Assets/WarpProjects/More World Locations/Mountain Pack 1/MWL_TempleShrine1.prefab",
        "Assets/WarpProjects/More World Locations/Mountain Pack 1/MWL_StoneAltar1.prefab",

        // Plains Pack 1
        "Assets/WarpProjects/More World Locations/Plains Pack 1/MWL_GoblinFort1.prefab",
        "Assets/WarpProjects/More World Locations/Plains Pack 1/MWL_FulingRock1.prefab",
        "Assets/WarpProjects/More World Locations/Plains Pack 1/MWL_FulingVillage1.prefab",
        "Assets/WarpProjects/More World Locations/Plains Pack 1/MWL_FulingVillage2.prefab",
        "Assets/WarpProjects/More World Locations/Plains Pack 1/MWL_PlainsPillar1.prefab",
        "Assets/WarpProjects/More World Locations/Plains Pack 1/MWL_FulingTemple1.prefab",
        "Assets/WarpProjects/More World Locations/Plains Pack 1/MWL_FulingTemple2.prefab",
        "Assets/WarpProjects/More World Locations/Plains Pack 1/MWL_FulingTemple3.prefab",
        "Assets/WarpProjects/More World Locations/Plains Pack 1/MWL_FulingWall1.prefab",
        "Assets/WarpProjects/More World Locations/Plains Pack 1/MWL_FulingTower1.prefab",
        "Assets/WarpProjects/More World Locations/Plains Pack 1/MWL_RockGarden1.prefab",

        // Mistlands Pack 1
        "Assets/WarpProjects/More World Locations/Mistlands Pack 1/MWL_MistFort2.prefab",
        "Assets/WarpProjects/More World Locations/Mistlands Pack 1/MWL_SecretRoom1.prefab",
        "Assets/WarpProjects/More World Locations/Mistlands Pack 1/MWL_MistWorkshop1.prefab",
        "Assets/WarpProjects/More World Locations/Mistlands Pack 1/MWL_MistTower1.prefab",
        "Assets/WarpProjects/More World Locations/Mistlands Pack 1/MWL_MistWall1.prefab",
        "Assets/WarpProjects/More World Locations/Mistlands Pack 1/MWL_MistTower2.prefab",
        "Assets/WarpProjects/More World Locations/Mistlands Pack 1/MWL_MistHut1.prefab",
        "Assets/WarpProjects/More World Locations/Mistlands Pack 1/MWL_DvergrEitrSingularity1.prefab",
        "Assets/WarpProjects/More World Locations/Mistlands Pack 1/MWL_DvergrHouse1.prefab",
        "Assets/WarpProjects/More World Locations/Mistlands Pack 1/MWL_DvergrKnowledgeExtractor1.prefab",
        "Assets/WarpProjects/More World Locations/Mistlands Pack 1/MWL_AncientShrine1.prefab",
        "Assets/WarpProjects/More World Locations/Mistlands Pack 1/MWL_MistShrine1.prefab",

        // Adventure Map Pack 1
        "Assets/WarpProjects/More World Locations/Adventure Map 1/MWL_CastleCorner1.prefab",
        "Assets/WarpProjects/More World Locations/Adventure Map 1/MWL_ForestCamp1.prefab",
        "Assets/WarpProjects/More World Locations/Adventure Map 1/MWL_MistHut2.prefab",
        "Assets/WarpProjects/More World Locations/Adventure Map 1/MWL_MountainDvergrShrine1.prefab",
        "Assets/WarpProjects/More World Locations/Adventure Map 1/MWL_MountainShrine1.prefab",
        "Assets/WarpProjects/More World Locations/Adventure Map 1/MWL_RuinedTower1.prefab",
        "Assets/WarpProjects/More World Locations/Adventure Map 1/MWL_TreeTowers1.prefab"
    };
    
    public static string[] assetPathsInBundle3 = new[]
    {
        // Ashlands Pack 1
        "Assets/WarpProjects/AshlandsPack1/MWL_AshlandsFort1.prefab",
        "Assets/WarpProjects/AshlandsPack1/MWL_AshlandsFort2.prefab",
        "Assets/WarpProjects/AshlandsPack1/MWL_AshlandsFort3.prefab"
    };
    
    public static string[] assetPathsInBundle4 = new[]
    {
        // Ashlands Pack 1
        "Assets/WarpProjects/LocationTesting/Location1.prefab",
    };

}