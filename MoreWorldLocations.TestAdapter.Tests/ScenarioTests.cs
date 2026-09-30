using MoreWorldLocations.SystemTests;
using Valheim.Testing.Game;
using Valheim.Testing.Game.Fakes;
using Xunit;

public class ScenarioTests
{
    [Fact] public void PaymentChecksFieldsAndSendsOnce()
    {
        var game = Probe(new() { ["result"] = "FIXED", ["expectedCost"] = "20", ["paymentSpent"] = "20" });
        using var actor = Actor(game); var report = new ScenarioReport("payment");
        PortScenarios.Payment(actor, report);
        Assert.True(report.Passed); Assert.Equal(1, Actions(game));
    }
    [Theory][InlineData("BUG_PRESENT", "20")][InlineData("FIXED", "0")][InlineData("FIXED", "40")]
    public void TransportOkIsNotPaymentSuccess(string verdict, string paid)
    {
        var game = Probe(new() { ["result"] = verdict, ["expectedCost"] = "20", ["paymentSpent"] = paid });
        using var actor = Actor(game); var report = new ScenarioReport("payment");
        Assert.Throws<InvalidOperationException>(() => PortScenarios.Payment(actor, report));
        Assert.False(report.Passed); Assert.Equal(1, Actions(game));
    }
    [Theory][InlineData(true)][InlineData(false)]
    public void OwnershipChecksActualGateAndLoad(bool owner)
    {
        var game = Probe(new() { ["result"] = owner ? "OWNER_ACCESS_OK" : "FIXED", ["hasOwnershipGate"] = "True", ["destinationLoaded"] = "True", ["canAccess"] = owner.ToString(), ["loaded"] = owner.ToString() });
        using var actor = Actor(game); var report = new ScenarioReport("ownership");
        PortScenarios.Ownership(actor, report, "fixture-shipment", owner); Assert.True(report.Passed);
    }
    [Fact] public void OwnershipTeleportNeverPassesOrAutomaticallyRetries()
    {
        var game = Probe(new() { ["retry"] = "true", ["teleported"] = "true" }, complete: false);
        using var actor = Actor(game); var report = new ScenarioReport("ownership");
        Assert.Throws<InvalidOperationException>(() => PortScenarios.Ownership(actor, report, "fixture", false));
        Assert.Equal(1, Actions(game)); Assert.False(report.Passed);
    }
    [Theory][InlineData("True", true)][InlineData("False", false)][InlineData("unknown", false)]
    public void DeliveryRequiresObservedOpenDelivery(string openDelivery, bool succeeds)
    {
        var game = Probe(new() { ["result"] = "FIXED", ["loaded"] = "True", ["hasOpenDelivery"] = openDelivery });
        using var actor = Actor(game); var report = new ScenarioReport("delivery");
        if (succeeds) PortScenarios.Delivery(actor, report);
        else Assert.Throws<InvalidOperationException>(() => PortScenarios.Delivery(actor, report));
        Assert.Equal(succeeds, report.Passed); Assert.Equal(1, Actions(game));
    }

    // The adapter's three probes, each answering with these fields as the port probe's reply. The scripted transport
    // throws on any command it was not given, so a scenario cannot send one its test did not expect.
    private static ScriptedTransport Probe(Dictionary<string, string> fields, bool complete = true)
    {
        var game = new ScriptedTransport();
        foreach (string probe in new[] { "payment-probe", "delivery-probe", "ownership-check" })
            game.Extension("mwl.testing", probe, _ => new { source = "mwl-port-probe", complete, fields }, readOnly: false);
        return game;
    }
    private static GameActor Actor(ScriptedTransport game) =>
        game.Actor("client", "cli_expect world=disposable plugin=0123456789abcdef0123456789abcdef");
    // Probe invocations only: not the pin checks and capability listings around them.
    private static int Actions(ScriptedTransport game) => game.Count("cli_extension");
}
