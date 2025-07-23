using MITREModels.STIX;

namespace MET.Models;

public class Platform
{
    public string? Name { get; set; }
    public List<string> Systems { get; set; } = new List<string>();
    public List<Relationship> Relationships { get; set; } = new List<Relationship>();
    public Dictionary<string, Relationship> OldRelationships { get; set; }  = new Dictionary<string, Relationship>();
}