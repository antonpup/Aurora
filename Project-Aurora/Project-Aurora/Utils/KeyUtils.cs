using Aurora.Devices;
using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Linearstar.Windows.RawInput.Native;

namespace Aurora.Utils;

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

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKeyEx(uint uCode, MapVirtualKeyMapTypes uMapType, IntPtr dwhkl);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetKeyboardLayout(uint idThread);

    [DllImport("user32", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern int GetKeyNameTextW(uint lParam, StringBuilder lpString, int nSize);

    public static string? GetAutomaticText(DeviceKeys associatedKey)
    {
        if (!Global.kbLayout.LoadedLocalization.IsAutomaticGeneration()) return null;

        var sb = new StringBuilder(2);
        var scanCode = GetScanCode(associatedKey);
        if (scanCode == -1)
            return null;

        GetKeyNameTextW((uint)scanCode << 16, sb, 2);
        return sb.ToString();
    }

    public static DeviceKeys GetDeviceKey(Keys formsKey, bool isExtendedKey = false)
    {
        var key = getDeviceKey(formsKey, isExtendedKey);
        return Global.kbLayout.LayoutKeyConversion.ContainsKey(key) ? Global.kbLayout.LayoutKeyConversion[key] : key;
    }

    private static readonly Dictionary<uint, DeviceKeys> ScanCodeConversion = new()
    {
        {40, DeviceKeys.TILDE},
        {12, DeviceKeys.MINUS},
        {13, DeviceKeys.EQUALS},
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
        {41, DeviceKeys.OEM8},
    };

    private static Dictionary<DeviceKeys, uint>? _keyToScanCode;

    public static int GetScanCode(DeviceKeys key)
    {
        _keyToScanCode ??= ScanCodeConversion.ToList().ToDictionary((kvp) => kvp.Value, kvp => kvp.Key);

        if (_keyToScanCode.TryGetValue(key, out var scan))
            return (int)scan;
        return -1;
    }

    /// <summary>
    /// Correcting RawInput data according to an article https://blog.molecular-matters.com/2011/09/05/properly-handling-keyboard-input/
    /// </summary>
    public static Keys CorrectRawInputData(int virtualKey, int keyboardDataScanCode, RawKeyboardFlags flags)
    {
        var key = (Keys)virtualKey;

        // e0 and e1 are escape sequences used for certain special keys, such as PRINT and PAUSE/BREAK.
        // see http://www.win.tue.nl/~aeb/linux/kbd/scancodes-1.html
        var isE0 = flags.HasFlag(RawKeyboardFlags.KeyE0);
        var isE1 = flags.HasFlag(RawKeyboardFlags.KeyE1);
        if (Global.kbLayout.LoadedLocalization.IsAutomaticGeneration() && key is >= Keys.A and <= Keys.Z or >= Keys.Oem1 and <= Keys.Oem102)
        {
            var thread = User32.GetWindowThreadProcessId(User32.GetForegroundWindow(), out _);
            var layout = GetKeyboardLayout(thread);
            var scanCodeLocale = MapVirtualKeyEx((uint)key, MapVirtualKeyMapTypes.MapvkVkToVsc, layout);
            if (scanCodeLocale == 0)
                Global.logger.Warning("Unable to convert key: {Key} to scan_code_locale. layout: {Layout}", key, layout);
            else
            {
                var k = (Keys)MapVirtualKey(scanCodeLocale, MapVirtualKeyMapTypes.MapvkVscToVk);
                if (k != Keys.None)
                    key = k;
                else
                    Global.logger.Warning("Unable to convert scan_code_locale: {ScanCodeLocale} to Keys. Key: {Key}, layout: {Layout}", scanCodeLocale, key, layout);
            }
        }

        if (!isE1)
            return key switch
            {
                Keys.ShiftKey =>
                    // correct left-hand / right-hand SHIFT
                    (Keys)MapVirtualKey((uint)keyboardDataScanCode, MapVirtualKeyMapTypes.MapvkVscToVkEx),
                Keys.ControlKey => isE0 ? Keys.RControlKey : Keys.LControlKey,
                Keys.Menu => isE0 ? Keys.RMenu : Keys.LMenu,
                _ => key
            };

        return key switch
        {
            Keys.ShiftKey =>
                // correct left-hand / right-hand SHIFT
                (Keys)MapVirtualKey((uint)keyboardDataScanCode, MapVirtualKeyMapTypes.MapvkVscToVkEx),
            Keys.ControlKey => isE0 ? Keys.RControlKey : Keys.LControlKey,
            Keys.Menu => isE0 ? Keys.RMenu : Keys.LMenu,
            _ => key
        };
    }

    /// <summary>
    /// Converts Devices.DeviceKeys to Forms.Keys
    /// </summary>
    /// <param name="deviceKeys">The Forms.Key to be converted</param>
    /// <returns>The resulting Devices.DeviceKeys</returns>
    public static Keys GetFormsKey(DeviceKeys deviceKeys)
    {
        return deviceKeys switch
        {
            DeviceKeys.ESC => Keys.Escape,
            DeviceKeys.BACKSPACE => Keys.Back,
            DeviceKeys.TAB => Keys.Tab,
            DeviceKeys.NUM_ENTER => Keys.Enter,
            DeviceKeys.ENTER => Keys.Enter,
            DeviceKeys.LEFT_SHIFT => Keys.LShiftKey,
            DeviceKeys.LEFT_CONTROL => Keys.LControlKey,
            DeviceKeys.LEFT_ALT => Keys.LMenu,
            DeviceKeys.JPN_MUHENKAN => Keys.IMENonconvert,
            DeviceKeys.JPN_HENKAN => Keys.IMEConvert,
            DeviceKeys.JPN_HIRAGANA_KATAKANA => Keys.IMEModeChange,
            DeviceKeys.RIGHT_SHIFT => Keys.RShiftKey,
            DeviceKeys.RIGHT_CONTROL => Keys.RControlKey,
            DeviceKeys.RIGHT_ALT => Keys.RMenu,
            DeviceKeys.PAUSE_BREAK => Keys.Pause,
            DeviceKeys.CAPS_LOCK => Keys.CapsLock,
            DeviceKeys.SPACE => Keys.Space,
            DeviceKeys.PAGE_UP => Keys.PageUp,
            DeviceKeys.PAGE_DOWN => Keys.PageDown,
            DeviceKeys.END => Keys.End,
            DeviceKeys.HOME => Keys.Home,
            DeviceKeys.ARROW_LEFT => Keys.Left,
            DeviceKeys.ARROW_UP => Keys.Up,
            DeviceKeys.ARROW_RIGHT => Keys.Right,
            DeviceKeys.ARROW_DOWN => Keys.Down,
            DeviceKeys.PRINT_SCREEN => Keys.PrintScreen,
            DeviceKeys.INSERT => Keys.Insert,
            DeviceKeys.DELETE => Keys.Delete,
            DeviceKeys.ZERO => Keys.D0,
            DeviceKeys.ONE => Keys.D1,
            DeviceKeys.TWO => Keys.D2,
            DeviceKeys.THREE => Keys.D3,
            DeviceKeys.FOUR => Keys.D4,
            DeviceKeys.FIVE => Keys.D5,
            DeviceKeys.SIX => Keys.D6,
            DeviceKeys.SEVEN => Keys.D7,
            DeviceKeys.EIGHT => Keys.D8,
            DeviceKeys.NINE => Keys.D9,
            DeviceKeys.A => Keys.A,
            DeviceKeys.B => Keys.B,
            DeviceKeys.C => Keys.C,
            DeviceKeys.D => Keys.D,
            DeviceKeys.E => Keys.E,
            DeviceKeys.F => Keys.F,
            DeviceKeys.G => Keys.G,
            DeviceKeys.H => Keys.H,
            DeviceKeys.I => Keys.I,
            DeviceKeys.J => Keys.J,
            DeviceKeys.K => Keys.K,
            DeviceKeys.L => Keys.L,
            DeviceKeys.M => Keys.M,
            DeviceKeys.N => Keys.N,
            DeviceKeys.O => Keys.O,
            DeviceKeys.P => Keys.P,
            DeviceKeys.Q => Keys.Q,
            DeviceKeys.R => Keys.R,
            DeviceKeys.S => Keys.S,
            DeviceKeys.T => Keys.T,
            DeviceKeys.U => Keys.U,
            DeviceKeys.V => Keys.V,
            DeviceKeys.W => Keys.W,
            DeviceKeys.X => Keys.X,
            DeviceKeys.Y => Keys.Y,
            DeviceKeys.Z => Keys.Z,
            DeviceKeys.LEFT_WINDOWS => Keys.LWin,
            DeviceKeys.RIGHT_WINDOWS => Keys.RWin,
            DeviceKeys.APPLICATION_SELECT => Keys.Apps,
            DeviceKeys.NUM_ZERO => Keys.NumPad0,
            DeviceKeys.NUM_ONE => Keys.NumPad1,
            DeviceKeys.NUM_TWO => Keys.NumPad2,
            DeviceKeys.NUM_THREE => Keys.NumPad3,
            DeviceKeys.NUM_FOUR => Keys.NumPad4,
            DeviceKeys.NUM_FIVE => Keys.NumPad5,
            DeviceKeys.NUM_SIX => Keys.NumPad6,
            DeviceKeys.NUM_SEVEN => Keys.NumPad7,
            DeviceKeys.NUM_EIGHT => Keys.NumPad8,
            DeviceKeys.NUM_NINE => Keys.NumPad9,
            DeviceKeys.NUM_ASTERISK => Keys.Multiply,
            DeviceKeys.NUM_PLUS => Keys.Add,
            DeviceKeys.NUM_MINUS => Keys.Subtract,
            DeviceKeys.NUM_PERIOD => Keys.Decimal,
            DeviceKeys.NUM_SLASH => Keys.Divide,
            DeviceKeys.F1 => Keys.F1,
            DeviceKeys.F2 => Keys.F2,
            DeviceKeys.F3 => Keys.F3,
            DeviceKeys.F4 => Keys.F4,
            DeviceKeys.F5 => Keys.F5,
            DeviceKeys.F6 => Keys.F6,
            DeviceKeys.F7 => Keys.F7,
            DeviceKeys.F8 => Keys.F8,
            DeviceKeys.F9 => Keys.F9,
            DeviceKeys.F10 => Keys.F10,
            DeviceKeys.F11 => Keys.F11,
            DeviceKeys.F12 => Keys.F12,
            DeviceKeys.NUM_LOCK => Keys.NumLock,
            DeviceKeys.SCROLL_LOCK => Keys.Scroll,
            DeviceKeys.VOLUME_MUTE => Keys.VolumeMute,
            DeviceKeys.VOLUME_DOWN => Keys.VolumeDown,
            DeviceKeys.VOLUME_UP => Keys.VolumeUp,
            DeviceKeys.MEDIA_NEXT => Keys.MediaNextTrack,
            DeviceKeys.MEDIA_PREVIOUS => Keys.MediaPreviousTrack,
            DeviceKeys.MEDIA_STOP => Keys.MediaStop,
            DeviceKeys.MEDIA_PLAY_PAUSE => Keys.MediaPlayPause,
            DeviceKeys.SEMICOLON => Keys.OemSemicolon,
            DeviceKeys.EQUALS => Keys.Oemplus,
            DeviceKeys.COMMA => Keys.Oemcomma,
            DeviceKeys.MINUS => Keys.OemMinus,
            DeviceKeys.PERIOD => Keys.OemPeriod,
            DeviceKeys.FORWARD_SLASH => Keys.OemQuestion,
            DeviceKeys.JPN_HALFFULLWIDTH => Keys.ProcessKey,
            DeviceKeys.TILDE => Keys.Oemtilde,
            DeviceKeys.OPEN_BRACKET => Keys.OemOpenBrackets,
            DeviceKeys.BACKSLASH => Keys.OemPipe,
            DeviceKeys.CLOSE_BRACKET => Keys.OemCloseBrackets,
            DeviceKeys.APOSTROPHE => Keys.OemQuotes,
            DeviceKeys.BACKSLASH_UK => Keys.OemBackslash,
            DeviceKeys.OEM8 => Keys.Oem8,
            DeviceKeys.MEDIA_PLAY => Keys.Play,
            _ => Keys.None
        };
    }

    public static DeviceKeys getDeviceKey(Keys formsKey, bool isExtendedKey = false)
    {
        return formsKey switch
        {
            Keys.Escape => DeviceKeys.ESC,
            Keys.Clear => DeviceKeys.NUM_FIVE,
            Keys.Back => DeviceKeys.BACKSPACE,
            Keys.Tab => DeviceKeys.TAB,
            Keys.Enter => isExtendedKey ? DeviceKeys.NUM_ENTER : DeviceKeys.ENTER,
            Keys.LShiftKey => DeviceKeys.LEFT_SHIFT,
            Keys.LControlKey => DeviceKeys.LEFT_CONTROL,
            Keys.LMenu => DeviceKeys.LEFT_ALT,
            Keys.IMENonconvert => DeviceKeys.JPN_MUHENKAN,
            Keys.IMEConvert => DeviceKeys.JPN_HENKAN,
            Keys.IMEModeChange => DeviceKeys.JPN_HIRAGANA_KATAKANA,
            Keys.RShiftKey => DeviceKeys.RIGHT_SHIFT,
            Keys.RControlKey => DeviceKeys.RIGHT_CONTROL,
            Keys.RMenu => DeviceKeys.RIGHT_ALT,
            Keys.Pause => DeviceKeys.PAUSE_BREAK,
            Keys.CapsLock => DeviceKeys.CAPS_LOCK,
            Keys.Space => DeviceKeys.SPACE,
            Keys.PageUp => isExtendedKey ? DeviceKeys.PAGE_UP : DeviceKeys.NUM_NINE,
            Keys.PageDown => isExtendedKey ? DeviceKeys.PAGE_DOWN : DeviceKeys.NUM_THREE,
            Keys.End => isExtendedKey ? DeviceKeys.END : DeviceKeys.NUM_ONE,
            Keys.Home => isExtendedKey ? DeviceKeys.HOME : DeviceKeys.NUM_SEVEN,
            Keys.Left => isExtendedKey ? DeviceKeys.ARROW_LEFT : DeviceKeys.NUM_FOUR,
            Keys.Up => isExtendedKey ? DeviceKeys.ARROW_UP : DeviceKeys.NUM_EIGHT,
            Keys.Right => isExtendedKey ? DeviceKeys.ARROW_RIGHT : DeviceKeys.NUM_SIX,
            Keys.Down => isExtendedKey ? DeviceKeys.ARROW_DOWN : DeviceKeys.NUM_TWO,
            Keys.Insert => isExtendedKey ? DeviceKeys.INSERT : DeviceKeys.NUM_ZERO,
            Keys.Delete => isExtendedKey ? DeviceKeys.DELETE : DeviceKeys.NUM_PERIOD,
            Keys.PrintScreen => DeviceKeys.PRINT_SCREEN,
            Keys.D0 => DeviceKeys.ZERO,
            Keys.D1 => DeviceKeys.ONE,
            Keys.D2 => DeviceKeys.TWO,
            Keys.D3 => DeviceKeys.THREE,
            Keys.D4 => DeviceKeys.FOUR,
            Keys.D5 => DeviceKeys.FIVE,
            Keys.D6 => DeviceKeys.SIX,
            Keys.D7 => DeviceKeys.SEVEN,
            Keys.D8 => DeviceKeys.EIGHT,
            Keys.D9 => DeviceKeys.NINE,
            Keys.A => DeviceKeys.A,
            Keys.B => DeviceKeys.B,
            Keys.C => DeviceKeys.C,
            Keys.D => DeviceKeys.D,
            Keys.E => DeviceKeys.E,
            Keys.F => DeviceKeys.F,
            Keys.G => DeviceKeys.G,
            Keys.H => DeviceKeys.H,
            Keys.I => DeviceKeys.I,
            Keys.J => DeviceKeys.J,
            Keys.K => DeviceKeys.K,
            Keys.L => DeviceKeys.L,
            Keys.M => DeviceKeys.M,
            Keys.N => DeviceKeys.N,
            Keys.O => DeviceKeys.O,
            Keys.P => DeviceKeys.P,
            Keys.Q => DeviceKeys.Q,
            Keys.R => DeviceKeys.R,
            Keys.S => DeviceKeys.S,
            Keys.T => DeviceKeys.T,
            Keys.U => DeviceKeys.U,
            Keys.V => DeviceKeys.V,
            Keys.W => DeviceKeys.W,
            Keys.X => DeviceKeys.X,
            Keys.Y => DeviceKeys.Y,
            Keys.Z => DeviceKeys.Z,
            Keys.LWin => DeviceKeys.LEFT_WINDOWS,
            Keys.RWin => DeviceKeys.RIGHT_WINDOWS,
            Keys.Apps => DeviceKeys.APPLICATION_SELECT,
            Keys.NumPad0 => DeviceKeys.NUM_ZERO,
            Keys.NumPad1 => DeviceKeys.NUM_ONE,
            Keys.NumPad2 => DeviceKeys.NUM_TWO,
            Keys.NumPad3 => DeviceKeys.NUM_THREE,
            Keys.NumPad4 => DeviceKeys.NUM_FOUR,
            Keys.NumPad5 => DeviceKeys.NUM_FIVE,
            Keys.NumPad6 => DeviceKeys.NUM_SIX,
            Keys.NumPad7 => DeviceKeys.NUM_SEVEN,
            Keys.NumPad8 => DeviceKeys.NUM_EIGHT,
            Keys.NumPad9 => DeviceKeys.NUM_NINE,
            Keys.Multiply => DeviceKeys.NUM_ASTERISK,
            Keys.Add => DeviceKeys.NUM_PLUS,
            Keys.Subtract => DeviceKeys.NUM_MINUS,
            Keys.Decimal => DeviceKeys.NUM_PERIOD,
            Keys.Divide => DeviceKeys.NUM_SLASH,
            Keys.F1 => DeviceKeys.F1,
            Keys.F2 => DeviceKeys.F2,
            Keys.F3 => DeviceKeys.F3,
            Keys.F4 => DeviceKeys.F4,
            Keys.F5 => DeviceKeys.F5,
            Keys.F6 => DeviceKeys.F6,
            Keys.F7 => DeviceKeys.F7,
            Keys.F8 => DeviceKeys.F8,
            Keys.F9 => DeviceKeys.F9,
            Keys.F10 => DeviceKeys.F10,
            Keys.F11 => DeviceKeys.F11,
            Keys.F12 => DeviceKeys.F12,
            Keys.NumLock => DeviceKeys.NUM_LOCK,
            Keys.Scroll => DeviceKeys.SCROLL_LOCK,
            Keys.VolumeMute => DeviceKeys.VOLUME_MUTE,
            Keys.VolumeDown => DeviceKeys.VOLUME_DOWN,
            Keys.VolumeUp => DeviceKeys.VOLUME_UP,
            Keys.MediaNextTrack => DeviceKeys.MEDIA_NEXT,
            Keys.MediaPreviousTrack => DeviceKeys.MEDIA_PREVIOUS,
            Keys.MediaStop => DeviceKeys.MEDIA_STOP,
            Keys.MediaPlayPause => DeviceKeys.MEDIA_PLAY_PAUSE,
            Keys.OemSemicolon => DeviceKeys.SEMICOLON,
            Keys.Oemplus => DeviceKeys.EQUALS,
            Keys.Oemcomma => DeviceKeys.COMMA,
            Keys.OemMinus => DeviceKeys.MINUS,
            Keys.OemPeriod => DeviceKeys.PERIOD,
            Keys.OemQuestion => DeviceKeys.FORWARD_SLASH,
            Keys.ProcessKey => DeviceKeys.JPN_HALFFULLWIDTH,
            Keys.Oemtilde => DeviceKeys.TILDE,
            Keys.OemOpenBrackets => DeviceKeys.OPEN_BRACKET,
            Keys.OemPipe => DeviceKeys.BACKSLASH,
            Keys.OemCloseBrackets => DeviceKeys.CLOSE_BRACKET,
            Keys.OemQuotes => DeviceKeys.APOSTROPHE,
            Keys.OemBackslash => DeviceKeys.BACKSLASH_UK,
            Keys.Oem8 => DeviceKeys.OEM8,
            Keys.Play => DeviceKeys.MEDIA_PLAY,
            _ => DeviceKeys.NONE
        };
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
            _returnKeys.Add(GetDeviceKey(formsKeys[i], extendedKeys));
            if (getBoth)
                _returnKeys.Add(GetDeviceKey(formsKeys[i], !extendedKeys));
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