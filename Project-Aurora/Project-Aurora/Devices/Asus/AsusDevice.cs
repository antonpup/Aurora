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
        private State _state = State.Off;
        private bool _isConnected => _state == State.On || _state == State.Starting;
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
        public string GetDeviceDetails()
        {
            var result = $"{DeviceName}: ";
            switch (_state)
            {
                case State.Off:
                    result += "Not initialized";
                    break;
                case State.Stopping:
                    result += "Stopping";
                    break;
                case State.Starting:
                    result += "Connecting";
                    break;
                case State.On:
                    result += "Connected. Devices: " + _asusHandler?.GetDeviceStatus();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }

        /// <inheritdoc />
        public string GetDeviceUpdatePerformance() => "";

        /// <inheritdoc />
        public bool Initialize()
        {
            if (_state == State.Starting || _state == State.On)
                return true;

            if (_asusHandler == null)
                _asusHandler = new AsusHandler();

            _state = State.Starting;

            return _asusHandler.GetControl(success => _state = success ? State.On : _state = State.Off);
        }

        /// <inheritdoc />
        public void Shutdown()
        {
            StopAsus();
        }

        /// <inheritdoc />
        public void Reset()
        {
            StopAsus();
        }

        private void StopAsus()
        {
            _state = State.Stopping;
            _asusHandler.ReleaseControl((uninitialized) =>
            {
                if (uninitialized)
                    _state = State.Off;
            });
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

        private enum State
        {
            Off,
            Starting,
            On,
            Stopping,
        }
    }
}
