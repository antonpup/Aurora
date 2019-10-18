using Aurora.Devices.RGBNet;
using RGB.NET.Devices.Corsair;
using System.IO;

namespace Aurora.Devices.Corsair
{
    public class CorsairDevice : AbstractRGBNetDevice
    {
        #region Properties & Fields

        protected override string DeviceName => "Corsair";

        #endregion

        #region Constructors
        
        public CorsairDevice()
            : base(new CorsairDeviceProviderLoader().GetDeviceProvider(), new CustomRGBNetBrush(Path.Combine(Global.ExecutingDirectory, "kb_layouts", "CorsairLedMapping.txt")))
        { }

        #endregion
    }
}
