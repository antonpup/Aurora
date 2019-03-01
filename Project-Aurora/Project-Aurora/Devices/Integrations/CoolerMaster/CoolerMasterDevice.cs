using Aurora.Devices.Layout.Layouts;
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
using Aurora.Devices.Layout;
using LEDINT = System.Int16;

namespace Aurora.Devices.CoolerMaster
{
    static class CoolerMasterKeys
    {
        public static readonly Dictionary<KeyboardKeys, int[]> KeyCoords = new Dictionary<KeyboardKeys, int[]>
        {
            {KeyboardKeys.ESC, new int [] {0,0} },
            {KeyboardKeys.F1, new int [] {0, 1} },
            {KeyboardKeys.F2, new int [] {0, 2} },
            {KeyboardKeys.F3, new int [] {0, 3} },
            {KeyboardKeys.F4, new int [] {0, 4} },
            {KeyboardKeys.F5, new int [] {0, 6} },
            {KeyboardKeys.F6, new int [] {0, 7} },
            {KeyboardKeys.F7, new int [] {0, 8} },
            {KeyboardKeys.F8, new int [] {0, 9} },
            {KeyboardKeys.F9, new int [] {0, 11} },
            {KeyboardKeys.F10, new int [] {0, 12} },
            {KeyboardKeys.F11, new int [] {0, 13} },
            {KeyboardKeys.F12, new int [] {0, 14} },
            {KeyboardKeys.PRINT_SCREEN, new int [] {0, 15} },
            {KeyboardKeys.SCROLL_LOCK, new int [] {0, 16} },
            {KeyboardKeys.PAUSE_BREAK, new int [] {0, 17} },
            {KeyboardKeys.Profile_Key1, new int [] {0, 18} },
            {KeyboardKeys.Profile_Key2, new int [] {0, 19} },
            {KeyboardKeys.Profile_Key3, new int [] {0, 20} },
            {KeyboardKeys.Profile_Key4, new int [] {0, 21} },
            {KeyboardKeys.TILDE, new int [] {1, 0} },
            {KeyboardKeys.ONE, new int [] {1, 1} },
            {KeyboardKeys.TWO, new int [] {1, 2} },
            {KeyboardKeys.THREE, new int [] {1, 3} },
            {KeyboardKeys.FOUR, new int [] {1, 4} },
            {KeyboardKeys.FIVE, new int [] {1, 5} },
            {KeyboardKeys.SIX, new int [] {1, 6} },
            {KeyboardKeys.SEVEN, new int [] {1, 7} },
            {KeyboardKeys.EIGHT, new int [] {1, 8} },
            {KeyboardKeys.NINE, new int [] {1, 9} },
            {KeyboardKeys.ZERO, new int [] {1, 10} },
            {KeyboardKeys.MINUS, new int [] {1, 11} },
            {KeyboardKeys.EQUALS, new int [] {1, 12} },
            {KeyboardKeys.BACKSPACE, new int [] {1, 14} },
            {KeyboardKeys.INSERT, new int [] {1, 15} },
            {KeyboardKeys.HOME, new int [] {1, 16} },
            {KeyboardKeys.PAGE_UP, new int [] {1, 17} },
            {KeyboardKeys.NUM_LOCK, new int [] {1, 18} },
            {KeyboardKeys.NUM_SLASH, new int [] {1, 19} },
            {KeyboardKeys.NUM_ASTERISK, new int [] {1, 20} },
            {KeyboardKeys.NUM_MINUS, new int [] {1, 21} },
            {KeyboardKeys.TAB, new int [] {2, 0} },
            {KeyboardKeys.Q, new int [] {2, 1} },
            {KeyboardKeys.W, new int [] {2, 2} },
            {KeyboardKeys.E, new int [] {2, 3} },
            {KeyboardKeys.R, new int [] {2, 4} },
            {KeyboardKeys.T, new int [] {2, 5} },
            {KeyboardKeys.Y, new int [] {2, 6} },
            {KeyboardKeys.U, new int [] {2, 7} },
            {KeyboardKeys.I, new int [] {2, 8} },
            {KeyboardKeys.O, new int [] {2, 9} },
            {KeyboardKeys.P, new int [] {2, 10} },
            {KeyboardKeys.OPEN_BRACKET, new int [] {2, 11} },
            {KeyboardKeys.CLOSE_BRACKET, new int [] {2,12} },
            {KeyboardKeys.BACKSLASH, new int [] {2, 14} },
            {KeyboardKeys.DELETE, new int [] {2, 15} },
            {KeyboardKeys.END, new int [] {2, 16} },
            {KeyboardKeys.PAGE_DOWN, new int [] {2, 17} },
            {KeyboardKeys.NUM_SEVEN, new int [] {2, 18} },
            {KeyboardKeys.NUM_EIGHT, new int [] {2, 19} },
            {KeyboardKeys.NUM_NINE, new int [] {2, 20} },
            {KeyboardKeys.NUM_PLUS, new int [] {2, 21} },
            {KeyboardKeys.CAPS_LOCK, new int [] {3, 0} },
            {KeyboardKeys.A, new int [] {3, 1} },
            {KeyboardKeys.S, new int [] {3, 2} },
            {KeyboardKeys.D, new int [] {3, 3} },
            {KeyboardKeys.F, new int [] {3, 4} },
            {KeyboardKeys.G, new int [] {3, 5} },
            {KeyboardKeys.H, new int [] {3, 6} },
            {KeyboardKeys.J, new int [] {3, 7} },
            {KeyboardKeys.K, new int [] {3, 8} },
            {KeyboardKeys.L, new int [] {3, 9} },
            {KeyboardKeys.SEMICOLON, new int [] {3, 10} },
            {KeyboardKeys.APOSTROPHE, new int [] {3, 11} },
            {KeyboardKeys.HASH, new int [] {3, 12} },
            {KeyboardKeys.ENTER, new int [] {3, 14} },
            {KeyboardKeys.NUM_FOUR, new int [] {3, 18} },
            {KeyboardKeys.NUM_FIVE, new int [] {3, 19} },
            {KeyboardKeys.NUM_SIX, new int [] {3, 20} },
            {KeyboardKeys.LEFT_SHIFT, new int [] {4, 0} },
            {KeyboardKeys.BACKSLASH_UK, new int [] {4, 1} },
            {KeyboardKeys.Z, new int [] {4, 2} },
            {KeyboardKeys.X, new int [] {4, 3} },
            {KeyboardKeys.C, new int [] {4, 4} },
            {KeyboardKeys.V, new int [] {4, 5} },
            {KeyboardKeys.B, new int [] {4, 6} },
            {KeyboardKeys.N, new int [] {4, 7} },
            {KeyboardKeys.M, new int [] {4, 8} },
            {KeyboardKeys.COMMA, new int [] {4, 9} },
            {KeyboardKeys.PERIOD, new int [] {4, 10} },
            {KeyboardKeys.FORWARD_SLASH, new int [] {4, 11} },
            {KeyboardKeys.RIGHT_SHIFT, new int [] {4, 14} },
            {KeyboardKeys.ARROW_UP, new int [] {4, 16} },
            {KeyboardKeys.NUM_ONE, new int [] {4, 18} },
            {KeyboardKeys.NUM_TWO, new int [] {4, 19} },
            {KeyboardKeys.NUM_THREE, new int [] {4, 20} },
            {KeyboardKeys.NUM_ENTER, new int [] {4, 21} },
            {KeyboardKeys.LEFT_CONTROL, new int [] {5, 0} },
            {KeyboardKeys.LEFT_WINDOWS, new int [] {5, 1} },
            {KeyboardKeys.LEFT_ALT, new int [] {5, 2} },
            {KeyboardKeys.SPACE, new int [] {5, 6} },
            {KeyboardKeys.RIGHT_ALT, new int [] {5, 10} },
            {KeyboardKeys.RIGHT_WINDOWS, new int [] {5, 11} },
            {KeyboardKeys.APPLICATION_SELECT, new int [] {5, 12} },
            {KeyboardKeys.RIGHT_CONTROL, new int [] {5, 14} },
            {KeyboardKeys.ARROW_LEFT, new int [] {5, 15} },
            {KeyboardKeys.ARROW_DOWN, new int [] {5, 16} },
            {KeyboardKeys.ARROW_RIGHT, new int [] {5, 17} },
            {KeyboardKeys.NUM_ZERO, new int [] {5, 18} },
            {KeyboardKeys.NUM_ZEROZERO, new int [] {5, 19} },
            {KeyboardKeys.NUM_PERIOD, new int [] {5, 20} }
        };

        public static readonly Dictionary<KeyboardKeys, int[]> ProMKeyCoords = new Dictionary<KeyboardKeys, int[]>
        {
            {KeyboardKeys.ESC, new int [] {0,0} },
            {KeyboardKeys.F1, new int [] {0, 1} },
            {KeyboardKeys.F2, new int [] {0, 2} },
            {KeyboardKeys.F3, new int [] {0, 3} },
            {KeyboardKeys.F4, new int [] {0, 4} },
            {KeyboardKeys.F5, new int [] {0, 6} },
            {KeyboardKeys.F6, new int [] {0, 7} },
            {KeyboardKeys.F7, new int [] {0, 8} },
            {KeyboardKeys.F8, new int [] {0, 9} },
            {KeyboardKeys.F9, new int [] {0, 11} },
            {KeyboardKeys.F10, new int [] {0, 12} },
            {KeyboardKeys.F11, new int [] {0, 13} },
            {KeyboardKeys.F12, new int [] {0, 14} },
            {KeyboardKeys.PRINT_SCREEN, new int [] {0, 15} },
            {KeyboardKeys.SCROLL_LOCK, new int [] {0, 16} },
            {KeyboardKeys.PAUSE_BREAK, new int [] {0, 17} },
            {KeyboardKeys.Profile_Key1, new int [] {0, 18} },
            {KeyboardKeys.Profile_Key2, new int [] {0, 19} },
            {KeyboardKeys.Profile_Key3, new int [] {0, 20} },
            {KeyboardKeys.Profile_Key4, new int [] {0, 21} },
            {KeyboardKeys.TILDE, new int [] {1, 0} },
            {KeyboardKeys.ONE, new int [] {1, 1} },
            {KeyboardKeys.TWO, new int [] {1, 2} },
            {KeyboardKeys.THREE, new int [] {1, 3} },
            {KeyboardKeys.FOUR, new int [] {1, 4} },
            {KeyboardKeys.FIVE, new int [] {1, 5} },
            {KeyboardKeys.SIX, new int [] {1, 6} },
            {KeyboardKeys.SEVEN, new int [] {1, 7} },
            {KeyboardKeys.EIGHT, new int [] {1, 8} },
            {KeyboardKeys.NINE, new int [] {1, 9} },
            {KeyboardKeys.ZERO, new int [] {1, 10} },
            {KeyboardKeys.MINUS, new int [] {1, 11} },
            {KeyboardKeys.EQUALS, new int [] {1, 12} },
            {KeyboardKeys.BACKSPACE, new int [] {1, 14} },
            {KeyboardKeys.NUM_LOCK, new int [] {1, 15} },
            {KeyboardKeys.NUM_SLASH, new int [] {1, 16} },
            {KeyboardKeys.NUM_ASTERISK, new int [] {1, 17} },
            {KeyboardKeys.NUM_MINUS, new int [] {1, 18} },
            {KeyboardKeys.TAB, new int [] {2, 0} },
            {KeyboardKeys.Q, new int [] {2, 1} },
            {KeyboardKeys.W, new int [] {2, 2} },
            {KeyboardKeys.E, new int [] {2, 3} },
            {KeyboardKeys.R, new int [] {2, 4} },
            {KeyboardKeys.T, new int [] {2, 5} },
            {KeyboardKeys.Y, new int [] {2, 6} },
            {KeyboardKeys.U, new int [] {2, 7} },
            {KeyboardKeys.I, new int [] {2, 8} },
            {KeyboardKeys.O, new int [] {2, 9} },
            {KeyboardKeys.P, new int [] {2, 10} },
            {KeyboardKeys.OPEN_BRACKET, new int [] {2, 11} },
            {KeyboardKeys.CLOSE_BRACKET, new int [] {2,12} },
            {KeyboardKeys.BACKSLASH, new int [] {2, 14} },
            {KeyboardKeys.NUM_SEVEN, new int [] {2, 15} },
            {KeyboardKeys.NUM_EIGHT, new int [] {2, 16} },
            {KeyboardKeys.NUM_NINE, new int [] {2, 17} },
            {KeyboardKeys.NUM_PLUS, new int [] {2, 18} },
            {KeyboardKeys.CAPS_LOCK, new int [] {3, 0} },
            {KeyboardKeys.A, new int [] {3, 1} },
            {KeyboardKeys.S, new int [] {3, 2} },
            {KeyboardKeys.D, new int [] {3, 3} },
            {KeyboardKeys.F, new int [] {3, 4} },
            {KeyboardKeys.G, new int [] {3, 5} },
            {KeyboardKeys.H, new int [] {3, 6} },
            {KeyboardKeys.J, new int [] {3, 7} },
            {KeyboardKeys.K, new int [] {3, 8} },
            {KeyboardKeys.L, new int [] {3, 9} },
            {KeyboardKeys.SEMICOLON, new int [] {3, 10} },
            {KeyboardKeys.APOSTROPHE, new int [] {3, 11} },
            {KeyboardKeys.HASH, new int [] {3, 12} },
            {KeyboardKeys.ENTER, new int [] {3, 14} },
            {KeyboardKeys.NUM_FOUR, new int [] {3, 15} },
            {KeyboardKeys.NUM_FIVE, new int [] {3, 16} },
            {KeyboardKeys.NUM_SIX, new int [] {3, 17} },
            {KeyboardKeys.LEFT_SHIFT, new int [] {4, 0} },
            {KeyboardKeys.BACKSLASH_UK, new int [] {4, 1} },
            {KeyboardKeys.Z, new int [] {4, 2} },
            {KeyboardKeys.X, new int [] {4, 3} },
            {KeyboardKeys.C, new int [] {4, 4} },
            {KeyboardKeys.V, new int [] {4, 5} },
            {KeyboardKeys.B, new int [] {4, 6} },
            {KeyboardKeys.N, new int [] {4, 7} },
            {KeyboardKeys.M, new int [] {4, 8} },
            {KeyboardKeys.COMMA, new int [] {4, 9} },
            {KeyboardKeys.PERIOD, new int [] {4, 10} },
            {KeyboardKeys.FORWARD_SLASH, new int [] {4, 11} },
            {KeyboardKeys.RIGHT_SHIFT, new int [] {4, 14} },
            {KeyboardKeys.NUM_ONE, new int [] {4, 15} },
            {KeyboardKeys.NUM_TWO, new int [] {4, 16} },
            {KeyboardKeys.NUM_THREE, new int [] {4, 17} },
            {KeyboardKeys.NUM_ENTER, new int [] {4, 18} },
            {KeyboardKeys.LEFT_CONTROL, new int [] {5, 0} },
            {KeyboardKeys.LEFT_WINDOWS, new int [] {5, 1} },
            {KeyboardKeys.LEFT_ALT, new int [] {5, 2} },
            {KeyboardKeys.SPACE, new int [] {5, 6} },
            {KeyboardKeys.RIGHT_ALT, new int [] {5, 10} },
            {KeyboardKeys.RIGHT_WINDOWS, new int [] {5, 11} },
            {KeyboardKeys.APPLICATION_SELECT, new int [] {5, 12} },
            {KeyboardKeys.RIGHT_CONTROL, new int [] {5, 14} },
            {KeyboardKeys.NUM_ZERO, new int [] {5, 15} },
            {KeyboardKeys.NUM_ZEROZERO, new int [] {5, 16} },
            {KeyboardKeys.NUM_PERIOD, new int [] {5, 17} }
        };

        public static readonly Dictionary<KeyboardKeys, int[]> MK750Coords = new Dictionary<KeyboardKeys, int[]>
        {
            {KeyboardKeys.ESC, new int [] {0,0} },
            {KeyboardKeys.F1, new int [] {0, 1} },
            {KeyboardKeys.F2, new int [] {0, 2} },
            {KeyboardKeys.F3, new int [] {0, 3} },
            {KeyboardKeys.F4, new int [] {0, 4} },
            {KeyboardKeys.F5, new int [] {0, 6} },
            {KeyboardKeys.F6, new int [] {0, 7} },
            {KeyboardKeys.F7, new int [] {0, 8} },
            {KeyboardKeys.F8, new int [] {0, 9} },
            {KeyboardKeys.F9, new int [] {0, 11} },
            {KeyboardKeys.F10, new int [] {0, 12} },
            {KeyboardKeys.F11, new int [] {0, 13} },
            {KeyboardKeys.F12, new int [] {0, 14} },
            {KeyboardKeys.PRINT_SCREEN, new int [] {0, 15} },
            {KeyboardKeys.SCROLL_LOCK, new int [] {0, 16} },
            {KeyboardKeys.PAUSE_BREAK, new int [] {0, 17} },
            {KeyboardKeys.VOLUME_MUTE, new int [] {0, 18} },
            {KeyboardKeys.MEDIA_PLAY_PAUSE, new int [] {0, 19} },
            {KeyboardKeys.MEDIA_PREVIOUS, new int [] {0, 20} },
            {KeyboardKeys.MEDIA_NEXT, new int [] {0, 21} },
            {KeyboardKeys.ADDITIONALLIGHT1, new int [] {0, 22} },
            {KeyboardKeys.ADDITIONALLIGHT23, new int [] {0, 23} },
            {KeyboardKeys.TILDE, new int [] {1, 0} },
            {KeyboardKeys.ONE, new int [] {1, 1} },
            {KeyboardKeys.TWO, new int [] {1, 2} },
            {KeyboardKeys.THREE, new int [] {1, 3} },
            {KeyboardKeys.FOUR, new int [] {1, 4} },
            {KeyboardKeys.FIVE, new int [] {1, 5} },
            {KeyboardKeys.SIX, new int [] {1, 6} },
            {KeyboardKeys.SEVEN, new int [] {1, 7} },
            {KeyboardKeys.EIGHT, new int [] {1, 8} },
            {KeyboardKeys.NINE, new int [] {1, 9} },
            {KeyboardKeys.ZERO, new int [] {1, 10} },
            {KeyboardKeys.MINUS, new int [] {1, 11} },
            {KeyboardKeys.EQUALS, new int [] {1, 12} },
            {KeyboardKeys.BACKSPACE, new int [] {1, 14} },
            {KeyboardKeys.INSERT, new int [] {1, 15} },
            {KeyboardKeys.HOME, new int [] {1, 16} },
            {KeyboardKeys.PAGE_UP, new int [] {1, 17} },
            {KeyboardKeys.NUM_LOCK, new int [] {1, 18} },
            {KeyboardKeys.NUM_SLASH, new int [] {1, 19} },
            {KeyboardKeys.NUM_ASTERISK, new int [] {1, 20} },
            {KeyboardKeys.NUM_MINUS, new int [] {1, 21} },
            {KeyboardKeys.ADDITIONALLIGHT2, new int [] {1, 22} },
            {KeyboardKeys.ADDITIONALLIGHT24, new int [] {1, 23} },
            {KeyboardKeys.TAB, new int [] {2, 0} },
            {KeyboardKeys.Q, new int [] {2, 1} },
            {KeyboardKeys.W, new int [] {2, 2} },
            {KeyboardKeys.E, new int [] {2, 3} },
            {KeyboardKeys.R, new int [] {2, 4} },
            {KeyboardKeys.T, new int [] {2, 5} },
            {KeyboardKeys.Y, new int [] {2, 6} },
            {KeyboardKeys.U, new int [] {2, 7} },
            {KeyboardKeys.I, new int [] {2, 8} },
            {KeyboardKeys.O, new int [] {2, 9} },
            {KeyboardKeys.P, new int [] {2, 10} },
            {KeyboardKeys.OPEN_BRACKET, new int [] {2, 11} },
            {KeyboardKeys.CLOSE_BRACKET, new int [] {2,12} },
            {KeyboardKeys.BACKSLASH, new int [] {2, 14} },
            {KeyboardKeys.DELETE, new int [] {2, 15} },
            {KeyboardKeys.END, new int [] {2, 16} },
            {KeyboardKeys.PAGE_DOWN, new int [] {2, 17} },
            {KeyboardKeys.NUM_SEVEN, new int [] {2, 18} },
            {KeyboardKeys.NUM_EIGHT, new int [] {2, 19} },
            {KeyboardKeys.NUM_NINE, new int [] {2, 20} },
            {KeyboardKeys.NUM_PLUS, new int [] {2, 21} },
            {KeyboardKeys.ADDITIONALLIGHT3, new int [] {2, 22} },
            {KeyboardKeys.ADDITIONALLIGHT25, new int [] {2, 23} },
            {KeyboardKeys.CAPS_LOCK, new int [] {3, 0} },
            {KeyboardKeys.A, new int [] {3, 1} },
            {KeyboardKeys.S, new int [] {3, 2} },
            {KeyboardKeys.D, new int [] {3, 3} },
            {KeyboardKeys.F, new int [] {3, 4} },
            {KeyboardKeys.G, new int [] {3, 5} },
            {KeyboardKeys.H, new int [] {3, 6} },
            {KeyboardKeys.J, new int [] {3, 7} },
            {KeyboardKeys.K, new int [] {3, 8} },
            {KeyboardKeys.L, new int [] {3, 9} },
            {KeyboardKeys.SEMICOLON, new int [] {3, 10} },
            {KeyboardKeys.APOSTROPHE, new int [] {3, 11} },
            {KeyboardKeys.HASH, new int [] {3, 12} },
            {KeyboardKeys.ENTER, new int [] {3, 14} },
            {KeyboardKeys.NUM_FOUR, new int [] {3, 18} },
            {KeyboardKeys.NUM_FIVE, new int [] {3, 19} },
            {KeyboardKeys.NUM_SIX, new int [] {3, 20} },
            {KeyboardKeys.ADDITIONALLIGHT4, new int [] {3, 22} },
            {KeyboardKeys.ADDITIONALLIGHT26, new int [] {3, 23} },
            {KeyboardKeys.LEFT_SHIFT, new int [] {4, 0} },
            {KeyboardKeys.BACKSLASH_UK, new int [] {4, 1} },
            {KeyboardKeys.Z, new int [] {4, 2} },
            {KeyboardKeys.X, new int [] {4, 3} },
            {KeyboardKeys.C, new int [] {4, 4} },
            {KeyboardKeys.V, new int [] {4, 5} },
            {KeyboardKeys.B, new int [] {4, 6} },
            {KeyboardKeys.N, new int [] {4, 7} },
            {KeyboardKeys.M, new int [] {4, 8} },
            {KeyboardKeys.COMMA, new int [] {4, 9} },
            {KeyboardKeys.PERIOD, new int [] {4, 10} },
            {KeyboardKeys.FORWARD_SLASH, new int [] {4, 11} },
            {KeyboardKeys.RIGHT_SHIFT, new int [] {4, 14} },
            {KeyboardKeys.ARROW_UP, new int [] {4, 16} },
            {KeyboardKeys.NUM_ONE, new int [] {4, 18} },
            {KeyboardKeys.NUM_TWO, new int [] {4, 19} },
            {KeyboardKeys.NUM_THREE, new int [] {4, 20} },
            {KeyboardKeys.NUM_ENTER, new int [] {4, 21} },
            {KeyboardKeys.LEFT_CONTROL, new int [] {5, 0} },
            {KeyboardKeys.LEFT_WINDOWS, new int [] {5, 1} },
            {KeyboardKeys.LEFT_ALT, new int [] {5, 2} },
            {KeyboardKeys.SPACE, new int [] {5, 6} },
            {KeyboardKeys.RIGHT_ALT, new int [] {5, 10} },
            {KeyboardKeys.RIGHT_WINDOWS, new int [] {5, 11} },
            {KeyboardKeys.FN_Key, new int [] {5, 12} },
            {KeyboardKeys.RIGHT_CONTROL, new int [] {5, 14} },
            {KeyboardKeys.ARROW_LEFT, new int [] {5, 15} },
            {KeyboardKeys.ARROW_DOWN, new int [] {5, 16} },
            {KeyboardKeys.ARROW_RIGHT, new int [] {5, 17} },
            {KeyboardKeys.NUM_ZERO, new int [] {5, 18} },
            {KeyboardKeys.NUM_PERIOD, new int [] {5, 20} },
            {KeyboardKeys.ADDITIONALLIGHT5, new int [] {6, 0} },
            {KeyboardKeys.ADDITIONALLIGHT6, new int [] {6, 1} },
            {KeyboardKeys.ADDITIONALLIGHT7, new int [] {6, 2} },
            {KeyboardKeys.ADDITIONALLIGHT8, new int [] {6, 3} },
            {KeyboardKeys.ADDITIONALLIGHT9, new int [] {6, 4} },
            {KeyboardKeys.ADDITIONALLIGHT10, new int [] {6, 5} },
            {KeyboardKeys.ADDITIONALLIGHT11, new int [] {6, 6} },
            {KeyboardKeys.ADDITIONALLIGHT12, new int [] {6, 7} },
            {KeyboardKeys.ADDITIONALLIGHT13, new int [] {6, 8} },
            {KeyboardKeys.ADDITIONALLIGHT14, new int [] {6, 9} },
            {KeyboardKeys.ADDITIONALLIGHT15, new int [] {6, 10} },
            {KeyboardKeys.ADDITIONALLIGHT16, new int [] {6, 11} },
            {KeyboardKeys.ADDITIONALLIGHT17, new int [] {6, 12} },
            {KeyboardKeys.ADDITIONALLIGHT18, new int [] {6, 13} },
            {KeyboardKeys.ADDITIONALLIGHT19, new int [] {6, 14} },
            {KeyboardKeys.ADDITIONALLIGHT20, new int [] {6, 15} },
            {KeyboardKeys.ADDITIONALLIGHT21, new int [] {6, 16} },
            {KeyboardKeys.ADDITIONALLIGHT22, new int [] {6, 17} },

        };

        public static readonly Dictionary<CoolerMasterSDK.DEVICE_INDEX, Dictionary<KeyboardKeys, int[]>> KeyboardLayoutMapping = new Dictionary<CoolerMasterSDK.DEVICE_INDEX, Dictionary<KeyboardKeys, int[]>>
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

        public bool IsInitialized()
        {
            return this.isInitialized;
        }

        public bool UpdateDevice(Color GlobalColor, List<DeviceLayout> devices, DoWorkEventArgs e, bool forced = false)
        {
            watch.Restart();

            bool updateResult = true;

            try
            {

                foreach (DeviceLayout layout in devices)
                {
                    switch (layout)
                    {
                        case KeyboardDeviceLayout kb:
                            if (!UpdateDevice(kb, e, forced))
                                updateResult = false;
                            break;
                        //case MouseDeviceLayout mouse:
                        //    if (!UpdateDevice(mouse, e, forced))
                        //        updateResult = false;
                        //    break;
                    }
                }
            }
            catch (Exception ex)
            {
                Global.logger.Error("CoolerMaster device, error when updating device: " + ex);
                return false;
            }

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return updateResult;
        }

        public bool UpdateDevice(KeyboardDeviceLayout keyboard, DoWorkEventArgs e, bool forced = false)
        {
            try
            {
                var coords = CoolerMasterKeys.KeyCoords;

                if (CoolerMasterKeys.KeyboardLayoutMapping.ContainsKey(CurrentDevice))
                    coords = CoolerMasterKeys.KeyboardLayoutMapping[CurrentDevice];

                foreach (KeyValuePair<LEDINT, Color> key in keyboard.DeviceColours.deviceColours)
                {
                    if (e.Cancel) return false;

                    int[] coordinates = new int[2];

                    KeyboardKeys dev_key = (KeyboardKeys)key.Key;

                    if (dev_key == KeyboardKeys.ENTER &&
                        (keyboard.Language != KeyboardDeviceLayout.PreferredKeyboardLocalization.us &&
                         keyboard.Language != KeyboardDeviceLayout.PreferredKeyboardLocalization.dvorak))
                        dev_key = KeyboardKeys.BACKSLASH;


                    if (dev_key == KeyboardKeys.ADDITIONALLIGHT10)
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

        private void SetOneKey(int[] key, Color color)
        {
            //color = Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(color, color.A / 255.0D));
            CoolerMasterSDK.KEY_COLOR key_color = new CoolerMasterSDK.KEY_COLOR(color.R, color.G, color.B);
            color_matrix.KeyColor[key[0], key[1]] = key_color;
        }

        private void SendColorsToKeyboard(bool forced = false)
        {
            if (!CoolerMasterSDK.Keyboards.Contains(CurrentDevice))
                return;

            CoolerMasterSDK.SetAllLedColor(color_matrix);
            //previous_key_colors = key_colors;
            keyboard_updated = true;
        }
    }
}
