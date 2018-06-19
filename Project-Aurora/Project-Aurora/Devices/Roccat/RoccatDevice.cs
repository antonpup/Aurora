/* Credit to https://github.com/horgeon 
 * for his initial developments in Roccat support
 * here: https://github.com/horgeon/Aurora/commits/dev
 */

/* Side notes about this device:
 * SDK Docs state "Due to hardware and protocol limitations, the approximate latency for on/off events is currently about 20 to 30ms." So there might be a delay for Ryos lighting.
 * The SDK also only allows for individual LEDs to be either ON or OFF, no per-key color lighting.
 */

/* 2018 Update:
 * Completed support for Ryos MK FX
 * Per-key color lighting is now supported, all leds are set in one SetMkFxKeyboardState() call
 * Only US layout is currently supported.
 *
 * REQUIREMENTS:
 * Roccat Talk FX must be installed and running (there should be an icon in system tray)
 * Download: https://www.roccat.org/en-US/Products/Gaming-Software/Talk-FX/
 *
 * 3rd party DLLs:
 * - Roccat-Talk.dll (from: https://github.com/mwasilak/roccat-talk-csharp , branch feature-ryos-mk-fx)
 * - talkfx-c.dll (from: https://github.com/mwasilak/talkfx-c-wrapper , branch feature-ryos-mk-fx)
 */

using Roccat_Talk.RyosTalkFX;
using Roccat_Talk.TalkFX;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

        private byte layout = 0x01; //TALKFX_RYOS_LAYOUT_US

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

                    if (talkFX == null ||
                        RyosTalkFX == null ||
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
                    Global.logger.Error("Roccat device, Exception! Message:" + ex);
                }

                isInitialized = false;
                return false;
            }

            return isInitialized;
        }

        public void Shutdown()
        {
            if (talkFX != null)
            {
                talkFX.RestoreLedRgb();
            }

            if (RyosTalkFX != null)
            {
                RyosTalkFX.ExitSdkMode();
            }
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

        public bool UpdateDevice(Dictionary<DeviceKeys, System.Drawing.Color> keyColors, CancellationToken token, bool forced = false)
        {
            if (token.IsCancellationRequested) return false;
            if (RyosTalkFX == null || !RyosInitialized)
            {
                return false;
            }

            try
            {
                byte[] stateStruct = new byte[110];
                Roccat_Talk.TalkFX.Color[] colorStruct = new Roccat_Talk.TalkFX.Color[110];

                for(byte i=0; i<110; i++)
                {
                    if (token.IsCancellationRequested) return false;
                    if (i==79) { //key no 79 is not available in US layout
                        continue;
                    }
                    DeviceKeys key = toAuroraKey(i);
                    System.Drawing.Color color = keyColors[key];
                    Global.logger.LogLine("Roccat update device: " + key + " , " + color);
                    Roccat_Talk.TalkFX.Color roccatColor = ConvertToRoccatColor(color);
                    stateStruct[i] = IsLedOn(roccatColor);
                    colorStruct[i] = roccatColor;
                }

                RyosTalkFX.SetMkFxKeyboardState(stateStruct, colorStruct, layout);

                return true;
            }
            catch (Exception e)
            {
                Global.logger.Error("Roccat device, error when updating device. Error: " + e);
                return false;
            }
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, CancellationToken token, bool forced = false)
        {
            watch.Restart();

            if (token.IsCancellationRequested) return false;

            bool update_result = UpdateDevice(colorComposition.keyColors, token, forced);

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return update_result;
        }

        private byte IsLedOn(Roccat_Talk.TalkFX.Color roccatColor)
        {
            if (roccatColor.Red == 0 && roccatColor.Green == 0 && roccatColor.Blue == 0)
            {
                return 0;
            }
            return 1;
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

        public static DeviceKeys toAuroraKey(byte code)
        {
            switch (code)
            {
                case (0):
                    return DeviceKeys.ESC;
                case (1):
                    return DeviceKeys.F1;
                case (2):
                    return DeviceKeys.F2;
                case (3):
                    return DeviceKeys.F3;
                case (4):
                    return DeviceKeys.F4;
                case (5):
                    return DeviceKeys.F5;
                case (6):
                    return DeviceKeys.F6;
                case (7):
                    return DeviceKeys.F7;
                case (8):
                    return DeviceKeys.F8;
                case (9):
                    return DeviceKeys.F9;
                case (10):
                    return DeviceKeys.F10;
                case (11):
                    return DeviceKeys.F11;
                case (12):
                    return DeviceKeys.F12;
                case (13):
                    return DeviceKeys.PRINT_SCREEN;
                case (14):
                    return DeviceKeys.SCROLL_LOCK;
                case (15):
                    return DeviceKeys.PAUSE_BREAK;
                case (16):
                    return DeviceKeys.G1;
                case (17):
                    return DeviceKeys.TILDE;
                case (18):
                    return DeviceKeys.ONE;
                case (19):
                    return DeviceKeys.TWO;
                case (20):
                    return DeviceKeys.THREE;
                case (21):
                    return DeviceKeys.FOUR;
                case (22):
                    return DeviceKeys.FIVE;
                case (23):
                    return DeviceKeys.SIX;
                case (24):
                    return DeviceKeys.SEVEN;
                case (25):
                    return DeviceKeys.EIGHT;
                case (26):
                    return DeviceKeys.NINE;
                case (27):
                    return DeviceKeys.ZERO;
                case (28):
                    return DeviceKeys.MINUS;
                case (29):
                    return DeviceKeys.EQUALS;
                case (30):
                    return DeviceKeys.BACKSPACE;
                case (31):
                    return DeviceKeys.INSERT;
                case (32):
                    return DeviceKeys.HOME;
                case (33):
                    return DeviceKeys.PAGE_UP;
                case (34):
                    return DeviceKeys.NUM_LOCK;
                case (35):
                    return DeviceKeys.NUM_SLASH;
                case (36):
                    return DeviceKeys.NUM_ASTERISK;
                case (37):
                    return DeviceKeys.NUM_MINUS;
                case (38):
                    return DeviceKeys.G2;
                case (39):
                    return DeviceKeys.TAB;
                case (40):
                    return DeviceKeys.Q;
                case (41):
                    return DeviceKeys.W;
                case (42):
                    return DeviceKeys.E;
                case (43):
                    return DeviceKeys.R;
                case (44):
                    return DeviceKeys.T;
                case (45):
                    return DeviceKeys.Y;
                case (46):
                    return DeviceKeys.U;
                case (47):
                    return DeviceKeys.I;
                case (48):
                    return DeviceKeys.O;
                case (49):
                    return DeviceKeys.P;
                case (50):
                    return DeviceKeys.OPEN_BRACKET;
                case (51):
                    return DeviceKeys.CLOSE_BRACKET;
                case (52):
                    return DeviceKeys.BACKSLASH;
                case (53):
                    return DeviceKeys.DELETE;
                case (54):
                    return DeviceKeys.END;
                case (55):
                    return DeviceKeys.PAGE_DOWN;
                case (56):
                    return DeviceKeys.NUM_SEVEN;
                case (57):
                    return DeviceKeys.NUM_EIGHT;
                case (58):
                    return DeviceKeys.NUM_NINE;
                case (59):
                    return DeviceKeys.NUM_PLUS;
                case (60):
                    return DeviceKeys.G3;
                case (61):
                    return DeviceKeys.CAPS_LOCK;
                case (62):
                    return DeviceKeys.A;
                case (63):
                    return DeviceKeys.S;
                case (64):
                    return DeviceKeys.D;
                case (65):
                    return DeviceKeys.F;
                case (66):
                    return DeviceKeys.G;
                case (67):
                    return DeviceKeys.H;
                case (68):
                    return DeviceKeys.J;
                case (69):
                    return DeviceKeys.K;
                case (70):
                    return DeviceKeys.L;
                case (71):
                    return DeviceKeys.SEMICOLON;
                case (72):
                    return DeviceKeys.APOSTROPHE;
                case (73):
                    return DeviceKeys.ENTER;
                case (74):
                    return DeviceKeys.NUM_FOUR;
                case (75):
                    return DeviceKeys.NUM_FIVE;
                case (76):
                    return DeviceKeys.NUM_SIX;
                case (77):
                    return DeviceKeys.G4;
                case (78):
                    return DeviceKeys.LEFT_SHIFT;
                case (79):
                    return DeviceKeys.BACKSLASH_UK;
                case (80):
                    return DeviceKeys.Z;
                case (81):
                    return DeviceKeys.X;
                case (82):
                    return DeviceKeys.C;
                case (83):
                    return DeviceKeys.V;
                case (84):
                    return DeviceKeys.B;
                case (85):
                    return DeviceKeys.N;
                case (86):
                    return DeviceKeys.M;
                case (87):
                    return DeviceKeys.COMMA;
                case (88):
                    return DeviceKeys.PERIOD;
                case (89):
                    return DeviceKeys.FORWARD_SLASH;
                case (90):
                    return DeviceKeys.RIGHT_SHIFT;
                case (91):
                    return DeviceKeys.ARROW_UP;
                case (92):
                    return DeviceKeys.NUM_ONE;
                case (93):
                    return DeviceKeys.NUM_TWO;
                case (94):
                    return DeviceKeys.NUM_THREE;
                case (95):
                    return DeviceKeys.NUM_ENTER;
                case (96):
                    return DeviceKeys.G5;
                case (97):
                    return DeviceKeys.LEFT_CONTROL;
                case (98):
                    return DeviceKeys.LEFT_WINDOWS;
                case (99):
                    return DeviceKeys.LEFT_ALT;
                case (100):
                    return DeviceKeys.SPACE;
                case (101):
                    return DeviceKeys.RIGHT_ALT;
                case (102):
                    return DeviceKeys.FN_Key;
                case (103):
                    return DeviceKeys.APPLICATION_SELECT;
                case (104):
                    return DeviceKeys.RIGHT_CONTROL;
                case (105):
                    return DeviceKeys.ARROW_LEFT;
                case (106):
                    return DeviceKeys.ARROW_DOWN;
                case (107):
                    return DeviceKeys.ARROW_RIGHT;
                case (108):
                    return DeviceKeys.NUM_ZERO;
                case (109):
                    return DeviceKeys.NUM_PERIOD;
                default:
                    return DeviceKeys.ESC;
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