using System;
using System.Collections.Generic;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// What to do about a component the stock client does not have.
///
/// <para><b>By capability, not by template.</b> The tempting shape here is a
/// list of templates to forgive, and it is the wrong one: it grows by one entry
/// every time a build is added, each entry records a judgement nobody can
/// re-derive, and the next build with the same component is judged again from
/// scratch. The question a component raises is the same wherever it appears —
/// does a client that has never heard of this type still end up with what the
/// author drew — so the answer belongs to the type.</para>
///
/// <para><b>Empty by default, and that is the point.</b> A foreign component on
/// an object the client receives is unknown client-dependent behaviour until
/// somebody measures otherwise. Adding a name here is a claim with evidence
/// behind it, written next to the name. Nothing has that evidence yet.</para>
/// </summary>
public sealed class ComponentPolicy
{
    private readonly HashSet<string> _harmless;
    private readonly HashSet<string> _gameAssemblies;

    public ComponentPolicy(IEnumerable<string>? harmlessForeignComponents = null, IEnumerable<string>? gameAssemblies = null)
    {
        _harmless = new HashSet<string>(harmlessForeignComponents ?? Array.Empty<string>(), StringComparer.Ordinal);
        _gameAssemblies = new HashSet<string>(gameAssemblies ?? DefaultGameAssemblies, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The assemblies a stock client loads. A component type from anywhere else
    /// exists only where the mod is installed.
    ///
    /// <c>assembly_valheim</c> is the game's own code, <c>assembly_utils</c> and
    /// <c>assembly_guiutils</c> its helpers; the rest is Unity. Everything here
    /// is present on a client that has never installed anything.
    /// </summary>
    public static readonly IReadOnlyList<string> DefaultGameAssemblies = new[]
    {
        "assembly_valheim",
        "assembly_utils",
        "assembly_guiutils",
        "assembly_postprocessing",
        "UnityEngine",
        "UnityEngine.CoreModule",
        "UnityEngine.PhysicsModule",
        "UnityEngine.AudioModule",
        "UnityEngine.ParticleSystemModule",
        "UnityEngine.AnimationModule",
        "UnityEngine.TerrainModule",
        "UnityEngine.UI",
        "Unity.TextMeshPro",
    };

    /// <summary>The shipped policy: the game's assemblies, and nothing forgiven.</summary>
    public static ComponentPolicy Default { get; } = new ComponentPolicy();

    /// <summary>Whether a component from this assembly exists on a client without the mod.</summary>
    public bool IsGameAssembly(string assemblyName)
    {
        if (string.IsNullOrEmpty(assemblyName))
            return false;
        // Unity ships its engine as many small modules and their names move
        // between versions, so the family is matched rather than each member.
        if (assemblyName.StartsWith("UnityEngine", StringComparison.OrdinalIgnoreCase))
            return true;
        return _gameAssemblies.Contains(assemblyName);
    }

    /// <summary>
    /// Whether a component type the stock client lacks can be left on an object
    /// the client receives.
    /// </summary>
    public bool IsHarmless(string componentTypeName) =>
        !string.IsNullOrEmpty(componentTypeName) && _harmless.Contains(componentTypeName);

    /// <summary>The forgiven types, for the report: a policy nobody can read is a policy nobody can challenge.</summary>
    public IReadOnlyCollection<string> HarmlessForeignComponents => _harmless;
}
