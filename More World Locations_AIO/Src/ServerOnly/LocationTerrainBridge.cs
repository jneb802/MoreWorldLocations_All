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
    public static bool WriteBack(TerrainComp compiler, TerrainZoneDeltas zone) =>
        WriteBack(compiler, zone, out _);

    /// <summary>
    /// As above, reporting why a write did not land.
    ///
    /// <para><b>The completion record goes in only after the terrain is
    /// confirmed.</b> It used to be written first and the save judged by
    /// "TCData is not null", which a blob left by anyone — a player's digging,
    /// an earlier site, the game itself — satisfies without this write having
    /// landed at all. That pairs a new completed identity with old terrain, and
    /// the site is then never looked at again.</para>
    ///
    /// <para>What is checked instead is the saved bytes, decoded back and
    /// compared against the arrays this call intended. A write that changes
    /// nothing is still a pass: the test is equality with the intended state,
    /// not that some hash moved.</para>
    ///
    /// <para>The remaining window is a crash between the confirmed terrain and
    /// the completion record. That direction is safe — the site is converted
    /// again on the next pass, and the conversion states absolute heights, so a
    /// repeat lands on the same ground. The unsafe direction, a completion
    /// record with no terrain, is what the ordering removes.</para>
    /// </summary>
    public static bool WriteBack(TerrainComp compiler, TerrainZoneDeltas zone, out string failure)
    {
        if (compiler == null) throw new ArgumentNullException(nameof(compiler));
        if (zone == null) throw new ArgumentNullException(nameof(zone));

        failure = null;
        ZNetView view = compiler.m_nview;
        if (view == null || !view.IsValid())
        {
            failure = "the compiler has no valid view";
            return false;
        }
        if (!view.IsOwner())
        {
            failure = "another peer owns this compiler, so the game's own Save would do nothing";
            return false;
        }

        int n = zone.Pitch * zone.Pitch;
        Array.Copy(zone.LevelDelta, compiler.m_levelDelta, n);
        Array.Copy(zone.SmoothDelta, compiler.m_smoothDelta, n);
        Array.Copy(zone.ModifiedHeight, compiler.m_modifiedHeight, n);
        Array.Copy(zone.PaintMask, compiler.m_paintMask, n);
        Array.Copy(zone.ModifiedPaint, compiler.m_modifiedPaint, n);

        compiler.Save();

        byte[] saved = view.GetZDO().GetByteArray(ZDOVars.s_TCData);
        if (saved == null)
        {
            failure = "the compiler saved no terrain data at all";
            return false;
        }
        if (!TerrainBlob.Matches(saved, zone, out string mismatch))
        {
            failure = "the saved terrain is not what this write intended: " + mismatch;
            return false;
        }

        view.GetZDO().Set(AppliedSitesKey, zone.SerializeApplied());

        // Valheim 1.0 turned Poke's bool into a selector for WHICH late pass
        // rebuilds; 1 is the LateUpdate pass the game's own terrain edits use.
        compiler.m_hmap?.Poke(1);
        return true;
    }

    /// <summary>
    /// The zone's saved compiler ZDO, or null.
    ///
    /// More than one is a fault in itself — the game keeps one per zone and two
    /// destroy each other on every load — so this reports the count rather than
    /// picking one.
    /// </summary>
    public static ZDO SavedCompiler(Vector2s zone, out int count)
    {
        count = 0;
        ZDO found = null;
        if (ZDOMan.instance == null)
            return null;
        var zdos = new List<ZDO>();
        ZDOMan.instance.FindObjects(zone, zdos, new HashSet<ZoneSystem.SectorIndex>());
        foreach (ZDO zdo in zdos)
        {
            if (zdo.GetPrefab() != TerrainCompilerPrefab)
                continue;
            count++;
            if (found == null)
                found = zdo;
        }
        return found;
    }

    /// <summary>
    /// Read a zone's terrain from its saved compiler, with no live compiler
    /// anywhere. Returns null when the zone has none.
    /// </summary>
    public static TerrainZoneDeltas AdoptSaved(Vector2s zoneId, int width, float scale, out TerrainBlob.Header header, out string problem)
    {
        header = TerrainBlob.Header.Fresh;
        problem = null;
        ZDO zdo = SavedCompiler(zoneId, out int count);
        if (count > 1)
        {
            problem = $"zone {zoneId.x},{zoneId.y} has {count} terrain compilers; the game keeps one, " +
                      "and two destroy each other on every load";
            return null;
        }

        var zone = new TerrainZoneDeltas(ZoneSystem.GetZonePos(zoneId), width, scale);
        if (zdo == null)
            return zone;

        byte[] saved = zdo.GetByteArray(ZDOVars.s_TCData);
        if (saved != null && !TerrainBlob.TryDecode(saved, zone, out header, out problem))
            return null;
        zone.DeserializeApplied(zdo.GetString(AppliedSitesKey, ""));
        return zone;
    }

    /// <summary>
    /// Write a zone's terrain with no live compiler: the repair path for a zone
    /// that is already generated and is not loaded here.
    ///
    /// A dedicated server keeps almost no zone loaded, and a zone's generation
    /// hook does not run a second time, so a site discovered from a NEIGHBOUR's
    /// generation has no other way to reach ground it already shaped. The bytes
    /// are the compiler's own format and the header is carried through unchanged,
    /// so a client loads this exactly as it loads the game's own write.
    ///
    /// <para>It refuses when a live compiler exists for the zone: that object
    /// owns those arrays, would overwrite this on its next save, and is the path
    /// to use instead.</para>
    /// </summary>
    public static bool WriteDetached(Vector2s zoneId, TerrainZoneDeltas zone, TerrainBlob.Header header, out string failure)
    {
        failure = null;
        if (zone == null) throw new ArgumentNullException(nameof(zone));
        if (ZDOMan.instance == null)
        {
            failure = "there is no ZDOMan, so there is nowhere to write";
            return false;
        }

        Vector3 zonePos = ZoneSystem.GetZonePos(zoneId);
        if (TerrainComp.FindTerrainCompiler(zonePos) != null)
        {
            failure = "this zone has a live compiler; write through it rather than behind it";
            return false;
        }

        ZDO zdo = SavedCompiler(zoneId, out int count);
        if (count > 1)
        {
            failure = $"zone {zoneId.x},{zoneId.y} already has {count} terrain compilers";
            return false;
        }
        if (zdo == null)
        {
            zdo = ZDOMan.instance.CreateNewZDO(zonePos, TerrainCompilerPrefab);
            zdo.Persistent = true;
            zdo.SetPrefab(TerrainCompilerPrefab);
            zdo.SetRotation(Quaternion.identity);
        }
        if (!zdo.IsOwner() && !Claimable(zdo, out string held))
        {
            failure = held;
            return false;
        }
        if (!zdo.IsOwner())
            zdo.SetOwner(ZDOMan.instance.m_sessionID);

        zdo.Set(ZDOVars.s_TCData, TerrainBlob.Encode(zone, header));

        byte[] saved = zdo.GetByteArray(ZDOVars.s_TCData);
        if (!TerrainBlob.Matches(saved, zone, out string mismatch))
        {
            failure = "the terrain written back does not read as intended: " + mismatch;
            return false;
        }
        zdo.Set(AppliedSitesKey, zone.SerializeApplied());
        return true;
    }

    /// <summary>
    /// Whether an owner recorded on a saved compiler is one this peer must
    /// respect.
    ///
    /// Ownership is saved with the ZDO, so after a restart a compiler can name a
    /// session that no longer exists — the client that last dug there. Refusing
    /// on that leaves the repair blocked for the life of the world: observed on
    /// 15 Sep, a zone stuck at "another peer owns this zone's compiler" with
    /// zero peers connected. A live peer's compiler is still theirs; a
    /// departed one's is nobody's, and vanilla reassigns by proximity anyway.
    /// </summary>
    private static bool Claimable(ZDO zdo, out string held)
    {
        held = null;
        long owner = zdo.GetOwner();
        if (owner == 0L)
            return true;
        if (ZNet.instance != null && ZNet.instance.GetPeer(owner) != null)
        {
            held = "a connected peer owns this zone's compiler";
            return false;
        }
        if (ZNet.instance == null)
        {
            held = "another peer owns this zone's compiler and there is no network to ask about it";
            return false;
        }
        return true;
    }

    /// <summary>
    /// The height a client generates for itself at a vertex of this zone — the
    /// value <c>TerrainConversion</c>'s contract asks for.
    ///
    /// <para><b>It is the heightmap builder's array, not
    /// <c>WorldGenerator.GetHeight</c>.</b> They are not the same function.
    /// <c>HeightmapBuilder.Build</c> takes the biome at the zone's four CORNERS
    /// and, when those disagree, bilinearly blends four <c>GetBiomeHeight</c>
    /// results with smoothstep weights; <c>WorldGenerator.GetHeight</c> looks the
    /// biome up per point. Inside one biome they agree, which is why a Meadows
    /// site measured correct in game; across a biome boundary they do not, and
    /// the difference would go straight into the written delta and displace the
    /// site by it.</para>
    ///
    /// <para>It is also deliberately not <c>m_hmap.GetHeight</c>, which is the
    /// height AFTER the compiler has been applied: that would measure the site
    /// against ground already carrying the deltas being replaced.</para>
    ///
    /// <para>Heights in the build data are relative to the heightmap's own
    /// <c>transform.position.y</c>, which is what <c>GetWorldBaseHeight</c> adds
    /// back, so this adds it too.</para>
    /// </summary>
    /// <param name="heightmap">
    /// The zone's heightmap when one is in hand — during generation it is, and
    /// its <c>m_buildData</c> is the exact array the client will use, already
    /// built. Null asks the builder instead.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// When neither the heightmap nor the builder can supply the array. There is
    /// no fallback to zero: converting against a height nobody generates writes
    /// the site into the wrong ground and records it as done.
    /// </exception>
    public static TerrainConversion.VertexHeight GeneratedHeightAt(TerrainZoneDeltas zone, Heightmap heightmap)
    {
        if (!TryGeneratedHeightAt(zone, heightmap, out TerrainConversion.VertexHeight height, out string reason))
            throw new InvalidOperationException(reason);
        return height;
    }

    /// <summary>
    /// The generated heights, fetched ONCE.
    ///
    /// Asking twice is a defect, not a repeated read: <c>HeightmapBuilder</c>'s
    /// ready list hands an entry out and REMOVES it
    /// (<c>RequestTerrain</c>: <c>m_ready.RemoveAt(i)</c>), so a "can I?" call
    /// followed by a "do it" call consumes the data in the first and finds
    /// nothing in the second. In game that threw on every retry, the exception
    /// was caught per zone, the attempt was never counted, and the site stayed
    /// outstanding for ever at attempt 11.
    /// </summary>
    public static bool TryGeneratedHeightAt(
        TerrainZoneDeltas zone, Heightmap heightmap,
        out TerrainConversion.VertexHeight height, out string reason)
    {
        // The ask-only form: nothing is held afterwards, so a caller that keeps
        // using the heights past this call must use ReadGeneratedHeightAt.
        HeightOutcome outcome = ReadGeneratedHeightAt(zone, heightmap, out height, out reason, out IDisposable hold);
        hold?.Dispose();
        return outcome == HeightOutcome.Read;
    }

    /// <summary>What a request for a zone's generated heights came back with.</summary>
    public enum HeightOutcome
    {
        /// <summary>Heights in hand, and held against eviction until the hold is returned.</summary>
        Read,
        /// <summary>Nothing can answer yet: the builder has not built the zone, or built the wrong grid. Try again.</summary>
        NotReady,
        /// <summary>
        /// The height budget has no room and everything in it is in use. Nothing
        /// was consumed; the work is deferred, and a deferral is not an attempt.
        /// </summary>
        Deferred,
    }

    /// <summary>
    /// The zone's generated heights, held for as long as the caller uses them.
    ///
    /// <para>Every buffer that comes back through here is counted while it is
    /// live: a cached entry is pinned, and a buffer the cache would not keep is
    /// adopted into the same accounting until <paramref name="hold"/> is
    /// returned. The heightmap's own build data is the game's array and is
    /// reported as the game's, not ours.</para>
    ///
    /// <para>Room is reserved BEFORE the builder is asked, because the builder
    /// hands its answer over once: consuming it and then finding no room would
    /// lose the answer the write needs. With no room the outcome is
    /// <see cref="HeightOutcome.Deferred"/>, and nothing was asked.</para>
    /// </summary>
    public static HeightOutcome ReadGeneratedHeightAt(
        TerrainZoneDeltas zone, Heightmap heightmap,
        out TerrainConversion.VertexHeight height, out string reason, out IDisposable hold)
    {
        if (zone == null) throw new ArgumentNullException(nameof(zone));
        height = null;
        reason = null;
        hold = null;

        HeightOutcome outcome = BaseHeights(zone, heightmap, out List<float> baseHeights, out float originY, out hold, out string why);
        if (outcome == HeightOutcome.Deferred)
        {
            reason = why;
            return outcome;
        }
        if (baseHeights == null)
        {
            reason = why ??
                $"no generated heights for the zone at {zone.Origin.x:0},{zone.Origin.z:0}: " +
                "neither its heightmap nor the builder has them, and converting against zero " +
                "would write the site into ground nobody generates";
            return HeightOutcome.NotReady;
        }

        int pitch = zone.Pitch;
        if (baseHeights.Count != pitch * pitch)
        {
            hold?.Dispose();
            hold = null;
            reason =
                $"the generated heights for the zone at {zone.Origin.x:0},{zone.Origin.z:0} are " +
                $"{baseHeights.Count} long, not {pitch * pitch}: the builder was asked for a " +
                "different width than the compiler holds";
            return HeightOutcome.NotReady;
        }

        float origin = originY;
        height = (x, y) => baseHeights[y * pitch + x] + origin;
        return HeightOutcome.Read;
    }

    /// <summary>
    /// Generated heights the builder has already handed over, by zone.
    ///
    /// <para>The builder's ready list hands an entry out and REMOVES it, so its
    /// answer can only be collected once. That was survivable while one caller
    /// asked once; it is not, now that the site preflight has to know the ground
    /// BEFORE a site is published and the conversion has to know it again
    /// afterwards. Keeping the answer is the only way both can ask — the
    /// alternative is one of them converting against nothing.</para>
    ///
    /// <para>Only zones somebody asked about are kept, which is the zones our
    /// sites touch, and the whole thing is dropped when a world is.</para>
    /// </summary>
    /// <summary>
    /// How many bytes of generated heights this mode may hold.
    ///
    /// One zone at width 64 is 65×65 floats — 16,900 bytes before overhead — so
    /// this is a few hundred zones. The dictionary it replaces had no bound at
    /// all and kept every zone a player had ever walked through until the world
    /// closed; ten thousand zones would have been about 161 MiB of payload for
    /// ground that had long since been written.
    /// </summary>
    public const long DefaultGeneratedHeightBudgetBytes = 32L * 1024 * 1024;

    /// <summary>The budget in force. The shipped value unless a test has narrowed it.</summary>
    public static long GeneratedHeightBudgetBytes => s_generatedHeights.BudgetBytes;

    private static ByteBudgetCache<GridKey, List<float>> s_generatedHeights =
        NewHeightCache(ValidationSwitches.HeightBudgetBytes(DefaultGeneratedHeightBudgetBytes));

    private static ByteBudgetCache<GridKey, List<float>> NewHeightCache(long budget) =>
        new ByteBudgetCache<GridKey, List<float>>(budget, heights => heights.Count * sizeof(float));

    /// <summary>
    /// Run with a different budget, for a test that needs to fill it. Never
    /// called by the mod: the shipped budget is a constant.
    /// </summary>
    internal static void UseGeneratedHeightBudgetForTest(long budgetBytes) =>
        s_generatedHeights = NewHeightCache(budgetBytes);

    /// <summary>Buffers in use that the cache could not keep, counted until they are returned.</summary>
    public static int GeneratedHeightAdopted => s_generatedHeights.Retired;

    /// <summary>Bytes one zone's heights take at the zone grid.</summary>
    public static int BytesPerZoneHeights =>
        (TerrainZoneDeltas.ZoneWidth + 1) * (TerrainZoneDeltas.ZoneWidth + 1) * sizeof(float);

    /// <summary>
    /// Whether <paramref name="zones"/> more zones' heights could be held now,
    /// evicting what is not in use to make the room.
    ///
    /// Asked by the readiness barrier before a zone is generated, so that a
    /// placement is held — not counted against its readiness budget — rather
    /// than reaching the site check with nowhere to keep the ground it reads.
    /// </summary>
    public static bool HasRoomForZoneHeights(int zones) =>
        zones <= 0 || s_generatedHeights.TryReserve(checked(zones * BytesPerZoneHeights));

    /// <summary>What the height cache is holding, for the operator command and a run's accounting.</summary>
    public static long GeneratedHeightBytes => s_generatedHeights.Bytes;

    /// <summary>How many zones' heights are held, and how many of those are in use.</summary>
    public static int GeneratedHeightEntries => s_generatedHeights.Count;

    public static int GeneratedHeightPins => s_generatedHeights.Pinned;

    /// <summary>
    /// What a kept set of heights is FOR: one zone, at one grid.
    ///
    /// The zone alone is not the identity. A 64-wide request and a 32-wide
    /// request for the same zone are different arrays of different lengths, and
    /// keeping them under one key hands the writer 1089 heights where it needs
    /// 4225 — for ever, because the wrong entry is preferred over a correct
    /// build that is sitting ready.
    /// </summary>
    internal readonly struct GridKey : System.IEquatable<GridKey>
    {
        private readonly Vector2s _zone;
        private readonly int _width;
        private readonly float _scale;

        public GridKey(Vector2s zone, int width, float scale)
        {
            _zone = zone;
            _width = width;
            _scale = scale;
        }

        public bool Equals(GridKey other) =>
            _zone == other._zone && _width == other._width && _scale.Equals(other._scale);

        public override bool Equals(object obj) => obj is GridKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = _zone.GetHashCode();
                hash = hash * 397 ^ _width;
                return hash * 397 ^ _scale.GetHashCode();
            }
        }
    }

    /// <summary>A new world generates different ground. See LocationTerrainWriter.Reset.</summary>
    internal static void ForgetGeneratedHeights() => s_generatedHeights.Clear();

    /// <summary>
    /// The zone's generated heights: the heightmap's own build data when it has
    /// it, then anything already collected from the builder, then the builder
    /// itself. Null when none of them can answer.
    /// </summary>
    private static HeightOutcome BaseHeights(
        TerrainZoneDeltas zone, Heightmap heightmap,
        out List<float> heights, out float originY, out IDisposable hold, out string why)
    {
        heights = null;
        originY = 0f;
        hold = null;
        why = null;
        int needed = zone.Pitch * zone.Pitch;

        // EXACTLY the right number, not at least. A longer array is a different
        // grid, and reading it with this grid's stride walks a 65-wide zone
        // along 33-wide rows: every row after the first comes from the wrong
        // place, and the result looks like terrain.
        if (heightmap != null && heightmap.m_buildData != null
            && heightmap.m_buildData.m_baseHeights != null
            && heightmap.m_buildData.m_baseHeights.Count == needed)
        {
            // The game's own array, owned by its heightmap: not ours to count.
            originY = heightmap.transform.position.y;
            heights = heightmap.m_buildData.m_baseHeights;
            return HeightOutcome.Read;
        }

        Vector2s zoneId = ZoneSystem.GetZone(new Vector3(zone.Origin.x, 0f, zone.Origin.z));
        var key = new GridKey(zoneId, zone.Width, zone.Scale);
        List<float> kept = s_generatedHeights.Peek(key);
        if (kept != null)
        {
            // A kept entry of the wrong length is a bug in the keeping, not
            // something to reinterpret. Drop it and ask again.
            if (kept.Count == needed)
            {
                hold = s_generatedHeights.Pin(key);
                heights = kept;
                return HeightOutcome.Read;
            }
            s_generatedHeights.Remove(key);
        }

        if (HeightmapBuilder.instance == null || WorldGenerator.instance == null)
            return HeightOutcome.NotReady;

        // Room first, builder second. The builder's answer can be collected
        // once, so it is not asked for until there is somewhere to keep it.
        int bytes = needed * sizeof(float);
        if (!s_generatedHeights.TryReserve(bytes))
        {
            why = $"the height budget has no room for the zone at {zone.Origin.x:0},{zone.Origin.z:0}: " +
                  $"{s_generatedHeights.Bytes / 1024} KiB of {s_generatedHeights.BudgetBytes / 1024 / 1024} MiB " +
                  $"is held by {s_generatedHeights.Pinned} zone(s) in use and {s_generatedHeights.Retired} adopted " +
                  "buffer(s). Deferred, not attempted: nothing was asked of the builder.";
            return HeightOutcome.Deferred;
        }

        // Asking for a zone the builder has not built blocks until it has.
        // IsTerrainReady queues the work and answers whether it is done, which is
        // the check SpawnZone itself makes before generating.
        var centre = new Vector3(zone.Origin.x, 0f, zone.Origin.z);
        if (!HeightmapBuilder.instance.IsTerrainReady(centre, zone.Width, zone.Scale, false, WorldGenerator.instance))
            return HeightOutcome.NotReady;

        HeightmapBuilder.HMBuildData data =
            HeightmapBuilder.instance.RequestTerrainSync(centre, zone.Width, zone.Scale, false, WorldGenerator.instance);
        List<float> built = data?.m_baseHeights;
        if (built == null)
            return HeightOutcome.NotReady;

        // Counted either way. Kept when it fits — room was reserved, so it does
        // unless one zone is larger than the whole budget — and adopted into the
        // same accounting when it does not, so a live buffer is never outside
        // the number this mode reports.
        hold = s_generatedHeights.Put(key, built)
            ? s_generatedHeights.Pin(key)
            : s_generatedHeights.Adopt(built);
        heights = built;
        return HeightOutcome.Read;
    }

    // HeightsReady is deliberately gone. It looked like a harmless question and
    // was a destructive one -- see TryGeneratedHeightAt.
}
