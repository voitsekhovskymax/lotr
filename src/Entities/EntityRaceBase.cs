using Lotr.Constants;
using Lotr.Utilities;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace Lotr.Entities;

/// <summary>
/// Universal base class for all LOTR race entities: hobbits, humans, elves,
/// dwarves, orcs, uruks, goblins, trolls, maiar, and creatures.
/// Caches faction and race on first initialize — zero per-tick allocations.
/// </summary>
public abstract class EntityRaceBase : EntityAgent
{
    /// <summary>Cached faction id, e.g. "lotr:faction-shire". Null if entity has no faction attribute.</summary>
    public string? FactionId { get; private set; }

    /// <summary>Cached race enum for fast comparisons and AI branching.</summary>
    public LotrRace Race { get; private set; }

    public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
    {
        base.Initialize(properties, api, InChunkIndex3d);

        // Both values pulled from the static cache — no per-instance allocation after first spawn of a type
        FactionId = EntityModelCache.GetFaction(this);
        Race      = EntityModelCache.GetRace(this);

        if (api.Side == EnumAppSide.Server)
            api.Logger.VerboseDebug($"{LotrConstants.LogPrefix} Spawned {Code} [{Race}] faction={FactionId ?? "none"} at {Pos.XYZ}");
    }
}
