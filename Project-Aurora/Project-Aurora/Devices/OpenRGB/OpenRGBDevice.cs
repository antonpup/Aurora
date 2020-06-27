using Aurora.Settings;
using OpenRGB.NET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DK = Aurora.Devices.DeviceKeys;

namespace Aurora.Devices.OpenRGB
{
    class OpenRGBAuroraDevice : Device
    {
        private string devicename = "OpenRGB";
        VariableRegistry varReg;
        bool isInitialized = false;
        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;

        OpenRGBClient client = new OpenRGBClient("localhost", 6742, "Aurora\0");
        List<OpenRGBDevice> controllers;
        List<OpenRGBColor[]> colors;

        public string GetDeviceDetails()
        {
            if (isInitialized)
            {
                string devString = devicename + ": ";
                devString += "Connected ";
                var names = controllers.Select(c => c.name);
                devString += string.Join(",", names);
                return devString;
            }
            else
            {
                return devicename + ": Not initialized";
            }
        }

        public string GetDeviceName()
        {
            return devicename;
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
                varReg.Register($"{devicename}_sleep", 25, "Sleep for", 1000, 0);
                varReg.Register($"{devicename}_generic", false, "Set colors on generic devices");
            }
            return varReg;
        }

        public bool Initialize()
        {
            if (isInitialized)
                return true;

            try
            {
                client.Connect();
            }
            catch (Exception e)
            {
                Global.logger.Error("error in OpenRGB device: " + e);
                isInitialized = false;
                return false;
            }

            var controllerCount = client.GetControllerCount();
            controllers = new List<OpenRGBDevice>();
            colors = new List<OpenRGBColor[]>();

            for (var i = 0; i < controllerCount; i++)
            {
                var dev = client.GetControllerData(i);
                controllers.Add(dev);
                var array = new OpenRGBColor[dev.colors.Length];
                for (var j = 0; j < dev.colors.Length; j++)
                    array[j] = new OpenRGBColor();
                colors.Add(array);
            }

            isInitialized = true;
            return isInitialized;
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

        public void Shutdown()
        {
            if (!isInitialized)
                return;

            for (var i = 0; i < controllers.Count; i++)
            {
                client.UpdateLeds(i, controllers[i].colors);
            }

            client.Disconnect();
            isInitialized = false;
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (!isInitialized)
                return false;

            for (var i = 0; i < controllers.Count; i++)
            {
                switch (controllers[i].name)
                {
                    case G810:
                        foreach (var kc in keyColors)
                        {
                            if (G810Dict.TryGetValue(kc.Key, out var index))
                            {
                                colors[i][index] = new OpenRGBColor(kc.Value.R, kc.Value.G, kc.Value.B);
                            }
                        }
                        break;
                    case AlloyElite:
                        foreach (var kc in keyColors)
                        {
                            if (kc.Key == DK.ENTER)
                            {
                                //hack

                                //ansi
                                if (Global.Configuration.keyboard_localization == PreferredKeyboardLocalization.us)
                                {
                                    colors[i][28] = new OpenRGBColor(kc.Value.R, kc.Value.G, kc.Value.B);
                                }
                                else//iso
                                {
                                    colors[i][14] = new OpenRGBColor(kc.Value.R, kc.Value.G, kc.Value.B);
                                }
                            }
                            else if (AlloyEliteRGBISODict.TryGetValue(kc.Key, out var index))
                            {
                                colors[i][index] = new OpenRGBColor(kc.Value.R, kc.Value.G, kc.Value.B);
                            }
                        }
                        break;
                    default:
                        if (!Global.Configuration.VarRegistry.GetVariable<bool>($"{devicename}_generic"))
                            continue;
                        if (keyColors.TryGetValue(DK.Peripheral_Logo, out var color))
                        {
                            for (int j = 0; j < colors[i].Length; j++)
                            {
                                colors[i][j] = new OpenRGBColor(color.R, color.G, color.B);
                            }
                        }
                        break;
                }

                client.UpdateLeds(i, colors[i]);
            }
            var sleep = Global.Configuration.VarRegistry.GetVariable<int>($"{devicename}_sleep");
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

        private readonly Dictionary<DK, int> G810Dict = new Dictionary<DK, int>()
        {
            { DK.A                 , 0   },
            { DK.B                 , 1   },
            { DK.C                 , 2   },
            { DK.D                 , 3   },
            { DK.E                 , 4   },
            { DK.F                 , 5   },
            { DK.G                 , 6   },
            { DK.H                 , 7   },
            { DK.I                 , 8   },
            { DK.J                 , 9   },
            { DK.K                 , 10  },
            { DK.L                 , 11  },
            { DK.M                 , 12  },
            { DK.N                 , 13  },
            { DK.O                 , 14  },
            { DK.P                 , 15  },
            { DK.Q                 , 16  },
            { DK.R                 , 17  },
            { DK.S                 , 18  },
            { DK.T                 , 19  },
            { DK.U                 , 20  },
            { DK.V                 , 21  },
            { DK.W                 , 22  },
            { DK.X                 , 23  },
            { DK.Y                 , 24  },
            { DK.Z                 , 25  },
            { DK.ONE               , 26  },
            { DK.TWO               , 27  },
            { DK.THREE             , 28  },
            { DK.FOUR              , 29  },
            { DK.FIVE              , 30  },
            { DK.SIX               , 31  },
            { DK.SEVEN             , 32  },
            { DK.EIGHT             , 33  },
            { DK.NINE              , 34  },
            { DK.ZERO              , 35  },
            { DK.ENTER             , 36  },
            { DK.ESC               , 37  },
            { DK.BACKSPACE         , 38  },
            { DK.TAB               , 39  },
            { DK.SPACE             , 40  },
            { DK.MINUS             , 41  },
            { DK.EQUALS            , 42  },
            { DK.OPEN_BRACKET      , 43  },
            { DK.CLOSE_BRACKET     , 44  },
            { DK.BACKSLASH         , 45  },
            { DK.HASHTAG           , 46  },
            { DK.SEMICOLON         , 47  },
            { DK.APOSTROPHE        , 48  },
            { DK.TILDE             , 49  },
            { DK.COMMA             , 50  },
            { DK.PERIOD            , 51  },
            { DK.FORWARD_SLASH     , 52  },
            { DK.CAPS_LOCK         , 53  },
            { DK.F1                , 54  },
            { DK.F2                , 55  },
            { DK.F3                , 56  },
            { DK.F4                , 57  },
            { DK.F5                , 58  },
            { DK.F6                , 59  },
            { DK.F7                , 60  },
            { DK.F8                , 61  },
            { DK.F9                , 62  },
            { DK.F10               , 63  },
            { DK.F11               , 64  },
            { DK.F12               , 65  },
            { DK.PRINT_SCREEN      , 66  },
            { DK.SCROLL_LOCK       , 67  },
            { DK.PAUSE_BREAK       , 68  },
            { DK.INSERT            , 69  },
            { DK.HOME              , 70  },
            { DK.PAGE_UP           , 71  },
            { DK.DELETE            , 72  },
            { DK.END               , 73  },
            { DK.PAGE_DOWN         , 74  },
            { DK.ARROW_RIGHT       , 75  },
            { DK.ARROW_LEFT        , 76  },
            { DK.ARROW_DOWN        , 77  },
            { DK.ARROW_UP          , 78  },
            { DK.NUM_LOCK          , 79  },
            { DK.NUM_SLASH         , 80  },
            { DK.NUM_ASTERISK      , 81  },
            { DK.NUM_MINUS         , 82  },
            { DK.NUM_PLUS          , 83  },
            { DK.NUM_ENTER         , 84  },
            { DK.NUM_ONE           , 85  },
            { DK.NUM_TWO           , 86  },
            { DK.NUM_THREE         , 87  },
            { DK.NUM_FOUR          , 88  },
            { DK.NUM_FIVE          , 89  },
            { DK.NUM_SIX           , 90  },
            { DK.NUM_SEVEN         , 91  },
            { DK.NUM_EIGHT         , 92  },
            { DK.NUM_NINE          , 93  },
            { DK.NUM_ZERO          , 94  },
            { DK.NUM_PERIOD        , 95  },
            { DK.BACKSLASH_UK      , 96  },
            { DK.APPLICATION_SELECT, 97  },
            { DK.LEFT_CONTROL      , 98  },
            { DK.LEFT_SHIFT        , 99  },
            { DK.LEFT_ALT          , 100 },
            { DK.LEFT_WINDOWS      , 101 },
            { DK.RIGHT_CONTROL     , 102 },
            { DK.RIGHT_SHIFT       , 103 },
            { DK.RIGHT_ALT         , 104 },
            { DK.RIGHT_WINDOWS     , 105 },
            { DK.MEDIA_NEXT        , 106 },
            { DK.MEDIA_PREVIOUS    , 107 },
            { DK.MEDIA_STOP        , 108 },
            { DK.MEDIA_PLAY        , 109 },
            { DK.VOLUME_MUTE       , 110 },
            { DK.LOGO              , 111 },
            { DK.BRIGHTNESS_SWITCH , 112 },
            { DK.G0                , 113 },
            { DK.G1                , 114 },
            { DK.G2                , 115 },
            { DK.G3                , 116 },
        };
        private const string G810 = "Logitech G810 Orion Spectrum";

        //ISO Dictionary, i don't have an ANSI keyboard lol
        private readonly Dictionary<DK, int> AlloyEliteRGBISODict = new Dictionary<DK, int>()
        {
            { DK.A                 , 19  },
            { DK.B                 , 79  },
            { DK.C                 , 58  },
            { DK.D                 , 47  },
            { DK.E                 , 46  },
            { DK.F                 , 57  },
            { DK.G                 , 68  },
            { DK.H                 , 78  },
            { DK.I                 , 97  },
            { DK.J                 , 87  },
            { DK.K                 , 98  },
            { DK.L                 , 11  },
            { DK.M                 , 99  },
            { DK.N                 , 88  },
            { DK.O                 , 10  },
            { DK.P                 , 25  },
            { DK.Q                 , 18  },
            { DK.R                 , 56  },
            { DK.S                 , 33  },
            { DK.T                 , 67  },
            { DK.U                 , 86  },
            { DK.V                 , 69  },
            { DK.W                 , 32  },
            { DK.X                 , 48  },
            { DK.Y                 , 77  },
            { DK.Z                 , 34  },
            { DK.ONE               , 17  },
            { DK.TWO               , 31  },
            { DK.THREE             , 45  },
            { DK.FOUR              , 55  },
            { DK.FIVE              , 66  },
            { DK.SIX               , 76  },
            { DK.SEVEN             , 85  },
            { DK.EIGHT             , 96  },
            { DK.NINE              , 9   },
            { DK.ZERO              , 24  },

            //{ DK.ENTER             , 14 },

            { DK.ESC               , 0   },
            { DK.BACKSPACE         , 37  },
            { DK.TAB               , 2   },
            { DK.SPACE             , 59  },
            { DK.MINUS             , 39  },
            { DK.EQUALS            , 7   },
            { DK.OPEN_BRACKET      , 40  },
            { DK.CLOSE_BRACKET     , 90  },
            { DK.BACKSLASH         , 101  },
            { DK.HASHTAG           , 28  },
            { DK.SEMICOLON         , 26  },
            { DK.APOSTROPHE        , 41  },
            { DK.TILDE             , 1   },
            { DK.COMMA             , 12  },
            { DK.PERIOD            , 27  },
            { DK.FORWARD_SLASH     , 42  },
            { DK.CAPS_LOCK         , 3   },
            { DK.F1                , 16  },
            { DK.F2                , 30  },
            { DK.F3                , 44  },
            { DK.F4                , 54  },
            { DK.F5                , 65  },
            { DK.F6                , 75  },
            { DK.F7                , 84  },
            { DK.F8                , 95  },
            { DK.F9                , 8   },
            { DK.F10               , 23  },
            { DK.F11               , 38  },
            { DK.F12               , 6   },
            { DK.PRINT_SCREEN      , 22  },
            { DK.SCROLL_LOCK       , 36  },
            { DK.PAUSE_BREAK       , 49  },
            { DK.INSERT            , 60  },
            { DK.HOME              , 70  },
            { DK.PAGE_UP           , 80  },
            { DK.DELETE            , 50  },
            { DK.END               , 61  },
            { DK.PAGE_DOWN         , 71  },
            { DK.ARROW_RIGHT       , 43  },
            { DK.ARROW_LEFT        , 15  },
            { DK.ARROW_DOWN        , 29  },
            { DK.ARROW_UP          , 102 },
            { DK.NUM_LOCK          , 52  },
            { DK.NUM_SLASH         , 63  },
            { DK.NUM_ASTERISK      , 73  },
            { DK.NUM_MINUS         , 82  },
            { DK.NUM_PLUS          , 93  },
            { DK.NUM_ENTER         , 104 },
            { DK.NUM_ONE           , 64  },
            { DK.NUM_TWO           , 74  },
            { DK.NUM_THREE         , 83  },
            { DK.NUM_FOUR          , 92  },
            { DK.NUM_FIVE          , 103 },
            { DK.NUM_SIX           , 53  },
            { DK.NUM_SEVEN         , 51  },
            { DK.NUM_EIGHT         , 62  },
            { DK.NUM_NINE          , 72  },
            { DK.NUM_ZERO          , 94  },
            { DK.NUM_PERIOD        , 105 },

            { DK.BACKSLASH_UK      , 20  },

            { DK.APPLICATION_SELECT, 13  },
            { DK.FN_Key            , 13  },
            { DK.LEFT_CONTROL      , 5   },
            { DK.LEFT_SHIFT        , 4   },
            { DK.LEFT_ALT          , 35  },

            { DK.LEFT_WINDOWS      , 21  },

            { DK.RIGHT_CONTROL     , 91  },
            { DK.RIGHT_SHIFT       , 81  },
            { DK.RIGHT_ALT         , 89  },
            { DK.RIGHT_WINDOWS     , 100 },

            { DK.ADDITIONALLIGHT1  , 106 },
            { DK.ADDITIONALLIGHT2  , 107 },
            { DK.ADDITIONALLIGHT3  , 108 },
            { DK.ADDITIONALLIGHT4  , 109 },
            { DK.ADDITIONALLIGHT5  , 110 },
            { DK.ADDITIONALLIGHT6  , 111 },
            { DK.ADDITIONALLIGHT7  , 112 },
            { DK.ADDITIONALLIGHT8  , 113 },
            { DK.ADDITIONALLIGHT9  , 114 },
            { DK.ADDITIONALLIGHT10 , 115 },
            { DK.ADDITIONALLIGHT11 , 116 },
            { DK.ADDITIONALLIGHT12 , 117 },
            { DK.ADDITIONALLIGHT13 , 118 },
            { DK.ADDITIONALLIGHT14 , 119 },
            { DK.ADDITIONALLIGHT15 , 120 },
            { DK.ADDITIONALLIGHT16 , 121 },
            { DK.ADDITIONALLIGHT17 , 122 },
            { DK.ADDITIONALLIGHT18 , 123 },

            { DK.MEDIA_PREVIOUS    , 124 },
            { DK.MEDIA_PLAY_PAUSE  , 125 },
            { DK.MEDIA_NEXT        , 126 },
            { DK.VOLUME_MUTE       , 127 },
        };
        private const string AlloyElite = "HyperX Alloy Elite RGB";
    }
}
