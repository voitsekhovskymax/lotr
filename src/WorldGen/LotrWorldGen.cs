using System;
using System.Collections.Generic;
using Lotr.Regions;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Lotr.WorldGen;

/// <summary>
/// Hooks into the "lotr" world type chunk generation. Fully data-driven:
/// each biome JSON (assets/lotr/worldgen/biomes/) declares surfaceBlock,
/// subSurfaceBlock, decorations, trees and oreBoost — this system applies them.
///
/// ExecuteOrder() = 0.2 → runs after base terrain (0.0) but before
/// vanilla decorators (0.5+), so our surface is decorated normally.
/// </summary>
public class LotrWorldGen : ModSystem
{
    const int ChunkSize = 32;

    ICoreServerAPI?        sapi;
    IWorldGenBlockAccessor blockAccessor = null!;
    RegionSystem?          regions;

    readonly Dictionary<string, BiomeDecorator> decorators = new();
    readonly Random worldgenRng = new();
    LCGRandom treeRng = null!;

    // Ore grades tried per boosted vein, most common first
    static readonly string[] OreGrades = ["poor", "medium", "rich"];

    // ── ModSystem lifecycle ──────────────────────────────────────────

    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Server;

    /// Runs AFTER base terrain gen (0.0), BEFORE vanilla decor passes (0.5)
    public override double ExecuteOrder() => 0.2;

    public override void StartServerSide(ICoreServerAPI api)
    {
        sapi = api;

        // Must register block accessor FIRST before any other worldgen event
        api.Event.GetWorldgenBlockAccessor(OnGetBlockAccessor);

        // InitWorldGenerator fires once per new world before first chunk gen
        api.Event.InitWorldGenerator(OnInitWorldGenerator, "standard");

        // Hook into Vegetation pass — terrain shape already done,
        // we just replace the top surface block per region
        api.Event.ChunkColumnGeneration(OnChunkColumnGen, EnumWorldGenPass.Vegetation, "standard");

        api.Event.SaveGameLoaded += OnSaveGameLoaded;
    }

    void OnInitWorldGenerator()
    {
        sapi!.Logger.Notification("[LOTR] Middle-earth world generator initializing.");
    }

    void OnSaveGameLoaded()
    {
        treeRng = new LCGRandom(sapi!.World.Seed);

        // Grab the RegionSystem from the LOTR mod instance
        var lotrMod = sapi!.ModLoader.GetModSystem<LotrModSystem>();
        regions = lotrMod?.Regions;

        decorators.Clear();
        if (regions != null)
        {
            foreach (var (id, biome) in regions.Biomes)
                decorators[id] = BiomeDecorator.Build(biome, sapi!);
        }

        sapi!.Logger.Notification(
            $"[LOTR] WorldGen ready: {decorators.Count} biome decorators, " +
            $"{regions?.Regions.Count ?? 0} regions.");
    }

    void OnGetBlockAccessor(IChunkProviderThread chunkProvider)
        => blockAccessor = chunkProvider.GetBlockAccessor(true);

    // ── Chunk generation ────────────────────────────────────────────

    void OnChunkColumnGen(IChunkColumnGenerateRequest request)
    {
        // REQUIRED — clears internal block accessor cache for this column
        blockAccessor.BeginColumn();

        if (regions == null || regions.SpawnX == 0) return;

        int baseX = request.ChunkX * ChunkSize;
        int baseZ = request.ChunkZ * ChunkSize;
        var heightMap = request.Chunks[^1].MapChunk.WorldGenTerrainHeightMap;

        var surfacePos = new BlockPos(0);
        var scratch    = new BlockPos(0);

        for (int lx = 0; lx < ChunkSize; lx++)
        for (int lz = 0; lz < ChunkSize; lz++)
        {
            var (region, biome) = regions.GetAt(baseX + lx, baseZ + lz);
            if (region == null || biome == null) continue;
            if (!decorators.TryGetValue(biome.Id, out var deco)) continue;

            int surfaceY = heightMap[lz * ChunkSize + lx];
            if (surfaceY <= 1) continue;

            surfacePos.Set(baseX + lx, surfaceY, baseZ + lz);
            int existing = blockAccessor.GetBlock(surfacePos).Id;
            if (!IsSurfaceReplaceable(existing)) continue;

            if (deco.SurfaceBlockId != 0)
                blockAccessor.SetBlock(deco.SurfaceBlockId, surfacePos);

            deco.DecorateColumn(blockAccessor, surfacePos, scratch, worldgenRng);

            TrySpawnTree(deco, surfacePos, worldgenRng);
        }

        ApplyOreBoost(request, baseX, baseZ, heightMap, scratch);
    }

    // ── Trees ───────────────────────────────────────────────────────

    void TrySpawnTree(BiomeDecorator deco, BlockPos surfacePos, Random rng)
    {
        var pick = deco.RollTree(rng);
        if (pick == null) return;

        var (code, size) = pick.Value;
        var generators = sapi!.World.TreeGenerators;
        if (generators == null || !generators.TryGetValue(code, out var gen)) return;

        // Trunk base = first air block above the surface
        var treePos = surfacePos.UpCopy();
        treeRng.InitPositionSeed(treePos.X, treePos.Z);

        gen.GrowTree(blockAccessor, treePos, new TreeGenParams
        {
            size = size,
            skipForestFloor = true,
        }, treeRng);
    }

    // ── Ore boost ───────────────────────────────────────────────────

    /// <summary>
    /// Spawns extra small ore veins for biomes with oreBoost multipliers.
    /// multiplier 2.0 ≈ 3 extra vein attempts per chunk for that ore type.
    /// </summary>
    void ApplyOreBoost(IChunkColumnGenerateRequest request, int baseX, int baseZ,
                       ushort[] heightMap, BlockPos scratch)
    {
        // Sample biome at chunk center — ore boost is coarse by design
        var (_, biome) = regions!.GetAt(baseX + ChunkSize / 2, baseZ + ChunkSize / 2);
        if (biome == null) return;
        if (!decorators.TryGetValue(biome.Id, out var deco) || deco.OreBoost.Count == 0) return;

        foreach (var (oreType, multiplier) in deco.OreBoost)
        {
            int tries = (int)Math.Round((multiplier - 1.0) * 3.0);
            for (int i = 0; i < tries; i++)
            {
                int lx = worldgenRng.Next(ChunkSize);
                int lz = worldgenRng.Next(ChunkSize);
                int surfaceY = heightMap[lz * ChunkSize + lx];
                if (surfaceY < 24) continue;

                int y = 8 + worldgenRng.Next(surfaceY - 16);
                scratch.Set(baseX + lx, y, baseZ + lz);

                var host = blockAccessor.GetBlock(scratch);
                var hostCode = host?.Code?.Path ?? "";
                if (!hostCode.StartsWith("rock-")) continue;

                string rockType = hostCode[5..];
                int oreId = ResolveOreBlock(oreType, rockType, worldgenRng);
                if (oreId == 0) continue;

                PlaceVein(oreId, scratch, worldgenRng);
            }
        }
    }

    /// <summary>Resolves ore-{grade}-{type}-{rock}, rolling grade (50% poor / 35% medium / 15% rich).</summary>
    int ResolveOreBlock(string oreType, string rockType, Random rng)
    {
        int roll = rng.Next(100);
        string grade = roll < 50 ? "poor" : roll < 85 ? "medium" : "rich";

        // Preferred grade first, then the rest — not every combo exists per rock
        int id = OreId(grade, oreType, rockType);
        if (id != 0) return id;

        foreach (var g in OreGrades)
        {
            if (g == grade) continue;
            id = OreId(g, oreType, rockType);
            if (id != 0) return id;
        }
        return 0;
    }

    int OreId(string grade, string oreType, string rockType)
        => sapi!.World.GetBlock(new AssetLocation($"game:ore-{grade}-{oreType}-{rockType}"))?.Id ?? 0;

    /// <summary>Places a small blob of 3–7 ore blocks, replacing only host rock.</summary>
    void PlaceVein(int oreId, BlockPos center, Random rng)
    {
        blockAccessor.SetBlock(oreId, center);

        int extra = 2 + rng.Next(5);
        var p = new BlockPos(0);
        for (int i = 0; i < extra; i++)
        {
            p.Set(center.X + rng.Next(3) - 1,
                  center.Y + rng.Next(2),
                  center.Z + rng.Next(3) - 1);
            var b = blockAccessor.GetBlock(p);
            if (b?.Code?.Path.StartsWith("rock-") == true)
                blockAccessor.SetBlock(oreId, p);
        }
    }

    // ── Helpers ─────────────────────────────────────────────────────

    /// Returns true for blocks we are allowed to replace at the surface.
    bool IsSurfaceReplaceable(int blockId)
    {
        if (blockId == 0) return false; // air
        var block = sapi!.World.Blocks[blockId];
        if (block == null) return false;
        var code = block.Code?.Path ?? "";
        // Allow replacing: soil, grass, sand, gravel, rock, stone
        return code.StartsWith("soil")    ||
               code.StartsWith("grass")   ||
               code.StartsWith("sand")    ||
               code.StartsWith("gravel")  ||
               code.StartsWith("rock")    ||
               code.StartsWith("stone");
    }
}
