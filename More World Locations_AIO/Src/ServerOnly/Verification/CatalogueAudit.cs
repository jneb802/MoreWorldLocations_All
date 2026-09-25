using System;
using System.Collections.Generic;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// The sweep: resolve every template this world could place, judge it, and say
/// so.
///
/// <para><b>Why it runs in the game and not beside it.</b> An offline audit of
/// the bundles answers a question about the bundles. What a player meets is the
/// template after Jötunn has resolved it, and resolution adds objects, brings
/// components with them, and changes the terrain — one template gains a second
/// modifier. Anything that decides what ships has to look at the thing that
/// ships, which means it has to run where that thing exists. That is also what
/// makes this repeatable: a build added next month is judged by the same code,
/// in the same place, without anybody checking it by hand first.</para>
///
/// <para>The current resolved-template verdict is the registration authority.
/// There is no shipped name list beside it. Every decision
/// carries its measured content and policy fingerprints for the evidence.</para>
/// </summary>
public static class CatalogueAudit
{
    /// <summary>What the last sweep found, for the console commands. Null until one has run.</summary>
    public static CatalogueReport? Report { get; private set; }

    /// <summary>Where a long sweep says how far it has got. Set by the caller that has a log.</summary>
    public static Action<string>? Progress { get; set; }

    /// <summary>
    /// Say how far the sweep has got, and never let saying it change anything.
    ///
    /// The guard is not defensive habit: adding this progress line was enough to
    /// reintroduce the exact defect R4 was about. A log sink that throws aborted
    /// the audit, the audit's failure registered nothing, and a caller that only
    /// wanted a message had decided what the world contains.
    /// </summary>
    private static void Say(string line)
    {
        try
        {
            Progress?.Invoke(line);
        }
        catch
        {
            // Nothing to report it to: the thing that reports is what failed.
        }
    }

    /// <summary>A new world sweeps again: the templates are reloaded and may resolve differently.</summary>
    internal static void Forget() => Report = null;

    /// <summary>
    /// Publish a report this process did not judge: verdicts a previous start
    /// reached over inputs <see cref="AuditCache"/> has shown to be identical.
    /// The same place <see cref="CatalogueAuditRun.Finish"/> publishes to, so
    /// the console commands cannot tell the two apart — they answer the same
    /// questions either way.
    /// </summary>
    internal static void Adopt(CatalogueReport report) =>
        Report = report ?? throw new ArgumentNullException(nameof(report));

    /// <summary>
    /// Judge every location in <paramref name="definitions"/> and return the
    /// names to keep.
    /// </summary>
    /// <param name="definitions">
    /// Every name the catalogue declares, with its pack — including the ones
    /// this mode does not serve. A report that only covered what was registered
    /// could not answer "why is this location missing", which is the question
    /// anyone actually has.
    /// </param>
    /// <param name="factsOf">
    /// How to read one template's facts. Injected rather than called directly
    /// so that everything deciding anything here stays free of the engine: the
    /// walk needs Unity, the order the names are judged in and what happens
    /// when one of them fails do not.
    /// </param>
    public static CatalogueReport Run(
        IEnumerable<CatalogueSubject> definitions,
        Func<CatalogueSubject, TemplateFacts> factsOf,
        StockPrefabRegistry registry,
        IReadOnlyCollection<string> excludedPacks,
        ComponentPolicy? components = null,
        IReadOnlyCollection<string>? only = null)
    {
        // The loop form of the stepper below: same order, same judgement, same
        // report. It exists for callers that have no frames to spread the work
        // over — a test, or a synchronous resweep.
        CatalogueAuditRun run = Begin(definitions, factsOf, registry, excludedPacks, components, only);
        while (!run.Done)
            run.JudgeNext();
        return run.Finish();
    }

    /// <summary>
    /// Start a sweep that judges one name per <see cref="CatalogueAuditRun.JudgeNext"/>.
    ///
    /// <para>Why a stepper. Judging the whole catalogue in one call held the
    /// game's main thread for the length of the sweep — six minutes on the
    /// station — and gave Unity no frame in which to finish the destruction and
    /// unloading each release had queued, so the sweep's peak was every
    /// template's wreckage at once. A caller with frames judges one name, lets
    /// the frame end, and judges the next; the report at the end is the same
    /// report.</para>
    /// </summary>
    public static CatalogueAuditRun Begin(
        IEnumerable<CatalogueSubject> definitions,
        Func<CatalogueSubject, TemplateFacts> factsOf,
        StockPrefabRegistry registry,
        IReadOnlyCollection<string> excludedPacks,
        ComponentPolicy? components = null,
        IReadOnlyCollection<string>? only = null)
    {
        if (definitions == null) throw new ArgumentNullException(nameof(definitions));
        if (factsOf == null) throw new ArgumentNullException(nameof(factsOf));
        if (registry == null) throw new ArgumentNullException(nameof(registry));
        if (excludedPacks == null) throw new ArgumentNullException(nameof(excludedPacks));

        return new CatalogueAuditRun(
            new List<CatalogueSubject>(definitions), factsOf, registry, excludedPacks,
            components ?? ComponentPolicy.Default, only ?? Array.Empty<string>());
    }

    /// <summary>
    /// One sweep in progress: the names still to judge, the rows judged so far,
    /// and the report once every name has one.
    /// </summary>
    public sealed class CatalogueAuditRun
    {
        private readonly List<CatalogueSubject> _subjects;
        private readonly Func<CatalogueSubject, TemplateFacts> _factsOf;
        private readonly StockPrefabRegistry _registry;
        private readonly IReadOnlyCollection<string> _only;
        private readonly IReadOnlyCollection<string> _excludedPacks;
        private readonly ComponentPolicy _components;
        private readonly string _policyFingerprint;
        private readonly List<CatalogueEntry> _entries = new List<CatalogueEntry>();
        private int _next;

        internal CatalogueAuditRun(
            List<CatalogueSubject> subjects, Func<CatalogueSubject, TemplateFacts> factsOf,
            StockPrefabRegistry registry,
            IReadOnlyCollection<string> excludedPacks, ComponentPolicy components, IReadOnlyCollection<string> only)
        {
            _subjects = subjects;
            _factsOf = factsOf;
            _registry = registry;
            _only = new HashSet<string>(only, StringComparer.Ordinal);
            _excludedPacks = excludedPacks;
            _components = components;
            _policyFingerprint = TemplateFingerprint.OfPolicy(registry, components, excludedPacks);
        }

        /// <summary>How many names the sweep has to judge.</summary>
        public int Total => _subjects.Count;

        /// <summary>The names, in the order they are judged. Exposed for the verdict cache's key.</summary>
        public IReadOnlyList<CatalogueSubject> Subjects => _subjects;

        /// <summary>The rules' digest this run's report will carry.</summary>
        public string PolicyFingerprint => _policyFingerprint;

        /// <summary>The stock snapshot this run checks names against.</summary>
        public StockPrefabRegistry Registry => _registry;

        /// <summary>The stock snapshot's build this run checks names against.</summary>
        public string StockBuildId => _registry.GameBuildId;

        /// <summary>The validation subset, as given.</summary>
        public IReadOnlyCollection<string> Only => _only;

        /// <summary>How many it has judged.</summary>
        public int Judged => _next;

        public bool Done => _next >= _subjects.Count;

        /// <summary>The next name to be judged, or null when there is none.</summary>
        public CatalogueSubject? Next => Done ? (CatalogueSubject?)null : _subjects[_next];

        /// <summary>
        /// Whether judging the next name opens a template. A name with no
        /// definition, or in an excluded pack, is settled from the catalogue
        /// alone and loads nothing.
        /// </summary>
        public bool NextOpensATemplate =>
            !Done && _subjects[_next].SourceDeclared && !IsExcluded(_excludedPacks, _subjects[_next].Pack);

        /// <summary>Judge exactly one name. Opens at most one template.</summary>
        public void JudgeNext()
        {
            if (Done)
                throw new InvalidOperationException("every name has been judged");

            CatalogueSubject subject = _subjects[_next];
            _next++;
            // A sweep over the whole catalogue opens 190-odd templates and takes
            // minutes. Without this it is indistinguishable from a hang, which
            // is what a station run first took it for.
            if (_next % 25 == 0)
                Say($"catalogue audit: {_next} name(s) judged");

            TemplateFacts facts;
            if (!subject.SourceDeclared)
            {
                // An asset with no definition places nothing anywhere. Resolving
                // it would answer a question nobody asked and load 200 MB to do
                // it.
                facts = TemplateFacts.MissingDefinition(subject.Name);
            }
            else if (IsExcluded(_excludedPacks, subject.Pack))
            {
                // Same reasoning: a pack this mode does not serve is settled
                // before any template is opened.
                facts = new TemplateFacts(subject.Name, subject.Pack);
            }
            else
            {
                facts = Read(subject, _factsOf);
            }

            TemplateEvaluation evaluation = TemplatePolicy.Evaluate(facts, _registry, _excludedPacks, _components);
            string fingerprint = TemplateFingerprint.Of(facts);
            bool selected = ServerOnlySelection.AllowsValidated(subject.Name, evaluation.Approved, _only);
            SelectionDecision decision = new SelectionDecision(subject.Name,
                selected ? SelectionOutcome.Registered : SelectionOutcome.NotSelected,
                selected ? "approved by the current resolved-template validator"
                    : evaluation.Approved ? "compatible, but outside this run's validation subset"
                    : $"current validator: {evaluation.Verdict}; " + string.Join(", ", evaluation.Codes));
            _entries.Add(new CatalogueEntry(evaluation, decision, fingerprint));
        }

        /// <summary>The report, once every name has a row. Also published as <see cref="Report"/>.</summary>
        public CatalogueReport Finish()
        {
            if (!Done)
                throw new InvalidOperationException($"{_subjects.Count - _next} name(s) have not been judged");
            var report = new CatalogueReport(_entries, _registry.GameBuildId, _policyFingerprint);
            Report = report;
            return report;
        }
    }

    /// <summary>
    /// One template's facts, with a resolution failure turned into a reason
    /// rather than an exception.
    ///
    /// One template's failure is one template's. Letting it out of here would
    /// stop the sweep and leave every name after it unjudged, which reads in the
    /// log exactly like a catalogue that got smaller.
    /// </summary>
    private static TemplateFacts Read(CatalogueSubject subject, Func<CatalogueSubject, TemplateFacts> factsOf)
    {
        try
        {
            return factsOf(subject) ?? TemplateFacts.Unreadable(
                subject.Name, subject.Pack, "the extractor returned nothing at all");
        }
        catch (Exception ex)
        {
            return TemplateFacts.Unreadable(subject.Name, subject.Pack,
                $"reading the template threw {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static bool IsExcluded(IReadOnlyCollection<string> excludedPacks, string pack)
    {
        foreach (string excluded in excludedPacks)
        {
            if (string.Equals(excluded, pack, StringComparison.Ordinal))
                return true;
        }
        return false;
    }
}

/// <summary>
/// One name to judge: what the catalogue declares about it, before anything is
/// loaded.
///
/// It carries <see cref="SourceDeclared"/> because an asset that exists with no
/// definition is a real entry in the catalogue with a real answer — "nothing
/// places it" — and folding it in with the names that failed a prefab check
/// would report a gap in the definitions as an incompatibility.
/// </summary>
public readonly struct CatalogueSubject
{
    public CatalogueSubject(string name, string pack, bool sourceDeclared = true,
        string interiorPrefabName = "", string dungeonTheme = "")
    {
        Name = name ?? "";
        Pack = pack ?? "";
        SourceDeclared = sourceDeclared;
        InteriorPrefabName = interiorPrefabName ?? "";
        DungeonTheme = dungeonTheme ?? "";
    }

    public string Name { get; }
    public string Pack { get; }
    public bool SourceDeclared { get; }
    public string InteriorPrefabName { get; }
    public string DungeonTheme { get; }
}
