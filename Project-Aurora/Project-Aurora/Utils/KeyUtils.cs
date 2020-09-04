﻿using Aurora.Devices;
using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.RawInput;

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

        public static DeviceKeys GetDeviceKey(Keys forms_key, int scanCode = 0, bool isExtendedKey = false)
        {
            DeviceKeys key = getDeviceKey(forms_key, scanCode, isExtendedKey);
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
        public static Keys GetFormsKey(Devices.DeviceKeys deviceKeys)
        {
            switch (deviceKeys)
            {
                case (DeviceKeys.ESC):
                    return Keys.Escape;
                case (DeviceKeys.BACKSPACE):
                    return Keys.Back;
                case (DeviceKeys.TAB):
                    return Keys.Tab;
                case (DeviceKeys.NUM_ENTER):
                    return Keys.Enter;
                case (DeviceKeys.ENTER):
                    return Keys.Enter;
                case (DeviceKeys.LEFT_SHIFT):
                    return Keys.LShiftKey;
                case (DeviceKeys.LEFT_CONTROL):
                    return Keys.LControlKey;
                case (DeviceKeys.LEFT_ALT):
                    return Keys.LMenu;
                case (DeviceKeys.JPN_MUHENKAN):
                    return Keys.IMENonconvert;
                case (DeviceKeys.JPN_HENKAN):
                    return Keys.IMEConvert;
                case (DeviceKeys.JPN_HIRAGANA_KATAKANA):
                    return Keys.IMEModeChange;
                case (DeviceKeys.RIGHT_SHIFT):
                    return Keys.RShiftKey;
                case (DeviceKeys.RIGHT_CONTROL):
                    return Keys.RControlKey;
                case (DeviceKeys.RIGHT_ALT):
                    return Keys.RMenu;
                case (DeviceKeys.PAUSE_BREAK):
                    return Keys.Pause;
                case (DeviceKeys.CAPS_LOCK):
                    return Keys.CapsLock;
                case (DeviceKeys.SPACE):
                    return Keys.Space;
                case (DeviceKeys.PAGE_UP):
                    return Keys.PageUp;

                case (DeviceKeys.PAGE_DOWN):
                    return Keys.PageDown;

                case (DeviceKeys.END):
                    return Keys.End;
                case (DeviceKeys.HOME):
                    return Keys.Home;
                case (DeviceKeys.ARROW_LEFT):
                    return Keys.Left;
                case (DeviceKeys.ARROW_UP):
                    return Keys.Up;
                case (DeviceKeys.ARROW_RIGHT):
                    return Keys.Right;
                case (DeviceKeys.ARROW_DOWN):
                    return Keys.Down;
                case (DeviceKeys.PRINT_SCREEN):
                    return Keys.PrintScreen;
                case (DeviceKeys.INSERT):
                    return Keys.Insert;
                case (DeviceKeys.DELETE):
                    return Keys.Delete;
                case (DeviceKeys.ZERO):
                    return Keys.D0;
                case (DeviceKeys.ONE):
                    return Keys.D1;
                case (DeviceKeys.TWO):
                    return Keys.D2;
                case (DeviceKeys.THREE):
                    return Keys.D3;
                case (DeviceKeys.FOUR):
                    return Keys.D4;
                case (DeviceKeys.FIVE):
                    return Keys.D5;
                case (DeviceKeys.SIX):
                    return Keys.D6;
                case (DeviceKeys.SEVEN):
                    return Keys.D7;
                case (DeviceKeys.EIGHT):
                    return Keys.D8;
                case (DeviceKeys.NINE):
                    return Keys.D9;
                case (DeviceKeys.A):
                    return Keys.A;
                case (DeviceKeys.B):
                    return Keys.B;
                case (DeviceKeys.C):
                    return Keys.C;
                case (DeviceKeys.D):
                    return Keys.D;
                case (DeviceKeys.E):
                    return Keys.E;
                case (DeviceKeys.F):
                    return Keys.F;
                case (DeviceKeys.G):
                    return Keys.G;
                case (DeviceKeys.H):
                    return Keys.H;
                case (DeviceKeys.I):
                    return Keys.I;
                case (DeviceKeys.J):
                    return Keys.J;
                case (DeviceKeys.K):
                    return Keys.K;
                case (DeviceKeys.L):
                    return Keys.L;
                case (DeviceKeys.M):
                    return Keys.M;
                case (DeviceKeys.N):
                    return Keys.N;
                case (DeviceKeys.O):
                    return Keys.O;
                case (DeviceKeys.P):
                    return Keys.P;
                case (DeviceKeys.Q):
                    return Keys.Q;
                case (DeviceKeys.R):
                    return Keys.R;
                case (DeviceKeys.S):
                    return Keys.S;
                case (DeviceKeys.T):
                    return Keys.T;
                case (DeviceKeys.U):
                    return Keys.U;
                case (DeviceKeys.V):
                    return Keys.V;
                case (DeviceKeys.W):
                    return Keys.W;
                case (DeviceKeys.X):
                    return Keys.X;
                case (DeviceKeys.Y):
                    return Keys.Y;
                case (DeviceKeys.Z):
                    return Keys.Z;
                case (DeviceKeys.LEFT_WINDOWS):
                    return Keys.LWin;
                case (DeviceKeys.RIGHT_WINDOWS):
                    return Keys.RWin;
                case (DeviceKeys.APPLICATION_SELECT):
                    return Keys.Apps;
                case (DeviceKeys.NUM_ZERO):
                    return Keys.NumPad0;
                case (DeviceKeys.NUM_ONE):
                    return Keys.NumPad1;
                case (DeviceKeys.NUM_TWO):
                    return Keys.NumPad2;
                case (DeviceKeys.NUM_THREE):
                    return Keys.NumPad3;
                case (DeviceKeys.NUM_FOUR):
                    return Keys.NumPad4;
                case (DeviceKeys.NUM_FIVE):
                    return Keys.NumPad5;
                case (DeviceKeys.NUM_SIX):
                    return Keys.NumPad6;
                case (DeviceKeys.NUM_SEVEN):
                    return Keys.NumPad7;
                case (DeviceKeys.NUM_EIGHT):
                    return Keys.NumPad8;
                case (DeviceKeys.NUM_NINE):
                    return Keys.NumPad9;
                case (DeviceKeys.NUM_ASTERISK):
                    return Keys.Multiply;
                case (DeviceKeys.NUM_PLUS):
                    return Keys.Add;
                case (DeviceKeys.NUM_MINUS):
                    return Keys.Subtract;
                case (DeviceKeys.NUM_PERIOD):
                    return Keys.Decimal;
                case (DeviceKeys.NUM_SLASH):
                    return Keys.Divide;
                case (DeviceKeys.F1):
                    return Keys.F1;
                case (DeviceKeys.F2):
                    return Keys.F2;
                case (DeviceKeys.F3):
                    return Keys.F3;
                case (DeviceKeys.F4):
                    return Keys.F4;
                case (DeviceKeys.F5):
                    return Keys.F5;
                case (DeviceKeys.F6):
                    return Keys.F6;
                case (DeviceKeys.F7):
                    return Keys.F7;
                case (DeviceKeys.F8):
                    return Keys.F8;
                case (DeviceKeys.F9):
                    return Keys.F9;
                case (DeviceKeys.F10):
                    return Keys.F10;
                case (DeviceKeys.F11):
                    return Keys.F11;
                case (DeviceKeys.F12):
                    return Keys.F12;
                case (DeviceKeys.NUM_LOCK):
                    return Keys.NumLock;
                case (DeviceKeys.SCROLL_LOCK):
                    return Keys.Scroll;
                case (DeviceKeys.VOLUME_MUTE):
                    return Keys.VolumeMute;
                case (DeviceKeys.VOLUME_DOWN):
                    return Keys.VolumeDown;
                case (DeviceKeys.VOLUME_UP):
                    return Keys.VolumeUp;
                case (DeviceKeys.MEDIA_NEXT):
                    return Keys.MediaNextTrack;
                case (DeviceKeys.MEDIA_PREVIOUS):
                    return Keys.MediaPreviousTrack;
                case (DeviceKeys.MEDIA_STOP):
                    return Keys.MediaStop;
                case (DeviceKeys.MEDIA_PLAY_PAUSE):
                    return Keys.MediaPlayPause;
                case (DeviceKeys.SEMICOLON):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return DeviceKeys.CLOSE_BRACKET;
                    else*/
                    return Keys.OemSemicolon;
                case (DeviceKeys.EQUALS):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return DeviceKeys.CLOSE_BRACKET;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return DeviceKeys.MINUS;
                    else*/
                    return Keys.Oemplus;
                case (DeviceKeys.COMMA):
                    return Keys.Oemcomma;
                case (DeviceKeys.MINUS):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return DeviceKeys.FORWARD_SLASH;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return DeviceKeys.FORWARD_SLASH;
                    else*/
                    return Keys.OemMinus;
                case (DeviceKeys.PERIOD):
                    return Keys.OemPeriod;
                case (DeviceKeys.FORWARD_SLASH):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return DeviceKeys.HASHTAG;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return DeviceKeys.HASHTAG;
                    else*/
                    return Keys.OemQuestion;
                case (DeviceKeys.JPN_HALFFULLWIDTH):
                    return Keys.ProcessKey;
                case (DeviceKeys.TILDE):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.uk)
                        return DeviceKeys.APOSTROPHE;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return DeviceKeys.SEMICOLON;
                    else*/
                    return Keys.Oemtilde;
                case (DeviceKeys.OPEN_BRACKET):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return DeviceKeys.MINUS;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return DeviceKeys.EQUALS;
                    else*/
                    return Keys.OemOpenBrackets;
                case (DeviceKeys.BACKSLASH):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.uk)
                        return DeviceKeys.BACKSLASH_UK;
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return DeviceKeys.TILDE;
                    else*/
                    return Keys.OemPipe;
                case (DeviceKeys.CLOSE_BRACKET):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return DeviceKeys.EQUALS;
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return DeviceKeys.OPEN_BRACKET;
                    else*/
                    return Keys.OemCloseBrackets;
                case (DeviceKeys.APOSTROPHE):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.uk)
                        return DeviceKeys.HASHTAG;
                    else*/
                    return Keys.OemQuotes;
                case (DeviceKeys.BACKSLASH_UK):
                    return Keys.OemBackslash;
                case (DeviceKeys.OEM8):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.uk)
                        return DeviceKeys.TILDE;
                    else*/
                    return Keys.Oem8;
                case (DeviceKeys.MEDIA_PLAY):
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
        public static DeviceKeys GetDeviceKey(this KeyboardInputEventArgs eventArgs)
        {
            return GetDeviceKey(eventArgs.Key, eventArgs.MakeCode, eventArgs.ScanCodeFlags.HasFlag(ScanCodeFlags.E0));
        }

        /// <summary>
        /// Converts Forms.Keys to Devices.DeviceKeys
        /// </summary>
        /// <param name="forms_key">The Forms.Key to be converted</param>
        /// <returns>The resulting Devices.DeviceKeys</returns>
        private static DeviceKeys getDeviceKey(Keys forms_key, int scanCode = 0, bool isExtendedKey = false)
        {
            switch (forms_key)
            {
                case (Keys.Escape):
                    return DeviceKeys.ESC;
                case (Keys.Clear):
                    return DeviceKeys.NUM_FIVE;
                case (Keys.Back):
                    return DeviceKeys.BACKSPACE;
                case (Keys.Tab):
                    return DeviceKeys.TAB;
                case (Keys.Enter):
                    return isExtendedKey ? DeviceKeys.NUM_ENTER : DeviceKeys.ENTER;
                case (Keys.LShiftKey):
                    return DeviceKeys.LEFT_SHIFT;
                case (Keys.LControlKey):
                    if (scanCode > 0 && leftControlScanCode > 0 && scanCode != leftControlScanCode) // Alt Graph
                        return DeviceKeys.NONE;
                    return DeviceKeys.LEFT_CONTROL;
                case (Keys.LMenu):
                    return DeviceKeys.LEFT_ALT;
                case (Keys.IMENonconvert):
                    return DeviceKeys.JPN_MUHENKAN;
                case (Keys.IMEConvert):
                    return DeviceKeys.JPN_HENKAN;
                case (Keys.IMEModeChange):
                    return DeviceKeys.JPN_HIRAGANA_KATAKANA;
                case (Keys.RShiftKey):
                    return DeviceKeys.RIGHT_SHIFT;
                case (Keys.RControlKey):
                    return DeviceKeys.RIGHT_CONTROL;
                case (Keys.RMenu):
                    return DeviceKeys.RIGHT_ALT;
                case (Keys.Pause):
                    return DeviceKeys.PAUSE_BREAK;
                case (Keys.CapsLock):
                    return DeviceKeys.CAPS_LOCK;
                case (Keys.Space):
                    return DeviceKeys.SPACE;
                case (Keys.PageUp):
                    return isExtendedKey ? DeviceKeys.PAGE_UP : DeviceKeys.NUM_NINE;
                case (Keys.PageDown):
                    return isExtendedKey ? DeviceKeys.PAGE_DOWN : DeviceKeys.NUM_THREE;
                case (Keys.End):
                    return isExtendedKey ? DeviceKeys.END : DeviceKeys.NUM_ONE;
                case (Keys.Home):
                    return isExtendedKey ? DeviceKeys.HOME : DeviceKeys.NUM_SEVEN;
                case (Keys.Left):
                    return isExtendedKey ? DeviceKeys.ARROW_LEFT : DeviceKeys.NUM_FOUR;
                case (Keys.Up):
                    return isExtendedKey ? DeviceKeys.ARROW_UP : DeviceKeys.NUM_EIGHT;
                case (Keys.Right):
                    return isExtendedKey ? DeviceKeys.ARROW_RIGHT : DeviceKeys.NUM_SIX;
                case (Keys.Down):
                    return isExtendedKey ? DeviceKeys.ARROW_DOWN : DeviceKeys.NUM_TWO;
                case (Keys.PrintScreen):
                    return DeviceKeys.PRINT_SCREEN;
                case (Keys.Insert):
                    return isExtendedKey ? DeviceKeys.INSERT : DeviceKeys.NUM_ZERO;
                case (Keys.Delete):
                    return isExtendedKey ? DeviceKeys.DELETE : DeviceKeys.NUM_PERIOD;
                case (Keys.D0):
                    return DeviceKeys.ZERO;
                case (Keys.D1):
                    return DeviceKeys.ONE;
                case (Keys.D2):
                    return DeviceKeys.TWO;
                case (Keys.D3):
                    return DeviceKeys.THREE;
                case (Keys.D4):
                    return DeviceKeys.FOUR;
                case (Keys.D5):
                    return DeviceKeys.FIVE;
                case (Keys.D6):
                    return DeviceKeys.SIX;
                case (Keys.D7):
                    return DeviceKeys.SEVEN;
                case (Keys.D8):
                    return DeviceKeys.EIGHT;
                case (Keys.D9):
                    return DeviceKeys.NINE;
                case (Keys.A):
                    return DeviceKeys.A;
                case (Keys.B):
                    return DeviceKeys.B;
                case (Keys.C):
                    return DeviceKeys.C;
                case (Keys.D):
                    return DeviceKeys.D;
                case (Keys.E):
                    return DeviceKeys.E;
                case (Keys.F):
                    return DeviceKeys.F;
                case (Keys.G):
                    return DeviceKeys.G;
                case (Keys.H):
                    return DeviceKeys.H;
                case (Keys.I):
                    return DeviceKeys.I;
                case (Keys.J):
                    return DeviceKeys.J;
                case (Keys.K):
                    return DeviceKeys.K;
                case (Keys.L):
                    return DeviceKeys.L;
                case (Keys.M):
                    return DeviceKeys.M;
                case (Keys.N):
                    return DeviceKeys.N;
                case (Keys.O):
                    return DeviceKeys.O;
                case (Keys.P):
                    return DeviceKeys.P;
                case (Keys.Q):
                    return DeviceKeys.Q;
                case (Keys.R):
                    return DeviceKeys.R;
                case (Keys.S):
                    return DeviceKeys.S;
                case (Keys.T):
                    return DeviceKeys.T;
                case (Keys.U):
                    return DeviceKeys.U;
                case (Keys.V):
                    return DeviceKeys.V;
                case (Keys.W):
                    return DeviceKeys.W;
                case (Keys.X):
                    return DeviceKeys.X;
                case (Keys.Y):
                    return DeviceKeys.Y;
                case (Keys.Z):
                    return DeviceKeys.Z;
                case (Keys.LWin):
                    return DeviceKeys.LEFT_WINDOWS;
                case (Keys.RWin):
                    return DeviceKeys.RIGHT_WINDOWS;
                case (Keys.Apps):
                    return DeviceKeys.APPLICATION_SELECT;
                case (Keys.NumPad0):
                    return DeviceKeys.NUM_ZERO;
                case (Keys.NumPad1):
                    return DeviceKeys.NUM_ONE;
                case (Keys.NumPad2):
                    return DeviceKeys.NUM_TWO;
                case (Keys.NumPad3):
                    return DeviceKeys.NUM_THREE;
                case (Keys.NumPad4):
                    return DeviceKeys.NUM_FOUR;
                case (Keys.NumPad5):
                    return DeviceKeys.NUM_FIVE;
                case (Keys.NumPad6):
                    return DeviceKeys.NUM_SIX;
                case (Keys.NumPad7):
                    return DeviceKeys.NUM_SEVEN;
                case (Keys.NumPad8):
                    return DeviceKeys.NUM_EIGHT;
                case (Keys.NumPad9):
                    return DeviceKeys.NUM_NINE;
                case (Keys.Multiply):
                    return DeviceKeys.NUM_ASTERISK;
                case (Keys.Add):
                    return DeviceKeys.NUM_PLUS;
                case (Keys.Subtract):
                    return DeviceKeys.NUM_MINUS;
                case (Keys.Decimal):
                    return DeviceKeys.NUM_PERIOD;
                case (Keys.Divide):
                    return DeviceKeys.NUM_SLASH;
                case (Keys.F1):
                    return DeviceKeys.F1;
                case (Keys.F2):
                    return DeviceKeys.F2;
                case (Keys.F3):
                    return DeviceKeys.F3;
                case (Keys.F4):
                    return DeviceKeys.F4;
                case (Keys.F5):
                    return DeviceKeys.F5;
                case (Keys.F6):
                    return DeviceKeys.F6;
                case (Keys.F7):
                    return DeviceKeys.F7;
                case (Keys.F8):
                    return DeviceKeys.F8;
                case (Keys.F9):
                    return DeviceKeys.F9;
                case (Keys.F10):
                    return DeviceKeys.F10;
                case (Keys.F11):
                    return DeviceKeys.F11;
                case (Keys.F12):
                    return DeviceKeys.F12;
                case (Keys.NumLock):
                    return DeviceKeys.NUM_LOCK;
                case (Keys.Scroll):
                    return DeviceKeys.SCROLL_LOCK;
                case (Keys.VolumeMute):
                    return DeviceKeys.VOLUME_MUTE;
                case (Keys.VolumeDown):
                    return DeviceKeys.VOLUME_DOWN;
                case (Keys.VolumeUp):
                    return DeviceKeys.VOLUME_UP;
                case (Keys.MediaNextTrack):
                    return DeviceKeys.MEDIA_NEXT;
                case (Keys.MediaPreviousTrack):
                    return DeviceKeys.MEDIA_PREVIOUS;
                case (Keys.MediaStop):
                    return DeviceKeys.MEDIA_STOP;
                case (Keys.MediaPlayPause):
                    return DeviceKeys.MEDIA_PLAY_PAUSE;
                case (Keys.OemSemicolon):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return DeviceKeys.CLOSE_BRACKET;
                    else*/
                        return DeviceKeys.SEMICOLON;
                case (Keys.Oemplus):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return DeviceKeys.CLOSE_BRACKET;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return DeviceKeys.MINUS;
                    else*/
                        return DeviceKeys.EQUALS;
                case (Keys.Oemcomma):
                    return DeviceKeys.COMMA;
                case (Keys.OemMinus):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return DeviceKeys.FORWARD_SLASH;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return DeviceKeys.FORWARD_SLASH;
                    else*/
                        return DeviceKeys.MINUS;
                case (Keys.OemPeriod):
                    return DeviceKeys.PERIOD;
                case (Keys.OemQuestion):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return DeviceKeys.HASHTAG;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return DeviceKeys.HASHTAG;
                    else*/
                        return DeviceKeys.FORWARD_SLASH;
                case (Keys.ProcessKey):
                    return DeviceKeys.JPN_HALFFULLWIDTH;
                case (Keys.Oemtilde):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.uk)
                        return DeviceKeys.APOSTROPHE;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return DeviceKeys.SEMICOLON;
                    else*/
                        return DeviceKeys.TILDE;
                case (Keys.OemOpenBrackets):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return DeviceKeys.MINUS;
                    else if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return DeviceKeys.EQUALS;
                    else*/
                        return DeviceKeys.OPEN_BRACKET;
                case (Keys.OemPipe):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.uk)
                        return DeviceKeys.BACKSLASH_UK;
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return DeviceKeys.TILDE;
                    else*/
                        return DeviceKeys.BACKSLASH;
                case (Keys.OemCloseBrackets):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.de)
                        return DeviceKeys.EQUALS;
                    if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.nordic)
                        return DeviceKeys.OPEN_BRACKET;
                    else*/
                        return DeviceKeys.CLOSE_BRACKET;
                case (Keys.OemQuotes):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.uk)
                        return DeviceKeys.HASHTAG;
                    else*/
                        return DeviceKeys.APOSTROPHE;
                case (Keys.OemBackslash):
                    return DeviceKeys.BACKSLASH_UK;
                case (Keys.Oem8):
                    /*if (Global.kbLayout.Loaded_Localization == Settings.PreferredKeyboardLocalization.uk)
                        return DeviceKeys.TILDE;
                    else*/
                        return DeviceKeys.OEM8;
                case (Keys.Play):
                    return DeviceKeys.MEDIA_PLAY;
                default:
                    return DeviceKeys.NONE;
            }
        }

        /// <summary>
        /// Converts Forms.Keys to Devices.DeviceKeys
        /// </summary>
        /// <param name="formsKeys">Array of Forms.Keys to be converted</param>
        /// <returns>The resulting Devices.DeviceKeys</returns>
        public static DeviceKeys[] GetDeviceKeys(Keys[] formsKeys, bool extendedKeys = false, bool getBoth = false)
        {
            HashSet<DeviceKeys> _returnKeys = new HashSet<DeviceKeys>();

            for (int i = 0; i < formsKeys.Length; i++)
            {
                _returnKeys.Add(GetDeviceKey(formsKeys[i], 0, extendedKeys));
                if (getBoth)
                    _returnKeys.Add(GetDeviceKey(formsKeys[i], 0, !extendedKeys));
            }

            return _returnKeys.ToArray();
        }

        public static DeviceKeys[] GetDeviceAllKeys()
        {
            return (DeviceKeys[])Enum.GetValues(typeof(DeviceKeys));
        }

        public static Keys GetStandardKey(Keys key)
        {
            switch (key)
            {
                case Keys.RControlKey:
                    return Keys.LControlKey;
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