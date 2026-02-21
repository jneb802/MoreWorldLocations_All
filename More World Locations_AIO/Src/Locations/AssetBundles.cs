using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Common;
using HarmonyLib;
using Jotunn.Utils;
using SoftReferenceableAssets;
using UnityEngine;
using BepInEx;

namespace More_World_Locations_AIO;

public class AssetBundles
{
    public static string manifestFull = "assetBundleManifest_Full";
    public static string manifestLower = "assetBundleManifest_full";

    public static string manifestPath = Path.Combine(BepInEx.Paths.PluginPath, "warpalicious-More_World_Locations_AIO", manifestFull);

    [HarmonyPatch(typeof(EntryPointSceneLoader), "Start")]
    public static class EntryPatch
    {
        public static void Prefix()
        {
            string manifestPathFull = GetManifest();
            
            if (manifestPathFull != null) Runtime.AddManifest(manifestPathFull);
        }
    }
    
    public static string GetManifest()
    {
        //get the full location of the assembly
        string fullPath = Assembly.GetExecutingAssembly().Location;
        string directoryPath = Path.GetDirectoryName(fullPath);
        
        //get the folder the assembly is in, and list the files in that folder
        string[] presentFiles = Directory.GetFiles(directoryPath);
        
        foreach (string file in presentFiles) {
            if (file.EndsWith(manifestFull, StringComparison.OrdinalIgnoreCase)) {
                Debug.Log($"Found manifest file: {file}");
                manifestPath = file;
            }
            else if (file.EndsWith(manifestLower, StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"Found manifest file: {file}");
                manifestPath = file;
            }
        }

        return manifestPath;
    }
    
    public static void BuildCombinedManifest(string bundleFolder, string suffix, string[] assetPathsInBundleFull)
    {
        Debug.Log($"Building combined manifest from folder: {bundleFolder}");

        string softRefManifestPath = Path.Combine(BepInEx.Paths.PluginPath, "assetBundleManifest_" + suffix);

        string bundleRelativeDir = "./Bundles";
        var manifest = new SoftReferenceableAssets.AssetBundleManifest(bundleRelativeDir);

        string[] bundleFiles = Directory.GetFiles(bundleFolder, "*", SearchOption.TopDirectoryOnly)
            .Where(f => !f.EndsWith(".manifest", StringComparison.OrdinalIgnoreCase)) // skip manifest files
            .ToArray();

        foreach (string bundleFile in bundleFiles)
        {
            string bundleName = Path.GetFileName(bundleFile);
            string unityManifestPath = bundleFile + ".manifest"; // Unity’s text manifest lives here

            Debug.Log($"Processing bundle: {bundleName}");

            foreach (string assetPath in assetPathsInBundleFull)
            {
                if (!assetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                    continue;

                string prefabName = Path.GetFileNameWithoutExtension(assetPath).ToLower();
                if (bundleName.StartsWith(prefabName, StringComparison.OrdinalIgnoreCase))
                {
                    AssetID assetId = Jotunn.Managers.AssetManager.Instance.GenerateAssetID(assetPath);
                    var location = new AssetLocation(bundleName, assetPath);
                    manifest.AddAssetLocation(assetId, location);

                    Debug.Log($"  Added {assetPath} to bundle {bundleName}");
                }
            }

            if (File.Exists(unityManifestPath))
            {
                string[] dependencies = ExtractDependenciesFromTextManifest(unityManifestPath);
                manifest.AddBundleDependencies(bundleName, dependencies);
            }
        }

        manifest.SerializeToDisk(softRefManifestPath, SerializationFormat.Text);

        Debug.Log($"Combined manifest written to {softRefManifestPath}");
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
}