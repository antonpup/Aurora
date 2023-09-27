using Common;
using Common.Devices;
using IronPython.Modules;
using Newtonsoft.Json;
using RGB.NET.Core;

namespace AurorDeviceManager.Devices.RGBNet.Config;

[Serializable]
public class RgbNetConfigDevice
{
    [JsonProperty(PropertyName = "n")]
    public string Name { get; init; }

    [JsonProperty(PropertyName = "k")]
    public Dictionary<LedId, DeviceKeys> KeyMapper { get; } = new(Constants.MaxKeyId);

    [JsonConstructor]
    public RgbNetConfigDevice(string name)
    {
        Name = name;
    }

    public RgbNetConfigDevice(IRGBDevice device)
    {
        Name = device.DeviceInfo.DeviceName;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is not RgbNetConfigDevice device)
            return false;

        return Name == device.Name;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}