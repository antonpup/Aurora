using Masterkeys;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Aurora.Devices.Masterkeys
{
    class MasterkeysDevice : Device
    {
        public static readonly Dictionary<string, int[]> KeyCoords = new Dictionary<string, int[]>
        {
                     {"ESC", new int [] {0,0} },
                     {"F1", new int [] {0, 1} },
                     {"F2", new int [] {0, 2} },
                     {"F3", new int [] {0, 3} },
                     {"F4", new int [] {0, 4} },
                     {"F5", new int [] {0, 6} },
                     {"F6", new int [] {0, 7} },
                     {"F7", new int [] {0, 8} },
                     {"F8", new int [] {0, 9} },
                     {"F9", new int [] {0, 11} },
                     {"F10", new int [] {0, 12} },
                     {"F11", new int [] {0, 13} },
                     {"F12", new int [] {0, 14} },
                     {"PRINT_SCREEN", new int [] {0, 15} },
                     {"SCROLL_LOCK", new int [] {0, 16} },
                     {"PAUSE_BREAK", new int [] {0, 17} },
                     {"TILDE", new int [] {1, 0} },
                     {"ONE", new int [] {1, 1} },
                     {"TWO", new int [] {1, 2} },
                     {"THREE", new int [] {1, 3} },
                     {"FOUR", new int [] {1, 4} },
                     {"FIVE", new int [] {1, 5} },
                     {"SIX", new int [] {1, 6} },
                     {"SEVEN", new int [] {1, 7} },
                     {"EIGHT", new int [] {1, 8} },
                     {"NINE", new int [] {1, 9} },
                     {"ZERO", new int [] {1, 10} },
                     {"MINUS", new int [] {1, 11} },
                     {"EQUALS", new int [] {1, 12} },
                     {"BACKSPACE", new int [] {1, 14} },
                     {"INSERT", new int [] {1, 15} },
                     {"HOME", new int [] {1, 16} },
                     {"PAGE_UP", new int [] {1, 17} },
                     {"NUM_LOCK", new int [] {1, 18} },
                     {"NUM_SLASH", new int [] {1, 19} },
                     {"NUM_ASTERISK", new int [] {1, 20} },
                     {"NUM_MINUS", new int [] {1, 21} },
                     {"TAB", new int [] {2, 0} },
                     {"Q", new int [] {2, 1} },
                     {"W", new int [] {2, 2} },
                     {"E", new int [] {2, 3} },
                     {"R", new int [] {2, 4} },
                     {"T", new int [] {2, 5} },
                     {"Y", new int [] {2, 6} },
                     {"U", new int [] {2, 7} },
                     {"I", new int [] {2, 8} },
                     {"O", new int [] {2, 9} },
                     {"P", new int [] {2, 10} },
                     {"OPEN_BRACKET", new int [] {2, 11} },
                     {"CLOSE_BRACKET", new int [] {2,12} },
                     {"BACKSLASH", new int [] {2, 14} },
                     {"DELETE", new int [] {2, 15} },
                     {"END", new int [] {2, 16} },
                     {"PAGE_DOWN", new int [] {2, 17} },
                     {"NUM_SEVEN", new int [] {2, 18} },
                     {"NUM_EIGHT", new int [] {2, 19} },
                     {"NUM_NINE", new int [] {2, 20} },
                     {"NUM_PLUS", new int [] {2, 21} },
                     {"CAPS_LOCK", new int [] {3, 0} },
                     {"A", new int [] {3, 1} },
                     {"S", new int [] {3, 2} },
                     {"D", new int [] {3, 3} },
                     {"F", new int [] {3, 4} },
                     {"G", new int [] {3, 5} },
                     {"H", new int [] {3, 6} },
                     {"J", new int [] {3, 7} },
                     {"K", new int [] {3, 8} },
                     {"L", new int [] {3, 9} },
                     {"SEMICOLON", new int [] {3, 10} },
                     {"APOSTROPHE", new int [] {3, 11} },
                     {"ENTER", new int [] {3, 14} },
                     {"NUM_FOUR", new int [] {3, 19} },
                     {"NUM_FIVE", new int [] {3, 20} },
                     {"NUM_SIX", new int [] {3, 21} },
                     {"LEFT_SHIFT", new int [] {4, 0} },
                     {"Z", new int [] {4, 2} },
                     {"X", new int [] {4, 3} },
                     {"C", new int [] {4, 4} },
                     {"V", new int [] {4, 5} },
                     {"B", new int [] {4, 6} },
                     {"N", new int [] {4, 7} },
                     {"M", new int [] {4, 8} },
                     {"COMMA", new int [] {4, 9} },
                     {"PERIOD", new int [] {4, 10} },
                     {"FORWARD_SLASH", new int [] {4, 11} },
                     {"RIGHT_SHIFT", new int [] {4, 14} },
                     {"ARROW_UP", new int [] {4, 16} },
                     {"NUM_ONE", new int [] {4, 18} },
                     {"NUM_TWO", new int [] {4, 19} },
                     {"NUM_THREE", new int [] {4, 20} },
                     {"NUM_ENTER", new int [] {4, 21} },
                     {"LEFT_CONTROL", new int [] {5, 0} },
                     {"LEFT_WINDOWS", new int [] {5, 1} },
                     {"LEFT_ALT", new int [] {5, 2} },
                     {"SPACE", new int [] {5, 6} },
                     {"RIGHT_ALT", new int [] {5, 10} },
                     {"RIGHT_WINDOWS", new int [] {5, 11} },
                     {"APPLICATION_SELECT", new int [] {5, 12} },
                     {"RIGHT_CONTROL", new int [] {5, 14} },
                     {"ARROW_LEFT", new int [] {5, 15} },
                     {"ARROW_DOWN", new int [] {5, 16} },
                     {"ARROW_RIGHT", new int [] {5, 17} },
                     {"NUM_ZERO", new int [] {5, 18} },
                     {"NUM_PERIOD", new int [] {5, 20} },
                     {"UNKNOWN", new int [] {-1, -1} }
        };

        private String devicename = "Masterkeys";
        private bool isInitialized = false;

        private bool keyboard_updated = false;
        private bool peripheral_updated = false;

        private readonly object action_lock = new object();


        //Keyboard stuff
        private MasterkeysSDK.COLOR_MATRIX color_matrix = new MasterkeysSDK.COLOR_MATRIX();
        private MasterkeysSDK.KEY_COLOR[,] key_colors = new MasterkeysSDK.KEY_COLOR[MasterkeysSDK.MAX_LED_ROW, MasterkeysSDK.MAX_LED_COLUMN];
        private Color peripheral_Color = Color.Black;

        //Previous data
        private MasterkeysSDK.KEY_COLOR[,] previous_key_colors = new MasterkeysSDK.KEY_COLOR[MasterkeysSDK.MAX_LED_ROW, MasterkeysSDK.MAX_LED_COLUMN];
        private Color previous_peripheral_Color = Color.Black;


        public bool Initialize()
        {
            lock (action_lock)
            {
                if (!isInitialized)
                {
                    try
                    {
                        MasterkeysSDK.SetControlDevice(MasterkeysSDK.DEVICE_INDEX.DEV_MKeys_L);
                        if (MasterkeysSDK.IsDevicePlug() && MasterkeysSDK.EnableLedControl(true))
                        {
                            isInitialized = true;
                            return true;
                        }
                        else
                        {
                            MasterkeysSDK.SetControlDevice(MasterkeysSDK.DEVICE_INDEX.DEV_MKeys_S);
                            if (MasterkeysSDK.IsDevicePlug() && MasterkeysSDK.EnableLedControl(true))
                            {
                                isInitialized = true;
                                return true;
                            }
                            else
                            {
                                Global.logger.LogLine("Masterkeys Keyboard control could not be initialized", Logging_Level.Error);

                                isInitialized = false;
                                return false;
                            }
                        }


                    }
                    catch (Exception exc)
                    {
                        Global.logger.LogLine("There was an error initializing Masterkeys LED SDK.\r\n" + exc.Message, Logging_Level.Error);

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
                    MasterkeysSDK.EnableLedControl(false);
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
            MasterkeysSDK.KEY_COLOR key_color = new MasterkeysSDK.KEY_COLOR(color.R, color.G, color.B);
            key_colors[key[0], key[1]] = key_color;
        }


        private void SendColorsToKeyboard(bool forced = false)
        {
            if (KeyColorComparer.Equals(key_colors, previous_key_colors) || forced)
            {
                color_matrix.KeyColor = key_colors;
                MasterkeysSDK.SetAllLedColor(color_matrix);
                previous_key_colors = null;
                previous_key_colors = key_colors;
                keyboard_updated = true;
            }
        }

        private void SendColorToPeripheral(Color color, bool forced = false)
        {
            peripheral_updated = false;
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
                    string localKey = ToMasterkeys(key.Key);

                    if (localKey != "UNKNOWN")
                    {
                        SetOneKey(KeyCoords[localKey], (Color)key.Value);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Global.logger.LogLine("Failed to Update Device" + e.ToString(), Logging_Level.Error);
                return false;
            }
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, bool forced = false)
        {
            return UpdateDevice(colorComposition.keyColors, forced);
        }

        public static string ToMasterkeys(DeviceKeys key)
        {
            switch (key)
            {
                case (DeviceKeys.ESC):
                    return "ESC";
                case (DeviceKeys.F1):
                    return "F1";
                case (DeviceKeys.F2):
                    return "F2";
                case (DeviceKeys.F3):
                    return "F3";
                case (DeviceKeys.F4):
                    return "F4";
                case (DeviceKeys.F5):
                    return "F5";
                case (DeviceKeys.F6):
                    return "F6";
                case (DeviceKeys.F7):
                    return "F7";
                case (DeviceKeys.F8):
                    return "F8";
                case (DeviceKeys.F9):
                    return "F9";
                case (DeviceKeys.F10):
                    return "F10";
                case (DeviceKeys.F11):
                    return "F11";
                case (DeviceKeys.F12):
                    return "F12";
                case (DeviceKeys.PRINT_SCREEN):
                    return "PRINT_SCREEN";
                case (DeviceKeys.SCROLL_LOCK):
                    return "SCROLL_LOCK";
                case (DeviceKeys.PAUSE_BREAK):
                    return "PAUSE_BREAK";
                case (DeviceKeys.TILDE):
                    return "TILDE";
                case (DeviceKeys.ONE):
                    return "ONE";
                case (DeviceKeys.TWO):
                    return "TWO";
                case (DeviceKeys.THREE):
                    return "THREE";
                case (DeviceKeys.FOUR):
                    return "FOUR";
                case (DeviceKeys.FIVE):
                    return "FIVE";
                case (DeviceKeys.SIX):
                    return "SIX";
                case (DeviceKeys.SEVEN):
                    return "SEVEN";
                case (DeviceKeys.EIGHT):
                    return "EIGHT";
                case (DeviceKeys.NINE):
                    return "NINE";
                case (DeviceKeys.ZERO):
                    return "ZERO";
                case (DeviceKeys.MINUS):
                    return "MINUS";
                case (DeviceKeys.EQUALS):
                    return "EQUALS";
                case (DeviceKeys.BACKSPACE):
                    return "BACKSPACE";
                case (DeviceKeys.INSERT):
                    return "INSERT";
                case (DeviceKeys.HOME):
                    return "HOME";
                case (DeviceKeys.PAGE_UP):
                    return "PAGE_UP";
                case (DeviceKeys.NUM_LOCK):
                    return "NUM_LOCK";
                case (DeviceKeys.NUM_SLASH):
                    return "NUM_SLASH";
                case (DeviceKeys.NUM_ASTERISK):
                    return "NUM_ASTERISK";
                case (DeviceKeys.NUM_MINUS):
                    return "NUM_MINUS";
                case (DeviceKeys.TAB):
                    return "TAB";
                case (DeviceKeys.Q):
                    return "Q";
                case (DeviceKeys.W):
                    return "W";
                case (DeviceKeys.E):
                    return "E";
                case (DeviceKeys.R):
                    return "R";
                case (DeviceKeys.T):
                    return "T";
                case (DeviceKeys.Y):
                    return "Y";
                case (DeviceKeys.U):
                    return "U";
                case (DeviceKeys.I):
                    return "I";
                case (DeviceKeys.O):
                    return "O";
                case (DeviceKeys.P):
                    return "P";
                case (DeviceKeys.OPEN_BRACKET):
                    return "OPEN_BRACKET";
                case (DeviceKeys.CLOSE_BRACKET):
                    return "CLOSE_BRACKET";
                case (DeviceKeys.BACKSLASH):
                    return "BACKSLASH";
                case (DeviceKeys.DELETE):
                    return "DELETE";
                case (DeviceKeys.END):
                    return "END";
                case (DeviceKeys.PAGE_DOWN):
                    return "PAGE_DOWN";
                case (DeviceKeys.NUM_SEVEN):
                    return "NUM_SEVEN";
                case (DeviceKeys.NUM_EIGHT):
                    return "NUM_EIGHT";
                case (DeviceKeys.NUM_NINE):
                    return "NUM_NINE";
                case (DeviceKeys.NUM_PLUS):
                    return "NUM_PLUS";
                case (DeviceKeys.CAPS_LOCK):
                    return "CAPS_LOCK";
                case (DeviceKeys.A):
                    return "A";
                case (DeviceKeys.S):
                    return "S";
                case (DeviceKeys.D):
                    return "D";
                case (DeviceKeys.F):
                    return "F";
                case (DeviceKeys.G):
                    return "G";
                case (DeviceKeys.H):
                    return "H";
                case (DeviceKeys.J):
                    return "J";
                case (DeviceKeys.K):
                    return "K";
                case (DeviceKeys.L):
                    return "L";
                case (DeviceKeys.SEMICOLON):
                    return "SEMICOLON";
                case (DeviceKeys.APOSTROPHE):
                    return "APOSTROPHE";
                case (DeviceKeys.ENTER):
                    return "ENTER";
                case (DeviceKeys.NUM_FOUR):
                    return "NUM_FOUR";
                case (DeviceKeys.NUM_FIVE):
                    return "NUM_FIVE";
                case (DeviceKeys.NUM_SIX):
                    return "NUM_SIX";
                case (DeviceKeys.LEFT_SHIFT):
                    return "LEFT_SHIFT";
                case (DeviceKeys.Z):
                    return "Z";
                case (DeviceKeys.X):
                    return "X";
                case (DeviceKeys.C):
                    return "C";
                case (DeviceKeys.V):
                    return "V";
                case (DeviceKeys.B):
                    return "B";
                case (DeviceKeys.N):
                    return "N";
                case (DeviceKeys.M):
                    return "M";
                case (DeviceKeys.COMMA):
                    return "COMMA";
                case (DeviceKeys.PERIOD):
                    return "PERIOD";
                case (DeviceKeys.FORWARD_SLASH):
                    return "FORWARD_SLASH";
                case (DeviceKeys.RIGHT_SHIFT):
                    return "RIGHT_SHIFT";
                case (DeviceKeys.ARROW_UP):
                    return "ARROW_UP";
                case (DeviceKeys.NUM_ONE):
                    return "NUM_ONE";
                case (DeviceKeys.NUM_TWO):
                    return "NUM_TWO";
                case (DeviceKeys.NUM_THREE):
                    return "NUM_THREE";
                case (DeviceKeys.NUM_ENTER):
                    return "NUM_ENTER";
                case (DeviceKeys.LEFT_CONTROL):
                    return "LEFT_CONTROL";
                case (DeviceKeys.LEFT_WINDOWS):
                    return "LEFT_WINDOWS";
                case (DeviceKeys.LEFT_ALT):
                    return "LEFT_ALT";
                case (DeviceKeys.SPACE):
                    return "SPACE";
                case (DeviceKeys.RIGHT_ALT):
                    return "RIGHT_ALT";
                case (DeviceKeys.RIGHT_WINDOWS):
                    return "RIGHT_WINDOWS";
                case (DeviceKeys.APPLICATION_SELECT):
                    return "APPLICATION_SELECT";
                case (DeviceKeys.RIGHT_CONTROL):
                    return "RIGHT_CONTROL";
                case (DeviceKeys.ARROW_LEFT):
                    return "ARROW_LEFT";
                case (DeviceKeys.ARROW_DOWN):
                    return "ARROW_DOWN";
                case (DeviceKeys.ARROW_RIGHT):
                    return "ARROW_RIGHT";
                case (DeviceKeys.NUM_ZERO):
                    return "NUM_ZERO";
                case (DeviceKeys.NUM_PERIOD):
                    return "NUM_PERIOD";
                default:
                    return "UNKNOWN";
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
