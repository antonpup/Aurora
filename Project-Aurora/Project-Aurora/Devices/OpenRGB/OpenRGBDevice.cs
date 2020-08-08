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
//        private MList<DK>[] _keyMappings;          a Try for later

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
//                _keyMappings = new MList<DK>[_devices.Length];               a try for later

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
                                _keyMappings[i].Add(DK.NONE);
                            }
                        }
//*****************************************************************************************************************************
//          Adding possibility to adress mousemats as Mousepadlight and not as LEDlights like now             (not working now)
//*****************************************************************************************************************************                       
//                        else if (dev.Type == OpenRGBDeviceType.Mousemat)
//                        {
//                            if (OpenRGBKeyNames.Mousemat.TryGetValue(dev.Leds[j].Name, out var dk))
//                            {
//                                _keyMappings[i].Add(dk);
//                            }
//                            else
//                            {
//                                _keyMappings[i].Add(DK.NONE);
//                            }
//                        }
//                        else
//                        {
//                            _keyMappings[i].Add(DK.Peripheral_Logo);
//                        }
//                    }
//    A Try for later if upper method does not work
//                    for (int j = 0; j < dev.Leds.Length; j++)
//                    {
//                        if (dev.Leds[j].Type == OpenRGBZoneType.Linear)
//                        {
//                            for (int k = 0; k < dev.Leds[j].LedCount; k++)
//                            {
//                                if (k < 15)
//                                {
//                                    _keyMappings[i][(int)(LedOffset + k)] = MousepadLights[k];
//                                }
//                            }
//                        }
//                        LedOffset += dev.Leds[j].LedCount;
//                    }

                    uint LedOffset = 0;
                    for (int j = 0; j < dev.Zones.Length; j++)
                    {
                        if (dev.Zones[j].Type == OpenRGBZoneType.Linear)
                        {
                            for (int k = 0; k < dev.Zones[j].LedCount; k++)
                            {
                                //TODO - scale zones with more than 100 LEDs XD
                                if (k < 100)
                                {
                                    _keyMappings[i][(int)(LedOffset + k)] = LedLights[k];
                                }
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
            isInitialized = false;
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (!isInitialized)
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
                    Global.logger.Error($"Failed to update OpenRGB device {_devices[i].Name}: " + exc);
                    Reset();
                }
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
                devString += string.Join(", ", names);
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
            return isInitialized ? lastUpdateTime + " ms" : "";
        }

        public VariableRegistry GetRegisteredVariables()
        {
            if (varReg == null)
            {
                varReg = new VariableRegistry();
                varReg.Register($"{deviceName}_sleep", 25, "Sleep for", 1000, 0);
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
//   A Try for later if upper method not work
//        private static readonly MList<DK> MousePadLights = new MList<DK>(new[]
//        {
//            DK.MOUSEPADLIGHT1,
//            DK.MOUSEPADLIGHT2,
//            DK.MOUSEPADLIGHT3,
//            DK.MOUSEPADLIGHT4,
//            DK.MOUSEPADLIGHT5,
//            DK.MOUSEPADLIGHT6,
//            DK.MOUSEPADLIGHT7,
//            DK.MOUSEPADLIGHT8,
//            DK.MOUSEPADLIGHT9,
//            DK.MOUSEPADLIGHT10,
//            DK.MOUSEPADLIGHT11,
//            DK.MOUSEPADLIGHT12,
//            DK.MOUSEPADLIGHT13,
//            DK.MOUSEPADLIGHT14,
//            DK.MOUSEPADLIGHT15,
//        });

        private static readonly List<DK> LedLights = new List<DK>(new[]
        {
            DK.LEDLIGHT1,
            DK.LEDLIGHT2,
            DK.LEDLIGHT3,
            DK.LEDLIGHT4,
            DK.LEDLIGHT5,
            DK.LEDLIGHT6,
            DK.LEDLIGHT7,
            DK.LEDLIGHT8,
            DK.LEDLIGHT9,
            DK.LEDLIGHT10,
            DK.LEDLIGHT11,
            DK.LEDLIGHT12,
            DK.LEDLIGHT13,
            DK.LEDLIGHT14,
            DK.LEDLIGHT15,
            DK.LEDLIGHT16,
            DK.LEDLIGHT17,
            DK.LEDLIGHT18,
            DK.LEDLIGHT19,
            DK.LEDLIGHT20,
            DK.LEDLIGHT21,
            DK.LEDLIGHT22,
            DK.LEDLIGHT23,
            DK.LEDLIGHT24,
            DK.LEDLIGHT25,
            DK.LEDLIGHT26,
            DK.LEDLIGHT27,
            DK.LEDLIGHT28,
            DK.LEDLIGHT29,
            DK.LEDLIGHT30,
            DK.LEDLIGHT31,
            DK.LEDLIGHT32,
            DK.LEDLIGHT33,
            DK.LEDLIGHT34,
            DK.LEDLIGHT35,
            DK.LEDLIGHT36,
            DK.LEDLIGHT37,
            DK.LEDLIGHT38,
            DK.LEDLIGHT39,
            DK.LEDLIGHT40,
            DK.LEDLIGHT41,
            DK.LEDLIGHT42,
            DK.LEDLIGHT43,
            DK.LEDLIGHT44,
            DK.LEDLIGHT45,
            DK.LEDLIGHT46,
            DK.LEDLIGHT47,
            DK.LEDLIGHT48,
            DK.LEDLIGHT49,
            DK.LEDLIGHT50,
            DK.LEDLIGHT51,
            DK.LEDLIGHT52,
            DK.LEDLIGHT53,
            DK.LEDLIGHT54,
            DK.LEDLIGHT55,
            DK.LEDLIGHT56,
            DK.LEDLIGHT57,
            DK.LEDLIGHT58,
            DK.LEDLIGHT59,
            DK.LEDLIGHT60,
            DK.LEDLIGHT61,
            DK.LEDLIGHT62,
            DK.LEDLIGHT63,
            DK.LEDLIGHT64,
            DK.LEDLIGHT65,
            DK.LEDLIGHT66,
            DK.LEDLIGHT67,
            DK.LEDLIGHT68,
            DK.LEDLIGHT69,
            DK.LEDLIGHT70,
            DK.LEDLIGHT71,
            DK.LEDLIGHT72,
            DK.LEDLIGHT73,
            DK.LEDLIGHT74,
            DK.LEDLIGHT75,
            DK.LEDLIGHT76,
            DK.LEDLIGHT77,
            DK.LEDLIGHT78,
            DK.LEDLIGHT79,
            DK.LEDLIGHT80,
            DK.LEDLIGHT81,
            DK.LEDLIGHT82,
            DK.LEDLIGHT83,
            DK.LEDLIGHT84,
            DK.LEDLIGHT85,
            DK.LEDLIGHT86,
            DK.LEDLIGHT87,
            DK.LEDLIGHT88,
            DK.LEDLIGHT89,
            DK.LEDLIGHT90,
            DK.LEDLIGHT91,
            DK.LEDLIGHT92,
            DK.LEDLIGHT93,
            DK.LEDLIGHT94,
            DK.LEDLIGHT95,
            DK.LEDLIGHT96,
            DK.LEDLIGHT97,
            DK.LEDLIGHT98,
            DK.LEDLIGHT99,
            DK.LEDLIGHT100,
        });
    }
}
