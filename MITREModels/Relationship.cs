using System.Text.Json.Serialization;

namespace MITREModels;

public class Relationship
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("spec_version")]
    public string SpecVersion { get; set; }
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("created")]
    public DateTime Created { get; set; }
    [JsonPropertyName("created_by_ref")]
    public string? CreatedByRef { get; set; }
    [JsonPropertyName("revoked")]
    public bool Revoked { get; set; }
    [JsonPropertyName("external_references")]
    public List<ExternalReference> ExternalReferences { get; set; } = new List<ExternalReference>();
    [JsonPropertyName("object_marking_refs")]
    public List<string> ObjectMarkingRefs { get; set; } = new List<string>();
    [JsonPropertyName("modified")]
    public DateTime Modified { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("relationship_type")]
    public string RelationshipType { get; set; }
    [JsonPropertyName("source_ref")]
    public string SourceRef { get; set; }
    [JsonPropertyName("target_ref")]
    public string TargetRef { get; set; }
    [JsonPropertyName("x_mitre_modified_by_ref")]
    public string XMitreModifiedByRef { get; set; }
    [JsonPropertyName("x_mitre_deprecated")]
    public bool XMitreDeprecated { get; set; }
    [JsonPropertyName("x_mitre_attack_spec_version")]
    public string XMitreAttackSpecVersion { get; set; }
    
    public string CourseOfActionExternalId { get; set; } = string.Empty;
    public string AttackPatternExternalId { get; set; } = string.Empty;
    public List<string> XMitrePlatforms { get; set; } = new List<string>();
    public Guid GroupGuid { get; set; }
    public bool GroupReference { get; set; }
    public string? GroupReferenceId { get; set; }

    public Dictionary<string, Sys> Systems { get; set; } = new Dictionary<string, Sys>();
}