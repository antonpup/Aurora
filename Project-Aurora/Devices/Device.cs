using Aurora.EffectsEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Aurora.Devices
{
    public enum DeviceKeys
    {
        [Description("Peripheral Device")]
        Peripheral = 0,
        [Description("Escape")]
        ESC = 1,
        [Description("F1")]
        F1 = 2,
        [Description("F2")]
        F2 = 3,
        [Description("F3")]
        F3 = 4,
        [Description("F4")]
        F4 = 5,
        [Description("F5")]
        F5 = 6,
        [Description("F6")]
        F6 = 7,
        [Description("F7")]
        F7 = 8,
        [Description("F8")]
        F8 = 9,
        [Description("F9")]
        F9 = 10,
        [Description("F10")]
        F10 = 11,
        [Description("F11")]
        F11 = 12,
        [Description("F12")]
        F12 = 13,
        [Description("Print Screen")]
        PRINT_SCREEN = 14,
        [Description("Scroll Lock")]
        SCROLL_LOCK = 15,
        [Description("Pause")]
        PAUSE_BREAK = 16,

        [Description("Half/Full width")]
        JPN_HALFFULLWIDTH = 152,
        [Description("OEM 5")]
        OEM5 = 156,
        [Description("Tilde")]
        TILDE = 17,
        [Description("1")]
        ONE = 18,
        [Description("2")]
        TWO = 19,
        [Description("3")]
        THREE = 20,
        [Description("4")]
        FOUR = 21,
        [Description("5")]
        FIVE = 22,
        [Description("6")]
        SIX = 23,
        [Description("7")]
        SEVEN = 24,
        [Description("8")]
        EIGHT = 25,
        [Description("9")]
        NINE = 26,
        [Description("0")]
        ZERO = 27,
        [Description("-")]
        MINUS = 28,
        [Description("=")]
        EQUALS = 29,
        [Description("Backspace")]
        BACKSPACE = 30,
        [Description("Insert")]
        INSERT = 31,
        [Description("Home")]
        HOME = 32,
        [Description("Page Up")]
        PAGE_UP = 33,
        [Description("Numpad Lock")]
        NUM_LOCK = 34,
        [Description("Numpad /")]
        NUM_SLASH = 35,
        [Description("Numpad *")]
        NUM_ASTERISK = 36,
        [Description("Numpad -")]
        NUM_MINUS = 37,

        [Description("Tab")]
        TAB = 38,
        [Description("Q")]
        Q = 39,
        [Description("W")]
        W = 40,
        [Description("E")]
        E = 41,
        [Description("R")]
        R = 42,
        [Description("T")]
        T = 43,
        [Description("Y")]
        Y = 44,
        [Description("U")]
        U = 45,
        [Description("I")]
        I = 46,
        [Description("O")]
        O = 47,
        [Description("P")]
        P = 48,
        [Description("{")]
        OPEN_BRACKET = 49,
        [Description("}")]
        CLOSE_BRACKET = 50,
        [Description("\\")]
        BACKSLASH = 51,
        [Description("Delete")]
        DELETE = 52,
        [Description("End")]
        END = 53,
        [Description("Page Down")]
        PAGE_DOWN = 54,
        [Description("Numpad 7")]
        NUM_SEVEN = 55,
        [Description("Numpad 8")]
        NUM_EIGHT = 56,
        [Description("Numpad 9")]
        NUM_NINE = 57,
        [Description("Numpad +")]
        NUM_PLUS = 58,

        [Description("Caps Lock")]
        CAPS_LOCK = 59,
        [Description("A")]
        A = 60,
        [Description("S")]
        S = 61,
        [Description("D")]
        D = 62,
        [Description("F")]
        F = 63,
        [Description("G")]
        G = 64,
        [Description("H")]
        H = 65,
        [Description("J")]
        J = 66,
        [Description("K")]
        K = 67,
        [Description("L")]
        L = 68,
        [Description("Ö")]
        DEU_O = 157,
        [Description("Semicolon")]
        SEMICOLON = 69,
        [Description("Apostrophe")]
        APOSTROPHE = 70,
        [Description("#")]
        HASHTAG = 71,
        [Description("Enter")]
        ENTER = 72,
        [Description("Numpad 4")]
        NUM_FOUR = 73,
        [Description("Numpad 5")]
        NUM_FIVE = 74,
        [Description("Numpad 6")]
        NUM_SIX = 75,

        [Description("Left Shift")]
        LEFT_SHIFT = 76,
        [Description("Non-US Backslash")]
        BACKSLASH_UK = 77,
        [Description("Z")]
        Z = 78,
        [Description("X")]
        X = 79,
        [Description("C")]
        C = 80,
        [Description("V")]
        V = 81,
        [Description("B")]
        B = 82,
        [Description("N")]
        N = 83,
        [Description("M")]
        M = 84,
        [Description("Comma")]
        COMMA = 85,
        [Description("Period")]
        PERIOD = 86,
        [Description("Forward Slash")]
        FORWARD_SLASH = 87,
        [Description("OEM 8")]
        OEM8 = 158,
        [Description("OEM 102")]
        OEM102 = 159,
        [Description("Right Shift")]
        RIGHT_SHIFT = 88,
        [Description("Arrow Up")]
        ARROW_UP = 89,
        [Description("Numpad 1")]
        NUM_ONE = 90,
        [Description("Numpad 2")]
        NUM_TWO = 91,
        [Description("Numpad 3")]
        NUM_THREE = 92,
        [Description("Numpad Enter")]
        NUM_ENTER = 93,

        [Description("Left Control")]
        LEFT_CONTROL = 94,
        [Description("Left Windows Key")]
        LEFT_WINDOWS = 95,
        [Description("Left Alt")]
        LEFT_ALT = 96,
        [Description("Non-conversion")]
        JPN_MUHENKAN = 153,
        [Description("Spacebar")]
        SPACE = 97,
        [Description("Conversion")]
        JPN_HENKAN = 154,
        [Description("Hiragana/Katakana")]
        JPN_HIRAGANA_KATAKANA = 155,
        [Description("Right Alt")]
        RIGHT_ALT = 98,
        [Description("Right Windows Key")]
        RIGHT_WINDOWS = 99,
        [Description("Application Select Key")]
        APPLICATION_SELECT = 100,
        [Description("Right Control")]
        RIGHT_CONTROL = 101,
        [Description("Arrow Left")]
        ARROW_LEFT = 102,
        [Description("Arrow Down")]
        ARROW_DOWN = 103,
        [Description("Arrow Right")]
        ARROW_RIGHT = 104,
        [Description("Numpad 0")]
        NUM_ZERO = 105,
        [Description("Numpad Period")]
        NUM_PERIOD = 106,

        [Description("FN Key")]
        FN_Key = 107,

        [Description("G1")]
        G1 = 108,
        [Description("G2")]
        G2 = 109,
        [Description("G3")]
        G3 = 110,
        [Description("G4")]
        G4 = 111,
        [Description("G5")]
        G5 = 112,
        [Description("G6")]
        G6 = 113,
        [Description("G7")]
        G7 = 114,
        [Description("G8")]
        G8 = 115,
        [Description("G9")]
        G9 = 116,
        [Description("G10")]
        G10 = 117,
        [Description("G11")]
        G11 = 118,
        [Description("G12")]
        G12 = 119,
        [Description("G13")]
        G13 = 120,
        [Description("G14")]
        G14 = 121,
        [Description("G15")]
        G15 = 122,
        [Description("G16")]
        G16 = 123,
        [Description("G17")]
        G17 = 124,
        [Description("G18")]
        G18 = 125,
        [Description("G19")]
        G19 = 126,
        [Description("G20")]
        G20 = 127,

        [Description("Brand Logo")]
        LOGO = 128,
        [Description("Brand Logo #2")]
        LOGO2 = 129,
        [Description("Brand Logo #3")]
        LOGO3 = 130,
        [Description("Brightness Switch")]
        BRIGHTNESS_SWITCH = 131,
        [Description("Lock Switch")]
        LOCK_SWITCH = 132,

        [Description("Media Play/Pause")]
        MEDIA_PLAY_PAUSE = 133,
        [Description("Media Play")]
        MEDIA_PLAY = 134,
        [Description("Media Pause")]
        MEDIA_PAUSE = 135,
        [Description("Media Stop")]
        MEDIA_STOP = 136,
        [Description("Media Previous")]
        MEDIA_PREVIOUS = 137,
        [Description("Media Next")]
        MEDIA_NEXT = 138,

        [Description("Volume Mute")]
        VOLUME_MUTE = 139,
        [Description("Volume Down")]
        VOLUME_DOWN = 140,
        [Description("Volume Up")]
        VOLUME_UP = 141,

        [Description("Additional Light 1")]
        ADDITIONALLIGHT1 = 142,
        [Description("Additional Light 2")]
        ADDITIONALLIGHT2 = 143,
        [Description("Additional Light 3")]
        ADDITIONALLIGHT3 = 144,
        [Description("Additional Light 4")]
        ADDITIONALLIGHT4 = 145,
        [Description("Additional Light 5")]
        ADDITIONALLIGHT5 = 146,
        [Description("Additional Light 6")]
        ADDITIONALLIGHT6 = 147,
        [Description("Additional Light 7")]
        ADDITIONALLIGHT7 = 148,
        [Description("Additional Light 8")]
        ADDITIONALLIGHT8 = 149,
        [Description("Additional Light 9")]
        ADDITIONALLIGHT9 = 150,
        [Description("Additional Light 10")]
        ADDITIONALLIGHT10 = 151,

        [Description("None")]
        NONE = -1,
    };

    public interface Device
    {
        String GetDeviceName();

        String GetDeviceDetails();

        bool Initialize();

        void Shutdown();

        void Reset();

        bool Reconnect();

        bool IsInitialized();

        bool IsConnected();

        bool IsKeyboardConnected();

        bool IsPeripheralConnected();

        bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, bool forced = false);
    }
}
