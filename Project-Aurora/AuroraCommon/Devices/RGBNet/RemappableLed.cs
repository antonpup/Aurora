using RGB.NET.Core;

namespace Common.Devices.RGBNet;

public record struct RemappableLed
{
    public LedId LedId;
    public DeviceKeys RemappedKey;
    public SimpleColor ColorOverride;

    public RemappableLed(LedId ledId, DeviceKeys remappedKey, SimpleColor colorOverride)
    {
        LedId = ledId;
        RemappedKey = remappedKey;
        ColorOverride = colorOverride;
    }
}