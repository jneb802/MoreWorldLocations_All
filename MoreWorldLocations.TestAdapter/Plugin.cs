using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Valheim.Testing.Adapter;
using valheimCLI.Extensions;

namespace MoreWorldLocations.TestAdapter;

[BepInPlugin("testing.mwl.adapter", "MWL Test Adapter", "0.1.0")]
[BepInDependency("valheimCLI.valheimCLI")]
[BepInDependency(MwlGuid, BepInDependency.DependencyFlags.SoftDependency)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string MwlGuid = "warpalicious.More_World_Locations_AIO";
    private ExtensionRegistration? _registration;

    // The toolkit's registration waits for ValheimCLI's extension API and adds the owned-session identity
    // (mwl.testing/session), complete once a server world is up with MWL installed. The port commands are MWL's;
    // mwl.testing/runtime reports whether the live MWL plugin is loaded.
    private IEnumerator Start()
    {
        var commands = PortCommandCatalog.Commands.Select(spec => new ExtensionCommand(spec.Name, spec.Alias,
            context => Execute(spec.Name, context), spec.ReadOnly,
            spec.ServerOnly ? ExtensionRole.Server : spec.ClientOnly ? ExtensionRole.Client : ExtensionRole.Any, needsWorld: true)).ToList();
        commands.Add(InstalledPlugin.Command("runtime", MwlGuid));
        return TestExtension.Register("mwl.testing", "0.1.0", "MWL_TEST_SESSION_TOKEN",
            () => InstalledPlugin.Version(MwlGuid) != null, Registered, Logger.LogError, commands.ToArray());
    }

    // The seven legacy console names run the extension commands through ValheimCLI's own dispatcher. A name another
    // plugin already owns (an old ValheimCLI with built-in MWL commands) refuses the whole adapter rather than run
    // beside it; disposing the registration removes only the aliases added here.
    private void Registered(ExtensionRegistration registration)
    {
        _registration = registration;
        try
        {
            ConsoleAliases.Add(registration, PortCommandCatalog.Commands.Select(spec => (spec.Alias, spec.Name)).ToArray());
        }
        catch (Exception error)
        {
            Logger.LogError("MWL test adapter disabled: " + error.Message + " Install the extracted ValheimCLI core.");
            _registration = null;
            registration.Dispose();
        }
    }

    private static IEnumerator Execute(string name, ExtensionContext context)
    {
        PortRequest request;
        try { request = PortCommandCatalog.Parse(name, context.Arguments); }
        catch (ArgumentException error) { context.Fail("usage", error.Message); yield break; }
        if (InstalledPlugin.Version(MwlGuid) == null)
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
