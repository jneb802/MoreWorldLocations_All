using System.Text.Json;
using MoreWorldLocations.SystemTests;
using Valheim.Testing.Game;
using valheim_cli.Testing;
using Xunit;

public class ScenarioTests
{
    [Fact] public void PaymentChecksFieldsAndSendsOnce()
    {
        var fake = new Fake(new() { ["result"] = "FIXED", ["expectedCost"] = "20", ["paymentSpent"] = "20" });
        using var actor = Actor(fake); var report = new ScenarioReport("payment");
        PortScenarios.Payment(actor, report);
        Assert.True(report.Passed); Assert.Equal(1, fake.Actions);
    }
    [Theory][InlineData("BUG_PRESENT", "20")][InlineData("FIXED", "0")][InlineData("FIXED", "40")]
    public void TransportOkIsNotPaymentSuccess(string verdict, string paid)
    {
        var fake = new Fake(new() { ["result"] = verdict, ["expectedCost"] = "20", ["paymentSpent"] = paid });
        using var actor = Actor(fake); var report = new ScenarioReport("payment");
        Assert.Throws<InvalidOperationException>(() => PortScenarios.Payment(actor, report));
        Assert.False(report.Passed); Assert.Equal(1, fake.Actions);
    }
    [Theory][InlineData(true)][InlineData(false)]
    public void OwnershipChecksActualGateAndLoad(bool owner)
    {
        var fake = new Fake(new() { ["result"] = owner ? "OWNER_ACCESS_OK" : "FIXED", ["hasOwnershipGate"] = "True", ["destinationLoaded"] = "True", ["canAccess"] = owner.ToString(), ["loaded"] = owner.ToString() });
        using var actor = Actor(fake); var report = new ScenarioReport("ownership");
        PortScenarios.Ownership(actor, report, "fixture-shipment", owner); Assert.True(report.Passed);
    }
    [Fact] public void OwnershipTeleportNeverPassesOrAutomaticallyRetries()
    {
        var fake = new Fake(new() { ["retry"] = "true", ["teleported"] = "true" }) { Complete = false };
        using var actor = Actor(fake); var report = new ScenarioReport("ownership");
        Assert.Throws<InvalidOperationException>(() => PortScenarios.Ownership(actor, report, "fixture", false));
        Assert.Equal(1, fake.Actions); Assert.False(report.Passed);
    }
    [Theory][InlineData("True", true)][InlineData("False", false)][InlineData("unknown", false)]
    public void DeliveryRequiresObservedOpenDelivery(string openDelivery, bool succeeds)
    {
        var fake = new Fake(new() { ["result"] = "FIXED", ["loaded"] = "True", ["hasOpenDelivery"] = openDelivery });
        using var actor = Actor(fake); var report = new ScenarioReport("delivery");
        if (succeeds) PortScenarios.Delivery(actor, report);
        else Assert.Throws<InvalidOperationException>(() => PortScenarios.Delivery(actor, report));
        Assert.Equal(succeeds, report.Passed); Assert.Equal(1, fake.Actions);
    }
    private static GameActor Actor(Fake transport)
    { var actor = new GameActor("client", transport); actor.VerifyEnvironment("cli_expect world=disposable plugin=0123456789abcdef0123456789abcdef"); return actor; }
    private sealed class Fake(Dictionary<string, string> fields) : IGameTransport
    {
        public int Actions; public bool Complete = true;
        public CommandResult Execute(string command, TimeSpan timeout)
        {
            string line;
            if (command.StartsWith("cli_expect ")) line = "OK: EXPECT";
            else if (command == "cli_extensions") line = "EXTENSIONS " + JsonSerializer.Serialize(new { apiVersion = 1, extensions = new[] { new { id = "mwl.testing", instance = "a", closing = false, commands = new[] { "payment-probe", "delivery-probe", "ownership-check" }.Select(name => new { name, resultVersion = 1, readOnly = false }) } } });
            else
            {
                Actions++;
                line = "EXTENSION_RESULT " + JsonSerializer.Serialize(new { schemaVersion = 1, ok = true, extension = "mwl.testing", instance = "a", data = new { source = "mwl-port-probe", complete = Complete, fields } });
            }
            return new() { Ok = true, Output = [line] };
        }
        public void Dispose() { }
    }
}
