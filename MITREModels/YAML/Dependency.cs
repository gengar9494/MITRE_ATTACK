using YamlDotNet.Serialization;

namespace MITREModels.YAML;

public class Dependency
{
    [YamlMember(Alias = "description")]
    public string Description { get; set; }

    [YamlMember(Alias = "prereq_command")]
    public string PrereqCommand { get; set; }

    [YamlMember(Alias = "get_prereq_command")]
    public string GetPrereqCommand { get; set; }
}