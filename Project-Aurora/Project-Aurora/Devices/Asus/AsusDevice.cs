using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Aurora.Settings;

namespace Aurora.Devices.Asus
{
    public class AsusDevice : Device
    {
        private const string DeviceName = "Asus";
        private readonly AsusHandler asusHandler = new AsusHandler();
        private bool isActive = false;
        
        /// <inheritdoc />
        public VariableRegistry GetRegisteredVariables()
        {
            return new VariableRegistry();
        }

        /// <inheritdoc />
        public string GetDeviceName() => DeviceName;

        /// <inheritdoc />
        public string GetDeviceDetails() => $"{DeviceName}: {GetDeviceStatus()}";

        private string GetDeviceStatus()
        {
            if (!isActive)
                return "Not initialized";
            
            if (asusHandler.DeviceCount == 0)
                return "No devices connected";

            return asusHandler?.GetDevicePerformance();
        }
        
        /// <inheritdoc />
        public string GetDeviceUpdatePerformance()
        {
            return "";
        }

        /// <inheritdoc />
        public bool Initialize()
        {
            isActive = asusHandler.Start();
            return isActive;
        }

        /// <inheritdoc />
        public void Shutdown()
        {
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
        public bool IsInitialized() => isActive;

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
