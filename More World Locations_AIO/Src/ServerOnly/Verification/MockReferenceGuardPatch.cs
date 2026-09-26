using System;
using System.Reflection;
using HarmonyLib;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// Where <see cref="MockReferenceGuard"/> sits: in front of Jötunn's
/// per-member fix. Installed only in server-only mode, where the audit
/// resolves every template in the catalogue, unresolvable mocks included.
/// </summary>
public static class MockReferenceGuardPatch
{
    private static BepInEx.Logging.ManualLogSource Log =>
        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger;

    private static bool s_installed;

    /// <summary>Patch Jötunn's walk. Called once, at startup, in server-only mode.</summary>
    public static void Install(Harmony harmony)
    {
        if (s_installed)
            return;
        try
        {
            Type? mockManager = AccessTools.TypeByName("Jotunn.Managers.MockManager");
            MethodInfo? target = mockManager == null ? null : AccessTools.Method(mockManager, "FixMemberReferences");
            if (target == null)
            {
                // Loud: without this the audit can mutate vanilla prefabs, and
                // that is worth knowing about before any template is opened.
                Log.LogError(
                    "Jötunn's MockManager.FixMemberReferences was not found, so mock resolution is NOT guarded " +
                    "against reparenting objects into vanilla prefabs. This build of Jötunn has not been measured with.");
                return;
            }
            harmony.Patch(target, prefix: new HarmonyMethod(typeof(MockReferenceGuardPatch), nameof(Prefix)));
            s_installed = true;
            Log.LogInfo("Server-only mode: mock resolution will not touch engine hierarchy members.");
        }
        catch (Exception ex)
        {
            Log.LogError($"Could not guard Jötunn's mock resolution: {ex.GetType().Name}: {ex.Message}");
        }
    }

    // Jötunn's MemberBase is internal and its concrete members keep their
    // reflection info in a private field; both are read by name.
    public static bool Prefix(object __0)
    {
        try
        {
            Traverse t = Traverse.Create(__0);
            object? info = t.Field("propertyInfo").GetValue() ?? t.Field("fieldInfo").GetValue();
            return MockReferenceGuard.Allow(info as MemberInfo);
        }
        catch
        {
            // A guard that cannot read the member lets the walk proceed as it
            // always did; it never throws into Jötunn.
            return true;
        }
    }
}
