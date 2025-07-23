using YamlDotNet.Serialization;

namespace MITREModels.YAML;

public class Executor
{
    [YamlMember(Alias = "command")]
    public string Command { get; set; }

    [YamlMember(Alias = "cleanup_command")]
    public string CleanupCommand { get; set; }

    [YamlMember(Alias = "name")]
    public string Name { get; set; }

    [YamlMember(Alias = "elevation_required")]
    public bool ElevationRequired { get; set; }

    [YamlMember(Alias = "steps")]
    public string Steps { get; set; }
}