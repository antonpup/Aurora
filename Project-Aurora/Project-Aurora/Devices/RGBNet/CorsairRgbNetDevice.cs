using System.Linq;
using RGB.NET.Devices.Corsair;

namespace Aurora.Devices.RGBNet;

public class CorsairRgbNetDevice : RgbNetDevice
{
    protected override CorsairDeviceProvider Provider => CorsairDeviceProvider.Instance;

    public override string DeviceName => "Corsair (RGB.NET)";
    protected override string DeviceInfo => string.Join(", ", Provider.Devices.Select(d => d.DeviceInfo.DeviceName));
    
}