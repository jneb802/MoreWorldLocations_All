using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace More_World_Locations_AIO.ServerOnly.Verification;

/// <summary>
/// The engine half of the audit: open each template, hand it to the extractor,
/// and make sure the world ends up holding exactly what the audit approved.
///
/// <para>Thin on purpose, like <see cref="LocationTerrainPatch"/>. Everything
/// that decides anything is in <see cref="CatalogueAudit"/>,
/// <see cref="TemplatePolicy"/> and <see cref="ApprovedSelection"/>, all tested
/// without a world. What lives here is the part that cannot be: where a
/// template comes from, and how to take a location out of the world before it
/// is placed.</para>
///
/// <para><b>Two steps, and the order matters.</b> The audit runs at
/// registration, before any location is added, so an unapproved template is
/// never registered in the first place. The sweep then runs again when the
/// game's location list is complete and withdraws anything the audit did not
/// approve — because registration and the world's list are two different things
/// and only the second one places buildings.</para>
/// </summary>
public static class CatalogueSweep
{
    private static BepInEx.Logging.ManualLogSource Log =>
        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger;

    /// <summary>
    /// A new world: forget the last report, and cancel a sweep in progress —
    /// it was judging templates for a world that no longer exists.
    /// </summary>
    internal static void Forget()
    {
        CatalogueAudit.Forget();
        s_generation++;
        s_run = null;
        State = SweepState.Idle;
        RegistrationReady = false;
        s_registeredReport = null;
        FailureReason = "";
        AuditCache.Recording = null;
        Observe(() => NewWorld?.Invoke());
    }

    /// <summary>Told when a world is forgotten, so an engine hold can forget with it.</summary>
    public static Action? NewWorld { get; set; }

    /// <summary>Where a sweep stands. Registration and generation read this.</summary>
    public enum SweepState
    {
        /// <summary>No sweep has been asked for since the last world.</summary>
        Idle,
        /// <summary>Judging; nothing unverified may be registered or generated.</summary>
        Auditing,
        /// <summary>Finished with a complete report.</summary>
        Done,
        /// <summary>Stopped by a fault. The report is incomplete and nothing was approved.</summary>
        Failed,
        /// <summary>Stopped because the world went away.</summary>
        Cancelled,
    }

    public static SweepState State { get; private set; }

    /// <summary>A complete report has been registered into this world's location map.</summary>
    public static bool RegistrationReady { get; private set; }
    public static string FailureReason { get; private set; } = "";
    public static Func<string>? WorldLoadStatus { get; set; }
    private static CatalogueReport? s_registeredReport;
    private static bool s_diagnostic;
    private static bool s_resweeping;

    /// <summary>"judged/total" while a sweep runs, for the status line.</summary>
    public static string Progress => s_run == null ? "" : $"{s_run.Judged}/{s_run.Total}";

    /// <summary>
    /// Initial generation waits until registration commits. Failed initial
    /// audits remain held; diagnostic resweeps preserve the committed world.
    /// </summary>
    public static bool HoldsGeneration => ServerOnlyMode.Enabled && (!RegistrationReady || GenerationHold.RestartRequired);

    /// <summary>
    /// How a sweep's frames are driven. The engine hands the routine to a
    /// coroutine host; with none (a test, or no host yet) the sweep runs to
    /// completion in the calling frame, which is the old synchronous behaviour.
    /// </summary>
    public static Func<IEnumerator, bool>? ScheduleRoutine { get; set; }

    /// <summary>Let initial loading/generation proceed after registration succeeds.</summary>
    public static Action? ReleaseGeneration { get; set; }

    /// <summary>The longest one template's preload may take before it is opened synchronously anyway.</summary>
    public const float PreloadTimeoutSeconds = 10f;

    /// <summary>The longest the sweep waits for a released template's bundle to go before opening the next.</summary>
    public const float SettleTimeoutSeconds = 3f;
    public const int SettleTimeoutFrames = 300;

    private static CatalogueAudit.CatalogueAuditRun? s_run;
    private static int s_generation;

    /// <summary>The longest one frame spent judging a single name in the last sweep, and which.</summary>
    public static double LongestJudgeMilliseconds { get; private set; }
    public static string LongestJudged { get; private set; } = "";
    private static double s_longestMs;
    private static string s_longestName = "";

    /// <summary>
    /// Judge the whole catalogue in the calling frame, and answer.
    ///
    /// The synchronous form, for a caller with no frames to give. Registration
    /// uses <see cref="BeginAudit"/> instead.
    /// </summary>
    public static CatalogueReport Audit()
    {
        CatalogueReport? result = null;
        Func<IEnumerator, bool>? scheduler = ScheduleRoutine;
        ScheduleRoutine = null;
        try
        {
            if (!BeginAudit(report => result = report, diagnostic: true))
                throw new InvalidOperationException("a sweep is already running");
        }
        finally
        {
            ScheduleRoutine = scheduler;
        }
        return result ?? throw new InvalidOperationException("the sweep did not complete");
    }

    /// <summary>
    /// Judge the whole catalogue, one name per frame, and hand the report to
    /// <paramref name="onComplete"/> when every name has one — or null when
    /// the sweep failed or was cancelled, in which case nothing is approved.
    ///
    /// <para>Called by <c>LocationDB.RegisterAll</c> before it registers
    /// anything, and by a resweep. Nothing here consults the registered set,
    /// which is the point: a template has to be openable without first being
    /// approved, or the audit can only ever confirm what it already
    /// approves.</para>
    ///
    /// <para><b>The barrier.</b> From this call until the report is committed,
    /// <see cref="State"/> is <see cref="SweepState.Auditing"/>: registration
    /// waits for the report, and the engine holds the world's location
    /// generation on <see cref="HoldsGeneration"/>. A wait here is a scheduling
    /// wait; it touches no site's readiness budget because no site exists
    /// yet.</para>
    /// </summary>
    /// <returns>False when a sweep is already running; nothing was started.</returns>
    public static bool BeginAudit(Action<CatalogueReport?> onComplete, bool diagnostic = false)
    {
        if (onComplete == null) throw new ArgumentNullException(nameof(onComplete));
        if (GenerationHold.RefuseFailedProcess())
            return false;
        if (State == SweepState.Auditing)
            return false;

        s_run = CatalogueAudit.Begin(
            Subjects(),
            FactsOf,
            VerificationData.StockPrefabs,
            ServerOnlyAllowlist.ExcludedPacks,
            only: ValidationSwitches.ApprovedForValidation());
        State = SweepState.Auditing;
        FailureReason = "";
        s_diagnostic = diagnostic;
        int token = ++s_generation;
        s_longestMs = 0;
        s_longestName = "";
        CatalogueAudit.Progress = line => Log.LogInfo(line);
        Observe(() => SweepStarted?.Invoke());

        // Only the registration sweep may reuse stored verdicts. A diagnostic
        // sweep, or a resweep retrying a failed registration, was asked for so
        // that the templates would be looked at again; see AuditCache.
        IEnumerator routine = diagnostic || s_resweeping || !AuditCache.Installed
            ? AuditRoutine(s_run, token, onComplete, null)
            : CachedRoutine(s_run, token, onComplete);
        if (ScheduleRoutine == null || !ScheduleRoutine(routine))
        {
            // No frames to spread it over: the old synchronous sweep.
            while (routine.MoveNext())
            {
            }
        }
        return true;
    }

    /// <summary>
    /// The registration sweep with the verdict cache in front of it: reuse what
    /// a previous start concluded when every input is provably the same, and
    /// otherwise the audit, unchanged.
    /// </summary>
    private static IEnumerator CachedRoutine(
        CatalogueAudit.CatalogueAuditRun run, int token, Action<CatalogueReport?> onComplete)
    {
        // One frame first, as the audit always takes. Registration has to land
        // where an audited registration lands: after Jötunn has injected its
        // list into the world. Concluding inside this call would register while
        // that injection is still ahead of us, and it would then visit every
        // location we had just put in the world a second time.
        yield return null;
        if (token != s_generation)
        {
            Conclude(SweepState.Cancelled, null, onComplete, "the world went away before the sweep finished");
            yield break;
        }

        AuditCache.Attempt? attempt = TryCache(run);
        if (attempt?.Reused != null)
        {
            // Nothing is opened: the stored report is what this run's audit
            // would produce, and it concludes exactly as that audit would.
            CatalogueAudit.Adopt(attempt.Reused);
            Conclude(SweepState.Done, attempt.Reused, onComplete, null);
            yield break;
        }
        if (attempt != null && attempt.Reusable.Count > 0)
        {
            // The verdicts that stand are taken in their place in the order;
            // the audit opens only the templates whose inputs changed.
            List<CatalogueEntry> reused = new List<CatalogueEntry>(attempt.Reusable.Count);
            foreach (StoredVerdict verdict in attempt.Reusable.Values)
                reused.Add(verdict.Entry);
            run.Reuse(reused);
        }

        IEnumerator audit = AuditRoutine(run, token, onComplete, attempt);
        while (audit.MoveNext())
            yield return audit.Current;
    }

    /// <summary>The cache's answer, with any failure of its own turned into "audit".</summary>
    private static AuditCache.Attempt? TryCache(CatalogueAudit.CatalogueAuditRun run)
    {
        try
        {
            return AuditCache.Try(run);
        }
        catch (Exception ex)
        {
            Observe(() => Log.LogWarning(
                $"catalogue audit: the verdict cache failed ({ex.GetType().Name}: {ex.Message}); auditing every template"));
            return null;
        }
    }

    private static IEnumerator AuditRoutine(
        CatalogueAudit.CatalogueAuditRun run, int token, Action<CatalogueReport?> onComplete, AuditCache.Attempt? attempt)
    {
        // What this audit reads of the live game is recorded while it reads
        // it, so that the verdicts can be stored with the baseline they were
        // reached against. Only when there is somewhere to store them.
        AuditCache.Recording = attempt?.Record;
        while (!run.Done)
        {
            if (token != s_generation)
            {
                Conclude(SweepState.Cancelled, null, onComplete, "the world went away before the sweep finished");
                yield break;
            }

            bool opens = run.NextOpensATemplate;
            string name = run.Next!.Value.Name;
            // Whatever the live game is asked from here until the next name is
            // this template's: its preload, its mocks, its comparison.
            if (opens)
                attempt?.Record?.Begin(name);

            // 1. Load ahead, off the main thread, and give it a bounded moment.
            ITemplatePreload? preload = opens ? SafePreload(name) : null;
            if (preload != null)
            {
                float deadline = UnityEngine.Time.realtimeSinceStartup + PreloadTimeoutSeconds;
                while (!preload.Loaded && UnityEngine.Time.realtimeSinceStartup < deadline)
                    yield return null;
                if (token != s_generation)
                {
                    preload.Dispose();
                    Conclude(SweepState.Cancelled, null, onComplete, "the world went away before the sweep finished");
                    yield break;
                }
            }

            // 2. Judge one name: open, read, release, in this frame. Timed,
            //    because "one template per frame" is only a bound if the
            //    longest one is known.
            var clock = System.Diagnostics.Stopwatch.StartNew();
            Exception? fault = TryJudge(run);
            clock.Stop();
            if (clock.Elapsed.TotalMilliseconds > s_longestMs)
            {
                s_longestMs = clock.Elapsed.TotalMilliseconds;
                s_longestName = name;
            }
            preload?.Dispose();
            if (fault != null)
            {
                Conclude(SweepState.Failed, null, onComplete,
                    $"judging '{name}' threw {fault.GetType().Name}: {fault.Message}");
                yield break;
            }

            // 3. Let the release land before the next template is opened: the
            //    deferred destruction and the bundle unload run between frames,
            //    and a sweep that never yielded piled every template's wreckage
            //    into one frame. Bounded, because a bundle somebody else holds
            //    is not ours to wait for.
            if (opens)
            {
                float deadline = UnityEngine.Time.realtimeSinceStartup + SettleTimeoutSeconds;
                int frames = 0;
                do
                {
                    yield return null;
                    frames++;
                }
                while (!TemplateAssets.Settled(name) && frames < SettleTimeoutFrames
                       && UnityEngine.Time.realtimeSinceStartup < deadline);
            }
            else
            {
                yield return null;
            }
        }

        if (token != s_generation)
        {
            Conclude(SweepState.Cancelled, null, onComplete, "the world went away before the sweep finished");
            yield break;
        }

        CatalogueReport? report = TryFinish(run, out Exception? finishFault);
        if (report == null)
        {
            Conclude(SweepState.Failed, null, onComplete,
                $"the report could not be assembled: {finishFault?.GetType().Name}: {finishFault?.Message}");
            yield break;
        }

        // Checked and rendered here, at the end of the sweep and before
        // registration releases the world: the baseline is re-signed in the
        // state the verdicts were reached in, not after a world has loaded on
        // top of it. Written only once the report has actually registered.
        AuditCache.Recording = null;
        string? stored = attempt == null ? null : AuditCache.Prepare(attempt, report);
        Conclude(SweepState.Done, report, onComplete, null);
        if (stored != null && token == s_generation && State == SweepState.Done)
            AuditCache.Write(attempt!, report, stored);
    }

    private static ITemplatePreload? SafePreload(string name)
    {
        try
        {
            return TemplateAssets.Preload(name);
        }
        catch (Exception ex)
        {
            // Not a verdict: the synchronous open still happens, and the read
            // decides. A preload that fails only costs the frame the disk.
            Log.LogWarning($"preloading '{name}' threw {ex.GetType().Name}: {ex.Message}; it will be opened synchronously");
            return null;
        }
    }

    private static Exception? TryJudge(CatalogueAudit.CatalogueAuditRun run)
    {
        try
        {
            run.JudgeNext();
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    private static CatalogueReport? TryFinish(CatalogueAudit.CatalogueAuditRun run, out Exception? fault)
    {
        fault = null;
        try
        {
            return run.Finish();
        }
        catch (Exception ex)
        {
            fault = ex;
            return null;
        }
    }

    /// <summary>
    /// End the sweep in one of its three ways, and let the world go on.
    ///
    /// The order matters: the report is announced before the caller registers
    /// against it, so a world that comes up wrong has its explanation above
    /// the symptom in the log; the caller registers before generation is
    /// released, so generation sees the complete list.
    /// </summary>
    private static void Conclude(
        SweepState state, CatalogueReport? report, Action<CatalogueReport?> onComplete, string? why)
    {
        // The signature cache is the AUDIT's, not the world's: it exists to
        // avoid re-walking a stock prefab within one sweep and has no reader
        // afterwards. Cleared however the sweep ended — finished, cancelled
        // or thrown — because "we did not get to the end" is exactly when
        // nobody is left to clear it.
        if (state == SweepState.Cancelled)
        {
            // The world this sweep was judging is gone, and its successor may
            // already be judging its own. Nothing of this one's is touched:
            // not the state, not the caller, not the generation hold.
            Log.LogWarning($"A server-only catalogue sweep was abandoned: {why}.");
            return;
        }

        TemplateFactsExtractor.ForgetStockSignatures();
        CatalogueAudit.Progress = null;
        AuditCache.Recording = null;
        s_run = null;
        LongestJudgeMilliseconds = s_longestMs;
        LongestJudged = s_longestName;

        if (state != SweepState.Done || report == null)
        {
            Fail(why ?? "the audit did not produce a complete report");
            return;
        }

        int token = s_generation;

        // A reused report judged nothing, and "0 ms on" nothing is noise.
        if (s_longestName.Length > 0)
            Observe(() => Log.LogInfo($"catalogue sweep: longest single frame judging one name was {s_longestMs:0} ms on {s_longestName}"));
        Announce(report);
        Observe(() => SweepFinished?.Invoke());

        if (token != s_generation)
            return;

        try
        {
            onComplete(report);
        }
        catch (Exception ex)
        {
            if (token == s_generation)
                Fail($"registration/completion failed: {ex.Message}");
            return;
        }

        if (token != s_generation)
            return;
        State = SweepState.Done;
        if (!s_diagnostic)
        {
            s_registeredReport = report;
            RegistrationReady = true;
            // A diagnostic resweep never reloads the world or changes the
            // committed registration, even if its result differs or it fails.
            Observe(() => ReleaseGeneration?.Invoke());
        }
    }

    private static void Fail(string reason)
    {
        State = SweepState.Failed;
        FailureReason = reason;
        Observe(() => Log.LogError(
            $"Catalogue sweep failed: {reason}. " + (RegistrationReady
                ? "The existing world's registration is unchanged."
                : "World loading and saving remain blocked. Fix the cause, then run mwl_memory resweep to retry registration.")));
    }

    /// <summary>
    /// Audit if this world has not been audited yet, then make its location list
    /// agree with the result.
    ///
    /// The two steps are separate in the ordinary path — the audit runs at
    /// registration, before anything is registered, and enforcement runs when
    /// the game's location list is complete — and this is for a caller that
    /// wants both and does not care which has happened.
    /// </summary>
    public static bool RunOnce()
    {
        if (CatalogueAudit.Report == null)
            Audit();
        return Enforce();
    }

    /// <summary>
    /// Make the world's location list agree with the audit, once the list is
    /// complete.
    ///
    /// <para>Returns false when it could not, which is the case that used to
    /// silently approve everything: the state saying "already swept" was set
    /// before the work, and an exception then left the unverified names exactly
    /// where they were.</para>
    /// </summary>
    public static bool Enforce()
        => Enforce(s_registeredReport ?? CatalogueAudit.Report);

    internal static bool Enforce(CatalogueReport? report)
    {
        // No "already done" flag. It existed to avoid repeating the work, and
        // the work is a dictionary lookup per location; what it actually bought
        // was a second pass that silently did nothing, so a location added after
        // registration would never be looked at again. Running twice is free and
        // catching a late addition is not.
        if (ZoneSystem.instance == null)
            return false;

        if (report == null)
        {
            // Nothing judged this world yet. Withdraw whatever is actually
            // there, which is the honest action, and do NOT mark enforcement
            // done: registration may still be to come. A station run measured
            // this hook firing at the main menu, before RegisterAll had run at
            // all, where "the audit never happened" was true and meant nothing.
            return WithdrawAll("no audit has judged this world yet") == 0;
        }

        var approved = new HashSet<string>(StringComparer.Ordinal);
        foreach (CatalogueEntry entry in report.Entries)
        {
            if (entry.Registered)
                approved.Add(entry.Name);
        }

        foreach (MWLLocation location in LocationDB.All)
        {
            if (approved.Contains(location.Name))
                continue;
            if (Withdraw(location.Name))
            {
                CatalogueEntry? entry = report.Find(location.Name);
                Log.LogWarning(
                    $"{location.Name} was in the world's location list and the audit does not stand behind it; " +
                    "it has been withdrawn before anything was placed. " +
                    (entry == null ? "It is not in the catalogue report at all." : entry.Decision.Reason) +
                    " A world already carrying it keeps what it has: nothing is removed from a save.");
            }
        }

        return true;
    }

    /// <summary>
    /// What this mode is holding, in the terms a memory run is judged in.
    ///
    /// One line, from the process itself, so that an external sampler's private
    /// bytes and this mode's own accounting can be read against each other. A
    /// sampler alone cannot say which owner grew; this alone cannot see the
    /// allocator.
    /// </summary>
    public static string MemoryStatus() =>
        $"sweep {State}{(State == SweepState.Auditing ? " " + Progress : "")}" +
        $"; registration {(RegistrationReady ? "ready" : "not ready")}" +
        (FailureReason.Length == 0 ? "" : $"; failure: {FailureReason}") +
        (WorldLoadStatus == null ? "" : $"; {WorldLoadStatus()}") +
        $"{(LongestJudged.Length > 0 ? $" (longest frame {LongestJudgeMilliseconds:0} ms on {LongestJudged})" : "")}; " +
        $"leases held {TemplateAssets.OutstandingLeases} (peak {TemplateAssets.PeakLeases}); " +
        $"signatures {TemplateFactsExtractor.StockSignatureBytes / 1024} KiB in " +
        $"{TemplateFactsExtractor.StockSignatureEntries} entr(ies) of " +
        $"{TemplateFactsExtractor.StockSignatureBudgetBytes / 1024 / 1024} MiB; " +
        $"heights {LocationTerrainBridge.GeneratedHeightBytes / 1024} KiB in " +
        $"{LocationTerrainBridge.GeneratedHeightEntries} zone(s) " +
        $"({LocationTerrainBridge.GeneratedHeightPins} in use, " +
        $"{LocationTerrainBridge.GeneratedHeightAdopted} adopted outside the table) of " +
        $"{LocationTerrainBridge.GeneratedHeightBudgetBytes / 1024 / 1024} MiB; " +
        $"managed heap {System.GC.GetTotalMemory(false) / 1024 / 1024} MiB; " +
        MockReferenceGuard.Status();

    /// <summary>
    /// Judge the catalogue again in this process.
    ///
    /// <para>Only a measurement uses it. A sweep runs once per world, so the
    /// question "does the second one retain more than the first" cannot be asked
    /// at all without a way to ask for another — and restarting between sweeps
    /// is exactly what would hide an owner that grows.</para>
    /// </summary>
    public static bool Resweep()
    {
        if (GenerationHold.RefuseFailedProcess())
            return false;
        if (State == SweepState.Auditing)
            return false;
        if (!RegistrationReady)
        {
            // A failed initial registration must repeat the real registration
            // callback, not merely produce a fresh diagnostic report. It audits
            // afresh rather than reusing stored verdicts: a resweep is somebody
            // asking for the templates to be looked at again.
            s_resweeping = true;
            try
            {
                LocationDB.RegisterAll();
            }
            finally
            {
                s_resweeping = false;
            }
            return true;
        }
        return BeginAudit(_ => { }, diagnostic: true);
    }

    /// <summary>
    /// Every name a report has to account for: the catalogue's known names and
    /// every definition this build declares, reconciled by identity.
    ///
    /// <para>The union, not either one. A name in the list with no definition is
    /// an asset nothing places, which is a real answer somebody needs; a
    /// definition the list has never heard of is a build added since, which has
    /// to be judged and not skipped.</para>
    /// </summary>
    internal static IEnumerable<CatalogueSubject> Subjects()
    {
        var byName = new Dictionary<string, MWLLocation>(StringComparer.Ordinal);
        foreach (MWLLocation location in LocationDB.All)
            byName[location.Name] = location;

        var emitted = new HashSet<string>(StringComparer.Ordinal);

        // The definitions first, in the packs' declared order, so that the
        // report's order matches the order registration walks.
        foreach (MWLLocation location in LocationDB.All)
        {
            emitted.Add(location.Name);
            yield return new CatalogueSubject(
                location.Name,
                PackOf(location.Name),
                sourceDeclared: true,
                interiorPrefabName: location.InteriorPrefabName ?? "",
                dungeonTheme: location.DungeonTheme ?? "");
        }

        // Then the names the catalogue knows about that no definition declares.
        foreach (CatalogueName known in VerificationData.CatalogueNames.All)
        {
            if (!emitted.Add(known.Name))
                continue;
            yield return new CatalogueSubject(known.Name, known.Pack, sourceDeclared: false);
        }
    }

    /// <summary>
    /// One template's facts, read after Jötunn has resolved it and compared
    /// against the stock prefabs the client would build.
    ///
    /// The handle is released as soon as the walk is done; see
    /// <see cref="ITemplateHandle"/>.
    /// </summary>
    private static TemplateFacts FactsOf(CatalogueSubject subject)
    {
        if (!TemplateAssets.CanLoad)
        {
            return TemplateFacts.Unreadable(subject.Name, subject.Pack,
                "this process has no way to load templates, so nothing was inspected. " +
                "Every verdict in this report is unresolved for that reason and none of them is about a template.");
        }

        TemplateFacts facts;
        using (ITemplateHandle? handle = TemplateAssets.Open(subject.Name))
        {
            if (handle?.Asset == null)
            {
                return TemplateFacts.Unreadable(subject.Name, subject.Pack,
                    "no asset loaded under this exact name. Names are case-sensitive: a definition and an asset that " +
                    "differ only in capitalisation are two names, and one of them places nothing.");
            }

            // Every stock lookup goes through the record when the verdicts are
            // to be stored: those are the live prefabs this verdict depends on,
            // and the next start checks each of them before reusing it.
            StockRecord? record = AuditCache.Recording;
            Func<string, GameObject?>? stockPrefabs = Recorded(TemplateAssets.StockPrefabs, record);

            // Judge what will be placed, not what came off the disk: the
            // registration path applies this same conversion to the asset the
            // location is built from, so a scale the author set is transmitted.
            // Doing it here and not there would approve a scale nobody sends.
            ScaleSyncConversion.Apply(handle.Asset, stockPrefabs);

            facts = TemplateFactsExtractor.Extract(
                subject.Name, subject.Pack, handle.Asset,
                subject.InteriorPrefabName, subject.DungeonTheme,
                stockPrefabs, TemplateAssets.BaselineProvenance);
            record?.Read(facts);
            Observe(() => TemplateRead?.Invoke(subject.Name, handle.Asset));
        }
        Observe(() => TemplateReleased?.Invoke(subject.Name));
        return facts;
    }

    /// <summary>The stock lookup, noting each name it is asked for in <paramref name="record"/>.</summary>
    private static Func<string, GameObject?>? Recorded(Func<string, GameObject?>? lookup, StockRecord? record)
    {
        if (lookup == null || record == null)
            return lookup;
        return name =>
        {
            record.Consulted(name);
            return lookup(name);
        };
    }

    /// <summary>
    /// Where a lifecycle trace watches the sweep from. Set only by a validation
    /// switch; null in a shipped run.
    ///
    /// <para>The three moments a trace needs and cannot get from outside: a
    /// template while the sweep still holds it, the same template the instant
    /// it has been given back, and the end of the sweep. They observe and never
    /// decide: an observer that throws is swallowed the same way a log sink that
    /// throws is, because R4 was exactly a caller that only wanted a message
    /// deciding what the world contains.</para>
    /// </summary>
    public static Action<string, GameObject?>? TemplateRead { get; set; }

    /// <summary>See <see cref="TemplateRead"/>.</summary>
    public static Action<string>? TemplateReleased { get; set; }

    /// <summary>See <see cref="TemplateRead"/>.</summary>
    public static Action? SweepFinished { get; set; }

    /// <summary>See <see cref="TemplateRead"/>: before the first template is opened.</summary>
    public static Action? SweepStarted { get; set; }

    private static void Observe(Action observer)
    {
        try
        {
            observer();
        }
        catch (Exception ex)
        {
            try
            {
                Log.LogWarning($"a lifecycle observer threw and was ignored: {ex.GetType().Name}: {ex.Message}");
            }
            catch
            {
                // The observer is not allowed to change anything, including by failing.
            }
        }
    }

    /// <summary>Which pack a name came from, for scope exclusion.</summary>
    private static string PackOf(string name)
    {
        foreach (KeyValuePair<string, MWLLocation[]> pack in Packs)
        {
            foreach (MWLLocation location in pack.Value)
            {
                if (string.Equals(location.Name, name, StringComparison.Ordinal))
                    return pack.Key;
            }
        }
        return "";
    }

    private static readonly KeyValuePair<string, MWLLocation[]>[] Packs =
    {
        new KeyValuePair<string, MWLLocation[]>("Meadows", LocationDefinitions.Meadows),
        new KeyValuePair<string, MWLLocation[]>("BlackForest", LocationDefinitions.BlackForest),
        new KeyValuePair<string, MWLLocation[]>("Swamp", LocationDefinitions.Swamp),
        new KeyValuePair<string, MWLLocation[]>("Mountains", LocationDefinitions.Mountains),
        new KeyValuePair<string, MWLLocation[]>("Plains", LocationDefinitions.Plains),
        new KeyValuePair<string, MWLLocation[]>("Mistlands", LocationDefinitions.Mistlands),
        new KeyValuePair<string, MWLLocation[]>("Ashlands", LocationDefinitions.Ashlands),
        new KeyValuePair<string, MWLLocation[]>("Ports", LocationDefinitions.Ports),
        new KeyValuePair<string, MWLLocation[]>("Traders", LocationDefinitions.Traders),
        new KeyValuePair<string, MWLLocation[]>("Trainers", LocationDefinitions.Trainers),
        new KeyValuePair<string, MWLLocation[]>("Dungeons", LocationDefinitions.Dungeons),
    };

    /// <summary>
    /// Take one location out of the world's list. True when it was there.
    ///
    /// Nothing is removed from a save: a world that already carries the location
    /// keeps what it has, and this only stops another being placed.
    /// </summary>
    private static bool Withdraw(string name)
    {
        int hash = name.GetStableHashCode();
        if (!ZoneSystem.instance.m_locationsByHash.TryGetValue(hash, out ZoneSystem.ZoneLocation location))
            return false;

        ZoneSystem.instance.m_locationsByHash.Remove(hash);
        ZoneSystem.instance.m_locations.Remove(location);
        ServerOnlySelection.Forget(name);
        return true;
    }

    /// <summary>
    /// Every MWL location out of the world, for the case where the audit did not
    /// happen at all.
    ///
    /// The alternative is falling back to the shipped selection, which is the
    /// very set of unverified names the guard exists to check. An empty world
    /// with a loud reason is the right way for a verification failure to fail.
    /// </summary>
    private static int WithdrawAll(string why)
    {
        int withdrawn = 0;
        foreach (MWLLocation location in LocationDB.All)
        {
            if (Withdraw(location.Name))
                withdrawn++;
        }
        // Loud only when something was actually taken out. Nothing registered is
        // not a failure; it is the ordinary state before registration runs, and
        // an error there teaches an operator to ignore the one that matters.
        if (withdrawn > 0)
        {
            Log.LogError(
                $"Server-only mode withdrew {withdrawn} location(s) from this world: {why}. " +
                "Nothing MWL registers is placed until a sweep has judged it.");
        }
        return withdrawn;
    }

    private static void Announce(CatalogueReport report)
    {
        // Every line is guarded. Reporting is not allowed to decide whether a
        // rejected template stays registered, and an exception on the way out of
        // a log call is exactly how it used to.
        try
        {
            foreach (string line in report.Summary().Split('\n'))
                Log.LogInfo(line);

            foreach (string name in report.ApprovedButNotRegistered())
            {
                // The one case nothing else in a run would mention: the shipped
                // file says this location is in the world and it is not.
                Log.LogWarning($"{name} is approved and was NOT registered: {report.Find(name)!.Decision.Reason}");
            }

            foreach (string problem in VerificationData.LoadProblems)
                Log.LogError($"verification data: {problem}");
        }
        catch (Exception ex)
        {
            try
            {
                Log.LogError($"The catalogue report could not be written out: {ex.GetType().Name}: {ex.Message}");
            }
            catch
            {
                // A logger that cannot log its own failure has nothing left to
                // tell us, and it still must not change what is registered.
            }
        }
    }
}

/// <summary>
/// Where enforcement is driven from.
///
/// <para><c>ZoneSystem.SetupLocations</c> is the game's own "the location list
/// is now complete" moment, and it is where Jötunn adds MWL's locations as
/// well, so this declares that it runs after Jötunn rather than hoping. It is
/// before <c>GenerateLocationsIfNeeded</c>, which is what makes withdrawing a
/// location mean something.</para>
///
/// <para>The audit itself has already run, at registration. This is the step
/// that makes the world agree with it.</para>
/// </summary>
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SetupLocations))]
[HarmonyAfter("com.jotunn.jotunn")]
public static class CatalogueSweepPatch
{
    private static void Postfix()
    {
        if (!ServerOnlyMode.Enabled)
            return;

        try
        {
            CatalogueSweep.Enforce();
        }
        catch (Exception ex)
        {
            // A verification failure must not become approval. Nothing MWL
            // registered is left in the world, and the failure is loud.
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogError(
                $"Server-only verification failed while enforcing the catalogue: {ex}");
            try
            {
                foreach (MWLLocation location in LocationDB.All)
                {
                    int hash = location.Name.GetStableHashCode();
                    if (ZoneSystem.instance != null
                        && ZoneSystem.instance.m_locationsByHash.TryGetValue(hash, out ZoneSystem.ZoneLocation zone))
                    {
                        ZoneSystem.instance.m_locationsByHash.Remove(hash);
                        ZoneSystem.instance.m_locations.Remove(zone);
                        ServerOnlySelection.Forget(location.Name);
                    }
                }
            }
            catch (Exception second)
            {
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogError(
                    $"and the locations could not be withdrawn either: {second}");
            }
        }
    }
}

/// <summary>A new world audits again: the templates are reloaded and may resolve differently.</summary>
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Awake))]
public static class CatalogueSweepReset
{
    private static void Prefix() => CatalogueSweep.Forget();
}
