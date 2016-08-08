using LedCSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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

        //Keyboard stuff
        private Logitech_keyboardBitmapKeys[] allKeys = Enum.GetValues(typeof(Logitech_keyboardBitmapKeys)).Cast<Logitech_keyboardBitmapKeys>().ToArray();
        private byte[] bitmap = new byte[LogitechGSDK.LOGI_LED_BITMAP_SIZE];
        private Color peripheral_Color = Color.Black;
        //Previous data
        private byte[] previous_bitmap = new byte[LogitechGSDK.LOGI_LED_BITMAP_SIZE];
        private Color previous_peripheral_Color = Color.Black;

        public bool Initialize()
        {
            if(!isInitialized)
            {
                try
                {
                    if (!LogitechGSDK.LogiLedInit())
                    {
                        Global.logger.LogLine("Logitech LED SDK could not be initialized.", Logging_Level.Error);

                        isInitialized = false;
                        return false;
                    }

                    if (LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_RGB | LogitechGSDK.LOGI_DEVICETYPE_PERKEY_RGB | LogitechGSDK.LOGI_DEVICETYPE_MONOCHROME) && LogitechGSDK.LogiLedSaveCurrentLighting())
                    {
                        if (Global.Configuration.logitech_first_time)
                        {
                            LogitechInstallInstructions instructions = new LogitechInstallInstructions();
                            instructions.ShowDialog();

                            Global.Configuration.logitech_first_time = false;
                            Settings.ConfigManager.Save(Global.Configuration);
                        }

                        isInitialized = true;
                        return true;
                    }
                    else
                    {
                        Global.logger.LogLine("Logitech LED SDK could not be initialized. (LogiLedSetTargetDevice or LogiLedSaveCurrentLighting failed)", Logging_Level.Error);

                        isInitialized = false;
                        return false;
                    }


                }
                catch (Exception exc)
                {
                    Global.logger.LogLine("There was an error initializing Logitech LED SDK.\r\n" + exc.Message, Logging_Level.Error);

                    return false;
                }
            }

            return isInitialized;
        }

        public void Shutdown()
        {
            if (isInitialized)
            {
                this.Reset();
                LogitechGSDK.LogiLedShutdown();
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
                LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_RGB | LogitechGSDK.LOGI_DEVICETYPE_PERKEY_RGB | LogitechGSDK.LOGI_DEVICETYPE_MONOCHROME);
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
            if (Global.Configuration.logitech_enhance_brightness)
            {
                float boost_amount = 0.0f;
                boost_amount += 3.0f - (color.R / 30.0f);
                boost_amount += 3.0f - (color.G / 30.0f);
                boost_amount += 3.0f - (color.B / 30.0f);
                boost_amount /= 3.0f;
                boost_amount = boost_amount <= 1.0f ? 1.0f : boost_amount;

                color = Utils.ColorUtils.MultiplyColorByScalar(color, boost_amount);
            }

            bitmap[(int)key] = color.B;
            bitmap[(int)key + 1] = color.G;
            bitmap[(int)key + 2] = color.R;
            bitmap[(int)key + 3] = color.A;
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

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, bool forced = false)
        {
            try
            {
                foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
                {
                    Logitech_keyboardBitmapKeys localKey = ToLogitechBitmap(key.Key);

                    if (localKey == Logitech_keyboardBitmapKeys.UNKNOWN && key.Key == DeviceKeys.Peripheral)
                    {
                        SendColorToPeripheral((Color)key.Value, forced);
                    }
                    if (localKey == Logitech_keyboardBitmapKeys.UNKNOWN && key.Key == DeviceKeys.OEM8)
                    {
                        double alpha_amt = (key.Value.A / 255.0);
                        int red_amt = (int)(((key.Value.R * alpha_amt) / 255.0) * 100.0);
                        int green_amt = (int)(((key.Value.G * alpha_amt) / 255.0) * 100.0);
                        int blue_amt = (int)(((key.Value.B * alpha_amt) / 255.0) * 100.0);

                        LogitechGSDK.LogiLedSetLightingForKeyWithHidCode(220, red_amt, green_amt, blue_amt);
                    }
                    else if (localKey != Logitech_keyboardBitmapKeys.UNKNOWN)
                    {
                        SetOneKey(localKey, (Color)key.Value);
                    }
                }

                SendColorsToKeyboard(forced);
                return true;
            }
            catch (Exception e)
            {
                Global.logger.LogLine(e.ToString(), Logging_Level.Error);
                return false;
            }
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
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return Logitech_keyboardBitmapKeys.APOSTROPHE;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return Logitech_keyboardBitmapKeys.SEMICOLON;
                    else
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
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return Logitech_keyboardBitmapKeys.A;
                    else
                        return Logitech_keyboardBitmapKeys.Q;
                case (DeviceKeys.W):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return Logitech_keyboardBitmapKeys.Z;
                    else
                        return Logitech_keyboardBitmapKeys.W;
                case (DeviceKeys.E):
                    return Logitech_keyboardBitmapKeys.E;
                case (DeviceKeys.R):
                    return Logitech_keyboardBitmapKeys.R;
                case (DeviceKeys.T):
                    return Logitech_keyboardBitmapKeys.T;
                case (DeviceKeys.Y):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return Logitech_keyboardBitmapKeys.Z;
                    else
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
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return Logitech_keyboardBitmapKeys.MINUS;
                    else
                        return Logitech_keyboardBitmapKeys.OPEN_BRACKET;
                case (DeviceKeys.CLOSE_BRACKET):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return Logitech_keyboardBitmapKeys.OPEN_BRACKET;
                    else
                        return Logitech_keyboardBitmapKeys.CLOSE_BRACKET;
                case (DeviceKeys.BACKSLASH):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return Logitech_keyboardBitmapKeys.HASHTAG;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return Logitech_keyboardBitmapKeys.TILDE;
                    else if(Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.ru)
                        return Logitech_keyboardBitmapKeys.HASHTAG;
                    else
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
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return Logitech_keyboardBitmapKeys.Q;
                    else
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
                case (DeviceKeys.DEU_O):
                    return Logitech_keyboardBitmapKeys.SEMICOLON;
                case (DeviceKeys.SEMICOLON):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return Logitech_keyboardBitmapKeys.CLOSE_BRACKET;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return Logitech_keyboardBitmapKeys.OPEN_BRACKET;
                    else
                        return Logitech_keyboardBitmapKeys.SEMICOLON;
                case (DeviceKeys.APOSTROPHE):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return Logitech_keyboardBitmapKeys.TILDE;
                    else
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
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return Logitech_keyboardBitmapKeys.W;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return Logitech_keyboardBitmapKeys.Y;
                    else
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
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return Logitech_keyboardBitmapKeys.SEMICOLON;
                    else
                        return Logitech_keyboardBitmapKeys.M;
                case (DeviceKeys.COMMA):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return Logitech_keyboardBitmapKeys.M;
                    else
                        return Logitech_keyboardBitmapKeys.COMMA;
                case (DeviceKeys.PERIOD):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return Logitech_keyboardBitmapKeys.COMMA;
                    else
                        return Logitech_keyboardBitmapKeys.PERIOD;
                case (DeviceKeys.FORWARD_SLASH):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return Logitech_keyboardBitmapKeys.PERIOD;
                    else
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
    }
}
