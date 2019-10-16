using Aurora.Devices.RGBNet;
using RGB.NET.Devices.Asus;

namespace Aurora.Devices.Asus
{
    public class AsusDevice : AbstractRGBNetDevice
    {
        #region Properties & Fields

        protected override string DeviceName => "Asus";

        #endregion

        #region Constructors

        public AsusDevice()
            : base(new AsusDeviceProviderLoader().GetDeviceProvider(), new AsusRGBNetBrush())
        { }

        #endregion
    }
}
