// Minimal stand-ins for the Valheim types the road logic uses.
// WorldGenerator is virtual here so tests can plug in synthetic worlds.

// ReSharper disable InconsistentNaming

/// <summary>Mirror of Valheim's string.GetStableHashCode extension (Utils).</summary>
public static class StringExtensionMethods
{
    public static int GetStableHashCode(this string str)
    {
        unchecked
        {
            int hash1 = 5381;
            int hash2 = hash1;
            for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1 || str[i + 1] == '\0')
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }
            return hash1 + hash2 * 1566083941;
        }
    }
}

/// <summary>Mirror of Valheim's global Vector2i (integer grid coordinate).</summary>
public struct Vector2i
{
    public int x;
    public int y;

    public Vector2i(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override bool Equals(object? other) =>
        other is Vector2i v && v.x == x && v.y == y;

    public override int GetHashCode() => x.GetHashCode() ^ (y.GetHashCode() << 16);

    public static bool operator ==(Vector2i a, Vector2i b) => a.x == b.x && a.y == b.y;
    public static bool operator !=(Vector2i a, Vector2i b) => !(a == b);

    public override string ToString() => $"({x}, {y})";
}

/// <summary>
/// Mirror of Valheim 1.0's global Vector2s, which replaced Vector2i as the
/// type of a zone id. The fields really are short in the game: a zone id is
/// small and the game packs a lot of them, so the mod must not assume it can
/// put an arbitrary int in one. The int constructor narrows exactly as the
/// game's does, which is why the mod's own grid coordinates stay Vector2i.
/// </summary>
public struct Vector2s
{
    public short x;
    public short y;

    public Vector2s(short x, short y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector2s(int x, int y)
    {
        this.x = (short)x;
        this.y = (short)y;
    }

    public Vector2s(Vector2i v)
    {
        x = (short)v.x;
        y = (short)v.y;
    }

    public override bool Equals(object? other) =>
        other is Vector2s v && v.x == x && v.y == y;

    public override int GetHashCode() => x.GetHashCode() ^ (y.GetHashCode() << 16);

    public static Vector2s operator +(Vector2s a, Vector2s b) =>
        new Vector2s((short)(a.x + b.x), (short)(a.y + b.y));
    public static Vector2s operator -(Vector2s a, Vector2s b) =>
        new Vector2s((short)(a.x - b.x), (short)(a.y - b.y));
    public static bool operator ==(Vector2s a, Vector2s b) => a.x == b.x && a.y == b.y;
    public static bool operator !=(Vector2s a, Vector2s b) => !(a == b);

    public override string ToString() => $"({x}, {y})";
}

public class Heightmap
{
    [System.Flags]
    public enum Biome
    {
        None = 0,
        Meadows = 1,
        Swamp = 2,
        Mountain = 4,
        BlackForest = 8,
        Plains = 16,
        AshLands = 32,
        DeepNorth = 64,
        Ocean = 256,
        Mistlands = 512,
    }

    // --- terrain-modifier surface (RoadTerrainModifier) ---
    // A zone heightmap: m_width vertices per side plus one, m_scale metres
    // per vertex, centred on transform.position. Tests build one per zone
    // with a TerrainComp whose arrays start zeroed, exactly like a fresh
    // _TerrainCompiler in the game.
    // The mask colours are the game's own values, copied from Heightmap in
    // assembly_valheim: the conversion lerps towards exactly these.
    public static UnityEngine.Color m_paintMaskDirt = new(1f, 0f, 0f, 1f);
    public static UnityEngine.Color m_paintMaskCultivated = new(0f, 1f, 0f, 1f);
    public static UnityEngine.Color m_paintMaskPaved = new(0f, 0f, 1f, 1f);
    public static UnityEngine.Color m_paintMaskNothing = new(0f, 0f, 0f, 1f);
    public static UnityEngine.Color m_paintMaskClearVegetation = new(0f, 0f, 0f, 0f);
    public static UnityEngine.Color m_paintMaskDeepSnow = new(1f, 1f, 1f, 1f);
    public static Heightmap? Registered;
    /// <summary>
    /// Every loaded heightmap, by zone. A dedicated server keeps almost none
    /// loaded, and the whole reconciliation question is what happens for a zone
    /// that has none, so the harness has to be able to say which are loaded.
    /// </summary>
    public static readonly System.Collections.Generic.Dictionary<Vector2s, Heightmap> Loaded = new();

    public Transform transform = new();
    public float m_scale = 1f;
    public TerrainComp? m_terrainComp;
    public int PokeCount;
    /// <summary>The generated heights this heightmap was built from; null until built.</summary>
    public HeightmapBuilder.HMBuildData? m_buildData;

    public static Heightmap? FindHeightmap(UnityEngine.Vector3 point) =>
        Loaded.TryGetValue(ZoneSystem.GetZone(point), out var hm) ? hm : Registered;
    public static System.Collections.Generic.List<Heightmap> GetAllHeightmaps() =>
        Loaded.Count > 0
            ? new System.Collections.Generic.List<Heightmap>(Loaded.Values)
            : (Registered == null ? new() : new() { Registered });
    /// <summary>Like the game: the zone's live compiler, or a new one (with a new ZDO) if it has none.</summary>
    public TerrainComp GetAndCreateTerrainCompiler() => m_terrainComp ??= new TerrainComp(this, 64);
    /// <summary>Valheim 1.0: the argument selects which late pass rebuilds
    /// (1 = LateUpdate, 2 = CustomLateUpdate), it is not a frame count.</summary>
    public void Poke(int delayed = 0, bool paintOnly = false) { PokeCount++; LastPokeDelayed = delayed; }
    public int LastPokeDelayed;

    public static Heightmap CreateForZone(Vector2s zoneID, int width = 64, bool withCompiler = true)
    {
        var hm = new Heightmap { m_scale = ZoneSystem.ZoneSize / width };
        hm.transform.position = ZoneSystem.GetZonePos(zoneID);
        if (withCompiler)
            hm.m_terrainComp = new TerrainComp(hm, width);
        return hm;
    }
}

/// <summary>Shim for UnityEngine.Transform: only the position is read.</summary>
public class Transform
{
    public UnityEngine.Vector3 position;
}

/// <summary>Shim for ZNetView: one ZDO behind it, ours unless a test says otherwise.</summary>
public class ZNetView
{
    public ZDO Zdo;
    public ZNetView(ZDO zdo) { Zdo = zdo; }
    public bool IsValid() => Zdo != null;
    public bool IsOwner() => Zdo.IsOwner();
    public bool HasOwner() => Zdo.HasOwner();
    public void ClaimOwnership() { if (!IsOwner()) Zdo.SetOwner(ZDOMan.instance?.m_sessionID ?? 1); }
    public ZDO GetZDO() => Zdo;
}

/// <summary>Mirror of Valheim's ZDOID, as far as the road code prints it.</summary>
public struct ZDOID : System.IEquatable<ZDOID>
{
    public long ID;
    public override string ToString() => ID.ToString();
    public bool Equals(ZDOID other) => ID == other.ID;
    public override bool Equals(object? obj) => obj is ZDOID other && Equals(other);
    public override int GetHashCode() => ID.GetHashCode();
}

/// <summary>
/// Shim for a Valheim ZDO: the typed key/value bag the road code stores its
/// network and per-zone markers in. Only the members the mod calls.
/// </summary>
public class ZDO
{
    private static long s_nextId = 1;

    public ZDOID m_uid = new() { ID = s_nextId++ };
    public bool Persistent;
    private int m_prefab;
    private long m_owner;
    private UnityEngine.Vector3 m_position;
    private readonly System.Collections.Generic.Dictionary<int, int> m_ints = new();
    private readonly System.Collections.Generic.Dictionary<int, long> m_longs = new();
    private readonly System.Collections.Generic.Dictionary<int, byte[]> m_byteArrays = new();
    private readonly System.Collections.Generic.Dictionary<int, string> m_strings = new();

    public ZDO(UnityEngine.Vector3 position, int prefab)
    {
        m_position = position;
        m_prefab = prefab;
    }

    public void SetPrefab(int prefab) => m_prefab = prefab;
    public int GetPrefab() => m_prefab;
    public void SetOwner(long owner) => m_owner = owner;
    public long GetOwner() => m_owner;
    public bool HasOwner() => m_owner != 0;
    /// <summary>Ours when it carries our session id; without a ZDOMan every ZDO counts as ours.</summary>
    public bool IsOwner() => ZDOMan.instance == null || m_owner == ZDOMan.instance.m_sessionID;
    public UnityEngine.Vector3 GetPosition() => m_position;
    public Vector2s GetSector() => ZoneSystem.GetZone(m_position);
    public void SetPosition(UnityEngine.Vector3 position) => m_position = position;
    public UnityEngine.Quaternion GetRotation() => m_rotation;
    public void SetRotation(UnityEngine.Quaternion rotation) => m_rotation = rotation;
    private UnityEngine.Quaternion m_rotation = UnityEngine.Quaternion.identity;

    public void Set(int hash, int value) => m_ints[hash] = value;
    public int GetInt(int hash, int defaultValue = 0) => m_ints.TryGetValue(hash, out int v) ? v : defaultValue;
    public void Set(int hash, long value) => m_longs[hash] = value;
    public long GetLong(int hash, long defaultValue = 0L) => m_longs.TryGetValue(hash, out long v) ? v : defaultValue;
    public void Set(int hash, string value) => m_strings[hash] = value;
    public string GetString(int hash, string defaultValue = "") =>
        m_strings.TryGetValue(hash, out string? v) ? v : defaultValue;
    public void Set(int hash, byte[] value) => m_byteArrays[hash] = value;
    public byte[]? GetByteArray(int hash, byte[]? defaultValue = null) =>
        m_byteArrays.TryGetValue(hash, out var v) ? v : defaultValue;
}

/// <summary>Shim for ZDOMan: the world's ZDOs as a list. Tests create one per world.</summary>
public class ZDOMan
{
    public static ZDOMan? instance;

    public long m_sessionID = 1;
    public readonly System.Collections.Generic.List<ZDO> Zdos = new();

    public ZDO CreateNewZDO(UnityEngine.Vector3 position, int prefabHash)
    {
        var zdo = new ZDO(position, prefabHash);
        Zdos.Add(zdo);
        return zdo;
    }

    /// <summary>Adds every ZDO of the prefab to the list; the real one pages, this one finishes in a single call.</summary>
    public bool GetAllZDOsWithPrefabIterative(string prefab, System.Collections.Generic.List<ZDO> zdos, ref int index)
    {
        int hash = prefab.GetStableHashCode();
        foreach (var zdo in Zdos)
            if (zdo.GetPrefab() == hash)
                zdos.Add(zdo);
        index = Zdos.Count;
        return true;
    }

    /// <summary>
    /// The ZDOs whose position lies in the sector (zone). Valheim 1.0 added the
    /// set of sectors the caller has already visited; it is required here, as it
    /// is in the game, so a caller that forgets it fails to compile rather than
    /// silently passing null. The shim records it and otherwise answers as before.
    /// </summary>
    public void FindObjects(Vector2s sector, System.Collections.Generic.List<ZDO> objects,
        System.Collections.Generic.HashSet<ZoneSystem.SectorIndex> visitedSectorIndices)
    {
        visitedSectorIndices.Add(new ZoneSystem.SectorIndex(sector));
        foreach (var zdo in Zdos)
            if (zdo.GetSector() == sector)
                objects.Add(zdo);
    }

    public int CountWithPrefab(string prefab)
    {
        var found = new System.Collections.Generic.List<ZDO>();
        int index = 0;
        GetAllZDOsWithPrefabIterative(prefab, found, ref index);
        return found.Count;
    }
}

/// <summary>
/// Shim for Valheim's TerrainComp (_TerrainCompiler): the per-vertex arrays
/// the road code writes. (m_width + 1)^2 vertices, row-major, y outer.
/// </summary>
public class TerrainComp
{
    public const string PrefabName = "_TerrainCompiler";

    public int m_width;
    public Heightmap m_hmap;
    public ZNetView m_nview;
    public float[] m_levelDelta;
    public float[] m_smoothDelta;
    public bool[] m_modifiedHeight;
    public UnityEngine.Color[] m_paintMask;
    public bool[] m_modifiedPaint;
    public int SaveCount;

    /// <summary>The zone's live compiler: the one on the registered heightmap, if that heightmap has one.</summary>
    public static TerrainComp? FindTerrainCompiler(UnityEngine.Vector3 pos)
    {
        if (Heightmap.Loaded.TryGetValue(ZoneSystem.GetZone(pos), out var loaded))
            return loaded.m_terrainComp;
        var hm = Heightmap.Registered;
        if (hm?.m_terrainComp == null)
            return null;
        return UnityEngine.Mathf.Abs(hm.transform.position.x - pos.x) < 32f && UnityEngine.Mathf.Abs(hm.transform.position.z - pos.z) < 32f
            ? hm.m_terrainComp : null;
    }

    /// <summary>A new compiler for the heightmap's zone with its own ZDO, registered with the ZDOMan when there is one and owned by us.</summary>
    public TerrainComp(Heightmap hmap, int width)
    {
        m_hmap = hmap;
        m_width = width;
        int prefab = PrefabName.GetStableHashCode();
        ZDO zdo = ZDOMan.instance != null
            ? ZDOMan.instance.CreateNewZDO(hmap.transform.position, prefab)
            : new ZDO(hmap.transform.position, prefab);
        zdo.Persistent = true;
        zdo.SetOwner(ZDOMan.instance?.m_sessionID ?? 1);
        m_nview = new ZNetView(zdo);
        int n = (width + 1) * (width + 1);
        m_levelDelta = new float[n];
        m_smoothDelta = new float[n];
        m_modifiedHeight = new bool[n];
        m_paintMask = new UnityEngine.Color[n];
        m_modifiedPaint = new bool[n];
    }

    /// <summary>
    /// Like the game: only the owner's compiler saves, into the ZDO's TCData.
    ///
    /// The bytes are transcribed from TerrainComp.Save in the decompiled 1.0
    /// assembly -- a version, the operation counter with its point and radius,
    /// then a flag per height vertex with the two deltas for the modified ones,
    /// then a flag per paint texel with its colour. The mod has its own
    /// transcription in TerrainBlob; that they agree is the point, so this one
    /// is written from the game and not from that.
    ///
    /// SaveFails makes the write do nothing while still reporting nothing, which
    /// is what the game does for a compiler this peer does not own.
    /// </summary>
    public bool SaveFails;

    public void Save()
    {
        if (m_nview == null || !m_nview.IsValid() || !m_nview.IsOwner())
            return;
        SaveCount++;
        if (SaveFails)
            return;
        var pkg = new ZPackage();
        pkg.Write(1);
        pkg.Write(Operations);
        pkg.Write(LastOpPoint);
        pkg.Write(LastOpRadius);
        pkg.Write(m_modifiedHeight.Length);
        for (int i = 0; i < m_modifiedHeight.Length; i++)
        {
            pkg.Write(m_modifiedHeight[i]);
            if (m_modifiedHeight[i]) { pkg.Write(m_levelDelta[i]); pkg.Write(m_smoothDelta[i]); }
        }
        pkg.Write(m_modifiedPaint.Length);
        for (int j = 0; j < m_modifiedPaint.Length; j++)
        {
            pkg.Write(m_modifiedPaint[j]);
            if (m_modifiedPaint[j])
            {
                pkg.Write(m_paintMask[j].r); pkg.Write(m_paintMask[j].g);
                pkg.Write(m_paintMask[j].b); pkg.Write(m_paintMask[j].a);
            }
        }
        m_nview.GetZDO().Set(ZDOVars.s_TCData, Utils.Compress(pkg.GetArray()));
    }

    public int Operations;
    public UnityEngine.Vector3 LastOpPoint;
    public float LastOpRadius;
}

/// <summary>Shim for ZDOVars: the ZDO keys the road code reads.</summary>
public static class ZDOVars
{
    public static readonly int s_TCData = "TCData".GetStableHashCode();
    /// <summary>The player who made an object; vanilla sets it on what a player builds.</summary>
    public static readonly int s_creator = "creator".GetStableHashCode();
}

/// <summary>
/// Shim base for Valheim's WorldGenerator exposing only the members the road
/// code calls. Tests subclass this with synthetic terrain.
/// </summary>
public class WorldGenerator
{
    public static WorldGenerator? instance;

    public virtual float GetHeight(float wx, float wy) => 0f;

    public virtual Heightmap.Biome GetBiome(float wx, float wy) => Heightmap.Biome.Meadows;

    public virtual void GetRiverWeight(float wx, float wy, out float weight, out float width)
    {
        weight = 0f;
        width = 0f;
    }

    public virtual int GetSeed() => 0;

    /// <summary>
    /// Valheim's base height is a normalised value where water lies below 0.05
    /// and the terrain height is roughly 200 × base; map the shim's metres onto
    /// that scale so ocean lands where the synthetic world puts it
    /// (sea level 30 m → 0.05).
    /// </summary>
    public virtual float GetBaseHeight(float wx, float wy, bool menuTerrain) =>
        0.05f + (GetHeight(wx, wy) - ShimWorld.SeaLevel) / 200f;

    public virtual float GetBiomeHeight(Heightmap.Biome biome, float wx, float wy, out UnityEngine.Color mask)
    {
        mask = default;
        return GetHeight(wx, wy);
    }
}

/// <summary>
/// Shim for Valheim's ZoneSystem exposing only the members the road code
/// references. GetLocationList returns an empty list unless a test fills it.
/// </summary>
public class ZoneSystem
{
    public const float ZoneSize = 64f;

    /// <summary>Valheim 1.0's index of a zone within the sector tables.</summary>
    public readonly struct SectorIndex : System.IEquatable<SectorIndex>
    {
        public readonly Vector2s Sector;
        public SectorIndex(Vector2s sector) => Sector = sector;
        public bool Equals(SectorIndex other) => Sector == other.Sector;
        public override bool Equals(object? o) => o is SectorIndex s && Equals(s);
        public override int GetHashCode() => Sector.GetHashCode();
    }

    public static ZoneSystem? instance;

    public class ZoneLocation
    {
        public PrefabEntry m_prefab = new();
        public float m_exteriorRadius;

        public class PrefabEntry
        {
            public string Name = "";
        }
    }

    public struct LocationInstance
    {
        public ZoneLocation m_location;
        public UnityEngine.Vector3 m_position;
    }

    public System.Collections.Generic.List<LocationInstance> Locations = new();

    public System.Collections.Generic.List<LocationInstance> GetLocationList() => Locations;

    // ---- locations-generated, as Valheim 1.0 actually behaves ----
    //
    // 1.0 kept the LocationsGenerated property and the GenerateLocationsCompleted
    // event, but changed who writes the backing field. The setter still raises the
    // event (once, then drops the handlers), and subscribing after the fact fires
    // immediately -- but ZoneSystem.Load now writes m_locationsGenerated DIRECTLY
    // from the save package, bypassing the setter. So a world read from disk never
    // raises the event, however early a handler subscribed. Before 1.0 the setter
    // was the only writer and every path raised it.
    //
    // The ordering that matters is the game's own: loading a world sets this from
    // the save DURING the load, before the world's ZDOs are read.
    //
    // The shim models all three doors so a test can tell them apart.

    private bool m_locationsGenerated;
    private System.Action? m_generateLocationsCompleted;

    public bool LocationsGenerated
    {
        get => m_locationsGenerated;
        set
        {
            m_locationsGenerated = value;
            if (!m_locationsGenerated) return;
            m_generateLocationsCompleted?.Invoke();
            m_generateLocationsCompleted = null;
        }
    }

    public event System.Action GenerateLocationsCompleted
    {
        add
        {
            if (m_locationsGenerated) { value?.Invoke(); return; }
            m_generateLocationsCompleted += value;
        }
        remove => m_generateLocationsCompleted -= value;
    }

    /// <summary>
    /// What ZoneSystem.Load does in 1.0: set the flag straight from the save and
    /// raise nothing. This is the door the mod used to be told about and is not.
    /// </summary>
    public void LoadLocationsGeneratedFromSave(bool generated) => m_locationsGenerated = generated;

    // Valheim 1.0 types a zone id as Vector2s, not Vector2i.
    /// <summary>Zones this world has generated. SpawnZone sets it; tests set it directly.</summary>
    public readonly System.Collections.Generic.HashSet<Vector2s> Generated = new();
    public bool IsZoneGenerated(Vector2s zone) => Generated.Contains(zone);

    public static Vector2s GetZone(UnityEngine.Vector3 point) =>
        new(UnityEngine.Mathf.FloorToInt((point.x + ZoneSize / 2f) / ZoneSize),
            UnityEngine.Mathf.FloorToInt((point.z + ZoneSize / 2f) / ZoneSize));

    public static UnityEngine.Vector3 GetZonePos(Vector2s id) =>
        new(id.x * ZoneSize, 0f, id.y * ZoneSize);
}


/// <summary>
/// Shim for Valheim's TerrainModifier. Only the paint types are read: the
/// conversion takes a modifier's settings as a value rather than reading the
/// component, so nothing else of it is needed headlessly. The order matters --
/// it is what the serialised m_paintType integer in a location bundle means.
/// </summary>
public class TerrainModifier
{
    public enum PaintType
    {
        Dirt,
        Cultivate,
        Paved,
        Reset,
        ClearVegetation,
        DeepSnow,
    }

    // The settings LocationTerrainReader copies, with the game's own defaults so
    // a test that leaves one out gets what an unconfigured modifier in Unity
    // gets, not a zero the game never produces.
    public bool enabled = true;
    public int m_sortOrder;
    public bool m_useTerrainCompiler;
    public bool m_playerModifiction;
    public float m_levelOffset;
    public bool m_level;
    public float m_levelRadius = 2f;
    public bool m_square = true;
    public bool m_smooth;
    public float m_smoothRadius = 2f;
    public float m_smoothPower = 3f;
    public bool m_paintCleared = true;
    public bool m_paintHeightCheck;
    public PaintType m_paintType;
    public float m_paintRadius = 2f;
    public float m_paintStrength = 1f;
}

/// <summary>
/// Shim for Valheim's ZPackage: the binary buffer the terrain blob is written
/// into. Only the reads and writes the blob uses, in the same order, so a
/// round trip through it exercises the encoder against the decoder.
/// </summary>
public class ZPackage
{
    private readonly System.Collections.Generic.List<byte> m_write = new();
    private readonly byte[] m_read;
    private int m_pos;

    public ZPackage() { m_read = System.Array.Empty<byte>(); }
    public ZPackage(byte[] data) { m_read = data; }

    public void Write(int v) => m_write.AddRange(System.BitConverter.GetBytes(v));
    public void Write(float v) => m_write.AddRange(System.BitConverter.GetBytes(v));
    public void Write(bool v) => m_write.Add(v ? (byte)1 : (byte)0);
    public void Write(UnityEngine.Vector3 v) { Write(v.x); Write(v.y); Write(v.z); }

    public int ReadInt() { int v = System.BitConverter.ToInt32(m_read, m_pos); m_pos += 4; return v; }
    public float ReadSingle() { float v = System.BitConverter.ToSingle(m_read, m_pos); m_pos += 4; return v; }
    public bool ReadBool() => m_read[m_pos++] != 0;
    public UnityEngine.Vector3 ReadVector3() => new(ReadSingle(), ReadSingle(), ReadSingle());

    public byte[] GetArray() => m_write.ToArray();
}

/// <summary>
/// Shim for Valheim's Utils, for the two calls the terrain blob makes. The
/// game's own compression is not ours to test, so here it is the identity: what
/// the tests exercise is the structure the encoder and decoder agree on.
/// </summary>
public static class Utils
{
    public static byte[] Compress(byte[] data) => data;
    public static byte[] Decompress(byte[] data) => data;
}

/// <summary>
/// Shim for HeightmapBuilder: the generated heights a client builds for itself.
/// Tests register an array per zone; a zone with none is one the builder has not
/// built, which is the case the bridge must refuse rather than convert against
/// zero.
/// </summary>
public class HeightmapBuilder
{
    public class HMBuildData
    {
        public System.Collections.Generic.List<float> m_baseHeights = new();
    }

    public static HeightmapBuilder? instance;

    public readonly System.Collections.Generic.Dictionary<Vector2s, HMBuildData> Built = new();
    public int SyncRequests;

    /// <summary>
    /// Like the game: true only while the entry is in the ready list. A consumed
    /// entry goes back to the build queue, so the NEXT question is false and the
    /// one after that is true again -- which is what makes a double read inside
    /// one pass fail here as it failed in game, without making the harness
    /// brittle for work that comes back later.
    /// </summary>
    public bool IsTerrainReady(UnityEngine.Vector3 centre, int width, float scale, bool distantLod, WorldGenerator gen)
    {
        var zone = ZoneSystem.GetZone(centre);
        if (Built.ContainsKey(zone))
            return true;
        if (Rebuilding.Remove(zone) && Recipes.TryGetValue(zone, out var gen2))
            Build(zone, gen2);          // the builder finished between questions
        return false;
    }

    /// <summary>Zones whose data was consumed and is being built again.</summary>
    public readonly System.Collections.Generic.HashSet<Vector2s> Rebuilding = new();
    /// <summary>The generator each zone was built from, so it can be built again.</summary>
    public readonly System.Collections.Generic.Dictionary<Vector2s, WorldGenerator> Recipes = new();

    /// <summary>
    /// Like the game: handing an entry out REMOVES it from the ready list
    /// (RequestTerrain does m_ready.RemoveAt), so asking twice does not answer
    /// twice. Modelling that is what makes a "can I?" call followed by a "do it"
    /// call fail here as it failed in game.
    /// </summary>
    public HMBuildData? RequestTerrainSync(UnityEngine.Vector3 centre, int width, float scale, bool distantLod, WorldGenerator gen)
    {
        SyncRequests++;
        var zone = ZoneSystem.GetZone(centre);
        if (!Built.TryGetValue(zone, out var d))
            return null;
        Built.Remove(zone);
        Rebuilding.Add(zone);
        return d;
    }

    /// <summary>Build a zone's heights from a generator, the way the game's builder does.</summary>
    public HMBuildData Build(Vector2s zone, WorldGenerator gen, int width = 64, float scale = 1f)
    {
        var centre = ZoneSystem.GetZonePos(zone);
        var data = new HMBuildData();
        int pitch = width + 1;
        for (int y = 0; y < pitch; y++)
            for (int x = 0; x < pitch; x++)
                data.m_baseHeights.Add(gen.GetHeight(
                    centre.x + (x - width / 2) * scale, centre.z + (y - width / 2) * scale));
        Built[zone] = data;
        Recipes[zone] = gen;
        Rebuilding.Remove(zone);
        return data;
    }
}


/// <summary>
/// Shim for ZNet: only the question "is this session id a peer that is here
/// now", which is what tells a stale saved owner from a live one.
/// </summary>
public class ZNet
{
    public static ZNet? instance;
    public readonly System.Collections.Generic.HashSet<long> ConnectedPeers = new();
    public object? GetPeer(long uid) => ConnectedPeers.Contains(uid) ? (object)uid : null;
}
