using Lotr.Entities;

namespace Lotr.Entities.Humanoids;

/// <summary>
/// Base class for Orc NPCs: Mordor Orc, Isengard Orc, Gundabad Orc, Angmar Orc, Half-orc.
/// Hitbox x:0.6 y:1.8, eyeHeight 1.6.
/// Phase 10: patrol AI, aggro on low-alignment player, alignment impact on kill.
/// </summary>
public class EntityOrc : EntityRaceBase { }
