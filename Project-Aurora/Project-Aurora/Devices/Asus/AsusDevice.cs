using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Aurora.Settings;
using Aurora.Utils;

namespace Aurora.Devices.Asus
{
    public class AsusDevice : DefaultDevice
    {
        private AsusHandler _asusHandler = new();

        /// <inheritdoc />
        public override string DeviceName => "Asus";

        /// <inheritdoc />
        public override string DeviceDetails => GetDeviceStatus();

        private string GetDeviceStatus()
        {
            if (!IsInitialized)
                return "Not Initialized";

            if (_asusHandler.DeviceCount == 0)
                return "Initialized: No devices connected";

            return "Initialized: " + _asusHandler?.GetDevicePerformance();
        }

        /// <inheritdoc />
        public override bool Initialize()
        {
            _asusHandler?.Stop();

            _asusHandler = new AsusHandler(
                Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_enable_unsupported_version"),
                Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_force_initialize"));
            IsInitialized = _asusHandler.Start();
            return IsInitialized;
        }

        /// <inheritdoc />
        public override void Shutdown()
        {
            if (!IsInitialized) return;

            IsInitialized = false;
            _asusHandler.Stop();
        }

        /// <inheritdoc />
        protected override bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (!IsInitialized)
            {
                return false;
            }
            _asusHandler.UpdateColors(keyColors);
            return true;
        }

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            variableRegistry.Register($"{DeviceName}_enable_unsupported_version", false, "Enable Unsupported Asus SDK Version");
            variableRegistry.Register($"{DeviceName}_force_initialize", false, "Force initialization");
            variableRegistry.Register($"{DeviceName}_color_cal", new RealColor(Color.FromArgb(255, 255, 255, 255)), "Color Calibration");
        }
    }
}
