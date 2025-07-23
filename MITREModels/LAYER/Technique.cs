using System.Text.Json.Serialization;

namespace MITREModels.LAYER;

public class Technique
{
    [JsonPropertyName("techniqueID")]
    public string? TechniqueId { get; set; }

    [JsonPropertyName("tactic")]
    public string? Tactic { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("metadata")]
    public List<MetaData> Metadata { get; set; } = new List<MetaData>();

    [JsonPropertyName("links")]
    public List<string> Links { get; set; } = new List<string>();

    [JsonPropertyName("showSubtechniques")]
    public bool ShowSubTechniques { get; set; }

    [JsonPropertyName("score")]
    public int? Score { get; set; }
}