using System.Text.Json.Serialization;

namespace MITREModels;

public class CourseOfAction
{
    [JsonPropertyName("modified")]
    public DateTime Modified { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("x_mitre_deprecated")]
    public bool XMitreDeprecated { get; init; }

    [JsonPropertyName("x_mitre_domains")]
    public required List<string> XMitreDomains { get; init; }

    [JsonPropertyName("x_mitre_version")]
    public required string XMitreVersion { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("spec_version")]
    public required string SpecVersion { get; init; }

    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("created")]
    public required DateTime Created { get; init; }

    [JsonPropertyName("created_by_ref")]
    public required string CreatedByRef { get; init; }

    [JsonPropertyName("revoked")]
    public bool Revoked { get; init; }

    [JsonPropertyName("external_references")]
    public List<ExternalReference> ExternalReferences { get; init; } = new List<ExternalReference>();

    [JsonPropertyName("object_marking_refs")]
    public List<string> ObjectMarkingRefs { get; init; } = new List<string>();

    [JsonPropertyName("x_mitre_attack_spec_version")]
    public required string XMitreAttackSpecVersion { get; init; }

    [JsonPropertyName("x_mitre_modified_by_ref")]
    public required string XMitreModifiedByRef { get; init; }
}

