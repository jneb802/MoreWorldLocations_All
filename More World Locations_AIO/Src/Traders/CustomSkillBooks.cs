using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Managers;

namespace More_World_Locations_AIO.Traders;

internal static class CustomSkillBooks
{
    private sealed class CustomSkill
    {
        internal readonly Skills.SkillType Type;
        internal readonly string Identifier;
        internal readonly string Source;

        internal CustomSkill(Skills.SkillType type, string identifier, string source)
        {
            Type = type;
            Identifier = identifier;
            Source = source;
        }
    }

    private static readonly Dictionary<Skills.SkillType, CustomSkill> Discovered = new();

    // Called with the vanilla book setup, before Jotunn registers items. No local player is needed.
    internal static void BuildBooks()
    {
        CollectJotunnSkills();
        CollectSkillManagerSkills();
        foreach (CustomSkill skill in Discovered.Values.OrderBy(skill => (int)skill.Type))
        {
            for (int tier = 1; tier <= 3; tier++)
            {
                TraderItems.CreateSkillBook(skill.Type, tier);
            }
        }

        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
            $"Created skill books for {Discovered.Count} custom skills.");
    }

    private static void Record(Skills.SkillType type, string identifier, string source)
    {
        if (Enum.IsDefined(typeof(Skills.SkillType), type) || string.IsNullOrEmpty(identifier)) return;
        if (!Discovered.ContainsKey(type)) Discovered.Add(type, new CustomSkill(type, identifier, source));
    }

    private static void CollectJotunnSkills()
    {
        try
        {
            FieldInfo? registry = AccessTools.Field(typeof(SkillManager), "CustomSkills");
            if (registry?.GetValue(SkillManager.Instance) is not IDictionary skills)
            {
                Warn("Cannot read Jotunn's custom skill registry. Its custom skill books will not be available.");
                return;
            }

            foreach (DictionaryEntry entry in skills)
            {
                if (entry.Key is Skills.SkillType type && entry.Value is SkillConfig config)
                    Record(type, config.Identifier, "Jotunn");
            }
        }
        catch (Exception exception)
        {
            Warn($"Cannot discover Jotunn custom skills: {exception.Message}");
        }
    }

    private static void CollectSkillManagerSkills()
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic) continue;
            try
            {
                Type? type = assembly.GetType("SkillManager.Skill", false);
                if (type == null) continue;
                FieldInfo? registry = type.GetField("skillByName", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (registry?.GetValue(null) is not IDictionary skills)
                {
                    Warn($"Cannot read the SkillManager registry in {assembly.GetName().Name}. Its custom skill books will not be available.");
                    continue;
                }

                foreach (DictionaryEntry entry in skills)
                {
                    if (entry.Key is string name && !string.IsNullOrEmpty(name))
                        Record((Skills.SkillType)Math.Abs(name.GetStableHashCode()), name, assembly.GetName().Name ?? "SkillManager");
                }
            }
            catch (Exception exception)
            {
                Warn($"Cannot discover SkillManager skills in {assembly.GetName().Name}: {exception.Message}");
            }
        }
    }

    internal static string BookName(Skills.SkillType skill, int tier)
    {
        string identifier = Enum.IsDefined(typeof(Skills.SkillType), skill)
            ? skill.ToString()
            : ((int)skill).ToString(CultureInfo.InvariantCulture);
        return $"MWL_skillBook_{identifier}_bookTier{tier}";
    }

    internal static void ListBooks(Terminal.ConsoleEventArgs args)
    {
        string filter = args.Length > 1 ? string.Join(" ", args.Args.Skip(1)) : "";
        int count = 0;
        foreach (CustomSkill skill in Discovered.Values.OrderBy(skill => skill.Identifier, StringComparer.Ordinal))
        {
            string name = Localization.instance.Localize("$skill_" + ((int)skill.Type).ToString(CultureInfo.InvariantCulture));
            string detail = $"{name} | {skill.Identifier} | ID {(int)skill.Type} | {skill.Source}";
            if (detail.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0) continue;
            args.Context.AddString(detail);
            for (int tier = 1; tier <= 3; tier++) args.Context.AddString("  " + BookName(skill.Type, tier));
            count++;
        }
        args.Context.AddString($"{count} custom skills with books. Both the server and clients need the matching skill mods. " +
                               "Use the book prefab names in the trader YAML configuration.");
    }

    private static void Warn(string message) =>
        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogWarning(message);
}
