using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace UpgradeWorld;

public class CommandRegistration
{
    public string name = "";
    public string description = "";
    public string[] commands = new string[0];

    public void AddCommand()
    {
        new Console.ConsoleCommand(name, description, (args) =>
        {
            foreach (var command in commands)
                args.Context.TryRunCommand(command);
        });
    }
}

public static class Upgrade
{
    private static List<CommandRegistration> registrations = new();

    public const string GUID = "upgrade_world";
    private static bool Patched = false;
    public static void Register(string name, string description, params string[] commands)
    {
        if (!Chainloader.PluginInfos.ContainsKey(GUID)) return;
        PatchIfNeeded();
        registrations.Add(new() { name = name, description = description, commands = commands });
    }
    private static void PatchIfNeeded()
    {
        if (Patched) return;
        Patched = true;
        Harmony harmony = new("helpers.upgrade_world");
        var toPatch = AccessTools.Method(typeof(Terminal), nameof(Terminal.InitTerminal));
        var postfix = AccessTools.Method(typeof(Upgrade), nameof(AddCommands));
        harmony.Patch(toPatch, postfix: new HarmonyMethod(postfix));
    }

    static void AddCommands()
    {
        foreach (var registration in registrations)
            registration.AddCommand();
    }
}