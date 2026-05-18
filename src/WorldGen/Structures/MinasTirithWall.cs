using Vintagestory.API.Server;

namespace Lotr.WorldGen.Structures;

/// <summary>
/// Placeholder for Minas Tirith outer wall segment.
/// Currently: L-shaped white limestone wall 3 thick x 20 tall x 40 long
///            with two corner towers.
/// TODO Phase 8: full city — 7 concentric tiers, Great Gate, citadel, Tower of Ecthelion.
/// </summary>
public class MinasTirithWall : StructureBase
{
    public MinasTirithWall(ICoreServerAPI api, IWorldGenBlockAccessor accessor)
        : base(api, accessor) { }

    public override void Generate(int ox, int surfaceY, int oz)
    {
        int by = surfaceY;
        const string stone = "game:cobblestone-limestone";
        const int wallH = 20;
        const int wallL = 40;

        // Main wall segment along Z axis
        Fill(ox,     by, oz,          ox + 2, by + wallH, oz + wallL, stone);

        // Return wall along X axis (L-shape)
        Fill(ox,     by, oz + wallL,  ox + wallL / 2, by + wallH, oz + wallL + 2, stone);

        // Corner tower 1 at (ox, oz)
        Fill(ox - 2, by, oz - 2,      ox + 4, by + wallH + 5, oz + 4, stone);
        Fill(ox - 1, by + 1, oz - 1,  ox + 3, by + wallH + 4, oz + 3, "game:air");

        // Corner tower 2 at (ox, oz+wallL)
        Fill(ox - 2, by, oz + wallL - 2, ox + 4, by + wallH + 5, oz + wallL + 4, stone);
        Fill(ox - 1, by + 1, oz + wallL - 1, ox + 3, by + wallH + 4, oz + wallL + 3, "game:air");

        // Gate opening in main wall
        Fill(ox, by, oz + wallL / 2 - 2, ox + 2, by + 6, oz + wallL / 2 + 2, "game:air");

        // Torches along wall
        for (int dz = 5; dz < wallL; dz += 8)
        {
            Set(ox + 3, by + 10, oz + dz, "game:torch-basic-up");
            Set(ox - 1, by + 10, oz + dz, "game:torch-basic-up");
        }
    }
}
