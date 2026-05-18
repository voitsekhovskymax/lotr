using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Lotr.Constants;

namespace Lotr.Entities.Humanoids;

/// <summary>
/// Base class for all Hobbit NPCs.
/// Phase 1: simple NPC that loads and exists in the world.
/// Phase 2+: add dialogue, alignment, faction reaction logic.
/// </summary>
public class EntityHobbit : EntityAgent
{
    public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
    {
        base.Initialize(properties, api, InChunkIndex3d);

        if (api.Side == EnumAppSide.Server)
        {
            api.Logger.VerboseDebug($"{LotrConstants.LogPrefix} Spawned {Code} at {Pos.XYZ}");
        }
    }
}
