﻿using Aurora.Settings;
using OpenRGB.NET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using Aurora.Utils;
using Microsoft.Scripting.Utils;
using DK = Aurora.Devices.DeviceKeys;
using OpenRGBColor = OpenRGB.NET.Models.Color;
using OpenRGBDevice = OpenRGB.NET.Models.Device;
using OpenRGBDeviceType = OpenRGB.NET.Enums.DeviceType;
using OpenRGBZoneType = OpenRGB.NET.Enums.ZoneType;

namespace Aurora.Devices.OpenRGB
{
    public class OpenRGBAuroraDevice : DefaultDevice
    {
        public override string DeviceName => "OpenRGB";
        protected override string DeviceInfo => string.Join(", ", _devices.Select(d => d.OrgbDevice.Name));

        private OpenRGBClient _openRgb;
        private List<HelperOpenRGBDevice> _devices;

        public override bool Initialize()
        {
            if (IsInitialized)
                return true;

            try
            {
                var ip = Global.Configuration.VarRegistry.GetVariable<string>($"{DeviceName}_ip");
                var port = Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_port");
                var usePeriphLogo = Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_use_periph_logo");

                _openRgb = new OpenRGBClient(name: "Aurora", ip: ip, port: port);
                _openRgb.Connect();

                var devices = _openRgb.GetAllControllerData();
                _devices = new List<HelperOpenRGBDevice>();

                for (var i = 0; i < devices.Length; i++)
                {
                    var device = devices[i];
                    var directMode = device.Modes.FirstOrDefault(m => m.Name.Equals("Direct"));
                    if (directMode == null) continue;
                    _openRgb.SetMode(i, device.Modes.FindIndex(mode => mode ==directMode));
                    var helper = new HelperOpenRGBDevice(i, device);
                    helper.ProcessMappings(usePeriphLogo);
                    _devices.Add(helper);
                }
            }
            catch (Exception e)
            {
                LogError("error in OpenRGB device: " + e);
                IsInitialized = false;
                return false;
            }

            IsInitialized = true;
            return IsInitialized;
        }

        public override void Shutdown()
        {
            if (!IsInitialized)
                return;

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

            foreach (var device in _devices)
            {
                var calibrationName = $"{DeviceName}_{device.OrgbDevice.Serial.Trim()}_cal";
                if (Global.Configuration.VarRegistry.GetRegisteredVariableKeys().Contains(calibrationName))
                {
                    var calibration =
                        Global.Configuration.VarRegistry.GetVariable<RealColor>(calibrationName).GetDrawingColor();
                    UpdateCalibratedDevice(device, keyColors, calibration);
                }
                else
                {
                    UpdateDevice(device, keyColors);
                }

                try
                {
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

        private void UpdateDevice(HelperOpenRGBDevice device, IReadOnlyDictionary<DeviceKeys, Color> keyColors)
        {
            for (var ledIndex = 0; ledIndex < device.Colors.Length; ledIndex++)
            {
                if (!keyColors.TryGetValue(device.Mapping[ledIndex], out var keyColor)) continue;
                var deviceKey = device.Colors[ledIndex];
                deviceKey.R = keyColor.R;
                deviceKey.G = keyColor.G;
                deviceKey.B = keyColor.B;
            }
        }

        private void UpdateCalibratedDevice(HelperOpenRGBDevice device, IReadOnlyDictionary<DeviceKeys, Color> keyColors,
            Color calibration)
        {
            for (var ledIndex = 0; ledIndex < device.Colors.Length; ledIndex++)
            {
                if (!keyColors.TryGetValue(device.Mapping[ledIndex], out var keyColor)) continue;
                var deviceKey = device.Colors[ledIndex];
                deviceKey.R = (byte) (keyColor.R * calibration.R / 255);
                deviceKey.G = (byte) (keyColor.G * calibration.G / 255);
                deviceKey.B = (byte) (keyColor.B * calibration.B / 255);
            }
        }

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            variableRegistry.Register($"{DeviceName}_sleep", 0, "Sleep for", 1000, 0);
            variableRegistry.Register($"{DeviceName}_ip", "127.0.0.1", "IP Address");
            variableRegistry.Register($"{DeviceName}_port", 6742, "Port", 1024, 65535);
            variableRegistry.Register($"{DeviceName}_use_periph_logo", true, "Use peripheral logo for unknown leds");
        }
    }

    public class HelperOpenRGBDevice
    {
        public int Index { get; }
        public OpenRGBDevice OrgbDevice { get; }
        public OpenRGBColor[] Colors { get; }
        public DK[] Mapping { get; }

        public HelperOpenRGBDevice(int idx, OpenRGBDevice dev)
        {
            Index = idx;
            OrgbDevice = dev;
            Colors = Enumerable.Range(0, dev.Leds.Length).Select(_ => new OpenRGBColor()).ToArray();
            Mapping = new DK[dev.Leds.Length];
        }

        internal void ProcessMappings(bool usePeriphLogo)
        {
            for (int ledIndex = 0; ledIndex < OrgbDevice.Leds.Length; ledIndex++)
            {
                if (OpenRGBKeyNames.KeyNames.TryGetValue(OrgbDevice.Leds[ledIndex].Name, out var devKey))
                {
                    Mapping[ledIndex] = devKey;
                }
                else
                {
                    Mapping[ledIndex] = usePeriphLogo ? DK.Peripheral_Logo : DK.NONE;
                }
            }

            if (usePeriphLogo)
                return;

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
                        if (OrgbDevice.Type == OpenRGBDeviceType.Mousemat)
                        {
                            if (zoneLedIndex < 15)
                            {
                                Mapping[(int)(ledOffset + zoneLedIndex)] = OpenRGBKeyNames.MousepadLights[zoneLedIndex];
                            }
                        }
                        else
                        {
                            //TODO - scale zones with more than 32 LEDs
                            if (zoneLedIndex < 60)
                            {
                                Mapping[(int)(ledOffset + zoneLedIndex)] = OpenRGBKeyNames.AdditionalLights[zoneLedIndex];
                            }
                        }
                    }
                }
                ledOffset += OrgbDevice.Zones[zoneIndex].LedCount;
            }
        }
    }
}
