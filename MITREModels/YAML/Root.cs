using YamlDotNet.Serialization;

namespace MITREModels.YAML;

public class Root
{
    [YamlMember(Alias = "defense-evasion")]
    public Dictionary<string, Dictionary<string, TechniqueContainer>> DefenseEvasion { get; set; }
}