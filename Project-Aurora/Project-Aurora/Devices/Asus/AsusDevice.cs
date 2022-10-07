using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Aurora.Settings;
using Aurora.Utils;

namespace Aurora.Devices.Asus
{
    public class AsusDevice : DefaultDevice
    {
        public const string deviceName = "Asus";
        private AsusHandler asusHandler = new AsusHandler();
        private bool isActive = false;

        /// <inheritdoc />
        public override string DeviceName => deviceName;

        /// <inheritdoc />
        public override string DeviceDetails => GetDeviceStatus();

        private string GetDeviceStatus()
        {
            if (!isActive)
                return "Not Initialized";

            if (asusHandler.DeviceCount == 0)
                return "Initialized: No devices connected";

            return "Initialized: " + asusHandler?.GetDevicePerformance();
        }

        /// <inheritdoc />
        public override bool Initialize()
        {
            asusHandler?.Stop();

            asusHandler = new AsusHandler(
                Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_enable_unsupported_version"),
                Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_force_initialize"));
            isActive = asusHandler.Start();
            return isActive;
        }

        /// <inheritdoc />
        public override void Shutdown()
        {
            if (!isActive) return;

            asusHandler.Stop();
            isActive = false;
        }

        /// <inheritdoc />
        public void Reset()
        {
            Shutdown();
            Initialize();
        }

        /// <inheritdoc />
        public bool IsInitialized => isActive;

        /// <inheritdoc />
        protected override bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            asusHandler.UpdateColors(keyColors);
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
