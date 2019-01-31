using Aurora.Settings;
using LedCSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Devices.Logitech
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public enum Logitech_keyboardBitmapKeys
    {
        UNKNOWN = -1,
        ESC = 0,
        F1 = 4,
        F2 = 8,
        F3 = 12,
        F4 = 16,
        F5 = 20,
        F6 = 24,
        F7 = 28,
        F8 = 32,
        F9 = 36,
        F10 = 40,
        F11 = 44,
        F12 = 48,
        PRINT_SCREEN = 52,
        SCROLL_LOCK = 56,
        PAUSE_BREAK = 60,
        //64
        //68
        //72
        //76
        //80

        TILDE = 84,
        ONE = 88,
        TWO = 92,
        THREE = 96,
        FOUR = 100,
        FIVE = 104,
        SIX = 108,
        SEVEN = 112,
        EIGHT = 116,
        NINE = 120,
        ZERO = 124,
        MINUS = 128,
        EQUALS = 132,
        BACKSPACE = 136,
        INSERT = 140,
        HOME = 144,
        PAGE_UP = 148,
        NUM_LOCK = 152,
        NUM_SLASH = 156,
        NUM_ASTERISK = 160,
        NUM_MINUS = 164,

        TAB = 168,
        Q = 172,
        W = 176,
        E = 180,
        R = 184,
        T = 188,
        Y = 192,
        U = 196,
        I = 200,
        O = 204,
        P = 208,
        OPEN_BRACKET = 212,
        CLOSE_BRACKET = 216,
        BACKSLASH = 220,
        KEYBOARD_DELETE = 224,
        END = 228,
        PAGE_DOWN = 232,
        NUM_SEVEN = 236,
        NUM_EIGHT = 240,
        NUM_NINE = 244,
        NUM_PLUS = 248,

        CAPS_LOCK = 252,
        A = 256,
        S = 260,
        D = 264,
        F = 268,
        G = 272,
        H = 276,
        J = 280,
        K = 284,
        L = 288,
        SEMICOLON = 292,
        APOSTROPHE = 296,
        HASHTAG = 300,//300
        ENTER = 304,
        //308
        //312
        //316
        NUM_FOUR = 320,
        NUM_FIVE = 324,
        NUM_SIX = 328,
        //332

        LEFT_SHIFT = 336,
        BACKSLASH_UK = 340,
        Z = 344,
        X = 348,
        C = 352,
        V = 356,
        B = 360,
        N = 364,
        M = 368,
        COMMA = 372,
        PERIOD = 376,
        FORWARD_SLASH = 380,
        OEM102 = 384,
        RIGHT_SHIFT = 388,
        //392
        ARROW_UP = 396,
        //400
        NUM_ONE = 404,
        NUM_TWO = 408,
        NUM_THREE = 412,
        NUM_ENTER = 416,

        LEFT_CONTROL = 420,
        LEFT_WINDOWS = 424,
        LEFT_ALT = 428,
        //432
        JPN_MUHENKAN = 436,//436
        SPACE = 440,
        //444
        //448
        JPN_HENKAN = 452,//452
        JPN_HIRAGANA_KATAKANA = 456,//456
        //460
        RIGHT_ALT = 464,
        RIGHT_WINDOWS = 468,
        APPLICATION_SELECT = 472,
        RIGHT_CONTROL = 476,
        ARROW_LEFT = 480,
        ARROW_DOWN = 484,
        ARROW_RIGHT = 488,
        NUM_ZERO = 492,
        NUM_PERIOD = 496,
        //500
    };
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    class LogitechDevice : Device
    {
        private String devicename = "Logitech";
        private bool isInitialized = false;

        private bool keyboard_updated = false;
        private bool peripheral_updated = false;

        private readonly object action_lock = new object();

        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;
        private VariableRegistry default_registry = null;

        //Keyboard stuff
        private Logitech_keyboardBitmapKeys[] allKeys = Enum.GetValues(typeof(Logitech_keyboardBitmapKeys)).Cast<Logitech_keyboardBitmapKeys>().ToArray();
        private byte[] bitmap = new byte[LogitechGSDK.LOGI_LED_BITMAP_SIZE];
        private Color peripheral_Color = Color.Black;
        //Previous data
        private byte[] previous_bitmap = new byte[LogitechGSDK.LOGI_LED_BITMAP_SIZE];
        private Color previous_peripheral_Color = Color.Black;

        public bool Initialize()
        {
            lock (action_lock)
            {
                if (!isInitialized)
                {
                    try
                    {
                        LogitechGSDK.LGDLL? dll = null;
                        if (Global.Configuration.VarRegistry.GetVariable<bool>($"{devicename}_override_dll"))
                            dll = Global.Configuration.VarRegistry.GetVariable<LogitechGSDK.LGDLL>($"{devicename}_override_dll_option");

                        LogitechGSDK.InitDLL(dll);

                        if (!LogitechGSDK.LogiLedInit())
                        {
                            Global.logger.Error("Logitech LED SDK could not be initialized.");

                            isInitialized = false;
                            return false;
                        }

                        if (LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_ALL) && LogitechGSDK.LogiLedSaveCurrentLighting())
                        {
                            if (Global.Configuration.logitech_first_time)
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    LogitechInstallInstructions instructions = new LogitechInstallInstructions();
                                    instructions.ShowDialog();
                                });
                                Global.Configuration.logitech_first_time = false;
                                Settings.ConfigManager.Save(Global.Configuration);
                            }

                            if (Global.Configuration.VarRegistry.GetVariable<bool>($"{devicename}_set_default"))
                            {
                                Color default_color = Global.Configuration.VarRegistry.GetVariable<Aurora.Utils.RealColor>($"{devicename}_default_color").GetDrawingColor();
                                double alpha_amt = (default_color.A / 255.0);
                                int red_amt = (int)(((default_color.R * alpha_amt) / 255.0) * 100.0);
                                int green_amt = (int)(((default_color.G * alpha_amt) / 255.0) * 100.0);
                                int blue_amt = (int)(((default_color.B * alpha_amt) / 255.0) * 100.0);
                                LogitechGSDK.LogiLedSetLighting(red_amt, green_amt, blue_amt);
                            }

                            isInitialized = true;
                            return true;
                        }
                        else
                        {
                            Global.logger.Error("Logitech LED SDK could not be initialized. (LogiLedSetTargetDevice or LogiLedSaveCurrentLighting failed)");

                            isInitialized = false;
                            return false;
                        }


                    }
                    catch (Exception exc)
                    {
                        Global.logger.Error("There was an error initializing Logitech LED SDK.\r\n" + exc.Message);

                        return false;
                    }
                }

                return isInitialized;
            }
        }

        public void Shutdown()
        {
            lock (action_lock)
            {
                if (isInitialized)
                {
                    this.Reset();
                    LogitechGSDK.LogiLedShutdown();
                    isInitialized = false;
                }
            }
        }

        public string GetDeviceDetails()
        {
            if (isInitialized)
            {
                return devicename + ": Connected";
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
                LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_ALL);
                LogitechGSDK.LogiLedRestoreLighting();
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

        private void SetOneKey(Logitech_keyboardBitmapKeys key, Color color)
        {
            if (color.A != 255)
                color = Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(color, color.A / 255.0D));

            bitmap[(int)key] = color.B;
            bitmap[(int)key + 1] = color.G;
            bitmap[(int)key + 2] = color.R;
            bitmap[(int)key + 3] = 255;
        }


        private void SendColorsToKeyboard(bool forced = false)
        {
            if (!Enumerable.SequenceEqual(bitmap, previous_bitmap) || forced)
            {
                LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_PERKEY_RGB);

                LogitechGSDK.LogiLedSetLightingFromBitmap(bitmap);
                bitmap.CopyTo(previous_bitmap, 0);
                keyboard_updated = true;
            }
        }

        private void SendColorToPeripheral(Color color, bool forced = false)
        {
            if (!previous_peripheral_Color.Equals(color) || forced)
            {
                LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_RGB);

                if (Global.Configuration.allow_peripheral_devices)
                {
                    double alpha_amt = (color.A / 255.0);
                    int red_amt = (int)(((color.R * alpha_amt) / 255.0) * 100.0);
                    int green_amt = (int)(((color.G * alpha_amt) / 255.0) * 100.0);
                    int blue_amt = (int)(((color.B * alpha_amt) / 255.0) * 100.0);

                    //System.Diagnostics.Debug.WriteLine("R: " + red_amt + " G: " + green_amt + " B: " + blue_amt);

                    LogitechGSDK.LogiLedSetLighting(red_amt, green_amt, blue_amt);
                    previous_peripheral_Color = color;
                    peripheral_updated = true;
                }
                else
                {
                    if (peripheral_updated)
                    {
                        LogitechGSDK.LogiLedRestoreLighting();
                        peripheral_updated = false;
                    }
                }
            }
        }

        public bool IsInitialized()
        {
            return this.isInitialized;
        }

        private void SetZoneColor(byte deviceType, int zone, byte red, byte green, byte blue)
        {
            LogitechGSDK.LogiLedSetLightingForTargetZone(deviceType, zone
                , (int)Math.Round((double)(100 * red) / 255.0f)
                , (int)Math.Round((double)(100 * green) / 255.0f)
                , (int)Math.Round((double)(100 * blue) / 255.0f));
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            try
            {
                bool isZoneKeyboard = (Global.Configuration.keyboard_brand == PreferredKeyboard.Logitech_G213);

                if (!Global.Configuration.devices_disable_keyboard && !isZoneKeyboard)
                {
                    foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
                    {
                        if (e.Cancel) return false;

                        Logitech_keyboardBitmapKeys localKey = ToLogitechBitmap(key.Key);

                        if (localKey == Logitech_keyboardBitmapKeys.UNKNOWN &&
                            (key.Key == DeviceKeys.Peripheral_Logo || key.Key == DeviceKeys.Peripheral))
                        {
                            if (!Global.Configuration.devices_disable_mouse ||
                                !Global.Configuration.devices_disable_headset)
                                SendColorToPeripheral((Color)key.Value, forced || !peripheral_updated);
                        }
                        else if (localKey == Logitech_keyboardBitmapKeys.UNKNOWN)
                        {
                            double alpha_amt = (key.Value.A / 255.0);
                            int red_amt = (int)(((key.Value.R * alpha_amt) / 255.0) * 100.0);
                            int green_amt = (int)(((key.Value.G * alpha_amt) / 255.0) * 100.0);
                            int blue_amt = (int)(((key.Value.B * alpha_amt) / 255.0) * 100.0);

                            if (!Global.Configuration.devices_disable_keyboard)
                            {
                                LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_PERKEY_RGB);
                                switch (key.Key)
                                {
                                    case DeviceKeys.OEM8:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithHidCode(220, red_amt, green_amt,
                                            blue_amt);
                                        break;
                                    case DeviceKeys.G1:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(keyboardNames.G_1, red_amt,
                                            green_amt, blue_amt);
                                        break;
                                    case DeviceKeys.G2:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(keyboardNames.G_2, red_amt,
                                            green_amt, blue_amt);
                                        break;
                                    case DeviceKeys.G3:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(keyboardNames.G_3, red_amt,
                                            green_amt, blue_amt);
                                        break;
                                    case DeviceKeys.G4:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(keyboardNames.G_4, red_amt,
                                            green_amt, blue_amt);
                                        break;
                                    case DeviceKeys.G5:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(keyboardNames.G_5, red_amt,
                                            green_amt, blue_amt);
                                        break;
                                    case DeviceKeys.G6:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(keyboardNames.G_6, red_amt,
                                            green_amt, blue_amt);
                                        break;
                                    case DeviceKeys.G7:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(keyboardNames.G_7, red_amt,
                                            green_amt, blue_amt);
                                        break;
                                    case DeviceKeys.G8:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(keyboardNames.G_8, red_amt,
                                            green_amt, blue_amt);
                                        break;
                                    case DeviceKeys.G9:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(keyboardNames.G_9, red_amt,
                                            green_amt, blue_amt);
                                        break;
                                    case DeviceKeys.LOGO:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(keyboardNames.G_LOGO, red_amt,
                                            green_amt, blue_amt);
                                        break;
                                    case DeviceKeys.LOGO2:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(keyboardNames.G_BADGE, red_amt,
                                            green_amt, blue_amt);
                                        break;
                                }
                            }
                        }
                        else if (localKey != Logitech_keyboardBitmapKeys.UNKNOWN)
                        {
                            if (!Global.Configuration.devices_disable_keyboard)
                            {
                                LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_PERKEY_RGB);
                                SetOneKey(localKey, (Color)key.Value);
                            }
                        }
                    }
                }

                else if (!Global.Configuration.devices_disable_keyboard && isZoneKeyboard)
                {
                    List<Color> leftColor = new List<Color>();
                    List<Color> centerColor = new List<Color>();
                    List<Color> rightColor = new List<Color>();
                    List<Color> arrowColor = new List<Color>();
                    List<Color> numpadColor = new List<Color>();

                    foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
                    {
                        if (e.Cancel) return false;

                        Logitech_keyboardBitmapKeys localKey = ToLogitechBitmap(key.Key);

                        if (localKey == Logitech_keyboardBitmapKeys.UNKNOWN &&
                            (key.Key == DeviceKeys.Peripheral_Logo || key.Key == DeviceKeys.Peripheral))
                        {
                            if (!Global.Configuration.devices_disable_mouse ||
                                !Global.Configuration.devices_disable_headset)
                                SendColorToPeripheral((Color)key.Value, forced || !peripheral_updated);
                        }
                        else if (localKey == Logitech_keyboardBitmapKeys.UNKNOWN)
                        {
                            double alpha_amt = (key.Value.A / 255.0);
                            int red_amt = (int)(((key.Value.R * alpha_amt) / 255.0) * 100.0);
                            int green_amt = (int)(((key.Value.G * alpha_amt) / 255.0) * 100.0);
                            int blue_amt = (int)(((key.Value.B * alpha_amt) / 255.0) * 100.0);

                            if (!Global.Configuration.devices_disable_keyboard)
                            {
                                LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_PERKEY_RGB);
                                switch (key.Key)
                                {
                                    case DeviceKeys.OEM8:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithHidCode(220, red_amt, green_amt,
                                            blue_amt);
                                        break;
                                    case DeviceKeys.G1:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(keyboardNames.G_1, red_amt,
                                            green_amt, blue_amt);
                                        break;
                                    case DeviceKeys.G2:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(keyboardNames.G_2, red_amt,
                                            green_amt, blue_amt);
                                        break;
                                    case DeviceKeys.G3:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(keyboardNames.G_3, red_amt,
                                            green_amt, blue_amt);
                                        break;
                                    case DeviceKeys.G4:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(keyboardNames.G_4, red_amt,
                                            green_amt, blue_amt);
                                        break;
                                    case DeviceKeys.G5:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(keyboardNames.G_5, red_amt,
                                            green_amt, blue_amt);
                                        break;
                                    case DeviceKeys.G6:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(keyboardNames.G_6, red_amt,
                                            green_amt, blue_amt);
                                        break;
                                    case DeviceKeys.G7:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(keyboardNames.G_7, red_amt,
                                            green_amt, blue_amt);
                                        break;
                                    case DeviceKeys.G8:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(keyboardNames.G_8, red_amt,
                                            green_amt, blue_amt);
                                        break;
                                    case DeviceKeys.G9:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(keyboardNames.G_9, red_amt,
                                            green_amt, blue_amt);
                                        break;
                                    case DeviceKeys.LOGO:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(keyboardNames.G_LOGO, red_amt,
                                            green_amt, blue_amt);
                                        break;
                                    case DeviceKeys.LOGO2:
                                        LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(keyboardNames.G_BADGE, red_amt,
                                            green_amt, blue_amt);
                                        break;
                                }
                            }
                        }
                        else if (localKey != Logitech_keyboardBitmapKeys.UNKNOWN)
                        {
                            //left
                            if ((localKey == Logitech_keyboardBitmapKeys.ESC /*|| localKey == Logitech_keyboardBitmapKeys.LEFT_FN*/ || localKey == Logitech_keyboardBitmapKeys.TILDE
                                || localKey == Logitech_keyboardBitmapKeys.TAB
                                || localKey == Logitech_keyboardBitmapKeys.CAPS_LOCK || localKey == Logitech_keyboardBitmapKeys.LEFT_SHIFT
                                || localKey == Logitech_keyboardBitmapKeys.LEFT_CONTROL || localKey == Logitech_keyboardBitmapKeys.F1
                                || localKey == Logitech_keyboardBitmapKeys.ONE || localKey == Logitech_keyboardBitmapKeys.Q
                                || localKey == Logitech_keyboardBitmapKeys.A || localKey == Logitech_keyboardBitmapKeys.Z
                                || localKey == Logitech_keyboardBitmapKeys.LEFT_WINDOWS || localKey == Logitech_keyboardBitmapKeys.F2
                                || localKey == Logitech_keyboardBitmapKeys.W || localKey == Logitech_keyboardBitmapKeys.S || localKey == Logitech_keyboardBitmapKeys.X
                                || localKey == Logitech_keyboardBitmapKeys.LEFT_ALT
                                || localKey == Logitech_keyboardBitmapKeys.F3 || localKey == Logitech_keyboardBitmapKeys.THREE)
                                && (key.Value.R >= 0 || key.Value.G >= 0 || key.Value.B >= 0))
                            {
                                leftColor.Add(key.Value);
                            }
                            //right
                            else if ((localKey == Logitech_keyboardBitmapKeys.F11
                                || localKey == Logitech_keyboardBitmapKeys.BACKSPACE
                                || localKey == Logitech_keyboardBitmapKeys.APOSTROPHE || localKey == Logitech_keyboardBitmapKeys.RIGHT_SHIFT
                                || localKey == Logitech_keyboardBitmapKeys.F6 || localKey == Logitech_keyboardBitmapKeys.SIX || localKey == Logitech_keyboardBitmapKeys.T
                                || localKey == Logitech_keyboardBitmapKeys.G || localKey == Logitech_keyboardBitmapKeys.B
                                || localKey == Logitech_keyboardBitmapKeys.MINUS
                                || localKey == Logitech_keyboardBitmapKeys.FORWARD_SLASH || localKey == Logitech_keyboardBitmapKeys.ENTER
                                || localKey == Logitech_keyboardBitmapKeys.RIGHT_CONTROL || localKey == Logitech_keyboardBitmapKeys.CLOSE_BRACKET)
                                && (key.Value.R >= 0 || key.Value.G >= 0 || key.Value.B >= 0))
                            {
                                rightColor.Add(key.Value);
                            }
                            //center
                            else if ((localKey == Logitech_keyboardBitmapKeys.Y || localKey == Logitech_keyboardBitmapKeys.H || localKey == Logitech_keyboardBitmapKeys.B
                                || localKey == Logitech_keyboardBitmapKeys.U || localKey == Logitech_keyboardBitmapKeys.J || localKey == Logitech_keyboardBitmapKeys.SEMICOLON
                                || localKey == Logitech_keyboardBitmapKeys.N || localKey == Logitech_keyboardBitmapKeys.I || localKey == Logitech_keyboardBitmapKeys.K
                                || localKey == Logitech_keyboardBitmapKeys.M || localKey == Logitech_keyboardBitmapKeys.O || localKey == Logitech_keyboardBitmapKeys.PERIOD
                                || localKey == Logitech_keyboardBitmapKeys.L || localKey == Logitech_keyboardBitmapKeys.COMMA || localKey == Logitech_keyboardBitmapKeys.LEFT_ALT
                                || localKey == Logitech_keyboardBitmapKeys.F8 || localKey == Logitech_keyboardBitmapKeys.F9
                                || localKey == Logitech_keyboardBitmapKeys.F10 || localKey == Logitech_keyboardBitmapKeys.F11 || localKey == Logitech_keyboardBitmapKeys.EIGHT
                                || localKey == Logitech_keyboardBitmapKeys.NINE || localKey == Logitech_keyboardBitmapKeys.ZERO
                                || localKey == Logitech_keyboardBitmapKeys.RIGHT_CONTROL || localKey == Logitech_keyboardBitmapKeys.RIGHT_ALT
                                || localKey == Logitech_keyboardBitmapKeys.BACKSLASH || localKey == Logitech_keyboardBitmapKeys.OPEN_BRACKET
                                || localKey == Logitech_keyboardBitmapKeys.F4
                                || localKey == Logitech_keyboardBitmapKeys.FOUR || localKey == Logitech_keyboardBitmapKeys.E || localKey == Logitech_keyboardBitmapKeys.D
                                || localKey == Logitech_keyboardBitmapKeys.C
                                || localKey == Logitech_keyboardBitmapKeys.F5 || localKey == Logitech_keyboardBitmapKeys.FIVE || localKey == Logitech_keyboardBitmapKeys.R
                                || localKey == Logitech_keyboardBitmapKeys.F || localKey == Logitech_keyboardBitmapKeys.V
                                || localKey == Logitech_keyboardBitmapKeys.F6 || localKey == Logitech_keyboardBitmapKeys.SIX || localKey == Logitech_keyboardBitmapKeys.T
                                || localKey == Logitech_keyboardBitmapKeys.G || localKey == Logitech_keyboardBitmapKeys.B
                                || localKey == Logitech_keyboardBitmapKeys.Y || localKey == Logitech_keyboardBitmapKeys.H || localKey == Logitech_keyboardBitmapKeys.SEVEN)
                                && (key.Value.R >= 0 || key.Value.G >= 0 || key.Value.B >= 0))
                            {
                                centerColor.Add(key.Value);
                            }

                            //arrow
                            else if ((localKey == Logitech_keyboardBitmapKeys.PRINT_SCREEN || localKey == Logitech_keyboardBitmapKeys.SCROLL_LOCK
                                || localKey == Logitech_keyboardBitmapKeys.PAUSE_BREAK || localKey == Logitech_keyboardBitmapKeys.INSERT
                                || localKey == Logitech_keyboardBitmapKeys.HOME || localKey == Logitech_keyboardBitmapKeys.PAGE_UP
                                || localKey == Logitech_keyboardBitmapKeys.KEYBOARD_DELETE || localKey == Logitech_keyboardBitmapKeys.END
                                || localKey == Logitech_keyboardBitmapKeys.PAGE_DOWN || localKey == Logitech_keyboardBitmapKeys.ARROW_UP
                                || localKey == Logitech_keyboardBitmapKeys.ARROW_DOWN || localKey == Logitech_keyboardBitmapKeys.ARROW_LEFT
                                || localKey == Logitech_keyboardBitmapKeys.ARROW_RIGHT)
                                && (key.Value.R >= 0 || key.Value.G >= 0 || key.Value.B >= 0))
                            {
                                arrowColor.Add(key.Value);
                            }

                            //numpad
                            else if (localKey == Logitech_keyboardBitmapKeys.NUM_ASTERISK || localKey == Logitech_keyboardBitmapKeys.NUM_EIGHT
                                || localKey == Logitech_keyboardBitmapKeys.NUM_ENTER || localKey == Logitech_keyboardBitmapKeys.NUM_FIVE
                                || localKey == Logitech_keyboardBitmapKeys.NUM_FOUR || localKey == Logitech_keyboardBitmapKeys.NUM_LOCK
                                || localKey == Logitech_keyboardBitmapKeys.NUM_MINUS || localKey == Logitech_keyboardBitmapKeys.NUM_NINE
                                || localKey == Logitech_keyboardBitmapKeys.NUM_ONE || localKey == Logitech_keyboardBitmapKeys.NUM_PERIOD
                                || localKey == Logitech_keyboardBitmapKeys.NUM_PLUS || localKey == Logitech_keyboardBitmapKeys.NUM_SEVEN
                                || localKey == Logitech_keyboardBitmapKeys.NUM_SIX || localKey == Logitech_keyboardBitmapKeys.NUM_SLASH
                                || localKey == Logitech_keyboardBitmapKeys.NUM_THREE || localKey == Logitech_keyboardBitmapKeys.NUM_TWO
                                || localKey == Logitech_keyboardBitmapKeys.NUM_ZERO /*|| localKey == Logitech_keyboardBitmapKeys.NUM_ZEROZERO)*/
                                && (key.Value.R >= 0 || key.Value.G >= 0 || key.Value.B >= 0))
                            {
                                numpadColor.Add(key.Value);
                            }
                        }
                    }

                    if (leftColor.Any())
                    {
                        Color mostUsed = leftColor.GroupBy(item => item).OrderByDescending(item => item.Count())
                                         .Select(item => new { Color = item.Key, Count = item.Count() })
                                         .First().Color;
                        int a = (int)Math.Round((double)(100 * mostUsed.R) / 255.0f);
                        SetZoneColor(0x0, 1, mostUsed.R, mostUsed.G, mostUsed.B);
                    }
                    else
                    {
                        SetZoneColor(0x0, 1, 0, 0, 0);
                    }

                    if (centerColor.Any())
                    {
                        Color mostUsed = centerColor.GroupBy(item => item).OrderByDescending(item => item.Count())
                                        .Select(item => new { Color = item.Key, Count = item.Count() })
                                        .First().Color;
                        SetZoneColor(0x0, 2, mostUsed.R, mostUsed.G, mostUsed.B);
                    }
                    else
                    {
                        SetZoneColor(0x0, 2, 0, 0, 0);
                    }

                    if (rightColor.Any())
                    {
                        Color mostUsed = rightColor.GroupBy(item => item).OrderByDescending(item => item.Count())
                                           .Select(item => new { Color = item.Key, Count = item.Count() })
                                           .First().Color;
                        SetZoneColor(0x0, 3, mostUsed.R, mostUsed.G, mostUsed.B);
                    }
                    else
                    {
                        SetZoneColor(0x0, 3, 0, 0, 0);
                    }

                    if (arrowColor.Any())
                    {
                        Color mostUsed = arrowColor.GroupBy(item => item).OrderByDescending(item => item.Count())
                                           .Select(item => new { Color = item.Key, Count = item.Count() })
                                           .First().Color;
                        SetZoneColor(0x0, 4, mostUsed.R, mostUsed.G, mostUsed.B);
                    }
                    else
                    {
                        SetZoneColor(0x0, 4, 0, 0, 0);
                    }

                    if (numpadColor.Any())
                    {
                        Color mostUsed = numpadColor.GroupBy(item => item).OrderByDescending(item => item.Count())
                                           .Select(item => new { Color = item.Key, Count = item.Count() })
                                           .First().Color;
                        SetZoneColor(0x0, 5, mostUsed.R, mostUsed.G, mostUsed.B);
                    }
                    else
                    {
                        SetZoneColor(0x0, 5, 0, 0, 0);
                    }
                }

                if (e.Cancel)
                {
                    return false;
                }

                if (!Global.Configuration.devices_disable_keyboard && !isZoneKeyboard)
                {
                    SendColorsToKeyboard(forced || !keyboard_updated);
                }
                return true;
            }
            catch (Exception exc)
            {
                Global.logger.Error(exc.ToString());
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

        public static DeviceKeys ToDeviceKey(keyboardNames key)
        {
            switch (key)
            {
                case (keyboardNames.ESC):
                    return DeviceKeys.ESC;
                case (keyboardNames.F1):
                    return DeviceKeys.F1;
                case (keyboardNames.F2):
                    return DeviceKeys.F2;
                case (keyboardNames.F3):
                    return DeviceKeys.F3;
                case (keyboardNames.F4):
                    return DeviceKeys.F4;
                case (keyboardNames.F5):
                    return DeviceKeys.F5;
                case (keyboardNames.F6):
                    return DeviceKeys.F6;
                case (keyboardNames.F7):
                    return DeviceKeys.F7;
                case (keyboardNames.F8):
                    return DeviceKeys.F8;
                case (keyboardNames.F9):
                    return DeviceKeys.F9;
                case (keyboardNames.F10):
                    return DeviceKeys.F10;
                case (keyboardNames.F11):
                    return DeviceKeys.F11;
                case (keyboardNames.F12):
                    return DeviceKeys.F12;
                case (keyboardNames.PRINT_SCREEN):
                    return DeviceKeys.PRINT_SCREEN;
                case (keyboardNames.SCROLL_LOCK):
                    return DeviceKeys.SCROLL_LOCK;
                case (keyboardNames.PAUSE_BREAK):
                    return DeviceKeys.PAUSE_BREAK;
                case (keyboardNames.TILDE):
                    return DeviceKeys.TILDE;
                case (keyboardNames.ONE):
                    return DeviceKeys.ONE;
                case (keyboardNames.TWO):
                    return DeviceKeys.TWO;
                case (keyboardNames.THREE):
                    return DeviceKeys.THREE;
                case (keyboardNames.FOUR):
                    return DeviceKeys.FOUR;
                case (keyboardNames.FIVE):
                    return DeviceKeys.FIVE;
                case (keyboardNames.SIX):
                    return DeviceKeys.SIX;
                case (keyboardNames.SEVEN):
                    return DeviceKeys.SEVEN;
                case (keyboardNames.EIGHT):
                    return DeviceKeys.EIGHT;
                case (keyboardNames.NINE):
                    return DeviceKeys.NINE;
                case (keyboardNames.ZERO):
                    return DeviceKeys.ZERO;
                case (keyboardNames.MINUS):
                    return DeviceKeys.MINUS;
                case (keyboardNames.EQUALS):
                    return DeviceKeys.EQUALS;
                case (keyboardNames.BACKSPACE):
                    return DeviceKeys.BACKSPACE;
                case (keyboardNames.INSERT):
                    return DeviceKeys.INSERT;
                case (keyboardNames.HOME):
                    return DeviceKeys.HOME;
                case (keyboardNames.PAGE_UP):
                    return DeviceKeys.PAGE_UP;
                case (keyboardNames.NUM_LOCK):
                    return DeviceKeys.NUM_LOCK;
                case (keyboardNames.NUM_SLASH):
                    return DeviceKeys.NUM_SLASH;
                case (keyboardNames.NUM_ASTERISK):
                    return DeviceKeys.NUM_ASTERISK;
                case (keyboardNames.NUM_MINUS):
                    return DeviceKeys.NUM_MINUS;
                case (keyboardNames.TAB):
                    return DeviceKeys.TAB;
                case (keyboardNames.Q):
                    return DeviceKeys.Q;
                case (keyboardNames.W):
                    return DeviceKeys.W;
                case (keyboardNames.E):
                    return DeviceKeys.E;
                case (keyboardNames.R):
                    return DeviceKeys.R;
                case (keyboardNames.T):
                    return DeviceKeys.T;
                case (keyboardNames.Y):
                    return DeviceKeys.Y;
                case (keyboardNames.U):
                    return DeviceKeys.U;
                case (keyboardNames.I):
                    return DeviceKeys.I;
                case (keyboardNames.O):
                    return DeviceKeys.O;
                case (keyboardNames.P):
                    return DeviceKeys.P;
                case (keyboardNames.OPEN_BRACKET):
                    return DeviceKeys.OPEN_BRACKET;
                case (keyboardNames.CLOSE_BRACKET):
                    return DeviceKeys.CLOSE_BRACKET;
                case (keyboardNames.BACKSLASH):
                    return DeviceKeys.BACKSLASH;
                case (keyboardNames.KEYBOARD_DELETE):
                    return DeviceKeys.DELETE;
                case (keyboardNames.END):
                    return DeviceKeys.END;
                case (keyboardNames.PAGE_DOWN):
                    return DeviceKeys.PAGE_DOWN;
                case (keyboardNames.NUM_SEVEN):
                    return DeviceKeys.NUM_SEVEN;
                case (keyboardNames.NUM_EIGHT):
                    return DeviceKeys.NUM_EIGHT;
                case (keyboardNames.NUM_NINE):
                    return DeviceKeys.NUM_NINE;
                case (keyboardNames.NUM_PLUS):
                    return DeviceKeys.NUM_PLUS;
                case (keyboardNames.CAPS_LOCK):
                    return DeviceKeys.CAPS_LOCK;
                case (keyboardNames.A):
                    return DeviceKeys.A;
                case (keyboardNames.S):
                    return DeviceKeys.S;
                case (keyboardNames.D):
                    return DeviceKeys.D;
                case (keyboardNames.F):
                    return DeviceKeys.F;
                case (keyboardNames.G):
                    return DeviceKeys.G;
                case (keyboardNames.H):
                    return DeviceKeys.H;
                case (keyboardNames.J):
                    return DeviceKeys.J;
                case (keyboardNames.K):
                    return DeviceKeys.K;
                case (keyboardNames.L):
                    return DeviceKeys.L;
                case (keyboardNames.SEMICOLON):
                    return DeviceKeys.SEMICOLON;
                case (keyboardNames.APOSTROPHE):
                    return DeviceKeys.APOSTROPHE;
                //case (keyboardNames.HASHTAG):
                //    return DeviceKeys.HASHTAG;
                case (keyboardNames.ENTER):
                    return DeviceKeys.ENTER;
                case (keyboardNames.NUM_FOUR):
                    return DeviceKeys.NUM_FOUR;
                case (keyboardNames.NUM_FIVE):
                    return DeviceKeys.NUM_FIVE;
                case (keyboardNames.NUM_SIX):
                    return DeviceKeys.NUM_SIX;
                case (keyboardNames.LEFT_SHIFT):
                    return DeviceKeys.LEFT_SHIFT;
                //case (keyboardNames.BACKSLASH_UK):
                //    return DeviceKeys.BACKSLASH_UK;
                case (keyboardNames.Z):
                    return DeviceKeys.Z;
                case (keyboardNames.X):
                    return DeviceKeys.X;
                case (keyboardNames.C):
                    return DeviceKeys.C;
                case (keyboardNames.V):
                    return DeviceKeys.V;
                case (keyboardNames.B):
                    return DeviceKeys.B;
                case (keyboardNames.N):
                    return DeviceKeys.N;
                case (keyboardNames.M):
                    return DeviceKeys.M;
                case (keyboardNames.COMMA):
                    return DeviceKeys.COMMA;
                case (keyboardNames.PERIOD):
                    return DeviceKeys.PERIOD;
                case (keyboardNames.FORWARD_SLASH):
                    return DeviceKeys.FORWARD_SLASH;
                case (keyboardNames.RIGHT_SHIFT):
                    return DeviceKeys.RIGHT_SHIFT;
                case (keyboardNames.ARROW_UP):
                    return DeviceKeys.ARROW_UP;
                case (keyboardNames.NUM_ONE):
                    return DeviceKeys.NUM_ONE;
                case (keyboardNames.NUM_TWO):
                    return DeviceKeys.NUM_TWO;
                case (keyboardNames.NUM_THREE):
                    return DeviceKeys.NUM_THREE;
                case (keyboardNames.NUM_ENTER):
                    return DeviceKeys.NUM_ENTER;
                case (keyboardNames.LEFT_CONTROL):
                    return DeviceKeys.LEFT_CONTROL;
                case (keyboardNames.LEFT_WINDOWS):
                    return DeviceKeys.LEFT_WINDOWS;
                case (keyboardNames.LEFT_ALT):
                    return DeviceKeys.LEFT_ALT;
                case (keyboardNames.SPACE):
                    return DeviceKeys.SPACE;
                case (keyboardNames.RIGHT_ALT):
                    return DeviceKeys.RIGHT_ALT;
                case (keyboardNames.RIGHT_WINDOWS):
                    return DeviceKeys.RIGHT_WINDOWS;
                case (keyboardNames.APPLICATION_SELECT):
                    return DeviceKeys.APPLICATION_SELECT;
                case (keyboardNames.RIGHT_CONTROL):
                    return DeviceKeys.RIGHT_CONTROL;
                case (keyboardNames.ARROW_LEFT):
                    return DeviceKeys.ARROW_LEFT;
                case (keyboardNames.ARROW_DOWN):
                    return DeviceKeys.ARROW_DOWN;
                case (keyboardNames.ARROW_RIGHT):
                    return DeviceKeys.ARROW_RIGHT;
                case (keyboardNames.NUM_ZERO):
                    return DeviceKeys.NUM_ZERO;
                case (keyboardNames.NUM_PERIOD):
                    return DeviceKeys.NUM_PERIOD;
                default:
                    return DeviceKeys.NONE;
            }
        }

        public static Logitech_keyboardBitmapKeys ToLogitechBitmap(DeviceKeys key)
        {
            switch (key)
            {
                case (DeviceKeys.ESC):
                    return Logitech_keyboardBitmapKeys.ESC;
                case (DeviceKeys.F1):
                    return Logitech_keyboardBitmapKeys.F1;
                case (DeviceKeys.F2):
                    return Logitech_keyboardBitmapKeys.F2;
                case (DeviceKeys.F3):
                    return Logitech_keyboardBitmapKeys.F3;
                case (DeviceKeys.F4):
                    return Logitech_keyboardBitmapKeys.F4;
                case (DeviceKeys.F5):
                    return Logitech_keyboardBitmapKeys.F5;
                case (DeviceKeys.F6):
                    return Logitech_keyboardBitmapKeys.F6;
                case (DeviceKeys.F7):
                    return Logitech_keyboardBitmapKeys.F7;
                case (DeviceKeys.F8):
                    return Logitech_keyboardBitmapKeys.F8;
                case (DeviceKeys.F9):
                    return Logitech_keyboardBitmapKeys.F9;
                case (DeviceKeys.F10):
                    return Logitech_keyboardBitmapKeys.F10;
                case (DeviceKeys.F11):
                    return Logitech_keyboardBitmapKeys.F11;
                case (DeviceKeys.F12):
                    return Logitech_keyboardBitmapKeys.F12;
                case (DeviceKeys.PRINT_SCREEN):
                    return Logitech_keyboardBitmapKeys.PRINT_SCREEN;
                case (DeviceKeys.SCROLL_LOCK):
                    return Logitech_keyboardBitmapKeys.SCROLL_LOCK;
                case (DeviceKeys.PAUSE_BREAK):
                    return Logitech_keyboardBitmapKeys.PAUSE_BREAK;
                case (DeviceKeys.JPN_HALFFULLWIDTH):
                    return Logitech_keyboardBitmapKeys.TILDE;
                case (DeviceKeys.OEM5):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.jpn)
                        return Logitech_keyboardBitmapKeys.UNKNOWN;
                    else
                        return Logitech_keyboardBitmapKeys.TILDE;
                case (DeviceKeys.TILDE):
                    return Logitech_keyboardBitmapKeys.TILDE;
                case (DeviceKeys.ONE):
                    return Logitech_keyboardBitmapKeys.ONE;
                case (DeviceKeys.TWO):
                    return Logitech_keyboardBitmapKeys.TWO;
                case (DeviceKeys.THREE):
                    return Logitech_keyboardBitmapKeys.THREE;
                case (DeviceKeys.FOUR):
                    return Logitech_keyboardBitmapKeys.FOUR;
                case (DeviceKeys.FIVE):
                    return Logitech_keyboardBitmapKeys.FIVE;
                case (DeviceKeys.SIX):
                    return Logitech_keyboardBitmapKeys.SIX;
                case (DeviceKeys.SEVEN):
                    return Logitech_keyboardBitmapKeys.SEVEN;
                case (DeviceKeys.EIGHT):
                    return Logitech_keyboardBitmapKeys.EIGHT;
                case (DeviceKeys.NINE):
                    return Logitech_keyboardBitmapKeys.NINE;
                case (DeviceKeys.ZERO):
                    return Logitech_keyboardBitmapKeys.ZERO;
                case (DeviceKeys.MINUS):
                    return Logitech_keyboardBitmapKeys.MINUS;
                case (DeviceKeys.EQUALS):
                    return Logitech_keyboardBitmapKeys.EQUALS;
                case (DeviceKeys.BACKSPACE):
                    return Logitech_keyboardBitmapKeys.BACKSPACE;
                case (DeviceKeys.INSERT):
                    return Logitech_keyboardBitmapKeys.INSERT;
                case (DeviceKeys.HOME):
                    return Logitech_keyboardBitmapKeys.HOME;
                case (DeviceKeys.PAGE_UP):
                    return Logitech_keyboardBitmapKeys.PAGE_UP;
                case (DeviceKeys.NUM_LOCK):
                    return Logitech_keyboardBitmapKeys.NUM_LOCK;
                case (DeviceKeys.NUM_SLASH):
                    return Logitech_keyboardBitmapKeys.NUM_SLASH;
                case (DeviceKeys.NUM_ASTERISK):
                    return Logitech_keyboardBitmapKeys.NUM_ASTERISK;
                case (DeviceKeys.NUM_MINUS):
                    return Logitech_keyboardBitmapKeys.NUM_MINUS;
                case (DeviceKeys.TAB):
                    return Logitech_keyboardBitmapKeys.TAB;
                case (DeviceKeys.Q):
                    return Logitech_keyboardBitmapKeys.Q;
                case (DeviceKeys.W):
                    return Logitech_keyboardBitmapKeys.W;
                case (DeviceKeys.E):
                    return Logitech_keyboardBitmapKeys.E;
                case (DeviceKeys.R):
                    return Logitech_keyboardBitmapKeys.R;
                case (DeviceKeys.T):
                    return Logitech_keyboardBitmapKeys.T;
                case (DeviceKeys.Y):
                    return Logitech_keyboardBitmapKeys.Y;
                case (DeviceKeys.U):
                    return Logitech_keyboardBitmapKeys.U;
                case (DeviceKeys.I):
                    return Logitech_keyboardBitmapKeys.I;
                case (DeviceKeys.O):
                    return Logitech_keyboardBitmapKeys.O;
                case (DeviceKeys.P):
                    return Logitech_keyboardBitmapKeys.P;
                case (DeviceKeys.OPEN_BRACKET):
                    return Logitech_keyboardBitmapKeys.OPEN_BRACKET;
                case (DeviceKeys.CLOSE_BRACKET):
                    return Logitech_keyboardBitmapKeys.CLOSE_BRACKET;
                case (DeviceKeys.BACKSLASH):
                    return Logitech_keyboardBitmapKeys.BACKSLASH;
                case (DeviceKeys.DELETE):
                    return Logitech_keyboardBitmapKeys.KEYBOARD_DELETE;
                case (DeviceKeys.END):
                    return Logitech_keyboardBitmapKeys.END;
                case (DeviceKeys.PAGE_DOWN):
                    return Logitech_keyboardBitmapKeys.PAGE_DOWN;
                case (DeviceKeys.NUM_SEVEN):
                    return Logitech_keyboardBitmapKeys.NUM_SEVEN;
                case (DeviceKeys.NUM_EIGHT):
                    return Logitech_keyboardBitmapKeys.NUM_EIGHT;
                case (DeviceKeys.NUM_NINE):
                    return Logitech_keyboardBitmapKeys.NUM_NINE;
                case (DeviceKeys.NUM_PLUS):
                    return Logitech_keyboardBitmapKeys.NUM_PLUS;
                case (DeviceKeys.CAPS_LOCK):
                    return Logitech_keyboardBitmapKeys.CAPS_LOCK;
                case (DeviceKeys.A):
                    return Logitech_keyboardBitmapKeys.A;
                case (DeviceKeys.S):
                    return Logitech_keyboardBitmapKeys.S;
                case (DeviceKeys.D):
                    return Logitech_keyboardBitmapKeys.D;
                case (DeviceKeys.F):
                    return Logitech_keyboardBitmapKeys.F;
                case (DeviceKeys.G):
                    return Logitech_keyboardBitmapKeys.G;
                case (DeviceKeys.H):
                    return Logitech_keyboardBitmapKeys.H;
                case (DeviceKeys.J):
                    return Logitech_keyboardBitmapKeys.J;
                case (DeviceKeys.K):
                    return Logitech_keyboardBitmapKeys.K;
                case (DeviceKeys.L):
                    return Logitech_keyboardBitmapKeys.L;
                case (DeviceKeys.SEMICOLON):
                    return Logitech_keyboardBitmapKeys.SEMICOLON;
                case (DeviceKeys.APOSTROPHE):
                    return Logitech_keyboardBitmapKeys.APOSTROPHE;
                case (DeviceKeys.HASHTAG):
                    return Logitech_keyboardBitmapKeys.HASHTAG;
                case (DeviceKeys.ENTER):
                    return Logitech_keyboardBitmapKeys.ENTER;
                case (DeviceKeys.NUM_FOUR):
                    return Logitech_keyboardBitmapKeys.NUM_FOUR;
                case (DeviceKeys.NUM_FIVE):
                    return Logitech_keyboardBitmapKeys.NUM_FIVE;
                case (DeviceKeys.NUM_SIX):
                    return Logitech_keyboardBitmapKeys.NUM_SIX;
                case (DeviceKeys.LEFT_SHIFT):
                    return Logitech_keyboardBitmapKeys.LEFT_SHIFT;
                case (DeviceKeys.BACKSLASH_UK):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.jpn)
                        return Logitech_keyboardBitmapKeys.OEM102;
                    else
                        return Logitech_keyboardBitmapKeys.BACKSLASH_UK;
                case (DeviceKeys.Z):
                    return Logitech_keyboardBitmapKeys.Z;
                case (DeviceKeys.X):
                    return Logitech_keyboardBitmapKeys.X;
                case (DeviceKeys.C):
                    return Logitech_keyboardBitmapKeys.C;
                case (DeviceKeys.V):
                    return Logitech_keyboardBitmapKeys.V;
                case (DeviceKeys.B):
                    return Logitech_keyboardBitmapKeys.B;
                case (DeviceKeys.N):
                    return Logitech_keyboardBitmapKeys.N;
                case (DeviceKeys.M):
                    return Logitech_keyboardBitmapKeys.M;
                case (DeviceKeys.COMMA):
                    return Logitech_keyboardBitmapKeys.COMMA;
                case (DeviceKeys.PERIOD):
                    return Logitech_keyboardBitmapKeys.PERIOD;
                case (DeviceKeys.FORWARD_SLASH):
                    return Logitech_keyboardBitmapKeys.FORWARD_SLASH;
                case (DeviceKeys.OEM8):
                    return Logitech_keyboardBitmapKeys.FORWARD_SLASH;
                case (DeviceKeys.OEM102):
                    return Logitech_keyboardBitmapKeys.OEM102;
                case (DeviceKeys.RIGHT_SHIFT):
                    return Logitech_keyboardBitmapKeys.RIGHT_SHIFT;
                case (DeviceKeys.ARROW_UP):
                    return Logitech_keyboardBitmapKeys.ARROW_UP;
                case (DeviceKeys.NUM_ONE):
                    return Logitech_keyboardBitmapKeys.NUM_ONE;
                case (DeviceKeys.NUM_TWO):
                    return Logitech_keyboardBitmapKeys.NUM_TWO;
                case (DeviceKeys.NUM_THREE):
                    return Logitech_keyboardBitmapKeys.NUM_THREE;
                case (DeviceKeys.NUM_ENTER):
                    return Logitech_keyboardBitmapKeys.NUM_ENTER;
                case (DeviceKeys.LEFT_CONTROL):
                    return Logitech_keyboardBitmapKeys.LEFT_CONTROL;
                case (DeviceKeys.LEFT_WINDOWS):
                    return Logitech_keyboardBitmapKeys.LEFT_WINDOWS;
                case (DeviceKeys.LEFT_ALT):
                    return Logitech_keyboardBitmapKeys.LEFT_ALT;
                case (DeviceKeys.JPN_MUHENKAN):
                    return Logitech_keyboardBitmapKeys.JPN_MUHENKAN;
                case (DeviceKeys.SPACE):
                    return Logitech_keyboardBitmapKeys.SPACE;
                case (DeviceKeys.JPN_HENKAN):
                    return Logitech_keyboardBitmapKeys.JPN_HENKAN;
                case (DeviceKeys.JPN_HIRAGANA_KATAKANA):
                    return Logitech_keyboardBitmapKeys.JPN_HIRAGANA_KATAKANA;
                case (DeviceKeys.RIGHT_ALT):
                    return Logitech_keyboardBitmapKeys.RIGHT_ALT;
                case (DeviceKeys.RIGHT_WINDOWS):
                    return Logitech_keyboardBitmapKeys.RIGHT_WINDOWS;
                case (DeviceKeys.FN_Key):
                    return Logitech_keyboardBitmapKeys.RIGHT_WINDOWS;
                case (DeviceKeys.APPLICATION_SELECT):
                    return Logitech_keyboardBitmapKeys.APPLICATION_SELECT;
                case (DeviceKeys.RIGHT_CONTROL):
                    return Logitech_keyboardBitmapKeys.RIGHT_CONTROL;
                case (DeviceKeys.ARROW_LEFT):
                    return Logitech_keyboardBitmapKeys.ARROW_LEFT;
                case (DeviceKeys.ARROW_DOWN):
                    return Logitech_keyboardBitmapKeys.ARROW_DOWN;
                case (DeviceKeys.ARROW_RIGHT):
                    return Logitech_keyboardBitmapKeys.ARROW_RIGHT;
                case (DeviceKeys.NUM_ZERO):
                    return Logitech_keyboardBitmapKeys.NUM_ZERO;
                case (DeviceKeys.NUM_PERIOD):
                    return Logitech_keyboardBitmapKeys.NUM_PERIOD;
                default:
                    return Logitech_keyboardBitmapKeys.UNKNOWN;
            }
        }

        public static Logitech_keyboardBitmapKeys ToLogitechBitmap(keyboardNames key)
        {
            switch (key)
            {
                case (keyboardNames.ESC):
                    return Logitech_keyboardBitmapKeys.ESC;
                case (keyboardNames.F1):
                    return Logitech_keyboardBitmapKeys.F1;
                case (keyboardNames.F2):
                    return Logitech_keyboardBitmapKeys.F2;
                case (keyboardNames.F3):
                    return Logitech_keyboardBitmapKeys.F3;
                case (keyboardNames.F4):
                    return Logitech_keyboardBitmapKeys.F4;
                case (keyboardNames.F5):
                    return Logitech_keyboardBitmapKeys.F5;
                case (keyboardNames.F6):
                    return Logitech_keyboardBitmapKeys.F6;
                case (keyboardNames.F7):
                    return Logitech_keyboardBitmapKeys.F7;
                case (keyboardNames.F8):
                    return Logitech_keyboardBitmapKeys.F8;
                case (keyboardNames.F9):
                    return Logitech_keyboardBitmapKeys.F9;
                case (keyboardNames.F10):
                    return Logitech_keyboardBitmapKeys.F10;
                case (keyboardNames.F11):
                    return Logitech_keyboardBitmapKeys.F11;
                case (keyboardNames.F12):
                    return Logitech_keyboardBitmapKeys.F12;
                case (keyboardNames.PRINT_SCREEN):
                    return Logitech_keyboardBitmapKeys.PRINT_SCREEN;
                case (keyboardNames.SCROLL_LOCK):
                    return Logitech_keyboardBitmapKeys.SCROLL_LOCK;
                case (keyboardNames.PAUSE_BREAK):
                    return Logitech_keyboardBitmapKeys.PAUSE_BREAK;
                case (keyboardNames.TILDE):
                    return Logitech_keyboardBitmapKeys.TILDE;
                case (keyboardNames.ONE):
                    return Logitech_keyboardBitmapKeys.ONE;
                case (keyboardNames.TWO):
                    return Logitech_keyboardBitmapKeys.TWO;
                case (keyboardNames.THREE):
                    return Logitech_keyboardBitmapKeys.THREE;
                case (keyboardNames.FOUR):
                    return Logitech_keyboardBitmapKeys.FOUR;
                case (keyboardNames.FIVE):
                    return Logitech_keyboardBitmapKeys.FIVE;
                case (keyboardNames.SIX):
                    return Logitech_keyboardBitmapKeys.SIX;
                case (keyboardNames.SEVEN):
                    return Logitech_keyboardBitmapKeys.SEVEN;
                case (keyboardNames.EIGHT):
                    return Logitech_keyboardBitmapKeys.EIGHT;
                case (keyboardNames.NINE):
                    return Logitech_keyboardBitmapKeys.NINE;
                case (keyboardNames.ZERO):
                    return Logitech_keyboardBitmapKeys.ZERO;
                case (keyboardNames.MINUS):
                    return Logitech_keyboardBitmapKeys.MINUS;
                case (keyboardNames.EQUALS):
                    return Logitech_keyboardBitmapKeys.EQUALS;
                case (keyboardNames.BACKSPACE):
                    return Logitech_keyboardBitmapKeys.BACKSPACE;
                case (keyboardNames.INSERT):
                    return Logitech_keyboardBitmapKeys.INSERT;
                case (keyboardNames.HOME):
                    return Logitech_keyboardBitmapKeys.HOME;
                case (keyboardNames.PAGE_UP):
                    return Logitech_keyboardBitmapKeys.PAGE_UP;
                case (keyboardNames.NUM_LOCK):
                    return Logitech_keyboardBitmapKeys.NUM_LOCK;
                case (keyboardNames.NUM_SLASH):
                    return Logitech_keyboardBitmapKeys.NUM_SLASH;
                case (keyboardNames.NUM_ASTERISK):
                    return Logitech_keyboardBitmapKeys.NUM_ASTERISK;
                case (keyboardNames.NUM_MINUS):
                    return Logitech_keyboardBitmapKeys.NUM_MINUS;
                case (keyboardNames.TAB):
                    return Logitech_keyboardBitmapKeys.TAB;
                case (keyboardNames.Q):
                    return Logitech_keyboardBitmapKeys.Q;
                case (keyboardNames.W):
                    return Logitech_keyboardBitmapKeys.W;
                case (keyboardNames.E):
                    return Logitech_keyboardBitmapKeys.E;
                case (keyboardNames.R):
                    return Logitech_keyboardBitmapKeys.R;
                case (keyboardNames.T):
                    return Logitech_keyboardBitmapKeys.T;
                case (keyboardNames.Y):
                    return Logitech_keyboardBitmapKeys.Y;
                case (keyboardNames.U):
                    return Logitech_keyboardBitmapKeys.U;
                case (keyboardNames.I):
                    return Logitech_keyboardBitmapKeys.I;
                case (keyboardNames.O):
                    return Logitech_keyboardBitmapKeys.O;
                case (keyboardNames.P):
                    return Logitech_keyboardBitmapKeys.P;
                case (keyboardNames.OPEN_BRACKET):
                    return Logitech_keyboardBitmapKeys.OPEN_BRACKET;
                case (keyboardNames.CLOSE_BRACKET):
                    return Logitech_keyboardBitmapKeys.CLOSE_BRACKET;
                case (keyboardNames.BACKSLASH):
                    return Logitech_keyboardBitmapKeys.BACKSLASH;
                case (keyboardNames.KEYBOARD_DELETE):
                    return Logitech_keyboardBitmapKeys.KEYBOARD_DELETE;
                case (keyboardNames.END):
                    return Logitech_keyboardBitmapKeys.END;
                case (keyboardNames.PAGE_DOWN):
                    return Logitech_keyboardBitmapKeys.PAGE_DOWN;
                case (keyboardNames.NUM_SEVEN):
                    return Logitech_keyboardBitmapKeys.NUM_SEVEN;
                case (keyboardNames.NUM_EIGHT):
                    return Logitech_keyboardBitmapKeys.NUM_EIGHT;
                case (keyboardNames.NUM_NINE):
                    return Logitech_keyboardBitmapKeys.NUM_NINE;
                case (keyboardNames.NUM_PLUS):
                    return Logitech_keyboardBitmapKeys.NUM_PLUS;
                case (keyboardNames.CAPS_LOCK):
                    return Logitech_keyboardBitmapKeys.CAPS_LOCK;
                case (keyboardNames.A):
                    return Logitech_keyboardBitmapKeys.A;
                case (keyboardNames.S):
                    return Logitech_keyboardBitmapKeys.S;
                case (keyboardNames.D):
                    return Logitech_keyboardBitmapKeys.D;
                case (keyboardNames.F):
                    return Logitech_keyboardBitmapKeys.F;
                case (keyboardNames.G):
                    return Logitech_keyboardBitmapKeys.G;
                case (keyboardNames.H):
                    return Logitech_keyboardBitmapKeys.H;
                case (keyboardNames.J):
                    return Logitech_keyboardBitmapKeys.J;
                case (keyboardNames.K):
                    return Logitech_keyboardBitmapKeys.K;
                case (keyboardNames.L):
                    return Logitech_keyboardBitmapKeys.L;
                case (keyboardNames.SEMICOLON):
                    return Logitech_keyboardBitmapKeys.SEMICOLON;
                case (keyboardNames.APOSTROPHE):
                    return Logitech_keyboardBitmapKeys.APOSTROPHE;
                //case (keyboardNames.HASHTAG):
                //    return Logitech_keyboardBitmapKeys.HASHTAG;
                case (keyboardNames.ENTER):
                    return Logitech_keyboardBitmapKeys.ENTER;
                case (keyboardNames.NUM_FOUR):
                    return Logitech_keyboardBitmapKeys.NUM_FOUR;
                case (keyboardNames.NUM_FIVE):
                    return Logitech_keyboardBitmapKeys.NUM_FIVE;
                case (keyboardNames.NUM_SIX):
                    return Logitech_keyboardBitmapKeys.NUM_SIX;
                case (keyboardNames.LEFT_SHIFT):
                    return Logitech_keyboardBitmapKeys.LEFT_SHIFT;
                //case (keyboardNames.BACKSLASH_UK):
                //    return Logitech_keyboardBitmapKeys.BACKSLASH_UK;
                case (keyboardNames.Z):
                    return Logitech_keyboardBitmapKeys.Z;
                case (keyboardNames.X):
                    return Logitech_keyboardBitmapKeys.X;
                case (keyboardNames.C):
                    return Logitech_keyboardBitmapKeys.C;
                case (keyboardNames.V):
                    return Logitech_keyboardBitmapKeys.V;
                case (keyboardNames.B):
                    return Logitech_keyboardBitmapKeys.B;
                case (keyboardNames.N):
                    return Logitech_keyboardBitmapKeys.N;
                case (keyboardNames.M):
                    return Logitech_keyboardBitmapKeys.M;
                case (keyboardNames.COMMA):
                    return Logitech_keyboardBitmapKeys.COMMA;
                case (keyboardNames.PERIOD):
                    return Logitech_keyboardBitmapKeys.PERIOD;
                case (keyboardNames.FORWARD_SLASH):
                    return Logitech_keyboardBitmapKeys.FORWARD_SLASH;
                case (keyboardNames.RIGHT_SHIFT):
                    return Logitech_keyboardBitmapKeys.RIGHT_SHIFT;
                case (keyboardNames.ARROW_UP):
                    return Logitech_keyboardBitmapKeys.ARROW_UP;
                case (keyboardNames.NUM_ONE):
                    return Logitech_keyboardBitmapKeys.NUM_ONE;
                case (keyboardNames.NUM_TWO):
                    return Logitech_keyboardBitmapKeys.NUM_TWO;
                case (keyboardNames.NUM_THREE):
                    return Logitech_keyboardBitmapKeys.NUM_THREE;
                case (keyboardNames.NUM_ENTER):
                    return Logitech_keyboardBitmapKeys.NUM_ENTER;
                case (keyboardNames.LEFT_CONTROL):
                    return Logitech_keyboardBitmapKeys.LEFT_CONTROL;
                case (keyboardNames.LEFT_WINDOWS):
                    return Logitech_keyboardBitmapKeys.LEFT_WINDOWS;
                case (keyboardNames.LEFT_ALT):
                    return Logitech_keyboardBitmapKeys.LEFT_ALT;
                case (keyboardNames.SPACE):
                    return Logitech_keyboardBitmapKeys.SPACE;
                case (keyboardNames.RIGHT_ALT):
                    return Logitech_keyboardBitmapKeys.RIGHT_ALT;
                case (keyboardNames.RIGHT_WINDOWS):
                    return Logitech_keyboardBitmapKeys.RIGHT_WINDOWS;
                case (keyboardNames.APPLICATION_SELECT):
                    return Logitech_keyboardBitmapKeys.APPLICATION_SELECT;
                case (keyboardNames.RIGHT_CONTROL):
                    return Logitech_keyboardBitmapKeys.RIGHT_CONTROL;
                case (keyboardNames.ARROW_LEFT):
                    return Logitech_keyboardBitmapKeys.ARROW_LEFT;
                case (keyboardNames.ARROW_DOWN):
                    return Logitech_keyboardBitmapKeys.ARROW_DOWN;
                case (keyboardNames.ARROW_RIGHT):
                    return Logitech_keyboardBitmapKeys.ARROW_RIGHT;
                case (keyboardNames.NUM_ZERO):
                    return Logitech_keyboardBitmapKeys.NUM_ZERO;
                case (keyboardNames.NUM_PERIOD):
                    return Logitech_keyboardBitmapKeys.NUM_PERIOD;
                default:
                    return Logitech_keyboardBitmapKeys.UNKNOWN;
            }
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
            if (default_registry == null)
            {
                default_registry = new VariableRegistry();
                default_registry.Register($"{devicename}_set_default", false, "Set Default Color");
                default_registry.Register($"{devicename}_default_color", new Aurora.Utils.RealColor(System.Drawing.Color.FromArgb(255, 255, 255, 255)), "Default Color", new Aurora.Utils.RealColor(System.Drawing.Color.FromArgb(255, 255, 255, 255)), new Aurora.Utils.RealColor(System.Drawing.Color.FromArgb(0, 0, 0, 0)));
                default_registry.Register($"{devicename}_override_dll", false, "Override DLL", null, null, "Requires restart to take effect");
                default_registry.Register($"{devicename}_override_dll_option", LogitechGSDK.LGDLL.LGS, "Override DLL Selection", null, null, "Requires restart to take effect");
            }
            return default_registry;
        }
    }
}
