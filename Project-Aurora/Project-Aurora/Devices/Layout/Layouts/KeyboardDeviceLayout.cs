using Aurora.Settings;
using Aurora.Settings.Keycaps;
using Aurora.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LEDINT = System.Int16;

namespace Aurora.Devices.Layout.Layouts
{
    /// <summary>
    /// Enum definition, representing everysingle pre-defined keyboard key
    /// </summary>
    public enum KeyboardKeys : LEDINT
    {
        [Description("Global")]
        Global = 0,

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
        HASH = 71,

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
        ADDITIONALLIGHT25 = 180,

        /// <summary>
        /// Additional Light 26
        /// </summary>
        [Description("Additional Light 26")]
        ADDITIONALLIGHT26 = 186,

        /// <summary>
        /// Additional Light 27
        /// </summary>
        [Description("Additional Light 27")]
        ADDITIONALLIGHT27 = 187,

        /// <summary>
        /// Additional Light 28
        /// </summary>
        [Description("Additional Light 28")]
        ADDITIONALLIGHT28 = 188,

        /// <summary>
        /// Additional Light 29
        /// </summary>
        [Description("Additional Light 29")]
        ADDITIONALLIGHT29 = 189,

        /// <summary>
        /// Additional Light 30
        /// </summary>
        [Description("Additional Light 30")]
        ADDITIONALLIGHT30 = 190,

        /// <summary>
        /// None
        /// </summary>
        [Description("None")]
        NONE = -1,
    };

    

    public class KeyboardDeviceLayout : DeviceLayout
    {
        public enum PreferredKeyboard
        {
            [Description("None")]
            None = 0,

            [Description("Generic Laptop")]
            GenericLaptop = 1,

            [Description("Generic Laptop (Numpad)")]
            GenericLaptopNumpad = 2,
            /*
            [Description("Logitech")]
            Logitech = 1,
            [Description("Corsair")]
            Corsair = 2,
            [Description("Razer")]
            Razer = 3,

            [Description("Clevo")]
            Clevo = 4,
            [Description("Cooler Master")]
            CoolerMaster = 5,
            */

            //Logitech range is 100-199
            [Description("Logitech - G910")]
            Logitech_G910 = 100,
            [Description("Logitech - G410")]
            Logitech_G410 = 101,
            [Description("Logitech - G810")]
            Logitech_G810 = 102,
            [Description("Logitech - GPRO")]
            Logitech_GPRO = 103,

            //Corsair range is 200-299
            [Description("Corsair - K95")]
            Corsair_K95 = 200,
            [Description("Corsair - K70")]
            Corsair_K70 = 201,
            [Description("Corsair - K65")]
            Corsair_K65 = 202,
            [Description("Corsair - STRAFE")]
            Corsair_STRAFE = 203,
            [Description("Corsair - K95 Platinum")]
            Corsair_K95_PL = 204,
            [Description("Corsair - K68")]
            Corsair_K68 = 205,
            [Description("Corsair - K70 MK2")]
            Corsair_K70MK2 = 206
            ,
            //Razer range is 300-399
            [Description("Razer - Blackwidow")]
            Razer_Blackwidow = 300,
            [Description("Razer - Blackwidow X")]
            Razer_Blackwidow_X = 301,
            [Description("Razer - Blackwidow Tournament Edition")]
            Razer_Blackwidow_TE = 302,
            [Description("Razer - Blade")]
            Razer_Blade = 303,

            //Clevo range is 400-499

            //Cooler Master range is 500-599
            [Description("Cooler Master - Masterkeys Pro L")]
            Masterkeys_Pro_L = 500,
            [Description("Cooler Master - Masterkeys Pro S")]
            Masterkeys_Pro_S = 501,
            [Description("Cooler Master - Masterkeys Pro M")]
            Masterkeys_Pro_M = 502,
            [Description("Cooler Master - Masterkeys MK750")]
            Masterkeys_MK750 = 503,

            //Roccat range is 600-699
            [Description("Roccat Ryos")]
            Roccat_Ryos = 600,

            //Steelseries range is 700-799
            [Description("SteelSeries Apex M800")]
            SteelSeries_Apex_M800 = 700,
            [Description("SteelSeries Apex M750")]
            SteelSeries_Apex_M750 = 701,
            [Description("SteelSeries Apex M750 TKL")]
            SteelSeries_Apex_M750_TKL = 702,

            [Description("Wooting One")]
            Wooting_One = 800,

            [Description("Asus Strix Flare")]
            Asus_Strix_Flare = 900,

            //Drevo range is 1000-1099
            [Description("Drevo BladeMaster")]
            Drevo_BladeMaster = 1000,

            //Creative range is 1100-1199
            [Description("SoundBlasterX VanguardK08")]
            SoundBlasterX_Vanguard_K08 = 1100,
        }

        public enum PreferredKeyboardLocalization
        {
            [Description("Automatic Detection")]
            None = 0,
            [Description("International")]
            intl = 1,
            [Description("United States")]
            us = 2,
            [Description("United Kingdom")]
            uk = 3,
            [Description("Russian")]
            ru = 4,
            [Description("French")]
            fr = 5,
            [Description("Deutsch")]
            de = 6,
            [Description("Japanese")]
            jpn = 7,
            [Description("Turkish")]
            tr = 8,
            [Description("Nordic")]
            nordic = 9,
            [Description("Swiss")]
            swiss = 10,
            [Description("Portuguese (Brazilian ABNT2)")]
            abnt2 = 11,
            [Description("DVORAK (US)")]
            dvorak = 12,
            [Description("DVORAK (INT)")]
            dvorak_int = 13,
            [Description("Hungarian")]
            hu = 14
        }

        [JsonIgnore]
        public new static readonly byte DeviceTypeID = 0;

        [JsonIgnore]
        public override byte GetDeviceTypeID { get { return DeviceTypeID; } }

        private PreferredKeyboard style = PreferredKeyboard.None;
        //[JsonIgnore]
        public PreferredKeyboard Style { get { return style; } set { UpdateVar(ref style, value); } }

        private PreferredKeyboardLocalization language = PreferredKeyboardLocalization.None;
        //[JsonIgnore]
        public PreferredKeyboardLocalization Language { get { return language; } set { UpdateVar(ref language, value); } }

        [JsonIgnore]
        public Dictionary<LEDINT, LEDINT> LayoutKeyConversion = new Dictionary<LEDINT, LEDINT>();
        [JsonIgnore]
        public Dictionary<LEDINT, string> KeyText { get { return virtualGroup.KeyText; } }

        private static string cultures_folder = "kb_layouts";
        private static string layoutsPath = "";
        [JsonIgnore]
        private PreferredKeyboardLocalization _loaded_localization;

        static KeyboardDeviceLayout()
        {
            layoutsPath = Path.Combine(Global.ExecutingDirectory, cultures_folder);
        }

        protected override void loadLayouts()
        {
            //load layouts for selection
        }

        public override void GenerateLayout()
        {
            try
            {
                //System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");

                //Global.logger.LogLine("Loading brand: " + brand.ToString() + " for: " + System.Threading.Thread.CurrentThread.CurrentCulture.Name);

                //Load keyboard layout
                if (Directory.Exists(layoutsPath))
                {
                    string culture = System.Threading.Thread.CurrentThread.CurrentCulture.Name;

                    switch (this.Language)
                    {
                        case PreferredKeyboardLocalization.None:
                            break;
                        case PreferredKeyboardLocalization.intl:
                            culture = "intl";
                            break;
                        case PreferredKeyboardLocalization.us:
                            culture = "en-US";
                            break;
                        case PreferredKeyboardLocalization.uk:
                            culture = "en-GB";
                            break;
                        case PreferredKeyboardLocalization.ru:
                            culture = "ru-RU";
                            break;
                        case PreferredKeyboardLocalization.fr:
                            culture = "fr-FR";
                            break;
                        case PreferredKeyboardLocalization.de:
                            culture = "de-DE";
                            break;
                        case PreferredKeyboardLocalization.jpn:
                            culture = "ja-JP";
                            break;
                        case PreferredKeyboardLocalization.nordic:
                            culture = "nordic";
                            break;
                        case PreferredKeyboardLocalization.tr:
                            culture = "tr-TR";
                            break;
                        case PreferredKeyboardLocalization.swiss:
                            culture = "de-CH";
                            break;
                        case PreferredKeyboardLocalization.abnt2:
                            culture = "pt-BR";
                            break;
                        case PreferredKeyboardLocalization.dvorak:
                            culture = "dvorak";
                            break;
                        case PreferredKeyboardLocalization.dvorak_int:
                            culture = "dvorak_int";
                            break;
                        case PreferredKeyboardLocalization.hu:
                            culture = "hu-HU";
                            break;
                    }

                    switch (culture)
                    {
                        case ("tr-TR"):
                            LoadCulture("tr");
                            break;
                        case ("ja-JP"):
                            LoadCulture("jpn");
                            break;
                        case ("de-DE"):
                        case ("hsb-DE"):
                        case ("dsb-DE"):
                            _loaded_localization = PreferredKeyboardLocalization.de;
                            LoadCulture("de");
                            break;
                        case ("fr-CH"):
                        case ("de-CH"):
                            _loaded_localization = PreferredKeyboardLocalization.swiss;
                            LoadCulture("swiss");
                            break;
                        case ("fr-FR"):
                        case ("br-FR"):
                        case ("oc-FR"):
                        case ("co-FR"):
                        case ("gsw-FR"):
                            _loaded_localization = PreferredKeyboardLocalization.fr;
                            LoadCulture("fr");
                            break;
                        case ("cy-GB"):
                        case ("gd-GB"):
                        case ("en-GB"):
                            _loaded_localization = PreferredKeyboardLocalization.uk;
                            LoadCulture("uk");
                            break;
                        case ("ru-RU"):
                        case ("tt-RU"):
                        case ("ba-RU"):
                        case ("sah-RU"):
                            _loaded_localization = PreferredKeyboardLocalization.ru;
                            LoadCulture("ru");
                            break;
                        case ("en-US"):
                            _loaded_localization = PreferredKeyboardLocalization.us;
                            LoadCulture("us");
                            break;
                        case ("da-DK"):
                        case ("se-SE"):
                        case ("nb-NO"):
                        case ("nn-NO"):
                        case ("nordic"):
                            _loaded_localization = PreferredKeyboardLocalization.nordic;
                            LoadCulture("nordic");
                            break;
                        case ("pt-BR"):
                            _loaded_localization = PreferredKeyboardLocalization.abnt2;
                            LoadCulture("abnt2");
                            break;
                        case ("dvorak"):
                            _loaded_localization = PreferredKeyboardLocalization.dvorak;
                            LoadCulture("dvorak");
                            break;
                        case ("dvorak_int"):
                            _loaded_localization = PreferredKeyboardLocalization.dvorak_int;
                            LoadCulture("dvorak_int");
                            break;
                        case ("hu-HU"):
                            _loaded_localization = PreferredKeyboardLocalization.hu;
                            LoadCulture("hu");
                            break;
                        default:
                            _loaded_localization = PreferredKeyboardLocalization.intl;
                            LoadCulture("intl");
                            break;

                    }
                }

                var layoutConfigPath = "";

                if (Style == PreferredKeyboard.Logitech_G910)
                    layoutConfigPath = Path.Combine(layoutsPath, "logitech_g910.json");
                else if (Style == PreferredKeyboard.Logitech_G810)
                    layoutConfigPath = Path.Combine(layoutsPath, "logitech_g810.json");
                else if (Style == PreferredKeyboard.Logitech_GPRO)
                    layoutConfigPath = Path.Combine(layoutsPath, "logitech_gpro.json");
                else if (Style == PreferredKeyboard.Logitech_G410)
                    layoutConfigPath = Path.Combine(layoutsPath, "logitech_g410.json");
                else if (Style == PreferredKeyboard.Corsair_K95)
                    layoutConfigPath = Path.Combine(layoutsPath, "corsair_k95.json");
                else if (Style == PreferredKeyboard.Corsair_K95_PL)
                    layoutConfigPath = Path.Combine(layoutsPath, "corsair_k95_platinum.json");
                else if (Style == PreferredKeyboard.Corsair_K70)
                    layoutConfigPath = Path.Combine(layoutsPath, "corsair_k70.json");
                else if (Style == PreferredKeyboard.Corsair_K70MK2)
                    layoutConfigPath = Path.Combine(layoutsPath, "corsair_k70_mk2.json");
                else if (Style == PreferredKeyboard.Corsair_K65)
                    layoutConfigPath = Path.Combine(layoutsPath, "corsair_k65.json");
                else if (Style == PreferredKeyboard.Corsair_STRAFE)
                    layoutConfigPath = Path.Combine(layoutsPath, "corsair_strafe.json");
                else if (Style == PreferredKeyboard.Corsair_K68)
                    layoutConfigPath = Path.Combine(layoutsPath, "corsair_k68.json");
                else if (Style == PreferredKeyboard.Razer_Blackwidow)
                    layoutConfigPath = Path.Combine(layoutsPath, "razer_blackwidow.json");
                else if (Style == PreferredKeyboard.Razer_Blackwidow_X)
                    layoutConfigPath = Path.Combine(layoutsPath, "razer_blackwidow_x.json");
                else if (Style == PreferredKeyboard.Razer_Blackwidow_TE)
                    layoutConfigPath = Path.Combine(layoutsPath, "razer_blackwidow_te.json");
                else if (Style == PreferredKeyboard.Razer_Blade)
                    layoutConfigPath = Path.Combine(layoutsPath, "razer_blade.json");
                else if (Style == PreferredKeyboard.Masterkeys_Pro_L)
                    layoutConfigPath = Path.Combine(layoutsPath, "masterkeys_pro_l.json");
                else if (Style == PreferredKeyboard.Masterkeys_Pro_S)
                    layoutConfigPath = Path.Combine(layoutsPath, "masterkeys_pro_s.json");
                else if (Style == PreferredKeyboard.Masterkeys_Pro_M)
                    layoutConfigPath = Path.Combine(layoutsPath, "masterkeys_pro_m.json");
                else if (Style == PreferredKeyboard.Masterkeys_MK750)
                    layoutConfigPath = Path.Combine(layoutsPath, "masterkeys_mk750.json");
                else if (Style == PreferredKeyboard.Roccat_Ryos)
                    layoutConfigPath = Path.Combine(layoutsPath, "roccat_ryos.json");
                else if (Style == PreferredKeyboard.SteelSeries_Apex_M800)
                    layoutConfigPath = Path.Combine(layoutsPath, "steelseries_apex_m800.json");
                else if (Style == PreferredKeyboard.SteelSeries_Apex_M750)
                    layoutConfigPath = Path.Combine(layoutsPath, "steelseries_apex_m750.json");
                else if (Style == PreferredKeyboard.SteelSeries_Apex_M750_TKL)
                    layoutConfigPath = Path.Combine(layoutsPath, "steelseries_apex_m750_tkl.json");
                else if (Style == PreferredKeyboard.Wooting_One)
                    layoutConfigPath = Path.Combine(layoutsPath, "wooting_one.json");
                else if (Style == PreferredKeyboard.Asus_Strix_Flare)
                    layoutConfigPath = Path.Combine(layoutsPath, "asus_strix_flare.json");
                else if (Style == PreferredKeyboard.SoundBlasterX_Vanguard_K08)
                    layoutConfigPath = Path.Combine(layoutsPath, "soundblasterx_vanguardk08.json");
                else if (Style == PreferredKeyboard.GenericLaptop)
                    layoutConfigPath = Path.Combine(layoutsPath, "generic_laptop.json");
                else if (Style == PreferredKeyboard.GenericLaptopNumpad)
                    layoutConfigPath = Path.Combine(layoutsPath, "generic_laptop_numpad.json");
                else if (Style == PreferredKeyboard.Drevo_BladeMaster)
                    layoutConfigPath = Path.Combine(layoutsPath, "drevo_blademaster.json");
                else
                {
                    //LoadNone();
                    //return;
                }

                if (!String.IsNullOrWhiteSpace(layoutConfigPath) && File.Exists(layoutConfigPath))
                {
                    string content = File.ReadAllText(layoutConfigPath, Encoding.UTF8);
                    VirtualGroupConfiguration layoutConfig = JsonConvert.DeserializeObject<VirtualGroupConfiguration>(content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });

                    virtualGroup.AdjustKeys(layoutConfig.key_modifications);
                    virtualGroup.RemoveKeys(layoutConfig.keys_to_remove);

                    if (layoutConfig.KeyConversion != null)
                    {
                        foreach (var key in layoutConfig.KeyConversion)
                        {
                            if (!this.LayoutKeyConversion.ContainsKey(key.Key))
                                this.LayoutKeyConversion.Add(key.Key, key.Value);
                        }
                    }

                    foreach (string feature in layoutConfig.included_features)
                    {
                        string feature_path = Path.Combine(layoutsPath, "Extra Features", feature);

                        if (File.Exists(feature_path))
                        {
                            string feature_content = File.ReadAllText(feature_path, Encoding.UTF8);
                            VirtualGroup feature_config = JsonConvert.DeserializeObject<VirtualGroup>(feature_content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });

                            virtualGroup.AddFeature(feature_config.grouped_keys.ToArray(), feature_config.origin_region);
                            if (feature_config.KeyConversion != null)
                            {
                                foreach (var key in feature_config.KeyConversion)
                                {
                                    if (!this.LayoutKeyConversion.ContainsKey(key.Key))
                                        this.LayoutKeyConversion.Add(key.Key, key.Value);
                                }
                            }
                        }
                    }

                    //Extra fix for Master keys Pro M White foreign layouts
                    if (Style == PreferredKeyboard.Masterkeys_Pro_M)
                    {
                        switch (_loaded_localization)
                        {
                            case PreferredKeyboardLocalization.intl:
                            case PreferredKeyboardLocalization.de:
                            case PreferredKeyboardLocalization.fr:
                            case PreferredKeyboardLocalization.jpn:
                            case PreferredKeyboardLocalization.ru:
                            case PreferredKeyboardLocalization.uk:
                                virtualGroup.AdjustKeys(new Dictionary<LEDINT, VirtualLight>() { { (LEDINT)KeyboardKeys.NUM_SEVEN, new VirtualLight(null, (LEDINT)KeyboardKeys.NUM_SEVEN, null, null, null, 60, null, null, null, null, null, 5, null) } });
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Global.logger.Error(e.ToString());
            }

            //Perform end of load functions
            //_bitmapMapInvalid = true;
            //_virtualKBInvalid = true;
            //CalculateBitmap();
            virtualGroup.CalculateBitmap();


            //Better description for these keys by using the DeviceKeys description instead
            Dictionary<LEDINT, string> keytext = KeyText;
            keytext.Remove((LEDINT)KeyboardKeys.NUM_ASTERISK);
            keytext.Remove((LEDINT)KeyboardKeys.NUM_EIGHT);
            keytext.Remove((LEDINT)KeyboardKeys.NUM_ENTER);
            keytext.Remove((LEDINT)KeyboardKeys.NUM_FIVE);
            keytext.Remove((LEDINT)KeyboardKeys.NUM_FOUR);
            keytext.Remove((LEDINT)KeyboardKeys.NUM_MINUS);
            keytext.Remove((LEDINT)KeyboardKeys.NUM_NINE);
            keytext.Remove((LEDINT)KeyboardKeys.NUM_ONE);
            keytext.Remove((LEDINT)KeyboardKeys.NUM_PERIOD);
            keytext.Remove((LEDINT)KeyboardKeys.NUM_PLUS);
            keytext.Remove((LEDINT)KeyboardKeys.NUM_SEVEN);
            keytext.Remove((LEDINT)KeyboardKeys.NUM_SIX);
            keytext.Remove((LEDINT)KeyboardKeys.NUM_SLASH);
            keytext.Remove((LEDINT)KeyboardKeys.NUM_THREE);
            keytext.Remove((LEDINT)KeyboardKeys.NUM_TWO);
            keytext.Remove((LEDINT)KeyboardKeys.NUM_ZERO);
            keytext.Remove((LEDINT)KeyboardKeys.NUM_ZEROZERO);
            keytext.Remove((LEDINT)KeyboardKeys.ADDITIONALLIGHT1);
            keytext.Remove((LEDINT)KeyboardKeys.ADDITIONALLIGHT2);
            keytext.Remove((LEDINT)KeyboardKeys.ADDITIONALLIGHT3);
            keytext.Remove((LEDINT)KeyboardKeys.ADDITIONALLIGHT4);
            keytext.Remove((LEDINT)KeyboardKeys.ADDITIONALLIGHT5);
            keytext.Remove((LEDINT)KeyboardKeys.ADDITIONALLIGHT6);
            keytext.Remove((LEDINT)KeyboardKeys.ADDITIONALLIGHT7);
            keytext.Remove((LEDINT)KeyboardKeys.ADDITIONALLIGHT8);
            keytext.Remove((LEDINT)KeyboardKeys.ADDITIONALLIGHT9);
            keytext.Remove((LEDINT)KeyboardKeys.ADDITIONALLIGHT10);
            keytext.Remove((LEDINT)KeyboardKeys.ADDITIONALLIGHT11);
            keytext.Remove((LEDINT)KeyboardKeys.ADDITIONALLIGHT12);
            keytext.Remove((LEDINT)KeyboardKeys.ADDITIONALLIGHT13);
            keytext.Remove((LEDINT)KeyboardKeys.ADDITIONALLIGHT14);
            keytext.Remove((LEDINT)KeyboardKeys.ADDITIONALLIGHT15);
            keytext.Remove((LEDINT)KeyboardKeys.ADDITIONALLIGHT16);
            keytext.Remove((LEDINT)KeyboardKeys.ADDITIONALLIGHT17);
            keytext.Remove((LEDINT)KeyboardKeys.ADDITIONALLIGHT18);
            keytext.Remove((LEDINT)KeyboardKeys.ADDITIONALLIGHT19);
            keytext.Remove((LEDINT)KeyboardKeys.LEFT_CONTROL);
            keytext.Remove((LEDINT)KeyboardKeys.LEFT_WINDOWS);
            keytext.Remove((LEDINT)KeyboardKeys.LEFT_ALT);
            keytext.Remove((LEDINT)KeyboardKeys.LEFT_SHIFT);
            keytext.Remove((LEDINT)KeyboardKeys.RIGHT_ALT);
            keytext.Remove((LEDINT)KeyboardKeys.FN_Key);
            keytext.Remove((LEDINT)KeyboardKeys.RIGHT_WINDOWS);
            keytext.Remove((LEDINT)KeyboardKeys.RIGHT_CONTROL);
            keytext.Remove((LEDINT)KeyboardKeys.RIGHT_SHIFT);

            LayoutChanged();
        }

        private class KeyboardLayout
        {
            [JsonProperty("key_conversion")]
            public Dictionary<LEDINT, LEDINT> KeyConversion = null;

            [JsonProperty("keys")]
            public VirtualLight[] Keys = null;
        }

        private void LoadCulture(String culture)
        {
            var fileName = "Plain Keyboard\\layout." + culture + ".json";
            var layoutPath = Path.Combine(layoutsPath, fileName);

            if (!File.Exists(layoutPath))
            {
                //LoadDefault();
                throw new Exception($"Device Layout with path `{layoutPath}` does not exist!");
            }

            string content = File.ReadAllText(layoutPath, Encoding.UTF8);
            KeyboardLayout keyboard = JsonConvert.DeserializeObject<KeyboardLayout>(content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });

            virtualGroup = new VirtualGroup(this, keyboard.Keys);

            LayoutKeyConversion = keyboard.KeyConversion ?? new Dictionary<LEDINT, LEDINT>();

            /*
            if (keyboard.Count > 0)
                keyboard.Last().line_break = false;

            keyboard.Add(new KeyboardKey("Mouse/\r\nHeadset", Devices.DeviceKeys.Peripheral, true, true, 12, 45, -60, 90, 90, 6, 6, 4, -3));

            if (keyboard.Count > 0)
                keyboard.Last().line_break = true;
            */
        }

        public override string GetLEDName(LEDINT ledID)
        {
            return KeyText.ContainsKey(ledID) ? KeyText[ledID] : ((KeyboardKeys)ledID).GetDescription();
        }

        public override LEDINT Sanitize(LEDINT ledID)
        {
            if (this.LayoutKeyConversion.TryGetValue(ledID, out LEDINT val))
                return val;

            return base.Sanitize(ledID);
        }
    }
}
