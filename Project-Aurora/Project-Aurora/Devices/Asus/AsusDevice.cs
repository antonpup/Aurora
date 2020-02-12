using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Aurora.Devices.RGBNet;
using Aurora.Settings;
using RGB.NET.Devices.Asus;

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
        public string GetDeviceDetails() => $"{DeviceName}: {asusHandler?.GetDevicePerformance()}";

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
            return true;
        }

        /// <inheritdoc />
        public bool IsPeripheralConnected()
        {
            return true;
        }

        /// <inheritdoc />
        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            // asusHandler.UpdateColors(keyColors);
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
