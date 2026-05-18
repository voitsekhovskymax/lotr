using System;
using System.Collections.Generic;
using System.Linq;
using Lotr.WorldGen;
using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Lotr.Regions;

public class RegionSystem
{
    readonly ICoreServerAPI api;
    PngBiomeMap? pngMap;

    public IReadOnlyList<RegionDefinition> Regions { get; private set; } = Array.Empty<RegionDefinition>();
    public IReadOnlyDictionary<string, BiomeDefinition> Biomes { get; private set; }
        = new Dictionary<string, BiomeDefinition>();

    public double SpawnX { get; private set; }
    public double SpawnZ { get; private set; }

    public RegionSystem(ICoreServerAPI api)
    {
        this.api = api;
    }

    /// <summary>Cache world spawn offset — call from WorldReady event.</summary>
    public void CacheSpawn()
    {
        // Try DefaultSpawnPosition first (EntityPos, reliable after WorldReady)
        var spawnPos = api.World.DefaultSpawnPosition;
        if (spawnPos != null && (spawnPos.X != 0 || spawnPos.Z != 0))
        {
            SpawnX = spawnPos.X;
            SpawnZ = spawnPos.Z;
            api.Logger.Notification($"[LOTR] RegionSystem: spawn cached at X={SpawnX:F0} Z={SpawnZ:F0}");
            return;
        }

        // Fallback: use half the world size as rough spawn estimate
        // (VS default world size is 1024000, spawn is at center ~512000)
        try
        {
            int half = api.WorldManager.MapSizeX / 2;
            SpawnX = half;
            SpawnZ = api.WorldManager.MapSizeZ / 2;
            api.Logger.Notification($"[LOTR] RegionSystem: spawn estimated from map center at X={SpawnX:F0} Z={SpawnZ:F0}");
            return;
        }
        catch { }

        api.Logger.Warning("[LOTR] RegionSystem: could not determine spawn position — regions will not work until player joins.");
    }

    // ── Load ────────────────────────────────────────────────────────

    public void Load()
    {
        var regions = new List<RegionDefinition>();
        var biomes  = new Dictionary<string, BiomeDefinition>();

        // Load biomes
        var biomeAssets = api.Assets.GetMany("worldgen/biomes/", "lotr");
        foreach (var asset in biomeAssets)
        {
            try
            {
                var b = JsonConvert.DeserializeObject<BiomeDefinition>(asset.ToText());
                if (b?.Id is { Length: > 0 })
                    biomes[b.Id] = b;
            }
            catch (Exception e)
            {
                api.Logger.Warning($"[LOTR] Failed to load biome {asset.Name}: {e.Message}");
            }
        }

        // Load regions
        var regionAssets = api.Assets.GetMany("worldgen/regions/", "lotr");
        foreach (var asset in regionAssets)
        {
            try
            {
                var r = JsonConvert.DeserializeObject<RegionDefinition>(asset.ToText());
                if (r?.Id is { Length: > 0 })
                    regions.Add(r);
            }
            catch (Exception e)
            {
                api.Logger.Warning($"[LOTR] Failed to load region {asset.Name}: {e.Message}");
            }
        }

        // Sort by priority descending (higher priority checked first)
        Regions = regions.OrderByDescending(r => r.Priority).ToList();
        Biomes  = biomes;

        // Load PNG biome map
        pngMap = new PngBiomeMap(api);
        pngMap.Load();

        api.Logger.Notification($"[LOTR] Loaded {Biomes.Count} biomes, {Regions.Count} regions, PNG map={pngMap.IsLoaded}.");
    }

    // ── Query ───────────────────────────────────────────────────────

    public RegionDefinition? GetRegionById(string id)
        => Regions.FirstOrDefault(r => r.Id == id);

    /// <summary>Returns the highest-priority region containing (x, z), or null if none.</summary>
    public RegionDefinition? GetRegionAt(double x, double z)
    {
        double rx = x - SpawnX;
        double rz = z - SpawnZ;

        // PNG lookup (preferred — pixel-accurate biome map)
        if (pngMap is { IsLoaded: true })
        {
            var regionId = pngMap.GetRegionId(rx, rz);
            if (regionId != null)
                return Regions.FirstOrDefault(r => r.Id == regionId);
            return null;
        }

        // Fallback: AABB/ellipse lookup (legacy)
        return Regions.FirstOrDefault(r => r.Contains(rx, rz));
    }

    /// <summary>Returns biome for the given region, or null.</summary>
    public BiomeDefinition? GetBiome(RegionDefinition region)
        => Biomes.TryGetValue(region.BiomeId, out var b) ? b : null;

    /// <summary>Convenience: region + biome for a position.</summary>
    public (RegionDefinition? Region, BiomeDefinition? Biome) GetAt(double x, double z)
    {
        var region = GetRegionAt(x, z);
        var biome  = region is not null ? GetBiome(region) : null;
        return (region, biome);
    }
}
