using YamlDotNet.Serialization;

namespace MITREModels.YAML;

public class InputArgument
{
    [YamlMember(Alias = "description")]
    public string Description { get; set; }

    [YamlMember(Alias = "type")]
    public string Type { get; set; }

    [YamlMember(Alias = "default")]
    public string Default { get; set; }
}