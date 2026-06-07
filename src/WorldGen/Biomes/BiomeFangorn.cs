using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Lotr.WorldGen.Biomes;

/// <summary>
/// Fangorn forest biome: rich ancient soil, very dense tall grass and ferns,
/// sparse white/purple flowers representing the primeval understory.
/// </summary>
public class BiomeFangorn
{
    int idSoilRich;
    int idSoilMed;
    int idTallGrass;
    int idFern;
    int idFlowerPurple;
    int idFlowerWhite;

    public int SurfaceBlock => idSoilRich;

    public void ResolveBlocks(ICoreServerAPI api)
    {
        idSoilRich    = Block(api, "game:soil-rich-normal")     ?? Block(api, "game:soil-medium-normal") ?? 0;
        idSoilMed     = Block(api, "game:soil-medium-normal")   ?? 0;
        idTallGrass   = Block(api, "game:tallgrass-tall-free")  ?? 0;
        idFern        = Block(api, "game:tallgrass-fern-free")  ?? Block(api, "game:tallgrass-medium-free") ?? 0;
        idFlowerPurple= Block(api, "game:flower-bluebells-free")  ?? Block(api, "game:flower-lavender-free") ?? 0;
        idFlowerWhite = Block(api, "game:flower-oxeyedaisy-free") ?? 0;
    }

    public void DecorateColumn(IWorldGenBlockAccessor ba, BlockPos pos, Random rng)
    {
        // Deep humus — 2 layers below
        if (idSoilMed != 0)
        {
            for (int dy = 1; dy <= 2; dy++)
            {
                var belowPos = new BlockPos(pos.X, pos.Y - dy, pos.Z);
                var b = ba.GetBlock(belowPos);
                if (b == null) break;
                var code = b.Code?.Path ?? "";
                if (code.StartsWith("soil") || code.StartsWith("rock") || code.StartsWith("gravel"))
                    ba.SetBlock(idSoilMed, belowPos);
                else break;
            }
        }

        var above = new BlockPos(pos.X, pos.Y + 1, pos.Z);
        var ab = ba.GetBlock(above);
        if (ab != null && ab.Id != 0) return;

        // Very dense undergrowth
        int roll = rng.Next(100);
        if      (roll < 3  && idFlowerPurple != 0) ba.SetBlock(idFlowerPurple, above);
        else if (roll < 5  && idFlowerWhite  != 0) ba.SetBlock(idFlowerWhite, above);
        else if (roll < 25 && idFern         != 0) ba.SetBlock(idFern, above);
        else if (roll < 50 && idTallGrass    != 0) ba.SetBlock(idTallGrass, above);
    }

    static int? Block(ICoreServerAPI api, string code)
    {
        int id = api.World.GetBlock(new AssetLocation(code))?.Id ?? 0;
        return id == 0 ? null : id;
    }
}
