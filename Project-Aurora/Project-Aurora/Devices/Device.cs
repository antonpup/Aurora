using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Devices
{
    /// <summary>
    /// Enum definition, representing everysingle supported device key
    /// </summary>
    public enum DeviceKeys
    {
        /// <summary>
        /// Peripheral Device
        /// <note type="note">Setting this key will make entire peripheral device one color</note>
        /// </summary>
        [Description("All Peripheral Devices")]
        Peripheral = 0,

        /// <summary>
        /// Escape key
        /// </summary>
        [Description("Escape")]
        ESC = 1,

        /// <summary>
        /// F1 key
        /// </summary>
        [Description("F1")]
        F1 = 2,

        /// <summary>
        /// F2 key
        /// </summary>
        [Description("F2")]
        F2 = 3,

        /// <summary>
        /// F3 key
        /// </summary>
        [Description("F3")]
        F3 = 4,

        /// <summary>
        /// F4 key
        /// </summary>
        [Description("F4")]
        F4 = 5,

        /// <summary>
        /// F5 key
        /// </summary>
        [Description("F5")]
        F5 = 6,

        /// <summary>
        /// F6 key
        /// </summary>
        [Description("F6")]
        F6 = 7,

        /// <summary>
        /// F7 key
        /// </summary>
        [Description("F7")]
        F7 = 8,

        /// <summary>
        /// F8 key
        /// </summary>
        [Description("F8")]
        F8 = 9,

        /// <summary>
        /// F9 key
        /// </summary>
        [Description("F9")]
        F9 = 10,

        /// <summary>
        /// F10 key
        /// </summary>
        [Description("F10")]
        F10 = 11,

        /// <summary>
        /// F11 key
        /// </summary>
        [Description("F11")]
        F11 = 12,

        /// <summary>
        /// F12 key
        /// </summary>
        [Description("F12")]
        F12 = 13,

        /// <summary>
        /// Print Screen key
        /// </summary>
        [Description("Print Screen")]
        PRINT_SCREEN = 14,

        /// <summary>
        /// Scroll Lock key
        /// </summary>
        [Description("Scroll Lock")]
        SCROLL_LOCK = 15,

        /// <summary>
        /// Pause/Break key
        /// </summary>
        [Description("Pause")]
        PAUSE_BREAK = 16,


        /// <summary>
        /// Half/Full width (Japanese layout) key
        /// </summary>
        [Description("Half/Full width")]
        JPN_HALFFULLWIDTH = 152,

        /// <summary>
        /// OEM 5 key
        /// </summary>
        [Description("OEM 5")]
        OEM5 = 156,

        /// <summary>
        /// Tilde key
        /// </summary>
        [Description("Tilde")]
        TILDE = 17,
        
        /// <summary>
        /// One key
        /// </summary>
        [Description("1")]
        ONE = 18,

        /// <summary>
        /// Two key
        /// </summary>
        [Description("2")]
        TWO = 19,

        /// <summary>
        /// Three key
        /// </summary>
        [Description("3")]
        THREE = 20,

        /// <summary>
        /// Four key
        /// </summary>
        [Description("4")]
        FOUR = 21,

        /// <summary>
        /// Five key
        /// </summary>
        [Description("5")]
        FIVE = 22,

        /// <summary>
        /// Six key
        /// </summary>
        [Description("6")]
        SIX = 23,

        /// <summary>
        /// Seven key
        /// </summary>
        [Description("7")]
        SEVEN = 24,

        /// <summary>
        /// Eight key
        /// </summary>
        [Description("8")]
        EIGHT = 25,

        /// <summary>
        /// Nine key
        /// </summary>
        [Description("9")]
        NINE = 26,

        /// <summary>
        /// Zero key
        /// </summary>
        [Description("0")]
        ZERO = 27,

        /// <summary>
        /// Minus key
        /// </summary>
        [Description("-")]
        MINUS = 28,

        /// <summary>
        /// Equals key
        /// </summary>
        [Description("=")]
        EQUALS = 29,

        /// <summary>
        /// OEM 6 key
        /// </summary>
        [Description("OEM 6")]
        OEM6 = 169,

        /// <summary>
        /// Backspace key
        /// </summary>
        [Description("Backspace")]
        BACKSPACE = 30,

        /// <summary>
        /// Insert key
        /// </summary>
        [Description("Insert")]
        INSERT = 31,

        /// <summary>
        /// Home key
        /// </summary>
        [Description("Home")]
        HOME = 32,

        /// <summary>
        /// Page up key
        /// </summary>
        [Description("Page Up")]
        PAGE_UP = 33,

        /// <summary>
        /// Numpad Lock key
        /// </summary>
        [Description("Numpad Lock")]
        NUM_LOCK = 34,

        /// <summary>
        /// Numpad divide key
        /// </summary>
        [Description("Numpad /")]
        NUM_SLASH = 35,

        /// <summary>
        /// Numpad multiply key
        /// </summary>
        [Description("Numpad *")]
        NUM_ASTERISK = 36,

        /// <summary>
        /// Numpad minus key
        /// </summary>
        [Description("Numpad -")]
        NUM_MINUS = 37,


        /// <summary>
        /// Tab key
        /// </summary>
        [Description("Tab")]
        TAB = 38,

        /// <summary>
        /// Q key
        /// </summary>
        [Description("Q")]
        Q = 39,

        /// <summary>
        /// W key
        /// </summary>
        [Description("W")]
        W = 40,

        /// <summary>
        /// E key
        /// </summary>
        [Description("E")]
        E = 41,

        /// <summary>
        /// R key
        /// </summary>
        [Description("R")]
        R = 42,

        /// <summary>
        /// T key
        /// </summary>
        [Description("T")]
        T = 43,

        /// <summary>
        /// Y key
        /// </summary>
        [Description("Y")]
        Y = 44,

        /// <summary>
        /// U key
        /// </summary>
        [Description("U")]
        U = 45,

        /// <summary>
        /// I key
        /// </summary>
        [Description("I")]
        I = 46,

        /// <summary>
        /// O key
        /// </summary>
        [Description("O")]
        O = 47,

        /// <summary>
        /// P key
        /// </summary>
        [Description("P")]
        P = 48,

        /// <summary>
        /// OEM 1 key
        /// </summary>
        [Description("OEM 1")]
        OEM1 = 170,

        /// <summary>
        /// Open Bracket key
        /// </summary>
        [Description("{")]
        OPEN_BRACKET = 49,

        /// <summary>
        /// OEM Plus key
        /// </summary>
        [Description("OEM Plus")]
        OEMPlus = 171,

        /// <summary>
        /// Close Bracket key
        /// </summary>
        [Description("}")]
        CLOSE_BRACKET = 50,

        /// <summary>
        /// Backslash key
        /// </summary>
        [Description("\\")]
        BACKSLASH = 51,

        /// <summary>
        /// Delete key
        /// </summary>
        [Description("Delete")]
        DELETE = 52,

        /// <summary>
        /// End key
        /// </summary>
        [Description("End")]
        END = 53,

        /// <summary>
        /// Page down key
        /// </summary>
        [Description("Page Down")]
        PAGE_DOWN = 54,

        /// <summary>
        /// Numpad seven key
        /// </summary>
        [Description("Numpad 7")]
        NUM_SEVEN = 55,

        /// <summary>
        /// Numpad eight key
        /// </summary>
        [Description("Numpad 8")]
        NUM_EIGHT = 56,

        /// <summary>
        /// Numpad nine key
        /// </summary>
        [Description("Numpad 9")]
        NUM_NINE = 57,

        /// <summary>
        /// Numpad add key
        /// </summary>
        [Description("Numpad +")]
        NUM_PLUS = 58,


        /// <summary>
        /// Caps Lock key
        /// </summary>
        [Description("Caps Lock")]
        CAPS_LOCK = 59,

        /// <summary>
        /// A key
        /// </summary>
        [Description("A")]
        A = 60,

        /// <summary>
        /// S key
        /// </summary>
        [Description("S")]
        S = 61,

        /// <summary>
        /// D key
        /// </summary>
        [Description("D")]
        D = 62,

        /// <summary>
        /// F key
        /// </summary>
        [Description("F")]
        F = 63,

        /// <summary>
        /// G key
        /// </summary>
        [Description("G")]
        G = 64,

        /// <summary>
        /// H key
        /// </summary>
        [Description("H")]
        H = 65,

        /// <summary>
        /// J key
        /// </summary>
        [Description("J")]
        J = 66,

        /// <summary>
        /// K key
        /// </summary>
        [Description("K")]
        K = 67,

        /// <summary>
        /// L key
        /// </summary>
        [Description("L")]
        L = 68,

        /// <summary>
        /// OEM Tilde key
        /// </summary>
        [Description("OEM Tilde")]
        OEMTilde = 157,

        /// <summary>
        /// Semicolon key
        /// </summary>
        [Description("Semicolon")]
        SEMICOLON = 69,

        /// <summary>
        /// Apostrophe key
        /// </summary>
        [Description("Apostrophe")]
        APOSTROPHE = 70,

        /// <summary>
        /// Hashtag key
        /// </summary>
        [Description("#")]
        HASHTAG = 71,

        /// <summary>
        /// Enter key
        /// </summary>
        [Description("Enter")]
        ENTER = 72,

        /// <summary>
        /// Numpad four key
        /// </summary>
        [Description("Numpad 4")]
        NUM_FOUR = 73,

        /// <summary>
        /// Numpad five key
        /// </summary>
        [Description("Numpad 5")]
        NUM_FIVE = 74,

        /// <summary>
        /// Numpad six key
        /// </summary>
        [Description("Numpad 6")]
        NUM_SIX = 75,


        /// <summary>
        /// Left Shift key
        /// </summary>
        [Description("Left Shift")]
        LEFT_SHIFT = 76,

        /// <summary>
        /// Non-US Backslash key
        /// </summary>
        [Description("Non-US Backslash")]
        BACKSLASH_UK = 77,

        /// <summary>
        /// Z key
        /// </summary>
        [Description("Z")]
        Z = 78,

        /// <summary>
        /// X key
        /// </summary>
        [Description("X")]
        X = 79,

        /// <summary>
        /// C key
        /// </summary>
        [Description("C")]
        C = 80,

        /// <summary>
        /// V key
        /// </summary>
        [Description("V")]
        V = 81,

        /// <summary>
        /// B key
        /// </summary>
        [Description("B")]
        B = 82,

        /// <summary>
        /// N key
        /// </summary>
        [Description("N")]
        N = 83,

        /// <summary>
        /// M key
        /// </summary>
        [Description("M")]
        M = 84,

        /// <summary>
        /// Comma key
        /// </summary>
        [Description("Comma")]
        COMMA = 85,

        /// <summary>
        /// Period key
        /// </summary>
        [Description("Period")]
        PERIOD = 86,

        /// <summary>
        /// Forward Slash key
        /// </summary>
        [Description("Forward Slash")]
        FORWARD_SLASH = 87,

        /// <summary>
        /// OEM 8 key
        /// </summary>
        [Description("OEM 8")]
        OEM8 = 158,

        /// <summary>
        /// OEM 102 key
        /// </summary>
        [Description("OEM 102")]
        OEM102 = 159,

        /// <summary>
        /// Right Shift key
        /// </summary>
        [Description("Right Shift")]
        RIGHT_SHIFT = 88,

        /// <summary>
        /// Arrow Up key
        /// </summary>
        [Description("Arrow Up")]
        ARROW_UP = 89,

        /// <summary>
        /// Numpad one key
        /// </summary>
        [Description("Numpad 1")]
        NUM_ONE = 90,

        /// <summary>
        /// Numpad two key
        /// </summary>
        [Description("Numpad 2")]
        NUM_TWO = 91,

        /// <summary>
        /// Numpad three key
        /// </summary>
        [Description("Numpad 3")]
        NUM_THREE = 92,

        /// <summary>
        /// Numpad enter key
        /// </summary>
        [Description("Numpad Enter")]
        NUM_ENTER = 93,


        /// <summary>
        /// Left Control key
        /// </summary>
        [Description("Left Control")]
        LEFT_CONTROL = 94,

        /// <summary>
        /// Left Windows key
        /// </summary>
        [Description("Left Windows Key")]
        LEFT_WINDOWS = 95,

        /// <summary>
        /// Left Alt key
        /// </summary>
        [Description("Left Alt")]
        LEFT_ALT = 96,

        /// <summary>
        /// Non-conversion (Japanese layout) key
        /// </summary>
        [Description("Non-conversion")]
        JPN_MUHENKAN = 153,

        /// <summary>
        /// Spacebar key
        /// </summary>
        [Description("Spacebar")]
        SPACE = 97,

        /// <summary>
        /// Conversion (Japanese layout) key
        /// </summary>
        [Description("Conversion")]
        JPN_HENKAN = 154,

        /// <summary>
        /// Hiragana/Katakana (Japanese layout) key
        /// </summary>
        [Description("Hiragana/Katakana")]
        JPN_HIRAGANA_KATAKANA = 155,

        /// <summary>
        /// Right Alt key
        /// </summary>
        [Description("Right Alt")]
        RIGHT_ALT = 98,

        /// <summary>
        /// Right Windows key
        /// </summary>
        [Description("Right Windows Key")]
        RIGHT_WINDOWS = 99,

        /// <summary>
        /// Application Select key
        /// </summary>
        [Description("Application Select Key")]
        APPLICATION_SELECT = 100,

        /// <summary>
        /// Right Control key
        /// </summary>
        [Description("Right Control")]
        RIGHT_CONTROL = 101,

        /// <summary>
        /// Arrow Left key
        /// </summary>
        [Description("Arrow Left")]
        ARROW_LEFT = 102,

        /// <summary>
        /// Arrow Down key
        /// </summary>
        [Description("Arrow Down")]
        ARROW_DOWN = 103,

        /// <summary>
        /// Arrow Right key
        /// </summary>
        [Description("Arrow Right")]
        ARROW_RIGHT = 104,

        /// <summary>
        /// Numpad zero key
        /// </summary>
        [Description("Numpad 0")]
        NUM_ZERO = 105,

        /// <summary>
        /// Numpad period key
        /// </summary>
        [Description("Numpad Period")]
        NUM_PERIOD = 106,


        /// <summary>
        /// Function key
        /// </summary>
        [Description("FN Key")]
        FN_Key = 107,


        /// <summary>
        /// Macrokey 1 key
        /// </summary>
        [Description("G1")]
        G1 = 108,

        /// <summary>
        /// Macrokey 2 key
        /// </summary>
        [Description("G2")]
        G2 = 109,

        /// <summary>
        /// Macrokey 3 key
        /// </summary>
        [Description("G3")]
        G3 = 110,

        /// <summary>
        /// Macrokey 4 key
        /// </summary>
        [Description("G4")]
        G4 = 111,

        /// <summary>
        /// Macrokey 5 key
        /// </summary>
        [Description("G5")]
        G5 = 112,

        /// <summary>
        /// Macrokey 6 key
        /// </summary>
        [Description("G6")]
        G6 = 113,

        /// <summary>
        /// Macrokey 7 key
        /// </summary>
        [Description("G7")]
        G7 = 114,

        /// <summary>
        /// Macrokey 8 key
        /// </summary>
        [Description("G8")]
        G8 = 115,

        /// <summary>
        /// Macrokey 9 key
        /// </summary>
        [Description("G9")]
        G9 = 116,

        /// <summary>
        /// Macrokey 10 key
        /// </summary>
        [Description("G10")]
        G10 = 117,

        /// <summary>
        /// Macrokey 11 key
        /// </summary>
        [Description("G11")]
        G11 = 118,

        /// <summary>
        /// Macrokey 12 key
        /// </summary>
        [Description("G12")]
        G12 = 119,

        /// <summary>
        /// Macrokey 13 key
        /// </summary>
        [Description("G13")]
        G13 = 120,

        /// <summary>
        /// Macrokey 14 key
        /// </summary>
        [Description("G14")]
        G14 = 121,

        /// <summary>
        /// Macrokey 15 key
        /// </summary>
        [Description("G15")]
        G15 = 122,

        /// <summary>
        /// Macrokey 16 key
        /// </summary>
        [Description("G16")]
        G16 = 123,

        /// <summary>
        /// Macrokey 17 key
        /// </summary>
        [Description("G17")]
        G17 = 124,

        /// <summary>
        /// Macrokey 18 key
        /// </summary>
        [Description("G18")]
        G18 = 125,

        /// <summary>
        /// Macrokey 19 key
        /// </summary>
        [Description("G19")]
        G19 = 126,

        /// <summary>
        /// Macrokey 20 key
        /// </summary>
        [Description("G20")]
        G20 = 127,


        /// <summary>
        /// Brand Logo
        /// </summary>
        [Description("Brand Logo")]
        LOGO = 128,

        /// <summary>
        /// Brand Logo #2
        /// </summary>
        [Description("Brand Logo #2")]
        LOGO2 = 129,

        /// <summary>
        /// Brand Logo #3
        /// </summary>
        [Description("Brand Logo #3")]
        LOGO3 = 130,

        /// <summary>
        /// Brightness Switch
        /// </summary>
        [Description("Brightness Switch")]
        BRIGHTNESS_SWITCH = 131,

        /// <summary>
        /// Lock Switch
        /// </summary>
        [Description("Lock Switch")]
        LOCK_SWITCH = 132,


        /// <summary>
        /// Multimedia Play/Pause
        /// </summary>
        [Description("Media Play/Pause")]
        MEDIA_PLAY_PAUSE = 133,

        /// <summary>
        /// Multimedia Play
        /// </summary>
        [Description("Media Play")]
        MEDIA_PLAY = 134,

        /// <summary>
        /// Multimedia Pause
        /// </summary>
        [Description("Media Pause")]
        MEDIA_PAUSE = 135,

        /// <summary>
        /// Multimedia Stop
        /// </summary>
        [Description("Media Stop")]
        MEDIA_STOP = 136,

        /// <summary>
        /// Multimedia Previous
        /// </summary>
        [Description("Media Previous")]
        MEDIA_PREVIOUS = 137,

        /// <summary>
        /// Multimedia Next
        /// </summary>
        [Description("Media Next")]
        MEDIA_NEXT = 138,


        /// <summary>
        /// Volume Mute
        /// </summary>
        [Description("Volume Mute")]
        VOLUME_MUTE = 139,

        /// <summary>
        /// Volume Down
        /// </summary>
        [Description("Volume Down")]
        VOLUME_DOWN = 140,

        /// <summary>
        /// Volume Up
        /// </summary>
        [Description("Volume Up")]
        VOLUME_UP = 141,


        /// <summary>
        /// Additional Light 1
        /// </summary>
        [Description("Additional Light 1")]
        ADDITIONALLIGHT1 = 142,

        /// <summary>
        /// Additional Light 2
        /// </summary>
        [Description("Additional Light 2")]
        ADDITIONALLIGHT2 = 143,

        /// <summary>
        /// Additional Light 3
        /// </summary>
        [Description("Additional Light 3")]
        ADDITIONALLIGHT3 = 144,

        /// <summary>
        /// Additional Light 4
        /// </summary>
        [Description("Additional Light 4")]
        ADDITIONALLIGHT4 = 145,

        /// <summary>
        /// Additional Light 5
        /// </summary>
        [Description("Additional Light 5")]
        ADDITIONALLIGHT5 = 146,

        /// <summary>
        /// Additional Light 6
        /// </summary>
        [Description("Additional Light 6")]
        ADDITIONALLIGHT6 = 147,

        /// <summary>
        /// Additional Light 7
        /// </summary>
        [Description("Additional Light 7")]
        ADDITIONALLIGHT7 = 148,

        /// <summary>
        /// Additional Light 8
        /// </summary>
        [Description("Additional Light 8")]
        ADDITIONALLIGHT8 = 149,

        /// <summary>
        /// Additional Light 9
        /// </summary>
        [Description("Additional Light 9")]
        ADDITIONALLIGHT9 = 150,

        /// <summary>
        /// Additional Light 10
        /// </summary>
        [Description("Additional Light 10")]
        ADDITIONALLIGHT10 = 151,

        /// <summary>
        /// Peripheral Logo
        /// </summary>
        [Description("Peripheral Logo")]
        Peripheral_Logo = 160,

        /// <summary>
        /// Peripheral Scroll Wheel
        /// </summary>
        [Description("Peripheral Scroll Wheel")]
        Peripheral_ScrollWheel = 161,

        /// <summary>
        /// Peripheral Front-facing lights
        /// </summary>
        [Description("Peripheral Front Lights")]
        Peripheral_FrontLight = 162,

        /// <summary>
        /// Profile key 1
        /// </summary>
        [Description("Profile Key 1")]
        Profile_Key1 = 163,

        /// <summary>
        /// Profile key 2
        /// </summary>
        [Description("Profile Key 2")]
        Profile_Key2 = 164,

        /// <summary>
        /// Profile key 3
        /// </summary>
        [Description("Profile Key 3")]
        Profile_Key3 = 165,

        /// <summary>
        /// Profile key 4
        /// </summary>
        [Description("Profile Key 4")]
        Profile_Key4 = 166,

        /// <summary>
        /// Profile key 5
        /// </summary>
        [Description("Profile Key 5")]
        Profile_Key5 = 167,

        /// <summary>
        /// Profile key 6
        /// </summary>
        [Description("Profile Key 6")]
        Profile_Key6 = 168,

        /// <summary>
        /// Numpad 00
        /// </summary>
        [Description("Numpad 00")]
        NUM_ZEROZERO = 169,

        /// <summary>
        /// Macrokey 0 key
        /// </summary>
        [Description("G0")]
        G0 = 170,

        /// <summary>
        /// Macrokey 0 key
        /// </summary>
        [Description("Left FN")]
        LEFT_FN = 171,

        /// <summary>
        /// Additional Light 11
        /// </summary>
        [Description("Additional Light 11")]
        ADDITIONALLIGHT11 = 172,

        /// <summary>
        /// Additional Light 12
        /// </summary>
        [Description("Additional Light 12")]
        ADDITIONALLIGHT12 = 173,

        /// <summary>
        /// Additional Light 13
        /// </summary>
        [Description("Additional Light 13")]
        ADDITIONALLIGHT13 = 174,

        /// <summary>
        /// Additional Light 14
        /// </summary>
        [Description("Additional Light 14")]
        ADDITIONALLIGHT14 = 175,

        /// <summary>
        /// Additional Light 15
        /// </summary>
        [Description("Additional Light 15")]
        ADDITIONALLIGHT15 = 176,

        /// <summary>
        /// Additional Light 16
        /// </summary>
        [Description("Additional Light 16")]
        ADDITIONALLIGHT16 = 177,

        /// <summary>
        /// Additional Light 17
        /// </summary>
        [Description("Additional Light 17")]
        ADDITIONALLIGHT17 = 178,

        /// <summary>
        /// Additional Light 18
        /// </summary>
        [Description("Additional Light 18")]
        ADDITIONALLIGHT18 = 179,

        /// <summary>
        /// Additional Light 19
        /// </summary>
        [Description("Additional Light 19")]
        ADDITIONALLIGHT19 = 180,

        /// <summary>
        /// Additional Light 20
        /// </summary>
        [Description("Additional Light 20")]
        ADDITIONALLIGHT20 = 181,

        /// <summary>
        /// Additional Light 21
        /// </summary>
        [Description("Additional Light 21")]
        ADDITIONALLIGHT21 = 182,

        /// <summary>
        /// Additional Light 22
        /// </summary>
        [Description("Additional Light 22")]
        ADDITIONALLIGHT22 = 183,

        /// <summary>
        /// Additional Light 23
        /// </summary>
        [Description("Additional Light 23")]
        ADDITIONALLIGHT23 = 184,

        /// <summary>
        /// Additional Light 24
        /// </summary>
        [Description("Additional Light 24")]
        ADDITIONALLIGHT24 = 185,

        /// <summary>
        /// Additional Light 25
        /// </summary>
        [Description("Additional Light 25")]
        ADDITIONALLIGHT25 = 186,

        /// <summary>
        /// Additional Light 26
        /// </summary>
        [Description("Additional Light 26")]
        ADDITIONALLIGHT26 = 187,

        /// <summary>
        /// Additional Light 27
        /// </summary>
        [Description("Additional Light 27")]
        ADDITIONALLIGHT27 = 188,
        
        /// <summary>
        /// Additional Light 28
        /// </summary>
        [Description("Additional Light 28")]
        ADDITIONALLIGHT28 = 189,

        /// <summary>
        /// Additional Light 29
        /// </summary>
        [Description("Additional Light 29")]
        ADDITIONALLIGHT29 = 190,

        /// <summary>
        /// Additional Light 30
        /// </summary>
        [Description("Additional Light 30")]
        ADDITIONALLIGHT30 = 191,

        /// <summary>
        /// Additional Light 31
        /// </summary>
        [Description("Additional Light 31")]
        ADDITIONALLIGHT31 = 192,

        /// <summary>
        /// Additional Light 32
        /// </summary>
        [Description("Additional Light 32")]
        ADDITIONALLIGHT32 = 193,

        /// <summary>
        /// Mousepad Light 1
        /// </summary>
        [Description("Mousepad Light 1")]
        MOUSEPADLIGHT1 = 201,

        /// <summary>
        /// Mousepad Light 2
        /// </summary>
        [Description("Mousepad Light 2")]
        MOUSEPADLIGHT2 = 202,

        /// <summary>
        /// Mousepad Light 3
        /// </summary>
        [Description("Mousepad Light 3")]
        MOUSEPADLIGHT3 = 203,

        /// <summary>
        /// Mousepad Light 4
        /// </summary>
        [Description("Mousepad Light 4")]
        MOUSEPADLIGHT4 = 204,

        /// <summary>
        /// Mousepad Light 5
        /// </summary>
        [Description("Mousepad Light 5")]
        MOUSEPADLIGHT5 = 205,

        /// <summary>
        /// Mousepad Light 6
        /// </summary>
        [Description("Mousepad Light 6")]
        MOUSEPADLIGHT6 = 206,

        /// <summary>
        /// Mousepad Light 7
        /// </summary>
        [Description("Mousepad Light 7")]
        MOUSEPADLIGHT7 = 207,

        /// <summary>
        /// Mousepad Light 8
        /// </summary>
        [Description("Mousepad Light 8")]
        MOUSEPADLIGHT8 = 208,

        /// <summary>
        /// Mousepad Light 9
        /// </summary>
        [Description("Mousepad Light 9")]
        MOUSEPADLIGHT9 = 209,

        /// <summary>
        /// Mousepad Light 10
        /// </summary>
        [Description("Mousepad Light 10")]
        MOUSEPADLIGHT10 = 210,

        /// <summary>
        /// Mousepad Light 11
        /// </summary>
        [Description("Mousepad Light 11")]
        MOUSEPADLIGHT11 = 211,

        /// <summary>
        /// Mousepad Light 12
        /// </summary>
        [Description("Mousepad Light 12")]
        MOUSEPADLIGHT12 = 212,

        /// <summary>
        /// Mousepad Light 13
        /// </summary>
        [Description("Mousepad Light 13")]
        MOUSEPADLIGHT13 = 213,

        /// <summary>
        /// Mousepad Light 14
        /// </summary>
        [Description("Mousepad Light 14")]
        MOUSEPADLIGHT14 = 214,

        /// <summary>
        /// Mousepad Light 15
        /// </summary>
        [Description("Mousepad Light 15")]
        MOUSEPADLIGHT15 = 215,

        ///<summary>
        /// Calculator Key
        /// </summary>
        [Description("Calculator")]
        CALC = 216,

        /// <summary>
        /// None
        /// </summary>
        [Description("None")]
        NONE = -1,

        /// <summary>
        /// Additional Light 33
        /// </summary>
        [Description("Additional Light 33")]
        ADDITIONALLIGHT33 = 217,

        /// <summary>
        /// Additional Light 34
        /// </summary>
        [Description("Additional Light 34")]
        ADDITIONALLIGHT34 = 218,

        /// <summary>
        /// Additional Light 35
        /// </summary>
        [Description("Additional Light 35")]
        ADDITIONALLIGHT35 = 219,

        /// <summary>
        /// Additional Light 36
        /// </summary>
        [Description("Additional Light 36")]
        ADDITIONALLIGHT36 = 220,

        /// <summary>
        /// Additional Light 37
        /// </summary>
        [Description("Additional Light 37")]
        ADDITIONALLIGHT37 = 221,

        /// <summary>
        /// Additional Light 38
        /// </summary>
        [Description("Additional Light 38")]
        ADDITIONALLIGHT38 = 222,

        /// <summary>
        /// Additional Light 39
        /// </summary>
        [Description("Additional Light 39")]
        ADDITIONALLIGHT39 = 223,

        /// <summary>
        /// Additional Light 40
        /// </summary>
        [Description("Additional Light 40")]
        ADDITIONALLIGHT40 = 224,

        /// <summary>
        /// Led Light 1
        /// </summary>
        [Description("Led Light 1")]
        LEDLIGHT1 = 1001,

        /// <summary>
        /// Led Light 2
        /// </summary>
        [Description("Led Light 2")]
        LEDLIGHT2 = 1002,

        /// <summary>
        /// Led Light 3
        /// </summary>
        [Description("Led Light 3")]
        LEDLIGHT3 = 1003,

        /// <summary>
        /// Led Light 4
        /// </summary>
        [Description("Led Light 4")]
        LEDLIGHT4 = 1004,

        /// <summary>
        /// Led Light 5
        /// </summary>
        [Description("Led Light 5")]
        LEDLIGHT5 = 1005,

        /// <summary>
        /// Led Light 6
        /// </summary>
        [Description("Led Light 6")]
        LEDLIGHT6 = 1006,

        /// <summary>
        /// Led Light 7
        /// </summary>
        [Description("Led Light 7")]
        LEDLIGHT7 = 1007,

        /// <summary>
        /// Led Light 8
        /// </summary>
        [Description("Led Light 8")]
        LEDLIGHT8 = 1008,

        /// <summary>
        /// Led Light 9
        /// </summary>
        [Description("Led Light 9")]
        LEDLIGHT9 = 1009,

        /// <summary>
        /// Led Light 10
        /// </summary>
        [Description("Led Light 10")]
        LEDLIGHT10 = 1010,

        /// <summary>
        /// Led Light 11
        /// </summary>
        [Description("Led Light 11")]
        LEDLIGHT11 = 1011,

        /// <summary>
        /// Led Light 12
        /// </summary>
        [Description("Led Light 12")]
        LEDLIGHT12 = 1012,

        /// <summary>
        /// Led Light 13
        /// </summary>
        [Description("Led Light 13")]
        LEDLIGHT13 = 1013,

        /// <summary>
        /// Led Light 14
        /// </summary>
        [Description("Led Light 14")]
        LEDLIGHT14 = 1014,

        /// <summary>
        /// Led Light 15
        /// </summary>
        [Description("Led Light 15")]
        LEDLIGHT15 = 1015,

        /// <summary>
        /// Led Light 16
        /// </summary>
        [Description("Led Light 16")]
        LEDLIGHT16 = 1016,

        /// <summary>
        /// Led Light 17
        /// </summary>
        [Description("Led Light 17")]
        LEDLIGHT17 = 1017,

        /// <summary>
        /// Led Light 18
        /// </summary>
        [Description("Led Light 18")]
        LEDLIGHT18 = 1018,

        /// <summary>
        /// Led Light 19
        /// </summary>
        [Description("Led Light 19")]
        LEDLIGHT19 = 1019,

        /// <summary>
        /// Led Light 20
        /// </summary>
        [Description("Led Light 20")]
        LEDLIGHT20 = 1020,

        /// <summary>
        /// Led Light 21
        /// </summary>
        [Description("Led Light 21")]
        LEDLIGHT21 = 1021,

        /// <summary>
        /// Led Light 22
        /// </summary>
        [Description("Led Light 22")]
        LEDLIGHT22 = 1022,

        /// <summary>
        /// Led Light 23
        /// </summary>
        [Description("Led Light 23")]
        LEDLIGHT23 = 1023,

        /// <summary>
        /// Led Light 24
        /// </summary>
        [Description("Led Light 24")]
        LEDLIGHT24 = 1024,

        /// <summary>
        /// Led Light 25
        /// </summary>
        [Description("Led Light 25")]
        LEDLIGHT25 = 1025,

        /// <summary>
        /// Led Light 26
        /// </summary>
        [Description("Led Light 26")]
        LEDLIGHT26 = 1026,

        /// <summary>
        /// Led Light 27
        /// </summary>
        [Description("Led Light 27")]
        LEDLIGHT27 = 1027,

        /// <summary>
        /// Led Light 28
        /// </summary>
        [Description("Led Light 28")]
        LEDLIGHT28 = 1028,

        /// <summary>
        /// Led Light 29
        /// </summary>
        [Description("Led Light 29")]
        LEDLIGHT29 = 1029,

        /// <summary>
        /// Led Light 30
        /// </summary>
        [Description("Led Light 30")]
        LEDLIGHT30 = 1030,

        /// <summary>
        /// Led Light 31
        /// </summary>
        [Description("Led Light 31")]
        LEDLIGHT31 = 1031,

        /// <summary>
        /// Led Light 32
        /// </summary>
        [Description("Led Light 32")]
        LEDLIGHT32 = 1032,

        /// <summary>
        /// Led Light 33
        /// </summary>
        [Description("Led Light 33")]
        LEDLIGHT33 = 1033,

        /// <summary>
        /// Led Light 34
        /// </summary>
        [Description("Led Light 34")]
        LEDLIGHT34 = 1034,

        /// <summary>
        /// Led Light 35
        /// </summary>
        [Description("Led Light 35")]
        LEDLIGHT35 = 1035,

        /// <summary>
        /// Led Light 36
        /// </summary>
        [Description("Led Light 36")]
        LEDLIGHT36 = 1036,

        /// <summary>
        /// Led Light 37
        /// </summary>
        [Description("Led Light 37")]
        LEDLIGHT37 = 1037,

        /// <summary>
        /// Led Light 38
        /// </summary>
        [Description("Led Light 38")]
        LEDLIGHT38 = 1038,

        /// <summary>
        /// Led Light 39
        /// </summary>
        [Description("Led Light 39")]
        LEDLIGHT39 = 1039,

        /// <summary>
        /// Led Light 40
        /// </summary>
        [Description("Led Light 40")]
        LEDLIGHT40 = 1040,

        /// <summary>
        /// Led Light 41
        /// </summary>
        [Description("Led Light 41")]
        LEDLIGHT41 = 1041,

        /// <summary>
        /// Led Light 42
        /// </summary>
        [Description("Led Light 42")]
        LEDLIGHT42 = 1042,

        /// <summary>
        /// Led Light 43
        /// </summary>
        [Description("Led Light 43")]
        LEDLIGHT43 = 1043,

        /// <summary>
        /// Led Light 44
        /// </summary>
        [Description("Led Light 44")]
        LEDLIGHT44 = 1044,

        /// <summary>
        /// Led Light 45
        /// </summary>
        [Description("Led Light 45")]
        LEDLIGHT45 = 1045,

        /// <summary>
        /// Led Light 46
        /// </summary>
        [Description("Led Light 46")]
        LEDLIGHT46 = 1046,

        /// <summary>
        /// Led Light 47
        /// </summary>
        [Description("Led Light 47")]
        LEDLIGHT47 = 1047,

        /// <summary>
        /// Led Light 48
        /// </summary>
        [Description("Led Light 48")]
        LEDLIGHT48 = 1048,

        /// <summary>
        /// Led Light 49
        /// </summary>
        [Description("Led Light 49")]
        LEDLIGHT49 = 1049,

        /// <summary>
        /// Led Light 50
        /// </summary>
        [Description("Led Light 50")]
        LEDLIGHT50 = 1050,

        /// <summary>
        /// Led Light 51
        /// </summary>
        [Description("Led Light 51")]
        LEDLIGHT51 = 1051,

        /// <summary>
        /// Led Light 52
        /// </summary>
        [Description("Led Light 52")]
        LEDLIGHT52 = 1052,

        /// <summary>
        /// Led Light 53
        /// </summary>
        [Description("Led Light 53")]
        LEDLIGHT53 = 1053,

        /// <summary>
        /// Led Light 54
        /// </summary>
        [Description("Led Light 54")]
        LEDLIGHT54 = 1054,

        /// <summary>
        /// Led Light 55
        /// </summary>
        [Description("Led Light 55")]
        LEDLIGHT55 = 1055,

        /// <summary>
        /// Led Light 56
        /// </summary>
        [Description("Led Light 56")]
        LEDLIGHT56 = 1056,

        /// <summary>
        /// Led Light 57
        /// </summary>
        [Description("Led Light 57")]
        LEDLIGHT57 = 1057,

        /// <summary>
        /// Led Light 58
        /// </summary>
        [Description("Led Light 58")]
        LEDLIGHT58 = 1058,

        /// <summary>
        /// Led Light 59
        /// </summary>
        [Description("Led Light 59")]
        LEDLIGHT59 = 1059,

        /// <summary>
        /// Led Light 60
        /// </summary>
        [Description("Led Light 60")]
        LEDLIGHT60 = 1060,

        /// <summary>
        /// Led Light 61
        /// </summary>
        [Description("Led Light 61")]
        LEDLIGHT61 = 1061,

        /// <summary>
        /// Led Light 62
        /// </summary>
        [Description("Led Light 62")]
        LEDLIGHT62 = 1062,

        /// <summary>
        /// Led Light 63
        /// </summary>
        [Description("Led Light 63")]
        LEDLIGHT63 = 1063,

        /// <summary>
        /// Led Light 64
        /// </summary>
        [Description("Led Light 64")]
        LEDLIGHT64 = 1064,

        /// <summary>
        /// Led Light 65
        /// </summary>
        [Description("Led Light 65")]
        LEDLIGHT65 = 1065,

        /// <summary>
        /// Led Light 66
        /// </summary>
        [Description("Led Light 66")]
        LEDLIGHT66 = 1066,

        /// <summary>
        /// Led Light 67
        /// </summary>
        [Description("Led Light 67")]
        LEDLIGHT67 = 1067,

        /// <summary>
        /// Led Light 68
        /// </summary>
        [Description("Led Light 68")]
        LEDLIGHT68 = 1068,

        /// <summary>
        /// Led Light 69
        /// </summary>
        [Description("Led Light 69")]
        LEDLIGHT69 = 1069,

        /// <summary>
        /// Led Light 70
        /// </summary>
        [Description("Led Light 70")]
        LEDLIGHT70 = 1070,

        /// <summary>
        /// Led Light 71
        /// </summary>
        [Description("Led Light 71")]
        LEDLIGHT71 = 1071,

        /// <summary>
        /// Led Light 72
        /// </summary>
        [Description("Led Light 72")]
        LEDLIGHT72 = 1072,

        /// <summary>
        /// Led Light 73
        /// </summary>
        [Description("Led Light 73")]
        LEDLIGHT73 = 1073,

        /// <summary>
        /// Led Light 24
        /// </summary>
        [Description("Led Light 74")]
        LEDLIGHT74 = 1074,

        /// <summary>
        /// Led Light 75
        /// </summary>
        [Description("Led Light 75")]
        LEDLIGHT75 = 1075,

        /// <summary>
        /// Led Light 76
        /// </summary>
        [Description("Led Light 76")]
        LEDLIGHT76 = 1076,

        /// <summary>
        /// Led Light 77
        /// </summary>
        [Description("Led Light 77")]
        LEDLIGHT77 = 1077,

        /// <summary>
        /// Led Light 78
        /// </summary>
        [Description("Led Light 78")]
        LEDLIGHT78 = 1078,

        /// <summary>
        /// Led Light 79
        /// </summary>
        [Description("Led Light 79")]
        LEDLIGHT79 = 1079,

        /// <summary>
        /// Led Light 80
        /// </summary>
        [Description("Led Light 80")]
        LEDLIGHT80 = 1080,

        /// <summary>
        /// Led Light 81
        /// </summary>
        [Description("Led Light 81")]
        LEDLIGHT81 = 1081,

        /// <summary>
        /// Led Light 82
        /// </summary>
        [Description("Led Light 82")]
        LEDLIGHT82 = 1082,

        /// <summary>
        /// Led Light 83
        /// </summary>
        [Description("Led Light 83")]
        LEDLIGHT83 = 1083,

        /// <summary>
        /// Led Light 84
        /// </summary>
        [Description("Led Light 84")]
        LEDLIGHT84 = 1084,

        /// <summary>
        /// Led Light 85
        /// </summary>
        [Description("Led Light 85")]
        LEDLIGHT85 = 1085,

        /// <summary>
        /// Led Light 86
        /// </summary>
        [Description("Led Light 86")]
        LEDLIGHT86 = 1086,

        /// <summary>
        /// Led Light 87
        /// </summary>
        [Description("Led Light 87")]
        LEDLIGHT87 = 1087,

        /// <summary>
        /// Led Light 88
        /// </summary>
        [Description("Led Light 88")]
        LEDLIGHT88 = 1088,

        /// <summary>
        /// Led Light 89
        /// </summary>
        [Description("Led Light 89")]
        LEDLIGHT89 = 1089,

        /// <summary>
        /// Led Light 90
        /// </summary>
        [Description("Led Light 90")]
        LEDLIGHT90 = 1090,

        /// <summary>
        /// Led Light 91
        /// </summary>
        [Description("Led Light 91")]
        LEDLIGHT91 = 1091,

        /// <summary>
        /// Led Light 92
        /// </summary>
        [Description("Led Light 92")]
        LEDLIGHT92 = 1092,

        /// <summary>
        /// Led Light 93
        /// </summary>
        [Description("Led Light 93")]
        LEDLIGHT93 = 1093,

        /// <summary>
        /// Led Light 94
        /// </summary>
        [Description("Led Light 94")]
        LEDLIGHT94 = 1094,

        /// <summary>
        /// Led Light 95
        /// </summary>
        [Description("Led Light 95")]
        LEDLIGHT95 = 1095,

        /// <summary>
        /// Led Light 96
        /// </summary>
        [Description("Led Light 96")]
        LEDLIGHT96 = 1096,

        /// <summary>
        /// Led Light 97
        /// </summary>
        [Description("Led Light 97")]
        LEDLIGHT97 = 1097,

        /// <summary>
        /// Led Light 98
        /// </summary>
        [Description("Led Light 98")]
        LEDLIGHT98 = 1098,

        /// <summary>
        /// Led Light 99
        /// </summary>
        [Description("Led Light 99")]
        LEDLIGHT99 = 1099,

        /// <summary>
        /// Led Light 100
        /// </summary>
        [Description("Led Light 100")]
        LEDLIGHT100 = 1100,
    };

    /// <summary>
    /// Struct representing color settings being sent to devices
    /// </summary>
    public class DeviceColorComposition
    {
        public readonly object bitmapLock = new object();
        public Dictionary<DeviceKeys, Color> keyColors;
        public Bitmap keyBitmap;
    }

    /// <summary>
    /// An interface for a device class.
    /// </summary>
    public interface Device
    {
        /// <summary>
        /// Gets registered variables by this device.
        /// </summary>
        /// <returns>Registered Variables</returns>
        Settings.VariableRegistry GetRegisteredVariables();

        /// <summary>
        /// Gets the device name.
        /// </summary>
        /// <returns>Device name</returns>
        string GetDeviceName();

        /// <summary>
        /// Gets specific details about the device instance.
        /// </summary>
        /// <returns>Details about the device instance</returns>
        string GetDeviceDetails();

        /// <summary>
        /// Gets the device update performance.
        /// </summary>
        /// <returns>Details about device's update performance</returns>
        string GetDeviceUpdatePerformance();

        /// <summary>
        /// Attempts to initialize the device instance.
        /// </summary>
        /// <returns>A boolean value representing the success of this call</returns>
        bool Initialize();

        /// <summary>
        /// Shuts down the device instance.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Resets the device instance.
        /// </summary>
        void Reset();

        /// <summary>
        /// Attempts to reconnect the device. [NOT IMPLEMENTED]
        /// </summary>
        /// <returns>A boolean value representing the success of this call</returns>
        bool Reconnect();

        /// <summary>
        /// Gets the initialization status of this device instance.
        /// </summary>
        /// <returns>A boolean value representing the initialization status of this device</returns>
        bool IsInitialized();

        /// <summary>
        /// Gets the connection status of this device instance. [NOT IMPLEMENTED]
        /// </summary>
        /// <returns>A boolean value representing the connection status of this device</returns>
        bool IsConnected();

        /// <summary>
        /// Gets the keyboard connection status for this device instance.
        /// </summary>
        /// <returns>A boolean value representing the keyboard connection status of this device</returns>
        bool IsKeyboardConnected();

        /// <summary>
        /// Gets the peripheral connection status for this device instance.
        /// </summary>
        /// <returns>A boolean value representing the peripheral connection status of this device</returns>
        bool IsPeripheralConnected();

        /// <summary>
        /// Updates the device with a specified color arrangement.
        /// </summary>
        /// <param name="keyColors">A dictionary of DeviceKeys their corresponding Colors</param>
        /// <param name="forced">A boolean value indicating whether or not to forcefully update this device</param>
        /// <returns></returns>
        bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false);

        /// <summary>
        /// Updates the device with a specified color composition.
        /// </summary>
        /// <param name="colorComposition">A struct containing a dictionary of colors as well as the resulting bitmap</param>
        /// <param name="forced">A boolean value indicating whether or not to forcefully update this device</param>
        /// <returns></returns>
        bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false);
    }
}
