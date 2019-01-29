using Aurora.Devices;
using Aurora.Settings;
using CUE.NET.Devices.Generic.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.RawInput;
using Aurora.Devices.Layout.Layouts;

namespace Aurora.Utils
{
    /// <summary>
    /// A class for Utilities pertaining to keys
    /// </summary>
    public static class KeyUtils
    {
        /// <summary>
        ///     Translates (maps) a virtual-key code into a scan code or character value, or translates a scan code into a
        ///     virtual-key code.
        /// </summary>
        /// <param name="uCode">
        ///     The virtual key code or scan code for a key. How this value is interpreted depends on the value of
        ///     the uMapType parameter.
        /// </param>
        /// <param name="uMapType">
        ///     The translation to be performed. The value of this parameter depends on the value of the uCode
        ///     parameter.
        /// </param>
        /// <returns>
        ///     The return value is either a scan code, a virtual-key code, or a character value, depending on the value of
        ///     uCode and uMapType. If there is no translation, the return value is zero.
        /// </returns>
        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, MapVirtualKeyMapTypes uMapType);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKeyEx(uint uCode, MapVirtualKeyMapTypes uMapType, IntPtr dwhkl);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("user32", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int GetKeyNameTextW(uint lParam, StringBuilder lpString, int nSize);

        private static readonly int leftControlScanCode;

        static KeyUtils()
        {
            leftControlScanCode = Utils.KeyUtils.GetScanCodeByKey(Keys.LControlKey);
        }

        /// <summary>
        /// Converts <see cref="Keys"/> to the hardware scan code/>
        /// </summary>
        /// <param name="key">The key to be converted</param>
        /// <returns>The scan code of the key. If the method fails, the return value is 0</returns>
        private static int GetScanCodeByKey(Keys key)
        {
            return (int)MapVirtualKey((uint)key, 0);
        }

        public static KeyboardKeys GetDeviceKey(Keys forms_key, int scanCode = 0, bool isExtendedKey = false)
        {
            KeyboardKeys key = getDeviceKey(forms_key, scanCode, isExtendedKey);
            //Global.logger.LogLine(key.ToString() + ":" + ((int)key).ToString());
            if (Global.kbLayout.LayoutKeyConversion.ContainsKey(key))
                return Global.kbLayout.LayoutKeyConversion[key];

            return key;
        }

        static Dictionary<uint, DeviceKeys> scanCodeConversion = new Dictionary<uint, DeviceKeys>()
        {
            /*{1, DeviceKeys.ESC},
            {59, DeviceKeys.F1},
            {60, DeviceKeys.F2},
            {61, DeviceKeys.F3},
            {62, DeviceKeys.F4},
            {63, DeviceKeys.F5},
            {64, DeviceKeys.F6},
            {65, DeviceKeys.F7},
            {66, DeviceKeys.F8},
            {67, DeviceKeys.F9},
            {68, DeviceKeys.F10},
            {87, DeviceKeys.F11},
            {88, DeviceKeys.F12},
            {84, DeviceKeys.PRINT_SCREEN},
            {70, DeviceKeys.SCROLL_LOCK},*/
            //{0, DeviceKeys.PAUSE_BREAK},
            {40, DeviceKeys.TILDE},
            /*{2, DeviceKeys.ONE},
            {3, DeviceKeys.TWO},
            {4, DeviceKeys.THREE},
            {5, DeviceKeys.FOUR},
            {6, DeviceKeys.FIVE},
            {7, DeviceKeys.SIX},
            {8, DeviceKeys.SEVEN},
            {9, DeviceKeys.EIGHT},
            {10, DeviceKeys.NINE},
            {11, DeviceKeys.ZERO},*/
            {12, DeviceKeys.MINUS},
            {13, DeviceKeys.EQUALS},
            /*{14, DeviceKeys.BACKSPACE},
            {114, DeviceKeys.INSERT},
            {103, DeviceKeys.HOME},
            {105, DeviceKeys.PAGE_UP},
            {69, DeviceKeys.NUM_LOCK},
            //{53, DeviceKeys.NUM_SLASH},
            {55, DeviceKeys.NUM_ASTERISK},
            {74, DeviceKeys.NUM_MINUS},
            {15, DeviceKeys.TAB},*/
            {16, DeviceKeys.Q},
            {17, DeviceKeys.W},
            {18, DeviceKeys.E},
            {19, DeviceKeys.R},
            {20, DeviceKeys.T},
            {21, DeviceKeys.Y},
            {22, DeviceKeys.U},
            {23, DeviceKeys.I},
            {24, DeviceKeys.O},
            {25, DeviceKeys.P},
            {26, DeviceKeys.OPEN_BRACKET},
            {27, DeviceKeys.CLOSE_BRACKET},
            {86, DeviceKeys.BACKSLASH},
            /*{115, DeviceKeys.DELETE},
            {111, DeviceKeys.END},
            {113, DeviceKeys.PAGE_DOWN},
            {71, DeviceKeys.NUM_SEVEN},
            {72, DeviceKeys.NUM_EIGHT},
            {73, DeviceKeys.NUM_NINE},
            {78, DeviceKeys.NUM_PLUS},
            {58, DeviceKeys.CAPS_LOCK},*/
            {30, DeviceKeys.A},
            {31, DeviceKeys.S},
            {32, DeviceKeys.D},
            {33, DeviceKeys.F},
            {34, DeviceKeys.G},
            {35, DeviceKeys.H},
            {36, DeviceKeys.J},
            {37, DeviceKeys.K},
            {38, DeviceKeys.L},
            {39, DeviceKeys.SEMICOLON},
            {43, DeviceKeys.APOSTROPHE},
            //{0, DeviceKeys.HASH},
            /*{28, DeviceKeys.ENTER},
            {75, DeviceKeys.NUM_FOUR},
            {76, DeviceKeys.NUM_FIVE},
            {77, DeviceKeys.NUM_SIX},
            {42, DeviceKeys.LEFT_SHIFT},*/
            //{0, DeviceKeys.BACKSLASH_UK},
            {44, DeviceKeys.Z},
            {45, DeviceKeys.X},
            {46, DeviceKeys.C},
            {47, DeviceKeys.V},
            {48, DeviceKeys.B},
            {49, DeviceKeys.N},
            {50, DeviceKeys.M},
            {51, DeviceKeys.COMMA},
            {52, DeviceKeys.PERIOD},
            {53, DeviceKeys.FORWARD_SLASH},
            /*{54, DeviceKeys.RIGHT_SHIFT},
            {104, DeviceKeys.ARROW_UP},
            {79, DeviceKeys.NUM_ONE},
            {80, DeviceKeys.NUM_TWO},
            {81, DeviceKeys.NUM_THREE},
            //{28, DeviceKeys.NUM_ENTER},
            {29, DeviceKeys.LEFT_CONTROL},
            {91, DeviceKeys.LEFT_WINDOWS},
            {56, DeviceKeys.LEFT_ALT},
            {57, DeviceKeys.SPACE},
            //{56, DeviceKeys.RIGHT_ALT},
            {92, DeviceKeys.RIGHT_WINDOWS},
            {93, DeviceKeys.APPLICATION_SELECT},
            //{29, DeviceKeys.RIGHT_CONTROL},
            {107, DeviceKeys.ARROW_LEFT},
            {112, DeviceKeys.ARROW_DOWN},
            {109, DeviceKeys.ARROW_RIGHT},
            {82, DeviceKeys.NUM_ZERO},
            {83, DeviceKeys.NUM_PERIOD},*/
            {41, DeviceKeys.OEM8},
            /*{34, DeviceKeys.MEDIA_PLAY_PAUSE},
            {0, DeviceKeys.MEDIA_PLAY},
            {0, DeviceKeys.MEDIA_PAUSE},
            {36, DeviceKeys.MEDIA_STOP},
            {16, DeviceKeys.MEDIA_PREVIOUS},
            {25, DeviceKeys.MEDIA_NEXT},
            {32, DeviceKeys.VOLUME_MUTE},
            {46, DeviceKeys.VOLUME_DOWN},
            {48, DeviceKeys.VOLUME_UP},
            {0, DeviceKeys.JPN_HALFFULLWIDTH},
            {0, DeviceKeys.JPN_MUHENKAN},
            {0, DeviceKeys.JPN_HENKAN},
            {0, DeviceKeys.JPN_HIRAGANA_KATAKANA},
            {0, DeviceKeys.OEM5},
            {0, DeviceKeys.OEMTilde},
            {0, DeviceKeys.OEM102},
            {0, DeviceKeys.OEM6},
            {0, DeviceKeys.OEM6},
            {0, DeviceKeys.OEM1},
            {0, DeviceKeys.OEM1},
            {0, DeviceKeys.OEMPlus},
            {0, DeviceKeys.OEMPlus},*/
        };

        static Dictionary<DeviceKeys, uint> KeyToScanCode = null;

        public static int GetScanCode(DeviceKeys key)
        {
            if (KeyToScanCode == null)
                KeyToScanCode = scanCodeConversion.ToList().ToDictionary((kvp) => kvp.Value, (kvp) => kvp.Key);

            if (KeyToScanCode.TryGetValue(key, out uint scan))
                return (int)scan;
            else
                return -1;
        }


        static bool hasOutput = false;

        /// <summary>
        /// Correcting RawInput data according to an article https://blog.molecular-matters.com/2011/09/05/properly-handling-keyboard-input/
        /// </summary>
        public static void CorrectRawInputData(KeyboardInputEventArgs e)
        {
            // e0 and e1 are escape sequences used for certain special keys, such as PRINT and PAUSE/BREAK.
            // see http://www.win.tue.nl/~aeb/linux/kbd/scancodes-1.html
            bool isE0 = e.ScanCodeFlags.HasFlag(ScanCodeFlags.E0);
            bool isE1 = e.ScanCodeFlags.HasFlag(ScanCodeFlags.E1);
            if (Global.kbLayout.Loaded_Localization.IsAutomaticGeneration() && ((e.Key >= Keys.A && e.Key <= Keys.Z) || (e.Key >= Keys.Oem1 && e.Key <= Keys.Oem102)))
            {
                uint thread = GetWindowThreadProcessId(ActiveProcessMonitor.GetForegroundWindow(), IntPtr.Zero);
                var layout = GetKeyboardLayout(thread);
                var scan_code_locale = MapVirtualKeyEx((uint)e.Key, MapVirtualKeyMapTypes.MapvkVkToVsc, layout);
                if (scan_code_locale == 0)
                    Global.logger.Warn($"Unable to convert key: {e.Key} to scan_code_locale. layout: {layout}");
                else
                {

                    Keys k = (Keys)MapVirtualKey(scan_code_locale, MapVirtualKeyMapTypes.MapvkVscToVk);
                    if (k != Keys.None)
                        e.Key = k;
                    else
                        Global.logger.Warn($"Unable to convert scan_code_locale: {scan_code_locale} to Keys. Key: {e.Key}, layout: {layout}");
                }
            }
            if (isE1)
            {
                // for escaped sequences, turn the virtual key into the correct scan code using MapVirtualKey.
                // however, MapVirtualKey is unable to map VK_PAUSE (this is a known bug), hence we map that by hand.
                if (e.Key == Keys.Pause)
                    e.MakeCode = 0x45;
                else
                    e.MakeCode = (int)MapVirtualKey((uint)e.Key, MapVirtualKeyMapTypes.MapvkVkToVsc);
            }

            switch (e.Key)
            {
                case Keys.NumLock:
                    // correct PAUSE/BREAK and NUM LOCK silliness, and set the extended bit
                    e.MakeCode = (int)(MapVirtualKey((uint)e.Key, MapVirtualKeyMapTypes.MapvkVkToVsc) | 0x100);
                    break;
                case Keys.ShiftKey:
                    // correct left-hand / right-hand SHIFT
                    e.Key = (Keys)MapVirtualKey((uint)e.MakeCode, MapVirtualKeyMapTypes.MapvkVscToVkEx);
                    break;
                case Keys.ControlKey:
                    e.Key = isE0 ? Keys.RControlKey : Keys.LControlKey;
                    break;
                case Keys.Menu:
                    e.Key = isE0 ? Keys.RMenu : Keys.LMenu;
                    break;
            }
        }
        /// <summary>
        /// Converts Devices.DeviceKeys to Forms.Keys
        /// </summary>
        /// <param name="deviceKeys">The Forms.Key to be converted</param>
        /// <returns>The resulting Devices.DeviceKeys</returns>
        public static Keys GetFormsKey(KeyboardKeys keyboardKeys)
        {
            switch (keyboardKeys)
            {
                case (KeyboardKeys.ESC):
                    return Keys.Escape;
                case (KeyboardKeys.BACKSPACE):
                    return Keys.Back;
                case (KeyboardKeys.TAB):
                    return Keys.Tab;
                case (KeyboardKeys.NUM_ENTER):
                    return Keys.Enter;
                case (KeyboardKeys.ENTER):
                    return Keys.Enter;
                case (KeyboardKeys.LEFT_SHIFT):
                    return Keys.LShiftKey;
                case (KeyboardKeys.LEFT_CONTROL):
                    return Keys.LControlKey;
                case (KeyboardKeys.LEFT_ALT):
                    return Keys.LMenu;
                case (KeyboardKeys.JPN_MUHENKAN):
                    return Keys.IMENonconvert;
                case (KeyboardKeys.JPN_HENKAN):
                    return Keys.IMEConvert;
                case (KeyboardKeys.JPN_HIRAGANA_KATAKANA):
                    return Keys.IMEModeChange;
                case (KeyboardKeys.RIGHT_SHIFT):
                    return Keys.RShiftKey;
                case (KeyboardKeys.RIGHT_CONTROL):
                    return Keys.RControlKey;
                case (KeyboardKeys.RIGHT_ALT):
                    return Keys.RMenu;
                case (KeyboardKeys.PAUSE_BREAK):
                    return Keys.Pause;
                case (KeyboardKeys.CAPS_LOCK):
                    return Keys.CapsLock;
                case (KeyboardKeys.SPACE):
                    return Keys.Space;
                case (KeyboardKeys.PAGE_UP):
                    return Keys.PageUp;

                case (KeyboardKeys.PAGE_DOWN):
                    return Keys.PageDown;

                case (KeyboardKeys.END):
                    return Keys.End;
                case (KeyboardKeys.HOME):
                    return Keys.Home;
                case (KeyboardKeys.ARROW_LEFT):
                    return Keys.Left;
                case (KeyboardKeys.ARROW_UP):
                    return Keys.Up;
                case (KeyboardKeys.ARROW_RIGHT):
                    return Keys.Right;
                case (KeyboardKeys.ARROW_DOWN):
                    return Keys.Down;
                case (KeyboardKeys.PRINT_SCREEN):
                    return Keys.PrintScreen;
                case (KeyboardKeys.INSERT):
                    return Keys.Insert;
                case (KeyboardKeys.DELETE):
                    return Keys.Delete;
                case (KeyboardKeys.ZERO):
                    return Keys.D0;
                case (KeyboardKeys.ONE):
                    return Keys.D1;
                case (KeyboardKeys.TWO):
                    return Keys.D2;
                case (KeyboardKeys.THREE):
                    return Keys.D3;
                case (KeyboardKeys.FOUR):
                    return Keys.D4;
                case (KeyboardKeys.FIVE):
                    return Keys.D5;
                case (KeyboardKeys.SIX):
                    return Keys.D6;
                case (KeyboardKeys.SEVEN):
                    return Keys.D7;
                case (KeyboardKeys.EIGHT):
                    return Keys.D8;
                case (KeyboardKeys.NINE):
                    return Keys.D9;
                case (KeyboardKeys.A):
                    return Keys.A;
                case (KeyboardKeys.B):
                    return Keys.B;
                case (KeyboardKeys.C):
                    return Keys.C;
                case (KeyboardKeys.D):
                    return Keys.D;
                case (KeyboardKeys.E):
                    return Keys.E;
                case (KeyboardKeys.F):
                    return Keys.F;
                case (KeyboardKeys.G):
                    return Keys.G;
                case (KeyboardKeys.H):
                    return Keys.H;
                case (KeyboardKeys.I):
                    return Keys.I;
                case (KeyboardKeys.J):
                    return Keys.J;
                case (KeyboardKeys.K):
                    return Keys.K;
                case (KeyboardKeys.L):
                    return Keys.L;
                case (KeyboardKeys.M):
                    return Keys.M;
                case (KeyboardKeys.N):
                    return Keys.N;
                case (KeyboardKeys.O):
                    return Keys.O;
                case (KeyboardKeys.P):
                    return Keys.P;
                case (KeyboardKeys.Q):
                    return Keys.Q;
                case (KeyboardKeys.R):
                    return Keys.R;
                case (KeyboardKeys.S):
                    return Keys.S;
                case (KeyboardKeys.T):
                    return Keys.T;
                case (KeyboardKeys.U):
                    return Keys.U;
                case (KeyboardKeys.V):
                    return Keys.V;
                case (KeyboardKeys.W):
                    return Keys.W;
                case (KeyboardKeys.X):
                    return Keys.X;
                case (KeyboardKeys.Y):
                    return Keys.Y;
                case (KeyboardKeys.Z):
                    return Keys.Z;
                case (KeyboardKeys.LEFT_WINDOWS):
                    return Keys.LWin;
                case (KeyboardKeys.RIGHT_WINDOWS):
                    return Keys.RWin;
                case (KeyboardKeys.APPLICATION_SELECT):
                    return Keys.Apps;
                case (KeyboardKeys.NUM_ZERO):
                    return Keys.NumPad0;
                case (KeyboardKeys.NUM_ONE):
                    return Keys.NumPad1;
                case (KeyboardKeys.NUM_TWO):
                    return Keys.NumPad2;
                case (KeyboardKeys.NUM_THREE):
                    return Keys.NumPad3;
                case (KeyboardKeys.NUM_FOUR):
                    return Keys.NumPad4;
                case (KeyboardKeys.NUM_FIVE):
                    return Keys.NumPad5;
                case (KeyboardKeys.NUM_SIX):
                    return Keys.NumPad6;
                case (KeyboardKeys.NUM_SEVEN):
                    return Keys.NumPad7;
                case (KeyboardKeys.NUM_EIGHT):
                    return Keys.NumPad8;
                case (KeyboardKeys.NUM_NINE):
                    return Keys.NumPad9;
                case (KeyboardKeys.NUM_ASTERISK):
                    return Keys.Multiply;
                case (KeyboardKeys.NUM_PLUS):
                    return Keys.Add;
                case (KeyboardKeys.NUM_MINUS):
                    return Keys.Subtract;
                case (KeyboardKeys.NUM_PERIOD):
                    return Keys.Decimal;
                case (KeyboardKeys.NUM_SLASH):
                    return Keys.Divide;
                case (KeyboardKeys.F1):
                    return Keys.F1;
                case (KeyboardKeys.F2):
                    return Keys.F2;
                case (KeyboardKeys.F3):
                    return Keys.F3;
                case (KeyboardKeys.F4):
                    return Keys.F4;
                case (KeyboardKeys.F5):
                    return Keys.F5;
                case (KeyboardKeys.F6):
                    return Keys.F6;
                case (KeyboardKeys.F7):
                    return Keys.F7;
                case (KeyboardKeys.F8):
                    return Keys.F8;
                case (KeyboardKeys.F9):
                    return Keys.F9;
                case (KeyboardKeys.F10):
                    return Keys.F10;
                case (KeyboardKeys.F11):
                    return Keys.F11;
                case (KeyboardKeys.F12):
                    return Keys.F12;
                case (KeyboardKeys.NUM_LOCK):
                    return Keys.NumLock;
                case (KeyboardKeys.SCROLL_LOCK):
                    return Keys.Scroll;
                case (KeyboardKeys.VOLUME_MUTE):
                    return Keys.VolumeMute;
                case (KeyboardKeys.VOLUME_DOWN):
                    return Keys.VolumeDown;
                case (KeyboardKeys.VOLUME_UP):
                    return Keys.VolumeUp;
                case (KeyboardKeys.MEDIA_NEXT):
                    return Keys.MediaNextTrack;
                case (KeyboardKeys.MEDIA_PREVIOUS):
                    return Keys.MediaPreviousTrack;
                case (KeyboardKeys.MEDIA_STOP):
                    return Keys.MediaStop;
                case (KeyboardKeys.MEDIA_PLAY_PAUSE):
                    return Keys.MediaPlayPause;
                case (KeyboardKeys.SEMICOLON):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return KeyboardKeys.CLOSE_BRACKET;
                    else*/
                    return Keys.OemSemicolon;
                case (KeyboardKeys.EQUALS):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return KeyboardKeys.CLOSE_BRACKET;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return KeyboardKeys.MINUS;
                    else*/
                    return Keys.Oemplus;
                case (KeyboardKeys.COMMA):
                    return Keys.Oemcomma;
                case (KeyboardKeys.MINUS):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return KeyboardKeys.FORWARD_SLASH;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return KeyboardKeys.FORWARD_SLASH;
                    else*/
                    return Keys.OemMinus;
                case (KeyboardKeys.PERIOD):
                    return Keys.OemPeriod;
                case (KeyboardKeys.FORWARD_SLASH):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return KeyboardKeys.HASHTAG;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return KeyboardKeys.HASHTAG;
                    else*/
                    return Keys.OemQuestion;
                case (KeyboardKeys.JPN_HALFFULLWIDTH):
                    return Keys.ProcessKey;
                case (KeyboardKeys.TILDE):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.uk)
                        return KeyboardKeys.APOSTROPHE;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return KeyboardKeys.SEMICOLON;
                    else*/
                    return Keys.Oemtilde;
                case (KeyboardKeys.OPEN_BRACKET):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return KeyboardKeys.MINUS;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return KeyboardKeys.EQUALS;
                    else*/
                    return Keys.OemOpenBrackets;
                case (KeyboardKeys.BACKSLASH):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.uk)
                        return KeyboardKeys.BACKSLASH_UK;
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return KeyboardKeys.TILDE;
                    else*/
                    return Keys.OemPipe;
                case (KeyboardKeys.CLOSE_BRACKET):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return KeyboardKeys.EQUALS;
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return KeyboardKeys.OPEN_BRACKET;
                    else*/
                    return Keys.OemCloseBrackets;
                case (KeyboardKeys.APOSTROPHE):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.uk)
                        return KeyboardKeys.HASHTAG;
                    else*/
                    return Keys.OemQuotes;
                case (KeyboardKeys.BACKSLASH_UK):
                    return Keys.OemBackslash;
                case (KeyboardKeys.OEM8):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.uk)
                        return KeyboardKeys.TILDE;
                    else*/
                    return Keys.Oem8;
                case (KeyboardKeys.MEDIA_PLAY):
                    return Keys.Play;
                default:
                    return Keys.None;
            }
        }
        /// <summary>
        /// Converts Devices.DeviceKeys from RawInput event
        /// </summary>
        /// <param name="eventArgs">RawInput event data</param>
        /// <returns>The resulting Devices.DeviceKeys</returns>
        public static KeyboardKeys GetKeyboardKey(this KeyboardInputEventArgs eventArgs)
        {
            return GetDeviceKey(eventArgs.Key, eventArgs.MakeCode, eventArgs.ScanCodeFlags.HasFlag(ScanCodeFlags.E0));
        }

        /// <summary>
        /// Converts Forms.Keys to Devices.DeviceKeys
        /// </summary>
        /// <param name="forms_key">The Forms.Key to be converted</param>
        /// <returns>The resulting Devices.DeviceKeys</returns>
        private static KeyboardKeys getDeviceKey(Keys forms_key, int scanCode = 0, bool isExtendedKey = false)
        {
            switch (forms_key)
            {
                case (Keys.Escape):
                    return KeyboardKeys.ESC;
                case (Keys.Clear):
                    return KeyboardKeys.NUM_FIVE;
                case (Keys.Back):
                    return KeyboardKeys.BACKSPACE;
                case (Keys.Tab):
                    return KeyboardKeys.TAB;
                case (Keys.Enter):
                    return isExtendedKey ? KeyboardKeys.NUM_ENTER : KeyboardKeys.ENTER;
                case (Keys.LShiftKey):
                    return KeyboardKeys.LEFT_SHIFT;
                case (Keys.LControlKey):
                    if (scanCode > 0 && leftControlScanCode > 0 && scanCode != leftControlScanCode) // Alt Graph
                        return KeyboardKeys.NONE;
                    return KeyboardKeys.LEFT_CONTROL;
                case (Keys.LMenu):
                    return KeyboardKeys.LEFT_ALT;
                case (Keys.IMENonconvert):
                    return KeyboardKeys.JPN_MUHENKAN;
                case (Keys.IMEConvert):
                    return KeyboardKeys.JPN_HENKAN;
                case (Keys.IMEModeChange):
                    return KeyboardKeys.JPN_HIRAGANA_KATAKANA;
                case (Keys.RShiftKey):
                    return KeyboardKeys.RIGHT_SHIFT;
                case (Keys.RControlKey):
                    return KeyboardKeys.RIGHT_CONTROL;
                case (Keys.RMenu):
                    return KeyboardKeys.RIGHT_ALT;
                case (Keys.Pause):
                    return KeyboardKeys.PAUSE_BREAK;
                case (Keys.CapsLock):
                    return KeyboardKeys.CAPS_LOCK;
                case (Keys.Space):
                    return KeyboardKeys.SPACE;
                case (Keys.PageUp):
                    return isExtendedKey ? KeyboardKeys.PAGE_UP : KeyboardKeys.NUM_NINE;
                case (Keys.PageDown):
                    return isExtendedKey ? KeyboardKeys.PAGE_DOWN : KeyboardKeys.NUM_THREE;
                case (Keys.End):
                    return isExtendedKey ? KeyboardKeys.END : KeyboardKeys.NUM_ONE;
                case (Keys.Home):
                    return isExtendedKey ? KeyboardKeys.HOME : KeyboardKeys.NUM_SEVEN;
                case (Keys.Left):
                    return isExtendedKey ? KeyboardKeys.ARROW_LEFT : KeyboardKeys.NUM_FOUR;
                case (Keys.Up):
                    return isExtendedKey ? KeyboardKeys.ARROW_UP : KeyboardKeys.NUM_EIGHT;
                case (Keys.Right):
                    return isExtendedKey ? KeyboardKeys.ARROW_RIGHT : KeyboardKeys.NUM_SIX;
                case (Keys.Down):
                    return isExtendedKey ? KeyboardKeys.ARROW_DOWN : KeyboardKeys.NUM_TWO;
                case (Keys.PrintScreen):
                    return KeyboardKeys.PRINT_SCREEN;
                case (Keys.Insert):
                    return isExtendedKey ? KeyboardKeys.INSERT : KeyboardKeys.NUM_ZERO;
                case (Keys.Delete):
                    return isExtendedKey ? KeyboardKeys.DELETE : KeyboardKeys.NUM_PERIOD;
                case (Keys.D0):
                    return KeyboardKeys.ZERO;
                case (Keys.D1):
                    return KeyboardKeys.ONE;
                case (Keys.D2):
                    return KeyboardKeys.TWO;
                case (Keys.D3):
                    return KeyboardKeys.THREE;
                case (Keys.D4):
                    return KeyboardKeys.FOUR;
                case (Keys.D5):
                    return KeyboardKeys.FIVE;
                case (Keys.D6):
                    return KeyboardKeys.SIX;
                case (Keys.D7):
                    return KeyboardKeys.SEVEN;
                case (Keys.D8):
                    return KeyboardKeys.EIGHT;
                case (Keys.D9):
                    return KeyboardKeys.NINE;
                case (Keys.A):
                    return KeyboardKeys.A;
                case (Keys.B):
                    return KeyboardKeys.B;
                case (Keys.C):
                    return KeyboardKeys.C;
                case (Keys.D):
                    return KeyboardKeys.D;
                case (Keys.E):
                    return KeyboardKeys.E;
                case (Keys.F):
                    return KeyboardKeys.F;
                case (Keys.G):
                    return KeyboardKeys.G;
                case (Keys.H):
                    return KeyboardKeys.H;
                case (Keys.I):
                    return KeyboardKeys.I;
                case (Keys.J):
                    return KeyboardKeys.J;
                case (Keys.K):
                    return KeyboardKeys.K;
                case (Keys.L):
                    return KeyboardKeys.L;
                case (Keys.M):
                    return KeyboardKeys.M;
                case (Keys.N):
                    return KeyboardKeys.N;
                case (Keys.O):
                    return KeyboardKeys.O;
                case (Keys.P):
                    return KeyboardKeys.P;
                case (Keys.Q):
                    return KeyboardKeys.Q;
                case (Keys.R):
                    return KeyboardKeys.R;
                case (Keys.S):
                    return KeyboardKeys.S;
                case (Keys.T):
                    return KeyboardKeys.T;
                case (Keys.U):
                    return KeyboardKeys.U;
                case (Keys.V):
                    return KeyboardKeys.V;
                case (Keys.W):
                    return KeyboardKeys.W;
                case (Keys.X):
                    return KeyboardKeys.X;
                case (Keys.Y):
                    return KeyboardKeys.Y;
                case (Keys.Z):
                    return KeyboardKeys.Z;
                case (Keys.LWin):
                    return KeyboardKeys.LEFT_WINDOWS;
                case (Keys.RWin):
                    return KeyboardKeys.RIGHT_WINDOWS;
                case (Keys.Apps):
                    return KeyboardKeys.APPLICATION_SELECT;
                case (Keys.NumPad0):
                    return KeyboardKeys.NUM_ZERO;
                case (Keys.NumPad1):
                    return KeyboardKeys.NUM_ONE;
                case (Keys.NumPad2):
                    return KeyboardKeys.NUM_TWO;
                case (Keys.NumPad3):
                    return KeyboardKeys.NUM_THREE;
                case (Keys.NumPad4):
                    return KeyboardKeys.NUM_FOUR;
                case (Keys.NumPad5):
                    return KeyboardKeys.NUM_FIVE;
                case (Keys.NumPad6):
                    return KeyboardKeys.NUM_SIX;
                case (Keys.NumPad7):
                    return KeyboardKeys.NUM_SEVEN;
                case (Keys.NumPad8):
                    return KeyboardKeys.NUM_EIGHT;
                case (Keys.NumPad9):
                    return KeyboardKeys.NUM_NINE;
                case (Keys.Multiply):
                    return KeyboardKeys.NUM_ASTERISK;
                case (Keys.Add):
                    return KeyboardKeys.NUM_PLUS;
                case (Keys.Subtract):
                    return KeyboardKeys.NUM_MINUS;
                case (Keys.Decimal):
                    return KeyboardKeys.NUM_PERIOD;
                case (Keys.Divide):
                    return KeyboardKeys.NUM_SLASH;
                case (Keys.F1):
                    return KeyboardKeys.F1;
                case (Keys.F2):
                    return KeyboardKeys.F2;
                case (Keys.F3):
                    return KeyboardKeys.F3;
                case (Keys.F4):
                    return KeyboardKeys.F4;
                case (Keys.F5):
                    return KeyboardKeys.F5;
                case (Keys.F6):
                    return KeyboardKeys.F6;
                case (Keys.F7):
                    return KeyboardKeys.F7;
                case (Keys.F8):
                    return KeyboardKeys.F8;
                case (Keys.F9):
                    return KeyboardKeys.F9;
                case (Keys.F10):
                    return KeyboardKeys.F10;
                case (Keys.F11):
                    return KeyboardKeys.F11;
                case (Keys.F12):
                    return KeyboardKeys.F12;
                case (Keys.NumLock):
                    return KeyboardKeys.NUM_LOCK;
                case (Keys.Scroll):
                    return KeyboardKeys.SCROLL_LOCK;
                case (Keys.VolumeMute):
                    return KeyboardKeys.VOLUME_MUTE;
                case (Keys.VolumeDown):
                    return KeyboardKeys.VOLUME_DOWN;
                case (Keys.VolumeUp):
                    return KeyboardKeys.VOLUME_UP;
                case (Keys.MediaNextTrack):
                    return KeyboardKeys.MEDIA_NEXT;
                case (Keys.MediaPreviousTrack):
                    return KeyboardKeys.MEDIA_PREVIOUS;
                case (Keys.MediaStop):
                    return KeyboardKeys.MEDIA_STOP;
                case (Keys.MediaPlayPause):
                    return KeyboardKeys.MEDIA_PLAY_PAUSE;
                case (Keys.OemSemicolon):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return KeyboardKeys.CLOSE_BRACKET;
                    else*/
                        return KeyboardKeys.SEMICOLON;
                case (Keys.Oemplus):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return KeyboardKeys.CLOSE_BRACKET;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return KeyboardKeys.MINUS;
                    else*/
                        return KeyboardKeys.EQUALS;
                case (Keys.Oemcomma):
                    return KeyboardKeys.COMMA;
                case (Keys.OemMinus):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return KeyboardKeys.FORWARD_SLASH;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return KeyboardKeys.FORWARD_SLASH;
                    else*/
                        return KeyboardKeys.MINUS;
                case (Keys.OemPeriod):
                    return KeyboardKeys.PERIOD;
                case (Keys.OemQuestion):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return KeyboardKeys.HASHTAG;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return KeyboardKeys.HASHTAG;
                    else*/
                        return KeyboardKeys.FORWARD_SLASH;
                case (Keys.ProcessKey):
                    return KeyboardKeys.JPN_HALFFULLWIDTH;
                case (Keys.Oemtilde):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.uk)
                        return KeyboardKeys.APOSTROPHE;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return KeyboardKeys.SEMICOLON;
                    else*/
                        return KeyboardKeys.TILDE;
                case (Keys.OemOpenBrackets):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return KeyboardKeys.MINUS;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return KeyboardKeys.EQUALS;
                    else*/
                        return KeyboardKeys.OPEN_BRACKET;
                case (Keys.OemPipe):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.uk)
                        return KeyboardKeys.BACKSLASH_UK;
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return KeyboardKeys.TILDE;
                    else*/
                        return KeyboardKeys.BACKSLASH;
                case (Keys.OemCloseBrackets):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return KeyboardKeys.EQUALS;
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return KeyboardKeys.OPEN_BRACKET;
                    else*/
                        return KeyboardKeys.CLOSE_BRACKET;
                case (Keys.OemQuotes):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.uk)
                        return KeyboardKeys.HASHTAG;
                    else*/
                        return KeyboardKeys.APOSTROPHE;
                case (Keys.OemBackslash):
                    return KeyboardKeys.BACKSLASH_UK;
                case (Keys.Oem8):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.uk)
                        return KeyboardKeys.TILDE;
                    else*/
                        return KeyboardKeys.OEM8;
                case (Keys.Play):
                    return KeyboardKeys.MEDIA_PLAY;
                default:
                    return KeyboardKeys.NONE;
            }
        }

        /// <summary>
        /// Converts Forms.Keys to Devices.DeviceKeys
        /// </summary>
        /// <param name="formsKeys">Array of Forms.Keys to be converted</param>
        /// <returns>The resulting Devices.DeviceKeys</returns>
        public static KeyboardKeys[] GetDeviceKeys(Keys[] formsKeys, bool extendedKeys = false, bool getBoth = false)
        {
            HashSet<KeyboardKeys> _returnKeys = new HashSet<KeyboardKeys>();

            for (int i = 0; i < formsKeys.Length; i++)
            {
                _returnKeys.Add(GetDeviceKey(formsKeys[i], 0, extendedKeys));
                if (getBoth)
                    _returnKeys.Add(GetDeviceKey(formsKeys[i], 0, !extendedKeys));
            }

            return _returnKeys.ToArray();
        }

        /// <summary>
        /// Converts CorsairLedId to Devices.DeviceKeys
        /// </summary>
        /// <param name="CorsairKey">The CorsairLedId to be converted</param>
        /// <returns>The resulting Devices.DeviceKeys</returns>
        public static KeyboardKeys ToKeyboardKeys(CorsairLedId CorsairKey)
        {
            switch (CorsairKey)
            {
                case (CorsairLedId.Logo):
                    return KeyboardKeys.LOGO;
                case (CorsairLedId.Brightness):
                    return KeyboardKeys.BRIGHTNESS_SWITCH;
                case (CorsairLedId.WinLock):
                    return KeyboardKeys.LOCK_SWITCH;

                case (CorsairLedId.Mute):
                    return KeyboardKeys.VOLUME_MUTE;
                case (CorsairLedId.VolumeUp):
                    return KeyboardKeys.VOLUME_UP;
                case (CorsairLedId.VolumeDown):
                    return KeyboardKeys.VOLUME_DOWN;
                case (CorsairLedId.Stop):
                    return KeyboardKeys.MEDIA_STOP;
                case (CorsairLedId.PlayPause):
                    return KeyboardKeys.MEDIA_PLAY_PAUSE;
                case (CorsairLedId.ScanPreviousTrack):
                    return KeyboardKeys.MEDIA_PREVIOUS;
                case (CorsairLedId.ScanNextTrack):
                    return KeyboardKeys.MEDIA_NEXT;

                case (CorsairLedId.Escape):
                    return KeyboardKeys.ESC;
                case (CorsairLedId.F1):
                    return KeyboardKeys.F1;
                case (CorsairLedId.F2):
                    return KeyboardKeys.F2;
                case (CorsairLedId.F3):
                    return KeyboardKeys.F3;
                case (CorsairLedId.F4):
                    return KeyboardKeys.F4;
                case (CorsairLedId.F5):
                    return KeyboardKeys.F5;
                case (CorsairLedId.F6):
                    return KeyboardKeys.F6;
                case (CorsairLedId.F7):
                    return KeyboardKeys.F7;
                case (CorsairLedId.F8):
                    return KeyboardKeys.F8;
                case (CorsairLedId.F9):
                    return KeyboardKeys.F9;
                case (CorsairLedId.F10):
                    return KeyboardKeys.F10;
                case (CorsairLedId.F11):
                    return KeyboardKeys.F11;
                case (CorsairLedId.F12):
                    return KeyboardKeys.F12;
                case (CorsairLedId.PrintScreen):
                    return KeyboardKeys.PRINT_SCREEN;
                case (CorsairLedId.ScrollLock):
                    return KeyboardKeys.SCROLL_LOCK;
                case (CorsairLedId.PauseBreak):
                    return KeyboardKeys.PAUSE_BREAK;
                case (CorsairLedId.GraveAccentAndTilde):
                    return KeyboardKeys.TILDE;
                case (CorsairLedId.D1):
                    return KeyboardKeys.ONE;
                case (CorsairLedId.D2):
                    return KeyboardKeys.TWO;
                case (CorsairLedId.D3):
                    return KeyboardKeys.THREE;
                case (CorsairLedId.D4):
                    return KeyboardKeys.FOUR;
                case (CorsairLedId.D5):
                    return KeyboardKeys.FIVE;
                case (CorsairLedId.D6):
                    return KeyboardKeys.SIX;
                case (CorsairLedId.D7):
                    return KeyboardKeys.SEVEN;
                case (CorsairLedId.D8):
                    return KeyboardKeys.EIGHT;
                case (CorsairLedId.D9):
                    return KeyboardKeys.NINE;
                case (CorsairLedId.D0):
                    return KeyboardKeys.ZERO;
                case (CorsairLedId.MinusAndUnderscore):
                    return KeyboardKeys.MINUS;
                case (CorsairLedId.EqualsAndPlus):
                    return KeyboardKeys.EQUALS;
                case (CorsairLedId.Backspace):
                    return KeyboardKeys.BACKSPACE;
                case (CorsairLedId.Insert):
                    return KeyboardKeys.INSERT;
                case (CorsairLedId.Home):
                    return KeyboardKeys.HOME;
                case (CorsairLedId.PageUp):
                    return KeyboardKeys.PAGE_UP;
                case (CorsairLedId.NumLock):
                    return KeyboardKeys.NUM_LOCK;
                case (CorsairLedId.KeypadSlash):
                    return KeyboardKeys.NUM_SLASH;
                case (CorsairLedId.KeypadAsterisk):
                    return KeyboardKeys.NUM_ASTERISK;
                case (CorsairLedId.KeypadMinus):
                    return KeyboardKeys.NUM_MINUS;
                case (CorsairLedId.Tab):
                    return KeyboardKeys.TAB;
                case (CorsairLedId.Q):
                    return KeyboardKeys.Q;
                case (CorsairLedId.W):
                    return KeyboardKeys.W;
                case (CorsairLedId.E):
                    return KeyboardKeys.E;
                case (CorsairLedId.R):
                    return KeyboardKeys.R;
                case (CorsairLedId.T):
                    return KeyboardKeys.T;
                case (CorsairLedId.Y):
                    return KeyboardKeys.Y;
                case (CorsairLedId.U):
                    return KeyboardKeys.U;
                case (CorsairLedId.I):
                    return KeyboardKeys.I;
                case (CorsairLedId.O):
                    return KeyboardKeys.O;
                case (CorsairLedId.P):
                    return KeyboardKeys.P;
                case (CorsairLedId.BracketLeft):
                    return KeyboardKeys.OPEN_BRACKET;
                case (CorsairLedId.BracketRight):
                    return KeyboardKeys.CLOSE_BRACKET;
                case (CorsairLedId.Backslash):
                    return KeyboardKeys.BACKSLASH;
                case (CorsairLedId.Delete):
                    return KeyboardKeys.DELETE;
                case (CorsairLedId.End):
                    return KeyboardKeys.END;
                case (CorsairLedId.PageDown):
                    return KeyboardKeys.PAGE_DOWN;
                case (CorsairLedId.Keypad7):
                    return KeyboardKeys.NUM_SEVEN;
                case (CorsairLedId.Keypad8):
                    return KeyboardKeys.NUM_EIGHT;
                case (CorsairLedId.Keypad9):
                    return KeyboardKeys.NUM_NINE;
                case (CorsairLedId.KeypadPlus):
                    return KeyboardKeys.NUM_PLUS;
                case (CorsairLedId.CapsLock):
                    return KeyboardKeys.CAPS_LOCK;
                case (CorsairLedId.A):
                    return KeyboardKeys.A;
                case (CorsairLedId.S):
                    return KeyboardKeys.S;
                case (CorsairLedId.D):
                    return KeyboardKeys.D;
                case (CorsairLedId.F):
                    return KeyboardKeys.F;
                case (CorsairLedId.G):
                    return KeyboardKeys.G;
                case (CorsairLedId.H):
                    return KeyboardKeys.H;
                case (CorsairLedId.J):
                    return KeyboardKeys.J;
                case (CorsairLedId.K):
                    return KeyboardKeys.K;
                case (CorsairLedId.L):
                    return KeyboardKeys.L;
                case (CorsairLedId.SemicolonAndColon):
                    return KeyboardKeys.SEMICOLON;
                case (CorsairLedId.ApostropheAndDoubleQuote):
                    return KeyboardKeys.APOSTROPHE;
                case (CorsairLedId.NonUsTilde):
                    return KeyboardKeys.HASHTAG;
                case (CorsairLedId.Enter):
                    return KeyboardKeys.ENTER;
                case (CorsairLedId.Keypad4):
                    return KeyboardKeys.NUM_FOUR;
                case (CorsairLedId.Keypad5):
                    return KeyboardKeys.NUM_FIVE;
                case (CorsairLedId.Keypad6):
                    return KeyboardKeys.NUM_SIX;
                case (CorsairLedId.LeftShift):
                    return KeyboardKeys.LEFT_SHIFT;
                case (CorsairLedId.NonUsBackslash):
                    return KeyboardKeys.BACKSLASH_UK;
                case (CorsairLedId.Z):
                    return KeyboardKeys.Z;
                case (CorsairLedId.X):
                    return KeyboardKeys.X;
                case (CorsairLedId.C):
                    return KeyboardKeys.C;
                case (CorsairLedId.V):
                    return KeyboardKeys.V;
                case (CorsairLedId.B):
                    return KeyboardKeys.B;
                case (CorsairLedId.N):
                    return KeyboardKeys.N;
                case (CorsairLedId.M):
                    return KeyboardKeys.M;
                case (CorsairLedId.CommaAndLessThan):
                    return KeyboardKeys.COMMA;
                case (CorsairLedId.PeriodAndBiggerThan):
                    return KeyboardKeys.PERIOD;
                case (CorsairLedId.SlashAndQuestionMark):
                    return KeyboardKeys.FORWARD_SLASH;
                case (CorsairLedId.RightShift):
                    return KeyboardKeys.RIGHT_SHIFT;
                case (CorsairLedId.UpArrow):
                    return KeyboardKeys.ARROW_UP;
                case (CorsairLedId.Keypad1):
                    return KeyboardKeys.NUM_ONE;
                case (CorsairLedId.Keypad2):
                    return KeyboardKeys.NUM_TWO;
                case (CorsairLedId.Keypad3):
                    return KeyboardKeys.NUM_THREE;
                case (CorsairLedId.KeypadEnter):
                    return KeyboardKeys.NUM_ENTER;
                case (CorsairLedId.LeftCtrl):
                    return KeyboardKeys.LEFT_CONTROL;
                case (CorsairLedId.LeftGui):
                    return KeyboardKeys.LEFT_WINDOWS;
                case (CorsairLedId.LeftAlt):
                    return KeyboardKeys.LEFT_ALT;
                case (CorsairLedId.Space):
                    return KeyboardKeys.SPACE;
                case (CorsairLedId.RightAlt):
                    return KeyboardKeys.RIGHT_ALT;
                case (CorsairLedId.RightGui):
                    return KeyboardKeys.RIGHT_WINDOWS;
                case (CorsairLedId.Application):
                    return KeyboardKeys.APPLICATION_SELECT;
                case (CorsairLedId.RightCtrl):
                    return KeyboardKeys.RIGHT_CONTROL;
                case (CorsairLedId.LeftArrow):
                    return KeyboardKeys.ARROW_LEFT;
                case (CorsairLedId.DownArrow):
                    return KeyboardKeys.ARROW_DOWN;
                case (CorsairLedId.RightArrow):
                    return KeyboardKeys.ARROW_RIGHT;
                case (CorsairLedId.Keypad0):
                    return KeyboardKeys.NUM_ZERO;
                case (CorsairLedId.KeypadPeriodAndDelete):
                    return KeyboardKeys.NUM_PERIOD;

                case (CorsairLedId.Fn):
                    return KeyboardKeys.FN_Key;

                case (CorsairLedId.G1):
                    return KeyboardKeys.G1;
                case (CorsairLedId.G2):
                    return KeyboardKeys.G2;
                case (CorsairLedId.G3):
                    return KeyboardKeys.G3;
                case (CorsairLedId.G4):
                    return KeyboardKeys.G4;
                case (CorsairLedId.G5):
                    return KeyboardKeys.G5;
                case (CorsairLedId.G6):
                    return KeyboardKeys.G6;
                case (CorsairLedId.G7):
                    return KeyboardKeys.G7;
                case (CorsairLedId.G8):
                    return KeyboardKeys.G8;
                case (CorsairLedId.G9):
                    return KeyboardKeys.G9;
                case (CorsairLedId.G10):
                    return KeyboardKeys.G10;
                case (CorsairLedId.G11):
                    return KeyboardKeys.G11;
                case (CorsairLedId.G12):
                    return KeyboardKeys.G12;
                case (CorsairLedId.G13):
                    return KeyboardKeys.G13;
                case (CorsairLedId.G14):
                    return KeyboardKeys.G14;
                case (CorsairLedId.G15):
                    return KeyboardKeys.G15;
                case (CorsairLedId.G16):
                    return KeyboardKeys.G16;
                case (CorsairLedId.G17):
                    return KeyboardKeys.G17;
                case (CorsairLedId.G18):
                    return KeyboardKeys.G18;

                default:
                    return KeyboardKeys.NONE;
            }
        }

        public static Keys GetStandardKey(Keys key)
        {
            switch (key)
            {
                case Keys.RControlKey:
                    return Keys.LControlKey;
                case Keys.RMenu:
                    return Keys.LMenu;
                case Keys.RShiftKey:
                    return Keys.LShiftKey;
                case Keys.RWin:
                    return Keys.LWin;
                default:
                    return key;
            }
        }

        /// <summary>
        ///     The set of valid MapTypes used in MapVirtualKey
        /// </summary>
        public enum MapVirtualKeyMapTypes : uint
        {
            /// <summary>
            ///     The uCode parameter is a virtual-key code and is translated into a scan code. If it is a virtual-key code that does
            ///     not distinguish between left- and right-hand keys, the left-hand scan code is returned. If there is no translation,
            ///     the function returns 0.
            /// </summary>
            MapvkVkToVsc = 0x00,

            /// <summary>
            ///     The uCode parameter is a scan code and is translated into a virtual-key code that does not distinguish between
            ///     left- and right-hand keys. If there is no translation, the function returns 0.
            /// </summary>
            MapvkVscToVk = 0x01,

            /// <summary>
            ///     The uCode parameter is a virtual-key code and is translated into an unshifted character value in the low order word
            ///     of the return value. Dead keys (diacritics) are indicated by setting the top bit of the return value. If there is
            ///     no translation, the function returns 0.
            /// </summary>
            MapvkVkToChar = 0x02,

            /// <summary>
            ///     The uCode parameter is a scan code and is translated into a virtual-key code that distinguishes between left- and
            ///     right-hand keys. If there is no translation, the function returns 0.
            /// </summary>
            MapvkVscToVkEx = 0x03,

            /// <summary>
            ///     The uCode parameter is a virtual-key code and is translated into a scan code. If it is a virtual-key code that does
            ///     not distinguish between left- and right-hand keys, the left-hand scan code is returned. If the scan code is an
            ///     extended scan code, the high byte of the uCode value can contain either 0xe0 or 0xe1 to specify the extended scan
            ///     code. If there is no translation, the function returns 0.
            /// </summary>
            MapvkVkToVscEx = 0x04
        }
    }
}