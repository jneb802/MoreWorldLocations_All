using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using BepInEx;
using Jotunn.Managers;
using UnityEngine;

[BepInPlugin("mwl.tests.customskills", "MWL custom skill book live tests", "1.0.0")]
[BepInDependency("warpalicious.More_World_Locations_AIO")]
public sealed class CustomSkillBookTests : BaseUnityPlugin
{
    private bool running;

    private void Awake()
    {
        new Terminal.ConsoleCommand("mwl_test_skillbooks", "Run skill book consumption checks on a test character",
            (Terminal.ConsoleEvent)(args =>
            {
                if (running || Player.m_localPlayer == null) return;
                running = true;
                StartCoroutine(Run(args));
            }));
        new Terminal.ConsoleCommand("mwl_test_savedbooks", "List saved skill books in the test character inventory",
            (Terminal.ConsoleEvent)(args =>
            {
                foreach (ItemDrop.ItemData item in Player.m_localPlayer.GetInventory().GetAllItems()
                             .Where(item => item.m_dropPrefab.name.StartsWith("MWL_skillBook_")))
                    Report(args, $"SAVED_BOOK {item.m_dropPrefab.name} name={Localization.instance.Localize(item.m_shared.m_name)} count={item.m_stack}");
            }));
    }

    private IEnumerator Run(Terminal.ConsoleEventArgs args)
    {
        int failures = 0;
        string[] identifiers = { "midnightsfx.animalwhisper", "midnightsfx.voyager", "midnightsfx.hauling", "midnightsfx.forging", "Run" };
        Player player = Player.m_localPlayer;
        foreach (string identifier in identifiers)
        {
            Skills.SkillType type = identifier == "Run" ? Skills.SkillType.Run : SkillManager.Instance.GetSkill(identifier).m_skill;
            MethodInfo getSkill = typeof(Skills).GetMethod("GetSkill", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Skills.Skill skill = (Skills.Skill)getSkill.Invoke(player.GetSkills(), new object[] { type });
            float oldLevel = skill.m_level;
            float oldAccumulator = skill.m_accumulator;
            for (int scenario = 0; scenario < 5; scenario++)
            {
                int tier = Math.Min(scenario + 1, 3);
                float start = scenario == 3 ? 98f : scenario == 4 ? 100f : 10.25f;
                int grant = tier == 1 ? 1 : tier == 2 ? 3 : 5;
                string prefab = $"MWL_skillBook_{type}_bookTier{tier}";
                skill.m_level = start;
                skill.m_accumulator = 0.25f;
                ItemDrop.ItemData item = player.GetInventory().AddItem(prefab, 1, 1, 0, 0L, "", true);
                if (item == null)
                {
                    failures++;
                    Report(args, "FAIL missing " + prefab);
                    continue;
                }
                // AddItem can merge into an existing stack; use the item actually held by the player.
                item = player.GetInventory().GetAllItems().First(held => held.m_dropPrefab.name == prefab);
                string name = Localization.instance.Localize(item.m_shared.m_name);
                int before = player.GetInventory().CountItems(item.m_shared.m_name);
                player.UseItem(player.GetInventory(), item, false);
                yield return new WaitForSeconds(0.4f);
                bool passed = Mathf.Approximately(skill.m_level, Mathf.Min(100f, start + grant)) &&
                              Mathf.Approximately(skill.m_accumulator, 0.25f) && !name.Contains("$") &&
                              player.GetInventory().CountItems(item.m_shared.m_name) == before - 1;
                if (!passed) failures++;
                Report(args, $"{(passed ? "PASS" : "FAIL")} {identifier} tier={tier} name={name} level={start}->{skill.m_level} accumulator={skill.m_accumulator}");
            }
            skill.m_level = oldLevel;
            skill.m_accumulator = oldAccumulator;
        }
        Report(args, $"MWL_BOOK_TEST_COMPLETE failures={failures}");
        running = false;
    }

    private void Report(Terminal.ConsoleEventArgs args, string message)
    {
        Logger.LogInfo(message);
        args.Context.AddString(message);
    }
}
