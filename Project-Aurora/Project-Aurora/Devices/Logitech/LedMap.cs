using LedCSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Devices.Logitech
{
    public static class LedMaps
    {
        public static readonly Dictionary<DeviceKeys, int> BitmapMap = new Dictionary<DeviceKeys, int>()
        {
            [DeviceKeys.ESC] = 0,
            [DeviceKeys.F1] = 4,
            [DeviceKeys.F2] = 8,
            [DeviceKeys.F3] = 12,
            [DeviceKeys.F4] = 16,
            [DeviceKeys.F5] = 20,
            [DeviceKeys.F6] = 24,
            [DeviceKeys.F7] = 28,
            [DeviceKeys.F8] = 32,
            [DeviceKeys.F9] = 36,
            [DeviceKeys.F10] = 40,
            [DeviceKeys.F11] = 44,
            [DeviceKeys.F12] = 48,
            [DeviceKeys.PRINT_SCREEN] = 52,
            [DeviceKeys.SCROLL_LOCK] = 56,
            [DeviceKeys.PAUSE_BREAK] = 60,
            //64
            //68
            //72
            //76
            //80
            [DeviceKeys.TILDE] = 84,
            [DeviceKeys.ONE] = 88,
            [DeviceKeys.TWO] = 92,
            [DeviceKeys.THREE] = 96,
            [DeviceKeys.FOUR] = 100,
            [DeviceKeys.FIVE] = 104,
            [DeviceKeys.SIX] = 108,
            [DeviceKeys.SEVEN] = 112,
            [DeviceKeys.EIGHT] = 116,
            [DeviceKeys.NINE] = 120,
            [DeviceKeys.ZERO] = 124,
            [DeviceKeys.MINUS] = 128,
            [DeviceKeys.EQUALS] = 132,
            [DeviceKeys.BACKSPACE] = 136,
            [DeviceKeys.INSERT] = 140,
            [DeviceKeys.HOME] = 144,
            [DeviceKeys.PAGE_UP] = 148,
            [DeviceKeys.NUM_LOCK] = 152,
            [DeviceKeys.NUM_SLASH] = 156,
            [DeviceKeys.NUM_ASTERISK] = 160,
            [DeviceKeys.NUM_MINUS] = 164,
            [DeviceKeys.TAB] = 168,
            [DeviceKeys.Q] = 172,
            [DeviceKeys.W] = 176,
            [DeviceKeys.E] = 180,
            [DeviceKeys.R] = 184,
            [DeviceKeys.T] = 188,
            [DeviceKeys.Y] = 192,
            [DeviceKeys.U] = 196,
            [DeviceKeys.I] = 200,
            [DeviceKeys.O] = 204,
            [DeviceKeys.P] = 208,
            [DeviceKeys.OPEN_BRACKET] = 212,
            [DeviceKeys.CLOSE_BRACKET] = 216,
            [DeviceKeys.BACKSLASH] = 220,
            [DeviceKeys.DELETE] = 224,
            [DeviceKeys.END] = 228,
            [DeviceKeys.PAGE_DOWN] = 232,
            [DeviceKeys.NUM_SEVEN] = 236,
            [DeviceKeys.NUM_EIGHT] = 240,
            [DeviceKeys.NUM_NINE] = 244,
            [DeviceKeys.NUM_PLUS] = 248,
            [DeviceKeys.CAPS_LOCK] = 252,
            [DeviceKeys.A] = 256,
            [DeviceKeys.S] = 260,
            [DeviceKeys.D] = 264,
            [DeviceKeys.F] = 268,
            [DeviceKeys.G] = 272,
            [DeviceKeys.H] = 276,
            [DeviceKeys.J] = 280,
            [DeviceKeys.K] = 284,
            [DeviceKeys.L] = 288,
            [DeviceKeys.SEMICOLON] = 292,
            [DeviceKeys.APOSTROPHE] = 296,
            [DeviceKeys.HASHTAG] = 300,
            [DeviceKeys.ENTER] = 304,
            //308
            //312
            //316
            [DeviceKeys.NUM_FOUR] = 320,
            [DeviceKeys.NUM_FIVE] = 324,
            [DeviceKeys.NUM_SIX] = 328,
            //332
            [DeviceKeys.LEFT_SHIFT] = 336,
            [DeviceKeys.BACKSLASH_UK] = 340,
            [DeviceKeys.Z] = 344,
            [DeviceKeys.X] = 348,
            [DeviceKeys.C] = 352,
            [DeviceKeys.V] = 356,
            [DeviceKeys.B] = 360,
            [DeviceKeys.N] = 364,
            [DeviceKeys.M] = 368,
            [DeviceKeys.COMMA] = 372,
            [DeviceKeys.PERIOD] = 376,
            [DeviceKeys.FORWARD_SLASH] = 380,
            [DeviceKeys.OEM102] = 384,
            [DeviceKeys.RIGHT_SHIFT] = 388,
            //392
            [DeviceKeys.ARROW_UP] = 396,
            //400
            [DeviceKeys.NUM_ONE] = 404,
            [DeviceKeys.NUM_TWO] = 408,
            [DeviceKeys.NUM_THREE] = 412,
            [DeviceKeys.NUM_ENTER] = 416,
            [DeviceKeys.LEFT_CONTROL] = 420,
            [DeviceKeys.LEFT_WINDOWS] = 424,
            [DeviceKeys.LEFT_ALT] = 428,
            //432
            [DeviceKeys.JPN_MUHENKAN] = 436,
            [DeviceKeys.SPACE] = 440,
            //444
            //448
            [DeviceKeys.JPN_HENKAN] = 452,
            [DeviceKeys.JPN_HIRAGANA_KATAKANA] = 456,
            //460
            [DeviceKeys.RIGHT_ALT] = 464,
            [DeviceKeys.RIGHT_WINDOWS] = 468,
            [DeviceKeys.APPLICATION_SELECT] = 472,
            [DeviceKeys.RIGHT_CONTROL] = 476,
            [DeviceKeys.ARROW_LEFT] = 480,
            [DeviceKeys.ARROW_DOWN] = 484,
            [DeviceKeys.ARROW_RIGHT] = 488,
            [DeviceKeys.NUM_ZERO] = 492,
            [DeviceKeys.NUM_PERIOD] = 496,
            //500
        };

        public static readonly Dictionary<DeviceKeys, keyboardNames> KeyMap = new Dictionary<DeviceKeys, keyboardNames>()
        {
            [DeviceKeys.G1] = keyboardNames.G_1,
            [DeviceKeys.G2] = keyboardNames.G_2,
            [DeviceKeys.G3] = keyboardNames.G_3,
            [DeviceKeys.G4] = keyboardNames.G_4,
            [DeviceKeys.G5] = keyboardNames.G_5,
            [DeviceKeys.G6] = keyboardNames.G_6,
            [DeviceKeys.G7] = keyboardNames.G_7,
            [DeviceKeys.G8] = keyboardNames.G_8,
            [DeviceKeys.G9] = keyboardNames.G_9,
            [DeviceKeys.LOGO] = keyboardNames.G_LOGO,
            [DeviceKeys.LOGO2] = keyboardNames.G_BADGE,
        };

        public static readonly Dictionary<DeviceKeys, (DeviceType type, int zone)> PeripheralMap = new Dictionary<DeviceKeys, (DeviceType, int)>()
        {
            //not sure how to handle this properly. the indexes do not match on different mice
            //g900 => 0 is dpi
            //g502 => 0 is dpi
            //g303 => 0 is side
            //g403 => 0 is scroll
            //g703 => 0 is scroll
            //gpro => 0 is everything
            //other mice are not documented on the sdk
            [DeviceKeys.PERIPHERAL_DPI] =  (DeviceType.Mouse, 0),
            [DeviceKeys.Peripheral_Logo] = (DeviceType.Mouse, 1),
            [DeviceKeys.Peripheral_ScrollWheel] = (DeviceType.Mouse, 2),
        };
    }
}
