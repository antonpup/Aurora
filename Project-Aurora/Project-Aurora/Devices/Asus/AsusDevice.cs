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
        private AuraState _state = AuraState.Off;
        private AuraState State
        {
            get => _state;
            set
            {
                Global.logger.Info($"[ASUS] Status {_state} -> {value}");
                _state = value;
            }
        }
        
        private VariableRegistry _defaultRegistry;
        private AsusHandler _asusHandler;
        private bool _runAsync;


        /// <inheritdoc />
        public string GetDeviceName() => DeviceName;

        /// <inheritdoc />
        public string GetDeviceDetails()
        {
            var result = $"{DeviceName}: ";
            switch (State)
            {
                case AuraState.Off:
                    result += "Not initialized";
                    break;
                case AuraState.Stopping:
                    result += "Stopping";
                    break;
                case AuraState.Starting:
                    result += "Connecting";
                    break;
                case AuraState.On:
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
        public VariableRegistry GetRegisteredVariables()
        {
            if (_defaultRegistry != null) return _defaultRegistry;

            _defaultRegistry = new VariableRegistry();
            _defaultRegistry.Register($"{DeviceName}_async", false, "Run asynchronously");
            return _defaultRegistry;
        }

        /// <inheritdoc />
        public bool Initialize()
        {
            if (State == AuraState.Starting || State == AuraState.On || State == AuraState.Stopping)
                return true;

            if (_asusHandler == null)
                _asusHandler = new AsusHandler();

            State = AuraState.Starting;

            _runAsync = Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_async");
            return _asusHandler.GetControl(_runAsync, success => State = success ? AuraState.On : State = AuraState.Off);
        }

        /// <inheritdoc />
        public void Shutdown()
        {
            StopAsus();
        }

        /// <inheritdoc />
        public void Reset()
        {
            StopAsus(() => Initialize());
        }

        private void StopAsus(Action onComplete = null)
        {
            if (State == AuraState.Off || State == AuraState.Stopping || State == AuraState.Starting)
                return;

            State = AuraState.Stopping;
            _asusHandler.ReleaseControl((uninitialized) =>
            {
                if (uninitialized)
                    State = AuraState.Off;

                onComplete?.Invoke();
            });
        }

        /// <inheritdoc />
        public bool Reconnect()
        {
            return false;
        }

        /// <inheritdoc />
        public bool IsInitialized() => IsConnected();
        /// <inheritdoc />
        public bool IsConnected() => State == AuraState.On || State == AuraState.Starting || State == AuraState.Stopping;

        /// <inheritdoc />
        public bool IsKeyboardConnected() => IsConnected();

        /// <inheritdoc />
        public bool IsPeripheralConnected() => IsConnected();

        /// <inheritdoc />
        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            var runAsyncRegistry = Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_async");
            if (_runAsync != runAsyncRegistry)
            {
                Reset();
                _runAsync = runAsyncRegistry;
                return false;
            }

            _asusHandler?.UpdateDevices(keyColors);
            return true;
        }

        /// <inheritdoc />
        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            return UpdateDevice(colorComposition.keyColors, e, forced);
        }

        private enum AuraState
        {
            Off,
            Starting,
            On,
            Stopping,
        }
    }
}
