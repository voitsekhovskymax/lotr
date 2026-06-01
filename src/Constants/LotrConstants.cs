using System.Collections.Generic;

namespace Lotr.Constants;

public static class LotrConstants
{
    public const string ModId     = "lotr";
    public const string LogPrefix = "[LOTR]";

    // ── Faction ids ───────────────────────────────────────────────
    public static class Factions
    {
        // Free Peoples — Men
        public const string Shire         = "lotr:faction-shire";
        public const string Gondor        = "lotr:faction-gondor";
        public const string Rohan         = "lotr:faction-rohan";
        public const string Bree          = "lotr:faction-bree";
        public const string Dale          = "lotr:faction-dale";
        public const string Esgaroth      = "lotr:faction-esgaroth";
        public const string Beornings     = "lotr:faction-beornings";
        public const string RangersNorth  = "lotr:faction-rangers-north";
        public const string Druedain      = "lotr:faction-druedain";

        // Free Peoples — Elves
        public const string Rivendell     = "lotr:faction-rivendell";
        public const string Lothlorien    = "lotr:faction-lothlorien";
        public const string Mirkwood      = "lotr:faction-mirkwood";
        public const string Lindon        = "lotr:faction-lindon";

        // Free Peoples — Dwarves
        public const string Erebor        = "lotr:faction-erebor";
        public const string IronHills     = "lotr:faction-iron-hills";
        public const string EredLuin      = "lotr:faction-ered-luin";
        public const string MoriaDwarves  = "lotr:faction-moria";

        // Forces of Evil — Mordor
        public const string Mordor        = "lotr:faction-mordor";
        public const string MordorOrcs    = "lotr:faction-mordor-orcs";
        public const string MordorUruks   = "lotr:faction-mordor-uruks";
        public const string OlogHai       = "lotr:faction-olog-hai";
        public const string Nazgul        = "lotr:faction-nazgul";

        // Forces of Evil — Isengard
        public const string Isengard      = "lotr:faction-isengard";
        public const string IsengardOrcs  = "lotr:faction-isengard-orcs";
        public const string Dunland       = "lotr:faction-dunland";
        public const string IsengardMen   = "lotr:faction-isengard-men";

        // Forces of Evil — Misty Mountains
        public const string GoblinsMoria  = "lotr:faction-goblins-moria";
        public const string Gundabad      = "lotr:faction-gundabad";
        public const string GoblinTown    = "lotr:faction-goblin-town";
        public const string Angmar        = "lotr:faction-angmar";

        // Forces of Evil — East & South
        public const string Harad         = "lotr:faction-harad";
        public const string Rhun          = "lotr:faction-rhun";
        public const string Umbar         = "lotr:faction-umbar";

        // Neutral & Ancient
        public const string Fangorn       = "lotr:faction-fangorn";
        public const string Eagles        = "lotr:faction-eagles";
        public const string Wargs         = "lotr:faction-wargs";
        public const string MirkwoodSpiders = "lotr:faction-mirkwood-spiders";
        public const string Oathbreakers  = "lotr:faction-oathbreakers";
        public const string Balrog        = "lotr:faction-balrog";
        public const string Bombadil      = "lotr:faction-bombadil";

        public static readonly IReadOnlyList<string> All = [
            Shire, Gondor, Rohan, Bree, Dale, Esgaroth, Beornings, RangersNorth, Druedain,
            Rivendell, Lothlorien, Mirkwood, Lindon,
            Erebor, IronHills, EredLuin, MoriaDwarves,
            Mordor, MordorOrcs, MordorUruks, OlogHai, Nazgul,
            Isengard, IsengardOrcs, Dunland, IsengardMen,
            GoblinsMoria, Gundabad, GoblinTown, Angmar,
            Harad, Rhun, Umbar,
            Fangorn, Eagles, Wargs, MirkwoodSpiders, Oathbreakers, Balrog, Bombadil
        ];

        public static readonly IReadOnlyList<string> Good = [
            Shire, Gondor, Rohan, Bree, Dale, Esgaroth, Beornings, RangersNorth, Druedain,
            Rivendell, Lothlorien, Mirkwood, Lindon,
            Erebor, IronHills, EredLuin
        ];

        public static readonly IReadOnlyList<string> Evil = [
            Mordor, MordorOrcs, MordorUruks, OlogHai, Nazgul,
            Isengard, IsengardOrcs, Dunland, IsengardMen,
            GoblinsMoria, Gundabad, GoblinTown, Angmar,
            Harad, Rhun, Umbar, Wargs, MirkwoodSpiders, Balrog
        ];

        // Factions shown by default in the UI (major ones only)
        public static readonly IReadOnlyList<string> Major = [
            Shire, Gondor, Rohan, Rivendell, Erebor,
            Mordor, Isengard, Angmar, Harad, Fangorn, Eagles
        ];
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

    public static class QuestPrefix
    {
        public const string Shire = "lotr:quest-shire-";
    }

    public const string DialoguePrefix = "lotr:dialogue-";

    // ── Save data keys ────────────────────────────────────────────
    public static class SaveKeys
    {
        public const string AlignmentPrefix  = "lotr_alignment_";
        public const string QuestDonePrefix  = "lotr_quest_done_";
        public const string QuestActivePrefix = "lotr_quest_active_";
        public const string FactionData      = "lotr_faction_data";
    }

    // ── Network channel ───────────────────────────────────────────
    public const string NetworkChannel = "lotr_align";

    // ── Default alignment scores per faction ─────────────────────
    public static readonly IReadOnlyDictionary<string, int> DefaultAlignments =
        new Dictionary<string, int>
        {
            { Factions.Shire,         200 },
            { Factions.Gondor,          0 },
            { Factions.Rohan,           0 },
            { Factions.Bree,           50 },
            { Factions.Dale,            0 },
            { Factions.Esgaroth,        0 },
            { Factions.Beornings,       0 },
            { Factions.RangersNorth,    0 },
            { Factions.Druedain,        0 },
            { Factions.Rivendell,       0 },
            { Factions.Lothlorien,      0 },
            { Factions.Mirkwood,        0 },
            { Factions.Lindon,          0 },
            { Factions.Erebor,          0 },
            { Factions.IronHills,       0 },
            { Factions.EredLuin,        0 },
            { Factions.MoriaDwarves,    0 },
            { Factions.Mordor,       -500 },
            { Factions.MordorOrcs,   -600 },
            { Factions.MordorUruks,  -600 },
            { Factions.OlogHai,      -700 },
            { Factions.Nazgul,       -800 },
            { Factions.Isengard,     -500 },
            { Factions.IsengardOrcs, -500 },
            { Factions.Dunland,      -200 },
            { Factions.IsengardMen,  -300 },
            { Factions.GoblinsMoria, -600 },
            { Factions.Gundabad,     -600 },
            { Factions.GoblinTown,   -600 },
            { Factions.Angmar,       -700 },
            { Factions.Harad,        -400 },
            { Factions.Rhun,         -400 },
            { Factions.Umbar,        -400 },
            { Factions.Fangorn,         0 },
            { Factions.Eagles,         50 },
            { Factions.Wargs,        -600 },
            { Factions.MirkwoodSpiders,-600 },
            { Factions.Oathbreakers, -300 },
            { Factions.Balrog,       -900 },
            { Factions.Bombadil,      100 },
        };

    // ── Alignment thresholds ──────────────────────────────────────
    public static class Alignment
    {
        public const int Min         = -1000;
        public const int Max         =  1000;

        public const int ExaltedMin  =  750;
        public const int ReveredMin  =  500;
        public const int HonoredMin  =  250;
        public const int FriendlyMin =    0;
        public const int NeutralMin  = -249;
        public const int HostileMin  = -499;
        public const int HatedMin    = -749;

        // Thresholds for gameplay effects
        public const int AggroThreshold  = -100;  // NPC becomes hostile below this
        public const int TradingMin      =  -50;  // Below this: no trading
        public const int BountyThreshold = -600;  // Below this: faction puts bounty
        public const int DisguiseBonus   =  200;  // Alignment boost while disguised
    }

    // ── Decay settings ────────────────────────────────────────────
    public static class Decay
    {
        public const int   IntervalMs   = 300_000; // 5 real minutes per tick
        public const float DecayRate    = 0.02f;   // 2% toward neutral per tick
        public const int   MinDecayStep = 1;       // Always decay at least 1 if non-zero
        public const int   DecayDeadzone = 5;      // Don't decay if within ±5 of default
    }

    // ── Alignment color scale (Tailwind red-500/green-500 gradient) ───
    // Maps a faction score to a hex color for UI rendering.
    // Negative → red scale, positive → green scale, zero → white.
    public static class AlignmentColors
    {
        public const string Neutral = "#ffffff";

        // Red scale: score < 0, deepens toward -1000
        private static readonly (int threshold, string color)[] NegativeScale =
        [
            (-900, "#450a0a"),
            (-800, "#7f1d1d"),
            (-700, "#991b1b"),
            (-600, "#b91c1c"),
            (-500, "#dc2626"),
            (-400, "#ef4444"),
            (-300, "#f87171"),
            (-200, "#fca5a5"),
            (-100, "#fecaca"),
            ( -50, "#fee2e2"),
            (   0, "#fef2f2"),
        ];

        // Green scale: score > 0, deepens toward +1000
        private static readonly (int threshold, string color)[] PositiveScale =
        [
            ( 900, "#052e16"),
            ( 800, "#14532d"),
            ( 700, "#166534"),
            ( 600, "#15803d"),
            ( 500, "#16a34a"),
            ( 400, "#22c55e"),
            ( 300, "#4ade80"),
            ( 200, "#86efac"),
            ( 100, "#bbf7d0"),
            (  50, "#dcfce7"),
            (   0, "#f0fdf4"),
        ];

        public static string ForScore(int score)
        {
            if (score == 0) return Neutral;

            if (score < 0)
            {
                foreach (var (threshold, color) in NegativeScale)
                    if (score <= threshold) return color;
                return NegativeScale[^1].color;
            }

            foreach (var (threshold, color) in PositiveScale)
                if (score >= threshold) return color;
            return PositiveScale[^1].color;
        }
    }
}
