using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using AuraServiceLib;
using Aurora.Devices.Asus.Config;
using Microsoft.Win32;

namespace Aurora.Devices.Asus
{
    public class AsusHandler
    {
        public IAuraSdk2 AuraSdk { get; }
        private readonly List<AuraSyncDevice> _devices = new();
        private readonly object _deviceLock = new();
        private const string RecommendedAsusVersion = "1.07.71";
        private const string AsusAuraPath = @"Software\Asus\AURA";

        /// <summary>
        /// The number of registered devices
        /// </summary>
        public int DeviceCount => _devices.Count;

        /// <summary>
        /// Does this user have the Aura SDK installed?
        /// </summary>
        public bool HasSdk => AuraSdk != null;

        public AsusHandler(bool enableUnsupportedVersion = false, bool forceInitialize = false)
        {
            var message = "";
            try
            {
                if (CheckVersion(enableUnsupportedVersion, forceInitialize, out message))
                    AuraSdk = new AuraSdk() as IAuraSdk2;
                else
                    AuraSdk = null;

                Log(message);
            }
            catch (Exception e)
            {
                Log(message);
                Log("There was an error initializing the AuraSDK: " + e);
                AuraSdk = null;
            }
        }

        /// <summary>
        /// Checks to see if the version of Aura installed is the correct one
        /// </summary>
        /// <returns>true if the registry entry equals to <see cref="RecommendedAsusVersion"/></returns>
        private bool CheckVersion(bool enableUnsupportedVersion, bool forceInitialize, out string message)
        {
            message = null;

            //Computer\HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Asus\AURA\Version
            using (var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                using (var key = root.OpenSubKey(AsusAuraPath, false))
                {
                    if (key != null)
                    {
                        var registeredOwner = key.GetValue("Version");
                        if (registeredOwner is string str)
                        {
                            if (str == RecommendedAsusVersion)
                            {
                                message = $"Found correct version of Asus Aura SDK v{RecommendedAsusVersion}";
                                return true;
                            }

                            if (enableUnsupportedVersion)
                            {
                                message =
                                    $"Found version of Asus Aura SDK v{str}, which is not supported, if you have issues uninstall and reinstall to v{RecommendedAsusVersion}";
                                return true;
                            }

                            message =
                                $"Found version of Asus Aura SDK v{str}, which is not supported, either uninstall and reinstall to v{RecommendedAsusVersion} or enable Unsupported Asus SDK Version in 'View Options'";
                            return false;
                        }
                    }
                }
            }

            if (forceInitialize)
            {
                message = $"Could not find Asus Aura SDK, forcefully initializing";
                return true;
            }

            message = $"Could not find Asus Aura SDK, please install version v{RecommendedAsusVersion}";
            return false;
        }

        public bool Start()
        {
            if (AuraSdk == null)
                return false;

            lock (_deviceLock)
            {
                try
                {
                    AuraSdk.SwitchMode();

                    var config = AsusConfig.LoadConfig();

                    var allDevices = AuraSdk.Enumerate((uint) AsusDeviceType.All);
                    foreach (IAuraSyncDevice device in allDevices)
                    {
                        AsusDeviceType deviceType;
                        if (Enum.IsDefined(typeof(AsusDeviceType), device.Type))
                        {
                            deviceType = (AsusDeviceType) device.Type;
                        }
                        else
                        {
                            deviceType = AsusDeviceType.Unknown;
                            Log($"Could not read device type {device.Type} marking as Unknown");
                        }

                        Log($"Added device {device.Name} of type {deviceType} it has {device.Lights.Count} lights");

                        var configIndex = config.Devices.IndexOf(new AsusConfig.AsusConfigDevice(device));

                        if (configIndex >= 0)
                        {
                            var configDevice = config.Devices[configIndex];
                            if (configDevice.Enabled)
                            {
                                _devices.Add(new AsusSyncConfiguredDevice(this, device, config.Devices[configIndex]));
                                continue;
                            }
                        }

                        // Claymore Keyboard is not a IAuraSyncKeyboard, so a custom class has been made for it
                        if (device.Name == "Armoury" && deviceType == AsusDeviceType.Keyboard)
                        {
                            _devices.Add(new AsusSyncClaymoreDevice(this, device));
                            continue;
                        }

                        switch (deviceType)
                        {
                            case AsusDeviceType.Keyboard:
                            case AsusDeviceType.NotebookKeyboard:
                            case AsusDeviceType.NotebookKeyboard4ZoneType:
                                try
                                {
                                    _devices.Add(new AuraSyncKeyboardDevice(this, (IAuraSyncKeyboard) device));
                                }
                                catch (Exception e)
                                {
                                    Log(
                                        $"Something went wrong with reading your device as a keyboard {device} using as generic aura sync device\r\n{e}");
                                    _devices.Add(new AuraSyncDevice(this, device));
                                }

                                break;
                            // ignore whatever this is
                            case AsusDeviceType.All:
                            // ignore terminal for now, there are 270 lights :0
                            case AsusDeviceType.Terminal:
                                break;
                            default:
                                _devices.Add(new AuraSyncDevice(this, device));
                                break;
                        }
                    }

                    foreach (AuraSyncDevice device in _devices)
                        device.Start();
                }
                catch (Exception e)
                {
                    Log($"ERROR: Are you using \"Lighting_Control_1.07.71\"? \r\n{e}");
                    return false;
                }
            }

            return true;
        }

        public void Stop()
        {
            lock (_deviceLock)
            {
                for (var i = _devices.Count - 1; i >= 0; i--)
                {
                    var device = _devices[i];
                    device.Stop(false);
                    device.Dispose();
                }

                _devices.Clear();
                AuraSdk?.ReleaseControl(0);
            }
        }

        public void UpdateColors(Dictionary<DeviceKeys, Color> colors)
        {
            lock (_deviceLock)
            {
                foreach (var device in _devices)
                    device.UpdateColors(colors);
            }
        }

        public string GetDevicePerformance()
        {
            StringBuilder stringBuilder = new StringBuilder();

            lock (_deviceLock)
            {
                for (var i = 0; i < _devices.Count; i++)
                {
                    var device = _devices[i];
                    if (i != 0)
                        stringBuilder.Append(", ");

                    stringBuilder.Append(device.Name).Append(" ").Append(device.LastUpdateMillis).Append("ms");
                }
            }

            return stringBuilder.ToString();
        }

        public bool KeyboardActive()
        {
            lock (_deviceLock)
            {
                foreach (var device in _devices)
                {
                    if (device.DeviceType == AsusDeviceType.Keyboard && device.Active)
                        return true;
                }
            }

            return false;
        }

        public bool MouseActive()
        {
            lock (_deviceLock)
            {
                foreach (var device in _devices)
                {
                    if (device.DeviceType == AsusDeviceType.Mouse && device.Active)
                        return true;
                }
            }

            return false;
        }

        public void DisconnectDevice(AuraSyncDevice device)
        {
            lock (_deviceLock)
            {
                Log($"Device {device.Name} was disconnected");
                device.Stop();
                _devices.Remove(device);
            }
        }

        public static void Log(string text)
        {
            Global.logger.Info($"[ASUS] {text}");
        }

        /// <summary>
        /// Devices specified in the AsusSDK documentation
        /// </summary>
        public enum AsusDeviceType : uint
        {
            All = 0x00000000,
            Motherboard = 0x00010000,
            MotherboardLedStrip = 0x00011000,
            AllInOnePc = 0x00012000,
            Vga = 131072,
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
            Terminal = 0x000E0000,
            Unknown = 0xFFFFFFFF,
        }
    }
}