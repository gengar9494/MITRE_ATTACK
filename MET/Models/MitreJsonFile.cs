using System.Text.Json.Serialization;

namespace MET.Models;

public class MitreJsonFile
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("download_url")]
    public Uri DownloadUrl { get; set; } = new Uri(@"");
    
    [JsonPropertyName("size")]
    public int? Size { get; set; }

    public Version? Version { get; set; }

    public void CalcVersion()
    {
        var text = Name.Split("-");
            
        if (text.Length != 3)
            return;
            
        var versionString = text[2].Replace(".json", "");
        Version = new Version(versionString);
    }
}