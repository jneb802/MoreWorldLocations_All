// Copied from MidnightsFX/Valheim_Fortress v0.39.0, API/API.cs (schema 3).
#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;

namespace ValheimFortress
{
    /// <summary>
    /// Public API for Valheim Fortress. Copy this file into your project and declare a soft dependency
    /// on Valheim Fortress to drive Fortress-style horde challenges from your own mod:
    /// <code>[BepInDependency("MidnightsFX.ValheimFortress", BepInDependency.DependencyFlags.SoftDependency)]</code>
    /// All access is through reflection, so your plugin never needs a hard reference to the Valheim
    /// Fortress assembly. Check <see cref="IsAvailable"/> before calling anything else.
    /// Requires a reference to the framework assembly <c>System.Runtime.Serialization</c>.
    /// </summary>
    public static class API
    {
        private static readonly Type APIReceiver;
        private static readonly MethodInfo RunChallengeMethod;
        private static readonly MethodInfo GetSpawnableCreaturesMethod;
        private static readonly MethodInfo GetRewardItemsMethod;
        private static readonly MethodInfo GetWaveStylesMethod;
        private static readonly MethodInfo GetShrineChallengeDefinitionMethod;
        private static readonly MethodInfo GetShrineTributesMethod;

        /// <summary>True when Valheim Fortress is loaded and the API is callable.</summary>
        public static bool IsAvailable => APIReceiver != null;

        static API()
        {
            APIReceiver = Type.GetType("ValheimFortress.APIReceiver, ValheimFortress");
            if (APIReceiver == null) return;
            RunChallengeMethod = APIReceiver.GetMethod("RunChallenge", BindingFlags.Public | BindingFlags.Static);
            GetSpawnableCreaturesMethod = APIReceiver.GetMethod("GetSpawnableCreatures", BindingFlags.Public | BindingFlags.Static);
            GetRewardItemsMethod = APIReceiver.GetMethod("GetRewardItems", BindingFlags.Public | BindingFlags.Static);
            GetWaveStylesMethod = APIReceiver.GetMethod("GetWaveStyles", BindingFlags.Public | BindingFlags.Static);
            GetShrineChallengeDefinitionMethod = APIReceiver.GetMethod("GetShrineChallengeDefinition", BindingFlags.Public | BindingFlags.Static);
            GetShrineTributesMethod = APIReceiver.GetMethod("GetShrineTributes", BindingFlags.Public | BindingFlags.Static);
        }

        /// <summary>
        /// Runs a wave-based challenge. Creatures emerge from <paramref name="spawnPoints"/> (one is
        /// chosen at random per creature; supply as many as you like for variety), waves advance as creatures
        /// die, and when the final phase is cleared the configured rewards are dropped at
        /// <paramref name="rewardLocation"/>. The runner is networked and self-destroys on completion.
        ///
        /// Must be called in-world. Like the physical shrines this is owner-authoritative, so call it on
        /// the instance that should drive the run (the server in dedicated setups, or the host).
        /// </summary>
        /// <param name="definition">The wave/creature/reward definition (see <see cref="VFChallengeDefinition"/>).</param>
        /// <param name="spawnPoints">World positions creatures spawn from. At least one is required.</param>
        /// <param name="rewardLocation">World position where rewards are dropped on completion.</param>
        /// <returns>True if the challenge was successfully started.</returns>
        public static bool RunChallenge(VFChallengeDefinition definition, Vector3[] spawnPoints, Vector3 rewardLocation)
        {
            if (RunChallengeMethod == null || definition == null) { return false; }
            string json = Serialize(definition);
            if (json == null) { return false; }
            // Guard the unbox: if the reflected call ever returns null/non-bool (e.g. a boundary hiccup),
            // treat it as failure rather than throwing an InvalidCastException back into the caller's mod.
            return RunChallengeMethod.Invoke(null, new object[] { json, spawnPoints, rewardLocation }) is bool ok && ok;
        }

        /// <summary>Convenience overload accepting a <see cref="List{T}"/> of spawn points.</summary>
        public static bool RunChallenge(VFChallengeDefinition definition, List<Vector3> spawnPoints, Vector3 rewardLocation)
        {
            return RunChallenge(definition, spawnPoints?.ToArray(), rewardLocation);
        }

        /// <summary>
        /// Returns the names of every spawnable creature. These are the valid values for
        /// <see cref="VFHoardEntry.Creature"/> and for the monster include/exclude lists.
        /// </summary>
        public static List<string> GetSpawnableCreatures()
        {
            if (GetSpawnableCreaturesMethod == null) { return new List<string>(); }
            return (List<string>)GetSpawnableCreaturesMethod.Invoke(null, null);
        }

        /// <summary>Returns the item prefab names valid as reward keys.</summary>
        public static List<string> GetRewardItems()
        {
            if (GetRewardItemsMethod == null) { return new List<string>(); }
            return (List<string>)GetRewardItemsMethod.Invoke(null, null);
        }

        /// <summary>Returns the available wave-style names (valid values for <see cref="VFChallengeDefinition.WaveStyle"/>).</summary>
        public static List<string> GetWaveStyles()
        {
            if (GetWaveStylesMethod == null) { return new List<string>(); }
            return (List<string>)GetWaveStylesMethod.Invoke(null, null);
        }

        /// <summary>
        /// Returns the challenge a wild shrine runs when it is offered <paramref name="tributePrefab"/>, built from
        /// the wild shrine configuration currently loaded (the server's <c>WildShrines.yaml</c> once synced). The
        /// definition carries the shrine level's waves, creature rules, rewards and messages; adjust it if you
        /// like, then pass it to <see cref="RunChallenge(VFChallengeDefinition, Vector3[], Vector3)"/> with your
        /// own spawn points and reward location. The tribute amount is not part of the definition, since the
        /// caller decides what (if anything) the player pays.
        /// </summary>
        /// <param name="shrineName">The wild shrine prefab name, for example <c>VF_wild_shrine_green1</c>
        /// (case-insensitive; a trailing <c>(Clone)</c> is ignored). See <see cref="GetShrineTributes"/>.</param>
        /// <param name="tributePrefab">The item prefab sacrificed at the shrine, for example <c>TrophyBoar</c>
        /// (case-insensitive).</param>
        /// <returns>The definition, or null when Valheim Fortress is missing or the shrine does not accept that tribute.</returns>
        public static VFChallengeDefinition GetShrineChallengeDefinition(string shrineName, string tributePrefab)
        {
            if (GetShrineChallengeDefinitionMethod == null || string.IsNullOrEmpty(shrineName) || string.IsNullOrEmpty(tributePrefab)) { return null; }
            string json = GetShrineChallengeDefinitionMethod.Invoke(null, new object[] { shrineName, tributePrefab }) as string;
            return Deserialize(json);
        }

        /// <summary>Returns every wild shrine prefab name mapped to the tribute item prefabs it accepts (valid inputs
        /// for <see cref="GetShrineChallengeDefinition"/>).</summary>
        public static Dictionary<string, List<string>> GetShrineTributes()
        {
            if (GetShrineTributesMethod == null) { return new Dictionary<string, List<string>>(); }
            return (Dictionary<string, List<string>>)GetShrineTributesMethod.Invoke(null, null);
        }

        // Serializer settings MUST stay in lockstep with APIReceiver on the Valheim Fortress side, since JSON
        // is the only thing that crosses the soft-dependency boundary, in both directions: definitions passed
        // to RunChallenge are serialized here, and shrine definitions are serialized there and read here.
        // UseSimpleDictionaryFormat writes the reward/override maps as plain JSON objects ({"Coins":5})
        // rather than key/value-pair arrays.
        private static readonly DataContractJsonSerializerSettings SerializerSettings =
            new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true };

        private static string Serialize(VFChallengeDefinition definition)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(VFChallengeDefinition), SerializerSettings);
                    serializer.WriteObject(ms, definition);
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch
            {
                return null;
            }
        }

        private static VFChallengeDefinition Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json)) { return null; }
            try
            {
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(VFChallengeDefinition), SerializerSettings);
                    return serializer.ReadObject(ms) as VFChallengeDefinition;
                }
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Defines a Fortress challenge: wave/creature selection and rewards in one place (the way WildShrines
    /// bundle them). There are two authoring modes:
    /// <list type="bullet">
    /// <item><b>Tuned / generated</b> (default): set <see cref="Biome"/>, <see cref="Difficulty"/>,
    /// <see cref="WaveStyle"/>, optional monster filters and <see cref="NumPhases"/>; Valheim Fortress
    /// generates the creature waves.</item>
    /// <item><b>Explicit</b>: set <see cref="ExplicitPhases"/> to control the exact creatures, counts and
    /// stars per phase. When present this overrides the tuned fields.</item>
    /// </list>
    /// Rewards can be auto-scaled, fixed, or both.
    /// </summary>
    [DataContract]
    public class VFChallengeDefinition
    {
        /// <summary>The wire schema version this copy of the API produces. Bumped when the serialized shape
        /// changes so a Valheim Fortress install can detect a caller whose copied <c>API.cs</c> is out of
        /// date. Callers do not set this.</summary>
        public const int CurrentSchemaVersion = 3;

        /// <summary>Schema marker stamped automatically on serialization (see <see cref="CurrentSchemaVersion"/>).
        /// Always emitted; the receiver warns when it does not match the installed version. Do not set this yourself.</summary>
        [DataMember]
        public int SchemaVersion { get; set; } = CurrentSchemaVersion;

        // ---- Tuned / generated mode (used when ExplicitPhases is null/empty) ----

        /// <summary>Biome whose creatures the wave is generated from. Supported: Meadows, BlackForest,
        /// Swamp, Mountain, Plains, Mistlands, AshLands.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Heightmap.Biome Biome { get; set; } = Heightmap.Biome.Meadows;

        /// <summary>Difficulty/level index. Higher means more wave points and larger rewards.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public short Difficulty { get; set; } = 1;

        /// <summary>Wave-style name controlling the common/rare/elite mix (see <see cref="API.GetWaveStyles"/>).
        /// Defaults to "Normal" if empty or unrecognized.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string WaveStyle { get; set; } = "Normal";

        /// <summary>Number of wave phases to generate. Defaults to 4 when left at 0.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public short NumPhases { get; set; } = 4;

        /// <summary>Optional cap on creatures generated per phase. 0 uses the Fortress default cap.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public short MaxCreaturesPerPhase { get; set; } = 0;

        /// <summary>Optional whitelist of creature names; when set, only these creatures are generated.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<string> OnlySelectMonsters { get; set; }

        /// <summary>Optional blacklist of creature names to exclude from generation.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<string> ExcludeSelectMonsters { get; set; }

        /// <summary>Optional shaping of how many common creatures each generated wave gets (see <see cref="VFSpawnModifiers"/>).</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public VFSpawnModifiers CommonSpawnModifiers { get; set; }

        /// <summary>Optional shaping of how many rare creatures each generated wave gets (see <see cref="VFSpawnModifiers"/>).</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public VFSpawnModifiers RareSpawnModifiers { get; set; }

        /// <summary>Optional shaping of how many elite creatures each generated wave gets (see <see cref="VFSpawnModifiers"/>).</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public VFSpawnModifiers EliteSpawnModifiers { get; set; }

        /// <summary>Increases wave points and rewards.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool HardMode { get; set; } = false;

        /// <summary>Uses the boss wave style and applies the boss reward multiplier.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool BossMode { get; set; } = false;

        /// <summary>Doubles the number of phases and applies the siege reward multiplier.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool SiegeMode { get; set; } = false;

        // ---- Creature behavior ----

        /// <summary>Global toggle: when true, challenge creatures drop their normal loot on death. Defaults
        /// to false, so (like the physical shrines) challenge creatures drop nothing and players are rewarded
        /// only via the configured challenge rewards. Can be overridden per creature via
        /// <see cref="CreatureDropOverrides"/>.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool EnableCreatureDrops { get; set; } = false;

        /// <summary>Optional per-creature loot-drop overrides, keyed by creature name (see
        /// <see cref="API.GetSpawnableCreatures"/>). A creature listed here uses its mapped value instead of
        /// <see cref="EnableCreatureDrops"/> (true = drops, false = no drops); creatures not listed fall back
        /// to the global value. Unknown creature names are ignored.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Dictionary<string, bool> CreatureDropOverrides { get; set; }

        /// <summary>Optional rules for creature stars and guaranteed Star Level System (SLS) modifiers, used in
        /// both tuned and explicit mode. Rules apply in list order; each matching rule overwrites the settings it
        /// sets, so put general rules first and specific ones after. See <see cref="VFCreatureRule"/>.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<VFCreatureRule> CreatureRules { get; set; }

        // ---- Explicit mode (overrides the tuned fields when set) ----

        /// <summary>Explicit per-phase creature lists. Each inner list is one phase. When non-empty this
        /// overrides the tuned/generated fields above.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<List<VFHoardEntry>> ExplicitPhases { get; set; }

        // ---- Rewards (either, both, or neither) ----

        /// <summary>Auto-scaled rewards. Keys are item prefab names, values are the per-unit point cost;
        /// the spawned amount scales with difficulty, modes and nearby-player count.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Dictionary<string, short> ScaledRewards { get; set; }

        /// <summary>Fixed rewards. Keys are item prefab names, values are the exact counts to spawn.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Dictionary<string, short> FixedRewards { get; set; }

        // ---- Messaging (optional) ----

        /// <summary>Optional message shown to nearby players when the challenge begins (supports $localization keys).</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string WaveStartMessage { get; set; }

        /// <summary>Optional message shown to nearby players when the challenge completes (supports $localization keys).</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string WaveEndMessage { get; set; }

        /// <summary>Optional phrases shown to nearby players during each between-wave pause. Each entry supports
        /// a $localization key or literal text. When non-empty this replaces the built-in phrase pool for this
        /// run; when null/empty the built-in pool is used. Selection order is controlled by
        /// <see cref="OrderedPhrases"/>.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<string> BetweenWavePhrases { get; set; }

        /// <summary>Controls how <see cref="BetweenWavePhrases"/> are selected. When true, phrases play in list
        /// order (wrapping when there are more pauses than phrases); when false (default), a phrase is picked at
        /// random each pause.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool OrderedPhrases { get; set; } = false;

        // ---- Presentation (optional) ----

        /// <summary>When true, each spawn point is marked on the minimap for the duration of the run (cleared
        /// automatically when the challenge finishes) using the vanilla event markers -- the shaded event-area
        /// circle plus the animated event icon -- the same markers the physical shrines and SLS raids use. The
        /// markers are client-local and cosmetic: they only render for the instance that drives the run (the
        /// caller in single-player or on a P2P host), and are skipped on a headless dedicated server. Defaults
        /// to false.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool DrawMapOverlay { get; set; } = false;
    }

    /// <summary>A single creature entry in an explicit wave phase.</summary>
    [DataContract]
    public class VFHoardEntry
    {
        /// <summary>The creature name (one of <see cref="API.GetSpawnableCreatures"/>).</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string Creature { get; set; }

        /// <summary>How many of this creature to spawn in the phase.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public short Amount { get; set; }

        /// <summary>Star level (0 = no stars). <see cref="VFChallengeDefinition.CreatureRules"/> can still raise or cap it.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public short Stars { get; set; }

        /// <summary>Optional guaranteed SLS modifiers for this entry. Applied after
        /// <see cref="VFChallengeDefinition.CreatureRules"/>: each list set here replaces that type's list.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public VFModifierSet Modifiers { get; set; }
    }

    /// <summary>
    /// A rule that sets stars and guaranteed SLS modifiers for the creatures it matches. The selectors
    /// (<see cref="Waves"/>, <see cref="Creatures"/>, <see cref="SpawnTypes"/>) are optional: one left null
    /// matches everything, and a rule must match every selector it sets. Settings left null keep the value from
    /// earlier rules. Valheim Fortress also reads this type from its YAML configs, with camelCase keys.
    /// </summary>
    [DataContract]
    public class VFCreatureRule
    {
        /// <summary>1-based wave numbers, in spawn order. Negative numbers count from the end, so -1 is the last
        /// wave (where bosses spawn), whatever the wave count.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<int> Waves { get; set; }

        /// <summary>Creature names (see <see cref="API.GetSpawnableCreatures"/>), case-insensitive.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<string> Creatures { get; set; }

        /// <summary>Creature categories: common, rare, elite or unique (bosses), case-insensitive.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<string> SpawnTypes { get; set; }

        /// <summary>Lowest star count for matching creatures.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public int? MinStars { get; set; }

        /// <summary>Highest star count for matching creatures; 0 forces no stars. When stars conflict the later rule wins;
        /// within one rule this wins over <see cref="MinStars"/>.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public int? MaxStars { get; set; }

        /// <summary>Guaranteed SLS modifiers. Each list set here replaces that type's list from earlier rules.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public VFModifierSet Modifiers { get; set; }
    }

    /// <summary>
    /// Guaranteed Star Level System modifiers, by SLS modifier type. Names are SLS modifier names (for example
    /// Fire, Fast, BossSummoner), case-insensitive. A null list leaves that type unchanged; an empty list clears
    /// it. A creature given any guaranteed modifiers does not also get SLS's own random modifiers. Ignored when
    /// SLS is not installed.
    /// </summary>
    [DataContract]
    public class VFModifierSet
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<string> Major { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<string> Minor { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<string> Boss { get; set; }

        /// <summary>Returns a copy of this set with each list that <paramref name="other"/> sets replacing this one's.</summary>
        public VFModifierSet Overlay(VFModifierSet other)
        {
            return new VFModifierSet
            {
                Major = Copy(other?.Major ?? Major),
                Minor = Copy(other?.Minor ?? Minor),
                Boss = Copy(other?.Boss ?? Boss)
            };
        }

        /// <summary>True when no list has any names.</summary>
        public bool IsEmpty()
        {
            return (Major == null || Major.Count == 0) && (Minor == null || Minor.Count == 0) && (Boss == null || Boss.Count == 0);
        }

        private static List<string> Copy(List<string> names)
        {
            return names == null ? null : new List<string>(names);
        }
    }

    /// <summary>
    /// Shapes how many creatures of one category (common, rare or elite) each generated wave gets, the same as the
    /// <c>commonSpawnModifiers</c>/<c>rareSpawnModifiers</c>/<c>eliteSpawnModifiers</c> settings in the Valheim
    /// Fortress <c>Levels.yaml</c> and <c>WildShrines.yaml</c> configs. Used in tuned mode only. Set one per
    /// category; when several are set, the last one in the order below wins. Leaving all unset spawns the full
    /// amount in every wave.
    /// </summary>
    [DataContract]
    public class VFSpawnModifiers
    {
        /// <summary>Front-loads the category: many in the first wave, tapering off in later waves (with some randomness).</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool LinearDecreaseRandomWaveAdjustment { get; set; }

        /// <summary>Back-loads the category: few in the first wave, ramping up in later waves (with some randomness).</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool LinearIncreaseRandomWaveAdjustment { get; set; }

        /// <summary>Randomly reduces the amount in each wave, by up to about half.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool PartialRandomWaveAdjustment { get; set; }

        /// <summary>Holds the category back until the second half of the waves.</summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool OnlyGenerateInSecondHalf { get; set; }
    }
}
