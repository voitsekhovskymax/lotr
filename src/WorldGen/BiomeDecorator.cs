using System;
using System.Collections.Generic;
using System.Linq;
using Lotr.Regions;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Lotr.WorldGen;

/// <summary>
/// Data-driven biome decorator built from BiomeDefinition JSON. Replaces the
/// hardcoded per-biome classes (BiomeShire, BiomeMordor, …). Handles surface
/// and sub-surface block overrides, weighted decorations, weighted tree
/// generation, and per-biome ore boosting.
/// </summary>
public class BiomeDecorator
{
    public int SurfaceBlockId { get; private set; }
    int subSurfaceId;

    // (blockId, cumulativeWeight) — binary-search-free small arrays
    (int id, int cumWeight)[] decorations = [];
    int decoTotalWeight;
    public float DecorationChance { get; private set; }

    (AssetLocation code, int cumWeight, float size)[] trees = [];
    int treeTotalWeight;
    public float TreeChance { get; private set; }

    /// <summary>oreType → multiplier (only entries &gt; 1.0 kept).</summary>
    public Dictionary<string, float> OreBoost { get; private set; } = new();

    public static BiomeDecorator Build(BiomeDefinition def, ICoreServerAPI api)
    {
        var d = new BiomeDecorator
        {
            SurfaceBlockId   = ResolveBlock(api, def.SurfaceBlock),
            subSurfaceId     = ResolveBlock(api, def.SubSurfaceBlock),
            DecorationChance = def.DecorationChance,
            TreeChance       = def.TreeChance,
        };

        // Decorations: resolve codes, drop missing blocks, build cumulative weights
        int cum = 0;
        var decoList = new List<(int, int)>();
        foreach (var deco in def.Decorations)
        {
            int id = ResolveBlock(api, deco.Block);
            if (id == 0)
            {
                api.Logger.Warning($"[LOTR] Biome {def.Id}: decoration block '{deco.Block}' not found, skipped.");
                continue;
            }
            cum += Math.Max(1, deco.Weight);
            decoList.Add((id, cum));
        }
        d.decorations = decoList.ToArray();
        d.decoTotalWeight = cum;

        cum = 0;
        var treeList = new List<(AssetLocation, int, float)>();
        foreach (var tree in def.Trees)
        {
            if (string.IsNullOrEmpty(tree.Code)) continue;
            cum += Math.Max(1, tree.Weight);
            treeList.Add((new AssetLocation(tree.Code), cum, tree.Size));
        }
        d.trees = treeList.ToArray();
        d.treeTotalWeight = cum;

        d.OreBoost = def.OreBoost
            .Where(kv => kv.Value > 1.0f)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        return d;
    }

    static int ResolveBlock(ICoreServerAPI api, string code)
        => string.IsNullOrEmpty(code) ? 0 : api.World.GetBlock(new AssetLocation(code))?.Id ?? 0;

    /// <summary>
    /// Applies sub-surface block and rolls one decoration above the surface.
    /// Surface block itself is set by the caller. Reuses caller's BlockPos buffers.
    /// </summary>
    public void DecorateColumn(IWorldGenBlockAccessor ba, BlockPos surfacePos, BlockPos scratch, Random rng)
    {
        if (subSurfaceId != 0)
        {
            scratch.Set(surfacePos.X, surfacePos.Y - 1, surfacePos.Z);
            var below = ba.GetBlock(scratch);
            var code = below?.Code?.Path ?? "";
            if (code.StartsWith("soil") || code.StartsWith("rock") ||
                code.StartsWith("gravel") || code.StartsWith("sand"))
            {
                ba.SetBlock(subSurfaceId, scratch);
            }
        }

        if (decoTotalWeight == 0 || DecorationChance <= 0) return;
        if (rng.NextDouble() >= DecorationChance) return;

        scratch.Set(surfacePos.X, surfacePos.Y + 1, surfacePos.Z);
        var above = ba.GetBlock(scratch);
        if (above != null && above.Id != 0) return;

        int roll = rng.Next(decoTotalWeight);
        foreach (var (id, cumWeight) in decorations)
        {
            if (roll < cumWeight)
            {
                ba.SetBlock(id, scratch);
                return;
            }
        }
    }

    /// <summary>Picks a weighted tree generator code, or null if the roll fails.</summary>
    public (AssetLocation code, float size)? RollTree(Random rng)
    {
        if (treeTotalWeight == 0 || TreeChance <= 0) return null;
        if (rng.NextDouble() >= TreeChance) return null;

        int roll = rng.Next(treeTotalWeight);
        foreach (var (code, cumWeight, size) in trees)
        {
            if (roll < cumWeight) return (code, size);
        }
        return null;
    }
}
