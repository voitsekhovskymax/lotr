using System.Collections.Generic;
using Lotr.Constants;

namespace Lotr.Factions;

// Defines how killing an entity of a given faction affects player alignment.
// Entry: killedFaction → list of (factionId, delta) pairs.
// Positive delta = gain, negative = lose.
public static class FactionRelationMatrix
{
    // Alliance groups — used to propagate alignment changes across allies
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Allies =
        new Dictionary<string, IReadOnlyList<string>>
        {
            [LotrConstants.Factions.Gondor]       = [LotrConstants.Factions.Rohan, LotrConstants.Factions.RangersNorth, LotrConstants.Factions.Dale],
            [LotrConstants.Factions.Rohan]         = [LotrConstants.Factions.Gondor, LotrConstants.Factions.Beornings],
            [LotrConstants.Factions.Rivendell]     = [LotrConstants.Factions.Lothlorien, LotrConstants.Factions.Lindon, LotrConstants.Factions.Mirkwood],
            [LotrConstants.Factions.Lothlorien]    = [LotrConstants.Factions.Rivendell, LotrConstants.Factions.Lindon],
            [LotrConstants.Factions.Erebor]        = [LotrConstants.Factions.IronHills, LotrConstants.Factions.EredLuin, LotrConstants.Factions.Dale],
            [LotrConstants.Factions.Mordor]        = [LotrConstants.Factions.MordorOrcs, LotrConstants.Factions.MordorUruks, LotrConstants.Factions.Nazgul],
            [LotrConstants.Factions.MordorOrcs]    = [LotrConstants.Factions.Mordor, LotrConstants.Factions.OlogHai],
            [LotrConstants.Factions.Isengard]      = [LotrConstants.Factions.IsengardOrcs, LotrConstants.Factions.Dunland],
            [LotrConstants.Factions.GoblinsMoria]  = [LotrConstants.Factions.Gundabad, LotrConstants.Factions.GoblinTown],
        };

    // Enemies — faction A considers faction B an enemy
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Enemies =
        new Dictionary<string, IReadOnlyList<string>>
        {
            [LotrConstants.Factions.Gondor]       = [LotrConstants.Factions.Mordor, LotrConstants.Factions.Isengard, LotrConstants.Factions.Harad, LotrConstants.Factions.Rhun],
            [LotrConstants.Factions.Rohan]         = [LotrConstants.Factions.Isengard, LotrConstants.Factions.Dunland, LotrConstants.Factions.Mordor],
            [LotrConstants.Factions.Shire]         = [LotrConstants.Factions.Mordor, LotrConstants.Factions.Isengard, LotrConstants.Factions.GoblinsMoria],
            [LotrConstants.Factions.Rivendell]     = [LotrConstants.Factions.Mordor, LotrConstants.Factions.Angmar, LotrConstants.Factions.Isengard],
            [LotrConstants.Factions.Erebor]        = [LotrConstants.Factions.GoblinsMoria, LotrConstants.Factions.Mordor, LotrConstants.Factions.Gundabad],
            [LotrConstants.Factions.Mordor]        = [LotrConstants.Factions.Gondor, LotrConstants.Factions.Rohan, LotrConstants.Factions.Rivendell, LotrConstants.Factions.Erebor],
            [LotrConstants.Factions.Isengard]      = [LotrConstants.Factions.Gondor, LotrConstants.Factions.Rohan, LotrConstants.Factions.Shire],
        };

    // Kill rewards: killing an entity of this faction gives these alignment changes.
    // Convention: the first entry is the faction you lose rep with (negative delta).
    // Subsequent entries are ally-of-your-enemy gains (positive delta).
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<(string factionId, int delta)>> KillRewards =
        new Dictionary<string, IReadOnlyList<(string, int)>>
        {
            // Killing Good NPCs
            [LotrConstants.Factions.Gondor] =
            [
                (LotrConstants.Factions.Gondor,     -80),
                (LotrConstants.Factions.Rohan,      -30),
                (LotrConstants.Factions.Shire,      -30),
                (LotrConstants.Factions.Rivendell,  -20),
                (LotrConstants.Factions.Mordor,     +20),
                (LotrConstants.Factions.Isengard,   +10),
            ],
            [LotrConstants.Factions.Rohan] =
            [
                (LotrConstants.Factions.Rohan,      -80),
                (LotrConstants.Factions.Gondor,     -30),
                (LotrConstants.Factions.Shire,      -20),
                (LotrConstants.Factions.Isengard,   +20),
                (LotrConstants.Factions.Dunland,    +15),
            ],
            [LotrConstants.Factions.Shire] =
            [
                (LotrConstants.Factions.Shire,      -80),
                (LotrConstants.Factions.Gondor,     -20),
                (LotrConstants.Factions.Rivendell,  -15),
                (LotrConstants.Factions.Mordor,     +10),
            ],
            [LotrConstants.Factions.Rivendell] =
            [
                (LotrConstants.Factions.Rivendell,  -80),
                (LotrConstants.Factions.Lothlorien, -30),
                (LotrConstants.Factions.Gondor,     -20),
                (LotrConstants.Factions.Mordor,     +20),
                (LotrConstants.Factions.Angmar,     +15),
            ],
            [LotrConstants.Factions.Lothlorien] =
            [
                (LotrConstants.Factions.Lothlorien, -80),
                (LotrConstants.Factions.Rivendell,  -30),
                (LotrConstants.Factions.Mordor,     +15),
            ],
            [LotrConstants.Factions.Erebor] =
            [
                (LotrConstants.Factions.Erebor,     -80),
                (LotrConstants.Factions.IronHills,  -20),
                (LotrConstants.Factions.EredLuin,   -20),
                (LotrConstants.Factions.GoblinsMoria, +15),
                (LotrConstants.Factions.Mordor,     +10),
            ],

            // Killing Evil NPCs
            [LotrConstants.Factions.MordorOrcs] =
            [
                (LotrConstants.Factions.MordorOrcs, -80),
                (LotrConstants.Factions.Mordor,     -40),
                (LotrConstants.Factions.Gondor,     +20),
                (LotrConstants.Factions.Rohan,      +15),
                (LotrConstants.Factions.Shire,      +10),
                (LotrConstants.Factions.Rivendell,  +10),
                (LotrConstants.Factions.Erebor,     +5),
            ],
            [LotrConstants.Factions.Mordor] =
            [
                (LotrConstants.Factions.Mordor,     -100),
                (LotrConstants.Factions.MordorOrcs, -50),
                (LotrConstants.Factions.Gondor,     +25),
                (LotrConstants.Factions.Rohan,      +20),
                (LotrConstants.Factions.Rivendell,  +20),
            ],
            [LotrConstants.Factions.MordorUruks] =
            [
                (LotrConstants.Factions.MordorUruks,-80),
                (LotrConstants.Factions.Mordor,     -30),
                (LotrConstants.Factions.Gondor,     +15),
                (LotrConstants.Factions.Rohan,      +10),
            ],
            [LotrConstants.Factions.OlogHai] =
            [
                (LotrConstants.Factions.OlogHai,    -60),
                (LotrConstants.Factions.Mordor,     -30),
                (LotrConstants.Factions.Gondor,     +30),
                (LotrConstants.Factions.Rohan,      +25),
            ],
            [LotrConstants.Factions.Nazgul] =
            [
                (LotrConstants.Factions.Nazgul,     -60),
                (LotrConstants.Factions.Mordor,     -50),
                (LotrConstants.Factions.Gondor,     +50),
                (LotrConstants.Factions.Rivendell,  +40),
                (LotrConstants.Factions.Shire,      +30),
            ],
            [LotrConstants.Factions.Isengard] =
            [
                (LotrConstants.Factions.Isengard,   -80),
                (LotrConstants.Factions.IsengardOrcs,-30),
                (LotrConstants.Factions.Gondor,     +20),
                (LotrConstants.Factions.Rohan,      +25),
                (LotrConstants.Factions.Shire,      +10),
            ],
            [LotrConstants.Factions.IsengardOrcs] =
            [
                (LotrConstants.Factions.IsengardOrcs,-80),
                (LotrConstants.Factions.Isengard,   -40),
                (LotrConstants.Factions.Rohan,      +20),
                (LotrConstants.Factions.Gondor,     +15),
            ],
            [LotrConstants.Factions.Dunland] =
            [
                (LotrConstants.Factions.Dunland,    -80),
                (LotrConstants.Factions.Isengard,   -20),
                (LotrConstants.Factions.Rohan,      +20),
                (LotrConstants.Factions.Gondor,     +10),
            ],
            [LotrConstants.Factions.GoblinsMoria] =
            [
                (LotrConstants.Factions.GoblinsMoria,-80),
                (LotrConstants.Factions.Gundabad,   -20),
                (LotrConstants.Factions.Erebor,     +20),
                (LotrConstants.Factions.MoriaDwarves,+15),
                (LotrConstants.Factions.Gondor,     +10),
            ],
            [LotrConstants.Factions.Gundabad] =
            [
                (LotrConstants.Factions.Gundabad,   -80),
                (LotrConstants.Factions.GoblinsMoria,-20),
                (LotrConstants.Factions.Erebor,     +20),
                (LotrConstants.Factions.Gondor,     +10),
            ],
            [LotrConstants.Factions.GoblinTown] =
            [
                (LotrConstants.Factions.GoblinTown, -80),
                (LotrConstants.Factions.Erebor,     +15),
                (LotrConstants.Factions.Shire,      +10),
            ],
            [LotrConstants.Factions.Angmar] =
            [
                (LotrConstants.Factions.Angmar,     -80),
                (LotrConstants.Factions.Mordor,     -30),
                (LotrConstants.Factions.RangersNorth,+25),
                (LotrConstants.Factions.Rivendell,  +20),
                (LotrConstants.Factions.Gondor,     +15),
            ],
            [LotrConstants.Factions.Harad] =
            [
                (LotrConstants.Factions.Harad,      -80),
                (LotrConstants.Factions.Gondor,     +20),
                (LotrConstants.Factions.Rohan,      +10),
            ],
            [LotrConstants.Factions.Rhun] =
            [
                (LotrConstants.Factions.Rhun,       -80),
                (LotrConstants.Factions.Gondor,     +20),
                (LotrConstants.Factions.Erebor,     +10),
            ],
            [LotrConstants.Factions.Umbar] =
            [
                (LotrConstants.Factions.Umbar,      -80),
                (LotrConstants.Factions.Gondor,     +20),
                (LotrConstants.Factions.Shire,      +10),
            ],
            [LotrConstants.Factions.Wargs] =
            [
                (LotrConstants.Factions.Wargs,      -60),
                (LotrConstants.Factions.Rohan,      +15),
                (LotrConstants.Factions.Beornings,  +20),
            ],
            [LotrConstants.Factions.MirkwoodSpiders] =
            [
                (LotrConstants.Factions.MirkwoodSpiders,-60),
                (LotrConstants.Factions.Mirkwood,   +20),
                (LotrConstants.Factions.Erebor,     +10),
            ],
            [LotrConstants.Factions.Balrog] =
            [
                (LotrConstants.Factions.Balrog,     -60),
                (LotrConstants.Factions.Mordor,     -50),
                (LotrConstants.Factions.Gondor,     +100),
                (LotrConstants.Factions.Rivendell,  +80),
                (LotrConstants.Factions.Erebor,     +80),
                (LotrConstants.Factions.Shire,      +50),
            ],
        };

    // Threshold below which an NPC of this faction treats the player as hostile
    public static readonly IReadOnlyDictionary<string, int> AggroThresholds =
        new Dictionary<string, int>
        {
            // Good factions: friendly by default, aggro if alignment goes negative
            [LotrConstants.Factions.Gondor]       = -100,
            [LotrConstants.Factions.Rohan]         = -100,
            [LotrConstants.Factions.Shire]         = -50,
            [LotrConstants.Factions.Rivendell]     = -100,
            [LotrConstants.Factions.Lothlorien]    = -150,
            [LotrConstants.Factions.Erebor]        = -100,
            [LotrConstants.Factions.MoriaDwarves]  = -100,
            [LotrConstants.Factions.Mirkwood]      = -150,
            [LotrConstants.Factions.Beornings]     = -50,
            [LotrConstants.Factions.RangersNorth]  = -100,
            [LotrConstants.Factions.Bree]          = -50,
            [LotrConstants.Factions.Eagles]        = -200,
            [LotrConstants.Factions.Fangorn]       = -300,
            [LotrConstants.Factions.Bombadil]      = int.MinValue, // never aggros

            // Evil factions: aggro unless player has HIGH alignment with them
            [LotrConstants.Factions.MordorOrcs]    = 100,
            [LotrConstants.Factions.Mordor]        = 200,
            [LotrConstants.Factions.MordorUruks]   = 100,
            [LotrConstants.Factions.OlogHai]       = 300,
            [LotrConstants.Factions.Nazgul]        = 500,
            [LotrConstants.Factions.Isengard]      = 100,
            [LotrConstants.Factions.IsengardOrcs]  = 100,
            [LotrConstants.Factions.Dunland]       = 0,
            [LotrConstants.Factions.IsengardMen]   = 0,
            [LotrConstants.Factions.GoblinsMoria]  = 100,
            [LotrConstants.Factions.Gundabad]      = 100,
            [LotrConstants.Factions.GoblinTown]    = 100,
            [LotrConstants.Factions.Angmar]        = 200,
            [LotrConstants.Factions.Harad]         = 0,
            [LotrConstants.Factions.Rhun]          = 0,
            [LotrConstants.Factions.Umbar]         = 0,
            [LotrConstants.Factions.Wargs]         = 100,
            [LotrConstants.Factions.MirkwoodSpiders] = 100,
            [LotrConstants.Factions.Oathbreakers]  = -100,
            [LotrConstants.Factions.Balrog]        = int.MaxValue, // always aggros
        };

    // Returns true if a player with the given score should be treated as hostile by this faction
    public static bool IsHostile(string factionId, int playerScore)
    {
        if (!AggroThresholds.TryGetValue(factionId, out int threshold))
            return false;
        // Evil factions: aggro when playerScore < threshold (player isn't allied enough)
        // Good factions: aggro when playerScore < threshold (player lost reputation)
        return LotrConstants.Factions.Evil.Contains(factionId)
            ? playerScore < threshold
            : playerScore < threshold;
    }
}
