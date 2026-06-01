using System.Collections.Concurrent;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace Lotr.Utilities;

/// <summary>
/// Maps entity codes → race and faction without per-tick allocations.
/// One cache entry per entity *type* (not per instance), so memory stays small
/// even with hundreds of spawned entities.
/// </summary>
public enum LotrRace
{
    Unknown, Hobbit, Human, Elf, Dwarf, Orc, Goblin, Uruk, Troll, Maiar, Creature
}

public static class EntityModelCache
{
    private static readonly ConcurrentDictionary<string, string?> _factionCache = new();
    private static readonly ConcurrentDictionary<string, LotrRace> _raceCache = new();

    // Full entity-code → race map — extend as new races are added
    private static readonly Dictionary<string, LotrRace> _codeToRace = new()
    {
        // Hobbits
        ["lotr:shire-hobbit"]          = LotrRace.Hobbit,
        ["lotr:shire-hobbit-male"]     = LotrRace.Hobbit,
        ["lotr:shire-hobbit-female"]   = LotrRace.Hobbit,
        ["lotr:frodo"]                 = LotrRace.Hobbit,
        ["lotr:bilbo"]                 = LotrRace.Hobbit,
        ["lotr:sam"]                   = LotrRace.Hobbit,
        ["lotr:merry"]                 = LotrRace.Hobbit,
        ["lotr:pippin"]                = LotrRace.Hobbit,
        // Humans
        ["lotr:aragorn"]               = LotrRace.Human,
        ["lotr:boromir"]               = LotrRace.Human,
        ["lotr:gondorian"]             = LotrRace.Human,
        ["lotr:rohirrim"]              = LotrRace.Human,
        ["lotr:bree-man"]              = LotrRace.Human,
        ["lotr:ranger"]                = LotrRace.Human,
        ["lotr:daleman"]               = LotrRace.Human,
        ["lotr:lakeman"]               = LotrRace.Human,
        ["lotr:beorning"]              = LotrRace.Human,
        ["lotr:haradrim"]              = LotrRace.Human,
        ["lotr:easterling"]            = LotrRace.Human,
        ["lotr:dunlending"]            = LotrRace.Human,
        ["lotr:corsair"]               = LotrRace.Human,
        ["lotr:wose"]                  = LotrRace.Human,
        // Elves
        ["lotr:legolas"]               = LotrRace.Elf,
        ["lotr:rivendell-elf"]         = LotrRace.Elf,
        ["lotr:galadhrim"]             = LotrRace.Elf,
        ["lotr:wood-elf"]              = LotrRace.Elf,
        ["lotr:lindon-elf"]            = LotrRace.Elf,
        // Dwarves
        ["lotr:gimli"]                 = LotrRace.Dwarf,
        ["lotr:erebor-dwarf"]          = LotrRace.Dwarf,
        ["lotr:iron-hills-dwarf"]      = LotrRace.Dwarf,
        ["lotr:ered-luin-dwarf"]       = LotrRace.Dwarf,
        ["lotr:moria-dwarf"]           = LotrRace.Dwarf,
        // Orcs
        ["lotr:mordor-orc"]            = LotrRace.Orc,
        ["lotr:isengard-orc"]          = LotrRace.Orc,
        ["lotr:gundabad-orc"]          = LotrRace.Orc,
        ["lotr:angmar-orc"]            = LotrRace.Orc,
        ["lotr:half-orc"]              = LotrRace.Orc,
        // Uruks
        ["lotr:isengard-uruk"]         = LotrRace.Uruk,
        ["lotr:black-uruk"]            = LotrRace.Uruk,
        // Goblins
        ["lotr:moria-goblin"]          = LotrRace.Goblin,
        ["lotr:goblin-town-goblin"]    = LotrRace.Goblin,
        // Trolls
        ["lotr:olog-hai"]              = LotrRace.Troll,
        // Maiar
        ["lotr:gandalf"]               = LotrRace.Maiar,
        ["lotr:tom-bombadil"]          = LotrRace.Maiar,
        // Creatures
        ["lotr:warg"]                  = LotrRace.Creature,
        ["lotr:ent"]                   = LotrRace.Creature,
        ["lotr:great-eagle"]           = LotrRace.Creature,
        ["lotr:giant-spider"]          = LotrRace.Creature,
        ["lotr:mumak"]                 = LotrRace.Creature,
        ["lotr:mearas"]                = LotrRace.Creature,
        ["lotr:creban"]                = LotrRace.Creature,
        ["lotr:ringwraith"]            = LotrRace.Creature,
        ["lotr:balrog"]                = LotrRace.Creature,
        ["lotr:oathbreaker"]           = LotrRace.Creature,
    };

    /// <summary>Returns the faction id from entity attributes, cached per entity type code.</summary>
    public static string? GetFaction(Entity entity)
    {
        string code = entity.Code.ToString();
        return _factionCache.GetOrAdd(code,
            _ => entity.Properties.Attributes?["faction"].AsString());
    }

    /// <summary>Returns the LOTR race for the entity, cached per entity type code.</summary>
    public static LotrRace GetRace(Entity entity)
    {
        string code = entity.Code.ToString();
        return _raceCache.GetOrAdd(code,
            c => _codeToRace.TryGetValue(c, out var race) ? race : LotrRace.Unknown);
    }

    /// <summary>Call on world dispose to free cached data.</summary>
    internal static void Clear()
    {
        _factionCache.Clear();
        _raceCache.Clear();
    }
}
