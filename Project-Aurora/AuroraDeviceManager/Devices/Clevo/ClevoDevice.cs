using System.ComponentModel;
using System.Drawing;
using Common.Devices;
using Microsoft.Win32;

namespace AuroraDeviceManager.Devices.Clevo
{
    class ClevoDevice : DefaultDevice
    {
        // Settings
        // TODO: Theese settings could be implemented with posibility of configuration from the Aurora GUI (Or external JSON, INI, Settings, etc)
        private bool _useGlobalPeriphericColors = false;
        private bool _useTouchpad = true;
        private bool _updateLightsOnLogon = true;

        // Clevo Controll Class
        private ClevoSetKBLED _clevo = new();

        // Color Variables
        private Color _colorKbCenter = Color.Black;
        private Color _colorKbLeft = Color.Black;
        private Color _colorKbRight = Color.Black;
        private Color _colorTouchpad = Color.Black;
        private bool _colorUpdated;
        private Color _lastColorKbCenter = Color.Black;
        private Color _lastColorKbLeft = Color.Black;
        private Color _lastColorKbRight = Color.Black;
        private Color _lastColorTouchpad = Color.Black;

        // Session Switch Handler
        private SessionSwitchEventHandler _sseh;

        public override string DeviceName => "Clevo Keyboard";

        protected override Task<bool> DoInitialize()
        {
            if (IsInitialized) return Task.FromResult(true);
            try
            {
                // Initialize Clevo WMI Interface Connection
                if (!_clevo.Initialize())
                {
                    throw new Exception("Could not connect to Clevo WMI Interface");
                }

                // Update Lights on Logon (Clevo sometimes resets the lights when you Hibernate, this would fix wrong colors)
                if (_updateLightsOnLogon)
                {
                    _sseh = SystemEvents_SessionSwitch;
                    SystemEvents.SessionSwitch += _sseh;
                }

                // Mark Initialized = TRUE
                IsInitialized = true;
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Global.Logger.Error(ex, "Clevo device, Exception");
            }

            // Mark Initialized = FALSE
            IsInitialized = false;
            return Task.FromResult(false);
        }

        // Handle Logon Event
        void SystemEvents_SessionSwitch(object? sender, SessionSwitchEventArgs e)
        {
            if (IsInitialized&& e.Reason.Equals(SessionSwitchReason.SessionUnlock))
            { // Only Update when Logged In
                SendColorsToKeyboard(true);
            }
        }

        protected override Task Shutdown()
        {
            if (!IsInitialized) return Task.CompletedTask;
            // Release Clevo Connection
            _clevo.ResetKBLEDColors();
            _clevo.Release();

            // Uninstantiate Session Switch
            if (_sseh == null) return Task.CompletedTask;
            SystemEvents.SessionSwitch -= _sseh;
            _sseh = null;
            return Task.CompletedTask;
        }

        public override Task Reset()
        {
            if (IsInitialized)
                _clevo.ResetKBLEDColors();
            return Task.CompletedTask;
        }

        protected override Task<bool> UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (e.Cancel) return Task.FromResult(false);
            bool updateResult;

            if (e.Cancel) return Task.FromResult(false);
            try
            {
                foreach (KeyValuePair<DeviceKeys, Color> pair in keyColors)
                {
                    if (e.Cancel) return Task.FromResult(false);
                    if (_useGlobalPeriphericColors)
                    {
                        if (pair.Key == DeviceKeys.Peripheral) // This is not working anymore. Was working in MASTER
                        {
                            _colorKbLeft = pair.Value;
                            _colorKbCenter = pair.Value;
                            _colorKbRight = pair.Value;
                            _colorTouchpad = pair.Value;
                            _colorUpdated = true;
                        }
                    }
                    else
                    {
                        // TouchPad (It would be nice to have a Touchpad Peripheral)
                        if (pair.Key == DeviceKeys.Peripheral)
                        {
                            _colorTouchpad = pair.Value;
                            _colorUpdated = true;
                        }
                    }
                }

                if (e.Cancel) return Task.FromResult(false);
                if (!_useGlobalPeriphericColors)
                {
                    // Clevo 3 region keyboard

                    // Left Side (From ESC to Half Spacebar)
                    Color regionLeftColor = keyColors[DeviceKeys.ADDITIONALLIGHT1];

                    if (!_colorKbLeft.Equals(regionLeftColor))
                    {
                        _colorKbLeft = regionLeftColor;
                        _colorUpdated = true;
                    }

                    if (e.Cancel) return Task.FromResult(false);

                    // Center (Other Half of Spacebar to F11) - Clevo keyboards are very compact and the right side color bleeds over to the up/left/right/down keys)
                    Color regionCenterColor = keyColors[DeviceKeys.ADDITIONALLIGHT2];

                    if (!_colorKbCenter.Equals(regionCenterColor))
                    {
                        _colorKbCenter = regionCenterColor;
                        _colorUpdated = true;
                    }

                    if (e.Cancel) return Task.FromResult(false);

                    // Right Side
                    Color regionRightColor = keyColors[DeviceKeys.ADDITIONALLIGHT3];

                    if (!_colorKbRight.Equals(regionRightColor))
                    {
                        _colorKbRight = regionRightColor;
                        _colorUpdated = true;
                    }
                }

                if (e.Cancel) return Task.FromResult(false);
                SendColorsToKeyboard(forced);
                updateResult = true;
            }
            catch (Exception exception)
            {
                Global.Logger.Error(exception, "Clevo device, error when updating device");
                updateResult = false;
            }

            return Task.FromResult(updateResult);
        }

        private void SendColorsToKeyboard(bool forced = false)
        {
            if (!forced && !_colorUpdated) return;
            if ((forced || !_lastColorKbLeft.Equals(_colorKbLeft)) && !Global.DeviceConfig.DevicesDisableKeyboard)
            {
                // MYSTERY: // Why is it B,R,G instead of R,G,B? SetKBLED uses R,G,B but only B,R,G returns the correct colors. Is bitshifting different in C# than in C++?
                _clevo.SetKBLED(ClevoSetKBLED.KBLEDAREA.ColorKBLeft, _colorKbLeft.B, _colorKbLeft.R, _colorKbLeft.G, _colorKbLeft.A / 0xff);
                _lastColorKbLeft = _colorKbLeft;
            }
            if ((forced || !_lastColorKbCenter.Equals(_colorKbCenter)) && !Global.DeviceConfig.DevicesDisableKeyboard)
            {
                _clevo.SetKBLED(ClevoSetKBLED.KBLEDAREA.ColorKBCenter, _colorKbCenter.B, _colorKbCenter.R, _colorKbCenter.G, _colorKbCenter.A / 0xff);
                _lastColorKbCenter = _colorKbCenter;
            }
            if ((forced || !_lastColorKbRight.Equals(_colorKbRight)) && !Global.DeviceConfig.DevicesDisableKeyboard)
            {
                _clevo.SetKBLED(ClevoSetKBLED.KBLEDAREA.ColorKBRight, _colorKbRight.B, _colorKbRight.R, _colorKbRight.G, _colorKbRight.A / 0xff);
                _lastColorKbRight = _colorKbRight;
            }
            if ((forced || (_useTouchpad && !_lastColorTouchpad.Equals(_colorTouchpad))) && !Global.DeviceConfig.DevicesDisableMouse)
            {
                _clevo.SetKBLED(ClevoSetKBLED.KBLEDAREA.ColorTouchpad, _colorTouchpad.B, _colorTouchpad.R, _colorTouchpad.G, _colorTouchpad.A / 0xff);
                _lastColorTouchpad = _colorTouchpad;
            }
            _colorUpdated = false;
        }
    }
}
