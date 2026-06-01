using System;
using Lotr.Constants;
using Lotr.Utilities;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace Lotr.Entities;

// Universal base class for all LOTR race entities: hobbits, humans, elves,
// dwarves, orcs, uruks, goblins, trolls, maiar, and creatures.
// Caches faction and race on first initialize — zero per-tick allocations.
public abstract class EntityRaceBase : EntityAgent
{
    // Fired server-side when a LOTR entity is killed.
    // Subscribed by CombatAlignmentHandler in LotrModSystem.
    public static event Action<Entity, DamageSource?>? Killed;

    public string? FactionId { get; private set; }
    public LotrRace Race { get; private set; }

    public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
    {
        base.Initialize(properties, api, InChunkIndex3d);

        FactionId = EntityModelCache.GetFaction(this);
        Race      = EntityModelCache.GetRace(this);

        if (api.Side == EnumAppSide.Server)
            api.Logger.VerboseDebug($"{LotrConstants.LogPrefix} Spawned {Code} [{Race}] faction={FactionId ?? "none"} at {Pos.XYZ}");
    }

    public override void Die(EnumDespawnReason reason = EnumDespawnReason.Death, DamageSource? damageSourceInfo = null)
    {
        base.Die(reason, damageSourceInfo);

        // Fire kill event only on server, only for actual death (not despawn/unload)
        if (Api?.Side == EnumAppSide.Server && reason == EnumDespawnReason.Death)
            Killed?.Invoke(this, damageSourceInfo);
    }
}
