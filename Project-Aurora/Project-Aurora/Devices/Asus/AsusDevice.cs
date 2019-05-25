using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Settings;

namespace Aurora.Devices.Asus
{
    /// <summary>
    /// Acts as the interfacing class to Aurora
    /// </summary>
    public class AsusDevice : Device
    {
        private const string DeviceName = "Asus";
        private bool _isConnected = false;
        private readonly VariableRegistry _defaultRegistry = new VariableRegistry();
        private AsusHandler _asusHandler;

        /// <inheritdoc />
        public VariableRegistry GetRegisteredVariables()
        {
            return _defaultRegistry;
        }

        /// <inheritdoc />
        public string GetDeviceName() => DeviceName;

        /// <inheritdoc />
        public string GetDeviceDetails() =>
            $"{DeviceName}: is {(_isConnected ? " Initialised": " Not Initialised")}";

        /// <inheritdoc />
        public string GetDeviceUpdatePerformance() => GetDeviceDetails();

        /// <inheritdoc />
        public bool Initialize()
        {
            if (_asusHandler == null)
            {
                _asusHandler = new AsusHandler();
            }
            
            _asusHandler.GetControl(success =>
                {
                    _isConnected = success;
                });

            return _isConnected;
        }

        /// <inheritdoc />
        public void Shutdown()
        {
            _asusHandler.ReleaseControl();
        }

        /// <inheritdoc />
        public void Reset()
        {
            _asusHandler.ReleaseControl();
        }

        /// <inheritdoc />
        public bool Reconnect()
        {
            return false;
        }

        /// <inheritdoc />
        public bool IsInitialized() => _isConnected;
        /// <inheritdoc />
        public bool IsConnected() => _isConnected;

        /// <inheritdoc />
        public bool IsKeyboardConnected() => _isConnected;

        /// <inheritdoc />
        public bool IsPeripheralConnected() => _isConnected;

        /// <inheritdoc />
        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            _asusHandler?.UpdateDevices(keyColors);
            return true;
        }

        /// <inheritdoc />
        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            return UpdateDevice(colorComposition.keyColors, e, forced);
        }

    }
}
