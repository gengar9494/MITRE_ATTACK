using System.Text.Json.Serialization;

namespace MITREModels.LAYER;

public class Filters
{
    [JsonPropertyName("platforms")]
    public List<string> Platforms { get; set; }
}