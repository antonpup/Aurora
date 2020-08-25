using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Aurora.Settings;
using Microsoft.Win32;

namespace Aurora.Devices.Asus
{
    public class AsusDevice : IDevice
    {
        public const string deviceName = "Asus";
        private AsusHandler asusHandler = new AsusHandler();
        private bool isActive = false;

        private VariableRegistry defaultRegistry = null;
        /// <inheritdoc />
        public VariableRegistry RegisteredVariables
        {
            get
            {
                if (defaultRegistry != null) return defaultRegistry;

                defaultRegistry = new VariableRegistry();
                defaultRegistry.Register($"{DeviceName}_enable_unsupported_version", false, "Enable Unsupported Asus SDK Version");
                defaultRegistry.Register($"{DeviceName}_force_initialize", false, "Force initialization");
                return defaultRegistry;
            }
        }

        /// <inheritdoc />
        public string DeviceName => deviceName;

        /// <inheritdoc />
        public string DeviceDetails => GetDeviceStatus();

        private string GetDeviceStatus()
        {
            if (!isActive)
                return "Not Initialized";
            
            if (asusHandler.DeviceCount == 0)
                return "Initialized: No devices connected";

            return "Initialized: " + asusHandler?.GetDevicePerformance();
        }

        /// <inheritdoc />
        public string DeviceUpdatePerformance => "";

        /// <inheritdoc />
        public bool Initialize()
        {
            asusHandler?.Stop();
            
            asusHandler = new AsusHandler(
                Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_enable_unsupported_version"),
                Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_force_initialize"));
            isActive = asusHandler.Start();
            return isActive;
        }

        /// <inheritdoc />
        public void Shutdown()
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
        public bool Reconnect()
        {
            Shutdown();
            Initialize();
            
            return isActive;
        }

        /// <inheritdoc />
        public bool IsInitialized => isActive;

        /// <inheritdoc />
        public bool IsConnected() => isActive;

        /// <inheritdoc />
        public bool IsKeyboardConnected()
        {
            return asusHandler?.KeyboardActive() ?? false;
        }

        /// <inheritdoc />
        public bool IsPeripheralConnected()
        {
            return asusHandler?.MouseActive() ?? false;
        }

        /// <inheritdoc />
        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            asusHandler.UpdateColors(keyColors);
            return true;
        }

        /// <inheritdoc />
        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            asusHandler.UpdateColors(colorComposition.keyColors);
            return true;
        }
        
    }
}
