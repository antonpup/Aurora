/* Credit to https://github.com/horgeon 
 * for his initial developments in Roccat support
 * here: https://github.com/horgeon/Aurora/commits/dev
 */

/* Side notes about this device:
 * SDK Docs state "Due to hardware and protocol limitations, the approximate latency for on/off events is currently about 20 to 30ms." So there might be a delay for Ryos lighting.
 * The SDK also only allows for individual LEDs to be either ON or OFF, no per-key color lighting.
 */

using Roccat_Talk.RyosTalkFX;
using Roccat_Talk.TalkFX;
using System;
using System.Collections.Generic;
using Aurora.Settings;

namespace Aurora.Devices.Roccat
{
    class RoccatDevice : Device
    {
        private String devicename = "Roccat";
        private bool isInitialized = false;

        private TalkFxConnection talkFX = null;
        private RyosTalkFXConnection RyosTalkFX = null;
        private bool RyosInitialized = false;

        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;

        private System.Drawing.Color previous_peripheral_Color = System.Drawing.Color.Black;

        public string GetDeviceName()
        {
            return devicename;
        }

        public string GetDeviceDetails()
        {
            if (isInitialized)
            {
                return devicename + ": " + (talkFX != null ? "TalkFX Initialized " : "") + (RyosTalkFX != null && RyosInitialized ? "RyosTalkFX Initialized " : "");
            }
            else
            {
                return devicename + ": Not initialized";
            }
        }

        public bool Initialize()
        {
            if (!isInitialized)
            {
                try
                {
                    talkFX = new TalkFxConnection();
                    RyosTalkFX = new RyosTalkFXConnection();

                    if (RyosTalkFX != null)
                    {
                        RyosInitialized = RyosTalkFX.Initialize();
                        RyosInitialized = RyosInitialized && RyosTalkFX.EnterSdkMode();
                    }


                    if (talkFX == null &&
                        RyosTalkFX == null &&
                        !RyosInitialized
                        )
                    {
                        throw new Exception("No devices connected");
                    }

                    isInitialized = true;
                    return true;
                }
                catch (Exception ex)
                {
                    Global.logger.LogLine("Roccat device, Exception! Message:" + ex, Logging_Level.Error);
                }

                isInitialized = false;
                return false;
            }

            return isInitialized;
        }

        public void Shutdown()
        {
            if (talkFX != null)
                talkFX.RestoreLedRgb();

            if (RyosTalkFX != null)
                RyosTalkFX.ExitSdkMode();

        }

        public void Reset()
        {
            if (this.IsInitialized())
            {
                talkFX.RestoreLedRgb();
            }
        }

        public bool Reconnect()
        {
            throw new NotImplementedException();
        }

        public bool IsInitialized()
        {
            return isInitialized;
        }

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, System.Drawing.Color> keyColors, bool forced = false)
        {
            if (RyosTalkFX == null || !RyosInitialized) //Not initialzied
                return false;

            try
            {
                foreach (KeyValuePair<DeviceKeys, System.Drawing.Color> key in keyColors)
                {
                    Roccat_Talk.RyosTalkFX.Key roc_key = ToRoccatKey(key.Key);

                    if (roc_key.Code != 255)
                    {
                        if (Utils.ColorUtils.IsColorDark(key.Value))
                            RyosTalkFX.SetLedOff(roc_key);
                        else
                            RyosTalkFX.SetLedOn(roc_key);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Global.logger.LogLine("Roccat device, error when updating device. Error: " + e, Logging_Level.Error);
                return false;
            }
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, bool forced = false)
        {
            watch.Restart();

            System.Drawing.Color averageColor = Utils.BitmapUtils.GetRegionColor(
                    colorComposition.keyBitmap,
                    new BitmapRectangle(0, 0, colorComposition.keyBitmap.Width, colorComposition.keyBitmap.Height)
                    );

            talkFX.SetLedRgb(Zone.Event, KeyEffect.On, Speed.Normal, ConvertToRoccatColor(averageColor));
            talkFX.SetLedRgb(Zone.Ambient, KeyEffect.On, Speed.Normal, ConvertToRoccatColor(averageColor));


            bool update_result = UpdateDevice(colorComposition.keyColors, forced);

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return update_result;
        }

        private void SendColorToPeripheral(System.Drawing.Color color)
        {
            talkFX.SetLedRgb(Zone.Event, KeyEffect.On, Speed.Normal, ConvertToRoccatColor(color));
        }

        public bool IsKeyboardConnected()
        {
            return false;
        }

        public bool IsPeripheralConnected()
        {
            return this.IsInitialized();
        }

        private Roccat_Talk.TalkFX.Color ConvertToRoccatColor(System.Drawing.Color color)
        {
            return new Roccat_Talk.TalkFX.Color(color.R, color.G, color.B);
        }

        public static Roccat_Talk.RyosTalkFX.Key ToRoccatKey(DeviceKeys key)
        {
            switch (key)
            {
                case (DeviceKeys.ESC):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.ESC;
                case (DeviceKeys.F1):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.F1;
                case (DeviceKeys.F2):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.F2;
                case (DeviceKeys.F3):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.F3;
                case (DeviceKeys.F4):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.F4;
                case (DeviceKeys.F5):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.F5;
                case (DeviceKeys.F6):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.F6;
                case (DeviceKeys.F7):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.F7;
                case (DeviceKeys.F8):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.F8;
                case (DeviceKeys.F9):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.F9;
                case (DeviceKeys.F10):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.F10;
                case (DeviceKeys.F11):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.F11;
                case (DeviceKeys.F12):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.F12;
                case (DeviceKeys.PRINT_SCREEN):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.PRINT_SCREEN;
                case (DeviceKeys.SCROLL_LOCK):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.SCROLL_LOCK;
                case (DeviceKeys.PAUSE_BREAK):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.PAUSE;
                case (DeviceKeys.TILDE):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.GRAVE;
                case (DeviceKeys.ONE):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.ONE;
                case (DeviceKeys.TWO):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.TWO;
                case (DeviceKeys.THREE):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.THREE;
                case (DeviceKeys.FOUR):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.FOUR;
                case (DeviceKeys.FIVE):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.FIVE;
                case (DeviceKeys.SIX):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.SIX;
                case (DeviceKeys.SEVEN):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.SEVEN;
                case (DeviceKeys.EIGHT):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.EIGHT;
                case (DeviceKeys.NINE):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.NINE;
                case (DeviceKeys.ZERO):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.ZERO;
                case (DeviceKeys.MINUS):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.HYPHEN;
                case (DeviceKeys.EQUALS):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.EQUALS;
                case (DeviceKeys.BACKSPACE):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.BACKSPACE;
                case (DeviceKeys.INSERT):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.INSERT;
                case (DeviceKeys.HOME):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.HOME;
                case (DeviceKeys.PAGE_UP):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.PAGE_UP;
                case (DeviceKeys.NUM_LOCK):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.NUM_LOCK;
                case (DeviceKeys.NUM_SLASH):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.KP_SLASH;
                case (DeviceKeys.NUM_ASTERISK):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.KP_ASTERISK;
                case (DeviceKeys.NUM_MINUS):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.KP_HYPHEN;
                case (DeviceKeys.TAB):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.TAB;
                case (DeviceKeys.Q):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.Q;
                case (DeviceKeys.W):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.W;
                case (DeviceKeys.E):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.E;
                case (DeviceKeys.R):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.R;
                case (DeviceKeys.T):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.T;
                case (DeviceKeys.Y):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.Y;
                case (DeviceKeys.U):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.U;
                case (DeviceKeys.I):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.I;
                case (DeviceKeys.O):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.O;
                case (DeviceKeys.P):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.P;
                case (DeviceKeys.OPEN_BRACKET):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.LEFT_BRACKET;
                case (DeviceKeys.CLOSE_BRACKET):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.RIGHT_BRACKET;
                case (DeviceKeys.BACKSLASH):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.BACKSLASH;
                case (DeviceKeys.DELETE):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.DELETE;
                case (DeviceKeys.END):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.END;
                case (DeviceKeys.PAGE_DOWN):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.PAGE_DOWN;
                case (DeviceKeys.NUM_SEVEN):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.KP_SEVEN;
                case (DeviceKeys.NUM_EIGHT):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.KP_EIGHT;
                case (DeviceKeys.NUM_NINE):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.KP_NINE;
                case (DeviceKeys.NUM_PLUS):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.KP_PLUS;
                case (DeviceKeys.CAPS_LOCK):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.CAPS_LOCK;
                case (DeviceKeys.A):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.A;
                case (DeviceKeys.S):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.S;
                case (DeviceKeys.D):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.D;
                case (DeviceKeys.F):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.F;
                case (DeviceKeys.G):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.G;
                case (DeviceKeys.H):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.H;
                case (DeviceKeys.J):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.J;
                case (DeviceKeys.K):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.K;
                case (DeviceKeys.L):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.L;
                case (DeviceKeys.SEMICOLON):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.SEMI_COLON;
                case (DeviceKeys.APOSTROPHE):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.APOSTROPHE;
                //case (DeviceKeys.HASHTAG):
                //    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.HASHTAG;
                case (DeviceKeys.ENTER):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.ENTER;
                case (DeviceKeys.NUM_FOUR):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.KP_FOUR;
                case (DeviceKeys.NUM_FIVE):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.KP_FIVE;
                case (DeviceKeys.NUM_SIX):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.KP_SIX;
                case (DeviceKeys.LEFT_SHIFT):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.LEFT_SHIFT;
                case (DeviceKeys.BACKSLASH_UK):
                    return new Roccat_Talk.RyosTalkFX.Key(79);
                case (DeviceKeys.Z):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.Z;
                case (DeviceKeys.X):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.X;
                case (DeviceKeys.C):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.C;
                case (DeviceKeys.V):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.V;
                case (DeviceKeys.B):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.B;
                case (DeviceKeys.N):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.N;
                case (DeviceKeys.M):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.M;
                case (DeviceKeys.COMMA):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.COMMA;
                case (DeviceKeys.PERIOD):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.PERIOD;
                case (DeviceKeys.FORWARD_SLASH):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.FORWARD_SLASH;
                case (DeviceKeys.RIGHT_SHIFT):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.RIGHT_SHIFT;
                case (DeviceKeys.ARROW_UP):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.UP;
                case (DeviceKeys.NUM_ONE):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.KP_ONE;
                case (DeviceKeys.NUM_TWO):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.KP_TWO;
                case (DeviceKeys.NUM_THREE):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.KP_THREE;
                case (DeviceKeys.NUM_ENTER):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.KP_ENTER;
                case (DeviceKeys.LEFT_CONTROL):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.LEFT_CTRL;
                case (DeviceKeys.LEFT_WINDOWS):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.WIN;
                case (DeviceKeys.LEFT_ALT):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.LEFT_ALT;
                case (DeviceKeys.SPACE):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.SPACE;
                case (DeviceKeys.RIGHT_ALT):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.RIGHT_ALT;
                case (DeviceKeys.FN_Key):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.FN;
                case (DeviceKeys.APPLICATION_SELECT):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.MENU;
                case (DeviceKeys.RIGHT_CONTROL):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.RIGHT_CTRL;
                case (DeviceKeys.ARROW_LEFT):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.LEFT;
                case (DeviceKeys.ARROW_DOWN):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.DOWN;
                case (DeviceKeys.ARROW_RIGHT):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.RIGHT;
                case (DeviceKeys.NUM_ZERO):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.KP_ZERO;
                case (DeviceKeys.NUM_PERIOD):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.KP_PERIOD;

                case (DeviceKeys.G1):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.M1;
                case (DeviceKeys.G2):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.M2;
                case (DeviceKeys.G3):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.M3;
                case (DeviceKeys.G4):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.M4;
                case (DeviceKeys.G5):
                    return Roccat_Talk.RyosTalkFX.KeyboardLayouts.KeyboardLayout_EN.M5;

                default:
                    return new Roccat_Talk.RyosTalkFX.Key(255);
            }
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