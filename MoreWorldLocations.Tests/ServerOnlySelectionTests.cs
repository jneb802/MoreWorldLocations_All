using System;
using System.Collections.Generic;
using System.Linq;
using More_World_Locations_AIO.ServerOnly;
using Xunit;

namespace More_World_Locations_AIO.Tests;

/// <summary>
/// Which templates a process registers, and what it says about the choice.
///
/// The rule this protects is <c>ServerOnlyAllowlist.Approved</c> stays empty
/// until a template has been watched on a stock client. A validation run has
/// to be able to register one template anyway, and the danger is that the
/// mechanism for doing so quietly becomes the mechanism for shipping one — so
/// the shipped set is never written to, and a run that used the switch says so
/// in the log.
/// </summary>
public class ServerOnlySelectionTests
{
    private static readonly IReadOnlyCollection<string> Nothing = new HashSet<string>();

    [Fact]
    public void WithNoSwitchTheApprovedSetIsTheShippedOne()
    {
        HashSet<string> approved = ServerOnlySelection.Compose(
            new HashSet<string> { "MWL_Audited" }, Nothing, serverOnly: true);

        Assert.Equal(new[] { "MWL_Audited" }, approved);
    }

    [Fact]
    public void AValidationRunAddsToTheApprovedSet()
    {
        HashSet<string> approved = ServerOnlySelection.Compose(
            Nothing, new HashSet<string> { "MWL_WoodTower2" }, serverOnly: true);

        Assert.Equal(new[] { "MWL_WoodTower2" }, approved);
    }

    [Fact]
    public void ComposingDoesNotWriteToTheShippedSet()
    {
        // The whole point of the switch being per-process: the shipped
        // allowlist is a commit, not something a run can reach.
        var shipped = new HashSet<string>();
        ServerOnlySelection.Compose(shipped, new HashSet<string> { "MWL_WoodTower2" }, serverOnly: true);

        Assert.Empty(shipped);
        // And the shipped allowlist itself is untouched: composing hands back a
        // copy, so a validation run can never add to what ships.
        Assert.DoesNotContain("MWL_WoodTower2", new HashSet<string>(ServerOnlyAllowlist.Approved)
            .Except(new[] { "MWL_MeadowsTomb4", "MWL_Ruins1", "MWL_RuinsWell1", "MWL_WoodTower2" }));
        Assert.Equal(4, ServerOnlyAllowlist.Approved.Count);
    }

    [Fact]
    public void OutsideServerOnlyModeAValidationRequestIsIgnored()
    {
        // Every location registers anyway, so there is nothing to approve. It
        // is ignored rather than remembered, so a later switch to server-only
        // mode does not inherit a set nobody meant.
        HashSet<string> approved = ServerOnlySelection.Compose(
            Nothing, new HashSet<string> { "MWL_WoodTower2" }, serverOnly: false);

        Assert.Empty(approved);
    }

    [Fact]
    public void AValidationRunSaysSo()
    {
        string? notice = ServerOnlySelection.ValidationNotice(
            new HashSet<string> { "MWL_WoodTower2", "MWL_MeadowsTomb4" });

        Assert.NotNull(notice);
        Assert.Contains(ValidationSwitches.ApproveVariable, notice);
        Assert.Contains("MWL_MeadowsTomb4", notice);
        Assert.Contains("MWL_WoodTower2", notice);
        Assert.Contains("validation run", notice);
    }

    [Fact]
    public void AnOrdinaryRunHasNoNoticeToGive()
    {
        Assert.Null(ServerOnlySelection.ValidationNotice(Nothing));
    }

    [Fact]
    public void AnApprovedNameThatMatchedNothingIsNamed()
    {
        // A misspelling registers no location and looks exactly like a
        // location the world placed nowhere. Telling them apart is the
        // difference between fixing a typo and walking to a site.
        IReadOnlyList<string> unmatched = ServerOnlySelection.Unmatched(
            approved: new HashSet<string> { "MWL_WoodTower2", "MWL_WoodTower22" },
            registered: new HashSet<string> { "MWL_WoodTower2" });

        Assert.Equal(new[] { "MWL_WoodTower22" }, unmatched);
    }

    [Fact]
    public void EverythingApprovedAndRegisteredLeavesNothingToReport()
    {
        Assert.Empty(ServerOnlySelection.Unmatched(
            new HashSet<string> { "MWL_WoodTower2" },
            new HashSet<string> { "MWL_WoodTower2" }));
    }

    [Fact]
    public void TheRegisteredNoticeNamesWhatWasRegistered()
    {
        string notice = ServerOnlySelection.RegisteredNotice(
            new HashSet<string> { "MWL_WoodTower2", "MWL_MeadowsTomb4" });

        Assert.Contains("2", notice);
        Assert.Contains("MWL_MeadowsTomb4", notice);
        Assert.Contains("MWL_WoodTower2", notice);
    }

    [Fact]
    public void RegisteringNothingIsStatedRatherThanLeftBlank()
    {
        // An empty allowlist is the correct behaviour for an unaudited build,
        // and it has to be distinguishable in a log from the mode being off.
        Assert.Equal("Server-only mode registered no locations.",
            ServerOnlySelection.RegisteredNotice(new HashSet<string>()));
    }

    // ---- telling our locations from the game's own -------------------------

    [Fact]
    public void AVanillaLocationIsNotOurs()
    {
        // Found in game, not by a test: the first terrain run converted
        // Eikthyrnir, three dolmens, three wood houses and a stone circle
        // alongside MWL_RuinsWell1. A stock client HAS those templates and
        // shapes their ground itself, so the conversion would have been added on
        // top and sunk every one of them a second time.
        ServerOnlySelection.SetRegistered(new[] { "MWL_RuinsWell1" });

        Assert.True(ServerOnlySelection.IsOurs("MWL_RuinsWell1"));
        Assert.False(ServerOnlySelection.IsOurs("Eikthyrnir"));
        Assert.False(ServerOnlySelection.IsOurs("Dolmen01"));
        Assert.False(ServerOnlySelection.IsOurs("StoneCircle"));
    }

    [Fact]
    public void AnMwlTemplateThatWasNotRegisteredIsNotOursEither()
    {
        // Registration is the fact, not the prefix. A template the allowlist
        // refused is one nobody audited, and its ground is not ours to write.
        ServerOnlySelection.SetRegistered(new[] { "MWL_RuinsWell1" });

        Assert.False(ServerOnlySelection.IsOurs("MWL_StoneBeacon1"));
    }

    [Fact]
    public void BeforeRegistrationNothingIsOurs()
    {
        ServerOnlySelection.SetRegistered(new string[0]);

        Assert.False(ServerOnlySelection.IsOurs("MWL_RuinsWell1"));
        Assert.False(ServerOnlySelection.IsOurs(""));
        Assert.False(ServerOnlySelection.IsOurs(null));
    }

    [Fact]
    public void RegisteringAgainReplacesTheSetRatherThanAddingToIt()
    {
        // One process can serve more than one world.
        ServerOnlySelection.SetRegistered(new[] { "MWL_WoodTower2" });
        ServerOnlySelection.SetRegistered(new[] { "MWL_RuinsWell1" });

        Assert.False(ServerOnlySelection.IsOurs("MWL_WoodTower2"));
        Assert.True(ServerOnlySelection.IsOurs("MWL_RuinsWell1"));
    }

    // ---- the switch itself ------------------------------------------------

    [Fact]
    public void NamesAreCommaSeparated()
    {
        Assert.Equal(
            new[] { "MWL_MeadowsTomb4", "MWL_WoodTower2" },
            ValidationSwitches.ParseNames("MWL_WoodTower2,MWL_MeadowsTomb4").OrderBy(n => n));
    }

    [Fact]
    public void SurroundingSpaceIsNotPartOfAName()
    {
        Assert.Equal(new[] { "MWL_WoodTower2" },
            ValidationSwitches.ParseNames("  MWL_WoodTower2  "));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(",")]
    [InlineData("MWL_WoodTower2,")]
    public void AnEmptyEntryIsNotATemplateNamedNothing(string? value)
    {
        // A trailing comma is a typo. Turning it into a template named "" would
        // put an empty string in the approved set and report it as unmatched.
        Assert.DoesNotContain("", ValidationSwitches.ParseNames(value));
    }

    [Fact]
    public void ARepeatedNameIsOneTemplate()
    {
        Assert.Single(ValidationSwitches.ParseNames("MWL_WoodTower2,MWL_WoodTower2"));
    }

    [Theory]
    [InlineData("3,-4", true, 3, -4)]
    [InlineData(" 3 , -4 ", true, 3, -4)]
    [InlineData("3", false, 0, 0)]
    [InlineData("3,-4,5", false, 0, 0)]
    [InlineData("x,y", false, 0, 0)]
    [InlineData("", false, 0, 0)]
    [InlineData(null, false, 0, 0)]
    public void AFaultZoneIsAZoneOrItIsNothing(string? value, bool parsed, int x, int z)
    {
        // A typo that silently faulted zone 0,0 would be worse than no switch.
        Assert.Equal(parsed, ValidationSwitches.TryParseZone(value, out int gx, out int gz));
        Assert.Equal(x, gx);
        Assert.Equal(z, gz);
    }

    [Fact]
    public void TheClockIntervalCanBeLengthenedForOneRun()
    {
        // At five seconds a deliberate failure recovers before the ledger can be
        // read, which leaves it ambiguous whether the clock or a neighbouring
        // zone's generation did it. One run lengthens it to tell them apart.
        string? saved = Environment.GetEnvironmentVariable(ValidationSwitches.TickSecondsVariable);
        try
        {
            Assert.Equal(5f, ValidationSwitches.TickSeconds(5f));

            Environment.SetEnvironmentVariable(ValidationSwitches.TickSecondsVariable, "120");
            Assert.Equal(120f, ValidationSwitches.TickSeconds(5f));

            // Nonsense and non-positive values leave the shipped interval alone.
            foreach (string bad in new[] { "soon", "0", "-3", "" })
            {
                Environment.SetEnvironmentVariable(ValidationSwitches.TickSecondsVariable, bad);
                Assert.Equal(5f, ValidationSwitches.TickSeconds(5f));
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable(ValidationSwitches.TickSecondsVariable, saved);
        }
    }

    [Fact]
    public void TheFaultSwitchLivesInTheEnvironmentToo()
    {
        Assert.Equal("MOREWORLDLOCATIONS_FAULT_ZONE_ONCE", ValidationSwitches.FaultZoneOnceVariable);

        string? saved = Environment.GetEnvironmentVariable(ValidationSwitches.FaultZoneOnceVariable);
        try
        {
            Environment.SetEnvironmentVariable(ValidationSwitches.FaultZoneOnceVariable, "21,-45");
            Assert.True(ValidationSwitches.FaultZoneOnce(out int x, out int z, out int times));
            Assert.Equal((21, -45), (x, z));
            Assert.Equal(1, times);

            // "x,z:n" fails n times. One failure is repaired before a ledger can
            // be read, because zone generation retries constantly while a player
            // moves; more than one is what lets a run SEE the outstanding state.
            Environment.SetEnvironmentVariable(ValidationSwitches.FaultZoneOnceVariable, "21,-45:6");
            Assert.True(ValidationSwitches.FaultZoneOnce(out x, out z, out times));
            Assert.Equal((21, -45), (x, z));
            Assert.Equal(6, times);

            // A count that is not a positive number leaves it at one.
            Environment.SetEnvironmentVariable(ValidationSwitches.FaultZoneOnceVariable, "21,-45:soon");
            Assert.True(ValidationSwitches.FaultZoneOnce(out _, out _, out times));
            Assert.Equal(1, times);

            Environment.SetEnvironmentVariable(ValidationSwitches.FaultZoneOnceVariable, null);
            Assert.False(ValidationSwitches.FaultZoneOnce(out _, out _, out _));
        }
        finally
        {
            Environment.SetEnvironmentVariable(ValidationSwitches.FaultZoneOnceVariable, saved);
        }
    }

    [Fact]
    public void TheEnvironmentIsWhereTheSwitchLives()
    {
        // Named here so the variable cannot be renamed without a test saying
        // so: it is written into station scripts and run notes.
        Assert.Equal("MOREWORLDLOCATIONS_APPROVE", ValidationSwitches.ApproveVariable);

        string? saved = Environment.GetEnvironmentVariable(ValidationSwitches.ApproveVariable);
        try
        {
            Environment.SetEnvironmentVariable(ValidationSwitches.ApproveVariable, "MWL_WoodTower2");
            Assert.Equal(new[] { "MWL_WoodTower2" }, ValidationSwitches.ApprovedForValidation());

            Environment.SetEnvironmentVariable(ValidationSwitches.ApproveVariable, null);
            Assert.Empty(ValidationSwitches.ApprovedForValidation());
        }
        finally
        {
            Environment.SetEnvironmentVariable(ValidationSwitches.ApproveVariable, saved);
        }
    }
}
