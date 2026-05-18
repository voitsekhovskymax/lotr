using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lotr.Regions;

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
    [JsonProperty("surfaceBlock")] public string SurfaceBlock { get; set; } = "";
    [JsonProperty("flora")]       public List<string> Flora   { get; set; } = new();
    [JsonProperty("fauna")]       public List<string> Fauna   { get; set; } = new();
    [JsonProperty("lava")]        public bool   Lava          { get; set; }
    [JsonProperty("notes")]       public string Notes         { get; set; } = "";

    public float TempMid      => (TempMin + TempMax) / 2f;
    public float RainMid      => (RainMin + RainMax) / 2f;
    public float FertilityMid => (FertilityMin + FertilityMax) / 2f;
}
