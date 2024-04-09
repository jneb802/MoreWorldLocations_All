using System.Collections.Generic;
using System.Reflection;
using Jotunn.Utils;
using UnityEngine;

namespace Meadows_Pack_1;

public static class WarpAssetManager
{
    private const string BundleName = "meadowspack1";
    public static AssetBundle assetBundle;

    public static void LoadAssetBundle()
    {
        assetBundle = AssetUtils.LoadAssetBundleFromResources(
            BundleName,
            Assembly.GetExecutingAssembly()
        );
        if (assetBundle == null)
        {
            WarpLogger.Logger.LogError("Failed to load asset bundle with name: " + BundleName);

        }
    }
}