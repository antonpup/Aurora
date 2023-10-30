using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Aurora.Modules.OnlineConfigs.Model;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class OnlineSettingsMeta
{
    public DateTimeOffset OnlineSettingsTime { get; set; } = DateTimeOffset.MinValue;
}