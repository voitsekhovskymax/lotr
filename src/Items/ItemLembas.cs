using Vintagestory.API.Common;

namespace Lotr.Items;

/// <summary>
/// Lembas bread — Elven waybread.
/// Phase 1: loads as a usable food item.
/// Phase 1b: satiation values tuned.
/// Phase 1c: custom buff/effect logic after item framework is stable.
/// </summary>
public class ItemLembas : Item
{
    // Phase 1: no custom logic yet — food values are handled in the JSON.
    // Override EatHeldItem() in Phase 1c to add custom buff effects.
}
