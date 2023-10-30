using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RGB.NET.Core;

namespace Common.Devices.RGBNet;

[JsonObject]
public class RemappableDevice
{
    public string DeviceId { get; }
    public string DeviceSummary { get; }
    
    // $"[{rgbDevice.DeviceInfo.DeviceType}] {rgbDevice.DeviceInfo.DeviceName}"
    public List<LedId> RgbNetLeds { get; }

    public RemappableDevice(string deviceId, string deviceSummary, List<LedId> rgbNetLeds)
    {
        DeviceId = deviceId;
        DeviceSummary = deviceSummary;
        RgbNetLeds = rgbNetLeds;
    }
}