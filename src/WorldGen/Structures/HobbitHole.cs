using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Lotr.WorldGen.Structures;

/// <summary>
/// A detailed hobbit hole dug into a hillside.
///
/// Layout (top-down, Z goes into hill):
///
///   [entrance arch] → [corridor 3 deep] → [main hall 5x5]
///                                               ↓
///                                         [bedroom 3x3] [pantry 3x3]
///
/// Origin point = base of the round door (outside face of hill).
/// The structure digs in the -Z direction (into the hill).
/// </summary>
public class HobbitHole : StructureBase
{
    public HobbitHole(ICoreServerAPI api, IWorldGenBlockAccessor accessor)
        : base(api, accessor) { }

    public override void Generate(int ox, int surfaceY, int oz)
    {
        int by = surfaceY; // base Y (ground level at entrance)

        // ── 1. Entrance mound smoothing ─────────────────────────────
        // Flatten a small area in front of the door
        for (int dx = -3; dx <= 3; dx++)
        for (int dz = -2; dz <= 0; dz++)
        {
            Set(ox + dx, by - 1, oz + dz, "game:soil-medium-none");
            Air(ox + dx, by,     oz + dz);
            Air(ox + dx, by + 1, oz + dz);
        }

        // ── 2. Round door arch (exterior face at Z=oz) ──────────────
        // Arch shape: 3 wide x 3 tall, rounded top
        //   [ ][ ][W][W][W][ ][ ]
        //   [ ][W][D][D][W][W][ ]   W=wall, D=door opening, A=arch
        //   [W][W][D][D][W][W][W]
        //   [W][G][G][G][G][G][W]   G=grass path in front

        // Arch frame — cobblestone
        for (int dy = 0; dy <= 3; dy++)
        {
            Set(ox - 2, by + dy, oz, "game:cobblestone-limestone");
            Set(ox + 2, by + dy, oz, "game:cobblestone-limestone");
        }
        // Arch top
        Set(ox - 1, by + 3, oz, "game:cobblestone-limestone");
        Set(ox,     by + 4, oz, "game:cobblestone-limestone");
        Set(ox + 1, by + 3, oz, "game:cobblestone-limestone");

        // Door opening (air) — 2 wide, 3 tall
        Air(ox - 1, by,     oz);
        Air(ox,     by,     oz);
        Air(ox + 1, by,     oz);
        Air(ox - 1, by + 1, oz);
        Air(ox,     by + 1, oz);
        Air(ox + 1, by + 1, oz);
        Air(ox - 1, by + 2, oz);
        Air(ox,     by + 2, oz);
        Air(ox + 1, by + 2, oz);

        // Round green door (left leaf = ox-1, right leaf = ox+1)
        Set(ox - 1, by,     oz - 1, "game:door-sleek-windowed-aged-south-opened-left-lower");
        Set(ox - 1, by + 1, oz - 1, "game:door-sleek-windowed-aged-south-opened-left-upper");
        Set(ox + 1, by,     oz - 1, "game:door-sleek-windowed-aged-south-opened-right-lower");
        Set(ox + 1, by + 1, oz - 1, "game:door-sleek-windowed-aged-south-opened-right-upper");

        // ── 3. Grass path in front of door ──────────────────────────
        for (int dx = -3; dx <= 3; dx++)
        for (int dz = 1; dz <= 4; dz++)
            Set(ox + dx, by - 1, oz + dz, "game:gravel-granite");

        // ── 4. Corridor (digs into hill, Z = oz-1 to oz-4) ──────────
        for (int dz = 1; dz <= 4; dz++)
        {
            // Hollow 3 wide, 3 tall corridor
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = 0; dy <= 2; dy++)
                Air(ox + dx, by + dy, oz - dz);

            // Floor
            for (int dx = -1; dx <= 1; dx++)
                Set(ox + dx, by - 1, oz - dz, "game:plank-oak-ns");

            // Walls
            Set(ox - 2, by,     oz - dz, "game:cobblestone-limestone");
            Set(ox - 2, by + 1, oz - dz, "game:cobblestone-limestone");
            Set(ox - 2, by + 2, oz - dz, "game:cobblestone-limestone");
            Set(ox + 2, by,     oz - dz, "game:cobblestone-limestone");
            Set(ox + 2, by + 1, oz - dz, "game:cobblestone-limestone");
            Set(ox + 2, by + 2, oz - dz, "game:cobblestone-limestone");

            // Ceiling
            for (int dx = -1; dx <= 1; dx++)
                Set(ox + dx, by + 3, oz - dz, "game:plank-oak-ew");
        }

        // ── 5. Main hall (5 wide x 5 deep x 4 tall, Z = oz-5 to oz-9) ─
        int hx1 = ox - 3, hx2 = ox + 3;
        int hz1 = oz - 5, hz2 = oz - 9;
        int hy1 = by - 1, hy2 = by + 4;

        // Hollow box — limestone walls
        Box(hx1, hy1, hz2, hx2, hy2, hz1, "game:cobblestone-limestone");

        // Floor — oak planks
        Fill(hx1 + 1, hy1, hz2 + 1, hx2 - 1, hy1, hz1 - 1, "game:plank-oak-ns");

        // Ceiling beams — log every 2 blocks
        for (int bz = hz2 + 1; bz <= hz1 - 1; bz += 2)
            Fill(hx1, hy2, bz, hx2, hy2, bz, "game:log-oak-ud");

        // Open corridor connection (knock through wall at z=oz-5)
        for (int dx = -1; dx <= 1; dx++)
        for (int dy = 0; dy <= 2; dy++)
            Air(ox + dx, by + dy, oz - 5);

        // Fireplace on far wall (north wall, z = oz-9)
        Set(ox, hy1 + 1, hz2 + 1, "game:firepit");
        Set(ox, hy2,     hz2 + 1, "game:log-oak-ud");  // chimney start

        // Chimney extends up through roof
        for (int dy = 1; dy <= 5; dy++)
            Set(ox, hy2 + dy, hz2 + 1, "game:cobblestone-limestone");

        // Candles / torches on walls
        Set(ox - 2, by + 2, oz - 6, "game:torch-basic-up");
        Set(ox + 2, by + 2, oz - 6, "game:torch-basic-up");
        Set(ox - 2, by + 2, oz - 8, "game:torch-basic-up");
        Set(ox + 2, by + 2, oz - 8, "game:torch-basic-up");

        // Table and chairs (rough approximation)
        Set(ox,     by,     oz - 7, "game:table-normal-ns");
        Set(ox - 1, by,     oz - 7, "game:stool-oak");
        Set(ox + 1, by,     oz - 7, "game:stool-oak");

        // ── 6. Bedroom (east side, Z = oz-5 to oz-7, X = ox+3 to ox+6) ─
        int brx1 = ox + 4, brx2 = ox + 6;
        int brz1 = oz - 5, brz2 = oz - 7;
        Box(brx1 - 1, hy1, brz2, brx2, by + 3, brz1, "game:cobblestone-limestone");
        Fill(brx1, hy1, brz2 + 1, brx2 - 1, hy1, brz1 - 1, "game:plank-oak-ns");

        // Doorway from hall to bedroom
        Air(brx1 - 1, by,     oz - 6);
        Air(brx1 - 1, by + 1, oz - 6);
        Air(brx1 - 1, by + 2, oz - 6);

        // Bed
        Set(brx1 + 1, hy1 + 1, brz2 + 1, "game:bed-wood-north-feet");
        Set(brx1 + 1, hy1 + 1, brz2 + 2, "game:bed-wood-north-head");

        // Candle
        Set(brx1, by + 1, brz2 + 1, "game:candle-lit");

        // ── 7. Pantry (west side, Z = oz-5 to oz-7, X = ox-3 to ox-6) ─
        int ptx1 = ox - 6, ptx2 = ox - 4;
        int ptz1 = oz - 5, ptz2 = oz - 7;
        Box(ptx1, hy1, ptz2, ptx2 + 1, by + 3, ptz1, "game:cobblestone-limestone");
        Fill(ptx1 + 1, hy1, ptz2 + 1, ptx2, hy1, ptz1 - 1, "game:plank-oak-ns");

        // Doorway from hall to pantry
        Air(ptx2 + 1, by,     oz - 6);
        Air(ptx2 + 1, by + 1, oz - 6);
        Air(ptx2 + 1, by + 2, oz - 6);

        // Storage chests
        Set(ptx1 + 1, hy1 + 1, ptz2 + 1, "game:chest-east");
        Set(ptx1 + 1, hy1 + 1, ptz2 + 2, "game:chest-east");
        Set(ptx2,     hy1 + 1, ptz2 + 1, "game:barrel");

        // ── 8. Round window above entrance ──────────────────────────
        Set(ox, by + 3, oz, "game:glass-plain");

        // ── 9. Cover roof with grass mound ──────────────────────────
        for (int dx = -4; dx <= 4; dx++)
        for (int dz = -10; dz <= -4; dz++)
        {
            // Add soil on top of the structure to make it look like a hill
            var roofPos = new BlockPos(ox + dx, by + 5, oz + dz);
            if (Accessor.GetBlock(roofPos).Id == 0)
                Set(ox + dx, by + 5, oz + dz, "game:soil-medium-tall");
        }
    }
}
