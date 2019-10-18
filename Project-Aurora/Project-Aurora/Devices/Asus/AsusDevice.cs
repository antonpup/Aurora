using Aurora.Devices.RGBNet;
using RGB.NET.Devices.Asus;
using System.IO;

namespace Aurora.Devices.Asus
{
    public class AsusDevice : AbstractRGBNetDevice
    {
        #region Properties & Fields

        protected override string DeviceName => "Asus";

        #endregion

        #region Constructors

        public AsusDevice()
            : base(new AsusDeviceProviderLoader().GetDeviceProvider(), new CustomRGBNetBrush(Path.Combine(Global.ExecutingDirectory, "kb_layouts", "AsusLedMapping.txt")))
        { }

        #endregion
    }
}
