using Aurora.Devices.RGBNet;
using RGB.NET.Devices.Corsair;

namespace Aurora.Devices.Corsair
{
    public class CorsairDevice : AbstractRGBNetDevice
    {
        #region Properties & Fields

        protected override string DeviceName => "Corsair";

        #endregion

        #region Constructors
        
        public CorsairDevice()
            : base(new CorsairDeviceProviderLoader().GetDeviceProvider(), new CorsairRGBNetBrush())
        { }

        #endregion
    }
}
