using System.Linq;
using RGB.NET.Core;
using RGB.NET.Devices.CoolerMaster;

namespace Aurora.Devices.RGBNet;

public class CoolerMasterRgbNetDevice : RgbNetDevice
{
    protected override IRGBDeviceProvider Provider => CoolerMasterDeviceProvider.Instance;

    public override string DeviceName => "CoolerMaster (RGB.NET)";
}