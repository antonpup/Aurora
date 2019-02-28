using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Aurora.Settings;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SteelSeries.GameSenseSDK;
using System.ComponentModel;
using Aurora.Devices.Layout.Layouts;
using Aurora.Devices.Layout;
using LEDINT = System.Int16;
using System.Dynamic;

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
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                SteelSeriesInstallInstructions instructions = new SteelSeriesInstallInstructions();
                                instructions.ShowDialog();
                            });
                            Global.Configuration.steelseries_first_time = false;
                            Settings.ConfigManager.Save(Global.Configuration);
                        }
                        isInitialized = true;
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Global.logger.Error("SteelSeries GameSense SDK could not be initialized: " + ex);

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
                    Global.logger.Error("There was an error shutting down SteelSeries GameSense SDK: " + ex);
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

        public bool UpdateDevice(MouseDeviceLayout device, PayloadColorEventJSON colorEvent, DoWorkEventArgs e, bool forced = false)
        {
            if (e.Cancel) return false;

            foreach (KeyValuePair<LEDINT, Color> key in device.DeviceColours.deviceColours)
            {
                if (e.Cancel) return false;

                Color color = (Color)key.Value;

                // JSON serializer doesn't understand keyvaluepairs, so single-item dictionaries are the way to go.
                Dictionary<string, dynamic> colorPayload = new Dictionary<string, dynamic>();
                colorPayload.Add("color", new int[] { color.R, color.G, color.B });
                switch ((MouseLights)key.Key)
                {
                    case MouseLights.Peripheral_ScrollWheel:
                        colorEvent.data.Add("mousewheel", colorPayload);
                        break;
                    case MouseLights.Peripheral_Logo:
                        colorEvent.data.Add("mouselogo", colorPayload);
                        break;
                }
            }

            if (e.Cancel) return false;
            return true;
        }

        public bool UpdateDevice(KeyboardDeviceLayout device, PayloadColorEventJSON colorEvent, DoWorkEventArgs e, bool forced = false)
        {
            if (e.Cancel) return false;

            List<byte> hids = new List<byte>();
            // The serializer considers byte arrays to be strings, we need ints
            List<int[]> colors = new List<int[]>();

            foreach (KeyValuePair<LEDINT, Color> key in device.DeviceColours.deviceColours)
            {
                if (e.Cancel) return false;
 
                byte hid = GetHIDCode((KeyboardKeys)key.Key);

                if (hid != (byte)USBHIDCodes.ERROR)
                {
                    hids.Add(hid);
                    colors.Add(new int[] { key.Value.R, key.Value.G, key.Value.B });
                }
            }

            if (e.Cancel) return false;

            Dictionary<string, dynamic> keyboardPayload = new Dictionary<string, dynamic>();
            keyboardPayload.Add("hids", hids);
            keyboardPayload.Add("colors", colors);
            colorEvent.data.Add("keyboard", keyboardPayload);

            return true;
        }

        public bool UpdateDevice(Color globalColor, List<DeviceLayout> devices, DoWorkEventArgs e, bool forced = false)
        {
            watch.Restart();

            bool updateResult = true;

            try
            {
                PayloadColorEventJSON colorEvent = new PayloadColorEventJSON();
                colorEvent.data = new Dictionary<string, dynamic>();
                // workaround for heartbeat/keepalive events every 10sec
                SendKeepalive();

                foreach (DeviceLayout layout in devices)
                {
                    switch (layout)
                    {
                        case KeyboardDeviceLayout kb:
                            if (!UpdateDevice(kb, colorEvent, e, forced))
                                updateResult = false;
                            break;
                        case MouseDeviceLayout mouse:
                            if (!UpdateDevice(mouse, colorEvent, e, forced))
                                updateResult = false;
                            break;
                    }
                }

                gameSenseSDK.sendEventPayload(colorEvent);
            }
            catch (Exception ex)
            {
                Global.logger.Error("SteelSeries GameSense SDK, error when updating device: " + ex);
                return false;
            }

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return updateResult;
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

        public static byte GetHIDCode(KeyboardKeys key)
        {

            switch (key)
            {
                case (KeyboardKeys.LOGO):
                    return (byte)SteelSeriesKeyCodes.LOGO;
                case (KeyboardKeys.FN_Key):
                    return (byte)SteelSeriesKeyCodes.SS_KEY;
                case (KeyboardKeys.G0):
                    return (byte)SteelSeriesKeyCodes.G0;
                case (KeyboardKeys.G1):
                    return (byte)SteelSeriesKeyCodes.G1;
                case (KeyboardKeys.G2):
                    return (byte)SteelSeriesKeyCodes.G2;
                case (KeyboardKeys.G3):
                    return (byte)SteelSeriesKeyCodes.G3;
                case (KeyboardKeys.G4):
                    return (byte)SteelSeriesKeyCodes.G4;
                case (KeyboardKeys.G5):
                    return (byte)SteelSeriesKeyCodes.G5;
                case (KeyboardKeys.ESC):
                    return (byte)USBHIDCodes.ESC;
                case (KeyboardKeys.F1):
                    return (byte)USBHIDCodes.F1;
                case (KeyboardKeys.F2):
                    return (byte)USBHIDCodes.F2;
                case (KeyboardKeys.F3):
                    return (byte)USBHIDCodes.F3;
                case (KeyboardKeys.F4):
                    return (byte)USBHIDCodes.F4;
                case (KeyboardKeys.F5):
                    return (byte)USBHIDCodes.F5;
                case (KeyboardKeys.F6):
                    return (byte)USBHIDCodes.F6;
                case (KeyboardKeys.F7):
                    return (byte)USBHIDCodes.F7;
                case (KeyboardKeys.F8):
                    return (byte)USBHIDCodes.F8;
                case (KeyboardKeys.F9):
                    return (byte)USBHIDCodes.F9;
                case (KeyboardKeys.F10):
                    return (byte)USBHIDCodes.F10;
                case (KeyboardKeys.F11):
                    return (byte)USBHIDCodes.F11;
                case (KeyboardKeys.F12):
                    return (byte)USBHIDCodes.F12;
                case (KeyboardKeys.PRINT_SCREEN):
                    return (byte)USBHIDCodes.PRINT_SCREEN;
                case (KeyboardKeys.SCROLL_LOCK):
                    return (byte)USBHIDCodes.SCROLL_LOCK;
                case (KeyboardKeys.PAUSE_BREAK):
                    return (byte)USBHIDCodes.PAUSE_BREAK;
                case (KeyboardKeys.JPN_HALFFULLWIDTH):
                    return (byte)USBHIDCodes.TILDE;
                case (KeyboardKeys.OEM5):
                    return (byte)USBHIDCodes.TILDE;
                case (KeyboardKeys.TILDE):
                    return (byte)USBHIDCodes.TILDE;
                case (KeyboardKeys.ONE):
                    return (byte)USBHIDCodes.ONE;
                case (KeyboardKeys.TWO):
                    return (byte)USBHIDCodes.TWO;
                case (KeyboardKeys.THREE):
                    return (byte)USBHIDCodes.THREE;
                case (KeyboardKeys.FOUR):
                    return (byte)USBHIDCodes.FOUR;
                case (KeyboardKeys.FIVE):
                    return (byte)USBHIDCodes.FIVE;
                case (KeyboardKeys.SIX):
                    return (byte)USBHIDCodes.SIX;
                case (KeyboardKeys.SEVEN):
                    return (byte)USBHIDCodes.SEVEN;
                case (KeyboardKeys.EIGHT):
                    return (byte)USBHIDCodes.EIGHT;
                case (KeyboardKeys.NINE):
                    return (byte)USBHIDCodes.NINE;
                case (KeyboardKeys.ZERO):
                    return (byte)USBHIDCodes.ZERO;
                case (KeyboardKeys.MINUS):
                    return (byte)USBHIDCodes.MINUS;
                case (KeyboardKeys.EQUALS):
                    return (byte)USBHIDCodes.EQUALS;
                case (KeyboardKeys.BACKSPACE):
                    return (byte)USBHIDCodes.BACKSPACE;
                case (KeyboardKeys.INSERT):
                    return (byte)USBHIDCodes.INSERT;
                case (KeyboardKeys.HOME):
                    return (byte)USBHIDCodes.HOME;
                case (KeyboardKeys.PAGE_UP):
                    return (byte)USBHIDCodes.PAGE_UP;
                case (KeyboardKeys.NUM_LOCK):
                    return (byte)USBHIDCodes.NUM_LOCK;
                case (KeyboardKeys.NUM_SLASH):
                    return (byte)USBHIDCodes.NUM_SLASH;
                case (KeyboardKeys.NUM_ASTERISK):
                    return (byte)USBHIDCodes.NUM_ASTERISK;
                case (KeyboardKeys.NUM_MINUS):
                    return (byte)USBHIDCodes.NUM_MINUS;
                case (KeyboardKeys.TAB):
                    return (byte)USBHIDCodes.TAB;
                case (KeyboardKeys.Q):
                    return (byte)USBHIDCodes.Q;
                case (KeyboardKeys.W):
                    return (byte)USBHIDCodes.W;
                case (KeyboardKeys.E):
                    return (byte)USBHIDCodes.E;
                case (KeyboardKeys.R):
                    return (byte)USBHIDCodes.R;
                case (KeyboardKeys.T):
                    return (byte)USBHIDCodes.T;
                case (KeyboardKeys.Y):
                    return (byte)USBHIDCodes.Y;
                case (KeyboardKeys.U):
                    return (byte)USBHIDCodes.U;
                case (KeyboardKeys.I):
                    return (byte)USBHIDCodes.I;
                case (KeyboardKeys.O):
                    return (byte)USBHIDCodes.O;
                case (KeyboardKeys.P):
                    return (byte)USBHIDCodes.P;
                case (KeyboardKeys.OPEN_BRACKET):
                    return (byte)USBHIDCodes.OPEN_BRACKET;
                case (KeyboardKeys.CLOSE_BRACKET):
                    return (byte)USBHIDCodes.CLOSE_BRACKET;
                case (KeyboardKeys.BACKSLASH):
                    return (byte)USBHIDCodes.BACKSLASH;
                case (KeyboardKeys.DELETE):
                    return (byte)USBHIDCodes.KEYBOARD_DELETE;
                case (KeyboardKeys.END):
                    return (byte)USBHIDCodes.END;
                case (KeyboardKeys.PAGE_DOWN):
                    return (byte)USBHIDCodes.PAGE_DOWN;
                case (KeyboardKeys.NUM_SEVEN):
                    return (byte)USBHIDCodes.NUM_SEVEN;
                case (KeyboardKeys.NUM_EIGHT):
                    return (byte)USBHIDCodes.NUM_EIGHT;
                case (KeyboardKeys.NUM_NINE):
                    return (byte)USBHIDCodes.NUM_NINE;
                case (KeyboardKeys.NUM_PLUS):
                    return (byte)USBHIDCodes.NUM_PLUS;
                case (KeyboardKeys.CAPS_LOCK):
                    return (byte)USBHIDCodes.CAPS_LOCK;
                case (KeyboardKeys.A):
                    return (byte)USBHIDCodes.A;
                case (KeyboardKeys.S):
                    return (byte)USBHIDCodes.S;
                case (KeyboardKeys.D):
                    return (byte)USBHIDCodes.D;
                case (KeyboardKeys.F):
                    return (byte)USBHIDCodes.F;
                case (KeyboardKeys.G):
                    return (byte)USBHIDCodes.G;
                case (KeyboardKeys.H):
                    return (byte)USBHIDCodes.H;
                case (KeyboardKeys.J):
                    return (byte)USBHIDCodes.J;
                case (KeyboardKeys.K):
                    return (byte)USBHIDCodes.K;
                case (KeyboardKeys.L):
                    return (byte)USBHIDCodes.L;
                case (KeyboardKeys.SEMICOLON):
                    return (byte)USBHIDCodes.SEMICOLON;
                case (KeyboardKeys.APOSTROPHE):
                    return (byte)USBHIDCodes.APOSTROPHE;
                case (KeyboardKeys.HASH):
                    return (byte)USBHIDCodes.HASHTAG;
                case (KeyboardKeys.ENTER):
                    return (byte)USBHIDCodes.ENTER;
                case (KeyboardKeys.NUM_FOUR):
                    return (byte)USBHIDCodes.NUM_FOUR;
                case (KeyboardKeys.NUM_FIVE):
                    return (byte)USBHIDCodes.NUM_FIVE;
                case (KeyboardKeys.NUM_SIX):
                    return (byte)USBHIDCodes.NUM_SIX;
                case (KeyboardKeys.LEFT_SHIFT):
                    return (byte)USBHIDCodes.LEFT_SHIFT;
                case (KeyboardKeys.BACKSLASH_UK):
                    return (byte)USBHIDCodes.BACKSLASH_UK;
                case (KeyboardKeys.Z):
                    return (byte)USBHIDCodes.Z;
                case (KeyboardKeys.X):
                    return (byte)USBHIDCodes.X;
                case (KeyboardKeys.C):
                    return (byte)USBHIDCodes.C;
                case (KeyboardKeys.V):
                    return (byte)USBHIDCodes.V;
                case (KeyboardKeys.B):
                    return (byte)USBHIDCodes.B;
                case (KeyboardKeys.N):
                    return (byte)USBHIDCodes.N;
                case (KeyboardKeys.M):
                    return (byte)USBHIDCodes.M;
                case (KeyboardKeys.COMMA):
                    return (byte)USBHIDCodes.COMMA;
                case (KeyboardKeys.PERIOD):
                    return (byte)USBHIDCodes.PERIOD;
                case (KeyboardKeys.FORWARD_SLASH):
                    return (byte)USBHIDCodes.FORWARD_SLASH;
                case (KeyboardKeys.OEM8):
                    return (byte)USBHIDCodes.FORWARD_SLASH;
                case (KeyboardKeys.OEM102):
                    return (byte)USBHIDCodes.ERROR;
                case (KeyboardKeys.RIGHT_SHIFT):
                    return (byte)USBHIDCodes.RIGHT_SHIFT;
                case (KeyboardKeys.ARROW_UP):
                    return (byte)USBHIDCodes.ARROW_UP;
                case (KeyboardKeys.NUM_ONE):
                    return (byte)USBHIDCodes.NUM_ONE;
                case (KeyboardKeys.NUM_TWO):
                    return (byte)USBHIDCodes.NUM_TWO;
                case (KeyboardKeys.NUM_THREE):
                    return (byte)USBHIDCodes.NUM_THREE;
                case (KeyboardKeys.NUM_ENTER):
                    return (byte)USBHIDCodes.NUM_ENTER;
                case (KeyboardKeys.LEFT_CONTROL):
                    return (byte)USBHIDCodes.LEFT_CONTROL;
                case (KeyboardKeys.LEFT_WINDOWS):
                    return (byte)USBHIDCodes.LEFT_WINDOWS;
                case (KeyboardKeys.LEFT_ALT):
                    return (byte)USBHIDCodes.LEFT_ALT;
                case (KeyboardKeys.JPN_MUHENKAN):
                    return (byte)USBHIDCodes.JPN_MUHENKAN;
                case (KeyboardKeys.SPACE):
                    return (byte)USBHIDCodes.SPACE;
                case (KeyboardKeys.JPN_HENKAN):
                    return (byte)USBHIDCodes.JPN_HENKAN;
                case (KeyboardKeys.JPN_HIRAGANA_KATAKANA):
                    return (byte)USBHIDCodes.JPN_HIRAGANA_KATAKANA;
                case (KeyboardKeys.RIGHT_ALT):
                    return (byte)USBHIDCodes.RIGHT_ALT;
                case (KeyboardKeys.RIGHT_WINDOWS):
                    return (byte)USBHIDCodes.RIGHT_WINDOWS;
                //case (KeyboardKeys.FN_Key):
                //return (byte) USBHIDCodes.RIGHT_WINDOWS;
                case (KeyboardKeys.APPLICATION_SELECT):
                    return (byte)USBHIDCodes.APPLICATION_SELECT;
                case (KeyboardKeys.RIGHT_CONTROL):
                    return (byte)USBHIDCodes.RIGHT_CONTROL;
                case (KeyboardKeys.ARROW_LEFT):
                    return (byte)USBHIDCodes.ARROW_LEFT;
                case (KeyboardKeys.ARROW_DOWN):
                    return (byte)USBHIDCodes.ARROW_DOWN;
                case (KeyboardKeys.ARROW_RIGHT):
                    return (byte)USBHIDCodes.ARROW_RIGHT;
                case (KeyboardKeys.NUM_ZERO):
                    return (byte)USBHIDCodes.NUM_ZERO;
                case (KeyboardKeys.NUM_PERIOD):
                    return (byte)USBHIDCodes.NUM_PERIOD;

                default:
                    return (byte)USBHIDCodes.ERROR;
            }
        }

    }
}
