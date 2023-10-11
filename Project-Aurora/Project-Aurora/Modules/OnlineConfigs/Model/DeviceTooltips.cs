using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Aurora.Modules.OnlineConfigs.Model;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class DeviceTooltips
{
    public bool Recommended { get; }
    public bool Beta { get; }
    public string? Info { get; }
    public string? SdkLink { get; }

    public DeviceTooltips()
    {
    }

    [JsonConstructor]
    public DeviceTooltips(bool recommended, bool beta, string? info, string? sdkLink)
    {
        Recommended = recommended;
        Beta = beta;
        Info = info;
        SdkLink = sdkLink;
    }
}