using System.Text.Json.Serialization;

namespace MITREModels.LAYER;


public class Layout
{
    [JsonPropertyName("layout")]
    public string LayoutType { get; set; }

    [JsonPropertyName("aggregateFunction")]
    public string AggregateFunction { get; set; }

    [JsonPropertyName("showID")]
    public bool ShowID { get; set; }

    [JsonPropertyName("showName")]
    public bool ShowName { get; set; }

    [JsonPropertyName("showAggregateScores")]
    public bool ShowAggregateScores { get; set; }

    [JsonPropertyName("countUnscored")]
    public bool CountUnscored { get; set; }

    [JsonPropertyName("expandedSubtechniques")]
    public string ExpandedSubtechniques { get; set; }
}