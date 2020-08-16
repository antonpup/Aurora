
using Aurora.Utils;
using LedCSharp;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace LedCSharp
{
    public enum keyboardNames
    {
        ESC = 0x01,
        F1 = 0x3b,
        F2 = 0x3c,
        F3 = 0x3d,
        F4 = 0x3e,
        F5 = 0x3f,
        F6 = 0x40,
        F7 = 0x41,
        F8 = 0x42,
        F9 = 0x43,
        F10 = 0x44,
        F11 = 0x57,
        F12 = 0x58,
        PRINT_SCREEN = 0x137,
        SCROLL_LOCK = 0x46,
        PAUSE_BREAK = 0x145,
        TILDE = 0x29,
        ONE = 0x02,
        TWO = 0x03,
        THREE = 0x04,
        FOUR = 0x05,
        FIVE = 0x06,
        SIX = 0x07,
        SEVEN = 0x08,
        EIGHT = 0x09,
        NINE = 0x0A,
        ZERO = 0x0B,
        MINUS = 0x0C,
        EQUALS = 0x0D,
        BACKSPACE = 0x0E,
        INSERT = 0x152,
        HOME = 0x147,
        PAGE_UP = 0x149,
        NUM_LOCK = 0x45,
        NUM_SLASH = 0x135,
        NUM_ASTERISK = 0x37,
        NUM_MINUS = 0x4A,
        TAB = 0x0F,
        Q = 0x10,
        W = 0x11,
        E = 0x12,
        R = 0x13,
        T = 0x14,
        Y = 0x15,
        U = 0x16,
        I = 0x17,
        O = 0x18,
        P = 0x19,
        OPEN_BRACKET = 0x1A,
        CLOSE_BRACKET = 0x1B,
        BACKSLASH = 0x2B,
        KEYBOARD_DELETE = 0x153,
        END = 0x14F,
        PAGE_DOWN = 0x151,
        NUM_SEVEN = 0x47,
        NUM_EIGHT = 0x48,
        NUM_NINE = 0x49,
        NUM_PLUS = 0x4E,
        CAPS_LOCK = 0x3A,
        A = 0x1E,
        S = 0x1F,
        D = 0x20,
        F = 0x21,
        G = 0x22,
        H = 0x23,
        J = 0x24,
        K = 0x25,
        L = 0x26,
        SEMICOLON = 0x27,
        APOSTROPHE = 0x28,
        ENTER = 0x1C,
        NUM_FOUR = 0x4B,
        NUM_FIVE = 0x4C,
        NUM_SIX = 0x4D,
        LEFT_SHIFT = 0x2A,
        Z = 0x2C,
        X = 0x2D,
        C = 0x2E,
        V = 0x2F,
        B = 0x30,
        N = 0x31,
        M = 0x32,
        COMMA = 0x33,
        PERIOD = 0x34,
        FORWARD_SLASH = 0x35,
        RIGHT_SHIFT = 0x36,
        ARROW_UP = 0x148,
        NUM_ONE = 0x4F,
        NUM_TWO = 0x50,
        NUM_THREE = 0x51,
        NUM_ENTER = 0x11C,
        LEFT_CONTROL = 0x1D,
        LEFT_WINDOWS = 0x15B,
        LEFT_ALT = 0x38,
        SPACE = 0x39,
        RIGHT_ALT = 0x138,
        RIGHT_WINDOWS = 0x15C,
        APPLICATION_SELECT = 0x15D,
        RIGHT_CONTROL = 0x11D,
        ARROW_LEFT = 0x14B,
        ARROW_DOWN = 0x150,
        ARROW_RIGHT = 0x14D,
        NUM_ZERO = 0x52,
        NUM_PERIOD = 0x53,
        G_1 = 0xFFF1,
        G_2 = 0xFFF2,
        G_3 = 0xFFF3,
        G_4 = 0xFFF4,
        G_5 = 0xFFF5,
        G_6 = 0xFFF6,
        G_7 = 0xFFF7,
        G_8 = 0xFFF8,
        G_9 = 0xFFF9,
        G_LOGO = 0xFFFF1,
        G_BADGE = 0xFFFF2
    };

    public enum Logitech_keyboardBitmapKeys
    {
        UNKNOWN = -1,
        ESC = 0,
        F1 = 4,
        F2 = 8,
        F3 = 12,
        F4 = 16,
        F5 = 20,
        F6 = 24,
        F7 = 28,
        F8 = 32,
        F9 = 36,
        F10 = 40,
        F11 = 44,
        F12 = 48,
        PRINT_SCREEN = 52,
        SCROLL_LOCK = 56,
        PAUSE_BREAK = 60,
        //64
        //68
        //72
        //76
        //80

        TILDE = 84,
        ONE = 88,
        TWO = 92,
        THREE = 96,
        FOUR = 100,
        FIVE = 104,
        SIX = 108,
        SEVEN = 112,
        EIGHT = 116,
        NINE = 120,
        ZERO = 124,
        MINUS = 128,
        EQUALS = 132,
        BACKSPACE = 136,
        INSERT = 140,
        HOME = 144,
        PAGE_UP = 148,
        NUM_LOCK = 152,
        NUM_SLASH = 156,
        NUM_ASTERISK = 160,
        NUM_MINUS = 164,

        TAB = 168,
        Q = 172,
        W = 176,
        E = 180,
        R = 184,
        T = 188,
        Y = 192,
        U = 196,
        I = 200,
        O = 204,
        P = 208,
        OPEN_BRACKET = 212,
        CLOSE_BRACKET = 216,
        BACKSLASH = 220,
        KEYBOARD_DELETE = 224,
        END = 228,
        PAGE_DOWN = 232,
        NUM_SEVEN = 236,
        NUM_EIGHT = 240,
        NUM_NINE = 244,
        NUM_PLUS = 248,

        CAPS_LOCK = 252,
        A = 256,
        S = 260,
        D = 264,
        F = 268,
        G = 272,
        H = 276,
        J = 280,
        K = 284,
        L = 288,
        SEMICOLON = 292,
        APOSTROPHE = 296,
        HASHTAG = 300,//300
        ENTER = 304,
        //308
        //312
        //316
        NUM_FOUR = 320,
        NUM_FIVE = 324,
        NUM_SIX = 328,
        //332

        LEFT_SHIFT = 336,
        BACKSLASH_UK = 340,
        Z = 344,
        X = 348,
        C = 352,
        V = 356,
        B = 360,
        N = 364,
        M = 368,
        COMMA = 372,
        PERIOD = 376,
        FORWARD_SLASH = 380,
        OEM102 = 384,
        RIGHT_SHIFT = 388,
        //392
        ARROW_UP = 396,
        //400
        NUM_ONE = 404,
        NUM_TWO = 408,
        NUM_THREE = 412,
        NUM_ENTER = 416,

        LEFT_CONTROL = 420,
        LEFT_WINDOWS = 424,
        LEFT_ALT = 428,
        //432
        JPN_MUHENKAN = 436,//436
        SPACE = 440,
        //444
        //448
        JPN_HENKAN = 452,//452
        JPN_HIRAGANA_KATAKANA = 456,//456
        //460
        RIGHT_ALT = 464,
        RIGHT_WINDOWS = 468,
        APPLICATION_SELECT = 472,
        RIGHT_CONTROL = 476,
        ARROW_LEFT = 480,
        ARROW_DOWN = 484,
        ARROW_RIGHT = 488,
        NUM_ZERO = 492,
        NUM_PERIOD = 496,
        //500
    };

    public enum DeviceType
    {
        Keyboard = 0x0,
        Mouse = 0x3,
        Mousemat = 0x4,
        Headset = 0x8,
        Speaker = 0xe
    }

    public enum LGDLL
    {
        LGS,
        GHUB
    }

    public static class LogitechGSDK
    {
        private const int LOGI_DEVICETYPE_MONOCHROME_ORD = 0;
        private const int LOGI_DEVICETYPE_RGB_ORD = 1;
        private const int LOGI_DEVICETYPE_PERKEY_RGB_ORD = 2;

        public const int LOGI_DEVICETYPE_MONOCHROME = (1 << LOGI_DEVICETYPE_MONOCHROME_ORD);
        public const int LOGI_DEVICETYPE_RGB = (1 << LOGI_DEVICETYPE_RGB_ORD);
        public const int LOGI_DEVICETYPE_PERKEY_RGB = (1 << LOGI_DEVICETYPE_PERKEY_RGB_ORD);
        public const int LOGI_DEVICETYPE_ALL = (LOGI_DEVICETYPE_MONOCHROME | LOGI_DEVICETYPE_RGB | LOGI_DEVICETYPE_PERKEY_RGB);

        public const int LOGI_LED_BITMAP_WIDTH = 21;
        public const int LOGI_LED_BITMAP_HEIGHT = 6;
        public const int LOGI_LED_BITMAP_BYTES_PER_KEY = 4;

        public const int LOGI_LED_BITMAP_SIZE = LOGI_LED_BITMAP_WIDTH * LOGI_LED_BITMAP_HEIGHT * LOGI_LED_BITMAP_BYTES_PER_KEY;
        public const int LOGI_LED_DURATION_INFINITE = 0;

        public static bool GHUB = true;

        public static bool LogiLedInit()
        {
            return GHUB ? GHUBImports.LogiLedInit() : LGSImports.LogiLedInit();
        }

        public static bool LogiLedSetTargetDevice(int targetDevice)
        {
            return GHUB ? GHUBImports.LogiLedSetTargetDevice(targetDevice) : LGSImports.LogiLedSetTargetDevice(targetDevice);
        }

        public static bool LogiLedSetLighting(Color color)
        {
            var (R, G, B) = GetColorValues(color);
            return GHUB ? GHUBImports.LogiLedSetLighting(R, G, B) : LGSImports.LogiLedSetLighting(R, G, B);
        }

        public static bool LogiLedExcludeKeysFromBitmap(keyboardNames[] keyList, int listCount)
        {
            return GHUB ? GHUBImports.LogiLedExcludeKeysFromBitmap(keyList, listCount) :
                     LGSImports.LogiLedExcludeKeysFromBitmap(keyList, listCount);
        }

        public static bool LogiLedSetLightingFromBitmap(byte[] bitmap)
        {
            return GHUB ? GHUBImports.LogiLedSetLightingFromBitmap(bitmap) :
                     LGSImports.LogiLedSetLightingFromBitmap(bitmap);
        }

        public static bool LogiLedSetLightingForKeyWithScanCode(int keyCode, Color color)
        {
            var (R, G, B) = GetColorValues(color);
            return GHUB ? GHUBImports.LogiLedSetLightingForKeyWithScanCode(keyCode, R, G, B) :
                LGSImports.LogiLedSetLightingForKeyWithScanCode(keyCode, R, G, B);
        }

        public static bool LogiLedSetLightingForKeyWithHidCode(int keyCode, Color color)
        {
            var (R, G, B) = GetColorValues(color);
            return GHUB ? GHUBImports.LogiLedSetLightingForKeyWithHidCode(keyCode, R, G, B) :
                LGSImports.LogiLedSetLightingForKeyWithHidCode(keyCode, R, G, B);
        }

        public static bool LogiLedSetLightingForKeyWithQuartzCode(int keyCode, Color color)
        {
            var (R, G, B) = GetColorValues(color);
            return GHUB ? GHUBImports.LogiLedSetLightingForKeyWithQuartzCode(keyCode, R, G, B) :
                LGSImports.LogiLedSetLightingForKeyWithQuartzCode(keyCode, R, G, B);
        }

        public static bool LogiLedSetLightingForKeyWithKeyName(keyboardNames keyCode, Color color)
        {
            var (R, G, B) = GetColorValues(color);
            return GHUB ? GHUBImports.LogiLedSetLightingForKeyWithKeyName(keyCode, R, G, B) :
                LGSImports.LogiLedSetLightingForKeyWithKeyName(keyCode, R, G, B);
        }

        public static bool LogiLedSetLightingForTargetZone(DeviceType deviceType, int zone, Color color)
        {
            var (R, G, B) = GetColorValues(color);
            return GHUB ? GHUBImports.LogiLedSetLightingForTargetZone((byte)deviceType, zone, R, G, B) :
                LGSImports.LogiLedSetLightingForTargetZone((byte)deviceType, zone, R, G, B);
        }

        public static void LogiLedShutdown()
        {
            if (GHUB)
                GHUBImports.LogiLedShutdown();
            else
                LGSImports.LogiLedShutdown();
        }

        public static bool LogiLedSaveCurrentLighting()
        {
            return GHUB ? GHUBImports.LogiLedSaveCurrentLighting() : LGSImports.LogiLedSaveCurrentLighting();
        }

        public static bool LogiLedRestoreLighting()
        {
            return GHUB ? GHUBImports.LogiLedRestoreLighting() : LGSImports.LogiLedRestoreLighting();
        }

        private static (int R, int G, int B) GetColorValues(Color clr)
        {
            clr = ColorUtils.CorrectWithAlpha(clr);
            return ((int)(clr.R / 255.0 * 100.0),
                    (int)(clr.G / 255.0 * 100.0),
                    (int)(clr.B / 255.0 * 100.0));
        }
    }

    internal static class LGSImports
    {
        private const string dllpath = "Logi\\LGS\\LogitechLed.dll";

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedInit();

        //Config option functions
        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedGetConfigOptionNumber([MarshalAs(UnmanagedType.LPWStr)] String configPath, ref double defaultNumber);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedGetConfigOptionBool([MarshalAs(UnmanagedType.LPWStr)] String configPath, ref bool defaultRed);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedGetConfigOptionColor([MarshalAs(UnmanagedType.LPWStr)] String configPath, ref int defaultRed, ref int defaultGreen, ref int defaultBlue);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedGetConfigOptionKeyInput([MarshalAs(UnmanagedType.LPWStr)] String configPath, StringBuilder buffer, int bufsize);
        /////////////////////

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetTargetDevice(int targetDevice);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedGetSdkVersion(ref int majorNum, ref int minorNum, ref int buildNum);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSaveCurrentLighting();

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLighting(int redPercentage, int greenPercentage, int bluePercentage);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedRestoreLighting();

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedFlashLighting(int redPercentage, int greenPercentage, int bluePercentage, int milliSecondsDuration, int milliSecondsInterval);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedPulseLighting(int redPercentage, int greenPercentage, int bluePercentage, int milliSecondsDuration, int milliSecondsInterval);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedStopEffects();

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedExcludeKeysFromBitmap(keyboardNames[] keyList, int listCount);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLightingFromBitmap(byte[] bitmap);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLightingForKeyWithScanCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLightingForKeyWithHidCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLightingForKeyWithQuartzCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLightingForKeyWithKeyName(keyboardNames keyCode, int redPercentage, int greenPercentage, int bluePercentage);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLightingForTargetZone(byte deviceType, int zone, int redPercentage, int greenPercentage, int bluePercentage);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSaveLightingForKey(keyboardNames keyName);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedRestoreLightingForKey(keyboardNames keyName);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedFlashSingleKey(keyboardNames keyName, int redPercentage, int greenPercentage, int bluePercentage, int msDuration, int msInterval);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedPulseSingleKey(keyboardNames keyName, int startRedPercentage, int startGreenPercentage, int startBluePercentage, int finishRedPercentage, int finishGreenPercentage, int finishBluePercentage, int msDuration, bool isInfinite);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedStopEffectsOnKey(keyboardNames keyName);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void LogiLedShutdown();
    }

    internal static class GHUBImports
    {
        private const string dllpath = "Logi\\GHUB\\LogitechLed.dll";

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedInit();

        //Config option functions
        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedGetConfigOptionNumber([MarshalAs(UnmanagedType.LPWStr)] String configPath, ref double defaultNumber);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedGetConfigOptionBool([MarshalAs(UnmanagedType.LPWStr)] String configPath, ref bool defaultRed);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedGetConfigOptionColor([MarshalAs(UnmanagedType.LPWStr)] String configPath, ref int defaultRed, ref int defaultGreen, ref int defaultBlue);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedGetConfigOptionKeyInput([MarshalAs(UnmanagedType.LPWStr)] String configPath, StringBuilder buffer, int bufsize);
        /////////////////////

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetTargetDevice(int targetDevice);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedGetSdkVersion(ref int majorNum, ref int minorNum, ref int buildNum);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSaveCurrentLighting();

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLighting(int redPercentage, int greenPercentage, int bluePercentage);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedRestoreLighting();

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedFlashLighting(int redPercentage, int greenPercentage, int bluePercentage, int milliSecondsDuration, int milliSecondsInterval);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedPulseLighting(int redPercentage, int greenPercentage, int bluePercentage, int milliSecondsDuration, int milliSecondsInterval);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedStopEffects();

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedExcludeKeysFromBitmap(LedCSharp.keyboardNames[] keyList, int listCount);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLightingFromBitmap(byte[] bitmap);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLightingForKeyWithScanCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLightingForKeyWithHidCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLightingForKeyWithQuartzCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLightingForKeyWithKeyName(keyboardNames keyCode, int redPercentage, int greenPercentage, int bluePercentage);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLightingForTargetZone(byte deviceType, int zone, int redPercentage, int greenPercentage, int bluePercentage);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSaveLightingForKey(keyboardNames keyName);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedRestoreLightingForKey(keyboardNames keyName);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedFlashSingleKey(keyboardNames keyName, int redPercentage, int greenPercentage, int bluePercentage, int msDuration, int msInterval);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedPulseSingleKey(keyboardNames keyName, int startRedPercentage, int startGreenPercentage, int startBluePercentage, int finishRedPercentage, int finishGreenPercentage, int finishBluePercentage, int msDuration, bool isInfinite);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedStopEffectsOnKey(keyboardNames keyName);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void LogiLedShutdown();
    }
}