using System.Collections.Generic;
using Newtonsoft.Json;

namespace Aurora.Utils.Steam;

[JsonObject]
public class SteamLibraryFolder
{
    [JsonProperty("path")]
    public string Path { get; set; } = "";

    [JsonProperty("label")]
    public string Label { get; set; } = "";

    [JsonProperty("apps")]
    public Dictionary<int, string> Apps { get; set; } = new();
}

[JsonObject]
public class SteamLibrary
{
    [JsonProperty("libraryfolders")]
    public Dictionary<int, SteamLibraryFolder> Libraryfolders { get; set; } = new();
}
