using System.Text.Json.Serialization;

namespace MITREModels.STIX;

public class AttackPattern
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("spec_version")]
    public required string SpecVersion { get; init; }

    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("created")]
    public DateTime Created { get; init; }

    [JsonPropertyName("created_by_ref")]
    public required string CreatedByRef { get; init; }

    [JsonPropertyName("revoked")]
    public bool Revoked { get; init; }

    [JsonPropertyName("external_references")]
    public List<ExternalReference> ExternalReferences { get; init; } = new List<ExternalReference>();

    [JsonPropertyName("object_marking_refs")]
    public List<string> ObjectMarkingRefs { get; init; } = new List<string>();

    [JsonPropertyName("modified")]
    public DateTime Modified { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("kill_chain_phases")]
    public List<KillChainPhase> KillChainPhases { get; init; } = new List<KillChainPhase>();

    [JsonPropertyName("x_mitre_attack_spec_version")]
    public required string XMitreAttackSpecVersion { get; init; }

    [JsonPropertyName("x_mitre_contributors")]
    public List<string> XMitreContributors { get; init; } = new List<string>();

    [JsonPropertyName("x_mitre_deprecated")]
    public bool XMitreDeprecated { get; init; }

    [JsonPropertyName("x_mitre_detection")]
    public string? XMitreDetection { get; init; }

    [JsonPropertyName("x_mitre_domains")]
    public List<string> XMitreDomains { get; init; } = new List<string>();

    [JsonPropertyName("x_mitre_is_subtechnique")]
    public bool XMitreIsSubtechnique { get; init; }

    [JsonPropertyName("x_mitre_modified_by_ref")]
    public required string XMitreModifiedByRef { get; init; }

    [JsonPropertyName("x_mitre_platforms")]
    public List<string> XMitrePlatforms { get; init; } = new List<string>();

    [JsonPropertyName("x_mitre_version")]
    public required string XMitreVersion { get; init; }

    [JsonPropertyName("x_mitre_data_sources")]
    public List<string> XMitreDataSources { get; init; }  = new List<string>();
}