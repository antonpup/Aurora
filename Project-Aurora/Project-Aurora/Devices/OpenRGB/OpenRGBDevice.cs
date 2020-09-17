using Aurora.Settings;
using OpenRGB.NET;
using OpenRGB.NET.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        protected override string DeviceInfo => string.Join(", ", _devices.Select(d => d.Name));

        private OpenRGBClient _openRgb;
        private OpenRGBDevice[] _devices;
        private OpenRGBColor[][] _deviceColors;
        private List<DK>[] _keyMappings;

        public override bool Initialize()
        {
            if (IsInitialized)
                return true;

            try
            {
                _openRgb = new OpenRGBClient(name: "Aurora");
                _openRgb.Connect();

                _devices = _openRgb.GetAllControllerData();

                _deviceColors = new OpenRGBColor[_devices.Length][];
                _keyMappings = new List<DK>[_devices.Length];

                for (var i = 0; i < _devices.Length; i++)
                {
                    var dev = _devices[i];

                    _deviceColors[i] = new OpenRGBColor[dev.Leds.Length];
                    for (var ledIdx = 0; ledIdx < dev.Leds.Length; ledIdx++)
                        _deviceColors[i][ledIdx] = new OpenRGBColor();

                    _keyMappings[i] = new List<DK>();

                    for (int j = 0; j < dev.Leds.Length; j++)
                    {
                        if (dev.Type == OpenRGBDeviceType.Keyboard)
                        {
                            if (OpenRGBKeyNames.Keyboard.TryGetValue(dev.Leds[j].Name, out var dk))
                            {
                                _keyMappings[i].Add(dk);
                            }
                            else
                            {
                                _keyMappings[i].Add(DK.NONE);
                            }
                        }
                        else if (dev.Type == OpenRGBDeviceType.Mouse)
                        {
                            if (OpenRGBKeyNames.Mouse.TryGetValue(dev.Leds[j].Name, out var dk))
                            {
                                _keyMappings[i].Add(dk);
                            }
                            else
                            {
                                _keyMappings[i].Add(DK.Peripheral_Logo);
                            }
                        }
                        else
                        {
                            _keyMappings[i].Add(DK.Peripheral_Logo);
                        }
                    }

                    uint LedOffset = 0;
                    for (int j = 0; j < dev.Zones.Length; j++)
                    {
                        if (dev.Zones[j].Type == OpenRGBZoneType.Linear)
                        {
                            for (int k = 0; k < dev.Zones[j].LedCount; k++)
                            {
                                if (dev.Type == OpenRGBDeviceType.Mousemat)
                                {
                                    if (k < 15)
                                    {
                                        _keyMappings[i][(int)(LedOffset + k)] = OpenRGBKeyNames.MousepadLights[k];
                                    }
                                }
                                else
                                {
                                    //TODO - scale zones with more than 32 LEDs
                                    if (k < 32)
                                    {
                                        _keyMappings[i][(int)(LedOffset + k)] = OpenRGBKeyNames.AdditionalLights[k];
                                    }
                                }
                            }
                        }
                        LedOffset += dev.Zones[j].LedCount;
                    }
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

            for (var i = 0; i < _devices.Length; i++)
            {
                try
                {
                    _openRgb.UpdateLeds(i, _devices[i].Colors);
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

        public override bool UpdateDevice(Dictionary<DK, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (!IsInitialized)
                return false;

            for (var i = 0; i < _devices.Length; i++)
            {
                //should probably store these bools somewhere when initing
                //might also add this as a property in the library
                if (!_devices[i].Modes.Any(m => m.Name == "Direct"))
                    continue;

                for (int ledIdx = 0; ledIdx < _devices[i].Leds.Length; ledIdx++)
                {
                    if (keyColors.TryGetValue(_keyMappings[i][ledIdx], out var keyColor))
                    {
                        _deviceColors[i][ledIdx] = new OpenRGBColor(keyColor.R, keyColor.G, keyColor.B);
                    }
                }

                try
                {
                    _openRgb.UpdateLeds(i, _deviceColors[i]);
                }
                catch (Exception exc)
                {
                    LogError($"Failed to update OpenRGB device {_devices[i].Name}: " + exc);
                    Reset();
                }
            }

            var sleep = Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_sleep");
            if (sleep > 0)
                Thread.Sleep(sleep);

            return true;
        }

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            variableRegistry.Register($"{DeviceName}_sleep", 25, "Sleep for", 1000, 0);
        }
    }
}
