# Vintage Story World Generation — Reference

> Version: VS 1.22 / .NET 10
> Status: Verified from game assets + wiki (wiki biome/climate pages are stubs — use this doc)

---

## Architecture Overview

VS world gen has NO single "biome" concept. Instead, three independent systems combine:

```
MapRegion (512x512 blocks)
  └── ClimateMap (temperature, rainfall, fertility noise)
  └── GeologicProvinceMap (rock type distribution)

MapChunk (32x32 columns)
  └── WorldGenTerrainHeightMap
  └── RainHeightMap (topmost solid block)

ChunkColumn (32x32x32 per vertical chunk)
  └── Actual blocks generated per-pass
```

---

## Generation Passes (EnumWorldGenPass)

| Index | Name                  | What happens                                      | Needs neighbours |
|-------|-----------------------|---------------------------------------------------|-----------------|
| 0     | None                  | Nothing yet                                       | No              |
| 1     | Terrain               | Basic terrain, rock strata, caves, block layers   | No              |
| 2     | TerrainFeatures       | Deposits, structures, ponds                       | Yes             |
| 3     | Vegetation            | Block patches, trees, rivulets, sunlight          | Yes             |
| 4     | NeighbourSunLightFlood| Snow layer, sunlight flood to neighbours          | Yes             |
| 5     | PreDone               | Done generating, spawn creatures                  | Yes             |
| 6     | Done                  | Not triggered as event                            | —               |

---

## C# Registration Pattern

```csharp
public class LotrWorldGen : ModSystem
{
    IWorldGenBlockAccessor blockAccessor;

    public override double ExecuteOrder() => 0.2; // after base (0), before decorators (0.5)

    public override void StartServerSide(ICoreServerAPI api)
    {
        // 1. ALWAYS register first — gets thread-safe accessor
        api.Event.GetWorldgenBlockAccessor(OnGetBlockAccessor);

        // 2. Register for generation events
        api.Event.ChunkColumnGeneration(OnChunkColumnGen, EnumWorldGenPass.Vegetation, "standard");
        api.Event.MapRegionGeneration(OnMapRegionGen, "standard");
    }

    void OnGetBlockAccessor(IChunkProviderThread chunkProvider)
    {
        blockAccessor = chunkProvider.GetBlockAccessor(updateHeightmap: true);
    }

    void OnChunkColumnGen(IChunkColumnGenerateRequest request)
    {
        blockAccessor.BeginColumn(); // MUST call — clears internal cache
        int worldX = request.ChunkX * GlobalConstants.ChunkSize;
        int worldZ = request.ChunkZ * GlobalConstants.ChunkSize;
        // place blocks via blockAccessor (NOT api.World.BlockAccessor)
    }

    void OnMapRegionGen(IMapRegion mapRegion, int regionX, int regionZ, ITreeAttribute chunkGenParams)
    {
        // regionX/regionZ in map-region units (each = 16 chunk columns = 512 blocks)
        // Read/write mapRegion.ModData for custom climate overrides
    }
}
```

---

## Landforms (assets/survival/worldgen/landforms.json)

Control terrain HEIGHT and SHAPE via a noise-to-Y spline.

```json
{
  "code": "lotr-shire-hills",
  "weight": 10,
  "terrainOctaves":          [1, 1, 1, 0, 0, 0.1, 0.1, 0, 0],
  "terrainOctaveThresholds": [0, 0, 0, 0, 0,   0,   0, 0, 0],
  "terrainYKeyPositions":  [0.42, 0.50],
  "terrainYKeyThresholds": [1.00, 0.00]
}
```

**terrainYKeyPositions** — normalised Y (0=world bottom, 1=world top, ~0.43 = sea level y≈110).
Higher values = higher terrain. Sea level is approx Y=110 in a 256-height world.

| Terrain type     | KeyPositions example       | Notes                       |
|------------------|----------------------------|-----------------------------|
| Deep ocean       | [0.30, 0.38]               | Well below sea level        |
| Flat lowlands    | [0.40, 0.46]               | Near sea level              |
| Rolling hills    | [0.42, 0.52]               | Gentle rise above sea level |
| Plateau          | [0.48, 0.56]               | Flat elevated land          |
| Mountains        | [0.42, 0.55, 0.75, 1.00]   | With multiple breakpoints   |
| High peaks       | [0.45, 0.65, 0.85, 1.00]   | Snow-capped                 |

---

## Climate Parameters

Set in `blocklayers.json` and `blockpatches.json` as conditions for block/flora placement.

| Parameter       | Range      | Notes                                          |
|-----------------|------------|------------------------------------------------|
| minTemp/maxTemp | -30..+40   | °C equivalent                                  |
| minRain/maxRain | 0.0..1.0   | Annual rainfall fraction                       |
| minFertility    | 0.0..1.0   | Soil fertility                                 |
| minY/maxY       | 0.0..1.0   | Normalised world height                        |
| maxForest       | 0.0..1.0   | Max forest density for patch to appear         |

---

## Water Bodies

| Type         | How generated                                                         |
|--------------|-----------------------------------------------------------------------|
| Ocean        | Terrain below sea level (~Y=110) fills with water automatically       |
| Lakes / Ponds| TerrainFeatures pass (pass 2) — driven by `deposits.json` pond entries|
| Rivers       | Vegetation pass (pass 3) — rivulets via `blockpatches.json`           |
| Lava lakes   | Same as ponds but blockCode = lava. Used in Mordor biome              |

---

## Geologic Provinces (assets/survival/worldgen/geologicprovinces.json)

Controls underground rock type distribution.

| Province          | Rock type     | Notes                              |
|-------------------|---------------|------------------------------------|
| shield            | Igneous only  | Thin soil, metamorphic rock        |
| platform          | Sedimentary+  | Standard fertile lands             |
| orogen            | Mixed         | Mountain regions                   |
| largeIgneousprovince | Volcanic   | Old volcanoes — good for Mordor    |

---

## Sea Level Reference

```
World height 256 blocks (default standard world)
  Y=256  — top of world (sky)
  Y=200  — high mountain peaks
  Y=150  — mid mountains
  Y=110  — SEA LEVEL (approximate)
  Y=90   — underground caves common
  Y=1    — bedrock
```

Normalised Y = actual Y / world height. For 256-height world: sea level ≈ 0.43.
