﻿using Aurora.Devices;
using CUE.NET.Devices.Generic.Enums;
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
        private static extern uint MapVirtualKey(uint uCode, MapVirtualKeyMapTypes uMapType);

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
            //if (Global.writeLogFile) Global.logger.LogLine(key.ToString() + ":" + ((int)key).ToString());
            if (Global.kbLayout.LayoutKeyConversion.ContainsKey(key))
                return Global.kbLayout.LayoutKeyConversion[key];

            return key;
        }

        /// <summary>
        /// Correcting RawInput data according to an article https://blog.molecular-matters.com/2011/09/05/properly-handling-keyboard-input/
        /// </summary>
        public static void CorrectRawInputData(KeyboardInputEventArgs e)
        {
            // e0 and e1 are escape sequences used for certain special keys, such as PRINT and PAUSE/BREAK.
            // see http://www.win.tue.nl/~aeb/linux/kbd/scancodes-1.html
            bool isE0 = e.ScanCodeFlags.HasFlag(ScanCodeFlags.E0);
            bool isE1 = e.ScanCodeFlags.HasFlag(ScanCodeFlags.E1);

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

        /// <summary>
        /// Converts CorsairLedId to Devices.DeviceKeys
        /// </summary>
        /// <param name="CorsairKey">The CorsairLedId to be converted</param>
        /// <returns>The resulting Devices.DeviceKeys</returns>
        public static DeviceKeys ToDeviceKeys(CorsairLedId CorsairKey)
        {
            switch (CorsairKey)
            {
                case (CorsairLedId.Logo):
                    return DeviceKeys.LOGO;
                case (CorsairLedId.Brightness):
                    return DeviceKeys.BRIGHTNESS_SWITCH;
                case (CorsairLedId.WinLock):
                    return DeviceKeys.LOCK_SWITCH;

                case (CorsairLedId.Mute):
                    return DeviceKeys.VOLUME_MUTE;
                case (CorsairLedId.VolumeUp):
                    return DeviceKeys.VOLUME_UP;
                case (CorsairLedId.VolumeDown):
                    return DeviceKeys.VOLUME_DOWN;
                case (CorsairLedId.Stop):
                    return DeviceKeys.MEDIA_STOP;
                case (CorsairLedId.PlayPause):
                    return DeviceKeys.MEDIA_PLAY_PAUSE;
                case (CorsairLedId.ScanPreviousTrack):
                    return DeviceKeys.MEDIA_PREVIOUS;
                case (CorsairLedId.ScanNextTrack):
                    return DeviceKeys.MEDIA_NEXT;

                case (CorsairLedId.Escape):
                    return DeviceKeys.ESC;
                case (CorsairLedId.F1):
                    return DeviceKeys.F1;
                case (CorsairLedId.F2):
                    return DeviceKeys.F2;
                case (CorsairLedId.F3):
                    return DeviceKeys.F3;
                case (CorsairLedId.F4):
                    return DeviceKeys.F4;
                case (CorsairLedId.F5):
                    return DeviceKeys.F5;
                case (CorsairLedId.F6):
                    return DeviceKeys.F6;
                case (CorsairLedId.F7):
                    return DeviceKeys.F7;
                case (CorsairLedId.F8):
                    return DeviceKeys.F8;
                case (CorsairLedId.F9):
                    return DeviceKeys.F9;
                case (CorsairLedId.F10):
                    return DeviceKeys.F10;
                case (CorsairLedId.F11):
                    return DeviceKeys.F11;
                case (CorsairLedId.F12):
                    return DeviceKeys.F12;
                case (CorsairLedId.PrintScreen):
                    return DeviceKeys.PRINT_SCREEN;
                case (CorsairLedId.ScrollLock):
                    return DeviceKeys.SCROLL_LOCK;
                case (CorsairLedId.PauseBreak):
                    return DeviceKeys.PAUSE_BREAK;
                case (CorsairLedId.GraveAccentAndTilde):
                    return DeviceKeys.TILDE;
                case (CorsairLedId.D1):
                    return DeviceKeys.ONE;
                case (CorsairLedId.D2):
                    return DeviceKeys.TWO;
                case (CorsairLedId.D3):
                    return DeviceKeys.THREE;
                case (CorsairLedId.D4):
                    return DeviceKeys.FOUR;
                case (CorsairLedId.D5):
                    return DeviceKeys.FIVE;
                case (CorsairLedId.D6):
                    return DeviceKeys.SIX;
                case (CorsairLedId.D7):
                    return DeviceKeys.SEVEN;
                case (CorsairLedId.D8):
                    return DeviceKeys.EIGHT;
                case (CorsairLedId.D9):
                    return DeviceKeys.NINE;
                case (CorsairLedId.D0):
                    return DeviceKeys.ZERO;
                case (CorsairLedId.MinusAndUnderscore):
                    return DeviceKeys.MINUS;
                case (CorsairLedId.EqualsAndPlus):
                    return DeviceKeys.EQUALS;
                case (CorsairLedId.Backspace):
                    return DeviceKeys.BACKSPACE;
                case (CorsairLedId.Insert):
                    return DeviceKeys.INSERT;
                case (CorsairLedId.Home):
                    return DeviceKeys.HOME;
                case (CorsairLedId.PageUp):
                    return DeviceKeys.PAGE_UP;
                case (CorsairLedId.NumLock):
                    return DeviceKeys.NUM_LOCK;
                case (CorsairLedId.KeypadSlash):
                    return DeviceKeys.NUM_SLASH;
                case (CorsairLedId.KeypadAsterisk):
                    return DeviceKeys.NUM_ASTERISK;
                case (CorsairLedId.KeypadMinus):
                    return DeviceKeys.NUM_MINUS;
                case (CorsairLedId.Tab):
                    return DeviceKeys.TAB;
                case (CorsairLedId.Q):
                    return DeviceKeys.Q;
                case (CorsairLedId.W):
                    return DeviceKeys.W;
                case (CorsairLedId.E):
                    return DeviceKeys.E;
                case (CorsairLedId.R):
                    return DeviceKeys.R;
                case (CorsairLedId.T):
                    return DeviceKeys.T;
                case (CorsairLedId.Y):
                    return DeviceKeys.Y;
                case (CorsairLedId.U):
                    return DeviceKeys.U;
                case (CorsairLedId.I):
                    return DeviceKeys.I;
                case (CorsairLedId.O):
                    return DeviceKeys.O;
                case (CorsairLedId.P):
                    return DeviceKeys.P;
                case (CorsairLedId.BracketLeft):
                    return DeviceKeys.OPEN_BRACKET;
                case (CorsairLedId.BracketRight):
                    return DeviceKeys.CLOSE_BRACKET;
                case (CorsairLedId.Backslash):
                    return DeviceKeys.BACKSLASH;
                case (CorsairLedId.Delete):
                    return DeviceKeys.DELETE;
                case (CorsairLedId.End):
                    return DeviceKeys.END;
                case (CorsairLedId.PageDown):
                    return DeviceKeys.PAGE_DOWN;
                case (CorsairLedId.Keypad7):
                    return DeviceKeys.NUM_SEVEN;
                case (CorsairLedId.Keypad8):
                    return DeviceKeys.NUM_EIGHT;
                case (CorsairLedId.Keypad9):
                    return DeviceKeys.NUM_NINE;
                case (CorsairLedId.KeypadPlus):
                    return DeviceKeys.NUM_PLUS;
                case (CorsairLedId.CapsLock):
                    return DeviceKeys.CAPS_LOCK;
                case (CorsairLedId.A):
                    return DeviceKeys.A;
                case (CorsairLedId.S):
                    return DeviceKeys.S;
                case (CorsairLedId.D):
                    return DeviceKeys.D;
                case (CorsairLedId.F):
                    return DeviceKeys.F;
                case (CorsairLedId.G):
                    return DeviceKeys.G;
                case (CorsairLedId.H):
                    return DeviceKeys.H;
                case (CorsairLedId.J):
                    return DeviceKeys.J;
                case (CorsairLedId.K):
                    return DeviceKeys.K;
                case (CorsairLedId.L):
                    return DeviceKeys.L;
                case (CorsairLedId.SemicolonAndColon):
                    return DeviceKeys.SEMICOLON;
                case (CorsairLedId.ApostropheAndDoubleQuote):
                    return DeviceKeys.APOSTROPHE;
                case (CorsairLedId.NonUsTilde):
                    return DeviceKeys.HASHTAG;
                case (CorsairLedId.Enter):
                    return DeviceKeys.ENTER;
                case (CorsairLedId.Keypad4):
                    return DeviceKeys.NUM_FOUR;
                case (CorsairLedId.Keypad5):
                    return DeviceKeys.NUM_FIVE;
                case (CorsairLedId.Keypad6):
                    return DeviceKeys.NUM_SIX;
                case (CorsairLedId.LeftShift):
                    return DeviceKeys.LEFT_SHIFT;
                case (CorsairLedId.NonUsBackslash):
                    return DeviceKeys.BACKSLASH_UK;
                case (CorsairLedId.Z):
                    return DeviceKeys.Z;
                case (CorsairLedId.X):
                    return DeviceKeys.X;
                case (CorsairLedId.C):
                    return DeviceKeys.C;
                case (CorsairLedId.V):
                    return DeviceKeys.V;
                case (CorsairLedId.B):
                    return DeviceKeys.B;
                case (CorsairLedId.N):
                    return DeviceKeys.N;
                case (CorsairLedId.M):
                    return DeviceKeys.M;
                case (CorsairLedId.CommaAndLessThan):
                    return DeviceKeys.COMMA;
                case (CorsairLedId.PeriodAndBiggerThan):
                    return DeviceKeys.PERIOD;
                case (CorsairLedId.SlashAndQuestionMark):
                    return DeviceKeys.FORWARD_SLASH;
                case (CorsairLedId.RightShift):
                    return DeviceKeys.RIGHT_SHIFT;
                case (CorsairLedId.UpArrow):
                    return DeviceKeys.ARROW_UP;
                case (CorsairLedId.Keypad1):
                    return DeviceKeys.NUM_ONE;
                case (CorsairLedId.Keypad2):
                    return DeviceKeys.NUM_TWO;
                case (CorsairLedId.Keypad3):
                    return DeviceKeys.NUM_THREE;
                case (CorsairLedId.KeypadEnter):
                    return DeviceKeys.NUM_ENTER;
                case (CorsairLedId.LeftCtrl):
                    return DeviceKeys.LEFT_CONTROL;
                case (CorsairLedId.LeftGui):
                    return DeviceKeys.LEFT_WINDOWS;
                case (CorsairLedId.LeftAlt):
                    return DeviceKeys.LEFT_ALT;
                case (CorsairLedId.Space):
                    return DeviceKeys.SPACE;
                case (CorsairLedId.RightAlt):
                    return DeviceKeys.RIGHT_ALT;
                case (CorsairLedId.RightGui):
                    return DeviceKeys.RIGHT_WINDOWS;
                case (CorsairLedId.Application):
                    return DeviceKeys.APPLICATION_SELECT;
                case (CorsairLedId.RightCtrl):
                    return DeviceKeys.RIGHT_CONTROL;
                case (CorsairLedId.LeftArrow):
                    return DeviceKeys.ARROW_LEFT;
                case (CorsairLedId.DownArrow):
                    return DeviceKeys.ARROW_DOWN;
                case (CorsairLedId.RightArrow):
                    return DeviceKeys.ARROW_RIGHT;
                case (CorsairLedId.Keypad0):
                    return DeviceKeys.NUM_ZERO;
                case (CorsairLedId.KeypadPeriodAndDelete):
                    return DeviceKeys.NUM_PERIOD;

                case (CorsairLedId.Fn):
                    return DeviceKeys.FN_Key;

                case (CorsairLedId.G1):
                    return DeviceKeys.G1;
                case (CorsairLedId.G2):
                    return DeviceKeys.G2;
                case (CorsairLedId.G3):
                    return DeviceKeys.G3;
                case (CorsairLedId.G4):
                    return DeviceKeys.G4;
                case (CorsairLedId.G5):
                    return DeviceKeys.G5;
                case (CorsairLedId.G6):
                    return DeviceKeys.G6;
                case (CorsairLedId.G7):
                    return DeviceKeys.G7;
                case (CorsairLedId.G8):
                    return DeviceKeys.G8;
                case (CorsairLedId.G9):
                    return DeviceKeys.G9;
                case (CorsairLedId.G10):
                    return DeviceKeys.G10;
                case (CorsairLedId.G11):
                    return DeviceKeys.G11;
                case (CorsairLedId.G12):
                    return DeviceKeys.G12;
                case (CorsairLedId.G13):
                    return DeviceKeys.G13;
                case (CorsairLedId.G14):
                    return DeviceKeys.G14;
                case (CorsairLedId.G15):
                    return DeviceKeys.G15;
                case (CorsairLedId.G16):
                    return DeviceKeys.G16;
                case (CorsairLedId.G17):
                    return DeviceKeys.G17;
                case (CorsairLedId.G18):
                    return DeviceKeys.G18;

                default:
                    return DeviceKeys.NONE;
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
        private enum MapVirtualKeyMapTypes : uint
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