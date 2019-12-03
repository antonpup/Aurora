using Aurora.Devices.RGBNet;
using Aurora.Settings;
using RGB.NET.Devices.Asus;

namespace Aurora.Devices.Asus
{
    public class AsusDevice : AbstractRGBNetDevice
    {
        #region Properties & Fields

        protected override string DeviceName => "Asus";
        private VariableRegistry default_registry = null;
        #endregion

        #region Constructors

        public AsusDevice()
            : base(new AsusDeviceProviderLoader().GetDeviceProvider(), new AsusRGBNetBrush())
        { }

        #endregion

        #region Methods
        public override VariableRegistry GetRegisteredVariables()
        {
            if (default_registry == null)
            {
                default_registry = new VariableRegistry();
                default_registry.Register($"{DeviceName}_devicekey", DeviceKeys.ESC, "Key to Use", DeviceKeys.MOUSEPADLIGHT15, DeviceKeys.Peripheral);
            }

            return default_registry;
        }
        #endregion
    }
}
