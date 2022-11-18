using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Aurora.Devices.RGBNet.Config;

[Serializable]
public class DeviceMappingConfig
{
    private static Lazy<DeviceMappingConfig> _configLoader = new(LoadConfig);
    public static DeviceMappingConfig Config => _configLoader.Value;
        
    [JsonIgnore]
    private static string _configPath = Path.Combine(Global.AppDataDirectory, "DeviceMappings.json");

    [JsonProperty(PropertyName = "d")]
    public List<RgbNetConfigDevice> Devices { get; set; } = new();

    private static DeviceMappingConfig LoadConfig()
    {
        var config = new DeviceMappingConfig();

        if (!File.Exists(_configPath))
        {
            return new DeviceMappingConfig();
        }

        var content = File.ReadAllText(_configPath, Encoding.UTF8);
        return JsonConvert.DeserializeObject<DeviceMappingConfig>(content) ?? config;
    }

    public void SaveConfig()
    {
        var content = JsonConvert.SerializeObject(this, Formatting.Indented);

        Directory.CreateDirectory(Path.GetDirectoryName(_configPath) ?? throw new InvalidOperationException());
        File.WriteAllText(_configPath, content, Encoding.UTF8);
    }
}