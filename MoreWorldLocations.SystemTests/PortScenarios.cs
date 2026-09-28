using System.Globalization;
using System.Text.Json;
using Valheim.Testing.Game;

namespace MoreWorldLocations.SystemTests;

// Run only on disposable full-MWL worlds with a local player at a prepared port.
// Each operation is sent ONCE. A retry/teleport response does not pass or get auto-replayed.
public static class PortScenarios
{
    public static void Payment(GameActor client, ScenarioReport report, string item = "Wood", int count = 10)
    {
        report.Step("charge shipping cost exactly once", () =>
        {
            var fields = Fields(client.Invoke(client.RequireCapability("mwl.testing/payment-probe"), item, count.ToString(CultureInfo.InvariantCulture)));
            Expect(fields, "result", "FIXED");
            int expected = Integer(fields, "expectedCost"), spent = Integer(fields, "paymentSpent");
            if (expected <= 0 || spent != expected) throw new InvalidOperationException("Shipping payment differs from expected positive cost.");
        });
    }
    public static void Delivery(GameActor client, ScenarioReport report, string item = "Wood", int count = 10)
    {
        report.Step("delivery loads the shipment and opens the chest", () =>
        {
            var fields = Fields(client.Invoke(client.RequireCapability("mwl.testing/delivery-probe"), item, count.ToString(CultureInfo.InvariantCulture)));
            Expect(fields, "result", "FIXED"); Expect(fields, "loaded", "True");
            Expect(fields, "hasOpenDelivery", "True");
        });
    }
    public static void Ownership(GameActor client, ScenarioReport report, string shipmentId, bool owner)
    {
        report.Step(owner ? "owner can open delivery" : "non-owner cannot open delivery", () =>
        {
            var fields = Fields(client.Invoke(client.RequireCapability("mwl.testing/ownership-check"), shipmentId, owner ? "allowed" : "blocked"));
            Expect(fields, "result", owner ? "OWNER_ACCESS_OK" : "FIXED");
            Expect(fields, "hasOwnershipGate", "True"); Expect(fields, "destinationLoaded", "True");
            Expect(fields, "canAccess", owner.ToString()); Expect(fields, "loaded", owner.ToString());
        });
    }
    private static JsonElement Fields(JsonElement reply)
    {
        if (reply.GetProperty("source").GetString() != "mwl-port-probe" || !reply.GetProperty("complete").GetBoolean())
            throw new InvalidOperationException("MWL probe incomplete; arrange the required player/port before rerunning explicitly.");
        return reply.GetProperty("fields");
    }
    private static void Expect(JsonElement fields, string name, string expected)
    {
        if (!fields.TryGetProperty(name, out var actual) || !string.Equals(actual.GetString(), expected, StringComparison.Ordinal))
            throw new InvalidOperationException("MWL assertion failed for " + name + "; expected " + expected);
    }
    private static int Integer(JsonElement fields, string name) => int.Parse(fields.GetProperty(name).GetString()!, CultureInfo.InvariantCulture);
}
