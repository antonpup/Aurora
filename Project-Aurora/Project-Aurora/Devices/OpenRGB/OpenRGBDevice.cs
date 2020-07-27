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
    public class OpenRGBAuroraDevice : Device
    {
        const string deviceName = "OpenRGB";
        private VariableRegistry varReg;
        private bool isInitialized = false;
        private readonly Stopwatch watch = new Stopwatch();
        private long lastUpdateTime = 0;

        private OpenRGBClient _openRgb;
        private OpenRGBDevice[] _devices;
        private OpenRGBColor[][] _deviceColors;
        private List<DK>[] _keyMappings;

        private List<DK> AdditionalLights = new List<DK>(new[]
        {
            DK.ADDITIONALLIGHT1,
            DK.ADDITIONALLIGHT2,
            DK.ADDITIONALLIGHT3,
            DK.ADDITIONALLIGHT4,
            DK.ADDITIONALLIGHT5,
            DK.ADDITIONALLIGHT6,
            DK.ADDITIONALLIGHT7,
            DK.ADDITIONALLIGHT8,
            DK.ADDITIONALLIGHT9,
            DK.ADDITIONALLIGHT10,
            DK.ADDITIONALLIGHT11,
            DK.ADDITIONALLIGHT12,
            DK.ADDITIONALLIGHT13,
            DK.ADDITIONALLIGHT14,
            DK.ADDITIONALLIGHT15,
            DK.ADDITIONALLIGHT16,
            DK.ADDITIONALLIGHT17,
            DK.ADDITIONALLIGHT18,
            DK.ADDITIONALLIGHT19,
            DK.ADDITIONALLIGHT20,
            DK.ADDITIONALLIGHT21,
            DK.ADDITIONALLIGHT22,
            DK.ADDITIONALLIGHT23,
            DK.ADDITIONALLIGHT24,
            DK.ADDITIONALLIGHT25,
            DK.ADDITIONALLIGHT26,
            DK.ADDITIONALLIGHT27,
            DK.ADDITIONALLIGHT28,
            DK.ADDITIONALLIGHT29,
            DK.ADDITIONALLIGHT30,
            DK.ADDITIONALLIGHT31,
            DK.ADDITIONALLIGHT32,
        });

        public bool Initialize()
        {
            if (isInitialized)
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
                            if (OpenRGBKeyNames.Names.TryGetValue(dev.Leds[j].Name, out var dk))
                            {
                                _keyMappings[i].Add(dk);
                            }
                            else
                            {
                                _keyMappings[i].Add(DK.NONE);
                            }
                        }
                        else if(dev.Type == OpenRGBDeviceType.Mouse)
                        {
                            if (OpenRGBMouseKeyNames.Names.TryGetValue(dev.Leds[j].Name, out var dk))
                            {
                                _keyMappings[i].Add(dk);
                            }
                            else
                            {
                                _keyMappings[i].Add(DK.NONE);
                            }
                        }
                        else
                        {
                            _keyMappings[i].Add(DK.Peripheral_Logo);
                        }
                    }

                    uint LedOffset = 0;
                    for(int j = 0; j < dev.Zones.Length; j++)
                    {
                        if(dev.Zones[j].Type == OpenRGBZoneType.Linear)
                        {
                            for(int k = 0; k < dev.Zones[j].LedCount; k++)
                            {
                                _keyMappings[i][(int)(LedOffset + k)] = AdditionalLights[k];
                            }
                        }
                        LedOffset += dev.Zones[j].LedCount;
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
                    case OpenRGBDeviceType.Mouse:
                    default:
                        for (int ledIdx = 0; ledIdx < _devices[i].Leds.Length; ledIdx++)
                        {
                            if (keyColors.TryGetValue(_keyMappings[i][ledIdx], out var keyColor))
                            {
                                _deviceColors[i][ledIdx] = new OpenRGBColor(keyColor.R, keyColor.G, keyColor.B);
                            }
                        }
                        break;

                    
                        //if (!Global.Configuration.VarRegistry.GetVariable<bool>($"{deviceName}_generic"))
                        //    continue;
                        //if (keyColors.TryGetValue(DK.Peripheral_Logo, out var color))
                        //{
                        //    for (int j = 0; j < _deviceColors[i].Length; j++)
                        //    {
                        //        _deviceColors[i][j] = new OpenRGBColor(color.R, color.G, color.B);
                        //    }
                        //}
                        //break;
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
