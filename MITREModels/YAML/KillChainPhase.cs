using YamlDotNet.Serialization;

namespace MITREModels.YAML;

public class KillChainPhase
{
    [YamlMember(Alias = "kill_chain_name")]
    public string KillChainName { get; set; }

    [YamlMember(Alias = "phase_name")]
    public string PhaseName { get; set; }
}