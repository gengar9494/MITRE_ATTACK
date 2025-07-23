using YamlDotNet.Serialization;

namespace MITREModels.YAML;

public class ExternalReference
{
    [YamlMember(Alias = "source_name")]
    public string SourceName { get; set; }

    [YamlMember(Alias = "url")]
    public string Url { get; set; }

    [YamlMember(Alias = "external_id")]
    public string ExternalId { get; set; }

    [YamlMember(Alias = "description")]
    public string Description { get; set; }
}