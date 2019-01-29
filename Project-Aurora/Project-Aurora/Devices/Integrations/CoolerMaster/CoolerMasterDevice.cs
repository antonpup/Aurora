using Aurora.Settings;
using Aurora.Utils;
using CoolerMaster;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Devices.CoolerMaster
{
    static class CoolerMasterKeys
    {
        public static readonly Dictionary<DeviceKeys, int[]> KeyCoords = new Dictionary<DeviceKeys, int[]>
        {
            {DeviceKeys.ESC, new int [] {0,0} },
            {DeviceKeys.F1, new int [] {0, 1} },
            {DeviceKeys.F2, new int [] {0, 2} },
            {DeviceKeys.F3, new int [] {0, 3} },
            {DeviceKeys.F4, new int [] {0, 4} },
            {DeviceKeys.F5, new int [] {0, 6} },
            {DeviceKeys.F6, new int [] {0, 7} },
            {DeviceKeys.F7, new int [] {0, 8} },
            {DeviceKeys.F8, new int [] {0, 9} },
            {DeviceKeys.F9, new int [] {0, 11} },
            {DeviceKeys.F10, new int [] {0, 12} },
            {DeviceKeys.F11, new int [] {0, 13} },
            {DeviceKeys.F12, new int [] {0, 14} },
            {DeviceKeys.PRINT_SCREEN, new int [] {0, 15} },
            {DeviceKeys.SCROLL_LOCK, new int [] {0, 16} },
            {DeviceKeys.PAUSE_BREAK, new int [] {0, 17} },
            {DeviceKeys.Profile_Key1, new int [] {0, 18} },
            {DeviceKeys.Profile_Key2, new int [] {0, 19} },
            {DeviceKeys.Profile_Key3, new int [] {0, 20} },
            {DeviceKeys.Profile_Key4, new int [] {0, 21} },
            {DeviceKeys.TILDE, new int [] {1, 0} },
            {DeviceKeys.ONE, new int [] {1, 1} },
            {DeviceKeys.TWO, new int [] {1, 2} },
            {DeviceKeys.THREE, new int [] {1, 3} },
            {DeviceKeys.FOUR, new int [] {1, 4} },
            {DeviceKeys.FIVE, new int [] {1, 5} },
            {DeviceKeys.SIX, new int [] {1, 6} },
            {DeviceKeys.SEVEN, new int [] {1, 7} },
            {DeviceKeys.EIGHT, new int [] {1, 8} },
            {DeviceKeys.NINE, new int [] {1, 9} },
            {DeviceKeys.ZERO, new int [] {1, 10} },
            {DeviceKeys.MINUS, new int [] {1, 11} },
            {DeviceKeys.EQUALS, new int [] {1, 12} },
            {DeviceKeys.BACKSPACE, new int [] {1, 14} },
            {DeviceKeys.INSERT, new int [] {1, 15} },
            {DeviceKeys.HOME, new int [] {1, 16} },
            {DeviceKeys.PAGE_UP, new int [] {1, 17} },
            {DeviceKeys.NUM_LOCK, new int [] {1, 18} },
            {DeviceKeys.NUM_SLASH, new int [] {1, 19} },
            {DeviceKeys.NUM_ASTERISK, new int [] {1, 20} },
            {DeviceKeys.NUM_MINUS, new int [] {1, 21} },
            {DeviceKeys.TAB, new int [] {2, 0} },
            {DeviceKeys.Q, new int [] {2, 1} },
            {DeviceKeys.W, new int [] {2, 2} },
            {DeviceKeys.E, new int [] {2, 3} },
            {DeviceKeys.R, new int [] {2, 4} },
            {DeviceKeys.T, new int [] {2, 5} },
            {DeviceKeys.Y, new int [] {2, 6} },
            {DeviceKeys.U, new int [] {2, 7} },
            {DeviceKeys.I, new int [] {2, 8} },
            {DeviceKeys.O, new int [] {2, 9} },
            {DeviceKeys.P, new int [] {2, 10} },
            {DeviceKeys.OPEN_BRACKET, new int [] {2, 11} },
            {DeviceKeys.CLOSE_BRACKET, new int [] {2,12} },
            {DeviceKeys.BACKSLASH, new int [] {2, 14} },
            {DeviceKeys.DELETE, new int [] {2, 15} },
            {DeviceKeys.END, new int [] {2, 16} },
            {DeviceKeys.PAGE_DOWN, new int [] {2, 17} },
            {DeviceKeys.NUM_SEVEN, new int [] {2, 18} },
            {DeviceKeys.NUM_EIGHT, new int [] {2, 19} },
            {DeviceKeys.NUM_NINE, new int [] {2, 20} },
            {DeviceKeys.NUM_PLUS, new int [] {2, 21} },
            {DeviceKeys.CAPS_LOCK, new int [] {3, 0} },
            {DeviceKeys.A, new int [] {3, 1} },
            {DeviceKeys.S, new int [] {3, 2} },
            {DeviceKeys.D, new int [] {3, 3} },
            {DeviceKeys.F, new int [] {3, 4} },
            {DeviceKeys.G, new int [] {3, 5} },
            {DeviceKeys.H, new int [] {3, 6} },
            {DeviceKeys.J, new int [] {3, 7} },
            {DeviceKeys.K, new int [] {3, 8} },
            {DeviceKeys.L, new int [] {3, 9} },
            {DeviceKeys.SEMICOLON, new int [] {3, 10} },
            {DeviceKeys.APOSTROPHE, new int [] {3, 11} },
            {DeviceKeys.HASHTAG, new int [] {3, 12} },
            {DeviceKeys.ENTER, new int [] {3, 14} },
            {DeviceKeys.NUM_FOUR, new int [] {3, 18} },
            {DeviceKeys.NUM_FIVE, new int [] {3, 19} },
            {DeviceKeys.NUM_SIX, new int [] {3, 20} },
            {DeviceKeys.LEFT_SHIFT, new int [] {4, 0} },
            {DeviceKeys.BACKSLASH_UK, new int [] {4, 1} },
            {DeviceKeys.Z, new int [] {4, 2} },
            {DeviceKeys.X, new int [] {4, 3} },
            {DeviceKeys.C, new int [] {4, 4} },
            {DeviceKeys.V, new int [] {4, 5} },
            {DeviceKeys.B, new int [] {4, 6} },
            {DeviceKeys.N, new int [] {4, 7} },
            {DeviceKeys.M, new int [] {4, 8} },
            {DeviceKeys.COMMA, new int [] {4, 9} },
            {DeviceKeys.PERIOD, new int [] {4, 10} },
            {DeviceKeys.FORWARD_SLASH, new int [] {4, 11} },
            {DeviceKeys.RIGHT_SHIFT, new int [] {4, 14} },
            {DeviceKeys.ARROW_UP, new int [] {4, 16} },
            {DeviceKeys.NUM_ONE, new int [] {4, 18} },
            {DeviceKeys.NUM_TWO, new int [] {4, 19} },
            {DeviceKeys.NUM_THREE, new int [] {4, 20} },
            {DeviceKeys.NUM_ENTER, new int [] {4, 21} },
            {DeviceKeys.LEFT_CONTROL, new int [] {5, 0} },
            {DeviceKeys.LEFT_WINDOWS, new int [] {5, 1} },
            {DeviceKeys.LEFT_ALT, new int [] {5, 2} },
            {DeviceKeys.SPACE, new int [] {5, 6} },
            {DeviceKeys.RIGHT_ALT, new int [] {5, 10} },
            {DeviceKeys.RIGHT_WINDOWS, new int [] {5, 11} },
            {DeviceKeys.APPLICATION_SELECT, new int [] {5, 12} },
            {DeviceKeys.RIGHT_CONTROL, new int [] {5, 14} },
            {DeviceKeys.ARROW_LEFT, new int [] {5, 15} },
            {DeviceKeys.ARROW_DOWN, new int [] {5, 16} },
            {DeviceKeys.ARROW_RIGHT, new int [] {5, 17} },
            {DeviceKeys.NUM_ZERO, new int [] {5, 18} },
            {DeviceKeys.NUM_ZEROZERO, new int [] {5, 19} },
            {DeviceKeys.NUM_PERIOD, new int [] {5, 20} }
        };

        public static readonly Dictionary<DeviceKeys, int[]> ProMKeyCoords = new Dictionary<DeviceKeys, int[]>
        {
            {DeviceKeys.ESC, new int [] {0,0} },
            {DeviceKeys.F1, new int [] {0, 1} },
            {DeviceKeys.F2, new int [] {0, 2} },
            {DeviceKeys.F3, new int [] {0, 3} },
            {DeviceKeys.F4, new int [] {0, 4} },
            {DeviceKeys.F5, new int [] {0, 6} },
            {DeviceKeys.F6, new int [] {0, 7} },
            {DeviceKeys.F7, new int [] {0, 8} },
            {DeviceKeys.F8, new int [] {0, 9} },
            {DeviceKeys.F9, new int [] {0, 11} },
            {DeviceKeys.F10, new int [] {0, 12} },
            {DeviceKeys.F11, new int [] {0, 13} },
            {DeviceKeys.F12, new int [] {0, 14} },
            {DeviceKeys.PRINT_SCREEN, new int [] {0, 15} },
            {DeviceKeys.SCROLL_LOCK, new int [] {0, 16} },
            {DeviceKeys.PAUSE_BREAK, new int [] {0, 17} },
            {DeviceKeys.Profile_Key1, new int [] {0, 18} },
            {DeviceKeys.Profile_Key2, new int [] {0, 19} },
            {DeviceKeys.Profile_Key3, new int [] {0, 20} },
            {DeviceKeys.Profile_Key4, new int [] {0, 21} },
            {DeviceKeys.TILDE, new int [] {1, 0} },
            {DeviceKeys.ONE, new int [] {1, 1} },
            {DeviceKeys.TWO, new int [] {1, 2} },
            {DeviceKeys.THREE, new int [] {1, 3} },
            {DeviceKeys.FOUR, new int [] {1, 4} },
            {DeviceKeys.FIVE, new int [] {1, 5} },
            {DeviceKeys.SIX, new int [] {1, 6} },
            {DeviceKeys.SEVEN, new int [] {1, 7} },
            {DeviceKeys.EIGHT, new int [] {1, 8} },
            {DeviceKeys.NINE, new int [] {1, 9} },
            {DeviceKeys.ZERO, new int [] {1, 10} },
            {DeviceKeys.MINUS, new int [] {1, 11} },
            {DeviceKeys.EQUALS, new int [] {1, 12} },
            {DeviceKeys.BACKSPACE, new int [] {1, 14} },
            {DeviceKeys.NUM_LOCK, new int [] {1, 15} },
            {DeviceKeys.NUM_SLASH, new int [] {1, 16} },
            {DeviceKeys.NUM_ASTERISK, new int [] {1, 17} },
            {DeviceKeys.NUM_MINUS, new int [] {1, 18} },
            {DeviceKeys.TAB, new int [] {2, 0} },
            {DeviceKeys.Q, new int [] {2, 1} },
            {DeviceKeys.W, new int [] {2, 2} },
            {DeviceKeys.E, new int [] {2, 3} },
            {DeviceKeys.R, new int [] {2, 4} },
            {DeviceKeys.T, new int [] {2, 5} },
            {DeviceKeys.Y, new int [] {2, 6} },
            {DeviceKeys.U, new int [] {2, 7} },
            {DeviceKeys.I, new int [] {2, 8} },
            {DeviceKeys.O, new int [] {2, 9} },
            {DeviceKeys.P, new int [] {2, 10} },
            {DeviceKeys.OPEN_BRACKET, new int [] {2, 11} },
            {DeviceKeys.CLOSE_BRACKET, new int [] {2,12} },
            {DeviceKeys.BACKSLASH, new int [] {2, 14} },
            {DeviceKeys.NUM_SEVEN, new int [] {2, 15} },
            {DeviceKeys.NUM_EIGHT, new int [] {2, 16} },
            {DeviceKeys.NUM_NINE, new int [] {2, 17} },
            {DeviceKeys.NUM_PLUS, new int [] {2, 18} },
            {DeviceKeys.CAPS_LOCK, new int [] {3, 0} },
            {DeviceKeys.A, new int [] {3, 1} },
            {DeviceKeys.S, new int [] {3, 2} },
            {DeviceKeys.D, new int [] {3, 3} },
            {DeviceKeys.F, new int [] {3, 4} },
            {DeviceKeys.G, new int [] {3, 5} },
            {DeviceKeys.H, new int [] {3, 6} },
            {DeviceKeys.J, new int [] {3, 7} },
            {DeviceKeys.K, new int [] {3, 8} },
            {DeviceKeys.L, new int [] {3, 9} },
            {DeviceKeys.SEMICOLON, new int [] {3, 10} },
            {DeviceKeys.APOSTROPHE, new int [] {3, 11} },
            {DeviceKeys.HASHTAG, new int [] {3, 12} },
            {DeviceKeys.ENTER, new int [] {3, 14} },
            {DeviceKeys.NUM_FOUR, new int [] {3, 15} },
            {DeviceKeys.NUM_FIVE, new int [] {3, 16} },
            {DeviceKeys.NUM_SIX, new int [] {3, 17} },
            {DeviceKeys.LEFT_SHIFT, new int [] {4, 0} },
            {DeviceKeys.BACKSLASH_UK, new int [] {4, 1} },
            {DeviceKeys.Z, new int [] {4, 2} },
            {DeviceKeys.X, new int [] {4, 3} },
            {DeviceKeys.C, new int [] {4, 4} },
            {DeviceKeys.V, new int [] {4, 5} },
            {DeviceKeys.B, new int [] {4, 6} },
            {DeviceKeys.N, new int [] {4, 7} },
            {DeviceKeys.M, new int [] {4, 8} },
            {DeviceKeys.COMMA, new int [] {4, 9} },
            {DeviceKeys.PERIOD, new int [] {4, 10} },
            {DeviceKeys.FORWARD_SLASH, new int [] {4, 11} },
            {DeviceKeys.RIGHT_SHIFT, new int [] {4, 14} },
            {DeviceKeys.NUM_ONE, new int [] {4, 15} },
            {DeviceKeys.NUM_TWO, new int [] {4, 16} },
            {DeviceKeys.NUM_THREE, new int [] {4, 17} },
            {DeviceKeys.NUM_ENTER, new int [] {4, 18} },
            {DeviceKeys.LEFT_CONTROL, new int [] {5, 0} },
            {DeviceKeys.LEFT_WINDOWS, new int [] {5, 1} },
            {DeviceKeys.LEFT_ALT, new int [] {5, 2} },
            {DeviceKeys.SPACE, new int [] {5, 6} },
            {DeviceKeys.RIGHT_ALT, new int [] {5, 10} },
            {DeviceKeys.RIGHT_WINDOWS, new int [] {5, 11} },
            {DeviceKeys.APPLICATION_SELECT, new int [] {5, 12} },
            {DeviceKeys.RIGHT_CONTROL, new int [] {5, 14} },
            {DeviceKeys.NUM_ZERO, new int [] {5, 15} },
            {DeviceKeys.NUM_ZEROZERO, new int [] {5, 16} },
            {DeviceKeys.NUM_PERIOD, new int [] {5, 17} }
        };

        public static readonly Dictionary<DeviceKeys, int[]> MK750Coords = new Dictionary<DeviceKeys, int[]>
        {
            {DeviceKeys.ESC, new int [] {0,0} },
            {DeviceKeys.F1, new int [] {0, 1} },
            {DeviceKeys.F2, new int [] {0, 2} },
            {DeviceKeys.F3, new int [] {0, 3} },
            {DeviceKeys.F4, new int [] {0, 4} },
            {DeviceKeys.F5, new int [] {0, 6} },
            {DeviceKeys.F6, new int [] {0, 7} },
            {DeviceKeys.F7, new int [] {0, 8} },
            {DeviceKeys.F8, new int [] {0, 9} },
            {DeviceKeys.F9, new int [] {0, 11} },
            {DeviceKeys.F10, new int [] {0, 12} },
            {DeviceKeys.F11, new int [] {0, 13} },
            {DeviceKeys.F12, new int [] {0, 14} },
            {DeviceKeys.PRINT_SCREEN, new int [] {0, 15} },
            {DeviceKeys.SCROLL_LOCK, new int [] {0, 16} },
            {DeviceKeys.PAUSE_BREAK, new int [] {0, 17} },
            {DeviceKeys.VOLUME_MUTE, new int [] {0, 18} },
            {DeviceKeys.MEDIA_PLAY_PAUSE, new int [] {0, 19} },
            {DeviceKeys.MEDIA_PREVIOUS, new int [] {0, 20} },
            {DeviceKeys.MEDIA_NEXT, new int [] {0, 21} },
            {DeviceKeys.ADDITIONALLIGHT1, new int [] {0, 22} },
            {DeviceKeys.ADDITIONALLIGHT23, new int [] {0, 23} },
            {DeviceKeys.TILDE, new int [] {1, 0} },
            {DeviceKeys.ONE, new int [] {1, 1} },
            {DeviceKeys.TWO, new int [] {1, 2} },
            {DeviceKeys.THREE, new int [] {1, 3} },
            {DeviceKeys.FOUR, new int [] {1, 4} },
            {DeviceKeys.FIVE, new int [] {1, 5} },
            {DeviceKeys.SIX, new int [] {1, 6} },
            {DeviceKeys.SEVEN, new int [] {1, 7} },
            {DeviceKeys.EIGHT, new int [] {1, 8} },
            {DeviceKeys.NINE, new int [] {1, 9} },
            {DeviceKeys.ZERO, new int [] {1, 10} },
            {DeviceKeys.MINUS, new int [] {1, 11} },
            {DeviceKeys.EQUALS, new int [] {1, 12} },
            {DeviceKeys.BACKSPACE, new int [] {1, 14} },
            {DeviceKeys.INSERT, new int [] {1, 15} },
            {DeviceKeys.HOME, new int [] {1, 16} },
            {DeviceKeys.PAGE_UP, new int [] {1, 17} },
            {DeviceKeys.NUM_LOCK, new int [] {1, 18} },
            {DeviceKeys.NUM_SLASH, new int [] {1, 19} },
            {DeviceKeys.NUM_ASTERISK, new int [] {1, 20} },
            {DeviceKeys.NUM_MINUS, new int [] {1, 21} },
            {DeviceKeys.ADDITIONALLIGHT2, new int [] {1, 22} },
            {DeviceKeys.ADDITIONALLIGHT24, new int [] {1, 23} },
            {DeviceKeys.TAB, new int [] {2, 0} },
            {DeviceKeys.Q, new int [] {2, 1} },
            {DeviceKeys.W, new int [] {2, 2} },
            {DeviceKeys.E, new int [] {2, 3} },
            {DeviceKeys.R, new int [] {2, 4} },
            {DeviceKeys.T, new int [] {2, 5} },
            {DeviceKeys.Y, new int [] {2, 6} },
            {DeviceKeys.U, new int [] {2, 7} },
            {DeviceKeys.I, new int [] {2, 8} },
            {DeviceKeys.O, new int [] {2, 9} },
            {DeviceKeys.P, new int [] {2, 10} },
            {DeviceKeys.OPEN_BRACKET, new int [] {2, 11} },
            {DeviceKeys.CLOSE_BRACKET, new int [] {2,12} },
            {DeviceKeys.BACKSLASH, new int [] {2, 14} },
            {DeviceKeys.DELETE, new int [] {2, 15} },
            {DeviceKeys.END, new int [] {2, 16} },
            {DeviceKeys.PAGE_DOWN, new int [] {2, 17} },
            {DeviceKeys.NUM_SEVEN, new int [] {2, 18} },
            {DeviceKeys.NUM_EIGHT, new int [] {2, 19} },
            {DeviceKeys.NUM_NINE, new int [] {2, 20} },
            {DeviceKeys.NUM_PLUS, new int [] {2, 21} },
            {DeviceKeys.ADDITIONALLIGHT3, new int [] {2, 22} },
            {DeviceKeys.ADDITIONALLIGHT25, new int [] {2, 23} },
            {DeviceKeys.CAPS_LOCK, new int [] {3, 0} },
            {DeviceKeys.A, new int [] {3, 1} },
            {DeviceKeys.S, new int [] {3, 2} },
            {DeviceKeys.D, new int [] {3, 3} },
            {DeviceKeys.F, new int [] {3, 4} },
            {DeviceKeys.G, new int [] {3, 5} },
            {DeviceKeys.H, new int [] {3, 6} },
            {DeviceKeys.J, new int [] {3, 7} },
            {DeviceKeys.K, new int [] {3, 8} },
            {DeviceKeys.L, new int [] {3, 9} },
            {DeviceKeys.SEMICOLON, new int [] {3, 10} },
            {DeviceKeys.APOSTROPHE, new int [] {3, 11} },
            {DeviceKeys.HASHTAG, new int [] {3, 12} },
            {DeviceKeys.ENTER, new int [] {3, 14} },
            {DeviceKeys.NUM_FOUR, new int [] {3, 18} },
            {DeviceKeys.NUM_FIVE, new int [] {3, 19} },
            {DeviceKeys.NUM_SIX, new int [] {3, 20} },
            {DeviceKeys.ADDITIONALLIGHT4, new int [] {3, 22} },
            {DeviceKeys.ADDITIONALLIGHT26, new int [] {3, 23} },
            {DeviceKeys.LEFT_SHIFT, new int [] {4, 0} },
            {DeviceKeys.BACKSLASH_UK, new int [] {4, 1} },
            {DeviceKeys.Z, new int [] {4, 2} },
            {DeviceKeys.X, new int [] {4, 3} },
            {DeviceKeys.C, new int [] {4, 4} },
            {DeviceKeys.V, new int [] {4, 5} },
            {DeviceKeys.B, new int [] {4, 6} },
            {DeviceKeys.N, new int [] {4, 7} },
            {DeviceKeys.M, new int [] {4, 8} },
            {DeviceKeys.COMMA, new int [] {4, 9} },
            {DeviceKeys.PERIOD, new int [] {4, 10} },
            {DeviceKeys.FORWARD_SLASH, new int [] {4, 11} },
            {DeviceKeys.RIGHT_SHIFT, new int [] {4, 14} },
            {DeviceKeys.ARROW_UP, new int [] {4, 16} },
            {DeviceKeys.NUM_ONE, new int [] {4, 18} },
            {DeviceKeys.NUM_TWO, new int [] {4, 19} },
            {DeviceKeys.NUM_THREE, new int [] {4, 20} },
            {DeviceKeys.NUM_ENTER, new int [] {4, 21} },
            {DeviceKeys.LEFT_CONTROL, new int [] {5, 0} },
            {DeviceKeys.LEFT_WINDOWS, new int [] {5, 1} },
            {DeviceKeys.LEFT_ALT, new int [] {5, 2} },
            {DeviceKeys.SPACE, new int [] {5, 6} },
            {DeviceKeys.RIGHT_ALT, new int [] {5, 10} },
            {DeviceKeys.RIGHT_WINDOWS, new int [] {5, 11} },
            {DeviceKeys.FN_Key, new int [] {5, 12} },
            {DeviceKeys.RIGHT_CONTROL, new int [] {5, 14} },
            {DeviceKeys.ARROW_LEFT, new int [] {5, 15} },
            {DeviceKeys.ARROW_DOWN, new int [] {5, 16} },
            {DeviceKeys.ARROW_RIGHT, new int [] {5, 17} },
            {DeviceKeys.NUM_ZERO, new int [] {5, 18} },
            {DeviceKeys.NUM_PERIOD, new int [] {5, 20} },
            {DeviceKeys.ADDITIONALLIGHT5, new int [] {6, 0} },
            {DeviceKeys.ADDITIONALLIGHT6, new int [] {6, 1} },
            {DeviceKeys.ADDITIONALLIGHT7, new int [] {6, 2} },
            {DeviceKeys.ADDITIONALLIGHT8, new int [] {6, 3} },
            {DeviceKeys.ADDITIONALLIGHT9, new int [] {6, 4} },
            {DeviceKeys.ADDITIONALLIGHT10, new int [] {6, 5} },
            {DeviceKeys.ADDITIONALLIGHT11, new int [] {6, 6} },
            {DeviceKeys.ADDITIONALLIGHT12, new int [] {6, 7} },
            {DeviceKeys.ADDITIONALLIGHT13, new int [] {6, 8} },
            {DeviceKeys.ADDITIONALLIGHT14, new int [] {6, 9} },
            {DeviceKeys.ADDITIONALLIGHT15, new int [] {6, 10} },
            {DeviceKeys.ADDITIONALLIGHT16, new int [] {6, 11} },
            {DeviceKeys.ADDITIONALLIGHT17, new int [] {6, 12} },
            {DeviceKeys.ADDITIONALLIGHT18, new int [] {6, 13} },
            {DeviceKeys.ADDITIONALLIGHT19, new int [] {6, 14} },
            {DeviceKeys.ADDITIONALLIGHT20, new int [] {6, 15} },
            {DeviceKeys.ADDITIONALLIGHT21, new int [] {6, 16} },
            {DeviceKeys.ADDITIONALLIGHT22, new int [] {6, 17} },

        };

        public static readonly Dictionary<CoolerMasterSDK.DEVICE_INDEX, Dictionary<DeviceKeys, int[]>> KeyboardLayoutMapping = new Dictionary<CoolerMasterSDK.DEVICE_INDEX, Dictionary<DeviceKeys, int[]>>
        {
            { CoolerMasterSDK.DEVICE_INDEX.DEV_MKeys_M, ProMKeyCoords },
            { CoolerMasterSDK.DEVICE_INDEX.DEV_MKeys_M_White, ProMKeyCoords },
            { CoolerMasterSDK.DEVICE_INDEX.DEV_MK750, MK750Coords }
        };

    }

    class CoolerMasterDevice : Device
    {
        private String devicename = "Cooler Master";
        private List<CoolerMasterSDK.DEVICE_INDEX> InitializedDevices = new List<CoolerMasterSDK.DEVICE_INDEX>();
        private CoolerMasterSDK.DEVICE_INDEX CurrentDevice = CoolerMasterSDK.DEVICE_INDEX.None;
        private bool isInitialized = false;

        private bool keyboard_updated = false;
        private bool peripheral_updated = false;

        private readonly object action_lock = new object();

        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;

        //Keyboard stuff
        private CoolerMasterSDK.COLOR_MATRIX color_matrix = new CoolerMasterSDK.COLOR_MATRIX() { KeyColor = new CoolerMasterSDK.KEY_COLOR[CoolerMasterSDK.MAX_LED_ROW, CoolerMasterSDK.MAX_LED_COLUMN] };
        //private Color peripheral_Color = Color.Black;

        //Previous data
        //private CoolerMasterSDK.KEY_COLOR[,] previous_key_colors = new CoolerMasterSDK.KEY_COLOR[CoolerMasterSDK.MAX_LED_ROW, CoolerMasterSDK.MAX_LED_COLUMN];
        //private Color previous_peripheral_Color = Color.Black;


        public bool Initialize()
        {
            lock (action_lock)
            {
                if (!isInitialized)
                {

                    try
                    {
                        foreach (CoolerMasterSDK.DEVICE_INDEX device in Enum.GetValues(typeof(CoolerMasterSDK.DEVICE_INDEX)))
                        {
                            if (device != CoolerMasterSDK.DEVICE_INDEX.None)
                            {
                                try
                                {
                                    bool init = SwitchToDevice(device);
                                    if (init)
                                    {
                                        InitializedDevices.Add(device);
                                        isInitialized = true;
                                        break;
                                    }
                                }
                                catch (Exception exc)
                                {
                                    Global.logger.Error("Exception while loading Cooler Master device: " + device.GetDescription() + ". Exception:" + exc);
                                }
                            }

                        }

                        List<CoolerMasterSDK.DEVICE_INDEX> devices = InitializedDevices.FindAll(x => CoolerMasterSDK.Keyboards.Contains(x));
                        if (devices.Count > 0)
                            SwitchToDevice(devices.First());
                    }
                    catch (Exception exc)
                    {
                        Global.logger.Error("There was an error initializing Cooler Master SDK.\r\n" + exc.Message);

                        return false;
                    }
                }

                if (!isInitialized)
                    Global.logger.Info("No Cooler Master devices successfully Initialized!");

                return isInitialized;
            }
        }

        protected bool SwitchToDevice(CoolerMasterSDK.DEVICE_INDEX device)
        {
            if (CurrentDevice == device)
                return true;

            bool init = false;
            CoolerMasterSDK.SetControlDevice(device);
            CurrentDevice = device;
            if (CoolerMasterSDK.IsDevicePlug() && CoolerMasterSDK.EnableLedControl(true))
            {
                init = true;
            }

            return init;
        }

        public void Shutdown()
        {
            lock (action_lock)
            {
                if (isInitialized)
                {
                    CoolerMasterSDK.EnableLedControl(false);
                    isInitialized = false;
                }
            }
        }

        public string GetDeviceDetails()
        {
            if (isInitialized)
            {
                string devString = devicename + ": ";
                foreach (var device in InitializedDevices)
                {
                    devString += device.GetDescription() + " ";
                }

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

        public void Reset()
        {
            if (this.IsInitialized() && (keyboard_updated || peripheral_updated))
            {
                keyboard_updated = false;
                peripheral_updated = false;
            }
        }

        public bool Reconnect()
        {
            throw new NotImplementedException();
        }

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        private void SetOneKey(int[] key, Color color)
        {
            color = Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(color, color.A / 255.0D));
            CoolerMasterSDK.KEY_COLOR key_color = new CoolerMasterSDK.KEY_COLOR(color.R, color.G, color.B);
            color_matrix.KeyColor[key[0], key[1]] = key_color;
        }

        private void SendColorsToKeyboard(bool forced = false)
        {
            if (Global.Configuration.devices_disable_keyboard)
                return;

            if (!CoolerMasterSDK.Keyboards.Contains(CurrentDevice))
                return;

            CoolerMasterSDK.SetAllLedColor(color_matrix);
            //previous_key_colors = key_colors;
            keyboard_updated = true;
        }

        private void SendColorToPeripheral(Color color, bool forced = false)
        {
            peripheral_updated = false;
        }

        public bool IsInitialized()
        {
            return this.isInitialized;
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            try
            {
                var coords = CoolerMasterKeys.KeyCoords;

                if (CoolerMasterKeys.KeyboardLayoutMapping.ContainsKey(CurrentDevice))
                    coords = CoolerMasterKeys.KeyboardLayoutMapping[CurrentDevice];
                
                foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
                {
                    if (e.Cancel) return false;

                    int[] coordinates = new int[2];

                    DeviceKeys dev_key = key.Key;

                    if (dev_key == DeviceKeys.ENTER &&
                        (Global.kbLayout.Loaded_Localization != Settings.PreferredKeyboardLocalization.us &&
                         Global.kbLayout.Loaded_Localization != Settings.PreferredKeyboardLocalization.dvorak))
                        dev_key = DeviceKeys.BACKSLASH;

                    if (Effects.possible_peripheral_keys.Contains(key.Key))
                    {
                        //Temp until mice support is added
                        continue;

                        //Move this to the SendColorsToMouse as they do not need to be set on every key, they only need to be directed to the correct method for setting key/light
                        /*List<CoolerMasterSDK.DEVICE_INDEX> devices = InitializedDevices.FindAll(x => CoolerMasterSDK.Mice.Contains(x));
                        if (devices.Count > 0)
                            SwitchToDevice(devices.First());
                        else
                            return false;*/
                    }


                    if (dev_key == DeviceKeys.ADDITIONALLIGHT10)
                        Console.Write("");
                    if (coords.TryGetValue(dev_key, out coordinates))
                        SetOneKey(coordinates, (Color)key.Value);
                }
                if (e.Cancel) return false;
                SendColorsToKeyboard(forced || !keyboard_updated);
                return true;
            }
            catch (Exception exc)
            {
                Global.logger.Error("Failed to Update Device" + exc.ToString());
                return false;
            }
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            watch.Restart();

            bool update_result = UpdateDevice(colorComposition.keyColors, e, forced);

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return update_result;
        }

        public bool IsKeyboardConnected()
        {
            return isInitialized;
        }

        public bool IsPeripheralConnected()
        {
            return isInitialized;
        }

        public string GetDeviceUpdatePerformance()
        {
            return (isInitialized ? lastUpdateTime + " ms" : "");
        }

        public VariableRegistry GetRegisteredVariables()
        {
            return new VariableRegistry();
        }
    }
}
