using System;
using System.Collections.Generic;
using Lotr.Regions;
using Lotr.WorldGen.Structures;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Lotr.WorldGen;

/// <summary>
/// Decides when and where to spawn LOTR structures during world generation.
/// Hooks into ChunkColumnGeneration at pass TerrainFeatures (after surface override).
///
/// Spawn rules per region:
///   lotr:region-shire       → HobbitHole,    ~1 per 5 chunks  (dense)
///   lotr:region-isengard    → OrthanctTower, 1 per region center
///   lotr:region-minas-tirith→ MinasTirithWall, sparse
/// </summary>
public class StructureSpawner : ModSystem
{
    ICoreServerAPI?        sapi;
    IWorldGenBlockAccessor blockAccessor = null!;
    RegionSystem?          regions;
    Random                 rng = new();

    // Track chunks where a unique structure was already placed
    readonly HashSet<string> placedUnique = new();

    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Server;

    // Run after LotrWorldGen (0.2) but before vanilla decorators (0.5)
    public override double ExecuteOrder() => 0.3;

    public override void StartServerSide(ICoreServerAPI api)
    {
        sapi = api;
        api.Event.GetWorldgenBlockAccessor(OnGetBlockAccessor);
        api.Event.ChunkColumnGeneration(OnChunkColumnGen, EnumWorldGenPass.TerrainFeatures, "standard");
        api.Event.SaveGameLoaded += OnSaveGameLoaded;
    }

    void OnSaveGameLoaded()
    {
        var lotrMod = sapi!.ModLoader.GetModSystem<LotrModSystem>();
        regions = lotrMod?.Regions;
    }

    void OnGetBlockAccessor(IChunkProviderThread chunkProvider)
        => blockAccessor = chunkProvider.GetBlockAccessor(true);

    void OnChunkColumnGen(IChunkColumnGenerateRequest request)
    {
        blockAccessor.BeginColumn();
        if (regions == null || regions.SpawnX == 0) return;

        const int chunkSize = 32;
        int baseX = request.ChunkX * chunkSize;
        int baseZ = request.ChunkZ * chunkSize;

        // Use chunk coords as seed for deterministic placement
        rng = new Random(request.ChunkX * 31337 + request.ChunkZ * 7919);

        // Sample region at chunk center
        double cx = baseX + 16;
        double cz = baseZ + 16;
        var (region, _) = regions.GetAt(cx, cz);
        if (region == null) return;

        switch (region.Id)
        {
            case "lotr:region-shire":
                TrySpawnHobbitHole(baseX, baseZ, chunkSize);
                break;

            case "lotr:region-isengard":
                TrySpawnOrthanc(baseX, baseZ, chunkSize, region.Id);
                break;

            case "lotr:region-minas-tirith":
                TrySpawnMinasTirithWall(baseX, baseZ, chunkSize, region.Id);
                break;

            case "lotr:region-misty-mountains":
            case "lotr:region-grey-mountains":
            case "lotr:region-angmar-mountains":
                TrySpawnMountainPeak(baseX, baseZ, chunkSize);
                break;
        }
    }

    // ── Spawn methods ────────────────────────────────────────────────

    void TrySpawnHobbitHole(int baseX, int baseZ, int chunkSize)
    {
        // ~20% chance per chunk = roughly 1 hobbit hole per 5 chunks
        if (rng.NextDouble() > 0.20) return;

        int lx = 6 + rng.Next(chunkSize - 12);
        int lz = 6 + rng.Next(chunkSize - 12);
        int wx = baseX + lx;
        int wz = baseZ + lz;

        int surfaceY = GetSurfaceY(wx, wz);
        if (surfaceY <= 10) return; // underwater, skip

        var hole = new HobbitHole(sapi!, blockAccessor);
        hole.Generate(wx, surfaceY, wz);

        sapi!.Logger.Notification($"[LOTR] Spawned HobbitHole at {wx},{surfaceY},{wz}");
    }

    void TrySpawnOrthanc(int baseX, int baseZ, int chunkSize, string regionId)
    {
        // Only one Orthanc per region — place at the geographic center chunk
        string key = $"orthanc-{regionId}";
        if (placedUnique.Contains(key)) return;

        // Only place if we're within ±64 blocks of the region center
        if (regions == null) return;
        var reg = regions.GetRegionById(regionId);
        if (reg == null) return;

        double worldCenterX = regions.SpawnX + reg.CenterX;
        double worldCenterZ = regions.SpawnZ + reg.CenterZ;
        double distX = Math.Abs(baseX + 16 - worldCenterX);
        double distZ = Math.Abs(baseZ + 16 - worldCenterZ);
        if (distX > 64 || distZ > 64) return;

        int wx = (int)worldCenterX;
        int wz = (int)worldCenterZ;
        int surfaceY = GetSurfaceY(wx, wz);
        if (surfaceY <= 10) return;

        var tower = new OrthanctTower(sapi!, blockAccessor);
        tower.Generate(wx, surfaceY, wz);
        placedUnique.Add(key);

        sapi!.Logger.Notification($"[LOTR] Spawned Orthanc at {wx},{surfaceY},{wz}");
    }

    void TrySpawnMinasTirithWall(int baseX, int baseZ, int chunkSize, string regionId)
    {
        string key = $"mt-wall-{regionId}";
        if (placedUnique.Contains(key)) return;

        if (regions == null) return;
        var reg = regions.GetRegionById(regionId);
        if (reg == null) return;

        double worldCenterX = regions.SpawnX + reg.CenterX;
        double worldCenterZ = regions.SpawnZ + reg.CenterZ;
        double distX = Math.Abs(baseX + 16 - worldCenterX);
        double distZ = Math.Abs(baseZ + 16 - worldCenterZ);
        if (distX > 64 || distZ > 64) return;

        int wx = (int)worldCenterX - 20;
        int wz = (int)worldCenterZ - 20;
        int surfaceY = GetSurfaceY(wx, wz);
        if (surfaceY <= 10) return;

        var wall = new MinasTirithWall(sapi!, blockAccessor);
        wall.Generate(wx, surfaceY, wz);
        placedUnique.Add(key);

        sapi!.Logger.Notification($"[LOTR] Spawned MinasTirith wall at {wx},{surfaceY},{wz}");
    }

    void TrySpawnMountainPeak(int baseX, int baseZ, int chunkSize)
    {
        // ~18% chance per chunk — peaks clustered throughout mountain range
        if (rng.NextDouble() > 0.18) return;

        int lx = 8 + rng.Next(chunkSize - 16);
        int lz = 8 + rng.Next(chunkSize - 16);
        int wx = baseX + lx;
        int wz = baseZ + lz;

        int surfaceY = GetSurfaceY(wx, wz);
        if (surfaceY <= 10) return;

        int peakH  = 20 + rng.Next(36);  // 20–55 blocks tall
        int radius = 5  + rng.Next(8);   // 5–12 block base radius

        var peak = new MistyMountainsRange(sapi!, blockAccessor);
        peak.GeneratePeak(wx, surfaceY, wz, peakH, radius, spawnSubSpires: true);

        sapi!.Logger.Notification($"[LOTR] Spawned MountainPeak h={peakH} r={radius} at {wx},{surfaceY},{wz}");
    }

    // ── Helpers ──────────────────────────────────────────────────────

    int GetSurfaceY(int wx, int wz)
    {
        const int chunkSize = 32;
        int chunkX = wx / chunkSize;
        int chunkZ = wz / chunkSize;
        var chunk  = sapi!.World.BlockAccessor.GetMapChunk(chunkX, chunkZ);
        if (chunk == null) return 0;
        int lx = wx % chunkSize;
        int lz = wz % chunkSize;
        return chunk.WorldGenTerrainHeightMap[lz * chunkSize + lx];
    }
}
