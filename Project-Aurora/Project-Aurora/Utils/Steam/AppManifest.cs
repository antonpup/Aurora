using Newtonsoft.Json;

namespace Aurora.Utils.Steam;

[JsonObject]
public class AppManifest
{
    public AppState AppState { get; set; } = new();
}

[JsonObject]
public class AppState
{
    [JsonProperty("appid")]
    public string AppId { get; set; } = "";
    
    [JsonProperty("installdir")]
    public string InstallDir { get; set; } = "";
}