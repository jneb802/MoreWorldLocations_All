using System;
using System.Collections.Generic;
using System.Globalization;

namespace More_World_Locations_AIO.ServerOnly;

/// <summary>What a site may do, decided before anything of it is published.</summary>
public enum SiteVerdict
{
    /// <summary>Every zone the site shapes can be served as the author drew it.</summary>
    Serve,

    /// <summary>This particular placement cannot be served. The site is not published at all.</summary>
    Refuse,

    /// <summary>
    /// Nothing could be established either way, so nothing is published yet.
    ///
    /// <para>This used to publish, on the reasoning that "we could not look" is
    /// not "the ground is wrong" and refusing would delete locations for a data
    /// availability reason. Both halves are true and the conclusion was wrong:
    /// if the ground later turns out to be contested or beyond the compiler's
    /// range, the buildings are already standing on it and the whole gap is
    /// back. Writing "unchecked" in a log makes the evidence honest; it does not
    /// make the site serveable.</para>
    ///
    /// <para>The placement is held instead — see
    /// <see cref="ZoneReadinessBarrier"/>, which keeps the zone ungenerated
    /// until the ground can be read, the way vanilla itself holds a zone whose
    /// terrain is not ready. Nothing is deleted and nothing is published on a
    /// guess.</para>
    /// </summary>
    Undecided,
}

/// <summary>One placement's answer, with the reason attached rather than inferred.</summary>
public sealed class SiteDecision
{
    public SiteDecision(SiteVerdict verdict, string code, string reason)
    {
        Verdict = verdict;
        Code = code ?? "";
        Reason = reason ?? "";
    }

    public SiteVerdict Verdict { get; }

    /// <summary>A stable reason code, so refusals can be counted and compared between runs.</summary>
    public string Code { get; }

    public string Reason { get; }

    /// <summary>
    /// Whether the site may be published. Only a decided, servable site may.
    ///
    /// An undecided one is held, not published: the point of asking before the
    /// buildings exist is lost the moment an unanswered question publishes them
    /// anyway.
    /// </summary>
    public bool MayPublish => Verdict == SiteVerdict.Serve;

    public bool Serves => Verdict == SiteVerdict.Serve;

    public static SiteDecision Serve { get; } = new SiteDecision(SiteVerdict.Serve, "", "every zone can be served");
}

/// <summary>The reasons a particular placement cannot be served. Site-specific, never template-wide.</summary>
public static class SiteRefusalCodes
{
    /// <summary>The site shapes ground more than one zone from its own, which the per-zone conversion cannot see.</summary>
    public const string OutOfReach = "site_out_of_reach";

    /// <summary>A vertex needs a height change the compiler cannot express; the ±8 m clamp.</summary>
    public const string Unrepresentable = "site_unrepresentable";

    /// <summary>Another writer has already moved ground this site needs — a road, or an overlapping site.</summary>
    public const string Contested = "site_contested";

    /// <summary>The generated ground for one of the zones could not be read, so nothing can be decided.</summary>
    public const string GroundUnknown = "site_ground_unknown";

    /// <summary>The template's own terrain could not be read, so what the site would do to the ground is unknown.</summary>
    public const string TemplateUnreadable = "site_template_unreadable";

    /// <summary>The check itself failed. The site is withheld rather than published unchecked.</summary>
    public const string CheckFailed = "site_check_failed";

    /// <summary>The ground never became readable within the bound, so the placement is given up rather than held for ever.</summary>
    public const string NeverReadable = "site_never_readable";
}

/// <summary>
/// Whether a placement can be served, asked BEFORE its objects exist.
///
/// <para><b>Why before.</b> The conversion used to run from a postfix on
/// <c>PlaceLocations</c>, finding its sites by reading the location proxies —
/// which exist only once <c>SpawnLocation</c> has run and every networked child
/// already has a ZDO. So a refusal always came after the buildings had been
/// published, and a site the conversion would not serve was left standing on
/// the ground the client generates for itself. A faithful site or no site: a
/// stairway hanging two metres above a hillside is not a third option.</para>
///
/// <para><b>It reuses the conversion rather than predicting it.</b> The question
/// "can this be served" and the question "what does serving it produce" have to
/// have the same answer, so this runs the real
/// <see cref="TerrainConversion.ApplyOnce"/> — over a copy it makes itself, so
/// that asking changes nothing. A second implementation would be a second set
/// of rules, and the two would disagree exactly where it mattered.</para>
///
/// <para><b>It is about the placement, never the template.</b> A template that
/// converts perfectly well can still meet a hillside it cannot be cut into.
/// That is what these codes record, by site, and it is deliberately separate
/// from <c>TemplatePolicy</c>, which asks whether the build itself is
/// serveable anywhere.</para>
/// </summary>
public static class SitePreflight
{
    /// <summary>Everything one zone can tell the preflight, or null when it cannot be read.</summary>
    /// <param name="zone">The zone being asked about.</param>
    /// <param name="deltas">
    /// What that zone's compiler already holds, so another writer's ground is
    /// visible. A zone with no compiler yet is an empty set of deltas, not a
    /// missing answer.
    /// </param>
    /// <param name="baseHeightAt">
    /// The ground the client generates for itself. Null means it could not be
    /// read, and the site is refused rather than converted against nothing.
    /// </param>
    public delegate bool ZoneGround(
        Vector2s zone, out TerrainZoneDeltas deltas, out TerrainConversion.VertexHeight baseHeightAt);

    /// <summary>
    /// Decide one placement.
    /// </summary>
    /// <param name="siteId">The identity a refusal is recorded under.</param>
    /// <param name="home">The zone the location itself is in.</param>
    /// <param name="operations">The site's terrain, already placed in world space.</param>
    /// <param name="groundOf">Where each touched zone's state comes from.</param>
    public static SiteDecision Decide(
        string siteId, Vector2s home,
        IReadOnlyList<LocationTerrainOperation> operations,
        ZoneGround groundOf)
    {
        if (operations == null) throw new ArgumentNullException(nameof(operations));
        if (groundOf == null) throw new ArgumentNullException(nameof(groundOf));

        // No terrain at all is nothing to refuse. Most templates are this.
        if (operations.Count == 0)
            return SiteDecision.Serve;

        List<Vector2s> touched = LocationTerrainReader.ZonesTouched(operations);

        // The per-zone scheme works because two neighbouring zones each find the
        // other's proxy. A site reaching two zones away would be found by the
        // near neighbour and not the far one, and the far one would keep the
        // ground the client generated -- a step through the middle of the site.
        foreach (Vector2s zone in touched)
        {
            if (Math.Abs(zone.x - home.x) <= 1 && Math.Abs(zone.y - home.y) <= 1)
                continue;
            return new SiteDecision(SiteVerdict.Refuse, SiteRefusalCodes.OutOfReach,
                string.Format(CultureInfo.InvariantCulture,
                    "it shapes ground in zone {0},{1}, more than one zone from its own {2},{3}. " +
                    "Converting only the zones that can see it would leave a step across the site, " +
                    "so no part of it is placed.",
                    zone.x, zone.y, home.x, home.y));
        }

        foreach (Vector2s zone in touched)
        {
            if (!groundOf(zone, out TerrainZoneDeltas deltas, out TerrainConversion.VertexHeight baseHeightAt)
                || deltas == null || baseHeightAt == null)
            {
                // Undecided: held, not published and not deleted. The zone stays
                // ungenerated until the ground can be read -- the same thing
                // vanilla does when its own terrain is not ready yet.
                return new SiteDecision(SiteVerdict.Undecided, SiteRefusalCodes.GroundUnknown,
                    string.Format(CultureInfo.InvariantCulture,
                        "the ground the client generates for zone {0},{1} could not be read, so nothing is " +
                        "established about this placement either way. It is not placed: the zone waits until " +
                        "the ground can be read, and publishing on an unanswered question is exactly the " +
                        "outcome this check exists to prevent.",
                        zone.x, zone.y));
            }

            // The real conversion, over a copy this makes itself.
            //
            // ApplyOnce clones internally to convert, but on SUCCESS it adopts
            // the result into the zone it was given and records the site as
            // applied there. That is right for the write and wrong for a
            // question: a preflight that left its answer behind would have the
            // later, real write find the site already recorded and skip it, and
            // the ground would never be shaped at all.
            TerrainZoneDeltas asked = deltas.Clone();
            TerrainConversion.ApplyOnce(siteId, operations, asked, baseHeightAt, out TerrainConversionResult result);

            if (!result.Representable)
            {
                return new SiteDecision(SiteVerdict.Refuse, SiteRefusalCodes.Unrepresentable,
                    string.Format(CultureInfo.InvariantCulture,
                        "in zone {0},{1} it needs ground a terrain compiler cannot express — {2}. " +
                        "A compiler delta is clamped to ±8 m and the authored path is not, so the site would stand " +
                        "on ground the author never drew.",
                        zone.x, zone.y, First(result.BeyondCompilerRange)));
            }

            if (result.ContestedVertices != null && result.ContestedVertices.Count > 0)
            {
                return new SiteDecision(SiteVerdict.Refuse, SiteRefusalCodes.Contested,
                    string.Format(CultureInfo.InvariantCulture,
                        "in zone {0},{1} another writer has already moved ground it needs — {2}, and {3} vertex(es) " +
                        "in all. Two writers on one vertex is a planning question, not something to merge quietly.",
                        zone.x, zone.y, First(result.ContestedVertices), result.ContestedVertices.Count));
            }
        }

        return SiteDecision.Serve;
    }

    private static string First(IReadOnlyList<string> values) =>
        values == null || values.Count == 0 ? "(no detail)" : values[0];
}
