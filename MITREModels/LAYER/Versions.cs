using System.Text.Json.Serialization;

namespace MITREModels.LAYER;

public class Versions
{
    [JsonPropertyName("attack")]
    public string Attack { get; set; }

    [JsonPropertyName("navigator")]
    public string Navigator { get; set; }

    [JsonPropertyName("layer")]
    public string Layer { get; set; }
}