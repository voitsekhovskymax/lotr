using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Lotr.WorldGen.Biomes;

/// <summary>
/// Mordor volcanic biome: basalt surface, ash sub-surface, rare fire/ember above.
/// </summary>
public class BiomeMordor
{
    int idBasalt;
    int idAsh;   // volcanic ash sand
    int idFire;

    public int SurfaceBlock => idBasalt;

    public void ResolveBlocks(ICoreServerAPI api)
    {
        idBasalt = Block(api, "game:rock-basalt")      ?? 0;
        idAsh    = Block(api, "game:sand-ash")         ?? Block(api, "game:gravel-basalt") ?? 0;
        idFire   = Block(api, "game:fire")             ?? 0;
    }

    public void DecorateColumn(IWorldGenBlockAccessor ba, BlockPos pos, Random rng)
    {
        // Ash sub-surface
        if (idAsh != 0)
        {
            var below = new BlockPos(pos.X, pos.Y - 1, pos.Z);
            var b = ba.GetBlock(below);
            if (b != null)
            {
                var code = b.Code?.Path ?? "";
                if (code.StartsWith("soil") || code.StartsWith("gravel") || code.StartsWith("rock"))
                    ba.SetBlock(idAsh, below);
            }
        }

        // ~2% chance of fire above surface
        int roll = rng.Next(100);
        if (roll < 2 && idFire != 0)
        {
            var above = new BlockPos(pos.X, pos.Y + 1, pos.Z);
            var aboveBlock = ba.GetBlock(above);
            bool airAbove = aboveBlock == null || aboveBlock.Id == 0;
            if (airAbove) ba.SetBlock(idFire, above);
        }
    }

    static int? Block(ICoreServerAPI api, string code)
    {
        int id = api.World.GetBlock(new AssetLocation(code))?.Id ?? 0;
        return id == 0 ? null : id;
    }
}
