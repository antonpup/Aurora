using Newtonsoft.Json;
using RGB.NET.Core;

namespace Common.Devices.RGBNet;

[JsonObject]
public class RemappableDevice
{
    public string DeviceId;
    public string DeviceSummary;
    
    // $"[{rgbDevice.DeviceInfo.DeviceType}] {rgbDevice.DeviceInfo.DeviceName}"
    public List<LedId> RgbNetLeds;

    public RemappableDevice(string deviceId, string deviceSummary, List<LedId> rgbNetLeds)
    {
        DeviceId = deviceId;
        DeviceSummary = deviceSummary;
        RgbNetLeds = rgbNetLeds;
    }
}