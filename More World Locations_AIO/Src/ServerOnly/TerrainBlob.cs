using System;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>
/// The bytes a <c>TerrainComp</c> keeps in its ZDO, read and written in the
/// game's own format.
///
/// Two things need this. Confirming a write means decoding what was saved and
/// comparing it with what was intended — "a blob exists" is satisfied by
/// anybody's blob, including one that was there before. And repairing a zone
/// that is already generated but not loaded means writing the blob without a
/// live compiler to do it, because a dedicated server keeps almost no zone
/// loaded and the zone's own generation hook will not run a second time.
///
/// The format is <c>TerrainComp.Save</c>'s, and the header is carried through
/// unchanged on a rewrite: the operation counter and its last point drive
/// vanilla's grass reset, and inventing values for them would make a client
/// redraw things this write did not touch.
/// </summary>
public static class TerrainBlob
{
    /// <summary>The header <c>TerrainComp.Save</c> writes before the arrays.</summary>
    public readonly struct Header
    {
        public readonly int Version;
        public readonly int Operations;
        public readonly Vector3 LastPoint;
        public readonly float LastRadius;

        public Header(int version, int operations, Vector3 lastPoint, float lastRadius)
        {
            Version = version;
            Operations = operations;
            LastPoint = lastPoint;
            LastRadius = lastRadius;
        }

        /// <summary>What the game writes for a compiler nothing has operated on.</summary>
        public static Header Fresh => new Header(1, 0, Vector3.zero, 0f);
    }

    /// <summary>
    /// Read a saved package into <paramref name="zone"/>. Returns false with a
    /// reason rather than throwing on anything malformed: a blob this cannot
    /// read is a finding, and the caller has to be able to say so instead of
    /// losing the zone to an exception.
    /// </summary>
    public static bool TryDecode(byte[] saved, TerrainZoneDeltas zone, out Header header, out string problem)
    {
        header = Header.Fresh;
        problem = null;
        if (saved == null) { problem = "there is no saved terrain"; return false; }
        if (zone == null) throw new ArgumentNullException(nameof(zone));

        try
        {
            ZPackage package = new ZPackage(global::Utils.Decompress(saved));
            int version = package.ReadInt();
            int operations = package.ReadInt();
            Vector3 lastPoint = package.ReadVector3();
            float lastRadius = package.ReadSingle();
            header = new Header(version, operations, lastPoint, lastRadius);

            int heights = package.ReadInt();
            if (heights != zone.ModifiedHeight.Length)
            {
                problem = $"it holds {heights} height vertices, not {zone.ModifiedHeight.Length}";
                return false;
            }
            for (int i = 0; i < heights; i++)
            {
                zone.ModifiedHeight[i] = package.ReadBool();
                if (zone.ModifiedHeight[i])
                {
                    zone.LevelDelta[i] = package.ReadSingle();
                    zone.SmoothDelta[i] = package.ReadSingle();
                }
                else
                {
                    zone.LevelDelta[i] = 0f;
                    zone.SmoothDelta[i] = 0f;
                }
            }

            int texels = package.ReadInt();
            if (texels != zone.ModifiedPaint.Length)
            {
                problem = $"it holds {texels} paint texels, not {zone.ModifiedPaint.Length}";
                return false;
            }
            for (int i = 0; i < texels; i++)
            {
                zone.ModifiedPaint[i] = package.ReadBool();
                if (zone.ModifiedPaint[i])
                    zone.PaintMask[i] = new Color(
                        package.ReadSingle(), package.ReadSingle(),
                        package.ReadSingle(), package.ReadSingle());
            }
            return true;
        }
        catch (Exception ex)
        {
            problem = "it could not be decoded: " + ex.GetType().Name + " " + ex.Message;
            return false;
        }
    }

    /// <summary>Write the zone's arrays in the game's format, compressed as it compresses them.</summary>
    public static byte[] Encode(TerrainZoneDeltas zone, Header header)
    {
        if (zone == null) throw new ArgumentNullException(nameof(zone));

        ZPackage package = new ZPackage();
        package.Write(header.Version == 0 ? 1 : header.Version);
        package.Write(header.Operations);
        package.Write(header.LastPoint);
        package.Write(header.LastRadius);

        package.Write(zone.ModifiedHeight.Length);
        for (int i = 0; i < zone.ModifiedHeight.Length; i++)
        {
            package.Write(zone.ModifiedHeight[i]);
            if (zone.ModifiedHeight[i])
            {
                package.Write(zone.LevelDelta[i]);
                package.Write(zone.SmoothDelta[i]);
            }
        }

        package.Write(zone.ModifiedPaint.Length);
        for (int i = 0; i < zone.ModifiedPaint.Length; i++)
        {
            package.Write(zone.ModifiedPaint[i]);
            if (zone.ModifiedPaint[i])
            {
                package.Write(zone.PaintMask[i].r);
                package.Write(zone.PaintMask[i].g);
                package.Write(zone.PaintMask[i].b);
                package.Write(zone.PaintMask[i].a);
            }
        }
        return global::Utils.Compress(package.GetArray());
    }

    /// <summary>
    /// Whether the saved bytes hold exactly the terrain <paramref name="zone"/>
    /// describes. Equality with the intended state, not a changed hash: a write
    /// that alters nothing is still a write that landed.
    /// </summary>
    public static bool Matches(byte[] saved, TerrainZoneDeltas zone, out string mismatch)
    {
        TerrainZoneDeltas readBack = new TerrainZoneDeltas(zone.Origin, zone.Width, zone.Scale);
        if (!TryDecode(saved, readBack, out _, out mismatch))
            return false;

        for (int i = 0; i < zone.ModifiedHeight.Length; i++)
        {
            if (readBack.ModifiedHeight[i] != zone.ModifiedHeight[i]
                || readBack.LevelDelta[i] != zone.LevelDelta[i]
                || readBack.SmoothDelta[i] != zone.SmoothDelta[i])
            {
                mismatch = $"vertex {i} reads modified={readBack.ModifiedHeight[i]} " +
                           $"level={readBack.LevelDelta[i]} smooth={readBack.SmoothDelta[i]}, intended " +
                           $"{zone.ModifiedHeight[i]} {zone.LevelDelta[i]} {zone.SmoothDelta[i]}";
                return false;
            }
        }
        for (int i = 0; i < zone.ModifiedPaint.Length; i++)
        {
            if (readBack.ModifiedPaint[i] != zone.ModifiedPaint[i]
                || (zone.ModifiedPaint[i] && readBack.PaintMask[i] != zone.PaintMask[i]))
            {
                mismatch = $"texel {i} reads painted={readBack.ModifiedPaint[i]} {readBack.PaintMask[i]}, " +
                           $"intended {zone.ModifiedPaint[i]} {zone.PaintMask[i]}";
                return false;
            }
        }
        return true;
    }
}
