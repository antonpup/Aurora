using System;
using System.Collections.Generic;
using Lombok.NET;
using Newtonsoft.Json;
using RGB.NET.Core;

namespace Aurora.Devices.RGBNet.Config;

[Serializable]
[NoArgsConstructor]
public partial class RgbNetConfigDevice
{
    [JsonProperty(PropertyName = "n")]
    public string Name { get; init; }

    [JsonProperty(PropertyName = "k")]
    public Dictionary<LedId, DeviceKeys> KeyMapper { get; init; }

    public RgbNetConfigDevice(IRGBDevice device)
    {
        Name = device.DeviceInfo.DeviceName;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Name}";
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
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