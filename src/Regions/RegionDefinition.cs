using Newtonsoft.Json;

namespace Lotr.Regions;

public class RegionDefinition
{
    [JsonProperty("id")]          public string Id          { get; set; } = "";
    [JsonProperty("displayName")] public string DisplayName { get; set; } = "";
    [JsonProperty("biome")]       public string BiomeId     { get; set; } = "";
    [JsonProperty("faction")]     public string FactionId   { get; set; } = "";
    [JsonProperty("centerX")]     public int    CenterX     { get; set; }
    [JsonProperty("centerZ")]     public int    CenterZ     { get; set; }
    [JsonProperty("radiusX")]     public int    RadiusX     { get; set; }
    [JsonProperty("radiusZ")]     public int    RadiusZ     { get; set; }
    [JsonProperty("priority")]    public int    Priority    { get; set; } = 10;
    [JsonProperty("notes")]       public string Notes       { get; set; } = "";

    /// <summary>True if world position (x, z) falls inside this region's AABB.</summary>
    public bool Contains(double x, double z)
    {
        double dx = (x - CenterX) / RadiusX;
        double dz = (z - CenterZ) / RadiusZ;
        return dx * dx + dz * dz <= 1.0; // ellipse check
    }
}
