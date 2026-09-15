using System;
using System.Collections.Generic;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// Between <see cref="TerrainZoneDeltas"/> and the game's <c>TerrainComp</c>.
///
/// The conversion is arithmetic over arrays; the compiler is where those arrays
/// have to live for a client without the mod to receive them. This carries them
/// across, and owns the two things that are easy to get wrong.
///
/// <para><b>The base height.</b> The conversion asks for the height the client
/// generates for itself, BEFORE any compiler delta — that is the contract on
/// <c>TerrainConversion.VertexHeight</c>, and it is the one way to silently
/// produce the wrong ground. A compiler's own <c>m_hmap.GetHeight</c> is the
/// height AFTER its deltas have been applied, so reading it would fold the
/// existing deltas in twice. <see cref="GeneratedHeightAt"/> takes the value the
/// world generator gives, which is what a stock client's heightmap builds
/// from.</para>
///
/// <para><b>When the compiler may be asked for.</b> The game keeps one
/// <c>TerrainComp</c> per zone and creates the saved one from its ZDO only after
/// the zone has loaded. A compiler asked for before that is a second one, and
/// the two destroy each other on every load — the roads work was bitten by this
/// twice. So this never calls <c>GetAndCreateTerrainCompiler</c> on a zone that
/// has a saved compiler which is not alive yet; the caller is told to wait.</para>
/// </summary>
public static class LocationTerrainBridge
{
    /// <summary>Prefab hash of the game's terrain compiler object, one per zone.</summary>
    internal static readonly int TerrainCompilerPrefab = "_TerrainCompiler".GetStableHashCode();

    /// <summary>
    /// ZDO key for the sites this zone's compiler already carries.
    ///
    /// It lives on the compiler rather than in memory because the question it
    /// answers — "has this site's terrain been written here" — has to survive a
    /// restart. A ledger that forgets would convert the same site again on every
    /// load, and the conversion states an absolute height, so a repeat is not
    /// destructive; but it would also overwrite whatever a player has dug since,
    /// which is.
    /// </summary>
    internal static readonly int AppliedSitesKey = "MWL_terrain_sites".GetStableHashCode();

    /// <summary>Why a zone could not be written now.</summary>
    public enum Readiness
    {
        /// <summary>The compiler is in hand and owned here.</summary>
        Ready,
        /// <summary>The zone has a saved compiler that has not come alive yet: ask again when it has.</summary>
        WaitForSavedCompiler,
        /// <summary>Another peer owns this zone's compiler. Theirs to write, not ours.</summary>
        OwnedElsewhere,
        /// <summary>No heightmap for the zone here, so nothing to write through.</summary>
        NoHeightmap,
    }

    /// <summary>
    /// The zone's compiler, if this peer may write it now.
    ///
    /// <paramref name="heightmap"/> is the zone's live heightmap — during ghost
    /// generation the game still has it in hand, which is the moment this is
    /// meant to be called.
    /// </summary>
    public static Readiness Acquire(Heightmap heightmap, Vector2s zone, out TerrainComp compiler)
    {
        compiler = null;
        if (heightmap == null)
            return Readiness.NoHeightmap;

        TerrainComp existing = TerrainComp.FindTerrainCompiler(heightmap.transform.position);
        if (existing == null && HasSavedCompiler(zone))
            return Readiness.WaitForSavedCompiler;

        compiler = existing ?? heightmap.GetAndCreateTerrainCompiler();
        if (compiler == null)
            return Readiness.NoHeightmap;

        if (compiler.m_nview == null || !compiler.m_nview.IsValid())
            return Readiness.NoHeightmap;
        if (!compiler.m_nview.IsOwner())
        {
            if (compiler.m_nview.HasOwner())
            {
                compiler = null;
                return Readiness.OwnedElsewhere;
            }
            compiler.m_nview.ClaimOwnership();
        }
        return Readiness.Ready;
    }

    /// <summary>Whether the zone's saved objects include a terrain compiler.</summary>
    public static bool HasSavedCompiler(Vector2s zone)
    {
        if (ZDOMan.instance == null)
            return false;
        var zdos = new List<ZDO>();
        // Valheim 1.0 asks callers to carry the sectors already visited; this is
        // a single-zone lookup, so it starts empty.
        ZDOMan.instance.FindObjects(zone, zdos, new HashSet<ZoneSystem.SectorIndex>());
        foreach (ZDO zdo in zdos)
            if (zdo.GetPrefab() == TerrainCompilerPrefab)
                return true;
        return false;
    }

    /// <summary>
    /// The compiler's arrays as a <see cref="TerrainZoneDeltas"/>, carrying the
    /// record of which sites it already holds.
    ///
    /// The deltas object is a copy, not a view: nothing reaches the compiler
    /// until <see cref="WriteBack"/>, which is what lets a refused conversion
    /// leave the zone untouched.
    /// </summary>
    public static TerrainZoneDeltas Adopt(TerrainComp compiler)
    {
        if (compiler == null) throw new ArgumentNullException(nameof(compiler));
        if (compiler.m_hmap == null) throw new ArgumentException("compiler has no heightmap", nameof(compiler));

        var zone = new TerrainZoneDeltas(
            compiler.m_hmap.transform.position, compiler.m_width, compiler.m_hmap.m_scale);

        int n = zone.Pitch * zone.Pitch;
        if (compiler.m_levelDelta == null || compiler.m_levelDelta.Length < n)
            throw new ArgumentException(
                $"compiler arrays are {compiler.m_levelDelta?.Length ?? 0} long, not {n}: it is not initialised yet",
                nameof(compiler));

        Array.Copy(compiler.m_levelDelta, zone.LevelDelta, n);
        Array.Copy(compiler.m_smoothDelta, zone.SmoothDelta, n);
        Array.Copy(compiler.m_modifiedHeight, zone.ModifiedHeight, n);
        Array.Copy(compiler.m_paintMask, zone.PaintMask, n);
        Array.Copy(compiler.m_modifiedPaint, zone.ModifiedPaint, n);

        zone.DeserializeApplied(compiler.m_nview?.GetZDO()?.GetString(AppliedSitesKey, "") ?? "");
        return zone;
    }

    /// <summary>
    /// Copy the deltas back into the compiler, save them into its ZDO, and poke
    /// the heightmap so the change is drawn.
    ///
    /// Returns whether the ZDO now carries terrain data. The game's
    /// <c>TerrainComp.Save</c> returns without a word when this peer does not own
    /// the compiler, so the write is confirmed rather than assumed.
    /// </summary>
    public static bool WriteBack(TerrainComp compiler, TerrainZoneDeltas zone)
    {
        if (compiler == null) throw new ArgumentNullException(nameof(compiler));
        if (zone == null) throw new ArgumentNullException(nameof(zone));

        ZNetView view = compiler.m_nview;
        if (view == null || !view.IsValid() || !view.IsOwner())
            return false;

        int n = zone.Pitch * zone.Pitch;
        Array.Copy(zone.LevelDelta, compiler.m_levelDelta, n);
        Array.Copy(zone.SmoothDelta, compiler.m_smoothDelta, n);
        Array.Copy(zone.ModifiedHeight, compiler.m_modifiedHeight, n);
        Array.Copy(zone.PaintMask, compiler.m_paintMask, n);
        Array.Copy(zone.ModifiedPaint, compiler.m_modifiedPaint, n);

        // The record goes in before Save, so a restart cannot find terrain
        // written with no note of which site wrote it.
        view.GetZDO().Set(AppliedSitesKey, zone.SerializeApplied());
        compiler.Save();
        if (view.GetZDO().GetByteArray(ZDOVars.s_TCData) == null)
            return false;

        // Valheim 1.0 turned Poke's bool into a selector for WHICH late pass
        // rebuilds; 1 is the LateUpdate pass the game's own terrain edits use.
        compiler.m_hmap?.Poke(1);
        return true;
    }

    /// <summary>
    /// The height a client generates for itself at a vertex of this zone — the
    /// value <c>TerrainConversion</c>'s contract asks for.
    ///
    /// This is the world generator's own answer, which is what a stock client's
    /// heightmap is built from before any compiler delta is added. It is
    /// deliberately not <c>m_hmap.GetHeight</c>: that is the height AFTER the
    /// compiler has been applied, and using it would measure the site against
    /// ground that already carries the deltas being replaced.
    /// </summary>
    public static TerrainConversion.VertexHeight GeneratedHeightAt(TerrainZoneDeltas zone)
    {
        if (zone == null) throw new ArgumentNullException(nameof(zone));
        int half = zone.Width / 2;
        return (x, y) =>
        {
            float wx = zone.Origin.x + (x - half) * zone.Scale;
            float wz = zone.Origin.z + (y - half) * zone.Scale;
            return WorldGenerator.instance != null ? WorldGenerator.instance.GetHeight(wx, wz) : 0f;
        };
    }
}
