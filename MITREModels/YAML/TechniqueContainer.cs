using YamlDotNet.Serialization;

namespace MITREModels.YAML;

public class TechniqueContainer
{
    [YamlMember(Alias = "technique")]
    public Technique Technique { get; set; }

    [YamlMember(Alias = "atomic_tests")]
    public List<AtomicTest> AtomicTests { get; set; }
}