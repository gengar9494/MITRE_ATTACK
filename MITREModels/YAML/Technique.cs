using YamlDotNet.Serialization;

namespace MITREModels.YAML;

public class Technique
{
    [YamlMember(Alias = "type")]
    public string Type { get; set; }

    [YamlMember(Alias = "id")]
    public string Id { get; set; }

    [YamlMember(Alias = "created")]
    public string Created { get; set; }

    [YamlMember(Alias = "created_by_ref")]
    public string CreatedByRef { get; set; }

    [YamlMember(Alias = "external_references")]
    public List<ExternalReference> ExternalReferences { get; set; }

    [YamlMember(Alias = "object_marking_refs")]
    public List<string> ObjectMarkingRefs { get; set; }

    [YamlMember(Alias = "modified")]
    public string Modified { get; set; }

    [YamlMember(Alias = "name")]
    public string Name { get; set; }

    [YamlMember(Alias = "description")]
    public string Description { get; set; }

    [YamlMember(Alias = "kill_chain_phases")]
    public List<KillChainPhase> KillChainPhases { get; set; }

    [YamlMember(Alias = "x_mitre_attack_spec_version")]
    public string XMitreAttackSpecVersion { get; set; }

    [YamlMember(Alias = "x_mitre_detection")]
    public string XMitreDetection { get; set; }

    [YamlMember(Alias = "x_mitre_domains")]
    public List<string> XMitreDomains { get; set; }

    [YamlMember(Alias = "x_mitre_is_subtechnique")]
    public bool XMitreIsSubtechnique { get; set; }

    [YamlMember(Alias = "x_mitre_modified_by_ref")]
    public string XMitreModifiedByRef { get; set; }

    [YamlMember(Alias = "x_mitre_platforms")]
    public List<string> XMitrePlatforms { get; set; }

    [YamlMember(Alias = "x_mitre_version")]
    public string XMitreVersion { get; set; }

    [YamlMember(Alias = "x_mitre_data_sources")]
    public List<string> XMitreDataSources { get; set; }

    [YamlMember(Alias = "identifier")]
    public string Identifier { get; set; }

    [YamlMember(Alias = "revoked")]
    public bool Revoked { get; set; }

    [YamlMember(Alias = "x_mitre_contributors")]
    public List<string> XMitreContributors { get; set; }

    [YamlMember(Alias = "x_mitre_deprecated")]
    public bool XMitreDeprecated { get; set; }
    
    [YamlMember(Alias = "x_mitre_remote_support")]
    public bool XMitreRemoteSupport { get; set; }
    
    [YamlMember(Alias = "x_mitre_impact_type")]
    public List<string> XMitreImpactType { get; set; }
}