using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using UnityEngine;
using valheimCLI;
using valheimCLI.Extensions;

namespace MoreWorldLocations.TestAdapter;

[BepInPlugin("testing.mwl.adapter", "MWL Test Adapter", "0.1.0")]
[BepInDependency("valheimCLI.valheimCLI")]
[BepInDependency(MwlGuid, BepInDependency.DependencyFlags.SoftDependency)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string MwlGuid = "warpalicious.More_World_Locations_AIO";
    private ExtensionRegistration? _registration;
    private readonly Dictionary<string, Terminal.ConsoleCommand> _aliases = new Dictionary<string, Terminal.ConsoleCommand>();
    // Publicized build references do not make this field public in the running game.
    private static Dictionary<string, Terminal.ConsoleCommand> CommandTable() =>
        (Dictionary<string, Terminal.ConsoleCommand>)(typeof(Terminal).GetField("commands", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null)
            ?? throw new InvalidOperationException("The game command table is unavailable."));
    private IEnumerator Start()
    {
        float deadline = Time.realtimeSinceStartup + 30;
        while (valheimCLIPlugin.Instance?.Extensions == null)
        {
            if (Time.realtimeSinceStartup > deadline) { Logger.LogError("Stable CLI extension API did not become ready."); yield break; }
            yield return null;
        }
        // Refuse an old CLI with its own built-in MWL commands. Never overwrite an unrelated command.
        var table = CommandTable();
        var conflict = PortCommandCatalog.Commands.FirstOrDefault(x => table.ContainsKey(x.Alias));
        if (conflict != null) { Logger.LogError("Command already registered: " + conflict.Alias + "; install the extracted CLI core."); yield break; }
        var registry = valheimCLIPlugin.Instance.Extensions;
        var commands = PortCommandCatalog.Commands.Select(spec => new ExtensionCommand(spec.Name, spec.Alias,
            context => Execute(spec.Name, context), spec.ReadOnly,
            spec.ServerOnly ? ExtensionRole.Server : spec.ClientOnly ? ExtensionRole.Client : ExtensionRole.Any, needsWorld: true)).ToList();
        commands.Add(new ExtensionCommand("runtime", "Report whether the live MWL plugin is installed", Runtime, readOnly: true));
        _registration = registry.Register("mwl.testing", "0.1.0", 1, commands.ToArray());
        _registration.OnDispose(() =>
        {
            foreach (var entry in _aliases)
                if (table.TryGetValue(entry.Key, out var current) && ReferenceEquals(current, entry.Value)) table.Remove(entry.Key);
            _aliases.Clear();
        });
        try
        {
            foreach (var spec in PortCommandCatalog.Commands)
            {
                // No independent permission waiver: this enters the exact cli_extension execution path.
                var command = new Terminal.ConsoleCommand(spec.Alias, "MWL test adapter: " + spec.Name, args =>
                    ExtensionHost.Execute(registry, "mwl.testing/" + spec.Name,
                        Enumerable.Range(1, args.Length - 1).Select(i => args[i]).ToArray(), args.Context.AddString));
                _aliases.Add(spec.Alias, command);
            }
        }
        catch { _registration.Dispose(); throw; }
    }
    private static IEnumerator Runtime(ExtensionContext context)
    {
        if (context.Arguments.Count != 0) { context.Fail("usage", "runtime takes no arguments"); yield break; }
        bool installed = Chainloader.PluginInfos.TryGetValue(MwlGuid, out var info) && info.Instance != null;
        context.Succeed(new Dictionary<string, object?>
        {
            ["source"] = "plugin-registry", ["complete"] = true, ["installed"] = installed,
            ["version"] = installed ? info!.Metadata.Version.ToString() : null
        });
    }
    private static IEnumerator Execute(string name, ExtensionContext context)
    {
        PortRequest request;
        try { request = PortCommandCatalog.Parse(name, context.Arguments); }
        catch (ArgumentException error) { context.Fail("usage", error.Message); yield break; }
        if (!Chainloader.PluginInfos.TryGetValue(MwlGuid, out var plugin) || plugin.Instance == null)
        { context.Fail("dependency_missing", "The live MWL plugin is not installed."); yield break; }
        var lines = new List<string>();
        try
        {
            switch (name)
            {
                case "port-status": PortOperations.PrintMwlPortStatus(request.Radius, lines.Add); break;
                case "goto-port": PortOperations.GotoMwlPort(request.Index, lines.Add); break;
                case "clear-shipments": PortOperations.ClearMwlShipments(lines.Add); break;
                case "payment-probe": PortOperations.RunMwlPortPaymentRegression(request.Item, request.Count, lines.Add); break;
                case "delivery-probe": PortOperations.RunMwlPortDeliveryRegression(request.Item, request.Count, lines.Add); break;
                case "ownership-seed": PortOperations.SeedMwlPortOwnershipShipment(request.Item, request.Count, lines.Add); break;
                case "ownership-check": PortOperations.CheckMwlPortOwnershipShipment(request.ShipmentId, request.Expectation, lines.Add); break;
            }
            var reply = new PortReply(lines);
            context.Succeed(new Dictionary<string, object?>
            {
                ["source"] = "mwl-port-probe", ["complete"] = reply.Complete && name != "goto-port" && name != "ownership-seed" && name != "clear-shipments",
                ["fields"] = reply.Values.ToDictionary(x => x.Key, x => (object?)x.Value),
                ["legacyLines"] = reply.Lines
            });
        }
        catch (Exception error) { context.Fail("mwl_probe_failed", error.GetBaseException().Message); }
        yield break;
    }
    private void OnDestroy() => _registration?.Dispose();
}
