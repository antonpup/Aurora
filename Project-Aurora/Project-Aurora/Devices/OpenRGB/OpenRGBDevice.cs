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
                        else
                        {
                            _keyMappings[i].Add(DK.Peripheral_Logo);
                        }
                    }

                    //*****************************************************************************************************************************
                    //          Following Code is a Try Adding possibility to adress mousemats as Mousepadlight and not as LEDlights like now
                    //          But i need someone to make it working as i am not understand codelogic (i am not a coder, just edit i can)  
                    //           The Dictionary Entrys below marked out aswell                         (not working now)
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
                    //*****************************************************************************************************************************
                    //          Following Code is another Method Try Adding possibility to adress mousemats as Mousepadlight and not as LEDlights like now
                    //          But i need someone to make it working as i am not understand codelogic (i am not a coder, just edit i can)  
                    //                                                                                                   (not working now)
                    //*****************************************************************************************************************************       
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
//   A Try for later if upper method not work                 Dictionairy 
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
            // More than 100 Leds
            DK.LEDLIGHT101,
            DK.LEDLIGHT102,
            DK.LEDLIGHT103,
            DK.LEDLIGHT104,
            DK.LEDLIGHT105,
            DK.LEDLIGHT106,
            DK.LEDLIGHT107,
            DK.LEDLIGHT108,
            DK.LEDLIGHT109,
            DK.LEDLIGHT110,
            DK.LEDLIGHT111,
            DK.LEDLIGHT112,
            DK.LEDLIGHT113,
            DK.LEDLIGHT114,
            DK.LEDLIGHT115,
            DK.LEDLIGHT116,
            DK.LEDLIGHT117,
            DK.LEDLIGHT118,
            DK.LEDLIGHT119,
            DK.LEDLIGHT120,
            DK.LEDLIGHT121,
            DK.LEDLIGHT122,
            DK.LEDLIGHT123,
            DK.LEDLIGHT124,
            DK.LEDLIGHT125,
            DK.LEDLIGHT126,
            DK.LEDLIGHT127,
            DK.LEDLIGHT128,
            DK.LEDLIGHT129,
            DK.LEDLIGHT130,
            DK.LEDLIGHT131,
            DK.LEDLIGHT132,
            DK.LEDLIGHT133,
            DK.LEDLIGHT134,
            DK.LEDLIGHT135,
            DK.LEDLIGHT136,
            DK.LEDLIGHT137,
            DK.LEDLIGHT138,
            DK.LEDLIGHT139,
            DK.LEDLIGHT140,
            DK.LEDLIGHT141,
            DK.LEDLIGHT142,
            DK.LEDLIGHT143,
            DK.LEDLIGHT144,
            DK.LEDLIGHT145,
            DK.LEDLIGHT146,
            DK.LEDLIGHT147,
            DK.LEDLIGHT148,
            DK.LEDLIGHT149,
            DK.LEDLIGHT150,
            DK.LEDLIGHT151,
            DK.LEDLIGHT152,
            DK.LEDLIGHT153,
            DK.LEDLIGHT154,
            DK.LEDLIGHT155,
            DK.LEDLIGHT156,
            DK.LEDLIGHT157,
            DK.LEDLIGHT158,
            DK.LEDLIGHT159,
            DK.LEDLIGHT160,
            DK.LEDLIGHT161,
            DK.LEDLIGHT162,
            DK.LEDLIGHT163,
            DK.LEDLIGHT164,
            DK.LEDLIGHT165,
            DK.LEDLIGHT166,
            DK.LEDLIGHT167,
            DK.LEDLIGHT168,
            DK.LEDLIGHT169,
            DK.LEDLIGHT170,
            DK.LEDLIGHT171,
            DK.LEDLIGHT172,
            DK.LEDLIGHT173,
            DK.LEDLIGHT174,
            DK.LEDLIGHT175,
            DK.LEDLIGHT176,
            DK.LEDLIGHT177,
            DK.LEDLIGHT178,
            DK.LEDLIGHT179,
            DK.LEDLIGHT180,
            DK.LEDLIGHT181,
            DK.LEDLIGHT182,
            DK.LEDLIGHT183,
            DK.LEDLIGHT184,
            DK.LEDLIGHT185,
            DK.LEDLIGHT186,
            DK.LEDLIGHT187,
            DK.LEDLIGHT188,
            DK.LEDLIGHT189,
            DK.LEDLIGHT190,
            DK.LEDLIGHT191,
            DK.LEDLIGHT192,
            DK.LEDLIGHT193,
            DK.LEDLIGHT194,
            DK.LEDLIGHT195,
            DK.LEDLIGHT196,
            DK.LEDLIGHT197,
            DK.LEDLIGHT198,
            DK.LEDLIGHT199,
            DK.LEDLIGHT200,
            // More than 200 Leds
    //  I need to define them first in Device.cs
//            DK.LEDLIGHT201,
//            DK.LEDLIGHT202,
//            DK.LEDLIGHT203,
//            DK.LEDLIGHT204,
//            DK.LEDLIGHT205,
//            DK.LEDLIGHT206,
//            DK.LEDLIGHT207,
//            DK.LEDLIGHT208,
//            DK.LEDLIGHT209,
//            DK.LEDLIGHT210,
//            DK.LEDLIGHT211,
//            DK.LEDLIGHT212,
//            DK.LEDLIGHT213,
//            DK.LEDLIGHT214,
//            DK.LEDLIGHT215,
//            DK.LEDLIGHT216,
//            DK.LEDLIGHT217,
//            DK.LEDLIGHT218,
//            DK.LEDLIGHT219,
//            DK.LEDLIGHT220,
//            DK.LEDLIGHT221,
//            DK.LEDLIGHT222,
//            DK.LEDLIGHT223,
//            DK.LEDLIGHT224,
//            DK.LEDLIGHT225,
//            DK.LEDLIGHT226,
//            DK.LEDLIGHT227,
//            DK.LEDLIGHT228,
//            DK.LEDLIGHT229,
//            DK.LEDLIGHT230,
//            DK.LEDLIGHT231,
//            DK.LEDLIGHT232,
//            DK.LEDLIGHT233,
//            DK.LEDLIGHT234,
//            DK.LEDLIGHT235,
//            DK.LEDLIGHT236,
//            DK.LEDLIGHT237,
//            DK.LEDLIGHT238,
//            DK.LEDLIGHT239,
//            DK.LEDLIGHT240,
//            DK.LEDLIGHT241,
//            DK.LEDLIGHT242,
//            DK.LEDLIGHT243,
//            DK.LEDLIGHT244,
//            DK.LEDLIGHT245,
//            DK.LEDLIGHT246,
//            DK.LEDLIGHT247,
//            DK.LEDLIGHT248,
//            DK.LEDLIGHT249,
//            DK.LEDLIGHT250,
//            DK.LEDLIGHT251,
//            DK.LEDLIGHT252,
//            DK.LEDLIGHT253,
//            DK.LEDLIGHT254,
//            DK.LEDLIGHT255,
//            DK.LEDLIGHT256,
//            DK.LEDLIGHT257,
//            DK.LEDLIGHT258,
//            DK.LEDLIGHT259,
//            DK.LEDLIGHT260,
//            DK.LEDLIGHT261,
//            DK.LEDLIGHT262,
//            DK.LEDLIGHT263,
//            DK.LEDLIGHT264,
//            DK.LEDLIGHT265,
//            DK.LEDLIGHT266,
//            DK.LEDLIGHT267,
//            DK.LEDLIGHT268,
//            DK.LEDLIGHT269,
//            DK.LEDLIGHT270,
//            DK.LEDLIGHT271,
//            DK.LEDLIGHT272,
//            DK.LEDLIGHT273,
//            DK.LEDLIGHT274,
//            DK.LEDLIGHT275,
//            DK.LEDLIGHT276,
//            DK.LEDLIGHT277,
//            DK.LEDLIGHT278,
//            DK.LEDLIGHT279,
//            DK.LEDLIGHT280,
//            DK.LEDLIGHT281,
//            DK.LEDLIGHT282,
//            DK.LEDLIGHT283,
//            DK.LEDLIGHT284,
//            DK.LEDLIGHT285,
//            DK.LEDLIGHT286,
//            DK.LEDLIGHT287,
//            DK.LEDLIGHT288,
//            DK.LEDLIGHT289,
//            DK.LEDLIGHT290,
//            DK.LEDLIGHT291,
//            DK.LEDLIGHT292,
//            DK.LEDLIGHT293,
//            DK.LEDLIGHT294,
//            DK.LEDLIGHT295,
//            DK.LEDLIGHT296,
//            DK.LEDLIGHT297,
//            DK.LEDLIGHT298,
//            DK.LEDLIGHT299,
//            DK.LEDLIGHT300,
//            // More than 300 Leds            
//            DK.LEDLIGHT301,
//            DK.LEDLIGHT302,
//            DK.LEDLIGHT303,
//            DK.LEDLIGHT304,
//            DK.LEDLIGHT305,
//            DK.LEDLIGHT306,
//            DK.LEDLIGHT307,
//            DK.LEDLIGHT308,
//            DK.LEDLIGHT309,
//            DK.LEDLIGHT310,
//            DK.LEDLIGHT311,
//            DK.LEDLIGHT312,
//            DK.LEDLIGHT313,
//            DK.LEDLIGHT314,
//            DK.LEDLIGHT315,
//            DK.LEDLIGHT316,
//            DK.LEDLIGHT317,
//            DK.LEDLIGHT318,
//            DK.LEDLIGHT319,
//            DK.LEDLIGHT320,
//            DK.LEDLIGHT321,
//            DK.LEDLIGHT322,
//            DK.LEDLIGHT323,
//            DK.LEDLIGHT324,
//            DK.LEDLIGHT325,
//            DK.LEDLIGHT326,
//            DK.LEDLIGHT327,
//            DK.LEDLIGHT328,
//            DK.LEDLIGHT329,
//            DK.LEDLIGHT330,
//            DK.LEDLIGHT331,
//            DK.LEDLIGHT332,
//            DK.LEDLIGHT333,
//            DK.LEDLIGHT334,
//            DK.LEDLIGHT335,
//            DK.LEDLIGHT336,
//            DK.LEDLIGHT337,
//            DK.LEDLIGHT338,
//            DK.LEDLIGHT339,
//            DK.LEDLIGHT340,
//            DK.LEDLIGHT341,
//            DK.LEDLIGHT342,
//            DK.LEDLIGHT343,
//            DK.LEDLIGHT344,
//            DK.LEDLIGHT345,
//            DK.LEDLIGHT346,
//            DK.LEDLIGHT347,
//            DK.LEDLIGHT348,
//            DK.LEDLIGHT349,
//            DK.LEDLIGHT350,
//            DK.LEDLIGHT351,
//            DK.LEDLIGHT352,
//            DK.LEDLIGHT353,
//            DK.LEDLIGHT354,
//            DK.LEDLIGHT355,
//            DK.LEDLIGHT356,
//            DK.LEDLIGHT357,
//            DK.LEDLIGHT358,
//            DK.LEDLIGHT359,
//            DK.LEDLIGHT360,
//            DK.LEDLIGHT361,
//            DK.LEDLIGHT362,
//            DK.LEDLIGHT363,
//            DK.LEDLIGHT364,
//            DK.LEDLIGHT365,
//            DK.LEDLIGHT366,
//            DK.LEDLIGHT367,
//            DK.LEDLIGHT368,
//            DK.LEDLIGHT369,
//            DK.LEDLIGHT370,
//            DK.LEDLIGHT371,
//            DK.LEDLIGHT372,
//            DK.LEDLIGHT373,
//            DK.LEDLIGHT374,
//            DK.LEDLIGHT375,
//            DK.LEDLIGHT376,
//            DK.LEDLIGHT377,
//            DK.LEDLIGHT378,
//            DK.LEDLIGHT379,
//            DK.LEDLIGHT380,
//            DK.LEDLIGHT381,
//            DK.LEDLIGHT382,
//            DK.LEDLIGHT383,
//            DK.LEDLIGHT384,
//            DK.LEDLIGHT385,
//            DK.LEDLIGHT386,
//            DK.LEDLIGHT387,
//            DK.LEDLIGHT388,
//            DK.LEDLIGHT389,
//            DK.LEDLIGHT390,
//            DK.LEDLIGHT391,
//            DK.LEDLIGHT392,
//            DK.LEDLIGHT393,
//            DK.LEDLIGHT394,
//            DK.LEDLIGHT395,
//            DK.LEDLIGHT396,
//            DK.LEDLIGHT397,
//            DK.LEDLIGHT398,
//            DK.LEDLIGHT399,
//            DK.LEDLIGHT400,
//            // More than 400 Leds
//            DK.LEDLIGHT401,
//            DK.LEDLIGHT402,
//            DK.LEDLIGHT403,
//            DK.LEDLIGHT404,
//            DK.LEDLIGHT405,
//            DK.LEDLIGHT406,
//            DK.LEDLIGHT407,
//            DK.LEDLIGHT408,
//            DK.LEDLIGHT409,
//            DK.LEDLIGHT410,
//            DK.LEDLIGHT411,
//            DK.LEDLIGHT412,
//            DK.LEDLIGHT413,
//            DK.LEDLIGHT414,
//            DK.LEDLIGHT415,
//            DK.LEDLIGHT416,
//            DK.LEDLIGHT417,
//            DK.LEDLIGHT418,
//            DK.LEDLIGHT419,
//            DK.LEDLIGHT420,
//            DK.LEDLIGHT421,
//            DK.LEDLIGHT422,
//            DK.LEDLIGHT423,
//            DK.LEDLIGHT424,
//            DK.LEDLIGHT425,
//            DK.LEDLIGHT426,
//            DK.LEDLIGHT427,
//            DK.LEDLIGHT428,
//            DK.LEDLIGHT429,
//            DK.LEDLIGHT430,
//            DK.LEDLIGHT431,
//            DK.LEDLIGHT432,
//            DK.LEDLIGHT433,
//            DK.LEDLIGHT434,
//            DK.LEDLIGHT435,
//            DK.LEDLIGHT436,
//            DK.LEDLIGHT437,
//            DK.LEDLIGHT438,
//            DK.LEDLIGHT439,
//            DK.LEDLIGHT440,
//            DK.LEDLIGHT441,
//            DK.LEDLIGHT442,
//            DK.LEDLIGHT443,
//            DK.LEDLIGHT444,
//            DK.LEDLIGHT445,
//            DK.LEDLIGHT446,
//            DK.LEDLIGHT447,
//            DK.LEDLIGHT448,
//            DK.LEDLIGHT449,
//            DK.LEDLIGHT450,
//            DK.LEDLIGHT451,
//            DK.LEDLIGHT452,
//            DK.LEDLIGHT453,
//            DK.LEDLIGHT454,
//            DK.LEDLIGHT455,
//            DK.LEDLIGHT456,
//            DK.LEDLIGHT457,
//            DK.LEDLIGHT458,
//            DK.LEDLIGHT459,
//            DK.LEDLIGHT460,
//            DK.LEDLIGHT461,
//            DK.LEDLIGHT462,
//            DK.LEDLIGHT463,
//            DK.LEDLIGHT464,
//            DK.LEDLIGHT465,
//            DK.LEDLIGHT466,
//            DK.LEDLIGHT467,
//            DK.LEDLIGHT468,
//            DK.LEDLIGHT469,
//            DK.LEDLIGHT470,
//            DK.LEDLIGHT471,
//            DK.LEDLIGHT472,
//            DK.LEDLIGHT473,
//            DK.LEDLIGHT474,
//            DK.LEDLIGHT475,
//            DK.LEDLIGHT476,
//            DK.LEDLIGHT477,
//            DK.LEDLIGHT478,
//            DK.LEDLIGHT479,
//            DK.LEDLIGHT480,
//            DK.LEDLIGHT481,
//            DK.LEDLIGHT482,
//            DK.LEDLIGHT483,
//            DK.LEDLIGHT484,
//            DK.LEDLIGHT485,
//            DK.LEDLIGHT486,
//            DK.LEDLIGHT487,
//            DK.LEDLIGHT488,
//            DK.LEDLIGHT489,
//            DK.LEDLIGHT490,
//            DK.LEDLIGHT491,
//            DK.LEDLIGHT492,
//            DK.LEDLIGHT493,
//            DK.LEDLIGHT494,
//            DK.LEDLIGHT495,
//            DK.LEDLIGHT496,
//            DK.LEDLIGHT497,
//            DK.LEDLIGHT498,
//            DK.LEDLIGHT499,
//            DK.LEDLIGHT500,
//            // More than 500 Leds
//            DK.LEDLIGHT501,
//            DK.LEDLIGHT502,
//            DK.LEDLIGHT503,
//            DK.LEDLIGHT504,
//            DK.LEDLIGHT505,
//            DK.LEDLIGHT506,
//            DK.LEDLIGHT507,
//            DK.LEDLIGHT508,
//            DK.LEDLIGHT509,
//            DK.LEDLIGHT510,
//            DK.LEDLIGHT511,
//            DK.LEDLIGHT512,
//            DK.LEDLIGHT513,
//            DK.LEDLIGHT514,
//            DK.LEDLIGHT515,
//            DK.LEDLIGHT516,
//            DK.LEDLIGHT517,
//            DK.LEDLIGHT518,
//            DK.LEDLIGHT519,
//            DK.LEDLIGHT520,
//            DK.LEDLIGHT521,
//            DK.LEDLIGHT522,
//            DK.LEDLIGHT523,
//            DK.LEDLIGHT524,
//            DK.LEDLIGHT525,
//            DK.LEDLIGHT526,
//            DK.LEDLIGHT527,
//            DK.LEDLIGHT528,
//            DK.LEDLIGHT529,
//            DK.LEDLIGHT530,
//            DK.LEDLIGHT531,
//            DK.LEDLIGHT532,
//            DK.LEDLIGHT533,
//            DK.LEDLIGHT534,
//            DK.LEDLIGHT535,
//            DK.LEDLIGHT536,
//            DK.LEDLIGHT537,
//            DK.LEDLIGHT538,
//            DK.LEDLIGHT539,
//            DK.LEDLIGHT540,
//            DK.LEDLIGHT541,
//            DK.LEDLIGHT542,
//            DK.LEDLIGHT543,
//            DK.LEDLIGHT544,
//            DK.LEDLIGHT545,
//            DK.LEDLIGHT546,
//            DK.LEDLIGHT547,
//            DK.LEDLIGHT548,
//            DK.LEDLIGHT549,
//            DK.LEDLIGHT550,
//            DK.LEDLIGHT551,
//            DK.LEDLIGHT552,
//            DK.LEDLIGHT553,
//            DK.LEDLIGHT554,
//            DK.LEDLIGHT555,
//            DK.LEDLIGHT556,
//            DK.LEDLIGHT557,
//            DK.LEDLIGHT558,
//            DK.LEDLIGHT559,
//            DK.LEDLIGHT560,
//            DK.LEDLIGHT561,
//            DK.LEDLIGHT562,
//            DK.LEDLIGHT563,
//            DK.LEDLIGHT564,
//            DK.LEDLIGHT565,
//            DK.LEDLIGHT566,
//            DK.LEDLIGHT567,
//            DK.LEDLIGHT568,
//            DK.LEDLIGHT569,
//            DK.LEDLIGHT570,
//            DK.LEDLIGHT571,
//            DK.LEDLIGHT572,
//            DK.LEDLIGHT573,
//            DK.LEDLIGHT574,
//            DK.LEDLIGHT575,
//            DK.LEDLIGHT576,
//            DK.LEDLIGHT577,
//            DK.LEDLIGHT578,
//            DK.LEDLIGHT579,
//            DK.LEDLIGHT580,
//            DK.LEDLIGHT581,
//            DK.LEDLIGHT582,
//            DK.LEDLIGHT583,
//            DK.LEDLIGHT584,
//            DK.LEDLIGHT585,
//            DK.LEDLIGHT586,
//            DK.LEDLIGHT587,
//            DK.LEDLIGHT588,
//            DK.LEDLIGHT589,
//            DK.LEDLIGHT590,
//            DK.LEDLIGHT591,
//            DK.LEDLIGHT592,
//            DK.LEDLIGHT593,
//            DK.LEDLIGHT594,
//            DK.LEDLIGHT595,
//            DK.LEDLIGHT596,
//            DK.LEDLIGHT597,
//            DK.LEDLIGHT598,
//            DK.LEDLIGHT599,
//            DK.LEDLIGHT600,
//            // More than 600 Leds
//            DK.LEDLIGHT601,
//            DK.LEDLIGHT602,
//            DK.LEDLIGHT603,
//            DK.LEDLIGHT604,
//            DK.LEDLIGHT605,
//            DK.LEDLIGHT606,
//            DK.LEDLIGHT607,
//            DK.LEDLIGHT608,
//            DK.LEDLIGHT609,
//            DK.LEDLIGHT610,
//            DK.LEDLIGHT611,
//            DK.LEDLIGHT612,
//            DK.LEDLIGHT613,
//            DK.LEDLIGHT614,
//            DK.LEDLIGHT615,
//            DK.LEDLIGHT616,
//            DK.LEDLIGHT617,
//            DK.LEDLIGHT618,
//            DK.LEDLIGHT619,
//            DK.LEDLIGHT620,
//            DK.LEDLIGHT621,
//            DK.LEDLIGHT622,
//            DK.LEDLIGHT623,
//            DK.LEDLIGHT624,
//            DK.LEDLIGHT625,
//            DK.LEDLIGHT626,
//            DK.LEDLIGHT627,
//            DK.LEDLIGHT628,
//            DK.LEDLIGHT629,
//            DK.LEDLIGHT630,
//            DK.LEDLIGHT631,
//            DK.LEDLIGHT632,
//            DK.LEDLIGHT633,
//            DK.LEDLIGHT634,
//            DK.LEDLIGHT635,
//            DK.LEDLIGHT636,
//            DK.LEDLIGHT637,
//            DK.LEDLIGHT638,
//            DK.LEDLIGHT639,
//            DK.LEDLIGHT640,
//            DK.LEDLIGHT641,
//            DK.LEDLIGHT642,
//            DK.LEDLIGHT643,
//            DK.LEDLIGHT644,
//            DK.LEDLIGHT645,
//            DK.LEDLIGHT646,
//            DK.LEDLIGHT647,
//            DK.LEDLIGHT648,
//            DK.LEDLIGHT649,
//            DK.LEDLIGHT650,
//            DK.LEDLIGHT651,
//            DK.LEDLIGHT652,
//            DK.LEDLIGHT653,
//            DK.LEDLIGHT654,
//            DK.LEDLIGHT655,
//            DK.LEDLIGHT656,
//            DK.LEDLIGHT657,
//            DK.LEDLIGHT658,
//            DK.LEDLIGHT659,
//            DK.LEDLIGHT660,
//            DK.LEDLIGHT661,
//            DK.LEDLIGHT662,
//            DK.LEDLIGHT663,
//            DK.LEDLIGHT664,
//            DK.LEDLIGHT665,
//            DK.LEDLIGHT666,
//            DK.LEDLIGHT667,
//            DK.LEDLIGHT668,
//            DK.LEDLIGHT669,
//            DK.LEDLIGHT670,
//            DK.LEDLIGHT671,
//            DK.LEDLIGHT672,
//            DK.LEDLIGHT673,
//            DK.LEDLIGHT674,
//            DK.LEDLIGHT675,
//            DK.LEDLIGHT676,
//            DK.LEDLIGHT677,
//            DK.LEDLIGHT678,
//            DK.LEDLIGHT679,
//            DK.LEDLIGHT680,
//            DK.LEDLIGHT681,
//            DK.LEDLIGHT682,
//            DK.LEDLIGHT683,
//            DK.LEDLIGHT684,
//            DK.LEDLIGHT685,
//            DK.LEDLIGHT686,
//            DK.LEDLIGHT687,
//            DK.LEDLIGHT688,
//            DK.LEDLIGHT689,
//            DK.LEDLIGHT690,
//            DK.LEDLIGHT691,
//            DK.LEDLIGHT692,
//            DK.LEDLIGHT693,
//            DK.LEDLIGHT694,
//            DK.LEDLIGHT695,
//            DK.LEDLIGHT696,
//            DK.LEDLIGHT697,
//            DK.LEDLIGHT698,
//            DK.LEDLIGHT699,
//            DK.LEDLIGHT700,
//            // More than 700 Leds
//            DK.LEDLIGHT701,
//            DK.LEDLIGHT702,
//            DK.LEDLIGHT703,
//            DK.LEDLIGHT704,
//            DK.LEDLIGHT705,
//            DK.LEDLIGHT706,
//            DK.LEDLIGHT707,
//            DK.LEDLIGHT708,
//            DK.LEDLIGHT709,
//            DK.LEDLIGHT710,
//            DK.LEDLIGHT711,
//            DK.LEDLIGHT712,
//            DK.LEDLIGHT713,
//            DK.LEDLIGHT714,
//            DK.LEDLIGHT715,
//            DK.LEDLIGHT716,
//            DK.LEDLIGHT717,
//            DK.LEDLIGHT718,
//            DK.LEDLIGHT719,
//            DK.LEDLIGHT720,
//            DK.LEDLIGHT721,
//            DK.LEDLIGHT722,
//            DK.LEDLIGHT723,
//            DK.LEDLIGHT724,
//            DK.LEDLIGHT725,
//            DK.LEDLIGHT726,
//            DK.LEDLIGHT727,
//            DK.LEDLIGHT728,
//            DK.LEDLIGHT729,
//            DK.LEDLIGHT730,
//            DK.LEDLIGHT731,
//            DK.LEDLIGHT732,
//            DK.LEDLIGHT733,
//            DK.LEDLIGHT734,
//            DK.LEDLIGHT735,
//            DK.LEDLIGHT736,
//            DK.LEDLIGHT737,
//            DK.LEDLIGHT738,
//            DK.LEDLIGHT739,
//            DK.LEDLIGHT740,
//            DK.LEDLIGHT741,
//            DK.LEDLIGHT742,
//            DK.LEDLIGHT743,
//            DK.LEDLIGHT744,
//            DK.LEDLIGHT745,
//            DK.LEDLIGHT746,
//            DK.LEDLIGHT747,
//            DK.LEDLIGHT748,
//            DK.LEDLIGHT749,
//            DK.LEDLIGHT750,
//            DK.LEDLIGHT751,
//            DK.LEDLIGHT752,
//            DK.LEDLIGHT753,
//            DK.LEDLIGHT754,
//            DK.LEDLIGHT755,
//            DK.LEDLIGHT756,
//            DK.LEDLIGHT757,
//            DK.LEDLIGHT758,
//            DK.LEDLIGHT759,
//            DK.LEDLIGHT760,
//            DK.LEDLIGHT761,
//            DK.LEDLIGHT762,
//            DK.LEDLIGHT763,
//            DK.LEDLIGHT764,
//            DK.LEDLIGHT765,
//            DK.LEDLIGHT766,
//            DK.LEDLIGHT767,
//            DK.LEDLIGHT768,
//            DK.LEDLIGHT769,
//            DK.LEDLIGHT770,
//            DK.LEDLIGHT771,
//            DK.LEDLIGHT772,
//            DK.LEDLIGHT773,
//            DK.LEDLIGHT774,
//            DK.LEDLIGHT775,
//            DK.LEDLIGHT776,
//            DK.LEDLIGHT777,
//            DK.LEDLIGHT778,
//            DK.LEDLIGHT779,
//            DK.LEDLIGHT780,
//            DK.LEDLIGHT781,
//            DK.LEDLIGHT782,
//            DK.LEDLIGHT783,
//            DK.LEDLIGHT784,
//            DK.LEDLIGHT785,
//            DK.LEDLIGHT786,
//            DK.LEDLIGHT787,
//            DK.LEDLIGHT788,
//            DK.LEDLIGHT789,
//            DK.LEDLIGHT790,
//            DK.LEDLIGHT791,
//            DK.LEDLIGHT792,
//            DK.LEDLIGHT793,
//            DK.LEDLIGHT794,
//            DK.LEDLIGHT795,
//            DK.LEDLIGHT796,
//            DK.LEDLIGHT797,
//            DK.LEDLIGHT798,
//            DK.LEDLIGHT799,
//            DK.LEDLIGHT800,
//            // More than 800 Leds
//            DK.LEDLIGHT801,
//            DK.LEDLIGHT802,
//            DK.LEDLIGHT803,
//            DK.LEDLIGHT804,
//            DK.LEDLIGHT805,
//            DK.LEDLIGHT806,
//            DK.LEDLIGHT807,
//            DK.LEDLIGHT808,
//            DK.LEDLIGHT809,
//            DK.LEDLIGHT810,
//            DK.LEDLIGHT811,
//            DK.LEDLIGHT812,
//            DK.LEDLIGHT813,
//            DK.LEDLIGHT814,
//            DK.LEDLIGHT815,
//            DK.LEDLIGHT816,
//            DK.LEDLIGHT817,
//            DK.LEDLIGHT818,
//            DK.LEDLIGHT819,
//            DK.LEDLIGHT820,
//            DK.LEDLIGHT821,
//            DK.LEDLIGHT822,
//            DK.LEDLIGHT823,
//            DK.LEDLIGHT824,
//            DK.LEDLIGHT825,
//            DK.LEDLIGHT826,
//            DK.LEDLIGHT827,
//            DK.LEDLIGHT828,
//            DK.LEDLIGHT829,
//            DK.LEDLIGHT830,
//            DK.LEDLIGHT831,
//            DK.LEDLIGHT832,
//            DK.LEDLIGHT833,
//            DK.LEDLIGHT834,
//            DK.LEDLIGHT835,
//            DK.LEDLIGHT836,
//            DK.LEDLIGHT837,
//            DK.LEDLIGHT838,
//            DK.LEDLIGHT839,
//            DK.LEDLIGHT840,
//            DK.LEDLIGHT841,
//            DK.LEDLIGHT842,
//            DK.LEDLIGHT843,
//            DK.LEDLIGHT844,
//            DK.LEDLIGHT845,
//            DK.LEDLIGHT846,
//            DK.LEDLIGHT847,
//            DK.LEDLIGHT848,
//            DK.LEDLIGHT849,
//            DK.LEDLIGHT850,
//            DK.LEDLIGHT851,
//            DK.LEDLIGHT852,
//            DK.LEDLIGHT853,
//            DK.LEDLIGHT854,
//            DK.LEDLIGHT855,
//            DK.LEDLIGHT856,
//            DK.LEDLIGHT857,
//            DK.LEDLIGHT858,
//            DK.LEDLIGHT859,
//            DK.LEDLIGHT860,
//            DK.LEDLIGHT861,
//            DK.LEDLIGHT862,
//            DK.LEDLIGHT863,
//            DK.LEDLIGHT864,
//            DK.LEDLIGHT865,
//            DK.LEDLIGHT866,
//            DK.LEDLIGHT867,
//            DK.LEDLIGHT868,
//            DK.LEDLIGHT869,
//            DK.LEDLIGHT870,
//            DK.LEDLIGHT871,
//            DK.LEDLIGHT872,
//            DK.LEDLIGHT873,
//            DK.LEDLIGHT874,
//            DK.LEDLIGHT875,
//            DK.LEDLIGHT876,
//            DK.LEDLIGHT877,
//            DK.LEDLIGHT878,
//            DK.LEDLIGHT879,
//            DK.LEDLIGHT880,
//            DK.LEDLIGHT881,
//            DK.LEDLIGHT882,
//            DK.LEDLIGHT883,
//            DK.LEDLIGHT884,
//            DK.LEDLIGHT885,
//            DK.LEDLIGHT886,
//            DK.LEDLIGHT887,
//            DK.LEDLIGHT888,
//            DK.LEDLIGHT889,
//            DK.LEDLIGHT890,
//            DK.LEDLIGHT891,
//            DK.LEDLIGHT892,
//            DK.LEDLIGHT893,
//            DK.LEDLIGHT894,
//            DK.LEDLIGHT895,
//            DK.LEDLIGHT896,
//            DK.LEDLIGHT897,
//            DK.LEDLIGHT898,
//            DK.LEDLIGHT899,
//            DK.LEDLIGHT900,
//            // More than 900 Leds
//            DK.LEDLIGHT901,
//            DK.LEDLIGHT902,
//            DK.LEDLIGHT903,
//            DK.LEDLIGHT904,
//            DK.LEDLIGHT905,
//            DK.LEDLIGHT906,
//            DK.LEDLIGHT907,
//            DK.LEDLIGHT908,
//            DK.LEDLIGHT909,
//            DK.LEDLIGHT910,
//            DK.LEDLIGHT911,
//            DK.LEDLIGHT912,
//            DK.LEDLIGHT913,
//            DK.LEDLIGHT914,
//            DK.LEDLIGHT915,
//            DK.LEDLIGHT916,
//            DK.LEDLIGHT917,
//            DK.LEDLIGHT918,
//            DK.LEDLIGHT919,
//            DK.LEDLIGHT920,
//            DK.LEDLIGHT921,
//            DK.LEDLIGHT922,
//            DK.LEDLIGHT923,
//            DK.LEDLIGHT924,
//            DK.LEDLIGHT925,
//            DK.LEDLIGHT926,
//            DK.LEDLIGHT927,
//            DK.LEDLIGHT928,
//            DK.LEDLIGHT929,
//            DK.LEDLIGHT930,
//            DK.LEDLIGHT931,
//            DK.LEDLIGHT932,
//            DK.LEDLIGHT933,
//            DK.LEDLIGHT934,
//            DK.LEDLIGHT935,
//            DK.LEDLIGHT936,
//            DK.LEDLIGHT937,
//            DK.LEDLIGHT938,
//            DK.LEDLIGHT939,
//            DK.LEDLIGHT940,
//            DK.LEDLIGHT941,
//            DK.LEDLIGHT942,
//            DK.LEDLIGHT943,
//            DK.LEDLIGHT944,
//            DK.LEDLIGHT945,
//            DK.LEDLIGHT946,
//            DK.LEDLIGHT947,
//            DK.LEDLIGHT948,
//            DK.LEDLIGHT949,
//            DK.LEDLIGHT950,
//            DK.LEDLIGHT951,
//            DK.LEDLIGHT952,
//            DK.LEDLIGHT953,
//            DK.LEDLIGHT954,
//            DK.LEDLIGHT955,
//            DK.LEDLIGHT956,
//            DK.LEDLIGHT957,
//            DK.LEDLIGHT958,
//            DK.LEDLIGHT959,
//            DK.LEDLIGHT960,
//            DK.LEDLIGHT961,
//            DK.LEDLIGHT962,
//            DK.LEDLIGHT963,
//            DK.LEDLIGHT964,
//            DK.LEDLIGHT965,
//            DK.LEDLIGHT966,
//            DK.LEDLIGHT967,
//            DK.LEDLIGHT968,
//            DK.LEDLIGHT969,
//            DK.LEDLIGHT970,
//            DK.LEDLIGHT971,
//            DK.LEDLIGHT972,
//            DK.LEDLIGHT973,
//            DK.LEDLIGHT974,
//            DK.LEDLIGHT975,
//            DK.LEDLIGHT976,
//            DK.LEDLIGHT977,
//            DK.LEDLIGHT978,
//            DK.LEDLIGHT979,
//            DK.LEDLIGHT980,
//            DK.LEDLIGHT981,
//            DK.LEDLIGHT982,
//            DK.LEDLIGHT983,
//            DK.LEDLIGHT984,
//            DK.LEDLIGHT985,
//            DK.LEDLIGHT986,
//            DK.LEDLIGHT987,
//            DK.LEDLIGHT988,
//            DK.LEDLIGHT989,
//            DK.LEDLIGHT990,
//            DK.LEDLIGHT991,
//            DK.LEDLIGHT992,
//            DK.LEDLIGHT993,
//            DK.LEDLIGHT994,
//            DK.LEDLIGHT995,
//            DK.LEDLIGHT996,
//            DK.LEDLIGHT997,
//            DK.LEDLIGHT998,
//            DK.LEDLIGHT999,
//            DK.LEDLIGHT1000,
            // Finally 1000 Leds
        });
    }
}
