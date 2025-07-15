using System.Text.Json.Serialization;

namespace MITREModels;

public class ExternalReference
{
    [JsonPropertyName("source_name")]
    public required string SourceName { get; init; }

    [JsonPropertyName("url")]
    public string? Url { get; init; }

    [JsonPropertyName("external_id")]
    public string? ExternalId { get; init; }
    
    [JsonPropertyName("description")]
    public string? Description { get; init; }
}