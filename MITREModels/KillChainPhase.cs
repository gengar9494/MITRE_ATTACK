using System.Text.Json.Serialization;

namespace MITREModels;

public class KillChainPhase
{
    [JsonPropertyName("kill_chain_name")]
    public required string KillChainName { get; init; }

    [JsonPropertyName("phase_name")]
    public required string PhaseName { get; init; }
}