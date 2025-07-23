using System.Text.Json.Serialization;

namespace MITREModels.LAYER;

public class Layer
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("versions")]
    public Versions Versions { get; set; }

    [JsonPropertyName("domain")]
    public string Domain { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("filters")]
    public Filters Filters { get; set; }

    [JsonPropertyName("sorting")]
    public int Sorting { get; set; }

    [JsonPropertyName("layout")]
    public Layout Layout { get; set; } 

    [JsonPropertyName("hideDisabled")]
    public bool HideDisabled { get; set; }

    [JsonPropertyName("techniques")]
    public List<Technique> Techniques { get; set; } = new List<Technique>();

    [JsonPropertyName("gradient")]
    public Gradient Gradient { get; set; }

    [JsonPropertyName("legendItems")]
    public List<object> LegendItems { get; set; } = new List<object>();

    [JsonPropertyName("metadata")]
    public List<object> Metadata { get; set; } = new List<object>();

    [JsonPropertyName("links")]
    public List<object> Links { get; set; }  = new List<object>();

    [JsonPropertyName("showTacticRowBackground")]
    public bool ShowTacticRowBackground { get; set; }

    [JsonPropertyName("tacticRowBackground")]
    public string TacticRowBackground { get; set; }

    [JsonPropertyName("selectTechniquesAcrossTactics")]
    public bool SelectTechniquesAcrossTactics { get; set; }

    [JsonPropertyName("selectSubtechniquesWithParent")]
    public bool SelectSubtechniquesWithParent { get; set; }

    [JsonPropertyName("selectVisibleTechniques")]
    public bool SelectVisibleTechniques { get; set; }
}