using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Aurora.Settings;
using System.Diagnostics;
using SteelSeries.GameSenseSDK;

namespace Aurora.Devices.SteelSeries
{
    public enum SteelSeriesKeyCodes
    {
        LOGO = 0x00,
        SS_KEY = 0xEF,
        G0 = 0xE8,
        G1 = 0xE9,
        G2 = 0xEA,
        G3 = 0xEB,
        G4 = 0xEC,
        G5 = 0xED,
    };

    class SteelSeriesDevice : Device
    {
        private String devicename = "SteelSeries";
        private bool isInitialized = false;

        private GameSenseSDK gameSenseSDK = new GameSenseSDK();

        private bool keyboard_updated = false;
        private bool peripheral_updated = false;

        private readonly object action_lock = new object();

        private Stopwatch watch = new Stopwatch();
        private Stopwatch keepaliveTimer = new Stopwatch();
        private long lastUpdateTime = 0;

        //Previous data
        private Color previous_peripheral_Color = Color.Black;

        public bool Initialize()
        {
            lock (action_lock)
            {
                if (!isInitialized)
                {
                    try
                    {
                        gameSenseSDK.init("PROJECTAURORA", "Project Aurora", 7);

                        if (Global.Configuration.steelseries_first_time)
                        {
                            SteelSeriesInstallInstructions instructions = new SteelSeriesInstallInstructions();
                            instructions.ShowDialog();

                            Global.Configuration.steelseries_first_time = false;
                            Settings.ConfigManager.Save(Global.Configuration);
                        }
                        isInitialized = true;
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Global.logger.LogLine("SteelSeries GameSense SDK could not be initialized: " + ex, Logging_Level.Error);

                        isInitialized = false;
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
                try
                {
                    if (isInitialized)
                    {
                        this.Reset();
                        //GameSenseSDK.sendStop(); doesn't work atm so just wait for timeout=15sec
                        isInitialized = false;
                    }
                }
                catch (Exception ex)
                {
                    Global.logger.LogLine("There was an error shutting down SteelSeries GameSense SDK: " + ex, Logging_Level.Error);
                    isInitialized = false;
                }

                if (keepaliveTimer.IsRunning)
                    keepaliveTimer.Stop();
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

        public bool IsInitialized()
        {
            return this.isInitialized;
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, bool forced = false)
        {
            try
            {
                // workaround for heartbeat/keepalive events every 10sec
                SendKeepalive();

                List<byte> hids = new List<byte>();
                List<Tuple<byte, byte, byte>> colors = new List<Tuple<byte, byte, byte>>();

                foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
                {
                    //CorsairLedId localKey = ToCorsair(key.Key);

                    Color color = (Color) key.Value;
                    //Apply and strip Alpha
                    color = Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(color, color.A / 255.0D));

                    if (key.Key == DeviceKeys.Peripheral)
                    {
                        SendColorToPeripheral(color, forced);
                    }
                    else if (key.Key == DeviceKeys.Peripheral_Logo || key.Key == DeviceKeys.Peripheral_FrontLight || key.Key == DeviceKeys.Peripheral_ScrollWheel)
                    {
                        SendColorToPeripheralZone(key.Key, color);
                    }
                    else
                    {
                        byte hid = GetHIDCode(key.Key);

                        if (hid != (byte) USBHIDCodes.ERROR)
                        {
                            hids.Add(hid);
                            colors.Add(Tuple.Create(color.R, color.G, color.B));
                        }
                    }
                }

                SendColorsToKeyboard(hids, colors);

                return true;
            }
            catch (Exception ex)
            {
                Global.logger.LogLine("SteelSeries GameSense SDK, error when updating device: " + ex, Logging_Level.Error);
                return false;
            }
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, bool forced = false)
        {
            watch.Restart();

            bool update_result = UpdateDevice(colorComposition.keyColors, forced);

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

        private void SendColorToPeripheral(Color color, bool forced = false)
        {
            if ((!previous_peripheral_Color.Equals(color) || forced))
            {
                if (Global.Configuration.allow_peripheral_devices)
                {
                    if (!Global.Configuration.devices_disable_mouse && !Global.Configuration.devices_disable_headset)
                    {
                        gameSenseSDK.setPeripheryColor(color.R, color.G, color.B);
                    }
                    else
                    {
                        if (!Global.Configuration.devices_disable_mouse)
                        {
                            gameSenseSDK.setMouseColor(color.R, color.G, color.B);
                        }

                        if (!Global.Configuration.devices_disable_headset)
                        {
                            gameSenseSDK.setHeadsetColor(color.R, color.G, color.B);
                        }
                    }

                    previous_peripheral_Color = color;
                    peripheral_updated = true;
                }
                else
                {
                    peripheral_updated = false;
                }
            }
        }

        private void SendColorToPeripheralZone(DeviceKeys zone, Color color)
        {
            if (Global.Configuration.allow_peripheral_devices && !Global.Configuration.devices_disable_mouse)
            {
                if (zone == DeviceKeys.Peripheral_Logo)
                {
                    gameSenseSDK.setMouseLogoColor(color.R, color.G, color.B);
                }
                else if (zone == DeviceKeys.Peripheral_ScrollWheel)
                {
                    gameSenseSDK.setMouseScrollWheelColor(color.R, color.G, color.B);
                }
                else if (zone == DeviceKeys.Peripheral_FrontLight)
                {
                    //NYI
                    Global.logger.LogLine("SteelSeries GameSense SDK: Unknown device zone Peripheral_FrontLight: " + zone, Logging_Level.Error);
                }
                /*else if (zone == DeviceKeys.Peripheral_Earcups || zone == DeviceKeys.Peripheral_Headset)
                {
                    GameSenseSDK.setHeadsetColor(color.R, color.G, color.B);
                }*/

                peripheral_updated = true;
            }
            else
            {
                peripheral_updated = false;
            }
        }

        private void SendColorsToKeyboard(List<byte> hids, List<Tuple<byte, byte, byte>> colors)
        {
            if (!Global.Configuration.devices_disable_keyboard)
            {
                if (hids.Count != 0)
                {
                    gameSenseSDK.setKeyboardColors(hids, colors);
                }
                keyboard_updated = true;
            }
            else
            {
                keyboard_updated = false;
            }
        }

        private void SendKeepalive(bool forced = false)
        {
            // workaround for heartbeat/keepalive events every 10sec
            if (!keepaliveTimer.IsRunning)
                keepaliveTimer.Start();

            if (keepaliveTimer.ElapsedMilliseconds > 10000 || forced)
            {
                gameSenseSDK.sendHeartbeat();
                keepaliveTimer.Restart();
            }
        }

        public static byte GetHIDCode(DeviceKeys key)
        {
            switch (key)
            {
                case (DeviceKeys.LOGO):
                    return (byte)SteelSeriesKeyCodes.LOGO;
                case (DeviceKeys.FN_Key):
                    return (byte)SteelSeriesKeyCodes.SS_KEY;
                case (DeviceKeys.G0):
                    return (byte)SteelSeriesKeyCodes.G0;
                case (DeviceKeys.G1):
                    return (byte)SteelSeriesKeyCodes.G1;
                case (DeviceKeys.G2):
                    return (byte)SteelSeriesKeyCodes.G2;
                case (DeviceKeys.G3):
                    return (byte)SteelSeriesKeyCodes.G3;
                case (DeviceKeys.G4):
                    return (byte)SteelSeriesKeyCodes.G4;
                case (DeviceKeys.G5):
                    return (byte)SteelSeriesKeyCodes.G5;
                case (DeviceKeys.ESC):
                    return (byte) USBHIDCodes.ESC;
                case (DeviceKeys.F1):
                    return (byte) USBHIDCodes.F1;
                case (DeviceKeys.F2):
                    return (byte) USBHIDCodes.F2;
                case (DeviceKeys.F3):
                    return (byte) USBHIDCodes.F3;
                case (DeviceKeys.F4):
                    return (byte) USBHIDCodes.F4;
                case (DeviceKeys.F5):
                    return (byte) USBHIDCodes.F5;
                case (DeviceKeys.F6):
                    return (byte) USBHIDCodes.F6;
                case (DeviceKeys.F7):
                    return (byte) USBHIDCodes.F7;
                case (DeviceKeys.F8):
                    return (byte) USBHIDCodes.F8;
                case (DeviceKeys.F9):
                    return (byte) USBHIDCodes.F9;
                case (DeviceKeys.F10):
                    return (byte) USBHIDCodes.F10;
                case (DeviceKeys.F11):
                    return (byte) USBHIDCodes.F11;
                case (DeviceKeys.F12):
                    return (byte) USBHIDCodes.F12;
                case (DeviceKeys.PRINT_SCREEN):
                    return (byte) USBHIDCodes.PRINT_SCREEN;
                case (DeviceKeys.SCROLL_LOCK):
                    return (byte) USBHIDCodes.SCROLL_LOCK;
                case (DeviceKeys.PAUSE_BREAK):
                    return (byte) USBHIDCodes.PAUSE_BREAK;
                case (DeviceKeys.JPN_HALFFULLWIDTH):
                    return (byte) USBHIDCodes.TILDE;
                case (DeviceKeys.OEM5):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.jpn)
                        return (byte) USBHIDCodes.ERROR;
                    else
                        return (byte) USBHIDCodes.TILDE;
                case (DeviceKeys.TILDE):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return (byte) USBHIDCodes.APOSTROPHE;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return (byte) USBHIDCodes.SEMICOLON;
                    else
                        return (byte) USBHIDCodes.TILDE;
                case (DeviceKeys.ONE):
                    return (byte) USBHIDCodes.ONE;
                case (DeviceKeys.TWO):
                    return (byte) USBHIDCodes.TWO;
                case (DeviceKeys.THREE):
                    return (byte) USBHIDCodes.THREE;
                case (DeviceKeys.FOUR):
                    return (byte) USBHIDCodes.FOUR;
                case (DeviceKeys.FIVE):
                    return (byte) USBHIDCodes.FIVE;
                case (DeviceKeys.SIX):
                    return (byte) USBHIDCodes.SIX;
                case (DeviceKeys.SEVEN):
                    return (byte) USBHIDCodes.SEVEN;
                case (DeviceKeys.EIGHT):
                    return (byte) USBHIDCodes.EIGHT;
                case (DeviceKeys.NINE):
                    return (byte) USBHIDCodes.NINE;
                case (DeviceKeys.ZERO):
                    return (byte) USBHIDCodes.ZERO;
                case (DeviceKeys.MINUS):
                    return (byte) USBHIDCodes.MINUS;
                case (DeviceKeys.EQUALS):
                    return (byte) USBHIDCodes.EQUALS;
                case (DeviceKeys.BACKSPACE):
                    return (byte) USBHIDCodes.BACKSPACE;
                case (DeviceKeys.INSERT):
                    return (byte) USBHIDCodes.INSERT;
                case (DeviceKeys.HOME):
                    return (byte) USBHIDCodes.HOME;
                case (DeviceKeys.PAGE_UP):
                    return (byte) USBHIDCodes.PAGE_UP;
                case (DeviceKeys.NUM_LOCK):
                    return (byte) USBHIDCodes.NUM_LOCK;
                case (DeviceKeys.NUM_SLASH):
                    return (byte) USBHIDCodes.NUM_SLASH;
                case (DeviceKeys.NUM_ASTERISK):
                    return (byte) USBHIDCodes.NUM_ASTERISK;
                case (DeviceKeys.NUM_MINUS):
                    return (byte) USBHIDCodes.NUM_MINUS;
                case (DeviceKeys.TAB):
                    return (byte) USBHIDCodes.TAB;
                case (DeviceKeys.Q):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return (byte) USBHIDCodes.A;
                    else
                        return (byte) USBHIDCodes.Q;
                case (DeviceKeys.W):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return (byte) USBHIDCodes.Z;
                    else
                        return (byte) USBHIDCodes.W;
                case (DeviceKeys.E):
                    return (byte) USBHIDCodes.E;
                case (DeviceKeys.R):
                    return (byte) USBHIDCodes.R;
                case (DeviceKeys.T):
                    return (byte) USBHIDCodes.T;
                case (DeviceKeys.Y):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return (byte) USBHIDCodes.Z;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.jpn)
                        return (byte) USBHIDCodes.Z;
                    else
                        return (byte) USBHIDCodes.Y;
                case (DeviceKeys.U):
                    return (byte) USBHIDCodes.U;
                case (DeviceKeys.I):
                    return (byte) USBHIDCodes.I;
                case (DeviceKeys.O):
                    return (byte) USBHIDCodes.O;
                case (DeviceKeys.P):
                    return (byte) USBHIDCodes.P;
                case (DeviceKeys.OPEN_BRACKET):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return (byte) USBHIDCodes.MINUS;
                    else
                        return (byte) USBHIDCodes.OPEN_BRACKET;
                case (DeviceKeys.CLOSE_BRACKET):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return (byte) USBHIDCodes.OPEN_BRACKET;
                    else
                        return (byte) USBHIDCodes.CLOSE_BRACKET;
                case (DeviceKeys.BACKSLASH):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.intl)
                        return (byte) USBHIDCodes.HASHTAG;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.ru)
                        return (byte) USBHIDCodes.HASHTAG;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return (byte) USBHIDCodes.HASHTAG;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return (byte) USBHIDCodes.TILDE;
                    else
                        return (byte) USBHIDCodes.BACKSLASH;
                case (DeviceKeys.DELETE):
                    return (byte) USBHIDCodes.KEYBOARD_DELETE;
                case (DeviceKeys.END):
                    return (byte) USBHIDCodes.END;
                case (DeviceKeys.PAGE_DOWN):
                    return (byte) USBHIDCodes.PAGE_DOWN;
                case (DeviceKeys.NUM_SEVEN):
                    return (byte) USBHIDCodes.NUM_SEVEN;
                case (DeviceKeys.NUM_EIGHT):
                    return (byte) USBHIDCodes.NUM_EIGHT;
                case (DeviceKeys.NUM_NINE):
                    return (byte) USBHIDCodes.NUM_NINE;
                case (DeviceKeys.NUM_PLUS):
                    return (byte) USBHIDCodes.NUM_PLUS;
                case (DeviceKeys.CAPS_LOCK):
                    return (byte) USBHIDCodes.CAPS_LOCK;
                case (DeviceKeys.A):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return (byte) USBHIDCodes.Q;
                    else
                        return (byte) USBHIDCodes.A;
                case (DeviceKeys.S):
                    return (byte) USBHIDCodes.S;
                case (DeviceKeys.D):
                    return (byte) USBHIDCodes.D;
                case (DeviceKeys.F):
                    return (byte) USBHIDCodes.F;
                case (DeviceKeys.G):
                    return (byte) USBHIDCodes.G;
                case (DeviceKeys.H):
                    return (byte) USBHIDCodes.H;
                case (DeviceKeys.J):
                    return (byte) USBHIDCodes.J;
                case (DeviceKeys.K):
                    return (byte) USBHIDCodes.K;
                case (DeviceKeys.L):
                    return (byte) USBHIDCodes.L;
                case (DeviceKeys.SEMICOLON):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return (byte) USBHIDCodes.CLOSE_BRACKET;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return (byte) USBHIDCodes.OPEN_BRACKET;
                    else
                        return (byte) USBHIDCodes.SEMICOLON;
                case (DeviceKeys.APOSTROPHE):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return (byte) USBHIDCodes.TILDE;
                    else
                        return (byte) USBHIDCodes.APOSTROPHE;
                case (DeviceKeys.HASHTAG):
                    return (byte) USBHIDCodes.HASHTAG;
                case (DeviceKeys.ENTER):
                    return (byte) USBHIDCodes.ENTER;
                case (DeviceKeys.NUM_FOUR):
                    return (byte) USBHIDCodes.NUM_FOUR;
                case (DeviceKeys.NUM_FIVE):
                    return (byte) USBHIDCodes.NUM_FIVE;
                case (DeviceKeys.NUM_SIX):
                    return (byte) USBHIDCodes.NUM_SIX;
                case (DeviceKeys.LEFT_SHIFT):
                    return (byte) USBHIDCodes.LEFT_SHIFT;
                case (DeviceKeys.BACKSLASH_UK):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.jpn)
                        return (byte) USBHIDCodes.ERROR;
                    else
                        return (byte) USBHIDCodes.BACKSLASH_UK;
                case (DeviceKeys.Z):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return (byte) USBHIDCodes.Y;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return (byte) USBHIDCodes.W;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.jpn)
                        return (byte) USBHIDCodes.Y;
                    else
                        return (byte) USBHIDCodes.Z;
                case (DeviceKeys.X):
                    return (byte) USBHIDCodes.X;
                case (DeviceKeys.C):
                    return (byte) USBHIDCodes.C;
                case (DeviceKeys.V):
                    return (byte) USBHIDCodes.V;
                case (DeviceKeys.B):
                    return (byte) USBHIDCodes.B;
                case (DeviceKeys.N):
                    return (byte) USBHIDCodes.N;
                case (DeviceKeys.M):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return (byte) USBHIDCodes.SEMICOLON;
                    else
                        return (byte) USBHIDCodes.M;
                case (DeviceKeys.COMMA):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return (byte) USBHIDCodes.M;
                    else
                        return (byte) USBHIDCodes.COMMA;
                case (DeviceKeys.PERIOD):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return (byte) USBHIDCodes.COMMA;
                    else
                        return (byte) USBHIDCodes.PERIOD;
                case (DeviceKeys.FORWARD_SLASH):
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.fr)
                        return (byte) USBHIDCodes.PERIOD;
                    else
                        return (byte) USBHIDCodes.FORWARD_SLASH;
                case (DeviceKeys.OEM8):
                    return (byte) USBHIDCodes.FORWARD_SLASH;
                case (DeviceKeys.OEM102):
                    return (byte) USBHIDCodes.ERROR;
                case (DeviceKeys.RIGHT_SHIFT):
                    return (byte) USBHIDCodes.RIGHT_SHIFT;
                case (DeviceKeys.ARROW_UP):
                    return (byte) USBHIDCodes.ARROW_UP;
                case (DeviceKeys.NUM_ONE):
                    return (byte) USBHIDCodes.NUM_ONE;
                case (DeviceKeys.NUM_TWO):
                    return (byte) USBHIDCodes.NUM_TWO;
                case (DeviceKeys.NUM_THREE):
                    return (byte) USBHIDCodes.NUM_THREE;
                case (DeviceKeys.NUM_ENTER):
                    return (byte) USBHIDCodes.NUM_ENTER;
                case (DeviceKeys.LEFT_CONTROL):
                    return (byte) USBHIDCodes.LEFT_CONTROL;
                case (DeviceKeys.LEFT_WINDOWS):
                    return (byte) USBHIDCodes.LEFT_WINDOWS;
                case (DeviceKeys.LEFT_ALT):
                    return (byte) USBHIDCodes.LEFT_ALT;
                case (DeviceKeys.JPN_MUHENKAN):
                    return (byte) USBHIDCodes.JPN_MUHENKAN;
                case (DeviceKeys.SPACE):
                    return (byte) USBHIDCodes.SPACE;
                case (DeviceKeys.JPN_HENKAN):
                    return (byte) USBHIDCodes.JPN_HENKAN;
                case (DeviceKeys.JPN_HIRAGANA_KATAKANA):
                    return (byte) USBHIDCodes.JPN_HIRAGANA_KATAKANA;
                case (DeviceKeys.RIGHT_ALT):
                    return (byte) USBHIDCodes.RIGHT_ALT;
                case (DeviceKeys.RIGHT_WINDOWS):
                    return (byte) USBHIDCodes.RIGHT_WINDOWS;
                //case (DeviceKeys.FN_Key):
                    //return (byte) USBHIDCodes.RIGHT_WINDOWS;
                case (DeviceKeys.APPLICATION_SELECT):
                    return (byte) USBHIDCodes.APPLICATION_SELECT;
                case (DeviceKeys.RIGHT_CONTROL):
                    return (byte) USBHIDCodes.RIGHT_CONTROL;
                case (DeviceKeys.ARROW_LEFT):
                    return (byte) USBHIDCodes.ARROW_LEFT;
                case (DeviceKeys.ARROW_DOWN):
                    return (byte) USBHIDCodes.ARROW_DOWN;
                case (DeviceKeys.ARROW_RIGHT):
                    return (byte) USBHIDCodes.ARROW_RIGHT;
                case (DeviceKeys.NUM_ZERO):
                    return (byte) USBHIDCodes.NUM_ZERO;
                case (DeviceKeys.NUM_PERIOD):
                    return (byte) USBHIDCodes.NUM_PERIOD;

                default:
                    return (byte) USBHIDCodes.ERROR;
            }
        }

    }
}
