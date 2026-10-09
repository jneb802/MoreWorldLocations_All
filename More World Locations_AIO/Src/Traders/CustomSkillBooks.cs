using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Common;
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

    private static readonly string[] DefaultTrainers =
    {
        "MWL_MeadowsTrainer1_Trainer",
        "MWL_SwampTrainer1_Trainer",
        "MWL_PlainsTrainer1_Trainer",
        "MWL_MistTrainer1_Trainer"
    };

    private static string DefaultTrainer(Skills.SkillType skill)
    {
        // Stable per skill, independent of discovery order, other installed skills, and Unity's random state.
        string key = "MWL_custom_skill_trainer_" + ((int)skill).ToString(CultureInfo.InvariantCulture);
        uint hash = unchecked((uint)key.GetStableHashCode());
        return DefaultTrainers[hash % (uint)DefaultTrainers.Length];
    }

    internal static void AddDefaultStock(string traderName, List<TraderManager.TradeItemYAML> items)
    {
        foreach (CustomSkill skill in Discovered.Values.OrderBy(skill => (int)skill.Type))
        {
            if (DefaultTrainer(skill.Type) != traderName) continue;
            for (int tier = 1; tier <= 3; tier++)
            {
                string bookName = BookName(skill.Type, tier);
                if (items.Any(item => item.PrefabName == bookName)) continue;
                // Each default trainer already defines the price and progression for all three tiers.
                TraderManager.TradeItemYAML? template = items.Find(item =>
                    item.PrefabName.StartsWith("MWL_skillBook_", StringComparison.Ordinal) &&
                    item.PrefabName.EndsWith("_bookTier" + tier, StringComparison.Ordinal));
                if (template == null) continue;

                items.Add(new TraderManager.TradeItemYAML
                {
                    PrefabName = bookName,
                    Stack = template.Stack,
                    Price = template.Price,
                    RequiredGlobalKey = template.RequiredGlobalKey,
                    NotRequiredGlobalKey = template.NotRequiredGlobalKey
                });
            }
        }
    }

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
            args.Context.AddString("  Default trainer: " + DefaultTrainer(skill.Type));
            for (int tier = 1; tier <= 3; tier++) args.Context.AddString("  " + BookName(skill.Type, tier));
            count++;
        }
        args.Context.AddString($"{count} custom skills with books. Both the server and clients need the matching skill mods. " +
                               "Custom skill books are stocked automatically with default trader configs. " +
                               "Use the book prefab names to set stock in custom trader YAML.");
    }

    private static void Warn(string message) =>
        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogWarning(message);
}
