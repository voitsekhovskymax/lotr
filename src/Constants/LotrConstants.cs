using System.Collections.Generic;

namespace Lotr.Constants;

/// <summary>
/// Central constants for the LOTR mod.
/// All magic strings live here — never hardcode these inline.
/// </summary>
public static class LotrConstants
{
    // ── Mod identity ──────────────────────────────────────────────
    public const string ModId     = "lotr";
    public const string LogPrefix = "[LOTR]";

    // ── Faction ids ───────────────────────────────────────────────
    public static class Factions
    {
        public const string Shire     = "lotr:faction-shire";
        public const string Gondor    = "lotr:faction-gondor";
        public const string Rohan     = "lotr:faction-rohan";
        public const string Rivendell = "lotr:faction-rivendell";
        public const string Moria     = "lotr:faction-moria";
        public const string Isengard  = "lotr:faction-isengard";
        public const string Mordor    = "lotr:faction-mordor";

        public static readonly IReadOnlyList<string> All = new[]
        {
            Shire, Gondor, Rohan, Rivendell, Moria, Isengard, Mordor
        };
    }

    // ── Region ids ────────────────────────────────────────────────
    public static class Regions
    {
        public const string Shire     = "lotr:region-shire";
        public const string Rivendell = "lotr:region-rivendell";
        public const string Moria     = "lotr:region-moria";
        public const string Rohan     = "lotr:region-rohan";
        public const string Gondor    = "lotr:region-gondor";
        public const string Isengard  = "lotr:region-isengard";
        public const string Mordor    = "lotr:region-mordor";
    }

    // ── Quest ids ─────────────────────────────────────────────────
    public static class Quests
    {
        public const string ShireLongExpectedParty = "lotr:quest-shire-long-expected-party";
        public const string ShireConcerningHobbits = "lotr:quest-shire-concerning-hobbits";
    }

    // ── Quest id naming convention ────────────────────────────────
    // Format: lotr:quest-<region>-<name>
    // Example: lotr:quest-shire-long-expected-party
    public static class QuestPrefix
    {
        public const string Shire = "lotr:quest-shire-";
    }

    // ── Dialogue ids ──────────────────────────────────────────────
    // Format: lotr:dialogue-<npcname>-<topic>
    // Example: lotr:dialogue-gandalf-greeting
    public const string DialoguePrefix = "lotr:dialogue-";

    // ── Save data keys ────────────────────────────────────────────
    public static class SaveKeys
    {
        public const string AlignmentPrefix = "lotr_alignment_";
        public const string QuestDonePrefix  = "lotr_quest_done_";
        public const string QuestActivePrefix = "lotr_quest_active_";
    }

    // ── Default alignment scores per faction ─────────────────────
    public static readonly IReadOnlyDictionary<string, int> DefaultAlignments =
        new Dictionary<string, int>
        {
            { Factions.Shire,       200 },
            { Factions.Gondor,        0 },
            { Factions.Rohan,         0 },
            { Factions.Rivendell,     0 },
            { Factions.Moria,         0 },
            { Factions.Isengard,   -500 },
            { Factions.Mordor,     -800 },
        };

    // ── Alignment thresholds ──────────────────────────────────────
    public static class Alignment
    {
        public const int Min         = -1000;
        public const int Max         =  1000;

        public const int ExaltedMin  =  750;  // 750..1000  = Exalted
        public const int ReveredMin  =  500;  // 500..749   = Revered
        public const int HonoredMin  =  250;  // 250..499   = Honored
        public const int FriendlyMin =    0;  // 0..249     = Friendly
        public const int NeutralMin  = -249;  // -249..-1   = Unfriendly
        public const int HostileMin  = -499;  // -499..-250 = Hostile
        public const int HatedMin    = -749;  // -749..-500 = Hated
        // -1000..-750 = Nemesis
    }
}
