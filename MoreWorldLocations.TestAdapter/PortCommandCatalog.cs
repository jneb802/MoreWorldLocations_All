using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MoreWorldLocations.TestAdapter;

// Shared by the adapter and its fast tests. This file has no Unity or CLI dependency.
public sealed class PortCommandSpec
{
    public string Name { get; }
    public string Alias { get; }
    public bool ReadOnly { get; }
    public bool ClientOnly { get; }
    public bool ServerOnly { get; }
    internal PortCommandSpec(string name, string alias, bool readOnly = false, bool clientOnly = true, bool serverOnly = false)
    { Name = name; Alias = alias; ReadOnly = readOnly; ClientOnly = clientOnly; ServerOnly = serverOnly; }
}
public sealed class PortRequest
{
    public PortCommandSpec Spec { get; }
    public float Radius { get; internal set; } = 80;
    public int Index { get; internal set; }
    public string Item { get; internal set; } = "Wood";
    public int Count { get; internal set; } = 10;
    public string ShipmentId { get; internal set; } = "";
    public string Expectation { get; internal set; } = "blocked";
    internal PortRequest(PortCommandSpec spec) { Spec = spec; }
}
public static class PortCommandCatalog
{
    public static IReadOnlyList<PortCommandSpec> Commands { get; } = Array.AsReadOnly(new[]
    {
        new PortCommandSpec("port-status", "cli_mwl_port_status", readOnly: true, clientOnly: false),
        new PortCommandSpec("goto-port", "cli_mwl_goto_port"),
        new PortCommandSpec("clear-shipments", "cli_mwl_clear_shipments", clientOnly: false, serverOnly: true),
        new PortCommandSpec("payment-probe", "cli_mwl_port_payment_regression"),
        new PortCommandSpec("delivery-probe", "cli_mwl_port_delivery_regression"),
        new PortCommandSpec("ownership-seed", "cli_mwl_port_ownership_seed"),
        // This can load a delivery or teleport. Its name does NOT make it read-only.
        new PortCommandSpec("ownership-check", "cli_mwl_port_ownership_check")
    });
    public static PortRequest Parse(string name, IReadOnlyList<string> args)
    {
        var spec = Commands.SingleOrDefault(x => x.Name == name) ?? throw new ArgumentException("Unknown MWL command: " + name);
        var request = new PortRequest(spec);
        switch (name)
        {
            case "port-status":
                Arity(args, 0, 1);
                if (args.Count > 0)
                {
                    if (!float.TryParse(args[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var radius) ||
                        float.IsNaN(radius) || float.IsInfinity(radius) || radius <= 0 || radius > 10000)
                        throw new ArgumentException("Radius must be finite, positive and at most 10000 metres.");
                    request.Radius = radius;
                }
                break;
            case "goto-port":
                Arity(args, 0, 1);
                if (args.Count > 0) request.Index = Integer(args[0], 0, int.MaxValue);
                break;
            case "clear-shipments": Arity(args, 0, 0); break;
            case "ownership-check":
                Arity(args, 1, 2); Token(args[0]); request.ShipmentId = args[0];
                if (args.Count > 1)
                {
                    request.Expectation = args[1].ToLowerInvariant();
                    if (request.Expectation != "allowed" && request.Expectation != "blocked") throw new ArgumentException("Expected allowed or blocked.");
                }
                break;
            default:
                Arity(args, 0, 2);
                if (args.Count > 0) { Token(args[0]); request.Item = args[0]; }
                if (args.Count > 1) request.Count = Integer(args[1], 1, 10000);
                break;
        }
        return request;
    }
    private static int Integer(string text, int min, int max)
    {
        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) || value < min || value > max)
            throw new ArgumentException("Integer is outside the supported range.");
        return value;
    }
    private static void Token(string text)
    { if (string.IsNullOrWhiteSpace(text) || text.Length > 200 || text.Any(char.IsWhiteSpace)) throw new ArgumentException("Expected one nonempty token, at most 200 characters."); }
    private static void Arity(IReadOnlyList<string> args, int min, int max)
    { if (args.Count < min || args.Count > max) throw new ArgumentException("Wrong number of arguments."); }
}
