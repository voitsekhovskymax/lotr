using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Lotr.WorldGen.Biomes;

/// <summary>
/// Gondor mountainous biome: low soil on lower slopes, bare stone on cliffs,
/// sparse white flowers and dry grass.
/// </summary>
public class BiomeGondor
{
    int idSoilLow;
    int idStone;
    int idTallGrass;
    int idFlowerWhite;
    int idFlowerPurple;

    public int SurfaceBlock => idSoilLow;

    public void ResolveBlocks(ICoreServerAPI api)
    {
        idSoilLow     = Block(api, "game:soil-low-none")         ?? 0;
        idStone       = Block(api, "game:rock-granite")          ?? 0;
        idTallGrass   = Block(api, "game:tallgrass-medium-free") ?? 0;
        idFlowerWhite = Block(api, "game:flower-oxeyedaisy-free")  ?? Block(api, "game:flower-chamomile-free") ?? 0;
        idFlowerPurple= Block(api, "game:flower-lavender-free")    ?? Block(api, "game:flower-bluebells-free") ?? 0;
    }

    public void DecorateColumn(IWorldGenBlockAccessor ba, BlockPos pos, Random rng)
    {
        var above = new BlockPos(pos.X, pos.Y + 1, pos.Z);
        var ab = ba.GetBlock(above);
        if (ab != null && ab.Id != 0) return;

        int roll = rng.Next(100);
        if      (roll < 3  && idFlowerWhite  != 0) ba.SetBlock(idFlowerWhite, above);
        else if (roll < 5  && idFlowerPurple != 0) ba.SetBlock(idFlowerPurple, above);
        else if (roll < 12 && idTallGrass    != 0) ba.SetBlock(idTallGrass, above);
    }

    static int? Block(ICoreServerAPI api, string code)
    {
        int id = api.World.GetBlock(new AssetLocation(code))?.Id ?? 0;
        return id == 0 ? null : id;
    }
}
