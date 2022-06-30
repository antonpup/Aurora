using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using Aurora.Settings;
using Aurora.Utils;
using Microsoft.Win32;

namespace Aurora.Devices.Clevo
{
    class ClevoDevice : IDevice
    {
        // Generic Variables
        private bool _isInitialized;

        private readonly Stopwatch _watch = new();
        private long _lastUpdateTime;

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

        public string DeviceName => "Clevo Keyboard";

        public string DeviceDetails => IsInitialized
            ? "Initialized"
            : "Not Initialized";

        public bool Initialize()
        {
            if (_isInitialized) return _isInitialized;
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
                _isInitialized = true;
                return true;
            }
            catch (Exception ex)
            {
                Global.logger.Error("Clevo device, Exception! Message:" + ex);
            }

            // Mark Initialized = FALSE
            _isInitialized = false;
            return false;

        }

        // Handle Logon Event
        void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (IsInitialized&& e.Reason.Equals(SessionSwitchReason.SessionUnlock))
            { // Only Update when Logged In
                SendColorsToKeyboard(true);
            }
        }

        public void Shutdown()
        {
            if (!IsInitialized) return;
            // Release Clevo Connection
            _clevo.ResetKBLEDColors();
            _clevo.Release();

            // Uninstantiate Session Switch
            if (_sseh == null) return;
            SystemEvents.SessionSwitch -= _sseh;
            _sseh = null;
        }

        public void Reset()
        {
            if (IsInitialized)
                _clevo.ResetKBLEDColors();
        }

        public bool IsInitialized => _isInitialized;

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            if (e.Cancel) return false;
            _watch.Restart();
            bool updateResult;

            Dictionary<int, Color> keyColors = colorComposition.KeyColors;
            if (e.Cancel) return false;
            try
            {
                foreach (KeyValuePair<int, Color> pair in keyColors)
                {
                    if (e.Cancel) return false;
                    if (_useGlobalPeriphericColors)
                    {
                        if (pair.Key == (int)DeviceKeys.Peripheral) // This is not working anymore. Was working in MASTER
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
                        if (pair.Key == (int)DeviceKeys.Peripheral)
                        {
                            _colorTouchpad = pair.Value;
                            _colorUpdated = true;
                        }
                    }
                }

                if (e.Cancel) return false;
                if (!_useGlobalPeriphericColors)
                {
                    // Clevo 3 region keyboard

                    // Left Side (From ESC to Half Spacebar)
                    BitmapRectangle keymapEsc = Effects.GetBitmappingFromDeviceKey(DeviceKeys.ESC);
                    BitmapRectangle keymapSpace = Effects.GetBitmappingFromDeviceKey(DeviceKeys.SPACE);
                    PointF spacebarCenter = keymapSpace.Center; // Key Center

                    int spacebarX = (int)spacebarCenter.X - keymapEsc.Left;
                    int height = (int)spacebarCenter.Y - keymapEsc.Top;

                    Rectangle regionLeft =
                        new Rectangle(keymapEsc.Left, keymapEsc.Top, spacebarX, height);

                    Color regionLeftColor;

                    lock (colorComposition.KeyBitmap)
                        regionLeftColor = BitmapUtils.GetRegionColor(colorComposition.KeyBitmap, regionLeft);

                    if (!_colorKbLeft.Equals(regionLeftColor))
                    {
                        _colorKbLeft = regionLeftColor;
                        _colorUpdated = true;
                    }

                    if (e.Cancel) return false;

                    // Center (Other Half of Spacebar to F11) - Clevo keyboards are very compact and the right side color bleeds over to the up/left/right/down keys)
                    BitmapRectangle keymapF11 = Effects.GetBitmappingFromDeviceKey(DeviceKeys.F11);

                    var f11XWidth = Convert.ToInt32(keymapF11.Center.X - spacebarX);

                    var regionCenter = new Rectangle(spacebarX, keymapEsc.Top, f11XWidth, height);

                    Color regionCenterColor;
                    lock (colorComposition.KeyBitmap)
                        regionCenterColor = BitmapUtils.GetRegionColor(colorComposition.KeyBitmap, regionCenter);

                    if (!_colorKbCenter.Equals(regionCenterColor))
                    {
                        _colorKbCenter = regionCenterColor;
                        _colorUpdated = true;
                    }

                    if (e.Cancel) return false;

                    // Right Side
                    BitmapRectangle keymapNumenter = Effects.GetBitmappingFromDeviceKey(DeviceKeys.NUM_ENTER);
                    Rectangle regionRight = new Rectangle(Convert.ToInt32(keymapF11.Center.X),
                        keymapEsc.Top, Convert.ToInt32(keymapNumenter.Center.X - keymapF11.Center.X), height);

                    Color regionRightColor;
                    lock (colorComposition.KeyBitmap)
                        regionRightColor = BitmapUtils.GetRegionColor(colorComposition.KeyBitmap, regionRight);

                    if (!_colorKbRight.Equals(regionRightColor))
                    {
                        _colorKbRight = regionRightColor;
                        _colorUpdated = true;
                    }
                }

                if (e.Cancel) return false;
                SendColorsToKeyboard(forced);
                updateResult = true;
            }
            catch (Exception exception)
            {
                Global.logger.Error("Clevo device, error when updating device. Error: " + exception);
                updateResult = false;
            }

            _watch.Stop();
            _lastUpdateTime = _watch.ElapsedMilliseconds;

            return updateResult;
        }

        private void SendColorsToKeyboard(bool forced = false)
        {
            if (!forced && !_colorUpdated) return;
            if ((forced || !_lastColorKbLeft.Equals(_colorKbLeft)) && !Global.Configuration.DevicesDisableKeyboard)
            {
                // MYSTERY: // Why is it B,R,G instead of R,G,B? SetKBLED uses R,G,B but only B,R,G returns the correct colors. Is bitshifting different in C# than in C++?
                _clevo.SetKBLED(ClevoSetKBLED.KBLEDAREA.ColorKBLeft, _colorKbLeft.B, _colorKbLeft.R, _colorKbLeft.G, _colorKbLeft.A / 0xff);
                _lastColorKbLeft = _colorKbLeft;
            }
            if ((forced || !_lastColorKbCenter.Equals(_colorKbCenter)) && !Global.Configuration.DevicesDisableKeyboard)
            {
                _clevo.SetKBLED(ClevoSetKBLED.KBLEDAREA.ColorKBCenter, _colorKbCenter.B, _colorKbCenter.R, _colorKbCenter.G, _colorKbCenter.A / 0xff);
                _lastColorKbCenter = _colorKbCenter;
            }
            if ((forced || !_lastColorKbRight.Equals(_colorKbRight)) && !Global.Configuration.DevicesDisableKeyboard)
            {
                _clevo.SetKBLED(ClevoSetKBLED.KBLEDAREA.ColorKBRight, _colorKbRight.B, _colorKbRight.R, _colorKbRight.G, _colorKbRight.A / 0xff);
                _lastColorKbRight = _colorKbRight;
            }
            if ((forced || (_useTouchpad && !_lastColorTouchpad.Equals(_colorTouchpad))) && !Global.Configuration.DevicesDisableMouse)
            {
                _clevo.SetKBLED(ClevoSetKBLED.KBLEDAREA.ColorTouchpad, _colorTouchpad.B, _colorTouchpad.R, _colorTouchpad.G, _colorTouchpad.A / 0xff);
                _lastColorTouchpad = _colorTouchpad;
            }
            _colorUpdated = false;
        }

        public string DeviceUpdatePerformance => (_isInitialized ? _lastUpdateTime + " ms" : "");

        public VariableRegistry RegisteredVariables => new();
    }
}
