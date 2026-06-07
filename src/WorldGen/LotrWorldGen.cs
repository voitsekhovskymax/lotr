using System;
using System.Collections.Generic;
using Lotr.Regions;
using Lotr.WorldGen.Biomes;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
namespace Lotr.WorldGen;

/// <summary>
/// Hooks into the "lotr" world type chunk generation to override surface
/// blocks per Middle-earth region (Mordor = basalt, Shire = fertile soil).
///
/// ExecuteOrder() = 0.2 → runs after base terrain (0.0) but before
/// vanilla decorators (0.5+), so our surface is decorated normally.
/// </summary>
public class LotrWorldGen : ModSystem
{
    ICoreServerAPI?        sapi;
    IWorldGenBlockAccessor blockAccessor = null!;
    RegionSystem?          regions;

    // Block IDs — resolved once after assets loaded
    int idBasalt;
    int idGravel;
    int idSand;
    int idSoilLow;
    int idSoilMed;
    int idStone;

    readonly BiomeShire     shireGen     = new();
    readonly BiomeMordor    mordorGen    = new();
    readonly BiomeRohan     rohanGen     = new();
    readonly BiomeGondor    gondorGen    = new();
    readonly BiomeLothlorien lothlorienGen = new();
    readonly BiomeFangorn   fangornGen   = new();
    readonly Random worldgenRng = new();

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
        // Resolve block IDs after world has loaded its block registry
        idBasalt  = sapi!.World.GetBlock(new AssetLocation("game:rock-basalt"))?.Id   ?? 0;
        idGravel  = sapi!.World.GetBlock(new AssetLocation("game:gravel-granite"))?.Id ?? 0;
        idSand    = sapi!.World.GetBlock(new AssetLocation("game:sand-ash"))?.Id
                 ?? sapi!.World.GetBlock(new AssetLocation("game:sand-volcanic"))?.Id
                 ?? sapi!.World.GetBlock(new AssetLocation("game:sand"))?.Id           ?? 0;
        idSoilLow = sapi!.World.GetBlock(new AssetLocation("game:soil-low-none"))?.Id ?? 0;
        idSoilMed = sapi!.World.GetBlock(new AssetLocation("game:soil-medium-none"))?.Id ?? 0;
        idStone   = sapi!.World.GetBlock(new AssetLocation("game:rock-granite"))?.Id  ?? 0;

        shireGen.ResolveBlocks(sapi!);
        mordorGen.ResolveBlocks(sapi!);
        rohanGen.ResolveBlocks(sapi!);
        gondorGen.ResolveBlocks(sapi!);
        lothlorienGen.ResolveBlocks(sapi!);
        fangornGen.ResolveBlocks(sapi!);

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

                switch (biome.Id)
                {
                    case "lotr:biome-shire":
                    case "lotr:biome-shire-woodlands":
                    case "lotr:biome-white-downs":
                    case "lotr:biome-shire-moors":
                        shireGen.DecorateColumn(blockAccessor, pos, worldgenRng);
                        break;

                    case "lotr:biome-mordor":
                    case "lotr:biome-gorgoroth":
                    case "lotr:biome-udun":
                    case "lotr:biome-morgul-vale":
                    case "lotr:biome-eastern-desolation":
                        mordorGen.DecorateColumn(blockAccessor, pos, worldgenRng);
                        break;

                    case "lotr:biome-rohan":
                    case "lotr:biome-rohan-woodlands":
                    case "lotr:biome-wold":
                    case "lotr:biome-adornland":
                        rohanGen.DecorateColumn(blockAccessor, pos, worldgenRng);
                        break;

                    case "lotr:biome-gondor":
                    case "lotr:biome-ithilien":
                    case "lotr:biome-lossarnach":
                    case "lotr:biome-lebennin":
                    case "lotr:biome-pelargir":
                        gondorGen.DecorateColumn(blockAccessor, pos, worldgenRng);
                        break;

                    case "lotr:biome-lothlorien":
                    case "lotr:biome-lothlorien-edge":
                    case "lotr:biome-celebrant":
                        lothlorienGen.DecorateColumn(blockAccessor, pos, worldgenRng);
                        break;

                    case "lotr:biome-fangorn":
                    case "lotr:biome-fangorn-clearing":
                    case "lotr:biome-old-forest":
                    case "lotr:biome-woodland-realm":
                    case "lotr:biome-mirkwood-north":
                        fangornGen.DecorateColumn(blockAccessor, pos, worldgenRng);
                        break;

                    // Mountains: gravel sub-surface
                    case "lotr:biome-blue-mountains":
                    case "lotr:biome-misty-mountains":
                    case "lotr:biome-grey-mountains":
                    case "lotr:biome-angmar-mountains":
                    case "lotr:biome-white-mountains":
                    case "lotr:biome-moria":
                        if (surfaceY > 1)
                        {
                            var below = new BlockPos(baseX + lx, surfaceY - 1, baseZ + lz);
                            if (IsSurfaceReplaceable(blockAccessor.GetBlock(below).Id))
                                blockAccessor.SetBlock(idGravel, below);
                        }
                        break;
                }
            }
        }
    }

    // ── Helpers ─────────────────────────────────────────────────────

    int SurfaceBlockFor(string biomeId)
    {
        // Exact overrides for specific biomes
        return biomeId switch
        {
            // ── Shire & surroundings (rich/medium soil) ─────────────────
            "lotr:biome-shire"               => shireGen.SurfaceBlock,
            "lotr:biome-shire-woodlands"     => shireGen.SurfaceBlock,
            "lotr:biome-white-downs"         => shireGen.SurfaceBlock,
            "lotr:biome-shire-moors"         => idSoilLow,
            "lotr:biome-shire-marshes"       => idSoilLow,
            "lotr:biome-old-forest"          => idSoilMed,
            "lotr:biome-barrow-downs"        => idSoilLow,

            // ── Rohan (medium grassland) ─────────────────────────────────
            "lotr:biome-rohan"               => idSoilMed,
            "lotr:biome-rohan-woodlands"     => idSoilMed,
            "lotr:biome-rohan-uruk-highlands"=> idSoilLow,
            "lotr:biome-wold"                => idSoilLow,
            "lotr:biome-adornland"           => idSoilMed,

            // ── Gondor & surroundings ────────────────────────────────────
            "lotr:biome-gondor"              => idSoilLow,
            "lotr:biome-minas-tirith"        => idSoilLow,
            "lotr:biome-ithilien"            => idSoilMed,
            "lotr:biome-ithilien-hills"      => idSoilLow,
            "lotr:biome-ithilien-wasteland"  => idSand,
            "lotr:biome-pelargir"            => idSoilLow,
            "lotr:biome-lossarnach"          => idSoilMed,
            "lotr:biome-imloth-melui"        => idSoilMed,
            "lotr:biome-lamedon"             => idSoilLow,
            "lotr:biome-lamedon-hills"       => idSoilLow,
            "lotr:biome-blackroot-vale"      => idSoilMed,
            "lotr:biome-pinnath-gelin"       => idSoilLow,
            "lotr:biome-dor-en-ernil"        => idSoilLow,
            "lotr:biome-dor-en-ernil-hills"  => idSoilLow,
            "lotr:biome-andrast"             => idSoilLow,
            "lotr:biome-lebennin"            => idSoilMed,
            "lotr:biome-gondor-woodlands"    => idSoilMed,
            "lotr:biome-pelennor"            => idSoilLow,
            "lotr:biome-pukel"               => idSoilLow,
            "lotr:biome-nan-curunir"         => idSoilMed,
            "lotr:biome-white-mountains-foothills" => idGravel,

            // ── White/Grey Mountains (stone) ─────────────────────────────
            "lotr:biome-white-mountains"     => idStone,
            "lotr:biome-grey-mountains"      => idStone,
            "lotr:biome-grey-mountains-foothills" => idGravel,

            // ── Bree & Eriador ────────────────────────────────────────────
            "lotr:biome-bree"                => idSoilLow,
            "lotr:biome-breeland"            => idSoilLow,
            "lotr:biome-chetwood"            => idSoilMed,
            "lotr:biome-eriador"             => idSoilLow,
            "lotr:biome-eriador-downs"       => idSoilLow,
            "lotr:biome-midgewater"          => idSoilLow,
            "lotr:biome-lone-lands"          => idSoilLow,
            "lotr:biome-lone-lands-hills"    => idSoilLow,
            "lotr:biome-angle"               => idSoilMed,
            "lotr:biome-coldfells"           => idGravel,
            "lotr:biome-ettenmoors"          => idSoilLow,
            "lotr:biome-tower-hills"         => idSoilLow,

            // ── Rivendell & Trollshaws ────────────────────────────────────
            "lotr:biome-rivendell"           => idSoilMed,
            "lotr:biome-trollshaws"          => idSoilMed,
            "lotr:biome-eregion"             => idSoilMed,

            // ── Blue Mountains ────────────────────────────────────────────
            "lotr:biome-blue-mountains"      => idStone,
            "lotr:biome-blue-mountains-foothills" => idGravel,
            "lotr:biome-lindon"              => idSoilMed,
            "lotr:biome-lindon-woodlands"    => idSoilMed,
            "lotr:biome-lindon-coast"        => idSand,
            "lotr:biome-minhiriath"          => idSoilMed,
            "lotr:biome-enedwaith"           => idSoilLow,
            "lotr:biome-dunland"             => idSoilLow,
            "lotr:biome-eryn-vorn"           => idSoilMed,
            "lotr:biome-swanfleet"           => idSoilLow,

            // ── Isengard & Fangorn ────────────────────────────────────────
            "lotr:biome-isengard"            => idSoilLow,
            "lotr:biome-fangorn"             => idSoilMed,
            "lotr:biome-fangorn-clearing"    => idSoilMed,
            "lotr:biome-fangorn-wasteland"   => idSoilLow,

            // ── Misty Mountains ───────────────────────────────────────────
            "lotr:biome-misty-mountains"     => idStone,
            "lotr:biome-misty-mountains-foothills" => idGravel,
            "lotr:biome-moria"               => idStone,

            // ── Anduin & Gladden Fields ───────────────────────────────────
            "lotr:biome-anduin-vale"         => idSoilMed,
            "lotr:biome-anduin-hills"        => idSoilLow,
            "lotr:biome-anduin-mouth"        => idSoilLow,
            "lotr:biome-gladden-fields"      => idSoilLow,
            "lotr:biome-nindalf"             => idSoilLow,
            "lotr:biome-entwash-mouth"       => idSoilLow,
            "lotr:biome-celebrant"           => idSoilMed,
            "lotr:biome-long-marshes"        => idSoilLow,
            "lotr:biome-brown-lands"         => idSoilLow,

            // ── Lothlórien ────────────────────────────────────────────────
            "lotr:biome-lothlorien"          => idSoilMed,
            "lotr:biome-lothlorien-edge"     => idSoilMed,

            // ── Mirkwood ─────────────────────────────────────────────────
            "lotr:biome-woodland-realm"      => idSoilMed,
            "lotr:biome-woodland-realm-hills"=> idSoilLow,
            "lotr:biome-mirkwood-corrupted"  => idSoilLow,
            "lotr:biome-mirkwood-mountains"  => idStone,
            "lotr:biome-mirkwood-north"      => idSoilMed,
            "lotr:biome-dol-guldur"          => idSoilLow,
            "lotr:biome-east-bight"          => idSoilLow,

            // ── Rhûn & Dorwinion ──────────────────────────────────────────
            "lotr:biome-rhun"                => idSoilLow,
            "lotr:biome-rhun-forest"         => idSoilMed,
            "lotr:biome-rhun-land"           => idSoilLow,
            "lotr:biome-rhun-land-steppe"    => idSoilLow,
            "lotr:biome-rhun-land-hills"     => idSoilLow,
            "lotr:biome-rhun-red-forest"     => idSoilMed,
            "lotr:biome-rhun-island"         => idSoilMed,
            "lotr:biome-rhun-island-forest"  => idSoilMed,
            "lotr:biome-red-mountains"       => idStone,
            "lotr:biome-red-mountains-foothills" => idGravel,
            "lotr:biome-dorwinion"           => idSoilMed,
            "lotr:biome-dorwinion-hills"     => idSoilLow,
            "lotr:biome-wilderland"          => idSoilLow,
            "lotr:biome-wilderland-north"    => idSoilLow,

            // ── Erebor, Dale & Iron Hills ─────────────────────────────────
            "lotr:biome-erebor"              => idStone,
            "lotr:biome-dale"                => idSoilLow,
            "lotr:biome-iron-hills"          => idStone,

            // ── Angmar & Far North ────────────────────────────────────────
            "lotr:biome-angmar"              => idGravel,
            "lotr:biome-angmar-mountains"    => idStone,
            "lotr:biome-forodwaith"          => idGravel,
            "lotr:biome-forodwaith-mountains"=> idStone,
            "lotr:biome-forodwaith-glacier"  => idGravel,
            "lotr:biome-forodwaith-coast"    => idSand,
            "lotr:biome-tundra"              => idGravel,
            "lotr:biome-taiga"               => idSoilLow,

            // ── Mordor & surroundings ─────────────────────────────────────
            "lotr:biome-mordor"              => idBasalt,
            "lotr:biome-mordor-mountains"    => idStone,
            "lotr:biome-gorgoroth"           => idBasalt,
            "lotr:biome-udun"                => idBasalt,
            "lotr:biome-dagorlad"            => idSand,
            "lotr:biome-dead-marshes"        => idSoilLow,
            "lotr:biome-nurn"                => idSoilLow,
            "lotr:biome-nurnen"              => idSoilLow,
            "lotr:biome-nurn-marshes"        => idSoilLow,
            "lotr:biome-emyn-muil"           => idStone,
            "lotr:biome-morgul-vale"         => idBasalt,
            "lotr:biome-nan-ungol"           => idGravel,
            "lotr:biome-eastern-desolation"  => idBasalt,

            // ── Harad ─────────────────────────────────────────────────────
            "lotr:biome-near-harad"          => idSand,
            "lotr:biome-near-harad-hills"    => idSand,
            "lotr:biome-near-harad-fertile"  => idSoilLow,
            "lotr:biome-near-harad-fertile-forest" => idSoilMed,
            "lotr:biome-near-harad-oasis"    => idSoilMed,
            "lotr:biome-near-harad-red-desert" => idSand,
            "lotr:biome-near-harad-riverbank"=> idSoilLow,
            "lotr:biome-near-harad-semi-desert" => idSand,
            "lotr:biome-far-harad"           => idSand,
            "lotr:biome-far-harad-arid"      => idSand,
            "lotr:biome-far-harad-arid-hills"=> idSand,
            "lotr:biome-far-harad-jungle"    => idSoilMed,
            "lotr:biome-far-harad-jungle-edge" => idSoilMed,
            "lotr:biome-far-harad-jungle-lake" => idSoilMed,
            "lotr:biome-far-harad-forest"    => idSoilMed,
            "lotr:biome-far-harad-bushland"  => idSoilLow,
            "lotr:biome-far-harad-bushland-hills" => idSoilLow,
            "lotr:biome-far-harad-cloud-forest" => idSoilMed,
            "lotr:biome-far-harad-swamp"     => idSoilLow,
            "lotr:biome-far-harad-mangrove"  => idSoilLow,
            "lotr:biome-far-harad-volcano"   => idBasalt,
            "lotr:biome-far-harad-coast"     => idSand,
            "lotr:biome-harad-mountains"     => idStone,
            "lotr:biome-umbar"               => idSand,
            "lotr:biome-umbar-hills"         => idSoilLow,
            "lotr:biome-umbar-forest"        => idSoilMed,
            "lotr:biome-gulf-harad"          => idSand,
            "lotr:biome-gulf-harad-forest"   => idSoilMed,
            "lotr:biome-pertorogwaith"       => idSand,
            "lotr:biome-lostladen"           => idSand,
            "lotr:biome-tauredain-clearing"  => idSoilMed,
            "lotr:biome-harnedor"            => idSoilLow,

            // ── Wind Mountains, Last Desert ───────────────────────────────
            "lotr:biome-wind-mountains"      => idStone,
            "lotr:biome-wind-mountains-foothills" => idGravel,
            "lotr:biome-last-desert"         => idSand,

            // ── Beaches ───────────────────────────────────────────────────
            "lotr:biome-beach"               => idSand,
            "lotr:biome-beach-gravel"        => idGravel,
            "lotr:biome-beach-white"         => idSand,

            _ => 0   // 0 = no override for ocean, rivers, and unknown
        };
    }

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
