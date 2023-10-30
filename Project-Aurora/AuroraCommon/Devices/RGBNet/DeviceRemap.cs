using Newtonsoft.Json;
using RGB.NET.Core;

namespace Common.Devices.RGBNet;

[Serializable]
public class DeviceRemap
{
    [JsonProperty(PropertyName = "n")]
    public string Name { get; init; }

    [JsonProperty(PropertyName = "k")]
    public Dictionary<LedId, DeviceKeys> KeyMapper { get; } = new(Constants.MaxKeyId);

    [JsonConstructor]
    public DeviceRemap(string name)
    {
        Name = name;
    }

    public DeviceRemap(RemappableDevice device)
    {
        Name = device.DeviceId;
    }

    public override string ToString()
    {
        return Name;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not DeviceRemap device)
            return false;

        return Name == device.Name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}