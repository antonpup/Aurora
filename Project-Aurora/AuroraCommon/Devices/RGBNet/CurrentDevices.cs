using Newtonsoft.Json;

namespace Common.Devices.RGBNet;

[JsonObject]
public class CurrentDevices
{
    public CurrentDevices(List<RemappableDevice> devices)
    {
        Devices = devices;
    }

    public List<RemappableDevice> Devices { get; }
}