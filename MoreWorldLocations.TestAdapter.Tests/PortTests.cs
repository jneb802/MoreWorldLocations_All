using System.Globalization;
using MoreWorldLocations.TestAdapter;
using Xunit;

public class PortTests
{
    [Fact] public void AllSevenLegacyNamesHaveOneOwner()
    {
        Assert.Equal(7, PortCommandCatalog.Commands.Count);
        Assert.Equal(7, PortCommandCatalog.Commands.Select(x => x.Alias).Distinct().Count());
        Assert.All(PortCommandCatalog.Commands, x => Assert.StartsWith("cli_mwl_", x.Alias));
    }
    [Fact] public void OnlyStatusIsReadOnly()
    {
        Assert.Equal(new[] { "port-status" }, PortCommandCatalog.Commands.Where(x => x.ReadOnly).Select(x => x.Name));
        Assert.True(PortCommandCatalog.Commands.Single(x => x.Name == "clear-shipments").ServerOnly);
        Assert.True(PortCommandCatalog.Commands.Single(x => x.Name == "ownership-check").ClientOnly);
    }
    [Theory][InlineData("payment-probe")][InlineData("delivery-probe")][InlineData("ownership-seed")]
    public void LegacyDefaultsArePreserved(string name)
    {
        var request = PortCommandCatalog.Parse(name, []);
        Assert.Equal("Wood", request.Item); Assert.Equal(10, request.Count);
    }
    [Theory][InlineData("NaN")][InlineData("Infinity")][InlineData("-1")][InlineData("0")][InlineData("10001")][InlineData("1,5")]
    public void InvalidRadiusIsNotSilentlyZero(string radius) => Assert.Throws<ArgumentException>(() => PortCommandCatalog.Parse("port-status", [radius]));
    [Fact] public void RadiusParsingIsInvariant()
    {
        var old = CultureInfo.CurrentCulture;
        try { CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR"); Assert.Equal(1.5f, PortCommandCatalog.Parse("port-status", ["1.5"]).Radius); }
        finally { CultureInfo.CurrentCulture = old; }
    }
    [Theory][InlineData("0")][InlineData("-1")][InlineData("10001")][InlineData("garbage")]
    public void BadItemCountDoesNotMutate(string count) => Assert.Throws<ArgumentException>(() => PortCommandCatalog.Parse("payment-probe", ["Wood", count]));
    [Fact] public void ExtraArgumentsAreRejected() => Assert.Throws<ArgumentException>(() => PortCommandCatalog.Parse("clear-shipments", ["all"]));
    [Fact] public void BadOwnershipExpectationIsNotSilentlyBlocked() => Assert.Throws<ArgumentException>(() => PortCommandCatalog.Parse("ownership-check", ["id", "alowed"]));
    [Fact] public void QuotedNamesDoNotHideResultFields()
    {
        var reply = new PortReply(["OK: MWL_PAYMENT_REGRESSION result=FIXED port='Port With Spaces' expectedCost=20 paymentSpent=20"]);
        Assert.Equal("Port With Spaces", reply.Values["port"]); Assert.Equal("20", reply.Values["paymentSpent"]);
    }
    [Fact] public void BugPresentIsAResultNotAutomaticallyAPassingTest()
    { Assert.Equal("BUG_PRESENT", new PortReply(["OK: MWL_DELIVERY_REGRESSION result=BUG_PRESENT"]).Values["result"]); }
    [Fact] public void RetryIsIncomplete() => Assert.False(new PortReply(["OK: MWL_OWNERSHIP_CHECK retry=true teleported=true"]).Complete);
    [Fact] public void ExistingTeleportReplyRemainsRecognized()
    { Assert.Equal("2", new PortReply(["OK: Teleported to MWL port index=2 totalPorts=3 pos=(1,2,3)"]).Values["index"]); }
    [Fact] public void MissingCollectionsRemainIncomplete()
    { Assert.False(new PortReply(["OK: MWL_PORT_STATUS shipments=-1 manifests=2"]).Complete); }
    [Fact] public void SilenceFails() => Assert.Throws<InvalidOperationException>(() => new PortReply([]));
    [Fact] public void ErrorCannotBeRescuedByLaterOk() => Assert.Throws<InvalidOperationException>(() => new PortReply(["ERROR: failed", "OK: MWL_PORT_STATUS loadedPorts=1"]));
    [Fact] public void DuplicateResultFieldsFail() => Assert.Throws<InvalidOperationException>(() => new PortReply(["OK: MWL_PAYMENT_REGRESSION result=BUG_PRESENT result=FIXED"]));
}
