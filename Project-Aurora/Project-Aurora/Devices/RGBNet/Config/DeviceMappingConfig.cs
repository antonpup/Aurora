using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Aurora.Devices.RGBNet.Config;

[Serializable]
public class DeviceMappingConfig
{
    private static Lazy<DeviceMappingConfig> _configLoader = new(LoadConfig, LazyThreadSafetyMode.ExecutionAndPublication);
    public static DeviceMappingConfig Config => _configLoader.Value;

    [JsonIgnore]
    private static string _configPath = Path.Combine(Global.AppDataDirectory, "DeviceMappings.json");

    [JsonProperty(PropertyName = "d")]
    public List<RgbNetConfigDevice> Devices { get; set; } = new();

    private DeviceMappingConfig()
    {
    }

    private static DeviceMappingConfig LoadConfig()
    {
        if (!File.Exists(_configPath))
        {
            return new DeviceMappingConfig();
        }

        using var file = File.OpenText(_configPath);
        var serializer = new JsonSerializer();
        
        return serializer.Deserialize<DeviceMappingConfig>(new JsonTextReader(file)) ?? new DeviceMappingConfig();
    }

    public void SaveConfig()
    {
        var content = JsonConvert.SerializeObject(this, Formatting.Indented);

        Directory.CreateDirectory(Path.GetDirectoryName(_configPath) ?? throw new InvalidOperationException());
        File.WriteAllText(_configPath, content, Encoding.UTF8);
    }
}