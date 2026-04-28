using System;
using UnityEngine;

namespace More_World_Locations_AIO.Dungeons.Packs;

[Flags]
public enum StripFlags
{
    None = 0,
    Piece = 1 << 0,
    WearNTear = 1 << 1,
    ZNetView = 1 << 2,
    StaticPhysics = 1 << 3,
    PieceWNTZNV = Piece | WearNTear | ZNetView,
}

public class KitClone
{
    public string Name { get; set; }
    public string VanillaSource { get; set; }
    public StripFlags Strip { get; set; } = StripFlags.None;
    public Action<GameObject> Configure { get; set; }
}

public class KitSpawner
{
    public string Name { get; set; }
    public string CreatureListName { get; set; }
}

public class KitContainer
{
    public string Name { get; set; }
    public string LootListName { get; set; }
    public Heightmap.Biome Biome { get; set; }
}

public enum CustomPrefabSource
{
    VanillaClone,
    Bundled,
}

public class CustomPrefabSpec
{
    public string Name { get; set; }
    public CustomPrefabSource Source { get; set; }
    public string VanillaSource { get; set; }
    public string BundleAsset { get; set; }
    public StripFlags Strip { get; set; } = StripFlags.None;
    public Action<GameObject> Configure { get; set; }
}

public class MWLDungeonPack
{
    public string Name { get; set; }
    public string ThemeName { get; set; }
    public string ExteriorLocationName { get; set; }
    public Func<AssetBundle> Bundle { get; set; }
    public KitClone[] KitClones { get; set; } = Array.Empty<KitClone>();

    public CustomPrefabSpec[] CustomPrefabs { get; set; } = Array.Empty<CustomPrefabSpec>();

    public CustomPrefabSpec DungeonGeneratorPrefab { get; set; }
}
