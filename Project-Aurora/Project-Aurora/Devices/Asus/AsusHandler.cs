using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AuraServiceLib;
using Aurora.Profiles.DeadCells;
using Timer = System.Timers.Timer;

namespace Aurora.Devices.Asus
{
    /// <summary>
    /// The main point of entry for the Asus SDK
    /// </summary>
    public class AsusHandler
    {
        private const int MaxDeviceNameLength = 5;

        private bool _initializing = false;
        private IAuraSdk2 _sdk;

        private readonly List<AsusSdkGenericDeviceWrapper> _devices = new List<AsusSdkGenericDeviceWrapper>();

        #region Windows Handlers
        [DllImport("user32.dll")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);

        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_CLOSE = 0xF060;
   
        #endregion

        /// <summary>
        /// Get control of all aura devices
        /// </summary>
        /// <returns>True if succeeded</returns>
        public void GetControl(Action<bool> onComplete)
        {
            if (_initializing)
                return;

            // Do this async because it may take a while to initialise
            Task.Run(() =>
            {
                try
                {
                    _initializing = true;
                    _sdk = (IAuraSdk2)new AuraSdk();
                    // run this async to close any pop up windows that this brings up
                    Task.Run(() => ClosePopUpWindows());
                    // get the devices (this can be slow
                    _sdk.SwitchMode();

                    // possible enum types that the Aura SDK supports
                    var possibleTypes = ((AsusDeviceType[])Enum.GetValues(typeof(AsusDeviceType))).Select(type => (uint)type).ToArray();
                    // enumerate all devices
                    IAuraSyncDeviceCollection devices = _sdk.Enumerate(0);
                    foreach (IAuraSyncDevice device in devices)
                    {
                        if (!possibleTypes.Contains(device.Type))
                        {
                            Global.logger.Info($"[ASUS] found unknown device {device.Type}... Ignoring for now.");
                            continue;
                        }

                        Global.logger.Info($"[ASUS] found device {device.Name} with type {(AsusDeviceType)device.Type}");
                        switch ((AsusDeviceType)device.Type)
                        {
                            case AsusDeviceType.Keyboard:
                                _devices.Add(new AuraSdkKeyboardWrapper(device));
                                break;
                            case AsusDeviceType.Mouse:
                                _devices.Add(new AuraSdkMouseWrapper(device));
                                break;
                            default:
                                _devices.Add(new AsusSdkGenericDeviceWrapper(device));
                                break;
                        }
                    }

                    onComplete?.Invoke(true);
                    _initializing = false;
                }
                catch
                {
                    onComplete?.Invoke(false);
                }
            });
        }

        /// <summary>
        /// Tell the aura sdk device to set the device back to it's previous state
        /// </summary>
        public void ReleaseControl(Action<bool> onComplete)
        {
            // Do this async because it may take a while to uninitialize
            Task.Run(() =>
            {
                try
                {
                    _sdk.ReleaseControl(0);
                    onComplete(true);
                }
                catch
                {
                    onComplete(true);
                }
            });
        }

        /// <summary>
        /// Sometimes the Aura SDK picks up devices that do not have a RGB and spits out
        /// an error popup. This is suppress these annoying notifications
        /// </summary>
        private void ClosePopUpWindows()
        {
            while (_initializing)
            {
                var window = FindWindow(null, "Message");
                SendMessage(window, WM_SYSCOMMAND, SC_CLOSE, 0);
            }
        }

        /// <summary>
        /// Update all of the connected devices
        /// </summary>
        /// <param name="keyColors">The colors to update the devices with</param>
        public void UpdateDevices(Dictionary<DeviceKeys, Color> keyColors)
        {
            foreach (var device in _devices)
            {
                device.SendColors(keyColors);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// Returns a string containing the elapsed time for each device</returns>
        public string GetDeviceStatus()
        {
            return _devices.Count > 0 
                ? _devices
                    .Select(device => $"{device.GetName()}[{device.GetHardwareType()}] {device.ElapsedMillis}ms")
                    .Aggregate((x, y) => $"{x}, {y}")
                : "";
        }

        /// <summary>
        /// Devices specified in the AsusSDK documentation
        /// </summary>
        private enum AsusDeviceType : uint
        {
            All = 0x00000000,
            Motherboard = 0x00010000,
            MotherboardLedStrip = 0x00011000,
            AllInOnePc = 0x00012000,
            Vga = 0x00020000,
            Display = 0x00030000,
            Headset = 0x00040000,
            Microphone = 0x00050000,
            ExternalHdd = 0x00060000,
            ExternalBdDrive = 0x00061000,
            Dram = 0x00070000,
            Keyboard = 0x00080000,
            NotebookKeyboard = 0x00081000,
            NotebookKeyboard4ZoneType = 0x00081001,
            Mouse = 0x00090000,
            Chassis = 0x000B0000,
            Projector = 0x000C0000,
        }

        /// <summary>
        /// In this wrapper we take the peripheral color and apply it on every
        /// LED that is detected by the SDK
        /// </summary>
        private class AsusSdkGenericDeviceWrapper
        {
            private const int DefaultFrameRate = 60;

            /// <summary>
            /// How many millis it took to run the last update
            /// </summary>
            public long ElapsedMillis { get; private set; }
            /// <summary>
            /// The device to update
            /// </summary>
            protected readonly IAuraSyncDevice Device;
            /// <summary>
            /// How many millis to count before updating the colors again
            /// </summary>
            private readonly int _frequency; // in millis
            /// <summary>
            /// Use to restrict update rate and for diagnostics
            /// </summary>
            private readonly Stopwatch _stopwatch = new Stopwatch();
            /// <summary>
            /// Acts a mutex lock for async threads
            /// </summary>
            private bool _applyColors = false;

            /// <summary>
            /// Initialise a generic device wrapper
            /// </summary>
            /// <param name="device">The device to use</param>
            /// <param name="frameRate">The rate to update the device, frames per second</param>
            public AsusSdkGenericDeviceWrapper(IAuraSyncDevice device, int frameRate = DefaultFrameRate)
            {
                Device = device;
                _frequency = (int)((1f / frameRate) * 1000);
                _stopwatch.Start();
            }

            /// <summary>
            /// Start a thread to send the colors across to the device
            /// </summary>
            /// <param name="keyColors"></param>
            public void SendColors(Dictionary<DeviceKeys, Color> keyColors)
            {
                // if we're still applying colors then dismiss this set of colors
                if (_applyColors || _stopwatch.ElapsedMilliseconds < _frequency)
                    return;

                ElapsedMillis = _stopwatch.ElapsedMilliseconds;

                _stopwatch.Restart();
                // clone the dictionary so we don't interfere with the original reference
                var colorCopy = new Dictionary<DeviceKeys, Color>(keyColors);
                ThreadPool.QueueUserWorkItem(ApplyColorsThreaded, colorCopy);
            }

            /// <summary>
            /// </summary>
            /// <returns>The hardware assigned name for the device</returns>
            public string GetName()
            {
                return Device.Name;
            }

            /// <summary>
            /// </summary>
            /// <returns>The hardware type</returns>
            public string GetHardwareType()
            {
                return ((AsusDeviceType) Device.Type).ToString();
            }

            private void ApplyColorsThreaded(object keyColorsObject)
            {
                _applyColors = true;
                ApplyColors((Dictionary<DeviceKeys, Color>) keyColorsObject);
                _applyColors = false;
            }

            /// <summary>
            /// Apply Colors to the device
            /// </summary>
            /// <param name="keyColors"></param>
            protected virtual void ApplyColors(Dictionary<DeviceKeys, Color> keyColors)
            {
                // by default set every key to Peripheral_Logo
                if (keyColors.TryGetValue(DeviceKeys.Peripheral_Logo, out var color))
                {
                    foreach (IAuraRgbLight light in Device.Lights)
                    {
                        SetRgbLight(light, color);
                    }
                }

                Device.Apply();
            }

            /// <summary>
            /// Sets an Aura RGB Light 
            /// </summary>
            /// <param name="rgbKey">The light to set</param>
            /// <param name="color">Color to set with</param>
            protected void SetRgbLight(IAuraRgbKey rgbKey, Color color)
            {
                rgbKey.Red = color.R;
                rgbKey.Green = color.G;
                rgbKey.Blue = color.B;
            }

            protected void SetRgbLight(IAuraRgbLight rgbLight, Color color)
            {
                rgbLight.Red = color.R;
                rgbLight.Green = color.G;
                rgbLight.Blue = color.B;
            }

            protected void SetRgbLightIfExist(Dictionary<DeviceKeys, Color> keyColors, DeviceKeys key, IAuraRgbKey rgbLight)
            {
                if (keyColors.TryGetValue(key, out var color))
                {
                    SetRgbLight(rgbLight, color);
                }
            }

            protected void SetRgbLightIfExist(Dictionary<DeviceKeys, Color> keyColors, DeviceKeys key, IAuraRgbLight rgbLight)
            {
                if (keyColors.TryGetValue(key, out var color))
                {
                    SetRgbLight(rgbLight, color);
                }
            }
        }

        /// <summary>
        /// A wrapper for Asus Mice
        /// </summary>
        private class AuraSdkMouseWrapper : AsusSdkGenericDeviceWrapper
        {
            public AuraSdkMouseWrapper(IAuraSyncDevice mouse) : base(mouse) { }

            /// <inheritdoc />
            protected override void ApplyColors(Dictionary<DeviceKeys, Color> keyColors)
            {
                // access keys directly since we know what we want
                SetRgbLightIfExist(keyColors, DeviceKeys.Peripheral_Logo, Device.Lights[DeviceKeyToAuraMouseKeyId(DeviceKeys.Peripheral_Logo)]);
                SetRgbLightIfExist(keyColors, DeviceKeys.Peripheral_ScrollWheel, Device.Lights[DeviceKeyToAuraMouseKeyId(DeviceKeys.Peripheral_ScrollWheel)]);
                SetRgbLightIfExist(keyColors, DeviceKeys.Peripheral_FrontLight, Device.Lights[DeviceKeyToAuraMouseKeyId(DeviceKeys.Peripheral_FrontLight)]);

                Device.Apply();
            }

            /// <summary>
            /// Determines the ushort ID from a DeviceKeys
            /// </summary>
            /// <param name="key">The key to translate</param>
            /// <returns>the index of that mouse LED</returns>
            private static int DeviceKeyToAuraMouseKeyId(DeviceKeys key)
            {
                switch (key)
                {
                    case DeviceKeys.Peripheral_Logo:
                        return 0;
                    case DeviceKeys.Peripheral_ScrollWheel:
                        return 1;
                    case DeviceKeys.Peripheral_FrontLight:
                        return 2;
                    default:
                        return ushort.MaxValue;
                }
            }
        }

        /// <summary>
        /// A wrapper for Asus Keyboards
        /// </summary>
        private class AuraSdkKeyboardWrapper : AsusSdkGenericDeviceWrapper
        {
            private readonly IAuraSyncKeyboard _keyboard;

            private readonly Dictionary<ushort, IAuraRgbKey> _idToKey
                = new Dictionary<ushort, IAuraRgbKey>();

            public AuraSdkKeyboardWrapper(IAuraSyncDevice keyboard) : base(keyboard)
            {
                _keyboard = (IAuraSyncKeyboard) keyboard;

                foreach (IAuraRgbKey key in _keyboard.Keys)
                    _idToKey[key.Code] = key;
            }

            /// <inheritdoc />
            protected override void ApplyColors(Dictionary<DeviceKeys, Color> keyColors)
            {
                foreach (var keyColor in keyColors)
                {
                    var deviceKey = keyColor.Key;
                    var color = keyColor.Value;

                    var keyId = DeviceKeyToAuraKeyboardKeyId(deviceKey);

                    // if key is invalid
                    if (keyId == ushort.MaxValue || !_idToKey.ContainsKey(keyId))
                        continue;

                    var key = _idToKey[keyId];
                    SetRgbLight(key, color);
                }
                _keyboard.Apply();
            }
            
            /// <summary>
            /// Determines the ushort ID from a DeviceKeys
            /// </summary>
            /// <param name="key">The key to translate</param>
            /// <returns>the ushort id, or ushort.MaxValue if invalid</returns>
            private static ushort DeviceKeyToAuraKeyboardKeyId(DeviceKeys key)
            {
                switch (key)
                {
                    case DeviceKeys.ESC:
                        return 1;
                    case DeviceKeys.F1:
                        return 59;
                    case DeviceKeys.F2:
                        return 60;
                    case DeviceKeys.F3:
                        return 61;
                    case DeviceKeys.F4:
                        return 62;
                    case DeviceKeys.F5:
                        return 63;
                    case DeviceKeys.F6:
                        return 64;
                    case DeviceKeys.F7:
                        return 65;
                    case DeviceKeys.F8:
                        return 66;
                    case DeviceKeys.F9:
                        return 67;
                    case DeviceKeys.F10:
                        return 68;
                    case DeviceKeys.F11:
                        return 87;
                    case DeviceKeys.F12:
                        return 88;
                    case DeviceKeys.PRINT_SCREEN:
                        return 183;
                    case DeviceKeys.SCROLL_LOCK:
                        return 70;
                    case DeviceKeys.PAUSE_BREAK:
                        return 197;
                    case DeviceKeys.OEM5:
                        return 6;
                    case DeviceKeys.TILDE:
                        return 41;
                    case DeviceKeys.ONE:
                        return 2;
                    case DeviceKeys.TWO:
                        return 3;
                    case DeviceKeys.THREE:
                        return 4;
                    case DeviceKeys.FOUR:
                        return 5;
                    case DeviceKeys.FIVE:
                        return 6;
                    case DeviceKeys.SIX:
                        return 7;
                    case DeviceKeys.SEVEN:
                        return 8;
                    case DeviceKeys.EIGHT:
                        return 9;
                    case DeviceKeys.NINE:
                        return 10;
                    case DeviceKeys.ZERO:
                        return 11;
                    case DeviceKeys.MINUS:
                        return 12;
                    case DeviceKeys.EQUALS:
                        return 13;
                    case DeviceKeys.OEM6:
                        return 7;
                    case DeviceKeys.BACKSPACE:
                        return 14;
                    case DeviceKeys.INSERT:
                        return 210;
                    case DeviceKeys.HOME:
                        return 199;
                    case DeviceKeys.PAGE_UP:
                        return 201;
                    case DeviceKeys.NUM_LOCK:
                        return 69;
                    case DeviceKeys.NUM_SLASH:
                        return 181;
                    case DeviceKeys.NUM_ASTERISK:
                        return 55;
                    case DeviceKeys.NUM_MINUS:
                        return 74;
                    case DeviceKeys.TAB:
                        return 15;
                    case DeviceKeys.Q:
                        return 16;
                    case DeviceKeys.W:
                        return 17;
                    case DeviceKeys.E:
                        return 18;
                    case DeviceKeys.R:
                        return 19;
                    case DeviceKeys.T:
                        return 20;
                    case DeviceKeys.Y:
                        return 21;
                    case DeviceKeys.U:
                        return 22;
                    case DeviceKeys.I:
                        return 23;
                    case DeviceKeys.O:
                        return 24;
                    case DeviceKeys.P:
                        return 25;
                    case DeviceKeys.OEM1:
                        return 2;
                    case DeviceKeys.OPEN_BRACKET:
                        return 26;
                    case DeviceKeys.OEMPlus:
                        return 13;
                    case DeviceKeys.CLOSE_BRACKET:
                        return 27;
                    case DeviceKeys.BACKSLASH:
                        return 43;
                    case DeviceKeys.DELETE:
                        return 211;
                    case DeviceKeys.END:
                        return 207;
                    case DeviceKeys.PAGE_DOWN:
                        return 209;
                    case DeviceKeys.NUM_SEVEN:
                        return 71;
                    case DeviceKeys.NUM_EIGHT:
                        return 72;
                    case DeviceKeys.NUM_NINE:
                        return 73;
                    case DeviceKeys.NUM_PLUS:
                        return 78;
                    case DeviceKeys.CAPS_LOCK:
                        return 58;
                    case DeviceKeys.A:
                        return 30;
                    case DeviceKeys.S:
                        return 31;
                    case DeviceKeys.D:
                        return 32;
                    case DeviceKeys.F:
                        return 33;
                    case DeviceKeys.G:
                        return 34;
                    case DeviceKeys.H:
                        return 35;
                    case DeviceKeys.J:
                        return 36;
                    case DeviceKeys.K:
                        return 37;
                    case DeviceKeys.L:
                        return 38;
                    case DeviceKeys.OEMTilde:
                        return 41;
                    case DeviceKeys.SEMICOLON:
                        return 39;
                    case DeviceKeys.APOSTROPHE:
                        return 40;
                    case DeviceKeys.HASHTAG:
                        return 3;
                    case DeviceKeys.ENTER:
                        return 28;
                    case DeviceKeys.NUM_FOUR:
                        return 75;
                    case DeviceKeys.NUM_FIVE:
                        return 76;
                    case DeviceKeys.NUM_SIX:
                        return 77;
                    case DeviceKeys.LEFT_SHIFT:
                        return 42;
                    case DeviceKeys.BACKSLASH_UK:
                        return 43;
                    case DeviceKeys.Z:
                        return 44;
                    case DeviceKeys.X:
                        return 45;
                    case DeviceKeys.C:
                        return 46;
                    case DeviceKeys.V:
                        return 47;
                    case DeviceKeys.B:
                        return 48;
                    case DeviceKeys.N:
                        return 49;
                    case DeviceKeys.M:
                        return 50;
                    case DeviceKeys.COMMA:
                        return 51;
                    case DeviceKeys.PERIOD:
                        return 52;
                    case DeviceKeys.FORWARD_SLASH:
                        return 53;
                    case DeviceKeys.OEM8:
                        return 9;
                    case DeviceKeys.RIGHT_SHIFT:
                        return 54;
                    case DeviceKeys.ARROW_UP:
                        return 200;
                    case DeviceKeys.NUM_ONE:
                        return 79;
                    case DeviceKeys.NUM_TWO:
                        return 80;
                    case DeviceKeys.NUM_THREE:
                        return 81;
                    case DeviceKeys.NUM_ENTER:
                        return 156;
                    case DeviceKeys.LEFT_CONTROL:
                        return 29;
                    case DeviceKeys.LEFT_WINDOWS:
                        return 219;
                    case DeviceKeys.LEFT_ALT:
                        return 56;
                    case DeviceKeys.SPACE:
                        return 57;
                    case DeviceKeys.RIGHT_ALT:
                        return 184;
                    case DeviceKeys.APPLICATION_SELECT:
                        return 221;
                    case DeviceKeys.RIGHT_CONTROL:
                        return 157;
                    case DeviceKeys.ARROW_LEFT:
                        return 203;
                    case DeviceKeys.ARROW_DOWN:
                        return 208;
                    case DeviceKeys.ARROW_RIGHT:
                        return 205;
                    case DeviceKeys.NUM_ZERO:
                        return 82;
                    case DeviceKeys.NUM_PERIOD:
                        return 83;
                    case DeviceKeys.FN_Key:
                        return 256;
                    case DeviceKeys.LOGO:
                        return 257;
                    case DeviceKeys.ADDITIONALLIGHT1:
                        // LEFT OF STRIX FLARE KEYBOARD
                        return 258;
                    case DeviceKeys.ADDITIONALLIGHT2:
                        //RIGHT OF STRIX FLARE KEYBOARD
                        return 259;
                    default:
                        return ushort.MaxValue;
                }
            }

        }
    }
}
