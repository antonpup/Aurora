using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AuraServiceLib;
using Newtonsoft.Json;

namespace Aurora.Devices.Asus.Config
{
    [Serializable]
    public struct AsusConfig
    {
        [JsonIgnore]
        private static string configPath = Path.Combine(Global.AppDataDirectory, "AsusDevices.json");
        
        [JsonProperty(PropertyName = "d")]
        public List<AsusConfigDevice> Devices;
        
        public static AsusConfig LoadConfig()
        {
            var config = new AsusConfig()
            {
                Devices = new List<AsusConfigDevice>()
            };
            
            if (!File.Exists(configPath))
                return config;

            string content = File.ReadAllText(configPath, Encoding.UTF8);

            if (string.IsNullOrWhiteSpace(content))
                return config;

            return JsonConvert.DeserializeObject<AsusConfig>(content);
        }
        
        public static void SaveConfig(AsusConfig config)
        {
            string content = JsonConvert.SerializeObject(config, Formatting.Indented);

            Directory.CreateDirectory(Path.GetDirectoryName(configPath) ?? throw new InvalidOperationException());
            File.WriteAllText(configPath, content, Encoding.UTF8);
        }
        
        [Serializable]
        public struct AsusConfigDevice
        {
            [JsonProperty(PropertyName = "n")]
            public string Name;
            [JsonProperty(PropertyName = "t")]
            public uint Type;
            [JsonProperty(PropertyName = "c")]
            public int KeyCount;
            [JsonProperty(PropertyName = "k")]
            public Dictionary<int, DeviceKeys> KeyMapper;
            [JsonProperty(PropertyName = "e")]
            public bool Enabled;

            public AsusConfigDevice(IAuraSyncDevice device)
            {
                Name = device.Name;
                Enabled = true;
                Type = device.Type;
                KeyCount = device.Lights.Count;
                KeyMapper = new Dictionary<int, DeviceKeys>();
            }
            
            /// <inheritdoc />
            public override string ToString()
            {
                return $"[{(AsusHandler.AsusDeviceType) Type}] {Name} {KeyCount}";
            }

            /// <inheritdoc />
            public override bool Equals(object obj)
            {
                if (!(obj is AsusConfigDevice device))
                    return false;

                return ToString() == device.ToString();
            }

            /// <inheritdoc />
            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }
        }
    }
}
