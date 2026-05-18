using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Lotr.WorldGen.Structures;

/// <summary>
/// Base class for all LOTR procedural structures.
/// Subclasses implement Generate() and place blocks via SetBlock().
/// </summary>
public abstract class StructureBase
{
    protected ICoreServerAPI Api;
    protected IWorldGenBlockAccessor Accessor;

    protected StructureBase(ICoreServerAPI api, IWorldGenBlockAccessor accessor)
    {
        Api      = api;
        Accessor = accessor;
    }

    /// <summary>Generate the structure at world position (originX, surfaceY, originZ).</summary>
    public abstract void Generate(int originX, int surfaceY, int originZ);

    // ── Helpers ──────────────────────────────────────────────────────

    protected void Set(int x, int y, int z, string blockCode)
    {
        var block = Api.World.GetBlock(new AssetLocation(blockCode));
        if (block == null || block.Id == 0) return;
        Accessor.SetBlock(block.Id, new BlockPos(x, y, z));
    }

    protected void SetIf(int x, int y, int z, string blockCode)
    {
        // Only place if current block is air or a replaceable surface block
        var existing = Accessor.GetBlock(new BlockPos(x, y, z));
        if (existing.Id != 0 && !IsReplaceable(existing)) return;
        Set(x, y, z, blockCode);
    }

    protected void Air(int x, int y, int z)
    {
        Accessor.SetBlock(0, new BlockPos(x, y, z));
    }

    protected bool IsReplaceable(Block block)
    {
        var code = block.Code?.Path ?? "";
        return code.StartsWith("soil")   ||
               code.StartsWith("grass")  ||
               code.StartsWith("sand")   ||
               code.StartsWith("gravel") ||
               code.StartsWith("rock")   ||
               code == "air";
    }

    /// Fill a solid cuboid with a block.
    protected void Fill(int x1, int y1, int z1, int x2, int y2, int z2, string blockCode)
    {
        for (int x = x1; x <= x2; x++)
        for (int y = y1; y <= y2; y++)
        for (int z = z1; z <= z2; z++)
            Set(x, y, z, blockCode);
    }

    /// Hollow cuboid — walls/floor/ceiling only, interior air.
    protected void Box(int x1, int y1, int z1, int x2, int y2, int z2, string blockCode)
    {
        for (int x = x1; x <= x2; x++)
        for (int y = y1; y <= y2; y++)
        for (int z = z1; z <= z2; z++)
        {
            if (x == x1 || x == x2 || y == y1 || y == y2 || z == z1 || z == z2)
                Set(x, y, z, blockCode);
            else
                Air(x, y, z);
        }
    }

    /// Carve a corridor (hollow tunnel) along X axis.
    protected void TunnelX(int x1, int x2, int cy, int cz, int w, int h, string wallCode)
    {
        for (int x = x1; x <= x2; x++)
        for (int dy = 0; dy < h; dy++)
        for (int dz = -(w/2); dz <= w/2; dz++)
        {
            int y = cy + dy;
            int z = cz + dz;
            bool isWall = dy == 0 || dy == h - 1 || dz == -(w/2) || dz == w/2;
            if (isWall) Set(x, y, z, wallCode);
            else        Air(x, y, z);
        }
    }
}
