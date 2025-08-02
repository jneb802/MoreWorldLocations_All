using System.Reflection;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace More_World_Locations_AIO.Shipments;

public class PortUI
{
    public static AssetBundle portBundle;
    private static GameObject shipmentsUI = null;
    private static bool shouldShowUI = false; // Track UI state across scene changes
    
    public static void LoadPrefabBundles()
    {
        portBundle = AssetUtils.LoadAssetBundleFromResources(
            "ports",
            Assembly.GetExecutingAssembly());

        // Subscribe to Jotunn's GUI event to handle scene changes
        GUIManager.OnCustomGUIAvailable += OnGUIAvailable;
    }

    public static void LoadPrefabs()
    {
        var prefabs = portBundle.LoadAllAssets<GameObject>();
    }

    private static void OnGUIAvailable()
    {
        // Recreate UI if it was shown before scene change
        if (shouldShowUI && shipmentsUI == null)
        {
            CreateShipmentsUI();
        }
    }

    public static void ToggleShipmentsUI(Terminal context)
    {
        // Toggle the state
        shouldShowUI = !shouldShowUI;

        if (shouldShowUI)
        {
            CreateShipmentsUI(context);
        }
        else
        {
            CloseShipmentsUI(context);
        }
    }

    private static void CreateShipmentsUI(Terminal context = null)
    {
        // Check if Jotunn GUI is available
        if (GUIManager.CustomGUIFront == null)
        {
            context?.AddString("Error: Jotunn CustomGUIFront not available");
            shouldShowUI = false;
            return;
        }

        // Ensure asset bundle is loaded
        if (portBundle == null)
        {
            context?.AddString("Error: shipments asset bundle not loaded");
            shouldShowUI = false;
            return;
        }

        // Load the UI prefab from the asset bundle
        string[] assetNames = portBundle.GetAllAssetNames();
        context?.AddString($"Found {assetNames.Length} assets in bundle");
        
        GameObject prefab = null;
        foreach (string assetName in assetNames)
        {
            if (assetName.Contains("gui") || assetName.Contains("ui") || assetName.Contains("crafting"))
            {
                prefab = portBundle.LoadAsset<GameObject>(assetName);
                if (prefab != null)
                {
                    context?.AddString($"Loaded prefab: {assetName}");
                    break;
                }
            }
        }

        // If no UI prefab found by name pattern, try loading the first GameObject
        if (prefab == null && assetNames.Length > 0)
        {
            prefab = portBundle.LoadAsset<GameObject>(assetNames[0]);
            context?.AddString($"Loaded first asset: {assetNames[0]}");
        }

        if (prefab == null)
        {
            context?.AddString("Error: No suitable UI prefab found in asset bundle");
            shouldShowUI = false;
            return;
        }

        // Instantiate the UI using Jotunn's CustomGUIFront
        shipmentsUI = UnityEngine.Object.Instantiate(prefab, GUIManager.CustomGUIFront.transform, false);
        
        if (shipmentsUI != null)
        {
            // Disable UIGamePad components to prevent conflicts with Valheim's UI system
            var uiGamePads = shipmentsUI.GetComponentsInChildren<UIGamePad>();
            foreach (var uiGamePad in uiGamePads)
            {
                uiGamePad.enabled = false;
            }
            context?.AddString($"Disabled {uiGamePads.Length} UIGamePad components");

            // Block input so the player can interact with the UI
            GUIManager.BlockInput(true);

            context?.AddString("Shipments UI opened successfully");
        }
        else
        {
            context?.AddString("Error: Failed to instantiate shipments UI");
            shouldShowUI = false;
        }
    }

    private static void CloseShipmentsUI(Terminal context = null)
    {
        if (shipmentsUI != null)
        {
            UnityEngine.Object.Destroy(shipmentsUI);
            shipmentsUI = null;
        }

        // Release input back to the player
        GUIManager.BlockInput(false);

        context?.AddString("Shipments UI closed");
    }

    public static void Cleanup()
    {
        // Unsubscribe from events to prevent memory leaks
        GUIManager.OnCustomGUIAvailable -= OnGUIAvailable;
        
        // Clean up UI if it exists
        if (shipmentsUI != null)
        {
            UnityEngine.Object.Destroy(shipmentsUI);
            shipmentsUI = null;
        }
        
        shouldShowUI = false;
        GUIManager.BlockInput(false);
    }
}