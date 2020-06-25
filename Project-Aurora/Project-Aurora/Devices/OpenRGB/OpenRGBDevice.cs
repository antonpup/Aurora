using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenRGB.NET;
using System.Threading;
using DK = Aurora.Devices.DeviceKeys;

namespace Aurora.Devices.OpenRGB
{
    class OpenRGBAuroraDevice : Device
    {
        VariableRegistry varReg = new VariableRegistry();
        bool isInitialized = false;
        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;
        OpenRGBClient client = new OpenRGBClient("localhost", 1338, "Aurora\0");
        uint deviceIndex;
        OpenRGBColor[] colors;
        private string devicename = "OpenRGB";

        public string GetDeviceDetails()
        {
            if (isInitialized)
            {
                string devString = devicename + ": ";
                devString += "Connected";
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
            return varReg;
        }

        public bool Initialize()
        {
            client.Connect();
            var controllerCount = client.GetControllerCount();
            var devices = new List<OpenRGBDevice>();

            for (uint i = 0; i < controllerCount; i++)
                devices.Add(client.GetControllerData(i));

            deviceIndex = (uint)devices.FindIndex(d => d.name.Contains("HyperX"));
            var data = devices[(int)deviceIndex];
            colors = new OpenRGBColor[data.leds.Length];
            for (int i = 0; i < data.leds.Length; i++)
                colors[i] = new OpenRGBColor();
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
            for (int i = 0; i < colors.Length; i++)
                colors[i] = new OpenRGBColor(255, 255, 255);
            client.UpdateLeds(deviceIndex, colors);

            client.Disconnect();
            isInitialized = false;
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            foreach (var dk in keyColors)
            {
                if (AlloyEliteRGBISODict.TryGetValue(dk.Key, out var idx))
                {
                    colors[idx] = new OpenRGBColor(dk.Value.R, dk.Value.G, dk.Value.B);
                }
            }

            client.UpdateLeds(deviceIndex, colors);
            Thread.Sleep(25);
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


        //ISO Dictionary, i don't have an ANSI keyboard lol
        private readonly Dictionary<DK, int> AlloyEliteRGBISODict = new Dictionary<DK, int>()
        {
            { DK.A                 , 18  },
            { DK.B                 , 77  },
            { DK.C                 , 56  },
            { DK.D                 , 45  },
            { DK.E                 , 44  },
            { DK.F                 , 55  },
            { DK.G                 , 66  },
            { DK.H                 , 76  },
            { DK.I                 , 95  },
            { DK.J                 , 85  },
            { DK.K                 , 96  },
            { DK.L                 , 11  },
            { DK.M                 , 97  },
            { DK.N                 , 86  },
            { DK.O                 , 10  },
            { DK.P                 , 23  },
            { DK.Q                 , 17  },
            { DK.R                 , 54  },
            { DK.S                 , 31  },
            { DK.T                 , 65  },
            { DK.U                 , 84  },
            { DK.V                 , 67  },
            { DK.W                 , 30  },
            { DK.X                 , 46  },
            { DK.Y                 , 75  },
            { DK.Z                 , 32  },
            { DK.ONE               , 16  },
            { DK.TWO               , 29  },
            { DK.THREE             , 43  },
            { DK.FOUR              , 53  },
            { DK.FIVE              , 64  },
            { DK.SIX               , 74  },
            { DK.SEVEN             , 83  },
            { DK.EIGHT             , 94  },
            { DK.NINE              , 9   },
            { DK.ZERO              , 22  },
            { DK.ENTER             , 99  },
            { DK.ESC               , 0   },
            { DK.BACKSPACE         , 35  },
            { DK.TAB               , 2   },
            { DK.SPACE             , 57  },
            { DK.MINUS             , 37  },
            { DK.EQUALS            , 7   },
            { DK.OPEN_BRACKET      , 38  },
            { DK.CLOSE_BRACKET     , 88  },
            { DK.BACKSLASH         , 45  },
            { DK.HASHTAG           , 26  },
            { DK.SEMICOLON         , 24  },
            { DK.APOSTROPHE        , 39  },
            { DK.TILDE             , 1   },
            { DK.COMMA             , 12  },
            { DK.PERIOD            , 25  },
            { DK.FORWARD_SLASH     , 40  },
            { DK.CAPS_LOCK         , 3   },
            { DK.F1                , 15  },
            { DK.F2                , 28  },
            { DK.F3                , 42  },
            { DK.F4                , 52  },
            { DK.F5                , 63  },
            { DK.F6                , 73  },
            { DK.F7                , 82  },
            { DK.F8                , 93  },
            { DK.F9                , 8   },
            { DK.F10               , 21  },
            { DK.F11               , 36  },
            { DK.F12               , 6   },
            { DK.PRINT_SCREEN      , 20  },
            { DK.SCROLL_LOCK       , 34  },
            { DK.PAUSE_BREAK       , 47  },
            { DK.INSERT            , 58  },
            { DK.HOME              , 68  },
            { DK.PAGE_UP           , 78  },
            { DK.DELETE            , 48  },
            { DK.END               , 59  },
            { DK.PAGE_DOWN         , 69  },
            { DK.ARROW_RIGHT       , 41  },
            { DK.ARROW_LEFT        , 14  },
            { DK.ARROW_DOWN        , 27  },
            { DK.ARROW_UP          , 100 },
            { DK.NUM_LOCK          , 50  },
            { DK.NUM_SLASH         , 61  },
            { DK.NUM_ASTERISK      , 71  },
            { DK.NUM_MINUS         , 80  },
            { DK.NUM_PLUS          , 91  },
            { DK.NUM_ENTER         , 102 },
            { DK.NUM_ONE           , 62  },
            { DK.NUM_TWO           , 72  },
            { DK.NUM_THREE         , 81  },
            { DK.NUM_FOUR          , 90  },
            { DK.NUM_FIVE          , 101 },
            { DK.NUM_SIX           , 51  },
            { DK.NUM_SEVEN         , 49  },
            { DK.NUM_EIGHT         , 60  },
            { DK.NUM_NINE          , 70  },
            { DK.NUM_ZERO          , 92  },
            { DK.NUM_PERIOD        , 103 },
            { DK.BACKSLASH_UK      , 104 },
            { DK.APPLICATION_SELECT, 13  },
            { DK.FN_Key            , 13  },
            { DK.LEFT_CONTROL      , 5   },
            { DK.LEFT_SHIFT        , 4   },
            { DK.LEFT_ALT          , 33  },
            { DK.LEFT_WINDOWS      , 19  },
            { DK.RIGHT_CONTROL     , 89  },
            { DK.RIGHT_SHIFT       , 79  },
            { DK.RIGHT_ALT         , 87  },
            { DK.RIGHT_WINDOWS     , 98  },
            { DK.MEDIA_NEXT        , 106 },
            { DK.MEDIA_PREVIOUS    , 107 },
            { DK.MEDIA_STOP        , 108 },
            { DK.MEDIA_PLAY        , 109 },
            { DK.VOLUME_MUTE       , 110 },
        };
    }
}
