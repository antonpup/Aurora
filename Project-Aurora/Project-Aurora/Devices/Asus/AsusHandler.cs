using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using AuraServiceLib;
using Aurora.Profiles.DeadCells;
using Aurora.Settings;
using Microsoft.Win32;
using Timer = System.Timers.Timer;

namespace Aurora.Devices.Asus
{
    /// <summary>
    /// The main point of entry for the Asus SDK
    /// </summary>
    public class AsusHandler
    {
        private const string AuraSdkRegistryEntry = @"{05921124-5057-483E-A037-E9497B523590}\InprocServer32";
        private AuraDevelopement _developmentSdk;

        private readonly List<IAsusSdkDeviceWrapper> _devices = new List<IAsusSdkDeviceWrapper>();
        private readonly List<IAsusSdkDeviceWrapper> _removedDevices = new List<IAsusSdkDeviceWrapper>();
        private bool _initializing = false;

        private bool _stopDevices = false;
        private Action<bool> _onFinishStopDevices;

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
        /// <returns>True if succeeded or initializing</returns>
        public bool GetControl(bool async, Action<bool> onComplete)
        {
            if (_initializing)
                return true;
            Global.logger.Error("[ASUS] Initializing Asus Device");
            var registryClassKey = Registry.ClassesRoot.OpenSubKey("CLSID");
            if (registryClassKey != null)
            {
                var auraSdkRegistry = registryClassKey.OpenSubKey(AuraSdkRegistryEntry);
                if (auraSdkRegistry == null)
                {
                    Global.logger.Error("[ASUS] Aura SDK not found in registry.");
                    onComplete(false);
                    return false;
                }
            }

            // Do this async because it may take a while to initialise
            Task.Run(() =>
            {
                try
                {
                    _initializing = true;
                    // start the dev sdk
                    _developmentSdk = new AuraDevelopementClass();
                    _developmentSdk.AURARequireToken(0);
                    
                    // possible enum types that the Aura SDK supports
                    var possibleTypes = ((AsusDeviceType[])Enum.GetValues(typeof(AsusDeviceType))).Select(type => (uint)type).ToArray();
                    
                    // enumerate all devices
                    foreach (IAuraDevice device in _developmentSdk.GetAllDevices())
                    {
                        if (!possibleTypes.Contains(device.Type))
                        {
                            Global.logger.Info($"[ASUS] Found unknown device {device.Type}... Ignoring for now.");
                            continue;
                        }

                        Global.logger.Info($"[ASUS] Found device {device.Name} with type {(AsusDeviceType)device.Type} has {device.Lights.Count} key{(device.Lights.Count > 1 ? "s" : "")}");
                        switch ((AsusDeviceType)device.Type)
                        {
                            case AsusDeviceType.Keyboard:
                                if (Global.Configuration.devices_disable_keyboard)
                                    continue;
                                _devices.Add(new AuraSdkKeyboardWrapper(device, async));
                                break;
                            case AsusDeviceType.Mouse:
                                if (Global.Configuration.devices_disable_mouse)
                                    continue;
                                _devices.Add(new AuraSdkMouseWrapper(device, async));
                                break;
                            default:
                                _devices.Add(new AuraSdkGenericDeviceWrapper(device, async));
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
            return true;
        }

        /// <summary>
        /// Tell the aura sdk device to set the device back to it's previous state
        /// </summary>
        public void ReleaseControl(Action<bool> onComplete)
        {
            // Do this async because it may take a while to uninitialize
            _onFinishStopDevices = onComplete;
            _stopDevices = true;
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
            if (_devices == null || _devices.Count == 0)
                return;
            
            // if the signal to stop has been given, proceed here
            if (_stopDevices)
            {
                _stopDevices = false;
                _devices.Clear();
                _onFinishStopDevices?.Invoke(true);
                return;
            }

            // update every devices, if one fails, mark it
            foreach (var device in _devices)
            {
                if (!device.SendColors(keyColors))
                {
                    _removedDevices.Add(device);
                }
            }

            if (_removedDevices.Count >= 0) return;
            foreach (var device in _removedDevices)
            {
                if (_devices.Contains(device))
                {
                    Global.logger.Info(
                        $"[ASUS] Removed {device.GetDeviceName()} {device.GetHardwareName()} for acting up.");
                    _devices.Remove(device);
                }
            }
           _removedDevices.Clear();
        }

        /// <summary>
        /// Returns a string containing the elapsed time for each device
        /// </summary>
        /// <returns> Returns a string containing the elapsed time for each device</returns>
        public string GetDeviceStatus()
        {
            var status = new StringBuilder();
            for (var i = 0; i < _devices.Count; i++)
            {
                var device = _devices[i];
                status.Append($"{device.GetHardwareName()} {device.GetElapsedMillis()}ms");
                if (i != _devices.Count - 1)
                    status.Append(", ");
            }

            return status.ToString();
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
        
        private interface IAsusSdkDeviceWrapper
        {
            bool SendColors(Dictionary<DeviceKeys, Color> keyColors);
            string GetDeviceName();
            string GetHardwareName();
            long GetElapsedMillis();
        }

        /// <summary>
        /// In this wrapper we take the peripheral color and apply it on every
        /// LED that is detected by the SDK
        /// </summary>
        private class AuraSdkGenericDeviceWrapper : IAsusSdkDeviceWrapper
        {
            private const int DefaultFrameRate = 30;
            private const int MaxFails = 5;
            /// <summary>
            ///  if a device takes more than this to print the colors, then try to disconnect it
            /// </summary>
            private const int MaxDeviceWait = 2000;
            /// <summary>
            /// How many millis it took to run the last update
            /// </summary>
            public long ElapsedMillis { get; private set; }
            /// <summary>
            /// The device to update
            /// </summary>
            protected readonly IAuraDevice Device;
            /// <summary>
            /// The device's name
            /// </summary>
            public readonly string DeviceName;
            /// <summary>
            /// The device's hardware name
            /// </summary>
            public readonly string HardwareName;
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
            /// How many updates failed in a row
            /// </summary>
            private int _failedUpdates = 0;
            /// <summary>
            /// How many updates failed in a row
            /// </summary>
            private int _longUpdates = 0;
            /// <summary>
            /// Render lights asynchronously?
            /// </summary>
            private readonly bool _async = false;
            /// <summary>
            /// Initialise a generic device wrapper
            /// </summary>
            /// <param name="device">The device to use</param>
            /// <param name="frameRate">The rate to update the device, frames per second</param>
            public AuraSdkGenericDeviceWrapper(IAuraDevice device, bool async, int frameRate = DefaultFrameRate)
            {
                Device = device;
                _async = async;
                DeviceName = device.Name;
                HardwareName = ((AsusDeviceType)Device.Type).ToString();
                _frequency = (int)((1f / frameRate) * 1000);
                _stopwatch.Start();
            }

            /// <summary>
            /// Start a thread to send the colors across to the device
            /// </summary>
            /// <param name="keyColors"></param>
            /// <returns> false if the device has been removed </returns>
            public bool SendColors(Dictionary<DeviceKeys, Color> keyColors)
            {
                if (Device == null || _failedUpdates > MaxFails)
                    return false;

                // if we're still applying colors then dismiss this set of colors
                if (_applyColors || _stopwatch.ElapsedMilliseconds < _frequency)
                    return true;

                if (_stopwatch.ElapsedMilliseconds > MaxDeviceWait)
                    _longUpdates++;
                else
                    _longUpdates = 0;

                if (_longUpdates > MaxFails)
                    return false;

                _stopwatch.Reset();
                if (_async)
                {
                    // clone the dictionary so we don't interfere with the original reference
                    var colorCopy = new Dictionary<DeviceKeys, Color>(keyColors);
                    ThreadPool.QueueUserWorkItem(ApplyColorsThreaded, colorCopy);
                }
                else
                {
                    ApplyColorsThreaded(keyColors);
                }
                return true;
            }

            /// <inheritdoc />
            public string GetDeviceName() => DeviceName;
            /// <inheritdoc />
            public string GetHardwareName() => HardwareName;
            /// <inheritdoc />
            public long GetElapsedMillis() => ElapsedMillis;

            [HandleProcessCorruptedStateExceptions, SecurityCritical]
            private void ApplyColorsThreaded(object keyColorsObject)
            {
                _stopwatch.Start();
                _applyColors = true;

                try
                {
                    lock (Device)
                    {
                        Device.SetMode(0);
                        ApplyColors((Dictionary<DeviceKeys, Color>) keyColorsObject);
                        _failedUpdates = 0;
                    }
                }
                catch (Exception e)
                {
                    _failedUpdates++;
                    Global.logger.Info(e);
                    Global.logger.Info($"[ASUS] failed to update device {DeviceName} {_failedUpdates} times :(");
                }

                _applyColors = false;
                ElapsedMillis = _stopwatch.ElapsedMilliseconds;
            }

            /// <summary>
            /// Apply Colors to the device
            /// </summary>
            /// <param name="keyColors"></param>
            [HandleProcessCorruptedStateExceptions, SecurityCritical]
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
                lock (rgbKey)
                {
                    rgbKey.Red = color.R;
                    rgbKey.Green = color.G;
                    rgbKey.Blue = color.B;
                }
            }

            protected void SetRgbLight(IAuraRgbLight rgbLight, Color color)
            {
                lock (rgbLight)
                {
                    rgbLight.Red = color.R;
                    rgbLight.Green = color.G;
                    rgbLight.Blue = color.B;
                }
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
        private class AuraSdkMouseWrapper : AuraSdkGenericDeviceWrapper
        {
            public AuraSdkMouseWrapper(IAuraDevice mouse, bool async) : base(mouse, async) { }

            /// <inheritdoc />
            [HandleProcessCorruptedStateExceptions, SecurityCritical]
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
        private class AuraSdkKeyboardWrapper : AuraSdkGenericDeviceWrapper
        {
            private readonly IAuraKeyboard _keyboard;

            private readonly Dictionary<ushort, IAuraRgbKey> _idToKey
                = new Dictionary<ushort, IAuraRgbKey>();

            public AuraSdkKeyboardWrapper(IAuraDevice keyboard, bool async) : base(keyboard, async)
            {
                _keyboard = (IAuraKeyboard)keyboard;

                foreach (IAuraRgbKey key in _keyboard.Keys)
                    _idToKey[key.Code] = key;
            }

            /// <inheritdoc />
            [HandleProcessCorruptedStateExceptions, SecurityCritical]
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
