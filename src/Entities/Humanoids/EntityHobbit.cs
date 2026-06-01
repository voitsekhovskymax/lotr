using Lotr.Entities;

namespace Lotr.Entities.Humanoids;

/// <summary>
/// Base class for all Hobbit NPCs (Frodo, Bilbo, Sam, Merry, Pippin, shire-hobbit variants).
/// Hitbox x:0.4 y:1.1, eyeHeight 0.95.
/// Phase 2+: dialogue, alignment, faction reaction logic via EntityRaceBase.
/// </summary>
public class EntityHobbit : EntityRaceBase { }
