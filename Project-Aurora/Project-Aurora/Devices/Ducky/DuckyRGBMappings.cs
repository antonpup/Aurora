using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Devices.Ducky
{
    static class DuckyRGBMappings
    {
        public const int DuckyID = 0x04D9;
        public static int[] KeyboardIDs = new int[]
        {
            0x0348, //Shine 7 & One 2 RGB Full-Size
            0x0356 //One 2 RGB TKL
        };
        public static byte[] DuckyStartingPacket => new byte[] { 0x56, 0x81, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0xAA, 0xAA, 0xAA, 0xAA };
        public static byte[] DuckyInitColourBytes => new byte[] { 0x01, 0x00, 0x00, 0x00, 0x80, 0x01, 0x00, 0xC1, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF };
        public static byte[] DuckyTerminateColourBytes => new byte[] { 0x51, 0x28, 0x00, 0x00, 0xFF };
        public static byte[] DuckyTakeover => new byte[] { 0x00, 0x41, 0x01 };
        public static byte[] DuckyRelease => new byte[] { 0x00, 0x41, 0x00 };
        public static Dictionary<DeviceKeys, (int PacketNum, int OffsetNum)> DuckyColourOffsetMap => new Dictionary<DeviceKeys, (int, int)>
        {
            {DeviceKeys.ESC, (1, 24)},
            {DeviceKeys.TILDE, (1, 27)}, // also the JPN_HALFFULLWIDTH in the JIS layout.
            {DeviceKeys.TAB, (1, 30)},
            {DeviceKeys.CAPS_LOCK, (1, 33)},
            {DeviceKeys.LEFT_SHIFT, (1, 36)},
            {DeviceKeys.LEFT_CONTROL, (1, 39)},
          //{DeviceKeys., (1, 42)}, Probably nothing.
            {DeviceKeys.ONE, (1, 45)},
            {DeviceKeys.Q, (1, 48)},
            {DeviceKeys.A, (1, 51)},
            {DeviceKeys.BACKSLASH_UK, (1, 54)}, //This is almost certain to be the backslash/'<' key for ISO layouts.
            {DeviceKeys.LEFT_WINDOWS, (1, 57)},
            {DeviceKeys.F1, (1,60)},
            {DeviceKeys.TWO, (1, 63)},

            {DeviceKeys.W, (2, 6)},
            {DeviceKeys.S, (2, 9)},
            {DeviceKeys.Z, (2, 12)},
            {DeviceKeys.LEFT_ALT, (2, 15)},
            {DeviceKeys.F2, (2, 18)},
            {DeviceKeys.THREE, (2, 21)},
            {DeviceKeys.E, (2, 24)},
            {DeviceKeys.D, (2, 27)},
            {DeviceKeys.X, (2, 30)},
          //{DeviceKeys.????, (2, 33)}, Possibly the JPN_MUHENKAN key for JIS keyboard layout.
            {DeviceKeys.F3, (2, 36)},
            {DeviceKeys.FOUR, (2, 39)},
            {DeviceKeys.R, (2, 42)},
            {DeviceKeys.F, (2, 45)},
            {DeviceKeys.C, (2, 48)},
          //{DeviceKeys.????, (2, 51)}, Don't have a clue.
            {DeviceKeys.F4, (2, 54)},
            {DeviceKeys.FIVE, (2, 57)},
            {DeviceKeys.T, (2, 60)},
            {DeviceKeys.G, (2, 63)},

            {DeviceKeys.V, (3, 6)},
          //{DeviceKeys., (3, 9)},
          //{DeviceKeys., (3, 12)}, These two are probably nothing.
            {DeviceKeys.SIX, (3, 15)},
            {DeviceKeys.Y, (3, 18)},
            {DeviceKeys.H, (3, 21)},
            {DeviceKeys.B, (3, 24)},
            {DeviceKeys.SPACE, (3, 27)},
            {DeviceKeys.F5, (3, 30)},
            {DeviceKeys.SEVEN, (3, 33)},
            {DeviceKeys.U, (3, 36)},
            {DeviceKeys.J, (3, 39)},
            {DeviceKeys.N, (3, 42)},
          //{DeviceKeys.????, (3, 45)}, Probably nothing. could very unlikely be JPN_HENKAN.
            {DeviceKeys.F6, (3, 48)},
            {DeviceKeys.EIGHT, (3, 51)},
            {DeviceKeys.I, (3, 54)},
            {DeviceKeys.K, (3, 57)},
            {DeviceKeys.M, (3, 60)},
          //{DeviceKeys.????, (3, 63)}, Could be the JPN_HENKAN key. (more likely than the one at {3,45})

            {DeviceKeys.F7, (4, 6)},
            {DeviceKeys.NINE, (4, 9)},
            {DeviceKeys.O, (4, 12)},
            {DeviceKeys.L, (4, 15)},
            {DeviceKeys.COMMA, (4, 18)},
          //{DeviceKeys.????, (4, 21)}, Could be the JPN_HIRAGANA_KATAKANA key.
            {DeviceKeys.F8, (4, 24)},
            {DeviceKeys.ZERO, (4, 27)},
            {DeviceKeys.P, (4, 30)},
            {DeviceKeys.SEMICOLON, (4, 33)}, // Could be different depending on what ISO layout you have (scandanavians have UmlautO here, UK stays the same)
            {DeviceKeys.PERIOD, (4, 36)},
            {DeviceKeys.RIGHT_ALT, (4, 39)},
            {DeviceKeys.F9, (4, 42)},
            {DeviceKeys.MINUS, (4, 45)}, //some ISO layouts might have minus and equals swapped... Why tho.
            {DeviceKeys.OPEN_BRACKET, (4, 48)}, // Could be different depending on what ISO layout you have (scandanavians have TittleA here, UK stays the same)
            {DeviceKeys.APOSTROPHE, (4, 51)}, // Could be different depending on what ISO layout you have (scandanavians have UmlautA here, UK has other stuff)
            {DeviceKeys.FORWARD_SLASH, (4, 54)}, //some ISO layouts have minus here.
          //{DeviceKeys.????, (4, 57)}, Don't know.
            {DeviceKeys.F10, (4, 60)},
            {DeviceKeys.EQUALS, (4, 63)}, //some ISO layouts have Accute Accent or minus here.

            {DeviceKeys.CLOSE_BRACKET, (5, 6)}, // Some ISO layouts have this as another Umlaut key
            {DeviceKeys.HASHTAG, (5, 9)}, //Could be the " ' " (apostrphe) key in ISO.
          //{DeviceKeys., (5, 12)}, Probably nothing
            {DeviceKeys.RIGHT_WINDOWS, (5, 15)},
            {DeviceKeys.F11, (5, 18)},
          //{DeviceKeys., (5, 21)},
          //{DeviceKeys., (5, 24)},
          //{DeviceKeys., (5, 27)}, These three are probably nothing.
            {DeviceKeys.RIGHT_SHIFT, (5, 30)},
            {DeviceKeys.FN_Key, (5, 33)}, //The problem with this keyboard is there's dip switches on the back to move where the FN key is... This assumes default position.
            {DeviceKeys.F12, (5, 36)},
            {DeviceKeys.BACKSPACE, (5, 39)},
            {DeviceKeys.BACKSLASH, (5, 42)}, // ISO and JIS layouts don't have this key.
            {DeviceKeys.ENTER, (5, 45)},
          //{DeviceKeys., (5, 48)}, Very likely to be nothing.
            {DeviceKeys.RIGHT_CONTROL, (5, 51)},
            {DeviceKeys.PRINT_SCREEN, (5, 54)},
            {DeviceKeys.INSERT, (5, 57)},
            {DeviceKeys.DELETE, (5, 60)},
          //{DeviceKeys., (5, 63)}, Nothing.

          //{DeviceKeys., (6, 6)}, Also nothing.
            {DeviceKeys.ARROW_LEFT, (6, 9)},
            {DeviceKeys.SCROLL_LOCK, (6, 12)},
            {DeviceKeys.HOME, (6, 15)},
            {DeviceKeys.END, (6, 18)},
          //{DeviceKeys., (6, 21)}, Also nothing.
            {DeviceKeys.ARROW_UP, (6, 24)},
            {DeviceKeys.ARROW_DOWN, (6, 27)},
            {DeviceKeys.PAUSE_BREAK, (6, 30)},
            {DeviceKeys.PAGE_UP, (6, 33)},
            {DeviceKeys.PAGE_DOWN, (6, 36)},
          //{DeviceKeys., (6, 39)},
          //{DeviceKeys., (6, 42)}, Both are nothing.
            {DeviceKeys.ARROW_RIGHT, (6, 45)},
            {DeviceKeys.CALC, (6, 48)},
            {DeviceKeys.NUM_LOCK, (6, 51)},
            {DeviceKeys.NUM_SEVEN, (6, 54)},
            {DeviceKeys.NUM_FOUR, (6, 57)},
            {DeviceKeys.NUM_ONE, (6, 60)},
            {DeviceKeys.NUM_ZERO, (6, 63)},

            {DeviceKeys.VOLUME_MUTE, (7, 6)},
            {DeviceKeys.NUM_SLASH, (7, 9)},
            {DeviceKeys.NUM_EIGHT, (7, 12)},
            {DeviceKeys.NUM_FIVE, (7, 15)},
            {DeviceKeys.NUM_TWO, (7, 18)},
          //{DeviceKeys., (7, 21)}, Nothing
            {DeviceKeys.VOLUME_DOWN, (7, 24)},
            {DeviceKeys.NUM_ASTERISK, (7, 27)},
            {DeviceKeys.NUM_NINE, (7, 30)},
            {DeviceKeys.NUM_SIX, (7, 33)},
            {DeviceKeys.NUM_THREE, (7, 36)},
            {DeviceKeys.NUM_PERIOD, (7, 39)},
            {DeviceKeys.VOLUME_UP, (7, 42)},
            {DeviceKeys.NUM_MINUS, (7, 45)},
            {DeviceKeys.NUM_PLUS, (7, 48)},
            //{DeviceKeys., (7, 51)},
            //{DeviceKeys., (7, 54)}, Nothing for both.
            {DeviceKeys.NUM_ENTER, (7, 57)}
        };
    }
}