using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using RGB.NET.Core;
using RGB.NET.Devices.Bloody;
using RGB.NET.Devices.Logitech;

namespace Aurora.Devices.RGBNet;

public class BloodyRgbNetDevice : RgbNetDevice
{
    protected override IRGBDeviceProvider Provider => BloodyDeviceFactory.Instance;

    public override string DeviceName => "Bloody (RGB.NET)";
    protected override string DeviceInfo => string.Join(", ", Provider.Devices.Select(d => d.DeviceInfo.DeviceName));
}