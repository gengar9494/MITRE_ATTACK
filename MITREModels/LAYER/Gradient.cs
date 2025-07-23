using System.Text.Json.Serialization;

namespace MITREModels.LAYER;

public class Gradient
{
    [JsonPropertyName("colors")]
    public List<string> Colors { get; set; }

    [JsonPropertyName("minValue")]
    public int MinValue { get; set; }

    [JsonPropertyName("maxValue")]
    public int MaxValue { get; set; }
}