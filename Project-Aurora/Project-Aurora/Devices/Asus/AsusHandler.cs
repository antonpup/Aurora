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
using Microsoft.Scripting.Utils;
using Microsoft.Win32;
using Mono.CSharp.Linq;
using Timer = System.Timers.Timer;
using static Aurora.Devices.Asus.AsusDevice;

namespace Aurora.Devices.Asus
{
    /// <summary>
    /// The main point of entry for the Asus SDK
    /// </summary>
    public class AsusHandler
    {
        private const string AuraSdkRegistryEntry = @"{05921124-5057-483E-A037-E9497B523590}\InprocServer32";
        /// <summary>
        /// How many times a device can fail before we give up and leave it
        /// </summary>
        private const int MaxDeviceFails = 3;
        private const int MaxDeviceFailsTime = 60; // in seconds
        private AuraDevelopement _developmentSdk;

        private readonly List<AsusGenericDeviceWrapper> _devices = new List<AsusGenericDeviceWrapper>();
        private readonly Dictionary<int, FailData> _deviceFails = new Dictionary<int, FailData>();
        private bool _initializing = false;

        private bool _stopDevices = false;
        private Action<bool> _onFinishStopDevices;
        
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

                    _deviceFails.Clear();
                    _devices.Clear();

                    // start the dev sdk
                    _developmentSdk = new AuraDevelopementClass();
                    _developmentSdk.AURARequireToken(0);
                    
                    // possible enum types that the Aura SDK supports
                    var possibleTypes = ((AsusDeviceType[])Enum.GetValues(typeof(AsusDeviceType))).Select(type => (uint)type).ToArray();
                    var allDevices = new List<IAuraDevice>(); 
                    foreach (IAuraDevice auraDevice in _developmentSdk.GetAllDevices())
                    {
                        allDevices.Add(auraDevice);
                    }

                    // enumerate all devices
                    foreach (IAuraDevice device in allDevices)
                    {
                        if (!possibleTypes.Contains(device.Type))
                        {
                            Log($"Found unknown device {device.Type}... Ignoring for now.");
                            continue;
                        }

                        Log($"Found device {device.Name} with type {(AsusDeviceType)device.Type} has {device.Lights.Count} key{(device.Lights.Count > 1 ? "s" : "")}");

                        AsusGenericDeviceWrapper asusDevice = CreateAsusDeviceWrapper(device, async);
                        if (asusDevice == null)
                            continue;

                        var deviceId = GetDeviceId(device, allDevices);
                        asusDevice.SetId(deviceId);
                        _deviceFails[deviceId] = new FailData();
                        Log($"That device has the ID of {deviceId}");
                        _devices.Add(asusDevice);
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

        private AsusGenericDeviceWrapper CreateAsusDeviceWrapper(IAuraDevice device, bool async)
        {
            AsusGenericDeviceWrapper asusDevice;
            switch ((AsusDeviceType)device.Type)
            {
                case AsusDeviceType.Keyboard:
                    if (Global.Configuration.devices_disable_keyboard)
                        return null;
                    asusDevice = new AsusKeyboardWrapper(device, async);
                    break;
                case AsusDeviceType.Mouse:
                    if (Global.Configuration.devices_disable_mouse)
                        return null;
                    asusDevice = new AsusMouseWrapper(device, async);
                    break;
                default:
                    asusDevice = new AsusGenericDeviceWrapper(device, async);
                    break;
            }

            return asusDevice;
        }

        private IAuraDevice GetNewInstanceOfDeviceById(int id)
        {
            var allDevices = new List<IAuraDevice>();
            foreach (IAuraDevice auraDevice in _developmentSdk.GetAllDevices())
            {
                allDevices.Add(auraDevice);
            }

            foreach (var device in allDevices)
            {
                var deviceId = GetDeviceId(device, allDevices);
                if (id == deviceId)
                    return device;
            }

            return null;
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

            var deviceNotActive = false;
            // update every devices
            foreach (var device in _devices)
            {
                device.SendColors(keyColors);
                // check the device's status
                if (device.DeviceStatus != AsusDeviceStatus.Active)
                    deviceNotActive = true;
            }

            if (deviceNotActive)
                HandleInactiveDevices();
        }

        /// <summary>
        /// We need to manage devices that are running slow or not found
        /// </summary>
        private void HandleInactiveDevices()
        {
            var deviceList = new List<AsusGenericDeviceWrapper>();
            // get all of the annoying devices
            foreach (var device in _devices)
            {
                if (device.DeviceStatus != AsusDeviceStatus.Active)
                    deviceList.Add(device);
            }

            var restartDeviceList = new List<AsusGenericDeviceWrapper>();
            // look into the fail data for each device
            foreach (var device in deviceList)
            {
                var failData = _deviceFails[device.DeviceId];
                failData.Fails++;
                if (ShouldRemoveDevice(failData))
                {
                    Log($"{device.DeviceName} {device.HardwareName} {device.DeviceId} has failed too many times, removing...");
                    _devices.Remove(device);
                }
                else
                {
                    // if the device already made MaxFails but not in the time span, reset the data
                    if (failData.Fails > MaxDeviceFails)
                    {
                        failData = new FailData();
                    }
                    // if we're stating a new record, set the time
                    if (failData.Fails == 0)
                    {
                        failData.EarliestFailTime = new DateTime();
                    }

                    _deviceFails[device.DeviceId] = failData;
                    restartDeviceList.Add(device);
                }
            }

            // try to get all of the device to restart and replace the older device
            foreach (var device in restartDeviceList)
            {
                Log($"Attempting to get device {device.DeviceName} {device.HardwareName} {device.DeviceId} again");
                var auraDevice = GetNewInstanceOfDeviceById(device.DeviceId);
                // if we couldn't find the device, then remove it from the main list
                if (auraDevice == null)
                {
                    _devices.Remove(device);
                    continue;
                }

                // try to replace the device
                _devices.Remove(device);
                var asusDevice = CreateAsusDeviceWrapper(auraDevice, device.Async);
                if (asusDevice == null)
                    continue;
                asusDevice.SetId(device.DeviceId);
                _devices.Add(asusDevice);
            }
        }

        /// <summary>
        /// Checks the fail data to see if we have failed a certain amount of times in a specified timespan
        /// </summary>
        private bool ShouldRemoveDevice(FailData failData)
        {
            return (failData.Fails >= MaxDeviceFails &&
                    DateTime.Now.Subtract(failData.EarliestFailTime).TotalSeconds > MaxDeviceFailsTime);
        }

        /// <summary>
        /// Generate a best attempt ID for an AuraDevice
        /// </summary>
        /// <param name="device">The device that you want the ID for</param>
        /// <param name="devices">A list of devices, to ensure uniqueness</param>
        /// <returns>Device ID</returns>
        private int GetDeviceId(IAuraDevice device, List<IAuraDevice> devices)
        {
            // this is a really bad algorithm, sorry
            var ids = devices.Where(d => d != device).Select(GetDeviceId);

            // generate the ID for this device
            var id = GetDeviceId(device);

            // in the case where two devices have the same ID
            // just append the index onto the ID
            if (ids.Contains(id))
            {
                id += devices.IndexOf(device);
            }

            return id;
        }

        /// <summary>
        /// Generate an id based on the device provided
        /// </summary>
        /// <param name="device">The device that you want the ID for</param>
        /// <returns>The device's ID</returns>
        private int GetDeviceId(IAuraDevice device)
        {
            return $"{(AsusDeviceType)device.Type}_{device.Manufacture}_{device.Model}_{device.Name}_{device.LightCount}".GetHashCode();
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
                status.Append($"{device.HardwareName} {device.ElapsedMillis}ms");
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

        /// <summary>
        /// A simple struct to record the amount of times an asus device has failed
        /// </summary>
        private struct FailData
        {
            public int Fails;
            public DateTime EarliestFailTime;
        }

        /// <summary>
        /// The status of the device
        /// </summary>
        enum AsusDeviceStatus
        {
            /// <summary>
            /// The normal status for the device
            /// </summary>
            Active,
            /// <summary>
            /// When the device is running slow, usually hardware related like the
            /// ROG Pugio mouse
            /// </summary>
            Slow,
            /// <summary>
            /// When the AuraSDK throws an System.AccessViolationException when
            /// the device tries to update the colours
            /// </summary>
            NotFound
        }

        #region Asus Device Wrappers

        /// <summary>
        /// In this wrapper we take the peripheral color and apply it on every
        /// LED that is detected by the SDK
        /// </summary>
        private class AsusGenericDeviceWrapper
        {
            private const int DefaultFrameRate = 30;
            private const int MaxSlowFails = 3;
            /// <summary>
            /// If a device takes more than this to print the colors, then try to disconnect it in ms
            /// </summary>
            private const int MaxDeviceWait = 1500;
            /// <summary>
            /// How many millis it took to run the last update
            /// </summary>
            public long ElapsedMillis { get; private set; }
            /// <summary>
            /// The current status of the device
            /// </summary>
            private AsusDeviceStatus _deviceStatus;
            /// <summary>
            /// The current status of the device
            /// </summary>
            public AsusDeviceStatus DeviceStatus
            {
                get => _deviceStatus;
                private set
                {
                    Log($"Device {DeviceName} {HardwareName} {DeviceId} switching status {_deviceStatus} -> {value}");
                    _deviceStatus = value;
                }
            }
            /// <summary>
            /// The current status of the device
            /// </summary>
            public int DeviceId { get; private set; }
            /// <summary>
            /// Render lights asynchronously?
            /// </summary>
            public readonly bool Async = false;
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
            private int _longUpdates = 0;
            /// <summary>
            /// Initialise a generic device wrapper
            /// </summary>
            /// <param name="device">The device to use</param>
            /// <param name="frameRate">The rate to update the device, frames per second</param>
            public AsusGenericDeviceWrapper(IAuraDevice device, bool async, int frameRate = DefaultFrameRate)
            {
                Device = device;
                Async = async;
                DeviceName = device.Name;
                HardwareName = ((AsusDeviceType)Device.Type).ToString();
                _frequency = (int)((1f / frameRate) * 1000);
                _stopwatch.Start();
            }

            public void SetId(int deviceId)
            {
                if (DeviceId != 0)
                    return;

                DeviceId = deviceId;
            }

            /// <summary>
            /// Start the process to send the colors across to the device
            /// </summary>
            /// <param name="keyColors">the colors to update it with</param>
            public void SendColors(Dictionary<DeviceKeys, Color> keyColors)
            {
                if (Device == null)
                {
                    DeviceStatus = AsusDeviceStatus.NotFound;
                    return;
                }

                // if we're still applying colors then dismiss this set of colors
                if (_applyColors || _stopwatch.ElapsedMilliseconds < _frequency)
                    return;

                if (ElapsedMillis > MaxDeviceWait)
                {
                    Log($"Device {DeviceName} {HardwareName} {DeviceId} took {_stopwatch.ElapsedMilliseconds}");
                    _longUpdates++;
                }
                else
                    _longUpdates = 0;

                if (_longUpdates >= MaxSlowFails)
                {
                    DeviceStatus = AsusDeviceStatus.Slow;
                    return;
                }

                _stopwatch.Reset();
                if (Async)
                {
                    // clone the dictionary so we don't interfere with the original reference
                    var colorCopy = new Dictionary<DeviceKeys, Color>(keyColors);
                    ThreadPool.QueueUserWorkItem(ApplyColorsThreaded, colorCopy);
                }
                else
                {
                    ApplyColorsThreaded(keyColors);
                }
            }

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
                        ApplyColors((Dictionary<DeviceKeys, Color>)keyColorsObject);
                    }
                }
                catch (AccessViolationException e)
                {
                    Log($"lost access to device {DeviceName}");
                    Global.logger.Info(e);
                    DeviceStatus = AsusDeviceStatus.NotFound;
                }
                catch (Exception e)
                {
                    Log($"{DeviceName} has an exception:\r\n{e}");
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
        private class AsusMouseWrapper : AsusGenericDeviceWrapper
        {
            public AsusMouseWrapper(IAuraDevice mouse, bool async) : base(mouse, async) { }

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
        private class AsusKeyboardWrapper : AsusGenericDeviceWrapper
        {
            private readonly IAuraKeyboard _keyboard;

            private readonly Dictionary<ushort, IAuraRgbKey> _idToKey
                = new Dictionary<ushort, IAuraRgbKey>();

            public AsusKeyboardWrapper(IAuraDevice keyboard, bool async) : base(keyboard, async)
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

    #endregion
}
