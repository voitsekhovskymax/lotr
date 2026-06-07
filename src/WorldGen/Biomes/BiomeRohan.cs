using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Lotr.WorldGen.Biomes;

/// <summary>
/// Rohan plainland biome: medium soil, dense tall grass, yellow wildflowers.
/// </summary>
public class BiomeRohan
{
    int idSoilMed;
    int idSoilLow;
    int idTallGrass;
    int idTallGrassMed;
    int idFlowerYellow;
    int idFlowerOrange;

    public int SurfaceBlock => idSoilMed;

    public void ResolveBlocks(ICoreServerAPI api)
    {
        idSoilMed      = Block(api, "game:soil-medium-normal") ?? 0;
        idSoilLow      = Block(api, "game:soil-low-none")      ?? 0;
        idTallGrass    = Block(api, "game:tallgrass-tall-free")   ?? 0;
        idTallGrassMed = Block(api, "game:tallgrass-medium-free") ?? 0;
        idFlowerYellow = Block(api, "game:flower-goldenrod-free")  ?? Block(api, "game:flower-buttercup-free") ?? 0;
        idFlowerOrange = Block(api, "game:flower-calendula-free")  ?? Block(api, "game:flower-buttercup-free") ?? 0;
    }

    public void DecorateColumn(IWorldGenBlockAccessor ba, BlockPos pos, Random rng)
    {
        // Enrich sub-surface
        if (idSoilLow != 0)
        {
            var below = new BlockPos(pos.X, pos.Y - 1, pos.Z);
            var b = ba.GetBlock(below);
            if (b != null && (b.Code?.Path.StartsWith("soil") == true || b.Code?.Path.StartsWith("rock") == true))
                ba.SetBlock(idSoilLow, below);
        }

        var above = new BlockPos(pos.X, pos.Y + 1, pos.Z);
        var ab = ba.GetBlock(above);
        if (ab != null && ab.Id != 0) return;

        int roll = rng.Next(100);
        if      (roll < 5  && idFlowerYellow != 0) ba.SetBlock(idFlowerYellow, above);
        else if (roll < 8  && idFlowerOrange != 0) ba.SetBlock(idFlowerOrange, above);
        else if (roll < 30 && idTallGrass    != 0) ba.SetBlock(idTallGrass, above);
        else if (roll < 45 && idTallGrassMed != 0) ba.SetBlock(idTallGrassMed, above);
    }

    static int? Block(ICoreServerAPI api, string code)
    {
        int id = api.World.GetBlock(new AssetLocation(code))?.Id ?? 0;
        return id == 0 ? null : id;
    }
}
