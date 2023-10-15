using Newtonsoft.Json;

namespace Common.Devices.RGBNet;

[JsonObject]
public class CurrentDevices
{
    public List<RemappableDevice> Devices;
}