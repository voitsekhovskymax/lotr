using System;
using System.Collections.Generic;
using Lotr.Regions;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
namespace Lotr.WorldGen;

/// <summary>
/// Phase 5 — hooks into VS chunk generation to override surface blocks
/// per LOTR region (Mordor = basalt/ash, Shire = fertile soil, etc.).
///
/// ExecuteOrder() = 0.2 → runs after base terrain (0.0) but before
/// vanilla decorators (0.5+), so our surface is decorated normally.
/// </summary>
public class LotrWorldGen : ModSystem
{
    ICoreServerAPI?        sapi;
    IWorldGenBlockAccessor blockAccessor = null!;
    RegionSystem?          regions;

    // Block IDs — resolved once in StartServerSide after all assets loaded
    int idBasalt;
    int idGravel;
    int idSand;
    int idSoilLow;   // soil-low (base fertile soil)
    int idSoilMed;   // soil-medium
    int idStone;     // rock-granite (mountains)

    // ── ModSystem lifecycle ──────────────────────────────────────────

    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Server;

    /// Runs AFTER base terrain gen (0.0), BEFORE vanilla decor passes (0.5)
    public override double ExecuteOrder() => 0.2;

    public override void StartServerSide(ICoreServerAPI api)
    {
        sapi = api;

        // Must register block accessor FIRST before any other worldgen event
        api.Event.GetWorldgenBlockAccessor(OnGetBlockAccessor);

        // Hook into Vegetation pass (pass 3) — terrain shape already done,
        // we just replace the top surface block here
        api.Event.ChunkColumnGeneration(OnChunkColumnGen, EnumWorldGenPass.Vegetation, "standard");

        api.Event.SaveGameLoaded += OnSaveGameLoaded;
    }

    void OnSaveGameLoaded()
    {
        // Resolve block IDs after world has loaded its block registry
        idBasalt  = sapi!.World.GetBlock(new AssetLocation("game:rock-basalt"))?.Id   ?? 0;
        idGravel  = sapi!.World.GetBlock(new AssetLocation("game:gravel-granite"))?.Id ?? 0;
        idSand    = sapi!.World.GetBlock(new AssetLocation("game:sand-ash"))?.Id
                 ?? sapi!.World.GetBlock(new AssetLocation("game:sand-volcanic"))?.Id
                 ?? sapi!.World.GetBlock(new AssetLocation("game:sand"))?.Id           ?? 0;
        idSoilLow = sapi!.World.GetBlock(new AssetLocation("game:soil-low-none"))?.Id ?? 0;
        idSoilMed = sapi!.World.GetBlock(new AssetLocation("game:soil-medium-none"))?.Id ?? 0;
        idStone   = sapi!.World.GetBlock(new AssetLocation("game:rock-granite"))?.Id  ?? 0;

        // Grab the RegionSystem from the LOTR mod instance
        var lotrMod = sapi!.ModLoader.GetModSystem<LotrModSystem>();
        regions = lotrMod?.Regions;

        sapi!.Logger.Notification(
            $"[LOTR] WorldGen ready. basalt={idBasalt} gravel={idGravel} sand={idSand} " +
            $"soilLow={idSoilLow} soilMed={idSoilMed} stone={idStone} " +
            $"regions={(regions?.Regions.Count ?? 0)}");
    }

    void OnGetBlockAccessor(IChunkProviderThread chunkProvider)
        => blockAccessor = chunkProvider.GetBlockAccessor(true);

    // ── Chunk generation ────────────────────────────────────────────

    void OnChunkColumnGen(IChunkColumnGenerateRequest request)
    {
        // REQUIRED — clears internal block accessor cache for this column
        blockAccessor.BeginColumn();

        if (regions == null || regions.SpawnX == 0) return;

        const int chunkSize = 32; // GlobalConstants.ChunkSize
        int baseX     = request.ChunkX * chunkSize;
        int baseZ     = request.ChunkZ * chunkSize;

        for (int lx = 0; lx < chunkSize; lx++)
        for (int lz = 0; lz < chunkSize; lz++)
        {
            double wx = baseX + lx;
            double wz = baseZ + lz;

            var (region, biome) = regions.GetAt(wx, wz);
            if (region == null || biome == null) continue;

            // Determine which surface block to place based on biome
            int targetBlock = SurfaceBlockFor(biome.Id);
            if (targetBlock == 0) continue;

            // Find the surface Y for this XZ column
            int surfaceY = request.Chunks[^1].MapChunk.WorldGenTerrainHeightMap[lz * chunkSize + lx];
            if (surfaceY <= 0) continue;

            // Replace only the top 1-2 blocks so we don't destroy strata
            var pos = new BlockPos(baseX + lx, surfaceY, baseZ + lz);
            int existing = blockAccessor.GetBlock(pos).Id;

            // Only replace soil/grass/stone surface blocks — not water, air, etc.
            if (IsSurfaceReplaceable(existing))
            {
                blockAccessor.SetBlock(targetBlock, pos);

                // For mountains: also place gravel 1 block below
                if (biome.Id is "lotr:biome-blue-mountains" or "lotr:biome-moria" && surfaceY > 1)
                {
                    var below = new BlockPos(baseX + lx, surfaceY - 1, baseZ + lz);
                    if (IsSurfaceReplaceable(blockAccessor.GetBlock(below).Id))
                        blockAccessor.SetBlock(idGravel, below);
                }
            }
        }
    }

    // ── Helpers ─────────────────────────────────────────────────────

    int SurfaceBlockFor(string biomeId) => biomeId switch
    {
        "lotr:biome-shire"         => idSoilMed,
        "lotr:biome-bree"          => idSoilLow,
        "lotr:biome-rivendell"     => idSoilMed,
        "lotr:biome-blue-mountains"=> idStone,
        "lotr:biome-moria"         => idStone,
        "lotr:biome-minas-tirith"  => idSoilLow,
        "lotr:biome-isengard"      => idSoilLow,
        "lotr:biome-mordor"        => idBasalt,
        _                          => 0   // 0 = no override
    };

    /// Returns true for blocks we are allowed to replace at the surface.
    bool IsSurfaceReplaceable(int blockId)
    {
        if (blockId == 0) return false; // air
        var block = sapi!.World.Blocks[blockId];
        if (block == null) return false;
        var code  = block.Code?.Path ?? "";
        // Allow replacing: soil, grass, sand, gravel, rock, stone
        return code.StartsWith("soil")    ||
               code.StartsWith("grass")   ||
               code.StartsWith("sand")    ||
               code.StartsWith("gravel")  ||
               code.StartsWith("rock")    ||
               code.StartsWith("stone");
    }
}
