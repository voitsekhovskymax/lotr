using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Lotr.WorldGen.Structures;

/// <summary>
/// Procedurally generates a jagged mountain peak spire rising above the terrain.
/// Creates the distinctive sawtooth profile of the Misty Mountains by stacking
/// a tapered granite cone with offset sub-spires.
/// </summary>
public class MistyMountainsRange : StructureBase
{
    public MistyMountainsRange(ICoreServerAPI api, IWorldGenBlockAccessor ba)
        : base(api, ba) { }

    // Entry point required by StructureBase — uses default parameters.
    public override void Generate(int originX, int surfaceY, int originZ)
        => GeneratePeak(originX, surfaceY, originZ, peakHeight: 35, baseRadius: 8, spawnSubSpires: true);

    public void GeneratePeak(int cx, int surfaceY, int cz, int peakHeight, int baseRadius, bool spawnSubSpires)
    {
        int idRock   = Api.World.GetBlock(new AssetLocation("game:rock-granite"))?.Id   ?? 0;
        int idGravel = Api.World.GetBlock(new AssetLocation("game:gravel-granite"))?.Id ?? 0;

        if (idRock == 0) return;

        for (int dy = 0; dy <= peakHeight; dy++)
        {
            float t      = (float)dy / peakHeight;
            int   radius = Math.Max(1, (int)Math.Round(baseRadius * (1f - t)));
            int   y      = surfaceY + dy;

            for (int dx = -radius; dx <= radius; dx++)
            for (int dz = -radius; dz <= radius; dz++)
            {
                if (dx * dx + dz * dz > radius * radius) continue;

                var pos      = new BlockPos(cx + dx, y, cz + dz);
                var existing = Accessor.GetBlock(pos);

                if (existing.Id == 0 || IsReplaceable(existing))
                {
                    int blockId = (dy == peakHeight && idGravel != 0) ? idGravel : idRock;
                    Accessor.SetBlock(blockId, pos);
                }
            }
        }

        if (!spawnSubSpires) return;

        // Add 2-3 offset sub-spires for a jagged ridgeline
        var rng    = new Random(cx * 7919 + cz * 31337);
        int spires = 2 + rng.Next(2);
        for (int s = 0; s < spires; s++)
        {
            int ox = (int)((rng.NextDouble() * 2 - 1) * baseRadius * 0.7);
            int oz = (int)((rng.NextDouble() * 2 - 1) * baseRadius * 0.7);
            int sh = peakHeight / 2 + rng.Next(peakHeight / 3);
            int sr = Math.Max(2, baseRadius / 3);
            GeneratePeak(cx + ox, surfaceY, cz + oz, sh, sr, spawnSubSpires: false);
        }
    }
}
