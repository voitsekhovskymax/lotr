using Vintagestory.API.Server;

namespace Lotr.WorldGen.Structures;

/// <summary>
/// Placeholder for Orthanc tower (Isengard).
/// Currently: solid obsidian-like pillar 5x40x5 with a platform on top.
/// TODO Phase 8: replace with detailed schematic — fluted black stone tower,
///               balcony, interior spiral staircase, palantir chamber.
/// </summary>
public class OrthanctTower : StructureBase
{
    public OrthanctTower(ICoreServerAPI api, IWorldGenBlockAccessor accessor)
        : base(api, accessor) { }

    public override void Generate(int ox, int surfaceY, int oz)
    {
        int by = surfaceY;

        // Base foundation 7x7
        Fill(ox - 3, by - 1, oz - 3, ox + 3, by - 1, oz + 3, "game:rock-basalt");

        // Pillar 5x5 x 40 tall
        for (int dy = 0; dy <= 40; dy++)
            Fill(ox - 2, by + dy, oz - 2, ox + 2, by + dy, oz + 2,
                 dy % 5 == 0 ? "game:cobblestone-basalt" : "game:rock-basalt");

        // Hollow interior from ground to top
        for (int dy = 1; dy <= 39; dy++)
            Fill(ox - 1, by + dy, oz - 1, ox + 1, by + dy, oz + 1, "game:air");

        // Crenellated top platform
        for (int dx = -3; dx <= 3; dx++)
        for (int dz = -3; dz <= 3; dz++)
            Set(ox + dx, by + 41, oz + dz, "game:rock-basalt");

        // Merlons (battlements)
        for (int i = -3; i <= 3; i += 2)
        {
            Set(ox + i, by + 42, oz - 3, "game:rock-basalt");
            Set(ox + i, by + 42, oz + 3, "game:rock-basalt");
            Set(ox - 3, by + 42, oz + i, "game:rock-basalt");
            Set(ox + 3, by + 42, oz + i, "game:rock-basalt");
        }
    }
}
