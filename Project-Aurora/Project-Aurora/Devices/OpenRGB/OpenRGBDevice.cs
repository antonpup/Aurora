using Aurora.Settings;
using OpenRGB.NET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Aurora.Modules.ProcessMonitor;
using Microsoft.Scripting.Utils;
using OpenRGB.NET.Models;
using Color = System.Drawing.Color;
using DK = Aurora.Devices.DeviceKeys;
using OpenRGBColor = OpenRGB.NET.Models.Color;
using OpenRGBDevice = OpenRGB.NET.Models.Device;
using OpenRGBDeviceType = OpenRGB.NET.Enums.DeviceType;
using OpenRGBZoneType = OpenRGB.NET.Enums.ZoneType;

namespace Aurora.Devices.OpenRGB
{
    public class OpenRgbAuroraDevice : DefaultDevice
    {
        public override string DeviceName => "OpenRGB";
        protected override string DeviceInfo => string.Join(", ", _devices.Select(d => d.OrgbDevice.Name));

        private OpenRGBClient _openRgb;
        private List<HelperOpenRgbDevice> _devices;

        private readonly object _updateLock = new();

        public override bool Initialize()
        {
            if (IsInitialized)
                return true;

            try
            {
                var ip = Global.Configuration.VarRegistry.GetVariable<string>($"{DeviceName}_ip");
                var port = Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_port");
                var connectSleepTimeSeconds = Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_connect_sleep_time");
                
                bool openrgbRunning = false;
                var processMonitor = RunningProcessMonitor.Instance;
                void OnProcessMonitorOnRunningProcessesChanged(object o, RunningProcessChanged runningProcessChanged)
                {
                    if (processMonitor.IsProcessRunning("openrgb.exe"))
                    {
                        openrgbRunning = true;
                    }
                }

                if (processMonitor.IsProcessRunning("openrgb.exe"))
                {
                    openrgbRunning = true;
                }
                processMonitor.RunningProcessesChanged += OnProcessMonitorOnRunningProcessesChanged;
                
                int remainingMillis = connectSleepTimeSeconds * 1000;
                while (!openrgbRunning)
                {
                    Thread.Sleep(100);
                    remainingMillis -= 100;
                    if (remainingMillis <= 0)
                    {
                        _openRgb = null;
                        return false;
                    }
                }
                processMonitor.RunningProcessesChanged -= OnProcessMonitorOnRunningProcessesChanged;

                while (remainingMillis > 0)
                {
                    try
                    {
                        _openRgb = new OpenRGBClient(name: "Aurora", ip: ip, port: port, autoconnect: true);
                    }
                    catch (Exception)
                    {
                        remainingMillis -= 1000;
                        if (remainingMillis <= 0)
                        {
                            _openRgb = null;
                            return false;
                        }
                        continue;
                    }
                    break;
                }

                _openRgb.DeviceListUpdated += OnDeviceListUpdated;
                UpdateDeviceList();
            }
            catch (Exception e)
            {
                LogError("error in OpenRGB device: " + e);
                IsInitialized = false;
                _openRgb = null;
                return false;
            }

            IsInitialized = true;
            return IsInitialized;
        }

        private void OnDeviceListUpdated(object sender, EventArgs e)
        {
            lock (_updateLock)
            {
                UpdateDeviceList();
            }
        }

        private void UpdateDeviceList()
        {
            if (_openRgb == null)
            {
                return;
            }
            
            _devices = new List<HelperOpenRgbDevice>();
            Queue<DeviceKeys> mouseLights = new Queue<DeviceKeys>(OpenRgbKeyNames.MouseLights);
            
            var fallbackKey = Global.Configuration.VarRegistry.GetVariable<DK>($"{DeviceName}_fallback_key");
            lock (_updateLock)
            {
                foreach (var device in _openRgb.EnumerateControllerData())
                {
                    var directMode = device.Modes.FirstOrDefault(m => m.Name.Equals("Direct"));
                    if (directMode == null) continue;
                    _openRgb.SetMode(device.ID, device.Modes.FindIndex(mode => mode == directMode));
                    var helper = new HelperOpenRgbDevice(device.ID, device, fallbackKey, mouseLights);
                    helper.ProcessMappings(fallbackKey);
                    _devices.Add(helper);
                }
                Thread.Sleep(500);
            }
        }

        public override void Shutdown()
        {
            if (!IsInitialized)
                return;

            lock (_updateLock)
                foreach (var d in _devices)
                {
                    try
                    {
                        _openRgb.UpdateLeds(d.Index, d.OrgbDevice.Colors);
                    }
                    catch
                    {
                        //we tried.
                    }
                }

            _openRgb?.Dispose();
            _openRgb = null;
            IsInitialized = false;
        }

        protected override bool UpdateDevice(Dictionary<DK, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (!IsInitialized)
                return false;

            lock (_updateLock)
                foreach (var device in _devices)
                {
                    try
                    {
                        UpdateDevice(device, keyColors);
                        _openRgb.UpdateLeds(device.Index, device.Colors);
                    }
                    catch (Exception exc)
                    {
                        LogError($"Failed to update OpenRGB device {device.OrgbDevice.Name}: " + exc);
                        Reset();
                    }
                }

            var sleep = Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_sleep");
            if (sleep > 0)
                Thread.Sleep(sleep);

            return true;
        }

        private void UpdateDevice(HelperOpenRgbDevice device, IReadOnlyDictionary<DeviceKeys, Color> keyColors)
        {
            var ledIndex = 0;
            foreach (var zone in device.OrgbDevice.Zones)
            {
                var zoneLed = 0;
                for (; zoneLed < zone.LedCount; ledIndex++, zoneLed++)
                {
                    if (!keyColors.TryGetValue(device.Mapping[ledIndex], out var keyColor)) continue;
                    
                    var deviceKey = device.Colors[ledIndex];
                    deviceKey.R = keyColor.R;
                    deviceKey.G = keyColor.G;
                    deviceKey.B = keyColor.B;

                    var calibrationName = CalibrationName(device, zone);
                    if (!Global.Configuration.DeviceCalibrations.TryGetValue(calibrationName, out var calibration))
                        continue;
                    deviceKey.R = (byte) (keyColor.R * calibration.R / 255);
                    deviceKey.G = (byte) (keyColor.G * calibration.G / 255);
                    deviceKey.B = (byte) (keyColor.B * calibration.B / 255);
                }
            }
        }

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            variableRegistry.Register($"{DeviceName}_sleep", 0, "Sleep for", 1000, 0);
            variableRegistry.Register($"{DeviceName}_ip", "127.0.0.1", "IP Address");
            variableRegistry.Register($"{DeviceName}_port", 6742, "Port", 1024, 65535);
            variableRegistry.Register($"{DeviceName}_fallback_key", DK.Peripheral_Logo, "Key to use for unknown leds. Select NONE to disable");
            variableRegistry.Register($"{DeviceName}_connect_sleep_time", 5, "Connection timeout seconds");
        }

        public override IEnumerable<string> GetDevices()
        {
            lock (_updateLock)
                return from device in _devices
                    from zone in device.OrgbDevice.Zones
                    select CalibrationName(device, zone);
        }

        private string CalibrationName(HelperOpenRgbDevice device, Zone zone)
        {
            return device.ZoneCalibrationNames[zone.ID];
        }
    }

    public class HelperOpenRgbDevice
    {
        public int Index { get; }
        public OpenRGBDevice OrgbDevice { get; }
        public OpenRGBColor[] Colors { get; }
        public DK[] Mapping { get; }
        public Dictionary<int, string> ZoneCalibrationNames { get; } = new();

        private readonly Queue<DeviceKeys> _mouseLights;

        public HelperOpenRgbDevice(int idx, Device dev, DeviceKeys fallbackKey, Queue<DeviceKeys> mouseLights)
        {
            Index = idx;
            OrgbDevice = dev;
            Colors = Enumerable.Range(0, dev.Leds.Length).Select(_ => new OpenRGBColor()).ToArray();
            Mapping = new DK[dev.Zones.Sum(z => z.LedCount)];
            _mouseLights = mouseLights;
            foreach (var zone in dev.Zones)
            {
                ZoneCalibrationNames.Add(zone.ID, $"OpenRGB_{dev.Name.Trim()}_{zone.Name}");
            }
        }

        internal void ProcessMappings(DK fallbackKey)
        {
            for (var ledIndex = 0; ledIndex < OrgbDevice.Leds.Length; ledIndex++)
            {
                var orgbKeyName = OrgbDevice.Leds[ledIndex].Name;
                if (OrgbDevice.Type == OpenRGBDeviceType.Mouse && (orgbKeyName.Equals("Logo") || orgbKeyName.Equals("Logo LED")))
                {
                    Mapping[ledIndex] = DeviceKeys.Peripheral_Logo;
                }
                else if (OpenRgbKeyNames.KeyNames.TryGetValue(orgbKeyName, out var devKey) ||
                         OpenRgbKeyNames.KeyNames.TryGetValue(orgbKeyName.Replace(" LED", ""), out devKey) ||
                         OpenRgbKeyNames.KeyNames.TryGetValue("Key: " + orgbKeyName, out devKey)
                        )
                {
                    Mapping[ledIndex] = devKey;
                }
                else
                {
                    Mapping[ledIndex] = fallbackKey;
                }
            }

            //if we have the option enabled,
            //we'll skip these as users may not want 
            //linear zones to depend on additionalllight

            uint ledOffset = 0;
            for (int zoneIndex = 0; zoneIndex < OrgbDevice.Zones.Length; zoneIndex++)
            {
                if (OrgbDevice.Zones[zoneIndex].Type == OpenRGBZoneType.Linear)
                {
                    for (int zoneLedIndex = 0; zoneLedIndex < OrgbDevice.Zones[zoneIndex].LedCount; zoneLedIndex++)
                    {
                        switch (OrgbDevice.Type)
                        {
                            case OpenRGBDeviceType.Mousemat:
                                if (zoneLedIndex < OpenRgbKeyNames.MousepadLights.Length)
                                {
                                    Mapping[(int)(ledOffset + zoneLedIndex)] =
                                        OpenRgbKeyNames.MousepadLights[zoneLedIndex];
                                }
                                break;
                            case OpenRGBDeviceType.Mouse:
                                if (zoneLedIndex < OpenRgbKeyNames.MouseLights.Length)
                                {
                                    if (_mouseLights.TryDequeue(out var res))
                                    {
                                        Mapping[(int)(ledOffset + zoneLedIndex)] = res;
                                    }
                                }
                                break;
                            default:
                                if (zoneLedIndex < OpenRgbKeyNames.AdditionalLights.Length)
                                {
                                    Mapping[(int)(ledOffset + zoneLedIndex)] =
                                        OpenRgbKeyNames.AdditionalLights[zoneLedIndex];
                                }
                                break;
                        }
                    }
                }
                ledOffset += OrgbDevice.Zones[zoneIndex].LedCount;
            }
        }
    }
}
