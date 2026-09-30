using System.Diagnostics;
using System.Text.Json;
using System.Security.Cryptography;
using Valheim.Testing.Game;
using valheim_cli.Testing;

// Boundary smoke only: this driver refuses a loaded world or an installed MWL plugin.
if (args.Length != 5)
{
    Console.Error.WriteLine("Usage: MoreWorldLocations.SystemTests <port> <adapter.dll> <empty-scripts-dir> <report.json> <pins-file>\nRuns only the main-menu adapter smoke. PortScenarios are called by an owned full-MWL fixture host.");
    return 2;
}
var records = new List<object>();
var aliases = new[] { "cli_mwl_port_status", "cli_mwl_goto_port", "cli_mwl_clear_shipments", "cli_mwl_port_payment_regression", "cli_mwl_port_delivery_regression", "cli_mwl_port_ownership_seed", "cli_mwl_port_ownership_check" };
var target = Path.Combine(Path.GetFullPath(args[2]), "MoreWorldLocations.TestAdapter.dll");
bool owned = false, passed = false;
try
{
    if (!Directory.Exists(args[2]) || Directory.EnumerateFiles(args[2], "*.dll", SearchOption.AllDirectories).Any())
        throw new InvalidOperationException("Use an empty owned scripts directory.");
    string baselinePins = StrictExpectations.WithPlugin(StrictExpectations.Load(args[4]), "testing.mwl.adapter", "absent");
    baselinePins = StrictExpectations.WithPlugin(baselinePins, "warpalicious.More_World_Locations_AIO", "absent");
    string installedPins = StrictExpectations.WithPlugin(baselinePins, "testing.mwl.adapter", Convert.ToHexString(MD5.HashData(File.ReadAllBytes(args[1]))));
    using var client = new GameActor("mwl-menu-smoke", new CliTransport("127.0.0.1", int.Parse(args[0]))) { CommandTimeout = TimeSpan.FromSeconds(15) };
    client.VerifyEnvironment(baselinePins);
    CommandResult Run(string command, bool requireSuccess = true)
    {
        var result = client.Execute(command, requireSuccess);
        records.Add(new { command, result.Ok, result.ErrorCode, result.Output });
        if (requireSuccess && !result.Ok) throw new InvalidOperationException(command + ": " + result.ErrorCode);
        return result;
    }
    void Require(bool condition, string check)
    { if (!condition) throw new InvalidOperationException(check); records.Add(new { check, passed = true }); }
    var world = Run("cli_world", false);
    Require(!world.Ok && world.Output.Any(line => line.StartsWith("ERROR: code=no_world message=", StringComparison.Ordinal)), "No world loaded");
    var manifest = Run("cli_manifest");
    Require(!manifest.Output.Any(x => x.Contains("warpalicious.More_World_Locations_AIO")), "MWL is absent; this smoke cannot exercise gameplay mutations");
    var build = Run("cli_build").Output.ToArray();
    foreach (string alias in aliases)
        Require(!Run(alias, false).Ok, "Core has no built-in " + alias);
    client.InvalidateEnvironment();
    owned = true;
    File.Copy(args[1], target + ".incoming", true); File.Move(target + ".incoming", target);
    JsonElement? Registration()
    {
        using var doc = GameActor.ParseLine(Run("cli_extensions"), "EXTENSIONS ");
        foreach (var extension in doc.RootElement.GetProperty("extensions").EnumerateArray())
            if (extension.GetProperty("id").GetString() == "mwl.testing") return extension.Clone();
        return null;
    }
    async Task AwaitPresence(bool present)
    {
        var clock = Stopwatch.StartNew();
        while ((Registration() != null) != present)
        {
            if (clock.Elapsed > TimeSpan.FromSeconds(40)) throw new TimeoutException("Adapter registration/removal timed out");
            await Task.Delay(500);
        }
    }
    await client.WaitForEnvironment(installedPins, TimeSpan.FromSeconds(45));
    await AwaitPresence(true);
    var registration = Registration()!.Value;
    Require(registration.GetProperty("commands").GetArrayLength() == 9, "All seven port commands plus the runtime and session capabilities registered");
    using (var doc = GameActor.ParseLine(Run("cli_extension mwl.testing/runtime"), "EXTENSION_RESULT "))
        Require(!doc.RootElement.GetProperty("data").GetProperty("installed").GetBoolean(), "Runtime observation accurately reports MWL absent");
    foreach (string command in aliases.Append("cli_extension mwl.testing/port-status"))
    {
        var refused = Run(command, false);
        using var doc = GameActor.ParseLine(refused, "EXTENSION_RESULT ");
        Require(!refused.Ok && doc.RootElement.GetProperty("code").GetString() == "extension_precondition", "Shared precondition protects " + command);
    }
    client.InvalidateEnvironment();
    File.Delete(target);
    await client.WaitForEnvironment(baselinePins, TimeSpan.FromSeconds(45));
    await AwaitPresence(false);
    foreach (string alias in aliases)
        Require(!Run(alias, false).Ok, "Unload removed " + alias);
    Require(Run("cli_build").Output.SequenceEqual(build), "Core unchanged and original connection remains usable");
    passed = true; Console.WriteLine("PASS: MWL adapter load/unload, seven aliases, shared refusal path, missing dependency observation, stable core.");
}
catch (Exception error) { records.Add(new { failure = error.ToString() }); Console.Error.WriteLine(error.Message); }
finally
{
    if (owned && File.Exists(target)) File.Delete(target);
    var report = Path.GetFullPath(args[3]); Directory.CreateDirectory(Path.GetDirectoryName(report)!);
    File.WriteAllText(report, JsonSerializer.Serialize(new { passed, records }, new JsonSerializerOptions { WriteIndented = true }));
}
return passed ? 0 : 1;
