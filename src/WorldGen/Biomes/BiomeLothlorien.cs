using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Lotr.WorldGen.Biomes;

/// <summary>
/// Lothlórien forest biome: rich soil, golden flowers, dense tall grass.
/// Simulates the golden radiance of the mallorn forest.
/// </summary>
public class BiomeLothlorien
{
    int idSoilRich;
    int idSoilMed;
    int idTallGrass;
    int idFlowerGold;
    int idFlowerYellow;
    int idFlowerWhite;

    public int SurfaceBlock => idSoilRich;

    public void ResolveBlocks(ICoreServerAPI api)
    {
        idSoilRich     = Block(api, "game:soil-rich-normal")     ?? Block(api, "game:soil-medium-normal") ?? 0;
        idSoilMed      = Block(api, "game:soil-medium-normal")   ?? 0;
        idTallGrass    = Block(api, "game:tallgrass-tall-free")  ?? 0;
        idFlowerGold   = Block(api, "game:flower-goldenrod-free")  ?? Block(api, "game:flower-buttercup-free") ?? 0;
        idFlowerYellow = Block(api, "game:flower-buttercup-free")  ?? 0;
        idFlowerWhite  = Block(api, "game:flower-oxeyedaisy-free") ?? 0;
    }

    public void DecorateColumn(IWorldGenBlockAccessor ba, BlockPos pos, Random rng)
    {
        // Rich sub-surface humus
        if (idSoilMed != 0)
        {
            var below = new BlockPos(pos.X, pos.Y - 1, pos.Z);
            var b = ba.GetBlock(below);
            if (b != null && (b.Code?.Path.StartsWith("soil") == true || b.Code?.Path.StartsWith("rock") == true))
                ba.SetBlock(idSoilMed, below);
        }

        var above = new BlockPos(pos.X, pos.Y + 1, pos.Z);
        var ab = ba.GetBlock(above);
        if (ab != null && ab.Id != 0) return;

        // Dense golden decorations — more flowers than other biomes
        int roll = rng.Next(100);
        if      (roll < 8  && idFlowerGold   != 0) ba.SetBlock(idFlowerGold, above);
        else if (roll < 12 && idFlowerYellow != 0) ba.SetBlock(idFlowerYellow, above);
        else if (roll < 15 && idFlowerWhite  != 0) ba.SetBlock(idFlowerWhite, above);
        else if (roll < 35 && idTallGrass    != 0) ba.SetBlock(idTallGrass, above);
    }

    static int? Block(ICoreServerAPI api, string code)
    {
        int id = api.World.GetBlock(new AssetLocation(code))?.Id ?? 0;
        return id == 0 ? null : id;
    }
}
