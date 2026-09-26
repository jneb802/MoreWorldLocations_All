using System;
using System.Collections.Generic;
using HarmonyLib;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// Keep Jötunn's mock resolution from moving objects between prefabs.
///
/// <para><b>What was measured (15 Sep 2026, on a dedicated test server).</b> Jötunn
/// resolves a template's mocks with a reflective walk over every field and
/// property of every component it visits, base types included, five levels
/// deep, and it SETS any settable Unity-object member whose current value is
/// named like a mock. <c>Transform.parent</c> and <c>Transform.parentInternal</c>
/// are such members. When the walk reaches an object whose parent is an
/// unresolvable mock — <c>MWL_ForestSkull1</c> carries
/// <c>JVLmock_JVLmock_Pickable_SurtlingCoreStand</c>, a double prefix that
/// resolves to nothing — it looks the parent's name up as a Transform and
/// reparents the object under whatever that finds: first the template's own
/// authored copy, then, because that copy is itself named like a mock, the
/// vanilla <c>Pickable_SurtlingCoreStand</c> prefab. The vanilla prefab went
/// from 5 objects to 13, the extra eight carrying runtime instance IDs.</para>
///
/// <para><b>Why it matters.</b> The moved objects came out of the template's
/// bundle. When the audit released the template and the bundle unloaded, the
/// script behind their <c>LightFlicker</c> died with it, and every copy of the
/// vanilla prefab made afterwards — every later mock resolution, and every
/// placement — carried a component with no script. That is the five-template
/// regression, and it is a mutation of a shared vanilla asset in either
/// case.</para>
///
/// <para><b>What this does.</b> A member declared by an engine hierarchy type
/// — <c>UnityEngine.Object</c>, <c>Component</c>, <c>Behaviour</c>,
/// <c>MonoBehaviour</c>, <c>GameObject</c>, <c>Transform</c> — is never a mock
/// reference a mod authored; it is the object's place in the scene. Those are
/// skipped. Everything a mod can author — mesh, material, prefab fields, drop
/// tables — is fixed exactly as before, so no compatibility check
/// changes.</para>
/// </summary>
public static class MockReferenceGuard
{
    /// <summary>The engine types whose members describe where an object IS, never what it references.</summary>
    private static readonly HashSet<string> HierarchyTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        "UnityEngine.Object",
        "UnityEngine.Component",
        "UnityEngine.Behaviour",
        "UnityEngine.MonoBehaviour",
        "UnityEngine.GameObject",
        "UnityEngine.Transform",
        "UnityEngine.RectTransform",
    };

    /// <summary>
    /// Whether a member declared by <paramref name="declaringTypeFullName"/> is
    /// one the walk must leave alone. Pure, so it is tested by name.
    /// </summary>
    public static bool IsHierarchyMember(string? declaringTypeFullName) =>
        declaringTypeFullName != null && HierarchyTypes.Contains(declaringTypeFullName);

    /// <summary>How many member visits were skipped, for the memory/status line.</summary>
    public static int Skipped { get; private set; }

    /// <summary>Distinct members skipped, by declaring type and name, so the report names them.</summary>
    public static IReadOnlyCollection<string> SkippedMembers => s_skippedMembers;

    private static readonly HashSet<string> s_skippedMembers = new HashSet<string>(StringComparer.Ordinal);

    internal static void ResetForTest()
    {
        Skipped = 0;
        s_skippedMembers.Clear();
    }

    /// <summary>
    /// Decide for one member of Jötunn's walk. True lets the walk proceed.
    /// </summary>
    public static bool Allow(System.Reflection.MemberInfo? member)
    {
        if (member == null)
            return true;
        if (!IsHierarchyMember(member.DeclaringType?.FullName))
            return true;
        Skipped++;
        if (s_skippedMembers.Count < 64)
            s_skippedMembers.Add($"{member.DeclaringType!.Name}.{member.Name}");
        return false;
    }

    public static string Status() =>
        Skipped == 0
            ? "mock resolution: no hierarchy member was touched"
            : $"mock resolution: {Skipped} hierarchy member visit(s) skipped ({string.Join(", ", new List<string>(s_skippedMembers).ToArray())})";
}
