using System.Collections.Generic;
using System.Reflection;
using Jotunn.Utils;
using UnityEngine;

namespace Common;

public class AssetManager
{
    public static AssetBundle assetBundle;
    public static string bundleName;

    public static void LoadAssetBundle()
    {
        assetBundle = AssetUtils.LoadAssetBundleFromResources(
            bundleName,
            Assembly.GetExecutingAssembly()
        );
        if (assetBundle == null)
        {
            WarpLogger.Logger.LogError("Failed to load asset bundle with name: " + bundleName);
        }
    }
}