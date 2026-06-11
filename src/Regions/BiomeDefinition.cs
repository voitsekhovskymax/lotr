using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lotr.Regions;

/// <summary>Weighted surface decoration (flower, tall grass, berry bush, fire…).</summary>
public class BiomeDecoration
{
    [JsonProperty("block")]  public string Block  { get; set; } = "";
    [JsonProperty("weight")] public int    Weight { get; set; } = 1;
}

/// <summary>Weighted tree generator entry (vanilla treegen codes, e.g. game:englishoak).</summary>
public class BiomeTree
{
    [JsonProperty("code")]   public string Code   { get; set; } = "";
    [JsonProperty("weight")] public int    Weight { get; set; } = 1;
    [JsonProperty("size")]   public float  Size   { get; set; } = 1f;
}

public class BiomeDefinition
{
    [JsonProperty("id")]          public string Id          { get; set; } = "";
    [JsonProperty("displayName")] public string DisplayName { get; set; } = "";
    [JsonProperty("tempMin")]     public float  TempMin     { get; set; }
    [JsonProperty("tempMax")]     public float  TempMax     { get; set; }
    [JsonProperty("rainMin")]     public float  RainMin     { get; set; }
    [JsonProperty("rainMax")]     public float  RainMax     { get; set; }
    [JsonProperty("fertilityMin")] public float FertilityMin { get; set; }
    [JsonProperty("fertilityMax")] public float FertilityMax { get; set; }
    [JsonProperty("landformHint")] public string LandformHint { get; set; } = "";

    /// <summary>Full VS block code for the surface ("game:soil-rich-normal"). Empty = no override.</summary>
    [JsonProperty("surfaceBlock")]    public string SurfaceBlock    { get; set; } = "";
    /// <summary>Full VS block code placed 1 block below the surface. Empty = keep existing.</summary>
    [JsonProperty("subSurfaceBlock")] public string SubSurfaceBlock { get; set; } = "";

    /// <summary>Per-column probability (0..1) of placing one weighted decoration above the surface.</summary>
    [JsonProperty("decorationChance")] public float DecorationChance { get; set; }
    [JsonProperty("decorations")]      public List<BiomeDecoration> Decorations { get; set; } = new();

    /// <summary>Per-column probability (0..1) of growing one weighted tree.</summary>
    [JsonProperty("treeChance")] public float TreeChance { get; set; }
    [JsonProperty("trees")]      public List<BiomeTree> Trees { get; set; } = new();

    /// <summary>Ore type → multiplier (&gt;1 spawns extra veins; e.g. "galena": 3.0 in Moria).</summary>
    [JsonProperty("oreBoost")] public Dictionary<string, float> OreBoost { get; set; } = new();

    [JsonProperty("flora")] public List<string> Flora { get; set; } = new();
    [JsonProperty("fauna")] public List<string> Fauna { get; set; } = new();
    [JsonProperty("lava")]  public bool   Lava  { get; set; }
    [JsonProperty("notes")] public string Notes { get; set; } = "";

    public float TempMid      => (TempMin + TempMax) / 2f;
    public float RainMid      => (RainMin + RainMax) / 2f;
    public float FertilityMid => (FertilityMin + FertilityMax) / 2f;
}
