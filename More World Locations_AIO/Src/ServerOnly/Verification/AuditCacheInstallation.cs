using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// The engine half of the verdict cache: where MWL, the game and the loader
/// are on disk, and what a live prefab looks like right now.
///
/// <para>Thin on purpose, like <see cref="GameTemplateAssets"/>. What counts as
/// the same input, how the file is checked, and when a verdict may be reused
/// are all in <see cref="AuditCache"/>, <see cref="AuditCacheKey"/> and
/// <see cref="AuditCacheFile"/>, tested without a game. What lives here is the
/// part that cannot be: BepInEx's folders, the loaded assemblies, and
/// Jötunn's view of the game's assets.</para>
/// </summary>
public static class AuditCacheInstallation
{
    private static BepInEx.Logging.ManualLogSource Log =>
        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger;

    private const string JotunnGuid = "com.jotunn.jotunn";

    /// <summary>Point the cache at this installation. Called once, at startup, in server-only mode.</summary>
    public static void Install(Harmony harmony)
    {
        try
        {
            // The mock resolution hook first: without it the record would miss
            // the prefabs a template's mocks are copied from when the copy is
            // not itself looked up (a child-path mock, say), so a cache that
            // cannot see those is a cache that stays off. Internal to Jötunn,
            // so found by name, as the reference guard finds it.
            Type? mockManager = AccessTools.TypeByName("Jotunn.Managers.MockManager");
            MethodInfo? target = mockManager == null ? null : AccessTools.Method(mockManager, "GetRealPrefabFromMock",
                new[] { typeof(UnityEngine.Object), typeof(Type) });
            if (target == null)
            {
                Log.LogWarning("catalogue audit: Jötunn's mock resolution entry point was not found, so the verdict " +
                               "cache is off and every start audits every template.");
                return;
            }
            harmony.Patch(target, postfix: new HarmonyMethod(typeof(AuditCacheInstallation), nameof(MockResolved)));

            AuditCache.DefaultPath = () => Path.Combine(Paths.CachePath, "MoreWorldLocations", "server-only-audit.txt");
            AuditCache.Installation = Compute;
            AuditCache.LiveStock = LiveStock;
        }
        catch (Exception ex)
        {
            AuditCache.Installation = null;
            AuditCache.LiveStock = null;
            Log.LogWarning($"catalogue audit: the verdict cache could not be installed ({ex.GetType().Name}: {ex.Message}); " +
                           "every start audits every template.");
        }
    }

    /// <summary>
    /// The installation's key parts and each template's own files. Throws on anything it cannot read: a part
    /// it could not compute is a part it cannot vouch for, and the caller turns
    /// that into a miss that stores nothing.
    /// </summary>
    private static InstalledInputs Compute(IReadOnlyCollection<string> templateNames)
    {
        string? cache = AuditCache.CachePath();
        string cacheFull = cache == null ? "" : Path.GetFullPath(cache);
        // The cache file and its temporaries are never an input, wherever the
        // environment put them; otherwise every write would change the key.
        Func<string, bool> isCache = path =>
            cacheFull.Length > 0 && (string.Equals(path, cacheFull, StringComparison.OrdinalIgnoreCase)
                                     || path.StartsWith(cacheFull + ".", StringComparison.OrdinalIgnoreCase));

        string mwlAssembly = PluginLocation(More_World_Locations_AIOPlugin.ModGUID, typeof(More_World_Locations_AIOPlugin).Assembly);
        string mwlDirectory = Path.GetDirectoryName(mwlAssembly) ?? throw new InvalidOperationException("MWL's folder is unknown");

        return AuditCacheCanonical.Installation(
            mwlDirectory,
            AssetBundles.manifestPath,
            mwlAssembly,
            templateNames,
            Paths.ConfigPath,
            More_World_Locations_AIOPlugin.ModGUID,
            Required(typeof(ZNet).Assembly.Location, "the game's assembly"),
            global::Version.GetVersionString(true),
            NetworkVersion(),
            SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null,
            PluginLocation(JotunnGuid, typeof(Jotunn.Main).Assembly),
            Paths.BepInExAssemblyDirectory,
            MwlCode(),
            isCache);
    }

    // The code does not change while the process runs; the text is computed once.
    private static string? s_mwlCode;

    /// <summary>MWL's server-only code and embedded data, as the key's <c>mwl-code</c> part (<see cref="AuditCacheCode"/>).</summary>
    private static string MwlCode() =>
        s_mwlCode ??= AuditCacheCode.Canonical(typeof(More_World_Locations_AIOPlugin).Assembly, AuditCacheCode.InServerOnly);

    /// <summary>
    /// Read at run time, not compiled in: a constant from the publicized
    /// assembly this was built against would describe that build, not the game
    /// that is running.
    /// </summary>
    private static string NetworkVersion()
    {
        FieldInfo? field = typeof(global::Version).GetField("c_networkVersion", BindingFlags.Public | BindingFlags.Static);
        object? value = field?.IsLiteral == true ? field.GetRawConstantValue() : field?.GetValue(null);
        return value?.ToString() ?? "(none)";
    }

    /// <summary>A plugin's DLL, by the loader's record and then by the assembly itself.</summary>
    private static string PluginLocation(string guid, Assembly assembly)
    {
        if (BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(guid, out PluginInfo info)
            && !string.IsNullOrEmpty(info?.Location))
            return info!.Location;
        return Required(assembly.Location, $"the DLL of {guid}");
    }

    private static string Required(string path, string what) =>
        string.IsNullOrEmpty(path) ? throw new InvalidOperationException($"{what} has no file location") : path;

    /// <summary>
    /// A stock name's live signature, from the lookup the comparison itself
    /// uses as its baseline (ZNetScene first). Not Jötunn's cache of every
    /// loaded object: that answers with whatever object of the name happens to
    /// be loaded, template pieces included, and its answer moves as the audit
    /// opens and releases templates.
    /// </summary>
    private static string? LiveStock(string name) => AuditCache.SignStock(name, TemplateAssets.StockPrefabs);

    /// <summary>
    /// Every mock Jötunn looks up while a storing audit runs, recorded by the
    /// asset it asks for. Observes only: it never changes what was resolved,
    /// and a failure in it is swallowed where Jötunn cannot see it.
    /// </summary>
    private static void MockResolved(UnityEngine.Object __0, Type __1)
    {
        try
        {
            StockRecord? record = AuditCache.Recording;
            if (record == null || __0 == null || __1 == null)
                return;
            string name = __0.name ?? "";
            // The same cleaning Jötunn applies before reading the name.
            if (__1 == typeof(Material) && name.EndsWith("(Instance)", StringComparison.Ordinal))
                name = name.Substring(0, name.Length - "(Instance)".Length);
            else if (__1 == typeof(Mesh) && name.EndsWith("Instance", StringComparison.Ordinal))
                name = name.Substring(0, name.Length - "Instance".Length);
            if (!AuditCache.TryMockAsset(name, out string asset, out _))
                return;

            // By the asset asked for, whatever the type: a GameObject mock, or
            // one with a child path, is copied from the prefab of that name, and
            // a plain asset mock falls back to looking inside it. Only a stock
            // name is signed; see StockRecord.
            record.Consulted(asset);
        }
        catch
        {
            // Recording is not allowed to change resolution.
        }
    }
}
