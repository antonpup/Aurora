﻿using Aurora.Settings;
using OpenRGB.NET;
using OpenRGB.NET.Enums;
using Roccat_Talk.RyosTalkFX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DK = Aurora.Devices.DeviceKeys;
using OpenRGBDevice = OpenRGB.NET.Models.Device;
using OpenRGBColor = OpenRGB.NET.Models.Color;
using OpenRGBDeviceType = OpenRGB.NET.Enums.DeviceType;

namespace Aurora.Devices.OpenRGB
{
    public class OpenRGBAuroraDevice : Device
    {
        const string deviceName = "OpenRGB";
        VariableRegistry varReg;
        bool isInitialized = false;
        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;

        private OpenRGBClient _openRgb;
        private OpenRGBDevice[] _devices;
        private OpenRGBColor[][] _deviceColors;
        private Dictionary<DK, int>[] _keyMappings;

        public bool Initialize()
        {
            if (isInitialized)
                return true;

            try
            {
                _openRgb = new OpenRGBClient(name: "Aurora");
                _openRgb.Connect();

                _devices = _openRgb.GetAllControllerData();

                for (var i = 0; i < _devices.Length; i++)
                {
                    var dev = _devices[i];

                    _deviceColors = new OpenRGBColor[_devices.Length][];
                    _deviceColors[i] =
                        Enumerable.Range(0, dev.Colors.Length)
                                  .Select(_ => new OpenRGBColor()).ToArray();

                    _keyMappings = new Dictionary<DK, int>[_devices.Length];
                    _keyMappings[i] = new Dictionary<DK, int>();
                    for (int j = 0; j < dev.Leds.Length; j++)
                    {
                        if (OpenRGBKeyNames.Names.TryGetValue(dev.Leds[j].Name, out var dk))
                        {
                            _keyMappings[i].Add(dk, j);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Global.logger.Error("error in OpenRGB device: " + e);
                isInitialized = false;
                return false;
            }

            isInitialized = true;
            return isInitialized;
        }

        public void Shutdown()
        {
            if (!isInitialized)
                return;

            for (var i = 0; i < _devices.Length; i++)
            {
                _openRgb.UpdateLeds(i, _devices[i].Colors);
            }

            _openRgb.Dispose();
            _openRgb = null;
            isInitialized = false;
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (!isInitialized)
                return false;

            for (var i = 0; i < _devices.Length; i++)
            {
                switch (_devices[i].Type)
                {
                    case OpenRGBDeviceType.Keyboard:

                        foreach (var led in keyColors)
                        {
                            if (_keyMappings[i].TryGetValue(led.Key, out int index))
                            {
                                _deviceColors[i][index] = new OpenRGBColor(led.Value.R, led.Value.G, led.Value.B);
                            }
                        }
                        break;

                    case OpenRGBDeviceType.Mouse:
                        break;

                    default:
                        if (!Global.Configuration.VarRegistry.GetVariable<bool>($"{deviceName}_generic"))
                            continue;
                        if (keyColors.TryGetValue(DK.Peripheral_Logo, out var color))
                        {
                            for (int j = 0; j < _deviceColors[i].Length; j++)
                            {
                                _deviceColors[i][j] = new OpenRGBColor(color.R, color.G, color.B);
                            }
                        }
                        break;
                }

                _openRgb.UpdateLeds(i, _deviceColors[i]);
            }
            var sleep = Global.Configuration.VarRegistry.GetVariable<int>($"{deviceName}_sleep");
            if (sleep > 0)
                Thread.Sleep(sleep);
            return true;
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            watch.Restart();

            bool update_result = UpdateDevice(colorComposition.keyColors, e, forced);

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return update_result;
        }

        public string GetDeviceDetails()
        {
            if (isInitialized)
            {
                string devString = deviceName + ": ";
                devString += "Connected ";
                var names = _devices.Select(c => c.Name);
                devString += string.Join(",", names);
                return devString;
            }
            else
            {
                return deviceName + ": Not initialized";
            }
        }

        public string GetDeviceName()
        {
            return deviceName;
        }

        public string GetDeviceUpdatePerformance()
        {
            return (isInitialized ? lastUpdateTime + " ms" : "");
        }

        public VariableRegistry GetRegisteredVariables()
        {
            if (varReg == null)
            {
                varReg = new VariableRegistry();
                varReg.Register($"{deviceName}_sleep", 25, "Sleep for", 1000, 0);
                varReg.Register($"{deviceName}_generic", false, "Set colors on generic devices");
            }
            return varReg;
        }

        public bool IsConnected()
        {
            return isInitialized;
        }

        public bool IsInitialized()
        {
            return isInitialized;
        }

        public bool IsKeyboardConnected()
        {
            return isInitialized;
        }

        public bool IsPeripheralConnected()
        {
            return isInitialized;
        }

        public bool Reconnect()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            Shutdown();
            Initialize();
        }
    }
}