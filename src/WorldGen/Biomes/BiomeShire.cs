using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Lotr.WorldGen.Biomes;

/// <summary>
/// Handles Shire-specific worldgen: rich topsoil, sub-surface humus, and
/// scattered wildflowers. Called from LotrWorldGen during the Vegetation pass.
/// </summary>
public class BiomeShire
{
    int idSoilRich;      // top surface
    int idSoilMed;       // sub-surface (1 block below top)
    int idFlowerYellow;  // dandelion / buttercup
    int idFlowerWhite;   // oxeye daisy / white flower
    int idTallGrass;     // tall grass patch

    public int SurfaceBlock => idSoilRich;

    public void ResolveBlocks(ICoreServerAPI api)
    {
        idSoilRich    = Block(api, "game:soil-rich-normal")    ?? Block(api, "game:soil-medium-normal") ?? 0;
        idSoilMed     = Block(api, "game:soil-medium-normal")  ?? 0;
        idFlowerYellow = Block(api, "game:flower-buttercup-free") ?? Block(api, "game:flower-goldenrod-free") ?? 0;
        idFlowerWhite  = Block(api, "game:flower-oxeyedaisy-free") ?? Block(api, "game:flower-chamomile-free") ?? 0;
        idTallGrass    = Block(api, "game:tallgrass-tall-free") ?? Block(api, "game:tallgrass-medium-free") ?? 0;
    }

    /// <summary>
    /// Places sub-surface humus and scatters flowers/grass at the given surface column.
    /// Call only after the surface block has already been set.
    /// </summary>
    public void DecorateColumn(IWorldGenBlockAccessor ba, BlockPos pos, Random rng)
    {
        // Enrich 1-2 blocks below the surface with medium soil
        if (idSoilMed != 0)
        {
            var below1 = new BlockPos(pos.X, pos.Y - 1, pos.Z);
            var b1 = ba.GetBlock(below1);
            if (b1 != null)
            {
                var code = b1.Code?.Path ?? "";
                if (code.StartsWith("soil") || code.StartsWith("rock") || code.StartsWith("gravel"))
                    ba.SetBlock(idSoilMed, below1);
            }
        }

        // Scatter surface decorations: ~8% flowers, ~12% tall grass
        int roll = rng.Next(100);
        var above = new BlockPos(pos.X, pos.Y + 1, pos.Z);
        var aboveBlock = ba.GetBlock(above);
        bool airAbove = aboveBlock == null || aboveBlock.Id == 0 || (aboveBlock.Code?.Path == "air");

        if (!airAbove) return;

        if (roll < 4 && idFlowerYellow != 0)
            ba.SetBlock(idFlowerYellow, above);
        else if (roll < 8 && idFlowerWhite != 0)
            ba.SetBlock(idFlowerWhite, above);
        else if (roll < 20 && idTallGrass != 0)
            ba.SetBlock(idTallGrass, above);
    }

    static int? Block(ICoreServerAPI api, string code)
    {
        int id = api.World.GetBlock(new AssetLocation(code))?.Id ?? 0;
        return id == 0 ? null : id;
    }
}
